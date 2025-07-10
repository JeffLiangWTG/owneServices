using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public class DirectPaymentCollection : ActiveBusinessObjectCollection<DirectPayment>
	{
		public DirectPaymentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.DirectPayment);
			result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.CashBook);

			return result;
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return findBoxListProvider ?? (findBoxListProvider = new DirectPaymentFindBoxListProvider(this)); }
		}

		DirectPaymentFindBoxListProvider findBoxListProvider;

		#region DirectPaymentFindBoxListProvider

		public class DirectPaymentFindBoxListProvider : FindBoxListProvider
		{
			public DirectPaymentFindBoxListProvider(DirectPaymentCollection collection)
				: base(collection)
			{
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				return BizObjsFromCodeWithCompleteFilter(code);
			}
		}

		#endregion
	}
}