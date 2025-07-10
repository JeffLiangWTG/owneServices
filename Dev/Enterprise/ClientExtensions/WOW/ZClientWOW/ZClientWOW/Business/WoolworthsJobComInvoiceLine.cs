using System.Collections.Generic;
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
	public class WoolworthsJobComInvoiceLine : JobComInvoiceLine
	{
		public WoolworthsJobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SyncInvoiceQuantitiesToOrderContainer()
		{
			bool isContainerDelivered = (LinkedCusContainer != null &&
				LinkedCusContainer.JobContainer != null &&
				LinkedCusContainer.JobContainer.JC_ArrivalCartageComplete != ZDateTime.Empty);
			if (isContainerDelivered && OrderLineDeliverContainer != null)
			{
				OrderLineDeliverContainer.J5_QuantityInStore = GetTotalInvoiceQuantityForLinkedOrderContainer();
			}
		}

		public ZDecimal GetTotalInvoiceQuantityForLinkedOrderContainer()
		{
			ZQuery invoiceFilter = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, JI_OrderNumber);
			invoiceFilter.AddToFilter(JobComInvoiceLineSchema.JI_PartNo, JI_PartNo);
			invoiceFilter.AddToFilter(JobComInvoiceLineSchema.JI_CustomDecimal1, JI_CustomDecimal1);

			var matchingInvoiceLines = Factory.Load<BaseJobComInvoiceLine>(invoiceFilter).Where(line => line is JobComInvoiceLine);

			ZDecimal newQuantityDelivered = 0;

			foreach (WoolworthsJobComInvoiceLine matchingInvoiceLine in matchingInvoiceLines)
			{
				if (matchingInvoiceLine.Declaration != null &&
					!matchingInvoiceLine.Declaration.JE_IsCancelled &&
					matchingInvoiceLine.ContainerNumber.ToLower() == ContainerNumber.ToLower() &&
					Declaration != null &&
					matchingInvoiceLine.Declaration.JE_RL_NKFinalDestination.Trim().ToUpper() == Declaration.JE_RL_NKFinalDestination.Trim().ToUpper())
				{
					newQuantityDelivered += matchingInvoiceLine.JI_InvoiceQuantity;
				}
			}

			return newQuantityDelivered;
		}

		#region Business Object Overrides

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (RequiresSyncInvoiceQuantitiesToOrderContainer())
			{
				SyncInvoiceQuantitiesToOrderContainer();
			}
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			var result = base.GetNewValidation();
			result.Add(new WoolworthsJobComInvoiceLineValidation(this));
			return result;
		}

		#endregion

		#region Property Overrides

		public override ZString JI_OrderNumber
		{
			get { return base.JI_OrderNumber; }
			set
			{
				if (IsCopying || Declaration == null || OrderLineDeliveries.Count == 0)
				{
					base.JI_OrderNumber = value;
				}
				else if (Declaration != null && Declaration.OnOrderInvoiceMismatching())
				{
					base.JI_OrderNumber = value;
				}
				else
				{
					JI_OrderNumberInfo.RefreshBinding();
				}
			}
		}

		public override ZString JI_PartNo
		{
			get { return base.JI_PartNo; }
			set
			{
				if (IsCopying || Declaration == null || OrderLineDeliveries.Count == 0)
				{
					base.JI_PartNo = value;
				}
				else if (Declaration != null && Declaration.OnOrderInvoiceMismatching())
				{
					base.JI_PartNo = value;
				}
				else
				{
					JI_PartNoInfo.RefreshBinding();
				}
			}
		}

		public override ZDecimal JI_CustomDecimal1
		{
			get { return base.JI_CustomDecimal1; }
			set
			{
				if (IsCopying || Declaration == null || OrderLineDeliveries.Count == 0)
				{
					base.JI_CustomDecimal1 = value;
				}
				else if (Declaration != null && Declaration.OnOrderInvoiceMismatching())
				{
					base.JI_CustomDecimal1 = value;
				}
				else
				{
					JI_CustomDecimal1Info.RefreshBinding();
				}
			}
		}

		#endregion

		#region New Properties

		public ZString ContainerNumber
		{
			get
			{
				ZString result = JI_CustomAttrib4;

				if (result.IsEmpty && Declaration?.CusContainers.Count == 1)
				{
					result = Declaration.CusContainers[0].CO_ContainerNumber;
				}

				return result;
			}
		}

		CusContainer LinkedCusContainer
		{
			get
			{
				CusContainer result = null;

				if (Declaration != null)
				{
					result = Declaration.CusContainers.Find(ContainerNumber);
				}

				return result;
			}
		}

		#endregion

		#region Related Business Objects

		public new WoolworthsJobDeclaration Declaration
		{
			get { return (WoolworthsJobDeclaration)base.Declaration; }
		}

		public IReadOnlyList<OrderLineDelivery> OrderLineDeliveries
		{
			get
			{
				ZQuery orderFilter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, JI_OrderNumber);
				orderFilter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumberSplit, (byte)0);
				Order[] orders = Factory.Load<Order>(orderFilter);

				var result = new List<OrderLineDelivery>();

				foreach (Order order in orders)
				{
					foreach (OrderLine orderLine in order.OrderLines)
					{
						if (JI_PartNo.ToUpper() == orderLine.JO_Partno.ToUpper() && JI_CustomDecimal1 == orderLine.JO_LineNo)
						{
							foreach (OrderLineDelivery delivery in orderLine.Deliveries)
							{
								if (!delivery.J4_RL_NKDestinationPort.IsEmpty &&
									Declaration != null &&
									delivery.J4_RL_NKDestinationPort.ToUpper() == Declaration.JE_RL_NKFinalDestination.ToUpper()
									)
								{
									result.Add(delivery);
								}
							}
						}
					}
				}
				return result;
			}
		}

		public WoolworthsOrderLineDeliverContainer OrderLineDeliverContainer
		{
			get
			{
				WoolworthsOrderLineDeliverContainer result = null;
				foreach (WoolworthsOrderLineDeliverContainer current in OrderLineDeliverContainers)
				{
					if (current.J5_ContainerNum.ToUpper() == ContainerNumber.ToUpper())
					{
						result = current;
						break;
					}
				}
				return result;
			}
		}

		OrderLineDeliverContainer[] OrderLineDeliverContainers
		{
			get
			{
				List<OrderLineDeliverContainer> result = new List<OrderLineDeliverContainer>();

				if (Order != null)
				{
					foreach (OrderLineDelivery delivery in OrderLineDeliveries)
					{
						foreach (OrderLineDeliverContainer deliverContainer in delivery.Containers)
						{
							foreach (CusContainer cusContainer in Declaration?.CusContainers)
							{
								if (deliverContainer.J5_ContainerNum.ToUpper() == cusContainer.CO_ContainerNumber.ToUpper())
								{
									result.Add(deliverContainer);
								}
							}
						}
					}
				}
				return result.ToArray();
			}
		}

		#endregion

		#region Implementation

		bool RequiresSyncInvoiceQuantitiesToOrderContainer()
		{
			bool result = false;
			if (Declaration != null && !Declaration.JE_IsCancelled)
			{
				result |= !IsInDatabase;
				result |= WasContainerOnDeclarationJustDelivered;
				result |= (ZDecimal)JI_InvoiceQuantityInfo.OriginalValue != (ZDecimal)JI_InvoiceQuantityInfo.Value;
				result |= (ZString)JI_PartNoInfo.OriginalValue != (ZString)JI_PartNoInfo.Value;
				result |= (ZString)JI_OrderNumberInfo.OriginalValue != (ZString)JI_OrderNumberInfo.Value;
				result |= JI_CustomDecimal1Info.OriginalValue != JI_CustomDecimal1Info.Value;
				result |= !IsCusContainerInDb;
				result |= HasContainerNumberChanged;
				result |= HasDeclarationFinalDestinationChanged;
			}
			return result;
		}

		bool IsCusContainerInDb
		{
			get { return Declaration != null && LinkedCusContainer != null && LinkedCusContainer.IsInDatabase; }
		}

		bool HasContainerNumberChanged
		{
			get { return Declaration != null && LinkedCusContainer != null && (ZString)LinkedCusContainer.CO_ContainerNumberInfo.OriginalValue != (ZString)LinkedCusContainer.CO_ContainerNumberInfo.Value; }
		}

		bool HasDeclarationFinalDestinationChanged
		{
			get { return Declaration != null && (ZString)Declaration.JE_RL_NKFinalDestinationInfo.OriginalValue != (ZString)Declaration.JE_RL_NKFinalDestinationInfo.Value; }
		}

		bool WasContainerOnDeclarationJustDelivered
		{
			get
			{
				if (LinkedCusContainer != null &&
					LinkedCusContainer.JobContainer != null &&
					LinkedCusContainer.JobContainer.JC_ArrivalCartageCompleteInfo.HasChanges)
				{
					return true;
				}
				return false;
			}
		}

		#endregion
	}
}
