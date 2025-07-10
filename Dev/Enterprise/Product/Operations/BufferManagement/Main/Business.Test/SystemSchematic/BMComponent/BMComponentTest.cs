using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponent))]
	public class BMComponentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBMComponent_AutoAssignTaskAgeReadlonly()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 200);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 1);

			AssertEquals(buffer.FC_AutoAssignTasksAgeInfo.ReadOnly, false);
			AssertEquals(bucket.FC_AutoAssignTasksAgeInfo.ReadOnly, true);
		}

		public void TestCloneBMComponent()
		{
			var system = Factory.New<BMSystem>();

			var component = system.Components.AddNew();
			component.FC_Name = "Pls comply";

			var glbGroup = Factory.New<GlbGroup>();
			var multiplier = component.ZoneCapacityMultipliers.AddNew();
			multiplier.BZC_GG_ReleaseGroup = glbGroup.PK;
			multiplier.BZC_Zone0Multiplier = 2m;

			var childBuffer = component.ChildComponents.AddNew();
			childBuffer.FC_Name = "Child complying";
			var clone = (BMComponent)component.Clone();

			AssertEquals(component.FC_Name, clone.FC_Name);

			AssertNotEquals(component.ZoneCapacityMultipliers.Single().PK, clone.ZoneCapacityMultipliers.Single().PK);
			AssertEquals(glbGroup.PK, clone.ZoneCapacityMultipliers.Single().BZC_GG_ReleaseGroup);
			AssertEquals(2m, clone.ZoneCapacityMultipliers.Single().BZC_Zone0Multiplier);

			AssertNotEquals(component.ChildComponents.Single().PK, clone.ChildComponents.Single().PK);
			AssertEquals(component.ChildComponents.Single().FC_Name, clone.ChildComponents.Single().FC_Name);
		}

		public void TestDelete_ShouldDeleteResourceLinks()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var resourceLink = bucket.GetOrCreateResourceLink(resource.GS_Code);

			Factory.Save();

			bucket.Delete();
			Factory.Save();

			AssertEquals(true, resourceLink.IsDeleted);
		}

		[TestDate(2014, 3, 31, 2, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestGetZone()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 3 * 60);
			BMSTestHelper.LinkComponents(bucket, buffer);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 1);

			Factory.Save();

			var context = WorkingTimeContext.Create(buffer);

			AssertEquals(buffer, workflow.CurrentComponent);
			AssertEquals(3, buffer.GetZone(workflow, context));

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-60);
			Factory.Save();

			AssertEquals(2, buffer.GetZone(workflow, context));

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-120);
			Factory.Save();

			AssertEquals(1, buffer.GetZone(workflow, context));

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-180);
			Factory.Save();

			AssertEquals(0, buffer.GetZone(workflow, context));

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-1000);
			Factory.Save();

			AssertEquals(0, buffer.GetZone(workflow, context));
		}

		public void TestCCRAndNonCCRMultipliers_ShouldBeSetWhenSettingComponentType()
		{
			var component = Factory.New<BMSystem>().Components.AddNew();

			AssertEquals(BMComponentTypeList.Codes.Bucket, component.FC_Type);
			AssertEquals(1m, component.FC_BufferTimeCapacityConstraintThresholdMultiple);
			AssertEquals(1m, component.FC_NonCCRTemporaryOverloadLimitMultiplier);

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertEquals(BMConstants.DefaultBufferTimeCapacityConstraintThresholdMultiple, component.FC_BufferTimeCapacityConstraintThresholdMultiple);
			AssertEquals(BMConstants.DefaultNonCCRTemporaryOverloadLimitMultiplier, component.FC_NonCCRTemporaryOverloadLimitMultiplier);

			component.FC_BufferTimespanInMinutes = 60;

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertEquals(1m, component.FC_BufferTimeCapacityConstraintThresholdMultiple);
			AssertEquals(1m, component.FC_NonCCRTemporaryOverloadLimitMultiplier);
			AssertEquals(0, component.FC_BufferTimespanInMinutes);
		}

		public void TestIsBuffer()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			AssertEquals(false, buffer.IsBuffer);

			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertEquals(true, buffer.IsBuffer);

			var childBuffer = buffer.ChildComponents.AddNew();
			AssertEquals(BMComponentTypeList.Codes.Buffer, childBuffer.FC_Type);
			AssertEquals(true, childBuffer.IsBuffer);
		}

		public void TestIsChildBuffer()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			AssertEquals(false, buffer.IsBuffer);

			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertEquals(false, buffer.IsChildBuffer);

			var childBuffer = buffer.ChildComponents.AddNew();
			AssertEquals(BMComponentTypeList.Codes.Buffer, childBuffer.FC_Type);
			AssertEquals(true, childBuffer.IsChildBuffer);

			childBuffer.FC_Type = BMComponentTypeList.Codes.Constraint;
			AssertEquals(false, childBuffer.IsChildBuffer);
		}

		public void TestBufferTimeSpanHours()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.BufferTimespan = new ZDateTime(2012, 1, 1, 11, 30, 0);
			AssertEquals(11.5, component.BufferTimeSpanHours);

			component.BufferTimespan = new ZDateTime(2012, 1, 13, 6, 0, 0);
			AssertEquals(294d, component.BufferTimeSpanHours);      // 12.25 days
		}

		public void TestBufferTimespan()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			component.FC_BufferTimespanInMinutes = 61;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(61), component.BufferTimespan);

			component.FC_BufferTimespanInMinutes = 0;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(0), component.BufferTimespan);

			component.FC_BufferTimespanInMinutes = 1000;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(1000), component.BufferTimespan);
		}

		public void TestSetParentComponentFK_AlsoSetsSystemFK()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			var childComponent = component.ChildComponents.AddNew();
			AssertEquals(system, childComponent.System);
		}

		public void TestIBranchDepartmentProviderReturnsCorrectly_Buffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var parentBuffer = BMSTestHelper.CreateBuffer(system);
			var babyBuffer = parentBuffer.ChildComponents.AddNew();

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var newDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			babyBuffer.FC_GB_AgingBranch = newBranch.PK;
			babyBuffer.FC_GE_AgingDepartment = newDepartment.PK;

			AssertEquals("IBranchDepartmentProvider should return the branch of parent", parentBuffer.AgingBranch, BMSTestHelper.GetBranch(babyBuffer, Factory));
			AssertEquals("IBranchDepartmentProvider should return the department of parent", parentBuffer.AgingDepartment, BMSTestHelper.GetDepartment(babyBuffer, Factory));

			parentBuffer.FC_GB_AgingBranch = ZGuid.Empty;
			parentBuffer.FC_GE_AgingDepartment = ZGuid.Empty;
			AssertNull(parentBuffer.AgingBranch);
			AssertNull(parentBuffer.AgingDepartment);

			AssertEquals("IBranchDepartmentProvider should return the babybuffers branch if no parent is available", newBranch, BMSTestHelper.GetBranch(babyBuffer, Factory));
			AssertEquals("IBranchDepartmentProvider should return the babybuffers department if no parent is available", newDepartment, BMSTestHelper.GetDepartment(babyBuffer, Factory));

			babyBuffer.FC_GB_AgingBranch = ZGuid.Empty;
			babyBuffer.FC_GE_AgingDepartment = ZGuid.Empty;

			AssertNull("If the component and parent component has no branch, null should be returned. SAD!", BMSTestHelper.GetBranch(babyBuffer, Factory));
			AssertNull("If the component and parent component has no department, null should be returned. SAD!", BMSTestHelper.GetDepartment(babyBuffer, Factory));

			ErrorReporter.Clear();
		}

		public void TestIBranchDepartmentProviderReturnsCorrectly_Bucket()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var newDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var bucket = BMSTestHelper.CreateBucket(system);
			AssertEquals("BUC", bucket.FC_Type);
			bucket.FC_GB_AgingBranch = ZGuid.Empty;
			bucket.FC_GE_AgingDepartment = ZGuid.Empty;

			AssertEquals("Buckets can fall back to the current branch/department because they aren't used by the release gate.", Env.CurrentBranch.Code, ((IBranchDepartmentProvider)bucket).GetBranch(Factory)?.GB_Code);
			AssertEquals("Buckets can fall back to the current branch/department because they aren't used by the release gate.", Env.CurrentDepartment.Code, ((IBranchDepartmentProvider)bucket).GetDepartment(Factory)?.GE_Code);

			bucket.FC_GB_AgingBranch = newBranch.PK;
			bucket.FC_GE_AgingDepartment = newDepartment.PK;

			AssertEquals("If a branch/department is specified on the bucket that should be used instead of the current one.", newBranch.GB_Code, ((IBranchDepartmentProvider)bucket).GetBranch(Factory)?.GB_Code);
			AssertEquals("Buckets can fall back to the current branch/department because they aren't used by the release gate.", newDepartment.GE_Code, ((IBranchDepartmentProvider)bucket).GetDepartment(Factory)?.GE_Code);
		}

		public void TestComponentRelationshipTypeIsCorrect()
		{
			var component = BMSTestHelper.CreateComponentRelationship(Factory);
			var code = component.FC_Type;
			var description = new BMComponentTypeList().GetDescriptionFromCode(code);

			AssertEquals("This code is the old code for 'Component View' because changing it would require a data transform AND a constraint update", "CVW", code);
			AssertEquals("Component Relationship", description);
		}

		#region Branch and Department

		public void TestGetBranchShouldErrorReport_WhenCurrentBranchIsNull()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "DUM");
				var bucket = BMSTestHelper.CreateBucket(system);

				AssertNull("Precondition: ParentComponent", bucket.ParentComponent);
				AssertNull("Precondition: AgingBranch", bucket.AgingBranch);

				var branch = ((IBranchDepartmentProvider)bucket).GetBranch(Factory);
				AssertNull(branch);
				AssertEquals("GlbBranch.GetCurrentBranch returned null branch", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		public void TestGetDepartmentShouldErrorReport_WhenCurrentDepartmentIsNull()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "DUM");
				var bucket = BMSTestHelper.CreateBucket(system);

				AssertNull("Precondition: ParentComponent", bucket.ParentComponent);
				AssertNull("Precondition: AgingDepartment", bucket.AgingDepartment);

				var department = ((IBranchDepartmentProvider)bucket).GetDepartment(Factory);
				AssertNull(department);
				AssertEquals("GlbBranch.GetCurrentDepartment returned null department", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		public void TestGetBranchShouldErrorReport_WhenAgingBranchIsNotSetOnBuffer()
		{
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, currentBranch.PK.ToGuid(), currentDepartment.PK.ToGuid()))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "DUM");
				var buffer = BMSTestHelper.CreateBuffer(system);
				buffer.FC_GB_AgingBranch = ZGuid.Empty;

				AssertNull("Precondition: ParentComponent", buffer.ParentComponent);
				AssertNull("Precondition: AgingBranch", buffer.AgingBranch);

				var branch = ((IBranchDepartmentProvider)buffer).GetBranch(Factory);
				AssertNull(branch);
				AssertEquals("The buffer has no aging branch which should not be allowed by validation", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestGetDepartmentShouldErrorReport_WhenAgingDepartmentIsNotSetOnBuffer()
		{
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, currentBranch.PK.ToGuid(), currentDepartment.PK.ToGuid()))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "DUM");
				var buffer = BMSTestHelper.CreateBuffer(system);
				buffer.FC_GE_AgingDepartment = ZGuid.Empty;

				AssertNull("Precondition: ParentComponent", buffer.ParentComponent);
				AssertNull("Precondition: AgingDepartment", buffer.AgingDepartment);

				var department = ((IBranchDepartmentProvider)buffer).GetDepartment(Factory);
				AssertNull(department);
				AssertEquals("The buffer has no aging department which should not be allowed by validation", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestGetBranchShouldNotErrorReport_WhenAgingBranchIsNotSetOnBucket()
		{
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, currentBranch.PK.ToGuid(), currentDepartment.PK.ToGuid()))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "DUM");
				var bucket = BMSTestHelper.CreateBucket(system);
				bucket.FC_GB_AgingBranch = ZGuid.Empty;

				AssertNull("Precondition: ParentComponent", bucket.ParentComponent);
				AssertNull("Precondition: AgingBranch", bucket.AgingBranch);

				var branch = ((IBranchDepartmentProvider)bucket).GetBranch(Factory);
				AssertNotNull(branch);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestGetDepartmentShouldNotErrorReport_WhenAgingDepartmentIsNotSetOnBucket()
		{
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			var currentDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, currentBranch.PK.ToGuid(), currentDepartment.PK.ToGuid()))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "DUM");
				var bucket = BMSTestHelper.CreateBucket(system);
				bucket.FC_GE_AgingDepartment = ZGuid.Empty;

				AssertNull("Precondition: ParentComponent", bucket.ParentComponent);
				AssertNull("Precondition: AgingDepartment", bucket.AgingDepartment);

				var department = ((IBranchDepartmentProvider)bucket).GetDepartment(Factory);
				AssertNotNull(department);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		#region TypeDecider

		class TypeDeciderTest : TestCaseWithFactory
		{
			public void TestGetTypeForNewAndBinding()
			{
				AssertEquals(typeof(BMComponent), BMComponent.TypeDecider.GetTypeForNew());
				AssertEquals(typeof(BMComponent), BMComponent.TypeDecider.GetTypeForBinding());
			}

			public void TestLoad()
			{
				var componentRelationship = Factory.NewWithValidTestData<BMComponent>();
				componentRelationship.FC_FS_System = Guid.Empty;
				componentRelationship.FC_Type = BMComponentTypeList.Codes.ComponentRelationship;
				BMComponent component = Factory.NewWithValidTestData<BMComponent>();

				Factory.Save();

				BusinessObjectFactory newFactory = Factory.CreateNewFactory();
				var loadedComponentRelationship = newFactory.Load<BMComponent>(componentRelationship.PK);
				AssertType<ComponentRelationship>("BMComponent Type decider loaded ComponentRelationship with wrong class.", loadedComponentRelationship);

				var loadedBMComponent = newFactory.Load<BMComponent>(component.PK);
				AssertType<BMComponent>("BMComponent Type decider loaded BMComponent wrong class.", loadedBMComponent);
			}

			public void TestCloneComponentRelationship_ShouldReturnCorrectType()
			{
				var componentRelationship = Factory.NewWithValidTestData<ComponentRelationship>();
				var clone = componentRelationship.Clone();

				AssertType<ComponentRelationship>(clone);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return component;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return AddValidCollectionData((BMComponent)base.GetBusinessObjectForFetchForLoad());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return AddValidCollectionData((BMComponent)base.GetNewBusinessObjectForDeleteTest(factory));
		}

		BusinessObject AddValidCollectionData(BMComponent component)
		{
			foreach (var childComponent in component.ChildComponents)
			{
				childComponent.FC_BufferTimespanInMinutes = 60;
			}

			return component;
		}

		BMComponent component;

		protected override void SetUp()
		{
			base.SetUp();

			var system = Factory.NewWithValidTestData<BMSystem>();
			component = Factory.NewWithValidTestData<BMComponent>();
			component.FC_FS_System = system.PK;
		}

		#endregion
	}
}
