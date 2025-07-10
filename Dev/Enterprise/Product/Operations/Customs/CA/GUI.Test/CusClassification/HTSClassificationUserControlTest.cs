using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class HTSClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestTariffUserControlForCAGlobalTariff()
		{
			var tariffFindBoxName_TrfCA = "tariffCodeFindBox";
			var tariffFindBoxName_SRDb = "tariffCodeFromSRDbFindBox";
			var tariffFindBoxType = typeof(Customs.GUI.TariffFindBox);
			using (var form = new ZForm())
			using (var control = new HTSClassificationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(control, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}
	}
}
