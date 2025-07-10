using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTasks.XTCredentialManagement.Inbound
{
	public class XTCredentialManagementInboundProcessor
	{
		readonly BusinessObjectFactory factory;
		public XTCredentialManagementInboundProcessor(BusinessObjectFactory businessObjectFactory)
		{
			factory = businessObjectFactory;
		}

		protected void AddFilter(ZQuery filter)
		{
			filter.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Queued);
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			filter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.XMS);
			filter.AddToFilter(EDIInterchangeSchema.EI_From, "XH");
			var transportFilter = new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
			transportFilter.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.tXT), JoinCondition.Or);
			filter.AddToFilter(transportFilter);
			filter.OrderBy = AutoEDIInterchange.Schema.EI_SystemCreateTimeUtc;
		}

		public virtual void Process(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var filter = new ZQuery();
			AddFilter(filter);

			foreach (var response in factory.Load<EDIInterchange>(filter))
			{
				using (Environment.DisposableEnvironment.ForBranch(response.EI_GB.ToGuid()))
				{
					cancellationToken.ThrowIfCancellationRequested();
					response.EI_Status = EDIInterchangeStatusList.Codes.Received;
					if (response.EI_BodyText.Contains("<EventType>IRJ</EventType>"))
					{
						var requestFilter = new ZQuery(EDIInterchangeSchema.PK, response.EI_SessionGUID);
						requestFilter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
						foreach (var request in factory.Load<EDIInterchange>(requestFilter))
						{
							request.EI_Status = EDIInterchangeStatusList.Codes.Queued;
						}
					}
					factory.Save();
				}
			}
		}
	}
}
