using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionAttachmentValidation : ZValidation
	{
		public EntryInstructionAttachmentValidation(EntryInstructionAttachment parent) : base(parent)
		{
			Parent = parent;
		}
		protected readonly EntryInstructionAttachment Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.EntryInstruction?.JobDeclaration;

		public override Type AutoValidationType => typeof(EntryInstructionAttachmentValidation);

		public override void ValidateAll()
		{
			ValidateAttachmentType();
			ValidateAttachmentNumber();
			ValidateEDoc();
		}

		#region AttachmentType

		public void ValidateAttachmentType()
		{
			ValidateCalculatedProperty(Parent.AttachmentTypeInfo);
		}

		protected void CheckAttachmentType()
		{
			var docType = Parent.AttachmentType;

			MandatoryValidation.CheckEntered(Parent.AttachmentTypeInfo);
			Parent.AttachmentTypeInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

			if (!docType.IsEmpty)
			{
				CheckAttachmentTypeDuplication();
				CheckLinkToInvoiceLines();

				ValidateAttachmentNumber();
				ValidateEDoc();
			}
		}

		void CheckAttachmentTypeDuplication()
		{
			var attachmentType = Parent.AttachmentType;
			if (!attachmentType.IsEmpty
				&& Parent.EntryInstruction is CusEntryInstruction entryInstruction
				&& entryInstruction.Attachments.Cast<EntryInstructionAttachment>().Any(att => att != Parent && att.AttachmentType == attachmentType))
			{
				Parent.AttachmentTypeInfo.AddNotification(Res.GetString("A97BF610-204E-4334-8501-E719A0244E2A", "The type is duplicated."), ValidationModeProvider);
			}
		}

		void CheckLinkToInvoiceLines()
		{
			if (Parent.CanLinkToInvoiceLine && Parent.CusStorageDocPivot?.InvoiceLineLinks.Count == 0)
			{
				Parent.AttachmentTypeInfo.AddNotification(Res.GetString("52E646B0-1C6B-4DDC-BA15-5E4C0E3B52B1", "Invoice Lines should be linked to this Attachment."), ValidationModeProvider);
			}
		}

		#endregion

		#region AttachmentNumber

		public void ValidateAttachmentNumber()
		{
			ValidateCalculatedProperty(Parent.AttachmentNumberInfo);
		}

		protected void CheckAttachmentNumber()
		{
			var docType = Parent.AttachmentType;
			var docNumber = Parent.AttachmentNumber;
			var targetInfo = Parent.AttachmentNumberInfo;
			if (docType == CSDDocTypeList.Codes._10000001 && !(docNumber.IsNumbersOnlyOrEmpty && docNumber.Length == 17))
			{
				targetInfo.AddNotification(Res.GetString("AA337E4E-E920-4284-A58D-9BC40D84C5FD", "The attachment number should be 17 digits."), ValidationModeProvider);
			}
			else if (docType == CSDDocTypeList.Codes._10000002 && !(docNumber.IsNumbersOnlyOrEmpty && docNumber.Length == 12))
			{
				targetInfo.AddNotification(Res.GetString("FF001417-4ABF-43DF-BAE0-50A4B9F61C57", "The attachment number should be 12 digits."), ValidationModeProvider);
			}
			else if (docType == CSDDocTypeList.Codes._10000003 && !(docNumber.IsNumbersOnlyOrEmpty && docNumber.Length == 13))
			{
				targetInfo.AddNotification(Res.GetString("A3DE5B40-E068-4C5E-BD7C-E43723B636E7", "The attachment number should be 13 digits."), ValidationModeProvider);
			}
		}

		#endregion

		#region EDoc

		public void ValidateEDoc()
		{
			ValidateCalculatedProperty(Parent.EDocInfo);
		}

		protected void CheckEDoc()
		{
			if (!Parent.EDocReadonly)
			{
				MandatoryValidation.CheckEntered(Parent.EDocInfo);
				TypeValidation.CheckValidGuid(Parent.EDocInfo);
			}

			if (IsLargerThan4M(Parent.EDoc))
			{
				Parent.EDocInfo.AddNotification(Res.GetString("D3D8ACA1-F3D1-4CB8-8E45-6E2569518532", "The selected eDoc is larger than 4M."), ValidationModeProvider);
			}
		}

		bool IsLargerThan4M(ZGuid eDocPK)
		{
			var eDoc = Parent.Lookups.GetEDocCollections().Cast<IStorageDocsBaseCollection>().SelectMany(x => x.Cast<IeDoc>()).FirstOrDefault(d => d.UniqueKey == eDocPK);
			if (eDoc == null)
			{
				return false;
			}
			return eDoc.ImageData.Length > (4 * 1024 * 1024);
		}

		#endregion
	}
}
