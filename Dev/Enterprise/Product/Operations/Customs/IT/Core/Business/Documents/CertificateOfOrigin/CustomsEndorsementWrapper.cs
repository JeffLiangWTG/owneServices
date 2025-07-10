using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class CustomsEndorsementWrapper : EU.Business.Documents.CertificateOfOrigin.CustomsEndorsementWrapper
{
	public CustomsEndorsementWrapper(CusEntryHeader entryHeader) : base(entryHeader?.Declaration)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	protected override ZString GetForm() => FormattableString.Invariant($"{Declaration.JE_EntryStyle} {EntryNumberWrapper.RegisterIncludingSeries}").Trim();

	protected override ZString GetFormNo() => EntryNumberWrapper.RegistrationNumber;

	protected override ZDate GetDate() => EntryNumberWrapper.IssueDate;

	protected override ZString GetEntryNumber() => entryHeader.EntryNumber;

	#region Implementation

	RegCusEntryNumberWrapper EntryNumberWrapper => entryNumberWrapper ?? (entryNumberWrapper = GetEntryNumberWrapper());
	RegCusEntryNumberWrapper entryNumberWrapper;

	RegCusEntryNumberWrapper GetEntryNumberWrapper()
	{
		if (entryHeader.ZG_AmendmentStatus == AmendmentStatusList.Codes.Amendment)
		{
			var retEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberConstants.EntryTypes.Rectification, entryHeader.CountryCode);
			return RegCusEntryNumberWrapper.Load(retEntryNumber);
		}
		return entryHeader.EntryNumbersProvider.RegistrationInfoWrapper;
	}

	#endregion
}
