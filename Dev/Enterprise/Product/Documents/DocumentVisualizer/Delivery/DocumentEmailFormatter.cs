using System;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Delivery
{
	sealed class DocumentEmailFormatter
	{
		public string GetEmailSubjectLine(string documentName)
		{
			EmailFormatter.UpdateDocumentName(documentName);
			return EmailFormatter.GetEmailSubjectLine(RegistryEmailFormat.EmailSubjectFields);
		}

		public string GetEmailSignature(string documentName)
		{
			EmailFormatter.UpdateDocumentName(documentName);
			return EmailFormatter.GetEmailSignature(RegistryEmailFormat.EmailSignatureFields);
		}

		EmailFormatter EmailFormatter => emailFormatter ?? (emailFormatter = new EmailFormatter(GlbStaff.CurrentUser));
		EmailFormatter emailFormatter;

		EmailFormat RegistryEmailFormat => registryEmailFormat ?? (registryEmailFormat = DocumentsDataRegistry.Instance.EmailFormat.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		EmailFormat registryEmailFormat;
	}
}