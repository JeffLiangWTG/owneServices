using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class CustomsMessagingControlTest : TestCaseWithFactory
	{
		public void TestMessageGrid()
		{
			using (var control = new CustomsMessagingControl())
			{
				var grid = control.FindSingleOrDefault<MessageZGrid>("MessagesBoundGrid");

				CombineAssertions(() =>
				{
					var visibleColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible).ToArray();
					AssertEquals("Grid should have 5 columns", 5, visibleColumns.Length);
					AssertEquals("EM_MessageNum", visibleColumns[0].ColumnName);
					AssertEquals("EM_MessageType", visibleColumns[1].ColumnName);
					AssertEquals("EM_Status", visibleColumns[2].ColumnName);
					AssertEquals("EM_User", visibleColumns[3].ColumnName);
					AssertEquals("EM_MessageDateTime", visibleColumns[4].ColumnName);
				});
			}
		}

		public void TestMessageDetailsTabPage()
		{
			using (var control = new CustomsMessagingControl())
			{
				var tab = control.FindSingleOrDefault<ZTabPage>("MessageDetailsTabPage");
				AssertEquals("Tab caption should be 'Interpretation'", "Interpretation", tab.Text);
			}
		}

		public void TestMessageTabControl()
		{
			using (var control = new CustomsMessagingControl())
			{
				var tabControl = control.FindSingleOrDefault<ZTabControl>("MessageTabControl");
				AssertEquals("Tab control should fill all the available space", DockStyle.Bottom, tabControl.Dock);

				AssertEquals("CustomsMessagingControl Should have 2 TabPages", 2, tabControl.TabPages.Count);
				var messageDetailsTabPage = (ZTabPage)tabControl.TabPages[0];
				AssertEquals("Should Contain MessageDetailsTabPage in the first tab", "MessageDetailsTabPage", messageDetailsTabPage.Name);
				AssertEquals("MessageDetailsTabPage.Text", "Interpretation", messageDetailsTabPage.Text);
				AssertEquals("interpretedMessageTabPage.Controls.Count == 1", 1, messageDetailsTabPage.Controls.Count);
				var innerMessageDetailsTabControl = messageDetailsTabPage.FindSingleOrDefault<ZTabControl>("InnerMessageDetailsTabControl");
				AssertNotNull("InnerMessageDetailsTabControl should be present", innerMessageDetailsTabControl);
				AssertEquals("InnerMessageDetailsTabControl Should have 2 TabPages", 2, tabControl.TabPages.Count);
				var htmlTabPage = innerMessageDetailsTabControl.TabPages[0];
				AssertEquals("InnerMessageDetailsTabControl.TabPages[0].Name", "HtmlTabPage", htmlTabPage.Name);
				AssertEquals("InnerMessageDetailsTabControl.TabPages[0].Text", "Message Interpretation", htmlTabPage.Text);
				var xmlTabPage = innerMessageDetailsTabControl.TabPages[1];
				AssertEquals("InnerMessageDetailsTabControl.TabPages[1].Name", "XmlTabPage", xmlTabPage.Name);
				AssertEquals("InnerMessageDetailsTabControl.TabPages[1].Text", "XML Interpretation", xmlTabPage.Text);
				var xmlInterpretedMessageTextBox = xmlTabPage.FindSingleOrDefault<ZTextBox>("XmlInterpretedMessageTextBox");
				AssertEquals("xmlInterpretedMessageTextBox.BindTo should be Messages.EM_FormattedMessageText", "Messages.EM_FormattedMessageText", xmlInterpretedMessageTextBox.BindTo);
				var interpretedMessageTextWebBrowser = xmlTabPage.FindSingleOrDefault<ZWebBrowser>("XmlInterpretedMessageTextWebBrowser");
				AssertNotNull("XmlInterpretedMessageTextWebBrowser should be present", interpretedMessageTextWebBrowser);
			}
		}

		public void TestVerticalSplitter()
		{
			using (var control = new CustomsMessagingControl())
			{
				var verticalSplitter = control.FindSingleOrDefault<KSplitter>("VerticalSplitter");
				Assert("Vertical splitter should be invisible", !verticalSplitter.Visible);
			}
		}

		public void TestHorizontalSplitter()
		{
			using (var control = new CustomsMessagingControl())
			{
				var horizontalSplitter = control.FindSingleOrDefault<KSplitter>("HorizontalSplitter");

				AssertNotNull("Horizontal splitter should be present", horizontalSplitter);
				Assert("Horizontal splitter should be visible", horizontalSplitter.Visible);
				AssertEquals("Horizontal splitter should be docked to the bottom", DockStyle.Bottom, horizontalSplitter.Dock);
			}
		}
	}
}
