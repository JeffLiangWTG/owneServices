using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CNDocTemplateForAttachmentValidation
	{
		public CNDocTemplateForAttachmentValidation(CNDocTemplateForAttachment parent)
		{
			this.parent = parent;
		}

		readonly CNDocTemplateForAttachment parent;

		public void ValidateAll()
		{
			ValidateOrganizationPK();
			ValidateDataContext();
			ValidateDocumentTemplate();
			ValidateDocumentType();
			ValidateDocumentDescription();
			ValidateAttachmentType();
		}

		public void ValidateOrganizationPK()
		{
			parent.OrganizationPKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(parent.OrganizationPKInfo);
			if (!parent.AttachmentType.IsEmpty)
			{
				ValidateAttachmentType();
			}
		}

		public void ValidateDataContext()
		{
			parent.DataContextInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DataContextInfo);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(parent.DataContextInfo);
		}

		public void ValidateDocumentTemplate()
		{
			parent.DocumentTemplateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DocumentTemplateInfo);
			ListValidation.ErrorIfInvalidCode(parent.DocumentTemplateInfo);
		}

		public void ValidateDocumentType()
		{
			parent.DocumentTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DocumentTypeInfo);
			ListValidation.ErrorIfInvalidCode(parent.DocumentTypeInfo);
		}

		public void ValidateDocumentDescription()
		{
			parent.DocumentDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DocumentDescriptionInfo);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(parent.DocumentDescriptionInfo);
		}

		public void ValidateAttachmentType()
		{
			parent.AttachmentTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.AttachmentTypeInfo);
			ListValidation.ErrorIfInvalidCode(parent.AttachmentTypeInfo);
			CheckDuplication();
		}

		void CheckDuplication()
		{
			var attachmentType = parent.AttachmentType;
			var organizationPK = parent.OrganizationPK;
			if (parent.Collection.Count > 1 && parent.Collection.Cast<CNDocTemplateForAttachment>().Count(x => x.AttachmentType == attachmentType && x.OrganizationPK == organizationPK) > 1)
			{
				parent.AttachmentTypeInfo.AddError(Res.GetString(
									"6E0F0807-6D48-4523-B990-A67A4D12766C",
									"The combination of Organization and Attachment Type must not be duplicated."));
			}
		}
	}
}
