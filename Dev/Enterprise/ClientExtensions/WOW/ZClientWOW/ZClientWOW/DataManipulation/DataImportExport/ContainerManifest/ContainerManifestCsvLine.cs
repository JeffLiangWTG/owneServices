using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class ContainerManifestCsvLine : CsvRecord
	{
		internal ContainerManifestCsvLine(ContainerManifestCsvImportFile importFile, string line) : base(line, 12)
		{
			this.ImportFile = importFile;
			this.ProductNo = FieldValues[3].Trim().PadLeft(6, '0');
			this.OrderNumber = FieldValues[4];
		}

		internal ContainerManifestCsvImportFile ImportFile;
		public string ProductNo;
		public string OrderNumber;

		public string ContainerNumber
		{
			get { return ImportFile.ContainerNumber; }
		}

		public string ContainerSeal
		{
			get { return ImportFile.ContainerSeal; }
		}

		public string PortOfLoading
		{
			get { return this.ImportFile.PortOfLoading; }
		}

		public string DestinationPort
		{
			get { return ImportFile.DestinationPort; }
		}

		public string Vessel
		{
			get { return this.ImportFile.Vessel; }
		}

		public string Voyage
		{
			get { return this.ImportFile.Voyage; }
		}

		public string Origin
		{
			get { return this.CsvLine.FieldValues[9].Trim() + this.CsvLine.FieldValues[10].Trim(); }
		}

		public override string DisplayIdentifier
		{
			get { return "Container for order " + OrderNumber; }
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			WoolworthsOrder order;
			OrderLine line;
			OrderLineDelivery delivery;
			LoadOrderAndOrderLineAndDelivery(factoryProvider.Current, notify, out order, out line, out delivery);

			if (order != null && line != null && delivery != null)
			{
				OrderLineDeliverContainer bO = GetOrCreateContainer(factoryProvider.Current, order, line, delivery, notify);
				if (bO != null)
				{
					((ISupportDataImporting)bO).IsImportingData = true;
					try
					{
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J5_PackCountInfo, FieldValues[5], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J5_VolumeInfo, FieldValues[6], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J5_WeightInfo, FieldValues[7], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J5_QuantityInvoicedInfo, FieldValues[8], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.J5_MasterBillInfo, FieldValues[11], ForeignKeyType.None, notify);

						bO.J5_ContainerNum = ContainerNumber;
						bO.J5_ContainerSeal = ContainerSeal;
						bO.J5_RL_NKLoadPort = PortOfLoading;
						bO.J5_RV_NKArrivalVessel = Vessel;
						bO.J5_Voyage = Voyage;
						bO.J5_CustomAttribute1 = Origin;
						bO.J5_ETD = this.ImportFile.ETD;
						if (!bO.J5_ETD.IsValid)
						{
							notify.Notify(new ErrorNotification(WowErrorType.InvalidFileFormat, "ETD"));
						}

						bO.J5_ETA = this.ImportFile.ETA;
						if (!bO.J5_ETA.IsValid)
						{
							notify.Notify(new ErrorNotification(WowErrorType.InvalidFileFormat, "ETA"));
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

		internal static readonly string[] ContainerPropertyMappings = new string[]
		{
			"", // Line Number
			"", // Supplier
			"", // Product Name
			"", // Product Reference
			"", // Order Number
			OrderLineDeliverContainer.Schema.J5_PackCount, // Packs
			OrderLineDeliverContainer.Schema.J5_Volume, // Volume
			OrderLineDeliverContainer.Schema.J5_Weight, // Volume
			OrderLineDeliverContainer.Schema.J5_QuantityInvoiced, // Quantity Invoiced
			"", // CO
			"", // Office
			OrderLineDeliverContainer.Schema.J5_MasterBill, // OBL
		};

		#endregion

		protected OrderLineDeliverContainer GetOrCreateContainer(
			BusinessObjectFactory factory, Order order, OrderLine line, OrderLineDelivery delivery, INotifications notify)
		{
			OrderLineDeliverContainer result = null;

			ZQuery containerFilter = new ZQuery();
			containerFilter.AddToFilter(JoinCondition.And, JobOrderLineDeliverContainerSchema.J5_J4, SQLComparisonOperator.Equal, delivery.PK);
			containerFilter.AddToFilter(JoinCondition.And, JobOrderLineDeliverContainerSchema.J5_ContainerNum, SQLComparisonOperator.Equal, this.ContainerNumber);
			NotificationBuffer buffer = new NotificationBuffer(notify);
			result = (OrderLineDeliverContainer)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(
				factory, typeof(OrderLineDeliverContainer), containerFilter, this.ContainerNumber, buffer, false);

			// if no delivery was found (and no duplicate deliveries found), create a new one
			if (!buffer.HasErrors && result == null)
			{
				result = delivery.Containers.AddNew();
				result.J5_J4 = delivery.PK;
			}

			return result;
		}

		protected void LoadOrderAndOrderLineAndDelivery(
			BusinessObjectFactory factory, INotifications notify,
			out WoolworthsOrder order, out OrderLine line, out OrderLineDelivery delivery)
		{
			line = null;
			delivery = null;
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
					notify.Notify(new ErrorNotification(WowErrorType.MissingMasterRecord, "OrderLine with part no '" + ProductNo + "'"));
				}
				else
				{
					// if a delivery was found, continue
					ZQuery deliveryFilter = new ZQuery();
					deliveryFilter.AddToFilter(JoinCondition.And, JobOrderLineDeliverySchema.J4_JO, SQLComparisonOperator.Equal, line.PK);
					deliveryFilter.AddToFilter(JoinCondition.And, JobOrderLineDeliverySchema.J4_RL_NKDestinationPort, SQLComparisonOperator.Equal, DestinationPort);

					delivery = (OrderLineDelivery)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(
						factory, typeof(OrderLineDelivery), deliveryFilter, DestinationPort, notify, false);
					if (delivery == null)
					{
						notify.Notify(new ErrorNotification(WowErrorType.MissingMasterRecord, "Delivery with destination port '" + DestinationPort + "', order #" + OrderNumber + ", part #" + ProductNo));
					}
				}
			}
		}

		#endregion
	}
}
