using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class FRCustomsEndorsementWrapper : EU.Business.Documents.CertificateOfOrigin.CustomsEndorsementWrapper
	{
		public FRCustomsEndorsementWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString GetForm() => Declaration.JE_EntryStyle + (Declaration.FirstActiveEntryHeaderWithEntryNum?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);
		protected override ZString GetIssuingCountry()
		{
			var result = ZString.Empty;
			var countryCode = CauOffice()?.ZZD_CountryOrGrouping ?? ZString.Empty;

			if (!countryCode.IsEmpty)
			{
				result = RefCountry.LoadFromCountryCode(Declaration.Factory, countryCode)?.Description ?? ZString.Empty;
			}
			return result;
		}

		protected override ZString GetPlace() => CauOffice()?.ZZD_Description ?? ZString.Empty;

		public ZZRefCusCodeListCombined CauOffice()
		{
			ZZRefCusCodeListCombined result = null;
			var customsOfficeCode = (Declaration as JobDeclaration)?.OfficeOfDeclaration ?? ZString.Empty;
			if (!customsOfficeCode.IsEmpty)
			{
				result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Declaration.Factory, customsOfficeCode, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today);
			}
			return result;
		}

		protected override ZBool GetShowEntryNumber()
		{
			return true;
		}
	}
}
