using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ExportClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestTariffFindBoxVisibility()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new ExportClassificationUserControl())
			{
				AssertEquals("tariffFindBox Visible", false, control.tariffFindBox.Visible);
				AssertEquals("tariffFindBoxAHECC Visible", true, control.tariffFindBoxAHECC.Visible);
			}

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new ExportClassificationUserControl())
			{
				AssertEquals("tariffFindBox Visible", true, control.tariffFindBox.Visible);
				AssertEquals("tariffFindBoxAHECC Visible", false, control.tariffFindBoxAHECC.Visible);
			}
		}

		public void TestTariffFindBoxEffectiveTariffCountryAndEffectiveDataGrouping()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (var control = new ExportClassificationUserControl())
				{
					AssertEquals("AU", control.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AU", control.tariffFindBox.EffectiveDataGrouping);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				using (var control = new ExportClassificationUserControl())
				{
					AssertEquals("AUT", control.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AUT", control.tariffFindBox.EffectiveDataGrouping);
				}
			}
		}
	}
}
