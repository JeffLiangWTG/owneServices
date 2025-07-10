using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineDelivery : OrderLineDelivery
	{
		#region ICustomLabelsProvider

		protected class CustomLabelsProviderSubClassImpl : OrderLineDelivery.CustomLabelsProvider
		{
			public CustomLabelsProviderSubClassImpl(ICustomLabelsConfigOrgProvider orgProvider) : base(orgProvider)
			{
			}

			public override CustomLabelInfoList GetCustomFields(OrgHeader org, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = base.GetCustomFields(org, factory);
				result.Remove(Schema.J4_CustomAttribute1);
				result.Add(Core.Constants.CustomLabels.OrderLineDelivery.CustomAttribute1, Schema.J4_CustomAttribute1, typeof(ZString), (NoResString)"Custom Attribute 1", (NoResString)"Custom Attribute 1", CustomLabelStyles.UpperCase | CustomLabelStyles.AvailableByDefault);
				return result;
			}
		}

		#endregion

		public WoolworthsOrderLineDelivery(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static void RegisterThisSubTypeOverride()
		{
			OrderLineDelivery.CustomLabelsProviderType = typeof(WoolworthsOrderLineDelivery.CustomLabelsProviderSubClassImpl);
		}

		#region Validation

		protected override JobOrderLineDeliveryValidation GetNewValidation()
		{
			return new WoolworthsOrderLineDeliveryValidation(this);
		}

		public new WoolworthsOrderLineDeliveryValidation Validation
		{
			get { return (WoolworthsOrderLineDeliveryValidation)base.Validation; }
		}

		#endregion

		#region Properties

		public override ZDecimal J4_Allocated
		{
			get { return base.J4_Allocated; }
			set
			{
				base.J4_Allocated = value;
				if (OrderLine != null)
				{
					OrderLine.JO_QtyInvoiced = OrderLine.JO_Calc_TotalQtyReceived;
				}
			}
		}

		public override ZString J4_RL_NKDestinationPort
		{
			get { return base.J4_RL_NKDestinationPort; }
			set
			{
				if (IsCopying || !HasJobComInvoiceLineLink)
				{
					base.J4_RL_NKDestinationPort = value;
				}
				else if (((WoolworthsOrder)OrderLine.Order).OnInvoiceOrderMismatching())
				{
					base.J4_RL_NKDestinationPort = value;
				}
				else
				{
					J4_RL_NKDestinationPortInfo.RefreshBinding();
				}
			}
		}

		#region J4_QuantityOrdered

		public ZDecimal J4_QuantityOrdered
		{
			get { return J4_CustomDecimal5; }
			set { J4_CustomDecimal5 = value; }
		}

		public override ZDecimal J4_CustomDecimal5 // this is quantity ordered
		{
			get { return base.J4_CustomDecimal5; }
			set
			{
				base.J4_CustomDecimal5 = value;
				if (OrderLine != null)
				{
					OrderLine.Validation.ValidateJO_Quantity();
					OrderLine.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		internal ZDecimal J4_Calc_TotalQuantityInvoiced
		{
			get
			{
				ZDecimal result = 0m;
				foreach (OrderLineDeliverContainer container in Containers)
				{
					result += container.J5_QuantityInvoiced;
				}
				return result;
			}
		}

		protected bool HasJobComInvoiceLineLink
		{
			get
			{
				foreach (WoolworthsJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (invoiceLine.Declaration != null && !invoiceLine.Declaration.JE_IsCancelled && invoiceLine.Declaration.JE_RL_NKFinalDestination.ToUpper() == J4_RL_NKDestinationPort)
					{
						return true;
					}
				}
				return false;
			}
		}

		JobComInvoiceLine[] InvoiceLines
		{
			get
			{
				JobComInvoiceLine[] result = null;

				if (OrderLine != null && OrderLine.Order != null)
				{
					ZQuery filter = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, OrderLine.Order.JD_OrderNumber);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_OrderNumber, SQLComparisonOperator.NotEqual, ZString.Empty);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_PartNo, OrderLine.JO_Partno);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_CustomDecimal1, OrderLine.JO_LineNo);
					result = Factory.Load<BaseJobComInvoiceLine>(filter).Where(line => line is JobComInvoiceLine).Cast<JobComInvoiceLine>().ToArray();
				}

				return result;
			}
		}

		#endregion
	}
}
