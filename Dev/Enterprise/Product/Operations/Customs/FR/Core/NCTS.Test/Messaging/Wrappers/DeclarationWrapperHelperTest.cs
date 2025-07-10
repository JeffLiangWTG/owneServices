using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class DeclarationWrapperHelperTest : TestCaseWithFactory
	{
		public void TestArrivalAgreementNumber_HeaderNull()
		{
			AssertEquals(ZString.Empty, DeclarationWrapperHelper.ArrivalAgreementNumber(null));
		}

		public void TestArrivalAgreementNumber_Declarant_Null()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			AssertEquals(ZString.Empty, DeclarationWrapperHelper.ArrivalAgreementNumber(header));
		}

		public void TestArrivalAgreementNumber_Declarant_NoCusAccount()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			header.Declarant.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(ZString.Empty, DeclarationWrapperHelper.ArrivalAgreementNumber(header));
		}

		public void TestArrivalAgreementNumber_Declarant()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			var org = CreateOrgHeaderWithDTA(Factory, "AC0001");
			header.Declarant.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("AC0001", DeclarationWrapperHelper.ArrivalAgreementNumber(header));
		}

		public void TestArrivalAgreementNumber_Consignee()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			var org = CreateOrgHeaderWithDTA(Factory, "AC0002");
			header.Declarant.E2_OA_Address = ZGuid.Empty;
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("AC0002", DeclarationWrapperHelper.ArrivalAgreementNumber(header));
		}
		public void TestDepartureAgreementNumber_HeaderNull()
		{
			AssertEquals(ZString.Empty, DeclarationWrapperHelper.DepartureAgreementNumber(null));
		}

		public void TestDepartureAgreementNumber_Declarant_Null()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			AssertEquals(ZString.Empty, DeclarationWrapperHelper.DepartureAgreementNumber(header));
		}

		public void TestDepartureAgreementNumber_Declarant_NoCusAccount()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			header.Principal.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(ZString.Empty, DeclarationWrapperHelper.DepartureAgreementNumber(header));
		}

		public void TestDepartureAgreementNumber_Declarant()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			var org = CreateOrgHeaderWithDTA(Factory, "CC0001");
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CC0001", DeclarationWrapperHelper.DepartureAgreementNumber(header));
		}

		public void TestDepartureAgreementNumber_Consignor()
		{
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			var org = CreateOrgHeaderWithDTA(Factory, "CC0002");
			header.Principal.E2_OA_Address = ZGuid.Empty;
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CC0002", DeclarationWrapperHelper.DepartureAgreementNumber(header));
		}

		public static OrgHeader CreateOrgHeaderWithDTA(BusinessObjectFactory factory, ZString accountNumber)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			var cusAccount = factory.New<OrgCusAccount>();
			cusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount.CZ_OH = org.PK;
			cusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			cusAccount.CZ_Account = accountNumber;
			return org;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
