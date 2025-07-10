using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business
{
	public interface IJobCostingPlugInHelpers
	{
		GlbBranch FindBranchFromConsolAgentsAndJobHeaders(IJobCostingPlugIn consol, ZGuid company, BusinessObjectFactory factory);
	}

	public class JobCostingPlugInHelpers : IJobCostingPlugInHelpers
	{
		GlbBranch IJobCostingPlugInHelpers.FindBranchFromConsolAgentsAndJobHeaders(IJobCostingPlugIn consol, ZGuid company, BusinessObjectFactory factory)
		{
			return IJobCostingPlugInExtensions.FindBranchFromConsolAgentsAndJobHeaders(consol, company, factory);
		}
	}

	public static class IJobCostingPlugInExtensions
	{
		public static GlbBranch FindBranchFromConsolAgentsAndJobHeaders(this IJobCostingPlugIn consol, ZGuid company, BusinessObjectFactory factory)
		{
			GlbBranch result = null;

			OrgHeader localAgent = (consol != null) ? (consol.CostSupporter.Direction == Directions.Export ? consol.CostSupporter.SendingForwarder : consol.CostSupporter.ReceivingForwarder) : null;

			if (localAgent != null)
			{
				ZQuery branchQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, localAgent.PK);
				branchQuery.AddToFilter(GlbBranchSchema.GB_GC, company);
				branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				result = factory.LoadTop1<GlbBranch>(branchQuery);
			}

			if (result == null && consol != null)
			{
				foreach (IJobInvoicingPlugIn shipment in consol.CostSupporter.ShipmentsList)
				{
					Job job = new Job.Loader(factory, shipment).Load();
					if (job != null)
					{
						if (result == null)
						{
							result = job.Branch;
						}

						if (result != null && job.JH_GB != result.PK)
						{
							result = null;
							break;
						}
					}
				}
			}

			return result;
		}

		public static bool IsLoadPortLocal(this IJobCostingPlugIn consol)
		{
			var result = false;
			if (consol != null)
			{
				result = consol.LoadPort != null && consol.DischargePort != null && ImportExportHelper.GetJobDirection(consol.LoadPort.RL_Code, consol.DischargePort.RL_Code) == Directions.Export;
			}

			return result;
		}

		public static OrgHeader AgentToInvoice(this IJobCostingPlugIn consol)
		{
			return consol.IsLoadPortLocal() ? consol.ReceivingAgentAPInvoicingParty : consol.SendingAgentAPInvoicingParty;
		}

		public static ZString GetApportionmentMethod(this IJobCostingPlugIn consol, AccChargeCode chargeCode)
		{
			if (consol == null || chargeCode == null)
			{
				return ZString.Empty;
			}

			var matchedMethods = GetApportionMethod(consol,
					chargeCode.ApportionmentMethodOverrides.Cast<AccChargeApportionmentMethodOverride>()
				) ?? GetApportionMethod(consol,
					AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.Value
					.ConsolCostDefaultApportionmentMethodCollection
					.Cast<ConsolCostDefaultApportionmentMethod>()
					);

			return matchedMethods != null
				? matchedMethods.ApportionmentMethod.ToString()
				: AllocationMethod.ChargeableUnits;
		}

		static IApportionmentMethodOverride GetApportionMethod(IJobCostingPlugIn consol, IEnumerable<IApportionmentMethodOverride> consolCostApportionmentMethods)
		{
			return consolCostApportionmentMethods
				.Where((method) => MatchesApportionmentMethod(consol, method))
				.OrderBy((method) => method.GetSpecificityScore())
				.LastOrDefault();
		}

		static bool MatchesApportionmentMethod(IJobCostingPlugIn consol, IApportionmentMethodOverride method)
		{
			return (method.TransportMode == ApportionmentMethod.AllCode || (consol != null && method.TransportMode == consol.TransportMode))
				&& (method.ContainerMode == ApportionmentMethod.AllCode || (consol != null && method.ContainerMode == consol.ContainerMode))
				&& (method.ConsolType == ApportionmentMethod.AllCode || (consol != null && method.ConsolType == consol.ConsolType))
				&& (method.Module == ApportionmentMethod.AllCode || (consol != null && method.Module == consol.Module))
				&& (method.Direction == ApportionmentMethod.AllCode || (consol != null && method.Direction == consol.Direction));
		}

		public static ZBool IsGatewayCharge(this IJobCostingPlugIn consol, Charge charge)
		{
			if (consol != null && charge != null
				&& charge.SellAccount != null
				&& charge.ChargeCode.IsGatewayRelated()
				&& consol is IGateway gatewaySupporter
				&& gatewaySupporter.GatewayBillingSupporter != null)
			{
				foreach (var company in charge.SellAccount.CompanyProxies(false))
				{
					var agents = gatewaySupporter.GatewayBillingSupporter.GatewayAgent(company);

					if (charge.SellAccount == agents.sendingAgent || charge.SellAccount == agents.receivingAgent)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static ZBool IsAgentCharge(this IJobCostingPlugIn consol, Charge charge)
		{
			return consol != null && charge?.Job != null
				&& charge.JR_OH_SellAccount.IsValid
				&& (charge.JR_OH_SellAccount == charge.Job.AgentCollectPK
				|| charge.JR_OH_SellAccount == consol.SendingAgent?.PK
				|| charge.JR_OH_SellAccount == (charge.InvoicingJob?.PlugInData?.InvoicingSupporter?.DeliveryAgent ?? consol.ReceivingAgent)?.PK
				|| charge.JR_OH_SellAccount == consol.ReceivingAgentAPInvoicingParty?.PK);
		}

		public static bool HasCostSupporterPK(this IJobCostingPlugIn businessObject)
		{
			return businessObject != null && businessObject.CostSupporter.PK != ZGuid.Empty;
		}
	}
}

