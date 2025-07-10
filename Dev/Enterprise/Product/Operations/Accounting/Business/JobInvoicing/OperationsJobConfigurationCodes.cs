using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class OperationsJobConfigurationCodes
	{
		public OperationsJobConfigurationCodes(IJobHeaderParent jobParent)
		{
			JobParent = jobParent;
		}

		readonly IJobHeaderParent JobParent;

		public string ConsumerTypeCode
		{
			get
			{
				string consumerTypeCode = InvoiceDateConfigurationLookups.JobTypeAdditionalCodes.All;

				var jobPlugin = JobParent as IJobInvoicingPlugIn;
				var parentBizO = JobParent as BusinessObject;

				if (jobPlugin != null && jobPlugin.InvoicingSupporter != null && parentBizO != null && !parentBizO.IsDeleted)
				{
					var consumerType = jobPlugin.InvoicingSupporter.ConsumerType;
					if (consumerType != null)
					{
						consumerTypeCode = consumerType.Code;
					}
				}

				return consumerTypeCode;
			}
		}

		public string DirectionCode
		{
			get
			{
				string directionCode = Constants.FreightShipmentDirection.Code.All;

				IJobInvoicingPlugIn jobPlugin = JobParent as IJobInvoicingPlugIn;
				BusinessObject jobParentBizO = JobParent as BusinessObject;
				bool isPluginDataDeleted = jobParentBizO != null && jobParentBizO.IsDeleted;

				if (jobPlugin != null && !isPluginDataDeleted)
				{
					directionCode = Job.GetDirection(jobPlugin);
				}

				return directionCode;
			}
		}

		public string TransportMode
		{
			get
			{
				string transportMode = InvoiceDateConfigurationLookups.ModeAdditionalCodes.All;

				IJobInvoicingPlugIn jobPlugin = JobParent as IJobInvoicingPlugIn;
				BusinessObject jobParentBizO = JobParent as BusinessObject;
				bool isPluginDataDeleted = jobParentBizO != null && jobParentBizO.IsDeleted;

				if (jobPlugin != null && !isPluginDataDeleted)
				{
					var mode = Job.GetTransportMode(jobPlugin);
					if (!mode.IsEmpty)
					{
						transportMode = mode;
					}
				}

				return transportMode;
			}
		}

		public string Broker
		{
			get
			{
				string broker = InvoiceDateConfigurationLookups.BrokerCodes.All;

				var jobPlugin = JobParent as IJobInvoicingPlugIn;
				var parentBizO = JobParent as BusinessObject;

				if (jobPlugin != null && jobPlugin.InvoicingSupporter != null && parentBizO != null && !parentBizO.IsDeleted)
				{
					var supporter = jobPlugin.InvoicingSupporter;
					if (supporter.Broker != null)
					{
						broker = jobPlugin.InvoicingSupporter.Broker.IsProxyOrg(GlbCompany.CurrentCompany)
							? InvoiceDateConfigurationLookups.BrokerCodes.Internal
							: InvoiceDateConfigurationLookups.BrokerCodes.External;
					}
					else if (supporter.ConsumerType.Equals(JobInvoicingConsumerTypes.ForwardingConsol)
						|| supporter.ConsumerType.Equals(JobInvoicingConsumerTypes.GatewayConsol))
					{
						broker = string.Empty;
					}
				}

				return broker;
			}
		}
	}
}
