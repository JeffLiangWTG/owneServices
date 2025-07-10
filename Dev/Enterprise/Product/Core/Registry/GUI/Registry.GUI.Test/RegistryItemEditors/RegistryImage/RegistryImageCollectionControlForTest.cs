using System.Drawing;
using System.Reflection;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryImageCollectionControlForTest : RegistryImageCollectionControl
	{
		public ZOpenFileDialog FileDialog
		{
			get
			{
				var fileDialogFieldInfo = base.RegistryImageSelectionControl.GetType()
					.GetField("FileDialog", BindingFlags.NonPublic | BindingFlags.Instance);

				return (ZOpenFileDialog)fileDialogFieldInfo.GetValue(RegistryImageSelectionControl);
			}
		}

		public Image RegistryImage => (Image)RegistryImageSelectionControl.ImageObjectForBinding;
	}
}
