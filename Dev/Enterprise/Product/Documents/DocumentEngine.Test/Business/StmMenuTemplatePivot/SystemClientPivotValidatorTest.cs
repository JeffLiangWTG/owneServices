using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Business.Validation.Testing
{
	sealed class SystemClientPivotValidatorTest : TestCaseWithFactory
	{
		public void TestSystemOnlyPivotCannotUseClientMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, true, true, false, true, false);
			AssertEquals("errorText", "System defined joining relationship cannot be added to a client specific document / report.", errorText);
		}

		public void TestSystemOnlyPivotCannotUseUserMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(false, false, true, false, true, false);
			AssertEquals("errorText", "System defined joining relationship cannot be added to a user defined document / report.", errorText);
		}

		public void TestSystemOnlyPivotCanUseSystemMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, false, true, false, true, false);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestSystemOnlyPivotCannotUseClientTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, false, true, true, true, false);
			AssertEquals("errorText", "System defined joining relationship cannot use a client specific template / document.", errorText);
		}

		public void TestSystemOnlyPivotCannotUseUserTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, false, false, false, true, false);
			AssertEquals("errorText", "System defined joining relationship cannot use a user defined template / document.", errorText);
		}

		public void TestClientPivotCanUseSystemMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, false, true, true, true, true);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestClientPivotCanUseClientMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, true, true, true, true, true);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestClientPivotCannotUseUserMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(false, false, true, true, true, true);
			AssertEquals("errorText", "Client specific joining relationship cannot be added to a user defined document / report.", errorText);
		}

		public void TestClientPivotCanUseSystemTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, true, true, false, true, true);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestClientPivotCanUseClientTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, true, true, true, true, true);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestClientPivotCannotUseUserTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, true, false, false, true, true);
			AssertEquals("errorText", "Client specific joining relationship cannot use user defined template / document.", errorText);
		}

		public void TestUserPivotCannotUseSystemMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, false, false, false, false, false);
			AssertEquals("errorText", "User defined joining relationship cannot be added to a system defined document / report.", errorText);
		}

		public void TestUserPivotCannotUseClientMenu()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(true, true, false, false, false, false);
			AssertEquals("errorText", "User defined joining relationship cannot be added to a client specific document / report.", errorText);
		}

		public void TestUserPivotCanUseUserMenuAndUserTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(false, false, false, false, false, false);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestUserPivotCanUseSystemTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(false, false, true, false, false, false);
			AssertEquals("errorText", ZString.Empty, errorText);
		}

		public void TestUserPivotCanUseClientTemplate()
		{
			ZString errorText = SystemClientPivotValidator.GetErrorText(false, false, true, true, false, false);
			AssertEquals("errorText", ZString.Empty, errorText);
		}
	}
}
