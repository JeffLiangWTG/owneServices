using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class SupportingDocumentsProvider : ISupportingDocument
{
	public SupportingDocumentsProvider(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument document, int sequence)
	{
		this.document = Argument.NotNull(document, nameof(document));
		SequenceNumber = sequence;
	}

	readonly EU.Business.Declaration.MultiLineAddInfos.SupportingDocument document;

	public int SequenceNumber { get; }

	public string Type => document.CSI_Code;

	public string ReferenceNumber => document.CSI_ReferenceNumber;

	public string ComplementOfInformation => string.Empty;

	public int DocumentLineItemNumber => document.CSI_ItemNumber;

	public string IssuingAuthorityName => document.CSI_AdditionalDescription;

	public DateTime? ValidityDate => document.CSI_DateOfExpiry.IsValid ? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(document.CSI_DateOfExpiry, removeMillisecond: true) : null;

	public decimal Amount => document.CSI_Value;

	public string Currency => document.CSI_RX_NKCurrency;

	public string MeasurementUnitAndQualifier => document.CSI_UnitOfQuantity;

	public decimal? Quantity => document.CSI_Quantity;
}
