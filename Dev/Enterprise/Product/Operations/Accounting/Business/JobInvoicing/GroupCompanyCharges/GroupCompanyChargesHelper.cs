using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Charges on a job raised in other companies
	/// and logic for matching/creating/updating similar charges in the current company.
	/// </summary>
	public class GroupCompanyChargesHelper
	{
		public static GroupCompanyChargesHelper Create(Job job, bool isForCreditor)
			=> new GroupCompanyChargesHelper(job, isForCreditor);

		public GroupCompanyChargesHelper(Job job, bool isForCreditor)
		{
			Argument.NotNull(job, nameof(job));
			this.job = job;
			this.isForCreditor = isForCreditor;
			otherCompanyCharges = LoadGroupCompanyCharges();

			localChargePkToMatch = new Dictionary<ZGuid, MatchState>(otherCompanyCharges.Count);
			otherChargePkToMatch = new Dictionary<ZGuid, MatchState>(otherCompanyCharges.Count);
		}

		readonly Job job;
		readonly bool isForCreditor;
		readonly IReadOnlyList<Charge> otherCompanyCharges;

		/// <summary>
		/// Stores the matches for speed, since the calculation can be slow.
		/// </summary>
		readonly Dictionary<ZGuid, MatchState> localChargePkToMatch;
		readonly Dictionary<ZGuid, MatchState> otherChargePkToMatch;

		public bool IsAcceptedCost(Charge charge)
			=> IsAccepted(charge);

		public bool IsAcceptedSell(Charge charge)
			=> IsAccepted(charge);

		bool IsAccepted(Charge charge)
		{
			if (otherCompanyCharges.Count == 0)
			{
				return false;
			}

			return GetOrCalculateMatch(charge) != null;
		}

		MatchState GetOrCalculateMatch(Charge localCharge)
		{
			if (!localChargePkToMatch.TryGetValue(localCharge.PK, out var match))
			{
				match = CalculateMatch(localCharge);
			}
			return match;
		}

		MatchState CalculateMatch(Charge localCharge)
		{
			(var bestMatch, var partialMatches) = FindExactMatchOrAllPartialMatchesForLocalCharge(localCharge);
			if (bestMatch != null)
			{
				AddMatch(bestMatch);
			}
			else if (partialMatches == null || partialMatches.Count == 0)
			{
				// No match. Assume it will never have a match, and permanently store a null match
				localChargePkToMatch[localCharge.PK] = null;
			}
			else
			{
				// Partial match - check if the other charges are an exact match to a different local charge first

				// No need to check localCharge again
				localChargePkToMatch[localCharge.PK] = null;

				foreach (var otherCharge in partialMatches)
				{
					var otherMatch = FindExactMatchForOtherCharge(otherCharge);
					if (otherMatch != null)
					{
						AddMatch(otherMatch);
					}
					else
					{
						bestMatch = new MatchState(localCharge, otherCharge, GetLocalPaymentBases(localCharge).First(), isExactMatch: false, OnMatchChanged);
						AddMatch(bestMatch);
						break;
					}
				}
			}

			return bestMatch;
		}

		void AddMatch(MatchState match)
		{
			localChargePkToMatch[match.LocalCharge.PK] = match;
			otherChargePkToMatch[match.OtherCharge.PK] = match;
		}

		void OnMatchChanged(MatchState match)
		{
			RemoveMatch(match.LocalCharge);
		}

		void RemoveMatch(Charge localCharge)
		{
			if (localChargePkToMatch.TryGetValue(localCharge.PK, out var match))
			{
				localCharge.InvalidateGroupChargeMatch(isForCreditor);
				localChargePkToMatch.Remove(localCharge.PK);

				if (match != null)
				{
					otherChargePkToMatch.Remove(match.OtherCharge.PK);
				}
			}
		}

		/// <summary>
		/// Stores a match between a local charge and another company charge
		/// and tracks all changes that can cause the match to need recalculating.
		/// </summary>
		class MatchState
		{
			/// <summary>
			/// Constructor.
			/// The payment basis parameter is any basis for the local charge (there is always at least one) and is used to track changes
			/// since if one basis changes they all change, since they are all calculated together.
			/// </summary>
			public MatchState(Charge localCharge, Charge otherCharge, JobPaymentBasis localBasis, bool isExactMatch, Action<MatchState> handleMatchChanged)
			{
				Argument.NotNull(localCharge, nameof(localCharge));
				Argument.NotNull(otherCharge, nameof(otherCharge));
				Argument.NotNull(localBasis, nameof(localBasis));
				Argument.NotNull(handleMatchChanged, nameof(handleMatchChanged));
				LocalCharge = localCharge;
				OtherCharge = otherCharge;
				LocalBasis = localBasis;
				IsExactMatch = isExactMatch;
				this.handleMatchChanged = handleMatchChanged;
				localBasis.HasChangesChanged += LocalBasis_HasChangesChanged;
				localCharge.BeforeSuccessfulDeleting += OnChargeDeleting;
			}

			void LocalBasis_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
				=> OnChanged();

			void OnChargeDeleting(object sender, EventArgs args)
				=> OnChanged();

			void OnChanged()
			{
				if (handleMatchChanged != null)
				{
					LocalBasis.HasChangesChanged -= LocalBasis_HasChangesChanged;
					LocalCharge.BeforeSuccessfulDeleting -= OnChargeDeleting;
					handleMatchChanged(this);
					handleMatchChanged = null;
				}
			}

			public Charge LocalCharge { get; }
			public Charge OtherCharge { get; }
			public JobPaymentBasis LocalBasis { get; }
			public bool IsExactMatch { get; }
			Action<MatchState> handleMatchChanged;
		}

		#region Accept

		public void AcceptSellChargesAsCosts(GroupCompanyCharge[] groupCompanyCharges)
		{
			foreach (var groupCompanyCharge in groupCompanyCharges)
			{
				switch (groupCompanyCharge.AcceptActionForCost)
				{
					case GroupCompanyCharge.AcceptAction.Create:
					case GroupCompanyCharge.AcceptAction.Update:
						CreateOrUpdate(groupCompanyCharge);
						break;

					case GroupCompanyCharge.AcceptAction.NoAction:
						break;
				}
			}
		}

		void CreateOrUpdate(GroupCompanyCharge groupCompanyCharge)
		{
			var oldCostCharge = groupCompanyCharge.GroupCompanyCostCharge;
			var costCharge = oldCostCharge != null && oldCostCharge.CanReautorate(CostSell.Cost, GetOperationalJobCode(job))
				? oldCostCharge
				: job.Charges.AddNew();

			costCharge.JR_AC = groupCompanyCharge.CostCompanyChargeCodePK;
			costCharge.JR_OH_CostAccount = groupCompanyCharge.GroupCompanySellCharge.Branch.GB_OH_OrgProxy;
			costCharge.JR_RX_NKCostCurrency = groupCompanyCharge.JR_RX_NKSellCurrency;
			costCharge.JR_OSCostAmt = groupCompanyCharge.JR_OSSellAmt;
			costCharge.CostCalculationDescription = groupCompanyCharge.GroupCompanySellCharge.RevenueCalculationDescription;
			costCharge.JR_CostRatingOverride = false;

			JobPaymentBasis firstNewBasis = null;
			foreach (var sellPaymentBasis in groupCompanyCharge.GroupCompanySellCharge.SellPaymentBases)
			{
				var newCostPaymentBasis = job.Factory.New<JobPaymentBasis>();
				if (firstNewBasis == null)
				{
					firstNewBasis = newCostPaymentBasis;
				}
				newCostPaymentBasis.CopyPersistentValuesFrom(sellPaymentBasis);
				costCharge.PaymentBases.Add(newCostPaymentBasis);
				newCostPaymentBasis.PBS_IsCost = true;
				if (costCharge.JR_E6.IsValid && costCharge.ParentConsolCost != null)
				{
					newCostPaymentBasis.PBS_E6 = costCharge.JR_E6;
				}
				else
				{
					newCostPaymentBasis.PBS_JR = costCharge.PK;
				}
			}

			if (oldCostCharge != null)
			{
				RemoveMatch(oldCostCharge);
			}
			AddMatch(new MatchState(costCharge, groupCompanyCharge.GroupCompanySellCharge, firstNewBasis, isExactMatch: true, OnMatchChanged));
		}

		static ZString GetOperationalJobCode(Job job)
		{
			var ratingAdapter = job.Parent as IAutoRating;
			var operationalJobCode = ratingAdapter?.OperationalJobCode ?? ZString.Empty;

			return operationalJobCode;
		}

		#endregion

		#region Find

		/// <summary>
		/// Build a collection for binding to the DebtorsAcceptGroupChargesForm grid.
		/// Don't use this for calculations, especially those for a single charge, since it calculates and includes all charges including unmatched ones.
		/// </summary>
		public GroupCompanyChargeCollection BuildGroupCompanyChargeCollectionForBinding()
		{
			var result = new GroupCompanyChargeCollection(job.Factory);

			var groupCompanyCharges = GetGroupCompanyCharges();
			foreach (var groupCompanyCharge in groupCompanyCharges)
			{
				result.Add(groupCompanyCharge);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		List<GroupCompanyCharge> GetGroupCompanyCharges()
		{
			var result = new List<GroupCompanyCharge>();
			var localChargeCollection = job.Charges;

			if (localChargeCollection.Count > 0 && otherCompanyCharges.Count > 0)
			{
				var localCharges = job.Charges.Cast<Charge>();
				foreach (var localCharge in localCharges)
				{
					var match = GetOrCalculateMatch(localCharge);
					if (match != null)
					{
						var action = match.IsExactMatch ? GroupCompanyCharge.AcceptAction.NoAction : GroupCompanyCharge.AcceptAction.Update;
						GroupCompanyCharge groupCharge;
						if (isForCreditor)
						{
							groupCharge = new GroupCompanyCharge(match.OtherCharge, localCharge, action, job.Factory);
						}
						else
						{
							groupCharge = new GroupCompanyCharge(localCharge, match.OtherCharge, action, job.Factory);
						}
						result.Add(groupCharge);
					}
				}
			}

			foreach (var otherCharge in otherCompanyCharges)
			{
				if (!otherChargePkToMatch.ContainsKey(otherCharge.PK))
				{
					if (isForCreditor)
					{
						if (otherCharge.CostPaymentBases.Any())
						{
							result.Add(new GroupCompanyCharge(otherCharge, null, GroupCompanyCharge.AcceptAction.Create, job.Factory));
						}
					}
					else
					{
						if (otherCharge.SellPaymentBases.Any())
						{
							result.Add(new GroupCompanyCharge(null, otherCharge, GroupCompanyCharge.AcceptAction.Create, job.Factory));
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Load charges in other companies where the creditor/debtor is an org proxy of the job company.
		/// Ignore companies that have "Enable Auto Job Revenue Journals" in the registry.
		/// Group Company Charges are incompatible with any JobCharges that belong to a Company where this registry item is enabled.
		/// This is to avoid creating Group Company Charges for JobCharges that are already self-billed by Auto Job Revenue Journal.
		/// </summary>
		Charge[] LoadGroupCompanyCharges()
		{
			var orgProxies = GetOrgProxyPKsFromCompanyAndBranches(job.Company);
			if (orgProxies.Count != 0)
			{
				var orgColumn = isForCreditor ? JobChargeSchema.JR_OH_CostAccount : JobChargeSchema.JR_OH_SellAccount;

				var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
				jobSubQuery.AddToFilter(JobHeaderSchema.JH_ParentID, job.JH_ParentID);
				var companyPksWithAutoJournal = GetCachedCompanyPksWithEnabledAutoJobRevenueJournal(job.Factory);
				var companyPksToExclude = companyPksWithAutoJournal.Contains(job.JH_GC)
					? companyPksWithAutoJournal
					: new List<ZGuid>(companyPksWithAutoJournal) { job.JH_GC };
				jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, companyPksToExclude);

				var jobChargeQuery = new ZDBOnlyQuery(typeof(JobCharge));
				jobChargeQuery.AddToFilter(orgColumn, orgProxies);
				jobChargeQuery.AddSubQuery(JobChargeSchema.JR_JH, jobSubQuery, JoinCondition.And);

				return new BusinessObjectFactory().Load<Charge>(jobChargeQuery);
			}

			return Array.Empty<Charge>();
		}

		static List<ZGuid> GetCachedCompanyPksWithEnabledAutoJobRevenueJournal(BusinessObjectFactory factory)
			=> factory.GetCachedValue("GroupCompanyChargesHelper.CompanyPksWithAutoJobRevenueJournal", () => LoadCompanyPksWithEnabledAutoJobRevenueJournal(factory));

		static List<ZGuid> LoadCompanyPksWithEnabledAutoJobRevenueJournal(BusinessObjectFactory factory)
		{
			var result = new List<ZGuid>();
			var companyQuery = new ZQuery(GlbCompanySchema.GC_IsActive, true);
			var activeCompanies = factory.Load<GlbCompany>(companyQuery);
			foreach (var company in activeCompanies)
			{
				if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled(company.PK.ToGuid()))
				{
					result.Add(company.PK);
				}
			}
			return result;
		}

		static HashSet<ZGuid> GetOrgProxyPKsFromCompanyAndBranches(GlbCompany company)
		{
			var orgProxyPKs = new HashSet<ZGuid>();
			orgProxyPKs.Add(company.GC_OH_OrgProxy);
			foreach (var branch in company.ActiveBranches)
			{
				orgProxyPKs.Add(branch.GB_OH_OrgProxy);
			}

			return orgProxyPKs;
		}

		(MatchState exactMatch, List<Charge> partialMatches) FindExactMatchOrAllPartialMatchesForLocalCharge(Charge localCharge)
		{
			List<Charge> partialMatches = null;
			MatchState exactMatch = null;
			var localPaymentBases = GetLocalPaymentBases(localCharge);

			foreach (var otherCharge in otherCompanyCharges)
			{
				if (!otherChargePkToMatch.ContainsKey(otherCharge.PK))
				{
					var otherPaymentBases = isForCreditor ? otherCharge.CostPaymentBases : otherCharge.SellPaymentBases;

					var chargeComparison = localPaymentBases.Compare(otherPaymentBases);
					if (chargeComparison == ChargeComparison.Same)
					{
						exactMatch = new MatchState(localCharge, otherCharge, localPaymentBases.First(), isExactMatch: true, OnMatchChanged);
						break;
					}
					else if (chargeComparison >= ChargeComparison.SameChargeableButDifferentRate)
					{
						if (partialMatches == null)
						{
							partialMatches = new List<Charge>();
						}
						partialMatches.Add(otherCharge);
						// Keep searching for an exact match
					}
				}
			}

			return (exactMatch, partialMatches);
		}

		IReadOnlyCollection<JobPaymentBasis> GetLocalPaymentBases(Charge localCharge)
			=> isForCreditor ? localCharge.SellPaymentBases : localCharge.CostPaymentBases;

		MatchState FindExactMatchForOtherCharge(Charge otherCharge)
		{
			MatchState match = null;
			var otherPaymentBases = isForCreditor ? otherCharge.CostPaymentBases : otherCharge.SellPaymentBases;

			foreach (Charge localCharge in job.Charges)
			{
				if (!localChargePkToMatch.ContainsKey(localCharge.PK))
				{
					var localPaymentBases = GetLocalPaymentBases(localCharge);

					var chargeComparison = localPaymentBases.Compare(otherPaymentBases);
					if (chargeComparison == ChargeComparison.Same)
					{
						match = new MatchState(localCharge, otherCharge, localPaymentBases.First(), isExactMatch: true, OnMatchChanged);
						break;
					}
				}
			}

			return match;
		}

		#endregion
	}
}
