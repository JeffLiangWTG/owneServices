using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Billing.Integration.Test
{
	public class SourceInfoTest : TestCaseWithFactory
	{
		public void TestEmptySourceInfo()
		{
			AssertNotNull(SourceInfo.EmptySourceInfo);
			AssertEquals(BillingDataSource.None.ToString(), SourceInfo.EmptySourceInfo.DataSource);
			AssertEquals(ZString.Empty, SourceInfo.EmptySourceInfo.FileName);
			AssertEquals(BillingInterfaceName.None.ToString(), SourceInfo.EmptySourceInfo.InterfaceName);
			AssertEquals(ZGuid.Empty, SourceInfo.EmptySourceInfo.EDIMessagePK);
			AssertEquals(ZString.Empty, SourceInfo.EmptySourceInfo.SenderId);
		}

		public void TestNullFileName()
		{
			var sourceInfo = new SourceInfo(null, null, ZGuid.Empty, ZGuid.Empty, ZString.Empty, null);
			AssertEquals(ZString.Empty, sourceInfo.FileName);
		}

		public void TestSourceInfoShouldSuspendValidation()
		{
			var billingInterfaceName = BillingInterfaceName.ConstructorExposed("databaseValue");
			AssertEquals(billingInterfaceName.ShouldSuspendValidation, true);
			var sourceInfo = new SourceInfo(BillingDataSource.DataWizard, billingInterfaceName, ZGuid.Empty, ZGuid.Empty, "", "");
			AssertEquals(sourceInfo.ShouldSuspendValidation, true);

			billingInterfaceName = BillingInterfaceName.ConstructorExposed("databaseValue", true);
			AssertEquals(billingInterfaceName.ShouldSuspendValidation, true);
			sourceInfo = new SourceInfo(BillingDataSource.DataWizard, billingInterfaceName, ZGuid.Empty, ZGuid.Empty, "", "");
			AssertEquals(sourceInfo.ShouldSuspendValidation, true);

			billingInterfaceName = BillingInterfaceName.ConstructorExposed("databaseValue", false);
			AssertEquals(billingInterfaceName.ShouldSuspendValidation, false);
			sourceInfo = new SourceInfo(BillingDataSource.DataWizard, billingInterfaceName, ZGuid.Empty, ZGuid.Empty, "", "");
			AssertEquals(sourceInfo.ShouldSuspendValidation, false);
		}
	}
}
