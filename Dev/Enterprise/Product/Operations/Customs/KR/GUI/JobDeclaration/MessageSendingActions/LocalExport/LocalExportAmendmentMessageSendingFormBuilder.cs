using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.GUI
{
	public class LocalExportAmendmentMessageSendingFormBuilder : MessageSendingFormBuilder
	{
		public MessageFunctionCode MessageFunctionCode { get; }

		public LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode messageFunctionCode) : base()
		{
			MessageFunctionCode = messageFunctionCode;
		}

		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.CustomsReceiptNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.ReasonCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsMandatory = true,
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentReason,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				}
			});
			return result.ToArray();
		}

		public override ZUserControl GetUserControl() => MessageFunctionCode == MessageFunctionCode.Amendment ? new AmendedItemsUserControl() : null;

		public override ResourceStringData GetUserControlGroupBoxCaption() => MessageFunctionCode == MessageFunctionCode.Amendment ? Res.GetData("9e83176a-0182-4c33-b465-5e81f3bad726", "Amended Items") : null;
		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid) => null;
		public override int[] GetFormSize() => MessageFunctionCode == MessageFunctionCode.Amendment ? new int[] { 880, 550 } : new int[] { 800, 460 };
		public override int Panel1MinSize => MessageFunctionCode == MessageFunctionCode.Amendment ? Panel1MinSizeForGridUserControl : base.Panel1MinSize;
	}
}
