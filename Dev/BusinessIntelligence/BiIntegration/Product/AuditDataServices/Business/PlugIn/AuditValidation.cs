namespace Enterprise.AuditDataServices.Business
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Registry.Business;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
	public class AuditValidation : ZValidation
	{
		public AuditValidation(Audit parent) : base(parent)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(Audit); }
		}

		public override void ValidateAll()
		{
			ValidateFilters();
		}

		Audit ParentAudit
		{
			get { return (Audit)ParentFilter; }
		}

		void ValidateFilters()
		{
			ValidateFilterTimeLocalFrom();
			ValidateFilterTimeLocalTo();
			ValidateFilterSourceEntity();
		}

		public void ValidateFilterTimeLocalFrom()
		{
			ValidateCalculatedProperty(ParentAudit.FilterTimeLocalFromInfo);
		}

		void CheckFilterTimeLocalFrom()
		{
			CheckFilterTimePropertyIsValid(ParentAudit.FilterTimeLocalFromInfo);
			CheckFilterTimeRange(ParentAudit.FilterTimeLocalFromInfo, ParentAudit.FilterTimeLocalFrom, ParentAudit.FilterTimeLocalTo);
		}

		public void ValidateFilterTimeLocalTo()
		{
			ValidateCalculatedProperty(ParentAudit.FilterTimeLocalToInfo);
		}

		void CheckFilterTimeLocalTo()
		{
			CheckFilterTimePropertyIsValid(ParentAudit.FilterTimeLocalToInfo);
			CheckFilterTimeRange(ParentAudit.FilterTimeLocalToInfo, ParentAudit.FilterTimeLocalFrom, ParentAudit.FilterTimeLocalTo);
		}

		void CheckFilterTimePropertyIsValid(ZPropertyInfo ptyInfo)
		{
			ZDateTime ptyValue = (ZDateTime)ptyInfo.Value;

			if (ptyValue.IsEmpty || !ptyValue.IsValid)
			{
				ptyInfo.AddError(Res.GetString("9ECA3A5A-DEAF-4DFD-80D7-680C85176302", "Please enter a valid date."));
			}
			else if (ptyValue.Year < 2000)
			{
				ptyInfo.AddError(Res.GetString("A7C82F7B-4197-40DB-8430-CCB5CB62FD2A", "Year must be equal or greater than 2000."));
			}
		}

		void CheckFilterTimeRange(ZPropertyInfo ptyInfo, ZDateTime timeFrom, ZDateTime timeTo)
		{
			if (timeFrom.IsValid && timeTo.IsValid)
			{
				if (timeFrom > timeTo)
				{
					ptyInfo.AddError(Res.GetString("2A0A849D-1D4D-40A0-A372-8B4FDE2B65CE", "Date From must be older than Date To."));
				}
				else if (timeFrom < ZDateTime.Now.AddMonths(-SystemDataRegistry.Instance.AuditRetentionPeriod.Value))
				{
					ptyInfo.AddError(Res.GetString("480B7315-DD25-4FBE-B4E6-7D89A0B0DA6F", $"Date range must not exceed audit data retention period of {SystemDataRegistry.Instance.AuditRetentionPeriod.Value} months."));
				}
			}
		}

		public void ValidateFilterSourceEntity()
		{
			ValidateCalculatedProperty(ParentAudit.FilterSourceEntityInfo);
		}

		void CheckFilterSourceEntity()
		{
			MandatoryValidation.CheckEntered(ParentAudit.FilterSourceEntityInfo);

			if (
				!string.IsNullOrWhiteSpace(ParentAudit.FilterSourceEntity)
				&& !ParentAudit.SourceEntities.Any(st => st.Code == ParentAudit.FilterSourceEntity))
			{
				ParentAudit.FilterSourceEntityInfo.AddError(Res.GetString("B2F0319E-F47B-4016-A5FA-77783ACE4835", "Please select a valid source."));
			}
		}
	}
}
