using System;
using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Common;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Common.Dtos;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IWorkItemService
	{
		TryUpdateDetailsResult TryUpdateDetails(Guid workItemId, UpdateContentWithHashRequest updateRequest);

		bool TryUpdateSummary(Guid workItemId, UpdateContentWithHashRequest updateRequest, out PaveError error);

		ContentWithHashDto GetDetails(Guid workItemId);

		IEnumerable<CodeDescriptionDto> GetWorkItemTypes();
		IEnumerable<CodeDescriptionDto> GetWorkItemAreas(string workItemType);
		IEnumerable<CodeDescriptionDto> GetActivityTypes(string workItemType, string workItemArea);
		IEnumerable<CodeDescriptionDto> GetActivitySubtypes(string workItemType, string workItemArea, string activityType);
		IEnumerable<CodeDescriptionDto> GetPriorities(string workItemType, string workItemArea, string activityType, string activitySubType);
	}
}
