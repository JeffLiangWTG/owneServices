using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public abstract class TriageAssistTreeBizObjWrapper : NonPersistentBusinessObject
	{
		public virtual IEnumerable<TriageAssistTreeBizObjWrapper> Children { get; protected set; } = Enumerable.Empty<TriageAssistTreeBizObjWrapper>();
		public virtual ZString Description => ZString.Empty;
		public virtual ZString Type => ZString.Empty;
		public virtual ZString Status => ZString.Empty;
		public virtual ZString Product => ZString.Empty;
		public virtual ZString Area => ZString.Empty;
		public virtual ZString Section => ZString.Empty;
		public virtual ZBool IsFocused => false;
		public abstract BusinessObject BizObj { get; }

		public virtual ZBool IsPublishedToAssist => false;
	}

	public class TriageAssistTreeTriageWrapper : TriageAssistTreeBizObjWrapper
	{
		public TriageAssistTreeTriageWrapper(TriageAssistBusinessObject parent, IncidentTriage triage)
		{
			Parent = parent;
			Triage = triage;

			var attachedCriteriaQuery = new ZQuery(IncidentDiagnosticCriteriaSchema.PK,
					triage.DiagnosticCriteriaPivots.Select(x => x.IMO_IMD_DiagnosticCriteria));
			attachedCriteriaQuery.AddToFilter(IncidentDiagnosticCriteriaSchema.IMD_IsActive, true);

			AttachedCriteria = Parent.LoadRelevantCriteria(attachedCriteriaQuery).OrderBy(x => x, new RelevantDiagnosticCriteria.CriteriaComparer()).ToArray();
			var attachedAndAlsoLinkedCriteria = AttachedCriteria.Intersect(Parent.LinkedCriteriaCollection.OfType<RelevantDiagnosticCriteria>()).ToArray();
			ConfirmCount = attachedAndAlsoLinkedCriteria.Count(x => x.Confirm);
			InvestigateCount = attachedAndAlsoLinkedCriteria.Count(x => x.Investigate);
			NegateCount = attachedAndAlsoLinkedCriteria.Count(x => x.Negate);

			Children = AttachedCriteria.Select(x => new TriageAssistTreeRelevantCriteriaWrapper(x)).ToArray();

			if (Parent.Parent.TriagePK == triage.PK)
			{
				TriageStatus = TriageNodeStatus.Finalised;
			}
			else if (InvestigateCount > 0 && NegateCount == 0 && AreEquivalentProducts(triage.IMT_Product, Parent.Product))
			{
				TriageStatus = TriageNodeStatus.Investigating;
			}
			else if (NegateCount > 0 || !AreEquivalentProducts(triage.IMT_Product, Parent.Product))
			{
				TriageStatus = TriageNodeStatus.Excluded;
			}
			else if (attachedAndAlsoLinkedCriteria.All(x => x.Confirm) && attachedAndAlsoLinkedCriteria.Length == AttachedCriteria.Count())
			{
				TriageStatus = TriageNodeStatus.AllConfirmed;
			}
			else
			{
				TriageStatus = TriageNodeStatus.Unknown;
			}

			if (TriageStatus != TriageNodeStatus.Excluded)
			{
				var linkedAndConfirmed = Parent.LinkedCriteriaCollection.OfType<RelevantDiagnosticCriteria>().Where(x => x.Confirm);
				var essentialLinkedAndConfirmed = linkedAndConfirmed.Where(x => x.DiagnosticCriteria.IMD_FocusRelatedTriageNodesOnly);
				if (linkedAndConfirmed.Any()
					&& (!essentialLinkedAndConfirmed.Any() || essentialLinkedAndConfirmed.Intersect(AttachedCriteria).Any()))
				{
					IsFocused = true;
				}
			}
		}

		public enum TriageNodeStatus
		{
			Finalised,
			AllConfirmed,
			Investigating,
			Unknown,
			Excluded,
		}

		public TriageAssistBusinessObject Parent { get; }
		public IncidentTriage Triage { get; }
		IEnumerable<RelevantDiagnosticCriteria> AttachedCriteria { get; }

		public override ZString Description => Triage.IMT_SupportDescription;
		public override ZString Type => Triage.TypeDescription;
		public TriageNodeStatus TriageStatus { get; }
		public override ZString Status => TriageStatus == TriageNodeStatus.Unknown ? "" : TriageStatus == TriageNodeStatus.AllConfirmed ? "All Confirmed" : Enum.GetName(typeof(TriageNodeStatus), TriageStatus);
		public override ZString Product => Triage.IMT_Product;
		public override ZString Area => Triage.IMT_SetProductAreaByMenuItem ? "***" : Triage.IMT_ProductArea;
		public override ZString Section => Triage.IMT_Module;
		public override ZBool IsPublishedToAssist => Triage.IMT_IsPublishedToAssist;
		public override BusinessObject BizObj => Triage;
		public int ConfirmCount { get; }
		public int InvestigateCount { get; }
		public int NegateCount { get; }
		public override ZBool IsFocused { get; }
		static bool AreEquivalentProducts(ZString firstProduct, ZString secondProduct)
		{
			return firstProduct.IsEmpty || secondProduct.IsEmpty || firstProduct.EqualsIgnoringCase(secondProduct);
			// If either product is empty, they are considered equivalent
		}
	}

	public class TriageAssistTreeRelevantCriteriaWrapper : TriageAssistTreeBizObjWrapper
	{
		public TriageAssistTreeRelevantCriteriaWrapper(RelevantDiagnosticCriteria criteria)
		{
			RelevantCriteria = criteria;
		}
		public RelevantDiagnosticCriteria RelevantCriteria { get; }
		public override ZString Description => RelevantCriteria.DiagnosticCriteria.IMD_Description;
		public override ZString Type => RelevantCriteria.DiagnosticCriteria.TypeDescription;
		public override ZString Status => RelevantCriteria.Confirm ? "Confirm" : RelevantCriteria.Investigate ? "Investigate" : RelevantCriteria.Negate ? "Negate" : "";
		public override BusinessObject BizObj => RelevantCriteria.DiagnosticCriteria;
	}
}
