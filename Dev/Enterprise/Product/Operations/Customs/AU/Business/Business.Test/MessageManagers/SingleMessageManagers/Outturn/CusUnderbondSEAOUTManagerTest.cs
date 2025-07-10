using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondSEAOUTManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Outturn Report for: ", Manager.MessageFriendlyName);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("StatusCalculators", 1, Manager.StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusOutturnHeaderStatusCalculator), Manager.StatusCalculators[0].GetType());
		}

		public void TestBusinessObject()
		{
			AssertEquals(OutturnHeader, Manager.BusinessObject);
		}

		public void TestGetMessages()
		{
			AssertEquals("SEAOUT Messages are attached to CusOutturnHeader", OutturnHeader.Messages, Manager.GetMessages(OutturnHeader));
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
		}

		public void TestGetBuilder()
		{
			var builders = Manager.GetBuilder(OutturnHeader);
			AssertEquals("Message Builder Type", typeof(SEAOUTMessageBuilder), builders[0].GetType());
		}

		public void TestGetAmendmentManager()
		{
			AssertEquals("AmendmentManagerType", typeof(CusOutturnHeaderSEAOUTAmendmentGenerator), Manager.GetAmendmentManager(OutturnHeader).GetType());
		}

		public void TestGetBuilderWithNoParent()
		{
			var builders = Manager.GetBuilder(OutturnHeader);
			AssertEquals("MessageBuilderType", typeof(SEAOUTMessageBuilder), builders[0].GetType());
		}

		public void TestSendingWithNoOutturns()
		{
			AssertEquals("precondition", 0, OutturnHeader.Outturns.Count);
			var result = Manager.GetNotificationsForSendingAnOriginal();
			var originalNotificationCount = result.Count;
			Assert("notification", result.ContainsError("There must be at least one outturn."));

			OutturnHeader.Outturns.AddNew();
			result = Manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("no notification", (originalNotificationCount - 1), result.Count);
		}

		public void TestSplitMessageWhenGreaterThanMaxLines()
		{
			AUCustomsDataRegistry.Instance.MaximumSeaOutturnLinesPerMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB023928";

			var house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "HOUSE";
			house.CA_ConsignorName = "CONSIGNOR";
			house.CA_ConsigneeName = "CONSIGNEE";
			house.CA_ConsigneeAddress1 = "STREET";
			house.CA_ConsigneeAddress2 = "STREET 2";
			house.CA_ConsigneeSuburb = "CITY STATE";
			house.CA_ConsigneePostcode = "2233";
			var pivot = house.Pivot.AddNew();

			var container = oceanBill.Containers.AddNew();
			pivot.CV_CN = container.PK;
			pivot.CV_GoodsDescription = "GOODS DESCRIPION";
			pivot.CV_HazardousGoods = true;
			pivot.CV_IsSAC = true;

			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "222";

			var outturn1 = OutturnHeader.Outturns.AddNew();
			outturn1.C5_OuterPacks = 5;
			outturn1.C5_PackagesOutturned = 5;
			outturn1.C5_CargoReceiptDate = ZDateTime.Now;
			outturn1.C5_GoodsDescription = "Test max lines split - first original";

			var outturn2 = OutturnHeader.Outturns.AddNew();
			outturn2.C5_OuterPacks = 2;
			outturn2.C5_PackagesOutturned = 2;
			outturn2.C5_CargoReceiptDate = ZDateTime.Now;
			outturn2.C5_GoodsDescription = "Should be in original message";

			var outturn3 = OutturnHeader.Outturns.AddNew();
			outturn3.C5_OuterPacks = 3;
			outturn3.C5_PackagesOutturned = 3;
			outturn3.C5_CargoReceiptDate = ZDateTime.Now;
			outturn3.C5_GoodsDescription = "Should be in original message";

			var outturn4 = OutturnHeader.Outturns.AddNew();
			outturn4.C5_OuterPacks = 4;
			outturn4.C5_PackagesOutturned = 4;
			outturn4.C5_CargoReceiptDate = ZDateTime.Now;
			outturn4.C5_GoodsDescription = "Should be in first split message";

			var outturn5 = OutturnHeader.Outturns.AddNew();
			outturn5.C5_OuterPacks = 5;
			outturn5.C5_PackagesOutturned = 5;
			outturn5.C5_CargoReceiptDate = ZDateTime.Now;
			outturn5.C5_GoodsDescription = "Should be in first split message";

			var outturn6 = OutturnHeader.Outturns.AddNew();
			outturn6.C5_OuterPacks = 6;
			outturn6.C5_PackagesOutturned = 6;
			outturn6.C5_CargoReceiptDate = ZDateTime.Now;
			outturn6.C5_GoodsDescription = "Should be in first split message";

			var outturn7 = OutturnHeader.Outturns.AddNew();
			outturn7.C5_OuterPacks = 7;
			outturn7.C5_PackagesOutturned = 7;
			outturn7.C5_CargoReceiptDate = ZDateTime.Now;
			outturn7.C5_GoodsDescription = "Should be in second split message";

			ISeaOutturnReportHeaderInformation headerInfo = new SeaOutturnStandAloneReportHeader(OutturnHeader);
			AssertEquals("Pre-condition: should be 7 lines", 7, headerInfo.Lines.Count());

			var builders = Manager.GetBuilder(OutturnHeader);
			AssertEquals("Should be 3 SEAOutMessageBuilders returned", 3, builders.Length);

			var builderCount = 0;
			foreach (var builder in builders)
			{
				builderCount++;
				if (builderCount == 1)
				{
					AssertEquals("First message builder should have SetStatusToPending as false", false, builder.SetStatusToPending);
				}
				else
				{
					AssertEquals("SetStatusToPending should be on for all subsequent split messages", true, builder.SetStatusToPending);
				}

				AssertEquals("SetSplitMessageIdentifier should be true for all split messages", true, builder.SetSplitMessageIdentifier);
			}
		}

		public void TestCanSendOriginalWhenSplitMessageFailed()
		{
			var outturn = OutturnHeader.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			AssertEquals("CanSendOriginal default", true, Manager.CanSendOriginal);

			OutturnHeader.Logs.AddNew(Events.Cancelled, "Split Outturn message not wholly processed");
			AssertEquals("CanSendOriginal is also allowed when split message has failed for SEA Outturn", true, Manager.CanSendOriginal);
		}

		public void TestShouldSendOriginalOnSave()
		{
			var outturn = OutturnHeader.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			AssertEquals("Pre-condition: CanSendOriginal", true, Manager.CanSendOriginal);
			AssertEquals("ShouldSendOriginalOnSave for SeaCargo Outturn Messages", true, Manager.ShouldSendOriginalOnSave);

			SetStatus(CMRBaseStatuses.Codes.OriginalAccepted);
			AssertEquals("Can no longer send original", false, Manager.CanSendOriginal);
			AssertEquals("ShouldSendOriginalOnSave is only for when 'Original'", false, Manager.ShouldSendOriginalOnSave);
		}

		public void TestResetToOriginal()
		{
			OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			OutturnHeader.Outturns.AddNew();
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, Manager.GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, OutturnHeader.Messages[0].EM_Status);
			foreach (CusUnderbond underbond in OutturnHeader.Underbonds)
			{
				AssertEquals("Outurned date should be cleared when Outturn is reset to original", ZDate.Empty, underbond.C4_Outurned);
			}
		}

		public new void TestCanSendOriginal()
		{
			AssertEquals("with no outturn", false, Manager.CanSendOriginal);
			AssertEquals("in sync with CanSendOriginal", false, Manager.ShouldSendOriginalOnSave);

			var outturn = OutturnHeader.Outturns.AddNew();
			AssertEquals("with blank outturn", false, Manager.CanSendOriginal);
			AssertEquals("in sync with CanSendOriginal", false, Manager.ShouldSendOriginalOnSave);

			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			AssertEquals("with receipt date", true, Manager.CanSendOriginal);
			AssertEquals("in sync with CanSendOriginal", true, Manager.ShouldSendOriginalOnSave);
		}

		protected override void GenerateOriginalMessageForSaving()
		{
			Manager.GenerateOriginalMessages(Manager.BusinessObject);
		}

		protected override CMRMessageManager GetManager() => new CusUnderbondSEAOUTManager(OutturnHeader);

		protected override void SetStatus(ZString status)
		{
			OutturnHeader.OutturnStatus.Code = status;
		}

		CusUnderbondSEAOUTManager manger;
		new CusUnderbondSEAOUTManager Manager => manger ?? (manger = (CusUnderbondSEAOUTManager)GetManager());

		CusOutturnHeader outturnHeader;
		CusOutturnHeader OutturnHeader => outturnHeader ?? (outturnHeader = Factory.New<CusOutturnHeader>());
	}
}
