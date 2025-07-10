using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class RelevantDiagnosticCriteria : AutoRelevantDiagnosticCriteria
	{
		public RelevantDiagnosticCriteria(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RelevantDiagnosticCriteria(IncidentDiagnosticCriteria diagnosticCriteria, TriageAssistBusinessObject parent) : base(diagnosticCriteria.Factory)
		{
			DiagnosticCriteria = diagnosticCriteria;
			Parent = parent;
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}
		}

		public IncidentDiagnosticCriteria DiagnosticCriteria { get; }

		TriageAssistBusinessObject Parent { get; }

		protected override ZGuid GetPK() => DiagnosticCriteria?.PK ?? ZGuid.Empty;

		protected override void SetPKAndDefaults()
		{
		}

		public override ZInt Sequence
		{
			get => base.Sequence;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.Sequence = value;
				}
			}
		}

		public override ZBool Confirm
		{
			get => base.Confirm;
			set
			{
				if (value)
				{
					using (Parent.OnPropertyValueChangedSuspender.Suspend())
					{
						Negate = Investigate = false;
					}
				}
				base.Confirm = value;
			}
		}

		public override ZBool Negate
		{
			get => base.Negate;
			set
			{
				if (value)
				{
					using (Parent.OnPropertyValueChangedSuspender.Suspend())
					{
						Confirm = Investigate = false;
					}
				}
				base.Negate = value;
			}
		}

		public override ZBool Investigate
		{
			get => base.Investigate;
			set
			{
				if (value)
				{
					using (Parent.OnPropertyValueChangedSuspender.Suspend())
					{
						Confirm = Negate = false;
					}
				}
				base.Investigate = value;
			}
		}

		protected override void OnFactorySaving()
		{
			if (Parent?.Parent != null && HasChanges)
			{
				var assistParentPK = Parent.Parent.PK;
				var query = new ZQuery(IncidentDiagnosticCriteriaPivotSchema.IMV_IMD_DiagnosticCriteria, DiagnosticCriteria.PK);
				query.AddToFilter(IncidentDiagnosticCriteriaPivotSchema.IMV_ParentID, assistParentPK);
				var pivot = Factory.LoadTop1<IncidentDiagnosticCriteriaPivot>(query);

				if (Confirm || Investigate || Negate)
				{
					if (pivot == null || pivot.IsDeleted || pivot.IsDeleting)
					{
						pivot = Factory.New<IncidentDiagnosticCriteriaPivot>();
						pivot.IMV_IMD_DiagnosticCriteria = DiagnosticCriteria.PK;
						pivot.LinkParent(Parent.Parent);
					}

					if (Confirm)
					{
						pivot.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;
					}
					else if (Negate)
					{
						pivot.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Negate;
					}
					else if (Investigate)
					{
						pivot.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Investigate;
					}
				}
				else
				{
					if (pivot != null && !pivot.IsDeleted && !pivot.IsDeleting)
					{
						pivot.Delete();
					}
				}
			}
 
			base.OnFactorySaving();
		}

		public class CriteriaComparer : IComparer<RelevantDiagnosticCriteria>
		{
			//Confirm >> Investigate >> Negate
			public int Compare(RelevantDiagnosticCriteria x, RelevantDiagnosticCriteria y)
			{
				return (x.Investigate ? 1 : 0 + (x.Negate ? 2 : 0)) - (y.Investigate ? 1 : 0 + (y.Negate ? 2 : 0));
			}
		}
	}
}
