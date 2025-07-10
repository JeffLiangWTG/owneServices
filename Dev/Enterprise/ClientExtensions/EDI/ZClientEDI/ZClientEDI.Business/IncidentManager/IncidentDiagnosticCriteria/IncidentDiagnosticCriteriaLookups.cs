//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentDiagnosticCriteriaLookups
//
//    This class should be used for overriding collections in AutoIncidentDiagnosticCriteriaLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentDiagnosticCriteriaLookups : AutoIncidentDiagnosticCriteriaLookups
	{
		public IncidentDiagnosticCriteriaLookups(AutoIncidentDiagnosticCriteria parent) : base(parent)
		{
		}

		public IncidentDiagnosticCriteriaLookups(BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		public IncidentDiagnosticCriteriaLookups(FilterStripBusinessObject businessObject) : base(null)
		{
			factory = businessObject.Factory;
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		public new IncidentDiagnosticCriteria Parent
		{
			get { return (IncidentDiagnosticCriteria)base.Parent; }
		}

		public CodeDescriptionPairList Types => new IncidentDiagnosticCriteriaTypes();

		public IncidentTriageCollection IncidentTriageNotLinked
		{
			get
			{
				if (incidentTriageNotLinked == null)
				{
					var query = new ZDBOnlyQuery(typeof(IncidentTriage));
					var subPivotQuery = new ZDBOnlySubQuery(typeof(IncidentTriageDiagnosticCriteriaPivot), IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage, notIn: true);
					subPivotQuery.AddToFilter(IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria, Parent.PK);
					query.AddSubQuery(IncidentTriageSchema.PK, subPivotQuery, JoinCondition.And);

					incidentTriageNotLinked = new IncidentTriageCollection(Factory, query);
				}
				return incidentTriageNotLinked;
			}
		}

		IncidentTriageCollection incidentTriageNotLinked;
	}
}
