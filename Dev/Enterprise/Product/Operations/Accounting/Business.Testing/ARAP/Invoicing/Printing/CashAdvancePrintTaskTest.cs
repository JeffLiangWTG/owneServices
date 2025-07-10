using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	public class CashAdvancePrintTaskTest : TestCaseWithFactory
	{
		public void TestInvoicePrintTaskSetMenuCommandCorrectlyForDocBuilderInvoice()
		{
			var docBuilderCommandInShipmentContext = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, CashAdvancePrintTask.DocumentMenuName), new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment))));
			var task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { CashAdvanceRequestHeader1 });
			AssertEquals("invoice print task should use the docBuilder invoice menu in the AR invoice context", docBuilderCommandInShipmentContext.PK, task.Task_ForTestOnly.ParentMenuCommand.PK);
		}

		public void TestCreatePrintTaskWithData()
		{
			var task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { CashAdvanceRequestHeader1 });
			AssertEquals(1, task.TaskCount);
		}

		public void TestCreatePrintTaskWithoutData()
		{
			var task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { });
			AssertNull(task.Task_ForTestOnly);
		}

		public void TestAddRequestsToPack()
		{
			var task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 });
			AssertEquals("Should have 1 document pack in print task since all requests have the same org", 1, task.TaskCount);
			AssertEquals("Should have 2 documents in the pack", 2, GetDocumentPack(task, 0).Count);

			task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { CashAdvanceRequestHeader1, CashAdvanceRequestHeader3 });
			AssertEquals("Should have 2 document pack in print task since requests have different same org", 2, task.TaskCount);
			AssertEquals("Should have 1 documents in the pack", 1, GetDocumentPack(task, 0).Count);
			AssertEquals("Should have 1 documents in the pack", 1, GetDocumentPack(task, 1).Count);

			task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2, CashAdvanceRequestHeader3 });
			AssertEquals("Should have 3 document pack in print task since requests have different orgs", 3, task.TaskCount);
			AssertEquals("Should have 1 documents in the pack", 1, GetDocumentPack(task, 0).Count);
			AssertEquals("Should have 1 documents in the pack", 1, GetDocumentPack(task, 1).Count);
			AssertEquals("Should have 1 documents in the pack", 1, GetDocumentPack(task, 2).Count);
		}

		public void TestGetDocumentCommandForRequest()
		{
			var documentCommand = CashAdvancePrintTask.GetDocumentCommandForRequest(CashAdvanceRequestHeader1);
			AssertNotNull(documentCommand);

			documentCommand = CashAdvancePrintTask.GetDocumentCommandForRequest(CashAdvanceRequestHeader4);
			AssertNotNull("expect NOT Null due to fallback logic can get command from request, although parent Job type doesn't have advance payment document menu", documentCommand);

			documentCommand = CashAdvancePrintTask.GetDocumentCommandForRequest(CashAdvanceRequestHeader5);
			AssertNull("expect Null due to parent Job type doesn't support IDocumentSupportable", documentCommand);
		}

		public void TestMarkCashAdvanceRequestPrinted_WhenReturnNotNonePreviewUserCancelled_ShouldMarkCashAdvanceHeaderAsPrinted()
		{
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.Print, true, "advance payment request's Printed should be marked as true");
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.TakenFromContact, true, "advance payment request's Printed should be marked as true");
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.Disk, true, "advance payment request's Printed should be marked as true");
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.Auto, true, "advance payment request's Printed should be marked as true");
		}

		public void TestMarkCashAdvanceRequestPrinted_WhenReturnNonePreviewUserCancelled_ShouldNotMarkCashAdvanceHeaderAsPrinted()
		{
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.Preview, false, "advance payment request's Printed should not be marked as true");
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.None, false, "advance payment request's Printed should not be marked as true");
			TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination.UserCancelled, false, "advance payment request's Printed should not be marked as true");
		}

		void TestMarkCashAdvanceRequestPrinted(DeliveryInstructionDestination deliveryInstructionDestination, bool expectedResult, string message)
		{
			var cashAdvanceRequestHeader = ObjectCreator.CreateCashAdvanceRequestHeader(Job1.PK, ObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 10m, 10m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested, Factory);
			Factory.Save();

			var task = new CashAdvancePrintTask(new List<AccCashAdvanceRequestHeader>() { cashAdvanceRequestHeader });
			var taskMock = new Mock<MockDocumentPrintSet>();

			taskMock.Setup(m => m.RunWithPartialInstructions(It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(deliveryInstructionDestination);
			taskMock.Setup(m => m.Count).Returns(1);
			task.Task_ForTestOnly = taskMock.Object;

			task.Run();

			AssertEquals(message, expectedResult, cashAdvanceRequestHeader.CAH_Printed);
		}

		#region Implementation

		AccCashAdvanceRequestHeader CashAdvanceRequestHeader1;
		AccCashAdvanceRequestHeader CashAdvanceRequestHeader2;
		AccCashAdvanceRequestHeader CashAdvanceRequestHeader3;
		AccCashAdvanceRequestHeader CashAdvanceRequestHeader4;
		AccCashAdvanceRequestHeader CashAdvanceRequestHeader5;
		JobHeader Job1;
		JobHeader Job2;
		JobHeader Job3;
		JobHeader Job4;

		protected override void SetUp()
		{
			base.SetUp();
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var dummyBO = Factory.NewWithValidTestData<DummyBusinessObject>();

			Job1 = ObjectCreator.CreateJob("S00001000", null, 0, null, 0);
			Job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Job1.JH_ParentID = shipment1.PK;
			Job2 = ObjectCreator.CreateJob("S00001111", null, 0, null, 0);
			Job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Job2.JH_ParentID = shipment2.PK;
			Job3 = ObjectCreator.CreateJob(quickBooking, false);
			Job4 = ObjectCreator.CreateJob("DM001", null, 0, null, 0);
			Job4.JH_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			Job4.JH_ParentID = dummyBO.PK;
			Factory.Save();

			CashAdvanceRequestHeader1 = ObjectCreator.CreateCashAdvanceRequestHeader(Job1.PK, ObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 10m, 10m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader2 = ObjectCreator.CreateCashAdvanceRequestHeader(Job2.PK, ObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 100m, 100m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader3 = ObjectCreator.CreateCashAdvanceRequestHeader(Job1.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 200m, 200m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader4 = ObjectCreator.CreateCashAdvanceRequestHeader(Job3.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 200m, 200m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader5 = ObjectCreator.CreateCashAdvanceRequestHeader(Job4.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 200m, 200m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		DocumentPack GetDocumentPack(CashAdvancePrintTask task, int index)
		{
			return task.Task_ForTestOnly[index];
		}

		public class MockDocumentPrintSet : DocumentPrintSet
		{
			public MockDocumentPrintSet() : base(null)
			{
			}
		}
		#endregion
	}
}
