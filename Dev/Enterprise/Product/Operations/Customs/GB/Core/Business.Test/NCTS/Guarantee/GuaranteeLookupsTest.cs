using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class GuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceNumbers()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var gua1 = CreateGuarantee("Test1", org, Core.Constants.CountryCodes.Spain, GBGuaranteeTypeList.Codes.TRA);
			var gua2 = CreateGuarantee("Test2", org, Core.Constants.CountryCodes.France, GBGuaranteeTypeList.Codes.GEN);
			var gua3 = CreateGuarantee("Test3", org, Core.Constants.CountryCodes.Spain, GBGuaranteeTypeList.Codes.COM);
			var gua4 = CreateGuarantee("Test4", org, Core.Constants.CountryCodes.France, "IMP");
			var permit1 = CreateGuarantee("Test5", org, Core.Constants.CountryCodes.France, GBGuaranteeTypeList.Codes.TRA);
			permit1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;

			header.Principal.E2_OA_Address = org.MainAddress.PK;
			var guarantee = header.Guarantees.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { "Test1", "Test2", "Test3" }, guarantee.Lookups.ReferenceNumbers.Select(x => x.CPH_Number));
		}

		public void TestReferenceNumbers_FilterBusinessObjectDefaults()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var principal = Factory.New<OrgHeader>();
			var guarantee = header.Guarantees.AddNew();
			guarantee.PW_BondNumber = "Test";
			header.Principal.OrganisationPK = principal.PK;

			var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
			CombineAssertions(() =>
			{
				AssertEquals(principal.PK, referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeHolders && x.PropertyName == "Property1").Value);
				AssertEquals(null, referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeHolders && x.PropertyName == "Property2").Value);
				AssertEquals("Test", referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeNumber && x.PropertyName == "Property").Value);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;

		CusGuaranteeHeader CreateGuarantee(ZString number, OrgHeader permitHolder, ZString countryCode, ZString type)
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = number;
			guarantee.CPH_OH_PermitHolder = permitHolder.PK;
			guarantee.CPH_RN_NKCountryCode = countryCode;
			guarantee.CPH_Type = type;
			return guarantee;
		}
	}
}
