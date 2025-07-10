using System;
using WiseTech.Business.Core;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IAcceptabilityBandService
	{
		bool TryGetWorkflowsMatchingAcceptabilityBand(Guid boardId, Guid acceptabilityBandId, out BoardHealthWorkflowDto[] acceptabilityBandWorkflowResultDto, out BusinessResponse businessResponse);
		bool TryGetAcceptabilityBand(Guid boardPK, Guid acceptabilityBandPk, out AcceptabilityBandResultDto acceptabilityBandResultDto, out BusinessResponse businessResponse);
	}
}
