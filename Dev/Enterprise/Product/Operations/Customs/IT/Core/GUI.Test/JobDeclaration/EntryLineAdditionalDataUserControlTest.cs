using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestGroupedPreviousDocumentsUserControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();

		using (var form = new ZForm(declaration))
		using (var userControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var extendInfoTabControl = userControl.FindSingleOrDefault<ZTabControl>("ExtendInfoTabControl");
			AssertNotNull(nameof(extendInfoTabControl), extendInfoTabControl);

			var m2LinesTabPage = extendInfoTabControl.FindSingleOrDefault<ZTabPage>("M2LinesTabPage");
			AssertNotNull(nameof(m2LinesTabPage), m2LinesTabPage);

			extendInfoTabControl.SelectTab(m2LinesTabPage);
			var groupedPreviousDocumentsUserControl = m2LinesTabPage.FindSingleOrDefault<GroupedPreviousDocumentsUserControl>("GroupedPreviousDocumentsUserControl");
			AssertNotNull(nameof(groupedPreviousDocumentsUserControl), groupedPreviousDocumentsUserControl);
			AssertEquals($"{nameof(groupedPreviousDocumentsUserControl)} visibility", true, groupedPreviousDocumentsUserControl.Visible);
		}
	}

	public void TestM2LinesTabPage()
	{
		using (var userControl = new EntryLineAdditionalDataUserControl())
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			userControl.SetDataBinding(declaration, "");
			var m2LinesTabPage = GetM2LinesTabPageControl();
			AssertNull("M2LinesTabPage, for IMP", m2LinesTabPage);

			declaration.JE_MessageType = "EXP";
			declaration.MessageVersion = "TXT";
			m2LinesTabPage = GetM2LinesTabPageControl();
			AssertEquals("M2LinesTabPage, Visibility for EXP with UCC6=false", true, m2LinesTabPage.TabVisible);

			declaration.MessageVersion = "XML";
			m2LinesTabPage = GetM2LinesTabPageControl();
			AssertNull("M2LinesTabPage, for EXP with UCC6=true", m2LinesTabPage);

			ZTabPage GetM2LinesTabPageControl() => userControl.FindSingleOrDefault<ZTabPage>("M2LinesTabPage");
		}
	}
}
