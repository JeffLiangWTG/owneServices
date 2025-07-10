using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(GlobalChargeCodeMapPivot))]
	internal abstract class GlobalChargeCodeMapPivotTest : EnterpriseBusinessObjectTestCase
	{
		public abstract void TestFetchStrategy();
		public void TestYP_Description()
		{
			GlobalChargeCodeMapPivot pivot = (GlobalChargeCodeMapPivot)GetNewBusinessObject();
			AssertEquals(ZString.Empty, pivot.YP_Description);
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Test description";
			pivot.YP_AC = chargeCode.PK;
			AssertEquals("Test description", pivot.YP_Description);
		}
	}
}
