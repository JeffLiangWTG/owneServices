using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TaskTypeRestrictions : RegistryBusinessObject
	{
		#region Schema

		public new static class Schema
		{
			public const string Active = "Active";
			public const string WorkflowType = "WorkflowType";
			public const string WorkflowTypeList = "WorkflowTypeList";
			public const string TaskType = "TaskType";
			public const string TaskTypeList = "TaskTypeList";
			public const string RestrictionType = "RestrictionType";
			public const string RestrictionTypeList = "RestrictionTypeList";
			public const string Scope = "Scope";
			public const string ScopeList = "ScopeList";
			public const string NotificationType = "NotificationType";
			public const string NotificationTypeList = "NotificationTypeList";
		}

		#endregion

		#region Lookups

		public RestrictionTypeList RestrictionTypeList
		{
			get { return new RestrictionTypeList(); }
		}

		public ScopeList ScopeList
		{
			get
			{
				var list = new ScopeList();

				if (!IsBufferManagementEnabled)
				{
					foreach (ICodeDescription item in new ScopeList())
					{
						if (item.Code != ScopeList.Codes.Job)
						{
							list.RemoveCode(item.Code);
						}
					}
				}

				return list;
			}
		}

		static bool IsBufferManagementEnabled => ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled;

		public NotificationTypeList NotificationTypeList
		{
			get { return new NotificationTypeList(); }
		}

		public CodeDescriptionPairList WorkflowTypeList
		{
			get { return (CodeDescriptionPairList)ObjectFactory.New<IWorkflowDescriptorList>(); }
		}

		public CodeDescriptionPairList TaskTypeList
		{
			get { return WorkflowDataRegistryHelper.GetTaskTypeList(WorkflowType); }
		}

		#endregion

		#region RegistryBusinessObject Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			Scope = IsBufferManagementEnabled ? ScopeList.Codes.Workflow : ScopeList.Codes.Job;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaskTypeRestrictions();
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel,
			BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var restrictions = (TaskTypeRestrictions)clone;

			restrictions.TaskTypesCollection.RemoveAll();
			restrictions.TaskTypesCollection.AddRange((BusinessObjectCollection)TaskTypesCollection.Clone(currentFallbackLevel, factory));
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("8a61b1e1-2c65-47f6-bdff-002aedb8bb1d", Caption = "Type Description")]
		public override ZString EnglishDescription
		{
			get { return base.EnglishDescription; }
			set { base.EnglishDescription = value; }
		}

		protected override string CodeDisplayName
		{
			get
			{
				return (NoResString)"[Workflow Type : Task Type : Scope] combination";
			}
		}

		#endregion

		#region TaskTypesCollection

		[ChildEditable]
		public RestrictedTaskTypesCollection TaskTypesCollection
		{
			get
			{
				if (taskTypesCollection == null)
				{
					taskTypesCollection = new RestrictedTaskTypesCollection(this);
					RegisterEditableChildObject(taskTypesCollection);
				}
				return taskTypesCollection;
			}
		}

		RestrictedTaskTypesCollection taskTypesCollection;

		#endregion

		#region Active

		[ResourceStringData("98e77511-5957-4005-95af-1ea8154570d0", Caption = "Active")]
		public ZBool Active
		{
			get { return active; }
			set { SetNonPersistentPropertyValue<ZBool>(ActiveInfo, ref active, value); }
		}

		public ZPropertyInfo ActiveInfo
		{
			get { return GetZPropertyInfo(Schema.Active); }
		}

		ZBool active = true;

		#endregion

		#region WorkflowType

		[List(Schema.WorkflowTypeList)]
		[ResourceStringData("b301fa94-7dc8-4a42-846a-55dedc5bff87", Caption = "Workflow Type")]
		public ZString WorkflowType
		{
			get { return workflowType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(WorkflowTypeInfo, ref workflowType, value);
				SetCode();

				if (!IsValidationSuspended)
				{
					ValidateWorkflowType();
				}
			}
		}

		public int WorkflowType_MaxLength
		{
			get { return CodeMaxLengthDefaultValue; }
		}

		public ZPropertyInfo WorkflowTypeInfo
		{
			get { return GetZPropertyInfo(Schema.WorkflowType); }
		}

		ZString workflowType;

		#endregion

		#region Task Type

		[List(Schema.TaskTypeList)]
		[ResourceStringData("ffa21e2d-f9e4-416f-898b-8f39010450b4", Caption = "Task Type")]
		public ZString TaskType
		{
			get { return taskType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(TaskTypeInfo, ref taskType, value);
				SetCode();

				if (!IsValidationSuspended)
				{
					ValidateTaskType();
				}
			}
		}

		public int TaskType_MaxLength
		{
			get { return CodeMaxLengthDefaultValue; }
		}

		public ZPropertyInfo TaskTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaskType); }
		}

		ZString taskType;

		public override MultilingualString Description
		{
			get { return (NoResString)TaskTypeList.GetDescriptionFromCode(TaskType); }
		}

		protected override int MaxDescriptionLength => 256;

		#endregion

		#region RestrictionType

		[List(Schema.RestrictionTypeList)]
		[ResourceStringData("5684f7b4-6e81-4799-bbb0-e682e27149ec", Caption = "Restriction Type")]
		public ZString RestrictionType
		{
			get { return restrictionType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(RestrictionTypeInfo, ref restrictionType, value);

				if (!IsValidationSuspended)
				{
					ValidateRestrictionType();
				}
			}
		}

		public ZPropertyInfo RestrictionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.RestrictionType); }
		}

		ZString restrictionType;

		#endregion

		#region Scope

		[List(Schema.ScopeList)]
		[ResourceStringData("075b7439-e187-4b3b-8234-3a4653e59ebd", Caption = "Scope")]
		public ZString Scope
		{
			get { return scope; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ScopeInfo, ref scope, value);
				SetCode();

				if (!IsValidationSuspended)
				{
					ValidateScope();
				}
			}
		}

		public int Scope_MaxLength
		{
			get { return CodeMaxLengthDefaultValue; }
		}

		public ZPropertyInfo ScopeInfo
		{
			get { return GetZPropertyInfo(Schema.Scope); }
		}

		ZString scope;

		#endregion

		#region NotificationType

		[List(Schema.NotificationTypeList)]
		[ResourceStringData("c94fdbfe-3539-4b0f-abc8-1a9ab7a9153f", Caption = "Notification Type")]
		public ZString NotificationType
		{
			get { return notificationType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(NotificationTypeInfo, ref notificationType, value);

				if (!IsValidationSuspended)
				{
					ValidateNotificationType();
				}
			}
		}

		public ZPropertyInfo NotificationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.NotificationType); }
		}

		ZString notificationType;

		#endregion

		#region Error Messages

		string InvalidRestrictionTypeAndNotificationTypeErrorMessage
		{
			get { return Res.GetString("5E407857-D999-4D01-9996-5BD1360AE30A", "The combination of Restriction Type [{0}] and Notification Type [{1}] would have no effect on resource assignments.", RestrictionTypeList.Codes.DifferentResource, NotificationTypeList.Codes.None); }
		}

		#endregion

		#region SetTaskTypes

		public void SetTaskTypes(RestrictedTaskTypesCollection taskTypesCollection)
		{
			UnRegisterEditableChildObject(TaskTypesCollection);
			this.taskTypesCollection = taskTypesCollection;
			RegisterEditableChildObject(TaskTypesCollection);
		}

		#endregion

		#region SetCode

		void SetCode()
		{
			CodeInfo.HumanReadableName = CodeDisplayName;

			if (WorkflowType != ZString.Empty && TaskType != ZString.Empty && Scope != ZString.Empty)
			{
				CodeMaxLength = 11;
				Code = ZString.Join(":", new[] { WorkflowType, TaskType, Scope });
			}
		}

		#endregion

		#region CheckForConflictingRestrictions

		void CheckForConflictingRestrictions()
		{
			ActiveInfo.ClearAllNotifications();

			if (ParentCollections.Any())
			{
				var collection = ParentCollections.First();
				var cyclicalCodeList = GetCyclicalCodeList();

				foreach (TaskTypeRestrictions parent in collection)
				{
					if (parent.Active)
					{
						CompareAgainstOtherRestrictions(cyclicalCodeList, parent, collection.Cast<TaskTypeRestrictions>().Where(restriction => restriction.Active).Except(new[] { parent }).ToList());
					}
				}
			}
		}

		List<ZString> GetCyclicalCodeList()
		{
			var list = new List<ZString>();

			foreach (TaskTypeRestrictions parent in ParentCollections.First())
			{
				if (parent.Active
					&& parent.RestrictionType == RestrictionTypeList.Codes.SameResource
					&& ((list.Count == 0)
					|| (list.Contains(parent.TaskType))
					|| (parent.TaskTypesCollection.Any(t => list.Contains(((RestrictedTaskTypes)t).Code)))))
				{
					list.Add(parent.TaskType);
					list.AddRange(parent.TaskTypesCollection.Select(t => ((RestrictedTaskTypes)t).Code));
				}
			}

			return list.Distinct().ToList();
		}

		void CompareAgainstOtherRestrictions(List<ZString> currentTaskTypeList, TaskTypeRestrictions currentRestriction, List<TaskTypeRestrictions> otherRestrictions)
		{
			if (currentRestriction.RestrictionType == RestrictionTypeList.Codes.SameResource)
			{
				foreach (var restriction in otherRestrictions)
				{
					if (currentRestriction.WorkflowType == restriction.workflowType
						&& restriction.RestrictionType == RestrictionTypeList.Codes.DifferentResource
						&& currentTaskTypeList.Contains(restriction.TaskType)
						&& restriction.TaskTypesCollection.Any(t => currentTaskTypeList.Contains(((RestrictedTaskTypes)t).Code)))
					{
						currentRestriction.ActiveInfo.AddError(Res.GetString("D539EAE0-5A49-4E09-90E3-A61A36793825", "Conflict detected with Workflow Type: [{0}], Task Type: [{1}], Restriction Type: [{2}].", restriction.WorkflowType, restriction.TaskType, restriction.RestrictionType));
					}
				}
			}
		}

		#endregion

		#region Xml Serialization

		protected override void ReadMoreElements(System.Xml.XmlReader reader)
		{
			base.ReadMoreElements(reader);

			if (reader.IsStartElement(Schema.Active))
			{
				Active = new ZBool(reader.ReadElementString(Schema.Active));
			}

			if (reader.IsStartElement(Schema.WorkflowType))
			{
				WorkflowType = reader.ReadElementString(Schema.WorkflowType);
			}

			if (reader.IsStartElement(Schema.TaskType))
			{
				TaskType = reader.ReadElementString(Schema.TaskType);
			}

			if (reader.IsStartElement(Schema.RestrictionType))
			{
				RestrictionType = reader.ReadElementString(Schema.RestrictionType);
			}

			if (reader.IsStartElement(Schema.Scope))
			{
				Scope = reader.ReadElementString(Schema.Scope);
			}

			if (reader.IsStartElement(Schema.NotificationType))
			{
				NotificationType = reader.ReadElementString(Schema.NotificationType);
			}

			SetTaskTypes((RestrictedTaskTypesCollection)CollectionSerialiser.Deserialize(reader));
		}

		protected override void WriteMoreElements(System.Xml.XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(Schema.Active, Active.ToString());
			writer.WriteElementString(Schema.WorkflowType, WorkflowType);
			writer.WriteElementString(Schema.TaskType, TaskType);
			writer.WriteElementString(Schema.RestrictionType, RestrictionType);
			writer.WriteElementString(Schema.Scope, Scope);
			writer.WriteElementString(Schema.NotificationType, NotificationType);

			CollectionSerialiser.Serialize(writer, TaskTypesCollection);
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(RestrictedTaskTypesCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion

		#region Validation

		void ValidateWorkflowType()
		{
			WorkflowTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(WorkflowTypeInfo);

			if (WorkflowTypeList != null && !WorkflowTypeList.ContainsCode(WorkflowType))
			{
				WorkflowTypeInfo.AddError(EnterValidSelectionErrorMessage);
			}
		}

		void ValidateTaskType()
		{
			TaskTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaskTypeInfo);

			if (!TaskTypeList.ContainsCode(TaskType))
			{
				TaskTypeInfo.AddError(EnterValidSelectionErrorMessage);
			}
		}

		void ValidateRestrictionType()
		{
			RestrictionTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RestrictionTypeInfo);

			if (!RestrictionTypeList.ContainsCode(RestrictionType))
			{
				RestrictionTypeInfo.AddError(EnterValidSelectionErrorMessage);
			}

			ValidateRestrictionTypeAndNotificationTypeCombination(true);
		}

		void ValidateScope()
		{
			ScopeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ScopeInfo);

			if (!ScopeList.ContainsCode(Scope))
			{
				ScopeInfo.AddError(EnterValidSelectionErrorMessage);
			}
		}

		void ValidateNotificationType()
		{
			NotificationTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NotificationTypeInfo);

			if (!NotificationTypeList.ContainsCode(NotificationType))
			{
				NotificationTypeInfo.AddError(EnterValidSelectionErrorMessage);
			}

			ValidateRestrictionTypeAndNotificationTypeCombination(false);
		}

		void ValidateRestrictionTypeAndNotificationTypeCombination(bool addErrortoRestrictionTypeInfo)
		{
			if (RestrictionType == RestrictionTypeList.Codes.DifferentResource && NotificationType == NotificationTypeList.Codes.None)
			{
				if (addErrortoRestrictionTypeInfo)
				{
					RestrictionTypeInfo.AddError(InvalidRestrictionTypeAndNotificationTypeErrorMessage);
				}
				else
				{
					NotificationTypeInfo.AddError(InvalidRestrictionTypeAndNotificationTypeErrorMessage);
				}
			}
			else
			{
				if (RestrictionTypeInfo.HasError(InvalidRestrictionTypeAndNotificationTypeErrorMessage))
				{
					RestrictionTypeInfo.ClearAllNotifications();
				}
				if (NotificationTypeInfo.HasError(InvalidRestrictionTypeAndNotificationTypeErrorMessage))
				{
					NotificationTypeInfo.ClearAllNotifications();
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateWorkflowType();
			ValidateTaskType();
			ValidateRestrictionType();
			ValidateScope();
			ValidateNotificationType();
			CheckForConflictingRestrictions();
		}

		#endregion
	}
}
