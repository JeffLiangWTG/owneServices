using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public abstract class MatchingStrategy
	{
		protected MatchingStrategy(InvoiceLineGroupingProvider lineGroupingProvider, ChargeGroupingProvider chargeGroupingProvider, bool isCrossLedgerImport)
		{
			LineGroupingProvider = lineGroupingProvider;
			ChargeGroupingProvider = chargeGroupingProvider;
			IsCrossLedger = isCrossLedgerImport;
		}

		InvoiceLineGroupingProvider LineGroupingProvider { get; }
		ChargeGroupingProvider ChargeGroupingProvider { get; }
		bool IsCrossLedger { get; }
		protected ZInt Multipler => IsCrossLedger ? 1 : -1;

		protected ZInt Weight => LineGroupingProvider.Weight + ChargeGroupingProvider.Weight;
		public IEnumerable<InvoiceLineGroup> GetInvoiceLineGroupsWithMatchingSuggestions(OrgType orgType, bool isConsolRelated)
		{
			var invoiceLineGroupsWithSuggestions = new List<InvoiceLineGroup>();
			foreach (var lineGroup in LineGroupingProvider.Groups)
			{
				var matchingChargeGroup = MatchCharges(orgType, lineGroup, ChargeGroupingProvider.Groups, isConsolRelated).FirstOrDefault();
				if (matchingChargeGroup != null)
				{
					var weight = GetWeightBasedOnOrgType(orgType) + Weight + GetWeightBasedOnProcessor(isConsolRelated);

					lineGroup.Suggestions.Add(weight, new MatchingSuggestion(matchingChargeGroup, weight));

					invoiceLineGroupsWithSuggestions.Add(lineGroup);
				}
			}

			return invoiceLineGroupsWithSuggestions;
		}

		static int GetWeightBasedOnProcessor(bool isConsolRelated)
		{
			return Constants.InvoiceLineToChargeMatchingFactor.ProcessorFactor * (isConsolRelated ? Constants.ProcessorWeight.Consol : Constants.ProcessorWeight.Job);
		}

		protected virtual IEnumerable<ChargeGroup> MatchCharges(OrgType orgType, InvoiceLineGroup lineGroup, IReadOnlyList<ChargeGroup> chargeGroups, bool isConsolRelated)
		{
			return chargeGroups.Where(chargeGroup =>
							chargeGroup.Key.OrgType == orgType
							&& chargeGroup.Key.Currency == lineGroup.Key.Currency);
		}

		static ZInt GetWeightBasedOnOrgType(OrgType orgType)
		{
			var result = Constants.InvoiceLineToChargeMatchingOrgWeight.NoOrgMatchingWeight;

			if (orgType == OrgType.OriginalOrg)
			{
				result = Constants.InvoiceLineToChargeMatchingOrgWeight.OriginalOrgMatchingWeight;
			}
			else if (orgType == OrgType.SettlementGroupOrg)
			{
				result = Constants.InvoiceLineToChargeMatchingOrgWeight.SettlementGroupOrgMatchingWeight;
			}
			else if (orgType == OrgType.ChildOrg)
			{
				result = Constants.InvoiceLineToChargeMatchingOrgWeight.ChildOrgMatchingWeight;
			}

			return Constants.InvoiceLineToChargeMatchingFactor.OrgTypeFactory * result;
		}
	}

	public class MatchWithJobMatchingStrategy : MatchingStrategy
	{
		public MatchWithJobMatchingStrategy(InvoiceLineGroupingProvider lineGroupingProvider, ChargeGroupingProvider chargeGroupingProvider, bool isCrossLedger)
			: base(lineGroupingProvider, chargeGroupingProvider, isCrossLedger)
		{
		}
		protected override IEnumerable<ChargeGroup> MatchCharges(OrgType orgType, InvoiceLineGroup lineGroup, IReadOnlyList<ChargeGroup> chargeGroups, bool isConsolRelated)
		{
			return base.MatchCharges(orgType, lineGroup, chargeGroups, isConsolRelated)
				.Where(chargeGroup =>
							chargeGroup.TotalCostAmount == lineGroup.TotalAmount * Multipler
							&& chargeGroup.IsConsolRelated == isConsolRelated
							&& (lineGroup.Key.JobNumber.IsEmpty || chargeGroup.Key.JobNumber == lineGroup.Key.JobNumber)
							&& (!isConsolRelated || lineGroup.Key.ConsolNumber.IsEmpty || chargeGroup.Key.ConsolNumber.IsEmpty || (chargeGroup.Key.ConsolNumber == lineGroup.Key.ConsolNumber)));
		}
	}

	public class MatchWithJobAndChargeCodeMatchingStrategy : MatchWithJobMatchingStrategy
	{
		public MatchWithJobAndChargeCodeMatchingStrategy(InvoiceLineGroupingProvider lineGroupingProvider, ChargeGroupingProvider chargeGroupingProvider, bool isCrossLedger)
			: base(lineGroupingProvider, chargeGroupingProvider, isCrossLedger)
		{
		}

		protected override IEnumerable<ChargeGroup> MatchCharges(OrgType orgType, InvoiceLineGroup lineGroup, IReadOnlyList<ChargeGroup> chargeGroups, bool isConsolRelated)
		{
			return base.MatchCharges(orgType, lineGroup, chargeGroups, isConsolRelated)
				.Where(chargeGroup => chargeGroup.Key.ChargeCode == lineGroup.Key.ChargeCode);
		}
	}
}
