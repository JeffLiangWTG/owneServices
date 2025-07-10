using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class EmptyCustomsEndorsementWrapper : ICustomsEndorsement
{
	public ZString EntryNumber => ZString.Empty;
	public ZBool ShowEntryNumber => false;

	public ZString EUR1Pg1Box11TextEntryNumber => ZString.Empty;

	ZString ICustomsEndorsement.Form => ZString.Empty;

	ZString ICustomsEndorsement.FormNo => ZString.Empty;

	ZDate ICustomsEndorsement.Date => ZDate.Empty;

	ZString ICustomsEndorsement.CustomsOffice => ZString.Empty;

	ZString ICustomsEndorsement.IssuingCountry => ZString.Empty;

	ZString ICustomsEndorsement.Place => ZString.Empty;

	ZDate ICustomsEndorsement.ReferenceDate => ZDate.Empty;
}
