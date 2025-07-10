using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.AuditDataServices.DataScience.Testing
{
	[TestedType(typeof(DataScienceSubscriberServiceTask))]
	class DataScienceSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<DataScienceSubscriberServiceTask>
	{
		public override DataScienceSubscriberServiceTask GenerateServiceTask()
		{
			return new DataScienceSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => true;

		public override string ServiceTaskName()
		{
			return DataScienceSubscriberServiceTask.Description;
		}

		public override void TestTaskServiceRequirementsAttribute()
		{
			AssertContainsExactElementsInAnyOrder(
				"Missing HostedServiceRequirement methods",
				new[] { "IsEdiClient", "CheckCdcIsEnabled", "IsAuditEnabled", "CheckCdcShouldBeDisabled" },
				new[]
				{
					typeof(DataScienceSubscriberServiceTask).GetMethods(),
					typeof(DataScienceSubscriberServiceTask).BaseType.GetMethods(),
					typeof(DataScienceSubscriberServiceTask).BaseType.BaseType.GetMethods(),
				}
					.SelectMany(methods => methods.Where(m => m.GetCustomAttributes(typeof(HostedServiceRequirementAttribute), false).Length > 0))
					.Select(m => m.Name));
		}

		public override void TestSubscriberServiceTaskAssemblyName()
		{
			var subscriberTask = GenerateServiceTask();

			AssertEquals("ZClientEDI.Business", subscriberTask.AssemblyName);
		}

		readonly IEnumerable<string> subscriberCodes = new List<string>
		{
			"DAC",
			"DAG",
			"DAT",
			"DBC",
			"DBS",
			"DBU",
			"DCC",
			"DDC",
			"DDP",
			"DGB",
			"DGC",
			"DGE",
			"DGG",
			"DGH",
			"DGK",
			"DGM",
			"DGO",
			"DGP",
			"DGR",
			"DGS",
			"DGT",
			"DGU",
			"DGV",
			"DGW",
			"DHE",
			"DHK",
			"DHO",
			"DIE",
			"DIG",
			"DIL",
			"DIM",
			"DIR",
			"DIT",
			"DJC",
			"DJM",
			"DJP",
			"DL6",
			"DL7",
			"DL9",
			"DLA",
			"DLC",
			"DLD",
			"DLE",
			"DO8",
			"DOC",
			"DOH",
			"DOO",
			"DPE",
			"DPH",
			"DPL",
			"DPT",
			"DPU",
			"DRU",
			"DSD",
			"DSM",
			"DST",
			"DTD",
			"DTL",
			"DTM",
			"DTP",
			"DW1",
			"DWI",
			"DWP",
		};

		public void TestSubscriberIsEnumeratedByEnumerateAllSubscriberTypes()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride(Clients.EDI)))
			{
				var subscriberKeyMap = new SubscriberLoader().EnumerateAllSubscriberTypes().ToDictionary(s => s.Code, s => s.GetType().FullName);
				var allSubscriberCodes = subscriberKeyMap.Keys;

				CombineAssertions("Missing subscriber from collection", () =>
				{
					foreach (var code in subscriberCodes)
					{
						AssertCollectionContains(code, code, allSubscriberCodes);
					}
				});
			}
		}

		public void TestSubscriberIsEnumeratedByEnumerateAllSubscriberTypes_NotEDI()
		{
			var loader = new SubscriberLoader();
			var subscriberKeyMap = loader.EnumerateAllSubscriberTypes().ToDictionary(s => s.Code, s => s.GetType().FullName);
			var allSubscriberCodes = subscriberKeyMap.Keys;

			CombineAssertions("Subscribers should not be loaded", () =>
			{
				foreach (var code in subscriberCodes)
				{
					AssertCollectionNotContains(code, code, allSubscriberCodes);
				}
			});
		}

		public void TestSubscriberIsEnumeratedByEnumerateSubscribersOfType()
		{
			var loader = new SubscriberLoader();
			var subscriberKeyMap = loader.EnumerateSubscribersOfType(SubscriberLoader.ZClientEdiBusinessAssemblyName, SubscriberLoader.DataScienceNamespace).ToDictionary(s => s.Code, s => s.GetType().FullName);
			var allSubscriberCodes = subscriberKeyMap.Keys;

			CombineAssertions("Missing subscriber from collection", () =>
			{
				foreach (var code in subscriberCodes)
				{
					AssertCollectionContains(code, code, allSubscriberCodes);
				}
			});
		}
	}
}
