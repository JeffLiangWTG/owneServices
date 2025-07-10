using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Phase5GoodsItemAdditionalDocumentsTabUserControl : EU.NCTS.GUI.Phase5GoodsItemAdditionalDocumentsTabUserControl
	{
		protected override IPanelLayoutWithGridProvider ArrivalOrDepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid(INctsPhase5LayoutProvider provider)
		{
			var header = (NctsHeader)DataSource;
			return header.ESNctsHeader.CEN_TNNArrival ? provider.DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid : base.ArrivalOrDepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid(provider);
		}
	}
}
