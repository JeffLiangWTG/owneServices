using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.PAVE.Common.DTO;
using CargoWise.PAVE.Common.Model;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Model = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	internal static class JobHelper
	{
		#region Model To DTO

		public static IEnumerable<JobDTO> ToJobDTOs(this IEnumerable<Model.Workflow> workflows)
		{
			var jobDictionary = workflows.DistinctBy(w => w.JobPK)
				.ToDictionary(workflow => workflow.JobPK, workflow => workflow.Job);

			var jobDTOs = jobDictionary.Select(keyJob => ToJobDTO(keyJob.Key, keyJob.Value));

			return jobDTOs;
		}

		static JobDTO ToJobDTO(Guid pk, Job job)
		{
			if (job == null)
			{
				return new JobDTO { PK = pk };
			}

			return new JobDTO
			{
				PK = pk,
				#region SuppressResourceStringsCheckRegion 
				Properties = new Dictionary<string, object>
				{
					{ "code", job.Code },
					{ "description", job.Description }
				}
				#endregion
			};
		}

		#endregion

		#region BusinessObject to DTO

		internal static IEnumerable<JobDTO> ToJobsDTO(this IEnumerable<ProcessHeader> workflows, PropertyCache cache, Dictionary<string, BMControlCustomisation> layouts)
		{
			return workflows.Select(w => new { JobPK = w.FH_ParentId, Job = w.Parent, Layout = CustomisedLayoutHelper.GetLayoutForRelatedJobType(layouts, w) })
				.Select(jobAndLayout => new JobDTO()
				{
					PK = jobAndLayout.JobPK.ToGuid(),
					Properties = jobAndLayout.Job?.LoadProperties(cache, jobAndLayout.Layout)
				}).DistinctBy(job => job.PK)
				.ToArray();
		}

		#endregion
	}
}
