using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public abstract class ASYCUDAManifestBillModuleCollection<T> : BusinessObjectCollection<T>
		where T : AsycudaBill
	{
		protected ASYCUDAManifestBillModuleCollection(BusinessObjectFactory factory, ZString country)
			: base(factory, DefaultFilter(country))
		{
		}

		static ZQuery DefaultFilter(ZString country)
		{
			var query = new ZDBOnlyQuery(typeof(T));
			var countryFilter = country.IsEmpty ? Invariant($@" AND AMA_RN_NKCountry <> '{Core.Constants.CountryCodes.Singapore}'") : Invariant($@" AND AMA_RN_NKCountry = @Country");
			var additionalSql = Invariant($@"
	ABL_PK IN (
	SELECT ABL_PK
	FROM dbo.AsycudaManifestHeader
	INNER JOIN dbo.AsycudaBill
	ON AMA_PK = ABL_AMA
	AND AMA_ClusterKey = ABL_ClusterKey
	AND ABL_BolType <> @BolType
	AND AMA_ApplicationCode <> @ApplicationCode1
	AND AMA_ApplicationCode <> @ApplicationCode2
	{countryFilter}
)");

			var sqlParameterCollection = new ZSqlParameterCollection(
				ZSqlParameter.New("@ApplicationCode1", AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut, AsycudaManifestHeaderSchema.AMA_ApplicationCode),
				ZSqlParameter.New("@ApplicationCode2", ApplicationCodeTypeList.Codes.TRETrade, AsycudaManifestHeaderSchema.AMA_ApplicationCode),
				ZSqlParameter.New("@BolType", AsycudaBill.ChildBolCode, AsycudaBillSchema.ABL_BolType)
				);
			if (!country.IsEmpty)
			{
				sqlParameterCollection.Add(ZSqlParameter.New("@Country", country, AsycudaManifestHeaderSchema.AMA_RN_NKCountry));
			}
			query.AddFilterAndZSQLParameterCollection(additionalSql, sqlParameterCollection);
			return query;
		}

		#region Fetch Strategy
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ASYCUDAManifestBillModuleCollectionFetchStrategy<T>(this);
		}
		#endregion
	}
}
