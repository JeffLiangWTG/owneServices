using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocAppointedAgentPortsCollection))]
	public class DocAppointedAgentPortsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocAppointedAgentPortsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgAppointedAgentPorts agentPorts = Factory.New<OrgAppointedAgentPorts>();
			return DocAppointedAgentPorts.New(agentPorts, Factory);
		}

		protected override DocAppointedAgentPortsCollection GetCollectionToTest()
		{
			return new DocAppointedAgentPortsCollection(Factory);
		}
	}
}
