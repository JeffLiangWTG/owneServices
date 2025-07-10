using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class SupportingDocumentWrapper : ISupportingDocument
{
	public SupportingDocumentWrapper(NctsSupportingDocument supportingDocument)
	{
		Argument.NotNull(supportingDocument, nameof(supportingDocument));
		InitializeLazy(supportingDocument);
	}

	void InitializeLazy(NctsSupportingDocument supportingDocument)
	{
		lazyDocumentType = new Lazy<string>(() => supportingDocument.CSI_Code);
		lazyReferenceNumber = new Lazy<string>(() => GetReferenceNumber(supportingDocument));
		lazyItemNumber = new Lazy<int?>(() => supportingDocument.CSI_ItemNumber);
		lazyComplementOfInformation = new Lazy<string>(() => supportingDocument.CSI_ReferenceNumber2);
	}

	string ISupportingDocument.DocumentType => lazyDocumentType.Value;
	Lazy<string> lazyDocumentType;

	string ISupportingDocument.ReferenceNumber => lazyReferenceNumber.Value;
	Lazy<string> lazyReferenceNumber;

	int? ISupportingDocument.ItemNumber => lazyItemNumber.Value;
	Lazy<int?> lazyItemNumber;

	string ISupportingDocument.ComplementOfInformation => lazyComplementOfInformation.Value;
	Lazy<string> lazyComplementOfInformation;

	string GetReferenceNumber(NctsSupportingDocument supportingDocument)
	{
		var data = new ZStringBuilder()
			.AppendIfNotEmpty(supportingDocument.CSI_YearOfIssue)
			.AppendIfNotEmpty(supportingDocument.CSI_RN_NKCountryCode)
			.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber)
			.ToStringWithDelimiterBetweenAppends("-");

		return data.IsNullOrEmpty()
			? "-"
			: data;
	}
}
