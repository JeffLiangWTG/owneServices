using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentApprovalItemCollection))]
	public class PaymentApprovalItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PaymentApprovalItemCollection(Factory);
		}
	}
}
