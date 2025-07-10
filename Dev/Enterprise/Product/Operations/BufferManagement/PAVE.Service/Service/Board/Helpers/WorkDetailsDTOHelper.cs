using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Models = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	internal static class WorkDetailsDTOHelper
	{
		internal static WorkDetailsDTO CreateWorkDetailsDTO(IEnumerable<ProcessHeader> workflows,
			IEnumerable<ProcessTask> tasks,
			Dictionary<Guid, IEnumerable<Models.Tag>> tagsPerTaskPks,
			Func<IDictionary<ZGuid, decimal>> getIndexes = null,
			PropertyCache cache = null,
			bool loadParentWorkflowPK = true,
			Dictionary<string, BMControlCustomisation> layouts = null)
		{
			if (cache == null)
			{
				cache = new PropertyCache();
			}

			if (!workflows.Any() && !tasks.Any())
			{
				return new WorkDetailsDTO();
			}

			return new WorkDetailsDTO()
			{
				Jobs = workflows.ToJobsDTO(cache, layouts),
				Workflows = workflows.ToWorkflowsDTO(getIndexes, loadParentWorkflowPK, cache, layouts),
				Capabilities = tasks.ToCapabilitiesDTO(),
				Tags = tagsPerTaskPks.ToTagDTOs(),
				Tasks = tasks.ToTasksDTO(tagsPerTaskPks, cache, layouts)
			};
		}
	}
}
