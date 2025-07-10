using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
{
	public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent) : base(parent)
	{
	}

	public new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

	protected override void CheckZG_AgreedPlaceCode()
	{
		var agreedPlaceCodeInfo = Parent.ZG_AgreedPlaceCodeInfo;
		var invoice = Parent.Parent;
		var declaration = invoice.JobDeclaration;
		var agreedPlaceCode = Parent.ZG_AgreedPlaceCode;

		ListValidation.MessageErrorIfInvalidCode(agreedPlaceCodeInfo);

		if (declaration?.IsImport ?? ZBool.False)
		{
			CheckAgreedPlaceCodeForImport(agreedPlaceCodeInfo, invoice, declaration, agreedPlaceCode);
		}

		if (invoice.CusEntryInstructions.Any(x => !x.AllRelatedInvoicesHaveSameAgreedPlaceCode))
		{
			agreedPlaceCodeInfo.AddMessageError(ValidationCaptions.Shared.InvoicesLinkedHaveDifferentAgreedPlaceCode);
		}

		if (declaration?.IsExport ?? false)
		{
			CheckAgreedPlaceCodeForExport(agreedPlaceCodeInfo, invoice);
		}
	}

	#region Implementation

	void CheckAgreedPlaceCodeForImport(ZPropertyInfo agreedPlaceCodeInfo, JobComInvoiceHeader invoice, JobDeclaration declaration, ZString agreedPlaceCode)
	{
		var incoterm = invoice.JZ_IncoTerm;

		MandatoryValidation.MessageErrorIfNotEntered(agreedPlaceCodeInfo);

		if (!CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode(incoterm))
		{
			CheckAggredPlaceCodeIsNotItalian(agreedPlaceCodeInfo, agreedPlaceCode);
		}

		if (!agreedPlaceCode.IsEmpty && CustomsRulesProvider.AgreedPlaceCodeRequiresInvoiceFreightCharges(agreedPlaceCode) && declaration.IsMergeDone && invoice.Entries.Cast<CusEntryHeader>().Any(x => x.CH_FreightAdjustment == ZDecimal.Zero))
		{
			agreedPlaceCodeInfo.AddMessageError(ValidationCaptions.InvoiceHeader.InvoiceFreightChargesRequired);
		}
	}

	void CheckAggredPlaceCodeIsNotItalian(ZPropertyInfo agreedPlaceCodeInfo, ZString agreedPlaceCode)
	{
		if (agreedPlaceCode == UniversalReferenceConstants.AgreedPlaceCodes.AgreedPlaceThisMemberState)
		{
			agreedPlaceCodeInfo.AddMessageError(ValidationCaptions.InvoiceHeader.AgreedPlaceCodeCannotBe1);
		}
		else if (agreedPlaceCode.StartsWith(Core.Constants.CountryCodes.Italy))
		{
			agreedPlaceCodeInfo.AddMessageError(ValidationCaptions.InvoiceHeader.IncotermPlaceCodeCannotBeIt);
		}
	}

	void CheckAgreedPlaceCodeForExport(ZPropertyInfo agreedPlaceCodeInfo, JobComInvoiceHeader invoice)
	{
		if (invoice.AgreedPlaceCodeSupportAndVisible)
		{
			MandatoryValidation.MessageErrorIfNotEntered(agreedPlaceCodeInfo);
		}
	}

	#endregion
}
