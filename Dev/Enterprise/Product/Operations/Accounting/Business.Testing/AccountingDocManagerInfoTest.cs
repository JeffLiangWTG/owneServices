using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccountingDocManagerInfo))]
	public class AccountingDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<ARReceipt>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<ARReceipt>();
		}
	}
}
