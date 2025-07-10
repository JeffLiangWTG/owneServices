using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsSupportingDocumentPhase5ArrivalReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("When supporting document is null", ()
				=> new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(supportingDocument: null, baseReadOnlyProvider: null));
				AssertExceptionThrown<ArgumentNullException>("When baseReadOnlyProvider is null", ()
					=> new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(supportingDocument: supportingDocumentBill, baseReadOnlyProvider: null));
			});
		}

		public void TestCSI_Status_ReadOnly()
		{
			var supportingDocumentMovementHeader = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
			supportingDocumentMovementHeader.CSI_Status = NctsUnloadedStateList.Codes.NEW;

			supportingDocumentBill.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			supportingDocumentGoodItem.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			CombineAssertions(() =>
			{
				AssertEquals("When Status is New, CSI_Status is readonly, MovementHeader", true, supportingDocumentMovementHeader.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is New, CSI_Status is readonly, Bill", true, supportingDocumentBill.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is New, CSI_Status is readonly, GoodsItem", true, supportingDocumentGoodItem.CSI_StatusInfo.ReadOnly);

				supportingDocumentMovementHeader.CSI_Status = NctsUnloadedStateList.Codes.MIS;
				supportingDocumentBill.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				supportingDocumentGoodItem.CSI_Status = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("When Status is not New, Unloading Remarks is not readonly and Unloading Remarks are not accepted then CSI_Status is not readonly, MovementHeader", false, supportingDocumentMovementHeader.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is not New, Unloading Remarks is not readonly and Unloading Remarks are not accepted then CSI_Status is not readonly, Bill", false, supportingDocumentBill.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is not New, Unloading Remarks is not readonly and Unloading Remarks are not accepted then CSI_Status is not readonly, GoodsItem", false, supportingDocumentGoodItem.CSI_StatusInfo.ReadOnly);

				nctsHeader.CommonMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When Status is not New but Unloading Remarks is sent, CSI_Status is readonly, MovementHeader", true, supportingDocumentMovementHeader.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is not New but Unloading Remarks is sent, CSI_Status is readonly, Bill", true, supportingDocumentBill.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is not New but Unloading Remarks is sent, CSI_Status is readonly, GoodsItem", true, supportingDocumentGoodItem.CSI_StatusInfo.ReadOnly);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.ArrivalGoodsItems.AddNew();
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;

			supportingDocumentBill = nctsBill.SupportingDocuments.AddNew();
			supportingDocumentGoodItem = goodsItem.SupportingDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsSupportingDocument supportingDocumentBill;
		NctsSupportingDocument supportingDocumentGoodItem;
	}
}
