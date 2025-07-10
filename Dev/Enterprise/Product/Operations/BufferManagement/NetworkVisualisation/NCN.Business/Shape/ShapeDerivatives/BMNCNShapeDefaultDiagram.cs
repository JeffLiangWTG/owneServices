using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNShapeDefaultDiagram : BMNCNShape
	{
		public BMNCNShapeDefaultDiagram(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("JobHeader")]
		public override ZGuid BNS_RelatedEntityID
		{
			get { return base.BNS_RelatedEntityID; }
			set
			{
				base.BNS_RelatedEntityID = value;
				SetupDefaultShapeNetworkForJobHeader();
			}
		}

		#endregion

		#region Related Business Objects

		public ProcessJobHeader JobHeader
		{
			get { return (ProcessJobHeader)ProcessHeader; }
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BNS_ShapeType = ShapeTypeList.Codes.DefaultDiagram;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMNCNShape)base.CloneInternal(new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, new[]
			{
				BMNCNShapeSchema.BNS_ShapeType.Name
			}.Concat(args.GetExcludedColumns()).ToArray(),
				typeof(BMNCNShape), args.PerformRowCopyWithoutTriggeringValidationAndSetter, args.CopyDecider));
			clone.BNS_ShapeType = ShapeTypeList.Codes.Diagram;
			return clone;
		}

		#endregion

		#region Cache

		internal void SetupDefaultShapeNetworkForJobHeader()
		{
			if (JobHeader != null)
			{
				PopulateChildShapes();
				HandleUpdates();
			}
		}

		void PopulateChildShapes()
		{
			var preCachePopulation = this.GetShapesWithinSameDiagram();
			foreach (var shape in preCachePopulation.Where(s => s.BNS_RelatedEntityID.IsValid && s.BNS_RelatedEntityID != BNS_RelatedEntityID && !defaultShapeCache.ContainsKey(s.BNS_RelatedEntityID)))
			{
				defaultShapeCache.Add(shape.BNS_RelatedEntityID, shape);
			}

			foreach (var workflow in JobHeader.ProcessHeaders.Where(workflow => !defaultShapeCache.ContainsKey(workflow.PK)))
			{
				CreateOrLoadDefaultChildShape(workflow);
			}

			UpdateDefaultNetwork();
		}

		void RemoveDefaultShapeFromCache(ProcessHeader workflow)
		{
			if (defaultShapeCache.ContainsKey(workflow.PK))
			{
				defaultShapeCache.Remove(workflow.PK);
			}
		}

#if DEBUG
		public
