using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHItemWrapperTest : TestCaseWithFactory
	{
		public void TestCusCAeMHItemWrapperProperties()
		{
			AssertEquals(123m, wrapper.Quantity);
			AssertEquals(ACROSSPackageTypes.Descriptions.AMMOPACK, wrapper.QuantityUQName);
			AssertEquals("9999.99.99", wrapper.HSCode);
			AssertEquals("LINE1", wrapper.GoodsDescription);
			AssertEquals("Marks&Numbers", wrapper.MarksAndNumbers);
			AssertEquals("XXXX", wrapper.DGCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "XXXX";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsA = UNDGSubstanceLoader.LoadSubstances(Factory, "XXXX", "", "IMO").First();

			OrgContact dgContact = Factory.NewWithValidTestData<OrgContact>();
			dgContact.OC_ContactName = "DG CONTACT";
			dgContact.OC_Email = "default@cargowise.com";

			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var item = house.Items.AddNew();
			item.BX_Quantity = 123m;
			item.BX_QuantityUQ = ACROSSPackageTypes.Codes.AMMOPACK;
			item.BX_HSCode = "9999.99.99";
			item.BX_Description = "LINE1";
			item.BX_Marks = "Marks&Numbers";
			var undg = item.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.DI_DG = subsA.PK;
			undg.DI_OC_DGContact = dgContact.PK;
			Factory.Save();

			wrapper = new CusCAeMHItemWrapper(item, 1);
		}

		CusCAeMHItemWrapper wrapper;
	}
}
