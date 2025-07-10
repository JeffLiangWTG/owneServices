using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceTaskBranchValidationAdapter : StmServiceTask, IStmServiceTaskBranchValidationAdapter
	{
		public StmServiceTaskBranchValidationAdapter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (GlbBranch.CurrentBranch != null)
			{
				S5_GB = GlbBranch.CurrentBranch.PK;
				S5_IsActive = true;
			}
		}

		public ZGuid S5_GB { get => SST_GB_Branch; set => SST_GB_Branch = value; }

		public ZString S5_ParentTableCode {
			get => Constants.ServiceTask.ParentTableCode;
			set { }
		}

		public ZString S5_ScheduleDescription
		{
			get => Description;
			set { }
		}

		public ZString S5_ScheduleType
		{
			get => SST_ServiceTaskCode;
			set { }
		}

		public ZBool S5_IsActive { get => SST_Active; set => SST_Active = value; }

		public ZDateTime S5_NextScheduledPrintRunTimeUtc {
			get => NextRunTime;
			set { }
		}
	}
}
