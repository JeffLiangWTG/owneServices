using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiERequestDocumentQueue : AutoEdiERequestDocumentQueue
	{
		public EdiERequestDocumentQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}


