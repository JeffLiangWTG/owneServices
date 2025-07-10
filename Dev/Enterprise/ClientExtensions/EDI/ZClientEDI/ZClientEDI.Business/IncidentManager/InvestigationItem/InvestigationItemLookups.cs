//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoInvestigationItemLookups
//
//    This class should be used for overriding collections in AutoInvestigationItemLookups
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
	public class InvestigationItemLookups : AutoInvestigationItemLookups
	{
		public InvestigationItemLookups(AutoInvestigationItem parent) : base(parent)
		{
		}

		public InvestigationItemLookups(BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		public InvestigationItemLookups(FilterStripBusinessObject businessObject) : base(null)
		{
			factory = businessObject.Factory;
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		public new InvestigationItem Parent
		{
			get { return (InvestigationItem)base.Parent; }
		}

		public CodeDescriptionPairList Types => new InvestigationItemTypes();

		public IncidentDiagnosticCriteriaCollection DiagnosticCriteriaNotLinked
		{
			get
			{
				if (diagnosticCriteriaNotLinked == null)
				{
					var query = new ZDBOnlyQuery(typeof(IncidentDiagnosticCriteria));
					var subPivotQuery = new ZDBOnlySubQuery(typeof(DiagnosticCriteriaInvestigationItemLink), DiagnosticCriteriaInvestigationItemLinkSchema.DIL_IMD_DiagnosticCriteria, notIn: true);
					subPivotQuery.AddToFilter(DiagnosticCriteriaInvestigationItemLinkSchema.DIL_INV_InvestigationItem, Parent.PK);
					query.AddSubQuery(IncidentDiagnosticCriteriaSchema.PK, subPivotQuery, JoinCondition.And);

					diagnosticCriteriaNotLinked = new IncidentDiagnosticCriteriaCollection(Factory, query);
				}
				return diagnosticCriteriaNotLinked;
			}
		}

		IncidentDiagnosticCriteriaCollection diagnosticCriteriaNotLinked;
	}
}
