using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using EUBusinessDeclaration = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusEntryLineValidation : EUBusinessDeclaration.CusEntryLineValidation
{
	public CusEntryLineValidation(EUBusinessDeclaration.CusEntryLine parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		var errorMessage = ValidationCaptions.CusEntryLine.TheMaximumCumulativeNumberOfSupportingTransportAndAdditionalReferenceExceeded;
		Parent.ClearRowNotificationsContaining(errorMessage);

		base.ValidateAll();

		CheckSupportingAndAdditionalDocumentsCountValidateRule1407();
	}

	void CheckSupportingAndAdditionalDocumentsCountValidateRule1407()
	{
		var declaration = Parent.Declaration;

		if (declaration.IsUCC6AndIsExport && declaration.IsTransitionPeriodAES30)
		{
			if (GetTotalNumberOfDocuments() > OverallMaxNumberOfDocuments)
			{
				Parent.AddRowMessageError(ValidationCaptions.CusEntryLine.TheMaximumCumulativeNumberOfSupportingTransportAndAdditionalReferenceExceeded);
			}
		}
	}

	#region Implementation

	bool IsSubTypeAdditionalReferenceOrTransportDocument(EUBusinessDeclaration.MultiLineAddInfos.AdditionalInfo additionalInfo)
		=> additionalInfo.IsAnAdditionalReference
		|| additionalInfo.IsATransportDocument;

	int GetNumberOfSupportingDocumentsIn(IEnumerable<ISupportingDocumentsProvider> providers) => providers.SelectMany(x => x.SupportingDocuments).Count();

	int GetNumberOfAdditionalInfosIn(IEnumerable<IAdditionalInfosProvider> providers) => providers.SelectMany(x => x.AdditionalInfos.Where(a => IsSubTypeAdditionalReferenceOrTransportDocument(a))).Count();

	int GetTotalNumberOfDocuments()
	{
		var invoiceLines = Parent.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
		var invoiceHeaders = invoiceLines.GetDistinctInvoiceHeaders();

		return GetNumberOfSupportingDocumentsIn(invoiceLines) + GetNumberOfAdditionalInfosIn(invoiceLines) + GetNumberOfSupportingDocumentsIn(invoiceHeaders) + GetNumberOfAdditionalInfosIn(invoiceHeaders);
	}

	const int OverallMaxNumberOfDocuments = 99;

	#endregion
}
