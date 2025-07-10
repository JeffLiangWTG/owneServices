using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class GDMBasicUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new GDMBasicUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("EffectiveDateDateEdit", control.FindSingle<ZDateEdit>("EffectiveDateDateEdit"));
					AssertNotNull("TariffCodeFindBox", control.FindSingle<ZCodeFindBox>("TariffCodeFindBox"));
					AssertNotNull("CountryOfDestinationDropEdit", control.FindSingle<ZDropEdit>("CountryOfDestinationDropEdit"));
					AssertNotNull("CountryOfOriginDropEdit", control.FindSingle<ZDropEdit>("CountryOfOriginDropEdit"));
					AssertNotNull("PreferenceDropEdit", control.FindSingle<ZDropEdit>("PreferenceDropEdit"));
					AssertNotNull("QuotaOrderNumberDropEdit", control.FindSingle<ZDropEdit>("QuotaOrderNumberDropEdit"));
					AssertNotNull("CustomsFirstQuantityCalcEdit", control.FindSingle<ZCalcEdit>("CustomsFirstQuantityCalcEdit"));
					AssertNotNull("CustomsSecondQuantityCalcDropEdit", control.FindSingle<ZCalcDropEdit>("CustomsSecondQuantityCalcDropEdit"));
					AssertNotNull("CustomsFirstQuantityCalcEdit", control.FindSingle<ZCalcDropEdit>("CustomsThirdQuantityCalcDropEdit"));
					AssertEquals("EffectiveDateDateEdit should be readonly", control.FindSingle<ZDateEdit>("EffectiveDateDateEdit").ReadOnly, true);
				});
			}
		}
	}
}
