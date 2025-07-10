using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(SalesTradeLaneChecklistUserControl))]
	sealed class SalesTradeLaneChecklistUserControlBaseTest : RuntimeOptionUserControlBaseTest<SalesTradeLaneChecklistUserControl>
	{
		public override void TestChangeLabelSizeForAlignment()
		{
			using (var box = new SalesTradeLaneChecklistUserControl())
			{
				box.Width = 500;
				var label = box.Controls.Find("fieldLabel", true).Single();
				box.ChangeLabelSizeForAlignment(200);
				AssertEquals(200 - label.Left, label.Width);
			}
		}

		public override void TestDesiredCaptionWidth()
		{
			using (var box = new SalesTradeLaneChecklistUserControl())
			{
				box.Width = 500;
				var label = box.Controls.Find("fieldLabel", true).Single();
				AssertEquals(box.Controls.Find("fieldLabelPanel", false).Single().Width - label.Padding.Vertical, label.Width);
			}
		}

		public void TestTranslation()
		{
			var field = new SalesTradeLaneChecklistField(Factory);
			field.FieldName = "MyField";
			field.ModeField = "MyModeField";
			field.TypeField = "MyTypeField";
			field.DisplayName = "Business Value";
			field.DisplayNameLocalizedData = new ResourceStringData("Business Value", "Geschäftswert");

			using (var control = new SalesTradeLaneChecklistUserControl())
			using (var form = new ZForm())
			{
				control.SetFilter(field);
				form.Controls.Add(control);
				form.Show();

				var label = control.Controls.Find("fieldLabel", true).Single();
				AssertEquals("Geschäftswert", label.Text);
			}
		}
	}
}
