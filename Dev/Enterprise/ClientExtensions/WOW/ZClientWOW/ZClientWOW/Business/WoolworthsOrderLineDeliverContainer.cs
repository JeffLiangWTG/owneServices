using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineDeliverContainer : OrderLineDeliverContainer
	{
		#region Schema

		public new class Schema : OrderLineDeliverContainer.Schema
		{
			public const string LastContainerFromWarfToDepot = "LastContainerFromWarfToDepot";
			public const string LastContainerUnpack = "LastContainerUnpack";
			public const string LastDeliveredToWarehouse = "LastDeliveredToWarehouse";
			public const string LastDeliveredToWarehouseReference = "LastDeliveredToWarehouseReference";
		}

		#endregion

		public WoolworthsOrderLineDeliverContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			((IZPropertyInfoObsolete)J5_QuantityInvoicedInfo).ReadOnly = true;
		}

		#region Validation

		public new WoolworthsOrderLineDeliveryContainerValidation Validation
		{
			get { return (WoolworthsOrderLineDeliveryContainerValidation)base.Validation; }
		}

		protected override JobOrderLineDeliverContainerValidation GetNewValidation()
		{
			return new WoolworthsOrderLineDeliveryContainerValidation(this);
		}

		#endregion

		#region Related Business Objects

		public new WoolworthsOrderLineDelivery OrderLineDelivery
		{
			get { return (WoolworthsOrderLineDelivery)base.OrderLineDelivery; }
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (OrderLineDelivery != null && OrderLineDelivery.OrderLine != null && OrderLineDelivery.OrderLine.Order != null)
				{
					ZQuery filter = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, OrderLineDelivery.OrderLine.Order.JD_OrderNumber);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_OrderNumber, SQLComparisonOperator.NotEqual, ZString.Empty);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_PartNo, OrderLineDelivery.OrderLine.JO_Partno);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_CustomDecimal1, OrderLineDelivery.OrderLine.JO_LineNo);
					var invoiceLines = Factory.Load<BaseJobComInvoiceLine>(filter).Where(line => line is JobComInvoiceLine);

					foreach (WoolworthsJobComInvoiceLine invoiceLine in invoiceLines)
					{
						if (invoiceLine.Declaration != null && invoiceLine.Declaration.JE_RL_NKFinalDestination.ToUpper() == OrderLineDelivery.J4_RL_NKDestinationPort.ToUpper()
							&& invoiceLine.Declaration.JE_MasterBill == J5_MasterBill
							&& invoiceLine.Declaration.CusContainers.Find(J5_ContainerNum) != null)
						{
							return invoiceLine.Declaration;
						}
					}
				}
				return null;
			}
		}

		CusContainer LinkedCusContainer
		{
			get
			{
				CusContainer result = null;

				if (Declaration != null)
				{
					result = Declaration.CusContainers.Find(J5_ContainerNum);
				}

				return result;
			}
		}

		#endregion

		#region Property Overrides

		public override bool IsDelivered
		{
			get { return LastDeliveredToWarehouse.IsValid; }
		}

		public override ZShort J5_PackCount
		{
			get { return base.J5_PackCount; }
			set
			{
				base.J5_PackCount = value;
				CalculateQuantityInvoiced();
			}
		}

		public override ZDecimal J5_QuantityInStore
		{
			get { return base.J5_QuantityInStore; }
			set
			{
				base.J5_QuantityInStore = value;
				OrderLineDelivery.J4_Allocated = OrderLineDelivery.J4_Calc_TotalQuantityAllocated;
			}
		}

		public override ZDecimal J5_QuantityInvoiced
		{
			get { return base.J5_QuantityInvoiced; }
			set
			{
				base.J5_QuantityInvoiced = value;
				OrderLineDelivery.MarkAsNeedingValidation();
			}
		}

		internal void CalculateQuantityInvoiced()
		{
			this.J5_QuantityInvoiced = (OrderLineDelivery.OrderLine.JO_OuterPacks * J5_PackCount);
		}

		#endregion

		#region Bindable JobDeclaration Dates

		public ZDateTime LastContainerFromWarfToDepot
		{
			get { return (ZDateTime)GetPropertyFromLinkedContainer(JobContainerSchema.JC_FCLWharfGateOut.Name, ZDateTime.Empty); }
		}

		public ZPropertyInfo LastContainerFromWarfToDepotInfo
		{
			get { return GetZPropertyInfo(Schema.LastContainerFromWarfToDepot); }
		}

		public ZDateTime LastContainerUnpack
		{
			get { return (ZDateTime)GetPropertyFromLinkedContainer(JobContainerSchema.JC_ArrivalCartageAdvised.Name, ZDateTime.Empty); }
		}
		public ZPropertyInfo LastContainerUnpackInfo
		{
			get { return GetZPropertyInfo(Schema.LastContainerUnpack); }
		}

		public ZDateTime LastDeliveredToWarehouse
		{
			get { return (ZDateTime)GetPropertyFromLinkedContainer(JobContainerSchema.JC_ArrivalCartageComplete.Name, ZDateTime.Empty); }
		}
		public ZPropertyInfo LastDeliveredToWarehouseInfo
		{
			get { return GetZPropertyInfo(Schema.LastDeliveredToWarehouse); }
		}

		public ZString LastDeliveredToWarehouseReference
		{
			get { return (ZString)GetPropertyFromLinkedContainer(JobContainerSchema.JC_ArrivalCartageRef.Name, ZString.Empty); }
		}
		public ZPropertyInfo LastDeliveredToWarehouseReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.LastDeliveredToWarehouseReference); }
		}

		#endregion

		#region Implementation

		object GetPropertyFromLinkedContainer(ZString fieldName, object defaultValue)
		{
			object result = defaultValue;

			if (LinkedCusContainer != null && LinkedCusContainer.JobContainer != null)
			{
				result = LinkedCusContainer.JobContainer[fieldName];
			}

			return result;
		}

		#endregion
	}
}
