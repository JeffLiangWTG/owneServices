
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Matching
{
	public abstract class DataLine
	{
		public ZString OtherRefNumber
		{
			get { return FullInvoiceNumber.SubstringSafe(0, 6); }
		}

		ZString fCurrentSubsidiary;
		public ZString CurrentSubsidiary
		{
			get
			{
				if (fCurrentSubsidiary.IsEmpty)
				{
					fCurrentSubsidiary = (GlbCompany.CurrentCompany.OrgProxy != null)
						? GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetUNC()
						: ZString.Empty;
				}
				return fCurrentSubsidiary;
			}
		}

		BusinessObjectFactory fFactory;
		internal protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		public abstract ZString CounterpartSubsidiary { get; }
		public abstract ZDateTime InvoiceDate { get; }
		public abstract ZDateTime MaturityDate { get; }
		public abstract ZString HouseBill { get; }
		public abstract ZString MasterBill { get; }
		public abstract ZString FullInvoiceNumber { get; }
		public abstract ZString Category { get; }
		public abstract ZDecimal Amount { get; }
		public abstract ZString CurrencyCode { get; }
		public abstract ZBool CreditNote { get; }
		public abstract ZString TransactionType { get; }

#region Implementation
#region DummyDataLine class
#endregion
#endregion
			}
}
