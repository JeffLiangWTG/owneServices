using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public sealed class FormDeliveryInfoStrategy : IFormDeliveryInfoStrategy
	{
		public DeliveryInfo CreateDeliveryInfo(IDeliverable deliverable, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			if (!(deliverable is IDocumentDeliverable documentDeliverable)
				|| deliveryContact == null
				|| deliveryInstructions == null)
			{
				return null;
			}

			var fileType = deliveryInstructions.Destination == DeliveryInstructionDestination.Preview
				? FileType.XLS
				: FileType.PDF;

			var	deliveryInfo = documentDeliverable.GetDeliveryInfo(deliveryInstructions.IsDraft, fileType);

			if (deliveryInfo == null)
			{
				return deliveryInfo;
			}

			deliveryInfo.Instructions = deliveryInstructions;

			if (deliveryInstructions.Destination == DeliveryInstructionDestination.Preview)
			{
				return deliveryInfo;
			}

			if (deliveryInstructions.GetOrCreateDeliveryGroup(deliveryContact, DeliveryExtensions.DeliveryGroupMatchStrategies.MatchAny) is StmDeliveryGroup deliveryGroup)
			{
				deliveryInfo.DeliveryGroupID = deliveryGroup.PK;
			}

			AddDocEngineEmailSupport(deliveryInfo, documentDeliverable);
			AddDocEngineEDocSupport(deliveryInfo, documentDeliverable);

			return deliveryInfo;
		}

		void AddDocEngineEmailSupport(DeliveryInfo deliveryInfo, IDocumentDeliverable documentDeliverable)
		{
			deliveryInfo.EmailSubjectLine = documentDeliverable.GetEmailSubjectLine();
			deliveryInfo.EmailSignature = documentDeliverable.GetEmailSignature();
		}

		void AddDocEngineEDocSupport(DeliveryInfo deliveryInfo, IDocumentDeliverable documentDeliverable)
		{
			if (documentDeliverable.EDocsParent is IBusiness biz)
			{
				deliveryInfo.ParentGuid = biz.Identifier;
				deliveryInfo.ParentTableName = biz.TableName;
				deliveryInfo.RelatedBusinessContext = documentDeliverable.EDocsParent?.DocManagerInfo?.DocManagerCode;
			}
		}
	}
}
