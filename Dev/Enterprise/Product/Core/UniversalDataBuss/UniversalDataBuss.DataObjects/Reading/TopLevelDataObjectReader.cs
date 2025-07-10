using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public abstract class TopLevelDataObjectReader<TDataObject, TBusinessObject> : DataObjectReader<TDataObject, TBusinessObject>, ITopLevelDataObjectReader, ITopLevelDataObjectReaderForTemplateRecord
		where TDataObject : TopLevelDataObject
		where TBusinessObject : BusinessObject
	{
		internal TopLevelDataObjectReader(TDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		// Even though this is a top level data object reader, this object may not be the top level when importing.. ie DtbBooking when importing from a DtbConsolidation import
		bool isTopLevel;

		internal override void FinaliseImport(TBusinessObject targetBO)
		{
			base.FinaliseImport(targetBO);
			logger.LogAddOrUpdate(targetBO, IsNewBO, typeof(TDataObject));
			logger.FireDataImportedToBusinessObject(targetBO);

			if (!isTopLevel && LogChildTopLevelObjectsOnImport)
			{
				var xmlSessionTracker = logger as IXmlSessionTracker;
				if (xmlSessionTracker != null)
				{
					xmlSessionTracker.LogChildTopLevelObject(DataContextType, () => targetBO.GetUniversalDataContextManager().DataContextKey);
				}
			}
		}

		// Architecture Logs Top Level Business Objects Automatically (only true top level objects in the xml)
		// Override this if this object is to be logged too
		// Will only be logged if it is a child (see isTopLevel)
		protected virtual bool LogChildTopLevelObjectsOnImport
		{
			get { return false; }
		}

		protected abstract TBusinessObject GetExistingBusinessObjectUsingModuleSpecificBusinessRules();

		protected abstract IMatchingBusinessEntityFinder<TBusinessObject> GetCombinedReferenceMatcher();

		protected internal override void PopulateFromTopLevelObject(TBusinessObject targetBO)
		{
			if (IsImportJobCostingAllowed(targetBO))
			{
				ImportJobCostingWhereApplicable(dataObject as IJobCostingData, targetBO);
			}

			if (IsImportConsolCostsAllowed(targetBO))
			{
				ImportConsolCostsWhereApplicable(dataObject as IConsolCostsData, targetBO);
			}
			PopulateWorkflowRelatedProperties(dataObject, targetBO);
		}

		protected virtual bool IsImportJobCostingAllowed(TBusinessObject targetBO)
		{
			return true;
		}

		void ImportJobCostingWhereApplicable(IJobCostingData jobCostingData, BusinessObject targetBO)
		{
			if (jobCostingData != null && jobCostingData.JobCosting != null)
			{
				if ((jobCostingData.DataContext != null && jobCostingData.DataContext.CodesMappedToTarget) ||
					(logger.TopLevelDataContext != null && logger.TopLevelDataContext.CodesMappedToTarget))
				{
					var jobCostingAdapter = ObjectFactory.Get<IJobCostingAdapter>();
					jobCostingAdapter.ImportCharges(factory.BOFactory, logger, jobCostingData, targetBO.PK, targetBO.TablePrefix);
				}
				else
				{
					logger.LogBoth(LogType.Warning, Res.GetString("410acf61-d22a-44cb-9b4e-060e991cd65d", "{0} element was ignored. To import {0} data the {1} must contain a matching {2} and {3}, and the {4} must match a valid Company in this system. This can also be overridden by setting the {5} element to true.",
						"JobCosting", "DataContext", "EnterpriseID", "ServerID", "Company Code", "CodesMappedToTarget"));
				}
			}
		}

		protected virtual bool IsImportConsolCostsAllowed(TBusinessObject targetBO)
		{
			return false;
		}

		void ImportConsolCostsWhereApplicable(IConsolCostsData consolCostData, BusinessObject targetBO)
		{
			if (consolCostData != null && consolCostData.ConsolCosts != null)
			{
				if ((consolCostData.DataContext != null && consolCostData.DataContext.CodesMappedToTarget) ||
					(logger.TopLevelDataContext != null && logger.TopLevelDataContext.CodesMappedToTarget))
				{
					var consolCostsAdapter = ObjectFactory.Get<IConsolCostsAdapter>();
					consolCostsAdapter.ImportConsolCosts(factory.BOFactory, logger, consolCostData, targetBO.PK, targetBO.TablePrefix);
				}
				else
				{
					 logger.LogBoth(LogType.Warning, Res.GetString("410acf61-d22a-44cb-9b4e-060e991cd65d", "{0} element was ignored. To import {0} data the {1} must contain a matching {2} and {3}, and the {4} must match a valid Company in this system. This can also be overridden by setting the {5} element to true.",
						   "ConsolCosting", "DataContext", "EnterpriseID", "ServerID", Res.GetString("94fe86a6-9933-48e2-a636-c66a3135c41f", "Company Code"), "CodesMappedToTarget"));
				}
			}
		}

		void PopulateWorkflowRelatedProperties(IDataObject dataObject, BusinessObject businessObject)
		{
			ObjectFactory.Get<IUniversalExceptionReader>().PopulateExceptions(dataObject, businessObject, logger, factory);
		}

		protected bool HasRecipientRole(RecipientRoleType type)
		{
			return dataObject.HasRecipientRole(type);
		}

		internal IDataContextManager DataContextManager
		{
			get { return DataContextType.GetUniversalDataContextManager(); }
		}

		public abstract DataContextType DataContextType { get; }

		#region ITopLevelDataObjectReader Members

		BusinessObject ITopLevelDataObjectReader.GetExistingBusinessObject()
		{
			return GetExistingBusinessObject();
		}

		void ITopLevelDataObjectReader.ReadIntoBusinessObject(ref BusinessObject targetBO)
		{
			this.isTopLevel = false;
			var targetBOTyped = (TBusinessObject)targetBO;
			ReadIntoBusinessObjectCore(ref targetBOTyped);
			targetBO = targetBOTyped;
		}

		BusinessObject ITopLevelDataObjectReader.ReadIntoTopLevelBusinessObject()
		{
			this.isTopLevel = true;
			var businessObject = (TBusinessObject)null;
			ReadIntoBusinessObjectCore(ref businessObject);
			return businessObject;
		}

		IEnumerable<(string KeyValue, string KeySource)> ITopLevelDataObjectReader.ReadKeysForParallelism() => ReadKeysForParallelismCore();

		protected virtual IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelismCore()
		{
			yield break;
		}

		#endregion

		#region ITopLevelDataObjectReaderForTemplateRecord Members

		void ITopLevelDataObjectReaderForTemplateRecord.ReadDirectlyIntoBusinessObject(BusinessObject targetBizo)
		{
			var targetBizoTyped = (TBusinessObject)targetBizo;
			ReadDirectlyIntoBusinessObjectOverrideAllChecks(targetBizoTyped);
		}

		#endregion
	}
}
