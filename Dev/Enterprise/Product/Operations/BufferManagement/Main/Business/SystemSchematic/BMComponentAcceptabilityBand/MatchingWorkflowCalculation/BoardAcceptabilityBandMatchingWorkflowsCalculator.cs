using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class BoardAcceptabilityBandMatchingWorkflowsCalculator
	{
		public static AcceptabilityBandMatchingWorkflowResultCollection GetMatchingWorkflows(BusinessObjectFactory factory, BMComponentAcceptabilityBand band, List<Guid> componentPKs, Guid releaseGroupPK)
		{
			var provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
			var parameters = new AcceptabilityBandSqlBuilderParameters(
				band.PK,
				(bool)band.BAB_FiltersByReleaseGroup,
				(bool)band.BAB_FiltersBySection,
				band.BoundaryValues,
				releaseGroupPK,
				int.MaxValue);

			parameters.ShouldFilterByReleaseGroup = releaseGroupPK != Guid.Empty;
			parameters.ShouldFilterBySection = componentPKs != null;

			if (parameters.ShouldFilterBySection)
			{
				parameters.WorkflowPKs = GetWorkflowPks(factory, componentPKs, parameters.ShouldFilterByReleaseGroup ? releaseGroupPK : null);
				parameters.AreValid = parameters.WorkflowPKs != null;
			}

			return AcceptabilityBandMatchingWorkflowsCalculator.GetMatchingWorkflows(band, provider, parameters);
		}

		public static HashSet<ZGuid> GetWorkflowPks(BusinessObjectFactory factory, List<Guid> componentPks, Guid? releaseGroupPk)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, componentPks);
			query.AllowTableValuedParameters = true;

			if (releaseGroupPk != null)
			{
				query.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, releaseGroupPk);
			}

			return factory.Load<ProcessHeader>(query)
				.Select(w => w.PK)
				.ToHashSet();
		}
	}
}
