using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectFetchStrategy : IBusinessObjectFetchStrategy
	{
		public BusinessObjectFetchStrategy(BusinessObject businessObject)
		{
			BusinessObject = Argument.NotNull(businessObject, "businessObject");
			additionalFetchStrategies = (businessObject as IAdditionalBusinessObjectFetchStrategyProvider)?.GetFetchStrategies()?.ToArray();
		}
		protected readonly BusinessObject BusinessObject;
		readonly IBusinessObjectFetchStrategy[] additionalFetchStrategies;

		protected BusinessObjectFactory Factory
		{
			get { return BusinessObject.Factory; }
		}

		#region FetchForLoad

		public void FetchForLoad()
		{
			FetchForLoadCore();
			if (BusinessObject is IAddInfoChildSupporter supporter)
			{
				var childForeignKeyColumn = supporter.ChildForeignKeyColumn;
				if (childForeignKeyColumn != null)
				{
					Factory.AddFetchHint(childForeignKeyColumn, supporter.PK);
				}
			}
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForLoad();
				}
			}
		}

		protected virtual void FetchForLoadCore()
		{
		}

		#endregion

		#region FetchForLoadChildEditableObjects

		public void FetchForLoadChildEditableObjects()
		{
			FetchForLoadChildEditableObjectsCore();
			if ((BusinessObject as IAddInfoChildSupporter)?.AddInfoChild is BusinessObject addInfoChild)
			{
				addInfoChild.FetchStrategy?.FetchForLoadChildEditableObjects();
			}
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForLoadChildEditableObjects();
				}
			}
		}

		protected virtual void FetchForLoadChildEditableObjectsCore()
		{
		}

		#endregion

		#region FetchForFactorySaveBeforeTransaction

		public void FetchForFactorySaveBeforeTransaction()
		{
			FetchForFactorySaveBeforeTransactionCore();
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForFactorySaveBeforeTransaction();
				}
			}
		}

		protected virtual void FetchForFactorySaveBeforeTransactionCore()
		{
		}

		#endregion

		#region FetchForFactorySave

		public void FetchForFactorySave()
		{
			FetchForFactorySaveCore();
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForFactorySave();
				}
			}
		}

		protected virtual void FetchForFactorySaveCore()
		{
			if (BusinessObject.HasChanges)
			{
				foreach (IBusinessObjectStrategy strategy in BusinessObject.Strategies)
				{
					ISavingFetchStrategy savingFetchStrategy = strategy as ISavingFetchStrategy;
					if (savingFetchStrategy != null)
					{
						savingFetchStrategy.FetchForSaving(BusinessObject);
					}
				}
			}
		}

		#endregion

		#region FetchForValidate

		public void FetchForValidate()
		{
			var fetchDetail = GetFetchDetail();
			var rowFactory = RowFactory;
			if (fetchDetail == null || !fetchDetail.HasRunFetchForValidate ||
				rowFactory != null && fetchDetail.QueryCacheClearCount < rowFactory.QueryCacheClearCount)
			{
				FetchForValidateCore();
				if ((BusinessObject as IAddInfoChildSupporter)?.AddInfoChild is BusinessObject addInfoChild)
				{
					addInfoChild.FetchStrategy?.FetchForValidate();
				}
				if (additionalFetchStrategies != null)
				{
					foreach (var fetchStrategy in additionalFetchStrategies)
					{
						fetchStrategy.FetchForValidate();
					}
				}
				if (fetchDetail == null)
				{
					fetchDetail = new FetchDetail();
					FetchDetailDictionary.Add(BusinessObject, fetchDetail);
				}
				fetchDetail.HasRunFetchForValidate = true;

				if (rowFactory != null)
				{
					fetchDetail.QueryCacheClearCount = rowFactory.QueryCacheClearCount;
				}
			}
		}

		protected virtual void FetchForValidateCore()
		{
		}

		#endregion

		#region FetchForView
		public void FetchForView(TableColumn[] columns)
		{
			FetchForViewCore(columns);
			if ((BusinessObject as IAddInfoChildSupporter)?.AddInfoChild is BusinessObject addInfoChild)
			{
				addInfoChild.FetchStrategy?.FetchForView(columns);
			}
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForView(columns);
				}
			}
		}

		protected virtual void FetchForViewCore(TableColumn[] columns)
		{
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			foreach (var tableColumn in columns)
			{
				if (tableColumn.TableName.Length > 0)
				{
					var schemaColumn = schemaResolver.GetSchemaColumnSafe(tableColumn.ColumnName, BusinessObject.TableName);
					if (schemaColumn != null && schemaColumn.ColumnType == SchemaColumnType.Guid)
					{
						var key = (ZGuid)BusinessObject[tableColumn.ColumnName];
						var pkSchemaColumn = schemaResolver.GetPkColumn(tableColumn.TableName);
						BusinessObject.Factory.AddFetchHint(pkSchemaColumn, key);
					}
				}
			}
		}
		#endregion

		#region FetchForDelete

		public void FetchForDelete()
		{
			FetchForDeleteCore();
			if ((BusinessObject as IAddInfoChildSupporter)?.AddInfoChild is BusinessObject addInfoChild)
			{
				addInfoChild.FetchStrategy?.FetchForDelete();
			}
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForDelete();
				}
			}
		}

		protected virtual void FetchForDeleteCore()
		{
			if (Factory != null)
			{
				foreach (IBusinessObjectStrategy strategy in BusinessObject.Strategies)
				{
					DeleteChecker checker = strategy as DeleteChecker;
					if (checker != null)
					{
						checker.AddFetchHint(BusinessObject);
					}
				}
			}
		}

		#endregion

		#region FetchForBind

		public void FetchForBind()
		{
			FetchForBindCore();
			if (additionalFetchStrategies != null)
			{
				foreach (var fetchStrategy in additionalFetchStrategies)
				{
					fetchStrategy.FetchForBind();
				}
			}
		}

		protected virtual void FetchForBindCore()
		{
		}

		#endregion

		#region Implementation

		RowFactory RowFactory
		{
			get
			{
				var factory = Factory;
				return factory != null ? factory.RowFactory : null;
			}
		}

		Dictionary<BusinessObject, FetchDetail> FetchDetailDictionary
		{
			get
			{
				var factory = Factory;
				return factory != null ? factory.GetCachedValue("BusinessObjectFetchStrategy_FetchDetail", delegate
				{
					return new Dictionary<BusinessObject, FetchDetail>();
				}) : new Dictionary<BusinessObject, FetchDetail>();
			}
		}

		FetchDetail GetFetchDetail()
		{
			FetchDetail details;
			return FetchDetailDictionary.TryGetValue(BusinessObject, out details) ? details : null;
		}

		class FetchDetail
		{
			public bool HasRunFetchForValidate;
			public int QueryCacheClearCount;
		}

		#endregion
	}
}
