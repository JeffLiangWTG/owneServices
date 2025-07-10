using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentSimilarityExclusion : AutoIncidentSimilarityExclusion, Integration.ZClientEDI.IIncidentSimilarityExclusion
	{
		public IncidentSimilarityExclusion(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
