using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class IncidentDiagnosticCriteriaPivot : AutoIncidentDiagnosticCriteriaPivot
	{
		public IncidentDiagnosticCriteriaPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void LinkParent(ITriageAssistParent parent)
		{
			IMV_ParentID = parent.PK;
			IMV_ParentTableCode = parent.TablePrefix;
		}

		public IncidentDiagnosticCriteria DiagnosticCriteria => Factory.Load<IncidentDiagnosticCriteria>(IMV_IMD_DiagnosticCriteria);

		[RelatedBusinessObject("DiagnosticCriteria")]
		public override ZGuid IMV_IMD_DiagnosticCriteria { get => base.IMV_IMD_DiagnosticCriteria; set => base.IMV_IMD_DiagnosticCriteria = value; }
	}
}
