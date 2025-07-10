using System.Reflection;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolStripButtonTest : TestCase
	{
		public void TestIButton_Parent()
		{
			using (var form = new Form())
			using (var toolStrip = new ZToolStrip())
			using (var toolButton = new ZToolStripButton())
			{
				toolStrip.Items.Add(toolButton);
				form.Controls.Add(toolStrip);

				AssertEquals(form, ((IButton)toolButton).Parent);
			}

			using (var toolStrip = new ZToolStrip())
			using (var toolButton = new ZToolStripButton())
			{
				toolStrip.Items.Add(toolButton);
				AssertNull(((IButton)toolButton).Parent);
			}

			using (var toolButton = new ZToolStripButton())
			{
				AssertNull(((IButton)toolButton).Parent);
			}
		}

		public void TestIButton_ShouldSetImage()
		{
			using (var toolButton = new ZToolStripButton())
			{
				AssertEquals(true, ((IButton)toolButton).ShouldSetImage);
			}
		}

		public void TestIButton_Focus()
		{
			using (var form = new Form())
			using (var toolStrip = new ZToolStrip())
			using (var toolButton = new ZToolStripButton())
			{
				toolStrip.Items.Add(toolButton);
				form.Controls.Add(toolStrip);
				form.Show();

				AssertEquals(false, toolStrip.Focused);
				AssertEquals(false, toolButton.Selected);
				((IButton)toolButton).Focus();
				Application.DoEvents();

				AssertEquals(true, toolStrip.Focused);
				AssertEquals(true, toolButton.Selected);
			}
		}

		public void TestIButtonControl_DialogResult()
		{
			using (var toolButton = new ZToolStripButton())
			{
				AssertEquals(DialogResult.OK, ((IButtonControl)toolButton).DialogResult);
			}
		}

		public void TestTextReturnsCaptionWithKeyAccelerator()
		{
			using (var form = new Form())
			using (var toolStrip = new ZToolStrip())
			using (var toolButton = new ZToolStripButton())
			{
				toolStrip.Items.Add(toolButton);
				form.Controls.Add(toolStrip);

				var resString = new ResourceStringData("key", "S&ave && Close");
				toolButton.CaptionResourceString = resString;

				AssertEquals("S&ave && Close", toolButton.Text);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZToolStripButton.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZToolStripButton)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZToolStripButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}
}
