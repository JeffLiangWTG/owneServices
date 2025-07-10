using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class SupportingDocumentCleanUpStrategy : ICleanUpStrategy
{
	public SupportingDocumentCleanUpStrategy(SupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		declaration = Argument.NotNull(supportingDocument.Declaration, nameof(supportingDocument.Declaration));
	}

	readonly SupportingDocument supportingDocument;
	readonly JobDeclaration declaration;

	void ICleanUpStrategy.CleanUp()
	{
		supportingDocument.CSI_Value = ZDecimal.Zero;
		supportingDocument.CSI_ReferenceNumber2 = ZString.Empty;
		supportingDocument.CSI_RX_NKCurrency = ZString.Empty;
		supportingDocument.CSI_DateOfExpiry = ZDate.Empty;

		if (!declaration.IsExport)
		{
			supportingDocument.CSI_Status = ZString.Empty;
		}

		if (!declaration.IsUCC6AndIsExport)
		{
			supportingDocument.CSI_LineNo = ZInt.Zero;
		}
	}
}
