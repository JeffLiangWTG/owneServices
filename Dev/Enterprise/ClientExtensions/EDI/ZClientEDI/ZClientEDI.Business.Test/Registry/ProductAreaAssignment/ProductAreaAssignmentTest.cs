using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaAssignment))]
	internal sealed class ProductAreaAssignmentTest : RegistryBusinessObjectTemplateTestCase<ProductAreaAssignment>
	{
		public void TestValidateProductArea()
		{
			ProductAreaAssignment assignment = NewPopulatedBusinessObject();
			assignment.ValidateProductArea();
			AssertHasErrors(assignment.ProductAreaInfo);

			assignment.ProductArea = "AA";
			assignment.ValidateProductArea();
			AssertHasErrors(assignment.ProductAreaInfo);

			assignment.ProductArea = "ARC";
			assignment.ValidateProductArea();
			AssertNoErrors(assignment.ProductAreaInfo);
		}

		public void TestValidateStaff()
		{
			ProductAreaAssignment assignment = NewPopulatedBusinessObject();
			assignment.ValidateStaff();
			AssertHasErrors(assignment.StaffInfo);

			assignment.Staff = "AA";
			assignment.ValidateStaff();
			AssertHasErrors(assignment.StaffInfo);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			Factory.Save();

			assignment.Staff = "SCW";
			assignment.ValidateStaff();
			AssertNoErrors(assignment.StaffInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ProductAreaAssignment GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override ProductAreaAssignment GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		ProductAreaAssignment NewPopulatedBusinessObject()
		{
			ProductAreaAssignment result = new ProductAreaAssignment(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
