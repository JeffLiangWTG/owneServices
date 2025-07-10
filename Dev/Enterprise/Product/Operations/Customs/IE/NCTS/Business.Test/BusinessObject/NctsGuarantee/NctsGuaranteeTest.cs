using System;
using System.Data;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuarantee))]
	sealed class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CreateNewObject(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewObject(factory);

		public void TestValidationType()
		{
			var nctsGuarantee = CreateNewObject(Factory);
			AssertType<NctsGuaranteeValidation>(nctsGuarantee.Validation);
		}

		public void TestDefaultLiabilityAmountWhenZeroDuties()
		{
			var propertyInfo = typeof(NctsGuarantee).GetProperty("Phase5DefaultLiabilityAmount", BindingFlags.NonPublic | BindingFlags.Instance);
			var nctsGuarantee = (NctsGuarantee)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			AssertEquals(10000m, propertyInfo.GetValue(nctsGuarantee));
		}

		public void TestCusGuaranteeTypeFilter()
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Ireland);

			var nctsHeader = Factory.NewWithValidTestData<NctsHeaderForGuaranteesTesting>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var cusGuarantee = Factory.New<CusGuaranteeHeader>();
			cusGuarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuarantee.CPH_Number = "GUA2";
			cusGuarantee.CPH_OH_PermitHolder = nctsHeader.Principal.Organisation.PK;
			cusGuarantee.CPH_SubType = "1";
			cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
			cusGuarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;
			nctsGuarantee.PW_BondNumber = "GUA2";

			cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			AssertSame("IE NctsGuarantee.CusGuarantee should match the CusGuaranteeHeader despite CPH_Type is TRA or other.", cusGuarantee, nctsGuarantee.CusGuarantee);

			cusGuarantee.CPH_Type = "1";
			AssertSame("IE NctsGuarantee.CusGuarantee should match the CusGuaranteeHeader despite CPH_Type is TRA or other.", cusGuarantee, nctsGuarantee.CusGuarantee);

			nctsGuarantee.IsCusGuaranteeType = false;
			AssertNull("CusGuarantee returns null when CusGuaranteeTypeFilter returns false.", nctsGuarantee.CusGuarantee);
		}

		NctsGuarantee CreateNewObject(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return nctsHeader.Guarantees.AddNew();
		}
	}

	class NctsHeaderForGuaranteesTesting : NctsHeader
	{
		public NctsHeaderForGuaranteesTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsDepartureMovementHeaderForGuaranteesTesting MovementHeader => (NctsDepartureMovementHeaderForGuaranteesTesting)base.MovementHeader;

		protected override EU.NCTS.Business.NctsDepartureMovementHeader GetNewDepartureMovementHeader() => NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeaderForGuaranteesTesting>(this, "D");

		public new INctsGuaranteeCollection<NctsGuaranteeForTypeFilterTesting> Guarantees => (INctsGuaranteeCollection<NctsGuaranteeForTypeFilterTesting>)base.Guarantees;
	}

	class NctsDepartureMovementHeaderForGuaranteesTesting : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForGuaranteesTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuaranteeForTypeFilterTesting>(this);

		public new INctsGuaranteeCollection<NctsGuaranteeForTypeFilterTesting> Guarantees => (INctsGuaranteeCollection<NctsGuaranteeForTypeFilterTesting>)base.Guarantees;
	}

	class NctsGuaranteeForTypeFilterTesting : NctsGuarantee
	{
		public NctsGuaranteeForTypeFilterTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsCusGuaranteeType { get; set; } = true;

		protected override Func<CusGuaranteeHeader, bool> CusGuaranteeTypeFilter => _ => IsCusGuaranteeType;
	}
}
