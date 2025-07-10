using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment.Testing
{
	sealed class TemporaryUserContextTest : TestCaseWithFactory
	{
		public void TestConstructorAndSetMethod()
		{
			ICompany testCurrentCompany = Env.Instance.CurrentCompany;
			IBranch testCurrentBranch = Env.Instance.CurrentBranch;
			IDepartment testCurrentDepartment = Env.Instance.CurrentDepartment;
			IUser testCurrentUser = Env.Instance.CurrentUser;
			DataRegistry testRegistry = Env.Instance.Registry;

			TemporaryUserContext userContext = new TemporaryUserContext()
			{
				StaffLoginName = "Test",
				BranchPK = Env.CurrentBranch.PK,
				DepartmentPK = Env.CurrentDepartment.PK
			};

			AssertEquals("CurrentCompany has not changed", testCurrentCompany, Env.Instance.CurrentCompany);
			AssertEquals("CurrentBranch has not changed", testCurrentBranch, Env.Instance.CurrentBranch);
			AssertEquals("CurrentDepartment has not changed", testCurrentDepartment, Env.Instance.CurrentDepartment);
			AssertEquals("CurrentUser has not changed", testCurrentUser, Env.Instance.CurrentUser);
			AssertEquals("CurrentUser has not changed", testRegistry, Env.Instance.Registry);

			using (userContext.Set())
			{
				AssertNotEquals("Temporary Environment CurrentCompany", testCurrentCompany, Env.Instance.CurrentCompany);
				AssertNotEquals("Temporary Environment CurrentBranch", testCurrentBranch, Env.Instance.CurrentBranch);
				AssertNotEquals("Temporary Environment CurrentDepartment", testCurrentDepartment, Env.Instance.CurrentDepartment);
				AssertNotEquals("Temporary Environment CurrentUser", testCurrentUser, Env.Instance.CurrentUser);
				AssertNotEquals("Temporary Environment Registry", testCurrentUser, Env.Instance.Registry);
			}

			AssertEquals("CurrentCompany was restored", testCurrentCompany, Env.Instance.CurrentCompany);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);
			AssertEquals("CurrentDepartment was restored", testCurrentDepartment, Env.Instance.CurrentDepartment);
			AssertEquals("CurrentUser was restored", testCurrentUser, Env.Instance.CurrentUser);
			AssertEquals("CurrentUser was restored", testRegistry, Env.Instance.Registry);
		}

		public void TestConstructor_WhenCurrentStaffBranchDepartmentAreNull_ShouldNotThrow()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, Guid.Empty))
			{
				AssertThingThatShouldBeNullAndCanInstantiateUserContext(Env.CurrentDepartment, "Env.CurrentDepartment", Env.CurrentUser, Env.CurrentBranch);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Guid.Empty, Env.CurrentDepartment.PK))
			{
				AssertThingThatShouldBeNullAndCanInstantiateUserContext(Env.CurrentBranch, "Env.CurrentBranch", Env.CurrentUser, Env.CurrentDepartment);
			}
			using (Env.SetTemporaryUserContext(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertThingThatShouldBeNullAndCanInstantiateUserContext(Env.CurrentUser, "Env.CurrentUser", Env.CurrentBranch, Env.CurrentDepartment);
			}
		}

		void AssertThingThatShouldBeNullAndCanInstantiateUserContext(object thingThatShouldBeNull, string nameOfThing, params object[] thingsThatShouldntBeNull)
		{
			AssertNull(nameOfThing + " should be null", thingThatShouldBeNull);
			foreach (var obj in thingsThatShouldntBeNull)
			{
				AssertNotNull(obj);
			}
			AssertNoExceptionThrown(string.Format("Should be able to create a TemporaryUserContext when {0} is null", nameOfThing),
				() => new TemporaryUserContext());
		}
	}
}
