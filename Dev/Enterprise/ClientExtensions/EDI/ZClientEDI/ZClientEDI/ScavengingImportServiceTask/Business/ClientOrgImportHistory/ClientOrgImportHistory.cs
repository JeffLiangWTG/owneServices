using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask.Business
{
	public class ClientOrgImportHistory : AutoClientOrgImportHistory
	{
		public ClientOrgImportHistory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
