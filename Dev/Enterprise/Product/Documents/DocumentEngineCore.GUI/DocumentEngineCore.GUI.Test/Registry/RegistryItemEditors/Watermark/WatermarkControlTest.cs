using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(WatermarkControl))]
	sealed class WatermarkControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public void FileDialogFilter()
		{
			using (WatermarkControl control = (WatermarkControl)GetNewControl())
			{
				AssertEquals("ImageWatermarkImageSelectionControl.FileDialogFilter", "Image Files (*.PNG)|*.PNG", control.ImageWatermarkImageSelectionControl.FileDialogFilter);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new Watermark();
		}

		protected override void AssertAdditionalObjectsAreReadOnly(RegistryZUserControl control, IBusiness businessEntity, bool readOnly)
		{
			base.AssertAdditionalObjectsAreReadOnly(control, businessEntity, readOnly);
			AssertEquals("ImageWatermarkImageSelectionControl.ReadOnly", readOnly, ((WatermarkControl)control).ImageWatermarkImageSelectionControl.ReadOnly);
		}

		#endregion
	}
}
