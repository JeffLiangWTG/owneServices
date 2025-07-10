using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business.Utilities.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Shape
{
	public class BMNCNShapeCopierForUniversalCopy : IUniversalCopyCustomFinishCopyAction
	{
		public int Priority { get => 1; }

		public void FinishCopyAction(Dictionary<object, object> copiedEntities)
		{
			if (copiedEntities is null)
			{
				return;
			}

			var copiedShapeEntities = copiedEntities
				.Where(keyValuePair => keyValuePair.Key is BMNCNShape && keyValuePair.Value is BMNCNShape)
				.ToDictionary(keyValuePair => (BMNCNShape)keyValuePair.Key, keyValuePair => (BMNCNShape)keyValuePair.Value);

			SetBMNCNShapesLayoutDataForUniversalCopy(copiedShapeEntities);
			SetBMNCNShapeParentShapesForUniversalCopy(copiedShapeEntities);
			SetBMNCNShapeLinkedCopiedWorkflows(copiedEntities, copiedShapeEntities);
			var newCopiedEntities = CopyBMNCNShapeAttachementsForUniversalCopy(copiedShapeEntities);

			foreach (var key in newCopiedEntities.Keys)
			{
				copiedEntities.Add(key, newCopiedEntities[key]);
			}
		}

		void SetBMNCNShapesLayoutDataForUniversalCopy(Dictionary<BMNCNShape, BMNCNShape> copiedShapeEntities)
		{
			foreach (var sourceShape in copiedShapeEntities.Keys)
			{
				var copiedShape = copiedShapeEntities[sourceShape];
				copiedShape.Left = sourceShape.Left;
				copiedShape.Top = sourceShape.Top;
				copiedShape.ScrollPosition = sourceShape.ScrollPosition;
				copiedShape.BackColor = sourceShape.BackColor;
				copiedShape.ForeColor = sourceShape.ForeColor;
				copiedShape.CornerRadius = sourceShape.CornerRadius;
				copiedShape.Width = sourceShape.Width;
				copiedShape.Height = sourceShape.Height;
				copiedShape.ZIndex = sourceShape.ZIndex;
			}
		}

		void SetBMNCNShapeParentShapesForUniversalCopy(Dictionary<BMNCNShape, BMNCNShape> copiedShapeEntities)
		{
			foreach (var sourceShape in copiedShapeEntities.Keys)
			{
				if (sourceShape.HasParent)
				{
					var sourceParent = copiedShapeEntities.Keys.FirstOrDefault(shape => shape.PK == sourceShape.BNS_BNS_ParentShape);
					if (sourceParent is not null)
					{
						var copiedShape = copiedShapeEntities[sourceShape];
						copiedShape.BNS_BNS_ParentShape = copiedShapeEntities[sourceParent].PK;
					}
				}
			}
		}

		void SetBMNCNShapeLinkedCopiedWorkflows(Dictionary<object, object> copiedEntities, Dictionary<BMNCNShape, BMNCNShape> copiedShapeEntities)
		{
			foreach (var sourceShape in copiedShapeEntities.Keys)
			{
				if (sourceShape.BNS_RelatedEntityID != ZGuid.Empty)
				{
					var copiedShape = copiedShapeEntities[sourceShape];
					if (copiedShape.BNS_RelatedEntityID == ZGuid.Empty)
					{
						var sourceRelatedEntity = copiedEntities.Keys.FirstOrDefault(k => k is ProcessHeader processHeader && processHeader.PK == sourceShape.BNS_RelatedEntityID);
						if (sourceRelatedEntity is not null)
						{
							copiedShape.BNS_RelatedEntityID = ((BusinessObject)copiedEntities[sourceRelatedEntity]).PK;
						}
						else
						{
							copiedShape.BNS_RelatedEntityTableCode = ZString.Empty; // We could not copy the related entity for this shape
						}
					}
				}
			}
		}

		Dictionary<object, object> CopyBMNCNShapeAttachementsForUniversalCopy(Dictionary<BMNCNShape, BMNCNShape> copiedShapeEntities)
		{
			var additionalCopiedEntities = new Dictionary<object, object>();

			var attachments = copiedShapeEntities.Keys.SelectMany(sourceChildShape => sourceChildShape.AllAttachments
				.Where(a => a.FromShape == sourceChildShape
					&& copiedShapeEntities.Keys.Contains(a.ToShape)
					&& copiedShapeEntities.Keys.Contains(a.OwnerShape)))
				.ToArray();

			foreach (var attachment in attachments)
			{
				var clonedAttachment = (BMNCNAttachment)attachment.Clone(new BusinessObjectCloneArgs(new[] {
					BMNCNAttachmentSchema.BNA_BNS_FromShape.Name,
					BMNCNAttachmentSchema.BNA_BNS_ToShape.Name,
					BMNCNAttachmentSchema.BNA_BNS_Owner.Name,
					BMNCNAttachmentSchema.BNA_FP_ProcessHeaderLink.Name }));

				var clonedFromShape = copiedShapeEntities[attachment.FromShape];
				var clonedToShape = copiedShapeEntities[attachment.ToShape];
				var clonedOwnerShape = copiedShapeEntities[attachment.OwnerShape];

				clonedAttachment.SuspendValidation();

				if (attachment.IsDependencyLink && clonedFromShape.ProcessHeader is not null && clonedToShape.ProcessHeader is not null)
				{
					var clonedProcessHeaderLink = clonedFromShape.ProcessHeader.LinksFromMeToOthers.FirstOrDefault(l => l.HeaderTo == clonedToShape.ProcessHeader);

					if (clonedProcessHeaderLink is null)
					{
						clonedProcessHeaderLink = (ProcessHeaderLink)attachment.ProcessHeaderLink.Clone(new BusinessObjectCloneArgs(new[] {
								ProcessHeaderLinkSchema.FP_FH_HeaderFrom.Name,
								ProcessHeaderLinkSchema.FP_FH_HeaderTo.Name }));

						clonedProcessHeaderLink.SuspendValidation();
						clonedProcessHeaderLink.FP_FH_HeaderFrom = clonedFromShape.ProcessHeader.PK;
						clonedProcessHeaderLink.FP_FH_HeaderTo = clonedToShape.ProcessHeader.PK;
						clonedProcessHeaderLink.ResumeValidation();

						additionalCopiedEntities.Add(attachment.ProcessHeaderLink, clonedProcessHeaderLink);
					}

					clonedAttachment.BNA_FP_ProcessHeaderLink = clonedProcessHeaderLink.PK;
				}

				clonedAttachment.BNA_BNS_Owner = clonedOwnerShape.PK;
				clonedAttachment.BNA_BNS_FromShape = clonedFromShape.PK;
				clonedAttachment.BNA_BNS_ToShape = clonedToShape.PK;

				clonedAttachment.ResumeValidation();

				additionalCopiedEntities.Add(attachment, clonedAttachment);
			}

			return additionalCopiedEntities;
		}
	}
}
