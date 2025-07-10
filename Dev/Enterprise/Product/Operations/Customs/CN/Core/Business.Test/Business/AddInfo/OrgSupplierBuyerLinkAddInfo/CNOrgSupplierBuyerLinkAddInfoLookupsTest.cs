using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNOrgSupplierBuyerLinkAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcedureCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CN", "A", "10", "", "", "Import", "IMP");
			helper.CreateRefCusProcedure("CN", "A", "11", "", "", "Export", "EXP");
			helper.CreateRefCusProcedure("CN", "H", "60", "", "", "Export", "EXP");
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var link = Factory.New<OrgSupplierBuyerLink>();
				var lookups = new CNOrgSupplierBuyerLinkAddInfoLookups(new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo()));
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
				AssertEquals("No Procedure for AU", 0, lookups.ProcedureCodeList.Count);
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals("No Procedure for AU", 0, lookups.ProcedureCodeList.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var link = Factory.New<OrgSupplierBuyerLink>();
				var lookups = new CNOrgSupplierBuyerLinkAddInfoLookups(new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo()));
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
				AssertEquals("Getting List for CN", 1, lookups.ProcedureCodeList.Count);
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals("Getting List for CN", 2, lookups.ProcedureCodeList.Count);
			}

			var newFactory = new BusinessObjectFactory();
			new UniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure("CN", "", "XX", "", "", "Export", "EXP");
			newFactory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var link = Factory.New<OrgSupplierBuyerLink>();
				var lookups = new CNOrgSupplierBuyerLinkAddInfoLookups(new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo()));
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
				AssertEquals("Use Cached List", 1, lookups.ProcedureCodeList.Count);
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals("Use Cached List", 2, lookups.ProcedureCodeList.Count);
				Factory.ClearCachedValue<CodeDescriptionPairList>(string.Format(CultureInfo.InvariantCulture, "CN_EXP_{0}_RefCusProcedures_ProcedureCode", ZDateTime.Today));
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals("Clear Cache and Get New List for CN EXP", 3, lookups.ProcedureCodeList.Count);
			}
		}

		public void TestLevyTypeList()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var lookups = new CNOrgSupplierBuyerLinkAddInfoLookups(new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo()));
			var list = lookups.LevyTypeList;
			AssertEquals(new LevyTypeList().Count, list.Count);
			var newLink = Factory.New<OrgSupplierBuyerLink>();
			var newLookups = new CNOrgSupplierBuyerLinkAddInfoLookups(new CNOrgSupplierBuyerLinkAddInfo(newLink.GetAddInfo()));
			var newList = newLookups.LevyTypeList;
			AssertSame(newList, list);
			Assert("levy type list should not be translatable", lookups.LevyTypeList is UntranslatableCodeDescriptionPairList);
		}
	}
}
