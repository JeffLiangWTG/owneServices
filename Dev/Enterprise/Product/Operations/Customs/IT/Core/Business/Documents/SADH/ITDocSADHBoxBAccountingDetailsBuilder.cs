using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class ITDocSADHBoxBAccountingDetailsBuilder
{
	public ITDocSADHBoxBAccountingDetailsBuilder(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	readonly CusEntryHeader entryHeader;

	public ZString GetAccountingDetails()
	{
		var sb = new ZStringBuilder();

		sb.Append(GetRegistrationDataRow());
		sb.Append(GetDeferralAccountRow());
		sb.Append(GetDutyAmountGRow());
		AppendEOrFRowDependingOnDeclaration(sb);
		sb.AppendIfNotEmpty(GetIvistoBlock());
		return sb.ToStringWithNewLineBetweenAppends();
	}

	#region Implementation

	void AppendEOrFRowDependingOnDeclaration(ZStringBuilder builder)
	{
		if (entryHeader?.Declaration?.IsUCC6 ?? false)
		{
			builder.Append(GetDutyAmountERow());
			return;
		}

		builder.Append(GetDutyAmountFRow());
	}

	ZString GetRegistrationDataRow()
	{
		var regCusEntryNumberWrapper = RegCusEntryNumberWrapper;

		if (!regCusEntryNumberWrapper.IsEmpty)
		{
			return FormattableString.Invariant($"REG. {regCusEntryNumberWrapper.RegistrationNumberIncludingRegisterAndSeries} {regCusEntryNumberWrapper.IssueDate.ToItalianShortDateString()}");
		}
		return ZString.Empty;
	}

	ZString GetDeferralAccountRow()
	{
		var sb = new ZStringBuilder();

		var defermentAccountNumber = entryHeader?.Declaration?.JE_DefermentAccountNumber ?? ZString.Empty;
		if (!defermentAccountNumber.IsEmpty)
		{
			sb.Append(FormattableString.Invariant($"ACCOUNT N. {defermentAccountNumber}"));
		}

		var a93Number = A93Collection.FirstOrDefault()?.A93Number ?? ZString.Empty;
		if (!a93Number.IsEmpty)
		{
			sb.Append(FormattableString.Invariant($"A93 N. {a93Number}"));
		}

		return sb.ToStringWithDelimiterBetweenAppends(" - ");
	}

	ZString GetDutyAmountGRow() => GetDutyAmountRow(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG);

	ZString GetDutyAmountERow() => GetDutyAmountRow(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE);

	ZString GetDutyAmountFRow() => GetDutyAmountRow(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentCustomsProcedureF);

	ZString GetDutyAmountRow(string methodOfPayment)
	{
		var sb = new ZStringBuilder();
		var a93Payment = A93Collection.SingleOrDefault(x => x.MethodOfPayment == methodOfPayment);

		sb.Append(FormattableString.Invariant($"EUR {a93Payment?.C9_PaymentAmount ?? 0.00m}"));
		var expirationDate = a93Payment?.C9_PaymentDate ?? ZDateTime.Empty;
		if (!expirationDate.IsEmpty)
		{
			sb.Append(FormattableString.Invariant($"EXP. {expirationDate.ToItalianShortDateString()}"));
		}
		return sb.ToStringWithDelimiterBetweenAppends(" ");
	}

	ZString GetIvistoBlock()
	{
		var entryNumbersProvider = entryHeader.EntryNumbersProvider;
		if (entryNumbersProvider.Ivisto == null)
		{
			return ZString.Empty;
		}

		var ivistoWrapper = entryNumbersProvider.IvistoWrapper;
		return new ZStringBuilder()
			.Append((NoResString)"Notifica Visto Uscire per l'esportazione")
			.Append(FormattableString.Invariant($"Esito: {new ExitStatusList().GetDescriptionFromCode(ivistoWrapper.Status)}     {ivistoWrapper.Date:dd/MM/yyyy}"))
			.Append(FormattableString.Invariant($"Dog. di destino: {ivistoWrapper.Office}     {ivistoWrapper.OfficeDescription.Left(25)}"))
			.ToStringWithNewLineBetweenAppends();
	}

	RegCusEntryNumberWrapper RegCusEntryNumberWrapper => entryHeader.EntryNumbersProvider.RegistrationInfoWrapper;
	IEnumerable<CusEntryPayInfo> A93Collection => entryHeader.EntryPayInfos.ElementsAsEnumerable.Where(x => x.C9_TransactionType == RegCusEntryNumberWrapper.RegisterIncludingSeries);

	#endregion
}
