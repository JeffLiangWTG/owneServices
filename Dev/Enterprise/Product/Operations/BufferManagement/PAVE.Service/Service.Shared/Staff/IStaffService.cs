using System;
using Enterprise.BufferManagement.Service.Shared.Staff.Dtos;

namespace Enterprise.BufferManagement.Service.Shared.Staff
{
	public interface IStaffService
	{
		StaffCapacityDto GetStaffCapacity(Guid componentId, Guid staffId);
	}
}
