using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class AttachmentWrapper : IAttachment
	{
		AttachmentWrapper(SupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}

		public static IAttachment NewOrNull(SupportingDocument supportingDocument)
			=> supportingDocument == null
			? null
			: new AttachmentWrapper(supportingDocument);

		byte[] IAttachment.Content => supportingDocument.Document?.ImageData;

		string IAttachment.FileName => supportingDocument.Document?.FileName;

		string IAttachment.DocumentType => supportingDocument.CSI_Code;

		string IAttachment.Keywords => null;

		string IAttachment.AttachmentId => null;

		string IAttachment.ExternalAttachmentId => supportingDocument.PK.ToString();

		string IAttachment.Remark => null;

		string IAttachment.IsAttachment => CustomsBoolean.True;

		Collection<IAttachmentAdditionalData> IAttachment.AdditionalData
			=> new Collection<IAttachmentAdditionalData>(
				supportingDocument.SupportingDocumentMetadataItems
				.Select(metaData => AttachmentAdditionalDataWrapper.NewOrNull(metaData))
				.WhereNotNull()
				.ToList());

		readonly SupportingDocument supportingDocument;
	}
}
