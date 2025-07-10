using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
{
	public AddInfoJobComInvoiceLineValidation(EU.Business.Declaration.AddInfoJobComInvoiceLine parent)
		: base(parent)
	{
	}
	public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	protected override void CheckZG_SteelType()
	{
		base.CheckZG_SteelType();

		var steelType = Parent.ZG_SteelType;
		if (!steelType.IsEmpty && !Parent.Lookups.SteelTypeList.ContainsCode(steelType))
		{
			if (steelType.IsNumbersOnlyOrEmpty)
			{
				Parent.ZG_SteelTypeInfo.AddWarning(ValidationCaptions.InvoiceLine.SteelTypeIsNormallyFrom1To4);
			}
			else if (steelType.IsLettersOnlyOrEmpty)
			{
				Parent.ZG_SteelTypeInfo.AddMessageError(ValidationCaptions.InvoiceLine.SteelTypeMustBeNumeric);
			}
			else
			{
				Parent.ZG_SteelTypeInfo.AddError(ValidationCaptions.InvoiceLine.InvalidSteelType);
			}
		}
	}

	protected override void CheckZG_PortTaxRate()
	{
		base.CheckZG_PortTaxRate();

		var invoiceLine = Parent.InvoiceLine;
		if (invoiceLine.Declaration?.NeedsPortTax ?? ZBool.False)
		{
			MandatoryValidation.MessageErrorIfNotEntered(invoiceLine.ZG_PortTaxRateInfo);
			ListValidation.MessageErrorIfInvalidCode(invoiceLine.ZG_PortTaxRateInfo, Parent.Lookups.PortTaxRateList);

			var barrierPort = invoiceLine.Declaration?.BarrierPort ?? ZString.Empty;
			if (!barrierPort.IsEmpty)
			{
				CheckPortTaxRateCanBeDetermined(invoiceLine, barrierPort);
				CheckValidPortTaxSupportingDocumentPresence(invoiceLine, barrierPort);
			}
		}
	}

	void CheckPortTaxRateCanBeDetermined(JobComInvoiceLine invoiceLine, ZString barrierPort)
	{
		var harbourRateProvider = (IHarbourRateProvider)new JobComInvoiceLineHarbourRateProvider(invoiceLine);
		if (harbourRateProvider.HarbourRate is null)
		{
			invoiceLine.ZG_PortTaxRateInfo.AddMessageError(ValidationCaptions.InvoiceLine.GetPortTaxRateCannotBeDeterminedCaption(barrierPort));
		}
	}

	void CheckValidPortTaxSupportingDocumentPresence(JobComInvoiceLine invoiceLine, ZString barrierPort)
	{
		if (invoiceLine.CusEntryLine?.Fees.Any(x => ((CusEntryLineFee)x).IsPortTax) ?? ZBool.False)
		{
			var docForPortTax = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == SupportingDocumentTypes.PortTax);
			var expectedRefNumber = Prefixes.PortTaxReference + barrierPort;
			if (docForPortTax == null || docForPortTax.CSI_ReferenceNumber != expectedRefNumber)
			{
				invoiceLine.ZG_PortTaxRateInfo.AddMessageError(ValidationCaptions.InvoiceLine.GetSupportingDocumentMustPresentWithType39YYAndReferenceCaption(expectedRefNumber));
			}
		}
	}

	protected override void CheckZG_CountryOfDestination()
	{
		base.CheckZG_CountryOfDestination();
		var invoiceLine = Parent.InvoiceLine;
		if (invoiceLine == null)
		{
			return;
		}

		if (invoiceLine.Declaration?.IsUCC6AndIsExport ?? false)
		{
			ValidateCountryOfDestinationForEmptyValue(invoiceLine);
		}
	}

	#region Implementation

	void ValidateCountryOfDestinationForEmptyValue(JobComInvoiceLine invoiceLine)
	{
		var entryInstruction = invoiceLine.EntryInstruction;
		var declaration = invoiceLine.Declaration;

		var entryInstructionSubStyle = entryInstruction?.CEI_SubStyle ?? ZString.Empty;

		if (entryInstruction is null || declaration.ZG_IsSecurityDeclaration || entryInstructionSubStyle.In(subStylesToIgnoreForMandatoryValidation))
		{
			return;
		}

		new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => declaration.JE_GoodsDestination,
			lineValuesProvider: () => entryInstruction
										.InvoiceLines
										.Cast<JobComInvoiceLine>()
										.Select(x => x.ZG_CountryOfDestination))
		{
			IsEmptyFunc = x => x.IsEmpty
		}
		.ValidateLine(invoiceLine.ZG_CountryOfDestinationInfo, invoiceLine.ZG_CountryOfDestination);
	}

	readonly ImmutableArray<ZString> subStylesToIgnoreForMandatoryValidation = new ZString[]
	{
		ITEntrySubStyleList.Codes.SimplifiedDeclarationOccasionallyB,
		ITEntrySubStyleList.Codes.SimplifiedDeclarationRegularlyC,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationE,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF,
	}.ToImmutableArray();

	#endregion

}
