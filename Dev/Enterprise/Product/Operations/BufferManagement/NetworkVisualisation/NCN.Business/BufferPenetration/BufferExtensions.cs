using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class BufferExtensions
	{
		public static ShapeNetworkEntity GetBuffer(this NetworkAttachment attachment)
		{
			var network = attachment.From?.Network;
			if (network != null)
			{
				var entities = network.Entities;

				var bufferShape = GetBuffer(attachment.Attachment, (fromShape) => entities.GetInstance(fromShape)
				.PostRequisiteEntities.FirstOrDefault(s => s.IsBufferShape && s.PostRequisiteLinks.Any(a => a.Attachment.BNA_BNS_ToShape == attachment.Attachment.BNA_BNS_ToShape))?.Shape);

				if (bufferShape != null)
				{
					return entities.GetInstance(bufferShape);
				}
			}

			return null;
		}

		/// <summary>
		/// Using this method is not recommended as loading dependencies directly off the BMNCNShape is expensive.
		/// </summary>
		public static BMNCNShape GetBuffer(this BMNCNAttachment attachment)
		{
			return GetBuffer(attachment, (fromShape) => fromShape.PostrequisiteShapes.FirstOrDefault(s => s.IsBufferShape && s.PostRequisiteLinks.Cast<BMNCNAttachment>().Any(a => a.BNA_BNS_ToShape == attachment.BNA_BNS_ToShape)));
		}

		static BMNCNShape GetBuffer(this BMNCNAttachment attachment, Func<BMNCNShape, BMNCNShape> getShapeFromRelationshipCollection)
		{
			if (attachment.BNA_Type == AttachmentTypeList.Codes.Dependency)
			{
				var fromShape = attachment.FromShape;
				var toShape = attachment.ToShape;

				if (fromShape != null && toShape != null)
				{
					if (toShape.IsBufferShape)
					{
						return toShape;
					}
					else if (fromShape.IsBufferShape)
					{
						return fromShape;
					}
					else
					{
						return getShapeFromRelationshipCollection(fromShape);
					}
				}
			}
			return null;
		}

		public static ShapeNetworkEntity CreateBuffer(this NetworkAttachment networkAttachment)
		{
			if (networkAttachment.GetBuffer() != null)
			{
				throw new InvalidOperationException("There is already a buffer on this dependency arrow");
			}

			var ownerShape = networkAttachment.Owner.Shape;

			if (!ownerShape.IsScaled)
			{
				throw new InvalidOperationException("How can you add a buffer to an unscaled shape?");
			}

			var buffer = networkAttachment.Attachment.Factory.New<BMNCNBufferShape>();
			buffer.MakeChildOf(ownerShape);
			buffer.BNS_Name = Res.GetString("1c87fd2b-4e90-42b6-9d2a-0c6955f07eed", "New Buffer");
			buffer.BufferType = BufferTypeList.Codes.Feeding;

			var attachment = networkAttachment.Attachment;

			CreateAttachmentOnSameLink(attachment, attachment.BNA_BNS_FromShape, buffer.PK, AttachmentTypeList.Codes.Dependency);
			CreateAttachmentOnSameLink(attachment, buffer.PK, attachment.BNA_BNS_ToShape, AttachmentTypeList.Codes.Dependency);

			var sourceShape = networkAttachment.From;
			var root = sourceShape.Root;
			var pixelsPerResolutionIncrement = root != null ? root.GetScaledPixelIncrements() : 0;
			attachment.RegisterEditableChildObject(buffer);

			var bufferEntity = sourceShape.Network.Entities.GetInstance(buffer);

			bufferEntity.Height = CCPMConstants.BufferShapeDefaultHeight;
			bufferEntity.LocateAfterShape(sourceShape);

			if (!double.IsNaN(pixelsPerResolutionIncrement))
			{
				bufferEntity.Width = pixelsPerResolutionIncrement * 3.0;
			}

			return bufferEntity;
		}

		static void CreateAttachmentOnSameLink(BMNCNAttachment crossingAttachment, ZGuid fromShapePK, ZGuid toShapePK, string type)
		{
			var attachment = crossingAttachment.Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_Owner = crossingAttachment.BNA_BNS_Owner;
			attachment.BNA_FP_ProcessHeaderLink = crossingAttachment.BNA_FP_ProcessHeaderLink;
			attachment.BNA_BNS_FromShape = fromShapePK;
			attachment.BNA_BNS_ToShape = toShapePK;
			attachment.BNA_Type = type;

			crossingAttachment.RegisterEditableChildObject(attachment);
		}
	}
}
