using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(InvoiceHasChargeCode))]
	sealed class InvoiceHasChargeCodeTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructors

		public void TestConstructors()
		{
			var testObject = new InvoiceHasChargeCode("FRT", ZBool.True);
			AssertEquals("FRT", testObject.ChargeCode);
			AssertEquals(true, testObject.Exists);

			testObject = new InvoiceHasChargeCode("BAF", ZBool.False);
			AssertEquals("BAF", testObject.ChargeCode);
			AssertEquals(false, testObject.Exists);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceHasChargeCode();
		}

		#endregion
	}
}
