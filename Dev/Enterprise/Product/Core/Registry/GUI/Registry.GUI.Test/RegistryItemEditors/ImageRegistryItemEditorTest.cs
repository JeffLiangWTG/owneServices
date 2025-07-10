using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ImageRegistryItemEditor))]
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class ImageRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestClearButtonClickSetsHasChangesToTrue()
		{
			using (var registryForm = GetNewRegistryFormForTest(RegistryItem))
			using (var image = new Bitmap(1, 1))
			{
				registryForm.Show();
				registryForm.DisplayRegistryItem();
				registryForm.OverrideCheckBox.Checked = true;
				registryForm.OnSaveResult = DialogResult.Yes;
				registryForm.SaveButton.PerformClick();

				var imageControl = registryForm.PluginControl as ImageSelectionControl;
				var clearButton = (ZButton)imageControl.Controls.Find("ClearImageButton", searchAllChildren: true)[0];
				imageControl.Image = image;

				Application.DoEvents();

				Assert("PRE: HasChanges is false", !registryForm.HasChanges);

				clearButton.PerformClick();
				Application.DoEvents();

				Assert("Form HasChanges should be set to true when clear button clicked", registryForm.HasChanges);
			}
		}

		public void TestViewModeButton()
		{
			using (var registryForm = GetNewRegistryFormForTest(RegistryItem))
			{
				registryForm.Show();
				registryForm.DisplayRegistryItem();
				registryForm.OverrideCheckBox.Checked = true;
				registryForm.OnSaveResult = DialogResult.Yes;
				registryForm.SaveButton.PerformClick();

				var imageControl = registryForm.PluginControl as ImageSelectionControl;
				var viewModeButton = (ZButton)imageControl.Controls.Find("ViewModeButton", searchAllChildren: true)[0];
				var fPictureBox = (PictureBox)imageControl.Controls.Find("fPictureBox", searchAllChildren: true)[0];

				AssertEquals("Precondition: Default Value should be Stretch", PictureBoxSizeMode.StretchImage, fPictureBox.SizeMode);
				AssertContains("Precondition: Default caption should be Stretch View", "Stretch View", viewModeButton.CaptionResourceString.Caption);
				AssertContains("Precondition: Default tooltip should contain Stretch mode", "Showing Stretch mode", viewModeButton.ToolTipCaption.GetUnresolvedString());

				viewModeButton.PerformClick();
				Application.DoEvents();

				AssertEquals("PictureBoxSizeMOde should be Zoom", PictureBoxSizeMode.Zoom, fPictureBox.SizeMode);
				AssertContains("Caption should be Zoom", "Zoom View", viewModeButton.CaptionResourceString.Caption);
				AssertContains("Tooltip should contain Zoom mode", "Showing Zoom mode", viewModeButton.ToolTipCaption.GetUnresolvedString());

				viewModeButton.PerformClick();
				Application.DoEvents();

				AssertEquals("PictureBoxSizeMOde should be Zoom", PictureBoxSizeMode.StretchImage, fPictureBox.SizeMode);
				AssertContains("Caption should be Stretch View", "Stretch View", viewModeButton.CaptionResourceString.Caption);
				AssertContains("Tooltip should contain Stretch mode", "Showing Stretch mode", viewModeButton.ToolTipCaption.GetUnresolvedString());
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ImageRegistryItemEditor(null);
		}

		protected override bool CanNotHaveReferenceEquality
		{
			get { return false; } // When we change the image value we change the reference, not just the properties
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ImageSelectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ImageRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Image.FromFile(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif") };
		}

		protected override void AssertGetValueTypeIsRegistryValueType(object getValue)
		{
			Assert("GetValueFromEditorPane() should be an Image", getValue is Image);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
