using System;
using CargoWise.Definitions;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class InvestigationItemActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.InvestigationItem; }
		}

		public override Type RootType
		{
			get { return typeof(InvestigationItem); }
		}

		public override SecurityCheckpoint BaseCheckpoint => EDISecurityCheckpoints.CustomerServiceInvestigationItem;

		public override string SingularElementNoun
		{
			get { return InvestigationItem.SingularName; }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("eeec4169-7356-4cef-afa5-b2d754cee249", "Investigation Items"); }
		}
	}
}
