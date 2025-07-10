using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class InvoiceDocumentPackTest : TestCaseWithFactory
	{
		public void TestInvoicesAreExcludedFromDocumentPacksCorrectly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_NoOriginalBills = 5;
			shipment.JS_NoCopyBills = 3;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment;

			var shipperDocPackMenu = Factory.New<StmMenuItem>();
			shipperDocPackMenu.SU_MenuName = "Shipper Document Pack (Sea)";

			var hblMenu = Factory.New<StmMenuItem>();
			hblMenu.SU_MenuName = "Bill Of Lading";

			shipment.JS_HouseBillOfLadingType = "FIA";

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = consignor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_JH = job.PK;

			var commandQuery = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Shipper Document Pack (Sea)");
			var command = Factory.LoadTop1<DocumentCommand>(commandQuery);
			command.SU_ContactType = ContactType.Consignor.Code;

			var pivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, "Invoice");
			var pivot = Factory.LoadTop1<StmMenuTemplatePivotBase>(pivotQuery);
			pivot.Template.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJobInvoice);

			command.Documents.Add(pivot);
			command.SU_IsDocPack = true;

			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var pack1 = new DocumentPack(command, shipment, null, null);
			Assert("Invoice must be not reversed when created.", !invoice.IsCancelled);
			AssertEquals("Pack should have the invoice included if it is not cancelled.", 1, pack1.Count(x => ((Report)x).IncludedInPrint));

			var reversing = new ARInvoiceReversing(invoice);
			reversing.Reverse();

			var pack2 = new DocumentPack(command, shipment, null, null);
			Assert("Invoice must be reversed when reversed.", invoice.IsCancelled);
			AssertEquals("Pack should exclude the invoice if it is cancelled.", 0, pack2.Count(x => ((Report)x).IncludedInPrint));

			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var pack3 = new DocumentPack(command, shipment, null, null);
			Assert("Invoice must be reversed when reversed.", invoice.IsCancelled);
			AssertEquals("Pack should include the cancelled invoice if the registry setting is turned to include it.", 1, pack3.Count(x => ((Report)x).IncludedInPrint));

			command.SU_IsDocPack = false;
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var pack4 = new DocumentPack(command, shipment, null, null);
			Assert("Invoice must be reversed when reversed.", invoice.IsCancelled);
			AssertEquals("Pack should include the cancelled invoice if it is not requesting a document pack", 1, pack4.Count(x => ((Report)x).IncludedInPrint));
		}

		public void TestInvoicesAreExcludedFromDocumentPacksWhenChildCommandIsNotDocPack()
		{
			var creator = new TestObjectCreator(Factory);

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "xxx";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = consignor.PK;
			address.OA_Code = "zzz";
			address.OA_Address1 = "test address";

			var consol = creator.CreateConsol();
			consol.JK_OA_ReceivingForwarderAddress = address.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;
			shipment.JS_HouseBillOfLadingType = "IAU";

			var job = creator.CreateJob(shipment);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = consignor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_JH = job.PK;

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			invoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = TransactionTypes.Invoice;
			invoice1.AH_OH = consignor.PK;
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_JH = job.PK;

			Factory.Save();

			var parentCommandQuery = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Consol Agent Pack (Sea)");
			parentCommandQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Consol");
			parentCommandQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Departure/Document Pack");
			var parentCommand = Factory.LoadTop1<DocumentCommand>(parentCommandQuery);
			Assert("Parent command should be doc pack.", parentCommand.SU_IsDocPack);

			var childCommandQuery = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice");
			childCommandQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");
			var childCommand = Factory.LoadTop1<DocumentCommand>(childCommandQuery);
			Assert("Child command should not be doc pack.", !childCommand.SU_IsDocPack);

			var wrapper = new ForwardingShipmentWrapperWithConsolAgent(shipment, consol.ReceivingForwarder);
			childCommand.Parent = wrapper;

			using (DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var pack1 = new DocumentPack(childCommand, childCommand.Parent, null, parentCommand);
				Assert("Invoice must be not reversed when created.", !invoice.IsCancelled);
				Assert("Invoice1 should not be reversed.", !invoice1.IsCancelled);
				AssertEquals("Pack should have the invoice included if it is not cancelled.", 2, pack1.Count(x => ((Report)x).IncludedInPrint));

				var reversing = new ARInvoiceReversing(invoice);
				reversing.Reverse();

				Factory.Save();

				var pack2 = new DocumentPack(childCommand, childCommand.Parent, null, parentCommand);
				Assert("Invoice must be reversed when reversed.", invoice.IsCancelled);
				Assert("Invoice1 should not be reversed.", !invoice1.IsCancelled);
				AssertEquals("Pack should exclude the invoice if it is cancelled.", 1, pack2.Count(x => ((Report)x).IncludedInPrint));
			}

			using (DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var pack3 = new DocumentPack(childCommand, childCommand.Parent, null, parentCommand);
				Assert("Invoice must be reversed when reversed.", invoice.IsCancelled);
				Assert("Invoice1 should not be reversed.", !invoice1.IsCancelled);
				AssertEquals("Pack should include the cancelled invoice if the registry setting is turned to include it.", 3, pack3.Count(x => ((Report)x).IncludedInPrint));
			}

			using (DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				parentCommand.SU_IsDocPack = false;

				var pack4 = new DocumentPack(childCommand, childCommand.Parent, null, parentCommand);
				Assert("Invoice must be reversed when reversed.", invoice.IsCancelled);
				Assert("Invoice1 should not be reversed.", !invoice1.IsCancelled);
				AssertEquals("Pack should include the cancelled invoice if it is not requesting a document pack", 3, pack4.Count(x => ((Report)x).IncludedInPrint));
			}
		}
	}
}
