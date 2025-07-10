using System;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Common;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Common.Dtos;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IProjectService
	{
		ContentWithHashDto GetDetails(Guid projectId);

		TryUpdateDetailsResult TryUpdateDetails(Guid projectId, UpdateContentWithHashRequest updateRequest);

		bool TryUpdateSummary(Guid projectId, UpdateContentWithHashRequest updateRequest, out PaveError error);
	}
}
