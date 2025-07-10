using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentSimilarityMatrix : AutoIncidentSimilarityMatrix
	{
		public IncidentSimilarityMatrix(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public SupportIncident GetOtherIncident(ZGuid notThisIncidentGuid)
		{
			if (ISM_IM_Incident1 != notThisIncidentGuid && ISM_IM_Incident2 != notThisIncidentGuid)
			{
				throw new ArgumentException("The provided Support Incident GUID is neither of the foreign GUIDs", nameof(notThisIncidentGuid));
			}

			if (ISM_IM_Incident1 == notThisIncidentGuid && ISM_IM_Incident2 == notThisIncidentGuid)
			{
				throw new InvalidOperationException("Both foreign Incident GUIDs are the same");
			}

			var guid = (notThisIncidentGuid == ISM_IM_Incident1 ? ISM_IM_Incident2 : ISM_IM_Incident1);

			return Factory.Load<SupportIncident>(guid);
		}
	}
}
