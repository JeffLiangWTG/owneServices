using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(RefundDutyFeeCollection))]
	public class RefundDutyFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new RefundDutyFeeCollection(entryLine);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusEntryLineFee>();

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		}
		CusEntryLine entryLine;

		public void TestCreateAdditionalFilter()
		{
			var collection = new RefundDutyFeeCollection(entryLine);
			var newFee = Factory.New<CusEntryLineFee>();
			newFee.CF_CL = entryLine.PK;
			newFee.CF_MethodOfCalculation = "NON";
			collection.Load();
			AssertEquals("CF_MethodOfCalculation invalid, collection should not contain.", false, collection.Contains(newFee));

			var confirmedReleaseFee = Factory.New<CusEntryLineFee>();
			confirmedReleaseFee.CF_CL = entryLine.PK;
			confirmedReleaseFee.CF_MethodOfCalculation = CusEntryLineFeeRefundDutyMethodOfCalculation.ConfirmedRelease;
			collection.Load();
			AssertEquals("CF_MethodOfCalculation ConfirmedRelease, collection should contain.", true, collection.Contains(confirmedReleaseFee));
		}
	}
}
