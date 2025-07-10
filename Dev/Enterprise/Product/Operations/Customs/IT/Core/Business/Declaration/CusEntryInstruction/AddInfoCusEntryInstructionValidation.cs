using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Validation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
{
	public AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	CusEntryInstruction EntryInstruction => (CusEntryInstruction)Parent.Parent;

	protected override void CheckZG_TempProcLimitDate()
	{
		base.CheckZG_TempProcLimitDate();
		var tempProcLimitDateInfo = Parent.ZG_TempProcLimitDateInfo;
		var procedureCode = EntryInstruction.CEI_Procedure;
		var tempProcLimitDateRequired = EntryInstruction.TempProcLimitDateRequired;
		var tempProcLimitDate = Parent.ZG_TempProcLimitDate;
		var message = ZString.Empty;

		if (tempProcLimitDateRequired)
		{
			if (tempProcLimitDate.IsEmpty)
			{
				message = ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired;
			}
			else if (tempProcLimitDate < ZDate.Today)
			{
				message = ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateShouldBeGreaterThanToday;
			}
		}
		else if (!tempProcLimitDate.IsEmpty)
		{
			if (procedureCode.IsEmpty)
			{
				message = ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateIsNotRequiredForEmptyProcedure;
			}
			else
			{
				message = ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateIsNotRequiredForProcedureCode(procedureCode);
			}
		}

		if (!message.IsEmpty)
		{
			tempProcLimitDateInfo.AddMessageError(message);
		}
	}

	protected override void CheckZG_ParticipantType()
	{
		base.CheckZG_ParticipantType();
		if (EntryInstruction.JobDeclaration?.IsExport ?? false)
		{
			var participantTypeInfo = Parent.ZG_ParticipantTypeInfo;
			MandatoryValidation.CheckEntered(participantTypeInfo);
			ListValidation.ErrorIfInvalidCode(participantTypeInfo);
		}
	}

	protected override void CheckZG_PreviousInvoiceAmount()
	{
		base.CheckZG_PreviousInvoiceAmount();
		if (EntryInstruction.IsTriangulationOrJointDeclaration)
		{
			var previousInvoiceAmountInfo = Parent.ZG_PreviousInvoiceAmountInfo;
			MandatoryValidation.MessageErrorIfIsZero(previousInvoiceAmountInfo);
			MandatoryValidation.MessageErrorIfIsNegative(previousInvoiceAmountInfo);
		}
	}

	protected override void CheckZG_PreviousInvoiceCurrency()
	{
		base.CheckZG_PreviousInvoiceCurrency();
		if (EntryInstruction.IsTriangulationOrJointDeclaration)
		{
			var previousInvoiceCurrencyInfo = Parent.ZG_PreviousInvoiceCurrencyInfo;
			MandatoryValidation.MessageErrorIfNotEntered(previousInvoiceCurrencyInfo);
			ListValidation.MessageErrorIfInvalidCode(previousInvoiceCurrencyInfo);
			CheckExistingExchangeRate(previousInvoiceCurrencyInfo);
		}
	}

	protected override void CheckZG_SimplifiedDecAcceptanceDate()
	{
		base.CheckZG_SimplifiedDecAcceptanceDate();
		var canSetSimplifiedDecAcceptanceDate = EntryInstruction.CanSetSimplifiedDecAcceptanceDate;
		if (canSetSimplifiedDecAcceptanceDate)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_SimplifiedDecAcceptanceDateInfo);
			DateValidation.MessageErrorIfDateIsInFuture(Parent.ZG_SimplifiedDecAcceptanceDateInfo);
		}
	}

	void CheckExistingExchangeRate(ZPropertyInfo previousInvoiceCurrencyInfo)
	{
		var exchangeRateValidator = new ExternalMessageValidation(EntryInstruction);
		exchangeRateValidator.ValidateExchangeRateExist((RefCurrencyCurrencyConverter)EntryInstruction.CurrencyConverter, previousInvoiceCurrencyInfo);
	}
}
