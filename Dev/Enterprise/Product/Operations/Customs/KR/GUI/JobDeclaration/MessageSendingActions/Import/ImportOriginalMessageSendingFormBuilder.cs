using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportOriginalMessageSendingFormBuilder : MessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles() => GetEntryNumberColumnStyle();

		public override ZUserControl GetUserControl() => null;
		public override ResourceStringData GetUserControlGroupBoxCaption() => null;
		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid) => null;
	}
}
