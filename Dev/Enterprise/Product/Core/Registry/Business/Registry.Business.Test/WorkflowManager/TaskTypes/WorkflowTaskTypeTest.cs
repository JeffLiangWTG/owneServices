using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowTaskType))]
	sealed class WorkflowTaskTypeTest : CodeDescriptionBoolTest
	{
		#region Validation

		public void TestValidateCode_Success()
		{
			var taskType = new WorkflowTaskType();
			taskType.Code = "AAA";
			AssertNoErrors(taskType.CodeInfo);
		}

		public void TestValidateCode_ReservedCodesHaveError()
		{
			CombineAssertions(() =>
			{
				foreach (var code in Core.Constants.Workflow.ReservedTaskTypes)
				{
					var taskType = new WorkflowTaskType();
					taskType.Code = code;
					AssertHasError(taskType.CodeInfo, "The Code must not be in the list of reserved codes 'EXC, MIL, TRG'.");
				}
			});
		}

		public void TestIsCurrentTaskType_ShouldNotAllowMultiples()
		{
			var collection = new WorkflowTaskTypeCollection();
			var taskType1 = collection.AddNew();
			var taskType2 = collection.AddNew();

			taskType1.IsCompletionStatementTaskType = true;
			AssertNoErrors(taskType1.IsCompletionStatementTaskTypeInfo);
			AssertNoErrors(taskType2.IsCompletionStatementTaskTypeInfo);

			taskType2.IsCompletionStatementTaskType = true;
			AssertHasError(taskType2.IsCompletionStatementTaskTypeInfo, "Only one Task Type per Workflow Type can be marked as the 'Completion Statement' task type.");
		}

		public void TestValidateWorkingStatusChangeType()
		{
			var collection = new WorkflowTaskTypeCollection();
			var taskType = collection.AddNew();

			taskType.WorkingStatusChangeType = ZString.Empty;
			AssertHasError(taskType.WorkingStatusChangeTypeInfo, "Please enter a valid selection from the drop down list.");

			taskType.WorkingStatusChangeType = "XXX";
			AssertHasError(taskType.WorkingStatusChangeTypeInfo, "Enter a valid selection from the drop down list.");

			taskType.WorkingStatusChangeType = WorkingStatusChangeTypeList.Codes.Allow;
			AssertNoErrors(taskType.WorkingStatusChangeTypeInfo);
		}

		public void TestValidateContainmentBarrierIterationType_WhenPlanningManagementEnabled_ShouldNotHaveError()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("PLN");

			var collection = new WorkflowTaskTypeCollection();
			var taskType = collection.AddNew();

			AssertEquals(ContainmentBarrierIterationTypeList.Codes.NCB, taskType.ContainmentBarrierIterationType);
			AssertNoErrors(taskType.ContainmentBarrierIterationTypeInfo);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.CWF;
			AssertNoErrors("Containment Barriers are allowed when Planning Management is enabled so there should be no error.", taskType.ContainmentBarrierIterationTypeInfo);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			AssertNoErrors("Containment Barriers are allowed when Planning Management is enabled so there should be no error.", taskType.ContainmentBarrierIterationTypeInfo);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.NWF;
			AssertNoErrors("Containment Barriers are allowed when Planning Management is enabled so there should be no error.", taskType.ContainmentBarrierIterationTypeInfo);
		}

		public void TestValidateContainmentBarrierIterationType_WhenIncludesBufferManagementEnabled_ShouldHaveError()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BUF");

			var collection = new WorkflowTaskTypeCollection();
			var taskType = collection.AddNew();

			AssertEquals(ContainmentBarrierIterationTypeList.Codes.NCB, taskType.ContainmentBarrierIterationType);
			AssertNoErrors(taskType.ContainmentBarrierIterationTypeInfo);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.CWF;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.NWF;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);
		}

		public void TestValidateContainmentBarrierIterationType_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("EWF");

			var collection = new WorkflowTaskTypeCollection();
			var taskType = collection.AddNew();

			AssertEquals(ContainmentBarrierIterationTypeList.Codes.NCB, taskType.ContainmentBarrierIterationType);
			AssertNoErrors(taskType.ContainmentBarrierIterationTypeInfo);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.CWF;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.NWF;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);
		}

		public void TestValidateContainmentBarrierIterationType_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BWF");

			var collection = new WorkflowTaskTypeCollection();
			var taskType = collection.AddNew();

			AssertEquals(ContainmentBarrierIterationTypeList.Codes.NCB, taskType.ContainmentBarrierIterationType);
			AssertNoErrors(taskType.ContainmentBarrierIterationTypeInfo);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.CWF;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);

			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.NWF;
			AssertHasError(taskType.ContainmentBarrierIterationTypeInfo, ContainmentBarriersNotAvailableError);
		}

		string ContainmentBarriersNotAvailableError => "Containment Barriers are only available when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to 'PLN - Planning Management'.";

		#endregion

		public void TestCanCloseTaskNotAssignedToSelf()
		{
			var taskType = new WorkflowTaskType();
			Assert("Should be set to true by default", taskType.CanCloseTaskNotAssignedToSelf);
		}

		public void TestCanCancelTaskNotAssignedToSelf()
		{
			var taskType = new WorkflowTaskType();
			Assert("Should be set to true by default", taskType.CanCancelTask);
		}

		public void TestSetCustomDefaultValues()
		{
			var taskType = new WorkflowTaskType();

			AssertEquals(WorkingStatusChangeTypeList.Codes.Allow, taskType.WorkingStatusChangeType);
			AssertEquals(true, taskType.IsActive);
			AssertEquals(false, taskType.AllowTaskReset);
			AssertEquals(ZGuid.Empty, taskType.DefaultCapability);
		}

		public void TestXmlSerialiseDeserialiseFromExistingRecord()
		{
			var guid = new ZGuid();

			var existingRecord = new WorkflowTaskType();
			existingRecord.Code = "MEH";
			existingRecord.Description = (NoResString)"Laughing is good for you";
			existingRecord.Bool = true;
			existingRecord.IsExcludedFromTransferRules = true;
			existingRecord.IsCompletionStatementTaskType = true;
			existingRecord.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			existingRecord.IsRequireActualDuration = true;
			existingRecord.IsWorkProduction = true;
			existingRecord.WorkingStatusChangeType = WorkingStatusChangeTypeList.Codes.Allow;
			existingRecord.AllowTaskReset = true;
			existingRecord.IsActive = false;
			existingRecord.CanCancelTask = false;
			existingRecord.CanCloseTaskNotAssignedToSelf = false;
			existingRecord.DefaultCapability = guid;
			existingRecord.IsApprovalTask = true;

			var newRecord = SerialiseAndDeserialise(existingRecord);

			AssertEquals("MEH", newRecord.Code);
			AssertEquals("Laughing is good for you", newRecord.Description);
			AssertEquals(true, newRecord.CreatesAppointment);
			AssertEquals(false, newRecord.CanCloseTaskNotAssignedToSelf);
			AssertEquals(false, newRecord.CanCancelTask);
			Assert(newRecord.IsExcludedFromTransferRules);
			Assert(newRecord.IsCompletionStatementTaskType);
			Assert(newRecord.IsRequireActualDuration);
			Assert(newRecord.IsWorkProduction);
			AssertEquals(WorkingStatusChangeTypeList.Codes.Allow, newRecord.WorkingStatusChangeType);
			AssertEquals(ContainmentBarrierIterationTypeList.Codes.GLB, newRecord.ContainmentBarrierIterationType);
			AssertEquals(false, newRecord.IsActive);
			AssertEquals(guid, newRecord.DefaultCapability);
			Assert(newRecord.IsApprovalTask);
		}

		public void TestXmlDeserialiseWhenIsApprovalTaskFlagIsMissing_ShouldDeserialiseToFalse()
		{
			var existingXmlWithoutIsApprovalTaskFlag = "<root><CodeMaxLength>3</CodeMaxLength><Code /><Description /><Bool>N</Bool><SystemDefined>False</SystemDefined><CanCloseTaskNotAssignedToSelf>Y</CanCloseTaskNotAssignedToSelf><IsExcludedFromTransferRules>N</IsExcludedFromTransferRules><IsCompletionStatementTaskType>N</IsCompletionStatementTaskType><ContainmentBarrierIterationType>N</ContainmentBarrierIterationType><IsRequireActualDuration>N</IsRequireActualDuration><IsWorkProduction>N</IsWorkProduction><IsActive>Y</IsActive><WorkingStatusChangeType>ALW</WorkingStatusChangeType><AllowTaskReset>N</AllowTaskReset><CanCancelTask>Y</CanCancelTask><DefaultCapability>2b8ee1c7-94d7-4424-8ed2-7f2144ad3e8f</DefaultCapability></root>";
			var newRecord = new WorkflowTaskType();

			using (var sReader = new StringReader(existingXmlWithoutIsApprovalTaskFlag))
			using (var reader = XmlReader.Create(sReader))
			{
				reader.Read();
				((IXmlSerializable)newRecord).ReadXml(reader);
			}

			Assert(!newRecord.IsApprovalTask);
			AssertEquals(newRecord.DefaultCapability, new Guid("2b8ee1c7-94d7-4424-8ed2-7f2144ad3e8f"));
		}

		public void TestIsActiveFlagSerialisation()
		{
			var taskType = new WorkflowTaskType();

			AssertEquals("Active flag should default to true", true, taskType.IsActive);

			taskType = SerialiseAndDeserialise(taskType);

			AssertEquals("Full serialisation process should retain true value", true, taskType.IsActive);

			taskType.IsActive = false;
			taskType = SerialiseAndDeserialise(taskType);

			AssertEquals("Full serialisation process should retain false value", false, taskType.IsActive);
		}

		static WorkflowTaskType SerialiseAndDeserialise(WorkflowTaskType workflowTaskType)
		{
			var builder = new StringBuilder();
			var settings = new XmlWriterSettings { OmitXmlDeclaration = true };

			using (var writer = XmlWriter.Create(builder, settings))
			{
				writer.WriteStartElement("root");
				((IXmlSerializable)workflowTaskType).WriteXml(writer);
				writer.WriteEndElement();
			}

			var newRecord = new WorkflowTaskType();

			using (var sReader = new StringReader(builder.ToString()))
			using (var reader = XmlReader.Create(sReader))
			{
				reader.Read();
				((IXmlSerializable)newRecord).ReadXml(reader);
			}

			return newRecord;
		}

		public void TestXmlDeserialiseFromOldFormatRecord()
		{
			const string oldFormatXml = @"<root><CodeMaxLength>3</CodeMaxLength><Code>MEH</Code><Description>Laughing is good for you</Description><Bool>Y</Bool><CanCloseTaskNotAssignedToSelf>Y</CanCloseTaskNotAssignedToSelf></root>";

			var newRecord = new WorkflowTaskType();
			using (var stringReader = new StringReader(oldFormatXml))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				xmlReader.Read();
				((IXmlSerializable)newRecord).ReadXml(xmlReader);
			}
			AssertEquals("MEH", newRecord.Code);
			AssertEquals("Laughing is good for you", newRecord.Description);
			AssertEquals(true, newRecord.CreatesAppointment);
			AssertEquals(true, newRecord.CanCloseTaskNotAssignedToSelf);
			AssertEquals(false, newRecord.IsExcludedFromTransferRules);
			AssertEquals(false, newRecord.IsCompletionStatementTaskType);
			AssertEquals(true, newRecord.IsActive);
		}

		public void TestCopyValuesToClone()
		{
			var guid = new ZGuid();
			var collection = new WorkflowTaskTypeCollection();
			var taskType = collection.AddNew();

			taskType.Code = "LOL";
			taskType.Description = (NoResString)"Laughing is good for you";
			taskType.Bool = true;
			taskType.IsExcludedFromTransferRules = false;
			taskType.IsCompletionStatementTaskType = true;
			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.NCB;
			taskType.IsRequireActualDuration = true;
			taskType.IsWorkProduction = true;
			taskType.CreatesAppointment = true;
			taskType.CanCloseTaskNotAssignedToSelf = false;
			taskType.CanCancelTask = false;
			taskType.WorkingStatusChangeType = WorkingStatusChangeTypeList.Codes.Allow;
			taskType.AllowTaskReset = true;
			taskType.IsActive = false;
			taskType.DefaultCapability = guid;
			taskType.IsApprovalTask = true;

			var clone = new WorkflowTaskType();

			clone = clone.CopyValuesToClone(taskType);

			AssertEquals("LOL", clone.Code);
			AssertEquals("Laughing is good for you", clone.Description);
			AssertEquals(true, clone.Bool);
			AssertEquals(false, clone.IsExcludedFromTransferRules);
			AssertEquals(true, clone.IsCompletionStatementTaskType);
			AssertEquals(true, clone.IsRequireActualDuration);
			AssertEquals(true, clone.IsWorkProduction);
			AssertEquals(true, clone.CreatesAppointment);
			AssertEquals(false, clone.CanCloseTaskNotAssignedToSelf);
			AssertEquals(false, clone.CanCancelTask);
			AssertEquals(WorkingStatusChangeTypeList.Codes.Allow, clone.WorkingStatusChangeType);
			AssertEquals(ContainmentBarrierIterationTypeList.Codes.NCB, clone.ContainmentBarrierIterationType);
			AssertEquals(true, clone.AllowTaskReset);
			AssertEquals(false, clone.IsActive);
			AssertEquals(guid, clone.DefaultCapability);
			AssertEquals(true, clone.IsApprovalTask);
		}

		public override void TestCanDelete()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var collection = new TaskTypeRestrictionsCollection();
			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "WKI";
			restriction.TaskType = "UDF";
			restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			restriction.Scope = ScopeList.Codes.Workflow;
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			BizObj.Code = "UDF";
			var canDelete = (ICanDelete)BizObj;

			AssertEquals("ReasonForNotAbleToDelete", "Task Type [UDF] is currently in used in Task Assignment Restrictions registry.", canDelete.ReasonForNotAbleToDelete);
			AssertEquals("CanDelete", false, canDelete.CanDelete);

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaskTypeRestrictionsCollection());
			AssertEquals("CanDelete", true, canDelete.CanDelete);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			var clone = (WorkflowTaskType)clone1;
			Assert("CreatesAppointment", clone.CreatesAppointment);
			Assert("CanCloseTaskNotAssignedToSelf", !clone.CanCloseTaskNotAssignedToSelf);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (WorkflowTaskType)base.GetBusinessObjectToClone();
			result.CreatesAppointment = true;
			result.CanCloseTaskNotAssignedToSelf = false;
			return result;
		}

		new WorkflowTaskType BizObj => (WorkflowTaskType)base.BizObj;
	}
}
