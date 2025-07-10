using System;
using CargoWise.Definitions;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentTriageActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.IncidentTriage; }
		}

		public override Type RootType
		{
			get { return typeof(IncidentTriage); }
		}

		public override SecurityCheckpoint BaseCheckpoint => EDISecurityCheckpoints.CustomerServiceIncidentTriage;

		public override string SingularElementNoun
		{
			get { return IncidentTriage.SingularName; }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("915e7dd2-1c17-4783-bb55-93cd1f01f9c5", "Incident Triages"); }
		}
	}
}
