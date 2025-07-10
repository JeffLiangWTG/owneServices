using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundDeclarationForm))]
	public class RefundDeclarationFormTest : ZFormBasherTest
	{
		public void TestAddMessageContentsTab()
		{
			using (var form = new RefundDeclarationForm(Declaration))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals(6, tabControl.Controls.Count);
				var index = 0;
				AssertEquals(tabControl.Controls[index++].Name, "MainTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "ImportEntriesTabPage");
				AssertEquals(tabControl.Controls[index].Name, "MessagesTabPage");
				AssertNotNull(tabControl.Controls[index++].Controls.Find("MessagesUserControl", true)[0]);
				AssertEquals(tabControl.Controls[index++].Name, "eDocsTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "NotesTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "LogsTabPage");
			}
		}

		public void TestFormCaption()
		{
			using (var form = new RefundDeclarationForm(Declaration))
			{
				AssertEquals("Refund Declaration ", form.FormCaption);
			}
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_ApplicationCode = "KRC";
			reconDeclaration.CRD_JobReferenceNumber = "CRD00000001";
			using (var form = new RefundDeclarationForm(reconDeclaration))
			{
				AssertEquals("Refund Declaration  - CRD00000001", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new RefundDeclarationForm(Declaration);
			result.ControllerID = ControllerIDs.Customs.KR.CusReconDeclaration;
			return result;
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		CusReconDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<CusReconDeclaration>()); }
		}
		CusReconDeclaration declaration;
	}
}
