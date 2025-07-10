using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.GUI
{
	public partial class CustomsCusContainersWithTrackingAndAdditionalSealUserControl : Customs.GUI.BaseCustomsCusContainersWithTrackingUserControl
	{
		public CustomsCusContainersWithTrackingAndAdditionalSealUserControl()
		{
			InitializeComponent();
			SetUpCusContainerBoundGrid();
		}

		void SetUpCusContainerBoundGrid()
		{
			CusContainersBoundGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().FirstOrDefault(style => style.ColumnName == CusContainerSchema.Constants.CO_SecondSeal).IsVisible = true;

			var containerColumnStyleInfo = CusContainersBoundGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().FirstOrDefault(style => style.ColumnName == CusContainerSchema.Constants.CO_RC);
			containerColumnStyleInfo.GroupName = Res.GetData("1AE32714-337C-46A7-A87E-BE78FEEFAE8D", "Container");
			var indexOfContainerColumnStyleInfo = CusContainersBoundGrid.ColumnStyles.IndexOf(containerColumnStyleInfo);

			var containerTypeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			containerTypeColumnStyleInfo.ColumnName = "ContainerType";
			containerTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			containerTypeColumnStyleInfo.GroupName = Res.GetData("1AE32714-337C-46A7-A87E-BE78FEEFAE8D", "Container");

			var containerSizeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			containerSizeColumnStyleInfo.ColumnName = "ContainerSize";
			containerSizeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			containerSizeColumnStyleInfo.GroupName = Res.GetData("1AE32714-337C-46A7-A87E-BE78FEEFAE8D", "Container");

			CusContainersBoundGrid.ColumnStyles.Insert(indexOfContainerColumnStyleInfo + 1, containerSizeColumnStyleInfo);
			CusContainersBoundGrid.ColumnStyles.Insert(indexOfContainerColumnStyleInfo + 2, containerTypeColumnStyleInfo);
		}
	}
}
