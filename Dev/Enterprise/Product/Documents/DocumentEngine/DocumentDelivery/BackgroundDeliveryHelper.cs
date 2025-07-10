using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine
{
	internal static class BackgroundDeliveryHelper
	{
		internal static void PrepareDeliveryInstructions(StmDocumentDelivery documentDelivery, DeliveryInstructions deliveryInstructions, DocumentCommand documentCommand)
		{
			UpdateDeliveryInstructions(documentDelivery, deliveryInstructions);
			ValidateDeliveryInstructions(documentDelivery, deliveryInstructions, documentCommand);
		}

		static void ValidateDeliveryInstructions(StmDocumentDelivery documentDelivery, DeliveryInstructions deliveryInstructions, DocumentCommand documentCommand)
		{
			if (deliveryInstructions == null)
			{
				throw new ArgumentNullException(nameof(deliveryInstructions));
			}

			if (documentCommand == null)
			{
				throw new ArgumentNullException(nameof(documentCommand));
			}

			if (deliveryInstructions.DeliverablesToBePrinted.OfType<IDeliverable>().All(d => !d.IncludedInPrint))
			{
				var message = @$"DeliverablesToBePrinted not matched to StmDocumentDelivery
DocumentCommand: {documentCommand.HumanReadableName}, DocumentId:{documentCommand.DocumentId}, PK:{documentCommand.PK}, IsSystemDefined:{documentCommand.SU_IsSystemDefined},
SDL_ParentControllerIdOrTableCode:{documentDelivery.SDL_ParentControllerIdOrTableCode},
DeliverableCollection Detail: {GetDeliverableCollectionMessage(deliveryInstructions.DeliverablesToBePrinted)}
SDL_Instructions: {documentDelivery.SDL_Instructions}
Please check whether the document menu has been modified after creating the BDD(Background Document Delivery) task.";
				ErrorReporter.ReportOnce("DeliverablesToBePrinted Not Matched To StmDocumentDelivery", message);
			}

			string GetDeliverableCollectionMessage(DeliverableCollectionView view)
			{
				var sb = new StringBuilder();

				var reports = view.OfType<Report>();
				foreach (var report in reports)
				{
					sb.AppendLine($"Identifier:{report.Identifier}, Name:{report.Name}, HumanReadableName:{report.HumanReadableName}, IdentifiablePK:{report.IdentifiablePK}, MenuTemplatePivotPK:{report.MenuTemplatePivotPK}, ParentBusinessObject IsInDatabase:{report.ParentBusinessObject?.IsInDatabase}, ParentBusinessObject Type:{report.ParentBusinessObject?.GetType()?.FullName}");
				}
				return sb.ToString();
			}
		}

		public static void UpdateDeliveryInstructions(StmDocumentDelivery documentDelivery, DeliveryInstructions deliveryInstructions)
		{
			if (documentDelivery == null)
			{
				throw new ArgumentNullException(nameof(documentDelivery));
			}

			if (deliveryInstructions == null)
			{
				throw new ArgumentNullException(nameof(deliveryInstructions));
			}

			var serializableDeliveryInstructions = SerializableDeliveryInstructions.DeserializeDeliveryInstructions(documentDelivery);

			if (serializableDeliveryInstructions != null)
			{
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Language = serializableDeliveryInstructions.Language;
				deliveryInstructions.IsDraft = serializableDeliveryInstructions.IsDraft;

				deliveryInstructions.PrinterDelivery.PrintQueuePK = serializableDeliveryInstructions.PrintQueuePK;
				deliveryInstructions.PrinterDelivery.NumberOfCopies = serializableDeliveryInstructions.NumberOfCopies;

				if (!string.IsNullOrEmpty(serializableDeliveryInstructions.CoverNote))
				{
					deliveryInstructions.IncludeCoverNote = true;
					deliveryInstructions.CoverNote = serializableDeliveryInstructions.CoverNote;
				}
				else
				{
					deliveryInstructions.IncludeCoverNote = false;
				}

				if (!string.IsNullOrEmpty(serializableDeliveryInstructions.SpecifiedPageRanges))
				{
					deliveryInstructions.PageRangesSpecified = true;
					deliveryInstructions.SpecifiedPageRangesText = serializableDeliveryInstructions.SpecifiedPageRanges;
				}

				deliveryInstructions.PrintMultiDocPack = serializableDeliveryInstructions.PrintMultiDocPack;
				deliveryInstructions.AutoDeliverMultiDocPack = serializableDeliveryInstructions.AutoDeliverMultiDocPack;

				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				foreach (var r in serializableDeliveryInstructions.Recipients)
				{
					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.OrgHeaderPK = r.OrgHeaderPK;
					recipient.DeliveryMethod = r.DeliveryMethod;
					recipient.AttachmentType = r.AttachmentType;
					recipient.SendIndividually = r.SendIndividually;
					recipient.DeliveryAddress = r.DeliveryAddress;
					recipient.Salutation = r.Salutation;
					recipient.EmailFromAddressWithType = r.SendFrom;
					recipient.EmailCarbonCopyRecipientsAsString = r.EmailCarbonCopyRecipientsAsString;
					recipient.EmailBlindCarbonCopyRecipientsAsString = r.EmailBlindCarbonCopyRecipientsAsString;
					recipient.EmailSubjectMacro = r.EmailSubjectMacro;
					recipient.StaffCode = r.StaffCode;
					recipient.Name = r.ContactName;
				}

				if (!deliveryInstructions.DocumentsToBeDelivered.IsNullOrEmpty() && !serializableDeliveryInstructions.DocumentsToBeDelivered.IsNullOrEmpty())
				{
					deliveryInstructions.DocumentsToBeDelivered.OfType<IDeliverable>().ForEach(d =>
					{
						var document = serializableDeliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(i =>
						{
							if (!i.IdentifiablePK.IsEmpty && !i.MenuTemplatePivotPK.IsEmpty && !d.IdentifiablePK.IsEmpty && !d.MenuTemplatePivotPK.IsEmpty)
							{
								return i.IdentifiablePK == d.IdentifiablePK && i.MenuTemplatePivotPK == d.MenuTemplatePivotPK;
							}
							else
							{
								return i.Identifier == d.Identifier;
							}
						});
						d.UpdateDocuments(document);
					});
				}

				if (!deliveryInstructions.EDocsToBeDelivered.IsNullOrEmpty() && !serializableDeliveryInstructions.EDocsToBeDelivered.IsNullOrEmpty())
				{
					deliveryInstructions.EDocsToBeDelivered.OfType<IDeliverable>().ForEach(d =>
					{
						d.UpdateEDocs(serializableDeliveryInstructions.EDocsToBeDelivered.FirstOrDefault(i => i.Identifier == d.Identifier));
					});
				}
			}
		}

		static void UpdateDocuments(this IDeliverable deliverable, SerializableDeliveryInstructions.Deliverable serializedDeliverable)
		{
			if (serializedDeliverable != null)
			{
				deliverable.IncludedInPrint = true;
				deliverable.Index = serializedDeliverable.Index;
				deliverable.PrinterDetails.PrintQueuePK = serializedDeliverable.PrintQueuePK;
				deliverable.PrinterDetails.NumberOfCopies = serializedDeliverable.Copies;
			}
			else
			{
				deliverable.IncludedInPrint = false;
			}
		}

		static void UpdateEDocs(this IDeliverable deliverable, SerializableDeliveryInstructions.Deliverable serializedDeliverable)
		{
			if (serializedDeliverable != null)
			{
				deliverable.IncludedInPrint = true;
				deliverable.Index = serializedDeliverable.Index;
			}
			else
			{
				deliverable.IncludedInPrint = false;
			}
		}
	}
}
