using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class PlaceOfUseOrProcessingColumnStyle : ZCodeFindBoxColumnStyle
	{
		public PlaceOfUseOrProcessingColumnStyle(PlaceOfUseOrProcessingColumnStyleInfo columnInfo)
			: base(() => new PlaceOfUseOrProcessingGridFindBox(), columnInfo)
		{
		}

		protected override void PrepareControlData(CurrencyManager source, int rowNum)
		{
			var businessObject = source.Position == -1 ? null : source.GetCurrent();
			if (businessObject != null)
			{
				var findBox = (PlaceOfUseOrProcessingGridFindBox)EditControl;
				findBox.PlaceOfUseOrProcessing = (PlaceOfUseOrProcessing)businessObject;
			}
		}

		protected override bool EditControlShownForReadOnlyCore => true;

		internal void CommitEditingRow()
		{
			if (parentZGrid.ListManager is CurrencyManager currencyManager)
			{
				currencyManager.EndCurrentEdit();
				parentZGrid.BeginEdit(this, LastFocusedCell.RowNumber);
				currencyManager.Refresh();
			}
		}
	}
}
