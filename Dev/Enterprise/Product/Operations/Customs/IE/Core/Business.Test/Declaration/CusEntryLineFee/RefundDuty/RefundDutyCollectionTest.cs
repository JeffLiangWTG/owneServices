using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(RefundDutyCollection))]
	public class RefundDutyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RefundDutyCollection>
	{
		protected override RefundDutyCollection GetCollectionToTest() => entryLine.RefundDuties;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RefundDuty(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat,  entryLine.RefundDuties.RefundDutyFees);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		}
		CusEntryLine entryLine;

		public override void TestAddNew()
		{
			var newRefund = Collection.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
			AssertEquals(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, newRefund.TaxType);
			Assert(Collection.Contains(newRefund));
		}
	}
}
