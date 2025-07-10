using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class AdditionalDocumentReference
	{
		internal DocumentReferenceType BuildAdditionalDocumentReference(string xsltFilename, byte[] xsltFileData, ZGuid externalXsltID)
		{
			var vGUID = Guid.NewGuid();
			var additionalDocumentReference = new DocumentReferenceType()
			{
				ID = new IDType() { Value = vGUID.ToString() },
				IssueDate = new IssueDateType() { Value = ZDateTime.Now.ToDateTime() }
			};

			if (externalXsltID.IsEmpty)
			{
				additionalDocumentReference.Attachment = new AttachmentType()
				{
					EmbeddedDocumentBinaryObject = new EmbeddedDocumentBinaryObjectType()
					{
						filename = xsltFilename,
						characterSetCode = "UTF-8",
						encodingCode = "Base64",
						mimeCode = "application/xml",
						Value = xsltFileData
					}
				};
			}
			else
			{
				additionalDocumentReference.DocumentType = new DocumentTypeType() { Value = (NoResString)"xslt" }; // Constant string used by intermediary for document type.
				additionalDocumentReference.DocumentTypeCode = new DocumentTypeCodeType() { Value = externalXsltID.ToString() };
			}

			return additionalDocumentReference;
		}

		internal DocumentReferenceType BuildAdditionalDocumentReferenceForTransactionNumber(string transactionNumber)
		{
			var additionalDocumentReference = new DocumentReferenceType()
			{
				ID = new IDType() { Value = transactionNumber },
				DocumentType = new DocumentTypeType() { Value = "TRANSACTION_NUMBER" },
				IssueDate = new IssueDateType() { Value = ZDateTime.Now.ToDateTime() }
			};
			return additionalDocumentReference;
		}
	}
}
