using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class PackageAndMarksAndNumbersInfo : EU.Business.Documents.DocDataObjects.PackageAndMarksAndNumbersInfo
	{
		public PackageAndMarksAndNumbersInfo(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override ZString GetPackageType(Customs.Business.BasePackage package)
		{
			var list = RefCusCodeListTypes.GetCachedList(package.Factory, Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, EU.Business.UniversalReferenceConstants.UNPackTypeStartDate, languageCode: CountryCodes.France);
			return list.GetDescriptionFromCode(base.GetPackageType(package)) ?? package.CW_PackType;
		}
	}
}
