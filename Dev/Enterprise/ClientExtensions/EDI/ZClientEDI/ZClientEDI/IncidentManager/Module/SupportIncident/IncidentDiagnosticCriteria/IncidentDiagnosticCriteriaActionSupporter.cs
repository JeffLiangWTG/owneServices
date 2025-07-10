using System;
using CargoWise.Definitions;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentDiagnosticCriteriaActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DiagnosticCriteria; }
		}

		public override Type RootType
		{
			get { return typeof(IncidentDiagnosticCriteria); }
		}

		public override SecurityCheckpoint BaseCheckpoint => EDISecurityCheckpoints.CustomerServiceIncidentDiagnosticCriteria;

		public override string SingularElementNoun
		{
			get { return IncidentDiagnosticCriteria.SingularName; }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("cb4b78cf-e297-4840-9d12-60e4a63c1f12", "Diagnostic Criteria"); }
		}
	}
}
