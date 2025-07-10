using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public static class InvoiceLineToChargeMatchingGenerator
	{
		public static MatchingResult GetMatchingResult(UniversalTransactionWrapper universalTransaction)
		{
			var jobPKs = new HashSet<ZGuid>();
			var currencies = new HashSet<ZString>();
			var lineCollection = new List<UniversalTransactionLineWrapper>();

			if (universalTransaction.CreditorOrgHeader == null || !universalTransaction.Lines.Any())
			{
				return new MatchingResult();
			}

			foreach (UniversalTransactionLineWrapper line in universalTransaction.Lines)
			{
				if (line.JobHeader == null)
				{
					if (line.JobConsol == null)
					{
						continue;
					}
					else
					{
						if (line.JobConsol.CostSupporter != null)
						{
							foreach (var shipment in line.JobConsol.CostSupporter.ShipmentsList)
							{
								var jobPk = shipment.InvoicingSupporter?.Job.PK ?? ZGuid.Empty;
								if (!jobPk.IsEmpty)
								{
									jobPKs.Add(jobPk);
								}
							}
						}
					}
				}
				else
				{
					jobPKs.Add(line.JobHeader.PK);
				}

				currencies.Add(line.OSCurrency);

				lineCollection.Add(line);
			}

			var charges = GetRecordedCharges(jobPKs.ToArray(), universalTransaction.CreditorOrgHeader.PK, currencies.ToArray());
			var strategies = GetMatchingStrategies(charges, lineCollection, universalTransaction.IsCrossLedger);

			var lineGroupsWithSuggestions = new List<InvoiceLineGroup>();
			foreach (var matchingProcessor in GetMatchingProcessors())
			{
				lineGroupsWithSuggestions.AddRange(matchingProcessor.Process(strategies, universalTransaction.IsCrossLedger));
			}

			return new MatchingResult(lineGroupsWithSuggestions, universalTransaction.Lines.Cast<UniversalTransactionLineWrapper>().ToList());
		}

		static MatchingProcessor[] GetMatchingProcessors()
		{
			return new MatchingProcessor[]
				{
					new JobLevelMatchingProcessor(),
					new ConsolLevelMatchingProcessor()
				};
		}

		static IEnumerable<MatchingStrategy> GetMatchingStrategies(IEnumerable<RecordedCharges> charges, List<UniversalTransactionLineWrapper> lineCollection, bool isCrossLedger)
		{
			var lineGroupingByJobAndCharge = new LineGroupingByJobAndChargeCodeProvider(lineCollection);
			var lineGroupingByJob = new LineGroupingByJobProvider(lineCollection);

			var chargeGroupingByJobAndCharge = new ChargeGroupingByJobAndChargeCodeProvider(charges);
			var chargeGroupingByJob = new ChargeGroupingByJobProvider(charges);

			return new MatchingStrategy[]
			{
				new MatchWithJobAndChargeCodeMatchingStrategy(lineGroupingByJobAndCharge, chargeGroupingByJobAndCharge, isCrossLedger),
				new MatchWithJobMatchingStrategy(lineGroupingByJobAndCharge, chargeGroupingByJob, isCrossLedger),
				new MatchWithJobMatchingStrategy(lineGroupingByJob, chargeGroupingByJob, isCrossLedger),
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static IEnumerable<RecordedCharges> GetRecordedCharges(ZGuid[] jobPKs, ZGuid orgPK, ZString[] currencies)
		{
			var sql = @"
SELECT
	JR_PK
	, JR_JH
	, JR_OH_CostAccount
	, JR_OSCostAmt
	, JR_OSCostGSTAmt
	, JR_AT_CostGSTRate
	, JR_CostTaxDate
	, JR_IsCostTaxAmountOverridden
	, JR_RX_NKCostCurrency
	, JR_E6
	, E6_OSCostAmount
	, E6_OSGSTAmount
	, E6_AT_TaxRate
	, E6_TaxDate
	, E6_IsTaxAmountOverridden
	, AC_PK
	, AC_Code
	, OH_Code
	, JH_GS_NKRepOps
	, AL_SystemCreateUser
	, OrgType
	, JH_JobNum
	, JK_UniqueConsignRef
FROM
	GetAccrualsForJobAndRelatedOrg(@JobList, @CurrencyList, @OrgPK, @CompanyPK)
OPTION (RECOMPILE)";

			DbCommand cmd;
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			using (var jobTable = new DataTable())
			using (var currencyTable = new DataTable())
			{
				jobTable.Locale = CultureInfo.InvariantCulture;
				jobTable.Columns.Add((NoResString)"Value", typeof(Guid));
				foreach (var jobPK in jobPKs)
				{
					jobTable.Rows.Add(jobPK.ToGuid());
				}

				currencyTable.Locale = CultureInfo.InvariantCulture;
				currencyTable.Columns.Add((NoResString)"Value", typeof(string));
				foreach (var currency in currencies)
				{
					currencyTable.Rows.Add(currency);
				}

				cmd = Db.Connection.Command(sql); // Calling DB function, which cannot be loaded through BusinessObjectFactory
				cmd.AddTableValuedParameter("@JobList", "dbo.TVP_uniqueidentifier", jobTable); //for now the table will include only one job, as we are reading the first line only. But later when we will read all the lines then TVP will be useful
				cmd.AddTableValuedParameter("@CurrencyList", "dbo.TVP_char_3", currencyTable); //for now the table will include only one currency, as we are reading the first line only. But later when we will read all the lines then TVP will be useful
				cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
				cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			}

			var recordedCharges = new List<RecordedCharges>();
			var factory = new BusinessObjectFactory();

			var argsForPopulatingTotalChargeCostAmount = new Dictionary<ZGuid, (ZDecimal costAmount, ZString currency, ZGuid taxRatePK, ZDate taxDate, ZGuid chargeCodePK, ZGuid jr_E6)>();
			var argsForPopulatingConsolCostAmount = new Dictionary<ZGuid, (ZDecimal costAmount, ZGuid taxRatePK, ZDate taxDate, ZString currency)>();

			using (var reader = cmd.ExecuteReader()) // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				while (reader.Read())
				{
					recordedCharges.Add(GetRecordedChargeFromReader(reader, argsForPopulatingTotalChargeCostAmount, argsForPopulatingConsolCostAmount));
				}
			}

			foreach (var charge in recordedCharges)
			{
				if (argsForPopulatingTotalChargeCostAmount.Keys.Contains(charge.JR_PK))
				{
					var chargeArgs = argsForPopulatingTotalChargeCostAmount[charge.JR_PK];
					charge.TotalCostAmount += BaseCharge.JobChargeOSGSTCalculationStrategyStaticHelper.CalculateOsCostTaxAmount
						(factory, chargeArgs.costAmount, chargeArgs.currency, chargeArgs.taxRatePK, chargeArgs.taxDate, chargeArgs.chargeCodePK, chargeArgs.jr_E6, countryCode, companyPK);

					var consolArgs = argsForPopulatingConsolCostAmount[charge.JR_PK];
					charge.ConsolCostAmount += JobConsolCost.OSGSTCalculationStrategyStaticHelper.CalculateTaxAmountCore
						(consolArgs.costAmount, JobConsolCost.OSGSTCalculationStrategyStaticHelper.GetTaxRateAmount(factory, consolArgs.taxRatePK, consolArgs.taxDate),
						JobConsolCost.OSGSTCalculationStrategyStaticHelper.GetExtraTaxRateAmount(factory, consolArgs.taxRatePK, consolArgs.taxDate),
						factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, consolArgs.currency), factory, consolArgs.taxRatePK);
				}
			}

			return recordedCharges;
		}

		static RecordedCharges GetRecordedChargeFromReader(IDataReader reader,
			Dictionary<ZGuid, (ZDecimal costAmount, ZString currency, ZGuid taxRatePK, ZDate taxDate, ZGuid chargeCodePK, ZGuid jr_E6)> argsForPopulatingTotalChargeCostAmount,
			Dictionary<ZGuid, (ZDecimal costAmount, ZGuid taxRatePK, ZDate taxDate, ZString currency)> argsForPopulatingConsolCostAmount)
		{
			var result = CreateNewRecordedCharges(reader);

			var chargeCodePK = reader["AC_PK"] != DBNull.Value ? (Guid)reader["AC_PK"] : Guid.Empty;
			var chargeTaxRatePK = reader["JR_AT_CostGSTRate"] != DBNull.Value ? (Guid)reader["JR_AT_CostGSTRate"] : Guid.Empty;
			var chargeTaxDate = reader["JR_CostTaxDate"] != DBNull.Value ? (ZDate)(DateTime)reader["JR_CostTaxDate"] : ZDate.Empty;
			var consolTaxRatePK = reader["E6_AT_TaxRate"] != DBNull.Value ? (Guid)reader["E6_AT_TaxRate"] : Guid.Empty;
			var consolTaxDate = reader["E6_TaxDate"] != DBNull.Value ? (ZDate)(DateTime)reader["E6_TaxDate"] : ZDate.Empty;
			var chargeCostAmount = reader["JR_OSCostAmt"] != DBNull.Value ? (decimal)reader["JR_OSCostAmt"] : decimal.Zero;
			var consolCostAmount = reader["E6_OSCostAmount"] != DBNull.Value ? (decimal)reader["E6_OSCostAmount"] : decimal.Zero;
			var chargeIsTaxOverridden = reader["JR_IsCostTaxAmountOverridden"] != DBNull.Value && (bool)reader["JR_IsCostTaxAmountOverridden"];
			var consolIsTaxOverridden = reader["E6_IsTaxAmountOverridden"] != DBNull.Value && (bool)reader["E6_IsTaxAmountOverridden"];
			var chargeGST = reader["JR_OSCostGSTAmt"] != DBNull.Value ? (ZDecimal)(decimal)reader["JR_OSCostGSTAmt"] : ZDecimal.Zero;
			var consolGST = reader["E6_OSGSTAmount"] != DBNull.Value ? (ZDecimal)(decimal)reader["E6_OSGSTAmount"] : ZDecimal.Zero;

			result.TotalCostAmount = chargeCostAmount;
			if (chargeIsTaxOverridden)
			{
				result.TotalCostAmount += chargeGST;
			}
			else
			{
				argsForPopulatingTotalChargeCostAmount[result.JR_PK] = (chargeCostAmount, result.Currency, chargeTaxRatePK, chargeTaxDate, chargeCodePK, result.E6_PK);
			}

			result.ConsolCostAmount = consolCostAmount;
			if (consolIsTaxOverridden)
			{
				result.ConsolCostAmount += consolGST;
			}
			else
			{
				argsForPopulatingConsolCostAmount[result.JR_PK] = (consolCostAmount, consolTaxRatePK, consolTaxDate, result.Currency);
			}

			return result;
		}

		static RecordedCharges CreateNewRecordedCharges(IDataReader reader)
		{
			return new RecordedCharges
			{
				JR_PK = reader["JR_PK"] != DBNull.Value ? (Guid)reader["JR_PK"] : Guid.Empty,
				JR_JH = reader["JR_JH"] != DBNull.Value ? (Guid)reader["JR_JH"] : Guid.Empty,
				OH_PK = reader["JR_OH_CostAccount"] != DBNull.Value ? (Guid)reader["JR_OH_CostAccount"] : Guid.Empty,
				Currency = reader["JR_RX_NKCostCurrency"] != DBNull.Value ? (string)reader["JR_RX_NKCostCurrency"] : string.Empty,
				E6_PK = reader["JR_E6"] != DBNull.Value ? (Guid)reader["JR_E6"] : Guid.Empty,
				AC_Code = reader["AC_Code"] != DBNull.Value ? (string)reader["AC_Code"] : string.Empty,
				OH_Code = reader["OH_Code"] != DBNull.Value ? (string)reader["OH_Code"] : string.Empty,
				JH_GS_NKRepOps = reader["JH_GS_NKRepOps"] != DBNull.Value ? (string)reader["JH_GS_NKRepOps"] : string.Empty,
				AL_SystemCreateUser = reader["AL_SystemCreateUser"] != DBNull.Value ? (string)reader["AL_SystemCreateUser"] : string.Empty,
				OrgType = reader["OrgType"] != DBNull.Value ? (OrgType)Enum.Parse(typeof(OrgType), (string)reader["OrgType"]) : default(OrgType),
				JH_JobNum = reader["JH_JobNum"] != DBNull.Value ? (string)reader["JH_JobNum"] : string.Empty,
				JK_UniqueConsignRef = reader["JK_UniqueConsignRef"] != DBNull.Value ? (string)reader["JK_UniqueConsignRef"] : string.Empty
			};
		}
	}
}
