using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration.Testing
{
	public class CustomsChargeTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			ZGuid creditor = ZGuid.NewZGuid();
			CustomsCharge charge1 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, creditor);
			CustomsCharge charge2 = new CustomsCharge(ZGuid.NewZGuid(), "", 100m, 10m, false, creditor);
			CustomsCharge charge3 = new CustomsCharge(ZGuid.Empty, "a", 100m, 10m, false, creditor);
			CustomsCharge charge4 = new CustomsCharge(ZGuid.Empty, "", 101m, 10m, false, creditor);
			CustomsCharge charge5 = new CustomsCharge(ZGuid.Empty, "", 100m, 11m, false, creditor);
			CustomsCharge charge6 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, true, creditor);
			CustomsCharge charge7 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, creditor);
			CustomsCharge charge8 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, ZGuid.NewZGuid());

			AssertEquals(false, charge1.Equals(null));
			AssertEquals(false, charge1.Equals(charge2));
			AssertEquals(false, charge1.Equals(charge3));
			AssertEquals(false, charge1.Equals(charge4));
			AssertEquals(false, charge1.Equals(charge5));
			AssertEquals(false, charge1.Equals(charge6));
			AssertEquals(true, charge1.Equals(charge7));
			AssertEquals(false, charge1.Equals(charge8));
		}

		public void TestGetHashCode()
		{
			ZGuid creditor = ZGuid.NewZGuid();
			CustomsCharge charge1 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, creditor);
			CustomsCharge charge2 = new CustomsCharge(ZGuid.NewZGuid(), "", 100m, 10m, false, creditor);
			CustomsCharge charge3 = new CustomsCharge(ZGuid.Empty, "a", 100m, 10m, false, creditor);
			CustomsCharge charge4 = new CustomsCharge(ZGuid.Empty, "", 101m, 10m, false, creditor);
			CustomsCharge charge5 = new CustomsCharge(ZGuid.Empty, "", 100m, 11m, false, creditor);
			CustomsCharge charge6 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, true, creditor);
			CustomsCharge charge7 = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, creditor);
			AssertNotEquals(charge2.GetHashCode(), charge1.GetHashCode());
			AssertNotEquals(charge3.GetHashCode(), charge1.GetHashCode());
			AssertNotEquals(charge4.GetHashCode(), charge1.GetHashCode());
			AssertNotEquals(charge5.GetHashCode(), charge1.GetHashCode());
			AssertNotEquals(charge6.GetHashCode(), charge1.GetHashCode());
			AssertEquals(charge7.GetHashCode(), charge1.GetHashCode());
		}

		public void TestAddAmountWith2Values()
		{
			ZGuid creditor = ZGuid.NewZGuid();
			CustomsCharge charge = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, creditor);
			AssertEquals("Precondition: Amount", 100m, charge.Amount);
			AssertEquals("Precondition: GST", 10m, charge.GST);

			charge.AddAmount(150m, 15m);
			AssertEquals("Amount", 250m, charge.Amount);
			AssertEquals("GST", 25m, charge.GST);

			charge.AddAmount(90m, 10m);
			AssertEquals("Amount", 340m, charge.Amount);
			AssertEquals("GST", 35m, charge.GST);
		}

		public void TestAddAmountWithOneValueAndGSTIndicator()
		{
			CustomsCharge charge = new CustomsCharge(ZGuid.Empty, "", 100m, 10m, false, ZGuid.NewZGuid());
			AssertEquals("Precondition: Amount", 100m, charge.Amount);
			AssertEquals("Precondition: GST", 10m, charge.GST);

			charge.AddAmount(false, 150m);
			AssertEquals("Amount", 250m, charge.Amount);
			AssertEquals("GST", 10m, charge.GST);

			charge.AddAmount(true, 90m);
			AssertEquals("Amount", 250m, charge.Amount);
			AssertEquals("GST", 100m, charge.GST);
		}

		public void TestConstructorAndProperties()
		{
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "Code1";

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "Code2";

			ZGuid creditor = ZGuid.NewZGuid();
			CustomsCharge charge = new CustomsCharge(chargeCode1.PK, "ChargeDescription", 100m, 10m, false, creditor);

			AssertEquals("Charge Code", chargeCode1.PK, charge.ChargeCodePK);
			AssertEquals("Description", "ChargeDescription", charge.Description);
			AssertEquals("Amount", 100m, charge.Amount);
			AssertEquals("GST", 10m, charge.GST);
			AssertEquals("IsPaidByBroker", false, charge.IsPaidByBroker);
			AssertEquals("IsInformationOnly", true, charge.IsInformationOnly);
			AssertEquals(creditor, charge.CreditorPK);

			CustomsCharge charge2 = new CustomsCharge(chargeCode2, "ChargeDescription2", 200m, 0m, true, creditor);

			AssertEquals("Charge Code", chargeCode2.PK, charge2.ChargeCodePK);
			AssertEquals("Description", "ChargeDescription2", charge2.Description);
			AssertEquals("Amount", 200m, charge2.Amount);
			AssertEquals("GST", 0m, charge2.GST);
			AssertEquals("IsPaidByBroker", true, charge2.IsPaidByBroker);
			AssertEquals("IsInformationOnly", false, charge2.IsInformationOnly);
		}
	}
}
