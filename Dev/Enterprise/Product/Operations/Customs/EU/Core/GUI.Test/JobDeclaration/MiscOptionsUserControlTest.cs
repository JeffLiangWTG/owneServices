using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestInitTabsVisibility()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			using (var control = new MiscOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var supportingDocument = control.FindSingle<ZTabPage>("SupportingDocumentTabPage");
				var additionalInfo = control.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				var previousDocument = control.FindSingle<ZTabPage>("PreviousDocumentTabPage");
				var guarantees = control.FindSingleOrDefault<ZTabPage>("GuaranteesTabPage");
				var supportingInformationTabControl = control.FindSingle<ZTabControl>("SupportingInformationTabControl");
				CombineAssertions(() =>
				{
					AssertEquals("Supporting Documents visible", true, supportingDocument.TabVisible);
					AssertEquals("Additional Info visible", true, additionalInfo.TabVisible);
					AssertEquals("Previous Documents visible", true, previousDocument.TabVisible);
					AssertNull("Guarantees not visible", guarantees);
					AssertEquals("SupportingInformationTabControl visible", true, supportingInformationTabControl.Visible);
				});
			}
		}

		public void TestRepresentationDropEdit_CaptionResourceString()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			using (var control = new MiscOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var representationDropEdit = control.FindSingle<ZDropEdit>("RepresentationDropEdit");
				AssertEquals("[14] Rep. Type", representationDropEdit.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}
	}
}
