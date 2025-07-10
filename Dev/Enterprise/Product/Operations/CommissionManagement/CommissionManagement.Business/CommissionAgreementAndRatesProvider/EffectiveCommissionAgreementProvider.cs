using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class EffectiveCommissionAgreementProvider : IMultipleCommissionAgreementAndRatesProvider
	{
		#region New

		public static EffectiveCommissionAgreementProvider New(BusinessObjectFactory factory, EffectiveCommissionItemArgs itemArgs, DataTable effectiveDateCacheTable = null)
		{
			return new EffectiveCommissionAgreementProvider(factory, itemArgs, effectiveDateCacheTable);
		}

		#endregion

		#region Constructor

		protected EffectiveCommissionAgreementProvider(BusinessObjectFactory factory, EffectiveCommissionItemArgs itemArgs, DataTable effectiveDateCacheTable = null)
		{
			this.factory = factory;
			this.itemArgs = itemArgs;
			this.effectiveDateCacheTable = effectiveDateCacheTable;
		}

		#endregion

		#region Fields

		readonly BusinessObjectFactory factory;
		readonly EffectiveCommissionItemArgs itemArgs;
		readonly DataTable effectiveDateCacheTable;

		#endregion

		#region ByStream

		public Dictionary<ZString, ICommissionAgreementAndRates> ByStream
		{
			get
			{
				if (byStream == null)
				{
					byStream = GetCommissionAgreementAndRatesByStream();
				}

				return byStream;
			}
		}

		Dictionary<ZString, ICommissionAgreementAndRates> byStream;

		Dictionary<ZString, ICommissionAgreementAndRates> GetCommissionAgreementAndRatesByStream()
		{
			var overallItemQuery = ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs, null, effectiveDateCacheTable);
			var sql = string.Format(CultureInfo.InvariantCulture, @"
CA0_PK IN
(
	SELECT CA0_PK
	FROM
	(
		SELECT
			CA0_PK,
			ROW_NUMBER() OVER (PARTITION BY CA0_CommissionStream ORDER BY VCI_TotalAllCodes ASC) RANK
		FROM
			dbo.OrgCommissionAgreement
			JOIN dbo.ViewCommissionAgreementOverallItem ON CA0_PK = VCI_CA0
		WHERE
			{0}
	) a
	WHERE
		RANK = 1
)
", overallItemQuery.ParameterisedText.ParameterisedQueryText);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(overallItemQuery.Params));

			var agreements = factory.Load<OrgCommissionAgreement>(query);
			var result = new Dictionary<ZString, ICommissionAgreementAndRates>();
			foreach (var agreement in agreements)
			{
				result[agreement.CA0_CommissionStream] = new CommissionAgreementAndRates(agreement, itemArgs.CommissionDateByChargeDictionary);
			}

			return result;
		}

		#endregion
	}
}
