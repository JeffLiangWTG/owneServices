using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business
{
	public class CusGuaranteeLineTransactionCollection : Customs.Business.CusGuaranteeLineTransactionCollection
	{
		public CusGuaranteeLineTransactionCollection(CusGuaranteeHeader master) : base(master)
		{
		}

		public CusGuaranteeLineTransactionCollection(CusGuaranteeHeader master, ZQuery query) : base(master, query)
		{
		}

		public new CusGuaranteeHeader GuaranteeHeader => (CusGuaranteeHeader)base.GuaranteeHeader;

		public new CusGuaranteeLineTransaction this[int index]
		{
			get { return (CusGuaranteeLineTransaction)base[index]; }
		}

		public new CusGuaranteeLineTransaction AddNew()
		{
			return (CusGuaranteeLineTransaction)base.AddNew();
		}
	}
}
