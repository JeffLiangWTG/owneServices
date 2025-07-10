using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business;

public static class AsycudaManifestHeaderHelper
{
	public static ZDBOnlyQuery GetManifestHeadersQuery(ZString masterBill)
	{
		var masterbillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
		masterbillQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, masterBill);
		masterbillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

		var manifestHeaderQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
		manifestHeaderQuery.AddSubQuery(masterbillQuery, JoinCondition.And);
		manifestHeaderQuery.OrderBy = AsycudaManifestHeaderSchema.Constants.AMA_SystemCreateTimeUtc + OrderByClause.Descending;

		manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut);
		var mawbRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
		if (mawbRecyclePeriod > 0)
		{
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mawbRecyclePeriod));
		}
		return manifestHeaderQuery;
	}

	public static ZString GetNumberWithHyphen(ZString number)
	{
		var m = number.KeepAlphanumericCharacters();
		return m.Left(3) + "-" + m.SubstringSafe(3, 8);
	}

	public static AsycudaManifestHeader CreateNew(BusinessObjectFactory factory, string countryCode, string manifestType, string applicationCode = null)
	{
		if (string.IsNullOrEmpty(applicationCode))
		{
			applicationCode = ApplicationCodeTypeList.Codes.Consolidator;
		}

		var typeDecider = new AsycudaManifestHeaderTypeDecider();
		var header = (AsycudaManifestHeader)factory.New(typeDecider.GetGlobalManifestType(factory, countryCode, manifestType, applicationCode));
		header.AMA_RN_NKCountry = countryCode;
		header.AMA_ApplicationCode = applicationCode;
		header.AMA_ManifestType = manifestType;
		return header;
	}
}
