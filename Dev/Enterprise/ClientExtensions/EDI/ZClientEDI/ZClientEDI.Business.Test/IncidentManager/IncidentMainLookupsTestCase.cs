using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	abstract class IncidentMainLookupsTestCase : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			TestCodeDescriptionPairList(ExpectedStatusList, Incident.Lookups.StatusList);
		}

		public void TestListHelper()
		{
			IncidentListHelper listHelper = Incident.Lookups.ListHelper;
			AssertNotNull(listHelper);
			AssertEquals("Should be cached", listHelper, Incident.Lookups.ListHelper);
		}

		protected virtual IReadOnlyList<CodeDescriptionPair> ExpectedStatusList
		{
			get
			{
				return new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(IncidentMainLookups.Status.Open, "Open"),
					new CodeDescriptionPair(IncidentMainLookups.Status.Working, "Working"),
					new CodeDescriptionPair(IncidentMainLookups.Status.Suspended, "Suspended"),
					new CodeDescriptionPair(IncidentMainLookups.Status.Closed, "Closed")
				};
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

		protected abstract AutoIncidentMain GetNewIncidentMain();

		protected AutoIncidentMain Incident
		{
			get { return incident ?? (incident = GetNewIncidentMain()); }
		}

		AutoIncidentMain incident;
	}
}
