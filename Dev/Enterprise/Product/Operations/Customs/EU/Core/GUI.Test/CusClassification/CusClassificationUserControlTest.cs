using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class CusClassificationUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCC_TariffNumFindBox()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Guadeloupe;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
			using (var form = new CusClassificationForm(classification))
			{
				form.Show();
				var tariffNumFindBox = form.Controls.Find("TariffFindBox", true)[0] as Universal.GUI.TariffFindBox;
				AssertEquals("CC_TariffNumFindBox.GetCountryCode()", Core.Constants.CountryCodes.Guadeloupe, tariffNumFindBox.GetCountryCode());

				classification.Delete();
				AssertEquals("CC_TariffNumFindBox.GetCountryCode()", Core.Constants.CountryCodes.Liechtenstein, tariffNumFindBox.GetCountryCode());

				using (var control = new CusClassificationUserControl())
				{
					tariffNumFindBox = control.Controls.Find("TariffFindBox", true)[0] as Universal.GUI.TariffFindBox;
					AssertEquals("CC_TariffNumFindBox.GetCountryCode()", Core.Constants.CountryCodes.Liechtenstein, tariffNumFindBox.GetCountryCode());
				}
			}
		}

		[RequiresSTA]
		public void TestSupplementaryCodes()
		{
			using (var control = new CusClassificationUserControl())
			{
				AssertNotNull("CC_EcAdditionalSupplementsTextBox", control.cC_EcAdditionalSupplementsTextBox);
				AssertNotNull("AdditionalSupplementaryCodesEditButton", control.additionalSupplementaryCodesEditButton);
			}
		}

		public void TestTariffFindBox()
		{
			var classification = Factory.New<CusClassification>();
			using (var form = new CusClassificationForm(classification))
			{
				form.Show();
				var zTariffFindBox = form.Controls.Find("TariffFindBox", true)[0] as Universal.GUI.TariffFindBox;
				AssertEquals("IMP", zTariffFindBox.GetTariffType());
				classification.CC_ClassificationType = "IMP";
				AssertEquals("IMP", zTariffFindBox.GetTariffType());
				classification.CC_ClassificationType = "EXP";
				AssertEquals("EXP", zTariffFindBox.GetTariffType());
				classification.CC_ClassificationType = "BTH";
				AssertEquals("IMP", zTariffFindBox.GetTariffType());
			}
		}

		public void TestTariffFindBoxForGB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var classification = Factory.New<CusClassification>();
				using (var form = new CusClassificationForm(classification))
				{
					form.Show();
					AssertType<Universal.GUI.TariffFindBox>(form.Controls.Find("TariffFindBox", searchAllChildren: true)[0]);
				}
			}
		}
	}
}
