using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DutyFeeInformationDocWrapper))]
	class DutyFeeInformationDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestType()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeType = "VAT";
			var wrapper = new DutyFeeInformationDocWrapper(fee);
			AssertEquals("VAT", wrapper.Type);
		}

		public void TestType_NullEntryLineFee()
		{
			var wrapper = new DutyFeeInformationDocWrapper(null);
			AssertEquals(ZString.Empty, wrapper.Type);
		}

		public void TestAmount()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeAmount = 123.123m;
			var wrapper = new DutyFeeInformationDocWrapper(fee);
			AssertEquals(123.12m, wrapper.Amount);
		}

		public void TestAmount_NullEntryLineFee()
		{
			var wrapper = new DutyFeeInformationDocWrapper(null);
			AssertEquals(ZDecimal.Zero, wrapper.Amount);
		}

		protected override BusinessObject GetNewBusinessObject() => new DutyFeeInformationDocWrapper(null);
	}
}
