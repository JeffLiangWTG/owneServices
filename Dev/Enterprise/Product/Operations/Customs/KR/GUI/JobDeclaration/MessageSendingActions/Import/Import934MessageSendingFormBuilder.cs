using System.Linq;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import934MessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = base.GetAdditionalColumnStyles().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(ValuationDeclarationMessageSendingObject.ValuationCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
				}
			});
			return result.ToArray();
		}

		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid)
		{
			ZTabPage[] result = null;
			var importValuationDeclarationUserControl = new ImportValuationDeclarationUserControl(messageSendingObjectsGrid);
			if (importValuationDeclarationUserControl != null)
			{
				importValuationDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				var tabPage = new ZTabPage();
				tabPage.CaptionResourceString = Res.GetData("31A166D0-571B-492C-B933-1411555B24DF", "Valuation Declaration");
				tabPage.Dock = System.Windows.Forms.DockStyle.Fill;
				tabPage.Controls.Add(importValuationDeclarationUserControl);

				result = new ZTabPage[] { tabPage };
			}

			return result;
		}

		public override int[] GetFormSize() => new int[] { 1240, 825 };
		public override int Panel1MinSize => Panel1MinSizeForNoUserControl;
		public override int Panel2MinSize => 378;
	}
}
