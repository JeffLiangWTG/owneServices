using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocREFACCInfoProvider : DocD99BCUSRESInfoProvider
	{
		DocREFACCInfoProvider(REFACCInfoProvider rEFACCInfoProvider, BusinessObjectFactory factory)
			: base(rEFACCInfoProvider, factory)
		{
		}

		public static DocREFACCInfoProvider New(REFACCInfoProvider rEFACCInfoProvider, BusinessObjectFactory factory)
		{
			return (rEFACCInfoProvider == null) ? null : new DocREFACCInfoProvider(rEFACCInfoProvider, factory);
		}

		REFACCInfoProvider REFACCInfoProvider
		{
			get { return (REFACCInfoProvider)WrappedObject; }
		}

		#region ZString Fields
		public ZString EFTRunNumber
		{
			get { return REFACCInfoProvider.EFTRunNumber; }
		}

		public ZString ClaimNumber
		{
			get { return REFACCInfoProvider.ClaimNumber; }
		}

		public ZString ICSReceiptNumber
		{
			get { return REFACCInfoProvider.ICSReceiptNumber; }
		}

		public ZString BankAccountName
		{
			get { return REFACCInfoProvider.BankAccountName; }
		}

		public ZString BankAccountNumber
		{
			get { return REFACCInfoProvider.BankAccountNumber; }
		}

		public ZString BSBNumber
		{
			get { return REFACCInfoProvider.BSBNumber; }
		}
		#endregion

		public override DocCMRMessageChargeItemCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new DocCMRMessageChargeItemCollection(Factory);
					fCharges.Add(new DocCMRMessageChargeItem("Total Refundable Amount", TotalPayable));
				}
				return fCharges;
			}
		}

		protected DocCMRMessageChargeItemCollection fCharges;

		public override ZDecimal TotalPayable
		{
			get { return REFACCInfoProvider.TotalAmountToBeRefunded; }
		}
	}
}
