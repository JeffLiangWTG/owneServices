using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask.Business
{
	public class ClientOrgConsol : AutoClientOrgConsol
	{
		public ClientOrgConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
