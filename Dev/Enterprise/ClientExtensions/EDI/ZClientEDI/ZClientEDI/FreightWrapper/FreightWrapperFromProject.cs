using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FreightWrapperFromProject : FreightWrapperEDI<EDIProject>
	{
		public FreightWrapperFromProject(EDIProject project, BusinessObjectFactory factory)
			: base(project, factory)
		{ }
	}
}
