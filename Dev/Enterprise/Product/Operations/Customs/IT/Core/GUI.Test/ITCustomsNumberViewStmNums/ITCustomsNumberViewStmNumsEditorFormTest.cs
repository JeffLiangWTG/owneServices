using System.Windows.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ITCustomsNumberViewStmNumsEditorForm))]
sealed class ITCustomsNumberViewStmNumsEditorFormTest : ZFormBasherTest
{
	public void TestFountainNameTextBoxIsNotVisible()
	{
		using (var form = GetFormToBash())
		{
			form.Show();
			var fountainNameTextBox = (ZTextBox)form.Controls.Find("FountainNameTextBox", true)[0];
			AssertEquals(false, fountainNameTextBox.Visible);
		}
	}

	protected override Form GetFormToBashCore()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var stmNumsProvider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, currentCompany.GC_RN_NKCountryCode, currentCompany.PK);
		CustomsNumberViewStmNumsHelper.NewStmNums(Factory, stmNumsProvider, currentCompany.PK);
		Factory.Save();
		var stmNumsLoaded = CustomsNumberViewStmNumsHelper.LoadStmNums(Factory, new CargoWise.EntityFramework.ZQuery())[0];
		var stmNumsWrapper = new ITCustomsNumberViewStmNumsWrapper(stmNumsLoaded);
		return new ITCustomsNumberViewStmNumsEditorForm(stmNumsWrapper);
	}
}
