using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: null));
		}

		public void TestCSI_LineNoReadOnly()
		{
			CombineAssertions("CSI_LineNo ReadOnly", () =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("In NCTS5", true, additionalDocument.CSI_LineNoInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("In NCTS4", false, additionalDocument.CSI_LineNoInfo.ReadOnly);
			});
		}

		public void TestCSI_StatusReadOnly()
		{
			CombineAssertions("CSI_Status ReadOnly", () =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("In NCTS5", true, additionalDocument.CSI_StatusInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("In NCTS4", false, additionalDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		public void TestCSI_StatusReadOnly_ForAcceptedUnloading()
		{
			CombineAssertions("CSI_Status ReadOnly", () =>
			{
				AssertEquals("It should be disabled by default", true, additionalDocument.CSI_StatusInfo.ReadOnly);
				var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
				var bill = goodsItem.Bill;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("When BM_CustomsStatus is ClosedFullRelease", true, additionalDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumberReadOnly()
		{
			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("When NCTS5, CSI_SubType = REF", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("When NCTS5, CSI_SubType = INF", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When NCTS5, CSI_SubType = INF, CSI_Status = DEC", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumberReadOnly_ForAcceptedUnloading()
		{
			AssertEquals("CSI_ReferenceNumber should be enabled by default", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
			var bill = goodsItem.Bill;
			var movementHeader = bill.MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementHeader,
				x => ((NctsAdditionalInfo)x).CSI_ReferenceNumberInfo.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_ReferenceNumberReadOnly_ForMessageSent()
		{
			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				AssertEquals("Default isn't ReadOnly", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
				var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
				var bill = goodsItem.Bill;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When BM_MessageStatus is Sent", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2ReadOnly_ForAcceptedUnloading()
		{
			AssertEquals("CSI_ReferenceNumber2 should be enabled by default", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
			var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
			var bill = goodsItem.Bill;
			var movementHeader = bill.MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementHeader,
				x => ((NctsAdditionalInfo)x).CSI_ReferenceNumber2Info.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_ReferenceNumber2ReadOnly_ForMessageSent()
		{
			CombineAssertions("CSI_ReferenceNumber2 ReadOnly", () =>
			{
				AssertEquals("Default is enabled", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
				var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
				var bill = goodsItem.Bill;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When BM_MessageStatus is Sent", true, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_DescriptionReadOnly()
		{
			CombineAssertions("CSI_Description ReadOnly", () =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("In NCTS4", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("In NCTS5", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_DescriptionReadOnly_ForAcceptedUnloading()
		{
			CombineAssertions("CSI_Description ReadOnly", () =>
			{
				AssertEquals("Default is disabled", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
				var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
				var bill = goodsItem.Bill;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("When BH_CustomsStatus is ClosedFullRelease", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_DescriptionReadOnly_ForMessageSent()
		{
			CombineAssertions("CSI_Description ReadOnly", () =>
			{
				AssertEquals("Default is disabled", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
				var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
				var bill = goodsItem.Bill;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When BH_CustomsStatus is Sent", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_CodeReadOnly()
		{
			CombineAssertions("CSI_Cdoe ReadOnly", () =>
			{
				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When NCTS5, CI_Status = DEC", true, additionalDocument.CSI_CodeInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("When NCTS5, CI_Status = DIF", false, additionalDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_CodeReadOnly_ForAcceptedUnloading()
		{
			AssertEquals("CSI_Code should be enabled by default", false, additionalDocument.CSI_CodeInfo.ReadOnly);
			var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
			var bill = goodsItem.Bill;
			var movementHeader = bill.MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementHeader,
				x => ((NctsAdditionalInfo)x).CSI_CodeInfo.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_CodeReadOnly_ForMessageSent()
		{
			CombineAssertions("CSI_Code ReadOnly", () =>
			{
				AssertEquals("Enabled by default", false, additionalDocument.CSI_CodeInfo.ReadOnly);
				var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
				var bill = goodsItem.Bill;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When BH_CustomsStatus is Sent", true, additionalDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumberReadOnly_TRATypeAndTD44NWithAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "CusCodeTypeTD44N");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N235", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);

			Factory.Save();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_Code is filled with valid code", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("When CSI_Code is empty code", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumberReadOnly_TRATypeAndTD44NWithoutAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "CusCodeTypeTD44N");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N235", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_Code is filled with valid code", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("When CSI_Code is empty code", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestAdditionalInfoReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.AdditionalInfoReadOnly);
		}

		public void TestReferenceNumberReadOnly_StatusIsMIS()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.ReferenceNumberReadOnly);
		}

		public void TestReferenceNumber2ReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.ReferenceNumber2ReadOnly);
		}

		public void TestDescriptionReadOnly_StatusIsMIS()
		{
			additionalDocument.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.DescriptionReadOnly);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			var movementDetail = arrivalMovementHeader.MovementDetails.AddNew();
			var bill = nctsHeader.Bills.AddNew();
			movementDetail.B9_B0 = bill.PK;
			additionalDocument = bill.ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
			additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		}

		NctsHeader nctsHeader;
		NctsArrivalMovementHeader arrivalMovementHeader;
		NctsAdditionalInfo additionalDocument;

		#endregion

		void AssertReadOnlyWhenCSI_StatusIsMIS(Func<IAdditionalDocumentReadOnlyProvider, bool> readOnly)
		{
			CombineAssertions(() =>
			{
				var provider = new NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument);
				additionalDocument.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty", expected: false, readOnly(provider));

				additionalDocument.CSI_Status = SupportingDocumentStatusList.Codes.MIS;
				provider = new NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument);
				AssertEquals("CSI_Status = 'MIS'", expected: true, readOnly(provider));

				additionalDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				provider = new NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument);
				AssertEquals("CSI_Status = 'NEW'", expected: false, readOnly(provider));
			});
		}
	}
}
