using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	sealed class SecurityTest : TestCaseWithFactory
	{
		public void TestRegistrySecurityCheckPointLength()
		{
			var testString = "";
			for (var i = 0; i < 33; ++i)
			{
				testString += "0123456789";
			}
			AssertEquals("RegASS" + testString.Substring(330 - (320 - 6), 320 - 6), ZSecurity.GetRegistrySecurityCheckPointCode(testString));
			AssertEquals("RegCat" + testString.Substring(330 - (320 - 6), 320 - 6), ZSecurity.GetRegistryCategorySecurityCheckPointCode(testString));
		}

		public void TestSupportOnlyRegistryShouldNotShowInSecurityFormInNoneEDI()
		{
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AAA");

			SupportOnlyRegistryCheckPointVisible(false);
		}

		public void TestSupportOnlyRegistryShouldShowInSecurityFormInEDI()
		{
			RawDataRegistry.Instance.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDI");

			SupportOnlyRegistryCheckPointVisible(true);
		}

		void SupportOnlyRegistryCheckPointVisible(bool isEdiClient)
		{
			var securityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty)
			{
				CachingEnabled = false
			};
			var registryCheckPoints = securityItem.GetAllRegistryCheckPoint();
			var supportOnlyRegistries = ObjectFactory.New<IRegistryItemSetLocator>().GetAllRegistryItems()
				.Where(registry => !(registry is LinkRegistryItem) && registry.Options == Integration.RegistryOptions.IsOnlyForSupport);

			foreach (var registry in supportOnlyRegistries)
			{
				var code = ZSecurity.GetRegistrySecurityCheckPointCode(registry.Name);
				var checkpoint = registryCheckPoints.FirstOrDefault(c => c.Code == code);
				if (isEdiClient)
				{
					AssertNotNull("Should find check point which registry is IsOnlyForSupport in EDI.", checkpoint);
				}
				else
				{
					AssertNull("Should not find check point which registry is IsOnlyForSupport if it's not EDI client.", checkpoint);
				}
			}
		}

		public void TestIsOnlyEditableBySupportIfHostedRegistry()
		{
			EnvProxy.SetHostedLocationForTest("");//Non-Hosted
			Assert(!EnvProxy.IsHostedWithCargowise);
			AssertIsOnlyEditableBySupportIfHostedVisible(true);

			EnvProxy.SetHostedLocationForTest("SYD");//Hosted
			Assert(EnvProxy.IsHostedWithCargowise);
			AssertIsOnlyEditableBySupportIfHostedVisible(false);
		}

		void AssertIsOnlyEditableBySupportIfHostedVisible(bool shouldVisible)
		{
			var securityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty)
			{
				CachingEnabled = false
			};
			var registryCheckPoints = securityItem.GetAllRegistryCheckPoint();
			var isOnlyEditableBySupportIfHostedRegistries = ObjectFactory.New<IRegistryItemSetLocator>().GetAllRegistryItems()
				.Where(registry => !(registry is LinkRegistryItem)
				&& registry.HasOption(Integration.RegistryOptions.IsOnlyEditableBySupportIfHosted)
				&& !registry.HasOption(Integration.RegistryOptions.IsOnlyForController));

			CombineAssertions(() =>
			{
				foreach (var registry in isOnlyEditableBySupportIfHostedRegistries)
				{
					var code = ZSecurity.GetRegistrySecurityCheckPointCode(registry.Name);
					var checkpoint = registryCheckPoints.FirstOrDefault(c => c.Code == code);
					if (shouldVisible)
					{
						AssertNotNull($"Should find check point: ({registry.Name}) which registry is IsOnlyEditableBySupportIfHosted.", checkpoint);
					}
					else
					{
						AssertNull($"Should not find check point: ({registry.Name}) which registry is IsOnlyEditableBySupportIfHosted", checkpoint);
					}
				}
			});
		}

		public void TestResetSecurity_DoNotLeakSecurityCollection()
		{
			var security = new ZSecurity(null, (GlbStaff)null, BranchPK, DepartmentPK, CompanyPK);
			var reference = GetWeakSecurityReference(security);
			security.ResetData(null, null, BranchPK, DepartmentPK, CompanyPK);
			GC.Collect();

			AssertEquals(false, reference.IsAlive);
		}

		WeakReference GetWeakSecurityReference(ZSecurity security)
		{
			var collection = security.SecurityDataForTesting;
			collection.AddNew();
			collection.Find(GlbSecurityRightsLookupKey.ForLookup(Env.Security.WorkItem, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));
			return new WeakReference(security.SecurityDataForTesting);
		}

		public void TestDeniedWhenEmptyNonSecurityGroupAttached()
		{
			var allGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
			allGroup.ResetGroupPermissions();

			var emptyNonSecurityGroup = fStaff.Factory.NewWithValidTestData<GlbGroup>();
			emptyNonSecurityGroup.IsNonSecurityGroup = true;
			emptyNonSecurityGroup.SecurityPermissions.RemoveAndDeleteAll();

			emptyNonSecurityGroup.Staff.Add(fStaff);

			Factory.Save();

			fStaff = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbStaff>(fStaff.PK);
			AssertEquals(2, fStaff.ActiveGroups.Count);

			emptyNonSecurityGroup = fStaff.Factory.Load<GlbGroup>(emptyNonSecurityGroup.PK);

			var collection = new GlbSecurityCollection(Factory);
			collection.Load();

			Assert(collection.Where(x => x.GU_GG == allGroup.PK).Any());

			var security = new SecurityCore(collection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			AssertEquals(false, security.Operations.IsAllowed);

			//however if we add a security right to a non-security right somehow, that CAN grant
			var granted = emptyNonSecurityGroup.SecurityPermissions.AddNew();
			granted.GU_SecurityItemIsAllowed = true;
			granted.GU_SecurityRight = security.Operations.Code;
			granted.GU_GG = emptyNonSecurityGroup.PK;
			fStaff.Factory.Save();

			collection = new GlbSecurityCollection(fStaff.Factory);
			collection.Load();

			Assert(collection.Where(x => x.GU_GG == allGroup.PK).Any());

			security = new SecurityCore(collection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			AssertEquals(true, security.Operations.IsAllowed);
		}

		public void TestDeniedWhenNoGroupsAttached()
		{
			AssertEquals(1, fStaff.ActiveGroups.Count);

			var collection = new GlbSecurityCollection(Factory);
			var security = new SecurityForTest(collection, fStaff, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			security.CachingEnabled = false;
			AssertEquals(true, security.Operations.IsAllowed);

			fStaff.GS_IsActive = false;
			fStaff.ActiveGroups.RemoveAll();
			Factory.Save();
			AssertEquals(0, fStaff.ActiveGroups.Count);

			collection = new GlbSecurityCollection(Factory);
			security = new SecurityForTest(collection, fStaff, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			security.CachingEnabled = false;
			AssertEquals(false, security.Operations.IsAllowed);
		}

		public void TestLoadingSecurityWithoutStaffMemberDoesNotLoadEntireSecurityRecordSet()
		{
			var security = new ZSecurity(null, (GlbStaff)null, BranchPK, DepartmentPK, CompanyPK);
			AssertEquals("security.SecurityData.Count with no staff record specified", 0, security.SecurityDataForTesting.Count);

			security = new ZSecurity(null, fStaff, BranchPK, DepartmentPK, CompanyPK);
			AssertNotEquals("security.SecurityData.Count with no staff record specified", 0, security.SecurityDataForTesting.Count);
		}

		public void TestCheckpointDictionarySizeHintIsCorrect()
		{
			var security = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			var checkpointDictionary = security.SecurityInstance.CheckPointLookUpTable_ForTest;
			var ratioOfFullness = checkpointDictionary.Count / (float)ZSecurity.CheckPointLookupTableCapacity;

			Assert("More than half full", ratioOfFullness > 0.5f);
			Assert("Not full", ratioOfFullness < 1);
		}

		public void TestFindOrCreateChangeStaffSecurityCheckpoint()
		{
			SecurityCore security = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			Guid guid = Guid.NewGuid();
			ISecurityCheckpoint checkpoint1 = ((IZSecurity)security).FindOrCreateChangeStaffSecurityCheckpoint(guid);
			ISecurityCheckpoint checkpoint2 = ((IZSecurity)security).FindOrCreateChangeStaffSecurityCheckpoint(guid);
			AssertSame(checkpoint1, checkpoint2);

			Assert(checkpoint1 is LocalAdminCheckpoint);
			AssertEquals(false, checkpoint1.Visible);
			AssertEquals(GlbSecurity.ChangeOtherStaffSecurityRightName, checkpoint1.Code);
			AssertEquals(guid, checkpoint1.LookupKey.ItemGuid);
		}

		public void TestFindOrCreateChangeGroupSecurityCheckpoint()
		{
			SecurityCore security = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			Guid guid = Guid.NewGuid();
			ISecurityCheckpoint checkpoint1 = ((IZSecurity)security).FindOrCreateChangeGroupSecurityCheckpoint(guid);
			ISecurityCheckpoint checkpoint2 = ((IZSecurity)security).FindOrCreateChangeGroupSecurityCheckpoint(guid);
			AssertSame(checkpoint1, checkpoint2);

			Assert(checkpoint1 is LocalAdminCheckpoint);
			AssertEquals(false, checkpoint1.Visible);
			AssertEquals(GlbSecurity.ChangeOtherGroupSecurityRightName, checkpoint1.Code);
			AssertEquals(guid, checkpoint1.LookupKey.ItemGuid);
		}

		public void TestFindOrCreateGroupOwnerSecurityCheckpoint()
		{
			SecurityCore security = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			Guid guid = Guid.NewGuid();
			ISecurityCheckpoint checkpoint1 = ((IZSecurity)security).FindOrCreateGroupOwnerSecurityCheckpoint(guid);
			ISecurityCheckpoint checkpoint2 = ((IZSecurity)security).FindOrCreateGroupOwnerSecurityCheckpoint(guid);
			AssertSame(checkpoint1, checkpoint2);

			Assert(checkpoint1 is LocalAdminCheckpoint);
			AssertEquals(false, checkpoint1.Visible);
			AssertEquals(GlbSecurity.GroupOwnerSecurityRightName, checkpoint1.Code);
			AssertEquals(guid, checkpoint1.LookupKey.ItemGuid);
		}

		public void TestIsGroupExplicitlyAllowed()
		{
			SecurityForTest securityInstance = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			AssertEquals("IsGroupExplicitlyAllowed", false, securityInstance.ZSecurityInstance.IsGroupExplicitlyAllowed(securityInstance.Operations));

			foreach (GlbSecurity existingSecurityRight in fSecurityCollection.Find(new ZQuery(GlbSecuritySchema.GU_SecurityRight, securityInstance.Operations.Code)))
			{
				existingSecurityRight.Delete();
			}
			Factory.Save();
			AssertEquals("IsGroupExplicitlyAllowed", false, securityInstance.ZSecurityInstance.IsGroupExplicitlyAllowed(securityInstance.Operations));

			GlbGroup newGroup = fStaff.Groups.AddNew();
			Factory.Save();
			AssertEquals("IsGroupExplicitlyAllowed", false, securityInstance.ZSecurityInstance.IsGroupExplicitlyAllowed(securityInstance.Operations));

			// if security exist, then delete it to avoid saving duplicates
			var oldSecurityRight = LoadOrCreateSecurity(securityInstance.Operations.Code, newGroup.PK);
			if (oldSecurityRight.IsInDatabase)
			{
				oldSecurityRight.Delete();
			}
			Factory.Save();

			GlbSecurity securityRight = fSecurityCollection.AddNew();
			securityRight.GU_GG = newGroup.PK;
			securityRight.GU_SecurityRight = securityInstance.Operations.Code;
			securityRight.GU_SecurityItemIsAllowed = true;
			Factory.Save();
			AssertEquals("IsGroupExplicitlyAllowed", true, securityInstance.ZSecurityInstance.IsGroupExplicitlyAllowed(securityInstance.Operations));

			newGroup.GG_IsActive = false;
			Factory.Save();
			AssertEquals("IsGroupExplicitlyAllowed", false, securityInstance.ZSecurityInstance.IsGroupExplicitlyAllowed(securityInstance.Operations));
		}

		public void TestResetData()
		{
			SecurityCore testSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);

			AssertEquals("Before TestSecurity reset Branch is current Branch", BranchPK, testSecurity.BranchPK);
			AssertEquals("Before TestSecurity reset Department is current Department", DepartmentPK, testSecurity.DepartmentPK);
			AssertEquals("Before TestSecurity reset Company is current Company", CompanyPK, testSecurity.CompanyPK);
			AssertEquals("Before TestSecurity reset UserPK is StaffPK", fStaff.PK, testSecurity.UserPK);

			testSecurity.ResetData(null, Env.CurrentUser.PK, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("After TestSecurity reset Branch is current branch", Env.CurrentBranch.PK, testSecurity.BranchPK);
			AssertEquals("After TestSecurity reset Department is current department", Env.CurrentDepartment.PK, testSecurity.DepartmentPK);
			AssertEquals("After TestSecurity reset Company is current company", Env.CurrentCompany.PK, testSecurity.CompanyPK);
			AssertEquals("After TestSecurity reset UserPK is current user", Env.CurrentUser.PK, testSecurity.UserPK);
		}

		public void TestFindCheckPoint()
		{
			SecurityCore testSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			testSecurity.CachingEnabled = false;
			SecurityCheckpoint foundCheckPoint = testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Operations.Code));
			AssertEquals("Code", testSecurity.Operations.Code, foundCheckPoint.Code);
			AssertEquals("DisplayText", testSecurity.Operations.DisplayText, foundCheckPoint.DisplayText);
		}

		public void TestFindOrCreateExportCheckPoint()
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint newCheckPoint = new SecurityCheckpoint("-----", (NoResString)"-----", testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Operations.Code)), testSecurity.ZSecurityInstance);
			SecurityCheckpoint newCheckPointExports = testSecurity.FindOrCreateExportCheckPoint(newCheckPoint);
			AssertEquals("New checkpoint should have code like", "-----" + SecurityCore.ExportToExcelAutoGeneratedCode, newCheckPointExports.Code);
			AssertEquals("New checkpoint should have display text like", SecurityCore.ExportToExcelAutoGeneratedDisplayText, newCheckPointExports.DisplayText);

			SecurityCheckpoint newCheckPointExportsFoundAgain = testSecurity.FindOrCreateExportCheckPoint(newCheckPoint);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointExports, newCheckPointExportsFoundAgain);
		}

		public void TestFindOrCreateExportCheckPoint_WhenParentIsNull()
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			AssertNoExceptionThrown(() => testSecurity.FindOrCreateExportCheckPoint(null));
		}

		public void TestFindOrCreateImportCheckPoint()
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint newCheckPoint = new SecurityCheckpoint("-----", (NoResString)"-----", testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Operations.Code)), testSecurity.ZSecurityInstance);
			SecurityCheckpoint newCheckPointImports = testSecurity.FindOrCreateImportCheckPoint(newCheckPoint);
			AssertEquals("New checkpoint should have code like", "-----" + SecurityCore.ImportToSystemAutoGeneratedCode, newCheckPointImports.Code);
			AssertEquals("New checkpoint should have display text like", SecurityCore.ImportToSystemAutoGeneratedDisplayText, newCheckPointImports.DisplayText);

			SecurityCheckpoint newCheckPointExportsFoundAgain = testSecurity.FindOrCreateImportCheckPoint(newCheckPoint);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointImports, newCheckPointExportsFoundAgain);
		}

		public void TestFindOrCreateImportCheckPoint_WhenParentIsNull()
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			AssertNoExceptionThrown(() => testSecurity.FindOrCreateImportCheckPoint(null));
		}

		public void TestFindOrCreateReportCheckpoint()
		{
			StmMenuItem menuItem = (StmMenuItem)Factory.NewWithValidTestData(typeof(StmMenuItem));
			menuItem.SU_MenuName = "Some Report";
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint newCheckpoint = new SecurityCheckpoint("-----", (NoResString)"-----", testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.OrderReports.Code)), testSecurity.ZSecurityInstance);
			SecurityCheckpoint newCheckPointReports = testSecurity.FindOrCreateReportCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, newCheckpoint);
			AssertEquals("New checkpoint should have the code: ", "Report", newCheckPointReports.Code);
			AssertEquals("New checkpoint should have the ItemGuid: ", menuItem.PK.ToGuid(), newCheckPointReports.ItemGuid);
			AssertEquals("New checkpoint should have display text like", "Some Report", newCheckPointReports.DisplayText);

			SecurityCheckpoint newCheckPointReportsFoundAgain = testSecurity.FindOrCreateReportCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, newCheckpoint);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointReports, newCheckPointReportsFoundAgain);
		}

		public void TestFindOrCreateDocumentCheckpoint()
		{
			StmMenuItem menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "Some Document";
			menuItem.SU_MenuPath = "Export/";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Quotation);
			Factory.Save();
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint parentCheckpoint = testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Quotation.Code));
			SecurityCheckpoint newCheckPointDocuments = testSecurity.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Quotations, parentCheckpoint);
			AssertEquals("New checkpoint should have code like", "Doc" + ModuleIDs.Quotations.ToString(), newCheckPointDocuments.Code);
			AssertEquals("New checkpoint should have Item Guid like", menuItem.PK.ToGuid(), newCheckPointDocuments.ItemGuid);

			SecurityCheckpoint newCheckPointDocumentFoundAgain = testSecurity.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Quotations, parentCheckpoint);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointDocuments, newCheckPointDocumentFoundAgain);
		}

		public void TestDisplayTextPathToSecurityRight_FindOrCreateDocumentCheckpoint()
		{
			StmMenuItem menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "Some Document";
			menuItem.SU_MenuPath = "DocumentPath";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Quotation);
			Factory.Save();
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint parentCheckpoint = testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Quotation.Code));
			SecurityCheckpoint checkPointDocument = testSecurity.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Quotations, parentCheckpoint);
			AssertEquals("Manage -> Tariffs & Rates -> Quotations -> DocumentPath -- Some Document", checkPointDocument.DisplayTextPathToSecurityRight);
		}

		public void TestFindOrCreateDocumentOverrideCheckpoint()
		{
			StmMenuItem menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "Some Document";
			menuItem.SU_MenuPath = "Export/";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Quotation);
			Factory.Save();
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint parentCheckpoint = testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Quotation.Code));
			SecurityCheckpoint newCheckPointDocuments = testSecurity.FindOrCreateDocumentOverrideCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Quotations, parentCheckpoint);
			AssertEquals("New checkpoint should have code like", "DocVis" + ModuleIDs.Quotations.ToString(), newCheckPointDocuments.Code);
			AssertEquals("New checkpoint should have Item Guid like", menuItem.PK.ToGuid(), newCheckPointDocuments.ItemGuid);

			SecurityCheckpoint newCheckPointDocumentFoundAgain = testSecurity.FindOrCreateDocumentOverrideCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Quotations, parentCheckpoint);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointDocuments, newCheckPointDocumentFoundAgain);
		}

		public void TestDisplayTextPathToSecurityRight_FindOrCreateDocumentOverrideCheckpoint()
		{
			StmMenuItem menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "Some Document";
			menuItem.SU_MenuPath = "OverrideDocumentPath";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Quotation);
			Factory.Save();
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint parentCheckpoint = testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Quotation.Code));
			SecurityCheckpoint checkPointDocumentOverride = testSecurity.FindOrCreateDocumentOverrideCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Quotations, parentCheckpoint);
			AssertEquals("Manage -> Tariffs & Rates -> Quotations -> OverrideDocumentPath -- Some Document", checkPointDocumentOverride.DisplayTextPathToSecurityRight);
		}

		#region Workflow

		public void TestFindOrCreateWorkflowCheckPoint()
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint newCheckPoint = new SecurityCheckpoint("-----", (NoResString)"-----", testSecurity.FindCheckPoint(testSecurity.Operations.Code), testSecurity.ZSecurityInstance);
			SecurityCheckpoint newCheckPointWorkflow = testSecurity.FindOrCreateWorkflowCheckpoint(newCheckPoint);
			AssertEquals("New checkpoint should have code like", "-----" + SecurityCore.WorkflowAutoGeneratedCode, newCheckPointWorkflow.Code);
			AssertEquals("New checkpoint should have display text like", SecurityCore.WorkflowAutoGeneratedDisplayText, newCheckPointWorkflow.DisplayText);

			SecurityCheckpoint newCheckPointWorkflowFoundAgain = testSecurity.FindOrCreateWorkflowCheckpoint(newCheckPoint);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointWorkflow, newCheckPointWorkflowFoundAgain);
		}

		public void TestFindOrCreateWorkflowTasksCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowTasksAutoGeneratedCode, SecurityCore.WorkflowTasksAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowMilestonesCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowMilestonesAutoGeneratedCode, SecurityCore.WorkflowMilestonesAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowExceptionsCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowExceptionsAutoGeneratedCode, SecurityCore.WorkflowExceptionsAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowTriggersCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowTriggersAutoGeneratedCode, SecurityCore.WorkflowTriggersAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowAddTasksCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowAddTasksAutoGeneratedCode, SecurityCore.WorkflowAddTasksAutoGeneratedDisplayText, SecurityCore.WorkflowTasksAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowAddTasksAutoGeneratedCode, SecurityCore.WorkflowAddTasksAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowAddMilestonesCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowAddMilestonesAutoGeneratedCode, SecurityCore.WorkflowAddMilestonesAutoGeneratedDisplayText, SecurityCore.WorkflowMilestonesAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowAddMilestonesAutoGeneratedCode, SecurityCore.WorkflowAddMilestonesAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowAddExceptionsCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowAddExceptionsAutoGeneratedCode, SecurityCore.WorkflowAddExceptionsAutoGeneratedDisplayText, SecurityCore.WorkflowExceptionsAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowAddExceptionsAutoGeneratedCode, SecurityCore.WorkflowAddExceptionsAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowAddTriggersCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowAddTriggersAutoGeneratedCode, SecurityCore.WorkflowAddTriggersAutoGeneratedDisplayText, SecurityCore.WorkflowTriggersAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowAddTriggersAutoGeneratedCode, SecurityCore.WorkflowAddTriggersAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowDeleteTasksCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowDeleteTasksAutoGeneratedCode, SecurityCore.WorkflowDeleteTasksAutoGeneratedDisplayText, SecurityCore.WorkflowTasksAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowDeleteTasksAutoGeneratedCode, SecurityCore.WorkflowDeleteTasksAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowDeleteMilestonesCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowDeleteMilestonesAutoGeneratedCode, SecurityCore.WorkflowDeleteMilestonesAutoGeneratedDisplayText, SecurityCore.WorkflowMilestonesAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowDeleteMilestonesAutoGeneratedCode, SecurityCore.WorkflowDeleteMilestonesAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowDeleteExceptionsCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowDeleteExceptionsAutoGeneratedCode, SecurityCore.WorkflowDeleteExceptionsAutoGeneratedDisplayText, SecurityCore.WorkflowExceptionsAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowDeleteExceptionsAutoGeneratedCode, SecurityCore.WorkflowDeleteExceptionsAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowDeleteTriggersCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowDeleteTriggersAutoGeneratedCode, SecurityCore.WorkflowDeleteTriggersAutoGeneratedDisplayText, SecurityCore.WorkflowTriggersAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowDeleteTriggersAutoGeneratedCode, SecurityCore.WorkflowDeleteTriggersAutoGeneratedDisplayText);
		}

		public void TestFindOrCreateWorkflowOverrideExceptionDurationCheckPoint()
		{
			FindOrCreateWorkflowCheckPoint(SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedCode, SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedDisplayText, SecurityCore.WorkflowExceptionsAutoGeneratedCode);
			FindOrCreateWorkflowSubCheckPoint(SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedCode, SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedDisplayText);
		}

		void FindOrCreateWorkflowCheckPoint(string code, MultilingualString displayText, string intermediate = "")
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint newCheckPoint = new SecurityCheckpoint("-----", (NoResString)"-----", testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Operations.Code)), testSecurity.ZSecurityInstance);
			var newCheckPointWorkflow = testSecurity.FindOrCreateWorkflowItemCheckpoint(newCheckPoint, code);
			AssertEquals("New checkpoint should have code like", $"-----{SecurityCore.WorkflowAutoGeneratedCode}{intermediate}{code}", newCheckPointWorkflow.Code);
			AssertEquals("New checkpoint should have display text like", displayText, newCheckPointWorkflow.DisplayText);

			SecurityCheckpoint newCheckPointWorkflowFoundAgain = testSecurity.FindOrCreateWorkflowItemCheckpoint(newCheckPoint, code);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointWorkflow, newCheckPointWorkflowFoundAgain);
		}

		void FindOrCreateWorkflowSubCheckPoint(string code, MultilingualString displayText)
		{
			SecurityForTest testSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			SecurityCheckpoint newCheckPoint = new SecurityCheckpoint("-----", (NoResString)"-----", testSecurity.FindCheckPoint(new CheckpointLookupKey(testSecurity.Operations.Code)), testSecurity.ZSecurityInstance);
			var newCheckPointWorkflow = testSecurity.FindOrCreateWorkflowSubItemCheckpoint(newCheckPoint, code);
			AssertEquals("New checkpoint should have code like", "-----" + code, newCheckPointWorkflow.Code);
			AssertEquals("New checkpoint should have display text like", displayText, newCheckPointWorkflow.DisplayText);

			SecurityCheckpoint newCheckPointWorkflowFoundAgain = testSecurity.FindOrCreateWorkflowSubItemCheckpoint(newCheckPoint, code);
			AssertEquals("Checkpoints should be equal, should not be created again", newCheckPointWorkflow, newCheckPointWorkflowFoundAgain);
		}

		#endregion

		public void TestDenyGrandParent()
		{
			SecurityForTest aSecurity = new SecurityForTest(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;

			SecurityCheckpoint grandParent = new SecurityCheckpoint("GrandParent", (NoResString)"Grand Parent", null, aSecurity.ZSecurityInstance);
			SecurityCheckpoint parent = new SecurityCheckpoint("Parent", (NoResString)"Parent", null, aSecurity.ZSecurityInstance);
			SecurityCheckpoint child = new SecurityCheckpoint("Child", (NoResString)"Child", null, aSecurity.ZSecurityInstance);

			grandParent.AddChild(parent);
			parent.AddChild(child);

			AssertEquals("Child.IsAllowed", true, child.IsAllowed);

			//Now deny grand parent

			GlbSecurity securityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_SecurityRight = grandParent.Code;
			securityRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(securityRecord);

			AssertEquals("Child.IsAllowed", false, child.IsAllowed);
		}

		public void TestIsAllowed()
		{
			GlbGroup group1 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			string securityRight = "MaintainConsol";

			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;
			Factory.Save();
			Assert("Rights not specified in security should be allowed", aSecurity.MaintainConsol.IsAllowed);

			GlbSecurity securityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			securityRecord.GU_SecurityItemIsAllowed = true;
			securityRecord.GU_SecurityRight = securityRight;
			securityRecord.GU_GG = group1.PK;
			fSecurityCollection.Add(securityRecord);

			Factory.Save();
			Assert("When Group has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			securityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When Group rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			GlbGroup groupCompany1 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(groupCompany1);

			GlbSecurity securityRecord2 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			securityRecord2.GU_SecurityItemIsAllowed = true;
			securityRecord2.GU_SecurityRight = securityRight;
			securityRecord2.GU_GG = groupCompany1.PK;
			securityRecord2.GU_GC = Env.CurrentCompany.PK;
			fSecurityCollection.Add(securityRecord2);

			Factory.Save();
			Assert("When GroupCompany1 has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			groupCompany1.GG_IsActive = false;
			Factory.Save();
			Assert("When GroupCompany1 is inactive, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);
			groupCompany1.GG_IsActive = true;
			securityRecord2.GU_SecurityItemIsAllowed = false;
			Assert("When GroupCompany1 rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			GlbGroup groupCompany2 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(groupCompany2);

			GlbSecurity securityRecord3 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			securityRecord3.GU_SecurityItemIsAllowed = true;
			securityRecord3.GU_SecurityRight = securityRight;
			securityRecord3.GU_GG = groupCompany2.PK;
			securityRecord3.GU_GC = Env.CurrentCompany.PK;
			fSecurityCollection.Add(securityRecord3);

			Factory.Save();
			Assert("When another GroupCompany2 has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);

			securityRecord3.GU_SecurityItemIsAllowed = false;
			Assert("When GroupCompany2 rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			securityRecord2.GU_SecurityItemIsAllowed = true;
			Assert("When GroupCompany1 has rights again, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);

			// Check when the user belongs to a third group that has no explicit denials
			securityRecord2.GU_SecurityItemIsAllowed = false;
			Assert("When GroupCompany1 has rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			GlbGroup group4 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(group4);
			Factory.Save();
			Assert("When User belongs to another group with no explicit denials, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			fStaff.Groups.Remove(group4);

			securityRecord2.GU_SecurityItemIsAllowed = true;

			//Add GroupBranch

			GlbGroup groupBranchGroup = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(groupBranchGroup);

			GlbSecurity branchSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			branchSecurityRecord.GU_SecurityItemIsAllowed = true;
			branchSecurityRecord.GU_SecurityRight = securityRight;
			branchSecurityRecord.GU_GG = groupBranchGroup.PK;
			branchSecurityRecord.GU_GC = Env.CurrentCompany.PK;
			fSecurityCollection.Add(branchSecurityRecord);

			Factory.Save();
			Assert("When GroupBranch has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);

			branchSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When GroupBranch rights denied, but another GroupCompany still has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			securityRecord2.GU_SecurityItemIsAllowed = false;

			Assert("When GroupBranch rights denied, and no other group has rights, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			//Add GroupCompanyDepartment

			GlbGroup groupCompanyDepartmentGroup = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(groupCompanyDepartmentGroup);

			GlbSecurity companyDepartmentSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			companyDepartmentSecurityRecord.GU_SecurityItemIsAllowed = true;
			companyDepartmentSecurityRecord.GU_SecurityRight = securityRight;
			companyDepartmentSecurityRecord.GU_GG = groupCompanyDepartmentGroup.PK;
			companyDepartmentSecurityRecord.GU_GC = Env.CurrentCompany.PK;
			companyDepartmentSecurityRecord.GU_GE = Env.CurrentDepartment.PK;
			fSecurityCollection.Add(companyDepartmentSecurityRecord);

			Factory.Save();
			Assert("When GroupCompanyDepartment has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);

			companyDepartmentSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When GroupCompanyDepartment rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			//Add GroupBranchDepartment

			GlbGroup groupBranchDepartmentGroup = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(groupBranchDepartmentGroup);

			GlbSecurity branchDepartmentSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			branchDepartmentSecurityRecord.GU_SecurityItemIsAllowed = true;
			branchDepartmentSecurityRecord.GU_SecurityRight = securityRight;
			branchDepartmentSecurityRecord.GU_GG = groupBranchDepartmentGroup.PK;
			branchDepartmentSecurityRecord.GU_GB = Env.CurrentBranch.PK;
			branchDepartmentSecurityRecord.GU_GE = Env.CurrentDepartment.PK;
			fSecurityCollection.Add(branchDepartmentSecurityRecord);

			Factory.Save();
			Assert("When GroupBranchDepartment has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			branchDepartmentSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When GroupBranchDepartment rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			// Add User

			GlbSecurity userSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userSecurityRecord.GU_SecurityItemIsAllowed = true;
			userSecurityRecord.GU_SecurityRight = securityRight;
			userSecurityRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(userSecurityRecord);

			Factory.Save();
			Assert("When User has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);

			userSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When User rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			//Add User Company

			GlbSecurity userCompanySecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userCompanySecurityRecord.GU_SecurityItemIsAllowed = true;
			userCompanySecurityRecord.GU_SecurityRight = securityRight;
			userCompanySecurityRecord.GU_GS = fStaff.PK;
			userCompanySecurityRecord.GU_GC = Env.CurrentCompany.PK;
			fSecurityCollection.Add(userCompanySecurityRecord);

			Factory.Save();
			Assert("When UserCompany has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			userCompanySecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When UserCompany rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			//Add User Branch

			GlbSecurity userBranchSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userBranchSecurityRecord.GU_SecurityItemIsAllowed = true;
			userBranchSecurityRecord.GU_SecurityRight = securityRight;
			userBranchSecurityRecord.GU_GS = fStaff.PK;
			userBranchSecurityRecord.GU_GB = Env.CurrentBranch.PK;
			fSecurityCollection.Add(userBranchSecurityRecord);

			Factory.Save();
			Assert("When UserBranch has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			userBranchSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When UserBranch rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			//Add User CompanyDepartment

			GlbSecurity userCompanyDepartmentSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userCompanyDepartmentSecurityRecord.GU_SecurityItemIsAllowed = true;
			userCompanyDepartmentSecurityRecord.GU_SecurityRight = securityRight;
			userCompanyDepartmentSecurityRecord.GU_GS = fStaff.PK;
			userCompanyDepartmentSecurityRecord.GU_GC = Env.CurrentCompany.PK;
			userCompanyDepartmentSecurityRecord.GU_GE = Env.CurrentDepartment.PK;
			fSecurityCollection.Add(userCompanyDepartmentSecurityRecord);

			Factory.Save();
			Assert("When UserCompanyDepartment has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			userCompanyDepartmentSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When UserCompanyDepartment rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);

			//Add User BranchDepartment

			GlbSecurity userBranchDepartmentSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userBranchDepartmentSecurityRecord.GU_SecurityItemIsAllowed = true;
			userBranchDepartmentSecurityRecord.GU_SecurityRight = securityRight;
			userBranchDepartmentSecurityRecord.GU_GS = fStaff.PK;
			userBranchDepartmentSecurityRecord.GU_GB = Env.CurrentBranch.PK;
			userBranchDepartmentSecurityRecord.GU_GE = Env.CurrentDepartment.PK;
			fSecurityCollection.Add(userBranchDepartmentSecurityRecord);

			Factory.Save();
			Assert("When UserBranchDepartment has rights, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
			userBranchDepartmentSecurityRecord.GU_SecurityItemIsAllowed = false;
			Assert("When UserBranchDepartment rights denied, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);
		}

		public void TestIsAllowedByCheckPoint()
		{
			string securityRight = "MaintainConsol";

			GlbSecurity maintenanceRightsSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			maintenanceRightsSecurityRecord.GU_SecurityItemIsAllowed = false;
			maintenanceRightsSecurityRecord.GU_SecurityRight = securityRight;
			maintenanceRightsSecurityRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(maintenanceRightsSecurityRecord);

			GlbSecurity otherSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			otherSecurityRecord.GU_SecurityItemIsAllowed = true;
			otherSecurityRecord.GU_SecurityRight = "AnotherSecurity";
			otherSecurityRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(otherSecurityRecord);

			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;

			Assert("When MaintenanceRightsSecurityRecord denied rights, then user should NOT be allowed", !aSecurity.MaintainConsol.IsAllowed);
			maintenanceRightsSecurityRecord.GU_SecurityItemIsAllowed = true;
			Assert("When MaintenanceRightsSecurityRecord rights granted, then user should be allowed", aSecurity.MaintainConsol.IsAllowed);
		}

		[StressTest]
		[ExpectNoExceptions]
		[SnailTest]
		public void TestIsAllowed_WithManyGroupsAndSmallStack_NoException()
		{
			var localFactory = new BusinessObjectFactory();

			var staff = localFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_IsController = false;

			for (int i = 0; i < 500; i++)
			{
				var group = localFactory.NewWithValidTestData<GlbGroup>();
				group.GG_Code = i.ToString().PadLeft(3, '0');
				staff.Groups.Add(group);

				GlbSecurity groupSecurity = localFactory.New<GlbSecurity>();
				groupSecurity.GU_SecurityItemIsAllowed = i > 495;
				groupSecurity.GU_SecurityRight = "MaintainConsol";
				groupSecurity.GU_GG = group.PK;
			}
			localFactory.Save();

			SecurityCore security = new SecurityCore(null, staff, BranchPK, DepartmentPK, CompanyPK) { CachingEnabled = false };
			Assert(security.MaintainConsol.IsAllowed); // First run to force ZSecurity to load GlbSecurity rows to its factory

			var task = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					security.ResetData(null, staff, BranchPK, DepartmentPK, CompanyPK);
					Assert(security.MaintainConsol.IsAllowed); // StackOverflowException happened here when many ORs were used in ZQuery
				}
			});

			task.Wait();
		}

		public void TestEverythingIsAllowedForWebUser()
		{
			string webUserLogin = User.WebUserName;

			using (Env.SetTemporaryUserContext(webUserLogin, BranchPK, DepartmentPK))
			{
				GlbStaff webUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_LoginName, webUserLogin));
				Assert("WebUser.GS_IsSystemAccount", webUser.GS_IsSystemAccount);
				AssertEquals("WebUser.GS_Code", "ZZ", webUser.GS_Code);

				SecurityCore securityForTest = new SecurityCore(null, webUser, Guid.Empty, Guid.Empty, Guid.Empty);

				foreach (SecurityCheckpoint checkPoint in securityForTest.AllLoadedCheckPoints)
				{
					Assert(checkPoint.ToString() + " Should be Allowed", checkPoint.IsAllowed);
				}
			}
		}

		public void TestEverythingIsAllowedForCWService()
		{
			string cwServiceLogin = User.ServiceUserName;

			using (Env.SetTemporaryUserContext(cwServiceLogin, BranchPK, DepartmentPK))
			{
				GlbStaff cwServiceUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_LoginName, cwServiceLogin));
				Assert("CWServiceUser.GS_IsSystemAccount", cwServiceUser.GS_IsSystemAccount);
				AssertEquals("CWServiceUser.GS_Code", "~BP", cwServiceUser.GS_Code);

				SecurityCore securityForTest = new SecurityCore(null, cwServiceUser, Guid.Empty, Guid.Empty, Guid.Empty);

				foreach (SecurityCheckpoint checkPoint in securityForTest.AllLoadedCheckPoints)
				{
					Assert(checkPoint.ToString() + " Should be Allowed", checkPoint.IsAllowed);
				}
			}
		}

		public void TestChangingCompanyBranchDepartmentUser()
		{
			var branch = (GlbBranch)Factory.NewWithValidTestData(typeof(GlbBranch));
			Guid newBranchPK = branch.PK.ToGuid();

			var department = (GlbDepartment)Factory.NewWithValidTestData(typeof(GlbDepartment));
			Guid newDepartmentPK = department.PK.ToGuid();

			var company = (GlbCompany)Factory.NewWithValidTestData(typeof(GlbCompany));
			Guid newCompanyPK = company.PK.ToGuid();

			SecurityCore testSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			string securityRight = testSecurity.MaintainConsol.Code;

			GlbSecurity userCompanyLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userCompanyLevelSecurityRecord.GU_SecurityItemIsAllowed = false;
			userCompanyLevelSecurityRecord.GU_SecurityRight = securityRight;
			userCompanyLevelSecurityRecord.GU_GS = fStaff.PK;
			userCompanyLevelSecurityRecord.GU_GC = newCompanyPK;
			fSecurityCollection.Add(userCompanyLevelSecurityRecord);

			GlbSecurity companyDepartmentLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			companyDepartmentLevelSecurityRecord.GU_SecurityItemIsAllowed = true;
			companyDepartmentLevelSecurityRecord.GU_SecurityRight = securityRight;
			companyDepartmentLevelSecurityRecord.GU_GS = fStaff.PK;
			companyDepartmentLevelSecurityRecord.GU_GC = newCompanyPK;
			companyDepartmentLevelSecurityRecord.GU_GE = newDepartmentPK;
			fSecurityCollection.Add(companyDepartmentLevelSecurityRecord);

			GlbSecurity branchDepartmentLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			branchDepartmentLevelSecurityRecord.GU_SecurityItemIsAllowed = false;
			branchDepartmentLevelSecurityRecord.GU_SecurityRight = securityRight;
			branchDepartmentLevelSecurityRecord.GU_GS = fStaff.PK;
			branchDepartmentLevelSecurityRecord.GU_GB = newBranchPK;
			branchDepartmentLevelSecurityRecord.GU_GE = newDepartmentPK;
			fSecurityCollection.Add(branchDepartmentLevelSecurityRecord);

			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;

			Factory.Save();
			Assert("Is not Allowed", !aSecurity.MaintainConsol.IsAllowed);

			aSecurity.CompanyPK = newCompanyPK;
			Assert("Is not Allowed", !aSecurity.MaintainConsol.IsAllowed);

			aSecurity.DepartmentPK = newDepartmentPK;
			Assert("Is Allowed", aSecurity.MaintainConsol.IsAllowed);

			aSecurity.BranchPK = newBranchPK;
			Assert("Is not Allowed", !aSecurity.MaintainConsol.IsAllowed);

			aSecurity.LockDownUserCompanyDepartmentBranch();

			try
			{
				aSecurity.DepartmentPK = Env.CurrentDepartment.PK;
				Fail("InvalidOperationException should be thrown");
			}
			catch (InvalidOperationException)
			{
			}

			try
			{
				aSecurity.BranchPK = Env.CurrentBranch.PK;
				Fail("InvalidOperationException should be thrown");
			}
			catch (InvalidOperationException)
			{
			}

			try
			{
				aSecurity.CompanyPK = Env.CurrentCompany.PK;
				Fail("InvalidOperationException should be thrown");
			}
			catch (InvalidOperationException)
			{
			}
		}

		public void TestSecurityCheckpointCodesAreCorrectLength()
		{
			BooleanRegistryDataType booleanRegistryDataType = new BooleanRegistryDataType();

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.SetBinaryValue(
					"AllowFuturePostingOfCashBookTransactions",
					Env.CurrentCompany.PK,
					Guid.Empty,
					booleanRegistryDataType.Serialise(true),
					Guid.Empty,
					"BIN",
					true,
					false);
			}

			Dictionary<string, string> uniqueTable = new Dictionary<string, string>();
			FieldInfo[] fieldInfos = typeof(SecurityCore).GetFields();

			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);

			foreach (FieldInfo info in fieldInfos)
			{
				var targetType = info.FieldType;
				var baseType = typeof(SecurityCheckpoint);
				if (targetType.IsSubclassOf(baseType) || targetType == baseType)
				{
					SecurityCheckpoint checkPoint = (SecurityCheckpoint)info.GetValue(aSecurity);
					AssertNotNull("CheckPoint " + info.Name + " should be instantiated on construction of Security", checkPoint);
					Assert("Security." + info.Name + ".Code cannot be greater than max-length of GlbSecurity.GU_SecurityRight", checkPoint.Code.Length <= GlbSecuritySchema.GU_SecurityRight.MaxLength);
					Assert("Code must be unique: " + checkPoint.Code, !uniqueTable.ContainsKey(checkPoint.Code));
					uniqueTable.Add(checkPoint.Code, checkPoint.Code);
				}
			}
		}

		public void TestThatDeletedRowExceptionWillNotOccurWhenGroupIsDeleted()
		{
			SecurityCore testSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			string securityRight = testSecurity.MaintainConsol.Code;

			GlbGroup group1 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(group1);
			GlbGroup group2 = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(group2);

			GlbSecurity securityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_SecurityRight = securityRight;
			securityRecord.GU_GG = group2.PK;
			fSecurityCollection.Add(securityRecord); // this group right will be ignored, because user belongs also to group1 which as no restrictions.

			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;

			Factory.Save();
			AssertEquals("IsAllowed should be true when user belongs to a group with implicit rights", true, aSecurity.MaintainConsol.IsAllowed);

			//Now delete the group link that has implicit rights
			fStaff.Groups.Remove(group1);

			Factory.Save();
			AssertEquals("IsAllowed should be false when user belongs to only a group with rights denied", false, aSecurity.MaintainConsol.IsAllowed);
		}

		public void TestGroupParentRightsAreInherited()
		{
			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;

			GlbGroup group1 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			GlbGroup group2 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(group2);

			//Add Security rows
			string parentRight = aSecurity.CompanyTariffRates.Code;
			string childRight = aSecurity.CompanyTariffRatesNew.Code;

			//Group 1 to deny parent right but with no explicit child right
			GlbSecurity group1SecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group1SecurityRecord.GU_SecurityItemIsAllowed = false;
			group1SecurityRecord.GU_SecurityRight = parentRight;
			group1SecurityRecord.GU_GG = group1.PK;
			fSecurityCollection.Add(group1SecurityRecord);

			//Group 2 to explicitly grant parent right, but with explicit denial on child right
			GlbSecurity group2SecurityRecord1 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group2SecurityRecord1.GU_SecurityItemIsAllowed = true;
			group2SecurityRecord1.GU_SecurityRight = parentRight;
			group2SecurityRecord1.GU_GG = group2.PK;
			fSecurityCollection.Add(group2SecurityRecord1);

			GlbSecurity group2SecurityRecord2 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group2SecurityRecord2.GU_SecurityItemIsAllowed = false;
			group2SecurityRecord2.GU_SecurityRight = childRight;
			group2SecurityRecord2.GU_GG = group2.PK;
			fSecurityCollection.Add(group2SecurityRecord2);

			Factory.Save();
			//child rights should be denied
			AssertEquals("Child should be denied", false, aSecurity.CompanyTariffRatesNew.IsAllowed);

			GlbSecurityCollection securityCollection = (GlbSecurityCollection)aSecurity.GetGroupRightsWithImplicitRights(aSecurity.CompanyTariffRatesNew);
			AssertEquals("Security Collection Count", 3, securityCollection.Count);
			AssertEquals("SecurityRight for group 1", false, securityCollection[0].GU_SecurityItemIsAllowed);
			AssertEquals("SecurityRight for group 2", false, securityCollection[1].GU_SecurityItemIsAllowed);
		}

		#region TestGetGroupRightsWithImplicitRights

		public void TestGetGroupRightsWithImplicitRights()
		{
			GlbGroup group1 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			group1.GG_Code = "G1";
			group1.GG_Desc = "Group 1";
			fStaff.Groups.Add(group1);

			GlbGroup group2 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			group2.GG_Code = "G2";
			group2.GG_Desc = "Group 2";
			fStaff.Groups.Add(group2);

			GlbGroup group3 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			group3.GG_Code = "G3";
			group3.GG_Desc = "Group 3";
			group3.GG_IsActive = false;
			fStaff.Groups.Add(group3);

			SecurityCore security = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			security.CachingEnabled = false;

			string securityRight = security.MaintainConsol.Code;

			GlbSecurity userLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userLevelSecurityRecord.GU_SecurityItemIsAllowed = false;
			userLevelSecurityRecord.GU_SecurityRight = securityRight;
			userLevelSecurityRecord.GU_ItemGUID = Guid.Empty;
			userLevelSecurityRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(userLevelSecurityRecord); // this user right will not be picked up

			GlbSecurity group2CompanyLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group2CompanyLevelSecurityRecord.GU_SecurityItemIsAllowed = false;
			group2CompanyLevelSecurityRecord.GU_SecurityRight = securityRight;
			group2CompanyLevelSecurityRecord.GU_GG = group2.PK;
			group2CompanyLevelSecurityRecord.GU_GC = CompanyPK;

			fSecurityCollection.Add(group2CompanyLevelSecurityRecord);

			GlbSecurity group3CompanyLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group3CompanyLevelSecurityRecord.GU_SecurityItemIsAllowed = false;
			group3CompanyLevelSecurityRecord.GU_SecurityRight = securityRight;
			group3CompanyLevelSecurityRecord.GU_GG = group3.PK;
			group3CompanyLevelSecurityRecord.GU_GC = CompanyPK;

			Factory.Save();
			AssertSecurityForGetGroupRightsWithImplicitRights(security, security.MaintainConsol, security.MaintainConsol.Code, group1.PK, group2.PK);
			AssertSecurityForGetGroupRightsWithImplicitRights(security, security.MaintainConsolEdit, security.MaintainConsolEdit.Code, group1.PK, group2.PK);
		}

		class GlbSecurityTest
		{
			public GlbSecurityTest(ZString securityRight, ZGuid groupPK, ZGuid staffPK, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, ZBool securityItemIsAllowed)
			{
				SecurityRight = securityRight;
				GroupPK = groupPK;
				StaffPK = staffPK;
				CompanyPK = companyPK;
				BranchPK = branchPK;
				DepartmentPK = departmentPK;
				SecurityItemIsAllowed = securityItemIsAllowed;
			}

			public string GetAssertMessage()
			{
				return string.Format("Expected: \r\n{0}", GetPropertiesAsString(this));
			}

			static string GetPropertiesAsString(GlbSecurityTest securityTest)
			{
				StringBuilder stringBuilder = new StringBuilder();

				Type type = securityTest.GetType();
				var fields = type.GetFields();

				foreach (var field in fields)
				{
					stringBuilder.Append(string.Format("{0}: {1}\r\n", field.Name, field.GetValue(securityTest)));
				}

				return stringBuilder.ToString();
			}

			public readonly ZString SecurityRight;
			public ZString GroupCode { get { return ""; } }
			public readonly ZGuid GroupPK;
			public readonly ZGuid StaffPK;
			public readonly ZGuid CompanyPK;
			public readonly ZGuid BranchPK;
			public readonly ZGuid DepartmentPK;
			public readonly ZBool SecurityItemIsAllowed;
		}

		void AssertSecurityForGetGroupRightsWithImplicitRights(SecurityCore security, SecurityCheckpoint checkpoint, string securityRight, ZGuid pk1, ZGuid pk2)
		{
			GlbSecurityCollection securityRightsCollection = (GlbSecurityCollection)security.GetGroupRightsWithImplicitRights(checkpoint);
			AssertEquals("Four security rights should be found", 4, securityRightsCollection.Count);

			var allGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));

			var securityTests = new[] {
				new GlbSecurityTest(securityRight, groupPK: pk1, staffPK: ZGuid.Empty, companyPK: ZGuid.Empty, branchPK: ZGuid.Empty, departmentPK: ZGuid.Empty, securityItemIsAllowed: true),
				new GlbSecurityTest(securityRight, groupPK: allGroup.PK, staffPK: ZGuid.Empty, companyPK: ZGuid.Empty, branchPK: ZGuid.Empty, departmentPK: ZGuid.Empty, securityItemIsAllowed: false),
				new GlbSecurityTest(securityRight, groupPK: pk2, staffPK: ZGuid.Empty, companyPK: ZGuid.Empty, branchPK: ZGuid.Empty, departmentPK: ZGuid.Empty, securityItemIsAllowed: true),
				new GlbSecurityTest(security.MaintainConsol.Code, groupPK: pk2, staffPK: ZGuid.Empty, companyPK: CompanyPK, branchPK: ZGuid.Empty, departmentPK: ZGuid.Empty, securityItemIsAllowed: false), // This security right is loaded for all children
			};

			foreach (var securityTest in securityTests)
			{
				var actualSecurity = securityRightsCollection.Where(s =>
					s.GU_SecurityRight == securityTest.SecurityRight
					&& s.Group.PK == securityTest.GroupPK
					&& (securityTest.CompanyPK.IsEmpty || (s.Company != null && s.Company.PK == securityTest.CompanyPK)))
				.FirstOrDefault();

				var assertMessage = securityTest.GetAssertMessage();

				CombineAssertions(assertMessage, () =>
				{
					AssertEquals("Staff", securityTest.StaffPK, actualSecurity.GU_GS);
					AssertEquals("Company", securityTest.CompanyPK, actualSecurity.GU_GC);
					AssertEquals("Branch", securityTest.BranchPK, actualSecurity.GU_GB);
					AssertEquals("Department", securityTest.DepartmentPK, actualSecurity.GU_GE);
					AssertEquals("Security Item is allowed", securityTest.SecurityItemIsAllowed, actualSecurity.GU_SecurityItemIsAllowed);
				});
			}
		}

		public void TestGetGroupRightsWithImplicitRightsGathersAllRelevantRights()
		{
			var branches = Enumerable.Range(0, 3).Select(b => (GlbBranch)Factory.NewWithValidTestData(typeof(GlbBranch))).Select(b => b.PK.ToGuid()).ToArray();
			var companies = Enumerable.Range(0, 3).Select(b => (GlbCompany)Factory.NewWithValidTestData(typeof(GlbCompany))).Select(b => b.PK.ToGuid()).ToArray();
			var departments = Enumerable.Range(0, 3).Select(b => (GlbDepartment)Factory.NewWithValidTestData(typeof(GlbDepartment))).Select(b => b.PK.ToGuid()).ToArray();

			Guid gEa = departments[0];
			Guid gEb = departments[1];
			Guid gEc = departments[2];

			Guid gCa = companies[0];
			Guid gCb = companies[1];
			Guid gCc = companies[2];

			Guid gBa = branches[0];
			Guid gBb = branches[1];
			Guid gBc = branches[2];

			GlbGroup group1 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			group1.GG_Code = "G1";
			group1.GG_Desc = "Group 1";
			fStaff.Groups.Add(group1);

			SecurityCore security = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			security.CachingEnabled = false;

			string securityRightParent = security.MaintainConsol.Parent.Code;
			string securityRight = security.MaintainConsol.Code;

			GlbSecurity userLevelSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userLevelSecurityRecord.GU_SecurityItemIsAllowed = false;
			userLevelSecurityRecord.GU_SecurityRight = securityRight;
			userLevelSecurityRecord.GU_ItemGUID = Guid.Empty;
			userLevelSecurityRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(userLevelSecurityRecord); // this user right will not be picked up

			GlbSecurity baseSecurityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			baseSecurityRecord.GU_SecurityItemIsAllowed = false;
			baseSecurityRecord.GU_SecurityRight = securityRight;
			baseSecurityRecord.GU_ItemGUID = security.MaintainConsol.ItemGuid;
			baseSecurityRecord.GU_GG = group1.PK;
			baseSecurityRecord.GU_GC = gCa;
			baseSecurityRecord.GU_GE = gEa;
			baseSecurityRecord.GU_GB = gBa;
			fSecurityCollection.Add(baseSecurityRecord);

			GlbSecurity parentSecurityRecord0 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord0.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord0.GU_SecurityRight = securityRightParent;
			parentSecurityRecord0.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord0.GU_GG = group1.PK;
			parentSecurityRecord0.GU_GC = gCa;
			parentSecurityRecord0.GU_GE = gEa;
			parentSecurityRecord0.GU_GB = gBb;
			fSecurityCollection.Add(parentSecurityRecord0);

			GlbSecurity parentSecurityRecord1 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord1.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord1.GU_SecurityRight = securityRightParent;
			parentSecurityRecord1.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord1.GU_GG = group1.PK;
			parentSecurityRecord1.GU_GC = gCb;
			parentSecurityRecord1.GU_GE = gEb;
			parentSecurityRecord1.GU_GB = gBb;
			fSecurityCollection.Add(parentSecurityRecord1);

			GlbSecurity parentSecurityRecord2 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord2.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord2.GU_SecurityRight = securityRightParent;
			parentSecurityRecord2.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord2.GU_GG = group1.PK;
			parentSecurityRecord2.GU_GC = gCa;
			parentSecurityRecord2.GU_GE = gEa;
			parentSecurityRecord2.GU_GB = gBa;
			fSecurityCollection.Add(parentSecurityRecord2);

			GlbSecurity parentSecurityRecord3 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord3.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord3.GU_SecurityRight = securityRightParent;
			parentSecurityRecord3.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord3.GU_GG = group1.PK;
			parentSecurityRecord3.GU_GC = gCa;
			parentSecurityRecord3.GU_GE = gEb;
			fSecurityCollection.Add(parentSecurityRecord3);

			GlbSecurity parentSecurityRecord4 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord4.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord4.GU_SecurityRight = securityRightParent;
			parentSecurityRecord4.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord4.GU_GG = group1.PK;
			parentSecurityRecord4.GU_GE = gEa;
			fSecurityCollection.Add(parentSecurityRecord4);

			GlbSecurity parentSecurityRecord5 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord5.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord5.GU_SecurityRight = securityRightParent;
			parentSecurityRecord5.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord5.GU_GG = group1.PK;
			parentSecurityRecord5.GU_GE = gEc;
			fSecurityCollection.Add(parentSecurityRecord5);

			GlbSecurity parentSecurityRecord6 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			parentSecurityRecord6.GU_SecurityItemIsAllowed = false;
			parentSecurityRecord6.GU_SecurityRight = securityRightParent;
			parentSecurityRecord6.GU_ItemGUID = security.MaintainConsol.Parent.ItemGuid;
			parentSecurityRecord6.GU_GG = group1.PK;
			fSecurityCollection.Add(parentSecurityRecord6);

			Factory.Save();
			GlbSecurityCollection securityRightsCollection = (GlbSecurityCollection)security.GetGroupRightsWithImplicitRights(security.MaintainConsol);

			AssertEquals("User right should not be contained", securityRightsCollection.Contains(userLevelSecurityRecord), false);
			AssertEquals("Base security record should be contained", securityRightsCollection.Contains(baseSecurityRecord), true);
			AssertEquals("Same weight but partially different area = should be contained", securityRightsCollection.Contains(parentSecurityRecord0), true);
			AssertEquals("Same weight and completely different area = should be contained", securityRightsCollection.Contains(parentSecurityRecord1), true);
			AssertEquals("Same weight and same area = should not be contained", securityRightsCollection.Contains(parentSecurityRecord2), false);
			AssertEquals("Lower weight and partially different area = should be contained", securityRightsCollection.Contains(parentSecurityRecord3), true);
			AssertEquals("Lower weight but complete overlap = should be contained", securityRightsCollection.Contains(parentSecurityRecord4), true);
			AssertEquals("Same weight and completely different area = should be contained", securityRightsCollection.Contains(parentSecurityRecord5), true);
			AssertEquals("Weight 0 = should be contained and end the collection for group G1", securityRightsCollection.Contains(parentSecurityRecord6), true);
		}

		#endregion

		public void TestUserBelongToGroupWithNoExplicitRights()
		{
			SecurityCore aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurityItem.CachingEnabled = false;
			string securityRight = aSecurityItem.MaintainConsol.Code;

			GlbGroup group1 = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			GlbGroup group2 = (GlbGroup)Factory.NewWithValidTestData(typeof(GlbGroup));
			fStaff.Groups.Add(group2);

			GlbSecurity group2Record = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group2Record.GU_SecurityItemIsAllowed = false;
			group2Record.GU_SecurityRight = securityRight;
			group2Record.GU_GG = group2.PK;
			fSecurityCollection.Add(group2Record); // this group right will be ignored, because user belongs also to group1 which as no restrictions.

			GlbSecurity group1Record = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group1Record.GU_SecurityItemIsAllowed = false;
			group1Record.GU_SecurityRight = "another";
			group1Record.GU_GG = group1.PK;
			fSecurityCollection.Add(group1Record); // this group right will be ignored, as its for a different checkpoint

			Factory.Save();
			AssertEquals("IsAllowed when user belongs to a group with implicit rights", true, aSecurityItem.MaintainConsol.IsAllowed);
		}

		public void TestWhenParentsGroupDeniesRight()
		{
			SecurityCore aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurityItem.CachingEnabled = false;

			GlbGroup group1 = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			//Add Security rows
			string parentRight = aSecurityItem.CompanyTariffRates.Code;
			string childRight = aSecurityItem.CompanyTariffRatesNew.Code;

			//Group 1 to deny parent right but with no explicit child right
			GlbSecurity group1Record = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group1Record.GU_SecurityItemIsAllowed = false;
			group1Record.GU_SecurityRight = parentRight;
			group1Record.GU_GG = group1.PK;
			fSecurityCollection.Add(group1Record);

			//Grant staff level right
			GlbSecurity userRightRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userRightRecord.GU_SecurityItemIsAllowed = true;
			userRightRecord.GU_SecurityRight = childRight;
			userRightRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(userRightRecord);

			//child right should be allowed
			AssertEquals("Child IsAllowed", true, aSecurityItem.CompanyTariffRatesNew.IsAllowed);
		}

		public void TestStaffParentRightsOverridesGroupParentRights()
		{
			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurity.CachingEnabled = false;

			GlbGroup group1 = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			//Add Security rows
			string parentRight = aSecurity.CompanyTariffRates.Code;
			string childRight = aSecurity.CompanyTariffRatesNew.Code;

			//Group 1 to deny group parent right
			GlbSecurity group1Record = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			group1Record.GU_SecurityItemIsAllowed = false;
			group1Record.GU_SecurityRight = parentRight;
			group1Record.GU_GG = group1.PK;
			fSecurityCollection.Add(group1Record);

			AssertEquals("Child IsAllowed", false, aSecurity.CompanyTariffRatesNew.IsAllowed);

			//Grant staff parent right
			GlbSecurity userRightRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			userRightRecord.GU_SecurityItemIsAllowed = true;
			userRightRecord.GU_SecurityRight = parentRight;
			userRightRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(userRightRecord);

			AssertEquals("Child IsAllowed", true, aSecurity.CompanyTariffRatesNew.IsAllowed);
		}
		public void TestFindNullSecurityCheckpoint()
		{
			SecurityCore aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);

			foreach (SecurityCheckpoint checkPoint in aSecurity.AllLoadedCheckPoints)
			{
				Assert("Null Security CheckPoint ", checkPoint != null);
			}
		}

		public void TestGetDocumentTypeUploadCheckPoint()
		{
			TestDocumentTypeCheckPoint("eDocsUploadDocumentType", Env.Security.eDocsUploadSpecificDocumentType.Code, (securityItem, docType) =>
			{
				return securityItem.GetDocumentTypeUploadCheckPoint(docType);
			});
		}

		void TestDocumentTypeCheckPoint(string docTypeCheckPointCodePrefix, string checkPointCode, Func<SecurityCore, string, SecurityCheckpoint> getCheckPointForDocType)
		{
			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			var docType1 = (BusinessObject)Factory.New<IRefDocType>();
			docType1[RefDocTypeSchema.RT_DocType.Name] = "XZ1";
			docType1[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			var docType2 = (BusinessObject)Factory.New<IRefDocType>();
			docType2[RefDocTypeSchema.RT_DocType.Name] = "XZ2";
			docType2[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			var group1 = Factory.New<GlbGroup>();
			fStaff.Groups.Add(group1);

			var documentType1RightsRecord = Factory.New<GlbSecurity>();
			documentType1RightsRecord.GU_SecurityItemIsAllowed = false;
			documentType1RightsRecord.GU_SecurityRight = docTypeCheckPointCodePrefix + "XZ1";
			documentType1RightsRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(documentType1RightsRecord);

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			var documentType1CheckPoint = getCheckPointForDocType(aSecurityItem, "XZ1");
			AssertEquals("Type of checkpoint returned", typeof(SecurityCheckpointNonOperationalAllowed), documentType1CheckPoint.GetType());
			AssertEquals("DocumentType1 IsAllowed", expected: false, documentType1CheckPoint.IsAllowed);

			var documentType1CheckPointAgain = getCheckPointForDocType(aSecurityItem, "XZ1");
			AssertEquals("Getting PrintQueue1 again should return the same print queue", documentType1CheckPoint, documentType1CheckPointAgain);

			var documentType2CheckPoint = getCheckPointForDocType(aSecurityItem, "XZ2");
			AssertEquals("DocumentType2 IsAllowed", expected: true, documentType2CheckPoint.IsAllowed);

			AssertEquals("DocumentType that doesn't exist", expected: true, getCheckPointForDocType(aSecurityItem, "XZ3").IsAllowed);

			var documentTypesRightsRecord = Factory.New<GlbSecurity>();
			documentTypesRightsRecord.GU_SecurityItemIsAllowed = false;
			documentTypesRightsRecord.GU_SecurityRight = checkPointCode;
			documentTypesRightsRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(documentTypesRightsRecord);

			AssertEquals("DocumentType1 IsAllowed", expected: false, documentType1CheckPoint.IsAllowed);
			AssertEquals("DocumentType2 IsAllowed", expected: false, documentType2CheckPoint.IsAllowed);
			AssertEquals("DocumentType that doesn't exist", expected: false, getCheckPointForDocType(aSecurityItem, "XZ3").IsAllowed);

			var documentTypesCheckPoint = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(checkPointCode));
			AssertNotNull(documentTypesCheckPoint);
			Assert("DocumentTypesCheckPoint should have DocumentType1 as child", documentTypesCheckPoint.IsAncestorOf(documentType1CheckPoint));
			Assert("DocumentTypesCheckPoint should have DocumentType2 as child", documentTypesCheckPoint.IsAncestorOf(documentType2CheckPoint));
		}

		public void TestDeleteDocPermanentlyCheckPoint()
		{
			TestDocumentTypeCheckPoint("eDocsPermanentDelete", Env.Security.eDocsPermanentDelete.Code, (securityItem, docType) =>
			{
				return securityItem.GetDocumentTypePermanentDeleteCheckPoint(docType);
			});
		}

		public void TestGetDocumentTypeViewCheckPoint()
		{
			TestDocumentTypeCheckPoint("eDocsViewDocumentType", Env.Security.ViewSpecificEDocTypes.Code, (securityItem, docType) =>
			{
				return securityItem.GetDocumentTypeViewCheckPoint(docType);
			});
		}

		public void TestGetDocumentTypeCutCheckPoint()
		{
			TestDocumentTypeCheckPoint("eDocsCutDocumentType", Env.Security.CutSpecificEDocTypes.Code, (securityItem, docType) =>
			{
				return securityItem.GetDocumentTypeCutCheckPoint(docType);
			});
		}

		public void TestGetPrintQueueCheckPoint()
		{
			GlbSecurityCollection collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			BusinessObjectCollection printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory });
			printQueueCollection.Load(new ZQuery());
			printQueueCollection.RemoveAndDeleteAll();

			BusinessObject printQueue1 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue1[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue1";
			printQueue1[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue1";
			Factory.Save();

			BusinessObject printQueue2 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue2[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue2";
			printQueue2[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue2";
			Factory.Save();

			GlbGroup group1 = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			GlbSecurity printQueue1RightsRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			printQueue1RightsRecord.GU_SecurityItemIsAllowed = false;
			printQueue1RightsRecord.GU_SecurityRight = StmPrintQueueSchema.Constants.TableName;
			printQueue1RightsRecord.GU_ItemGUID = printQueue1.PK;
			printQueue1RightsRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(printQueue1RightsRecord);

			SecurityCore aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			SecurityCheckpoint printQueue1CheckPoint = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue1.PK.ToGuid()));
			AssertEquals("Type of checkpoint returned", typeof(SecurityCheckpointNonOperationalAllowed), printQueue1CheckPoint.GetType());
			AssertEquals("PrintQueue1 IsAllowed", false, printQueue1CheckPoint.IsAllowed);
			AssertEquals("PrintQueue1 security right should be StmPrintQueue", StmPrintQueueSchema.Constants.TableName, printQueue1CheckPoint.Code);
			AssertEquals("PrintQueue1 ItemGuid should be PrintQueue1 PK", printQueue1.PK, printQueue1CheckPoint.ItemGuid);

			SecurityCheckpoint printQueue1CheckPointAgain = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue1.PK.ToGuid()));
			AssertEquals("Getting PrintQueue1 again should return the same print queue", printQueue1CheckPoint, printQueue1CheckPointAgain);

			SecurityCheckpoint printQueue2CheckPoint = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue2.PK.ToGuid()));
			AssertEquals("PrintQueue2 IsAllowed", true, printQueue2CheckPoint.IsAllowed);
			AssertEquals("PrintQueue2 security right should be StmPrintQueue", StmPrintQueueSchema.Constants.TableName, printQueue2CheckPoint.Code);
			AssertEquals("PrintQueue2 ItemGuid should be PrintQueue2 PK", printQueue2.PK, printQueue2CheckPoint.ItemGuid);

			AssertEquals("PrintQueue that doesn't exist", true, aSecurityItem.GetPrintQueueCheckPoint(Guid.NewGuid(), "").IsAllowed);

			GlbSecurity printerRightsRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			printerRightsRecord.GU_SecurityItemIsAllowed = false;
			printerRightsRecord.GU_SecurityRight = aSecurityItem.PrintQueuesPrintTo.Code;
			printerRightsRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(printerRightsRecord);

			AssertEquals("PrintQueue1 IsAllowed", false, printQueue1CheckPoint.IsAllowed);
			AssertEquals("PrintQueue2 IsAllowed", false, printQueue2CheckPoint.IsAllowed);
			AssertEquals("PrintQueue that doesn't exist", false, aSecurityItem.GetPrintQueueCheckPoint(Guid.NewGuid(), "").IsAllowed);

			SecurityCheckpoint printQueuesCheckPoint = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(Env.Security.PrintQueuesPrintTo.Code));
			AssertNotNull(printQueuesCheckPoint);
			Assert("PrintQueuesCheckPoint should have PrintQueue1 as child", printQueuesCheckPoint.IsAncestorOf(printQueue1CheckPoint));
			Assert("PrintQueuesCheckPoint should have PrintQueue2 as child", printQueuesCheckPoint.IsAncestorOf(printQueue2CheckPoint));
		}

		public void TestGetSecurityCheckpoints_ServerNames()
		{
			var printQueue1 = Factory.New<IStmPrintQueue>();
			printQueue1.SQ_DisplayName = "Test Print Queue1";
			printQueue1.QueueName = "TestPrintQueue1";
			printQueue1.SQ_ServerName = "TestServer1";

			var printQueue2 = Factory.New<IStmPrintQueue>();
			printQueue2.SQ_DisplayName = "Test Print Queue21";
			printQueue2.QueueName = "TestPrintQueue21";
			printQueue2.SQ_ServerName = "TestServer2";

			var printQueue3 = Factory.New<IStmPrintQueue>();
			printQueue3.SQ_DisplayName = "Test Print Queue22";
			printQueue3.QueueName = "TestPrintQueue22";
			printQueue3.SQ_ServerName = "TestServer2";
			Factory.Save();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			var printQueuesCheckPoint = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(Env.Security.PrintQueuesPrintTo.Code));

			AssertEquals("2 servers", 2, printQueuesCheckPoint.ChildCheckPoints.Count());
			AssertEquals("Server1", "TestServer1", printQueuesCheckPoint.ChildCheckPoints.ElementAt(0).DisplayText);
			AssertEquals("Server1", printQueue1.SQ_SPS_Server, printQueuesCheckPoint.ChildCheckPoints.ElementAt(0).ItemGuid);
			AssertEquals("PrintQueue on Server1", "Test Print Queue1", printQueuesCheckPoint.ChildCheckPoints.ElementAt(0).ChildCheckPoints.ElementAt(0).DisplayText);
			AssertEquals("Server2", "TestServer2", printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).DisplayText);
			AssertEquals("Server2", printQueue2.SQ_SPS_Server, printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).ItemGuid);
			AssertEquals("PrintQueue on Server2", "Test Print Queue21", printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(0).DisplayText);
			AssertEquals("PrintQueue on Server2", "Test Print Queue22", printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(1).DisplayText);
		}

		public void TestGetAllPrintQueueCheckPoints()
		{
			GlbSecurityCollection collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			BusinessObjectCollection printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory });
			printQueueCollection.Load(new ZQuery());
			printQueueCollection.RemoveAndDeleteAll();

			SecurityCore aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			Factory.Save();
			SecurityCheckpoint[] printQueueCheckPoints = aSecurityItem.GetAllPrintQueueCheckPoint();
			AssertEquals("Count", 0, printQueueCheckPoints.Length);

			BusinessObject printQueue1 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue1[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue1";
			printQueue1[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue1";
			printQueue1[StmPrintQueueSchema.SQ_AllowPrinting.Name] = true;
			Factory.Save();

			BusinessObject printQueue2 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue2[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue2";
			printQueue2[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue2";
			printQueue2[StmPrintQueueSchema.SQ_AllowPrinting.Name] = true;
			Factory.Save();

			BusinessObject printQueue3 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue3[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue3";
			printQueue3[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue3";
			printQueue3[StmPrintQueueSchema.SQ_AllowPrinting.Name] = false;
			Factory.Save();

			printQueueCheckPoints = aSecurityItem.GetAllPrintQueueCheckPoint();
			AssertEquals("Count", 2, printQueueCheckPoints.Length);

			AssertEquals("Queue 1 DisplayText", "Test Print Queue1", printQueueCheckPoints[0].DisplayText);
			AssertEquals("Queue 2 DisplayText", "Test Print Queue2", printQueueCheckPoints[1].DisplayText);
		}

		public void TestReloadPrintCheckpoint()
		{
			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			var printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory });
			printQueueCollection.Load(new ZQuery());
			printQueueCollection.RemoveAndDeleteAll();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			Factory.Save();
			var printQueueCheckPoints = aSecurityItem.GetAllPrintQueueCheckPoint();
			AssertEquals("Count", 0, printQueueCheckPoints.Length);

			var printQueue1 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue1[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue1";
			printQueue1[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue1";
			printQueue1[StmPrintQueueSchema.SQ_AllowPrinting.Name] = true;

			Factory.Save();

			printQueueCheckPoints = aSecurityItem.GetAllPrintQueueCheckPoint();
			AssertEquals("Count", 1, printQueueCheckPoints.Length);

			var printQueue2 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue2[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue2";
			printQueue2[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue2";
			printQueue2[StmPrintQueueSchema.SQ_AllowPrinting.Name] = true;

			var printQueue3 = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printQueue3[StmPrintQueueSchema.SQ_DisplayName.Name] = "Test Print Queue3";
			printQueue3[StmPrintQueueSchema.SQ_QueueName.Name] = "TestPrintQueue3";
			printQueue3[StmPrintQueueSchema.SQ_AllowPrinting.Name] = true;

			Factory.Save();

			AssertEquals(null, aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue2.PK.ToGuid())));
			AssertEquals(null, aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue3.PK.ToGuid())));
			aSecurityItem.ReloadPrintCheckpoints();
			AssertNotEquals(null, aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue2.PK.ToGuid())));
			AssertNotEquals(null, aSecurityItem.FindCheckPoint(new CheckpointLookupKey(StmPrintQueueSchema.Constants.TableName, printQueue3.PK.ToGuid())));
			printQueueCheckPoints = aSecurityItem.GetAllPrintQueueCheckPoint();
			AssertEquals("Count", 3, printQueueCheckPoints.Length);
		}

		public void TestReloadPrintCheckpoint_ServerNames()
		{
			var printQueue1 = Factory.New<IStmPrintQueue>();
			printQueue1.SQ_DisplayName = "Test Print Queue1";
			printQueue1.QueueName = "TestPrintQueue1";
			printQueue1.SQ_ServerName = "TestServer1";

			var printQueue2 = Factory.New<IStmPrintQueue>();
			printQueue2.SQ_DisplayName = "Test Print Queue21";
			printQueue2.QueueName = "TestPrintQueue21";
			printQueue2.SQ_ServerName = "TestServer2";
			Factory.Save();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			var printQueuesCheckPoint = aSecurityItem.FindCheckPoint(new CheckpointLookupKey(Env.Security.PrintQueuesPrintTo.Code));

			AssertEquals("2 servers", 2, printQueuesCheckPoint.ChildCheckPoints.Count());
			AssertEquals("Server1", "TestServer1", printQueuesCheckPoint.ChildCheckPoints.ElementAt(0).DisplayText);
			AssertEquals("Server1", printQueue1.SQ_SPS_Server, printQueuesCheckPoint.ChildCheckPoints.ElementAt(0).ItemGuid);
			AssertEquals("PrintQueue on Server1", "Test Print Queue1", printQueuesCheckPoint.ChildCheckPoints.ElementAt(0).ChildCheckPoints.ElementAt(0).DisplayText);
			AssertEquals("Server2", "TestServer2", printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).DisplayText);
			AssertEquals("Server2", printQueue2.SQ_SPS_Server, printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).ItemGuid);
			AssertEquals("PrintQueue on Server2", "Test Print Queue21", printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(0).DisplayText);

			var printQueue3 = Factory.New<IStmPrintQueue>();
			printQueue3.SQ_DisplayName = "Test Print Queue22";
			printQueue3.QueueName = "TestPrintQueue22";
			printQueue3.SQ_ServerName = "TestServer2";

			var printQueue4 = Factory.New<IStmPrintQueue>();
			printQueue4.SQ_DisplayName = "Test Print Queue3";
			printQueue4.QueueName = "TestPrintQueue3";
			printQueue4.SQ_ServerName = "TestServer3";
			Factory.Save();

			aSecurityItem.ReloadPrintCheckpoints();

			AssertEquals("3 servers", 3, printQueuesCheckPoint.ChildCheckPoints.Count());
			AssertEquals("PrintQueue on Server2", "Test Print Queue22", printQueuesCheckPoint.ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(1).DisplayText);
			AssertEquals("Server3", "TestServer3", printQueuesCheckPoint.ChildCheckPoints.ElementAt(2).DisplayText);
			AssertEquals("Server3", printQueue4.SQ_SPS_Server, printQueuesCheckPoint.ChildCheckPoints.ElementAt(2).ItemGuid);
			AssertEquals("PrintQueue on Server3", "Test Print Queue3", printQueuesCheckPoint.ChildCheckPoints.ElementAt(2).ChildCheckPoints.ElementAt(0).DisplayText);
		}

		[TestDate(2021, 08, 27)]
		public void TestDbHitsWhileGettingPrintQueues()
		{
			var printQueue1 = Factory.New<IStmPrintQueue>();
			printQueue1.SQ_DisplayName = "Test Print Queue1";
			printQueue1.QueueName = "TestPrintQueue1";
			printQueue1.SQ_ServerName = "TestServer1";
			printQueue1.SQ_AllowPrinting = true;
			Factory.Save();

			var printQueue2 = Factory.New<IStmPrintQueue>();
			printQueue2.SQ_DisplayName = "Test Print Queue21";
			printQueue2.QueueName = "TestPrintQueue21";
			printQueue2.SQ_ServerName = "TestServer2";
			printQueue2.SQ_AllowPrinting = true;
			Factory.Save();

			var printQueue3 = Factory.New<IStmPrintQueue>();
			printQueue3.SQ_DisplayName = "Test Print Queue3";
			printQueue3.QueueName = "TestPrintQueue3";
			printQueue3.SQ_ServerName = "TestServer3";
			printQueue3.SQ_AllowPrinting = true;
			Factory.Save();

			var printQueue4 = Factory.New<IStmPrintQueue>();
			printQueue4.SQ_DisplayName = "Test Print Queue41";
			printQueue4.QueueName = "TestPrintQueue41";
			printQueue4.SQ_ServerName = "TestServer4";
			printQueue4.SQ_AllowPrinting = true;
			Factory.Save();

			var printQueue5 = Factory.New<IStmPrintQueue>();
			printQueue5.SQ_DisplayName = "Test Print Queue22";
			printQueue5.QueueName = "TestPrintQueue22";
			printQueue5.SQ_ServerName = "TestServer2";
			printQueue5.SQ_AllowPrinting = true;
			Factory.Save();

			var printQueue6 = Factory.New<IStmPrintQueue>();
			printQueue6.SQ_DisplayName = "Test Print Queue42";
			printQueue6.QueueName = "TestPrintQueue42";
			printQueue6.SQ_ServerName = "TestServer4";
			printQueue6.SQ_AllowPrinting = true;
			Factory.Save();

			var hits = new Dictionary<string, int>() { { StmPrintServerSchema.Constants.TableName, 1 } };
			Factory.ResetDatabaseLoadCount();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var staffSecurity = new SecurityCore(staff.StaffSecurityPermissionsCollection, staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK, false);
				using (AssertDbHitsWithUsefulQueryInformation(hits, ((ZSecurity)staffSecurity.SecurityInstance).Factory_Exposed, true))
				{
					staffSecurity.GetAllPrintQueueCheckPoint();
				}
			}
		}

		public void TestGetRegistryCheckpoints()
		{
			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			var printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory });
			printQueueCollection.Load(new ZQuery());
			printQueueCollection.RemoveAndDeleteAll();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			Factory.Save();
			var registryCheckpoints = aSecurityItem.GetAllRegistryCheckPoint();
			for (var i = 0; i < registryCheckpoints.Length; ++i)
			{
				var check1 = registryCheckpoints[i];
				var check2 = aSecurityItem.FindCheckPoint(check1.Code);
				AssertEquals(check1, check2);
				if (check2.Code.StartsWith("RegASS"))
				{
					var check3 = aSecurityItem.GetRegistryCheckPoint(check1.Code.Substring("RegASS".Length), check1.DisplayText);
					AssertEquals(check2, check3);
				}
			}
		}

		public void TestReloadDocumentCheckpoint_Upload()
		{
			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			var printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory });
			printQueueCollection.Load(new ZQuery());
			printQueueCollection.RemoveAndDeleteAll();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			Factory.Save();
			var documentCheckPoints = aSecurityItem.GetAllDocumentTypeUploadCheckPoint();
			var docAmount = documentCheckPoints.Length;

			var docType1 = (BusinessObject)Factory.New<IRefDocType>();
			docType1[RefDocTypeSchema.RT_DocType.Name] = "XZ1";
			docType1[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			documentCheckPoints = aSecurityItem.GetAllDocumentTypeUploadCheckPoint();
			AssertEquals("Count", docAmount + 1, documentCheckPoints.Length);

			var docType2 = (BusinessObject)Factory.New<IRefDocType>();
			docType2[RefDocTypeSchema.RT_DocType.Name] = "XZ2";
			docType2[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			AssertEquals(null, aSecurityItem.FindCheckPoint("eDocsUploadDocumentType" + "XZ2"));
			aSecurityItem.ReloadDocumentCheckpoints();
			AssertNotEquals(null, aSecurityItem.FindCheckPoint("eDocsUploadDocumentType" + "XZ2"));
			documentCheckPoints = aSecurityItem.GetAllDocumentTypeUploadCheckPoint();
			AssertEquals("Count", docAmount + 2, documentCheckPoints.Length);
		}

		public void TestReloadDocumentCheckpoint_View()
		{
			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			Factory.Save();
			var documentCheckPoints = aSecurityItem.GetAllDocumentTypeViewCheckPoint();
			var docAmount = documentCheckPoints.Length;

			var docType1 = (BusinessObject)Factory.New<IRefDocType>();
			docType1[RefDocTypeSchema.RT_DocType.Name] = "XZ1";
			docType1[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			documentCheckPoints = aSecurityItem.GetAllDocumentTypeViewCheckPoint();
			AssertEquals("Count", docAmount + 1, documentCheckPoints.Length);

			var docType2 = (BusinessObject)Factory.New<IRefDocType>();
			docType2[RefDocTypeSchema.RT_DocType.Name] = "XZ2";
			docType2[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			AssertNull(aSecurityItem.FindCheckPoint("eDocsViewDocumentType" + "XZ2"));
			aSecurityItem.ReloadDocumentCheckpoints();
			AssertNotNull(aSecurityItem.FindCheckPoint("eDocsViewDocumentType" + "XZ2"));
			documentCheckPoints = aSecurityItem.GetAllDocumentTypeViewCheckPoint();
			AssertEquals("Count", docAmount + 2, documentCheckPoints.Length);
		}

		public void TestReloadDocumentCheckpoint_PermanentDelete()
		{
			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			var aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty);
			aSecurityItem.CachingEnabled = false;

			Factory.Save();
			var documentCheckPoints = aSecurityItem.GetAllDocumentTypePermanentDeleteCheckPoint();
			var docAmount = documentCheckPoints.Length;

			var docType1 = (BusinessObject)Factory.New<IRefDocType>();
			docType1[RefDocTypeSchema.RT_DocType.Name] = "XZ1";
			docType1[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			documentCheckPoints = aSecurityItem.GetAllDocumentTypePermanentDeleteCheckPoint();
			AssertEquals("Count", docAmount + 1, documentCheckPoints.Length);

			var docType2 = (BusinessObject)Factory.New<IRefDocType>();
			docType2[RefDocTypeSchema.RT_DocType.Name] = "XZ2";
			docType2[RefDocTypeSchema.RT_ReferenceType.Name] = "ALL";

			Factory.Save();

			AssertNull(aSecurityItem.FindCheckPoint("eDocsPermanentDelete" + "XZ2"));
			aSecurityItem.ReloadDocumentCheckpoints();
			AssertNotNull(aSecurityItem.FindCheckPoint("eDocsPermanentDelete" + "XZ2"));
			documentCheckPoints = aSecurityItem.GetAllDocumentTypePermanentDeleteCheckPoint();
			AssertEquals("Count", docAmount + 2, documentCheckPoints.Length);
		}

		public void TestGetRatesSecurityCheckPoint()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("111", "The First One");
			codeDescriptionPairList.AddPair("222", "The Second One");
			codeDescriptionPairList.AddPair("333", "The Third One");

			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionPairList);

			var glbGroup = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(glbGroup);

			var glbSecurity1 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			glbSecurity1.GU_SecurityItemIsAllowed = false;
			glbSecurity1.GU_SecurityRight = "RatesSecurity111";
			glbSecurity1.GU_GS = fStaff.PK;
			fSecurityCollection.Add(glbSecurity1);

			var glbSecurity2 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			glbSecurity2.GU_SecurityItemIsAllowed = true;
			glbSecurity2.GU_SecurityRight = "RatesSecurity333";
			glbSecurity2.GU_GS = fStaff.PK;
			fSecurityCollection.Add(glbSecurity2);

			Factory.Save();
			var securityItem = new SecurityCore(fSecurityCollection, fStaff, Guid.Empty, Guid.Empty, Guid.Empty) { CachingEnabled = false };

			SecurityCheckpoint ratesSecurityCheckPoint = securityItem.FindCheckPoint(new CheckpointLookupKey(Env.Security.RatesSecurity.Code));
			AssertNotNull(ratesSecurityCheckPoint);
			Assert("Top Level Rates Security's IsAllowed", ratesSecurityCheckPoint.IsAllowed);

			SecurityCheckpoint ratesSecurity1CheckPoint = securityItem.FindCheckPoint(new CheckpointLookupKey("RatesSecurity111"));
			Assert("RatesSecurityCheckPoint should have RatesSecurity1 as child", ratesSecurityCheckPoint.IsAncestorOf(ratesSecurity1CheckPoint));
			AssertEquals("Type of checkpoint returned", typeof(SecurityCheckpoint), ratesSecurity1CheckPoint.GetType());
			Assert("RatesSecurity1 IsAllowed", !ratesSecurity1CheckPoint.IsAllowed);

			SecurityCheckpoint ratesSecurity1CheckPointAgain = securityItem.FindCheckPoint(new CheckpointLookupKey("ratesSecurity111"));
			AssertEquals("Getting ratesSecurity1 again should return the same print queue", ratesSecurity1CheckPoint, ratesSecurity1CheckPointAgain);

			SecurityCheckpoint ratesSecurity2CheckPoint = securityItem.FindCheckPoint(new CheckpointLookupKey("ratesSecurity222"));
			Assert("ratesSecurityCheckPoint should have ratesSecurity2 as child", ratesSecurityCheckPoint.IsAncestorOf(ratesSecurity2CheckPoint));
			Assert("ratesSecurity2 IsAllowed", ratesSecurity2CheckPoint.IsAllowed);

			SecurityCheckpoint ratesSecurity3CheckPoint = securityItem.FindCheckPoint(new CheckpointLookupKey("ratesSecurity333"));
			Assert("ratesSecurityCheckPoint should have ratesSecurity2 as child", ratesSecurityCheckPoint.IsAncestorOf(ratesSecurity3CheckPoint));
			Assert("ratesSecurity3 IsAllowed", ratesSecurity3CheckPoint.IsAllowed);

			var ratesSecurityRightsRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			ratesSecurityRightsRecord.GU_SecurityItemIsAllowed = false;
			ratesSecurityRightsRecord.GU_SecurityRight = securityItem.RatesSecurity.Code;
			ratesSecurityRightsRecord.GU_GS = fStaff.PK;
			fSecurityCollection.Add(ratesSecurityRightsRecord);

			Assert("Top Level Rate Group Security's IsAllowed", !ratesSecurityCheckPoint.IsAllowed);
			Assert("ratesSecurity1 IsAllowed", !ratesSecurity1CheckPoint.IsAllowed);
			Assert("ratesSecurity2 IsAllowed", !ratesSecurity2CheckPoint.IsAllowed);
			Assert("ratesSecurity3 IsAllowed", ratesSecurity3CheckPoint.IsAllowed);
		}

		public void TestGroupChildRightsOverrideParent()
		{
			SecurityCore aSecurityItem = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
			aSecurityItem.CachingEnabled = false;

			GlbGroup group1 = (GlbGroup)Factory.New(typeof(GlbGroup));
			fStaff.Groups.Add(group1);

			string parentRight = aSecurityItem.CompanyTariffRates.Code;
			string childRight = aSecurityItem.CompanyTariffRatesNew.Code;

			//Group 1 to deny group parent right
			GlbSecurity groupSecurityRecord1 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			groupSecurityRecord1.GU_SecurityItemIsAllowed = false;
			groupSecurityRecord1.GU_SecurityRight = parentRight;
			groupSecurityRecord1.GU_GG = group1.PK;
			fSecurityCollection.Add(groupSecurityRecord1);

			AssertEquals("Parent IsAllowed", false, aSecurityItem.CompanyTariffRates.IsAllowed);
			AssertEquals("Child IsAllowed", false, aSecurityItem.CompanyTariffRatesNew.IsAllowed);

			//Group1 to allow child right
			GlbSecurity groupSecurityRecord2 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			groupSecurityRecord2.GU_SecurityItemIsAllowed = true;
			groupSecurityRecord2.GU_SecurityRight = childRight;
			groupSecurityRecord2.GU_GG = group1.PK;
			fSecurityCollection.Add(groupSecurityRecord2);

			Factory.Save();
			AssertEquals("Parent IsAllowed", false, aSecurityItem.CompanyTariffRates.IsAllowed);
			AssertEquals("Child IsAllowed", true, aSecurityItem.CompanyTariffRatesNew.IsAllowed);

			//Staff denies Parent right
			GlbSecurity staffSecurityRecord1 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			staffSecurityRecord1.GU_SecurityItemIsAllowed = false;
			staffSecurityRecord1.GU_SecurityRight = parentRight;
			staffSecurityRecord1.GU_GS = fStaff.PK;
			fSecurityCollection.Add(staffSecurityRecord1);

			Factory.Save();
			AssertEquals("Parent IsAllowed", false, aSecurityItem.CompanyTariffRates.IsAllowed);
			AssertEquals("Child IsAllowed", false, aSecurityItem.CompanyTariffRatesNew.IsAllowed);

			//Staff explicitly allowing Child right
			GlbSecurity staffSecurityRecord2 = (GlbSecurity)Factory.New(typeof(GlbSecurity));
			staffSecurityRecord2.GU_SecurityItemIsAllowed = true;
			staffSecurityRecord2.GU_SecurityRight = childRight;
			staffSecurityRecord2.GU_GS = fStaff.PK;
			fSecurityCollection.Add(staffSecurityRecord2);

			Factory.Save();
			AssertEquals("Parent IsAllowed", false, aSecurityItem.CompanyTariffRates.IsAllowed);
			AssertEquals("Child IsAllowed", true, aSecurityItem.CompanyTariffRatesNew.IsAllowed);
		}

		public void TestWhenLoggingInAsNonOperationalUserAndViewingSecurityRightsInForm()
		{
			var nonOperationalUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_IsOperational, false));

			using (Env.SetTemporaryUserContext(nonOperationalUser.GS_LoginName, BranchPK, DepartmentPK))
			{
				GlbStaff nonOperationalStaff = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.PK, Env.CurrentUser.PK));
				SecurityCore aSecurity = new SecurityCore(fSecurityCollection, nonOperationalStaff, BranchPK, DepartmentPK, CompanyPK);

				GlbSecurity securityRecord = (GlbSecurity)Factory.New(typeof(GlbSecurity));
				securityRecord.GU_SecurityItemIsAllowed = true;
				securityRecord.GU_SecurityRight = aSecurity.Forwarding.Code;
				securityRecord.GU_GS = fStaff.PK;
				fSecurityCollection.Add(securityRecord);

				AssertEquals("Forwarding IsAllowed", false, aSecurity.Forwarding.IsAllowed);
				aSecurity = new SecurityCore(fSecurityCollection, fStaff, BranchPK, DepartmentPK, CompanyPK);
				AssertEquals("Forwarding IsAllowed", true, aSecurity.Forwarding.IsAllowed);
			}
		}

		public void TestShowErrorAndGetErrorMessageForNotAllowedForSingleSecurityCheckpoint()
		{
			var nonOperationalUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_IsOperational, false));

			using (Env.SetTemporaryUserContext(nonOperationalUser.GS_LoginName, BranchPK, DepartmentPK))
			{
				SecurityForTest testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				SecurityCheckpoint checkPoint = new SecurityCheckpoint("Security Right", (NoResString)"Security Right", null, testSecurity.ZSecurityInstance);

				Env.Security.ShowError(checkPoint);

				AssertEquals("Administrator logged in", SecurityCore.NonOperationalErrorMessage,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("GetErrorMessageForNotAllowed()", SecurityCore.NonOperationalErrorMessage, Env.Security.GetErrorMessageForNotAllowed(checkPoint));

				var filter1 = new ZQuery(GlbStaffSchema.GS_IsOperational, SQLComparisonOperator.Equal, 1);
				var filter2 = new ZQuery(GlbStaffSchema.GS_IsController, SQLComparisonOperator.Equal, 'N');

				GlbStaff operationalUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(filter1, filter2));

				using (Env.SetTemporaryUserContext(operationalUser.GS_LoginName, BranchPK, DepartmentPK))
				{
					Env.Security.ShowError(checkPoint);
					string expectedMessage = SecurityCore.SecurityErrorMessage + System.Environment.NewLine + System.Environment.NewLine + "Security Right";
					AssertEquals("Non-Administrator logged in", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("GetErrorMessageForNotAllowed()", expectedMessage, Env.Security.GetErrorMessageForNotAllowed(checkPoint));
				}
			}

			using (Env.SetTemporaryUserContext("invalidName", BranchPK, DepartmentPK))
			{
				var testSecurity = new SecurityForTest(null, Guid.NewGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				var checkPoint = new SecurityCheckpoint("Security Right", (NoResString)"Security Right", null, testSecurity.ZSecurityInstance);

				Env.Security.ShowError(checkPoint);

				AssertEquals("Invalid logged in", SecurityCore.NonOperationalErrorMessage,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(SecurityCore.NonOperationalErrorMessage, Env.Security.GetErrorMessageForNotAllowed(checkPoint));
			}
		}

		public void TestShowErrorAndGetErrorMessageForNotAllowedForMultipleSecurityCheckpoint()
		{
			var nonOperationalUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_IsOperational, false));

			using (Env.SetTemporaryUserContext(nonOperationalUser.GS_LoginName, BranchPK, DepartmentPK))
			{
				SecurityForTest testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				SecurityCheckpoint checkPoint1 = new SecurityCheckpoint("Security Right1", (NoResString)"Security Right1", null, testSecurity.ZSecurityInstance);
				SecurityCheckpoint checkPoint2 = new SecurityCheckpoint("Security Right2", (NoResString)"Security Right2", null, testSecurity.ZSecurityInstance);

				SecurityCheckpoint[] deniedCheckpoints = new SecurityCheckpoint[] { checkPoint1, checkPoint2 };
				Env.Security.ShowError(deniedCheckpoints);

				AssertEquals("Administrator logged in", SecurityCore.NonOperationalErrorMessage,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("GetErrorMessageForNotAllowed()", SecurityCore.NonOperationalErrorMessage, Env.Security.GetErrorMessageForNotAllowed(deniedCheckpoints));

				var filter1 = new ZQuery(GlbStaffSchema.GS_IsOperational, SQLComparisonOperator.Equal, 1);
				var filter2 = new ZQuery(GlbStaffSchema.GS_IsController, SQLComparisonOperator.Equal, 'N');

				GlbStaff operationalUser = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(filter1, filter2));

				using (Env.SetTemporaryUserContext(operationalUser.GS_LoginName, BranchPK, DepartmentPK))
				{
					Env.Security.ShowError(deniedCheckpoints);
					string expectedMessage = SecurityCore.SecurityErrorMessage + System.Environment.NewLine + System.Environment.NewLine + "Security Right1" + System.Environment.NewLine + "Security Right2";
					AssertEquals("Non-Administrator logged in", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("GetErrorMessageForNotAllowed()", expectedMessage, Env.Security.GetErrorMessageForNotAllowed(deniedCheckpoints));
				}
			}

			using (Env.SetTemporaryUserContext("invalidName", BranchPK, DepartmentPK))
			{
				var testSecurity = new SecurityForTest(null, Guid.NewGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				var checkPoint1 = new SecurityCheckpoint("Security Right1", (NoResString)"Security Right1", null, testSecurity.ZSecurityInstance);
				var checkPoint2 = new SecurityCheckpoint("Security Right2", (NoResString)"Security Right2", null, testSecurity.ZSecurityInstance);

				var deniedCheckpoints = new SecurityCheckpoint[] { checkPoint1, checkPoint2 };
				Env.Security.ShowError(deniedCheckpoints);

				AssertEquals("Invalid logged in", SecurityCore.NonOperationalErrorMessage,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(SecurityCore.NonOperationalErrorMessage, Env.Security.GetErrorMessageForNotAllowed(deniedCheckpoints));
			}
		}

		public void TestMultipleCallsToFindCheckPointDoesNotHitDb_Upload()
		{
			Env.Security.GetDocumentTypeUploadCheckPoint("foo");
			RegistryItemDictionary.Instance.PurgeAll();
			using (Db.Connection.TrackExecutedCommands())
			{
				Env.Security.GetDocumentTypeUploadCheckPoint("foo");
				AssertContainsExactElementsInAnyOrder("Second call to find a check point should not hit the registry or db", Array.Empty<string>(), Db.Connection.ExecutedCommands);
			}
		}

		public void TestMultipleCallsToFindCheckPointDoesNotHitDb_View()
		{
			Env.Security.GetDocumentTypeViewCheckPoint("foo");
			RegistryItemDictionary.Instance.PurgeAll();
			using (Db.Connection.TrackExecutedCommands())
			{
				Env.Security.GetDocumentTypeViewCheckPoint("foo");
				AssertContainsExactElementsInAnyOrder("Second call to find a check point should not hit the registry or db", Array.Empty<string>(), Db.Connection.ExecutedCommands);
			}
		}

		public void TestRatesSecurityCheckpointsLoadedByFindCheckPoint()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranchPK = newCompany.Branches.AddNew().PK.ToGuid();
			Factory.Save();

			var ratesSecurityOptions = new CodeDescriptionPairList();
			ratesSecurityOptions.AddPair("RS1", "Rates' Security Option 1");
			ratesSecurityOptions.AddPair("RS2", "Rates' Security Option 2");
			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ratesSecurityOptions);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, newBranchPK, Env.CurrentDepartment.PK))
			{
				var security = new SecurityCore(fSecurityCollection, fStaff, newBranchPK, Guid.Empty, Guid.Empty);
				security.CachingEnabled = false;

				var message = "This test is based on the assumption that Env.Security.TariffsAndRates is the parent of Env.Security.RatesSecurity";
				var parentCheckPoint = security.FindCheckPoint(security.TariffsAndRates.Code);
				AssertNotNull(message, parentCheckPoint);

				message = "Pre-condition: by default this registry has no children but we want to ensure that it's loaded when FindCheckPoint also needs to loadProviders";
				var ratesSecurityCheckPointChildCheckPoints = parentCheckPoint.ChildCheckPoints.FirstOrDefault(x => x.Code == security.RatesSecurity.Code).ChildCheckPoints;
				AssertEquals(message, 2, ratesSecurityCheckPointChildCheckPoints.Count());

				message = "Should find the check points added in the registry";
				ratesSecurityCheckPointChildCheckPoints = security.FindCheckPoint(security.RatesSecurity.Code).ChildCheckPoints;
				AssertEquals(message, 2, ratesSecurityCheckPointChildCheckPoints.Count());
			}
		}

		public void TestRatesSecurityCheckpointsReloadedWhenQueryingRateSecurityCheckPoints()
		{
			AssertNull(Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "FOO"));

			var list = new CodeDescriptionPairList();
			list.AddPair("FOO", "Whatever");
			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, list);

			AssertNotNull(Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "FOO"));
		}

		public void TestUserSwitchPerformance()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ZAC";
			Factory.Save();

			var security = new ZSecurity(null, (GlbStaff)null, BranchPK, DepartmentPK, CompanyPK);
			var factoryBeforeReset = security.Factory_Exposed;
			((IZSecurity)security).ResetData(null, staff.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty, false);
			var factoryAfterReset = security.Factory_Exposed;

			AssertEquals("Factory instance shouldn't be reset after ResetData for improve performance", factoryAfterReset, factoryBeforeReset);
		}

		public void TestDisableRefreshBusForServiceTasks()
		{
			var security = new ZSecurity(null, (GlbStaff)null, BranchPK, DepartmentPK, CompanyPK);
			AssertEquals("I don't know whether or not it is possible to update the current security object from the UI so, leaving refresh enabled for safety",
				true, security.Factory_Exposed.RefreshEnabled);
			Globals.IsUserInteractive = false;
			security = new ZSecurity(null, (GlbStaff)null, BranchPK, DepartmentPK, CompanyPK);
			AssertEquals("Service tasks should never update the current security instance, so having refreshbus is pointless.", false, security.Factory_Exposed.RefreshEnabled);
		}

		#region Implementation

		GlbSecurityCollection fSecurityCollection;
		GlbStaff fStaff;
		Guid BranchPK;
		Guid CompanyPK;
		Guid DepartmentPK;
		bool wasController;

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "First Controller Staff";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;

			Factory.Save();

			GlbGroup allUsersGroup = DenyPermissionsOnAllUsersGroup();
			fStaff = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.PK, Env.CurrentUser.PK));
			wasController = fStaff.GS_IsController;
			fStaff.GS_IsController = false;
			if (!allUsersGroup.Staff.Contains(fStaff.PK))
			{
				allUsersGroup.Staff.Add(fStaff);
			}

			fSecurityCollection = new GlbSecurityCollection(Factory);
			fSecurityCollection.Load();

			BranchPK = Env.CurrentBranch.PK;
			CompanyPK = Env.CurrentCompany.PK;
			DepartmentPK = Env.CurrentDepartment.PK;

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			fStaff.GS_IsController = wasController;

			fStaff.Factory.Save();
		}

		GlbGroup DenyPermissionsOnAllUsersGroup()
		{
			// Limit the check points for tests to avoid StressTestAtttribute
			var testCheckPoints = new[] { Env.Security.Operations.Code, Env.Security.MaintainConsol.Code, Env.Security.MaintainConsolEdit.Code, Env.Security.PrintQueuesPrintTo.Code, Env.Security.RatesSecurity.Code, Env.Security.CompanyTariffRates.Code, Env.Security.CompanyTariffRatesNew.Code, Env.Security.Forwarding.Code };

			GlbGroup allUsersGroup = (GlbGroup)Factory.LoadTop1(typeof(GlbGroup), new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
			foreach (SecurityCheckpoint checkPoint in Env.Security.AllLoadedCheckPoints.Where(s => testCheckPoints.Contains(s.Code)))
			{
				var securityRecord = LoadOrCreateSecurity(checkPoint.Code, allUsersGroup.PK);
				securityRecord.GU_SecurityItemIsAllowed = false;
				allUsersGroup.SecurityPermissions.Add(securityRecord);
			}
			return allUsersGroup;
		}

		GlbSecurity LoadOrCreateSecurity(string securityRight, ZGuid groupPK)
		{
			var filter = new ZQuery(GlbSecuritySchema.GU_SecurityRight, securityRight);
			filter.AddToFilter(new ZQuery(GlbSecuritySchema.GU_GG, groupPK));
			var security = Factory.LoadTop1<GlbSecurity>(filter);

			if (security == null)
			{
				security = (GlbSecurity)Factory.New(typeof(GlbSecurity));
				security.GU_SecurityRight = securityRight;
				security.GU_GG = groupPK;
			}

			return security;
		}

		#endregion
	}
}
