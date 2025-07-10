using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLHeaderValidationTest : BaseCusSeaManOBLHeaderValidationTest
	{
		public void TestMessageErrorOnConsigneeNameToOrder()
		{
			const string errorMessage = "TO ORDER is not a valid consignee, and will be rejected by Customs. Please enter in the correct consignee details.";

			Header.BO_ConsigneeName = "TO ORDER";
			AssertHasMessageError(Header.BO_ConsigneeNameInfo, errorMessage);

			Header.BO_ConsigneeName = "TOORDER";
			AssertHasMessageError(Header.BO_ConsigneeNameInfo, errorMessage);

			Header.BO_ConsigneeName = "TOORDR";
			AssertHasMessageError(Header.BO_ConsigneeNameInfo, errorMessage);

			Header.BO_ConsigneeName = "TO";
			AssertNoNotifications(Header.BO_ConsigneeNameInfo);

			Header.BO_ConsigneeName = "TO ";
			AssertNoNotifications(Header.BO_ConsigneeNameInfo);

			Header.BO_ConsigneeName = "T";
			AssertNoNotifications(Header.BO_ConsigneeNameInfo);

			Header.BO_ConsigneeName = "";
			AssertNoMessageError(Header.BO_ConsigneeNameInfo, errorMessage);

			Header.BO_ConsigneeName = "TO ORDER OF SHIPPER";
			AssertHasMessageError(Header.BO_ConsigneeNameInfo, errorMessage);

			Header.BO_ConsigneeName = "TU OREDR";
			AssertHasMessageError(Header.BO_ConsigneeNameInfo, errorMessage);

			Header.BO_ConsigneeName = "Wang Enterprises";
			AssertNoNotifications(Header.BO_ConsigneeNameInfo);
		}

		public void TestMessageErrorOnConsigneeAddressToOrder()
		{
			const string errorMessage = "TO ORDER is not a valid consignee, and will be rejected by Customs. Please enter in the correct consignee details.";

			Header.BO_ConsigneeAddress1 = "TO ORDER";
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, errorMessage);

			Header.BO_ConsigneeAddress1 = "TOORDER";
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, errorMessage);

			Header.BO_ConsigneeAddress1 = "TOORDR";
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, errorMessage);

			Header.BO_ConsigneeAddress1 = "TO";
			AssertNoNotifications(Header.BO_ConsigneeAddress1Info);

			Header.BO_ConsigneeAddress1 = "TO ";
			AssertNoNotifications(Header.BO_ConsigneeAddress1Info);

			Header.BO_ConsigneeAddress1 = "T";
			AssertNoNotifications(Header.BO_ConsigneeAddress1Info);

			Header.BO_ConsigneeAddress1 = "";
			AssertNoMessageError(Header.BO_ConsigneeAddress1Info, errorMessage);

			Header.BO_ConsigneeAddress1 = "TO ORDER OF SHIPPER";
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, errorMessage);

			Header.BO_ConsigneeAddress1 = "TU OREDR";
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, errorMessage);

			Header.BO_ConsigneeAddress1 = "Wang Enterprises";
			AssertNoNotifications(Header.BO_ConsigneeAddress1Info);
		}

		public void TestMessageErrorOnConsignorNameToOrder()
		{
			const string errorMessage = "TO ORDER is not a valid consignor, and will be rejected by Customs. Please enter in the correct consignor details.";

			Header.BO_ConsignorName = "TO ORDER";
			AssertHasMessageError(Header.BO_ConsignorNameInfo, errorMessage);

			Header.BO_ConsignorName = "TOORDER";
			AssertHasMessageError(Header.BO_ConsignorNameInfo, errorMessage);

			Header.BO_ConsignorName = "TOORDR";
			AssertHasMessageError(Header.BO_ConsignorNameInfo, errorMessage);

			Header.BO_ConsignorName = "TO";
			AssertNoNotifications(Header.BO_ConsignorNameInfo);

			Header.BO_ConsignorName = "TO ";
			AssertNoNotifications(Header.BO_ConsignorNameInfo);

			Header.BO_ConsignorName = "T";
			AssertNoNotifications(Header.BO_ConsignorNameInfo);

			Header.BO_ConsignorName = "";
			AssertNoMessageError(Header.BO_ConsignorNameInfo, errorMessage);

			Header.BO_ConsignorName = "TO ORDER OF SHIPPER";
			AssertHasMessageError(Header.BO_ConsignorNameInfo, errorMessage);

			Header.BO_ConsignorName = "TU OREDR";
			AssertHasMessageError(Header.BO_ConsignorNameInfo, errorMessage);

			Header.BO_ConsignorName = "Wang Enterprises";
			AssertNoNotifications(Header.BO_ConsignorNameInfo);
		}

		public void TestMessageErrorOnConsignorAddressToOrder()
		{
			const string errorMessage = "TO ORDER is not a valid consignor, and will be rejected by Customs. Please enter in the correct consignor details.";

			Header.BO_ConsignorAddress1 = "TO ORDER";
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, errorMessage);

			Header.BO_ConsignorAddress1 = "TOORDER";
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, errorMessage);

			Header.BO_ConsignorAddress1 = "TOORDR";
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, errorMessage);

			Header.BO_ConsignorAddress1 = "TO";
			AssertNoNotifications(Header.BO_ConsignorAddress1Info);

			Header.BO_ConsignorAddress1 = "TO ";
			AssertNoNotifications(Header.BO_ConsignorAddress1Info);

			Header.BO_ConsignorAddress1 = "T";
			AssertNoNotifications(Header.BO_ConsignorAddress1Info);

			Header.BO_ConsignorAddress1 = "";
			AssertNoMessageError(Header.BO_ConsignorAddress1Info, errorMessage);

			Header.BO_ConsignorAddress1 = "TO ORDER OF SHIPPER";
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, errorMessage);

			Header.BO_ConsignorAddress1 = "TU OREDR";
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, errorMessage);

			Header.BO_ConsignorAddress1 = "Wang Enterprises";
			AssertNoNotifications(Header.BO_ConsignorAddress1Info);
		}

		public void TestValidateBO_OceanBill()
		{
			Header.Validation.ValidateBO_OceanBill();
			AssertHasMessageErrors("by default", Header.BO_OceanBillInfo);

			Header.BO_OceanBill = "Blah";
			AssertNoMessageErrors("when contains something", Header.BO_OceanBillInfo);
		}

		public void TestValidateBO_RN_NKGoodsCountryOfOrigin()
		{
			AssertListValidation(Header.BO_RN_NKGoodsCountryOfOriginInfo, (Factory.NewWithValidTestData<RefCountry>()).RN_Code);
		}

		public void TestValidateBO_PaymentMethod()
		{
			AssertListValidation(Header.BO_PaymentMethodInfo, new ZString(CMRMethodsOfPayment.Codes.Collect));
		}

		public void TestValidateBO_RL_NKDischargePort()
		{
			AssertListValidation(Header.BO_RL_NKDischargePortInfo, "AUSYD");
		}

		public void TestValidateBO_RL_NKOriginPort()
		{
			AssertListValidation(Header.BO_RL_NKOriginPortInfo, "NZAKL");
		}

		public void TestValidateBO_ConsigneeName()
		{
			Header.Validation.ValidateBO_ConsigneeName();
			AssertHasMessageErrors("by default", Header.BO_ConsigneeNameInfo);

			Header.BO_ConsigneeName = "Bleh";
			AssertNoMessageErrors("when filled", Header.BO_ConsigneeNameInfo);
		}

		public void TestConsigneeRequiresOnePieceOfInfoApartFromName()
		{
			const string expectedMessage = "Consignee details require at least one piece of information entered, apart from the name.";

			Header.Validation.ValidateBO_ConsigneeAddress1();
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, expectedMessage);

			Header.BO_ConsigneeAddress1 = "foo";
			AssertNoMessageErrors(Header.BO_ConsigneeAddress1Info);
			Header.BO_ConsigneeAddress1 = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, expectedMessage);

			Header.BO_ConsigneeAddress2 = "foo";
			AssertNoMessageErrors(Header.BO_ConsigneeAddress1Info);
			Header.BO_ConsigneeAddress2 = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, expectedMessage);

			Header.BO_ConsigneeCity = "foo";
			AssertNoMessageErrors(Header.BO_ConsigneeAddress1Info);
			Header.BO_ConsigneeCity = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, expectedMessage);

			Header.BO_ConsigneePostCode = "foo";
			AssertNoMessageErrors(Header.BO_ConsigneeAddress1Info);
			Header.BO_ConsigneePostCode = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, expectedMessage);

			Header.BO_ConsigneeState = "foo";
			AssertNoMessageErrors(Header.BO_ConsigneeAddress1Info);
			Header.BO_ConsigneeState = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsigneeAddress1Info, expectedMessage);
		}

		public void TestConsignorRequiresOnePieceOfInfoApartFromName()
		{
			const string expectedMessage = "Consignor details require at least one piece of information entered, apart from the name.";

			Header.Validation.ValidateBO_ConsignorAddress1();
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, expectedMessage);

			Header.BO_ConsignorAddress1 = "foo";
			AssertNoMessageErrors(Header.BO_ConsignorAddress1Info);
			Header.BO_ConsignorAddress1 = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, expectedMessage);

			Header.BO_ConsignorAddress2 = "foo";
			AssertNoMessageErrors(Header.BO_ConsignorAddress1Info);
			Header.BO_ConsignorAddress2 = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, expectedMessage);

			Header.BO_ConsignorCity = "foo";
			AssertNoMessageErrors(Header.BO_ConsignorAddress1Info);
			Header.BO_ConsignorCity = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, expectedMessage);

			Header.BO_ConsignorPostCode = "foo";
			AssertNoMessageErrors(Header.BO_ConsignorAddress1Info);
			Header.BO_ConsignorPostCode = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, expectedMessage);

			Header.BO_ConsignorState = "foo";
			AssertNoMessageErrors(Header.BO_ConsignorAddress1Info);
			Header.BO_ConsignorState = ZString.Empty;
			AssertHasMessageError(Header.BO_ConsignorAddress1Info, expectedMessage);
		}

		public void TestValidateBO_ConsignorName()
		{
			Header.Validation.ValidateBO_ConsignorName();
			AssertHasMessageErrors("by default", Header.BO_ConsignorNameInfo);

			Header.BO_ConsignorName = "Bleh";
			AssertNoMessageErrors("when filled", Header.BO_ConsignorNameInfo);
		}

		public new void TestValidateBO_RL_NKDestinationPort()
		{
			Header.BO_RL_NKDestinationPort = "AUSYD";

			CusSeaManOBLDetail detail = Header.Details.AddNew();
			ICusUnderbondDependentCollectionParent underbondParentDetail = detail;
			detail.BD_ContainerNumber = "FOO";

			CusUnderbond underbond = (CusUnderbond)underbondParentDetail.Underbonds.AddNew();

			underbond.C4_MovementReason = "MOV";
			Header.Validation.ValidateAll();
			AssertNoNotifications("no errors when destination port au, and has underbonds with reasons other than TSH", Header.BO_RL_NKDestinationPortInfo);

			underbond.C4_MovementReason = "TSH";
			Header.Validation.ValidateAll();
			AssertHasMessageErrors("message error when destination port au, and has underbond that is TSH", Header.BO_RL_NKDestinationPortInfo);

			Header.BO_RL_NKDestinationPort = "NZAKL";
			Header.Validation.ValidateAll();
			AssertNoNotifications("no errors when destination port nz, and has underbond that is TSH", Header.BO_RL_NKDestinationPortInfo);

			underbond.C4_MovementReason = "MOV";
			Header.Validation.ValidateAll();
			AssertNoNotifications("no errors when destination port nz, and has underbonds with reasons other than TSH", Header.BO_RL_NKDestinationPortInfo);
		}

		#region Implementation

		protected override BaseCusSeaManOBLHeader GetNewOBLHeader()
		{
			return (BaseCusSeaManOBLHeader)Factory.New(typeof(CusSeaManOBLHeader));
		}

		CusSeaManOBLHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = (CusSeaManOBLHeader)GetNewOBLHeader();
				}
				return fHeader;
			}
		}
		CusSeaManOBLHeader fHeader;

		#endregion
	}
}
