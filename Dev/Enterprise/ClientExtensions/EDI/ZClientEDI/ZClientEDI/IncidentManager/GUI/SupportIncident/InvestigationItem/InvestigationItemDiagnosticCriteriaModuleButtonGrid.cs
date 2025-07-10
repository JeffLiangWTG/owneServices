using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class InvestigationItemDiagnosticCriteriaModuleButtonGrid : ZModuleButtonGrid
	{
		InvestigationItem InvestigationItem => (InvestigationItem)Form.BusinessEntity;

		public InvestigationItemDiagnosticCriteriaModuleButtonGrid()
		{
			if (!DesignMode)
			{
				ModuleID = ClientModuleRegistration.IncidentDiagnosticCriteria;
			}
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((DiagnosticCriteriaInvestigationItemLink)selected).IncidentDiagnosticCriteria);
		}

		protected override void Detach(BusinessObject selected)
		{
			var resultPivots = ((DiagnosticCriteriaInvestigationItemLink)selected).InvestigationResultPivots.ToList();
			resultPivots?.ForEach(p => p.Delete());
			selected?.Delete();
			base.Detach(selected);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new InvestigationItemGridAttacher(destinationCollection, findBoxList, moduleID, InvestigationItem);
		}

		#region Attacher

		/// <summary>
		/// Since our Grid List is bounded to IncidentTriageDiagnosticCriteriaPivot
		/// We needed to create a class to override some of the functions in ZRecordAttacher
		/// </summary>
		internal class InvestigationItemGridAttacher : ZRecordAttacher
		{
			readonly InvestigationItem InvestigationItem;

			public InvestigationItemGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, InvestigationItem investigationItem)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.InvestigationItem = investigationItem;
			}

			protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
			{
				var loadedDiagnosticCriteria = bizO.Factory.Load<IncidentDiagnosticCriteria>(bizO.PK);
				if (loadedDiagnosticCriteria != null && !loadedDiagnosticCriteria.IsDeleted)
				{
					InvestigationItem.DiagnosticCriteriaPivots.Reload(true);
					var pivot = InvestigationItem.Factory.New<DiagnosticCriteriaInvestigationItemLink>();
					pivot.DIL_IMD_DiagnosticCriteria = loadedDiagnosticCriteria.PK;
					pivot.DIL_INV_InvestigationItem = InvestigationItem.PK;
					listToBulkAdd.Add(pivot);

					var optons = InvestigationItem.Factory.Load<InvestigationItemResponseOption>(new ZQuery(InvestigationItemResponseOptionSchema.INR_INV_InvestigationItem, InvestigationItem.PK));
					if (optons != null && optons.Length != 0)
					{
						foreach (var option in optons)
						{
							var result = InvestigationItem.Factory.New<DiagnosticCriteriaInvestigationResult>();
							result.DCR_DIL_ParentLink = pivot.PK;
							result.DCR_INR_ResponseOption = option.PK;
							result.DCR_ResponseResult = DiagnosticCriteriaInvestigationResults.Codes.Null;
						}
					}

					return true;
				}

				return false;
			}

			protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
			{
				return ((DiagnosticCriteriaInvestigationItemLink)bizObj).DIL_IMD_DiagnosticCriteria;
			}
		}

		#endregion

		public override void Refresh()
		{
			InnerGrid?.ListManager?.Refresh();
			base.Refresh();
		}
	}
}
