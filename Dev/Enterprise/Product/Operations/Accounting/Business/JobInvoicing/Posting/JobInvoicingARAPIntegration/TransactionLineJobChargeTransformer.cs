#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCriticalValidationDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Updates Job Invoicing screen when an AP transaction is being posted
	/// </summary>
	public class TransactionLineJobChargeTransformer : BaseIntegrationTransformer, IService
	{
		public TransactionLineJobChargeTransformer(BusinessObjectFactory factory)
			: base(factory)
		{
			if (AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.Value)
			{
				factory.ServiceContainer.AddService<IInvoicingLineCacheUtility>(new InvoicingLineCacheUtility());
			}
		}

		ConsolCostListWithStrategy ConsolCostsToSearch
		{
			get { return consolCostsToSearch ?? (consolCostsToSearch = new ConsolCostListWithStrategy(ConsolCostStrategy.ConsolCostCalculationStrategyWithoutCalculations)); }
		}
		ConsolCostListWithStrategy consolCostsToSearch;

#if DEBUG
		public
#endif
		ChargesByPK ChargesToSearch
		{
			get
			{
				return chargesToSearch ?? (chargesToSearch = new ChargesByPK(OutOfSyncChecker));
			}
		}
		ChargesByPK chargesToSearch;

		NegativeReaccrualChecker NegativeReAccrualChecker => negativeReAccrualChecker ?? (negativeReAccrualChecker = new NegativeReaccrualChecker(this));
		NegativeReaccrualChecker negativeReAccrualChecker;

#if DEBUG
		public
#endif
		CarryForwardConsolCostCreator CarryForwardCostCreator => carryForwardConsolCostCreator ?? (carryForwardConsolCostCreator = new CarryForwardConsolCostCreator(this));
		CarryForwardConsolCostCreator carryForwardConsolCostCreator;

		ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker OutOfSyncChecker => outOfSyncChecker ?? (outOfSyncChecker = new ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker(this));
		ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker outOfSyncChecker;

		public sealed class ChargesByPK : IDisposable
		{
			public ChargesByPK() : this(null)
			{
			}

			public ChargesByPK(ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker outOfSyncChecker)
			{
				dictionary = new Dictionary<ZGuid, Charge>();
				this.outOfSyncChecker = outOfSyncChecker;
			}

			readonly Dictionary<ZGuid, Charge> dictionary;
			readonly ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker outOfSyncChecker;

			public void AddChargeRange(IEnumerable<Charge> charges)
			{
				if (charges != null)
				{
					foreach (var charge in charges)
					{
						AddCharge(charge);
					}
				}
			}

			public void AddCharge(Charge charge)
			{
				if (!dictionary.ContainsKey(charge.PK))
				{
					dictionary.Add(charge.PK, charge);
					charge.SetContext(BusinessContext.ReportDeletingCharges);
					charge.BeforeSuccessfulDeleting += RemoveOnChargeDelete;
				}
			}

			public void RemoveCharge(Charge charge)
			{
				if (dictionary.Remove(charge.PK))
				{
					outOfSyncChecker?.CollectInfoWhenChargeRemovedFromChargesToSearch(charge);
					charge.RemoveContext(BusinessContext.ReportDeletingCharges);
					charge.BeforeSuccessfulDeleting -= RemoveOnChargeDelete;
				}
			}

			public void RemoveChargeByPK(ZGuid pk)
			{
				Charge charge = null;
				if (dictionary.TryGetValue(pk, out charge))
				{
					RemoveCharge(charge);
				}
			}

			public IEnumerable<ZGuid> PKs
			{
				get { return dictionary.Keys; }
			}

			public IEnumerable<Charge> Charges
			{
				get { return dictionary.Values; }
			}

			void RemoveOnChargeDelete(object sender, EventArgs args)
			{
				var charge = (Charge)sender;
				RemoveCharge(charge);
			}

			public void Dispose()
			{
				dictionary.Values.ToArray().ForEach(x => RemoveCharge(x));
			}
		}

		sealed class NegativeReaccrualChecker : IDisposable
		{
			public NegativeReaccrualChecker(TransactionLineJobChargeTransformer transformer)
			{
				Transformer = transformer;
				InvoiceLinesCheckedForNegativeReaccrual = new Dictionary<ZGuid, bool>();
			}
			TransactionLineJobChargeTransformer Transformer;
			Dictionary<ZGuid, bool> InvoiceLinesCheckedForNegativeReaccrual;

			public bool CanApplyNegativeReaccrual(InvoicingLineBase invoiceLine)
			{
				if (!InvoiceLinesCheckedForNegativeReaccrual.TryGetValue(invoiceLine.PK, out var isNegativeReaccrualEnabled))
				{
					isNegativeReaccrualEnabled = IsNegativeAccrualBehaviorsEnabled &&
							 ((invoiceLine.IsPopulatedFromImportedJobCharge && ((ZDecimal)invoiceLine.OriginalJobCharge.JR_OSCostAmtInfo.OriginalValue) < 0 && invoiceLine.AL_LineAmount > 0) ||
							 (invoiceLine.InvoiceBase.Lines.Cast<InvoicingLineBase>().Where(x =>
									x.AL_JH == invoiceLine.AL_JH &&
									x.AL_AC == invoiceLine.AL_AC &&
									x.AL_GB == invoiceLine.AL_GB &&
									x.AL_GE == invoiceLine.AL_GE &&
									x.AL_Calc_RelatedJobPK == invoiceLine.AL_Calc_RelatedJobPK).All(x => x.AL_LineAmount > 0) &&
									Transformer.ChargesToSearch.Charges.Where(x =>
									x.JR_JH == invoiceLine.AL_JH &&
									x.JR_AC == invoiceLine.AL_AC &&
									x.JR_GB == invoiceLine.AL_GB &&
									x.JR_GE == invoiceLine.AL_GE &&
									x.RelatedJobID == invoiceLine.AL_Calc_RelatedJobPK &&
									(invoiceLine.ApportionmentChargeImportedFrom != null || x.JR_E6.IsEmpty)).All(x => x.JR_OSCostAmt <= 0)));

					InvoiceLinesCheckedForNegativeReaccrual.Add(invoiceLine.PK, isNegativeReaccrualEnabled);
				}
				return isNegativeReaccrualEnabled;
			}

			public void Dispose()
			{
				InvoiceLinesCheckedForNegativeReaccrual?.Clear();
				Transformer = null;
				InvoiceLinesCheckedForNegativeReaccrual = null;
			}
		}

#if DEBUG
		public
#endif
		sealed class CarryForwardConsolCostCreator : IDisposable
		{
			public CarryForwardConsolCostCreator(TransactionLineJobChargeTransformer transformer)
			{
				this.transformer = transformer;
				carryForwardApportionChargePKs = new Dictionary<ZGuid, (ZGuid ChargePK, ZGuid ConsolPK, ZString ParentTableCode)>();
				apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount = new RedundantApportionChargeCollection();
			}
			TransactionLineJobChargeTransformer transformer;
			Dictionary<ZGuid, (ZGuid ChargePK, ZGuid ConsolPK, ZString ParentTableCode)> carryForwardApportionChargePKs;
			readonly RedundantApportionChargeCollection apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount;
			public List<ZGuid> apportionChargePks => carryForwardApportionChargePKs.Keys.ToList();

			public void AddTocarryForwardApportionChargePKList(Charge charge, ZGuid consolPK, ZString parentTableCode)
			{
				if (!carryForwardApportionChargePKs.ContainsKey(charge.PK))
				{
					carryForwardApportionChargePKs.Add(charge.PK, (charge.PK, consolPK, parentTableCode));
					transformer.OutOfSyncChecker.CollectInfoWhenChargeAddedToCarryForwardApportionChargePKs(charge);
				}
			}
			public bool IsItACarryForwardApportionCharge(Charge charge) => carryForwardApportionChargePKs.ContainsKey(charge.PK);

			void CreateCarryForwardConsolCost()
			{
				if (carryForwardApportionChargePKs.Any())
				{
					if (transformer.ChargesToSearch.Charges.Any())
					{
						var chargePKsGroupedByConsolPK = carryForwardApportionChargePKs.Values.GroupBy(x => x.ConsolPK);
						var factory = transformer.ChargesToSearch.Charges.First().Factory;
						foreach (var group in chargePKsGroupedByConsolPK)
						{
							var apportionChargePKsThatBelongToSameConsol = group.Select(x => x.ChargePK).ToHashSet();
							ReorganizeConsolCosts(factory, group.First().ConsolPK, group.First().ParentTableCode, apportionChargePKsThatBelongToSameConsol);
						}
					}
					else
					{
						var service = CriticalValidationInfoCollectorService.GetOrCreateService(transformer.Factory);
						var errorMessage = new ZStringBuilder();
						foreach (var carryForwardCharge in carryForwardApportionChargePKs)
						{
							errorMessage.Append(service.GetInfo(carryForwardCharge.Key, CriticalValidationInfoCollectorServiceKeyType.ChargesToSearchAndCarryForwardApportionChargePKsAreOutOfSync));
							errorMessage.AppendLine();
						}
						ErrorReporter.Instance.Report("TransactionLineJobChargeTransformer_ChargesToSearchAndCarryForwardApportionChargePKsAreOutOfSync", errorMessage.ToStringWithNewLineBetweenAppends(), null);
					}
				}
			}

			void ReorganizeConsolCosts(BusinessObjectFactory factory, ZGuid consolPK, ZString parentTableCode, HashSet<ZGuid> apportionChargePKsThatBelongToSameConsol)
			{
				// Building a dictionary that holds mapping of which group of carry forward charge will be apporitoned to which consol cost
				var costToChargeMapping = GetCostToChargeMapping(factory, consolPK, parentTableCode, apportionChargePKsThatBelongToSameConsol);

				#region Link carry forward charges to the consol cost with which they were mapped above

				foreach (KeyValuePair<JobConsolCost, List<ApportionSplitCharge>> item in costToChargeMapping)
				{
					var charges = item.Value;
					var targetConsolCost = item.Key;

					CriticalValidationInfoCollectorService.GetOrCreateService(factory).AddLastInfoWhenAllowed(targetConsolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ReorganizeConsolCosts, () =>
					{
						var result = new ZStringBuilder();
						result.AppendLine((NoResString)"Consol cost data before reorgnize:");
						result.AppendLine(((IJobConsolCost)targetConsolCost).GetJobConsolCostInfo());
						if (charges.Any())
						{
							result.AppendLine($"Apportion charges mapped to target consol cost ({charges.Count}):");
							charges.ForEach(charge => result.AppendLine(charge.GetJobChargeInfo()));
						}
						return result.ToString();
					});

					//Update Target Cost's various Amount fields
					var templateCharge = charges.First();

					var (totalOSCostAmt, totalOSCostGSTAmt, totalLocalCostAmt) = CalculateCarryForwardConsolCostAmounts(charges, templateCharge.JR_OSCostExRate);

					UpdateConsolCost(targetConsolCost, templateCharge.JR_OSCostExRate, totalOSCostAmt, totalOSCostGSTAmt, totalLocalCostAmt, templateCharge.JR_CostPlaceOfSupply);

					// Now update Target Cost's apportionment charge list 
					ConsolCostImporter.ImportChargeIntoCosting(targetConsolCost, charges, false);

					// As InvoicingBaseConsolCostImporter.ImportChargeIntoCosting will create/update target cost's own apportionment charges, 
					// we can delete all new charges which were created during carry forward charge creation/update process 
					// along with any charge which were apportioned before but not linked to any consol cost anymore. 
					// We do not need those anymore.
					var chargesWhichWereCreatedDuringCarryForwardCalculation = charges.Where(x => !x.IsInDatabase || !x.JR_E6.IsValid).ToList();
					foreach (var appChargeToDelete in chargesWhichWereCreatedDuringCarryForwardCalculation)
					{
						DeleteRedundantApportionCharges(appChargeToDelete);
					}
					chargesWhichWereCreatedDuringCarryForwardCalculation = null;

					//As InvoicingBaseConsolCostImporter.ImportChargeIntoCosting may create new apportionment charges, 
					//we need to add this to ChargesToSearch collection which will make sure ACR gets created
					var newChargePKs = targetConsolCost.ApportionmentCharges.Where(apc => !apc.IsInDatabase && !transformer.ChargesToSearch.PKs.Contains(apc.PK)).Select(apc => apc.PK);
#if NETFRAMEWORK
					var chunks = newChargePKs.Chunk(20);
#else
					var chunks = System.Linq.Enumerable.Chunk(newChargePKs, 20);
#endif
					foreach (var chargePKsChunk in chunks)
					{
						var cacheOnlyChargeFilter = new ZQuery(JobChargeSchema.PK, chargePKsChunk);
						cacheOnlyChargeFilter.FetchOnlyFromLocalCache = true;
						var reloadedCharges = factory.Load<Charge>(cacheOnlyChargeFilter);
						transformer.ChargesToSearch.AddChargeRange(reloadedCharges);
					}

					targetConsolCost.SynchroniseUnpostedInvoiceDetailsIfNecessary();
				}

				#endregion

				apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount.ForEach(x => DeleteRedundantApportionCharges(x));
			}

			void DeleteRedundantApportionCharges(ApportionSplitCharge appChargeToDelete)
			{
				transformer.ChargesToSearch.RemoveChargeByPK(appChargeToDelete.PK);
				if (!appChargeToDelete.IsInDatabase || appChargeToDelete.JR_LocalSellAmt == 0M)
				{
					appChargeToDelete.Delete();
				}
				else
				{
					appChargeToDelete.ClearCostSide();
				}
			}

			static (decimal TotalOSCostAmt, decimal TotalOSCostGSTAmt, decimal TotalLocalCostAmt) CalculateCarryForwardConsolCostAmounts(List<ApportionSplitCharge> charges, ZDecimal costExRate)
			{
				//Recalculating Amounts based on the Exchange Rate of the template charge
				foreach (var charge in charges)
				{
					using (charge.SuspendAmountsCalculations())
					{
						charge.JR_OSCostExRate = costExRate;
						charge.JR_LocalCostAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(charge.JR_OSCostAmt, charge.JR_OSCostExRate);
					}
				}

				var totalOSCostAmt = charges.Sum(x => x.JR_OSCostAmt);
				var totalOSCostGSTAmt = charges.Sum(x => x.JR_OSCostGSTAmt_Calc);
				var totalLocalCostAmt = charges.Sum(x => x.JR_LocalCostAmt);

				return (totalOSCostAmt, totalOSCostGSTAmt, totalLocalCostAmt);
			}

			Dictionary<JobConsolCost, List<ApportionSplitCharge>> GetCostToChargeMapping(BusinessObjectFactory factory, ZGuid consolPK, ZString parentTableCode, HashSet<ZGuid> apportionChargePKsThatBelongToSameConsol)
			{
				var consol = GenericConsol.GenericConsol.LoadConsolBOFromParentIdAndCode(factory, consolPK, parentTableCode);

				var carryForwardChargesThatBelongToSameConsol = transformer.ChargesToSearch.Charges.Where(x => apportionChargePKsThatBelongToSameConsol.Contains(x.PK));

				var groupsOfCarryForwardChargesThatGoingToBeApportionedToTheSameConsolCost = carryForwardChargesThatBelongToSameConsol
					.GroupBy(x => new
					{
						chargeCodePK = x.JR_AC,
						creditorPK = x.JR_OH_CostAccount,
						currency = x.JR_RX_NKCostCurrency
					});

				var costToChargeMapping = new Dictionary<JobConsolCost, List<ApportionSplitCharge>>();
				foreach (var carryForwardCharges in groupsOfCarryForwardChargesThatGoingToBeApportionedToTheSameConsolCost)
				{
					//Searching for an existing consol cost to which carry forward charges can be linked.
					var targetConsolCost = carryForwardCharges
											.OrderBy(x => x.ParentConsolCost?.E6_Sequence ?? int.MaxValue)
											.FirstOrDefault(x => x.ParentConsolCost != null && // we need this null check, as we can have carry forward charges which were created during carry forward calculation process and not linked to any consol cost
																	!costToChargeMapping.ContainsKey(x.ParentConsolCost) &&
																	x.ParentConsolCost.E6_OH_Creditor == x.JR_OH_CostAccount &&
																	x.ParentConsolCost.E6_RX_NKCurrency == x.JR_RX_NKCostCurrency)?.ParentConsolCost;

					var carryForwardChargePKs = carryForwardCharges.Select(x => x.PK).ToArray();

					if (targetConsolCost != null)
					{
						var allApportionmentCharges = MapToExistingConsolCost(factory, targetConsolCost, carryForwardChargePKs);

						if (allApportionmentCharges.Any())
						{
							costToChargeMapping.Add(targetConsolCost, allApportionmentCharges);
							UpdateFPOSOfAllApporitionCharges(allApportionmentCharges, carryForwardCharges.First().JR_CostPlaceOfSupply);
						}
					}
					else
					{
						var newConsolCostWithCharges = MapToNewConsolCost(factory, consol, carryForwardChargePKs);

						targetConsolCost = newConsolCostWithCharges.newConsolCost;

						if (targetConsolCost != null && newConsolCostWithCharges.carryForwardChargesAsApportionSplitCharge.Any())
						{
							costToChargeMapping.Add(targetConsolCost, newConsolCostWithCharges.carryForwardChargesAsApportionSplitCharge);
							UpdateFPOSOfAllApporitionCharges(newConsolCostWithCharges.carryForwardChargesAsApportionSplitCharge, carryForwardCharges.First().JR_CostPlaceOfSupply);
						}
					}

					ClearLinkBetweenConsolCostAndApportionCharge(costToChargeMapping, targetConsolCost);
				}

				return costToChargeMapping;
			}

			static void ClearLinkBetweenConsolCostAndApportionCharge(Dictionary<JobConsolCost, List<ApportionSplitCharge>> costToChargeMapping, JobConsolCost targetConsolCost)
			{
				if (targetConsolCost != null && costToChargeMapping.ContainsKey(targetConsolCost))
				{
					//As we are going to import these charges to the mapped consol cost below, we are clearing existing value of JR_E6 here.
					costToChargeMapping[targetConsolCost].ForEach((x) =>
					{
						if (x.ParentConsolCost?.PK != targetConsolCost.PK)
						{
							x.ParentConsolCost?.ApportionmentCharges.Remove(x);
							x.JR_E6 = ZGuid.Empty;
						}
					});
				}
			}

			static void UpdateFPOSOfAllApporitionCharges(List<ApportionSplitCharge> apportionSplitCharges, ZString fixedPlaceofSupply)
			{
				if (apportionSplitCharges != null)
				{
					foreach (var apportionSplitCharge in apportionSplitCharges)
					{
						apportionSplitCharge.JR_CostPlaceOfSupply = fixedPlaceofSupply;
					}
				}
			}

			List<ApportionSplitCharge> MapToExistingConsolCost(BusinessObjectFactory factory, JobConsolCost targetConsolCost, ZGuid[] carryForwardChargePKs)
			{
				var pKs = transformer.ChargesToSearch.PKs.ToHashSet();

				// These are those apportionment charges which do not exist in the ChargesToSearch.Charges collection but they are linked to the consol cost (target consol cost) to which carry forward charges are going to be apportioned
				var pkOfApportionChargeWhichAreLinkedToJobsThatHaveNotBeenLoadedForTheInvoiceBeingPosted = targetConsolCost.ApportionmentCharges.Select(x => x.PK).Except(pKs);

				// All apportionment charge PKs which are going to be linked to the target consol cost
				var pkOfAllChargesThatGoingToBeApportionedToThisConsolCost = pkOfApportionChargeWhichAreLinkedToJobsThatHaveNotBeenLoadedForTheInvoiceBeingPosted
																				.Union(carryForwardChargePKs);

				// loading all charges as apportionment charge so that they can be imported to the target consol cost
				var allApportionmentCharges = factory.Load<ApportionSplitCharge>(new ZQuery(JobChargeSchema.PK, pkOfAllChargesThatGoingToBeApportionedToThisConsolCost)).ToList();

				var totalAmounts = CalculateCarryForwardConsolCostAmounts(allApportionmentCharges, allApportionmentCharges.First().JR_OSCostExRate);
				targetConsolCost.PushUnApportionedAmountBasedOnRepresentation(totalAmounts.TotalOSCostAmt, JobChargeSchema.JR_OSCostAmt, allApportionmentCharges.ToArray());

				var chargesWithZeroLocalCostAmountOrZeroOSCostAmount = allApportionmentCharges.Where(x => x.JR_LocalCostAmt == 0m || x.JR_OSCostAmt == 0m).ToList();
				apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount.AddRange(chargesWithZeroLocalCostAmountOrZeroOSCostAmount);

				return allApportionmentCharges.Except(chargesWithZeroLocalCostAmountOrZeroOSCostAmount).ToList();
			}

			(JobConsolCost newConsolCost, List<ApportionSplitCharge> carryForwardChargesAsApportionSplitCharge) MapToNewConsolCost(BusinessObjectFactory factory, IJobCostingPlugIn consol, ZGuid[] carryForwardChargePKs)
			{
				var carryForwardChargesAsApportionSplitCharge = factory.Load<ApportionSplitCharge>(new ZQuery(JobChargeSchema.PK, carryForwardChargePKs)).ToList();
				var carryForwardChargesWithNonZeroLocalAndOSCostAmount = new List<ApportionSplitCharge>();

				var totalAmounts = CalculateCarryForwardConsolCostAmounts(carryForwardChargesAsApportionSplitCharge, carryForwardChargesAsApportionSplitCharge.First().JR_OSCostExRate);

				JobConsolCost newTargetConsolCost = null;

				if (totalAmounts.TotalLocalCostAmt != 0m && totalAmounts.TotalOSCostAmt != 0m)
				{
					newTargetConsolCost = CreateNewConsolCost(carryForwardChargesAsApportionSplitCharge.First(), consol);
					newTargetConsolCost.PushUnApportionedAmountBasedOnRepresentation(totalAmounts.TotalOSCostAmt, JobChargeSchema.JR_OSCostAmt, carryForwardChargesAsApportionSplitCharge.ToArray());
					var chargesWithZeroLocalCostAmountOrZeroOSCostAmount = carryForwardChargesAsApportionSplitCharge.Where(x => x.JR_LocalCostAmt == 0m || x.JR_OSCostAmt == 0m).ToList();
					apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount.AddRange(chargesWithZeroLocalCostAmountOrZeroOSCostAmount);
					carryForwardChargesWithNonZeroLocalAndOSCostAmount = carryForwardChargesAsApportionSplitCharge.Except(chargesWithZeroLocalCostAmountOrZeroOSCostAmount).ToList();
				}
				else
				{
					apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount.AddRange(carryForwardChargesAsApportionSplitCharge);
				}

				return (newTargetConsolCost, carryForwardChargesWithNonZeroLocalAndOSCostAmount);
			}

			JobConsolCost CreateNewConsolCost(ApportionSplitCharge charge, IGenericJobCostPlugIn consol)
			{
				var factory = charge.Factory;
				var listing = new ApportionmentListing(factory, consol);
				listing.IsPosting = true;
				var newConsolCost = listing.CostsCollection.TryAddNew();

				var chargeSuspenders = newConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Select(x => x.SetEstimatedCostSuspender.GetSuspender()).ToList();
				try
				{
					using (IDisposable suspender = newConsolCost.SuspendSplittingApportionAmount())
					{
						newConsolCost.E6_AC_ChargeCode = charge.JR_AC;
						newConsolCost.E6_OH_Creditor = charge.JR_OH_CostAccount;
						newConsolCost.E6_RX_NKCurrency = charge.JR_RX_NKCostCurrency;
						newConsolCost.E6_ExchangeRate = charge.JR_OSCostExRate;
						newConsolCost.E6_PPDCLT = "ALL";
						newConsolCost.E6_SupplyType = charge.JR_CostSupplyType;
						newConsolCost.E6_GB_CostTaxBranch = charge.JR_GB_CostTaxBranch;
						newConsolCost.E6_AT_TaxRate = charge.JR_AT_CostGSTRate;
						newConsolCost.E6_A9_VATClass = charge.JR_A9_CostVATClass;
						newConsolCost.E6_CostReference = charge.JR_CostReference;
						newConsolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
					}

					transformer.ConsolCostsToSearch.Add(newConsolCost);
				}
				finally
				{
					chargeSuspenders.ForEach(x => x.Dispose());
				}

				return newConsolCost;
			}

			void UpdateConsolCost(JobConsolCost cost, ZDecimal exchangeRate, ZDecimal oSCostAmount, ZDecimal oSCostGSTAmount, ZDecimal localCostAmount, ZString fixedPlaceOfSupply)
			{
				var chargeSuspenders = cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Select(x => x.SetEstimatedCostSuspender.GetSuspender()).ToList();
				try
				{
					using (IDisposable suspender = cost.SuspendSplittingApportionAmount())
					{
						cost.E6_PlaceOfSupply = fixedPlaceOfSupply;
						cost.E6_ApportionmentMethod = AllocationMethod.Manual;
						cost.E6_ExchangeRate = exchangeRate;
						cost.E6_OSCostAmount = oSCostAmount;
						cost.E6_LocalCostAmount = localCostAmount;

						cost.E6_IsTaxAmountOverridden = oSCostGSTAmount != 0;
						if (cost.E6_IsTaxAmountOverridden)
						{
							cost.E6_OSGSTAmount_Calc = oSCostGSTAmount;
						}

						// We are resetting these properties, as carry forward charges do not have any value in the corresposnding properties.
						cost.E6_InvoiceNum = ZString.Empty;
						cost.E6_InvoiceDate = ZDateTime.Empty;
						cost.E6_DocumentReceivedDate = ZDateTime.Empty;
						cost.E6_CostReference = ZString.Empty;
						cost.E6_PaymentDate = ZDateTime.Empty;
						cost.E6_PaymentType = ZString.Empty;
						cost.E6_AB_BankAccount = ZGuid.Empty;
						cost.E6_AK_ChequeBook = ZGuid.Empty;
					}
				}
				finally
				{
					chargeSuspenders.ForEach(x => x.Dispose());
				}
			}

			void DeleteEmptyJobConsolCosts()
			{
				for (int i = transformer.ConsolCostsToSearch.Count - 1; i >= 0; i--)
				{
					var consolCost = transformer.ConsolCostsToSearch[i];
					consolCost.RemoveNonApplicableCharges();
					consolCost.RecalculateCostAmountWithUnApportionedAmount();

					if (!consolCost.ApportionmentCharges.Any() || consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.IsDeleted || x.JR_OSCostAmt == 0))
					{
						transformer.ConsolCostsToSearch.Remove(consolCost);
						consolCost.CalculationStrategy.HandleDelete();
					}
				}
			}

			public void Dispose()
			{
				CreateCarryForwardConsolCost();
				DeleteEmptyJobConsolCosts();
				carryForwardApportionChargePKs.Clear();
				apportionChargesWithZeroLocalCostAmountOrZeroOSCostAmount.Clear();
				transformer = null;
				carryForwardApportionChargePKs = null;
			}

			IConsolCostImporter ConsolCostImporter => consolCostImporter ??= ObjectFactory.Get<IConsolCostImporter>();
			IConsolCostImporter consolCostImporter;
		}

		public sealed class ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker
		{
			public ChargesToSearchAndCarryForwardApportionChargePKsOutOfSyncChecker(TransactionLineJobChargeTransformer transformer)
			{
				this.transformer = transformer;
			}
			readonly TransactionLineJobChargeTransformer transformer;

			public void CollectInfoWhenChargeRemovedFromChargesToSearch(Charge removedCharge)
			{
				if (transformer.CarryForwardCostCreator.IsItACarryForwardApportionCharge(removedCharge))
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(transformer.Factory).AddInfoWhenAllowed(
						removedCharge.PK,
						CriticalValidationInfoCollectorServiceKeyType.ChargesToSearchAndCarryForwardApportionChargePKsAreOutOfSync,
						() => BuildErrorMessage(removedCharge, System.Environment.StackTrace, true),
						CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				}
			}

			public void CollectInfoWhenChargeAddedToCarryForwardApportionChargePKs(Charge addedCharge)
			{
				if (!transformer.ChargesToSearch.PKs.Contains(addedCharge.PK))
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(transformer.Factory).AddInfoWhenAllowed(
						addedCharge.PK,
						CriticalValidationInfoCollectorServiceKeyType.ChargesToSearchAndCarryForwardApportionChargePKsAreOutOfSync,
						() => BuildErrorMessage(addedCharge, System.Environment.StackTrace, false),
						CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				}
			}

			ZString BuildErrorMessage(Charge charge, string stacktrace, bool isRemoved)
			{
				var errorMessage = new ZStringBuilder();
				errorMessage.Append(isRemoved ? (NoResString)"Charge removed from ChargesToSearch, but it is not removed from carryForwardApportionChargePKs." : (NoResString)"Charge added to carryForwardApportionChargePKs, but it is not added to ChargesToSearch.");
				errorMessage.AppendLine();
				errorMessage.Append(isRemoved ? (NoResString)"Removed Charge Details:" : (NoResString)"Added Charge Details:");
				errorMessage.Append(charge.GetJobChargeInfo());
				errorMessage.AppendLine();
				errorMessage.Append((NoResString)"ChargesToSearch collection:");
				errorMessage.Append(FormattableString.Invariant($"Count: {transformer.ChargesToSearch.Charges.Count()}"));
				transformer.ChargesToSearch.Charges.ForEach(x => errorMessage.Append(x.GetJobChargeInfo()));
				errorMessage.AppendLine();
				errorMessage.Append((NoResString)"carryForwardApportionChargePKs collection:");
				errorMessage.Append(FormattableString.Invariant($"Count: {transformer.CarryForwardCostCreator.apportionChargePks.Count}"));
				transformer.CarryForwardCostCreator.apportionChargePks.ForEach(x => errorMessage.Append(GetCarryForwardChargeDetails(x)));
				errorMessage.AppendLine();
				errorMessage.Append(stacktrace);
				return errorMessage.ToStringWithNewLineBetweenAppends();
			}

			ZString GetCarryForwardChargeDetails(ZGuid chargePK)
			{
				var charge = transformer.Factory.Load<JobCharge>(chargePK);
				if (charge == null)
				{
					return FormattableString.Invariant($"Could not load Charge with PK : {chargePK}.");
				}
				else
				{
					return charge.GetJobChargeInfo();
				}
			}
		}

		public class RedundantApportionChargeCollection : List<ApportionSplitCharge>
		{
			public new void Add(ApportionSplitCharge apportionCharge)
			{
				if (apportionCharge != null)
				{
					apportionCharge.JR_E6 = ZGuid.Empty;
				}
				base.Add(apportionCharge);
			}

			public new void AddRange(IEnumerable<ApportionSplitCharge> collection)
			{
				collection?.ForEach(c => c.JR_E6 = ZGuid.Empty);
				base.AddRange(collection);
			}
		}

		protected override void TransformCore(TransactionHeaderWithLines transaction)
		{
			Factory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
			try
			{
				var invoice = transaction as InvoicingBase;
				if (invoice == null)
				{
					return;
				}

				PopulateCostsAndChargesToSearch(Factory, invoice, ChargesToSearch, ConsolCostsToSearch, null);

				using (invoice.ConsolCosting.ConsolSummary.UpdateSuspender.GetSuspender())
				{
					SynchroniseChargesWithInvoice(invoice);
				}
			}
			finally
			{
				CarryForwardCostCreator.Dispose();
				NegativeReAccrualChecker.Dispose();
				negativeReAccrualChecker = null;
				carryForwardConsolCostCreator = null;

				Factory.RemoveContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
				if (!transaction.HasContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer))
				{
					ChargesToSearch.Dispose();
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public static void PopulateCostsAndChargesToSearch(BusinessObjectFactory factory, InvoicingBase invoice, ChargesByPK chargesToSearch, ConsolCostListWithStrategy consolCostsToSearch, IList<ZGuid> additionalChargeCodesToQuery)
		{
			if ((invoice is APInvoice || invoice is APCreditNote) && invoice.Lines.Any())
			{
				var lines = invoice.Lines.Cast<InvoicingLineBase>().Where(x => x.AL_AC.IsValid && x.AL_JH.IsValid);
				if (lines.Any())
				{
					var chargeData = GetChargeToSearch(invoice, lines);
					if (chargeData.Any())
					{
						var lineJobInfos = lines.Select(x => new
						{
							JH = x.AL_JH,
							AC = x.AL_AC,
							GB = x.AL_GB,
							GE = x.AL_GE
						}).ToHashSet();

						var chargePKsMatchingLines = chargeData.Where(x => lineJobInfos.Contains(new
						{
							JH = x.JR_JH,
							AC = x.JR_AC,
							GB = x.JR_GB,
							GE = x.JR_GE
						})).Select(x => x.JR_PK).ToArray();

						var chargesExistedInMemoryBefore = factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, chargePKsMatchingLines) { FetchOnlyFromLocalCache = true });
						var chargesExistedInMemoryBeforeWithoutChanges = chargesExistedInMemoryBefore.Where(x => !x.HasChanges).ToArray();
						ChargeReloader.ReloadChargesPreservingDisplayOrder(chargesExistedInMemoryBeforeWithoutChanges);

						var chargesMatchingLines = factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, chargePKsMatchingLines));
						chargesMatchingLines.Where(x => !x.IsCostPosted).ForEach(x => chargesToSearch.AddCharge(x));

						foreach (var line in lines)
						{
							if (line.OriginalJobCharge != null)
							{
								if (!chargesToSearch.PKs.Contains(line.OriginalJobCharge.PK))
								{
									line.OriginalJobCharge = null;
									if (line.ApportionmentChargeImportedFrom != null)
									{
										line.ApportionmentChargeImportedFrom.RelatedApportionChargeFromDB = null;
										line.ApportionmentChargeImportedFrom.ParentConsolCost.RelatedConsolCostPK = ZGuid.Empty;
									}
								}
								else if (!line.IsPopulatedFromImportedApportionment && line.OriginalJobCharge.JR_IsApportioned)
								{
									line.OriginalJobCharge = null;
								}
							}
						}
					}
				}

				PopulateConsolCostsToSearch(factory, invoice, consolCostsToSearch, additionalChargeCodesToQuery);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Table Valued Parameter only works for DBCommand")]
		static IEnumerable<ChargeToSearchInfo> GetChargeToSearch(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines)
		{
			var associatedChargeFilterRelatedToConsolCost = GetAssociatedChargeFilterRelatedToConsolCost(invoice, lines);
			var associatedChargeFilterNotRelatedToConsolCost = GetAssociatedChargeFilterNotRelatedToConsolCost(invoice, lines);

			var sql = $@"
SELECT 
	{JobChargeSchema.Constants.PK}, 
	{JobChargeSchema.Constants.JR_JH}, 
	{JobChargeSchema.Constants.JR_AC}, 
	{JobChargeSchema.Constants.JR_GB}, 
	{JobChargeSchema.Constants.JR_GE} 
FROM 
	{JobChargeSchema.Constants.SqlSchemaName}.{JobChargeSchema.Constants.TableName}
{associatedChargeFilterRelatedToConsolCost.GetAsWhereAndOrderByClause(false)}
UNION ALL
SELECT 
	{JobChargeSchema.Constants.PK}, 
	{JobChargeSchema.Constants.JR_JH}, 
	{JobChargeSchema.Constants.JR_AC}, 
	{JobChargeSchema.Constants.JR_GB}, 
	{JobChargeSchema.Constants.JR_GE} 
FROM 
	{JobChargeSchema.Constants.SqlSchemaName}.{JobChargeSchema.Constants.TableName}
{associatedChargeFilterNotRelatedToConsolCost.GetAsWhereAndOrderByClause(false)}";

#if DEBUG
			SqlForGetChargeToSearch_ForTestOnly = sql;
#endif

			var chargeData = new List<ChargeToSearchInfo>();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameters(associatedChargeFilterRelatedToConsolCost.Params);
				AddAdditionalTableValuedParameter(invoice, lines, command);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						chargeData.Add(new ChargeToSearchInfo((Guid)reader[AutoJobCharge.Schema.PK], (Guid)reader[AutoJobCharge.Schema.JR_JH], (Guid)reader[AutoJobCharge.Schema.JR_AC], (Guid)reader[AutoJobCharge.Schema.JR_GB], (Guid)reader[AutoJobCharge.Schema.JR_GE]));
					}
				}
			}

			return chargeData;
		}

		static void AddAdditionalTableValuedParameter(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines, DbCommand command)
		{
			command.AddTableValuedParameter<Guid>(JobHeaderPKParameterName, TVPHelper.TVP_uniqueidentifier, lines.Select(x => x.AL_JH.ToGuid()).Distinct());

			if (invoice.IsCompletingInvoice)
			{
				var chargeImportedInTheCurrentInvoice = lines.Cast<InvoicingLineBase>()
									.Where(x => x.IsPopulatedFromImportedJobCharge && x.OriginalJobChargePK.IsValid)
									.Select(x => x.OriginalJobChargePK.ToGuid())
									.Distinct();
				command.AddTableValuedParameter<Guid>(JobChargePKParameterName, TVPHelper.TVP_uniqueidentifier, chargeImportedInTheCurrentInvoice);
			}
		}

		const string JobHeaderPKParameterName = "@JobHeaderPKs";
		const string JobChargePKParameterName = "@JobChargePKs";

		static void PopulateConsolCostsToSearch(BusinessObjectFactory factory, InvoicingBase invoice, ConsolCostListWithStrategy consolCostsToSearch, IList<ZGuid> additionalChargeCodesToQuery)
		{
			if (!invoice.IsInvoiceApproving) //all consol costs already posted with UA Invoice we converting and shouldn't be deleted and replaced with any other ones.
			{
				var lines = invoice.Lines.Cast<InvoicingLineBase>().Where(x => x.ApportionmentChargeImportedFrom != null && x.ApportionmentChargeImportedFrom.JR_E6.IsValid);
				if (lines.Any())
				{
					var chargeCodePKs = lines.Select(x => x.AL_AC).Distinct();
					
					if (additionalChargeCodesToQuery != null)
					{
						chargeCodePKs = chargeCodePKs.Union(additionalChargeCodesToQuery);
					}

					var consolPKs = lines.Select(x => x.ApportionmentChargeImportedFrom.ParentConsolCost.E6_ParentID).Distinct();

					var associatedCostQuery = new ZQuery();
					associatedCostQuery.AddToFilter(JobConsolCostSchema.E6_AC_ChargeCode, chargeCodePKs);
					associatedCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, consolPKs);
					associatedCostQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
					associatedCostQuery.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);

					var costImportedInTheCompleteInvoiceFilter = new ZDBOnlyQuery(typeof(JobConsolCost));
					costImportedInTheCompleteInvoiceFilter.AddSubQuery(GetConsolCostImportedInTheCompleteInvoiceExcludingFilter(invoice, lines), JoinCondition.And);
					associatedCostQuery.AddToFilter(costImportedInTheCompleteInvoiceFilter);

					if (IsBringForwardAgainstCreditorEnabled)
					{
						var creditorQuery = new ZQuery(JobConsolCostSchema.E6_OH_Creditor, invoice.AH_OH);
						creditorQuery.AddToFilter(JoinCondition.Or, JobConsolCostSchema.E6_OH_Creditor, null);
						associatedCostQuery.AddToFilter(creditorQuery);
						associatedCostQuery.OrderBy = JobConsolCostSchema.E6_OH_Creditor.Name + " DESC";
					}

					var consolCosts = factory.Load<JobConsolCost>(associatedCostQuery).Where(x => x.IsInDatabase && !x.IsDeleted && !x.IsPosted);
					consolCostsToSearch.AddRange(consolCosts);
				}
			}
		}

		static ZQuery GetAssociatedChargeFilterNotRelatedToConsolCost(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines)
		{
			var associatedChargeFilter = GetAssociatedChargeFilterCore(invoice, lines);

			var subQueryToExcludeApportionJobChargesThatAreImportedInTheCurrentIncompleteInvoice = new ZDBOnlyQuery(typeof(JobCharge));
			subQueryToExcludeApportionJobChargesThatAreImportedInTheCurrentIncompleteInvoice.AddToFilter(JobChargeSchema.JR_E6, null);
			associatedChargeFilter.AddToFilter(subQueryToExcludeApportionJobChargesThatAreImportedInTheCurrentIncompleteInvoice);

			return associatedChargeFilter;
		}

		static ZQuery GetAssociatedChargeFilterRelatedToConsolCost(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines)
		{
			var associatedChargeFilter = GetAssociatedChargeFilterCore(invoice, lines);

			var subQueryToExcludeApportionJobChargesThatAreImportedInTheCurrentIncompleteInvoice = new ZDBOnlyQuery(typeof(JobCharge));
			subQueryToExcludeApportionJobChargesThatAreImportedInTheCurrentIncompleteInvoice.AddSubQuery(JobChargeSchema.JR_E6, GetConsolCostImportedInTheCompleteInvoiceExcludingFilter(invoice, lines), JoinCondition.Or);
			associatedChargeFilter.AddToFilter(subQueryToExcludeApportionJobChargesThatAreImportedInTheCurrentIncompleteInvoice);

			return associatedChargeFilter;
		}

		static ZQuery GetAssociatedChargeFilterCore(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines)
		{
			var associatedChargeFilter = new ZQuery();

			var invoiceNumberFilter = new ZQuery(JobChargeSchema.JR_APInvoiceNum, ZString.Empty);
			invoiceNumberFilter.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_APInvoiceNum, invoice.AH_TransactionNum);
			associatedChargeFilter.AddToFilter(invoiceNumberFilter);

			if (!IsNegativeAccrualBehaviorsEnabled)
			{
				if (invoice is APInvoice)
				{
					associatedChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.GreaterThanOrEqualTo, 0);
				}
				else if (invoice is APCreditNote)
				{
					associatedChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.LessThan, 0);
				}
			}

			var relatedJobPKs = lines.Select(x => x.AL_Calc_RelatedJobPK).Distinct();
			var relatedJobNumberFilter = new ZDBOnlyQuery(typeof(JobCharge));

			if (relatedJobPKs.Any(x => !x.IsEmpty))
			{
				var nonEmptyRelatedJobFilter = new ZDBOnlySubQuery(typeof(JobChargeTarget), JobChargeTargetSchema.JRT_JR);
				nonEmptyRelatedJobFilter.AddToFilter(JobChargeTargetSchema.JRT_RelatedJobID, relatedJobPKs.Where(x => !x.IsEmpty));
				relatedJobNumberFilter.AddSubQuery(JobChargeSchema.PK, nonEmptyRelatedJobFilter, JoinCondition.And);
			}

			if (relatedJobPKs.Any(x => x.IsEmpty))
			{
				var emptyRelatedJobFilter = new ZDBOnlySubQuery(typeof(JobChargeTarget), JobChargeTargetSchema.JRT_JR, true);
				relatedJobNumberFilter.AddSubQuery(JobChargeSchema.PK, emptyRelatedJobFilter, JoinCondition.Or);
			}

			associatedChargeFilter.AddToFilter(relatedJobNumberFilter);

			var branchPKs = lines.Select(x => x.AL_GB).Distinct();
			var departmentPKs = lines.Select(x => x.AL_GE).Distinct();
			var chargeCodePKs = lines.Select(x => x.AL_AC).Distinct();

			associatedChargeFilter.AddFilterAndZSQLParameterCollection($"{JobChargeSchema.Constants.JR_JH} IN (SELECT VALUE FROM {JobHeaderPKParameterName})", null);
			associatedChargeFilter.AddToFilter(JobChargeSchema.JR_GB, branchPKs);
			associatedChargeFilter.AddToFilter(JobChargeSchema.JR_GE, departmentPKs);
			associatedChargeFilter.AddToFilter(JobChargeSchema.JR_AC, chargeCodePKs);

			var chargesNotImportedIntoINIFilter = new ZDBOnlyQuery(typeof(JobCharge));
			var chargeAttribFilter = new ZDBOnlySubQuery(typeof(JobChargeAttrib), JobChargeAttribSchema.EC_JR, true);
			if (invoice.IsCompletingInvoice)
			{
				chargeAttribFilter.AddFilterAndZSQLParameterCollection($"{JobChargeAttribSchema.Constants.EC_JR} NOT IN (SELECT VALUE FROM {JobChargePKParameterName})", null);
			}
			chargeAttribFilter.AddToFilter(JobChargeAttribSchema.EC_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
			chargesNotImportedIntoINIFilter.AddSubQuery(chargeAttribFilter, JoinCondition.And);
			associatedChargeFilter.AddToFilter(chargesNotImportedIntoINIFilter);

			var chargeAcrualLines = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			chargeAcrualLines.AddToFilter(AccTransactionLinesSchema.AL_GC, invoice.AH_GC);
			chargeAcrualLines.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);

			var chargeNotPostedFilter = new ZDBOnlyQuery(typeof(JobCharge));
			chargeNotPostedFilter.AddToFilter(JobChargeSchema.JR_AL_APLine, null);
			chargeNotPostedFilter.AddSubQuery(JobChargeSchema.JR_AL_APLine, chargeAcrualLines, JoinCondition.Or);
			associatedChargeFilter.AddToFilter(chargeNotPostedFilter);

			return associatedChargeFilter;
		}

		static ZDBOnlySubQuery GetConsolCostImportedInTheCompleteInvoiceExcludingFilter(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines)
		{
			var costAttribFilter = new ZDBOnlySubQuery(typeof(JobConsolCostAttrib), JobConsolCostAttribSchema.E6A_E6_JobConsolCost, notIn: true);
			if (invoice.IsCompletingInvoice)
			{
				var jobConsolCostPKs = lines.Cast<InvoicingLineBase>()
					.Select(x => x.ApportionmentChargeImportedFrom?.ParentConsolCost?.RelatedConsolCostPK)
					.Where(x => x.HasValue && x.Value.IsValid && x.Value != ZGuid.Empty)
					.Select(x => x.Value.ToGuid())
					.Distinct().ToList();

				if (jobConsolCostPKs.Any())
				{
					var parameterCollection = new ZSqlParameterCollection(ZSqlParameter.New(JobConsolCostPKParameterName, jobConsolCostPKs, JobConsolCostAttribSchema.E6A_E6_JobConsolCost, isTableValued: true));
					costAttribFilter.AddFilterAndZSQLParameterCollection($"{JobConsolCostAttribSchema.Constants.E6A_E6_JobConsolCost} NOT IN (SELECT VALUE FROM {JobConsolCostPKParameterName})", parameterCollection);
				}
			}
			costAttribFilter.AddToFilter(JobConsolCostAttribSchema.E6A_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);

			return costAttribFilter;
		}

		const string JobConsolCostPKParameterName = "@jobConsolCostPKs";

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void SynchroniseChargesWithInvoice(InvoicingBase invoice)
		{
			RecordCurrentStateOfConsolCosts(invoice.ConsolCosting.ConsolCosts, CriticalValidationInfoCollectorServiceKeyType.ConsolCostAndApportionmentCharges_BeforeSynchroniseChargesWithInvoice);

			if (ExchangeRateCalculator.IsExRateOptionApplicable(invoice.GetExRateLedger(), invoice.IsLocalCurrencyTransaction, invoice.AH_GC))
			{
				ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice);
			}

			using (invoice.ConsolCosting.ConsolCosts.SuspendListChanged())
			{
				SynchroniseCosts(invoice);

				foreach (JobConsolCost cost in invoice.ConsolCosting.ConsolCosts)
				{
					cost.E6_AH_APInvoice = invoice.PK;
				}

				NegateAllCreditNoteConsolCosts(invoice);
			}

			var reorderLines = invoice.Lines.Cast<InvoicingLineBase>().OrderByDescending(x => x.IsPopulatedFromImportedJobCharge).ToList();
			var invoicingLineCacheUtility = Factory.ServiceContainer.GetService<IInvoicingLineCacheUtility>();
			invoicingLineCacheUtility?.GenerateInvoicingLineCache(reorderLines);
			foreach (InvoicingLineBase invoiceLine in reorderLines)
			{
				var associatedJob = Factory.Load<Job>(invoiceLine.AL_JH);
				if (associatedJob != null)
				{
					AddCurrencyAndExchangeRateToJob(invoiceLine, associatedJob);

					if (invoiceLine.IsInvoicingBaseApproving)
					{
						var chargeForUpdate = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, invoiceLine.PK));
						if (chargeForUpdate != null)
						{
							UpdateExistingCharge(invoiceLine, chargeForUpdate);
						}
					}
					else if (invoiceLine.IsPopulatedFromImportedApportionment)
					{
						ProcessApportionedLine(invoiceLine, associatedJob);
					}
					else
					{
						if (!IsNegativeAccrualBehaviorsEnabled && invoiceLine is APCreditNoteLine)
						{
							CreateNewCharge(invoiceLine, associatedJob);
						}
						else
						{
							var isOriginalChargeNeedToBeProcessedFirst = IsBringForwardAgainstCreditorEnabled &&
																			invoiceLine.OriginalJobCharge != null && !invoiceLine.OriginalJobCharge.IsDeleted &&
																			invoiceLine.OriginalJobCharge.JR_OH_CostAccount.IsValid &&
																			invoiceLine.OriginalJobCharge.JR_OH_CostAccount != invoiceLine.InvoiceBase.AH_OH;
							Charge[] associatedCharges;
							if (invoiceLine.IsPopulatedFromImportedJobCharge)
							{
								if (invoiceLine.OriginalJobCharge.IsCostPosted)
								{
									throw new ZSaveConcurrencyException(
										new ZDataConcurrencyException(
											new InvalidOperationException(Res.GetString("714E9534-6AFD-4CEF-8D83-5CCFD2AEB969", "Another user has already posted at least one Charge in this invoice. Please close the form and retry."))
											, ((IBusinessObjectInternals)invoiceLine).Row, ((IDbConnected)Factory).Connection), Factory);
								}
								associatedCharges = new Charge[] { (Charge)invoiceLine.OriginalJobCharge };
							}
							else
							{
								associatedCharges = FindAssociatedCharges(invoiceLine, isOriginalChargeNeedToBeProcessedFirst);
							}
							if (associatedCharges.Length == 0)
							{
								CreateNewCharge(invoiceLine, associatedJob);
							}
							else
							{
								var carryForwardAmount = CalculateCarryForwardAmount(invoiceLine, associatedCharges);
								var chargeToUpdate = associatedCharges[0];
								var carryForwardAmountForChargeToUpdate = CalculateCarryForwardAmountForChargeToUpdate(invoiceLine, associatedCharges, isOriginalChargeNeedToBeProcessedFirst);
								var doesOriginalAccrualExist = associatedCharges.Any(x => IsNegativeAccrualBehaviorsEnabled ? x.JR_LocalCostAmt != 0 : x.JR_LocalCostAmt > 0m);
								var createWIPWhenCostIsFinal = AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsFinal.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

								UpdateExistingCharge(invoiceLine, chargeToUpdate);
								if (associatedCharges.Length == 1)
								{
									UpdateOtherForSingleCharge(invoiceLine, associatedJob, chargeToUpdate,
										carryForwardAmount, carryForwardAmountForChargeToUpdate,
										doesOriginalAccrualExist, chargeToUpdate.IsRevenuePosted, createWIPWhenCostIsFinal);
								}
								else
								{
									UpdateOtherForMultipleCharges(invoiceLine, associatedJob, chargeToUpdate, associatedCharges,
										carryForwardAmount, carryForwardAmountForChargeToUpdate, doesOriginalAccrualExist, createWIPWhenCostIsFinal, 1);
								}
							}
						}
					}
				}
			}

			invoicingLineCacheUtility?.ValidateInvoicingLineCache(invoice.Lines.Cast<InvoicingLineBase>());

			RecordCurrentStateOfConsolCosts(invoice.ConsolCosting.ConsolCosts, CriticalValidationInfoCollectorServiceKeyType.ConsolCostAndApportionmentCharges_AfterSynchroniseChargesWithInvoice);
		}

		void RecordCurrentStateOfConsolCosts(APInvoiceConsolCostCollection consolCosts, CriticalValidationInfoCollectorServiceKeyType keyType)
		{
			foreach (JobConsolCost cost in consolCosts)
			{
				var info = GetConsolCostInfo(cost);
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(
					cost.PK,
					keyType,
					() => info,
					CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			}
		}

		string GetConsolCostInfo(JobConsolCost cost)
		{
			var charges = cost.ApportionmentCharges.OfType<ApportionSplitCharge>().ToList();
			var sumOfApportionedChargesLocalCostAmt = charges.Sum(x => x.JR_LocalCostAmt);
			var infoBuilder = new ZStringBuilder(FormattableString.Invariant($"E6_LocalCostAmount = {cost.E6_LocalCostAmount}, Sum of ApportionmentCharges JR_LocalCostAmt = {sumOfApportionedChargesLocalCostAmt}."));
			infoBuilder.Append(FormattableString.Invariant($"Consol Cost: E6_OSCostAmount = {cost.E6_OSCostAmount}, E6_LocalCostAmount = {cost.E6_LocalCostAmount}, E6_RX_NKCurrency = {cost.E6_RX_NKCurrency}, LocalCurrency = {cost.LocalCurrency}, E6_ExchangeRate = {cost.E6_ExchangeRate}"));
			charges.ForEach(c => infoBuilder.Append(c.GetJobChargeInfo()));
			return infoBuilder.ToStringWithNewLineBetweenAppends();
		}

		static void NegateAllCreditNoteConsolCosts(InvoicingBase invoice)
		{
			if (invoice is APCreditNote)
			{
				foreach (JobConsolCost cost in invoice.ConsolCosting.ConsolCosts)
				{
					if (!cost.IsInDatabase || invoice.IsApprovingInvoice)
					{
						using (cost.SuspendSplittingApportionAmount())
						{
							var originalOSGSTAmount = cost.E6_OSGSTAmount_Calc;
							cost.E6_OSCostAmount = -cost.E6_OSCostAmount;
							cost.E6_OSGSTAmount_Calc = -originalOSGSTAmount;
						}
					}
				}
			}
		}

		Charge[] FindAssociatedCharges(InvoicingLineBase invoiceLine, bool isOriginalChargeNeedToBeProcessedFirst)
		{
			var associatedChargeFilter = ChargesToSearch.Charges.Where(x =>
				x.JR_JH == invoiceLine.AL_JH &&
				x.JR_AC == invoiceLine.AL_AC &&
				x.JR_GB == invoiceLine.AL_GB &&
				x.JR_GE == invoiceLine.AL_GE &&
				x.RelatedJobID == invoiceLine.AL_Calc_RelatedJobPK &&
				x.JR_E6.IsEmpty);

			if (IsBringForwardAgainstCreditorEnabled)
			{
				associatedChargeFilter = associatedChargeFilter.Where(x =>
					x.JR_OH_CostAccount == invoiceLine.InvoiceBase.AH_OH ||
					x.JR_OH_CostAccount.IsEmpty ||
					isOriginalChargeNeedToBeProcessedFirst && x.PK == invoiceLine.OriginalJobCharge.PK);
			}
			var associatedChargeFilterSorted = associatedChargeFilter.OrderByDescending(x => x.JR_APInvoiceNum);
			if (IsBringForwardAgainstCreditorEnabled)
			{
				associatedChargeFilterSorted = associatedChargeFilterSorted.ThenByDescending(x => x.JR_OH_CostAccount);
			}
			associatedChargeFilterSorted = associatedChargeFilterSorted.ThenBy(x => x.JR_DisplaySequence);

			var associatedChargesList = associatedChargeFilterSorted.ToList();
			if (isOriginalChargeNeedToBeProcessedFirst)
			{
				for (int i = 0; i < associatedChargesList.Count; i++)
				{
					var associatedCharge = associatedChargesList[i];
					if (associatedCharge.PK == invoiceLine.OriginalJobCharge.PK)
					{
						associatedChargesList.RemoveAt(i);
						associatedChargesList.Insert(0, associatedCharge);
						break;
					}
				}
			}

			return associatedChargesList.ToArray();
		}

		#region Implementation

		static void AddCurrencyAndExchangeRateToJob(InvoicingLineBase invoiceLine, Job associatedJob)
		{
			var ledger = ExchangeRateEnumsExtensions.GetLedgerFromCode(invoiceLine.TransactionHeader.AH_Ledger);
			var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(invoiceLine.Company, ledger, invoiceLine.TransactionHeader.AH_RX_NKTransactionCurrency);
			var existingRate = ((IExchangeRateProvider)associatedJob).GetExchangeRate(invoiceLine.TransactionCurrency.RX_Code, invoiceLine.TransactionHeader.AH_OH, ledger, invoiceCurrencyType: invoiceCurrencyType);

			if (existingRate == null)
			{
				var rate = associatedJob.AddCurrency(invoiceLine.TransactionCurrency, invoiceLine.AL_ExchangeRate, invoiceLine.TransactionHeader.AH_OH, ledger, invoiceCurrencyType);
				if (rate != null)
				{
					rate.JF_IsTransformed = true;
				}
			}
			else if (existingRate.Rate == 0m)
			{
				existingRate.SetBaseRate(invoiceLine.AL_ExchangeRate);
			}
		}

		InvoicingLineBase FindInvoiceLineFromEnteredCharge(InvoicingBase invoice, ApportionSplitCharge apportionedCharge)
		{
			InvoicingLineBase result = null;
			foreach (InvoicingLineBase invoiceLine in invoice.Lines)
			{
				if (invoiceLine.ApportionmentChargeImportedFrom != null &&
					invoiceLine.ApportionmentChargeImportedFrom.PK == apportionedCharge.PK)
				{
					result = invoiceLine;
					break;
				}
			}
			return result;
		}

		void CopyConsolCostDetails(InvoicingBase invoice, JobConsolCost costToUpdate, JobConsolCost enteredCost)
		{
			using (costToUpdate.E6_OSCostAmountRoundingErrorReproterFunctionalitySuspender.GetSuspender())
			{
				int multiplier = invoice is APCreditNote ? -1 : 1;
				costToUpdate.E6_OH_Creditor = enteredCost.E6_OH_Creditor;
				costToUpdate.E6_PlaceOfSupply = enteredCost.E6_PlaceOfSupply;
				costToUpdate.E6_SupplyType = enteredCost.E6_SupplyType;
				costToUpdate.E6_GB_CostTaxBranch = enteredCost.E6_GB_CostTaxBranch;
				costToUpdate.E6_RX_NKCurrency = enteredCost.E6_RX_NKCurrency;
				costToUpdate.E6_ExchangeRate = enteredCost.E6_ExchangeRate;
				costToUpdate.E6_OSCostAmount = enteredCost.E6_OSCostAmount * multiplier;
				costToUpdate.E6_LocalCostAmount = enteredCost.E6_LocalCostAmount * multiplier;
				costToUpdate.E6_PPDCLT = enteredCost.E6_PPDCLT;
				costToUpdate.E6_AT_TaxRate = enteredCost.E6_AT_TaxRate;
				costToUpdate.SetTaxDateSafe(enteredCost.E6_TaxDate);
				costToUpdate.E6_A9_VATClass = enteredCost.E6_A9_VATClass;
				costToUpdate.E6_IsTaxAmountOverridden = true;
				costToUpdate.E6_OSGSTAmount_Calc = enteredCost.E6_OSGSTAmount_Calc * multiplier;
				costToUpdate.E6_InvoiceNum = enteredCost.E6_InvoiceNum;
				costToUpdate.E6_InvoiceDate = enteredCost.E6_InvoiceDate;
				costToUpdate.E6_DocumentReceivedDate = enteredCost.E6_DocumentReceivedDate;
				costToUpdate.E6_PaymentDate = enteredCost.E6_PaymentDate;
				costToUpdate.E6_CostReference = enteredCost.E6_CostReference;
				costToUpdate.E6_ApportionmentMethod = enteredCost.E6_ApportionmentMethod;
				costToUpdate.E6_CostGovtChargeCode = enteredCost.E6_CostGovtChargeCode;
				costToUpdate.E6_SellGovtChargeCode = enteredCost.E6_SellGovtChargeCode;
			}
		}

		public static bool CanImportNewCostIntoExistingCost(JobConsolCost existingCost, ZGuid newCostChargeCodePK, ZGuid newCostParentPK)
		{
			return existingCost.E6_AC_ChargeCode == newCostChargeCodePK &&
							existingCost.E6_ParentID == newCostParentPK;
		}

		bool IsTheSameSignCost(InvoicingBase invoice, JobConsolCost enteredCost, JobConsolCost costToUpdate)
		{
			var isTheSameSignCost = true;
			if (!IsNegativeAccrualBehaviorsEnabled)
			{
				var isNegativeCost = (invoice is APCreditNote && enteredCost.E6_OSCostAmount > 0) ||
										(invoice is APInvoice && enteredCost.E6_OSCostAmount < 0);
				isTheSameSignCost = (!isNegativeCost && costToUpdate.E6_OSCostAmount > 0) || (isNegativeCost && costToUpdate.E6_OSCostAmount < 0);
			}
			return isTheSameSignCost;
		}

		void SynchroniseCosts(InvoicingBase invoice)
		{
			var costsToBeReplacedInCosting = new List<JobConsolCost>();
			var costsToBeAddedToCosting = new List<JobConsolCost>();

			foreach (JobConsolCost enteredCost in invoice.ConsolCosting.ConsolCosts)
			{
				var validCosts = ConsolCostsToSearch.Where(x => CanImportNewCostIntoExistingCost(enteredCost, x.E6_AC_ChargeCode, x.E6_ParentID)
															&& IsTheSameSignCost(invoice, enteredCost, x)
															&& (!IsBringForwardAgainstCreditorEnabled
																|| !invoice.IsConvertedFromARInvoice
																|| enteredCost.E6_OH_Creditor == x.E6_OH_Creditor
																|| !x.E6_OH_Creditor.IsValid)).ToArray();
				JobConsolCost costToUpdate = null;
				invoice.ValidateAndFixConsolCostsMarkedAsImported(enteredCost);
				if (enteredCost.RelatedConsolCostPK.IsValid)
				{
					costToUpdate = validCosts.FirstOrDefault(x => x.PK == enteredCost.RelatedConsolCostPK);
				}
				if (costToUpdate == null)
				{
					costToUpdate = ConsolCostSelector.GetBestMatch(enteredCost, validCosts, invoice is APCreditNote ? -1 : 1);
				}
				if (costToUpdate != null)
				{
					costsToBeReplacedInCosting.Add(enteredCost);
					costsToBeAddedToCosting.Add(costToUpdate);
					UpdateCharges(invoice, costToUpdate, enteredCost);
					ConsolCostsToSearch.Remove(costToUpdate);
				}
			}

			using (invoice.ConsolCosting.ConsolCosts.SuspendListChanged())
			{
				foreach (JobConsolCost cost in costsToBeReplacedInCosting)
				{
					invoice.ConsolCosting.ConsolCosts.RemoveAndDelete(cost);
				}

				invoice.ConsolCosting.ConsolCosts.AddRange(costsToBeAddedToCosting.ToArray());
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
#if DEBUG
		public
#endif
		void UpdateCharges(InvoicingBase invoice, JobConsolCost costToUpdate, JobConsolCost enteredCost)
		{
			var chargeSuspenders = costToUpdate.ApportionmentCharges.Cast<ApportionSplitCharge>().Select(x => x.SetEstimatedCostSuspender.GetSuspender()).ToList();
			try
			{
				CopyConsolCostDetails(invoice, costToUpdate, enteredCost);
			}
			finally
			{
				chargeSuspenders.ForEach(x => x.Dispose());
			}

			enteredCost.RemoveNonApplicableCharges();
			var updatedCharges = new List<ApportionSplitCharge>();
			var chargesInEnteredCostButNotUpdated = new List<ApportionSplitCharge>();

			foreach (ApportionSplitCharge enteredCharge in enteredCost.ApportionmentCharges)
			{
				var chargeFound = false;
				var invoiceLine = FindInvoiceLineFromEnteredCharge(invoice, enteredCharge);
				try
				{
					foreach (ApportionSplitCharge chargeToUpdate in costToUpdate.ApportionmentCharges)
					{
						if (chargeToUpdate.JR_JH == enteredCharge.JR_JH)
						{
							using (chargeToUpdate.SetEstimatedCostSuspender.GetSuspender())
							{
								updatedCharges.Add(chargeToUpdate);
								chargeFound = true;
								chargeToUpdate.JR_OH_CostAccount = enteredCharge.JR_OH_CostAccount;
								chargeToUpdate.JR_RX_NKCostCurrency = enteredCharge.JR_RX_NKCostCurrency;
								chargeToUpdate.JR_OSCostExRate = enteredCharge.JR_OSCostExRate;
								chargeToUpdate.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);
								chargeToUpdate.JR_GE = enteredCharge.JR_GE;
								chargeToUpdate.JR_GB = enteredCharge.JR_GB;
								chargeToUpdate.JR_CostPlaceOfSupply = enteredCharge.JR_CostPlaceOfSupply;
								chargeToUpdate.JR_CostSupplyType = enteredCharge.JR_CostSupplyType;
								chargeToUpdate.JR_GB_CostTaxBranch = enteredCharge.JR_GB_CostTaxBranch;

								chargeToUpdate.JR_OSCostAmt = enteredCharge.JR_OSCostAmt;
								chargeToUpdate.JR_LocalCostAmt = enteredCharge.JR_LocalCostAmt;

								chargeToUpdate.JR_AT_CostGSTRate = enteredCharge.JR_AT_CostGSTRate;
								chargeToUpdate.SetCostTaxDateSafe(enteredCharge.JR_CostTaxDate);
								chargeToUpdate.JR_A9_CostVATClass = enteredCharge.JR_A9_CostVATClass;
								chargeToUpdate.JR_AW_CostWHTRate = enteredCharge.JR_AW_CostWHTRate;

								chargeToUpdate.JR_OSCostGSTAmt_Calc = enteredCharge.JR_OSCostGSTAmt_Calc;

								chargeToUpdate.ReverseAccrual(GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine), true);

								chargeToUpdate.JR_AL_APLine = invoiceLine.PK;
								ChargesToSearch.RemoveChargeByPK(chargeToUpdate.PK);
								chargeToUpdate.JR_APInvoiceNum = invoice.AH_TransactionNum;
								chargeToUpdate.JR_APInvoiceDate = invoice.AH_InvoiceDate;
								chargeToUpdate.JR_APDocumentReceivedDate = invoice.AH_DocumentReceivedDate;
								chargeToUpdate.JR_PaymentDate = invoice.AH_DueDate;
								chargeToUpdate.JR_CostReference = invoice.AH_ChequeOrReference;
								chargeToUpdate.JR_CostGovtChargeCode = invoiceLine.AL_GovtChargeCode; //We are taking it from the InvoiceLine as user can override the Value in Invoice Line after Importing the Consol Cost.

								invoiceLine.ApportionmentChargeImportedFrom = chargeToUpdate;
							}
						}
					}
				}
				catch (NullReferenceException e) when (invoice.Lines.Cast<InvoicingLineBase>().Any(x => x.IsLinkedChargeDeleted))
				{
					throw new NullReferenceException("Exception due to charge linked to line deleted. We are here most likely because we post invoice ignoring validation errors. If so, to fix this particular issue add validation call and error check before invoice saving. To fix original issue investigate error key ApportionmentChargeImportedFromIsDeleted.", e);
				}
				if (!chargeFound)
				{
					chargesInEnteredCostButNotUpdated.Add(enteredCharge);
				}
			}

			foreach (ApportionSplitCharge chargeInEnteredCost in chargesInEnteredCostButNotUpdated)
			{
				enteredCost.ApportionmentCharges.Remove(chargeInEnteredCost);
				chargeInEnteredCost.JR_E6 = costToUpdate.PK;
				costToUpdate.ApportionmentCharges.Add(chargeInEnteredCost);
			}

			foreach (ApportionSplitCharge chargeInUpdatedCost in costToUpdate.ApportionmentCharges)
			{
				if (!updatedCharges.Contains(chargeInUpdatedCost) && chargeInUpdatedCost.IsInDatabase)
				{
					chargeInUpdatedCost.ClearCostAmount();
				}
			}

			costToUpdate.RemoveNonApplicableCharges();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void ProcessApportionedLine(InvoicingLineBase invoiceLine, Job associatedJob)
		{
			var apportionedCharge = invoiceLine.ApportionmentChargeImportedFrom;
			if (apportionedCharge != null)
			{
				var possibleChargesToCalculateCarryForwardAmount = GetPossibleChargesToCalculateCarryForwardAmountAndUpdate(apportionedCharge, invoiceLine);

				if (apportionedCharge.IsInDatabase)
				{
					ProcessApportionChargeThatExistsInDB(invoiceLine, associatedJob, apportionedCharge, possibleChargesToCalculateCarryForwardAmount);
				}
				else
				{
					ProcessApportionChargeThatDoesNotExistInDB(invoiceLine, associatedJob, apportionedCharge, possibleChargesToCalculateCarryForwardAmount);
				}
			}
		}

		void ProcessApportionChargeThatExistsInDB(InvoicingLineBase invoiceLine, Job associatedJob, ApportionSplitCharge apportionedCharge, Charge[] possibleChargesToCalculateCarryForwardAmount)
		{
			if (invoiceLine.IsPopulatedFromImportedJobCharge)
			{
				possibleChargesToCalculateCarryForwardAmount = Array.Empty<Charge>();
			}

			var apportionedChargeAsCharge = Factory.Load<Charge>(apportionedCharge.PK);

			var shouldUseApportionedChargeCurrency = apportionedCharge.JR_RX_NKCostCurrency == (ZString)apportionedCharge.JR_RX_NKCostCurrencyInfo.OriginalValue;
			var shouldUseOSCurrencyAndLineExchangeRate = shouldUseApportionedChargeCurrency && ShouldUseOSCurrencyAndLineExchangeRate(invoiceLine, possibleChargesToCalculateCarryForwardAmount);

			var shouldUseOSCurrency = shouldUseOSCurrencyAndLineExchangeRate;

			if (!shouldUseOSCurrency)
			{
				var isExRateOverrodeByExRateOption = false;
				if (invoiceLine.AL_RX_NKTransactionCurrency != invoiceLine.Company.GC_RX_NKLocalCurrency
					&& ExchangeRateCalculator.IsExRateOptionApplicable(invoiceLine.GetExRateLedger(), invoiceLine.TransactionHeader.AH_RX_NKTransactionCurrency == invoiceLine.Company.GC_RX_NKLocalCurrency, invoiceLine.AL_GC))
				{
					isExRateOverrodeByExRateOption = (apportionedCharge.JR_RX_NKCostCurrency == invoiceLine.AL_RX_NKTransactionCurrency
						&& apportionedCharge.JR_OSCostExRate == invoiceLine.AL_ExchangeRate);
				}

				shouldUseOSCurrency = shouldUseApportionedChargeCurrency
					&& (isExRateOverrodeByExRateOption || apportionedCharge.JR_OSCostExRate == (ZDecimal)apportionedCharge.JR_OSCostExRateInfo.OriginalValue)
					&& AmountWrapper.IsOnlyOneOSCurrencyAndRate(invoiceLine, apportionedChargeAsCharge);
			}

			var originalOSAmount = (ZDecimal)apportionedCharge.JR_OSCostAmtInfo.OriginalValue;
			ZDecimal originalLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal((ZDecimal)apportionedCharge.JR_OSCostAmtInfo.OriginalValue,
												shouldUseOSCurrencyAndLineExchangeRate ? invoiceLine.AL_ExchangeRate : (ZDecimal)apportionedCharge.JR_OSCostExRateInfo.OriginalValue);

			var originalAmount = new AmountWrapper(originalLocalAmount, originalOSAmount, shouldUseOSCurrency);

			var invoiceLineAsCreditNoteLine = invoiceLine as APCreditNoteLine;
			var carryForwardAmount = !IsNegativeAccrualBehaviorsEnabled && invoiceLineAsCreditNoteLine != null ? new AmountWrapper() :
											originalAmount + CalculateCarryForwardAmount(invoiceLine, possibleChargesToCalculateCarryForwardAmount, true);
			var carryForwardAmountForChargeToUpdate = !IsNegativeAccrualBehaviorsEnabled && invoiceLineAsCreditNoteLine != null ? new AmountWrapper() :
								originalAmount + CalculateCarryForwardAmountForChargeToUpdate(invoiceLine, possibleChargesToCalculateCarryForwardAmount, false, true);

			UpdateExistingCharge(invoiceLine, apportionedChargeAsCharge);
			if (possibleChargesToCalculateCarryForwardAmount.Length == 0)
			{
				UpdateOtherForSingleCharge(invoiceLine, associatedJob, apportionedChargeAsCharge,
					carryForwardAmount, carryForwardAmountForChargeToUpdate, true, true, false, true);
			}
			else if (possibleChargesToCalculateCarryForwardAmount.Length >= 1)
			{
				var possibleChargeList = possibleChargesToCalculateCarryForwardAmount.ToList();
				ReorderChargesToPlaceTheBestCandidateCarryForwardChargeOnTop(possibleChargeList, 0, HasCarryForwardAmountForTheCreditor(invoiceLine, carryForwardAmountForChargeToUpdate));
				possibleChargesToCalculateCarryForwardAmount = possibleChargeList.ToArray();

				UpdateOtherForMultipleCharges(invoiceLine, associatedJob, apportionedChargeAsCharge, possibleChargesToCalculateCarryForwardAmount,
					carryForwardAmount, carryForwardAmountForChargeToUpdate, true, false, 0, true);
			}
		}

		void ProcessApportionChargeThatDoesNotExistInDB(InvoicingLineBase invoiceLine, Job associatedJob, ApportionSplitCharge apportionedCharge, Charge[] possibleChargesToCalculateCarryForwardAmount)
		{
			var chargeToUpdate = CreateNewApportionedCharge(associatedJob, apportionedCharge, invoiceLine, possibleChargesToCalculateCarryForwardAmount.Length == 0);

			if (possibleChargesToCalculateCarryForwardAmount.Length > 0)
			{
				var carryForwardAmount = CalculateCarryForwardAmount(invoiceLine, possibleChargesToCalculateCarryForwardAmount);
				var carryForwardAmountForChargeToUpdate = CalculateCarryForwardAmountForChargeToUpdate(invoiceLine, possibleChargesToCalculateCarryForwardAmount);
				var doesOriginalAccrualExist = possibleChargesToCalculateCarryForwardAmount.Any(x => IsNegativeAccrualBehaviorsEnabled ? x.JR_LocalCostAmt != 0 : x.JR_LocalCostAmt > 0m);

				ReorderChargesToPlaceTheBestCandidateCarryForwardChargeOnTop(possibleChargesToCalculateCarryForwardAmount.ToList(), 0, HasCarryForwardAmountForTheCreditor(invoiceLine, carryForwardAmountForChargeToUpdate));
				UpdateOtherForMultipleCharges(invoiceLine, associatedJob, chargeToUpdate, possibleChargesToCalculateCarryForwardAmount.ToArray(),
					carryForwardAmount, carryForwardAmountForChargeToUpdate, doesOriginalAccrualExist, false, 0, isCarryForwardApportionCharge: possibleChargesToCalculateCarryForwardAmount.Any(x => x.ParentConsolCost != null));
			}

			apportionedCharge.Delete();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		Charge[] GetPossibleChargesToCalculateCarryForwardAmountAndUpdate(ApportionSplitCharge apportionedCharge, InvoicingLineBase line)
		{
			var possibleChargesToUpdate = ChargesToSearch.Charges.Where(x =>
				x.JR_JH == apportionedCharge.JR_JH &&
				x.JR_AC == apportionedCharge.JR_AC &&
				x.JR_GB == apportionedCharge.JR_GB &&
				x.JR_GE == apportionedCharge.JR_GE &&
				(x.JR_E6.IsEmpty || x.ParentConsolCost?.E6_ParentID == apportionedCharge.ParentConsolCost.E6_ParentID) &&
				x.PK != apportionedCharge.PK);

			if (!IsBringForwardAgainstCreditorEnabled)
			{
				possibleChargesToUpdate = possibleChargesToUpdate.Where(x => x.JR_E6.IsEmpty);
			}

			if (!IsNegativeAccrualBehaviorsEnabled && line is APCreditNoteLine)
			{
				possibleChargesToUpdate = possibleChargesToUpdate.Where(x => x.JR_OSCostAmt < 0m);
			}
			if (IsBringForwardAgainstCreditorEnabled)
			{
				possibleChargesToUpdate = possibleChargesToUpdate.Where(x => x.JR_OH_CostAccount == line.InvoiceBase.AH_OH || x.JR_OH_CostAccount.IsEmpty);
				possibleChargesToUpdate = possibleChargesToUpdate.OrderByDescending(x => x.JR_OH_CostAccount).ThenBy(x => x.JR_DisplaySequence);
			}
			else
			{
				possibleChargesToUpdate = possibleChargesToUpdate.OrderBy(x => x.JR_DisplaySequence);
			}

			var possibleApportionedChargesToUpdate = possibleChargesToUpdate.Where(x => x.ParentConsolCost?.E6_ParentID == apportionedCharge.ParentConsolCost.E6_ParentID);

			var result = (apportionedCharge.IsInDatabase ||
						possibleApportionedChargesToUpdate.Any() ||
						possibleChargesToUpdate.Any(x => CarryForwardCostCreator.IsItACarryForwardApportionCharge(x)))
						? possibleApportionedChargesToUpdate.ToArray()
						: possibleChargesToUpdate.ToArray();

			return result;
		}

		/// <summary>
		/// Main Purpose of this funciton is to put TWO most suitable charges at the top of the collection so that carry forward amounts can be assigned to them.
		/// 1. If there is at least one charge in the collection with a valid creditor and carry forward amount for that valid creditor is a positive value, then a charge with that valid creditor will be placed at posiiton 1
		/// 2. If a charge is found for position 1 by satisfying the condition described in point 1, then a charge with an empty creditor will be placed at position 2. Otherwise it will be in the position 1.
		/// </summary>
		/// <param name="charges"></param>
		/// <param name="startIndex"></param>
		/// <param name="CarryForwardAmountForChargeToUpdate">carry forward amount for the valid creditor</param>
		void ReorderChargesToPlaceTheBestCandidateCarryForwardChargeOnTop(List<Charge> charges, int startIndex, bool hasCarryForwardAmountForTheCreditor)
		{
			var index = startIndex;

			Action<Func<Charge, bool>[]> repositionChargeInCollection = (expGroup) =>
		   {
			   foreach (var fallbackExpression in expGroup)
			   {
				   var bestMatchedCharge = charges.Skip(startIndex).FirstOrDefault(fallbackExpression);
				   if (bestMatchedCharge != null)
				   {
					   charges.Remove(bestMatchedCharge);
					   charges.Insert(index, bestMatchedCharge);
					   index++;
					   break;
				   }
			   }
		   };

			if (hasCarryForwardAmountForTheCreditor)
			{
				//Expressions for getting the best matched carry forward charge that has a creditor
				var expressionGroupForChargesWithCreditor = new Func<Charge, bool>[] { (x => !x.JR_OH_CostAccount.IsEmpty && x.JR_E6.IsEmpty), x => !x.JR_OH_CostAccount.IsEmpty };

				repositionChargeInCollection(expressionGroupForChargesWithCreditor);
			}

			//Expressions for getting the best matched carry forward charge that doesn't have a creditor
			var expressionGroupForChargesWithEmptyCreditor = new Func<Charge, bool>[] { (x => x.JR_OH_CostAccount.IsEmpty && x.JR_E6.IsEmpty), x => x.JR_OH_CostAccount.IsEmpty };

			repositionChargeInCollection(expressionGroupForChargesWithEmptyCreditor);
		}

#if DEBUG
		public
#endif
		Charge CreateNewApportionedCharge(Job associatedJob, ApportionSplitCharge apportionedCharge, InvoicingLineBase invoiceLine, bool setSellAmountFromCost)
		{
			Charge toCharge;
			using (associatedJob.ChargesLoadSuspender.GetSuspender())
			{
				toCharge = associatedJob.Charges.AddNew();
			}

			using (toCharge.ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender())
			{
				toCharge.JR_E6 = apportionedCharge.JR_E6;
				toCharge.JR_AC = apportionedCharge.JR_AC;
				if (ObjectFactory.Get<IElectronicProcessingChargeProvider>().ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(toCharge.JR_AC, associatedJob.JH_GC))
				{
					toCharge.JR_Desc = apportionedCharge.JR_Desc;
				}
				toCharge.JR_OH_CostAccount = invoiceLine.InvoiceBase.AH_OH;
				toCharge.JR_CostPlaceOfSupply = invoiceLine.AL_PlaceOfSupply; //We are taking it from the InvoiceLine as user can override the Value in Invoice Line after Importing the Consol Cost.
				toCharge.JR_CostSupplyType = invoiceLine.AL_SupplyType;
				toCharge.JR_GB_CostTaxBranch = invoiceLine.AL_GB_TaxBranch;
				toCharge.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);
				toCharge.JR_AT_CostGSTRate = apportionedCharge.JR_AT_CostGSTRate;
				toCharge.SetCostTaxDateSafe(apportionedCharge.JR_CostTaxDate);
				toCharge.JR_A9_CostVATClass = apportionedCharge.JR_A9_CostVATClass;
				toCharge.JR_AW_CostWHTRate = apportionedCharge.JR_AW_CostWHTRate;
				toCharge.JR_RX_NKCostCurrency = apportionedCharge.JR_RX_NKCostCurrency;
				toCharge.JR_OSCostExRate = apportionedCharge.JR_OSCostExRate;
				toCharge.JR_GE = apportionedCharge.JR_GE;
				toCharge.JR_GB = apportionedCharge.JR_GB;
				toCharge.JR_CostGovtChargeCode = invoiceLine.AL_GovtChargeCode; //We are taking it from the InvoiceLine as user can override the Value in Invoice Line after Importing the Consol Cost.
				toCharge.JR_SellGovtChargeCode = apportionedCharge.JR_SellGovtChargeCode; //for Sell govt charge code we are taking it from the sell govt charge code field in the apportionment.
			}

			Func<IDisposable> getCalculationSuspender =
				() => setSellAmountFromCost
					? toCharge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations()
					: toCharge.Calculations.SuspendCalculations();

			using (SetNewChargeCostIsGoingToBePostedInTransformerContext(toCharge.Factory))
			using (getCalculationSuspender())
			{
				AddExchangeRate(apportionedCharge, associatedJob, invoiceLine.InvoiceBase);
				UpdateChargeAmountsWithCorrectSign(toCharge, apportionedCharge, invoiceLine);

				if (!ShouldCreateWIPForNewCharge(toCharge.JR_OSSellAmt, apportionedCharge.IsFinal))
				{
					using (toCharge.Calculations.SuspendCalculations())
					{
						toCharge.JR_OSSellAmt = 0;
						toCharge.JR_LocalSellAmt = 0;
						toCharge.JR_OSSellWHTAmt = 0;

						AddExchangeRate(apportionedCharge, associatedJob, invoiceLine.InvoiceBase);
						UpdateChargeAmountsWithCorrectSign(toCharge, apportionedCharge, invoiceLine);
					}
				}
			}

			SetSellExchangeRateIfZero(toCharge, invoiceLine);

			SetAPInvoiceValues(toCharge, invoiceLine.InvoiceBase);
			toCharge.JR_AL_APLine = invoiceLine.PK;
			toCharge.SetEstimatedCost(0);

			invoiceLine.ApportionmentChargeImportedFrom = Factory.Load<ApportionSplitCharge>(toCharge.PK);

			return toCharge;
		}

		void AddExchangeRate(ApportionSplitCharge fromCharge, Job parentJob, InvoicingBase invoice)
		{
			if (!fromCharge.JR_RX_NKCostCurrency.IsEmpty && fromCharge.JR_RX_NKCostCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				if (parentJob != null)
				{
					var ledger = ExchangeRateEnumsExtensions.GetLedgerFromCode(invoice.AH_Ledger);
					var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(invoice.Company, ledger, fromCharge);
					var rate = ((IExchangeRateProvider)parentJob).GetExchangeRate(fromCharge.CostCurrency.RX_Code, invoice.AH_OH, ledger, invoiceCurrencyType: invoiceCurrencyType)
						as ExchangeRateWrapper;

					if (rate == null)
					{
						if (fromCharge.JR_RX_NKCostCurrency.IsEmpty)
						{
							ErrorReporter.ReportOnce(GetType() + " Cost Currency", "_" + ApportionSplitCharge.Schema.JR_RX_NKCostCurrency + " is not valid! Cost Currency: " + fromCharge.JR_RX_NKCostCurrency);
						}

						parentJob.AddCurrency(RefCurrency.LoadFromCurrencyCode(this.Factory, invoice.AH_RX_NKTransactionCurrency), fromCharge.JR_OSCostExRate, invoice.AH_OH, ExchangeRateEnumsExtensions.GetLedgerFromCode(invoice.AH_Ledger), invoiceCurrencyType);
					}
					else
					{
						if (rate.Rate.IsEmpty)
						{
							rate.SetBaseRate(fromCharge.JR_OSCostExRate);
						}
					}
				}
			}
		}

		void UpdateOtherForSingleCharge(InvoicingLineBase invoiceLine, Job associatedJob, Charge associatedCharge,
			AmountWrapper carryForwardAmount, AmountWrapper carryForwardAmountForChargeToUpdate,
			bool doesOriginalAccrualExist, bool associatedCharge_IsRevenuePosted, bool createWIPWhenCostIsFinal, bool isCarryForwardApportionCharge = false)
		{
			var isFinal = IsFinalInvoice(invoiceLine);
			var createWIPWhenOriginalAccrualNotExist = (IsNegativeAccrualBehaviorsEnabled ? carryForwardAmount != 0 : carryForwardAmount < 0) && !doesOriginalAccrualExist;
			if (!isFinal)
			{
				if (createWIPWhenOriginalAccrualNotExist)
				{
					CreateWIPWhenCostIsGreaterThanAccrual(invoiceLine, associatedJob, associatedCharge, -carryForwardAmount);
				}
				else if (carryForwardAmount != 0 && (HasCarryForwardAmount(invoiceLine, carryForwardAmount) || (ShouldCreateWIP(invoiceLine) && associatedCharge_IsRevenuePosted)))
				{
					var shouldPreventCarryForwardWIPCreation = associatedCharge.IsManualJobAccrualCharge && carryForwardAmount < 0 && !NegativeReAccrualChecker.CanApplyNegativeReaccrual(invoiceLine);
					if (!shouldPreventCarryForwardWIPCreation)
					{
						var charge = CreateCarryForwardCharge(invoiceLine, associatedJob, associatedCharge, carryForwardAmount, HasCarryForwardAmountForTheCreditor(invoiceLine, carryForwardAmountForChargeToUpdate));

						if (isCarryForwardApportionCharge && HasCarryForwardAmount(invoiceLine, carryForwardAmount) && charge != null)
						{
							CarryForwardCostCreator.AddTocarryForwardApportionChargePKList(charge, associatedCharge.ParentConsolCost.E6_ParentID, associatedCharge.ParentConsolCost.E6_ParentTableCode);
						}
					}
				}
				else if (carryForwardAmount < 0 && ShouldCreateWIP(invoiceLine))
				{
					CreateWIPWhenCostIsGreaterThanAccrual(invoiceLine, associatedJob, associatedCharge, -carryForwardAmount);
				}
			}
			else if (createWIPWhenCostIsFinal && createWIPWhenOriginalAccrualNotExist)
			{
				CreateWIPWhenCostIsGreaterThanAccrual(invoiceLine, associatedJob, associatedCharge, -carryForwardAmount);
			}
		}

		void UpdateOtherForMultipleCharges(InvoicingLineBase invoiceLine, Job associatedJob, Charge chargeWasUpdated, Charge[] associatedCharges,
					AmountWrapper carryForwardAmount, AmountWrapper carryForwardAmountForChargeToUpdate,
					bool doesOriginalAccrualExist, bool createWIPWhenCostIsFinal, int reverseChargesStartingFromIndex, bool isCarryForwardApportionCharge = false)
		{
			var isFinal = IsFinalInvoice(invoiceLine);
			var createWIPWhenOriginalAccrualNotExist = (IsNegativeAccrualBehaviorsEnabled ? carryForwardAmount != 0 : carryForwardAmount < 0) && !doesOriginalAccrualExist;
			var totalLocalEstimatedCost = associatedCharges.Skip(reverseChargesStartingFromIndex).Sum(x => Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(x.JR_EstimatedCost, x.JR_OSCostExRate));
			if (!isFinal)
			{
				if (createWIPWhenOriginalAccrualNotExist)
				{
					var chargeToCarryForward = GetNextChargeWithUnPostedRevenue(associatedCharges);
					CreateWIPWhenCostIsGreaterThanAccrual(invoiceLine, associatedJob, chargeToCarryForward, -carryForwardAmount);
				}
				else
				{
					if (HasCarryForwardAmountForTheCreditor(invoiceLine, carryForwardAmountForChargeToUpdate))
					{
						if (associatedCharges[reverseChargesStartingFromIndex].JR_OH_CostAccount == invoiceLine.InvoiceBase.AH_OH)
						{
							UpdateCarryForwardCharge(invoiceLine, associatedCharges[reverseChargesStartingFromIndex], carryForwardAmountForChargeToUpdate, true);
							if (isCarryForwardApportionCharge)
							{
								CarryForwardCostCreator.AddTocarryForwardApportionChargePKList(associatedCharges[reverseChargesStartingFromIndex], associatedCharges[reverseChargesStartingFromIndex].ParentConsolCost.E6_ParentID, associatedCharges[reverseChargesStartingFromIndex].ParentConsolCost.E6_ParentTableCode);
							}
							reverseChargesStartingFromIndex++;
						}
						else
						{
							var newCarryForwardCharge = CreateCarryForwardCharge(invoiceLine, associatedJob, associatedCharges[reverseChargesStartingFromIndex], carryForwardAmountForChargeToUpdate, true);
							if (isCarryForwardApportionCharge)
							{
								CarryForwardCostCreator.AddTocarryForwardApportionChargePKList(newCarryForwardCharge, chargeWasUpdated.ParentConsolCost.E6_ParentID, chargeWasUpdated.ParentConsolCost.E6_ParentTableCode);
							}
						}
						carryForwardAmount -= carryForwardAmountForChargeToUpdate;
					}
					if (reverseChargesStartingFromIndex < associatedCharges.Length)
					{
						if (HasCarryForwardAmount(invoiceLine, carryForwardAmount))
						{
							UpdateCarryForwardCharge(invoiceLine, associatedCharges[reverseChargesStartingFromIndex], carryForwardAmount, false);
							if (isCarryForwardApportionCharge)
							{
								CarryForwardCostCreator.AddTocarryForwardApportionChargePKList(associatedCharges[reverseChargesStartingFromIndex], associatedCharges[reverseChargesStartingFromIndex].ParentConsolCost.E6_ParentID, associatedCharges[reverseChargesStartingFromIndex].ParentConsolCost.E6_ParentTableCode);
							}
							reverseChargesStartingFromIndex++;
						}
						else if (!NegativeReAccrualChecker.CanApplyNegativeReaccrual(invoiceLine) && carryForwardAmount < 0 && ShouldCreateWIP(invoiceLine))
						{
							var chargeToCarryForward = GetNextChargeWithUnPostedRevenue(associatedCharges);
							CreateWIPWhenCostIsGreaterThanAccrual(invoiceLine, associatedJob, chargeToCarryForward, -carryForwardAmount);
						}
					}
				}
			}
			else if (createWIPWhenCostIsFinal && createWIPWhenOriginalAccrualNotExist)
			{
				var chargeToCarryForward = GetNextChargeWithUnPostedRevenue(associatedCharges);
				CreateWIPWhenCostIsGreaterThanAccrual(invoiceLine, associatedJob, chargeToCarryForward, -carryForwardAmount);
			}

			ReverseOtherCharges(invoiceLine, chargeWasUpdated, associatedCharges, reverseChargesStartingFromIndex, totalLocalEstimatedCost);

			RemoveEmptyCharges(invoiceLine, associatedCharges);
		}

		void ReverseOtherCharges(InvoicingLineBase invoiceLine, Charge chargeWasUpdated, Charge[] associatedCharges, int reverseChargesStartingFromIndex, decimal totalLocalEstimatedCost)
		{
			Charge otherCharge;
			for (int i = reverseChargesStartingFromIndex; i < associatedCharges.Length; i++)
			{
				otherCharge = associatedCharges[i];
				using (otherCharge.Calculations.SuspendCalculations())
				{
					ResetAPInvoiceRelatedValues(otherCharge);
					otherCharge.JR_OSCostAmt = 0M;
					otherCharge.JR_LocalCostAmt = 0M;
					otherCharge.SetEstimatedCost(0);
					otherCharge.JR_OH_CostAccount = ZGuid.Empty;
					otherCharge.ReverseAccrual(GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine), true);
				}
			}
			var postedChargeAggregatedEstimatedCost = chargeWasUpdated.JR_EstimatedCost +
				Env.CurrentCompany.ExchangeRate.LocalToForeignWithoutRounding(totalLocalEstimatedCost, chargeWasUpdated.JR_OSCostExRate, chargeWasUpdated.JR_RX_NKCostCurrency);
			chargeWasUpdated.SetEstimatedCost(postedChargeAggregatedEstimatedCost);
		}

		void RemoveEmptyCharges(InvoicingLineBase invoiceLine, Charge[] associatedCharges)
		{
			for (int i = associatedCharges.Length - 1; i >= 0; i--)
			{
				var otherCharge = associatedCharges[i];
				if (!otherCharge.IsRevenuePosted && otherCharge.JR_OSCostAmt == 0 && otherCharge.JR_OSSellAmt == 0 && otherCharge.JR_LocalCostAmt == 0 && otherCharge.JR_LocalSellAmt == 0 && !otherCharge.IsCommentChargeCode)
				{
					otherCharge.ReverseAccrual(GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine), true);
					otherCharge.ReverseWIP(GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine), true);
					ChargesToSearch.RemoveChargeByPK(otherCharge.PK);
					otherCharge.Delete();
				}
			}
		}

		Charge GetNextChargeWithUnPostedRevenue(Charge[] associatedCharges)
		{
			var chargeToCarryForward = associatedCharges[0];

			for (int i = 0; i < associatedCharges.Length; i++)
			{
				if (!associatedCharges[i].IsRevenuePosted)
				{
					chargeToCarryForward = associatedCharges[i];
					break;
				}
			}

			return chargeToCarryForward;
		}

		AmountWrapper CalculateCarryForwardAmount(InvoicingLineBase aPInvoiceLine, Charge[] charges, bool shouldIgnoreEmptyCharges = false)
		{
			var shouldUseOSCurrencyAndLineExchangeRate = ShouldUseOSCurrencyAndLineExchangeRate(aPInvoiceLine, charges);
			var invoiceAmountInLocalCurrency = -aPInvoiceLine.AL_LineAmount;
			decimal chargeAmoutInLocalCurrency = 0;
			foreach (Charge charge in charges)
			{
				chargeAmoutInLocalCurrency += Env.CurrentCompany.ExchangeRate.ForeignToLocal(charge.JR_OSCostAmt, shouldUseOSCurrencyAndLineExchangeRate ? aPInvoiceLine.AL_ExchangeRate : charge.JR_OSCostExRate);
			}

			var localAmt = chargeAmoutInLocalCurrency - invoiceAmountInLocalCurrency;

			var oSAmt = 0m;
			var shouldUseOSCurrency = shouldUseOSCurrencyAndLineExchangeRate || AmountWrapper.IsOnlyOneOSCurrencyAndRate(aPInvoiceLine, charges, shouldIgnoreEmptyCharges);
			ZGuid infoCollectionID = ZGuid.NewZGuid();

			if (shouldUseOSCurrency)
			{
				decimal invoiceAmountInForeignCurrency = aPInvoiceLine.AL_DBAH_OSExTaxAmount;
				decimal chargeAmoutInForeignCurrency = 0;
				foreach (Charge charge in charges)
				{
					chargeAmoutInForeignCurrency += charge.JR_OSCostAmt;
				}
				oSAmt = chargeAmoutInForeignCurrency - invoiceAmountInForeignCurrency;

				Func<ZString> infoCollectorStringResult = () => FormattableString.Invariant(
$@"{nameof(IsCarryForwardAccrualBasedOnOSAmount)}: {IsCarryForwardAccrualBasedOnOSAmount}
{nameof(shouldUseOSCurrencyAndLineExchangeRate)}: {shouldUseOSCurrencyAndLineExchangeRate}
{nameof(invoiceAmountInLocalCurrency)}: {invoiceAmountInLocalCurrency}
{nameof(chargeAmoutInLocalCurrency)}: {chargeAmoutInLocalCurrency}
{nameof(invoiceAmountInForeignCurrency)}: {invoiceAmountInForeignCurrency}
{nameof(chargeAmoutInForeignCurrency)}: {chargeAmoutInForeignCurrency}");
				AccountingCriticalValidationDependencyFactory.GetAccountingCriticalValidationInfoCollectionHelper().AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(infoCollectionID, aPInvoiceLine, charges, localAmt, oSAmt, infoCollectorStringResult);
			}

			return new AmountWrapper(localAmt, oSAmt, shouldUseOSCurrency, infoCollectionID);
		}

		ZBool ShouldUseOSCurrencyAndLineExchangeRate(InvoicingLineBase invoiceLine, Charge[] charges)
		{
			return IsCarryForwardAccrualBasedOnOSAmount &&
				invoiceLine.IsPopulatedFromImportedJobCharge &&
				invoiceLine.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
				(charges.IsNullOrEmpty() || charges.All(x => x.JR_RX_NKCostCurrency == invoiceLine.AL_RX_NKTransactionCurrency));
		}

		ZBool ShouldUseOSCurrencyAndLineExchangeRate(InvoicingLineBase invoiceLine, Charge charge)
		{
			return ShouldUseOSCurrencyAndLineExchangeRate(invoiceLine, new Charge[] { charge });
		}

		AmountWrapper CalculateCarryForwardAmountForChargeToUpdate(InvoicingLineBase aPInvoiceLine, Charge[] chargeToUpdate, bool doesItIncludeOriginalChargeCreditorDifferentToInvoiceOne = false, bool shouldIgnoreEmptyCharges = false)
		{
			List<Charge> chargeList = new List<Charge>();
			foreach (Charge charge in chargeToUpdate)
			{
				if (charge.JR_OH_CostAccount == aPInvoiceLine.InvoiceBase.AH_OH ||
					doesItIncludeOriginalChargeCreditorDifferentToInvoiceOne && charge.JR_OH_CostAccount == aPInvoiceLine.OriginalJobCharge.JR_OH_CostAccount)
				{
					chargeList.Add(charge);
				}
			}
			return CalculateCarryForwardAmount(aPInvoiceLine, chargeList.ToArray(), shouldIgnoreEmptyCharges);
		}

		bool ShouldCreateWIP(InvoicingLineBase invoiceLine)
		{
			return !NegativeReAccrualChecker.CanApplyNegativeReaccrual(invoiceLine) && AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		static bool ShouldCreateWIPForNewCharge(InvoicingLineBase line)
		{
			bool isFinal = (line is APInvoiceLine || line is APCreditNoteLine) && line.AL_IsFinalCharge;
			return ShouldCreateWIPForNewCharge(line.AL_DBAH_OSExTaxAmount, isFinal);
		}

		static bool ShouldCreateWIPForNewCharge(ZDecimal amount, bool isFinal)
		{
			return (amount > 0 || IsNegativeAccrualBehaviorsEnabled) &&
				(!isFinal || AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsFinal.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
		}

		#region Create or Update Charges

#if DEBUG
		public
#endif
		void CreateNewCharge(InvoicingLineBase invoiceLine, Job associatedJob)
		{
			Charge newCharge;
			using (associatedJob.ChargesLoadSuspender.GetSuspender())
			{
				newCharge = associatedJob.Charges.AddNew();
			}

			#region SetPlugInDataOnJob
			GenericJob.GenericJob genericJob;
			if (newCharge.InvoicingJob.PlugInData == null)
			{
				genericJob = newCharge.InvoicingJob.GenericJobView;
				if (genericJob != null)
				{
					newCharge.InvoicingJob.PlugInData = genericJob.Consumer;
				}
			}
			#endregion

			SetStandardValues(newCharge, invoiceLine);
			SetRelatedJobNumber(newCharge, invoiceLine);
			newCharge.JR_OH_CostAccount = invoiceLine.InvoiceBase.AH_OH;
			newCharge.JR_RX_NKCostCurrency = invoiceLine.AL_RX_NKTransactionCurrency;

			using (SetNewChargeCostIsGoingToBePostedInTransformerContext(newCharge.Factory))
			using (newCharge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations())
			{
				newCharge.JR_OSCostAmt = invoiceLine.AL_DBAH_OSExTaxAmount; // must come before setting CostExRate
				newCharge.JR_AL_APLine = invoiceLine.PK;
				newCharge.JR_OSCostExRate = invoiceLine.AL_ExchangeRate;
				newCharge.JR_LocalCostAmt = -invoiceLine.AL_LineAmount;
			}

			newCharge.JR_GB_CostTaxBranch = invoiceLine.AL_GB_TaxBranch;
			newCharge.JR_AT_CostGSTRate = invoiceLine.AL_AT;
			newCharge.SetCostTaxDateSafe(invoiceLine.AL_TaxDate);
			newCharge.JR_A9_CostVATClass = invoiceLine.AL_A9_VATClass;
			newCharge.JR_AW_CostWHTRate = invoiceLine.AL_AW;

			if (!newCharge.SetNZEntryFeeChargeTaxAmountSuspender.IsSuspended &&
			!NZCustomsEntryFeeTaxCalculator.IsEntryFeeChargeWithCorrectAmount(invoiceLine))
			{
				using (newCharge.SetNZEntryFeeChargeTaxAmountSuspender.GetSuspender())
				{
					newCharge.JR_OSCostGSTAmt_Calc = invoiceLine.AL_DBAH_OSTaxAmount;
				}
			}
			else
			{
				newCharge.JR_OSCostGSTAmt_Calc = invoiceLine.AL_DBAH_OSTaxAmount;
			}

			if (!ShouldCreateWIPForNewCharge(invoiceLine))
			{
				using (newCharge.Calculations.SuspendCalculations())
				{
					newCharge.JR_OSSellAmt = 0M;
					newCharge.JR_LocalSellAmt = 0M;
					newCharge.JR_EstimatedRevenue = 0M;
				}
			}

			newCharge.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);

			SetSellExchangeRateIfZero(newCharge, invoiceLine);

			SetAPInvoiceValues(newCharge, invoiceLine.InvoiceBase);
			newCharge.SetEstimatedCost(0);
		}

		Charge CreateCarryForwardCharge(InvoicingLineBase invoiceLine, Job associatedJob, Charge associatedCharge, AmountWrapper carryForwardAmount, bool bringForwardAgainstCreditor)
		{
			Charge newCharge;
			using (associatedJob.ChargesLoadSuspender.GetSuspender())
			{
				newCharge = associatedJob.Charges.AddNew();
			}
			ChargesToSearch.AddCharge(newCharge);
			using (newCharge.Calculations.SuspendCalculations())
			{
				SetStandardValues(newCharge, invoiceLine);
				SetRelatedJobNumber(newCharge, invoiceLine);

				if (bringForwardAgainstCreditor || AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value)
				{
					newCharge.JR_OH_CostAccount = invoiceLine.InvoiceBase.AH_OH;
					newCharge.JR_CostPlaceOfSupply = invoiceLine.AL_PlaceOfSupply;
				}
				else
				{
					newCharge.JR_OH_CostAccount = ZGuid.Empty;
				}

				var shouldUseOSCurrency = carryForwardAmount.IsOSCurrencyApplicable;

				var invoiceCurrencyTypeForAR = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(invoiceLine.Company, ExchangeRateValidLedgerEnum.AR, invoiceLine.TransactionHeader.AH_RX_NKTransactionCurrency);
				var jobExchangeRate = associatedJob.GetExchangeRate(invoiceLine.AL_RX_NKTransactionCurrency, newCharge.JR_OH_CostAccount, ExchangeRateOrgTypeEnum.Creditor, ExchangeRateType.Buy, InvoiceCurrencyType.NotApplicable);
				var jobExchangeRateSell = associatedJob.GetExchangeRate(invoiceLine.AL_RX_NKTransactionCurrency, newCharge.JR_OH_SellAccount, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateType.Sell, invoiceCurrencyTypeForAR);
				var shouldUseJobExchangeRate = jobExchangeRate != 0 && jobExchangeRate != invoiceLine.AL_ExchangeRate && !ShouldUseOSCurrencyAndLineExchangeRate(invoiceLine, associatedCharge);
				var sellRateBefore = ZDecimal.Zero;

				if (shouldUseOSCurrency)
				{
					newCharge.JR_RX_NKCostCurrency = invoiceLine.AL_RX_NKTransactionCurrency;
					newCharge.JR_RX_NKSellCurrency = invoiceLine.AL_RX_NKTransactionCurrency;

					if (shouldUseJobExchangeRate)
					{
						if (newCharge.CostExchangeRate != null)
						{
							newCharge.CostExchangeRate.SetBaseRate(jobExchangeRate);
						}
						else
						{
							newCharge.JR_OSCostExRate = jobExchangeRate;
						}

						newCharge.JR_OSSellExRate = sellRateBefore = jobExchangeRateSell;
					}
					else
					{
						if (newCharge.CostExchangeRate != null)
						{
							newCharge.CostExchangeRate.SetBaseRate(invoiceLine.AL_ExchangeRate);
						}
						else
						{
							newCharge.JR_OSCostExRate = invoiceLine.AL_ExchangeRate;
						}

						newCharge.JR_OSSellExRate = sellRateBefore = invoiceLine.AL_ExchangeRate;
					}
				}
				else
				{
					newCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					newCharge.JR_OSCostExRate = 1M;

					newCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					newCharge.JR_OSSellExRate = sellRateBefore = 1M;
				}

				newCharge.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);

				if (HasCarryForwardAmount(invoiceLine, carryForwardAmount))
				{
					if (shouldUseOSCurrency)
					{
						newCharge.JR_OSCostAmt = carryForwardAmount.OSAmount;

						if (shouldUseJobExchangeRate)
						{
							newCharge.JR_LocalCostAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(carryForwardAmount.OSAmount, jobExchangeRate);
						}
						else
						{
							newCharge.JR_LocalCostAmt = carryForwardAmount.LocalAmount;
						}
					}
					else
					{
						newCharge.JR_OSCostAmt = carryForwardAmount.LocalAmount;
						newCharge.JR_LocalCostAmt = carryForwardAmount.LocalAmount;
					}

					newCharge.SetEstimatedCost(0);
					newCharge.JR_OSSellAmt = 0M;
					newCharge.JR_LocalSellAmt = 0M;

					Func<ZString> infoCollectorStringResult = () => FormattableString.Invariant(
$@"{nameof(jobExchangeRate)}: {jobExchangeRate}
{nameof(shouldUseJobExchangeRate)}: {shouldUseJobExchangeRate}");
					AccountingCriticalValidationDependencyFactory.GetAccountingCriticalValidationInfoCollectionHelper().AddInfo_CarryForwardChargeAmountWithDifferentSigns(newCharge, invoiceLine, carryForwardAmount, newCharge.JR_LocalCostAmt, newCharge.JR_OSCostAmt, infoCollectorStringResult);
				}
				else
				{
					if (shouldUseOSCurrency)
					{
						newCharge.JR_OSSellAmt = Math.Abs(carryForwardAmount.OSAmount);

						if (shouldUseJobExchangeRate)
						{
							newCharge.JR_LocalSellAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(Math.Abs(carryForwardAmount.OSAmount), newCharge?.RevenueExchangeRate.SellRate ?? jobExchangeRateSell);
						}
						else
						{
							newCharge.JR_LocalSellAmt = Math.Abs(carryForwardAmount.LocalAmount);
						}
					}
					else
					{
						newCharge.JR_OSSellAmt = Math.Abs(carryForwardAmount.LocalAmount);
						newCharge.JR_LocalSellAmt = Math.Abs(carryForwardAmount.LocalAmount);
					}

					newCharge.JR_OSCostAmt = 0M;
					newCharge.JR_LocalCostAmt = 0M;

					SetDebtorForCarryForwardChargeWhenWIPMustHaveDebtorCode(newCharge, associatedCharge);
					var sellRateAfter = newCharge.RevenueExchangeRate?.SellRate ?? 1m;

					if (sellRateBefore != sellRateAfter)
					{
						newCharge.JR_OSSellExRate = sellRateAfter;
						newCharge.JR_LocalSellAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(newCharge.JR_OSSellAmt, newCharge.JR_OSSellExRate);
					}
				}
			}

			return newCharge;
		}

#if DEBUG
		public
#endif
		void UpdateExistingCharge(InvoicingLineBase invoiceLine, Charge associatedCharge)
		{
			using (associatedCharge.SetEstimatedCostSuspender.GetSuspender())
			{
				using (associatedCharge.Calculations.SuspendCalculations())
				{
					ResetAPInvoiceRelatedValues(associatedCharge);
					associatedCharge.JR_OH_CostAccount = invoiceLine.InvoiceBase.AH_OH;
					associatedCharge.JR_RX_NKCostCurrency = invoiceLine.AL_RX_NKTransactionCurrency;
					associatedCharge.JR_OSCostExRate = invoiceLine.AL_ExchangeRate;
					associatedCharge.JR_CostPlaceOfSupply = invoiceLine.AL_PlaceOfSupply;
					associatedCharge.JR_CostSupplyType = invoiceLine.AL_SupplyType;
					associatedCharge.JR_GB_CostTaxBranch = invoiceLine.AL_GB_TaxBranch;
					associatedCharge.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);

					associatedCharge.JR_OSCostAmt = invoiceLine.AL_DBAH_OSExTaxAmount;
					associatedCharge.JR_LocalCostAmt = -invoiceLine.AL_LineAmount;

					associatedCharge.JR_AT_CostGSTRate = invoiceLine.AL_AT;
					associatedCharge.SetCostTaxDateSafe(invoiceLine.AL_TaxDate);
					associatedCharge.JR_A9_CostVATClass = invoiceLine.AL_A9_VATClass;
					associatedCharge.JR_AW_CostWHTRate = invoiceLine.AL_AW;
					//associatedCharge.JR_OSCostWHTAmt = Env.CurrentCompany.ExchangeRate.LocalToForeign(invoiceLine.AL_LocalWHTAmount, invoiceLine.AL_ExchangeRate, invoiceLine.AL_RX_NKTransactionCurrency);
					associatedCharge.JR_CostGovtChargeCode = invoiceLine.AL_GovtChargeCode;

					associatedCharge.ReverseAccrual(GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine), true);
					associatedCharge.JR_AL_APLine = invoiceLine.PK;

					if (!associatedCharge.SetNZEntryFeeChargeTaxAmountSuspender.IsSuspended && !NZCustomsEntryFeeTaxCalculator.IsEntryFeeChargeWithCorrectAmount(invoiceLine))
					{
						using (associatedCharge.SetNZEntryFeeChargeTaxAmountSuspender.GetSuspender())
						{
							associatedCharge.JR_OSCostGSTAmt_Calc = invoiceLine.AL_DBAH_OSTaxAmount;
						}
					}
					else
					{
						associatedCharge.JR_OSCostGSTAmt_Calc = invoiceLine.AL_DBAH_OSTaxAmount;
					}
					chargesToSearch.RemoveCharge(associatedCharge);
					SetAPInvoiceValues(associatedCharge, invoiceLine.InvoiceBase);

					if (invoiceLine.IsPopulatedFromImportedApportionment)
					{
						associatedCharge.JR_E6 = invoiceLine.ApportionmentChargeImportedFrom.JR_E6;
					}
				}
			}
		}

		void CreateWIPWhenCostIsGreaterThanAccrual(InvoicingLineBase invoiceLine, Job associatedJob, Charge associatedCharge, AmountWrapper carryForwardAmount)
		{
			if (!associatedCharge.IsManualJobAccrualCharge)
			{
				var shouldUseOSCurrency = carryForwardAmount.IsOSCurrencyApplicable;

				if (associatedCharge.IsRevenuePosted || (!associatedCharge.IsRevenuePosted && shouldUseOSCurrency && associatedCharge.JR_OSSellCurrencyCode != invoiceLine.AL_RX_NKTransactionCurrency))
				{
					Charge carryForwardCharge;
					using (associatedJob.ChargesLoadSuspender.GetSuspender())
					{
						carryForwardCharge = associatedJob.Charges.AddNew();
					}
					ChargesToSearch.AddCharge(carryForwardCharge);
					carryForwardCharge.JR_AC = associatedCharge.JR_AC;
					SetRelatedJobNumber(carryForwardCharge, invoiceLine);

					if (shouldUseOSCurrency)
					{
						carryForwardCharge.JR_RX_NKCostCurrency = invoiceLine.AL_RX_NKTransactionCurrency;
						carryForwardCharge.JR_RX_NKSellCurrency = invoiceLine.AL_RX_NKTransactionCurrency;

						ZDecimal jobExchangeRate = associatedJob.GetExchangeRate(invoiceLine.AL_RX_NKTransactionCurrency, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor, ExchangeRateType.Buy, InvoiceCurrencyType.NotApplicable);
						var shouldUseJobExchangeRate = jobExchangeRate != 0 && jobExchangeRate != invoiceLine.AL_ExchangeRate && !ShouldUseOSCurrencyAndLineExchangeRate(invoiceLine, associatedCharge);

						if (shouldUseJobExchangeRate)
						{
							carryForwardCharge.JR_OSCostExRate = jobExchangeRate;
							carryForwardCharge.JR_OSSellExRate = jobExchangeRate;
						}
						else
						{
							carryForwardCharge.JR_OSCostExRate = invoiceLine.AL_ExchangeRate;
							carryForwardCharge.JR_OSSellExRate = invoiceLine.AL_ExchangeRate;
						}
						carryForwardCharge.JR_OSSellAmt = carryForwardAmount.OSAmount;
					}
					else
					{
						carryForwardCharge.JR_LocalSellAmt = carryForwardAmount.LocalAmount;
					}

					carryForwardCharge.JR_OSCostAmt = 0;

					carryForwardCharge.JR_OH_SellAccount = associatedCharge.JR_OH_SellAccount;
					carryForwardCharge.JR_OA_SellInvoiceAddress = associatedCharge.JR_OA_SellInvoiceAddress;
					carryForwardCharge.JR_OC_SellInvoiceContact = associatedCharge.JR_OC_SellInvoiceContact;
					carryForwardCharge.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);
					carryForwardCharge.JR_SellGovtChargeCode = associatedCharge.JR_SellGovtChargeCode;
				}
				else
				{
					if (shouldUseOSCurrency && associatedCharge.JR_OSSellCurrencyCode == invoiceLine.AL_RX_NKTransactionCurrency)
					{
						associatedCharge.JR_OSSellAmt += carryForwardAmount.OSAmount;
					}
					else
					{
						associatedCharge.JR_LocalSellAmt += carryForwardAmount.LocalAmount;
					}
				}
			}
		}

