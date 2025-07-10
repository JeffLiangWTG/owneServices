using CargoWise.Types;
using Enterprise.Client.Wow.Business;
using Enterprise.Client.Wow.CASSKIRKOrderIntegration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDocOrder))]
	class WowDocOrderTest : DocumentWrapperTestCase
	{
		public void TestIsValidBranch()
		{
			var wrapper = WowDocOrder.New(Order, Factory);
			Assert(!wrapper.IsValidBranch);
			WowDataRegistry.Instance.CASSKIRKDisplayOption = true;
			Assert(wrapper.IsValidBranch);
		}

		public void TestEstimatedDates()
		{
			OrderLine line1 = Order.OrderLines.AddNew();
			line1.JO_CustomDate1 = new ZDateTime(2009, 3, 16);
			line1.JO_CustomDate2 = new ZDateTime(2009, 5, 20);
			AssertEquals("EsitmatedDlvDate", line1.JO_CustomDate1, Wrapper.EsitmatedDlvDate);
			AssertEquals("EsitmatedArvDate", line1.JO_CustomDate2, Wrapper.EsitmatedArvDate);
		}

		public void TestAmendment()
		{
			AssertEquals("Amendment should be empty string as Order is not an woolworthsimportedorder", ZString.Empty, Wrapper.Amendment);
			WoolworthsImportedOrder importedOrder = Factory.NewWithValidTestData<WoolworthsImportedOrder>();
			importedOrder.IsUpdated = true;
			WowDocOrder wowWrapper = WowDocOrder.New(importedOrder, Factory);
			AssertEquals("Amendment", "*** Amendment ***", wowWrapper.Amendment);
			importedOrder.IsUpdated = false;
			AssertEquals("Amendment", ZString.Empty, wowWrapper.Amendment);
		}

		public void TestBuyerBranchName()
		{
			GlbBranch dummyBranch = SetupDummyBranch();
			OrgHeader buyer = TestHelper.Importer;
			buyer.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranch.PK;
			Order.BuyerPK = buyer.PK;
			AssertEquals("BuyerBranchName", Env.CurrentBranch.Name, Wrapper.BuyerBranchName);
			buyer.CompanyData.OB_GB_ControllingBranch = dummyBranch.PK;
			AssertEquals("BuyerBranchName", dummyBranch.GB_BranchName, Wrapper.BuyerBranchName);
		}

		public void TestReplenisherNotes()
		{
			AssertEquals("replenisher notes", ZString.Empty, Wrapper.ReplenisherNotes);
			Order.Notes.AddNew(false, CASSKIRKOrderIntegration.CASSKIRKConstant.ReplenishersEmailNoteType, "email@email.com");
			AssertEquals("replenisher notes", "(Email to email@email.com)", Wrapper.ReplenisherNotes);
			Order.Notes.RemoveAndDeleteAll();
			Order.Notes.AddNew(false, CASSKIRKOrderIntegration.CASSKIRKConstant.ReplenishersFaxNoteType, "13454");
			AssertEquals("replenisher notes", "(Fax to 13454)", Wrapper.ReplenisherNotes);
			Order.Notes.AddNew(false, CASSKIRKOrderIntegration.CASSKIRKConstant.ReplenishersEmailNoteType, "email@email.com");
			AssertEquals("replenisher notes", "(Email to email@email.com  or Fax to 13454)", Wrapper.ReplenisherNotes);
		}

		public void TestDestinationInfos()
		{
			Assert("Orderlines > 0", Order.OrderLines.Count > 0);
			Assert("OrderlineDeliveries of 1st order line > 0", Order.OrderLines[0].Deliveries.Count > 0);
			AssertEquals("DestinationPortName", "SYDNEY", Wrapper.DestinationPortName);
			OrgAddress deliveryAddr = Order.OrderLines[0].Deliveries[0].DeliveryPoint;
			AssertNotNull(deliveryAddr);
			DocAddress docDelAddress = DocAddress.New(deliveryAddr, Factory);
			AssertEquals("DestinationAddress", docDelAddress.PostalAddress, Wrapper.DestinationAddress);
			Order.OrderLines[0].Deliveries.DeleteAll();
			AssertEquals("DestinationPortName", ZString.Empty, Wrapper.DestinationPortName);
			AssertEquals("DestinationAddress", ZString.Empty, Wrapper.DestinationAddress);
		}

		protected override void SetUp()
		{
			TestHelper.SetupNoteType();
			Order = TestHelper.GetOrder();
			Wrapper = WowDocOrder.New(Order, Factory);
			base.SetUp();
		}

		GlbBranch SetupDummyBranch()
		{
			GlbBranch dummyBranch = Factory.NewWithValidTestData<GlbBranch>();
			dummyBranch.GB_Code = "ABC";
			dummyBranch.GB_BranchName = "ABC Branch";
			dummyBranch.GB_Address1 = "abc street 1";
			dummyBranch.GB_RL_NKHomePort = "AUSYD";
			return dummyBranch;
		}

		Order Order;
		WowDocOrder Wrapper;

		CASSKIRKTestHelper TestHelper
		{
			get
			{
				return (testHelper) ?? (testHelper = new CASSKIRKTestHelper(Factory));
			}
		}

		CASSKIRKTestHelper testHelper;
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { Wrapper };
		}
	}
}
