using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class TriageAssistTreeBizObjWrapperCollection : NonPersistentBusinessObjectCollection<TriageAssistTreeBizObjWrapper>
	{
		public TriageAssistTreeBizObjWrapperCollection(TriageAssistBusinessObject parent) : base(parent.Factory)
		{
			Parent = parent;
		}

		public override void Load()
		{
			try
			{
				using (Parent.FilteredTriageAssistTreeWrapperCollection.SuppressRebuild())
				using (Parent.FilteredTriageAssistTreeWrapperCollection.SuspendCountChanged(null))
				using (SuspendCountChanged(null))
				{
					RemoveAll();

					var subQueryIncidentTriageDiagnosticCriteriaPivot = new ZDBOnlySubQuery(typeof(IncidentTriageDiagnosticCriteriaPivot), IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage);
					subQueryIncidentTriageDiagnosticCriteriaPivot.AddToFilter(IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria,
											Parent.LinkedCriteriaCollection.Where(x => x.Confirm).Select(x => x.PK));

					var query = new ZDBOnlyQuery(typeof(IncidentTriage));
					query.AddToFilter(IncidentTriageSchema.IMT_IsActive, true);
					query.AddSubQuery(subQueryIncidentTriageDiagnosticCriteriaPivot, JoinCondition.And);

					var wrappers = Parent.Factory.Load<IncidentTriage>(query).Select(x => new TriageAssistTreeTriageWrapper(Parent, x))
										.OrderBy(x => Parent.Parent.TriagePK == x.Triage.PK ? 0 : 1)
										.ThenBy(x => x.IsFocused ? 0 : 1)
										.ThenBy(x => x.TriageStatus)
										.ThenByDescending(x => x.ConfirmCount)
										.ThenByDescending(x => x.InvestigateCount)
										.ThenBy(x => x.NegateCount)
										.ToArray();

					AddRange(wrappers);
				}
			}
			finally
			{
				OnCountChanged(null);
			}
		}

		public TriageAssistBusinessObject Parent { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
