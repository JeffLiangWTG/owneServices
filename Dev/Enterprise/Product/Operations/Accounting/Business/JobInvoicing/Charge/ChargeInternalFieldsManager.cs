using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChargeInternalFieldsManager
	{
		public ChargeInternalFieldsManager(BaseCharge charge)
		{
			Charge = charge;
		}

		protected BaseCharge Charge;
		protected BusinessObjectFactory Factory => Charge.Factory;

		public void SetCostAndSellAccountFromInternalBranch()
		{
			if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled()
				&& (Charge.InternalBranch?.OrgProxy?.IsProxyOrg(Charge.CalculatedCompany) ?? false))
			{
				if (Charge.CostAccountIsOrgProxy)
				{
					Charge.JR_OH_CostAccount = Charge.InternalBranch.GB_OH_OrgProxy;
					Charge.Validation.ValidateJR_GB_InternalBranch();
				}

				if (Charge.SellAccountIsOrgProxy)
				{
					Charge.JR_OH_SellAccount = Charge.InternalBranch.GB_OH_OrgProxy;
					Charge.Validation.ValidateJR_GB_InternalBranch();
				}
			}
		}

		public void ResetInternalFields(ZGuid newOrgPK, bool isSettingFromDebtor = false)
		{
			if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled()
				&& Charge.IsConsumerTypeShoudCreateCostOrSellJRJ
				&& !Charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.IsSuspended)
			{
				using (Charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					var orgHeader = Factory.Load<OrgHeader>(newOrgPK);
					var isNewValueOrgProxy = orgHeader?.IsProxyOrg(Charge.CalculatedCompany) ?? false;
					if (Charge.Job != null && isNewValueOrgProxy && !Charge.IsExcludedFromAutoJRJ(orgHeader))
					{
						if (Charge is ApportionSplitCharge splitCharge
							&& (splitCharge.ParentConsolCost?.Consol?.IsGatewayBillingEnabled() ?? false))
						{
							splitCharge.CopyInternalFieldsOver(splitCharge.ParentConsolCost.SellChargeFromSellToCostSynchronisation);
						}
						else
						{
							var relatedJob = Charge.RelatedJob?.InvoicingSupporter.Job;
							if (relatedJob != null
								&& isSettingFromDebtor
								&& Charge.Job.IsGatewayBillingJob())
							{
								DefaultInternalFieldsFromJob(relatedJob, false, true);
							}
							else
							{
								var shipment = Charge.Job?.Parent as ForwardingShipment;
								var forwardingConsol = Charge.Job?.Parent as ForwardingConsol;
								var needToResetInternalFieldsToBlank = false;
								if (!isSettingFromDebtor)
								{
									if (shipment != null && shipment.Gateways.Any(x => x.ForwarderPK == newOrgPK))
									{
										needToResetInternalFieldsToBlank = true;
									}
									else if (forwardingConsol != null && forwardingConsol.IsGatewayBillingEnabled())
									{
										var gwAgents = forwardingConsol.GatewayAgent();
										if (gwAgents.sendingAgent?.PK == newOrgPK || gwAgents.receivingAgent?.PK == newOrgPK)
										{
											needToResetInternalFieldsToBlank = true;
										}
									}
								}
								if (needToResetInternalFieldsToBlank)
								{
									ResetInternalFieldsToBlank(Charge);
								}
								else
								{
									DefaultInternalFieldsFromJob(Charge.Job);
									DefaultInternalBranchFromOrgProxy(newOrgPK);
								}
							}
						}
					}
					else
					{
						ResetInternalFieldsToBlank(Charge);
					}
				}
			}
		}

		void ResetInternalFieldsToBlank(BaseCharge charge)
		{
			charge.JR_GB_InternalBranch = ZGuid.Empty;
			charge.JR_GE_InternalDept = ZGuid.Empty;
			charge.JR_JH_InternalJob = ZGuid.Empty;
		}

		void DefaultInternalFieldsFromJob(JobHeader job, bool setOnlyIfEmpty = true, bool isGatewayBillingJob = false)
		{
			if (Charge.JR_GE_InternalDept.IsEmpty || !setOnlyIfEmpty)
			{
				Charge.JR_GE_InternalDept = job.JH_GE;
			}
			if (Charge.JR_GB_InternalBranch.IsEmpty || !setOnlyIfEmpty)
			{
				Charge.JR_GB_InternalBranch = job.JH_GB;
			}
			if (Charge.JR_JH_InternalJob.IsEmpty || !setOnlyIfEmpty)
			{
				if (isGatewayBillingJob && AccountingMasterFilesRegistry.Instance.GetInternalJobConfigurationSetting.Value)
				{
					Charge.JR_JH_InternalJob = GatewayInvoiceTargetJobFinder.GetInternalJob(job, Charge.JR_Calc_RelatedJobNumber, Charge.InvoicingJob);
					Charge.JR_GB_InternalBranch = Charge.InternalJob?.JH_GB ?? ZGuid.Empty;
					Charge.JR_GE_InternalDept = Charge.InternalJob?.JH_GE ?? ZGuid.Empty;
				}
				else
				{
					Charge.JR_JH_InternalJob = job.PK;
				}
			}
		}

		void DefaultInternalBranchFromOrgProxy(ZGuid newOrgProxyPK)
		{
			var branchQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, newOrgProxyPK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, Charge.CalculatedCompany.PK);
			var branches = Factory.Load<GlbBranch>(branchQuery);

			Charge.JR_GB_InternalBranch = ZGuid.Empty;
			if (branches.Length == 1)
			{
				Charge.JR_GB_InternalBranch = branches.First().PK;
			}
			else if (branches.Length > 1)
			{
				var agents = Charge.Job.Parent?.GatewayAgent();
				if (agents?.sendingAgent?.PK == newOrgProxyPK || agents?.receivingAgent?.PK == newOrgProxyPK)
				{
					Charge.JR_GB_InternalBranch = Charge.InternalJob?.JH_GB ?? ZGuid.Empty;
				}
			}
		}
	}
}