#if DEBUG
		public
#endif
		void UpdateCarryForwardCharge(InvoicingLineBase invoiceLine, Charge associatedCharge, AmountWrapper carryForwardAmount, bool bringForwardAgainstCreditor)
		{
			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			using (associatedCharge.Calculations.SuspendCalculations())
			{
				ResetAPInvoiceRelatedValues(associatedCharge);

				if (bringForwardAgainstCreditor || AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value)
				{
					associatedCharge.JR_OH_CostAccount = invoiceLine.InvoiceBase.AH_OH;
				}
				else
				{
					associatedCharge.JR_OH_CostAccount = ZGuid.Empty;
				}

				var shouldUseOSCurrency = carryForwardAmount.IsOSCurrencyApplicable && AmountWrapper.IsOnlyOneOSCurrencyAndRate(invoiceLine, associatedCharge);

				if (shouldUseOSCurrency)
				{
					associatedCharge.JR_RX_NKCostCurrency = invoiceLine.AL_RX_NKTransactionCurrency;
					associatedCharge.JR_OSCostExRate = invoiceLine.AL_ExchangeRate;
				}
				else
				{
					associatedCharge.JR_RX_NKCostCurrency = localCurrency;
					associatedCharge.JR_OSCostExRate = 1;
				}

				associatedCharge.WIPAccrualCreationDate = GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine);

				if (HasCarryForwardAmount(invoiceLine, carryForwardAmount))
				{
					associatedCharge.JR_CostPlaceOfSupply = invoiceLine.AL_PlaceOfSupply;
					associatedCharge.JR_OSCostAmt = shouldUseOSCurrency ? carryForwardAmount.OSAmount : carryForwardAmount.LocalAmount;
					associatedCharge.JR_LocalCostAmt = carryForwardAmount.LocalAmount;
					associatedCharge.SetEstimatedCost(0);
				}
				associatedCharge.ReverseAccrual(GetInvoiceLineReverseDateWithPostDateFallback(invoiceLine), true);
				associatedCharge.ClearCostLink();
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		void SetStandardValues(Charge charge, TransactionLine invoiceLine)
		{
			charge.JR_AC = invoiceLine.ChargeCode.PK;
			var chargeCode = charge.ChargeCode;
			var isLocalDescriptionUsed = charge.JR_Desc == chargeCode.AC_LocalLanguageDescription;
			var isLineHavingNewDescription = invoiceLine.AL_Desc != chargeCode.AC_Desc;
			if (!isLocalDescriptionUsed || isLineHavingNewDescription)
			{
				charge.JR_Desc = invoiceLine.AL_Desc;
			}

			charge.JR_GB = invoiceLine.AL_GB;
			charge.JR_GE = invoiceLine.AL_GE;
			charge.JR_CostPlaceOfSupply = invoiceLine.AL_PlaceOfSupply;
			charge.JR_CostSupplyType = invoiceLine.AL_SupplyType;
			charge.JR_CostGovtChargeCode = invoiceLine.AL_GovtChargeCode;
		}

		void SetAPInvoiceValues(Charge charge, InvoicingBase invoice)
		{
			charge.JR_APInvoiceNum = invoice.AH_TransactionNum;
			charge.JR_APInvoiceDate = invoice.AH_InvoiceDate;
			charge.JR_APDocumentReceivedDate = invoice.AH_DocumentReceivedDate;
			charge.JR_PaymentDate = invoice.AH_DueDate;
			charge.JR_CostReference = invoice.AH_ChequeOrReference;
		}

		void ResetAPInvoiceRelatedValues(Charge charge)
		{
			charge.JR_APInvoiceNum = ZString.Empty;
			charge.JR_APInvoiceDate = ZDateTime.Empty;
			charge.JR_APDocumentReceivedDate = ZDateTime.Empty;
			charge.JR_CostReference = ZString.Empty;
			charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_PaymentType = ZString.Empty;
			charge.JR_AB = ZGuid.Empty;
			charge.JR_AK = ZGuid.Empty;
			charge.JR_ChequeNo = ZString.Empty;
		}

		void UpdateChargeAmountsWithCorrectSign(Charge charge, ApportionSplitCharge apportionedCharge, InvoicingLineBase invoiceLine)
		{
			if (invoiceLine is APCreditNoteLine)
			{
				charge.JR_OSCostAmt = -apportionedCharge.JR_OSCostAmt;
				charge.JR_LocalCostAmt = -apportionedCharge.JR_LocalCostAmt;
				charge.JR_OSCostGSTAmt_Calc = -apportionedCharge.JR_OSCostGSTAmt_Calc;
				charge.JR_OSCostWHTAmt = -apportionedCharge.JR_OSCostWHTAmt;
			}
			else
			{
				charge.JR_OSCostAmt = apportionedCharge.JR_OSCostAmt;
				charge.JR_LocalCostAmt = apportionedCharge.JR_LocalCostAmt;
				charge.JR_OSCostGSTAmt_Calc = apportionedCharge.JR_OSCostGSTAmt_Calc;
				charge.JR_OSCostWHTAmt = apportionedCharge.JR_OSCostWHTAmt;
			}
		}

		static void SetDebtorForCarryForwardChargeWhenWIPMustHaveDebtorCode(Charge charge, Charge chargeToGetDebtor)
		{
			if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value && !charge.JR_IsRevenuePosted)
			{
				if (!BaseChargeValidation.IsDebtorValidWhenWIPMustHaveDebtorCode(charge))
				{
					if (chargeToGetDebtor.JR_OH_SellAccount.IsValid)
					{
						charge.JR_OH_SellAccount = chargeToGetDebtor.JR_OH_SellAccount;
					}
				}
			}
		}

		static void SetSellExchangeRateIfZero(Charge charge, InvoicingLineBase invoiceLine)
		{
			if (charge.JR_OSSellExRate <= 0m
				&& charge.JR_RX_NKSellCurrency == invoiceLine.AL_RX_NKTransactionCurrency)
			{
				charge?.RevenueExchangeRate.SetBaseRate(invoiceLine.AL_ExchangeRate);
			}
		}

		#endregion

		#region Related Job Number

		void SetRelatedJobNumber(Charge charge, InvoicingLineBase invoiceLine)
		{
			charge.JR_Calc_RelatedJobNumber = invoiceLine.AL_Calc_RelatedJobNumber;
		}

		#endregion

		ZDateTime GetInvoiceLineReverseDateWithPostDateFallback(TransactionLine invoiceLine)
		{
			var resultDate = invoiceLine.AL_PostDate.IsEmpty && invoiceLine.TransactionHeader != null ?
				invoiceLine.TransactionHeader.AH_PostDate :
				invoiceLine.AL_PostDate;
			if (invoiceLine.AL_ReverseDate.IsValid && invoiceLine.AL_ReverseDate > ZDateTime.MinSmallDateTimeValue)
			{
				var period = PeriodCalculator.GetPeriodManagementFromDate(invoiceLine.AL_ReverseDate);
				if (period != null && !period.AM_IsGeneralLedgerClosed && !period.AM_IsSubLedgerClosed)
				{
					resultDate = invoiceLine.AL_ReverseDate;
				}
			}
			return resultDate;
		}

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		AccountingPeriodCalculator fPeriodCalculator;

		static bool IsNegativeAccrualBehaviorsEnabled
		{
			get { return AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value; }
		}

		static bool IsFinalInvoice(InvoicingLineBase line)
		{
			return (line is APInvoiceLine || (IsNegativeAccrualBehaviorsEnabled && line is APCreditNoteLine)) && line.AL_IsFinalCharge;
		}

		bool HasCarryForwardAmountForTheCreditor(InvoicingLineBase invoiceLine, AmountWrapper carryForwardAmountForTheCredtior)
		{
			var negativeReaccrualApplicable = NegativeReAccrualChecker.CanApplyNegativeReaccrual(invoiceLine);
			return ((!negativeReaccrualApplicable && carryForwardAmountForTheCredtior > 0) || (negativeReaccrualApplicable && carryForwardAmountForTheCredtior < 0)) && IsBringForwardAgainstCreditorEnabled;
		}

		bool HasCarryForwardAmount(InvoicingLineBase invoiceLine, AmountWrapper carryForwardAmount)
		{
			var negativeReaccrualApplicable = NegativeReAccrualChecker.CanApplyNegativeReaccrual(invoiceLine);
			return (!negativeReaccrualApplicable && carryForwardAmount > 0) || (negativeReaccrualApplicable && carryForwardAmount < 0);
		}

		public static bool IsBringForwardAgainstCreditorEnabled
		{
			get { return AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		static bool IsCarryForwardAccrualBasedOnOSAmount
		{
			get { return AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Value; }
		}

		IAccountingCriticalValidationDependencyFactory AccountingCriticalValidationDependencyFactory => accountingCriticalValidationDependencyFactory ?? (accountingCriticalValidationDependencyFactory = ObjectFactory.Get<IAccountingCriticalValidationDependencyFactory>());
		IAccountingCriticalValidationDependencyFactory accountingCriticalValidationDependencyFactory;
#if DEBUG
		public
#endif
		static IDisposable SetNewChargeCostIsGoingToBePostedInTransformerContext(BusinessObjectFactory factory)
			=> new DisposableAction(() => factory?.SetContext(BusinessContext.NewChargeCostIsGoingToBePostedInTransformer), () => factory?.RemoveContext(BusinessContext.NewChargeCostIsGoingToBePostedInTransformer));

		public class AmountWrapper
		{
			public ZDecimal LocalAmount { get; }
			public ZDecimal OSAmount { get; }
			public bool IsOSCurrencyApplicable { get; }
			public ZGuid InfoCollectionID { get; }

			public AmountWrapper()
			{
				this.LocalAmount = 0;
				this.OSAmount = 0;
				this.IsOSCurrencyApplicable = false;
			}

			public AmountWrapper(ZDecimal localAmount, ZDecimal oSAmount, bool isOSCurrencyApplicable, ZGuid? infoCollectionID = null)
			{
				this.LocalAmount = localAmount;
				this.OSAmount = oSAmount;
				this.IsOSCurrencyApplicable = isOSCurrencyApplicable;
				this.InfoCollectionID = infoCollectionID ?? ZGuid.Empty;
			}

			[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class is private")]
			public static AmountWrapper operator +(AmountWrapper amt1, AmountWrapper amt2)
			{
				if (amt1.IsOSCurrencyApplicable && amt2.IsOSCurrencyApplicable)
				{
					return new AmountWrapper(amt1.LocalAmount + amt2.LocalAmount, amt1.OSAmount + amt2.OSAmount, true);
				}
				else
				{
					return new AmountWrapper(amt1.LocalAmount + amt2.LocalAmount, 0, false);
				}
			}

			[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class is private")]
			public static AmountWrapper operator -(AmountWrapper amt1, AmountWrapper amt2)
			{
				if (amt1.IsOSCurrencyApplicable && amt2.IsOSCurrencyApplicable)
				{
					return new AmountWrapper(amt1.LocalAmount - amt2.LocalAmount, amt1.OSAmount - amt2.OSAmount, true);
				}
				else
				{
					return new AmountWrapper(amt1.LocalAmount - amt2.LocalAmount, 0, false);
				}
			}

			[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class is private")]
			public static AmountWrapper operator -(AmountWrapper amt)
			{
				return new AmountWrapper(amt.LocalAmount * -1, amt.OSAmount * -1, amt.IsOSCurrencyApplicable);
			}

			public static bool operator !=(AmountWrapper amt, ZDecimal localAmount)
			{
				return amt.LocalAmount != localAmount;
			}

			public static bool operator !=(AmountWrapper amt1, AmountWrapper amt2) => !(amt1 == amt2);

			public static bool operator ==(AmountWrapper amt1, AmountWrapper amt2)
			{
				if (amt1.IsOSCurrencyApplicable && amt2.IsOSCurrencyApplicable)
				{
					return amt1.LocalAmount == amt2.LocalAmount && amt1.OSAmount == amt2.OSAmount;
				}
				else
				{
					return amt1.LocalAmount == amt2.LocalAmount;
				}
			}

			public static bool operator ==(AmountWrapper amt, ZDecimal localAmount)
			{
				return amt.LocalAmount == localAmount;
			}

			[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class is private")]
			public static bool operator >(AmountWrapper amt1, ZDecimal localAmount)
			{
				return amt1.LocalAmount > localAmount;
			}

			[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class is private")]
			public static bool operator <(AmountWrapper amt1, ZDecimal localAmount)
			{
				return amt1.LocalAmount < localAmount;
			}

			[SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0661")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			[SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0660")]
			public override bool Equals(object obj)
			{
				return base.Equals(obj);
			}

			public static bool IsOnlyOneOSCurrencyAndRate(InvoicingLineBase invoiceLine, Charge associatedCharge, bool shouldIgnoreEmptyCharges = false)
			{
				return IsOnlyOneOSCurrencyAndRate(invoiceLine, new Charge[] { associatedCharge }, shouldIgnoreEmptyCharges);
			}

			public static bool IsOnlyOneOSCurrencyAndRate(InvoicingLineBase invoiceLine, IEnumerable<Charge> associatedCharges, bool shouldIgnoreEmptyCharges = false)
			{
				var result = false;
				if (invoiceLine.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					//an invoice might contain multiple lines have the same charge codes but different os currencies / rates.
					if (DoesAnyInvoiceLineHaveDifferentCurrencyOrExchangeRate(invoiceLine))
					{
						result = false;
					}
					else if (associatedCharges == null || !associatedCharges.Any())
					{
						result = shouldIgnoreEmptyCharges;
					}
					else
					{
						//get the charges match the invoice line
						var charges = (from Charge charge in associatedCharges
									   where charge.ChargeCode.PK == invoiceLine.ChargeCode.PK
									   && charge.Branch.PK == invoiceLine.Branch.PK
									   && charge.Department.PK == invoiceLine.Department.PK
									   && charge.Job.PK == invoiceLine.Job.PK
									   && charge.RelatedJobID == invoiceLine.AL_Calc_RelatedJobPK
									   && !charge.JR_IsPosted
									   select charge);

						var chargesCount = charges.Count();
						if (chargesCount > 0)
						{
							var chargesWithSameRateCount = charges.Count(c => c.JR_RX_NKCostCurrency == invoiceLine.AL_RX_NKTransactionCurrency && c.JR_OSCostExRate == invoiceLine.AL_ExchangeRate);
							result = chargesCount == chargesWithSameRateCount;
						}
					}
				}
				return result;
			}

			static bool DoesAnyInvoiceLineHaveDifferentCurrencyOrExchangeRate(InvoicingLineBase invoiceLine)
			{
				var invoicingLinePlainObject = default(InvoicingLinePlainDataObject);
				var invoicingLinePlainDataObjects = default(IEnumerable<InvoicingLinePlainDataObject>);
				var invoicingLineCacheUtility = invoiceLine.Factory.ServiceContainer.GetService<IInvoicingLineCacheUtility>();

				if (AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.Value &&
					(invoicingLineCacheUtility?.InvoicingLineCache?.TryGetValue(invoiceLine.PK, out var cachedInvoiceLinePlainDataObject) ?? false))
				{
					invoicingLinePlainObject = cachedInvoiceLinePlainDataObject;
					invoicingLinePlainDataObjects = invoicingLineCacheUtility.InvoicingLinePlainDataObjects;
				}
				else
				{
					invoicingLinePlainObject = InvoicingLinePlainDataObject.Create(invoiceLine);
					invoicingLinePlainDataObjects = invoiceLine.InvoiceBase.Lines.OfType<InvoicingLineBase>().Select(x => InvoicingLinePlainDataObject.Create(x));
				}

				var result = invoicingLinePlainDataObjects
					.Any(x => x.PK != invoicingLinePlainObject.PK &&
							x.ChargeCodePK != ZGuid.Empty &&
							invoicingLinePlainObject.ChargeCodePK != ZGuid.Empty &&
							x.ChargeCodePK == invoicingLinePlainObject.ChargeCodePK &&
							x.BranchPK == invoicingLinePlainObject.BranchPK &&
							x.DepartmentPK == invoicingLinePlainObject.DepartmentPK &&
							x.JobPK == invoicingLinePlainObject.JobPK &&
							x.RelatedJobPK == invoicingLinePlainObject.RelatedJobPK &&
							(x.Currrency != invoicingLinePlainObject.Currrency ||
								x.ExchangeRate != invoicingLinePlainObject.ExchangeRate)
					);

				return result;
			}

#if DEBUG
			public static bool DoesAnyInvoiceLineHaveDifferentCurrencyOrExchangeRate_ForTestOnly(InvoicingLineBase invoiceLine) => DoesAnyInvoiceLineHaveDifferentCurrencyOrExchangeRate(invoiceLine);
#endif
		}

		#endregion

#if DEBUG
		public static IEnumerable<ChargeToSearchInfo> GetChargeToSearch_ForTestOnly(InvoicingBase invoice, IEnumerable<InvoicingLineBase> lines) => GetChargeToSearch(invoice, lines);
		public static string SqlForGetChargeToSearch_ForTestOnly { get; private set; }
#endif
	}
}
