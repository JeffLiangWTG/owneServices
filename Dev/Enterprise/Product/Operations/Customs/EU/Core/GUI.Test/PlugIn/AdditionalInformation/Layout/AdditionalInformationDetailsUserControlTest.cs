using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class AdditionalInformationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new AdditionalInformationDetailsUserControl())
			{
				AssertEquals(typeof(AdditionalInfo), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new AdditionalInformationDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("FullTypeCodeFindBox", () => control.FindSingle<ZCodeFindBox>("FullTypeCodeFindBox"));
					AssertNoExceptionThrown("DescriptionTextBox", () => control.FindSingle<ZTextBox>("DescriptionTextBox"));
					AssertNoExceptionThrown("KindDropEdit", () => control.FindSingle<ZDropEdit>("KindDropEdit"));
					AssertNoExceptionThrown("ReferenceTextBox", () => control.FindSingle<ZTextBox>("ReferenceTextBox"));
					AssertNoExceptionThrown("DetailTextBox", () => control.FindSingle<ZTextBox>("DetailTextBox"));
					AssertNoExceptionThrown("CurrencyDropEdit", () => control.FindSingle<ZDropEdit>("CurrencyDropEdit"));
					AssertNoExceptionThrown("AmountCalcEdit", () => control.FindSingle<ZCalcEdit>("AmountCalcEdit"));
				});
			}
		}
		public void TestControlsCasingAndDecimals()
		{
			using (var control = new AdditionalInformationDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("ReferenceTextBox casing", CharacterCasing.Normal, control.ReferenceTextBox.CharacterCasing);
					AssertEquals("DescriptionTextBox casing", CharacterCasing.Normal, control.DescriptionTextBox.CharacterCasing);
					AssertEquals("DetailTextBox casing", CharacterCasing.Normal, control.DetailTextBox.CharacterCasing);
					AssertEquals("AmountCalcEdit decimals", 2, control.AmountCalcEdit.Decimals);
				});
			}
		}
	}
}
