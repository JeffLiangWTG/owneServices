using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public static class MessageProviderHelper
	{
		public static bool StatusIsNewOrMissing(ZString status) => status.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW) || status.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS);

		public static string GetRegCodeFromCustomsCodes(OrgHeader organisation)
		{
			var eori = organisation.GetEoriDetails();
			if (!eori.IsEmpty)
			{
				return eori;
			}
			var tcu = EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(organisation, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
			if (!tcu.IsEmpty)
			{
				return tcu;
			}
			return null;
		}

		public static IEnumerable<AdditionalInfo> FindAdditionalDocuments(NctsDepartureCargoDesc cargoDesc, string subType) => cargoDesc.AdditionalInfos.Where(inf => inf.CSI_SubType == subType).OrderBy(inf => inf.CSI_SystemCreateTimeUtc);
		public static IEnumerable<AdditionalInfo> FindAdditionalDocuments(NctsHeader header, string subType) => header.AdditionalDocuments.Where(inf => inf.CSI_SubType == subType).OrderBy(inf => inf.CSI_SystemCreateTimeUtc);
		public static IEnumerable<AdditionalInfo> FindAdditionalDocuments(NctsBill bill, string subType) => bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Where(inf => inf.CSI_SubType == subType).OrderBy(inf => inf.CSI_SystemCreateTimeUtc);
		public static IEnumerable<NctsSupportingDocument> FindSupportingDocuments(NctsBill bill, string subType) => bill.SupportingDocuments.Where(inf => inf.CSI_SubType == subType).OrderBy(inf => inf.CSI_SystemCreateTimeUtc);
	}
}
