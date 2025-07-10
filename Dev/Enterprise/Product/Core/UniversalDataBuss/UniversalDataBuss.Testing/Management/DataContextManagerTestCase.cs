using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using UniversalCollectionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalComplianceReport = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalSchedule = Enterprise.UniversalDataBuss.DataObjects.Universal.Schedule;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	[TestsSubclassesOf(typeof(DataContextManager<>))]
	public abstract class DataContextManagerTestCase<T, U> : TestCaseWithFactoryAndMessagingHelpers
		where T : DataContextManager<U>, new()
		where U : BusinessObject
	{
		public void TestAttributeIsOnBusinessObject() => TestAttributeIsOnBusinessObjectCore();

		protected virtual void TestAttributeIsOnBusinessObjectCore()
		{
			var attribute = typeof(U).GetAttribute<UniversalDataContextAttribute>();
			AssertNotNull("Must have UniversalDataContextAttribute applied.", attribute);
			AssertEquals("DataContextType matches on Attribute and DataContextManager", attribute.DataContextType, new T().DataContextType);
		}

		public void TestBusinessObjectImplementsIJobNumber() => TestBusinessObjectImplementsIJobNumberCore();

		protected virtual void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("Workflow requires IJobNumber on the Top Level BusinessObject type so the (*JobNumber*) macro works on FileNames and in the Email Subject from EDI Communications Modes.",
				typeof(IJobNumber).IsAssignableFrom(typeof(U)));
		}

		public void TestUniversalShipmentTopLevelDataObjectCreatesTheDataContext()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as IShipmentDataContextManager;
			if (manager != null && manager.ManagesShipments)
			{
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, businessObject)));
				if (writer != null)
				{
					var dataContext = ((UniversalShipment)writer.GetDataObject(businessObject)).DataContext as DataObjects.Universal._2011_11.DataContext;
					AssertNotNull("DataContext", dataContext);
					AssertNotNull("DataSourceCollection", dataContext.DataSourceCollection);
					var expectedDataSource = GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(manager);
					AssertEquals("dataContext.DataSourceCollection", expectedDataSource, dataContext.GetDataSources());
					AssertNull("Company should not be filled in by the Top Level Data Object", dataContext.Company);
					AssertNull("Enterprise ID should not be filled in by the Top Level Data Object", dataContext.EnterpriseID);
					AssertNull("ServerID should not be filled in by the Top Level Data Object", dataContext.ServerID);
				}
				else
				{
					Assert("This manager does not support writing, therefore this test is not relevant.", true);
				}
			}
			else
			{
				Assert("This manager does not manage Shipments, therefore this test is not relevant.", true);
			}
		}

		public void TestUniversalEventTopLevelDataObjectCreatesTheDataContext()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as IEventDataContextManager;
			if (manager != null && manager.ManagesEvents)
			{
				var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, businessObject)));
				var logBO = businessObject.GetLogs().AddNew(Events.Dehire);
				var dataContext = ((UniversalEvent)writer.GetDataObject(logBO)).DataContext as DataObjects.Universal._2011_11.DataContext;
				AssertNotNull("DataContext", dataContext);
				AssertNotNull("DataSourceCollection", dataContext.DataSourceCollection);
				AssertEquals("DataSourceCollection should contain one reference", 1, dataContext.DataSourceCollection.Count);
				AssertNotNull("Reference should contain the right type and non empty Key", dataContext.DataSourceCollection.FirstOrDefault(r => r.Type.GetValueOrDefault() == manager.DataContextType.ToString() && r.Key.GetValueOrDefault() == manager.DataContextKey));
				AssertNull("Company should not be filled in by the Top Level Data Object", dataContext.Company);
				AssertNull("Enterprise ID should not be filled in by the Top Level Data Object", dataContext.EnterpriseID);
				AssertNull("ServerID should not be filled in by the Top Level Data Object", dataContext.ServerID);
			}
			else
			{
				Assert("This DataContextManager doesn't manage events", true);
			}
		}

		public void TestUniversalCollectionBatchTopLevelDataObjectCreatesTheDataContext()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as ITransactionBatchDataContextManager;
			if (manager != null && manager.ManagesTransactionBatches)
			{
				var writer = manager.GetTransactionBatchDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, businessObject)));
				if (writer != null)
				{
					var dataContext = ((UniversalCollectionBatch)writer.GetDataObject(businessObject)).DataContext;
					AssertNotNull("DataContext", dataContext);
					AssertNotNull("DataSourceCollection", dataContext.DataSourceCollection);
					var expectedDataSource = GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(manager);
					AssertEquals("dataContext.DataSourceCollection", expectedDataSource, dataContext.GetDataSources());
				}
				else
				{
					Assert("This manager does not support writing, therefore this test is not relevant.", true);
				}
			}
			else
			{
				Assert("This manager does not manage Transactions, therefore this test is not relevant.", true);
			}
		}

		public void TestUniversalTransactionTopLevelDataObjectCreatesTheDataContext()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as ITransactionDataContextManager;
			if (manager != null && manager.ManagesTransactions)
			{
				var writer = manager.GetTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, businessObject)));
				var dataContext = ((UniversalTransaction)writer.GetDataObject(businessObject)).DataContext;
				AssertNotNull("DataContext", dataContext);
				AssertNotNull("DataSourceCollection", dataContext.DataSourceCollection);
				var expectedDataSource = GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(manager);
				AssertEquals("dataContext.DataSourceCollection", expectedDataSource, dataContext.GetDataSources());
			}
			else
			{
				Assert("This manager does not manage Transactions, therefore this test is not relevant.", true);
			}
		}

		public void TestUniversalComplianceReportTopLevelDataObjectCreatesTheDataContext()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as ITransactionBatchDataContextManager;
			if (manager != null && manager.ManagesTransactionBatches)
			{
				var writer = manager.GetTransactionBatchDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, businessObject)));
				if (writer != null)
				{
					var dataContext = ((UniversalComplianceReport)writer.GetDataObject(businessObject)).DataContext;
					AssertNotNull("DataContext", dataContext);
					AssertNotNull("DataSourceCollection", dataContext.DataSourceCollection);
					var expectedDataSource = GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(manager);
					AssertEquals("dataContext.DataSourceCollection", expectedDataSource, dataContext.GetDataSources());
				}
				else
				{
					Assert("This manager does not support writing, therefore this test is not relevant.", true);
				}
			}
			else
			{
				Assert("This manager does not manage Transactions, therefore this test is not relevant.", true);
			}
		}

		public void TestUniversalScheduleTopLevelDataObjectCreatesTheDataContext()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as IScheduleDataContextManager;
			if (manager != null && manager.ManagesSchedules)
			{
				var writer = manager.GetScheduleDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, businessObject)));
				var dataContext = ((UniversalSchedule)writer.GetDataObject(businessObject)).DataContext;
				AssertNotNull("DataContext", dataContext);
				AssertNotNull("DataSourceCollection", dataContext.DataSourceCollection);
				var expectedDataSource = GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(manager);
				AssertEquals("dataContext.DataSourceCollection", expectedDataSource, dataContext.GetDataSources());
			}
			else
			{
				Assert("This manager does not manage Schedules, therefore this test is not relevant.", true);
			}
		}

		public void TestManagerIsLoadableViaSpring() => TestManagerIsLoadableViaSpringCore();
		protected virtual void TestManagerIsLoadableViaSpringCore()
		{
			var businessObject = GetNewBusinessObjectForTesting();
			var manager = businessObject.GetUniversalDataContextManager() as T;
			AssertNotNull("Should be able to obtain UniversalDataContextManager via UniversalDataContextAttribute and extension method using spring.", manager);
			AssertEquals("manager.ParentBO should be set via extension method.", businessObject, manager.ParentBO);
			AssertEquals("Returned Manager should be right type.", typeof(T), manager.GetType());
		}

		#region Implementation

		protected virtual string GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(IDataContextManager manager)
		{
			return manager.DataContextType.ToString() + " [" + manager.DataContextKey + "]";
		}

		protected virtual U GetNewBusinessObjectForTesting()
		{
			return Factory.NewWithValidTestData<U>();
		}

		#endregion
	}

	public class DataContextManagerTest : TestCaseWithFactory
	{
		public void TestCacheDataContextManagerForManyTypes()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			AssertNull("Bizo does not have a data context manager", bizo.GetUniversalDataContextManager());

			var manager = DataContextType.DocManager.GetUniversalDataContextManager() as IDataContextManagerForManyTypes;
			using (bizo.SetUniversalDataContextManagerForManyTypes(manager))
			{
				AssertNotNull(bizo.GetUniversalDataContextManager());
				AssertExceptionThrown<InvalidOperationException>(() => bizo.SetUniversalDataContextManagerForManyTypes(manager));
			}

			AssertNull(bizo.GetUniversalDataContextManager());
		}

		public void TestSetDataManagerContextKeyOnObject()
		{
			var bizo1 = Factory.New<DummyBusinessObject>();
			var bizo2 = Factory.New<DummyBusinessObject>();
			bizo1.SetDataManagerContextKey("1");
			bizo2.SetDataManagerContextKey("2");
			AssertEquals("1", bizo1.GetDataManagerContextKey());
			AssertEquals("2", bizo2.GetDataManagerContextKey());
			bizo1.SetDataManagerContextKey("3");
			AssertEquals("3", bizo1.GetDataManagerContextKey());

			Factory.Save();
			AssertEquals("Reset Cache on save", ZString.Empty, bizo1.GetDataManagerContextKey());
			AssertEquals("Reset Cache on save", ZString.Empty, bizo2.GetDataManagerContextKey());
		}
	}
}
