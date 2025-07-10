using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(EXTDATMessageProcessor))]
	sealed class EXTDATMessageProcessorTest : MessageProcessorAbstractTest<EXTDATMessageProcessor, AesInboundEDIMessage<IEXTDAT>>
	{
		protected override ZString MessageFriendlyName => "Export EXTDAT Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXTDAT>> Processor => new EXTDATMessageProcessor(logger);

		[TestDate(2023, 08, 17, 15, 18, 00)]
		public void TestNewEventAlwaysCreatdWhenCER_StatusSet()
		{
			ProcessMessage(message);
			var errEvent = report.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == A0116ATLASStatusCodeList.Codes._310);
			AssertEquals("NewEventRaised", new ZDateTime(2023, 08, 17, 15, 18, 00), errEvent.SL_EventTime);
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(report, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IEXTDAT)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("2 items created", 2, consignment.CusExitConsignmentItems.Count);
				AssertEquals("3 packages created", 3, header.CusExitConsignmentPackages.Count);
				AssertEquals("2 containers created", 2, header.CusExitContainers.Count);
			});
		}

		public void TestConsignmentItems()
		{
			ProcessMessage(message);

			var item1 = consignment.CusExitConsignmentItems[0];
			var item2 = consignment.CusExitConsignmentItems[1];
			var package1 = header.CusExitConsignmentPackages[0];
			var package2 = header.CusExitConsignmentPackages[1];

			CombineAssertions(() =>
			{
				AssertEquals("Declaration goods item number", (short)1, item1.CCI_LineNumber);
				AssertEquals("Reference number UCR", "001", item1.CCI_UniqueConsignmentReference);
				AssertEquals("Registration number (external)", "00001", item1.CCI_ReferenceNumber);
				AssertEquals("Gross mass", 100.00m, item1.CCI_GrossMass);
				AssertEquals("Net mass", 90.00m, item1.CCI_NetMass);

				AssertEquals("Declaration goods item number", (short)2, item2.CCI_LineNumber);
				AssertEquals("Reference number UCR", "002", item2.CCI_UniqueConsignmentReference);
				AssertEquals("Registration number (external)", "00002", item2.CCI_ReferenceNumber);
				AssertEquals("Gross mass", ZDecimal.Zero, item2.CCI_GrossMass);
				AssertEquals("Net mass", 110.00m, item2.CCI_NetMass);

				AssertEquals("Sequence number", (short)1, package1.CXP_Sequence);
				AssertEquals("Type of packages", "CR", package1.CXP_PackageType);
				AssertEquals("Number of packages", 10, package1.CXP_Quantity);
				AssertEquals("Shipping marks", "PACK1", package1.CXP_MarksAndNumbers);

				AssertEquals("Sequence number", (short)2, package2.CXP_Sequence);
				AssertEquals("Type of packages", "AM", package2.CXP_PackageType);
				AssertEquals("Number of packages", 8, package2.CXP_Quantity);
				AssertEquals("Shipping marks", "PACK2", package2.CXP_MarksAndNumbers);
			});
		}

		public void TestTransportEquipment()
		{
			ProcessMessage(message);

			var container1 = header.CusExitContainers[0];
			var container2 = header.CusExitContainers[1];

			CombineAssertions(() =>
			{
				AssertEquals("Container number 1", "CONT1", container1.CXN_ContainerNumber);
				AssertEquals("Seal1 1", "SEAL1", ((CusSeal)container1.AllSealNumbers.FirstOrDefault()).BK_SealNumber);
				AssertEquals("AllSealNumbers 1", "SEAL2", container1.AllSealNumbers.Skip(1).Cast<CusSeal>().Single().BK_SealNumber);
				Assert("IsEquipment 1", container1.CXN_IsEquipment);

				AssertEquals("Container number 2", "CONT2", container2.CXN_ContainerNumber);
				AssertNull("Seal1 2", container2.AllSealNumbers.FirstOrDefault());
				AssertEquals("AllSealNumbers count 2", 0, container2.AllSealNumbers.Count);
				Assert("IsEquipment 2", container2.CXN_IsEquipment);
			});
		}

		public void TestIsEquipment()
		{
			dataProviderMock.Setup(m => m.ContainerIndicator).Returns(1);

			ProcessMessage(message);

			var container1 = header.CusExitContainers[0];
			Assert(!container1.CXN_IsEquipment);
		}

		public void TestTransportEquipmentLinks()
		{
			ProcessMessage(message);

			var item1 = consignment.CusExitConsignmentItems[0];
			var item2 = consignment.CusExitConsignmentItems[1];

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Item 1", new[] { ("PACK1", "CONT1") },
					item1.CusExitConsignmentPackagePivots.Select(x => ((string)x.Package.CXP_MarksAndNumbers, (string)x.Container?.CXN_ContainerNumber)));
				AssertContainsExactElementsInAnyOrder("Item 2", new[] { ("PACK2", "CONT2"), ("PACK3", "CONT2") },
					item2.CusExitConsignmentPackagePivots.Select(x => ((string)x.Package.CXP_MarksAndNumbers, (string)x.Container?.CXN_ContainerNumber)));
			});
		}

		public void TestReportStatus()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("Report Status '310'", "310", report.CER_Status);
				AssertEquals("Report Type 'TRA'", "TRA", report.CER_Type);
				AssertEquals("Report Message Status 'ACC'", LogicalStatusList.Codes.Accepted, report.CER_MessageStatus);
				AssertEquals("Consignment Status '310'", "310", consignment.CXC_Status);
			});
		}

		public void TestGenerateEmail()
		{
			ProcessMessage(message);

			const string statusText = "Presentation is confirmed. The data of the export declaration have been provided.";

			var email = EnvProxy.Instance.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Exit Control  Message Response for {report.Header.CXH_JobReference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = string.Empty;
			var bodyMessageSummary = $"Your Exit Control Presentation for Job {report.Header.CXH_JobReference} is confirmed. The data of the export declaration have been provided.";

			var table = new HtmlTableCreator();
			table.WriteRow("MRN", "19DE000000001234E0");
			table.WriteRow("LRN", "DE123456789");
			table.WriteRow("Status Text", statusText);

			AssertEmailForSingleRecipientWithTable("Email sent", email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, table.ToHtml());
		}

		public void TestLogbookRegistrationNumber()
		{
			const string mrn = "19DE000000001234E0";
			ProcessMessage(message);
			var actualLogbookRegistrationNumber =
				message.GetNote(LogbookHelper.LogbookRegistrationNumberNoteDescription);
			AssertEquals(mrn, actualLogbookRegistrationNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();
			report.CER_Type = "PRE";

			report.CER_CXC_Consignment = consignment.PK;

			var outgoingMessage = CreateOriginalMessageLinkedToParent<AesInboundEDIMessage<IEXTCTL>>(report, "0123456");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXTDAT>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("0123456");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("19DE000000001234E0");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("DE123456789");

			var package1 = new Mock<IEXTDATPackage>();
			package1.Setup(m => m.SequenceNumber).Returns(1);
			package1.Setup(m => m.TypeOfPackages).Returns("CR");
			package1.Setup(m => m.NumberOfPackages).Returns(10);
			package1.Setup(m => m.ShippingMarks).Returns("PACK1");
			package1.Setup(m => m.DeclarationGoodsItemNumber).Returns(1);

			var package2 = new Mock<IEXTDATPackage>();
			package2.Setup(m => m.SequenceNumber).Returns(2);
			package2.Setup(m => m.TypeOfPackages).Returns("AM");
			package2.Setup(m => m.NumberOfPackages).Returns(8);
			package2.Setup(m => m.ShippingMarks).Returns("PACK2");
			package2.Setup(m => m.DeclarationGoodsItemNumber).Returns(2);

			var package3 = new Mock<IEXTDATPackage>();
			package3.Setup(m => m.SequenceNumber).Returns(3);
			package3.Setup(m => m.TypeOfPackages).Returns("CR");
			package3.Setup(m => m.NumberOfPackages).Returns(10);
			package3.Setup(m => m.ShippingMarks).Returns("PACK3");
			package3.Setup(m => m.DeclarationGoodsItemNumber).Returns(2);

			var item1 = new Mock<IEXTDATGoodsItem>();
			item1.Setup(m => m.DeclarationGoodsItemNumber).Returns(1);
			item1.Setup(m => m.ReferenceNumberUCR).Returns("001");
			item1.Setup(m => m.RegistrationNumberExternal).Returns("00001");
			item1.Setup(m => m.GrossMass).Returns(100.0m);
			item1.Setup(m => m.NetMass).Returns(90.0m);
			item1.Setup(m => m.Packages).Returns(new[] { package1.Object });

			var item2 = new Mock<IEXTDATGoodsItem>();
			item2.Setup(m => m.DeclarationGoodsItemNumber).Returns(2);
			item2.Setup(m => m.ReferenceNumberUCR).Returns("002");
			item2.Setup(m => m.RegistrationNumberExternal).Returns("00002");
			item2.Setup(m => m.GrossMass).Returns((decimal?)null);
			item2.Setup(m => m.NetMass).Returns(110.0m);
			item2.Setup(m => m.Packages).Returns(new[] { package2.Object, package3.Object });

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { item1.Object, item2.Object });

			var container1 = new Mock<IEXTDATTransportEquipment>();
			container1.Setup(m => m.ContainerIdentificationNumber).Returns("CONT1");
			container1.Setup(m => m.SealIdentifier).Returns(new[] { "SEAL1", "SEAL2" });
			container1.Setup(m => m.DeclarationGoodsItemNumber).Returns(new[] { 1 });

			var container2 = new Mock<IEXTDATTransportEquipment>();
			container2.Setup(m => m.ContainerIdentificationNumber).Returns("CONT2");
			container2.Setup(m => m.SealIdentifier).Returns(Array.Empty<string>());
			container2.Setup(m => m.DeclarationGoodsItemNumber).Returns(new[] { 1, 2 });

			dataProviderMock.Setup(m => m.TransportEquipment).Returns(new[] { container1.Object, container2.Object });

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXTDAT>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		Mock<AesInboundEDIMessage<IEXTDAT>> messageMock;
		AesInboundEDIMessage<IEXTDAT> message;
		Mock<IEXTDAT> dataProviderMock;

		CusExitHeader header;
		CusExitReport report;
		CusExitConsignment consignment;
	}
}
