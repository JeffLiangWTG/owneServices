using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class SupportingDocumentWrapper : ISupportingDocument
{
	public SupportingDocumentWrapper(SupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}

	readonly SupportingDocument supportingDocument;

	public decimal? Amount => null;

	public string Code => supportingDocument.CSI_Code;

	public string Currency => null;

	public DateTime? ExpiryDate => null;

	public string IssuingAuthority => null;

	public decimal? Quantity => null;

	public string ReferenceNumber => supportingDocument.CSI_ReferenceNumber;

	public string UnitOfQuantity => null;

	public int? ItemNumber => null;
}
