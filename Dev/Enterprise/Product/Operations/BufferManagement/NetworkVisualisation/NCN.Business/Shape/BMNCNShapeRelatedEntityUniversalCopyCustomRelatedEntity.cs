using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWise.UniversalCopy.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Shape
{
	public class BMNCNShapeRelatedEntityUniversalCopyCustomRelatedEntity : IUniversalCopyCustomRelatedEntity
	{
		public string SourceTypeName { get => "IBMNCNShape"; }

		public string RelatedPropertyName { get => BMNCNShapeSchema.Constants.BNS_RelatedEntityID; }

		public void InitializeRelatedEntityCopyTemplateTreeNode(EntityCopyTemplateNode entityNode,
			IDictionary<string, object> processedProperties, Func<Type, CopyTemplateNode> initializeSubEntityNode)
		{
			var workItemPropertyType = GlowInterfaceReferenceAttribute.GetGlowInterface("IWorkItem");
			var workProjectPropertyType = GlowInterfaceReferenceAttribute.GetGlowInterface("IWorkProject");

			var workItemElementNode = initializeSubEntityNode(workItemPropertyType);
			var workProjectElementNode = initializeSubEntityNode(workProjectPropertyType);

			var workItemRelatedEntityNode = new RelatedEntityCopyTemplateNode(WorkItemSchema.Constants.TableName,
				BMNCNShapeSchema.Constants.BNS_RelatedEntityID, WorkItemSchema.Constants.TableName, workItemElementNode);
			var workProjectRelatedEntityNode = new RelatedEntityCopyTemplateNode(WorkProjectSchema.Constants.TableName,
				BMNCNShapeSchema.Constants.BNS_RelatedEntityID, WorkProjectSchema.Constants.TableName, workProjectElementNode);

			entityNode.Nodes.Add(workItemRelatedEntityNode);
			entityNode.Nodes.Add(workProjectRelatedEntityNode);

			processedProperties.Add(BMNCNShapeSchema.Constants.BNS_RelatedEntityID, null);
			processedProperties.Add(BMNCNShapeSchema.Constants.BNS_BNS_ParentShape, null);
			processedProperties.Add(BMNCNShapeSchema.Constants.BNS_BNS_RootShape, null);
		}

		public ZGuid? GetRelatedEntityObjectFK(BusinessObjectFactory factory,
			RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode, ZGuid relatedEntityFK)
		{
			var processHeader = factory.Load<ProcessHeader>(relatedEntityFK);

			if (processHeader is not null && processHeader.FH_Category == BMConstants.JobLevelWorkflowCategoryCode)
			{
				var jobType = processHeader.FH_ParentTableCode;
				if (!(jobType == WorkItemSchema.Constants.Prefix
						&& relatedEntityCopyTemplateNode.Name == WorkItemSchema.Constants.TableName)
					&& !(jobType == WorkProjectSchema.Constants.Prefix
						&& relatedEntityCopyTemplateNode.Name == WorkProjectSchema.Constants.TableName))
				{
					return null;
				}

				var jobFK = processHeader.FH_ParentId;
				return jobFK.IsValid ? jobFK : null;
			}
			else
			{
				return null;
			}
		}

		public object GetOriginalRelatedEntityFromRelatedEntityObject(RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode,
			object relatedEntityObject, Func<object, string, object> getPropertyValue)
		{
			if (relatedEntityCopyTemplateNode.Name == WorkItemSchema.Constants.TableName || relatedEntityCopyTemplateNode.Name == WorkProjectSchema.Constants.TableName)
			{
				return getPropertyValue(relatedEntityObject, "JobWorkflow");
			}

			return relatedEntityObject;
		}
	}
}
