//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentTriageChecklistItemPivotValidation
//
//    This class should be used for overriding validation in AutoIncidentTriageChecklistItemPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageChecklistItemPivotValidation : AutoIncidentTriageChecklistItemPivotValidation
	{
		public IncidentTriageChecklistItemPivotValidation(AutoIncidentTriageChecklistItemPivot parent) : base(parent)
		{
		}

		new IncidentTriageChecklistItemPivot Parent
		{
			get { return (IncidentTriageChecklistItemPivot)base.Parent; }
		}

		protected override void CheckIMP_IMC_ChecklistItem()
		{
			base.CheckIMP_IMC_ChecklistItem();

			if (Parent.Triage != null)
			{
				if (Parent.Triage.ChecklistPivots.Cast<IncidentTriageChecklistItemPivot>().Count(x => x.IMP_IMC_ChecklistItem == Parent.IMP_IMC_ChecklistItem) > 1)
				{
					Parent.IMP_IMC_ChecklistItemInfo.AddError(Res.GetString("db63ee13-4548-4a62-8b27-ad1c39de4f07", "Cannot add the same checklist item to a triage twice."));
				}
			}
		}

		protected override void CheckIMP_Sequence()
		{
			var sequenceErrorMes = Res.GetString("d64f58cf-ed7a-4b16-a2ac-04022e862261", "Numbers must be sequential from 1 and increasing by 1.");

			base.CheckIMP_Sequence();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.IMP_SequenceInfo, 1);
			if (Parent.Triage != null)
			{
				Parent.Triage.RemoveRowError(sequenceErrorMes);
				var arr = Parent.Triage.ChecklistPivots.Cast<IncidentTriageChecklistItemPivot>().Select(x => x.IMP_Sequence).ToArray();
				if (arr.Length < 2)
				{
					if (arr.Length == 1 && arr[0] != 1)
					{
						Parent.Triage.AddRowError(sequenceErrorMes);
					}
					return;
				}
				Array.Sort(arr);
				if (arr[0] != 1)
				{
					Parent.Triage.AddRowError(sequenceErrorMes);
				}
				for (int i = 1; i < arr.Length; i++)
				{
					if (arr[i] - arr[i - 1] != 1)
					{
						Parent.Triage.AddRowError(sequenceErrorMes);
					}
				}
			}
		}
	}
}
