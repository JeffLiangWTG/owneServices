using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business
{
	[DependentBusinessObject(typeof(CusGuaranteeHeader), "CusGuaranteeLineTransactions")]
	public class CusGuaranteeLineTransaction : BaseCusGuaranteeLineTransaction
	{
		public CusGuaranteeLineTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGuaranteeHeader GuaranteeHeader => (CusGuaranteeHeader)base.GuaranteeHeader;
	}
}
