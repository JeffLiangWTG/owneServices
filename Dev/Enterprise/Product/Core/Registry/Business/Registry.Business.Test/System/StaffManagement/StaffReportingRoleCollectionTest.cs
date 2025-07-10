using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffReportingRoleCollection))]
	sealed class StaffReportingRoleCollectionTest : CodeDescriptionBoolCollectionAbstractTest<StaffReportingRoleCollection>
	{
		#region Implementation

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(true, new StaffReportingRoleCollection(false, false).AddNew().Bool);
			AssertEquals(true, new StaffReportingRoleCollection(true, false).AddNew().Bool);
			AssertEquals(true, new StaffReportingRoleCollection(false, true).AddNew().Bool);
			AssertEquals(true, new StaffReportingRoleCollection(true, true).AddNew().Bool);
			AssertEquals(true, new StaffReportingRoleCollection().AddNew().Bool);
		}

		public void TestDefaultSharedRoleAllowedForNewChild()
		{
			AssertEquals(false, new StaffReportingRoleCollection(false, false).AddNew().SharedRoleAllowed);
			AssertEquals(true, new StaffReportingRoleCollection(true, false).AddNew().SharedRoleAllowed);
			AssertEquals(false, new StaffReportingRoleCollection(false, true).AddNew().SharedRoleAllowed);
			AssertEquals(true, new StaffReportingRoleCollection(true, true).AddNew().SharedRoleAllowed);
			AssertEquals(false, new StaffReportingRoleCollection().AddNew().SharedRoleAllowed);
		}

		public void TestDefaultIsMandatoryAllowedForNewChild()
		{
			AssertEquals(false, new StaffReportingRoleCollection(false, false).AddNew().IsMandatory);
			AssertEquals(false, new StaffReportingRoleCollection(true, false).AddNew().IsMandatory);
			AssertEquals(true, new StaffReportingRoleCollection(false, true).AddNew().IsMandatory);
			AssertEquals(true, new StaffReportingRoleCollection(true, true).AddNew().IsMandatory);
			AssertEquals(false, new StaffReportingRoleCollection().AddNew().IsMandatory);
		}

		public void TestEnabledStatuses()
		{
			var roles = new StaffReportingRoleCollection();
			roles.Add("PRM", (NoResString)"Payroll Manager", true, false, false);
			roles.Add("LVM", (NoResString)"Leave Manager", false, false, false);
			roles.Add("TST", (NoResString)"Test", true, false, false);
			int trueCount = 0;
			int falseCount = 0;

			foreach (StaffReportingRole t in roles)
			{
				if (t.Bool)
				{
					trueCount++;
				}
				else if (!t.Bool)
				{
					falseCount++;
				}
			}
			AssertEquals("1 Value should be false", 1, falseCount);
			AssertEquals("2 Values are enabled, so the count should be 2", 2, trueCount);
		}

		protected override StaffReportingRoleCollection GetCollectionToTest()
		{
			return new StaffReportingRoleCollection();
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StaffReportingRole();
		}

		#endregion
	}
}
