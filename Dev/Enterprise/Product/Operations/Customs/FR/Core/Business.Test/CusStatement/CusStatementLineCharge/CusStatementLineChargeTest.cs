using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementLineCharge))]
	class CusStatementLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeDetail()
		{
			var lineCharge = Factory.New<CusStatementLineCharge>();
			AssertNull(lineCharge.ChargesDetail);

			var chargesDetail = Factory.New<CusStatementChargesDetail>();
			lineCharge.B4_B3 = chargesDetail.PK;
			AssertSame(chargesDetail, lineCharge.ChargesDetail);
		}

		public void TestB4_ChargeType()
		{
			AssertEquals("Tax Code", DataBoundResourceStrings.GetDataForProperty(charge.B4_ChargeTypeInfo).Caption);
		}

		public void TestB4_MethodOfPayment()
		{
			AssertEquals("Method of Payment", DataBoundResourceStrings.GetDataForProperty(charge.B4_MethodOfPaymentInfo).Caption);
		}

		public void TestB4_ChargeGroup()
		{
			AssertEquals("Tax Code (EU)", DataBoundResourceStrings.GetDataForProperty(charge.B4_ChargeGroupInfo).Caption);
		}

		public void TestB4_ChargeAmount()
		{
			AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(charge.B4_ChargeAmountInfo).Caption);
		}

		public void TestLookups()
		{
			AssertType<CusStatementLineChargeLookups>(Factory.New<CusStatementLineCharge>().Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => charge;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => charge;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => charge;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = StatementPeriodicityList.Codes.Day;
			charge = header.ChargesDetail.Charges.AddNew();
			header.ChargesDetail.B3_BrokerReference = "B00000001";
		}

		CusStatementLineCharge charge;
	}
}
