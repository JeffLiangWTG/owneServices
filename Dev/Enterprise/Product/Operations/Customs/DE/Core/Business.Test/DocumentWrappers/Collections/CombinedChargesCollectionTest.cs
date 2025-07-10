using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(CombinedChargesCollection))]
	sealed class CombinedChargesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CombinedChargesCollection>
	{
		public void TestSumAmount()
		{
			var collection = GetCollectionToTest();
			var aCharge = collection.Cast<CombinedCharges>().Single(c => c.Type == "A");
			var bCharge = collection.Cast<CombinedCharges>().Single(c => c.Type == "B");

			AssertEquals("3,00", aCharge.Amount);
			AssertEquals("1,00", bCharge.Amount);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => CombinedCharges.New(new[] { Factory.NewWithValidTestData<InvoiceLineCharge>() }, Factory);

		protected override CombinedChargesCollection GetCollectionToTest() => new CombinedChargesCollection(GetInvoiceLineCharges(), Factory);

		IEnumerable<InvoiceLineCharge> GetInvoiceLineCharges()
		{
			var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var charge = line.Charges.AddNew();
			charge.J7_ChargeType = "A";
			charge.J7_Amount = 1;

			var charge2 = line.Charges.AddNew();
			charge2.J7_ChargeType = "A";
			charge2.J7_Amount = 2;

			var charge3 = line.Charges.AddNew();
			charge3.J7_ChargeType = "B";
			charge3.J7_Amount = 1;

			return line.Charges;
		}
	}
}
