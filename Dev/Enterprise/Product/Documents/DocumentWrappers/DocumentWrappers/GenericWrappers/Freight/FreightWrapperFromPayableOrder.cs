using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PayableOrder;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPayableOrder : FreightWrapper
	{
		public FreightWrapperFromPayableOrder(AccPayableOrderHeader aPOrderBO, BusinessObjectFactory factory)
			: base(aPOrderBO, factory)
		{
			this.APOrderBO = aPOrderBO;
		}
		readonly AccPayableOrderHeader APOrderBO;

		protected override AccPayableOrderHeader GetPayableOrder()
		{
			return APOrderBO;
		}

		protected override ZDateTime GetOrderDate()
		{
			return PayableOrder.APH_SystemCreateTimeUtc;
		}

		protected override ZString GetJobNumber()
		{
			return PayableOrder.APH_OrderNumber;
		}

		protected override ZString GetSecondaryHeading()
		{
			return Res.GetString("6abba6ba-3b6a-4bbc-b253-513f6c9aa5b4", "Split");
		}

		protected override ZString GetSecondaryNumber()
		{
			return PayableOrder.APH_OrderNumberSplit.ToString();
		}

		protected override ZString GetGoodsDescription()
		{
			return APOrderBO.APH_GoodsDescription;
		}

		protected override ZString GetShippersReference()
		{
			return PayableOrder.APH_BookingConfRef;
		}

		protected override OrganisationWrapper GetBuyer()
		{
			return new OrganisationWrapper(OrganisationUsageType.Buyer, PayableOrder.Buyer, PayableOrder.BuyerContact, Factory);
		}

		protected override OrganisationWrapper GetSupplier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Supplier, PayableOrder.SupplierDocumentaryAddress, Factory);
		}
	}
}
