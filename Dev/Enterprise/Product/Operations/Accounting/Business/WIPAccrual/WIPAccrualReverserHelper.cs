namespace Enterprise.Accounting.Business.WIPAccrual
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.ZArchitecture.Core;

	public class WIPAccrualReverserHelper
	{
		readonly IEnumerable<BaseWIPAccrual> accrualsToReverse;

		public WIPAccrualReverserHelper(IEnumerable<BaseWIPAccrual> accrualsToReverse)
		{
			Argument.NotNull(accrualsToReverse, "accrualsToReverse");
			this.accrualsToReverse = accrualsToReverse;
		}

		public Dictionary<ZGuid, ZString> PopulateErrorMessagesForMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals()
		{
			var selectedAccruals = from x in accrualsToReverse
								   where x.AL_LineType == TransactionLineTypes.Accrual
								   select x;

			var costsLinkedToSelectedAccruals = new List<JobConsolCost>();
			GetLinkedConsolCosts(selectedAccruals, costsLinkedToSelectedAccruals);
			var consolCostSelectedAccrualsAndMissingAccruals = PopulateConsolCostAndAccruals(selectedAccruals, costsLinkedToSelectedAccruals);
			return PrepareErrorMessagesForSelectedAccruals(consolCostSelectedAccrualsAndMissingAccruals);
		}

		void GetLinkedConsolCosts(IEnumerable<BaseWIPAccrual> selectedAccruals, List<JobConsolCost> costsLinkedToSelectedAccruals)
		{
			// Get list of costs
			foreach (var accrual in selectedAccruals)
			{
				if (accrual.IsApportioned)
				{
					ZGuid costID = accrual.RelatedJobCharge.ParentConsolCost.PK;
					if (costsLinkedToSelectedAccruals.Find(x => x.PK == costID) == null)
					{
						costsLinkedToSelectedAccruals.Add(accrual.RelatedJobCharge.ParentConsolCost);
					}
				}
			}
		}

		List<ConsolCostSelectedAccrualsAndMissingAccruals> PopulateConsolCostAndAccruals(IEnumerable<BaseWIPAccrual> selectedAccruals, List<JobConsolCost> costsLinkedToSelectedAccruals)
		{
			// For each cost check that every ACR is in our collection
			var selectedAccrualPKs = new HashSet<ZGuid>(selectedAccruals.Select(x => x.PK));

			var result = new List<ConsolCostSelectedAccrualsAndMissingAccruals>();

			foreach (var cost in costsLinkedToSelectedAccruals)
			{
				var missingAccruals = new List<Accrual>();
				var userSelectedAccruals = new List<ZGuid>();
				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					if (charge.Accrual != null)
					{
						if (!selectedAccrualPKs.Contains(charge.Accrual.PK))
						{
							missingAccruals.Add(charge.Accrual);
						}
						else
						{
							userSelectedAccruals.Add(charge.Accrual.PK);
						}
					}
				}
				if (missingAccruals.Any())
				{
					result.Add(new ConsolCostSelectedAccrualsAndMissingAccruals(cost.PK, userSelectedAccruals, missingAccruals));
				}
			}

			return result;
		}

		Dictionary<ZGuid, ZString> PrepareErrorMessagesForSelectedAccruals(List<ConsolCostSelectedAccrualsAndMissingAccruals> consolCostSelectedAccrualsAndMissingAccruals)
		{
			var selectedAccrualAndMissingAccrualDetailedMessage = new Dictionary<ZGuid, ZString>();

			if (consolCostSelectedAccrualsAndMissingAccruals.Any())
			{
				foreach (var consolCost in consolCostSelectedAccrualsAndMissingAccruals)
				{
					var message = new ZStringBuilder();
					message.AppendLine(Res.GetString("af5f0437-ad1c-44bc-ada9-11147c66a7b3", @"This accrual is apportioned at consol level.
To continue reversing this accrual, following accruals will have to be selected as well."));
					message.AppendLine();

					foreach (var missingAccrual in consolCost.missingAccrualPKs)
					{
						message.AppendLine(GetJobDetailsForMissingAccrual(missingAccrual));
					}

					foreach (var selectedAccrual in consolCost.selectedAccrualPKs)
					{
						selectedAccrualAndMissingAccrualDetailedMessage.Add(selectedAccrual, message.ToString());
					}
				}
			}

			return selectedAccrualAndMissingAccrualDetailedMessage;
		}

		ZString GetJobDetailsForMissingAccrual(Accrual missingAccrual)
		{
			var jobNumber = missingAccrual.Job != null ? missingAccrual.Job.JH_JobNum : ZString.Empty;
			var chargeCode = missingAccrual.ChargeCode != null ? missingAccrual.ChargeCode.AC_Code : ZString.Empty;
			return Res.GetString("8630a371-1340-46e1-9bda-0ad4289d1d08", "Job Number: {0} - Charge Code: {1}", jobNumber, chargeCode);
		}
	}

	struct ConsolCostSelectedAccrualsAndMissingAccruals
	{
		public ConsolCostSelectedAccrualsAndMissingAccruals(ZGuid consolCostPK, List<ZGuid> selectedAccrualPKs, List<Accrual> missingAccrualPKs)
		{
			this.consolCostPK = consolCostPK;
			this.selectedAccrualPKs = new List<ZGuid>();
			this.selectedAccrualPKs.AddRange(selectedAccrualPKs);
			this.missingAccrualPKs = new List<Accrual>();
			this.missingAccrualPKs.AddRange(missingAccrualPKs);
		}

		public readonly ZGuid consolCostPK;
		public readonly List<ZGuid> selectedAccrualPKs;
		public readonly List<Accrual> missingAccrualPKs;
	}
}

