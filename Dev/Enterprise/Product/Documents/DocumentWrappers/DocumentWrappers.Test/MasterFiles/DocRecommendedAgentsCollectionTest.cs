using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocRecommendedAgentsCollection))]
	public class DocRecommendedAgentsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocRecommendedAgentsCollection>
	{
		protected override DocRecommendedAgentsCollection GetCollectionToTest()
		{
			return new DocRecommendedAgentsCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocOrganisation.New(OrgHeader.New(Factory), Factory);
		}

		public void TestLoad()
		{
			DocUNLOCO docAUSYD = DocUNLOCO.New(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"), Factory);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgAddress aIR_EXP_HAN = GetForwarderAgent(newFactory, docAUSYD.Code, transportAir, directionExport, statusHandles);
			OrgAddress sEA_IMP_APP = GetForwarderAgent(newFactory, docAUSYD.Code, transportSea, directionImport, statusAppointed);

			OrgAddress aIR_EXP_PUB = GetForwarderAgent(newFactory, docAUSYD.Code, transportAir, directionExport, statusPublished);
			OrgAddress rAI_IMP_PUB = GetForwarderAgent(newFactory, docAUSYD.Code, transportRail, directionImport, statusPublished);
			OrgAddress rOA_EXP_PUB = GetForwarderAgent(newFactory, docAUSYD.Code, transportRoad, directionExport, statusPublished);

			OrgHeader forwarder = GetForwarder(newFactory);
			OrgAddress rAI_EXP_PUB = AddForwarderAgent(forwarder, docAUSYD.Code, transportRail, directionExport, statusPublished);
			OrgAddress rOA_IMP_PUB = AddForwarderAgent(forwarder, docAUSYD.Code, transportRoad, directionImport, statusPublished);
			OrgAddress sEA_EXP_PUB = AddForwarderAgent(forwarder, docAUSYD.Code, transportSea, directionExport, statusPublished);
			OrgAddress sEA_IMP_PUB = AddForwarderAgent(forwarder, docAUSYD.Code, transportSea, directionImport, statusPublished);

			newFactory.Save();

			DocRecommendedAgentsCollection collection;

			collection = new DocRecommendedAgentsCollection(Factory, null, transportAir, directionExport);
			AssertForwarderAgents(collection);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportAir, directionExport);
			AssertForwarderAgents(collection, aIR_EXP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportAir, directionImport);
			AssertForwarderAgents(collection);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportRail, directionExport);
			AssertForwarderAgents(collection, rAI_EXP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportRail, directionImport);
			AssertForwarderAgents(collection, rAI_IMP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportRoad, directionExport);
			AssertForwarderAgents(collection, rOA_EXP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportRoad, directionImport);
			AssertForwarderAgents(collection, rOA_IMP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportSea, directionExport);
			AssertForwarderAgents(collection, sEA_EXP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportSea, directionImport);
			AssertForwarderAgents(collection, sEA_IMP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportAll, directionExport);
			AssertForwarderAgents(collection, aIR_EXP_PUB, rOA_EXP_PUB, sEA_EXP_PUB);
			//AssertForwarderAgents(collection, AIR_EXP_PUB, ROA_EXP_PUB, RAI_EXP_PUB, SEA_EXP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, transportAll, directionImport);
			AssertForwarderAgents(collection, rAI_IMP_PUB, sEA_IMP_PUB);
			//AssertForwarderAgents(collection, RAI_IMP_PUB, ROA_IMP_PUB, SEA_IMP_PUB);

			collection = new DocRecommendedAgentsCollection(Factory, docAUSYD, "AAA", directionImport);
			AssertForwarderAgents(collection);
		}

		#region Implementation

		#region Forwarder Agents

		const string transportAll = Core.Constants.TransportModes.All;
		const string transportAir = Core.Constants.TransportModes.Air;
		const string transportRail = Core.Constants.TransportModes.Rail;
		const string transportRoad = Core.Constants.TransportModes.Road;
		const string transportSea = Core.Constants.TransportModes.Sea;

		const string statusPublished = AgentStatusList.Codes.Published;
		const string statusAppointed = AgentStatusList.Codes.Appointed;
		const string statusHandles = AgentStatusList.Codes.Handles;

		const string directionExport = AgentDirectionList.Codes.Export;
		const string directionImport = AgentDirectionList.Codes.Import;

		int addressCount;

		OrgHeader GetForwarder(BusinessObjectFactory forwarderFactory)
		{
			OrgHeader result = forwarderFactory.New<OrgHeader>();
			result.OH_Code = string.Format("FWD{0}", ++addressCount);
			result.OH_RL_NKClosestPort = "AUSYD";
			result.MainAddress.OA_Address1 = string.Format("{0} main address", result.OH_Code);
			result.OH_IsForwarder = true;

			return result;
		}

		OrgAddress GetForwarderAgent(BusinessObjectFactory forwarderFactory, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgHeader forwarder = GetForwarder(forwarderFactory);
			OrgAddress address = AddForwarderAgent(forwarder, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);

			return address;
		}

		OrgAddress AddForwarderAgent(OrgHeader forwarder, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgAddress address = forwarder.Addresses.AddNew();
			address.OA_Code = string.Format("{0}_{1}_{2}_{3}_{4}", agentPortOrCountry, agentTransportMode, agentDirection, agentStatus, ++addressCount);
			address.OA_Address1 = address.OA_Code;

			return AddForwarderAgent(forwarder, address, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);
		}

		OrgAddress AddForwarderAgent(OrgHeader forwarder, OrgAddress address, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgAppointedAgentPorts newAppAgent = forwarder.AppointedAgentPorts.AddNew();
			newAppAgent.O5_PortOrCountry = agentPortOrCountry;
			newAppAgent.O5_OA_AgentOfficeAddress = address.PK;
			newAppAgent.O5_AgentDirection = agentDirection;

			switch (agentTransportMode)
			{
				case Core.Constants.TransportModes.Air:
					newAppAgent.O5_AirAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Rail:
					newAppAgent.O5_RailAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Road:
					newAppAgent.O5_RoadAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Sea:
					newAppAgent.O5_SeaAgentStatus = agentStatus;
					break;
			}

			return address;
		}

		void AssertForwarderAgents(DocRecommendedAgentsCollection collection, params OrgAddress[] expectedAddresses)
		{
			collection.Load();
			ZString[] actual = Array.ConvertAll(collection.ToArray<DocOrganisation>(), c => c.SelectedAddress.Code);
			ZString[] expected = Array.ConvertAll(expectedAddresses, c => c.OA_Code);
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#endregion
	}
}
