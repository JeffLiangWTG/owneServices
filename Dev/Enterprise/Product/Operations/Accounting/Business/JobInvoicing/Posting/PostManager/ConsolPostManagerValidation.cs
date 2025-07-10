using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolPostManagerValidation : PostManagerValidation
	{
		public ConsolPostManagerValidation(IEnumerable<Job> jobs, IJobCostingPlugIn consol, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs, bool isBulkPosting = false)
			: base(jobs, postingOption, originalJobs, isBulkPosting)
		{
			Consol = consol;
		}

		readonly IJobCostingPlugIn Consol;

		protected override ZBool IsAgentCharge(Charge charge) => Consol.IsAgentCharge(charge);

		protected override ZBool IsGatewayCharge(Charge charge) => Consol.IsGatewayCharge(charge);

		protected override ZBool IsEnableValidationForSupplyTypeOfConsolApportionedCharges => true;

		protected override string RunNoChargesValidation()
		{
			string result = "";
			if (PostingOption == JobInvoicingPostingOption.Costs)
			{
				result = base.RunNoChargesValidation();
			}
			return result;
		}

		protected override string RunConsolCostTaxBranchValidation()
		{
			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				return string.Empty;
			}

			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						var postInfoCost = !charge.IsCostPosted && IsCostEligibleToPost(charge) ? jobToPost.PlugInData.InvoicingSupporter.ConsumerType.ShouldPostCharges(jobToPost.PlugInData, charge.JR_InvoiceType, true) : null;
						if (postInfoCost != null && postInfoCost.PostAllowed && charge.JR_IsApportioned)
						{
							var consolCost = charge.Factory.Load<JobConsolCost>(charge.JR_E6);
							consolCost.Validation.ValidateE6_GB_CostTaxBranch();
							if (consolCost.E6_GB_CostTaxBranchInfo.HasErrors())
							{
								var notificationCollector = new ZNotificationCollector(consolCost, false, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
								return notificationCollector.GetErrors().ToUniqueMessageListString();
							}
						}
					}
				}
			}

			return string.Empty;
		}
	}
}
