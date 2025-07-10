using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CSupportingDocumentProvider : CC043CDocumentProvider
	{
		public CC043CSupportingDocumentProvider(SupportingDocumentType02 supportingDocument) : base(supportingDocument)
		{
			supportingDocumentType = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		}

		readonly SupportingDocumentType02 supportingDocumentType;

		public ZString ComplementOfInformation => supportingDocumentType.ComplementOfInformation ?? ZString.Empty;
	}
}
