using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMSystemSchematicForm))]
	public class BMSystemSchematicFormTest : ZFormBasherTest
	{
		[UseSnapshotProtection]
		public void TestSchematicImageForm_ShouldShowMessageWhenSchematicImageOversized()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 50); // A schematic form currently cannot be generated if the associated system has no components.
			Factory.Save();

			using (var form = new BMSystemSchematicForm(bms, SchematicDrawMode.Graphical))
			{
				form.Show();

				var schematicPictureBox = (KPictureBox)(form.Controls.Find("schematicPictureBox", true)[0]);
				var labelInvalidSchematicPictureBoxImage = (ZLabel)(form.Controls.Find("labelInvalidSchematicPictureBoxImage", true)[0]);
				var refreshButton = (ZButton)(form.Controls.Find("RefreshButton", true)[0]);

				AssertNotNull("GIVEN the BMS has appropriate schematic width and height dimensions, WHEN a schematic is requested for generation, THEN schematicPictureBox holds an image",
											schematicPictureBox.Image);
				Assert("GIVEN the BMS has appropriate schematic width and height dimensions, WHEN a schematic is requested for generation AND schematicPictureBox holds an image, THEN labelInvalidSchematicPictureBoxImage is not shown",
								!labelInvalidSchematicPictureBoxImage.Visible);

				BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 100);
				Factory.Save();
				refreshButton.PerformClick();

				schematicPictureBox = (KPictureBox)(form.Controls.Find("schematicPictureBox", true)[0]);
				labelInvalidSchematicPictureBoxImage = (ZLabel)(form.Controls.Find("labelInvalidSchematicPictureBoxImage", true)[0]);

				AssertNull("GIVEN the BMS has oversized schematic width and height dimensions, WHEN a schematic is requested for generation, THEN schematicPictureBox shows no image",
										schematicPictureBox.Image);
				Assert("GIVEN the BMS has oversized schematic width and height dimensions, WHEN a schematic is requested for generation, THEN labelInvalidSchematicPictureBoxImage is shown",
								labelInvalidSchematicPictureBoxImage.Visible);
				AssertEquals("GIVEN the BMS has appropriate schematic width and height dimensions, WHEN a schematic is requested for generation, THEN labelInvalidSchematicPictureBoxImage holds the expected message",
							"There are too many components for the specified buffer management system to generate an image. Please view the Text Schematic, available in the Edit/View Buffer Management System window.", labelInvalidSchematicPictureBoxImage.Text);
			}

			using (var form = new BMSystemSchematicForm(bms, SchematicDrawMode.Text))
			{
				form.Show();

				AssertEquals("GIVEN the BMS has oversized schematic width and height dimensions, WHEN a text schematic is requested for generation, THEN schematicPictureBox does not hold an image",
											0, form.Controls.Find("schematicPictureBox", true).Length);
				var labelInvalidSchematicPictureBoxImage = (ZLabel)(form.Controls.Find("labelInvalidSchematicPictureBoxImage", true)[0]);
				var refreshButton = (ZButton)(form.Controls.Find("RefreshButton", true)[0]);

				Assert("GIVEN the BMS has oversized schematic width and height dimensions, WHEN a text schematic is requested for generation, THEN labelInvalidSchematicPictureBoxImage is not shown",
								!labelInvalidSchematicPictureBoxImage.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Comp 1";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Comp 2";

			var link1 = component1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			return new BMSystemSchematicForm(system, SchematicDrawMode.Graphical);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return base.ShouldIgnoreMissingBindingMember(control) || control.Name == "IncludeNonPrimaryPathCheckBox";
		}
	}
}
