using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientWorkProject : AutoClientWorkProject
	{
		public ClientWorkProject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
