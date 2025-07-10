using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentDiagnosticCriteriaTriageModuleButtonGrid : ZModuleButtonGrid
	{
		IncidentDiagnosticCriteria DiagnosticCriteria => (IncidentDiagnosticCriteria)Form.BusinessEntity;

		public IncidentDiagnosticCriteriaTriageModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.IncidentTriage;
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((IncidentTriageDiagnosticCriteriaPivot)selected).Triage);
		}

		protected override void Detach(BusinessObject selected)
		{
			selected?.Delete();
			base.Detach(selected);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new IncidentDiagnosticCriteriaGridAttacher(destinationCollection, findBoxList, moduleID, DiagnosticCriteria);
		}

		#region Attacher

		/// <summary>
		/// Since our Grid List is bounded to IncidentTriageDiagnosticCriteriaPivot
		/// We needed to create a class to override some of the functions in ZRecordAttacher
		/// </summary>
		internal class IncidentDiagnosticCriteriaGridAttacher : ZRecordAttacher
		{
			readonly IncidentDiagnosticCriteria DiagnosticCriteria;

			public IncidentDiagnosticCriteriaGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentDiagnosticCriteria diagnosticCriteria)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.DiagnosticCriteria = diagnosticCriteria;
			}

			protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
			{
				var loadedIncidentTriage = bizO.Factory.Load<IncidentTriage>(bizO.PK);
				if (loadedIncidentTriage != null && !loadedIncidentTriage.IsDeleted)
				{
					DiagnosticCriteria.IncidentTriagePivots.Reload(true);
					var pivot = DiagnosticCriteria.Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
					pivot.IMO_IMT_Triage = loadedIncidentTriage.PK;
					pivot.IMO_IMD_DiagnosticCriteria = DiagnosticCriteria.PK;
					listToBulkAdd.Add(pivot);
					return true;
				}

				return false;
			}

			protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
			{
				return ((IncidentTriageDiagnosticCriteriaPivot)bizObj).IMO_IMT_Triage;
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
