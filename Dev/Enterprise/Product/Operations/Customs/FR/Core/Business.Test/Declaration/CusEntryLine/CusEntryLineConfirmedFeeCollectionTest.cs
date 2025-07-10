using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineConfirmedFeeCollection))]
	sealed class CusEntryLineConfirmedFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusEntryLineConfirmedFeeCollection>
	{
		public void TestConstructor()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_ConfirmedCustomsValue = 1;
			cusEntryLine.CL_ConfirmedStatisticalValue = 2;
			cusEntryLine.CL_ConfirmedValueForVAT = 3;
			cusEntryLine.CL_AddInfo = "ConfirmedCIFValue=4";

			var collection = new CusEntryLineConfirmedFeeCollection(cusEntryLine);
			var description = collection.Select(x => x.Description).ToArray();
			var amount = collection.Select(x => x.Amount).ToArray();
			var currency = collection.Select(x => x.Currency).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(4, collection.Count);
				AssertArrayEqualsByElements(new ZString[] { "Customs Value", "Stat. Value", "VAT Value", "CIF Value" }, description);
				AssertArrayEqualsByElements(new ZDecimal[] { 1, 2, 3, 4 }, amount);
				AssertArrayEqualsByElements(new ZString[] { "EUR", "EUR", "EUR", "EUR" }, currency);
			});
		}

		protected override CusEntryLineConfirmedFeeCollection GetCollectionToTest() => new CusEntryLineConfirmedFeeCollection(null);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CusEntryLineConfirmedFee(ZString.Empty, ZDecimal.Zero, ZString.Empty);
	}
}
