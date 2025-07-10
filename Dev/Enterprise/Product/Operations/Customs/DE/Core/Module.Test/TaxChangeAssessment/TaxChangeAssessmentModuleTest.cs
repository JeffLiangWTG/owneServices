using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(TaxChangeAssessmentModule))]
	class TaxChangeAssessmentModuleTest : ZModuleBasherTest
	{
		public void TestAttributes()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				CombineAssertions(() =>
				{
					AssertEquals("AllowDelete", false, module.AllowDelete);
					AssertEquals("AllowEdit", true, module.AllowEdit);
					AssertEquals("AllowNew", false, module.AllowNew);
					AssertEquals("AllowView", true, module.AllowView);
				});
			}
		}

		public override void TestModuleShowsAndCanSearch()
		{
			var taxChangeAssessment = Factory.NewWithValidTestData<TaxChangeAssessment>();
			taxChangeAssessment.EM_ApplicationReference = "NSTAXJ";
			Factory.Save();

			base.TestModuleShowsAndCanSearch();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.TaxChangeAssessment;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
