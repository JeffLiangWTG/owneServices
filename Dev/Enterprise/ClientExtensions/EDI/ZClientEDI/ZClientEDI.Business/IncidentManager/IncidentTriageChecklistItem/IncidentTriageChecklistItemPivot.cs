using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[DependentBusinessObject(typeof(IncidentTriage), "ChecklistPivots")]
	public class IncidentTriageChecklistItemPivot : AutoIncidentTriageChecklistItemPivot
	{
		public IncidentTriageChecklistItemPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IncidentTriage Triage
		{
			get
			{
				if (triage == null && !IMP_IMT_Triage.IsEmpty)
				{
					triage = Factory.LoadTop1<IncidentTriage>(new ZQuery(IncidentTriageSchema.PK, IMP_IMT_Triage));
				}
				return triage;
			}

			private set
			{
				triage = value;
			}
		}

		IncidentTriage triage;

		public IncidentTriageChecklistItem ChecklistItem
		{
			get
			{
				if (checklistItem == null && !IMP_IMC_ChecklistItem.IsEmpty)
				{
					checklistItem = Factory.LoadTop1<IncidentTriageChecklistItem>(new ZQuery(IncidentTriageChecklistItemSchema.PK, IMP_IMC_ChecklistItem));
				}
				return checklistItem;
			}

			private set
			{
				checklistItem = value;
			}
		}

		IncidentTriageChecklistItem checklistItem;

		public override ZGuid IMP_IMT_Triage
		{
			get => base.IMP_IMT_Triage;
			set
			{
				base.IMP_IMT_Triage = value;
				Triage = null;
			}
		}

		[ResourceStringData("IncidentTriageChecklistItemPivot|IMP_Sequence", Caption = "Sequence", ShortCaption = "Seq.")]
		public override ZShort IMP_Sequence
		{
			get => base.IMP_Sequence;
			set
			{
				if (base.IMP_Sequence != value)
				{
					if (Triage != null)
					{
						Triage.HasChanges = true;
					}
					base.IMP_Sequence = value;
					IMP_SequenceInfo.RefreshBinding();
					Validation.ValidateIMP_Sequence();
				}
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			IMP_IMT_Triage = triage.PK;
			var checklistItem = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			IMP_IMC_ChecklistItem = checklistItem.PK;
		}

#endif
	}
}
