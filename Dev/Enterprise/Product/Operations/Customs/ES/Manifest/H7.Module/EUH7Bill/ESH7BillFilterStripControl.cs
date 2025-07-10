using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class ESH7BillFilterStripControl : EUH7BillFilterStripControl
	{
		public ESH7BillFilterStripControl(IBusinessObjectCollection gridCollection, ESH7BillFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		protected override IEnumerable<ZGridColumnInfo> GetReferenceNumberColumns()
		{
			var g3LrnTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			g3LrnTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.G3LocalReferenceNumber;
			g3LrnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return g3LrnTextBoxColumnStyleInfo;

			var h7MrnTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			h7MrnTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.H7MovementReferenceNumber;
			h7MrnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return h7MrnTextBoxColumnStyleInfo;

			var g3MrnTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			g3MrnTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.G3MovementReferenceNumber;
			g3MrnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return g3MrnTextBoxColumnStyleInfo;
		}
	}
}
