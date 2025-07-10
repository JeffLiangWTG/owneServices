using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(ImportClassificationForm))]
	sealed class ImportClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestMenuIsNotNull()
		{
			using (ImportClassificationForm form = new ImportClassificationForm(Factory.New<Classification>()))
			{
				AssertNotNull("Menu should not be null.", form.Menu);
			}
		}

		public void TestFormBorderStyle()
		{
			using (ImportClassificationForm form = (ImportClassificationForm)GetFormToBash())
			{
				AssertEquals("FormBorderStyle", FormBorderStyle.FixedSingle, form.FormBorderStyle);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override Form GetFormToBashCore() => new ImportClassificationForm(Factory.New<Classification>());
	}
}
