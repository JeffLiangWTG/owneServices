using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	public class CustomsContainerDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportContainerAndSeals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) }));
				shipment.SetContainerCollection(GetContainersForTesting);
				CombineAssertions(() =>
				{
					var message = GetQueuedUniversalShipmentMessage(shipment);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					var newFactory = new BusinessObjectFactory();
					var declarationQuery = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					var declaration = newFactory.LoadTop1<JobDeclaration>(declarationQuery);
					AssertEquals("Container Count (1)", 1, declaration.CusContainers.Count);
					var container = declaration.CusContainers[0];
					AssertContainsExactElementsInAnyOrder("Seal Numbers", new[] { "12345", "98765" }, container.AdditionalSeals.Select(s => s.BK_SealNumber.ToString()));
				});
			}
		}

		public void TestExistingSealsReorderedByImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var existingDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				existingDeclaration.JE_MasterBill = "MB123";
				var existingContainer = existingDeclaration.CusContainers.AddNew();
				existingContainer.CO_ContainerNumber = "TEST123";
				var seal1 = existingContainer.AdditionalSeals.AddNew();
				seal1.BK_SealNumber = "98765";
				seal1.BK_SequenceNumber = 1;
				var seal2 = existingContainer.AdditionalSeals.AddNew();
				seal2.BK_SealNumber = "12345";
				seal2.BK_SequenceNumber = 2;
				Factory.SaveForTesting();
				AssertContainsExactElementsInExactOrder("Setup of existing seal numbers", new[] { "98765", "12345" }, existingDeclaration.CusContainers[0].AdditionalSeals.OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber.ToString()));

				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) }));
				shipment.SetContainerCollection(GetContainersForTesting);
				CombineAssertions(() =>
				{
					var message = GetQueuedUniversalShipmentMessage(shipment);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					var newFactory = new BusinessObjectFactory();
					var declarationQuery = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					var declaration = newFactory.LoadTop1<JobDeclaration>(declarationQuery);
					AssertEquals("Container Count (1)", 1, declaration.CusContainers.Count);
					AssertContainsExactElementsInExactOrder("Seal Numbers", new[] { "12345", "98765" }, declaration.CusContainers[0].AdditionalSeals.OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber.ToString()));
				});
			}
		}

		public void TestUnmatchedExistingSealsRemoved()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var existingDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				existingDeclaration.JE_MasterBill = "MB123";
				var existingContainer = existingDeclaration.CusContainers.AddNew();
				existingContainer.CO_ContainerNumber = "TEST123";
				var seal1 = existingContainer.AdditionalSeals.AddNew();
				seal1.BK_SealNumber = "98765";
				seal1.BK_SequenceNumber = 1;
				var seal2 = existingContainer.AdditionalSeals.AddNew();
				seal2.BK_SealNumber = "12345";
				seal2.BK_SequenceNumber = 2;
				var seal3 = existingContainer.AdditionalSeals.AddNew();
				seal3.BK_SealNumber = "55555";
				seal3.BK_SequenceNumber = 3;
				Factory.SaveForTesting();
				AssertEquals("Setup of additional seal Count (3)", 3, existingDeclaration.CusContainers[0].AdditionalSeals.Count);
				AssertContainsExactElementsInExactOrder("Setup of existing seal numbers", new[] { "98765", "12345", "55555" }, existingDeclaration.CusContainers[0].AdditionalSeals.OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber.ToString()));

				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) }));
				shipment.SetContainerCollection(GetContainersForTesting);
				CombineAssertions(() =>
				{
					var message = GetQueuedUniversalShipmentMessage(shipment);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					var newFactory = new BusinessObjectFactory();
					var declarationQuery = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					var declaration = newFactory.LoadTop1<JobDeclaration>(declarationQuery);
					AssertEquals("Container Count (1)", 1, declaration.CusContainers.Count);
					AssertEquals("Additional seal Count (2)", 2, declaration.CusContainers[0].AdditionalSeals.Count);
					AssertContainsExactElementsInExactOrder("Seal Numbers", new[] { "12345", "98765" }, declaration.CusContainers[0].AdditionalSeals.OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber.ToString()));
				});
			}
		}

		public void TestPopulateCountrySpecificData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			var dataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ThirdSeal = ZString.Empty };
			var reader = new CustomsContainerDataObjectReaderForTest(
				dataObject,
				new Mock<IXmlImportLogger>().Object,
				new Mock<UniversalDataObjectReaderHelper>(Factory, (ZString)"EUN", new ZString("EUN")).Object,
				declaration);

			AssertNoExceptionThrown("No any exception should be thrown if dataObject.AdditionalSealNumberCollection is null", () => reader.PopulateCountrySpecificData(cusContainer, dataObject));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			localCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface);
		}
		IDisposable localCountryCustomsInterface;

		protected override void TearDown()
		{
			base.TearDown();
			localCountryCustomsInterface?.Dispose();
		}

		DataObjectList<Container> GetContainersForTesting()
		{
			var containers = new List<Container>();
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			container.ContainerNumber = "TEST123";
			container.ThirdSeal = ZString.Empty;
			var seals = new List<SealNumber>();
			seals.Add(new SealNumber() { Number = "12345" });
			seals.Add(new SealNumber() { Number = "98765" });
			seals.Add(new SealNumber() { Number = ZString.Empty });
			container.SetAdditionalSealNumberCollection(() => { return seals; });
			containers.Add(container);
			return new DataObjectList<Container>(containers);
		}

		class CustomsContainerDataObjectReaderForTest : CustomsContainerDataObjectReader
		{
			public CustomsContainerDataObjectReaderForTest(Container containerDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobDeclaration declaration)
				: base(containerDataObject, logger, helper, declaration, null)
			{
			}

			new public void PopulateCountrySpecificData(CusContainer container, Container dataObject)
			{
				base.PopulateCountrySpecificData(container, dataObject);
			}
		}
	}
}

