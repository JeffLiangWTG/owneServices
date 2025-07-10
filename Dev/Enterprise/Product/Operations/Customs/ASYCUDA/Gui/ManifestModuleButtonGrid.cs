using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class ManifestModuleButtonGrid : ZModuleButtonGrid
	{
		public ManifestModuleButtonGrid()
		{
		}

		public void HideButtonRow()
		{
			if (mainLayoutPanel.RowStyles.Count > 1)
			{
				mainLayoutPanel.RowStyles[1].Height = 0F;
			}
		}

		public new void ShowEditForm(BusinessObject objectToEdit) => base.ShowEditForm(objectToEdit);
	}
}
