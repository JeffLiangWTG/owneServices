using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class AttachmentAdditionalDataWrapper : IAttachmentAdditionalData
	{
		AttachmentAdditionalDataWrapper(SupportingDocumentMetaData metaData)
		{
			this.metaData = metaData;
		}

		public static IAttachmentAdditionalData NewOrNull(SupportingDocumentMetaData metaData)
			=> metaData == null || metaData.CY_Data.IsEmpty ? null : new AttachmentAdditionalDataWrapper(metaData);

		int IAttachmentAdditionalData.FieldId => int.TryParse(metaData.CY_Code, out var res)
			? res
			: ZInt.Zero;

		string IAttachmentAdditionalData.FieldData => metaData.CY_Data;

		readonly SupportingDocumentMetaData metaData;
	}
}
