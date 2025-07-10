using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class DeliveryCsvRecord : CsvRecord
	{
		public DeliveryCsvRecord(string line) : base(line, 7)
		{
			OrderNumber = FieldValues[1].Trim();
			ProductNo = FieldValues[2].Trim();
			DeliverPoint = FieldValues[3].Trim();
		}

		public ZString OrderNumber;
		public ZString ProductNo;
		public ZString DeliverPoint;

		public override string DisplayIdentifier
		{
			get { return "Delivery for order " + OrderNumber; }
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			WoolworthsOrder order;
			OrderLine line;
			LoadOrderAndOrderLine(factoryProvider.Current, notify, out order, out line);

			if (line != null)
			{
				OrderLineDelivery bO = GetOrCreateDelivery(factoryProvider.Current, order, line, notify);
				if (bO != null)
				{
					((ISupportDataImporting)bO).IsImportingData = true;
					try
					{
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.ZPropertyInfoHash[WoolworthsOrder.DeliverPointIDProperty], FieldValues[3], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J4_RL_NKDestinationPortInfo, FieldValues[4], ForeignKeyType.PortNK, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J4_AllocatedInfo, FieldValues[5], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J4_CustomAttribute1Info, FieldValues[6], ForeignKeyType.None, notify);

						OrgAddress deliverPointBO = LoadDeliverPoint(factoryProvider.Current, order, notify);
						if (deliverPointBO != null)
						{
							bO.J4_OA_NKDeliveryPoint = deliverPointBO.OA_Code;
						}
					}
					finally
					{
						((ISupportDataImporting)bO).IsImportingData = false;
					}
				}
			}
		}

		#region Implementation

		#region Column Mappings

		internal static readonly string[] DeliveryPropertyMappings = new string[]
		{
			"",									// Record Type
			"",									// Order No
			"",									// Part No
			WoolworthsOrder.DeliverPointIDProperty, // this also gets populated to J4_OA_NKDeliveryPoint, if possible
			OrderLineDelivery.Schema.J4_RL_NKDestinationPort,
			OrderLineDelivery.Schema.J4_Allocated,
			OrderLineDelivery.Schema.J4_CustomAttribute1
		};

		#endregion

		protected OrderLineDelivery GetOrCreateDelivery(
			BusinessObjectFactory factory, Order order, OrderLine line, INotifications notify)
		{
			OrderLineDelivery result = null;

			ZQuery deliveryFilter = new ZQuery();
			deliveryFilter.AddToFilter(JoinCondition.And, JobOrderLineDeliverySchema.J4_JO, SQLComparisonOperator.Equal, line.PK);
			deliveryFilter.AddToFilter(JoinCondition.And, JobOrderLineDeliverySchema.J4_CustomAttribute2, SQLComparisonOperator.Equal, this.DeliverPoint);
			NotificationBuffer buffer = new NotificationBuffer(notify);
			result = (OrderLineDelivery)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(
				factory, typeof(OrderLineDelivery), deliveryFilter, DeliverPoint, buffer, false);

			// if no delivery was found (and no duplicate deliveries found), create a new one
			if (!buffer.HasErrors && result == null)
			{
				result = line.Deliveries.AddNew();
			}

			return result;
		}

		protected void LoadOrderAndOrderLine(
			BusinessObjectFactory factory, INotifications notify, out WoolworthsOrder order, out OrderLine line)
		{
			line = null;
			order = (WoolworthsOrder)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(
				factory,
				typeof(WoolworthsOrder),
				new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, OrderNumber),
				OrderNumber,
				notify,
				false);
			if (order == null)
			{
				notify.Notify(new ErrorNotification(WowErrorType.MissingMasterRecord, OrderNumber));
			}
			else
			{
				// if an order was found, continue
				ZQuery lineFilter = new ZQuery();
				lineFilter.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, order.PK);
				lineFilter.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_Partno, SQLComparisonOperator.Equal, ProductNo);

				line = (OrderLine)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(
					factory, typeof(OrderLine), lineFilter, ProductNo, notify, false);
				if (line == null)
				{
					notify.Notify(new ErrorNotification(WowErrorType.MissingMasterRecord, "OrderLine with part number '" + this.ProductNo + "'"));
				}
			}
		}

		protected OrgAddress LoadDeliverPoint(BusinessObjectFactory factory, WoolworthsOrder order, INotifications notify)
		{
			ZQuery filter = order.GetDeliverPointFilter(DeliverPoint);
			OrgAddress result = null;
			if (order.Buyer != null)
			{
				string nKStringForErrorMessage = "buyer=" + order.Buyer.OH_Code + ",warehouse code=" + DeliverPoint;
				result = (OrgAddress)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(
					factory, typeof(OrgAddress), filter, nKStringForErrorMessage, notify, true);
			}
			return result;
		}

		#endregion
	}
}
