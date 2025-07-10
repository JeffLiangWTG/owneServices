using System;
using System.ComponentModel;
using Enterprise.Client;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Module.Testing
{
	public class EDIIncidentWebCollectionSorterTest : WebCollectionSorterTest
	{
		public void TestIncidentMainCollectionSorting()
		{
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			IncidentMainBase lowIncident = collection.AddNew();
			lowIncident.IM_Priority = "CR4";
			lowIncident.IM_Description = "An incident description";
			IncidentMainBase criticalIncident = collection.AddNew();
			criticalIncident.IM_Priority = "CR1";
			criticalIncident.IM_Description = "More incidents";
			IncidentMainBase hiIncident = collection.AddNew();
			hiIncident.IM_Priority = "CR2";
			hiIncident.IM_Description = "Yet another incident";
			IncidentMainBase mediumIncident = collection.AddNew();
			mediumIncident.IM_Priority = "CR3";
			mediumIncident.IM_Description = "More incidents that have broken";
			AssertEquals("Collection should not be sorted initially", lowIncident.PK, collection[0].PK);
			collection.Sort(new EDIIncidentWebCollectionSorter(IncidentMainSchema.IM_Priority.Name, ListSortDirection.Ascending));
			AssertEquals("Collection should be sorted ascending", "CR4", collection[0].IM_Priority);
			AssertEquals("Collection should be sorted ascending", "CR3", collection[1].IM_Priority);
			AssertEquals("Collection should be sorted ascending", "CR2", collection[2].IM_Priority);
			AssertEquals("Collection should be sorted ascending", "CR1", collection[3].IM_Priority);
			collection.Sort(new EDIIncidentWebCollectionSorter(IncidentMainSchema.IM_Priority.Name, ListSortDirection.Descending));
			AssertEquals("Collection should be sorted ascending", "CR1", collection[0].IM_Priority);
			AssertEquals("Collection should be sorted ascending", "CR2", collection[1].IM_Priority);
			AssertEquals("Collection should be sorted ascending", "CR3", collection[2].IM_Priority);
			AssertEquals("Collection should be sorted ascending", "CR4", collection[3].IM_Priority);
			collection.Sort(new EDIIncidentWebCollectionSorter(IncidentMainSchema.IM_Description.Name, ListSortDirection.Ascending));
			AssertEquals("Collection should be sorted ascending", "An incident description", collection[0].IM_Description);
			AssertEquals("Collection should be sorted ascending", "More incidents", collection[1].IM_Description);
			AssertEquals("Collection should be sorted ascending", "More incidents that have broken", collection[2].IM_Description);
			AssertEquals("Collection should be sorted ascending", "Yet another incident", collection[3].IM_Description);
			collection.Sort(new EDIIncidentWebCollectionSorter(IncidentMainSchema.IM_Description.Name, ListSortDirection.Descending));
			AssertEquals("Collection should be sorted ascending", "Yet another incident", collection[0].IM_Description);
			AssertEquals("Collection should be sorted ascending", "More incidents that have broken", collection[1].IM_Description);
			AssertEquals("Collection should be sorted ascending", "More incidents", collection[2].IM_Description);
			AssertEquals("Collection should be sorted ascending", "An incident description", collection[3].IM_Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			overridenClientHook = ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance);
		}

		protected override void TearDown()
		{
			overridenClientHook.Dispose();
			base.TearDown();
		}

		IDisposable overridenClientHook;
	}
}
