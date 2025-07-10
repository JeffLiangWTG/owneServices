using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ImportClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestTariffFindBoxVisibility()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var control = new ImportClassificationUserControl())
			{
				AssertEquals("tariffFindBox Visible", false, control.tariffFindBox.Visible);
				AssertEquals("tariffFindBox for AUCClass Visible", true, control.tariffFindBoxAUCClass.Visible);
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var control = new ImportClassificationUserControl())
			{
				AssertEquals("tariffFindBox Visible", true, control.tariffFindBox.Visible);
				AssertEquals("tariffFindBox for AUCClass Visible", false, control.tariffFindBoxAUCClass.Visible);
			}
		}

		public void TestTariffFindBoxEffectiveTariffCountryAndEffectiveDataGrouping()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (var control = new ImportClassificationUserControl())
				{
					AssertEquals("AU", control.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AU", control.tariffFindBox.EffectiveDataGrouping);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				using (var control = new ImportClassificationUserControl())
				{
					AssertEquals("AUT", control.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AUT", control.tariffFindBox.EffectiveDataGrouping);
				}
			}
		}
	}
}
