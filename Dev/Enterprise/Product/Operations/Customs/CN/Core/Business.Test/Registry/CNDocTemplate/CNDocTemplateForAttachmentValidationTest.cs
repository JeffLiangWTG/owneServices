using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNDocTemplateForAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			CombineAssertions(() =>
			{
				docTemplate.OrganizationPK = ZGuid.Invalid;
				docTemplate.DataContext = ZString.Empty;
				docTemplate.Validation.ValidateAll();
				AssertHasErrorContaining("OrganizationPK", docTemplate.OrganizationPKInfo, ListValidation.InvalidCodeError);
				AssertHasErrorContaining("DataContext", docTemplate.DataContextInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("DocumentTemplate", docTemplate.DocumentTemplateInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("DocumentType", docTemplate.DocumentTypeInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("DocumentDescription", docTemplate.DocumentDescriptionInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("AttachmentType", docTemplate.AttachmentTypeInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestValidateOrganizationPK()
		{
			CombineAssertions(() =>
			{
				docTemplate.OrganizationPK = ZGuid.Invalid;
				AssertHasErrorContaining("Invalid", docTemplate.OrganizationPKInfo, ListValidation.InvalidCodeError);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				docTemplate.OrganizationPK = org.PK;
				AssertNoErrorContaining("Valid", docTemplate.OrganizationPKInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestValidateDataContext_CheckEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(docTemplate.DataContextInfo);
		}

		public void TestValidateDataContext_ErrorIfNotWesternEuropean()
		{
			const string message = "Data Context only accepts Western European languages characters.";
			CombineAssertions(() =>
			{
				docTemplate.DataContext = "非英文";
				AssertHasError("Not western european", docTemplate.DataContextInfo, message);
				docTemplate.DataContext = "Context";
				AssertNoError("Western european", docTemplate.DataContextInfo, message);
			});
		}

		public void TestValidateDocumentTemplate_CheckEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(docTemplate.DocumentTemplateInfo);
		}

		public void TestValidateDocumentTemplate_ErrorIfInvalidCode()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "TT";
			template.SO_DataContext = "Context";
			Factory.Save();

			docTemplate.DataContext = "Context";
			ValidationTestHelper.AssertErrorIfInvalidCode(docTemplate.DocumentTemplateInfo, "~", "TT");
		}

		public void TestValidateDocumentType_CheckEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(docTemplate.DocumentTypeInfo);
		}

		public void TestValidateDocumentType_ErrorIfInvalidCode()
		{
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "TT";
			docType.RT_ReferenceType = "ALL";
			Factory.Save();

			ValidationTestHelper.AssertErrorIfInvalidCode(docTemplate.DocumentTypeInfo, "~", "TT");
		}

		public void TestValidateDocumentDescription_CheckEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(docTemplate.DocumentDescriptionInfo);
		}

		public void TestValidateDocumentDescription_ErrorIfNotWesternEuropean()
		{
			const string message = "Document Description only accepts Western European languages characters.";
			CombineAssertions(() =>
			{
				docTemplate.DocumentDescription = "非英文";
				AssertHasError("Not western european", docTemplate.DocumentDescriptionInfo, message);
				docTemplate.DocumentDescription = "Dec";
				AssertNoError("Western european", docTemplate.DocumentDescriptionInfo, message);
			});
		}

		public void TestValidateAttachmentType_CheckEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(docTemplate.AttachmentTypeInfo);
		}

		public void TestValidateAttachmentType_ErrorIfInvalidCode()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(docTemplate.AttachmentTypeInfo, "~", "00000001");
		}

		public void TestValidateAttachmentType_CheckDuplication()
		{
			CombineAssertions(() =>
			{
				var org = Factory.New<OrgHeader>();
				docTemplate.OrganizationPK = org.PK;
				docTemplate.AttachmentType = "00000001";
				var docTemplate2 = collection.AddNew();
				docTemplate2.OrganizationPK = org.PK;
				docTemplate2.AttachmentType = "00000001";
				AssertHasErrorContaining("Duplicate", docTemplate2.AttachmentTypeInfo, "The combination of Organization and Attachment Type must not be duplicated.");
				docTemplate2.AttachmentType = "00000002";
				AssertNoErrorContaining("Not duplicate", docTemplate2.AttachmentTypeInfo, "The combination of Organization and Attachment Type must not be duplicated.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			collection = new CNDocTemplateForAttachmentCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			docTemplate = collection.AddNew();
		}
		CNDocTemplateForAttachment docTemplate;
		CNDocTemplateForAttachmentCollection collection;
	}
}
