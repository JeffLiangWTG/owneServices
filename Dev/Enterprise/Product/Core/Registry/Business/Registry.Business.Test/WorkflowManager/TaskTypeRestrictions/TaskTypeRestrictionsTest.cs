using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TaskTypeRestrictions))]
	sealed class TaskTypeRestrictionsTest : RegistryBusinessObjectTest
	{
		#region Validation

		public void TestScope_DefaultValue_ShouldBeValidWithOrWithoutBufferManagement()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var restrictions = (TaskTypeRestrictions)GetNewBusinessObject();

			AssertEquals(ScopeList.Codes.Workflow, restrictions.Scope);
			AssertNoErrors(restrictions.ScopeInfo);

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			restrictions = (TaskTypeRestrictions)GetNewBusinessObject();

			AssertEquals(ScopeList.Codes.Job, restrictions.Scope);
			AssertNoErrors(restrictions.ScopeInfo);
		}

		public void TestScopeValidation_WhenBufferManagementDisabled_ShouldPreventAnythingOtherThanJob()
		{
			var restrictions = (TaskTypeRestrictions)GetNewBusinessObject();

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			foreach (ICodeDescription scope in new ScopeList())
			{
				restrictions.Scope = scope.Code;

				AssertNoErrors(restrictions.ScopeInfo);
			}

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			foreach (ICodeDescription scope in new ScopeList())
			{
				restrictions.Scope = scope.Code;

				if (scope.Code == ScopeList.Codes.Job)
				{
					AssertNoErrors(restrictions.ScopeInfo);
				}
				else
				{
					AssertHasError(restrictions.ScopeInfo, "Please enter a valid selection from the drop down list.");
				}
			}
		}

		public void TestValidation_DIFRestrictionType_NONNotificationType()
		{
			var result = (TaskTypeRestrictions)GetNewBusinessObject();

			result.Active = true;
			result.WorkflowType = "WKI";
			result.Code = "INV";
			result.Description = (NoResString)"Investigation";
			result.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			result.Scope = ScopeList.Codes.Workflow;
			result.NotificationType = NotificationTypeList.Codes.None;

			AssertEquals(true, result.NotificationTypeInfo.HasError("The combination of Restriction Type [DIF] and Notification Type [NON] would have no effect on resource assignments."));
		}

		public void TestTaskTypesCollection()
		{
			var newTaskTypes = new RestrictedTaskTypesCollection();
			var oldTaskTypes = BizObj.TaskTypesCollection;
			Assert(BizObj.IsRegisteredEditableChildObject(oldTaskTypes));

			BizObj.SetTaskTypes(newTaskTypes);
			Assert(!BizObj.IsRegisteredEditableChildObject(oldTaskTypes));
			Assert(BizObj.IsRegisteredEditableChildObject(newTaskTypes));
			AssertEquals(newTaskTypes, BizObj.TaskTypesCollection);
		}

		public void TestConflictingRestrictions_CyclicalConflict()
		{
			var collection = new TaskTypeRestrictionsCollection();
			var restriction1 = collection.AddNew();
			restriction1.Active = true;
			restriction1.WorkflowType = "WKI";
			restriction1.TaskType = "INV";
			restriction1.Description = (NoResString)"Investigation";
			restriction1.RestrictionType = RestrictionTypeList.Codes.SameResource;
			restriction1.Scope = ScopeList.Codes.Workflow;
			restriction1.NotificationType = NotificationTypeList.Codes.Error;

			var restriction1_child1 = restriction1.TaskTypesCollection.AddNew();
			restriction1_child1.Code = "CDU";

			restriction1.RunPreSaveValidation();
			AssertNoErrors(restriction1.ActiveInfo);

			var restriction2 = collection.AddNew();
			restriction2.Active = true;
			restriction2.WorkflowType = "WKI";
			restriction2.TaskType = "CDU";
			restriction2.Description = (NoResString)"Code Unit Test";
			restriction2.RestrictionType = RestrictionTypeList.Codes.SameResource;
			restriction2.Scope = ScopeList.Codes.Workflow;
			restriction2.NotificationType = NotificationTypeList.Codes.Error;

			var restriction2_child1 = restriction2.TaskTypesCollection.AddNew();
			restriction2_child1.Code = "CDF";

			restriction1.RunPreSaveValidation();
			AssertNoErrors(restriction1.ActiveInfo);
			AssertNoErrors(restriction2.ActiveInfo);

			var restriction3 = collection.AddNew();
			restriction3.Active = true;
			restriction3.WorkflowType = "WKI";
			restriction3.TaskType = "CDF";
			restriction3.Description = (NoResString)"Code Functionality";
			restriction3.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			restriction3.Scope = ScopeList.Codes.Workflow;
			restriction3.NotificationType = NotificationTypeList.Codes.Error;

			var restriction3_child1 = restriction3.TaskTypesCollection.AddNew();
			restriction3_child1.Code = "INV";

			restriction1.RunPreSaveValidation();
			AssertHasError(restriction1.ActiveInfo, "Conflict detected with Workflow Type: [WKI], Task Type: [CDF], Restriction Type: [DIF].");
			AssertHasError(restriction2.ActiveInfo, "Conflict detected with Workflow Type: [WKI], Task Type: [CDF], Restriction Type: [DIF].");
		}

		public void TestConflictingRestrictions_DirectConflict()
		{
			var collection = new TaskTypeRestrictionsCollection();
			var restriction1 = collection.AddNew();
			restriction1.Active = true;
			restriction1.WorkflowType = "WKI";
			restriction1.TaskType = "INV";
			restriction1.Description = (NoResString)"Investigation";
			restriction1.RestrictionType = RestrictionTypeList.Codes.SameResource;
			restriction1.Scope = ScopeList.Codes.Workflow;
			restriction1.NotificationType = NotificationTypeList.Codes.Error;

			var restriction1_child1 = restriction1.TaskTypesCollection.AddNew();
			restriction1_child1.Code = "CDU";
			var restriction1_child2 = restriction1.TaskTypesCollection.AddNew();
			restriction1_child1.Code = "CDF";
			var restriction1_child3 = restriction1.TaskTypesCollection.AddNew();
			restriction1_child1.Code = "CRF";

			restriction1.RunPreSaveValidation();
			AssertNoErrors(restriction1.ActiveInfo);

			var restriction2 = collection.AddNew();
			restriction2.Active = true;
			restriction2.WorkflowType = "WKI";
			restriction2.TaskType = "CRF";
			restriction2.Description = (NoResString)"Code Review";
			restriction2.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			restriction2.Scope = ScopeList.Codes.Workflow;
			restriction2.NotificationType = NotificationTypeList.Codes.Error;

			var restriction2_child1 = restriction2.TaskTypesCollection.AddNew();
			restriction2_child1.Code = "INV";

			restriction1.RunPreSaveValidation();
			AssertHasError(restriction1.ActiveInfo, "Conflict detected with Workflow Type: [WKI], Task Type: [CRF], Restriction Type: [DIF].");
		}

		public void TestConflictingRestrictions_NoConflict()
		{
			var collection = new TaskTypeRestrictionsCollection();
			var restriction1 = collection.AddNew();
			restriction1.Active = true;
			restriction1.WorkflowType = "WKI";
			restriction1.TaskType = "INV";
			restriction1.Description = (NoResString)"Investigation";
			restriction1.RestrictionType = RestrictionTypeList.Codes.SameResource;
			restriction1.Scope = ScopeList.Codes.Workflow;
			restriction1.NotificationType = NotificationTypeList.Codes.Error;

			var restriction1_child1 = restriction1.TaskTypesCollection.AddNew();
			restriction1_child1.Code = "CDU";
			var restriction1_child2 = restriction1.TaskTypesCollection.AddNew();
			restriction1_child1.Code = "CDF";

			restriction1.RunPreSaveValidation();
			AssertNoErrors(restriction1.ActiveInfo);

			var restriction2 = collection.AddNew();
			restriction2.Active = true;
			restriction2.WorkflowType = "WKI";
			restriction2.TaskType = "CRF";
			restriction2.Description = (NoResString)"Code Review";
			restriction2.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			restriction2.Scope = ScopeList.Codes.Workflow;
			restriction2.NotificationType = NotificationTypeList.Codes.Error;

			var restriction2_child1 = restriction2.TaskTypesCollection.AddNew();
			restriction2_child1.Code = "INV";

			restriction1.RunPreSaveValidation();
			AssertNoErrors(restriction1.ActiveInfo);
			AssertNoErrors(restriction2.ActiveInfo);
		}

		public void TestValidationShouldNotChangeObjectState()
		{
			var result = (TaskTypeRestrictions)GetNewBusinessObject();
			result.WorkflowType = "WKI";
			result.TaskType = "INV";
			AssertEquals("Description should be set from the code/pair list", "Investigation", result.Description);

			//this is Registry specific non persistent BO which cannot be created by Factory
			//therefore we cannot save this BO in the Factory to clear HasChanges property
			//so clear HasChanges manually for testing purpose only
			result.HasChanges = false;

			result.RunPreSaveValidation();
			Assert(@"Validation should not set HasChanges property to true because 
validation is not supposed to change a business object.
We should not set BO properties in validation methods.", !result.HasChanges);
		}

		#endregion

		public void TestCodeWillNotExceedMaxLength()
		{
			var bizo = (TaskTypeRestrictions)GetNewBusinessObject();
			AssertEquals("Workflow Type Max Length", 3, bizo.WorkflowTypeInfo.MaxLength);
			AssertEquals("Task Type Max Length", 3, bizo.TaskTypeInfo.MaxLength);
			AssertEquals("Scope Max Length", 3, bizo.ScopeInfo.MaxLength);
		}

		#region RegistryBusinessObject.Test Overrides

		public override void TestCloneValues()
		{
			var boToClone = (TaskTypeRestrictions)GetBusinessObjectToClone();
			var newObject = (TaskTypeRestrictions)boToClone.Clone(null, null);

			Assert(!boToClone.TaskTypesCollection.Equals(newObject.TaskTypesCollection));
			AssertCloneValues(newObject);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("Custom MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedTypes = (TaskTypeRestrictions)clone;
			AssertEquals(true, clonedTypes.Active);
			AssertEquals("WKI", clonedTypes.WorkflowType);
			AssertEquals("INV", clonedTypes.TaskType);
			AssertEquals("Investigation", clonedTypes.Description);
			AssertEquals(RestrictionTypeList.Codes.DifferentResource, clonedTypes.RestrictionType);
			AssertEquals(ScopeList.Codes.Workflow, clonedTypes.Scope);
			AssertEquals(NotificationTypeList.Codes.Error, clonedTypes.NotificationType);

			AssertEquals(2, clonedTypes.TaskTypesCollection.Count);
			AssertEquals("WKI", clonedTypes.TaskTypesCollection[0].WorkflowType);
			AssertEquals("COD", clonedTypes.TaskTypesCollection[0].Code);
			AssertEquals("Coding", clonedTypes.TaskTypesCollection[0].Description);
			AssertEquals("WKI", clonedTypes.TaskTypesCollection[1].WorkflowType);
			AssertEquals("CRF", clonedTypes.TaskTypesCollection[1].Code);
			AssertEquals("Code Review", clonedTypes.TaskTypesCollection[1].Description);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (TaskTypeRestrictions)GetNewBusinessObject();

			result.Active = true;
			result.WorkflowType = "WKI";
			result.TaskType = "INV";
			result.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			result.Scope = ScopeList.Codes.Workflow;
			result.NotificationType = NotificationTypeList.Codes.Error;

			var taskType1 = result.TaskTypesCollection.AddNew();
			taskType1.Code = "COD";

			var taskType2 = result.TaskTypesCollection.AddNew();
			taskType2.Code = "CRF";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new TaskTypeRestrictions BizObj
		{
			get { return (TaskTypeRestrictions)base.BizObj; }
		}

		protected override string CodeDisplayName
		{
			get { return "[Workflow Type : Task Type : Scope] combination"; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowDataRegistryTestHelper.SetupTaskTypesRegistry();
			Factory.Save();
		}

		#endregion
	}
}
