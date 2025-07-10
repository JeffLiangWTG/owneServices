using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportAmendmentMessageSendingFormBuilder : MessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>(GetEntryNumberColumnStyle());
			result.AddRange(GetCommonAmendmentColumnStyleInfo());

			return result.ToArray();
		}

		public override ZUserControl GetUserControl() => new AmendedItemsUserControl();
		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("9e83176a-0182-4c33-b465-5e81f3bad726", "Amended Items");
		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid) => null;

		public override int[] GetFormSize() => new int[] { 880, 550 };
		public override int Panel1MinSize => Panel1MinSizeForGridUserControl;
	}
}
