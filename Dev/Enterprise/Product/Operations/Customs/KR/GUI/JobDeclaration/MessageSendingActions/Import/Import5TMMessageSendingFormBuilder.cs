using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5TMMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		public override ZUserControl GetUserControl() => new GoldVATDeclarationEntryLinesUserControl();

		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("13CA9BE2-6C23-4306-96FC-9B01003202D4", "Entry Lines");

		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = base.GetAdditionalColumnStyles().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObject.Schema.PayerBusinessNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				}
			});
			return result.ToArray();
		}
		public override int[] GetFormSize() => new int[] { 900, 550 };
		public override int Panel1MinSize => Panel1MinSizeForGridUserControl;
	}
}
