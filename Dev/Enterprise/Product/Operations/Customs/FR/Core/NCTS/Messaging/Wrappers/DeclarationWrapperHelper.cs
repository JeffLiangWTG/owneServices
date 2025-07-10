using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public static class DeclarationWrapperHelper
	{
		public static ZString ArrivalAgreementNumber(NctsHeader nctsHeader)
		{
			var result = ZString.Empty;
			if (nctsHeader != null)
			{
				var organisationPK = nctsHeader.Declarant.OrganisationPK;
				if (organisationPK.IsEmpty)
				{
					organisationPK = nctsHeader.Consignee.OrganisationPK;
				}
				result = OrgCusAccount.Loader.LoadTop1ByCodeAndCountry(nctsHeader.Factory, organisationPK, Business.OrgCusAccountCodeList.Codes.DTA, nctsHeader.CountryCode)?.CZ_Account ?? ZString.Empty;
			}
			return result;
		}

		public static ZString DepartureAgreementNumber(NctsHeader nctsHeader)
		{
			var result = ZString.Empty;
			if (nctsHeader != null)
			{
				var organisationPK = nctsHeader.Principal.OrganisationPK;
				if (organisationPK.IsEmpty)
				{
					organisationPK = nctsHeader.Consignor.OrganisationPK;
				}
				result = OrgCusAccount.Loader.LoadTop1ByCodeAndCountry(nctsHeader.Factory, organisationPK, Business.OrgCusAccountCodeList.Codes.DTA, nctsHeader.CountryCode)?.CZ_Account ?? ZString.Empty;
			}
			return result;
		}

		public static JustificationReglementaireInvalidation CancellationJustification(NctsHeader nctsHeader)
		{
			var result = JustificationReglementaireInvalidation.Item1;
			if (nctsHeader != null)
			{
				result = nctsHeader.HasDepartureReachedGoodsReleasedForTransitStatus ? JustificationReglementaireInvalidation.Item2 : JustificationReglementaireInvalidation.Item1;
			}
			return result;
		}
	}
}
