using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.DE.GUI
{
	public partial class REGLineToSplitUserControl : ZUserControl
	{
		public REGLineToSplitUserControl()
		{
			InitializeComponent();
		}

		void FormattedATBNumberCodeFindBoxWithSelectedEvent_SelectedChanged(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length > 0 && e.SelectedBusinessObjects[0] is CusTempStorageRegLine regLine)
			{
				var consolidatedLine = (CUSPCSConsolidatedCusTempStorageLine)CurrentDataItem;
				consolidatedLine.UpdatePropertiesFromRegLine(regLine);
			}
		}
	}
}
