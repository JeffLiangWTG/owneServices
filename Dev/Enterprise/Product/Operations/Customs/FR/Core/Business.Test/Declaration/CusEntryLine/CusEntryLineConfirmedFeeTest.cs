using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineConfirmedFee))]
	sealed class CusEntryLineConfirmedFeeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var calculatedFee = new CusEntryLineConfirmedFee("Customs Value", 10, "EUR");
			CombineAssertions(() =>
			{
				AssertEquals("Customs Value", calculatedFee.Description);
				AssertEquals(10m, calculatedFee.Amount);
				AssertEquals("EUR", calculatedFee.Currency);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusEntryLineConfirmedFee(ZString.Empty, ZDecimal.Zero, ZString.Empty);
		}
	}
}
