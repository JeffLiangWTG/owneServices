using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CN.Business
{
	public class AttachmentInvoiceLineLink : NonPersistentBusinessObject
	{
		public AttachmentInvoiceLineLink(EntryInstructionAttachment attachment, JobComInvoiceLine invoiceLine)
		{
			Attachment = Argument.NotNull(attachment, nameof(attachment));
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			CusStorageDocPivot = Argument.NotNull(attachment.CusStorageDocPivot, nameof(attachment.CusStorageDocPivot));
		}

		internal EntryInstructionAttachment Attachment { get; }
		internal JobComInvoiceLine InvoiceLine { get; }
		internal CusStorageDocPivot CusStorageDocPivot { get; }

		[ResourceStringData("Enterprise.Customs.CN.Business.AttachmentInvoiceLineLink|AttachmentType", Caption = "Attachment Type")]
		public ZString AttachmentType => Attachment.AttachmentType;

		public ZPropertyInfo AttachmentTypeInfo => GetZPropertyInfo(nameof(AttachmentType));

		[ResourceStringData("Enterprise.Customs.CN.Business.AttachmentInvoiceLineLink|AttachmentDescription", Caption = "Attachment Description")]
		public ZString AttachmentDescription => Attachment.AttachmentTypeDescription;

		public ZPropertyInfo AttachmentDescriptionInfo => GetZPropertyInfo(nameof(AttachmentDescription));

		[List(nameof(EDocList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.AttachmentInvoiceLineLink|EDoc", Caption = "eDoc")]
		public ZGuid EDoc => Attachment.EDoc;

		public ZPropertyInfo EDocInfo => GetZPropertyInfo(nameof(EDoc));

		public ICodeDescriptionPairList EDocList => Attachment.Lookups.EDocList;

		public AttachmentInvoiceLineGenPivot Pivot => CusStorageDocPivot.InvoiceLineLinks.GetRelatedPivot(InvoiceLine);

		[ResourceStringData("Enterprise.Customs.CN.Business.AttachmentInvoiceLineLink|IsLinked", Caption = "Is For Invoice Line?")]
		public ZBool IsLinked
		{
			get => Pivot != null;
			set
			{
				if (IsLinked != value)
				{
					if (value)
					{
						CusStorageDocPivot.InvoiceLineLinks.AddPivotFor(InvoiceLine);
					}
					else
					{
						CusStorageDocPivot.InvoiceLineLinks.DeletePivotFor(InvoiceLine);
					}
				}
				IsLinkedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsLinkedInfo => GetZPropertyInfo(nameof(IsLinked));
	}
}
