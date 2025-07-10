using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Newtonsoft.Json;

// Commenting out HostedServiceAttribute and sealing class,
// This class should not exist as a service task, though it's function needs to remain for testing purposes while being swapped out

//[assembly: HostedService(IncidentAssociationDatasetBuilderServiceTask.Code,
//	"Incident Association Dataset Builder",
//	"SYS",
//	typeof(IncidentAssociationDatasetBuilderServiceTask),
//	AllowsMultipleInstances = false,
//	CanRunInAnyBranch = true,
//	MinimumPeriod = "1day",
//	MaximumPeriod = "7days",
//	DefaultScheduleRunEvery = "1day"
//	)]

namespace Enterprise.Client.EDI
{
	public sealed class IncidentAssociationDatasetBuilderServiceTask
		//: ServiceProviderImpl
	{
		public const string Code = "IAD";

		public static void InitializeIncidentAssociationSystem()
		{
			var factory = new BusinessObjectFactory();
			IncidentAssociationSystemStatus.DeleteAllStatusRows(factory);
			IncidentAssociationSystemStatus.SetStatus(IncidentAssociationSystemStatus.Initializing, factory);
			InitializeTokensFromResource(factory);
			IncidentAssociationSystemStatus.SetStatus(IncidentAssociationSystemStatus.ReadyToBootstrap, factory);
		}

		public static void InitializeTokensFromResource(BusinessObjectFactory factory)
		{
			var tokenPairs = GetTokenPairsFromJson();

			new SimilarIncidentRepository(new IncidentAssociationQuery(), 0)
				.SetTokenPairs(tokenPairs);

			SimilarIncidentRepository.SetLatestVersion(0, factory);
		}

		public static Dictionary<string, int> GetTokenPairsFromJson()
		{
			var assembly = Assembly.GetExecutingAssembly();

			string json;
			using (var sr = new StreamReader(assembly.GetManifestResourceStream("Enterprise.Client.EDI.ServiceTasks.IncidentAssociation.Bootstrapper.InitialTokens.json")))
			{
				json = sr.ReadToEnd();
			}

			var groups = JsonConvert.DeserializeObject<List<List<string>>>(json);

			var terms = groups.SelectMany(s => s).Distinct();

			var tokenPairs = new Dictionary<string, int>();

			foreach (var term in terms)
			{
				for (var i = 0; i < groups.Count; i++)
				{
					if (groups[i].Contains(term))
					{
						tokenPairs.Add(term, i);
						break;
					}
				}
			}

			return tokenPairs;
		}
	}
}
