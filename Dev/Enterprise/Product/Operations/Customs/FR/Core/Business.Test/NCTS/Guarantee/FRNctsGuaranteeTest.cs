using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(FRNctsGuarantee))]
	class FRNctsGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);
			AssertType<FRNctsGuaranteeValidation>(nctsGuarantee.Validation);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<FRNctsGuaranteePhase5Validation>(nctsGuarantee.Validation);
		}

		public void TestCusGuaranteeType()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateGuaranteeHeader("1234", org1, "COD", "1", Core.Constants.CountryCodes.France);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "1234";
			AssertNotNull(nctsGuarantee.CusGuarantee);
			AssertType<CusGuaranteeHeader>(nctsGuarantee.CusGuarantee);
		}

		public void TestDefaultPW_RX_NKCurrency_IsEUR()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, nctsGuarantee.PW_RX_NKCurrency);
		}

		public void TestGuaranteeReferenceNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_SubType = "1";
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = guaranteeHeader.PermitHolder.MainAddress.PK;

			var guarantee = (FRNctsGuarantee)nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;

			AssertEquals("GuaranteeReferenceNumber should be equal to PW_BondNumber when AdditionalGuaranteeReference of type TR does not exist.", "GUA1", guarantee.GuaranteeReferenceNumber);

			var cusGuaranteeReferenceNumber = guaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			cusGuaranteeReferenceNumber.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			cusGuaranteeReferenceNumber.CY_Data = "15FR9860000447372";

			AssertEquals("GuaranteeReferenceNumber should be equal to CY_Data when AdditionalGuaranteeReference of type TR exists.", "15FR9860000447372", guarantee.GuaranteeReferenceNumber);
		}

		CusGuaranteeHeader CreateGuaranteeHeader(ZString number, OrgHeader permitHolder, ZString type, ZString subType, ZString countryCode)
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = number;
			guarantee.CPH_OH_PermitHolder = permitHolder.PK;
			guarantee.CPH_Type = type;
			guarantee.CPH_SubType = subType;
			guarantee.CPH_RN_NKCountryCode = countryCode;
			return guarantee;
		}

		protected override BusinessObject GetNewBusinessObject() => CreateHeaderAndGuarantee(Factory).Guarantee;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateHeaderAndGuarantee(factory).Guarantee;

		(NctsHeader Header, FRNctsGuarantee Guarantee) CreateHeaderAndGuarantee(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var guarantee = nctsHeader.Guarantees.AddNew();

			return (nctsHeader, guarantee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		}
	}
}