#endif
			readonly Dictionary<ZGuid, BMNCNShape> defaultShapeCache = new Dictionary<ZGuid, BMNCNShape>();

		#endregion

		#region Child Shapes

		internal BMNCNShape GetDefaultChildShape(ProcessHeader workflow)
		{
			if (!defaultShapeCache.ContainsKey(workflow.PK))
			{
				var shape = CreateOrLoadDefaultChildShape(workflow);
				defaultShapeCache.Add(workflow.PK, shape);
			}

			return defaultShapeCache[workflow.PK];
		}

		#region Create / Load

		BMNCNShape CreateOrLoadDefaultChildShape(ProcessHeader workflow)
		{
			var shape = workflow.GetDefaultShape(this);
			HandleWorkflowUpdates(this, shape, workflow);
			return shape;
		}

		#endregion

		#region Handle Updates

		void HandleUpdates()
		{
			HandleWorkflowUpdates(this, this, JobHeader);
			JobHeader.RegisterEditableChildObject(this);
		}

		static void HandleWorkflowUpdates(BMNCNShapeDefaultDiagram diagram, BMNCNShape childShape, ProcessHeader workflow)
		{
			workflow.FH_CompletionStatementInfo.ValueChanged += CreateCompletionStatementChangedEvent(childShape, workflow);

			if (diagram != childShape)
			{
				workflow.Deleting += CreateOnDeleteEvent(diagram, workflow);
			}
		}

		static EventHandler CreateCompletionStatementChangedEvent(BMNCNShape shape, ProcessHeader workflow)
		{
			return (s, e) =>
			{
				using (shape.SuspendSettingHasChanges())
				{
					shape.BNS_Name = workflow.FH_CompletionStatement.SubstringSafe(0, BMNCNShapeSchema.BNS_Name.MaxLength);
				}
			};
		}

		static EventHandler CreateOnDeleteEvent(BMNCNShapeDefaultDiagram diagram, ProcessHeader workflow)
		{
			return (s, e) => diagram.RemoveDefaultShapeFromCache(workflow);
		}

		#endregion

		#endregion

		#region Manage Attachments

		public void UpdateDefaultNetwork()
		{
			foreach (ProcessHeader workflow in JobHeader.ProcessHeaders)
			{
				CreateDependencyAttachments(JobHeader, workflow, this, GetDefaultChildShape(workflow), defaultShapeCache);
			}
		}

		public static BMNCNAttachment[] CreateDependencyAttachments(ProcessJobHeader jobHeader, ProcessHeader workflow, BMNCNShapeDefaultDiagram diagramShape, BMNCNShape workflowShape, Dictionary<ZGuid, BMNCNShape> childShapes = null)
		{
			return CreateDependencyAttachments(DependencyDirection.PreRequisite, jobHeader, workflow, diagramShape, workflowShape, childShapes)
						.Concat(CreateDependencyAttachments(DependencyDirection.PostRequisite, jobHeader, workflow, diagramShape, workflowShape, childShapes)).ToArray();
		}

		static IEnumerable<BMNCNAttachment> CreateDependencyAttachments(DependencyDirection dependencyDirection, ProcessJobHeader jobHeader, ProcessHeader workflow, BMNCNShapeDefaultDiagram diagramShape, BMNCNShape workflowShape, Dictionary<ZGuid, BMNCNShape> childShapes = null)
		{
			var links = dependencyDirection == DependencyDirection.PreRequisite ? workflow.PrerequisiteLinks : workflow.PostrequisiteLinks;
			var linksWithinJob = links.Where(l => jobHeader.ProcessHeaders.Any(w => w.PK == l.FP_FH_HeaderTo));

			foreach (var linkWithinJob in linksWithinJob)
			{
				var linkTargetHeader = dependencyDirection == DependencyDirection.PreRequisite ? linkWithinJob.HeaderFrom : linkWithinJob.HeaderTo;
				if (linkTargetHeader != null)
				{
					IBMNCNShape linkTargetHeaderShape = null;

					if (childShapes != null)
					{
						linkTargetHeaderShape = childShapes.ContainsKey(linkTargetHeader.PK) ? childShapes[linkTargetHeader.PK] : null;
					}
					else
					{
						linkTargetHeaderShape = diagramShape.GetDefaultChildShape(linkTargetHeader);
					}

					if (linkTargetHeaderShape != null)
					{
						var fromShapePK = dependencyDirection == DependencyDirection.PreRequisite ? linkTargetHeaderShape.Identifier : workflowShape.PK;
						var toShapePK = dependencyDirection == DependencyDirection.PreRequisite ? workflowShape.PK : linkTargetHeaderShape.Identifier;

						var existingAttachmentsQuery = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, diagramShape.PK)
						{
							FetchOnlyFromLocalCache = !diagramShape.IsInDatabase
						};
						var attachments = jobHeader.Factory.Load<BMNCNAttachment>(existingAttachmentsQuery);

						var existingAttachment = attachments.SingleOrDefault(a => a.BNA_BNS_FromShape == fromShapePK && a.BNA_BNS_ToShape == toShapePK);
						if (existingAttachment == null)
						{
							var dependencyAttachment = jobHeader.Factory.New<BMNCNAttachment>();
							using (dependencyAttachment.SuspendSettingHasChanges())
							{
								dependencyAttachment.BNA_FP_ProcessHeaderLink = linkWithinJob.PK;
								dependencyAttachment.BNA_BNS_Owner = diagramShape.PK;
								dependencyAttachment.BNA_BNS_FromShape = fromShapePK;
								dependencyAttachment.BNA_BNS_ToShape = toShapePK;
								dependencyAttachment.BNA_Type = AttachmentTypeList.Codes.Dependency;
							}

							yield return dependencyAttachment;

							linkWithinJob.RegisterEditableChildObject(dependencyAttachment);
						}
						else
						{
							linkWithinJob.RegisterEditableChildObject(existingAttachment);
						}
					}
				}
			}
		}

		#endregion
	}
}
