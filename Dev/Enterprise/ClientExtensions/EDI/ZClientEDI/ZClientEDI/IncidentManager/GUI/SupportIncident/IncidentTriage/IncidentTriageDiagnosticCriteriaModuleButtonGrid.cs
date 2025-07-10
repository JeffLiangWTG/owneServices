using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentTriageDiagnosticCriteriaModuleButtonGrid : ZModuleButtonGrid
	{
		IncidentTriage IncidentTriage => (IncidentTriage)Form.BusinessEntity;

		public IncidentTriageDiagnosticCriteriaModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.IncidentDiagnosticCriteria;
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((IncidentTriageDiagnosticCriteriaPivot)selected).DiagnosticCriteria);
		}

		protected override void Detach(BusinessObject selected)
		{
			selected?.Delete();
			base.Detach(selected);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new IncidentTriageGridAttacher(destinationCollection, findBoxList, moduleID, IncidentTriage);
		}

		#region Attacher

		/// <summary>
		/// Since our Grid List is bounded to IncidentTriageDiagnosticCriteriaPivot But DataBinding is for IncidentTriageDiagnosticCriteria
		/// We needed to create a class to override some of the functions in ZRecordAttacher
		/// </summary>
		internal class IncidentTriageGridAttacher : ZRecordAttacher
		{
			readonly IncidentTriage incidentTriage;

			public IncidentTriageGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentTriage incidentTriage)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.incidentTriage = incidentTriage;
			}

			protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
			{
				var loadedDiagnosticCriteria = bizO.Factory.Load<IncidentDiagnosticCriteria>(bizO.PK);
				if (loadedDiagnosticCriteria != null && !loadedDiagnosticCriteria.IsDeleted)
				{
					incidentTriage.DiagnosticCriteriaPivots.Reload(true);
					var pivot = incidentTriage.Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
					pivot.IMO_IMD_DiagnosticCriteria = loadedDiagnosticCriteria.PK;
					pivot.IMO_IMT_Triage = incidentTriage.PK;
					listToBulkAdd.Add(pivot);
					return true;
				}

				return false;
			}

			protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
			{
				return ((IncidentTriageDiagnosticCriteriaPivot)bizObj).IMO_IMD_DiagnosticCriteria;
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
