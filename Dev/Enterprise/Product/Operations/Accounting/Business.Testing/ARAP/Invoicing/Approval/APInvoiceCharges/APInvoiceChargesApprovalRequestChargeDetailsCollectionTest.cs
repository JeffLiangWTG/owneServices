using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesApprovalRequestChargeDetailsCollection))]
	public class APInvoiceChargesApprovalRequestChargeDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<APInvoiceChargesApprovalRequestChargeDetailsCollection>
	{
		protected override APInvoiceChargesApprovalRequestChargeDetailsCollection GetCollectionToTest()
		{
			return new APInvoiceChargesApprovalRequestChargeDetailsCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new APInvoiceChargesApprovalRequestChargeDetails(Factory);
		}
	}
}
