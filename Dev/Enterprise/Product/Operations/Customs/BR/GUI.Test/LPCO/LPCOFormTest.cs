using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(LPCOForm))]
	sealed class LPCOFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestWorkflowTabPage()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			using (var form = new LPCOForm(lpcoHeader))
			{
				form.Show();
				var workflowTabPage = form.WorkflowTabPage;
				Assert(workflowTabPage.TabRelevant);
			}
		}

		public void TestMenuItems()
		{
			using (var form = GetFormToBashCore())
			{
				var menuItems = form.Menu.MenuItems;
				AssertSequencesEqual(new[] { "&File", "&Edit", "Actio&ns", "Send to Customs", "&Help" }, menuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestMessageTab()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			using (var form = new LPCOForm(lpcoHeader))
			{
				form.Show();
				var messageTabPage = form.MessageTabPage;
				AssertEquals("Messages", messageTabPage.CaptionResourceString.Caption);
				AssertType<MessagesTabUserControl>(messageTabPage.Controls[0]);

				var messageStatusDropEdit = messageTabPage.Controls.Find("MessageStatusDropEdit", true).FirstOrDefault() as ZDropEdit;
				AssertNotNull(messageStatusDropEdit);
			}
		}

		public void TestReorderTabPages()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			using (var form = new LPCOForm(lpcoHeader))
			using (var control = new ImportInvoiceLineUserControl())
			{
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				var tabPages = mainTabControl.TabPages;
				AssertSequencesEqual(new[]
				{
					"MainTabPage",
					"MessageTabPage",
					"WorkflowTabPage",
					"NotesTabPage",
					"LogsTabPage"
				}, tabPages.Cast<ZTabPage>().Where(x => x.TabVisible).Select(x => x.Name).ToArray());
			}
		}

		public void TestFormCaption()
		{
			var holder = OrgHeader.New(Factory);
			holder.OH_Code = "TEST1";

			var lpcoHeader = Factory.New<CusLPCOHeader>();
			lpcoHeader.CPH_Number = "111";
			lpcoHeader.CPH_OH_PermitHolder = holder.PK;

			using (var form = new LPCOForm(lpcoHeader))
			{
				AssertEquals("Caption should be", "LPCO TEST1 - 111", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusLPCOHeader>();
			var form = new LPCOForm(header);
			form.ControllerID = ControllerIDs.Customs.BR.LPCO;
			return form;
		}
	}
}
