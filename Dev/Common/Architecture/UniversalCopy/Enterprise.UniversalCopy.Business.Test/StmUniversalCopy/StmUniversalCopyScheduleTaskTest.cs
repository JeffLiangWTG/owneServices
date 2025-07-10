using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(StmUniversalCopyScheduleTask))]
	class StmUniversalCopyScheduleTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDescriptionForLog()
		{
			var task = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			task.S5_ScheduleDescription = "Foo";
			AssertEquals("Foo, Module: DummyBizo, Object: DummyBizo", task.DescriptionForLog);

			task.Parent.GridContext = DummyModuleIDs.Dummy.Name + "_UC";
			task.Parent.CopyObject = Factory.New<DummyBusinessObject>();
			AssertStartsWith(null, "Foo, Module: Test Module, Object: DummyBizo", task.DescriptionForLog);

			Factory.Save();
			AssertContains("edient:", task.DescriptionForLog);
		}

		public void TestInvalidStatesAreHandledGracefully()
		{
			var task = Factory.New<StmUniversalCopyScheduleTask>();
			task.S5_ScheduleDescription = "Foo";
			task.S5_ParentID = ZGuid.NewZGuid();
			Factory.Save();
			var notifications = new NotificationBuffer();
			task.Run(notifications);
			AssertContains(string.Format("Scheduled copy definition not found. Parent ID = {0}", task.S5_ParentID), notifications.AsString);
			var parent = Factory.New<StmUniversalCopy>();
			parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			parent.SUC_CopyObjectId = ZGuid.NewZGuid();
			task.S5_ParentID = parent.PK;
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "XXX_UC";
			parent.SUC_S9_CopyTemplate = copyTemplate.PK;
			Factory.Save();
			notifications.Clear();
			task.Run(notifications);
			AssertContains(string.Format("Copy object not found. Table Code = {0}, ID = {1}", DummyBizoSchema.Constants.Prefix, parent.SUC_CopyObjectId), notifications.AsString);
			var dummy = Factory.New<DummyBusinessObject>();
			parent.SUC_CopyObjectId = dummy.PK;
			Factory.Save();
			notifications.Clear();
			task.Run(notifications);

			AssertContains(string.Format("Copy template not found or invalid. Template ID = {0}", parent.SUC_S9_CopyTemplate), notifications.AsString);
		}

		public void TestIScheduleTaskModuleServiceReleaseAfterCopy()
		{
			var task = Factory.New<StmUniversalCopyScheduleTask>();
			task.S5_ScheduleDescription = "Dummy";
			var parent = Factory.New<StmUniversalCopy>();

			parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			var dummy = Factory.New<DummyBusinessObject>();
			parent.SUC_CopyObjectId = dummy.PK;
			task.S5_ParentID = parent.PK;

			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "XXX_UC";
			var copyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyEnterpriseBusinessObject), true)), copyTemplate);
			copyTemplate.CopyTemplateTree = copyTemplateTree;
			parent.SUC_S9_CopyTemplate = copyTemplate.PK;

			Factory.Save();

			var copy = task.Parent.RunCopyAndGetCopyObjectInNewFactory();
			var scheduleTaskModuleService = copy.Copy.Factory.ServiceContainer.GetService<IScheduleTaskModuleService>();
			AssertNull(scheduleTaskModuleService);
		}

		public void TestEmailIsSentToLastEditingUsersWithEmail()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = string.Empty;
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "ABC" }, copyTemplate);
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				var templateUser = Factory.New<GlbStaff>();
				templateUser.GS_Code = "UR1";
				templateUser.GS_LoginName = "User1";
				templateUser.GS_EmailAddress = "user1@mail.box";
				Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(templateUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					copyTemplate.CopyTemplateTree.ConfigurationName = "Xxx";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					copyTemplate.Logs.AddNew(AutoEvents.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Factory.Save();
				}

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}

			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("user1@mail.box", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestEmailNotificationContainsHyperLink()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = string.Empty;
			StmUniversalCopyScheduleTask task;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "ABC" }, copyTemplate);
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				var templateUser = Factory.New<GlbStaff>();
				templateUser.GS_Code = "UR1";
				templateUser.GS_LoginName = "User1";
				templateUser.GS_EmailAddress = "user1@mail.box";
				Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(templateUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					copyTemplate.CopyTemplateTree.ConfigurationName = "Xxx";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					copyTemplate.Logs.AddNew(AutoEvents.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Factory.Save();
				}

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}

			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertContains("Body should containt hyperlink to Bizo", task.BusinessObjectHyperLink_Exposed, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("hyper link should be formated correctly", "Click to open Copy Schedule", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("hyper link should be formated correctly", "<a href=", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestNotificationEmailIsSentToLastEditingUser()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = "user2@mail.box";
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			var emails = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Cast<RecipientDef>().Select(rd => rd.Email).OrderBy(e => e).ToArray();
			AssertEquals("user2@mail.box", emails[0]);

			var templateUser = Factory.New<GlbStaff>();
			templateUser.GS_Code = "UR1";
			templateUser.GS_LoginName = "User1";
			templateUser.GS_EmailAddress = "user1@mail.box";
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(templateUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			emails = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Cast<RecipientDef>().Select(rd => rd.Email).OrderBy(e => e).ToArray();
			AssertEquals("user1@mail.box", emails[0]);

			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].CCRecipients.Count);
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].BCCRecipients.Count);
		}

		public void TestNotificationEmailIsNotSent_WhenLastEditingUserIsSystemAccount()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = "user2@mail.box";
			scheduleUser.GS_IsSystemAccount = true;
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNotificationEmailIsNotSent_WhenLastEditingUserEmailIsNotValid()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = "user2email";
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNotificationEmailIsNotSent_WhenLastEditingUserIsInactive()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = "user2@mail.box";
			scheduleUser.GS_IsActive = false;
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				parent.SUC_CopyObjectId = ZGuid.NewZGuid();
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = "XXX_UC";
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
			}
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestTaskIsDeletedWhenCopyObjectIsDeleted()
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "XXX_UC";

			var dummy1 = Factory.New<DummyBusinessObject>();
			var copyJob1 = Factory.New<StmUniversalCopy>();
			copyJob1.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob1.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob1.SUC_CopyObjectId = dummy1.PK;
			var task1 = Factory.New<StmUniversalCopyScheduleTask>();
			task1.S5_ParentID = copyJob1.PK;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var copyJob2 = Factory.New<StmUniversalCopy>();
			copyJob2.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob2.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob2.SUC_CopyObjectId = dummy2.PK;
			var task2 = Factory.New<StmUniversalCopyScheduleTask>();
			task2.S5_ParentID = copyJob2.PK;

			Factory.Save();

			AssertEquals(false, copyJob1.IsDeleted);
			AssertEquals(false, task1.IsDeleted);

			AssertEquals(false, copyJob2.IsDeleted);
			AssertEquals(false, task2.IsDeleted);

			dummy1.Delete();
			Factory.Save();

			AssertEquals(true, copyJob1.IsDeleted);
			AssertEquals(true, task1.IsDeleted);

			AssertEquals(false, copyJob2.IsDeleted);
			AssertEquals(false, task2.IsDeleted);
		}

		public void TestCopyJobIsDeletedWhenTaskIsDeleted()
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "XXX_UC";

			var dummy1 = Factory.New<DummyBusinessObject>();
			var copyJob1 = Factory.New<StmUniversalCopy>();
			copyJob1.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob1.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob1.SUC_CopyObjectId = dummy1.PK;
			var task1 = Factory.New<StmUniversalCopyScheduleTask>();
			task1.S5_ParentID = copyJob1.PK;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var copyJob2 = Factory.New<StmUniversalCopy>();
			copyJob2.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob2.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob2.SUC_CopyObjectId = dummy2.PK;
			var task2 = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			task2.S5_ParentID = copyJob2.PK;

			Factory.Save();

			AssertEquals(false, copyJob1.IsDeleted);
			AssertEquals(false, task1.IsDeleted);

			AssertEquals(false, copyJob2.IsDeleted);
			AssertEquals(false, task2.IsDeleted);

			task1.Delete();
			Factory.Save();

			AssertEquals(true, copyJob1.IsDeleted);
			AssertEquals(true, task1.IsDeleted);

			AssertEquals(false, copyJob2.IsDeleted);
			AssertEquals(false, task2.IsDeleted);
		}

		public void TestTaskSetInactiveOnValidationError()
		{
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR1";
			scheduleUser.GS_LoginName = "User1";
			scheduleUser.GS_EmailAddress = "user1@mail.box";
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(scheduleUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = Factory.New<StmUniversalCopyScheduleTask>();
				task.S5_ScheduleDescription = "Foo";

				var parent = Factory.New<StmUniversalCopy>();
				var copyObject = Factory.New<DummyEnterpriseBusinessObject>();
				copyObject.Z0_Description = "error";
				parent.CopyObject = copyObject;
				task.S5_ParentID = parent.PK;

				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = DummyModuleIDs.Dummy + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;
				var copyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyEnterpriseBusinessObject), true)), copyTemplate);
				var entityCopyNode = new EntityCopyTemplateNode();
				entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Description, CopyMethod = CopyMethod.Copy });
				copyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
				copyTemplate.CopyTemplateTree = copyTemplateTree;
				parent.SUC_S9_CopyTemplate = copyTemplate.PK;

				Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				copyObject.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				task.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Factory.Save();

				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				var notifications = new NotificationBuffer();
				task.Run(notifications);
				Assert(!task.S5_IsActive);
			}

			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}
	}
}
