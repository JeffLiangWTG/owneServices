using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class ChargeCleaner
	{
		enum CleanupAction
		{
			None,
			ResetCosts,
			ResetRevenue,
			DeleteCharge
		}

		public static List<RatesAdditionInfo> CleanupExistingCharges(RatingStrategiesRepository strategiesRepository, AutoRateOptions options)
		{
			var chargeDeletionInfo = new List<RatesAdditionInfo>();
			var interactor = strategiesRepository.Interactor;
			var strategies = strategiesRepository.Strategies.Where(x => x.Job != null && (x.Supports(CostSell.Revenue) || x.Supports(CostSell.Cost))).ToArray();

			AddFetchHintsForCleanup(strategies);

			foreach (var strategy in strategies)
			{
				var entityName = strategy.HostBusinessEntity.HumanReadableName;
				interactor.Information((NoResString)"Resetting previously auto-rated charges for " + entityName);

				var adapterIDs = GetAdapterIDs(strategiesRepository, strategy, options);
				var jobPK = strategy.Job.PK;
				var chargesToClean = strategy.Job.Charges.Where(x => jobPK == x.JR_JH && x.IsAllowedToModifyThisCharge).ToArray();
				foreach (var charge in chargesToClean)
				{
					var cleanUpAction = charge.GetCleanupAction(options, adapterIDs);
					charge.Cleanup(cleanUpAction, entityName, chargeDeletionInfo, interactor);
				}
			}

			return chargeDeletionInfo;
		}

		static ZString[] GetAdapterIDs(RatingStrategiesRepository strategiesRepository, IAutoRatingStrategy strategy, AutoRateOptions options)
		{
			var costAdapters = strategiesRepository.GetRatingAdapters(strategy, options.With(autoRateCost: true, autoRateRevenue: false)).Select(x => x.OperationalJobCode);
			var sellAdapters = strategiesRepository.GetRatingAdapters(strategy, options.With(autoRateCost: false, autoRateRevenue: true)).Select(x => x.OperationalJobCode);
			var results = sellAdapters.Union(costAdapters).Where(x => !x.IsEmpty).Distinct().ToArray();

			return results;
		}

		static CleanupAction GetCleanupAction(this Charge charge, AutoRateOptions options, ZString[] ratingAdapters)
		{
			var resetRevenue = options.AutoRateRevenue && charge.CanReautorate(CostSell.Revenue, ratingAdapters);
			var resetCosts = options.AutoRateCost && charge.CanReautorate(CostSell.Cost, ratingAdapters);

			if (resetRevenue && resetCosts && !charge.IsCommentChargeCode)
			{
				return CleanupAction.DeleteCharge;
			}

			if (resetCosts)
			{
				return CleanupAction.ResetCosts;
			}

			if (resetRevenue)
			{
				return CleanupAction.ResetRevenue;
			}

			return CleanupAction.None;
		}

		static void Cleanup(this Charge charge, CleanupAction cleanupAction, string entityName, List<RatesAdditionInfo> chargeDeletionInfo, ILogger interactor)
		{
			#region SuppressResourceStringsCheckRegion

			void LogActionInformation(string action)
			{
				var chargeCodeOrEmpty = charge.ChargeCode?.AC_Code ?? "''";
				interactor.Information(chargeCodeOrEmpty + action);
			}

			switch (cleanupAction)
			{
				case CleanupAction.ResetRevenue:
					ResetRevenue(charge);
					LogActionInformation(" Charge Revenue reset by AutoRating.");
					break;

				case CleanupAction.ResetCosts:
					LogActionInformation(" Charge Cost reset by AutoRating.");
					ResetCost(charge);
					break;

				case CleanupAction.DeleteCharge:
					LogActionInformation(" Charge deleted by AutoRating.");
					IncrementDeletedChargesCount(entityName, chargeDeletionInfo);
					charge.Delete();
					break;

				case CleanupAction.None:
					break;
			}

			#endregion
		}

		static void ResetRevenue(Charge charge)
		{
			bool oldOverrideValue = charge.JR_CostRatingOverride;
			charge.RecalculateMarginRevenue();
			charge.JR_CostRatingOverride = oldOverrideValue;
		}

		static void ResetCost(Charge charge)
		{
			bool oldOverrideValue = charge.JR_SellRatingOverride;
			charge.RecalculateMarginCost();
			charge.JR_SellRatingOverride = oldOverrideValue;
			charge.JR_CostRatingOverride = false;
		}

		static void IncrementDeletedChargesCount(string entityName, List<RatesAdditionInfo> chargeDeletionInfo)
		{
			var existingDeletedCharges = chargeDeletionInfo.Where(x => x.Target == entityName).ToList();
			existingDeletedCharges.ForEach(c => c.DeletedChargesCount += 1);

			if (!existingDeletedCharges.Any())
			{
				//Since we can only have deleted charges when rating both Cost and Revenue, we need to create two RatesAdditionInfo results when the charge is deleted.
				chargeDeletionInfo.Add(new RatesAdditionInfo() { Target = entityName, DeletedChargesCount = 1, CostSell = AutoRatingStarter.CostResult });
				chargeDeletionInfo.Add(new RatesAdditionInfo() { Target = entityName, DeletedChargesCount = 1, CostSell = AutoRatingStarter.RevenueResult });
			}
		}

		static void AddFetchHintsForCleanup(IAutoRatingStrategy[] strategies)
		{
			var chargePKs = new List<ZGuid>();

			foreach (var strategy in strategies)
			{
				var chargesToClean = strategy.Job.Charges.Where(x => strategy.Job.PK == x.JR_JH);
				chargePKs.AddRange(chargesToClean.Select(c => c.PK));
			}

			if (chargePKs.Count > 0)
			{
				var factory = strategies.First().Job.Factory;

				foreach (var chargePK in chargePKs)
				{
					factory.AddFetchHint(JobPaymentBasisSchema.PBS_JR, chargePK);
					factory.AddFetchHint(StmNoteSchema.ST_ParentID, chargePK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentID, chargePK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentRelatedID, chargePK);
					factory.AddFetchHint(StmUniversalCopySchema.SUC_CopyObjectId, chargePK);
				}

				var chargeAttribsQuery = new ZQuery() { AllowTableValuedParameters = true };
				chargeAttribsQuery.AddToFilter(JobChargeAttribSchema.EC_JR, chargePKs);
				var chargeAttribs = factory.Load<JobChargeAttrib>(chargeAttribsQuery);
				foreach (var chargeAttrib in chargeAttribs)
				{
					factory.AddFetchHint(StmNoteSchema.ST_ParentID, chargeAttrib.PK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentID, chargeAttrib.PK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentRelatedID, chargeAttrib.PK);
					factory.AddFetchHint(StmUniversalCopySchema.SUC_CopyObjectId, chargeAttrib.PK);
				}

				var basisQuery = new ZQuery() { AllowTableValuedParameters = true };
				basisQuery.AddToFilter(JobPaymentBasisSchema.PBS_JR, chargePKs);
				var jobPaymentBases = factory.Load<JobPaymentBasis>(basisQuery);
				foreach (var jobPaymentBasis in jobPaymentBases)
				{
					factory.AddFetchHint(StmNoteSchema.ST_ParentID, jobPaymentBasis.PK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentID, jobPaymentBasis.PK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentRelatedID, jobPaymentBasis.PK);
					factory.AddFetchHint(StmUniversalCopySchema.SUC_CopyObjectId, jobPaymentBasis.PK);
				}

				var noteQuery = new ZQuery() { AllowTableValuedParameters = true };
				noteQuery.AddToFilter(StmNoteSchema.ST_ParentID, chargePKs);
				var notes = factory.Load<StmNote>(noteQuery);
				foreach (var note in notes)
				{
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentID, note.PK);
					factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentRelatedID, note.PK);
					factory.AddFetchHint(StmUniversalCopySchema.SUC_CopyObjectId, note.PK);
				}
			}
		}
	}
}
