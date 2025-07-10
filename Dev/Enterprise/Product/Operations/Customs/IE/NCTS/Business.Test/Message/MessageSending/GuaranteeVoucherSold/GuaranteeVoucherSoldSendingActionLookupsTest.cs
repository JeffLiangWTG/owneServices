using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class GuaranteeVoucherSoldSendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHolderOfTransitProcedureList()
		{
			var header = Factory.New<IE.Business.CusGuaranteeHeader>();
			var lookups = new GuaranteeVoucherSoldSendingAction(header).Lookups;
			var holderOfTransitProcedureList = lookups.HolderOfTransitProcedure;
			AssertNotNull(holderOfTransitProcedureList);
			AssertType<OrgHeaderCollection>(holderOfTransitProcedureList);
		}

		public void TestCustomsOfficeOfGuaranteeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeIEDUB100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeGB000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C#@";
			company.GC_Name = "COMP TEST";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B#@";
			branch.GB_BranchName = "BRANCH TEST";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			var header = Factory.New<IE.Business.CusGuaranteeHeader>();
			using (branch.SetAsTemporaryContext())
			{
				var lookups = new GuaranteeVoucherSoldSendingAction(header).Lookups;
				var officeCodeList = lookups.CustomsOfficeOfGuarantee;
				officeCodeList.Load();
				AssertEquals(1, officeCodeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "IEDUB100" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			}
		}
	}
}
