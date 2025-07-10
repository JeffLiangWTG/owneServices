using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.Module
{
	public partial class EUH7BillFilterStripControl : ZFilterStripControl
	{
		public EUH7BillFilterStripControl(IBusinessObjectCollection gridCollection, EUH7BillFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			CustomizeGrid();
		}

		protected virtual void CustomizeGrid()
		{
			var billNumberColumnStyleInfo = grid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == AsycudaBill.Schema.ABL_BillNumber);
			var billNumberColumnIndex = grid.ColumnStyles.IndexOf(billNumberColumnStyleInfo);

			var referenceNumberColumns = GetReferenceNumberColumns().ToArray();

			if (billNumberColumnIndex < 0)
			{
				grid.ColumnStyles.AddRange(referenceNumberColumns);
			}
			else
			{
				grid.ColumnStyles.InsertRange(billNumberColumnIndex + 1, referenceNumberColumns);
			}
		}

		protected virtual IEnumerable<ZGridColumnInfo> GetReferenceNumberColumns()
		{
			var mrnTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			mrnTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.MovementReferenceNumber;
			mrnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return mrnTextBoxColumnStyleInfo;

			var lrnTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			lrnTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.LocalReferenceNumber;
			lrnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return lrnTextBoxColumnStyleInfo;
		}
	}
}
