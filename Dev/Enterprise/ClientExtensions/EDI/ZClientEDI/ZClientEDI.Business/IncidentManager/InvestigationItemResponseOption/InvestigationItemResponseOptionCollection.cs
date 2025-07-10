using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class InvestigationItemResponseOptionCollection : DependentBusinessObjectCollection<InvestigationItemResponseOption, InvestigationItem>
	{
		public InvestigationItemResponseOptionCollection(InvestigationItem investigationItem, BusinessObjectFactory factory)
			: base(investigationItem, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => InvestigationItemResponseOptionSchema.INR_INV_InvestigationItem;

		protected override bool AllowNewCore => true;

		protected override bool AllowRemoveCore => true;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((InvestigationItemResponseOption)child).INR_Sequence = GetNextSequenceNumber();
		}

		ZShort GetNextSequenceNumber()
		{
			var nextSequenceNumber = new ZShort(1);
			if (this.Count != 0)
			{
				nextSequenceNumber = this.OfType<InvestigationItemResponseOption>().OrderBy(x => x.INR_Sequence).LastOrDefault().INR_Sequence + 1;
			}
			return nextSequenceNumber;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var option = (InvestigationItemResponseOption)bizOAdded;
			var diagnosticCriteriaInvestigationItemLinks = option.InvestigationItem.DiagnosticCriteriaPivots;
			if (diagnosticCriteriaInvestigationItemLinks != null && diagnosticCriteriaInvestigationItemLinks.Count != 0)
			{
				foreach (var link in diagnosticCriteriaInvestigationItemLinks)
				{
					var itemLink = link as DiagnosticCriteriaInvestigationItemLink;

					if (!itemLink.InvestigationResultPivots.Where(result => result.INR_Sequence == option.INR_Sequence).Any())
					{
						var result = itemLink.InvestigationResultPivots.AddNew();
						result.DCR_DIL_ParentLink = link.PK;
						result.DCR_INR_ResponseOption = option.PK;
					}
				}
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			var option = (InvestigationItemResponseOption)bizO;
			if (option.InvestigationItem == null)
			{
				return;
			}
			var diagnosticCriteriaInvestigationItemLinks = option.InvestigationItem.DiagnosticCriteriaPivots;
			if (diagnosticCriteriaInvestigationItemLinks != null && diagnosticCriteriaInvestigationItemLinks.Count != 0)
			{
				foreach (var link in diagnosticCriteriaInvestigationItemLinks)
				{
					var itemLink = link as DiagnosticCriteriaInvestigationItemLink;

					var resultsToRemove = itemLink.InvestigationResultPivots
					.Where(result => result.DCR_INR_ResponseOption == option.PK)
					.ToList();
					foreach (var result in resultsToRemove)
					{
						itemLink.InvestigationResultPivots.RemoveAndDelete(result);
					}
				}
			}
			base.OnRemoving(bizO);
		}
	}
}
