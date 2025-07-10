using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	[TestsSubclassesOf(typeof(IShipmentDataContextManager), ExcludePrivate = true)]
	public abstract class ShipmentDataContextManagerTestCase<T, U> : DataContextManagerTestCase<T, U>
		where T : DataContextManager<U>, IShipmentDataContextManager, new()
		where U : BusinessObject
	{
		public void TestDefaultDataTargetFromRecipientRole()
		{
			var manager = new T();
			if (manager.ManagesShipments)
			{
				var shipmentForRecipientRole = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

				foreach (RecipientRoleType recipientRoleType in Enum.GetValues(typeof(RecipientRoleType)))
				{
					var allRecipientServiceTypesIncludingNone = Enum.GetValues(typeof(ServiceCodeType)).Cast<ServiceCodeType?>().Append(null);
					foreach (ServiceCodeType? recipientServiceType in allRecipientServiceTypesIncludingNone)
					{
						shipmentForRecipientRole.DataContext = DataContextFactory.New();
						shipmentForRecipientRole.DataContext.SetWorkflowInfo(new WorkflowInfo()
						{
							EventType = null,
							EventUser = null,
							EventBranch = null,
							EventDepartment = null,
							ActionPurpose = null,
							TriggerDescription = "",
							TriggerCount = 0,
							TriggerDate = ZDateTimeOffset.Empty,
							TriggerReference = "",
							TriggerType = TriggerType.Manual,
							RecipientRoles = new[] { new RecipientRoleDetail() { Type = recipientRoleType, ServiceCode = recipientServiceType } }
						});

						foreach (DataContextType dataSource in Enum.GetValues(typeof(DataContextType)))
						{
							if (dataSource != manager.DataContextType)
							{
								shipmentForRecipientRole.DataContext.AddDataSource(dataSource, null);
								ConfigureDataContext(shipmentForRecipientRole.DataContext);
							}
						}

						MakeShipmentUsableForThisRole(recipientRoleType, shipmentForRecipientRole);
						manager.DefaultDataTargetFromRecipientRole(shipmentForRecipientRole.DataContext, null);
						var shouldContainNewDataTarget = SupportedRecipientRoleTypes.Contains(recipientRoleType) && SupportedRecipientServices(recipientRoleType).Contains(recipientServiceType);
						AssertEquals("The Recipient Role Type [" + recipientRoleType.ToString() + "] was not handled correctly", shouldContainNewDataTarget, shipmentForRecipientRole.GetMatchingDataTarget(manager.DataContextType) != null);
					}
				}
			}
			else
			{
				Assert("This manager does not manage Shipments, therefore this test is not relevant.", true);
			}
		}

		protected abstract RecipientRoleType[] SupportedRecipientRoleTypes { get; }

		protected virtual ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return Enum.GetValues(typeof(ServiceCodeType)).Cast<ServiceCodeType?>().Append(null).ToArray();
		}

		protected virtual void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
		}

		public void TestDataContextManagerAcceptsCorrectShipments()
		{
			try
			{
				var manager = new T();
				if (manager.ManagesShipments)
				{
					var logger = new DummyLogger();
					var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
					AssertEquals(false, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));

					SetupDataForDataContextManagerTestCase();

					using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ValidPopulatedUniversalShipmentXML)))
					{
						ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
					}

					var hasCorrectDataTarget = shipment.DataContext?.DataTargetCollection?.Select(t => t.Type).Contains(manager.DataContextType.ToString()) ?? false;
					if (!hasCorrectDataTarget)
					{
						shipment.DataContext = DataContextFactory.New();
						shipment.DataContext.AddDataTarget(manager.DataContextType, null);
						ConfigureDataContext(shipment.DataContext);
					}

					logger.TopLevelDataObject = shipment;
					var universalFactory = new UniversalObjectFactory();
					var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(new BusinessObjectFactory());
					using (whsHelper.UseAllocationEngineMock())
					{
						AssertEquals(ManagerChecksDataTargetToImport, manager.UseIncomingShipmentData(shipment, logger, universalFactory));
					}
					if (SaveAfterUseIncomingShipment)
					{
						universalFactory.SaveForTesting();
					}
				}
				else
				{
					Assert("This manager does not manage Shipments, therefore this test is not relevant.", true);
				}
			}
			finally
			{
				CleanUpForDataContextManagerTestCase();
			}
		}

		protected virtual bool SaveAfterUseIncomingShipment { get; } = true;

		protected virtual bool ManagerChecksDataTargetToImport
		{
			get { return true; }
		}

		public void TestDataContextReaderCanAlwaysGetBackToTheRightTypeOfDataContextManager()
		{
			var manager = new T();
			if (manager.ManagesShipments)
			{
				var readerGetterMethod = typeof(T).GetMethod("GetShipmentDataObjectReader", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(UniversalShipment), typeof(IXmlImportLogger), typeof(UniversalObjectFactory) }, null);
				var logger = new DummyLogger();
				var factory = new UniversalObjectFactory();
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				SetupDataForDataContextManagerTestCase();

				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ValidPopulatedUniversalShipmentXML)))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				}

				shipment.DataContext = DataContextFactory.New();
				shipment.DataContext.AddDataTarget(manager.DataContextType, null);
				ConfigureDataContext(shipment.DataContext);
				logger.TopLevelDataObject = shipment;

				var reader = readerGetterMethod.Invoke(manager, new object[] { shipment, logger, factory }) as ShipmentDataObjectReader<U>;
				if (reader != null)
				{
					AssertEquals("reader.DataContextType", manager.DataContextType, reader.DataContextType);
				}
				else
				{
					AssertEquals("It's ok to not have a reader as long as UseIncomingShipmentData returns false.", false, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
				}
			}
			else
			{
				Assert("This manager does not manage Shipments, therefore this test is not relevant.", true);
			}
		}

		protected virtual void SetupDataForDataContextManagerTestCase()
		{
		}

		protected virtual void CleanUpForDataContextManagerTestCase()
		{
		}

		protected virtual void ConfigureDataContext(IDataContextDataObject dataContext)
		{
		}

		protected abstract string ValidPopulatedUniversalShipmentXML { get; }
	}
}
