using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestSendManifest()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CustomsManifest, Core.Constants.CountryCodes.Spain, ZDateTime.Now, true))
			{
				var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
				header.AMA_RL_NKPortOfLoading = "DEFRA";
				header.AMA_RL_NKPortOfDischarge = "ESMIL";
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
				header.AMA_ManifestType = ESManifestTypes.Codes.ICS;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.Bills.AddNew();
				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						AssertEquals("Build menu as required", 0, menu.MenuItems.Count);
					}
				}
			}
		}
	}
}
