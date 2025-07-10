using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Staff"), WrapperTypeName("AssignedStaffMember")]
	public class AssignedStaffWrapper : GenericWrapper
	{
		public AssignedStaffWrapper(GlbStaff staff, ZString category, ZString relationship, BusinessObjectFactory factory)
			: base(staff, factory)
		{
			this.Category = category;
			this.Relationship = relationship;
		}

		[CodeStringFinderHint(typeof(AssignedStaffWrapperCollection), "AddSalesRepresentatives")]
		[CodeStringFinderHint(typeof(AssignedStaffWrapperCollection), "AddCustomerServiceAgents")]
		[CodeStringFinderHint(typeof(AssignedStaffWrapperCollection), "AddCartageCoordinators")]
		public ZString Relationship { get; private set; }

		public ZString Category { get; private set; }

		public StaffWrapper Staff
		{
			get { return staff ?? (staff = new StaffWrapper(StaffBO, Factory)); }
		}
		StaffWrapper staff;

		#region Implementation

		GlbStaff StaffBO
		{
			get { return (GlbStaff)WrappedBO; }
		}

		#endregion
	}
}
