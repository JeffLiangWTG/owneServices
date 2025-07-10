using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBStandaloneHeaderWrapper : INBHeader
{
	public NBStandaloneHeaderWrapper(INBWrappableBusinessObject entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}
	readonly INBWrappableBusinessObject entryLine;

	public ZString MessageCodeEntry => FormattableString.Invariant($"{EntryNumberWrapper?.Register ?? ZString.Empty} {EntryNumberWrapper?.Series ?? ZString.Empty}").Trim();

	public ZString ReferenceNumber => EntryNumberWrapper?.RegistrationNumberWithoutCin ?? ZString.Empty;

	public ZString DeclarationCIN => EntryNumberWrapper?.RegistrationNumberCin ?? ZString.Empty;

	public ZDate DeclarationDate => EntryNumberWrapper?.IssueDate ?? ZDate.Empty;

	public ZInt ItemNumber => entryLine.LineNumber;

	RegCusEntryNumberWrapper EntryNumberWrapper => entryLine.EntryNumberWrapper;
}
