using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

public class TransportDocumentsDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		using (var control = new TransportDocumentsDetailsUserControl())
		{
			AssertEquals(typeof(TransportDocument), control.BindingSource.DataSourceType);
		}
	}

	public void TestFieldsProperties()
	{
		using (var control = new TransportDocumentsDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_CodeCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.CSI_CodeCodeFindBox.CodeBox.CharacterCasing);
				AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, control.CSI_ReferenceNumberTextBox.CharacterCasing);
			});
		}
	}
}
