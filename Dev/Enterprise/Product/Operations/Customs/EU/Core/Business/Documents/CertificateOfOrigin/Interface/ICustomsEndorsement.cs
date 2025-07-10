using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface ICustomsEndorsement
	{
		ZString Form { get; }
		ZString FormNo { get; }
		ZDate Date { get; }
		ZString CustomsOffice { get; }
		ZString IssuingCountry { get; }
		ZString Place { get; }
		ZString EntryNumber { get; }
		ZBool ShowEntryNumber { get; }
		ZString EUR1Pg1Box11TextEntryNumber { get; }
		ZDate ReferenceDate { get; }
	}
}
