using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAppointedAgentPortsCollection : DocumentWrapperCollection
	{
		public DocAppointedAgentPortsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocAppointedAgentPortsCollection(OrgAppointedAgentPortsDependentCollection collectionSource, BusinessObjectFactory factory)
			: base(collectionSource, factory)
		{
		}

		public new DocAppointedAgentPorts this[int index]
		{
			get { return (DocAppointedAgentPorts)base[index]; }
		}
	}
}
