using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlCustomsEndorsement = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICustomsEndorsement;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlCustomsEndorsementWrapper : ICustomsEndorsement
{
	public XmlCustomsEndorsementWrapper(IXmlCustomsEndorsement customsEndorsement)
	{
		this.customsEndorsement = Argument.NotNull(customsEndorsement, nameof(customsEndorsement));
	}

	readonly IXmlCustomsEndorsement customsEndorsement;

	ZString ICustomsEndorsement.Form => form ?? (form = customsEndorsement.Form);
	string form;

	ZString ICustomsEndorsement.FormNo => formNo ?? (formNo = customsEndorsement.FormNo);
	string formNo;

	ZDate ICustomsEndorsement.Date => date ?? (date = new ZDate(customsEndorsement.IssuingDate)).Value;
	ZDate? date;

	ZString ICustomsEndorsement.CustomsOffice => customsOffice ?? (customsOffice = customsEndorsement.CustomsOffice);
	string customsOffice;

	ZString ICustomsEndorsement.IssuingCountry => issuingCountry ?? (issuingCountry = customsEndorsement.IssuingCountry);
	string issuingCountry;

	ZString ICustomsEndorsement.Place => place ?? (place = customsEndorsement.ReferencePlace);
	string place;

	ZDate ICustomsEndorsement.ReferenceDate => referenceDate ?? (referenceDate = new ZDate(customsEndorsement.ReferenceDate)).Value;
	ZDate? referenceDate;

	public ZString EntryNumber => ZString.Empty;

	public ZBool ShowEntryNumber => false;

	public ZString EUR1Pg1Box11TextEntryNumber => ZString.Empty;
}
