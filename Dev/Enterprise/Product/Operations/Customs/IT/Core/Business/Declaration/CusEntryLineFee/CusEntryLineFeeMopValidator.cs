using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using MoP = Enterprise.Customs.IT.Business.UniversalReferenceConstants.DutyMethodOfPayment;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusEntryLineFeeMopValidator
{
	public CusEntryLineFeeMopValidator(CusEntryLineFee entryLineFee)
	{
		this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
	}

	readonly CusEntryLineFee entryLineFee;

	public void CheckMethodOfPayment()
	{
		var warnings = CheckMethodOfPayment(entryLineFee.CF_MethodOfPayment);

		foreach (var warning in warnings.Where(x => !x.IsEmpty))
		{
			entryLineFee.CF_MethodOfPaymentInfo.AddWarning(warning);
		}
	}

	#region Implementation

	IEnumerable<ZString> CheckMethodOfPayment(ZString methodOfPayment)
	{
		if (entryLineFee.IsTemporaryAntiDumping || entryLineFee.IsTemporaryCountervailing)
		{
			yield return CheckAntiDumpingAndCountervailingMoP(methodOfPayment);
		}
		else
		{
			yield return CheckMopRelatedToDefermentAccountNumber(methodOfPayment);
		}
	}

	ZString CheckMopRelatedToDefermentAccountNumber(ZString methodOfPayment)
	{
		var defermentAccountNumber = entryLineFee.EntryLine?.Declaration?.JE_DefermentAccountNumber ?? ZString.Empty;

		if (defermentAccountNumber.IsEmpty)
		{
			return CheckMopWhenDefermentAccountIsEmpty(methodOfPayment);
		}
		return CheckMopWhenDefermentAccountIsNotEmpty(methodOfPayment);
	}

	ZString CheckMopWhenDefermentAccountIsEmpty(ZString methodOfPayment)
	{
		if (NeedApprovalDeferNo(methodOfPayment))
		{
			return ValidationCaptions.CusEntryLineFee.YouHaveNotEnteredApprovalDeferNoForThisMop;
		}
		return ZString.Empty;
	}

	ZString CheckMopWhenDefermentAccountIsNotEmpty(ZString methodOfPayment)
	{
		if (IsDutyOrSanMarino())
		{
			return CheckDutyOrSanMarinoMoPWhenDefermentAccountIsNotEmpty(methodOfPayment);
		}

		if (methodOfPayment == MoP.ImmediatePaymentInCashA)
		{
			return ValidationCaptions.CusEntryLineFee.DefermentAccountNumberHasBeenSetButNotUsed;
		}

		return ZString.Empty;

		bool IsDutyOrSanMarino() => entryLineFee.IsDuty || entryLineFee.IsSanMarinoDuty;
	}

	ZString CheckDutyOrSanMarinoMoPWhenDefermentAccountIsNotEmpty(ZString methodOfPayment)
	{
		if (IsImport)
		{
			return CheckImportUCC6MoPWhenDefermentAccountIsNotEmpty(methodOfPayment);
		}
		if (IsUCC6 && methodOfPayment != MoP.DeferredPaymentE && methodOfPayment != MoP.OthersD)
		{
			return ValidationCaptions.CusEntryLineFee.MethodOfPaymentForExportUCC6DecDutiesWhenApprovalDeferNoIsSetShouldBeEorD;
		}
		if (!IsUCC6 && methodOfPayment != MoP.DeferredPaymentCustomsProcedureF && methodOfPayment != MoP.AgentGeneralGuaranteeAccountT)
		{
			return ValidationCaptions.CusEntryLineFee.MethodOfPaymentForDutiesWhenApprovalDeferNoIsSetShouldBeForT;
		}

		return ZString.Empty;
	}

	ZString CheckImportUCC6MoPWhenDefermentAccountIsNotEmpty(ZString methodOfPayment)
	{
		var isDefermentAccountNumberEqualToDatCode = Declaration?.IsDefermentAccountNumberEqualToDatCode() ?? ZBool.False;

		if (isDefermentAccountNumberEqualToDatCode && methodOfPayment != MoP.OthersD)
		{
			return ValidationCaptions.CusEntryLineFee.MopShouldBe(MoP.OthersD);
		}

		if (!isDefermentAccountNumberEqualToDatCode && methodOfPayment != MoP.DeferredPaymentE)
		{
			var procedureAttributeBasedCalculator = ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(entryLineFee);
			var defaultValueFromProcedureAttribute = procedureAttributeBasedCalculator.GetDefaultValue();

			return defaultValueFromProcedureAttribute.IsEmpty
				? ValidationCaptions.CusEntryLineFee.MopShouldBe(MoP.DeferredPaymentE)
				: string.Empty;
		}

		return ZString.Empty;
	}

	ZString CheckAntiDumpingAndCountervailingMoP(ZString methodOfPayment)
	{
		if (methodOfPayment != MoP.SecurityDepositDeferredPaymentR)
		{
			return ValidationCaptions.CusEntryLineFee.MopShouldBeRWhenChargeTypeIsA35orA45;
		}
		return ZString.Empty;
	}

	bool NeedApprovalDeferNo(ZString methodOfPayment)
	{
		var methodOfPaymentsThatNeedApprovalDeferNo = IsUCC6
			? MethodOfPaymentsThatNeedApprovalDeferNoForUCC6
			: MethodOfPaymentsThatNeedApprovalDeferNo;

		return methodOfPaymentsThatNeedApprovalDeferNo.Contains(methodOfPayment);
	}

	bool IsImport
	{
		get
		{
			if (!isImport.HasValue)
			{
				var declaration = Declaration;
				isImport = declaration != null && declaration.IsImport;
			}
			return isImport.Value;
		}
	}
	bool? isImport;

	bool IsUCC6
	{
		get
		{
			if (!isUCC6.HasValue)
			{
				var declaration = Declaration;
				isUCC6 = declaration != null && declaration.IsUCC6;
			}
			return isUCC6.Value;
		}
	}
	bool? isUCC6;

	#endregion

	JobDeclaration Declaration => entryLineFee.EntryLine?.Declaration;

	ImmutableArray<ZString> MethodOfPaymentsThatNeedApprovalDeferNo => (methodOfPaymentsThatNeedApprovalDeferNo ?? (methodOfPaymentsThatNeedApprovalDeferNo = GetMethodOfPaymentsThatNeedApprovalDeferNo().ToImmutableArray())).Value;
	ImmutableArray<ZString>? methodOfPaymentsThatNeedApprovalDeferNo;

	IEnumerable<ZString> GetMethodOfPaymentsThatNeedApprovalDeferNo()
	{
		yield return MoP.DeferredPaymentE;
		yield return MoP.DeferredPaymentCustomsProcedureF;
		yield return MoP.DeferredPaymentVatProcedureG;
		yield return MoP.AgentGeneralGuaranteeAccountT;
	}

	ImmutableArray<ZString> MethodOfPaymentsThatNeedApprovalDeferNoForUCC6 => (methodOfPaymentsThatNeedApprovalDeferNoForUCC6 ?? (methodOfPaymentsThatNeedApprovalDeferNoForUCC6 = GetMethodOfPaymentsThatNeedApprovalDeferNoForUCC6())).Value;
	ImmutableArray<ZString>? methodOfPaymentsThatNeedApprovalDeferNoForUCC6;

	ImmutableArray<ZString> GetMethodOfPaymentsThatNeedApprovalDeferNoForUCC6()
	{
		return MethodOfPaymentsThatNeedApprovalDeferNo.Union(new ZString[] { MoP.OthersD }).ToImmutableArray();
	}
}
