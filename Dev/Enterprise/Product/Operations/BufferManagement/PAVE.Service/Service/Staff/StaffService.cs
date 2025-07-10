using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Shared.Staff;
using Enterprise.BufferManagement.Service.Shared.Staff.Dtos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Service
{
	public class StaffService : IStaffService
	{
		public StaffCapacityDto GetStaffCapacity(Guid componentId, Guid staffId)
		{
			var factory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = nameof(StaffService), RefreshEnabled = false };

			if (componentId == Guid.Empty || staffId == Guid.Empty)
			{
				return null;
			}

			var staff = factory.Load<GlbStaff>(staffId);

			if (staff == null)
			{
				return null;
			}

			var component = factory.Load<BMComponent>(componentId);

			if (component == null)
			{
				return null;
			}

			var capacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff.WrapWithEnumerable(), component).FirstOrDefault().Value;

			if (capacity == null)
			{
				return null;
			}

			return new StaffCapacityDto
			{
				UtilisedPercent = capacity.FullCapacity > 0 ? Convert.ToInt32(capacity.UtilisedCapacity * 1m / capacity.FullCapacity * 100) : 0,
				IsOverloaded = capacity.IsOverloaded
			};
		}
	}
}
