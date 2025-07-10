using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmActivityLog : AutoStmActivityLog
	{
		#region Schema

		public abstract new class Schema : AutoStmActivityLog.Schema
		{
			public const string S7_OpenDateTime = "S7_OpenDateTime";
			public const string S7_CloseDateTime = "S7_CloseDateTime";
		}

		#endregion

		public StmActivityLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S7_OpenDateTimeUtc = ZDateTime.UtcNow;
		}

		#region Populate from Stats

		public void PopulateFromStatistics(FormUserStatistics log)
		{
			if (log != null && log.ShownDateTimeUtc != DateTime.MinValue)
			{
				if (!log.IsClosed)
				{
					log.NotifyFormClosed(Guid.Empty, string.Empty);
				}

				if (log.FormCaption != null)
				{
					S7_FormCaption = ((ZString)log.FormCaption).SubstringSafe(0, StmActivityLogSchema.S7_FormCaption.MaxLength);
				}

				S7_KeyStrokes = log.KeyPresses;
				S7_MouseClicks = log.MouseClicks;
				S7_ControlChanges = log.ControlFocusChanges;

				try
				{
					S7_ActiveTime = Convert.ToInt32(log.ActiveDuration.TotalSeconds);
					S7_InactiveTime = Convert.ToInt32(log.InactiveDuration.TotalSeconds);
				}
				catch (OverflowException) { }

				if (log.ModuleName != null)
				{
					S7_ControllerID = ((ZString)log.ModuleName).SubstringSafe(0, StmActivityLogSchema.S7_ControllerID.MaxLength);
				}

				S7_OpenDateTimeUtc = log.ShownDateTimeUtc;
				S7_CloseDateTimeUtc = log.CloseDateTimeUtc;
				S7_GS_NKUser = EnvProxy.Instance.CurrentUser.Initials;
				S7_ParentID = log.BusinessObjectPK;
				S7_ParentTableCode = log.BusinessObjectTableCode;
				S7_EnterpriseActivity = !log.IsExternalProcess;
			}
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Properties

		#region Active Duration

		public ZDecimal ActiveDurationMinutes
		{
			get { return Enterprise.ZArchitecture.Core.Utilities.Round(S7_ActiveTime / 60m, 1); }
		}

		public ZPropertyInfo ActiveDurationMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(ActiveDurationMinutes)); }
		}

		#endregion

		#region Inactive Duration

		public ZDecimal InactiveDurationMinutes
		{
			get { return Enterprise.ZArchitecture.Core.Utilities.Round(S7_InactiveTime / 60m, 1); }
		}

		public ZPropertyInfo InactiveDurationMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(InactiveDurationMinutes)); }
		}

		#endregion

		#region Related Business Object Name

		public ZString RelatedBusinessObjectButtonText
		{
			get
			{
				string recordName = "";
				if (!S7_ParentTableCode.IsEmpty && !S7_ControllerID.IsEmpty)
				{
					ITableSchema schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(S7_ParentTableCode);

					if (schema != null)
					{
						recordName = DataBoundResourceStrings.GetTableDescriptiveName(schema.TableName);
					}
				}
				return string.IsNullOrEmpty(recordName) ? Res.GetString("93c96799-1e7f-4ed4-9853-6ab7c571b84a", "Open Record") : Res.GetString("433d0877-df96-4fe1-9529-f77ef34e975c", "Open {0} Record", recordName);
			}
		}

		public ZPropertyInfo RelatedBusinessObjectButtonTextInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedBusinessObjectButtonText)); }
		}

		#endregion

		#region UserFullName

		public ZString UserFullName
		{
			get { return User != null ? User.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo UserFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(UserFullName)); }
		}

		#endregion

		#region S7_OpenDateTime

		public ZDateTime S7_OpenDateTime
		{
			get
			{
				var env = EnvProxy.Instance;
				ZDateTime result = (S7_OpenDateTimeUtc.IsValid) ? env.Time.GetLocalTimeFromUtc(S7_OpenDateTimeUtc.ToDateTime()) : ZDateTime.Empty;
				return result;
			}
		}

		public ZPropertyInfo S7_OpenDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.S7_OpenDateTime); }
		}

		#endregion

		#region S7_CloseDateTime

		public ZDateTime S7_CloseDateTime
		{
			get
			{
				var env = EnvProxy.Instance;
				ZDateTime result = (S7_CloseDateTimeUtc.IsValid) ? env.Time.GetLocalTimeFromUtc(S7_CloseDateTimeUtc.ToDateTime()) : ZDateTime.Empty;
				return result;
			}
		}

		public ZPropertyInfo S7_CloseDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.S7_CloseDateTime); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public IGlbStaff User
		{
			get { return (IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, S7_GS_NKUser); }
		}

		#endregion
	}
}
