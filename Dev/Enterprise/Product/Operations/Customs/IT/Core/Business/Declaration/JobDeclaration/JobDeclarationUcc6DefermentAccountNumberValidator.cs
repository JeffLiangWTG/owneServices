using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobDeclarationUcc6DefermentAccountNumberValidator
{
	public JobDeclarationUcc6DefermentAccountNumberValidator(JobDeclaration jobDeclaration)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
	}

	public void Validate()
	{
		var defermentAccountNumberInfo = declaration.JE_DefermentAccountNumberInfo;
		var defermentAccountNumberIsEmpty = declaration.JE_DefermentAccountNumber.IsEmpty;
		var hasFeeWithRequiredMethodOfPayment = HasFeeWithRequiredMethodOfPayment;

		if (defermentAccountNumberIsEmpty && hasFeeWithRequiredMethodOfPayment)
		{
			var message = declaration.IsImport
				? ValidationCaptions.JobDeclaration.TheSelectedMethodOfPaymentInTaxOrFeeRequiresToEnterApprovalDeferNo
				: ValidationCaptions.JobDeclaration.TheSelectedMethodOfPaymentInTaxOrFeeRequiresToEnterApprovalDeferNoCN0558;
			defermentAccountNumberInfo.AddMessageError(message);
		}
		if (!defermentAccountNumberIsEmpty && !hasFeeWithRequiredMethodOfPayment)
		{
			defermentAccountNumberInfo.AddMessageError(ValidationCaptions.JobDeclaration.DefermentAccountNumberMustBeEmptyIfMethodOfPaymentIsNotDEorG);
		}
	}

	bool HasFeeWithRequiredMethodOfPayment => declaration.ActiveEntryHeaders.Cast<CusEntryHeader>()
		.Any(aeh => aeh.MergedLines.Cast<CusEntryLine>()
			.Any(ml => ml.Fees.Cast<CusEntryLineFee>()
				.Any(elf => requiredMethodOfPaymentList.Contains(elf.CF_MethodOfPayment))));

	readonly ImmutableArray<ZString> requiredMethodOfPaymentList = new ZString[]
	{
		UniversalReferenceConstants.DutyMethodOfPayment.OthersD,
		UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE,
		UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG
	}.ToImmutableArray();

	readonly JobDeclaration declaration;
}
