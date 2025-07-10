using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentTriageModuleButtonGrid : ZModuleButtonGrid
	{
		IncidentTriage IncidentTriage => (IncidentTriage)Form.BusinessEntity;

		public IncidentTriageModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.IncidentTriageChecklistItem;
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((IncidentTriageChecklistItemPivot)selected).ChecklistItem);
		}

		protected override void Detach(BusinessObject selected)
		{
			OnPivotDetached((IncidentTriageChecklistItemPivot)selected);
			base.Detach(selected);
		}

		void OnPivotDetached(IncidentTriageChecklistItemPivot pivot)
		{
			var triage = pivot?.Triage;
			var checklistItem = pivot?.ChecklistItem;
			if (triage != null && checklistItem != null)
			{
				PostDTCEvent(pivot);
			}
			var currentSeq = pivot.IMP_Sequence;
			pivot?.Delete();

			var pivots = IncidentTriage.ChecklistPivots;
			if (pivots != null && pivots.Count > 0)
			{
				var sequenceNeedDecrementByOnes = pivots.Cast<IncidentTriageChecklistItemPivot>().Where(x => x.IMP_Sequence > currentSeq).ToArray();
				foreach (var item in sequenceNeedDecrementByOnes)
				{
					item.IMP_Sequence -= 1;
				}
			}
		}

		static string GetDTCEventReference(IncidentTriageChecklistItemPivot pivot)
		{
			return GetBaseAttachAndDetachEventReference(pivot, $"Checklist item {pivot.ChecklistItem.IMC_ChecklistNumber} removed from Incident Triage {pivot.Triage.IMT_TriageNumber}");
		}

		static string GetATCEventReference(IncidentTriageChecklistItemPivot pivot)
		{
			return GetBaseAttachAndDetachEventReference(pivot, $"Checklist item {pivot.ChecklistItem.IMC_ChecklistNumber} added for Incident Triage {pivot.Triage.IMT_TriageNumber}");
		}

		static void PostDTCEvent(IncidentTriageChecklistItemPivot link)
		{
			var reference = GetDTCEventReference(link);
			link.Triage.Logs.AddNew(AutoEvents.Detached, reference);
			link.ChecklistItem.Logs.AddNew(AutoEvents.Detached, reference);
		}

		static void PostATCEvent(IncidentTriageChecklistItemPivot link)
		{
			var reference = GetATCEventReference(link);
			link.Triage.Logs.AddNew(AutoEvents.Attached, reference);
			link.ChecklistItem.Logs.AddNew(AutoEvents.Attached, reference);
		}

		static string GetBaseAttachAndDetachEventReference(IncidentTriageChecklistItemPivot pivot, string description)
		{
			return EventLogReferenceBuilder.New()
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, description)
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, pivot.ChecklistItem.IMC_ChecklistNumber)
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, pivot.Triage.IMT_TriageNumber).Build();
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new IncidentTriageGridAttacher(destinationCollection, findBoxList, moduleID, IncidentTriage);
		}

		#region Attacher

		/// <summary>
		/// Since our Grid List is bounded to IncidentTriageChecklistItemPivot But DataBinding is for IncidentTriageChecklistItem
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
				var loadedIncident = bizO.Factory.Load<IncidentTriageChecklistItem>(bizO.PK);
				if (loadedIncident != null && !loadedIncident.IsDeleted)
				{
					incidentTriage.ChecklistPivots.Reload(true);
					var pivot = incidentTriage.Factory.New<IncidentTriageChecklistItemPivot>();
					pivot.IMP_IMC_ChecklistItem = loadedIncident.PK;
					pivot.IMP_IMT_Triage = incidentTriage.PK;
					var pivotNum = incidentTriage.ChecklistPivots.Cast<IncidentTriageChecklistItemPivot>().Select(x => x.IMP_Sequence);
					var maxSeq = !pivotNum.Any() ? 1 : pivotNum.Max() + 1;
					pivot.IMP_Sequence = (ZShort)maxSeq;
					listToBulkAdd.Add(pivot);
					PostATCEvent(pivot);
					return true;
				}

				return false;
			}

			protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
			{
				return ((IncidentTriageChecklistItemPivot)bizObj).IMP_IMC_ChecklistItem;
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
