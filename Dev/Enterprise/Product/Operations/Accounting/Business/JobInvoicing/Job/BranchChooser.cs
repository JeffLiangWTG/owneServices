using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class BranchChooser
	{
		public const string PickupAgentBranch = "PAB";
		public const string DeliveryAgentBranch = "DAB";
		public const string ControllingAgentBranch = "CAB";
		public const string SendingForwarderBranch = "SFB";
		public const string ReceivingForwarderBranch = "RFB";

		public BranchChooser(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			CustomBranchChooser = new CustomBranchChooser(factory);
		}

		readonly CustomBranchChooser CustomBranchChooser;

		public ZGuid GetBranch(OrgHeader localClient, OrgHeader overseasAgent, IJobInvoicingPlugIn pluginParent)
		{
			var result = ZGuid.Empty;
			if (pluginParent != null)
			{
				if (AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.Value)
				{
					var branchDefaultingManager = ObjectFactory.Get<IJobBillingBranchDefaultingManager>();
					branchDefaultingManager.SetDefaultValue(pluginParent, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
					if (branchDefaultingManager.DefaultValue is ZGuid defaultAsGuid)
					{
						result = defaultAsGuid;
					}
					else if (branchDefaultingManager.DefaultValue is ZString defaultAsString)
					{
						result = GetBranchByDynamicBranchCode(defaultAsString, pluginParent);
					}
				}

				if (result.IsEmpty)
				{
					var branchCode = CustomBranchChooser.GetCodeWithConfiguration(pluginParent);
					if (!branchCode.IsEmpty)
					{
						result = GetBranchForCustomConfiguration(branchCode);
					}
					else
					{
						result = GetBranchCore(pluginParent.InvoicingSupporter.ConsumerType, localClient, overseasAgent, pluginParent.InvoicingSupporter.Origin, pluginParent.InvoicingSupporter.Destination, pluginParent.InvoicingSupporter.OperationsBranch);
					}
				}
			}
			else
			{
				result = GetBranchCore(null, localClient, overseasAgent, null, null, null);
			}

			return result;
		}

		internal ZGuid GetBranchByDynamicBranchCode(ZString dynamicBranchCode, IJobInvoicingPlugIn pluginParent)
		{
			var shipment = pluginParent as ForwardingShipment;
			var consol = pluginParent as ForwardingConsol;
			OrgHeader org = null;

			switch (dynamicBranchCode)
			{
				case PickupAgentBranch:
					org = shipment?.PickupAgent;
					break;
				case DeliveryAgentBranch:
					org = shipment?.DeliveryAgent;
					break;
				case ControllingAgentBranch:
					org = shipment?.ControllingAgent;
					break;
				case SendingForwarderBranch:
					org = consol?.SendingForwarder;
					break;
				case ReceivingForwarderBranch:
					org = consol?.ReceivingForwarder;
					break;
			}

			var branch = GlbBranch.FindByOrgProxy(Factory, org, true, true);
			var result = (branch == null) ? ZGuid.Empty : branch.PK;

			return result;
		}

		public ZGuid GetBranch(OrgHeader localClient, OrgHeader overseasAgent, RefUNLOCO loadPort, RefUNLOCO dischargePort)
		{
			return GetBranchCore(null, localClient, overseasAgent, loadPort, dischargePort, null);
		}

		public ZGuid GetBranch(JobInvoicingConsumerType consumerType, OrgHeader localClient, OrgHeader overseasAgent,
			RefUNLOCO loadPort, RefUNLOCO dischargePort, GlbBranch operationsBranch)
		{
			return GetBranchCore(consumerType, localClient, overseasAgent, loadPort, dischargePort, operationsBranch);
		}

		protected ZGuid GetBranchCore(JobInvoicingConsumerType consumerType, OrgHeader localCharges, OrgHeader overseasAgent,
			RefUNLOCO loadPort, RefUNLOCO dischargePort, GlbBranch operationsBranch)
		{
			IJobBranchDefaultOrderRule orderRulesForDefaulting = AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.Value;
			ZGuid result = ZGuid.Empty;

			for (int i = 1; i <= 4; i++)
			{
				if (i == orderRulesForDefaulting.DefaultToBlank)
				{
					result = ZGuid.Empty;
					break;
				}
				else if (i == orderRulesForDefaulting.DefaultToBranchRelatedToPortOrWarehouseBranch)
				{
					result = GetBranchRelatedToPortOrOperationsBranch(consumerType, loadPort, dischargePort, operationsBranch);
					if (result.IsValid)
					{
						break;
					}
				}
				else if (i == orderRulesForDefaulting.DefaultToBranchOfOrganisation)
				{
					result = GetBranchOfOrganisation(localCharges, overseasAgent);
					if (result.IsValid)
					{
						break;
					}
				}
				else if (i == orderRulesForDefaulting.DefaultToLoginUserDefault)
				{
					result = GlbBranch.CurrentBranch.PK;
					if (result.IsValid)
					{
						break;
					}
				}
			}

			return result;
		}

		#region Implementation

		protected ZGuid GetBranchRelatedToPortOrOperationsBranch(JobInvoicingConsumerType consumerType,
			RefUNLOCO loadPort, RefUNLOCO dischargePort, GlbBranch operationsBranch)
		{
			ZGuid result = ZGuid.Empty;

			if (operationsBranch != null && operationsBranch.GB_IsActive && operationsBranch.GB_GC == GlbCompany.CurrentCompany.PK &&
				(consumerType == JobInvoicingConsumerTypes.WarehouseInwards ||
				consumerType == JobInvoicingConsumerTypes.WarehouseOutwards ||
				consumerType == JobInvoicingConsumerTypes.WarehouseStorage ||
				consumerType == JobInvoicingConsumerTypes.LocalCartage ||
				consumerType == JobInvoicingConsumerTypes.WarehouseAdHocServiceJob))
			{
				result = operationsBranch.PK;
			}
			else if (operationsBranch != null && operationsBranch.GB_IsActive &&
				(consumerType == JobInvoicingConsumerTypes.TransportBookingConsignment ||
				consumerType == JobInvoicingConsumerTypes.TransportBooking))
			{
				result = operationsBranch.PK;
			}
			else
			{
				if (dischargePort != null)
				{
					result = FindBranchInCurrentCompanyByHomePort(dischargePort);
				}

				if (loadPort != null && result.IsEmpty)
				{
					result = FindBranchInCurrentCompanyByHomePort(loadPort);
				}
			}

			return result;
		}

		protected ZGuid FindBranchInCurrentCompanyByHomePort(RefUNLOCO port)
		{
			ZGuid result = ZGuid.Empty;

			if (port != null && !port.RL_Code.IsEmpty)
			{
				ZQuery filter = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, port.RL_Code) { OrderBy = GlbBranchSchema.GB_Code.Name };
				filter.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				filter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				GlbBranch branch = Factory.LoadTop1<GlbBranch>(filter);

				if (branch != null)
				{
					result = branch.PK;
				}
				else
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbBranch)) { OrderBy = GlbBranchSchema.GB_Code.Name };
					query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
					query.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
					subQuery.AddToFilter(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, port.RL_Code);
					query.AddSubQuery(subQuery, JoinCondition.And);

					branch = Factory.LoadTop1<GlbBranch>(query);
					result = branch != null ? branch.PK : ZGuid.Empty;
				}
			}

			return result;
		}

		protected ZGuid GetBranchOfOrganisation(OrgHeader localClient, OrgHeader overseasAgent)
		{
			ZGuid result = ZGuid.Empty;

			if (localClient != null && localClient.Branch != null && localClient.Branch.GB_IsActive && localClient.Branch.GB_GC == GlbCompany.CurrentCompany.PK)
			{
				result = localClient.CompanyData.OB_GB_ControllingBranch;
			}
			else if (overseasAgent != null && overseasAgent.Branch != null && overseasAgent.Branch.GB_IsActive && overseasAgent.Branch.GB_GC == GlbCompany.CurrentCompany.PK)
			{
				result = overseasAgent.CompanyData.OB_GB_ControllingBranch;
			}

			return result;
		}

		ZGuid GetBranchForCustomConfiguration(ZString branchCode)
		{
			var branchBizObj = GetBranchBizObjFromBranchCode(Factory, branchCode);
			if (branchBizObj != null && branchBizObj.GB_IsActive)
			{
				return branchBizObj.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		internal static GlbBranch GetBranchBizObjFromBranchCode(BusinessObjectFactory factory, ZString branchCode)
		{
			GlbBranch branch = null;
			if (factory != null && !branchCode.IsEmpty)
			{
				var query = new ZQuery(GlbBranchSchema.GB_Code, branchCode);
				branch = factory.LoadTop1<GlbBranch>(query);
			}
			return branch;
		}

		readonly BusinessObjectFactory Factory;

		#endregion
	}
}
