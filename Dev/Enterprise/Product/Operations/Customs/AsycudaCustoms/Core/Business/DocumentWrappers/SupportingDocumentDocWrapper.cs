using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers
{
	public class SupportingDocumentDocWrapper : NonPersistentBusinessObject
	{
		public SupportingDocumentDocWrapper(SupportingDocument supportingDocument)
		{
			if (supportingDocument != null)
			{
				Code = supportingDocument.CSI_Code;
				Reference = supportingDocument.CSI_ReferenceNumber;
				Comments = supportingDocument.CSI_AdditionalDescription;
			}
		}

		public ZString Code { get; }

		public ZString Reference { get; }

		public ZString Comments { get; }
	}
}
