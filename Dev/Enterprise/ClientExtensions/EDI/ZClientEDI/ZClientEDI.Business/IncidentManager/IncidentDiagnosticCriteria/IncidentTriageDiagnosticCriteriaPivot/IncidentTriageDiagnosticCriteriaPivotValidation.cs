//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentTriageDiagnosticCriteriaPivotValidation
//
//    This class should be used for overriding validation in AutoIncidentTriageDiagnosticCriteriaPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class IncidentTriageDiagnosticCriteriaPivotValidation : AutoIncidentTriageDiagnosticCriteriaPivotValidation
	{
		public IncidentTriageDiagnosticCriteriaPivotValidation(AutoIncidentTriageDiagnosticCriteriaPivot parent) : base(parent)
		{
		}

		new IncidentTriageDiagnosticCriteriaPivot Parent =>  (IncidentTriageDiagnosticCriteriaPivot)base.Parent;

		protected override void CheckIMO_IMD_DiagnosticCriteria()
		{
			base.CheckIMO_IMD_DiagnosticCriteria();

			if (Parent.Triage != null && Parent.DiagnosticCriteria != null)
			{
				if (Parent.Triage.IMT_Level == IncidentTriageLevels.Codes.PreliminaryAssignment)
				{
					if (Parent.DiagnosticCriteria.IMD_Type != IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom)
					{
						Parent.IMO_IMD_DiagnosticCriteriaInfo.AddError("Only Diagnostic Criteria of type 'SMP' can be attached to a 'Preliminary Assignment' Triage Node.");
					}
					else
					{
						var subQuery = new ZDBOnlySubQuery(typeof(IncidentTriage), IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage);
						subQuery.AddToFilter(IncidentTriageSchema.IMT_Level, IncidentTriageLevels.Codes.PreliminaryAssignment);
						subQuery.AddToFilter(IncidentTriageSchema.PK, SQLComparisonOperator.NotEqual, Parent.IMO_IMT_Triage);

						var query = new ZDBOnlyQuery(typeof(IncidentTriageDiagnosticCriteriaPivot));
						query.AddToFilter(IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria, Parent.IMO_IMD_DiagnosticCriteria);
						query.AddSubQuery(subQuery, JoinCondition.And);

						if (Parent.Factory.Exists(typeof(IncidentTriageDiagnosticCriteriaPivot), query))
						{
							Parent.IMO_IMD_DiagnosticCriteriaInfo.AddError("The 'SMP' Diagnostic Criteria can only be attached to a single 'Preliminary Assignment' Triage Node.");
						}
					}
				}
			}
		}
	}
}


