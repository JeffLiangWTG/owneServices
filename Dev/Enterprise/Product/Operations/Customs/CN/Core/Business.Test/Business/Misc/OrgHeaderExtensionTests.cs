using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OrgHeaderExtensionTests : TestCaseWithFactory
	{
		public void TestGetChinaCustomsRegNo()
		{
			AssertEquals("CustomsClientCode", "12345", orgHeader.GetChinaCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode));
			AssertEquals("AgentCode", ZString.Empty, orgHeader.GetChinaCustomsRegNo(OrgCusCode.CodeTypes.AgentCode));

			AssertEquals("USCI", "USCI1112", orgHeader.GetUSCI());
			AssertEquals("CCD", "12345", orgHeader.GetCCD());
			AssertEquals("CIQ", "CIQ12356", orgHeader.GetCIQ());
		}

		public void TestGetEconomicZoneType()
		{
			var code = orgHeader.GetEconomicZoneType(OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("5", code);

			code = orgHeader.GetEconomicZoneType(OrgCusCode.CodeTypes.BondHolderCode);
			AssertEquals(ZString.Empty, code);
		}

		public void TestIsInSupervisionArea()
		{
			Assert(orgHeader.IsInSupervisionArea());

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "12343", Core.Constants.CountryCodes.China);
			Assert(!orgHeader1.IsInSupervisionArea());
		}

		public void TestGetGetChinaCustomsRegNoForNullOrgHeader()
		{
			OrgHeader orgHeader = null;
			AssertEquals("USCI", "", orgHeader.GetUSCI());
			AssertEquals("CCD", "", orgHeader.GetCCD());
			AssertEquals("CIQ", "", orgHeader.GetCIQ());
		}

		public void TestUntranslatableEconomicZone()
		{
			Assert("Economic zone type list should not be translatable", new EconomicZoneTypeList() is UntranslatableCodeDescriptionPairList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "45678", Core.Constants.CountryCodes.China);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "12345", Core.Constants.CountryCodes.China);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "78901", Core.Constants.CountryCodes.UnitedStates);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "123", Core.Constants.CountryCodes.China);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "USCI1112", Core.Constants.CountryCodes.China);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.CIQ, "CIQ12356", Core.Constants.CountryCodes.China);
		}
		OrgHeader orgHeader;
	}
}
