using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalExtension;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class CostVarianceApprovalHelper
	{
		internal class CostVarianceKey
		{
			public CostVarianceKey(APInvoiceLine line)
			{
				Factory = line.Factory;
				Company = line.Company ?? GlbCompany.CurrentCompany;

				if (IsVarianceByImportedCharge && line.IsPopulatedFromImportedJobCharge)
				{
					ChargePK = line.OriginalJobCharge.PK;
					SetDefaultValueForProperty();
				}
				else
				{
					SetVarianceKey(line.AL_JH, line.AL_GB, line.AL_GE, line.AL_AC, line.APInvoice?.AH_OH ?? ZGuid.Empty);
				}
			}

			public CostVarianceKey(ApportionSplitCharge charge)
			{
				Factory = charge.Factory;
				Company = charge.Company ?? GlbCompany.CurrentCompany;

				if (IsVarianceByImportedCharge && charge.IsParentConsolCostImported)
				{
					ChargePK = charge.RelatedApportionChargeFromDB.PK;
					SetDefaultValueForProperty();
				}
				else
				{
					SetVarianceKey(charge.JR_JH, charge.JR_GB, charge.JR_GE, charge.JR_AC, charge.JR_OH_CostAccount);
				}
			}

			internal CostVarianceKey(IEnumerable<CostVarianceKey> keyCollection)
			{
				if (keyCollection.Any())
				{
					var key = keyCollection.First();
					Factory = key.Factory;
					Company = key.Company;

					ChargePK = key.ChargePK;
					JobPK = key.JobPK;
					BranchPK = key.BranchPK;
					DepartmentPK = key.DepartmentPK;
					ChargeCodePK = key.ChargeCodePK;
					CreditorPK = key.CreditorPK;
					ChargeCodePKCollection = keyCollection.SelectMany(x => x.ChargeCodePKCollection).ToHashSet();
					CreditorPKCollection = keyCollection.SelectMany(x => x.CreditorPKCollection).ToHashSet();
				}
			}

			CostVarianceKey(DynamicBusinessObject dynamicBizo)
			{
				if (dynamicBizo != null)
				{
					Factory = dynamicBizo.Factory;
					Company = Factory.Load<GlbBranch>(this.BranchPK)?.Company ?? GlbCompany.CurrentCompany;

					if (dynamicBizo.PropertyNames.Contains(AutoJobCharge.Schema.PK))
					{
						ChargePK = (ZGuid)dynamicBizo[AutoJobCharge.Schema.PK];
						SetDefaultValueForProperty();
					}
					else
					{
						SetVarianceKey((ZGuid)dynamicBizo[AutoJobCharge.Schema.JR_JH], (ZGuid)dynamicBizo[AutoJobCharge.Schema.JR_GB], (ZGuid)dynamicBizo[AutoJobCharge.Schema.JR_GE], (ZGuid)dynamicBizo[AutoJobCharge.Schema.JR_AC], (ZGuid)dynamicBizo[AutoJobCharge.Schema.JR_OH_CostAccount]);
					}
				}
			}

			void SetDefaultValueForProperty()
			{
				JobPK = ZGuid.Empty;
				BranchPK = ZGuid.Empty;
				DepartmentPK = ZGuid.Empty;
				ChargeCodePK = ZGuid.Empty;
				CreditorPK = ZGuid.Empty;
				ChargeCodePKCollection = new HashSet<ZGuid>();
				CreditorPKCollection = new HashSet<ZGuid>();
			}

			void SetVarianceKey(ZGuid jobPK, ZGuid branchPK, ZGuid departmentPK, ZGuid chargeCodePK, ZGuid creditorPK)
			{
				ChargePK = ZGuid.Empty;
				JobPK = jobPK;
				BranchPK = branchPK;
				DepartmentPK = departmentPK;
				if (IsVarianceByJob)
				{
					ChargeCodePK = ZGuid.Empty;
					CreditorPK = ZGuid.Empty;
				}
				else if (IsVarianceByCreditor)
				{
					ChargeCodePK = ZGuid.Empty;
					CreditorPK = creditorPK;
				}
				else
				{
					ChargeCodePK = chargeCodePK;
					CreditorPK = ZGuid.Empty;
				}
				ChargeCodePKCollection = new HashSet<ZGuid> { chargeCodePK };
				CreditorPKCollection = new HashSet<ZGuid> { creditorPK };
			}

			bool IsVarianceByJob => Factory.VarianceByJob(Company);

			bool IsVarianceByImportedCharge => Factory.VarianceByImportedCharge(Company);

			bool IsVarianceByCreditor => Factory.VarianceByCreditor(Company);

			BusinessObjectFactory Factory { get; }
			GlbCompany Company { get; }

			public override bool Equals(Object obj)
			{
				if (obj == null)
				{
					return false;
				}
				var castedVal = (CostVarianceKey)obj;
				return castedVal.ChargePK == ChargePK && castedVal.JobPK == JobPK && castedVal.BranchPK == BranchPK && castedVal.DepartmentPK == DepartmentPK && castedVal.ChargeCodePK == ChargeCodePK && castedVal.CreditorPK == CreditorPK;
			}

			public static bool operator ==(CostVarianceKey x1, CostVarianceKey x2)
			{
				return Equals(x1, x2);
			}

			public static bool operator !=(CostVarianceKey x1, CostVarianceKey x2)
			{
				return !Equals(x1, x2);
			}

			public override int GetHashCode()
			{
				return ChargePK.GetHashCode() ^ JobPK.GetHashCode() ^ BranchPK.GetHashCode() ^ DepartmentPK.GetHashCode() ^ ChargeCodePK.GetHashCode() ^ CreditorPK.GetHashCode();
			}

			public bool IsValid
			{
				get
				{
					bool isValid = ChargePK.IsValid;
					if (!isValid)
					{
						if (IsVarianceByJob)
						{
							isValid = JobPK.IsValid && BranchPK.IsValid && DepartmentPK.IsValid;
						}
						else if (IsVarianceByCreditor)
						{
							isValid = JobPK.IsValid && BranchPK.IsValid && DepartmentPK.IsValid && CreditorPK.IsValid;
						}
						else
						{
							isValid = JobPK.IsValid && BranchPK.IsValid && DepartmentPK.IsValid && ChargeCodePK.IsValid;
						}
					}
					return isValid;
				}
			}

			public ZGuid JobPK { get; private set; }
			public ZGuid BranchPK { get; private set; }
			public ZGuid DepartmentPK { get; private set; }
			public ZGuid ChargeCodePK { get; private set; }
			public ZGuid CreditorPK { get; private set; }
			public ZGuid ChargePK { get; private set; }
			HashSet<ZGuid> ChargeCodePKCollection { get; set; }
			HashSet<ZGuid> CreditorPKCollection { get; set; }

			public static Dictionary<CostVarianceKey, ZDecimal> LoadLineUnpostedCostsFromDB(HashSet<CostVarianceKey> keysToGetFromDB, ZGuid ah_OH)
			{
				var lineUnpostedCosts = new Dictionary<CostVarianceKey, ZDecimal>();
				ZString sqlText;
				if (keysToGetFromDB.Count > 0)
				{
					var parameters = new ZSqlParameterCollection();
					parameters.Add(ZSqlParameter.New("@ACRLineType", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType));

					var chargePKs = keysToGetFromDB.Where(x => x.ChargePK.IsValid).Select(x => x.ChargePK).ToList();

					if (chargePKs.Count > 0)
					{
						parameters.Add(ZSqlParameter.New("@ChargePKTable", chargePKs, JobChargeSchema.JR_JH, true));
						sqlText = SQLWithoutGroup;
						AddLineUnpostedCostsFromDB(lineUnpostedCosts, keysToGetFromDB, ah_OH, sqlText, parameters);
					}

					var jobPKs = new HashSet<ZGuid>();
					var branchPKs = new HashSet<ZGuid>();
					var departmentPKs = new HashSet<ZGuid>();
					var chargeCodePKsForSQL = new HashSet<ZGuid>();
					var creditorPKsForSQL = new HashSet<ZGuid>();

					foreach (var key in keysToGetFromDB.Where(x => x.JobPK.IsValid))
					{
						jobPKs.Add(key.JobPK);
						branchPKs.Add(key.BranchPK);
						departmentPKs.Add(key.DepartmentPK);
						chargeCodePKsForSQL.UnionWith(key.ChargeCodePKCollection);
						creditorPKsForSQL.UnionWith(key.CreditorPKCollection);
					}

					if (jobPKs.Count > 0)
					{
						sqlText = SQLWithGroupByJobAndChargeCode;
						var sqlTextToAppend = string.Empty;
						var isVarianceByCreditor = false;
						parameters.Add(ZSqlParameter.New("@JobPKTable", jobPKs.ToList(), JobChargeSchema.JR_JH, true));
						parameters.Add(ZSqlParameter.New("@BranchPKTable", branchPKs.ToList(), JobChargeSchema.JR_GB, true));
						parameters.Add(ZSqlParameter.New("@DepartmentPKTable", departmentPKs.ToList(), JobChargeSchema.JR_GE, true));
						if (keysToGetFromDB.FirstOrDefault(x => x.JobPK.IsValid)?.IsVarianceByCreditor ?? false)
						{
							isVarianceByCreditor = true;
							parameters.Add(ZSqlParameter.New("@CreditorPKTable", creditorPKsForSQL.ToList(), JobChargeSchema.JR_OH_CostAccount, true));
							sqlTextToAppend = "AND JR_OH_CostAccount IN (SELECT Value FROM @CreditorPKTable)";
						}
						else
						{
							parameters.Add(ZSqlParameter.New("@ChargeCodePKTable", chargeCodePKsForSQL.ToList(), JobChargeSchema.JR_AC, true));
							sqlTextToAppend = "AND JR_AC IN (SELECT Value FROM @ChargeCodePKTable)";
						}

						if (chargePKs.Count > 0)
						{
							parameters.Add(ZSqlParameter.New("@ExcludeJobChargePKTable", chargePKs, JobChargeSchema.PK, true));
							sqlTextToAppend += " AND JR_PK NOT IN (SELECT Value FROM @ExcludeJobChargePKTable)";
						}

						AddLineUnpostedCostsFromDB(lineUnpostedCosts, keysToGetFromDB, ah_OH, sqlText, parameters, isVarianceByCreditor, sqlTextToAppend);
					}
				}

				return lineUnpostedCosts;
			}

			static void AddLineUnpostedCostsFromDB(Dictionary<CostVarianceKey, ZDecimal> lineUnpostedCosts, HashSet<CostVarianceKey> keysToGetFromDB, ZGuid ah_OH, string sqlText, ZSqlParameterCollection parameters, bool isVarianceByCreditor = false, string sqlToAppend = "")
			{
				var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
				var innerWhereBuilder = new ZStringBuilder();

				innerWhereBuilder.Append(sqlToAppend);
				if (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) && ah_OH.IsValid && !isVarianceByCreditor)
				{
					innerWhereBuilder.AppendFormat("AND (JR_OH_CostAccount = '{0}' OR JR_OH_CostAccount IS NULL)", ah_OH.ToString());
				}

				sqlText = ZString.Format(sqlText, innerWhereBuilder.ToStringWithNewLineBetweenAppends());

				collection.Load(sqlText, parameters);

				foreach (DynamicBusinessObject lineUnpostedCost in collection)
				{
					var lineUnpostedCostKey = new CostVarianceKey(lineUnpostedCost);
					var key = keysToGetFromDB.FirstOrDefault(x => x == lineUnpostedCostKey);
					if (key != null)
					{
						if (lineUnpostedCosts.ContainsKey(key))
						{
							lineUnpostedCosts[key] += (ZDecimal)lineUnpostedCost["Sum_LocalCostAmt"];
						}
						else
						{
							lineUnpostedCosts.Add(key, (ZDecimal)lineUnpostedCost["Sum_LocalCostAmt"]);
						}
					}
				}
			}

			#region sql to get charge amount in DB

			static string SQLWithoutGroup
			{
				get
				{
					return @"SELECT JR_LocalCostAmt AS Sum_LocalCostAmt, JR_PK
							FROM dbo.JobCharge 
							WHERE (JR_AL_APLine IS NULL OR EXISTS (SELECT 1 FROM dbo.AccTransactionLines WHERE AL_LineType = @ACRLineType AND AL_PK = JR_AL_APLine))
							AND JR_PK IN (SELECT Value FROM @ChargePKTable)
							{0}";
				}
			}

			static string SQLWithGroupByJobAndChargeCode
			{
				get
				{
					return @"SELECT SUM(ISNULL(JR_LocalCostAmt, 0)) AS Sum_LocalCostAmt, JR_JH, JR_GB, JR_GE, JR_AC, JR_OH_CostAccount
							FROM dbo.JobCharge 
							WHERE (JR_AL_APLine IS NULL OR EXISTS (SELECT 1 FROM dbo.AccTransactionLines WHERE AL_LineType = @ACRLineType AND AL_PK = JR_AL_APLine))
							AND JR_JH IN (SELECT Value FROM @JobPKTable)
							AND JR_GB IN (SELECT Value FROM @BranchPKTable)
							AND JR_GE IN (SELECT Value FROM @DepartmentPKTable)
							{0}
							GROUP BY JR_JH, JR_GB, JR_GE, JR_AC, JR_OH_CostAccount";
				}
			}

			#endregion
		}
	}
}
