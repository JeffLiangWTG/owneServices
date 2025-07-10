using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Implementation;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public delegate void LoopedWorkflowsHandler(string loopedWorkflows, string relationshipDescription);

	public static class ProcessHeaderLinkCycleValidationHelper
	{
		public static void CheckLoop(ProcessHeaderLink link, LoopedWorkflowsHandler loopedWorkflowsHandler, Action hierarchyRelationshipBetweenDependenciesHandler)
		{
			var headerFrom = link.HeaderFrom;
			var headerTo = link.HeaderTo;

			if (ShouldValidateLoops(link))
			{
				CheckLoop(link, loopedWorkflowsHandler, hierarchyRelationshipBetweenDependenciesHandler, headerFrom, headerTo);
			}
		}

		static void CheckLoop(ILoopDetectable link, LoopedWorkflowsHandler loopedWorkflowsHandler, Action hierarchyRelationshipBetweenDependenciesHandler, ProcessHeader headerFrom, ProcessHeader headerTo, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			if (headerFrom != null && headerTo != null)
			{
				CheckCycles(headerFrom.Factory, headerFrom, headerTo, DependencyValidationCacheKey, Res.GetString("b4e9d375-39aa-4aea-8d69-bfc8da64d7bc", "dependency"), WorkflowRelationshipGraphBuilder.GetDependencyCycles, loopedWorkflowsHandler, tempLinks);

				if (link.LinkType == ProcessHeaderLinkTypeList.Codes.Dependency)
				{
					ValidateNoHierarchyRelationshipBetweenDependencies(headerFrom, headerTo, hierarchyRelationshipBetweenDependenciesHandler);
				}
				else if (link.LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild)
				{
					CheckCycles(headerFrom.Factory, headerFrom, headerTo, ParentChildValidationCacheKey, Res.GetString("575e7a3b-699d-40f9-ae35-2e4bf22502a8", "Parent-Child relationship"), WorkflowRelationshipGraphBuilder.GetHierarchicCycles, loopedWorkflowsHandler, tempLinks);
				}
			}
		}

		static bool ShouldValidateLoops(ProcessHeaderLink link) => link.IsLoopValidationForced || !link.IsInDatabase || link.FP_LinkTypeInfo.HasChanges || link.FP_FH_HeaderFromInfo.HasChanges || link.FP_FH_HeaderToInfo.HasChanges;

		public const string DependencyValidationCacheKey = "b4e9d375-39aa-4aea-8d69-bfc8da64d7bc";
		public const string ParentChildValidationCacheKey = "575e7a3b-699d-40f9-ae35-2e4bf22502a8";

		static void ValidateNoHierarchyRelationshipBetweenDependencies(ProcessHeader headerFrom, ProcessHeader headerTo, Action hierarchyRelationshipBetweenDependenciesHandler, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var hierarchyGraph = headerFrom.GetHierarchyGraph(tempLinks);

			if (hierarchyGraph.ContainsVertex(headerTo))
			{
				if (hierarchyGraph.TryGetShortestPathInclusive(headerFrom, headerTo, f => 0, out IEnumerable<ProcessHeader> header)
					|| hierarchyGraph.TryGetShortestPathInclusive(headerTo, headerFrom, f => 0, out header))
				{
					hierarchyRelationshipBetweenDependenciesHandler();
				}
			}
		}

		static void CheckCycles(BusinessObjectFactory factory, ProcessHeader headerFrom, ProcessHeader headerTo, string key, string relationshipDescription, Func<ProcessHeader, WorkflowCycleCache, IEnumerable<TemporaryTemplateLink>, ProcessHeader[]> getCycles, LoopedWorkflowsHandler loopedWorkflowsHandler, IEnumerable<TemporaryTemplateLink> tempLinks = null)
		{
			var cycleCache = (WorkflowCycleCache)factory.ValidationCache.CachedData.GetOrAdd(key, () => new WorkflowCycleCache());
			var headerFromCycles = getCycles(headerFrom, cycleCache, tempLinks);
			var headerToCycles = headerTo.GetHierarchicCycles(cycleCache, tempLinks);

			if (headerFromCycles.Length > 1 || headerToCycles.Length > 1)
			{
				var loopedPCHWorkflows = GetHeadersList(headerToCycles, headerFromCycles);
				loopedWorkflowsHandler(loopedPCHWorkflows, relationshipDescription);
			}
		}

		static string GetHeadersList(ProcessHeader[] headerTo, ProcessHeader[] headerFrom)
		{
			return string.Join(System.Environment.NewLine, headerFrom.Union(headerTo).Select(h => GetDescription(h)).OrderBy(a => a));
		}

		static string GetDescription(ProcessHeader header)
		{
			if (header.IsTemplate)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0}.{1}", header.Template.P0_Name, header.Description);
			}

			return header.Description;
		}
	}
}
