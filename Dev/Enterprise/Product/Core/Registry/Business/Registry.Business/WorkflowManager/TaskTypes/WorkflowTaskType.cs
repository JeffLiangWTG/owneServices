using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WorkflowTaskType : CodeDescriptionBool, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string CanCancelTask = "CanCancelTask";
			public const string CanCloseTaskNotAssignedToSelf = "CanCloseTaskNotAssignedToSelf";
			public const string IsExcludedFromTransferRules = "IsExcludedFromTransferRules";
			public const string IsCompletionStatementTaskType = "IsCompletionStatementTaskType";
			public const string IsRequireActualDuration = "IsRequireActualDuration";
			public const string IsWorkProduction = "IsWorkProduction";
			public const string IsActive = "IsActive";
			public const string WorkingStatusChangeType = "WorkingStatusChangeType";
			public const string WorkingStatusChangeTypeList = "WorkingStatusChangeTypeList";
			public const string ContainmentBarrierIterationType = "ContainmentBarrierIterationType";
			public const string ContainmentBarrierIterationTypeList = "ContainmentBarrierIterationTypeList";
			public const string AllowTaskReset = "AllowTaskReset";
			public const string DefaultCapability = "DefaultCapability";
			public const string IsApprovalTask = "IsApprovalTask";
		}

		#endregion

		public WorkflowTaskType()
		{
		}

		public WorkflowTaskType(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Set Default Value

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			WorkingStatusChangeType = WorkingStatusChangeTypeList.Codes.Allow;
			containmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.NCB;
			IsActive = ZBool.True;
			AllowTaskReset = false;
			CanCancelTask = true;
			DefaultCapability = Guid.Empty;
		}

		#endregion

		#region CreatesAppointment

		public ZBool CreatesAppointment
		{
			get { return Bool; }
			set { Bool = value; }
		}

		public ZPropertyInfo CreatesAppointmentInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CreatesAppointment), x => BoolInfo); }
		}

		#endregion

		#region CanCloseTaskNotAssignedToSelf

		public ZBool CanCloseTaskNotAssignedToSelf
		{
			get { return canCloseTaskNotAssignedToSelf; }
			set { SetNonPersistentPropertyValue<ZBool>(CanCloseTaskNotAssignedToSelfInfo, ref canCloseTaskNotAssignedToSelf, value); }
		}

		public ZPropertyInfo CanCloseTaskNotAssignedToSelfInfo
		{
			get { return GetZPropertyInfo(nameof(CanCloseTaskNotAssignedToSelf)); }
		}

		ZBool canCloseTaskNotAssignedToSelf = true;

		#endregion

		#region CanCancelTask

		public ZBool CanCancelTask
		{
			get { return canCancelTask; }
			set { SetNonPersistentPropertyValue<ZBool>(CanCancelTaskInfo, ref canCancelTask, value); }
		}

		public ZPropertyInfo CanCancelTaskInfo
		{
			get { return GetZPropertyInfo(nameof(CanCancelTask)); }
		}

		ZBool canCancelTask;

		#endregion

		#region IsActive

		public ZBool IsActive
		{
			get { return isActive; }
			set { SetNonPersistentPropertyValue(IsActiveInfo, ref isActive, value); }
		}
		ZBool isActive;

		public ZPropertyInfo IsActiveInfo => GetZPropertyInfo(nameof(IsActive));

		#endregion

		#region IsExcludedFromTransferRules

		public ZBool IsExcludedFromTransferRules
		{
			get { return isExcludedFromTransferRules; }
			set { SetNonPersistentPropertyValue(IsExcludedFromTransferRulesInfo, ref isExcludedFromTransferRules, value); }
		}
		ZBool isExcludedFromTransferRules;

		public ZPropertyInfo IsExcludedFromTransferRulesInfo
		{
			get { return GetZPropertyInfo(nameof(IsExcludedFromTransferRules)); }
		}

		#endregion

		#region IsCompletionStatementTaskType

		public ZBool IsCompletionStatementTaskType
		{
			get { return isCompletionStatementTaskType; }
			set
			{
				SetNonPersistentPropertyValue(IsCompletionStatementTaskTypeInfo, ref isCompletionStatementTaskType, value);
				if (!IsValidationSuspended)
				{
					ValidateIsCompletionStatementTaskType();
				}
			}
		}
		ZBool isCompletionStatementTaskType;

		public ZPropertyInfo IsCompletionStatementTaskTypeInfo
		{
			get { return GetZPropertyInfo(nameof(IsCompletionStatementTaskType)); }
		}

		#endregion

		#region IsRequireActualDuration

		public ZBool IsRequireActualDuration
		{
			get { return isRequireActualDuration; }
			set { SetNonPersistentPropertyValue(IsRequireActualDurationInfo, ref isRequireActualDuration, value); }
		}
		ZBool isRequireActualDuration;

		public ZPropertyInfo IsRequireActualDurationInfo
		{
			get { return GetZPropertyInfo(nameof(IsRequireActualDuration)); }
		}

		#endregion

		#region ContainmentBarrierIterationType

		[List(Schema.ContainmentBarrierIterationTypeList)]
		[ResourceStringData("WorkflowTaskType.ContainmentBarrierIterationType", Caption = "Containment Barrier Type", ShortCaption = "Containment Barrier", FullDescription = "Specifies whether tasks of this type will trigger Containment Barrier logic when closed and the method used to create quality iteration tasks.")]
		public ZString ContainmentBarrierIterationType
		{
			get { return containmentBarrierIterationType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ContainmentBarrierIterationTypeInfo, ref containmentBarrierIterationType, value);

				if (!IsValidationSuspended)
				{
					ValidateContainmentBarrierIterationType();
				}
			}
		}

		ZString containmentBarrierIterationType;

		public ZPropertyInfo ContainmentBarrierIterationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainmentBarrierIterationType); }
		}

		public ContainmentBarrierIterationTypeList ContainmentBarrierIterationTypeList => new ContainmentBarrierIterationTypeList();

		#endregion

		#region IsWorkProduction

		public ZBool IsWorkProduction
		{
			get { return isWorkProduction; }
			set { SetNonPersistentPropertyValue(IsWorkProductionInfo, ref isWorkProduction, value); }
		}
		ZBool isWorkProduction;

		public ZPropertyInfo IsWorkProductionInfo
		{
			get { return GetZPropertyInfo(nameof(IsWorkProduction)); }
		}

		#endregion

		#region Allow Working Status Changes in Buckets

		[List(Schema.WorkingStatusChangeTypeList)]
		[ResourceStringData("e6ae1b0e-5835-444e-bf5a-0bf58a2583c3", Caption = "Allow Working Status Changes in Buckets")]
		public ZString WorkingStatusChangeType
		{
			get { return workingStatusChangeType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(WorkingStatusChangeTypeInfo, ref workingStatusChangeType, value);
				if (!IsValidationSuspended)
				{
					ValidateWorkingStatusChangeType();
				}
			}
		}

		public ZPropertyInfo WorkingStatusChangeTypeInfo
		{
			get { return GetZPropertyInfo(Schema.WorkingStatusChangeType); }
		}

		ZString workingStatusChangeType;

		public WorkingStatusChangeTypeList WorkingStatusChangeTypeList => new WorkingStatusChangeTypeList();

		#endregion

		#region Reset Task Penetration on New Task Addition Assignment or Re-assignment

		public ZBool AllowTaskReset
		{
			get { return allowTaskReset; }
			set { SetNonPersistentPropertyValue(AllowTaskResetInfo, ref allowTaskReset, value); }
		}
		ZBool allowTaskReset;

		public ZPropertyInfo AllowTaskResetInfo
		{
			get { return GetZPropertyInfo(nameof(AllowTaskReset)); }
		}

		#endregion

		#region Default Capability

		public ZGuid DefaultCapability
		{
			get { return defaultCapability; }
			set
			{
				SetNonPersistentPropertyValue(DefaultCapabilityInfo, ref defaultCapability, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultCapability();
				}
			}
		}
		ZGuid defaultCapability;

		public ZPropertyInfo DefaultCapabilityInfo
		{
			get { return GetZPropertyInfo(nameof(DefaultCapability)); }
		}

		public IGlbCapabilityCollection GlbCapabilityCollection
		{
			get
			{
				return glbCapabilityCollection ?? (glbCapabilityCollection = (IGlbCapabilityCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IGlbCapabilityCollection>(), new object[] { CurrentFactory }));
			}
		}
		IGlbCapabilityCollection glbCapabilityCollection;

		#endregion

		#region IsApprovalTasks

		public ZBool IsApprovalTask
		{
			get { return isApprovalTask; }
			set { SetNonPersistentPropertyValue(IsApprovalTaskInfo, ref isApprovalTask, value); }
		}
		ZBool isApprovalTask;

		public ZPropertyInfo IsApprovalTaskInfo
		{
			get { return GetZPropertyInfo(nameof(IsApprovalTask)); }
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowTaskType();
		}

		public WorkflowTaskType CopyValuesToClone(WorkflowTaskType taskType)
		{
			var clone = new WorkflowTaskType
			{
				Code = taskType.Code,
				Description = taskType.Description,
				Bool = taskType.Bool,
				IsExcludedFromTransferRules = taskType.IsExcludedFromTransferRules,
				IsCompletionStatementTaskType = taskType.isCompletionStatementTaskType,
				containmentBarrierIterationType = taskType.containmentBarrierIterationType,
				IsRequireActualDuration = taskType.isRequireActualDuration,
				IsWorkProduction = taskType.isWorkProduction,
				IsActive = taskType.IsActive,
				CanCancelTask = taskType.canCancelTask,
				CanCloseTaskNotAssignedToSelf = taskType.canCloseTaskNotAssignedToSelf,
				CreatesAppointment = taskType.CreatesAppointment,
				WorkingStatusChangeType = taskType.WorkingStatusChangeType,
				AllowTaskReset = taskType.AllowTaskReset,
				DefaultCapability = taskType.DefaultCapability,
				IsApprovalTask = taskType.isApprovalTask,
			};

			return clone;
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			if (reader.Name == Schema.CanCloseTaskNotAssignedToSelf)
			{
				CanCloseTaskNotAssignedToSelf = new ZBool(reader.ReadElementString(Schema.CanCloseTaskNotAssignedToSelf));

				while (reader.Name != Schema.IsExcludedFromTransferRules && reader.IsStartElement())
				{
					reader.ReadElementString(reader.Name);
				}

				if (reader.IsStartElement(Schema.IsExcludedFromTransferRules))
				{
					IsExcludedFromTransferRules = new ZBool(reader.ReadElementString(Schema.IsExcludedFromTransferRules));
				}
				if (reader.IsStartElement(Schema.IsCompletionStatementTaskType))
				{
					IsCompletionStatementTaskType = new ZBool(reader.ReadElementString(Schema.IsCompletionStatementTaskType));
				}
				if (reader.IsStartElement(Schema.ContainmentBarrierIterationType))
				{
					ContainmentBarrierIterationType = new ZString(reader.ReadElementString(Schema.ContainmentBarrierIterationType));
				}
				if (reader.IsStartElement(Schema.IsRequireActualDuration))
				{
					IsRequireActualDuration = new ZBool(reader.ReadElementString(Schema.IsRequireActualDuration));
				}
				if (reader.IsStartElement(Schema.IsWorkProduction))
				{
					IsWorkProduction = new ZBool(reader.ReadElementString(Schema.IsWorkProduction));
				}
				if (reader.IsStartElement(Schema.IsActive))
				{
					IsActive = new ZBool(reader.ReadElementString(Schema.IsActive));
				}
				if (reader.IsStartElement(Schema.WorkingStatusChangeType))
				{
					WorkingStatusChangeType = new ZString(reader.ReadElementString(Schema.WorkingStatusChangeType));
				}
				if (reader.IsStartElement(Schema.AllowTaskReset))
				{
					AllowTaskReset = new ZBool(reader.ReadElementString(Schema.AllowTaskReset));
				}
				if (reader.IsStartElement(Schema.CanCancelTask))
				{
					CanCancelTask = new ZBool(reader.ReadElementString(Schema.CanCancelTask));
				}
				if (reader.IsStartElement(Schema.DefaultCapability))
				{
					DefaultCapability = new Guid(reader.ReadElementString(Schema.DefaultCapability));
				}
				if (reader.IsStartElement(Schema.IsApprovalTask))
				{
					IsApprovalTask = new ZBool(reader.ReadElementString(Schema.IsApprovalTask));
				}
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(Schema.CanCloseTaskNotAssignedToSelf, CanCloseTaskNotAssignedToSelf.ToString());
			writer.WriteElementString(Schema.IsExcludedFromTransferRules, IsExcludedFromTransferRules.ToString());
			writer.WriteElementString(Schema.IsCompletionStatementTaskType, IsCompletionStatementTaskType.ToString());
			writer.WriteElementString(Schema.ContainmentBarrierIterationType, containmentBarrierIterationType.ToString());
			writer.WriteElementString(Schema.IsRequireActualDuration, IsRequireActualDuration.ToString());
			writer.WriteElementString(Schema.IsWorkProduction, IsWorkProduction.ToString());
			writer.WriteElementString(Schema.IsActive, IsActive.ToString());
			writer.WriteElementString(Schema.WorkingStatusChangeType, WorkingStatusChangeType.ToString());
			writer.WriteElementString(Schema.AllowTaskReset, AllowTaskReset.ToString());
			writer.WriteElementString(Schema.CanCancelTask, CanCancelTask.ToString());
			writer.WriteElementString(Schema.DefaultCapability, DefaultCapability.ToString());
			writer.WriteElementString(Schema.IsApprovalTask, IsApprovalTask.ToString());
		}

		#endregion

		#region Validation

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();

			if (Core.Constants.Workflow.IsReservedTaskType(Code))
			{
				var reservedCodes = string.Join(", ", Core.Constants.Workflow.ReservedTaskTypes);
				CodeInfo.AddError(Res.GetString("94eff8f9-ec36-41b2-ac9b-f3875d252d33", "The Code must not be in the list of reserved codes '{0}'.", reservedCodes));
			}
		}

		void ValidateIsCompletionStatementTaskType()
		{
			IsCompletionStatementTaskTypeInfo.ClearAllNotifications();

			if (ParentCollections.Any())
			{
				var collection = ParentCollections.First();
				if (collection != null && IsCompletionStatementTaskType)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(IsCompletionStatementTaskTypeInfo,
						collection,
						Res.GetString("6ffbcae7-2a79-4eba-a14b-a82ea664ad25",
							"Only one Task Type per Workflow Type can be marked as the 'Completion Statement' task type."));
				}
			}
		}

		void ValidateWorkingStatusChangeType()
		{
			WorkingStatusChangeTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(WorkingStatusChangeTypeInfo, Res.GetString("5b018ecb-0a32-47ab-a4c5-4ed135e49da4", "valid selection from the drop down list"));
			ListValidation.ErrorIfInvalidCode(WorkingStatusChangeTypeInfo, WorkingStatusChangeTypeList, ResString.GetMultilingualString("E47AC94F-A10A-4820-A7DD-E9C49BE3BBEE", "selection from the drop down list"));
		}

		void ValidateContainmentBarrierIterationType()
		{
			ContainmentBarrierIterationTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ContainmentBarrierIterationTypeInfo);
			ListValidation.ErrorIfInvalidCode(ContainmentBarrierIterationTypeInfo, ContainmentBarrierIterationTypeList);

			if (!ContainmentBarrierIterationType.EqualsIgnoringCase(ContainmentBarrierIterationTypeList.Codes.NCB) &&
				!ObjectFactory.Get<IBMSRegistry>().IsPlanningManagementEnabled)
			{
				ContainmentBarrierIterationTypeInfo.AddError(Res.GetString("d4d0b8ee-1d96-4481-b725-2563f50eb1b9",
					"Containment Barriers are only available when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to 'PLN - Planning Management'."));
			}
		}

		void ValidateDefaultCapability()
		{
			DefaultCapabilityInfo.ClearAllNotifications();
			if (!DefaultCapability.IsEmpty)
			{
				TypeValidation.CheckValidGuid(DefaultCapabilityInfo);

				if (!DefaultCapabilityInfo.HasErrors())
				{
					var capability = GlbCapabilityCollection.FindByPK(DefaultCapability) as IGlbCapability;
					if (capability != null && !capability.G4_IsActive)
					{
						DefaultCapabilityInfo.AddError(Res.GetString("B68D2D85-FA02-4E0F-BA51-A4937955FAC1", "The assigned capability is inactive."));
					}
				}
			}
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete => !WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value.IsTaskTypeUsedInTaskAssignmentRestrictions(Code);

		MultilingualString ICanDelete.ReasonForNotAbleToDelete => ResString.GetMultilingualString("113B6F3C-2623-407C-83D6-202F43475C12", "Task Type [{0}] is currently in used in Task Assignment Restrictions registry.", Code);

		#endregion
	}
}
