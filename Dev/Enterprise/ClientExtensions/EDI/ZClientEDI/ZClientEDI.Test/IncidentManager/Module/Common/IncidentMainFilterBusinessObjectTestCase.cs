using System.Collections.Generic;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	abstract class IncidentMainFilterBusinessObjectTestCase : FilterStripBusinessObjectTestCase
	{
		public void TestIncidentTypeFilter_AlwaysAppliedAndHidden()
		{
			ModuleTextFilter incidentTypeFilter = (ModuleTextFilter)CachedBusinessObject["Incident Type"];
			AssertNotNull(incidentTypeFilter);
			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, incidentTypeFilter.Visibility);
			AssertEquals(IncidentMainSchema.IM_IncidentType, incidentTypeFilter.FilterColumn);
			AssertEquals(ExpectedIncidentType, incidentTypeFilter.Property);
		}

		public void TestStatusList()
		{
			TestCodeDescriptionPairList(ExpectedStatusList, CachedBusinessObject.InternalStatusList);
		}

		protected virtual IReadOnlyList<CodeDescriptionPair> ExpectedStatusList
		{
			get
			{
				CodeDescriptionPairList lookupsStatusList = CachedBusinessObject.InternalLookups.StatusList;
				CodeDescriptionPair[] result = new CodeDescriptionPair[lookupsStatusList.Count + 2];
				result[0] = new CodeDescriptionPair("", "All Statuses");
				result[1] = new CodeDescriptionPair(IncidentMainLookups.Status.NotClosed, "Not Closed");
				lookupsStatusList.CopyTo(result, 2);
				return result;
			}
		}

		protected void TestCodeDescriptionPairList(IReadOnlyList<CodeDescriptionPair> expected, CodeDescriptionPairList actual)
		{
			AssertEquals(expected.Count, actual.Count);
			for (int i = 0; i < expected.Count; i++)
			{
				AssertEquals(expected[i].Code, actual[i].Code);
				AssertEquals(expected[i].Description, actual[i].Description);
			}
		}

		protected IncidentMainLookups Lookups
		{
			get
			{
				return CachedBusinessObject.InternalLookups;
			}
		}

		protected new IncidentMainFilterBusinessObject CachedBusinessObject
		{
			get
			{
				return (IncidentMainFilterBusinessObject)base.CachedBusinessObject;
			}
		}

		protected abstract string ExpectedIncidentType { get; }
	}
}
