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
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLine : OrderLine
	{
		public new class Schema : OrderLine.Schema
		{
			public const string JO_PartDepartment = "JO_PartDepartment";
			public const string JO_PartDivision = "JO_PartDivision";
			public const string JO_ProductFirstUsedHere = "JO_ProductFirstUsedHere";
			public const string JO_ProductFirstUsedHereDbColumn = JO_CustomFlag4;
		}

		#region ICustomLabelsProvider

		protected class CustomLabelsProviderSubClassImpl : OrderLine.CustomLabelsProvider
		{
			public CustomLabelsProviderSubClassImpl(ICustomLabelsConfigOrgProvider orgProvider) : base(orgProvider)
			{
			}

			public override CustomLabelInfoList GetCustomFields(OrgHeader org, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = base.GetCustomFields(org, factory);
				result.Add(Enterprise.Core.Constants.CustomLabels.Parts.Department, Schema.JO_PartDepartment, typeof(ZString), (NoResString)"Product Department", (NoResString)"", CustomLabelStyles.None);
				result.Add(Enterprise.Core.Constants.CustomLabels.Parts.Division, Schema.JO_PartDivision, typeof(ZString), (NoResString)"Product Division", (NoResString)"", CustomLabelStyles.None);
				return result;
			}
		}

		#endregion

		public WoolworthsOrderLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Validation

		protected override JobOrderLineValidation GetNewValidation()
		{
			return new WoolworthsOrderLineValidation(this);
		}

		public new WoolworthsOrderLineValidation Validation
		{
			get { return (WoolworthsOrderLineValidation)base.Validation; }
		}

		#endregion

		public static void RegisterThisSubTypeOverride()
		{
			OrderLine.CustomLabelsProviderType = typeof(WoolworthsOrderLine.CustomLabelsProviderSubClassImpl);
		}

		public override ZDecimal JO_OuterPacks
		{
			get { return base.JO_OuterPacks; }
			set
			{
				base.JO_OuterPacks = value;
				foreach (WoolworthsOrderLineDelivery delivery in Deliveries)
				{
					foreach (WoolworthsOrderLineDeliverContainer container in delivery.Containers)
					{
						container.CalculateQuantityInvoiced();
					}
				}
			}
		}

		#region Property Overrides

		public override ZString JO_Partno
		{
			get { return base.JO_Partno; }
			set
			{
				if (IsCopying || !HasJobComInvoiceLineLink)
				{
					SetPartnoImpl(value);
				}
				else if (((WoolworthsOrder)Order).OnInvoiceOrderMismatching())
				{
					SetPartnoImpl(value);
				}
				else
				{
					JO_PartnoInfo.RefreshBinding();
				}
			}
		}

		void SetPartnoImpl(ZString partno)
		{
			base.JO_Partno = partno;
			JO_PartDepartmentInfo.RefreshBinding();
			JO_PartDivisionInfo.RefreshBinding();

			UpdateProductFirstUsedHere();
		}

		#endregion

		#region New Bound Properties

		#region JO_PartDepartment

		public ZString JO_PartDepartment
		{
			get { return Product != null ? Product.OP_Department : ZString.Empty; }
		}

		public ZPropertyInfo JO_PartDepartmentInfo
		{
			get { return GetZPropertyInfo(Schema.JO_PartDepartment); }
		}

		#endregion

		#region JO_PartDivision

		public ZString JO_PartDivision
		{
			get { return Product != null ? Product.OP_Division : ZString.Empty; }
		}

		public ZPropertyInfo JO_PartDivisionInfo
		{
			get { return GetZPropertyInfo(Schema.JO_PartDivision); }
		}

		#endregion

		#region JO_ProductFirstUsedHere

		public ZBool JO_ProductFirstUsedHere
		{
			get { return (ZBool)this[Schema.JO_ProductFirstUsedHereDbColumn]; }
			set
			{
				this[Schema.JO_ProductFirstUsedHereDbColumn] = value;
				ZPropertyInfoHash[Schema.JO_ProductFirstUsedHereDbColumn].RefreshBinding();
				JO_ProductFirstUsedHereInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JO_ProductFirstUsedHereInfo
		{
			get { return GetZPropertyInfo(Schema.JO_ProductFirstUsedHere); }
		}

		#endregion

		#region JO_Calc_TotalQuantityOrdered

		public ZDecimal JO_Calc_TotalQuantityOrdered
		{
			get
			{
				ZDecimal result = 0;
				foreach (WoolworthsOrderLineDelivery delivery in Deliveries)
				{
					result += delivery.J4_QuantityOrdered;
				}
				return result;
			}
		}

		public ZPropertyInfo JO_Calc_TotalQuantityOrderedInfo
		{
			get { return GetZPropertyInfo(nameof(JO_Calc_TotalQuantityOrdered)); }
		}

		#endregion

		#endregion

		#region Implementation

		protected sealed override void UpdateOrderLineStatus()
		{
			base.UpdateOrderLineStatus();
			UpdateOrderLineStatusCore();
		}

		protected virtual void UpdateOrderLineStatusCore()
		{
			if (JO_LineStatus == Constants.OrderStatus.Open)
			{
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					if (delivery.Containers.Count > 0)
					{
						JO_LineStatus = WowConstants.OrderStatuses.ContainersAttached;
						break;
					}
				}
			}
		}

		void UpdateProductFirstUsedHere()
		{
			ZQuery query = new ZQuery(JobOrderLineSchema.JO_Partno, JO_Partno);
			OrderLine[] lines = Factory.Load<WoolworthsOrderLine>(query);
			JO_ProductFirstUsedHere = (Product != null) && (lines.Length == 1);
		}

		protected bool HasJobComInvoiceLineLink
		{
			get
			{
				if (Order != null)
				{
					ZQuery filter = new ZQuery(JobComInvoiceLineSchema.JI_OrderNumber, Order.JD_OrderNumber);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_OrderNumber, SQLComparisonOperator.NotEqual, ZString.Empty);

					filter.AddToFilter(JobComInvoiceLineSchema.JI_PartNo, SQLComparisonOperator.Equal, JO_Partno);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_CustomDecimal1, JO_LineNo);
					var lines = Factory.Load<BaseJobComInvoiceLine>(filter).Where(line => line is JobComInvoiceLine);

					foreach (var line in lines)
					{
						if (line.Declaration?.JE_IsCancelled == false)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#endregion
	}
}
