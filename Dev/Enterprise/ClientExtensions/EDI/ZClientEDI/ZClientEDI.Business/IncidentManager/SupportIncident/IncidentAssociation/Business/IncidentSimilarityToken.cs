using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentSimilarityToken : AutoIncidentSimilarityToken
	{
		public IncidentSimilarityToken(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
