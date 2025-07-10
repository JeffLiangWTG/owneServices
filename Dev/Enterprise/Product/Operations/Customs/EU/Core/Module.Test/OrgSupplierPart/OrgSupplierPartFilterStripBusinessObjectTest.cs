using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	class OrgSupplierPartFilterBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
	{
		public void TestTariffFilter()
		{
			var part1 = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "PartNum1";
			var euPivot1 = part1.PivotsForBinding.AddNew();
			euPivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.Latvia;
			euPivot1.CI_TariffNum = "11111111";
			var usPivot1 = (Customs.Business.BaseCusClassPartPivot)Factory.New(new Customs.Business.BaseCusClassPartPivotTypeDecider().GetTypeForCountryCode(Core.Constants.CountryCodes.UnitedStates));
			usPivot1.CI_OP = part1.PK;
			usPivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			usPivot1.CI_TariffNum = "22222222";

			var part2 = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "PartNum2";
			var euPivot2 = part2.PivotsForBinding.AddNew();
			euPivot2.CI_RN_NKCountry = Core.Constants.CountryCodes.Latvia;
			euPivot2.CI_TariffNum = "33333333";
			var usPivot2 = (Customs.Business.BaseCusClassPartPivot)Factory.New(new Customs.Business.BaseCusClassPartPivotTypeDecider().GetTypeForCountryCode(Core.Constants.CountryCodes.UnitedStates));
			usPivot2.CI_OP = part2.PK;
			usPivot2.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			usPivot2.CI_TariffNum = "44444444";
			Factory.Save();

			var stripBO = (OrgSupplierPartFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO["Tariff"];
			var coll = new Customs.Business.OrgSupplierPartCollection(Factory);
			filter.IsActive = true;

			filter.Property = "11";
			coll.Load(stripBO.Filter);
			Assert(!coll.Contains(part2));
			Assert(coll.Contains(part1));

			filter.Property = "22";
			coll.Load(stripBO.Filter);
			Assert(!coll.Contains(part2));
			Assert(!coll.Contains(part1));

			filter.Property = "33";
			coll.Load(stripBO.Filter);
			Assert(coll.Contains(part2));
			Assert(!coll.Contains(part1));

			filter.Property = "44";
			coll.Load(stripBO.Filter);
			Assert(!coll.Contains(part2));
			Assert(!coll.Contains(part1));

			filter.Property = "1.1 1";
			coll.Load(stripBO.Filter);
			Assert(coll.Contains(part1));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();
	}
}
