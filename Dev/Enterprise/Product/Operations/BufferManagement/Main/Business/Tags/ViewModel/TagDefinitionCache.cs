using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// This class *must* load everything eagerly in order to avoid thread related problems.
	/// </summary>
	public class TagDefinitionCache
	{
		public TagDefinitionCache(BusinessObjectFactory factory)
		{
			definitionsCache = factory.Load<TagDefinition>(new ZQuery { ReLoadExistingRows = true })
				.Where(d => d.TGD_Code != BMConstants.WorkQueuesTagGroupCode || BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement)
				.ToDictionary(d => d.PK);
			var magnitudes = factory.Load<TagMagnitude>(new ZQuery { ReLoadExistingRows = true });

			magnitudesByDefinition = magnitudes.GroupBy(m => m.TGM_TGD_Tag).ToDictionary(group => group.Key, group => new HashSet<ZGuid>(group.Select(m => m.PK)));
			MagnitudesCache = magnitudes.ToDictionary(t => t.PK);
		}

		readonly Dictionary<ZGuid, TagDefinition> definitionsCache;
		readonly Dictionary<ZGuid, HashSet<ZGuid>> magnitudesByDefinition;
		public Dictionary<ZGuid, TagMagnitude> MagnitudesCache { get; private set; }

		public IEnumerable<TagDefinition> AllDefinitionsRelevantToCurrentWorkflowManagementMode
		{
			get { return definitionsCache.Values; }
		}

		public IEnumerable<TagMagnitude> AllMagnitudes
		{
			get { return MagnitudesCache.Values; }
		}

		public IEnumerable<TagMagnitude> GetMagnitudes(TagDefinition definition)
		{
			HashSet<ZGuid> magnitudes;
			if (magnitudesByDefinition.TryGetValue(definition.PK, out magnitudes))
			{
				foreach (var pk in magnitudes)
				{
					TagMagnitude mag;
					if (MagnitudesCache.TryGetValue(pk, out mag))
					{
						yield return mag;
					}
				}
			}
		}

		public void AddMagnitude(TagDefinition definition, TagMagnitude magnitude)
		{
			if (!magnitudesByDefinition.ContainsKey(definition.PK))
			{
				magnitudesByDefinition[definition.PK] = new HashSet<ZGuid>();
			}

			magnitudesByDefinition[definition.PK].Add(magnitude.PK);
			MagnitudesCache[magnitude.PK] = magnitude;
		}

		public void RemoveMagnitude(TagDefinition definition, TagMagnitude magnitude)
		{
			magnitudesByDefinition[definition.PK].Remove(magnitude.PK);
			MagnitudesCache.Remove(magnitude.PK);
		}
	}
}
