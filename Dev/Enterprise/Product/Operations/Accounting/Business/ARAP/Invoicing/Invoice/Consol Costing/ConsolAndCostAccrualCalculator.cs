using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ConsolAndCostAccrualCalculator
	{
		public const string ConsolAndCostAccrualCalculatorKey = "ConsolAndCostAccrualCalculatorKey";
		public ConsolAndCostAccrualCalculator(BusinessObjectFactory factory, ZGuid creditorPK)
		{
			this.Factory = factory;
			this.creditorPK = creditorPK;
			PendingConsolCosts = new List<JobConsolCost>();
		}

		readonly BusinessObjectFactory Factory;
		readonly ZGuid creditorPK;

		#region PendingConsolCosts
		public List<JobConsolCost> PendingConsolCosts
		{
			get;
			set;
		}

		void PopulatePendingConsolCostsIfExists()
		{
			if (PendingConsolCosts != null && PendingConsolCosts.Count > 0)     // run the populate SQL for pending costs first, if there are any
			{
				PopulateAccrualDictionary(PendingConsolCosts);
				PendingConsolCosts.Clear();
			}
		}

		#endregion

		AccrualKey GetConsolAccrualCachingKey(JobConsolCost consolCost)
		{
			Argument.NotNull(consolCost, "consolCost");
			return CreateAccrualCacheKey(consolCost.E6_OH_Creditor, consolCost.E6_AC_ChargeCode, consolCost.E6_ParentID, consolCost.E6_GC);
		}

		void PopulateAccrualDictionary(IEnumerable<JobConsolCost> consolCosts)
		{
			SetupAccrualDictionaryBasedOnConsolCost(consolCosts);
			SetupAccrualDictionaryBasedOnConsolShipment(consolCosts);
		}

		#region Accrual based on consol cost
		void SetupAccrualDictionaryBasedOnConsolCost(IEnumerable<JobConsolCost> consolCosts)
		{
			if (consolCosts != null && consolCosts.Any())
			{
				var consolPKsToSearch = consolCosts.Select(x => x.E6_ParentID).Distinct();
				var consolPKAlreadyInCache = AccrualBasedOnConsolCost.Keys.Select(x => x.ConsolPK);
				var consolPKsNotInCache = consolPKsToSearch.Except(consolPKAlreadyInCache);

				if (consolPKsNotInCache.Any())    // only run sql if there are consols which have not been evaluated yet
				{
					string sQL =
	@"SELECT 
		ISNULL(CostWithCreditor.E6_OH_Creditor, CostWithoutCreditor.E6_OH_Creditor) AS E6_OH_Creditor, ISNULL(CostWithCreditor.E6_AC_ChargeCode, CostWithoutCreditor.E6_AC_ChargeCode) AS E6_AC_ChargeCode, 
		ISNULL(CostWithCreditor.E6_ParentID, CostWithoutCreditor.E6_ParentID) AS E6_ParentID, ISNULL(CostWithCreditor.E6_GC, CostWithoutCreditor.E6_GC) AS E6_GC, 
		ISNULL(CostWithCreditor.TotalCostAccrual, 0) + ISNULL(CostWithoutCreditor.TotalCostAccrual, 0) AS TotalCostAccrual
		FROM
		(
			SELECT E6_OH_Creditor, E6_AC_ChargeCode, E6_ParentID, E6_GC, SUM(E6_LocalCostAmount) TotalCostAccrual from dbo.JobConsolCost
			WHERE E6_ParentID IN (SELECT Value FROM @ConsolPKs)
			AND E6_AH_APInvoice IS NULL
			AND E6_GC =  @CompanyPK
			AND E6_OH_Creditor = @Creditor
			GROUP BY E6_OH_Creditor, E6_AC_ChargeCode, E6_ParentID, E6_GC
		) AS CostWithCreditor
		FULL OUTER JOIN
		(
			SELECT E6_OH_Creditor, E6_AC_ChargeCode, E6_ParentID, E6_GC, SUM(E6_LocalCostAmount) TotalCostAccrual from dbo.JobConsolCost
			WHERE E6_ParentID IN (SELECT Value FROM @ConsolPKs)
			AND E6_AH_APInvoice IS NULL
			AND E6_GC =  @CompanyPK
			AND E6_OH_Creditor IS NULL
			GROUP BY E6_OH_Creditor, E6_AC_ChargeCode, E6_ParentID, E6_GC
		) AS CostWithoutCreditor
		ON CostWithCreditor.E6_AC_ChargeCode = CostWithoutCreditor.E6_AC_ChargeCode
		AND CostWithCreditor.E6_ParentID = CostWithoutCreditor.E6_ParentID
		AND CostWithCreditor.E6_GC = CostWithoutCreditor.E6_GC";

					var parameters = new ZSqlParameter[]
					{
							ZSqlParameter.New("@CompanyPK", GlbCompany.CurrentCompany.PK, JobConsolCostSchema.E6_GC),
							ZSqlParameter.New("@Creditor", creditorPK, JobConsolCostSchema.E6_OH_Creditor),
							ZSqlParameter.New("@ConsolPKs", consolPKsNotInCache.ToList(), JobConsolCostSchema.E6_ParentID, true)
					};
					DynamicBusinessObjectCollection dynamicCollection = new DynamicBusinessObjectCollection(Factory);
					dynamicCollection.Load(sQL, parameters);

					foreach (DynamicBusinessObject datarow in dynamicCollection)
					{
						var key = CreateAccrualCacheKey(creditorPK, (ZGuid)datarow[JobConsolCost.Schema.E6_AC_ChargeCode], (ZGuid)datarow[JobConsolCost.Schema.E6_ParentID], (ZGuid)datarow[JobConsolCost.Schema.E6_GC]);
						if (!AccrualBasedOnConsolCost.ContainsKey(key))
						{
							AccrualBasedOnConsolCost.Add(key, (ZDecimal)datarow["TotalCostAccrual"]);
						}
					}
				}
			}
		}

		Dictionary<AccrualKey, ZDecimal> AccrualBasedOnConsolCost
		{
			get
			{
				if (fAccrualBasedOnConsolCost == null)
				{
					fAccrualBasedOnConsolCost = new Dictionary<AccrualKey, ZDecimal>();
				}
				return fAccrualBasedOnConsolCost;
			}
		}
		Dictionary<AccrualKey, ZDecimal> fAccrualBasedOnConsolCost;

		#endregion

		#region Accrual based on consol shipment

		void SetupAccrualDictionaryBasedOnConsolShipment(IEnumerable<JobConsolCost> consolCosts)
		{
			if (consolCosts != null && consolCosts.Any())
			{
				var consolPKsToSearch = consolCosts.Select(x => x.E6_ParentID).Distinct();
				var consolPKAlreadyInCache = AccrualBasedOnConsolShipment.Keys.Select(x => x.ConsolPK);
				var consolPKsNotInCache = consolPKsToSearch.Except(consolPKAlreadyInCache);

				if (consolPKsNotInCache.Any())    // only run sql if there are consols which have not been evaluated yet
				{
					string sQL =
@"
		SELECT 
		ISNULL(ChargesWithCreditor.JR_OH_CostAccount, ChargesWithoutCreditor.JR_OH_CostAccount) AS JR_OH_CostAccount, ISNULL(ChargesWithCreditor.JR_AC, ChargesWithoutCreditor.JR_AC) AS JR_AC, 
		ISNULL(ChargesWithCreditor.JN_JK, ChargesWithoutCreditor.JN_JK) AS JN_JK, ISNULL(ChargesWithCreditor.JH_GC, ChargesWithoutCreditor.JH_GC) AS JH_GC, 
		ISNULL(ChargesWithCreditor.TotalAccrual, 0) + ISNULL(ChargesWithoutCreditor.TotalAccrual, 0) AS TotalAccrual
		FROM
		(
			SELECT JR_OH_CostAccount, JR_AC, JN_JK, JH_GC, SUM(JR_LocalCostAmt) As TotalAccrual
			FROM dbo.JobConShipLink join dbo.JobShipment ON JN_JS = JS_PK
			JOIN dbo.JobHeader ON JH_ParentID = JS_PK
			JOIN dbo.JobCharge ON JH_PK = JR_JH
			LEFT JOIN dbo.AccTransactionLines ON JR_AL_APLine = AL_PK
			WHERE JN_JK IN (SELECT Value FROM @ConsolPKs)
			AND JH_GC = @CompanyPK
			AND JR_OH_CostAccount = @Creditor
			AND (AL_LineType = 'ACR' OR AL_LineType IS NULL)
			AND JR_E6 IS NULL
			GROUP BY JR_OH_CostAccount, JR_AC, JN_JK, JH_GC
		) AS ChargesWithCreditor
		FULL OUTER JOIN
		(
			SELECT JR_OH_CostAccount, JR_AC, JN_JK, JH_GC, SUM(JR_LocalCostAmt) As TotalAccrual
			FROM dbo.JobConShipLink join dbo.JobShipment ON JN_JS = JS_PK
			JOIN dbo.JobHeader ON JH_ParentID = JS_PK
			JOIN dbo.JobCharge ON JH_PK = JR_JH
			LEFT JOIN dbo.AccTransactionLines ON JR_AL_APLine = AL_PK
			WHERE JN_JK IN (SELECT Value FROM @ConsolPKs)
			AND JH_GC = @CompanyPK
			AND JR_OH_CostAccount IS NULL
			AND (AL_LineType = 'ACR' OR AL_LineType IS NULL)
			AND JR_E6 IS NULL
			GROUP BY JR_OH_CostAccount, JR_AC, JN_JK, JH_GC
		) AS ChargesWithoutCreditor
		ON ChargesWithCreditor.JR_AC = ChargesWithoutCreditor.JR_AC 
		AND ChargesWithCreditor.JN_JK = ChargesWithoutCreditor.JN_JK 
		AND ChargesWithCreditor.JH_GC = ChargesWithoutCreditor.JH_GC";

					var parameters = new ZSqlParameter[]
					{
							ZSqlParameter.New("@CompanyPK", GlbCompany.CurrentCompany.PK, JobConsolCostSchema.E6_GC),
							ZSqlParameter.New("@Creditor", creditorPK, JobConsolCostSchema.E6_OH_Creditor),
							ZSqlParameter.New("@ConsolPKs", consolPKsNotInCache.ToList(), JobConsolCostSchema.E6_ParentID, true)
					};
					DynamicBusinessObjectCollection dynamicCollection = new DynamicBusinessObjectCollection(Factory);
					dynamicCollection.Load(sQL, parameters);

					foreach (DynamicBusinessObject datarow in dynamicCollection)
					{
						var key = CreateAccrualCacheKey(creditorPK, (ZGuid)datarow[JobCharge.Schema.JR_AC], (ZGuid)datarow[JobConShipLink.Schema.JN_JK], (ZGuid)datarow[JobHeader.Schema.JH_GC]);
						if (!AccrualBasedOnConsolShipment.ContainsKey(key))
						{
							AccrualBasedOnConsolShipment.Add(key, (ZDecimal)datarow["TotalAccrual"]);
						}
					}
				}
			}
		}

		Dictionary<AccrualKey, ZDecimal> AccrualBasedOnConsolShipment
		{
			get
			{
				if (fAccrualBasedOnConsolShipment == null)
				{
					fAccrualBasedOnConsolShipment = new Dictionary<AccrualKey, ZDecimal>();
				}
				return fAccrualBasedOnConsolShipment;
			}
		}
		Dictionary<AccrualKey, ZDecimal> fAccrualBasedOnConsolShipment;

		#endregion

		#region public Get methods
		public ZDecimal GetAccrualBasedOnConsolCost(JobConsolCost consolCost)
		{
			ZDecimal result = 0;
			var key = GetConsolAccrualCachingKey(consolCost);
			if (!AccrualBasedOnConsolCost.ContainsKey(key)) // key doesn't exist means item has not been populated into the dictionary yet
			{
				PopulatePendingConsolCostsIfExists();
			}
			if (!AccrualBasedOnConsolCost.TryGetValue(key, out result)) // after populate still not found, then add the item with 0 amount to the dictionary
			{
				AccrualBasedOnConsolCost.Add(key, result);
			}
			return result;
		}

		public ZDecimal GetAccrualBasedOnConsolShipment(JobConsolCost consolCost)
		{
			ZDecimal result = 0;
			var key = GetConsolAccrualCachingKey(consolCost);
			if (!AccrualBasedOnConsolShipment.ContainsKey(key)) // key doesn't exist means item has not been populated into the dictionary yet
			{
				PopulatePendingConsolCostsIfExists();
			}
			if (!AccrualBasedOnConsolShipment.TryGetValue(key, out result))
			{
				AccrualBasedOnConsolShipment.Add(key, result); // after populate still not found, then add the item with 0 amount to the dictionary
			}
			return result;
		}

		public ZDecimal GetAccrualBasedOnConsolShipmentAndConsolCost(JobConsolCost consolCost)
		{
			return GetAccrualBasedOnConsolCost(consolCost) + GetAccrualBasedOnConsolShipment(consolCost);
		}

		#endregion

		#region AccrualKey
		static AccrualKey CreateAccrualCacheKey(ZGuid creditorPK, ZGuid chargeCodePK, ZGuid consolPK, ZGuid companyPK)
		{
			return new AccrualKey(creditorPK, chargeCodePK, consolPK, companyPK);
		}

		struct AccrualKey
		{
			public AccrualKey(ZGuid creditorPK, ZGuid chargeCodePK, ZGuid consolPK, ZGuid companyPK)
			{
				this.CreditorPK = creditorPK;
				this.ChargeCodePK = chargeCodePK;
				this.ConsolPK = consolPK;
				this.CompanyPK = companyPK;
			}

			public readonly ZGuid CreditorPK;
			public readonly ZGuid ChargeCodePK;
			public readonly ZGuid ConsolPK;
			public readonly ZGuid CompanyPK;
		}

		#endregion
	}
}
