using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskValidation : StmScheduleTaskValidation
	{
		public ReportScheduleTaskValidation(ReportScheduleTask parent)
			: base(parent)
		{
		}

		protected new ReportScheduleTask Parent
		{
			get { return (ReportScheduleTask)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUserFK();
		}

		protected override void CheckS5_NextScheduledPrintRunTimeUtc()
		{
			base.CheckS5_NextScheduledPrintRunTimeUtc();
			if (Parent.S5_IsPrivate)
			{
				MandatoryValidation.CheckEntered(Parent.S5_NextScheduledPrintRunTimeUtcInfo);
			}
			var maxActiveScheduledReportsWarningThreshold = SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.Value;
			if (!Parent.S5_NextScheduledPrintRunTimeUtc.IsEmpty && maxActiveScheduledReportsWarningThreshold > 0)
			{
				var query = new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, ReportScheduleTask.ScheduleType);
				query.AddToFilter(new DateQueryBuilder().CreateDateTimeRange(
					DateComparisonOperator.HasDateInRange,
					StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc,
					Parent.S5_NextScheduledPrintRunTimeUtc.AddMinutes(-10),
					Parent.S5_NextScheduledPrintRunTimeUtc.AddMinutes(10),
					lowerDatePartOnly: false,
					upperDatePartOnly: false));

				var scheduledReports = Parent.Factory.Load<ReportScheduleTask>(query).Length;
				var scheduledReportsInDatabase = Parent.IsInDatabase ? scheduledReports : scheduledReports - 1;

				if (scheduledReports > maxActiveScheduledReportsWarningThreshold)
				{
					Parent.S5_NextScheduledPrintRunTimeUtcInfo.AddWarning(Res.GetString("79ef8ba7-d046-4179-8678-96148a1fc9b9", "There are currently {0} reports scheduled for processing at this time which potentially can lead to delays in the processing of this report.", scheduledReportsInDatabase));
				}
			}
		}

		protected override void CheckS5_ParentID()
		{
			MandatoryValidation.CheckEntered(Parent.S5_ParentIDInfo);

			if (Parent.S5_ParentID.IsValid)
			{
				var command = Parent.Factory.Load<ReportCommand>(Parent.S5_ParentID);
				if (command != null)
				{
					var possibleModule = ModuleTree.Tree.FindByID(command.SU_BusinessContext.RemoveSafe(0, 3));
					var parentSecurityCheckPoint = possibleModule?.SecurityCheckpoint;
					parentSecurityCheckPoint = parentSecurityCheckPoint == Env.Security.None ? null : parentSecurityCheckPoint;

					var checkpoint = Env.Security.FindOrCreateReportCheckpoint(command.PK.ToGuid(), command.SU_MenuNameMultilingual, possibleModule?.ModuleID, parentSecurityCheckPoint);

					if (!checkpoint.IsAllowed)
					{
						Parent.S5_ParentIDInfo.AddError(checkpoint.ErrorMessageForNotAllowed);
					}
				}
			}
		}

		protected override void CheckS5_ScheduleDescription()
		{
			base.CheckS5_ScheduleDescription();
			if (!Parent.S5_IsPrivate)
			{
				MandatoryValidation.CheckEntered(Parent.S5_ScheduleDescriptionInfo);
				if (!Parent.S5_ScheduleDescription.IsEmpty)
				{
					CheckForDuplicateDescription();
				}
			}
			if (Parent.Recipients.Count == 0)
			{
				Parent.S5_ScheduleDescriptionInfo.AddError(Res.GetString("530839a0-bf17-48ee-8967-ef1e7481127c", "Please enter at least one recipient."));
			}
		}

		protected override void CheckS5_ScheduleStateIsValidZBlobSize()
		{
		}

		void CheckForDuplicateDescription()
		{
			if (!Parent.S5_ScheduleDescription.IsValid || !Parent.S5_ParentID.IsValid)
			{
				return;
			}

			var query = new ZQuery(StmScheduleTaskSchema.S5_ParentID, Parent.S5_ParentID);
			query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, ReportScheduleTask.ScheduleType);
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmMenuItemSchema.Constants.Prefix);
			query.AddToFilter(StmScheduleTaskSchema.S5_IsPrivate, Parent.S5_IsPrivate);
			query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleDescription, Parent.S5_ScheduleDescription);
			query.AddToFilter(StmScheduleTaskSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.ExistsInDatabase(StmScheduleTaskSchema.Constants.TableName, query))
			{
				Parent.S5_ScheduleDescriptionInfo.AddWarning(Res.GetString("31833b4e-8b81-4738-9207-8f1accf68e9e", "There is already a Scheduled Report with this Description using this template."));
			}
		}

		public void ValidateUserFK()
		{
			ValidateCalculatedProperty(Parent.UserFKInfo);
		}

		protected virtual void CheckUserFK()
		{
			MandatoryValidation.CheckEntered(Parent.UserFKInfo, Res.GetString("13b7df4c-ca68-434c-b68b-9d5524bd5565", "Print user"));

			if (Parent.S5_IsActive)
			{
				CheckUserIsValidAndActive();
				CheckUserHasValidEmailAddressIfUsingEPrint();
			}
		}

		void CheckUserIsValidAndActive()
		{
			if (!Parent.UserFK.IsEmpty)
			{
				var printUser = Parent.Factory.Load<GlbStaff>(Parent.UserFK);
				if (printUser == null || printUser.IsDeleted)
				{
					Parent.UserFKInfo.AddError(Res.GetString("b843afb6-11b8-4ab0-8c20-a9ccef909018", "This user does not exist or has been deleted."));
				}
				else if (!printUser.GS_IsActive)
				{
					Parent.UserFKInfo.AddError(Res.GetString("4004B53C-2696-4C0E-9055-484868044EEA", "Cannot assign to an inactive user."));
				}
				else if (printUser.GS_IsSystemAccount && !((IUser)printUser).IsWebUser)
				{
					Parent.UserFKInfo.AddError(Res.GetString("9a8fcc83-da7e-43e7-a567-4b4a042e31b9", "Cannot assign to a system user."));
				}
			}
		}

		void CheckUserHasValidEmailAddressIfUsingEPrint()
		{
			if (!Parent.UserFK.IsEmpty)
			{
				var printUser = Parent.Factory.Load<GlbStaff>(Parent.UserFK);
				if (printUser != null && (printUser.GS_EmailAddress.IsEmpty || printUser.GS_EmailAddressInfo.HasErrors()))
				{
					if (Parent.Recipients.Cast<ReportScheduleTaskRecipient>().Any(recipient => recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint))
					{
						Parent.UserFKInfo.AddError(Res.GetString("e88f0e3f-7a00-40e8-9200-8f47f0c5182d", "This user doesn't have an email address set up. It is required to use the ePrint delivery method."));
					}
				}
			}
		}
	}
}
