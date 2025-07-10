using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class CustomsEndorsementWrapper : ICustomsEndorsement
	{
		public CustomsEndorsementWrapper(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected JobDeclaration Declaration { get; }

		ZString ICustomsEndorsement.Form => form ?? (form = GetForm());
		string form;

		protected virtual ZString GetForm() => ZString.Empty;

		ZString ICustomsEndorsement.FormNo => formNo ?? (formNo = GetFormNo());
		string formNo;

		protected virtual ZString GetFormNo() => ZString.Empty;

		ZDate ICustomsEndorsement.Date => date ?? (date = GetDate()).Value;
		ZDate? date;

		protected virtual ZDate GetDate() => ZDate.Empty;

		ZDate ICustomsEndorsement.ReferenceDate => referenceDate ?? (referenceDate = GetReferenceDate()).Value;
		ZDate? referenceDate;

		protected virtual ZDate GetReferenceDate() => ZDate.Empty;

		ZString ICustomsEndorsement.CustomsOffice => customsOffice ?? (customsOffice = GetCustomsOffice());
		string customsOffice;

		protected virtual ZString GetCustomsOffice()
		{
			var customsOfficeCode = Declaration.JE_CustomsOffice;
			if (!customsOfficeCode.IsEmpty)
			{
				var countryCode = customsOfficeCode.Left(2);
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Declaration.Factory, customsOfficeCode, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.ZZD_Description ?? ZString.Empty;
			}
			return ZString.Empty;
		}

		ZString ICustomsEndorsement.IssuingCountry => issuingCountry ?? (issuingCountry = GetIssuingCountry());
		string issuingCountry;

		protected virtual ZString GetIssuingCountry() => ZString.Empty;

		ZString ICustomsEndorsement.Place => place ?? (place = GetPlace());
		string place;

		protected virtual ZString GetPlace() => ZString.Empty;

		ZString ICustomsEndorsement.EUR1Pg1Box11TextEntryNumber => eUR1Pg1Box11TextEntryNumber ?? (eUR1Pg1Box11TextEntryNumber = GetEUR1Pg1Box11TextEntryNumber());
		string eUR1Pg1Box11TextEntryNumber;

		protected virtual ZString GetEUR1Pg1Box11TextEntryNumber() => Res.GetString("127DB641-F32B-4BEE-8F0A-065D7717A70E", "No. of");

		ZString ICustomsEndorsement.EntryNumber => entryNumber ?? (entryNumber = GetEntryNumber());
		string entryNumber;

		protected virtual ZString GetEntryNumber() => Declaration.FirstActiveEntryHeaderWithEntryNum?.EntryNumber ?? ZString.Empty;

		ZBool ICustomsEndorsement.ShowEntryNumber => GetShowEntryNumber();

		protected virtual ZBool GetShowEntryNumber() => false;
	}
}
