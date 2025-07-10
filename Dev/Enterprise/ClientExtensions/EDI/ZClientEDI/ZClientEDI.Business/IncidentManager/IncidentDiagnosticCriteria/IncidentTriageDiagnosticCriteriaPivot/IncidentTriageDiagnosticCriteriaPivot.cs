using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[DependentBusinessObject(typeof(IncidentTriage), "DiagnosticCriteriaPivots")]
	public class IncidentTriageDiagnosticCriteriaPivot : AutoIncidentTriageDiagnosticCriteriaPivot
	{
		public IncidentTriageDiagnosticCriteriaPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IncidentDiagnosticCriteria DiagnosticCriteria => Factory.Load<IncidentDiagnosticCriteria>(IMO_IMD_DiagnosticCriteria);

		[RelatedBusinessObject("DiagnosticCriteria")]
		public override ZGuid IMO_IMD_DiagnosticCriteria { get => base.IMO_IMD_DiagnosticCriteria; set => base.IMO_IMD_DiagnosticCriteria = value; }

		public IncidentTriage Triage => Factory.Load<IncidentTriage>(IMO_IMT_Triage);

		[RelatedBusinessObject("Triage")]
		public override ZGuid IMO_IMT_Triage { get => base.IMO_IMT_Triage; set => base.IMO_IMT_Triage = value; }
	}
}
