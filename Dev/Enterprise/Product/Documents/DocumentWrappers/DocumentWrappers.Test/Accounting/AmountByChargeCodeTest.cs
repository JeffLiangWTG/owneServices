using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(AmountByChargeCode))]
	sealed class AmountByChargeCodeTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructors

		public void TestConstructors()
		{
			var testObject = new AmountByChargeCode("FRT", 10m, 9m);
			AssertEquals("FRT", testObject.ChargeCode);
			AssertEquals(10m, testObject.TotalAmount);
			AssertEquals(9m, testObject.TotalAmountExTax);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AmountByChargeCode();
		}

		#endregion
	}
}
