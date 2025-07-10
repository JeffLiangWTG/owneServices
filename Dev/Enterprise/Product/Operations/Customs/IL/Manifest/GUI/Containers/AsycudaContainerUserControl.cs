using System.Linq;
using Enterprise.Core.Forms;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaContainerUserControl : ASYCUDA.GUI.AsycudaContainerUserControl
	{
		public AsycudaContainerUserControl()
		{
			var seal1UnloadingStateColumnInfo = this.containersGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "ACN_Seal1UnloadingState");
			var seal2UnloadingStateColumnInfo = this.containersGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "ACN_Seal2UnloadingState");
			var seal3UnloadingStateColumnInfo = this.containersGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "ACN_Seal3UnloadingState");
			this.containersGrid.ColumnStyles.Remove(seal1UnloadingStateColumnInfo);
			this.containersGrid.ColumnStyles.Remove(seal2UnloadingStateColumnInfo);
			this.containersGrid.ColumnStyles.Remove(seal3UnloadingStateColumnInfo);

			InitializeComponent();
		}
	}
}
