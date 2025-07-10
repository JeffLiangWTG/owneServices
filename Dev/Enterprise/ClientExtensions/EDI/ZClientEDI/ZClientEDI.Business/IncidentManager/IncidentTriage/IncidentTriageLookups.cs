//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentTriageLookups
//
//    This class should be used for overriding collections in AutoIncidentTriageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageLookups : AutoIncidentTriageLookups
	{
		public IncidentTriageLookups(AutoIncidentTriage parent) : base(parent)
		{
		}

		public IncidentTriageLookups(BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		public IncidentTriageLookups(FilterStripBusinessObject businessObject) : base(null)
		{
			factory = businessObject.Factory;
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		public new IncidentTriage Parent
		{
			get { return (IncidentTriage)base.Parent; }
		}

		public CodeDescriptionPairList Types => new IncidentTriageTypes();

		public CodeDescriptionPairList Levels => new IncidentTriageLevels();

		#region Product

		public CodeDescriptionPairList ProductList
		{
			get
			{
				return Factory.GetCachedValue("IncidentTriageLookups.ProductList",
					() =>
					{
						return IncidentDetailsLookupsHelper.ProductList;
					});
			}
		}

		#endregion

		#region Product Area

		public CodeDescriptionPairList ProductAreaList
		{
			get {
				return Factory.GetCachedValue("IncidentTriageLookups.ProductAreaList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(EDIDataRegistry.Instance.ProductAreas.Value);
						return result;
					});
			}
		}

		#endregion

		#region Module

		public CodeDescriptionPairList ModuleListAllModules
		{
			get
			{
				var moduleListType = Parent?.ModuleType ?? ModuleListType.Unspecified;
				var product = Parent?.IMT_Product ?? string.Empty;
				var productArea = Parent?.IMT_ProductArea ?? string.Empty;

				return Factory.GetCachedValue("ModuleList:" + moduleListType.ToString() + product.PadRight(3) + productArea.PadRight(3),
				delegate
				{
					return new SupportIncidentModuleListBuilder().Build(moduleListType, product, productArea);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "<Pending>")]
		public static readonly CodeDescriptionPairList ModuleListForTemplate = new SupportIncidentModuleListBuilder().Build(ModuleListType.Unspecified, string.Empty, string.Empty);

		#endregion

		public IncidentTriageChecklistItemCollection IncidentsNotLinked
		{
			get
			{
				if (incidentsNotLinked == null)
				{
					RefreshIncidentNotLinked();
				}
				return incidentsNotLinked;
			}
		}

		public void RefreshIncidentNotLinked()
		{
			var query = new ZDBOnlyQuery(typeof(IncidentTriageChecklistItem));
			var subPivotQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItemPivot), IncidentTriageChecklistItemPivotSchema.IMP_IMC_ChecklistItem, notIn: true);
			subPivotQuery.AddToFilter(IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage, Parent.PK);
			query.AddSubQuery(IncidentTriageChecklistItemSchema.PK, subPivotQuery, JoinCondition.And);

			incidentsNotLinked = new IncidentTriageChecklistItemCollection(Factory, query);
		}

		IncidentTriageChecklistItemCollection incidentsNotLinked;

		public IncidentDiagnosticCriteriaCollection DiagnosticCriteriaNotLinked
		{
			get
			{
				if (diagnosticCriteriaNotLinked == null)
				{
					var query = new ZDBOnlyQuery(typeof(IncidentDiagnosticCriteria));
					var subPivotQuery = new ZDBOnlySubQuery(typeof(IncidentTriageDiagnosticCriteriaPivot), IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria, notIn: true);
					subPivotQuery.AddToFilter(IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage, Parent.PK);
					query.AddSubQuery(IncidentDiagnosticCriteriaSchema.PK, subPivotQuery, JoinCondition.And);

					diagnosticCriteriaNotLinked = new IncidentDiagnosticCriteriaCollection(Factory, query);
				}
				return diagnosticCriteriaNotLinked;
			}
		}

		IncidentDiagnosticCriteriaCollection diagnosticCriteriaNotLinked;
	}
}

