using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business
{
	public class StmUniversalCopyScheduleTask : StmScheduleTask, IUniversalCopyScheduleTask
	{
		public StmUniversalCopyScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			controllerID = ControllerIDs.UniversalCopySchedule;
			businessObjectGuid = PK.ToGuid();
			businessObjectName = HumanReadableShortcutName;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ParentTableCode = StmUniversalCopySchema.Constants.Prefix;
		}

		public StmUniversalCopy Parent
		{
			get
			{
				if (parent == null)
				{
					if (!S5_ParentID.IsDefault)
					{
						parent = Factory.Load<StmUniversalCopy>(S5_ParentID);
					}
					else if (!IsInDatabase)
					{
						parent = Factory.New<StmUniversalCopy>();
						S5_ParentID = parent.PK;
					}
					if (parent != null)
					{
						RegisterEditableChildObject(parent);
					}
				}
				return parent;
			}
		}
		StmUniversalCopy parent;

		protected override bool S5_GB_ReadOnly
		{
			get { return false; }
		}

		protected override StmScheduleTaskValidation GetNewValidation()
		{
			return new StmUniversalCopyScheduleTaskValidation(this);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("1e0f7b08-6f10-4e77-8e6c-a2f3f4ad3a4d", "Copy Schedule {0}", S5_ScheduleDescription);

		public override ZString DescriptionForLog
		{
			get
			{
				ZString description = S5_ScheduleDescription;
				using (SetEnvironmentForTask(this))
				{
					if (Parent != null && Parent.CopyObject != null)
					{
						description += ", " + Res.GetString("f8f689de-e69d-491a-a116-05ead9ef5e00", "Module: {0}, Object: {1}", parent.ModuleDescription, parent.CopyObjectDescription);
						var copyObjectLink = parent.CopyObjectLink;
						if (!string.IsNullOrEmpty(copyObjectLink))
						{
							description += string.Format(CultureInfo.InvariantCulture, " ({0})", copyObjectLink);
						}
					}
				}
				return description;
			}
		}

		protected override void RunCore(INotifications notifications, CancellationToken token)
		{
			if (Parent == null)
			{
				notifications.AddError(Res.GetString("14e3ff2a-340f-4f1f-83e4-a5e53dc1a994", "Scheduled copy definition not found. Parent ID = {0}", S5_ParentID));
			}
			else if (Parent.CopyObject == null)
			{
				notifications.AddError(Res.GetString("1e3fc310-d7ef-42eb-af07-09dd0cbbd38d", "Copy object not found. Table Code = {0}, ID = {1}", Parent.SUC_CopyObjectTableCode, Parent.SUC_CopyObjectId));
			}
			else if (Parent.CopyTemplate == null || Parent.Template.CopyTemplateTree == null)
			{
				notifications.AddError(Res.GetString("c847ea95-5f5a-465a-b8b0-352c6ef64558", "Copy template not found or invalid. Template ID = {0}", Parent.SUC_S9_CopyTemplate));
			}
			else
			{
				const int maxTries = 3;
				for (var i = 0; i < maxTries; ++i)
				{
					try
					{
						var copyResult = Parent.RunCopyAndGetCopyObjectInNewFactory();
						var copy = copyResult.Copy;
						if (copy == null)
						{
							notifications.AddError(Res.GetString("80876c5c-3da8-4182-9775-01c52f05826b", "Copy failed for {0}. Error details are:\r\n{1}", DescriptionForLog, copyResult.Error));
						}
						else
						{
							copy.RunPreSaveValidation();
							if (copy.HasErrors)
							{
								S5_IsActive = false;
								Factory.Save();
								notifications.AddRange(copy.GetErrors());
								notifications.Add(CargoWise.ComponentModel.NotificationType.Information, Res.GetString("afc30a24-f585-40c8-8a87-1989c22cd165", "Universal Copy task has been deactivated due to validation errors in the copy result.\r\nTask description: {0}", DescriptionForLog));
							}
							else
							{
								copy.Factory.Save();
							}
						}
						break;
					}
					catch (Exception e) when  (!e.IsCriticalException())
					{
						if (i >= (maxTries - 1))
						{
							throw;
						}
						Parent.ReloadSafe();
					}
				}
			}
		}

		public override void Delete()
		{
			if (!inDelete)
			{
				inDelete = true;
				try
				{
					if (Parent != null && !Parent.IsDeleted && !Parent.IsDeleting)
					{
						Parent.Delete();
					}

					base.Delete();
				}
				finally
				{
					inDelete = false;
				}
			}
		}

		bool inDelete;

		protected override string[] GetRecipientsWithEmailAddress()
		{
			var addresses = new List<string>();
			addresses.AddRange(base.GetRecipientsWithEmailAddress().Where(scheduleUser => !string.IsNullOrEmpty(scheduleUser)));

			if (Parent != null && Parent.CopyTemplate != null)
			{
				var templateUserEmail = GetEmailOfLastEditingUser();
				if (!string.IsNullOrEmpty(templateUserEmail))
				{
					addresses.Add(templateUserEmail);
				}
			}

			return addresses.ToArray();
		}

		protected override string GetEmailOfLastEditingUser()
		{
			var emailAddress = string.Empty;
			var moduleFilter = Parent.CopyTemplate;
			if (moduleFilter != null)
			{
				if (!moduleFilter.S9_SystemLastEditUser.IsEmpty)
				{
					emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, moduleFilter.S9_SystemLastEditUser);
				}

				if (string.IsNullOrEmpty(emailAddress) && !moduleFilter.S9_SystemLastEditUser.Equals(moduleFilter.S9_SystemCreateUser) && !moduleFilter.S9_SystemCreateUser.IsEmpty)
				{
					emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, moduleFilter.S9_SystemCreateUser);
				}
			}

			return emailAddress;
		}

		protected override bool CopyToNotificationsGroupIfRecipientsNotEmpty
		{
			get { return false; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			S5_ParentID = Parent.PK;
			S5_ParentTableCode = StmUniversalCopySchema.Constants.Prefix;

			var dummyBusinessObject = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();
			var copyTemplate = Factory.LoadTop1<UniversalCopyTemplate>(new ZQuery(StmModuleFilterSchema.S9_ModuleID, "S9_UC"));
			if (copyTemplate == null)
			{
				copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "S9_UC";
			}

			Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
			Parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			Parent.SUC_CopyObjectId = dummyBusinessObject.PK;

			HasChanges = false;
		}
#endif
	}
}
