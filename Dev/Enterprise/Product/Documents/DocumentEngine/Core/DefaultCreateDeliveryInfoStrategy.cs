using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	using System.IO;
	using System.Linq;
	using Enterprise.DocumentEngine.DeliveryMethods;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;

	public class DefaultCreateDeliveryInfoStrategy : ICreateDeliveryInfoStrategy
	{
		public DeliveryInfo CreateDeliveryInfo(IDeliverable deliverable, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			var outputStream = new MemoryStream();

			var report = deliverable as Report;
			if (report != null && report.IsLegacyDocument && deliveryInstructions.DocPack != null)
			{
				report.TranslateLegacyDocument = deliveryInstructions.DocPack.SupportsLanguageSelection;
			}

			deliverable.Save(deliveryContact, deliveryInstructions.OfficialRecipient, outputStream);

			if (outputStream.Length != 0 && !ShouldExclude(report, deliveryInstructions))
			{
				var deliveryInfo = deliverable.GetDeliveryInfo(deliveryInstructions.IsDraft);

				var deliveryGroupId = ZGuid.Empty;
				if (deliveryContact != null)
				{
					deliveryGroupId = deliveryContact.DeliveryGroupId;
				}
				if (deliveryGroupId.IsEmpty)
				{
					var deliveryGroup = deliveryInstructions.DeliveryGroups.FirstOrDefault(group => deliverable != null && group.PK == deliverable.DeliveryGroupID) ?? deliveryInstructions.DeliveryGroups[0];
					deliveryGroupId = deliveryGroup.PK;
				}

				if (deliveryContact != null && !deliveryContact.EmailFromAddress.IsEmpty)
				{
					deliveryInfo.EmailFromAddress = deliveryContact.EmailFromAddress;
				}

				deliveryInfo.DeliveryGroupID = deliveryGroupId;
				deliveryInfo.RunDateTime = deliveryInstructions.RunDateTime;
				deliveryInfo.Instructions = deliveryInstructions;
				deliveryInfo.SetFileContents(outputStream, deliverable.FileExtension);

				var deliveryInstructionsWithEmailSubjectOverride = deliveryInstructions as IDeliveryInstructionsWithEmailSubjectOverride;
				if (!(deliveryInstructionsWithEmailSubjectOverride?.EmailSubjectOverride ?? string.Empty).IsEmpty)
				{
					deliveryInfo.EmailSubjectLine = deliveryInstructionsWithEmailSubjectOverride.EmailSubjectOverride;
				}

				IDocumentSupportable documentSupportable = null;

				if (report?.MenuItem is DocumentCommand documentCommand)
				{
					documentSupportable = documentCommand.Parent;
				}

				if (documentSupportable == null && deliverable is IeDoc eDoc && eDoc.ParentMain is IStorageMain storageMain)
				{
					documentSupportable = storageMain.DocumentOwner as IDocumentSupportable;
				}

				if (documentSupportable != null && documentSupportable.DocumentSupporter != null)
				{
					deliveryInfo.PDFEncryptionPassword = documentSupportable.DocumentSupporter.GetEncryptedPDFPassword(new DeliverableInfo(deliverableName: deliverable.Name,
																											documentType: deliverable.DocumentTypeCode,
																											menuItem: deliverable.MenuItem,
																											orgHeader: deliveryContact?.OrgHeader));
				}

				return deliveryInfo;
			}

			return null;
		}

		static bool ShouldExclude(Report report, DeliveryInstructions deliveryInstructions)
		{
			var result = false;

			if (report != null && deliveryInstructions.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows)
			{
				var analyzer = report.Analyser;
				if (analyzer != null && analyzer.Sections.Count > 0 && !report.ContainsDataRows)
				{
					result = true;
				}
			}

			if (report != null && deliveryInstructions.ExcludeDocumentsWhichContainNoBusinessObjectDataRows && !report.ContainsDataRowsForBusinessObjectDataSource)
			{
				result = true;
			}

			return result;
		}
	}
}
