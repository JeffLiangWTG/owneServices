using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Core.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestDataSourceType()
		{
			AssertEquals(typeof(EDIMessageCollection), control.DataSourceType);
		}

		[RequiresSTA]
		public void TestMessageInterpretation()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();
				var interpretationControl = control.FindSingle<ZTextBox>("MessageInterpretationTextBox");
				AssertEquals("Someone's hidden MessageInterpretationTextBox again!", true, interpretationControl.Visible);
			}
		}

#if WINZOR
		[RequiresSTA]
		public void TestMessageInterpretationRichTextBox()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();
				var messageInterpretationRichTextBoxControl = control.FindSingle<ZRichTextBox>("MessageInterpretationRichTextBox");
				AssertEquals("Someone's hidden MessageInterpretationRichTextBox again!", true, messageInterpretationRichTextBoxControl.Visible);
			}
		}

		[RequiresSTA]
		public void TestRefreshMessageInterpretationRichTextBox_WhenInputIsXML()
		{
			using (var form = new ZForm())
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var interpretationControl = control.FindSingle<ZTextBox>("MessageInterpretationTextBox");
				var messageInterpretationRichTextBoxControl = control.FindSingle<ZRichTextBox>("MessageInterpretationRichTextBox");

				interpretationControl.Text = "<Test>Hellow</Test>";
				var messageInterpretationRichTextBoxControlHtmlExpected = GetEmbeddedResourceFile("expectedXmlOuput.html");

				AssertEquals("MessageInterpretationRichTextBox Incorrect Html!", messageInterpretationRichTextBoxControlHtmlExpected, messageInterpretationRichTextBoxControl.Html);
			}
		}
#endif

		[RequiresSTA]
		public void TestEM_ApplicationReferenceAvailable()
		{
			var columns = control.MessageGrid_Exposed.ColumnStyles.Cast<ZGridColumnInfo>();
			var expectedColumn = "EM_ApplicationReference";
			Assert("EM_ApplicationReference should be added.", columns.Any(x => x.ColumnName == expectedColumn));
			Assert("EM_ApplicationReference should be visible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
		}

		[RequiresSTA]
		public void TestMessageEdifactTextBox_Binding()
		{
			AssertEquals("BindTo", $"{nameof(EDIMessage.EM_MessageTextIndentedXml)}", control.MessageEdifactTextBox_Exposed.BindTo);
		}

		[RequiresSTA]
		public void TestMessageEdifactTextBox_FindDialog()
		{
			using (var form = new ZForm())
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var textBox = control.FindSingle<ZTextBox>("MessageEdifactTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("HideSelection", false, textBox.HideSelection);
					AssertEquals("EnableFindDialog", true, textBox.EnableFindDialog);
				});
			}
		}

		public string GetEmbeddedResourceFile(string embeddedResourceFile)
		{
			var retiever = new EmbeddedResourceRetriever();
			return retiever.GetString("Enterprise.Customs.EU.NCTS.GUI.Testing.Common.TestFiles." + embeddedResourceFile);
		}

		class MessageTabUserControlForTest : MessagesTabUserControl
		{
			public ZGrid MessageGrid_Exposed => MessageGrid;
			public ZTextBox MessageEdifactTextBox_Exposed => MessageEdifactTextBox;
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new MessageTabUserControlForTest();
		}
		MessageTabUserControlForTest control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
