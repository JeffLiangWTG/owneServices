using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class SupportingInformationControlTest : TestCaseWithFactory
	{
		public void TestDynamicControlCreationUserControls()
		{
			using (var control = new MiscOptionsLayoutUserControl())
			{
				var additionalInfosUserControl = control.Controls.Find("additionalInfosUserControl", true).FirstOrDefault() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(AdditionalInfosUserControl), additionalInfosUserControl.UserControlType);

				var supportingDocumentsUserControl = control.Controls.Find("SupportingDocumentsUserControl", true).FirstOrDefault() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(SupportingDocumentsUserControl), supportingDocumentsUserControl.UserControlType);

				var previousDocumentsUserControl = control.Controls.Find("previousDocumentsUserControl", true).FirstOrDefault() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(PreviousDocumentsUserControl), previousDocumentsUserControl.UserControlType);

				var guaranteesUserControl = control.Controls.Find("GuaranteesUserControl", true).FirstOrDefault() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(GuaranteesUserControl), guaranteesUserControl.UserControlType);
			}
		}

		public void TestInitTabsVisibility()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			using (var control = new MiscOptionsLayoutUserControl())
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
	}
}
