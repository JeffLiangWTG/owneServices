using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class OrderLineCsvRecord : CsvRecord
	{
		public OrderLineCsvRecord(string line) : base(line, 20)
		{
			OrderNumber = FieldValues[1];
			ProductNo = FieldValues[2];
			OperationCode = char.ToUpper(FieldValues[3][0]);
		}

		public ZString OrderNumber;
		public ZString ProductNo;
		public char OperationCode;

		public override string DisplayIdentifier
		{
			get { return "Order line for order " + OrderNumber; }
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			if (OperationCode == 'D')
			{
				WoolworthsOrderLine line = GetOrCreateOrderLine(factoryProvider.Current, notify);
				if (line != null)
				{
					line.Delete();
				}
			}
			else
			{
				WoolworthsOrderLine bO = GetOrCreateOrderLine(factoryProvider.Current, notify);
				if (bO != null)
				{
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_PartnoInfo, FieldValues[2], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_QuantityInfo, FieldValues[4], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_ItemPriceInfo, FieldValues[5], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomDate1Info, FieldValues[6], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomDate2Info, FieldValues[7], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomDate3Info, FieldValues[8], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomDate4Info, FieldValues[9], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_InnerPacksInfo, FieldValues[10], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_OuterPacksInfo, FieldValues[11], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomDate5Info, FieldValues[12], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_PartAttrib3Info, FieldValues[13], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_DescriptionInfo, FieldValues[14], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomAttrib4Info, FieldValues[15], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_PartAttrib1Info, FieldValues[16], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomFlag2Info, FieldValues[17], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						bO.JO_CustomAttrib5Info, FieldValues[18], ForeignKeyType.None, notify);
				}
			}
		}

		#region Implementation

		#region Column Mappings

		internal static readonly string[] OrderLinePropertyMappings = new string[]
		{
			"",									// Record Type
			"",									// Order No
			OrderLine.Schema.JO_Partno,			// Item Ref No
			"",									// Order Line Ind
			OrderLine.Schema.JO_Quantity,		// Qty Order
			OrderLine.Schema.JO_ItemPrice,		// Home Cost
			OrderLine.Schema.JO_CustomDate1,	// Ship Date
			OrderLine.Schema.JO_CustomDate2,	// Port Arrive Date
			OrderLine.Schema.JO_CustomDate3,	// Warehouse Date
			OrderLine.Schema.JO_CustomDate4,	// Advert. Date
			OrderLine.Schema.JO_InnerPacks,		// OM Size
			OrderLine.Schema.JO_OuterPacks,		// VP Size 
			OrderLine.Schema.JO_CustomDate5,	// New Line Date
			OrderLine.Schema.JO_PartAttrib3,	// Fine Departmen
			OrderLine.Schema.JO_Description  ,	// Item Description
			OrderLine.Schema.JO_CustomAttrib4,	// Prefix
			OrderLine.Schema.JO_PartAttrib1,	// Port Split Ind
			OrderLine.Schema.JO_CustomFlag2,	// QA Application
			OrderLine.Schema.JO_CustomAttrib5,	// Country Code
			"",									// Tot. Home Cost
		};

		#endregion

		protected WoolworthsOrderLine GetOrCreateOrderLine(BusinessObjectFactory factory, INotifications notify)
		{
			WoolworthsOrderLine result = null;
			ZGuid orderPK = WowStringToBusinessObjectFieldConverter.Instance.GetPKFromNK(
				factory, typeof(WoolworthsOrder), JobOrderHeaderSchema.JD_OrderNumber, OrderNumber, notify);

			if (orderPK.IsEmpty)
			{
				notify.Notify(new ErrorNotification(WowErrorType.MissingMasterRecord, OrderNumber));
			}
			else
			{
				WoolworthsOrder theOrder = (WoolworthsOrder)factory.Load(typeof(WoolworthsOrder), orderPK);
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, orderPK);
				filter.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_Partno, SQLComparisonOperator.Equal, ProductNo);

				result = (WoolworthsOrderLine)WowStringToBusinessObjectFieldConverter.Instance.LoadBOAndExpectOnly1(factory, typeof(WoolworthsOrderLine), filter, ProductNo, notify, false);
				if (result == null)
				{
					result = (WoolworthsOrderLine)theOrder.OrderLines.AddNew();
					result.JO_LineNo = 0;
					result.JO_LineNo = GetNextOrderLineNo(factory, orderPK);
				}
			}
			return result;
		}

		protected ZInt GetNextOrderLineNo(BusinessObjectFactory factory, ZGuid orderPK)
		{
			ZQuery lastLineFilter = new ZQuery(JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, orderPK);
			lastLineFilter.OrderBy = OrderLine.Schema.JO_LineNo + " DESC";
			OrderLine lastLine = (OrderLine)factory.LoadTop1(typeof(OrderLine), lastLineFilter);
			return (lastLine == null) ? 1 : lastLine.JO_LineNo + 1;
		}

		#endregion
	}
}
