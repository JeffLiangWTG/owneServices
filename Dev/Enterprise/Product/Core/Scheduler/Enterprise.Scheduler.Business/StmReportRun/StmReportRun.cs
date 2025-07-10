using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	[CodeProperty(Schema.RRI_ReportName)]
	public class StmReportRun : AutoStmReportRun, IDocManagerSupport
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public StmReportRun(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.IsPersistent)
				{
					ConcurrencyInfo.SetConcurrencyPolicy(this, info.Name, ConcurrencyPolicy.Ignore);
				}
			}
		}

		public static CodeDescriptionPairList StmReportRunStatus => new CodeDescriptionPairList(OLookUpEditType.ReportStatisticsStatus);

		public ZDateTime Duration
		{
			get
			{
				if (RRI_TotalRunDurationMilliseconds > 0)
				{
					return TimeSpan.FromMilliseconds(RRI_TotalRunDurationMilliseconds);
				}
				if (!RRI_StartTimeUtc.IsValid || !RRI_EndTimeUtc.IsValid)
				{
					return TimeSpan.Zero;
				}
				return RRI_EndTimeUtc - RRI_StartTimeUtc;
			}
		}

		public ZPropertyInfo DurationInfo => GetZPropertyInfo(nameof(Duration));

		public ZDateTime QueueDuration
		{
			get
			{
				if (!RRI_StartTimeInQueueUtc.IsValid || !RRI_StartTimeUtc.IsValid)
				{
					return TimeSpan.Zero;
				}
				return RRI_StartTimeUtc - RRI_StartTimeInQueueUtc;
			}
		}

		public ZPropertyInfo QueueDurationInfo => GetZPropertyInfo(nameof(QueueDuration));

		public ZDateTime ClientCPUDuration
		{
			get
			{
				return TimeSpan.FromMilliseconds(RRI_ClientCPUDurationMilliseconds);
			}
		}

		public ZPropertyInfo ClientCPUDurationInfo => GetZPropertyInfo(nameof(ClientCPUDuration));

		public ZDateTime ClientRunDuration
		{
			get
			{
				return TimeSpan.FromMilliseconds(RRI_ClientRunDurationMilliseconds);
			}
		}

		public ZPropertyInfo ClientRunDurationInfo => GetZPropertyInfo(nameof(ClientRunDuration));

		public ZDateTime SQLCPUDuration
		{
			get
			{
				return TimeSpan.FromMilliseconds(RRI_SQLCPUDurationMilliseconds);
			}
		}

		public ZPropertyInfo SQLCPUDurationInfo => GetZPropertyInfo(nameof(SQLCPUDuration));

		public ZDateTime SQLRunDuration
		{
			get
			{
				return TimeSpan.FromMilliseconds(RRI_SQLRunDurationMilliseconds);
			}
		}

		public ZPropertyInfo SQLRunDurationInfo => GetZPropertyInfo(nameof(SQLRunDuration));

		public ZDateTime TimeBeingProcessed
		{
			get
			{
				if (!RRI_StartTimeUtc.IsValid)
				{
					return TimeSpan.Zero;
				}

				return ZDateTime.UtcNow - RRI_StartTimeUtc;
			}
		}

		public ZPropertyInfo TimeBeingProcessedInfo => GetZPropertyInfo(nameof(TimeBeingProcessed));

		protected ZDateTime GetLocalDate(ZDateTime utcDate)
		{
			if (utcDate.IsEmpty || !utcDate.IsValid)
			{
				return utcDate;
			}

			return Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime());
		}

		public ZDateTime StartTimeLocal => GetLocalDate(RRI_StartTimeUtc);

		public ZPropertyInfo StartTimeLocalInfo => GetZPropertyInfo(nameof(StartTimeLocal));

		public ZDateTime EndTimeLocal => GetLocalDate(RRI_EndTimeUtc);

		public ZPropertyInfo EndTimeLocalInfo => GetZPropertyInfo(nameof(EndTimeLocal));

		[List("Lookups.AllStaff")]
		public ZGuid UserFK
		{
			get
			{
				if (userFK == null)
				{
					if (!string.IsNullOrEmpty(RRI_GS_NKPrintUser))
					{
						GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, RRI_GS_NKPrintUser);
						if (staff != null)
						{
							userFK = staff.PK;
						}
					}
				}
				return userFK ?? ZGuid.Empty;
			}
		}
		ZGuid? userFK;

		public ZPropertyInfo UserFKInfo => GetZPropertyInfo(nameof(UserFK));

		[List("Lookups.StmReportRunStatus")]
		public override ZString RRI_Status
		{
			get => base.RRI_Status;
			set => base.RRI_Status = value;
		}

		public static string PreviewTaskDescription => (NoResString)"Preview Task";
		
		public ZBool IsPreview => RRI_S5_Schedule == ZGuid.Empty && RRI_ReportDescription == PreviewTaskDescription;

		public ZPropertyInfo IsPreviewInfo => GetZPropertyInfo(nameof(IsPreview));

		protected override ZString HumanReadableNameCore => RRI_ReportName + ": " + RRI_ReportDescription;

		public StmScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					scheduleTask = Factory.Load<StmScheduleTask>(RRI_S5_Schedule);
				}
				return scheduleTask;
			}
		}
		StmScheduleTask scheduleTask;

		#region eDocs

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ReportStatistic)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		public override void Delete()
		{
			DeleteStorageMain();
			base.Delete();
		}

		void DeleteStorageMain()
		{
			var docFactory = ((IDocManagerSupport)this).DocManagerInfo.MasterFactory;
			var storageMain = docFactory.GetStorageMainForPK(PK) as BusinessObject;
			if (storageMain != null)
			{
				storageMain.Delete();
				((IDocManagerSupport)this).DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
			}
		}
	}
}
