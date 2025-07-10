using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsArrivalAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("When additional document is null", ()
				=> new NctsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: null, baseReadOnlyProvider: null));

				AssertExceptionThrown<ArgumentNullException>("When baseReadOnlyProvider is null", ()
					=> new NctsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: additionalDocumentGoodsItem, baseReadOnlyProvider: null));
			});
		}

		public void TestCSI_Status_ReadOnly()
		{
			var additionalDocumentMovementHeader = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			additionalDocumentMovementHeader.CSI_Status = NctsUnloadedStateList.Codes.NEW;

			additionalDocumentGoodsItem.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			CombineAssertions(() =>
			{
				AssertEquals("When Status is New, CSI_Status is readonly, MovementHeader", true, additionalDocumentMovementHeader.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is New, CSI_Status is readonly, GoodsItem", true, additionalDocumentGoodsItem.CSI_StatusInfo.ReadOnly);

				additionalDocumentMovementHeader.CSI_Status = NctsUnloadedStateList.Codes.MIS;
				additionalDocumentGoodsItem.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When Status is not New, Unloading Remarks is not readonly and Unloading Remarks are not accepted then CSI_Status is not readonly, MovementHeader", false, additionalDocumentMovementHeader.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is not New, Unloading Remarks is not readonly and Unloading Remarks are not accepted then CSI_Status is not readonly, GoodsItem", false, additionalDocumentGoodsItem.CSI_StatusInfo.ReadOnly);

				nctsHeader.CommonMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When Status is not New but Unloading Remarks is sent, CSI_Status is readonly, MovementHeader", true, additionalDocumentMovementHeader.CSI_StatusInfo.ReadOnly);
				AssertEquals("When Status is not New but Unloading Remarks is sent, CSI_Status is readonly, GoodsItem", true, additionalDocumentGoodsItem.CSI_StatusInfo.ReadOnly);
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

			additionalDocumentGoodsItem = goodsItem.AdditionalInfos.AddNew();
		}
		NctsHeader nctsHeader;
		NctsAdditionalInfo additionalDocumentGoodsItem;
	}
}
