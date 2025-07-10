using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(HotChequeLink))]
	public class HotChequeLinkTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			AccHotChequeCollection hotCheques = new AccHotChequeCollection(Factory);
			return new HotChequeLink(hotCheques);
		}
	}
}
