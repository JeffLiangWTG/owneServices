using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin;

public class CustomsEndorsementWrapper : EU.Business.Documents.CertificateOfOrigin.CustomsEndorsementWrapper
{
	public CustomsEndorsementWrapper(CusEntryHeader entryHeader) : base(entryHeader?.Declaration)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	protected override ZString GetForm() => DUAForm;

	protected override ZString GetFormNo() => entryHeader.MovementReferenceNumber;

	protected override ZDate GetDate() => entryHeader.MovementReferenceNumberIssueDate.Date;

	protected override ZString GetCustomsOffice()
	{
		var baseCustomsOffice = base.GetCustomsOffice();
		return baseCustomsOffice.IsEmpty ? string.Empty : FormattableString.Invariant($"{Declaration.JE_CustomsOffice} {baseCustomsOffice}");
	}

	protected override ZString GetIssuingCountry()
	{
		using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
		{
			return Res.GetString("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", "SPAIN"); //Change this to use ES translation
		}
	}

	protected override ZString GetEntryNumber() => entryHeader.EntryNumber;

	protected override ZString GetPlace() => entryHeader.Declaration.DeclarantOrgAddress?.OA_City ?? ZString.Empty;

	const string DUAForm = "D.U.A";
}
