using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AsycudaBillValidationForRegularBillTest : BusinessObjectLookupsTestCase
	{
		public void TestGoods_Location()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "CL";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;

			org.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "62318879");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "1";
			bill.ABL_OA_GoodsLocation = ZGuid.Empty;
			Factory.Save();

			AssertHasMessageError(bill.ABL_OA_GoodsLocationInfo, "A Warehouse is required");

			bill.ABL_OA_GoodsLocation = orgAddress.PK;
			AssertHasMessageError(bill.ABL_OA_GoodsLocationInfo, "The Warehouse should have RUT and CCP assigned numbers.");

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "62318879");

			bill.ABL_OA_GoodsLocation = ZGuid.Empty;
			bill.ABL_OA_GoodsLocation = orgAddress.PK;

			AssertNoMessageErrors(bill.ABL_OA_GoodsLocationInfo);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "CL";
			org2.OH_FullName = "FULL NAME";
			var orgAddress2 = org2.MainAddress;

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			bill.ABL_BillNumber = "1";
			bill.ABL_OA_GoodsLocation = ZGuid.Empty;
			Factory.Save();

			AssertHasMessageError(bill.ABL_OA_GoodsLocationInfo, "A Warehouse is required");

			bill.ABL_OA_GoodsLocation = orgAddress2.PK;
			AssertHasMessageError(bill.ABL_OA_GoodsLocationInfo, "The Warehouse should have RUT assigned number.");

			org2.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "62318879");

			bill.ABL_OA_GoodsLocation = ZGuid.Empty;
			bill.ABL_OA_GoodsLocation = orgAddress2.PK;

			AssertNoMessageErrors(bill.ABL_OA_GoodsLocationInfo);
		}

		#region Consignee

		public void TestCheckABL_ConsigneeRegNo()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_RN_NKConsigneeCountry = "CL";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ConsigneeRegNo = "1996";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNoType = "RUT";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ConsigneeRegNoType = "VAT";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RN_NKConsigneeCountry = "UY";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNoType = "RUT";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ConsigneeRegNo = "1996";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoInfo);
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_RN_NKConsigneeCountry = "CL";
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ConsigneeRegNoType = "VAT";
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, "The code you have selected is not in the list.");

			bill.ABL_ConsigneeRegNoType = "RUT";
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_RN_NKConsigneeCountry = "UY";
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_ConsigneeRegNoType = "VAT";
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_ConsigneeRegNoType = "RUT";
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrors(bill.ABL_ConsigneeRegNoTypeInfo);
		}

		#endregion

		#region Shipper

		public void TestCheckABL_ShipperRegNo()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoInfo);

			bill.ABL_RN_NKShipperCountry = "CL";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ShipperRegNo = "1996";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperRegNoType = "RUT";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperRegNo = ZString.Empty;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ShipperRegNoType = "VAT";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RN_NKShipperCountry = "UY";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperRegNoType = "RUT";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ShipperRegNo = "1996";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoTypeInfo);

			bill.ABL_RN_NKShipperCountry = "CL";
			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ShipperRegNoType = "VAT";
			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoTypeInfo, "The code you have selected is not in the list.");

			bill.ABL_ShipperRegNoType = "RUT";
			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoTypeInfo);

			bill.ABL_RN_NKShipperCountry = "UY";
			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoTypeInfo);

			bill.ABL_ShipperRegNoType = "VAT";
			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoTypeInfo);

			bill.ABL_ShipperRegNoType = "RUT";
			bill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoMessageErrors(bill.ABL_ShipperRegNoTypeInfo);
		}

		#endregion

		#region Notify Party

		public void TestCheckABL_NotifyPartyRegNo()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoInfo);

			bill.ABL_RN_NKNotifyPartyCountry = "CL";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_NotifyPartyRegNo = "1996";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoInfo);

			bill.ABL_NotifyPartyRegNoType = "RUT";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoInfo);

			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_NotifyPartyRegNoType = "VAT";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RN_NKNotifyPartyCountry = "UY";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoInfo);

			bill.ABL_NotifyPartyRegNoType = "RUT";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_NotifyPartyRegNo = "1996";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoInfo);
		}

		public void TestCheckABL_NotifyPartyRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoTypeInfo);

			bill.ABL_RN_NKNotifyPartyCountry = "CL";
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_NotifyPartyRegNoType = "VAT";
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, "The code you have selected is not in the list.");

			bill.ABL_NotifyPartyRegNoType = "RUT";
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoTypeInfo);

			bill.ABL_RN_NKNotifyPartyCountry = "UY";
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoTypeInfo);

			bill.ABL_NotifyPartyRegNoType = "VAT";
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoTypeInfo);

			bill.ABL_NotifyPartyRegNoType = "RUT";
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrors(bill.ABL_NotifyPartyRegNoTypeInfo);
		}

		#endregion

		public void TestCheckABL_BillNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "1";
			AssertNoNotifications(bill.ABL_BillNumberInfo);

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "1";
			AssertHasErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
			bill2.ABL_BillNumber = "2";
			AssertNoErrorContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
		}
	}
}
