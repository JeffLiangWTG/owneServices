using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder
{
	public ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	readonly CusEntryHeader entryHeader;

	public ZString GetOfficeOfDepartureAndRegistrationData()
	{
		var regCusEntryNumberWrapper = entryHeader.EntryNumbersProvider.RegistrationInfoWrapper;

		var sb = new ZStringBuilder();
		if (!regCusEntryNumberWrapper.IsEmpty)
		{
			sb.Append(FormattableString.Invariant($"{regCusEntryNumberWrapper.CustomsOfficeCode} - {regCusEntryNumberWrapper.CustomsOfficeDescription}"));
			sb.Append(ZString.Empty);
			sb.Append(FormattableString.Invariant($"REG: {regCusEntryNumberWrapper.RegistrationNumberIncludingRegisterAndSeries}"));
			sb.Append(FormattableString.Invariant($"DEL {regCusEntryNumberWrapper.IssueDate.ToItalianShortDateString()}"));
		}

		return sb.ToStringWithNewLineBetweenAppends();
	}
}
