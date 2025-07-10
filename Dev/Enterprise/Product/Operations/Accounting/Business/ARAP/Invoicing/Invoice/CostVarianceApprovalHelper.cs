using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalExtension;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class CostVarianceApprovalHelper
	{
		public CostVarianceApprovalHelper(APInvoice invoice)
		{
			Argument.NotNull(invoice, nameof(invoice));
			APInvoice = invoice;
		}

		public CostVarianceApprovalHelper(APInvoiceConsolCosting consolCosting) : this(consolCosting.ParentAPInvoice as APInvoice)
		{
			Argument.NotNull(consolCosting, nameof(consolCosting));
			ConsolCosting = consolCosting;
		}

		readonly APInvoice APInvoice;
		readonly APInvoiceConsolCosting ConsolCosting;

		BusinessObjectFactory Factory => ConsolCosting?.Factory ?? APInvoice.Factory;

		GlbCompany Company => ConsolCosting?.ConsolCosts.OfType<JobConsolCost>().FirstOrDefault()?.Company ?? APInvoice.Company ?? GlbCompany.CurrentCompany;

		bool IsVarianceByCreditor => Factory.VarianceByCreditor(Company);

		CostVarianceApproval CostVarianceApproval => Factory.GetCostVarianceApproval(Company);

#if DEBUG
		public void ClearCachedCostVarianceApproval_ForTestOnly() => Factory.ClearCachedCostVarianceApproval(Company);
#endif

		ZBool ShouldExcludedFromCalculation => AccountingConfigurationRegistry.Instance.CostVarianceNoApprovalRequired.Value && Factory.VarianceByImportedCharge(Company);

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		Dictionary<CostVarianceKey, ZDecimal> GetLocalCostAmountGroups()
		{
			var apInvoiceLines = APInvoice.Lines.OfType<APInvoiceLine>();
			var apInvoiceLinesExcludedPKs = Enumerable.Empty<ZGuid>();
			if (ShouldExcludedFromCalculation)
			{
				apInvoiceLinesExcludedPKs = apInvoiceLines.Where(x => x.IsPopulatedFromImportedJobCharge &&
													x.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
													x.AL_RX_NKTransactionCurrency == x.OriginalJobCharge.JR_RX_NKCostCurrency &&
													x.AL_OSExTaxAmount == x.OriginalJobCharge.JR_OSCostAmt).Select(x => x.PK).ToHashSet();
			}
			var listCostAmounts = apInvoiceLines.Where(line => !apInvoiceLinesExcludedPKs.Contains(line.PK) && (ConsolCosting == null || (line.ApportionmentChargeImportedFrom == null && line.AL_JH.IsValid))).
									Select(x => new { Key = new CostVarianceKey((APInvoiceLine)x), Value = x.AL_LocalExTaxAmount }).
									Concat(ApportionSplitCharges.Select(x => new { Key = new CostVarianceKey(x), Value = x.JR_LocalCostAmt }));

			return (from p in listCostAmounts
					where p.Key.IsValid
					group p by p.Key into g
					select new
					{
						Key = new CostVarianceKey(g.Select(x => x.Key)),
						Value = (ZDecimal)g.Sum(x => x.Value)
					}).ToDictionary(x => x.Key, x => x.Value);
		}

		internal List<ApportionSplitCharge> ApportionSplitCharges
		{
			get
			{
				if (ConsolCosting != null)
				{
					var apportionSplitCharge = ConsolCosting.ConsolCosts.Cast<JobConsolCost>()
					.SelectMany(a => a.ApportionmentCharges)
					.Cast<ApportionSplitCharge>()
					.Where(b => b.JR_IsUsedForApportionment);

					if (ShouldExcludedFromCalculation)
					{
						var apportionSplitChargeExcludedPKs = apportionSplitCharge.Where(x => x.IsParentConsolCostImported &&
														x.JR_CostCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
														x.JR_CostCurrency == x.RelatedApportionChargeFromDB.JR_RX_NKCostCurrency &&
														x.JR_OSCostAmt == x.RelatedApportionChargeFromDB.JR_OSCostAmt).Select(x => x.PK).ToHashSet();
						apportionSplitCharge = apportionSplitCharge.Where(x => !apportionSplitChargeExcludedPKs.Contains(x.PK));
					}

					return apportionSplitCharge.ToList();
				}

				return new List<ApportionSplitCharge>();
			}
		}

		readonly Dictionary<CostVarianceKey, CostVarianceApprovalAuthorisationRequirement> AuthorisationRequirements = new Dictionary<CostVarianceKey, CostVarianceApprovalAuthorisationRequirement>();
		Dictionary<CostVarianceKey, ZDecimal> UnpostedCosts = new Dictionary<CostVarianceKey, ZDecimal>();
		readonly Dictionary<CostVarianceKey, ZDecimal> PostedUnpostedDifferences = new Dictionary<CostVarianceKey, ZDecimal>();
		CostVarianceApprovalAuthorisationRequirement TotalAuthorisationRequirement;

		internal bool AutoTickFinalFlag
		{
			get { return CostVarianceApproval.AutoTickFinalFlag; }
		}

		internal Dictionary<CostVarianceKey, CostVarianceApprovalAuthorisationRequirement> GetAuthorisationRequirements()
		{
			return AuthorisationRequirements;
		}

		internal Dictionary<CostVarianceKey, ZDecimal> GetPostedUnpostedDifferences()
		{
			return PostedUnpostedDifferences;
		}

		internal CostVarianceApprovalAuthorisationRequirement GetTotalAuthorisationRequirement()
		{
			return TotalAuthorisationRequirement;
		}

		internal void ClearLineAuthorisationCache()
		{
			UnpostedCosts.Clear();
			ClearAuthorisationDerivedValuesCache();
		}

		void ClearAuthorisationDerivedValuesCache()
		{
			AuthorisationRequirements.Clear();
			PostedUnpostedDifferences.Clear();
			TotalAuthorisationRequirement = null;
		}

		internal void CalculateAuthorisationByLines()
		{
			if (APInvoice == null)
			{
				return;
			}

			if (!APInvoice.HasContext(BusinessContext.CASS) && CostVarianceApproval.AuthorisationRequirements.Any())
			{
				var localCostAmountGroups = GetLocalCostAmountGroups();
				if (localCostAmountGroups.Any())
				{
					if (!APInvoice.ClearLineAuthorisationCacheSuspender.IsSuspended)
					{
						ClearLineAuthorisationCache();
					}
					else
					{
						ClearAuthorisationDerivedValuesCache();
					}

					SetCostVariancesForLines(localCostAmountGroups);
					SetTotalAuthorisationRequirement(PostedUnpostedDifferences, UnpostedCosts);
				}
				else
				{
					ClearLineAuthorisationCache();
				}
			}
			else
			{
				ClearLineAuthorisationCache();
			}
		}

		void SetCostVariancesForLines(Dictionary<CostVarianceKey, ZDecimal> localCostAmountGroups)
		{
			var keysToGetFromDB = new HashSet<CostVarianceKey>();
			var lineUpostedCosts_ForCurrentLines = new Dictionary<CostVarianceKey, ZDecimal>();
			foreach (var groupedSumPair in localCostAmountGroups)
			{
				var key = groupedSumPair.Key;
				ZDecimal lineUpostedAmountValue;
				if (UnpostedCosts.TryGetValue(key, out lineUpostedAmountValue))
				{
					lineUpostedCosts_ForCurrentLines[key] = lineUpostedAmountValue;
				}
				else
				{
					keysToGetFromDB.Add(key);
				}
			}
			UnpostedCosts = lineUpostedCosts_ForCurrentLines;

			if (keysToGetFromDB.Count != 0)
			{
				RefreshLineUnpostedCosts(keysToGetFromDB);
				keysToGetFromDB = null;
			}

			foreach (var lineUnpostedCostsPair in UnpostedCosts)
			{
				var key = lineUnpostedCostsPair.Key;
				PostedUnpostedDifferences.Add(key, localCostAmountGroups[key] - lineUnpostedCostsPair.Value);
			}
		}

		void SetTotalAuthorisationRequirement(Dictionary<CostVarianceKey, ZDecimal> linePostedUpostedDifference, Dictionary<CostVarianceKey, ZDecimal> lineUnpostedCosts)
		{
			var totalPositiveVariance = 0m;
			var totalNegativeVariance = 0m;

			foreach (var lineDifferencePair in linePostedUpostedDifference)
			{
				if (lineDifferencePair.Value >= 0)
				{
					totalPositiveVariance += lineDifferencePair.Value;
				}
				else
				{
					totalNegativeVariance += lineDifferencePair.Value;
				}

				ZDecimal groupedUnpostedCostSumValue;
				lineUnpostedCosts.TryGetValue(lineDifferencePair.Key, out groupedUnpostedCostSumValue);

				var resultRequirement = CostVarianceApproval.AuthorisationRequirements.GetMatchingAuthorisationRequirement(lineDifferencePair.Value, groupedUnpostedCostSumValue);

				if (resultRequirement != null)
				{
					AuthorisationRequirements.Add(lineDifferencePair.Key, (CostVarianceApprovalAuthorisationRequirement)resultRequirement.Clone(resultRequirement.CurrentFallbackLevel, resultRequirement.Factory));
				}
			}

			var resultTotalPositiveAuthorisationRequirement = CostVarianceApproval.AuthorisationRequirements.GetAuthorisationRequiredForTotal(totalPositiveVariance);
			var resultTotalNegativeAuthorisationRequirement = CostVarianceApproval.AuthorisationRequirements.GetAuthorisationRequiredForTotal(totalNegativeVariance);

			if (resultTotalPositiveAuthorisationRequirement != null &&
				(resultTotalNegativeAuthorisationRequirement == null || CompareLevelValues(resultTotalPositiveAuthorisationRequirement, resultTotalNegativeAuthorisationRequirement.AuthorisationRequirement) > 0))
			{
				TotalAuthorisationRequirement = (CostVarianceApprovalAuthorisationRequirement)resultTotalPositiveAuthorisationRequirement.Clone(resultTotalPositiveAuthorisationRequirement.CurrentFallbackLevel, resultTotalPositiveAuthorisationRequirement.Factory);
			}
			else if (resultTotalNegativeAuthorisationRequirement != null)
			{
				TotalAuthorisationRequirement = (CostVarianceApprovalAuthorisationRequirement)resultTotalNegativeAuthorisationRequirement.Clone(resultTotalNegativeAuthorisationRequirement.CurrentFallbackLevel, resultTotalNegativeAuthorisationRequirement.Factory);
			}
		}

		internal bool MonitorTotalInvoiceVariance
		{
			get
			{
				return CostVarianceApproval.AuthorisationRequirements.Cast<CostVarianceApprovalAuthorisationRequirement>().Any(item => item.MonitorTotalInvoiceVariance);
			}
		}

#if DEBUG
		internal
#endif
		void RefreshLineUnpostedCosts(HashSet<CostVarianceKey> keysToGetFromDB)
		{
#if DEBUG
			APInvoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly++;
#endif

			var lineUnpostedCostsFromDB = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, APInvoice.AH_OH);

			foreach (var lineUnpostedCost in lineUnpostedCostsFromDB)
			{
				UnpostedCosts.Add(lineUnpostedCost.Key, lineUnpostedCost.Value);
			}

			foreach (var key in keysToGetFromDB)
			{
				ZDecimal lineUnpostedCostValue;
				if (!UnpostedCosts.TryGetValue(key, out lineUnpostedCostValue))
				{
					UnpostedCosts.Add(key, ZDecimal.Zero);
				}
			}
		}

		static int CompareLevelValues(CostVarianceApprovalAuthorisationRequirement approvalRequirement, ZString maxLevel)
		{
			return Math.Sign(approvalRequirement.GetAuthorisationRequirementWeight(approvalRequirement.AuthorisationRequirement) - approvalRequirement.GetAuthorisationRequirementWeight(maxLevel));
		}

		internal CostVarianceApprovalAuthorisationRequirement GetLineAuthorisationRequirement(ApportionSplitCharge charge)
		{
			return GetLineAuthorisationObject(charge, AuthorisationRequirements);
		}

		internal CostVarianceApprovalAuthorisationRequirement GetLineAuthorisationRequirement(APInvoiceLine line)
		{
			return GetLineAuthorisationObject(line, AuthorisationRequirements);
		}

		internal TValue GetLineAuthorisationObject<TValue>(ApportionSplitCharge charge,
			Dictionary<CostVarianceKey, TValue> lineAuthorisationCollection)
		{
			var resultAuthorisationRequirement = default(TValue);

			if (lineAuthorisationCollection.Count > 0 &&
				charge.JR_JH.IsValid && charge.JR_GB.IsValid && charge.JR_GE.IsValid && charge.JR_AC.IsValid && (!IsVarianceByCreditor || charge.JR_OH_CostAccount.IsValid))
			{
				lineAuthorisationCollection.TryGetValue(new CostVarianceKey(charge), out resultAuthorisationRequirement);
			}

			return resultAuthorisationRequirement;
		}

		internal TValue GetLineAuthorisationObject<TValue>(APInvoiceLine line,
			Dictionary<CostVarianceKey, TValue> lineAuthorisationCollection)
		{
			TValue resultAuthorisationRequirement = default(TValue);

			if (line.AL_AH == APInvoice.PK && lineAuthorisationCollection.Count > 0 &&
				line.AL_JH.IsValid && line.AL_GB.IsValid && line.AL_GE.IsValid && line.AL_AC.IsValid && (!IsVarianceByCreditor || APInvoice.AH_OH.IsValid))
			{
				lineAuthorisationCollection.TryGetValue(new CostVarianceKey(line), out resultAuthorisationRequirement);
			}
			return resultAuthorisationRequirement;
		}

		internal AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore()
		{
			AmountBasedMultiLevelAuthorisationRequirement resultRequirement = null;

			CalculateAuthorisationByLines();

			var tmpCostVarianceApproval = (CostVarianceApproval)CostVarianceApproval.Clone(CostVarianceApproval.CurrentFallbackLevel, CostVarianceApproval.Factory);
			tmpCostVarianceApproval.AuthorisationRequirements.RemoveAll();
			tmpCostVarianceApproval.AuthorisationRequirements.AddRange(AuthorisationRequirements.Values);

			if (TotalAuthorisationRequirement != null)
			{
				tmpCostVarianceApproval.AuthorisationRequirements.Add(TotalAuthorisationRequirement);
			}

			tmpCostVarianceApproval.AuthorisationRequirements.Sort();

			if (tmpCostVarianceApproval.AuthorisationRequirements.Count > 0)
			{
				resultRequirement = tmpCostVarianceApproval.AuthorisationRequirements[tmpCostVarianceApproval.AuthorisationRequirements.Count - 1];
			}

			return resultRequirement;
		}

		internal AmountBasedAuthorisationRequirementCollection AuthorizationSettingsCollection
		{
			get
			{
				return CostVarianceApproval.AuthorisationRequirements;
			}
		}

		internal AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore(ZDecimal localAmount)
		{
			CostVarianceApprovalAuthorisationRequirement result = null;

			if (localAmount != 0)
			{
				var registryValue = AuthorizationSettingsCollection as CostVarianceApprovalAuthorisationRequirementCollection;
				if (registryValue != null)
				{
					result = registryValue.GetElementsBySign(localAmount).GetAuthorisationRequired(localAmount);
				}
			}

			return result;
		}
	}
}