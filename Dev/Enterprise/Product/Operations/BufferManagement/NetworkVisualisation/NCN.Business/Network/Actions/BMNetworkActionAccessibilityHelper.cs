using CargoWise.NetworkVisualisation.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class BMNetworkActionAccessibilityHelper
	{
		public static NetworkActionAccessibility CheckEntityIsNotDeleted(ShapeNetworkEntity entity) => new NetworkActionAccessibility(
			!entity.IsDeleted,
			entity,
			() => Res.GetString("73D9471F-A60C-4AA1-81CC-4E6538986D36", "The entity was removed from the database."));

		public static NetworkActionAccessibility CheckEntityIsRoot(ShapeNetworkEntity entity) => new NetworkActionAccessibility(
			entity.IsRoot,
			entity,
			() => Res.GetString("8061CB24-1835-4E25-9D06-420CBE5D057B", "This action is accessible to the root diagram only."));

		public static NetworkActionAccessibility CheckEntityIsRoot(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.RootShapePK == shape.PK,
			shape,
			() => Res.GetString("2251e88c-5713-4fae-abe3-9d7263f5c45e", "This action is accessible to the root diagram only."));

		public static NetworkActionAccessibility CheckEntityIsNotRoot(ShapeNetworkEntity entity) => new NetworkActionAccessibility(
			!entity.IsRoot,
			entity,
			() => Res.GetString("A2CC003C-E3E3-4F4E-9DC8-1B5C13CDC213", "This action is not accessible to the root diagram."));

		public static NetworkActionAccessibility CheckParentOfShapeIsRootDiagram(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.BNS_BNS_ParentShape == shape.BNS_BNS_RootShape,
			shape,
			() => Res.GetString("c88d67c4-25b9-4be1-9a4c-251799cbebfd", "This action is only applicable to shapes that are placed directly on the diagram surface."));

		public static NetworkActionAccessibility CheckIsShape(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.BNS_ShapeType == ShapeTypeList.Codes.Shape,
			shape,
			() => Res.GetString("FA2046DE-B358-436C-B30B-6B5E9E5476C8", "The action can be applied to shapes only."));

		public static NetworkActionAccessibility CheckIsShapeOrAnnotation(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.BNS_ShapeType == ShapeTypeList.Codes.Shape || shape.BNS_ShapeType == ShapeTypeList.Codes.Annotation,
			shape,
			() => Res.GetString("9b50db4e-e6e2-4923-ad1d-d3fd3e7dba4e", "The action can be applied to shapes and annotations only."));

		public static NetworkActionAccessibility CheckShapeCanHaveChildren(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.CanHaveChildren,
			shape,
			() => Res.GetString("8F4E4DC0-7BC7-496A-9BCA-4BE928923A17", "The shape should not be a buffer or annotation."));

		public static NetworkActionAccessibility CheckShapeIsNotAnnotation(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsAnnotation,
			shape,
			() => Res.GetString("83F3BE5E-6080-456D-A706-233686090F12", "The shape should not be an annotation."));

		public static NetworkActionAccessibility CheckShapeIsLinkedToEntity(BMNCNShape shape, bool needsNotification = true) => new NetworkActionAccessibility(
			shape.LinkedEntity != null,
			() => new NetworkActionDenialReason(
				shape,
				Res.GetString("9D291778-9396-46B5-988C-79E82BC4B41C", "The shape should be linked to an entity"),
				needsNotification));

		public static NetworkActionAccessibility CheckShapeIsLinkedToRealEntity(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.IsLinkedToRealEntity,
			shape,
			() => Res.GetString("D0427BE4-6D76-43B9-BD36-3E00344DF611", "The shape should be linked to a job or workflow."));

		public static NetworkActionAccessibility CheckShapeIsNotLinkedToRealEntity(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsLinkedToRealEntity,
			shape,
			() => Res.GetString("F3BFB1D5-7546-48DC-B885-9CDA836486FB", "The shape is already linked to a job or workflow."));

		public static NetworkActionAccessibility CheckShapeIsLinkedToRealEntityEnabledForBMS(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.IsLinkedToRealEntityEnabledForBMS,
			shape,
			() => Res.GetString("1E41FE05-4A54-4A2D-9C87-767E74998026", "The shape should be linked to a job or workflow of a job which type is associated with and enabled for a Buffer Management System."));

		public static NetworkActionAccessibility CheckShapeBelongsToDefaultDiagram(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.IsDefaultDiagram || shape.IsDefaultDiagramChild,
			shape,
			() => Res.GetString("77CA1C8E-D9E7-43ED-A7BB-BFF383828688", "This action is accessible in the Workflow Relationship Designer only."));

		public static NetworkActionAccessibility CheckShapeDoesNotBelongToDefaultDiagram(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsDefaultDiagram && !shape.IsDefaultDiagramChild,
			shape,
			() => Res.GetString("37CFBCE2-A4CF-47BA-B5E6-53200C133EC0", "This action is not accessible in the Workflow Relationship Designer."));

		public static NetworkActionAccessibility CheckShapeIsNotDefaultDiagram(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsDefaultDiagram,
			shape,
			() => Res.GetString("71958E60-56C6-4F4C-9E71-E798160BE081", "The operation is not applicable to jobs."));

		public static NetworkActionAccessibility CheckShapeIsNotDefaultDiagramChild(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsDefaultDiagramChild,
			shape,
			() => Res.GetString("DEB0C085-25E5-4A46-9AD1-3DEFEDB9653F", "The operation is not applicable to workflows."));

		public static NetworkActionAccessibility CheckShapeIsNormalDiagramOrChild(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
			.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape));

		public static NetworkActionAccessibility CheckDiagramIsScaled(ShapeNetworkEntity diagramEntity, BMNCNShape shape) => new NetworkActionAccessibility(
			diagramEntity.IsScaled,
			shape,
			() => Res.GetString("08CCC00E-9C72-4E48-B6D3-A06AE3A927DC", "Cannot execute when the diagram is not in scaled mode."));

		public static NetworkActionAccessibility CheckDiagramIsNotScaled(ShapeNetworkEntity diagramEntity, BMNCNShape shape) => new NetworkActionAccessibility(
			!diagramEntity.IsScaled,
			shape,
			() => Res.GetString("4A6F3AFF-8D6D-4E4B-A00B-C3B5B98DF7D3", "Cannot execute when the diagram is in scaled mode."));

		public static NetworkActionAccessibility CheckShapeIsApproved(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.IsApproved,
			shape,
			() => Res.GetString("97B90C0D-9624-4201-960C-D4896EAFF8C6", "The shape should be approved."));

		public static NetworkActionAccessibility CheckShapeIsNotApproved(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsApproved,
			shape,
			() => Res.GetString("6329F1FF-17CF-4F68-8BF4-C114365AFE9F", "The shape should not be approved."));

		public static NetworkActionAccessibility CheckShapeIsNotPinned(BMNCNShape shape) => new NetworkActionAccessibility(
			!shape.IsPinned,
			shape,
			() => Res.GetString("32aaa106-d539-4225-856c-27dde9d979a4", "The action is not applicable to pinned shapes."));

		public static NetworkActionAccessibility CheckCanUnlinkEntity(BMNCNShape shape, bool needsNotification = true) => CheckShapeDoesNotBelongToDefaultDiagram(shape).UnionIfAllowed(() => CheckShapeIsLinkedToEntity(shape, needsNotification));

		public static NetworkActionAccessibility CheckCanDoActionOnJobType(BMNCNShape shape, string jobType) => new NetworkActionAccessibility(
			shape.Lookups.JobTypes.ContainsCode(jobType),
			shape,
			() => Res.GetString("571b8456-a5df-4fad-80da-385e0ff628da", "There is no buffer management system registered for this job type."));

		public static NetworkActionAccessibility CheckDiagramIsShowingNonScheduledSection(BMNCNShape shape) => new NetworkActionAccessibility(
			(shape.RootShape as BMNCNRootDiagramShape)?.ShouldShowNonScheduledSection ?? false,
			shape,
			() => Res.GetString("cc70174d-97f3-4fd1-a8cd-84bb675c02e8", "The Non-Scheduled section is not enabled for this diagram."));

		public static NetworkActionAccessibility CheckShapeHasJobTypes(BMNCNShape shape) => new NetworkActionAccessibility(
			shape.Lookups.JobTypes.Count != 0,
			shape,
			() => Res.GetString("58FCB6A5-2D93-4848-ACB0-56DC7D0E57D1", "At least one job type must be associated with a Buffer Management System."));
	}
}
