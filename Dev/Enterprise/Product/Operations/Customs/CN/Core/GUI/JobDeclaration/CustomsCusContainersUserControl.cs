using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CustomsCusContainersUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public CustomsCusContainersUserControl()
		{
			InitializeComponent();
			AddColumnsForGrid();
		}

		void AddColumnsForGrid()
		{
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "ContainerCode",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				GroupName = Res.GetData("ECF02DB7-E01B-4788-AA95-F1AB21800499", "CN Container Code")
			};

			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				ColumnName = "ContainerCodeDescription",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				GroupName = Res.GetData("ECF02DB7-E01B-4788-AA95-F1AB21800499", "CN Container Code")
			};

			this.CusContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CusContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			CusContainersBoundGrid.InnerGrid.ReOrderColumns(ColumnNamesInSortOrder);
			CusContainersBoundGrid.InnerGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
			CusContainersBoundGrid.InnerGrid.SetColumnVisible(true, DefaultColumnsForGrid);
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (fcolumnNamesInSortOrder == null)
				{
					var columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);
					columns.Add(CusContainer.Schema.CO_Weight);
					columns.Add(CusContainer.Schema.CO_WeightUQ);
					fcolumnNamesInSortOrder = columns.ToArray();
				}
				return fcolumnNamesInSortOrder.ToArray();
			}
		}
		string[] fcolumnNamesInSortOrder;

		string[] DefaultColumnsForGrid => fdefaultColumnsForGrid ?? (fdefaultColumnsForGrid = new[]
		{
			CusContainer.Schema.CO_ContainerNumber,
			CusContainer.Schema.CO_RC,
			CusContainer.Schema.ContainerCode,
			CusContainer.Schema.ContainerCodeDescription,
			CusContainer.Schema.CO_FCL_LCL_AIR,
			CusContainer.Schema.CO_Seal,
			CusContainer.Schema.CO_SecondSeal
		});
		string[] fdefaultColumnsForGrid;
	}
}
