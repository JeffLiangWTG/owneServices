using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Messaging.Business;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class RemovalAndFallbackMessagingTests : TestCaseWithFactory
	{
		public void TestCreateCusdecsFromExistingCusunderbonds_BasicSplit()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000AAA";
			basic.ShipmentDescriptionCode = "T";
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			var fbk1 = basic.FBKs.AddNew();
			fbk1.SplitReferenceToWhichThisRemovalPertains = "01";
			Factory.Save();
			var shutup = new Biz.SendsMessagesToCustomsShutterUpperer();
			UnderbondSenderHelper.SendMessage(fbk1, new CcsukTransmissionMessageFunction.CUSDEC.FBK(), shutup);
			AssertEquals(1, basic.Messages.Count);
			AssertContains("FBK", basic.Messages[0].EM_MessageText);
			AssertContains("ACD::01", basic.Messages[0].EM_MessageText);
		}

		public void TestCreateCusdecsFromExistingCusunderbonds_House()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000AAA";
			var hawb = mawb.ChildBills.AddNew();
			var fbk1 = hawb.FBKs.AddNew();
			var split = hawb.Splits.AddNew();
			split.SplitReference = "01";
			fbk1.SplitReferenceToWhichThisRemovalPertains = "01";
			Factory.Save();
			var shutup = new Biz.SendsMessagesToCustomsShutterUpperer();
			UnderbondSenderHelper.SendMessage(fbk1, new CcsukTransmissionMessageFunction.CUSDEC.FBK(), shutup);
			AssertEquals(0, mawb.Messages.Count);
			AssertEquals(1, hawb.Messages.Count);
			AssertContains("FBK", hawb.Messages[0].EM_MessageText);
			AssertContains("ACD::01", hawb.Messages[0].EM_MessageText);
		}

		public void TestCannotReSendFromCancelledUnderbond()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000AAA";
			var hawb = mawb.ChildBills.AddNew();
			var iar = hawb.IARs.AddNew();
			iar.C4_SendersMessageReference = "Something unsaved";
			iar.C4_Status = EDIMessage.Status.Cancelled;
			iar.LicenseRestrictionInd = "N";
			Factory.Save();
			var notifier = new Biz.SendsMessagesToCustomsShutterUpperer(false);
			UnderbondSenderHelper.SendMessage(iar, new CcsukTransmissionMessageFunction.CUSDEC.IAR(), notifier);
			AssertEquals(0, hawb.Messages.Count);
			AssertContains(notifier.LastErrorsAsString, "cancelled");
			iar.C4_Status = "";
			Factory.Save();
			notifier = new Biz.SendsMessagesToCustomsShutterUpperer(false);
			UnderbondSenderHelper.SendMessage(iar, new CcsukTransmissionMessageFunction.CUSDEC.IAR(), notifier);
			Assert(string.IsNullOrEmpty(notifier.LastErrorsAsString));
			AssertEquals(1, hawb.Messages.Count);
		}

		public void TestDemandsSave()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var fbk1 = hawb.FBKs.AddNew();
			fbk1.C4_SendersMessageReference = "Something unsaved";
			// no save....
			var notifier = new Biz.SendsMessagesToCustomsShutterUpperer(false);
			UnderbondSenderHelper.SendMessage(fbk1, new CcsukTransmissionMessageFunction.CUSDEC.FBK(), notifier);
			AssertEquals(0, hawb.Messages.Count);
			AssertContains("User is told to save first", notifier.LastErrorsAsString, "save your changes");
		}

		public void TestConfirmationAwbNumber()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12387654321";
			var fbk1 = basic.FBKs.AddNew();
			var notification = UnderbondSenderHelper.ConfirmationMessageCreateCusUnderbondsOnly(fbk1);
			AssertEndsWith("", "123-87654321", notification);
			var split = basic.Splits.AddNew();
			split.SplitReference = "69";
			fbk1.SplitReferenceToWhichThisRemovalPertains = split.SplitReference;
			notification = UnderbondSenderHelper.ConfirmationMessageCreateCusUnderbondsOnly(fbk1);
			AssertEndsWith("", "123-87654321/69", notification);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
		}
	}
}
