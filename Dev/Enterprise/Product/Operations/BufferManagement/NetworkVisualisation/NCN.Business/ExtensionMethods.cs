using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class ExtensionMethods
	{
		#region Network Refresh

		public static void Refresh(this IJobNetwork network, RefreshType type, params ShapeNetworkEntity[] entities)
		{
			network.Refresher.Refresh(new RefreshArgs(type, entities));
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public static void SetWidthForDuration(this ShapeNetworkEntity shapeEntity, int durationInMinutes)
		{
			var root = shapeEntity.Root;
			if (root.IsScaled)
			{
				var minutes = Math.Max(durationInMinutes, new ZDecimal(root.Shape.ResolutionIncrement.GetMinutesFromDateTimeSpan()).Round(0).ToZInt());
				if (shapeEntity.ShouldStoreWidthOnSchedule)
				{
					shapeEntity.Shape.ExplicitDurationMinutes = minutes;
				}
				else
				{
					shapeEntity.Width = ShapeOffsetToDateConverter.GetSizeForMinutes(root.Shape.Scale, minutes);
				}
			}
		}

		public static BMNCNShapeDefaultDiagram GetDefaultDiagram(this ProcessJobHeader header)
		{
			return (BMNCNShapeDefaultDiagram)GetDefaultShape(header, null);
		}

		public static BMNCNShape GetDefaultShape(this ProcessHeader header, BMNCNShapeDefaultDiagram diagram)
		{
			return header.CreateOrLoadDefaultShape(diagram);
		}

		public static BMNCNShape[] GetShapesWithinSameDiagram(this BMNCNShape diagram)
		{
			return diagram.Factory.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_BNS_RootShape, diagram.RootShapePK));
		}

		public static ZQuery GetDescendantAttachmentQuery(IEnumerable<BMNCNShape> shapes)
		{
			return new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, SQLComparisonOperator.Equal, shapes.Select(s => s.PK));
		}

		#region Default Diagram

		static BMNCNShape CreateOrLoadDefaultShape(this ProcessHeader header, BMNCNShapeDefaultDiagram diagram)
		{
			if (header.IsWorkflow)
			{
				return header.CreateOrLoadDefaultWorkflowShape(diagram);
			}
			else
			{
				return ((ProcessJobHeader)header).CreateOrLoadDefaultDiagram();
			}
		}

		static BMNCNShape CreateOrLoadDefaultWorkflowShape(this ProcessHeader workflow, BMNCNShapeDefaultDiagram diagram)
		{
			return workflow.LoadDefaultWorkflowShape() ?? workflow.CreateDefaultWorkflowShape(diagram);
		}

		static BMNCNShapeDefaultDiagram CreateOrLoadDefaultDiagram(this ProcessJobHeader jobHeader)
		{
			return jobHeader.LoadDefaultDiagramShape() ?? jobHeader.CreateDefaultDiagramShape();
		}

		static BMNCNShape LoadDefaultWorkflowShape(this ProcessHeader workflow)
		{
			var defaultShapeQuery = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow.PK);
			defaultShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.DefaultWorkflow);
			defaultShapeQuery.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);
			defaultShapeQuery.FetchOnlyFromLocalCache = !workflow.IsInDatabase;

			return workflow.Factory.LoadTop1<BMNCNShape>(defaultShapeQuery);
		}

		static BMNCNShapeDefaultDiagram LoadDefaultDiagramShape(this ProcessJobHeader jobHeader)
		{
			var defaultShapeQuery = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, jobHeader.PK);
			defaultShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.DefaultDiagram);
			defaultShapeQuery.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);
			defaultShapeQuery.FetchOnlyFromLocalCache = !jobHeader.IsInDatabase;

			return jobHeader.Factory.LoadTop1<BMNCNShapeDefaultDiagram>(defaultShapeQuery);
		}

		static BMNCNShape CreateDefaultWorkflowShape(this ProcessHeader workflow, BMNCNShapeDefaultDiagram diagram)
		{
			var shape = workflow.Factory.New<BMNCNShape>();

			using (shape.SuspendSettingHasChanges())
			{
				shape.BNS_RelatedEntityID = workflow.PK;
				shape.BNS_ShapeType = ShapeTypeList.Codes.DefaultWorkflow;
				shape.BNS_Name = workflow.FH_CompletionStatement.SubstringSafe(0, BMNCNShapeSchema.BNS_Name.MaxLength);

				shape.MakeChildOf(diagram);
			}

			return shape;
		}

		static BMNCNShapeDefaultDiagram CreateDefaultDiagramShape(this ProcessJobHeader jobHeader)
		{
			var diagramShape = jobHeader.Factory.New<BMNCNShapeDefaultDiagram>();

			using (jobHeader.SuspendSettingHasChanges())
			{
				using (diagramShape.SuspendSettingHasChanges())
				{
					diagramShape.BNS_RelatedEntityID = jobHeader.PK;
					diagramShape.BNS_Name = jobHeader.FH_CompletionStatement.SubstringSafe(0, BMNCNShapeSchema.BNS_Name.MaxLength);
				}
			}

			return diagramShape;
		}

		#endregion

		public static BMNCNShape AsShape(this IProposedNetworkEntity entity)
		{
			return (entity as ShapeNetworkEntity)?.Shape ?? entity as BMNCNShape;
		}

		public static BMNCNAttachment AsAttachment(this IEntityRelationship relationship)
		{
			return (relationship as NetworkAttachment)?.Attachment ?? relationship as BMNCNAttachment;
		}

		public static IEnumerable<BMNCNShape> AsShapes<T>(this IEnumerable<T> entities)
			where T : IProposedNetworkEntity
		{
			return entities.Select(s => s.AsShape());
		}

		public static IEnumerable<BMNCNShape> GetAncestorShapes(this BMNCNShape shape, JobNetwork network)
		{
			BMNCNShape parentShape;
			ShapeNetworkEntity entity;

			while (shape != null && (entity = network.Entities.GetInstance(shape)) != null)
			{
				parentShape = ((INetworkEntity)entity.Owner)?.AsShape();

				if (parentShape != null)
				{
					yield return parentShape;
				}

				shape = parentShape;
			}
		}

		#region NetworkViewModel

		public static IJobNetwork GetJobNetwork(this INetworkViewModel networkViewModel)
		{
			var network = networkViewModel?.Network;
			var jobNetwork = network as IJobNetwork;

			if (jobNetwork == null && network != null)
			{
				ErrorReporter.ReportOnce("Cannot cast Network to IJobNetwork. if you obtained the network view model from NetworkDiagramForm, don't forget to use Application.DoEvents().");
			}
			return jobNetwork;
		}

		public static IBMNetworkEntityController GetJobController(this INetworkViewModel networkViewModel) => (IBMNetworkEntityController)networkViewModel?.Controller;

		#endregion
	}
}
