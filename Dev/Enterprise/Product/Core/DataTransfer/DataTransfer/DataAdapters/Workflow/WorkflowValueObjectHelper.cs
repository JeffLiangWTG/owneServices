using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class WorkflowValueObjectHelper
	{
		#region Import

		public void ImportFromValueObject(Xsd.Workflow xmlWorkflow, ProcessTaskCollection workflowItems, IValueObjectImportContext context)
		{
			if (xmlWorkflow.IsSpecified)
			{
				ImportTriggers(xmlWorkflow.Triggers, workflowItems.Triggers, context);
			}
		}

		void ImportTriggers(Xsd.WorkflowTriggerCollection xmlTriggers, WorkflowTriggerCollectionView triggers, IValueObjectImportContext context)
		{
			if (xmlTriggers.IsSpecified)
			{
				foreach (Xsd.WorkflowTrigger xmlTrigger in xmlTriggers)
				{
					if (xmlTrigger.Description.IsEmpty)
					{
						context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, Res.GetString("a200b075-9132-4757-91eb-0dc355764217", "A Workflow Trigger Event must have a 'Description' specified")));
					}
					else if (xmlTrigger.Item.Code.IsEmpty)
					{
						context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, Res.GetString("30eeb4c9-1a99-4867-838d-58bc18967e09", "A Workflow Trigger Event must have a 'Code' specified")));
					}
					else
					{
						foreach (ProcessTask existingTrigger in triggers.Find(new ZQuery(ProcessTasksSchema.P9_Description, xmlTrigger.Description)))
						{
							existingTrigger.Delete();
						}
						ProcessTask trigger = triggers.AddNew();
						context.SetPropertyInfoValue(trigger.P9_DescriptionInfo, xmlTrigger.Description);
						context.SetPropertyInfoValue(trigger.TriggerConditions.TriggerEventCodeInfo, xmlTrigger.Item.Code);
						if (!xmlTrigger.Item.Reference.IsEmpty)
						{
							context.SetPropertyInfoValue(trigger.TriggerConditions.TriggerConditionInfo, EventReferenceConditionList.Codes.EventReference);
							context.SetPropertyInfoValue(trigger.TriggerConditions.TriggerConditionValueInfo, xmlTrigger.Item.Reference);
						}
						foreach (Xsd.WorkflowTriggerAction xmlTriggerAction in xmlTrigger.Item.Actions)
						{
							ImportTriggerAction(trigger, xmlTriggerAction, context);
						}
					}
				}
			}
		}

		void ImportTriggerAction(ProcessTask trigger, Xsd.WorkflowTriggerAction xmlTriggerAction, IValueObjectImportContext context)
		{
			if (xmlTriggerAction.IsSpecified)
			{
				ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
				context.SetPropertyInfoValue(action.PQ_TriggerTypeInfo, WorkflowTriggerActionTypeXmlCodeMappings.Instance.GetEnterpriseCode(xmlTriggerAction.Type, Res.GetString("7eb137f0-7d4b-4c1c-8a2f-ff9d037e9c06", "Workflow Trigger Action Type"), context));
				context.SetPropertyInfoValue(action.PQ_TriggerPartyInfo, WorkflowTriggerActionRecipientXmlCodeMappings.Instance.GetEnterpriseCode(xmlTriggerAction.Recipient, Res.GetString("b0bccf3b-622d-41c0-acd7-7c960e1b76e8", "Workflow Trigger Action Recipient"), context));
				context.SetPropertyInfoValue(action.PQ_EmailAddrInfo, xmlTriggerAction.EmailAddress);
				if (xmlTriggerAction.PurposeSpecified)
				{
					context.SetPropertyInfoValue(action.PQ_MessagePurposeInfo, WorkflowTriggerActionPurposeXmlCodeMappings.Instance.GetEnterpriseCode(xmlTriggerAction.Purpose, Res.GetString("b58c40f7-b159-46f4-8eb0-1609574b4a32", "Workflow Trigger Action Purpose"), context));
				}
				if (xmlTriggerAction.EmailHtmlContentSpecified)
				{
					action.OverrideEmail = true;
					context.SetPropertyInfoValue(action.PQ_EmailTextInfo, xmlTriggerAction.EmailHtmlContent);
				}
			}
		}

		#endregion

		#region Export

		public void ExportToValueObject(ProcessTaskCollection workflowItems, Xsd.Workflow xmlWorkflow, INotifications notifications)
		{
			if (workflowItems != null && xmlWorkflow != null)
			{
				ExportTriggers(workflowItems.Triggers, xmlWorkflow.Triggers, notifications);
				xmlWorkflow.IsSpecified = xmlWorkflow.Triggers.Count > 0;
			}
		}

		void ExportTriggers(WorkflowTriggerCollectionView triggers, Xsd.WorkflowTriggerCollection xmlTriggers, INotifications notifications)
		{
			foreach (ProcessTask trigger in triggers)
			{
				xmlTriggers.IsSpecified = true;
				if (!trigger.P9_SE_NKMilestoneEvent.IsEmpty)
				{
					Xsd.WorkflowTrigger xmlTrigger = xmlTriggers.AddNew();
					xmlTrigger.Description = trigger.P9_Description;
					xmlTrigger.DescriptionSpecified = true;
					Xsd.WorkflowTriggerEvent xmlEvent = xmlTrigger.Item;
					xmlEvent.Code = trigger.P9_SE_NKMilestoneEvent;
					xmlEvent.CodeSpecified = true;
					if (trigger.TriggerConditions.HasEventReferenceTriggerCondition)
					{
						xmlEvent.Reference = trigger.P9_TriggerConditionValue;
					}
					ExportTriggerAction(trigger.ProcessTaskNotifications, xmlEvent.Actions, notifications);
				}
			}
		}

		void ExportTriggerAction(ProcessTaskNotificationCollection triggerActions, Xsd.WorkflowTriggerActionCollection xmlEventActions, INotifications notifications)
		{
			xmlEventActions.IsSpecified = triggerActions.Count > 0;
			foreach (ProcessTaskNotification action in triggerActions)
			{
				Xsd.WorkflowTriggerAction xmlEventAction = xmlEventActions.AddNew();
				xmlEventAction.IsSpecified = true;
				xmlEventAction.Recipient = WorkflowTriggerActionRecipientXmlCodeMappings.Instance.GetExternalCode(action.PQ_TriggerParty, Res.GetString("fc54827b-55cc-4c5a-a611-dcb37f6fcf5f", "Trigger Recipient"), notifications);
				xmlEventAction.Type = WorkflowTriggerActionTypeXmlCodeMappings.Instance.GetExternalCode(action.PQ_TriggerType, Res.GetString("184f7307-c4ec-4ad0-abf1-9b0084687ac5", "Trigger Type"), notifications);
				if (!action.PQ_MessagePurpose.IsEmpty)
				{
					xmlEventAction.Purpose = WorkflowTriggerActionPurposeXmlCodeMappings.Instance.GetExternalCode(action.PQ_MessagePurpose, Res.GetString("89b13fe1-1008-458b-a2e4-62c7c056b7bf", "Purpose Code"), notifications);
					xmlEventAction.PurposeSpecified = true;
				}
				if (action.PQ_TriggerParty == MessageRecipientPartyTypeList.Codes.Email)
				{
					xmlEventAction.EmailAddress = action.PQ_EmailAddr;
					xmlEventAction.EmailAddressSpecified = true;
				}
				if (WorkflowTriggerActionTypeConstants.IsNotificationEmail(action.PQ_TriggerType))
				{
					xmlEventAction.EmailHtmlContent = action.PQ_EmailTextFallbackToTemplate;
					xmlEventAction.EmailHtmlContentSpecified = true;
				}
			}
		}

		#endregion
	}
}
