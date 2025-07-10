using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public abstract class IncidentMainFilterBusinessObject : FilterStripBusinessObject
	{
		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = GetNewModuleFilterCollection();
			AddIncidentTypeFilter(filters);
			return filters;
		}

		protected virtual ModuleFilterCollection GetNewModuleFilterCollection()
		{
			return new ModuleFilterCollection();
		}

		internal protected IEnumerable<ModuleFilter> GetActiveFiltersByDescription(string description)
		{
			if (moduleFiltersCreated)
			{
				Regex descriptionRegExp = new Regex(@"^" + description + @"(\s\([\d]\))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
				foreach (ModuleFilter filter in ActiveModuleFilters)
				{
					if (descriptionRegExp.IsMatch(filter.Description))
					{
						yield return filter;
					}
				}
			}
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			moduleFiltersCreated = true;
		}

		protected override void OnModuleFiltersReset()
		{
			base.OnModuleFiltersReset();
			moduleFiltersCreated = false;
		}

		bool moduleFiltersCreated;

		void AddIncidentTypeFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter incidentTypeFilter = filters.AddTextFilter("Incident Type", IncidentMainSchema.IM_IncidentType);
			incidentTypeFilter.Property = IncidentType;
			incidentTypeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		protected abstract string IncidentType { get; }

		#endregion

		#region Lookups

		protected virtual CodeDescriptionPairList StatusList
		{
			get
			{
				CodeDescriptionPairList result = Lookups.StatusList;
				result.Insert(0, new CodeDescriptionPair("", "All Statuses"));
				result.Insert(1, new CodeDescriptionPair(IncidentMainLookups.Status.NotClosed, "Not Closed"));
				return result;
			}
		}

		protected IncidentMainLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected abstract IncidentMainLookups GetNewLookups();

		IncidentMainLookups lookups;

		// expose attibutes for tests
		internal CodeDescriptionPairList InternalStatusList
		{
			get { return StatusList; }
		}
		internal IncidentMainLookups InternalLookups
		{
			get { return Lookups; }
		}

		#endregion
	}
}
