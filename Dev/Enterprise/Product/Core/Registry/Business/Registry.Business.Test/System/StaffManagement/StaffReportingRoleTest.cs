using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffReportingRole))]
	sealed class StaffReportingRoleTest : CodeDescriptionBoolTest
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new StaffReportingRole();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;
			result.SharedRoleAllowed = true;
			result.IsMandatory = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((StaffReportingRole)clone).Bool);
			AssertEquals(true, ((StaffReportingRole)clone).SharedRoleAllowed);
			AssertEquals(true, ((StaffReportingRole)clone).IsMandatory);
		}

		public void TestValidatIsMandatory()
		{
			TestBizObj.Bool = true;
			TestBizObj.IsMandatory = true;
			AssertNoErrors(TestBizObj.IsMandatoryInfo);

			TestBizObj.Bool = false;
			AssertNoErrors(TestBizObj.IsMandatoryInfo);

			TestBizObj.IsMandatory = true;
			AssertHasError(TestBizObj.IsMandatoryInfo, "Role which is disabled cannot be mandatory.");
		}

		public void TestDefaultDescriptionEditable()
		{
			var result = (StaffReportingRole)GetNewBusinessObject();
			result.Code = DefaultStaffReportingRoles.Codes.DirectManager;
			result.Description = DefaultStaffReportingRoles.Descriptions.DirectManager;
			result.Bool = true;
			result.SharedRoleAllowed = false;
			result.IsMandatory = false;
			AssertEquals(false, result.DescriptionInfo.ReadOnly);

			result.Description = (NoResString)"Test Manager";
			AssertEquals("Test Description", "Test Manager", result.Description);
		}

		public override void TestReadOnlyStates()
		{
			var result = (StaffReportingRole)GetNewBusinessObject();
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);

			result.Code = DefaultStaffReportingRoles.Codes.DirectManager;
			result.Description = DefaultStaffReportingRoles.Descriptions.DirectManager;
			result.Bool = true;
			result.SharedRoleAllowed = false;
			result.IsMandatory = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new StaffReportingRole();
		}

		StaffReportingRole TestBizObj
		{
			get { return (StaffReportingRole)BizObj; }
		}

		#endregion
	}
}
