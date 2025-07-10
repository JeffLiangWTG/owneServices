using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public abstract class ApplicationGUIProviderBase : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new ManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new BillLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new ItineraryForManifestHeaderUserControl();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var specialMentionsDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			specialMentionsDropEditColumnStyleInfo.ColumnName = Enterprise.Customs.EU.Manifest.Business.AsycudaBill.Schema.SpecialMentions;
			specialMentionsDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			specialMentionsDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			specialMentionsDropEditColumnStyleInfo.IsVisible = false;
			yield return specialMentionsDropEditColumnStyleInfo;
		}
	}
}
