
using System;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared.Common;

namespace Enterprise.BufferManagement.Service.Shared.Incident
{
	public interface IIncidentService
	{
		bool TryUpdateSummary(Guid incidentId, UpdateContentWithHashRequest updateRequest, out PaveError error);
	}
}
