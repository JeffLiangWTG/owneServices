using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
	{
	}

	public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	JobDeclaration Declaration => Parent.JobDeclaration;

	protected override void CheckJZ_OH_Supplier()
	{
		base.CheckJZ_OH_Supplier();

		if (Parent.JobDeclaration != null && !Parent.JobDeclaration.AreDeclarationAndSupplierCountryCodesCompatible())
		{
			Parent.JZ_OH_SupplierInfo.AddWarning(ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
		}
	}

	protected override void CheckSupplierCannotBeSavedBecauseNoAddressIsSelected()
	{
	}

	protected override void CheckJZ_IncoTermPlace()
	{
		base.CheckJZ_IncoTermPlace();

		var incoTermPlaceInfo = Parent.JZ_IncoTermPlaceInfo;
		if (!incoTermPlaceInfo.ReadOnly)
		{
			CheckEntryInstructionsHaveConsistentIncotermPlaces(incoTermPlaceInfo);
		}
	}

	protected override void CheckJZ_RX_NKInvoice_Currency()
	{
		base.CheckJZ_RX_NKInvoice_Currency();

		var parent = Parent;

		if (parent.IsImport)
		{
			var invoiceCurrencyInfo = parent.JZ_RX_NKInvoice_CurrencyInfo;
			MessageValidation.CheckEntered(invoiceCurrencyInfo, Res.GetString("AD269C01-03C4-4CE8-9AF7-536A1E2EDB47", "Please enter a Currency"));
		}
	}

	protected override void CheckJZ_AdditionalTerms()
	{
		base.CheckJZ_AdditionalTerms();

		if (Parent.AdditionalTermsSupport)
		{
			var additionalTermsInfo = Parent.JZ_AdditionalTermsInfo;
			MandatoryValidation.MessageErrorIfNotEntered(additionalTermsInfo);
			CheckEntryInstructionsHaveConsistentDeliveryTerms(additionalTermsInfo);
		}
	}

	protected override ZBool RequireJZ_IncoTermPlaceMandatory
	{
		get
		{
			return Parent.IsImport
				|| IsAgreedPlaceCodeAvailableAndFilledWithCountryCode()
				|| IsIncotermPlaceMandatoryForNonUcc6ExportJob();
		}
	}

	protected override void CheckJZ_IncoTerm()
	{
		base.CheckJZ_IncoTerm();
		if (!CustomsRulesProvider.IncotermIsValidForIT(Parent.JZ_IncoTerm))
		{
			Parent.JZ_IncoTermInfo.AddMessageError(ValidationCaptions.Shared.IncotermInvalid);
		}
	}

	protected override void CheckJZ_ValuationCode()
	{
		base.CheckJZ_ValuationCode();

		CheckJZ_ValuationCodeE1301();
	}

	protected override bool IsJZ_ValuationCodeMandatory => Parent.CusEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure();

	protected override ZBool ShouldCheckMissingPreviousDocuments => !IsUcc6Export && base.ShouldCheckMissingPreviousDocuments;

	protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine => true;

	#region Implementation

	bool IsAgreedPlaceCodeAvailableAndFilledWithCountryCode() => Parent.AgreedPlaceCodeSupportAndVisible && Parent.ZG_AgreedPlaceCode.Length == 2;

	bool IsIncotermPlaceMandatoryForNonUcc6ExportJob()
	{
		var declaration = Declaration;
		return declaration != null && !declaration.IsUCC6 && declaration.IsExport && !Parent.JZ_IncoTerm.IsEmpty;
	}

	void CheckEntryInstructionsHaveConsistentDeliveryTerms(ZPropertyInfo additionalTermsInfo)
	{
		if (Parent.CusEntryInstructions.Any(x => !x.AllRelatedInvoicesHaveSameDeliveryTerms))
		{
			additionalTermsInfo.AddMessageError(ValidationCaptions.Shared.InvoicesLinkedHaveDifferentDeliveryTerms);
		}
	}

	void CheckEntryInstructionsHaveConsistentIncotermPlaces(ZPropertyInfo incoTermPlaceInfo)
	{
		if (Parent.CusEntryInstructions.Any(x => !x.AllRelatedInvoicesHaveSameIncoTermPlace))
		{
			incoTermPlaceInfo.AddMessageError(ValidationCaptions.Shared.InvoicesLinkedHaveDifferentIncoPlaces);
		}
	}

	void CheckJZ_ValuationCodeE1301()
	{
		if (IsTransitionPeriodAES30 && AtLeastOneInvoiceInTheSameEntryHasDifferentValuationCode())
		{
			Parent.JZ_ValuationCodeInfo.AddMessageError(ValidationCaptions.InvoiceHeader.AllInvoicesMustHaveTheSameTransactionNature);
		}

		bool AtLeastOneInvoiceInTheSameEntryHasDifferentValuationCode() => Parent.CusEntryInstructions
				.SelectMany(x => x.Invoices)
				.Any(x => x.JZ_ValuationCode != Parent.JZ_ValuationCode);
	}

	bool IsTransitionPeriodAES30 => Declaration?.IsTransitionPeriodAES30 ?? false;

	bool IsUcc6Export => Declaration?.IsUCC6AndIsExport ?? false;

	#endregion
}
