using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.eNett_Integration
{
	public class StorageFeeInvoicePaymentLookups : ZLookups
	{
		public StorageFeeInvoicePaymentLookups(StorageFeeInvoicePayment parent)
			: base(parent) { }

		public RefUNLOCOCollection Ports
		{
			get { return FindboxLookupCollections.GetUNLOCOCollection(Factory); }
		}

		public CodeDescriptionPairList PaymentTypes
		{
			get { return Parent.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentMethod); }
		}

		public CodeDescriptionPairList ApportionmentMethods
		{
			get
			{
				var list = new CodeDescriptionPairList(OLookUpEditType.AllocationMethod);
				list.RemoveCode(AllocationMethod.FreeSpaceContribution);
				list.RemoveCode(AllocationMethod.OuterPackTotal);

				return list;
			}
		}

		public AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		#region Implementation

		protected new StorageFeeInvoicePayment Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (StorageFeeInvoicePayment)base.Parent; }
		}

		#endregion
	}
}
