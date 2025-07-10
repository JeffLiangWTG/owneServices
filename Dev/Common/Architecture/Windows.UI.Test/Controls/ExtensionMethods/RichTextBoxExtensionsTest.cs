using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class RichTextBoxExtensionsTest : TestCase
	{
		public void TestSetGetLink()
		{
			TextBox.Text = "Text Link Text";
			TextBox.Select(5, 4);
			TextBox.SetSelectionLink(true);

			TextBox.Select(0, 4);
			AssertEquals("Not A Link", false, TextBox.GetSelectionLink());

			TextBox.Select(5, 4);
			AssertEquals("Link", true, TextBox.GetSelectionLink());

			TextBox.Select(2, 5);
			AssertEquals("Some Link, Some Not", null, TextBox.GetSelectionLink());

			TextBox.Select(4, 6);
			TextBox.SetSelectionLink(false);
			TextBox.Select(0, 14);
			AssertEquals("Link Flag Cleared", false, TextBox.GetSelectionLink());
		}

		public void TestSetGetNumberedList()
		{
			var items = Enumerable.Range(1, 10).Select(n => "Item " + n).ToArray();

			Array.ForEach(items, text => TextBox.SelectedText = text + "\n");

			var expectedText = string.Join("[^{}]*?", items);
			Assert("TextBox rtf should not include any numbering", Regex.IsMatch(TextBox.Rtf, expectedText, RegexOptions.Singleline));

			TextBox.Clear();
			TextBox.SetSelectionNumberedList(true);
			AssertEquals(true, TextBox.GetSelectionNumberedList());

			Array.ForEach(items, text => TextBox.SelectedText = text + "\n");

			var numberedRtfRegexItems = items.Select((item, index) => string.Format(@"\\pntext{0}{1}\.{0}{2}{0}", ".*?", index + 1, item)).ToArray();
			expectedText = string.Join("", numberedRtfRegexItems);

			Assert("Textbox rtf should include numbering in a format similar to {\\pntext\\f0 1.}Text here", Regex.IsMatch(TextBox.Rtf, expectedText, RegexOptions.Singleline));

			TextBox.SetSelectionNumberedList(false);
			AssertEquals(false, TextBox.GetSelectionNumberedList());

			var additionalText = "Some more text";
			TextBox.SelectedText = additionalText;
			Assert("TextBox Rtf should match the the previous expected added to the additional text without numbering for the next line", Regex.IsMatch(TextBox.Rtf, string.Format("{0}[^{{}}]*?({1})", expectedText, additionalText), RegexOptions.Singleline));
		}

		public void TestBulletsCancelNumberedList()
		{
			TextBox.SetSelectionNumberedList(true);
			AssertEquals(true, TextBox.GetSelectionNumberedList());

			TextBox.SelectionBullet = true;
			AssertEquals(false, TextBox.GetSelectionNumberedList());
		}

		#region Implementation

		protected override void TearDown()
		{
			if (textBox != null)
			{
				KForm form = (KForm)textBox.FindForm();

				if (form != null)
				{
					form.Dispose();
				}
				else
				{
					textBox.Dispose();
				}
			}
			base.TearDown();
		}

		RichTextBox TextBox
		{
			get
			{
				if (textBox == null)
				{
					textBox = new RichTextBox();
					textBox.Dock = DockStyle.Fill;
					textBox.DetectUrls = false;
					KForm form = new KForm();
					form.Controls.Add(textBox);
					form.Show();
				}

				return textBox;
			}
		}
		RichTextBox textBox;

		#endregion
	}
}
