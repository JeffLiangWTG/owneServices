using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class DataContextManager<T> : IDataContextManager where T : BusinessObject
	{
		protected DataContextManager() { }

		public T ParentBO
		{
			get;
			private set;
		}

		public Type TopLevelBusinessObjectType => typeof(T);

		void IDataContextManager.Init(BusinessObject parentBO)
		{
			ParentBO = (T)parentBO;
		}

		string IEntityID.DataContextKey { get { return DataContextKey; } }

		public abstract DataContextType DataContextType { get; }
		public abstract ZString DataContextKey { get; }
		public abstract string DefaultOutputDirectory { get; }

		IUniversalXmlSchema IDataContextManager.SchemaOverride
		{
			get { return GetSchemaOverride(); }
		}

		protected virtual IUniversalXmlSchema GetSchemaOverride()
		{
			return null;
		}

		#region LoadBusinessObject(s)FromDataSource/DataTarget

		public BusinessObject LoadBusinessObjectFromDataSource(ITopLevelDataObject topLevelDataObject, IDataSourceDataObject dataSource, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var businessObjects = LoadBusinessObjectsFromDataSourceOrTarget(new DataContextMatchingKey(topLevelDataObject, dataSource), factory, logger);

			return LoadBusinessObjectFromDataSourceOrTarget(logger, businessObjects, dataSource.Key.GetValueOrDefault(), "DataSource");
		}

		public BusinessObject LoadBusinessObjectFromDataTarget(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var businessObjects = LoadBusinessObjectsFromDataTarget(topLevelDataObject, dataTarget, factory, logger);

			return LoadBusinessObjectFromDataSourceOrTarget(logger, businessObjects, dataTarget.Key.GetValueOrDefault(), "DataTarget");
		}

		public BusinessObject[] LoadBusinessObjectsFromDataTarget(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return LoadBusinessObjectsFromDataSourceOrTarget(new DataContextMatchingKey(topLevelDataObject, dataTarget, logger), factory, logger);
		}

		BusinessObject[] LoadBusinessObjectsFromDataSourceOrTarget(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			if (matchingValues != null && !matchingValues.Key.IsEmpty)
			{
				var bizos = LoadBusinessObjectsFromDataContextKey(matchingValues, factory, logger);
				if (bizos != null)
				{
					return bizos;
				}

				var query = GetDataContextKeyMatchingQuery(matchingValues, factory, logger);

				if (query != null)
				{
					try
					{
						return LoadBusinessObjects(factory, query);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ex.Data.Add("Context Manager Type", GetType().FullName);
						ex.Data.Add("Query", query.LiteralTextADO);
						throw;
					}
				}
			}

			return Array.Empty<BusinessObject>();
		}

		BusinessObject LoadBusinessObjectFromDataSourceOrTarget(IXmlImportLogger logger, BusinessObject[] businessObjects, string dataSourceOrTargetValue, string dataSourceOrTargetString)
		{
			BusinessObject businessObject = null;

			if (businessObjects.Length == 1)
			{
				businessObject = businessObjects[0];
				var cancellableBO = businessObject as ICancellable;
				if (cancellableBO == null || !cancellableBO.IsCancelled)
				{
					if (businessObject != null)
					{
						logger.LogVerboseOnly(LogType.Information, Res.GetString("e4fb4f10-9a6d-4e62-9c24-b8520240de18", "Found matching {0} using the {2} Key {1}.", DataContextType.ToString(), dataSourceOrTargetValue, "DataTarget"));
					}
				}
			}
			else if (businessObjects.Length > 1)
			{
				var parentBusinessObjects = string.Join("\r\n", businessObjects.Select(o => string.Format("{0} PK: [{1}]", o.HumanReadableName, o.PK)).ToArray());
				throw new DataObjectReadFailureException(string.Format("Found more than one Parent Business Object using the {0} Key {1}. Parents found were:\r\n{2}", dataSourceOrTargetString, dataSourceOrTargetValue, parentBusinessObjects));
			}

			return businessObject;
		}

		class DataContextMatchingKey : IDataContextMatchingKey
		{
			internal DataContextMatchingKey(ITopLevelDataObject topLevelDataObject, IDataSourceDataObject dataSource)
				: this(topLevelDataObject, dataSource.Key.GetValueOrDefault())
			{
			}

			internal DataContextMatchingKey(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, IXmlImportLogger logger)
				: this(topLevelDataObject, dataTarget.Key.GetValueOrDefault())
			{
				this.dataTarget = dataTarget;
				this.logger = logger;
			}

			DataContextMatchingKey(ITopLevelDataObject topLevelDataObject, ZString key)
			{
				DataObject = Argument.NotNull(topLevelDataObject, "topLevelDataObject");
				var dataContext = topLevelDataObject.DataContext;
				if (dataContext != null)
				{
					Key = key;
					CompanyCode = dataContext.CompanyCodeToImportInto;
				}
			}

			readonly IDataTargetDataObject dataTarget;
			readonly IXmlImportLogger logger;

			public ITopLevelDataObject DataObject
			{
				get;
				private set;
			}
			public ZString Key
			{
				get;
				private set;
			}

			public ZString CompanyCode
			{
				get;
				private set;
			}

			public ZString OwnerOrganisationCode
			{
				get
				{
					if (dataTarget != null)
					{
						var ownerAddressData = dataTarget.Owner;
						if (ownerAddressData != null)
						{
							return ObjectFactory.New<IOrganizationAddressMatcher>().GetMatchedOrganisationCode(ownerAddressData, logger);
						}
					}
					return ZString.Empty;
				}
			}
		}

		/// <summary>
		/// Return null if you don't want to do any matching (you probably *do* want to do matching).  
		/// To match, return a query that will load your BizO using the key provided in the DataTarget. 
		/// e.g. source key provided by
		///			<DataTarget>
		///				<Type>USImporterSecurityFiling</Type>
		///				<Key>ISF1255S000010</Key>
		///			</DataTarget>
		///	Then you can make a query like this:  new ZQuery(CusISFHeaderSchema.BF_JobReference, matchingValues.Key);
		///	i.e.  "select * from dbo.CusISFHeader where BF_JobReference= 'ISF1255S000010' "
		/// See C:\dev\Enterprise\Product\Operations\Customs\US\ISF\DataTransfer\Universal\ISFHeaderDataContextManager.cs
		/// </summary>
		protected abstract ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger);

		protected virtual T[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery query)
		{
			return factory.Load<T>(query);
		}

		protected virtual BusinessObject[] LoadBusinessObjectsFromDataContextKey(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		#endregion

		protected virtual bool IsNotFromSameSystemAndModule(IDataContextDataObject dataContext, ISimpleLogger logger)
		{
			if (dataContext != null && dataContext.IsFromSameSystem() && dataContext.DataSourceCollection != null && dataContext.DataSourceCollection.Any())
			{
				var dataTargetType = DataContextType.ToString();
				if (dataContext.DataSourceCollection.Any(s => s.Type.HasValue && s.Type.Value == dataTargetType))
				{
					LogIgnoredModule(logger, dataTargetType);
					return false;
				}
			}

			return true;
		}

		void LogIgnoredModule(ISimpleLogger logger, ZString source)
		{
			var import = (IXmlImportLogger)logger;
			if (!import.HasIgnoredModule)
			{
				import.HasIgnoredModule = true;
				logger.LogVerboseOnly(LogType.Information, Res.GetString("acd36819-6724-4f07-9758-b798d2f67523", "Import into {0} was skipped as it was the original source of this data.", source));
			}
		}
	}
}


