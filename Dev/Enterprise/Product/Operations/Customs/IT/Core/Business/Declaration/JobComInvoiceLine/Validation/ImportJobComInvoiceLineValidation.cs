using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ImportJobComInvoiceLineValidation : CommonJobComInvoiceLineValidation
{
	public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateEntryLineRequiredSupportingDocuments();
	}

	protected override void CheckJI_ValuationCode()
	{
		base.CheckJI_ValuationCode();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ValuationCodeInfo);
	}

	protected override void CheckJI_Weight()
	{
		base.CheckJI_Weight();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_WeightInfo);
	}

	protected override void CheckJI_OA_ExporterAddress()
	{
		base.CheckJI_OA_ExporterAddress();

		var parent = Parent;
		var propertyInfo = parent.JI_OA_ExporterAddressInfo;

		if (parent.ExporterAddress != null && parent.Declaration != null)
		{
			var consignorCaption = Res.GetString("D1FD5FD8-1039-4D4B-A35B-CC0025F14FE8", "Consignor");

			new CustomsAddressValidator(parent.ExporterAddress, consignorCaption, parent.Declaration)
				.ValidateMaximumLengthCustomsFields(propertyInfo);
		}
	}

	protected override void CheckCustomsQuantityAndNetWeightEquality()
	{
		if (Parent.CustomsFirstQuantityInKG.Round(3) != Parent.NetWeightInKG)
		{
			Parent.JI_CustomsQuantityInfo.AddWarning(ValidationCaptions.InvoiceLine.CustomsQtyIsUsuallyEqualNetWeight);
		}
	}

	protected override void CheckJI_Tariff()
	{
		base.CheckJI_Tariff();

		CheckTariffTurkeyCustomsDutyExemption();
	}

	protected override void CheckJI_PrimaryPreference()
	{
		base.CheckJI_PrimaryPreference();
		var parent = Parent;

		if (!(parent.EntryInstruction?.HasIntoWarehouseProcedure ?? ZBool.True))
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JI_PrimaryPreferenceInfo);
		}

		CheckPrimaryPreferenceForTurkeyCustomsDutyExemption();
	}

	protected override ZInt? GetDescriptionMaxLengthBasedOnDeclarationType() => SADConstants.CustomsFieldMaxLength.EntryLine.DescriptionForUCC6;

	#region Implementation

	void CheckPrimaryPreferenceForTurkeyCustomsDutyExemption()
	{
		var parent = Parent;

		if (parent.IsEligibleForTurkeyCustomsDutyExempt
			&& !parent.ActualSupportingDocumentCodeCollection.Any(x => x.CSI_Code == SupportingDocumentTypes.ATRCertificateN018))
		{
			parent.JI_PrimaryPreferenceInfo.AddMessageError(ValidationCaptions.InvoiceLine.PreferenceCannotBe400WithoutN018);
		}
	}

	void CheckTariffTurkeyCustomsDutyExemption()
	{
		var parent = Parent;

		if (parent.JI_PrimaryPreference.IsEmpty
			&& parent.IsNonTurkishImportWithTurkishDispatch
			&& parent.Lookups.PrimaryPreferenceList.ContainsCode(RefCusPreferences.NonImpositionOfCustomsDuties))
		{
			var messageError = ValidationCaptions
				.InvoiceLine
				.GetPreference400MissingForTurkeyCustomsDutyExemptionCaption(parent.JI_Tariff);

			parent.JI_TariffInfo.AddMessageError(messageError);
		}
	}

	void ValidateEntryLineRequiredSupportingDocuments()
	{
		if (Parent.IsImport && IsMergeDone && EntryLine.PreferenceCodeStartWith2)
		{
			if (IsFirstInvoiceLineOfRelatedEntryLine)
			{
				ValidateEntryLineHasC100OrU166SupportingDocument();
			}
			ValidateEntryLineHasC164OrC165SupportingDocuments();
		}
	}

	void ValidateEntryLineHasC100OrU166SupportingDocument()
	{
		var entryLineHasC100SupportingDocuments = EntryLineSupportingDocuments.HasDocument(SupportingDocumentTypes.C100);
		var entryLineHasU166SupportingDocuments = EntryLineSupportingDocuments.HasDocument(SupportingDocumentTypes.U166);

		if (!entryLineHasC100SupportingDocuments && !entryLineHasU166SupportingDocuments)
		{
			Parent.AddRowMessageError(ValidationCaptions.InvoiceLine.GetPreferenceRequiresSupportingDocumentC100orU166Caption(EntryLinePreferenceCode));
		}
		else if (entryLineHasC100SupportingDocuments && entryLineHasU166SupportingDocuments)
		{
			Parent.AddRowMessageError(ValidationCaptions.InvoiceLine.SupportingDocumentC100andU166CannotBeUsedTogheterCaption);
		}
	}

	void ValidateEntryLineHasC164OrC165SupportingDocuments()
	{
		var entryLineHasC164SupportingDocuments = EntryLineSupportingDocuments.HasDocument(SupportingDocumentTypes.U164);
		var entryLineHasU165SupportingDocuments = EntryLineSupportingDocuments.HasDocument(SupportingDocumentTypes.U165);

		if (!entryLineHasC164SupportingDocuments && !entryLineHasU165SupportingDocuments)
		{
			var isCustomsValueIsLessOrEqualThan6000 = EntryLine.IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration;
			var messageError = isCustomsValueIsLessOrEqualThan6000
				? ValidationCaptions.InvoiceLine.GetPreferenceRequiresSupportingDocumentC164orC165WhenItemPriceIsLessThanThresholdCaption(EntryLinePreferenceCode)
				: ValidationCaptions.InvoiceLine.GetPreferenceRequiresSupportingDocumentC164orC165WhenItemPriceIsGreaterOrEqualThanThresholdCaption(EntryLinePreferenceCode);

			if (!isCustomsValueIsLessOrEqualThan6000 || IsFirstInvoiceLineOfRelatedEntryLine)
			{
				Parent.AddRowMessageError(messageError);
			}
		}
		else if (entryLineHasC164SupportingDocuments && entryLineHasU165SupportingDocuments)
		{
			Parent.AddRowMessageError(ValidationCaptions.InvoiceLine.GetInvoiceLineRequiresOnlyOneCertificateCaption);
		}
	}

	IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> EntryLineSupportingDocuments => EntryLine?.SupportingDocuments ?? Enumerable.Empty<SupportingDocument>();
	ZString EntryLinePreferenceCode => EntryLine.PreferenceCode;

	#endregion
}
