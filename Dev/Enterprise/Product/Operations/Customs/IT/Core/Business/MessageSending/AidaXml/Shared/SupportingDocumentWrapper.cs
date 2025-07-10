using System;
using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public class SupportingDocumentWrapper : CustomsMessageBuilder.ISupportingDocument
{
	public SupportingDocumentWrapper(SupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));

		lazyExpiryDate = new Lazy<DateTime?>(GetExpiryDate);
		lazyReferenceNumber = new Lazy<string>(GetReferenceNumber);
		lazyItemNumber = new Lazy<int?>(() => supportingDocument.CSI_LineNo.NullIfZero());
	}

	readonly SupportingDocument supportingDocument;

	decimal? CustomsMessageBuilder.ISupportingDocument.Amount => supportingDocument.CSI_Value.NullIfZero();

	string CustomsMessageBuilder.ISupportingDocument.Code => supportingDocument.CSI_Code;

	string CustomsMessageBuilder.ISupportingDocument.Currency => supportingDocument.CSI_RX_NKCurrency;

	DateTime? CustomsMessageBuilder.ISupportingDocument.ExpiryDate => lazyExpiryDate.Value;
	readonly Lazy<DateTime?> lazyExpiryDate;

	string CustomsMessageBuilder.ISupportingDocument.IssuingAuthority => supportingDocument.CSI_ReferenceNumber2;

	decimal? CustomsMessageBuilder.ISupportingDocument.Quantity => supportingDocument.CSI_Quantity.NullIfZero();

	string CustomsMessageBuilder.ISupportingDocument.ReferenceNumber => lazyReferenceNumber.Value;
	readonly Lazy<string> lazyReferenceNumber;

	string CustomsMessageBuilder.ISupportingDocument.UnitOfQuantity => supportingDocument.CSI_UnitOfQuantity;

	int? CustomsMessageBuilder.ISupportingDocument.ItemNumber => lazyItemNumber.Value;
	readonly Lazy<int?> lazyItemNumber;

	#region Implementation

	DateTime? GetExpiryDate()
	{
		var dateOfExpiry = supportingDocument.CSI_DateOfExpiry;

		return !dateOfExpiry.IsEmpty
			? dateOfExpiry.ToDateTime()
			: null;
	}

	string GetReferenceNumber() => supportingDocument.GetReferenceNumberWithYearOfIssueAndCountry();

	#endregion
}
