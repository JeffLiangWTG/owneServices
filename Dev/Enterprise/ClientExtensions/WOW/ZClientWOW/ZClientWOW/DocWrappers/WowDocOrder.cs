using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.Wow.Business;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.Wow
{
	class WowDocOrder : DocOrder
	{
		protected WowDocOrder(Order order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		public new static WowDocOrder New(Order order, BusinessObjectFactory factoryToWrap)
		{
			return (order != null) ? new WowDocOrder(order, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocOrder OverriddenNewMethod(Order order, BusinessObjectFactory factoryToWrap)
		{
			return WowDocOrder.New(order, factoryToWrap);
		}

		public ZString Amendment
		{
			get
			{
				return ((ParentBusinessObject as WoolworthsImportedOrder) != null && ((WoolworthsImportedOrder)ParentBusinessObject).IsUpdated) ?
			  new ZString("*** Amendment ***") : ZString.Empty;
			}
		}

		public ZString BuyerBranchName
		{
			get
			{
				return (Consignee.Branch != null && !Consignee.Branch.BranchName.IsEmpty) ?
					Consignee.Branch.BranchName : ZString.Empty;
			}
		}

		public ZString DestinationPortName
		{
			get
			{
				return (FirstDeliveryOnOrderLine != null && FirstDeliveryOnOrderLine.DestinationPort != null) ?
					FirstDeliveryOnOrderLine.DestinationPort.PortName.ToUpper() : ZString.Empty;
			}
		}

		public ZString DestinationAddress
		{
			get
			{
				return (FirstDeliveryOnOrderLine != null && FirstDeliveryOnOrderLine.DeliverPoint != null) ?
					FirstDeliveryOnOrderLine.DeliverPoint.PostalAddress : ZString.Empty;
			}
		}

		DocOrderLineDelivery FirstDeliveryOnOrderLine
		{
			get { return (OrderLines.Count > 0 && OrderLines[0].Deliveries.Count > 0) ? OrderLines[0].Deliveries[0] : null; }
		}

		public ZDateTime EsitmatedDlvDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				foreach (DocOrderLine current in OrderLines)
				{
					if (!current.CustomDate1.IsEmpty)
					{
						result = current.CustomDate1;
						break;
					}
				}
				return result;
			}
		}

		public ZDateTime EsitmatedArvDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				foreach (DocOrderLine current in OrderLines)
				{
					if (!current.CustomDate2.IsEmpty)
					{
						result = current.CustomDate2;
						break;
					}
				}
				return result;
			}
		}

		public ZString ReplenisherNotes
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();

				ZString email = GetNoteContent(CASSKIRKOrderIntegration.CASSKIRKConstant.ReplenishersEmailNoteType);
				ZString faxNo = GetNoteContent(CASSKIRKOrderIntegration.CASSKIRKConstant.ReplenishersFaxNoteType);

				if (!email.IsEmpty)
				{
					builder.Append("Email to ");
					builder.Append(email);
				}

				if (!faxNo.IsEmpty)
				{
					ZString faxNoStart = (!builder.IsEmpty) ? "  or Fax to " : "Fax to ";

					builder.Append(faxNoStart);
					builder.Append(faxNo);
				}

				if (!builder.IsEmpty)
				{
					builder.Prepend("(");
					builder.Append(")");
				}

				return builder.ToString();
			}
		}

		ZString GetNoteContent(ZString noteType)
		{
			ZString result = ZString.Empty;
			StmNote[] notes = Order.Notes.FindByDescription(noteType);
			if (notes.Length > 0)
			{
				result = notes[0].ST_NoteDataAsText;
			}
			return result;
		}

		public WowDocImportedOrderLineCollection ImportedOrderLines
		{
			get
			{
				WowDocImportedOrderLineCollection result = new WowDocImportedOrderLineCollection(Factory);
				WoolworthsImportedOrder importedOrder = Order as WoolworthsImportedOrder;

				foreach (OrderLine line in Order.OrderLines)
				{
					bool updateState = (importedOrder != null &&
					   importedOrder.OrderLineStates != null &&
					   importedOrder.OrderLineStates.ContainsKey(line.PK)) && importedOrder.OrderLineStates[line.PK];

					result.Add(WowDocImportOrderLine.New(line, Factory, updateState));
				}

				return result;
			}
		}

		public ZBool IsValidBranch
		{
			get
			{
				return WowDataRegistry.Instance.CASSKIRKDisplayOption;
			}
		}
	}
}
