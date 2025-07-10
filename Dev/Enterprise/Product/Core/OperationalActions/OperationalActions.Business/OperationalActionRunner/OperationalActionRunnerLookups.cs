
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionRunnerLookups : ZLookups
	{
		public OperationalActionRunnerLookups(OperationalActionRunner parent)
			: base(parent) { }

		#region Printer_List

		public CodeDescriptionPairList Printer_List
		{
			get { return printer_List ?? (printer_List = new DocDeliveryPrintDetails(Factory).PrinterNames); }
		}
		CodeDescriptionPairList printer_List;

		#endregion

		#region BulkDeliveryMethod_List

		public BulkDeliveryMethodList BulkDeliveryMethod_List
		{
			get { return bulkDeliveryMethod_List ?? (bulkDeliveryMethod_List = new BulkDeliveryMethodList()); }
		}
		BulkDeliveryMethodList bulkDeliveryMethod_List;

		#endregion

		#region AttachmentOptions_List

		public CodeDescriptionPairList AttachmentOptions_List
		{
			get
			{
				if (attachmentOptions_List == null)
				{
					attachmentOptions_List = new CodeDescriptionPairList();
					attachmentOptions_List.AddPair(AttachmentOptionsCodes.SingleAttachment, ResString.GetMultilingualString("89C7898D-58B5-45D1-BA17-2265AB07050D", "Single Attachment"));
					attachmentOptions_List.AddPair(AttachmentOptionsCodes.MultipleAttachments, ResString.GetMultilingualString("55B6039C-FEFA-4A51-A5B5-46F500FD834C", "Multiple Attachments"));
				}
				return attachmentOptions_List;
			}
		}
		CodeDescriptionPairList attachmentOptions_List;

		internal static class AttachmentOptionsCodes
		{
			public const string SingleAttachment = "SAT";
			public const string MultipleAttachments = "MAT";
		}

		#endregion

		#region Implementation

		public new OperationalActionRunner Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalActionRunner)base.Parent; }
		}

		#endregion
	}
}
