using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class BuyerAndOrderNumAndSplitIsUniqueValidation : ValidationProvider
	{
		public BuyerAndOrderNumAndSplitIsUniqueValidation(AccPayableOrderHeader order)
			: base(order)
		{
			this.Order = order;
		}

		public void CheckBuyerAndOrderNumAndSplitIsUnique(ZPropertyInfo propertyToSetErrorOn)
		{
			if (Order.APH_OA_Buyer.IsValid && !Order.APH_OrderNumber.IsEmpty)
			{
				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(AccPayableOrderHeaderSchema.PK, SQLComparisonOperator.NotEqual, Order.PK);
				sQLFilter.AddToFilter(AccPayableOrderHeaderSchema.APH_OA_Buyer, Order.APH_OA_Buyer);
				sQLFilter.AddToFilter(AccPayableOrderHeaderSchema.APH_OrderNumber, Order.APH_OrderNumber);
				sQLFilter.AddToFilter(AccPayableOrderHeaderSchema.APH_OrderNumberSplit, Order.APH_OrderNumberSplit);
				sQLFilter.IgnoreActiveFilter = true;

				BusinessObjectFactory factory = Order.Factory;
				if (factory.ExistsInDatabase(AccPayableOrderHeaderSchema.Constants.TableName, sQLFilter))
				{
					string error = Res.GetString("d00eacdc-a51a-46ce-8d5a-938964340442", "An payable order already exists with the same {0}, {1} and {2}.",
						Order.APH_OA_BuyerInfo.Description,
						Order.APH_OrderNumberInfo.Description,
						Order.APH_OrderNumberSplitInfo.Description);

					propertyToSetErrorOn.AddError(error);
				}
			}
		}

		#region Implementation

		protected AccPayableOrderHeader Order;

		#endregion
	}
}
