using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AssignedStaffWrapper))]
	sealed class AssignedStaffWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			AssignedStaffWrapper wrapper = (AssignedStaffWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapper.Category.Code", "CAT", wrapper.Category);
			AssertEquals("wrapper.Relationship", "Relationship", wrapper.Relationship);
			AssertEquals("wrapper.Staff", GlbStaff.CurrentUser, wrapper.Staff.WrappedObject);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
Staff : CargoWise Support
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new AssignedStaffWrapper(GlbStaff.CurrentUser, "CAT", "relationship", Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
AssignedStaffMember                             (Default Field: Staff)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Staff                                   StaffMember
Category                                String
Relationship                            String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new AssignedStaffWrapper(GlbStaff.CurrentUser, "CAT", "Relationship", Factory);
		}

		#endregion
	}
}
