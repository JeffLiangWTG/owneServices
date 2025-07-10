using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers.AES.Common
{
	public class AESCommonSupportingDocumentExtraFieldsWrapper : IAESCommonSupportingDocumentExtraFields
	{
		public AESCommonSupportingDocumentExtraFieldsWrapper(CusSupportingInfo doc)
		{
			document = doc;
			var documentDate = document.CSI_DateOfExpiry.IsEmpty ? document.CSI_DateOfIssue : document.CSI_DateOfExpiry;

			DocumentDate = documentDate.IsEmpty ? default(DateTime) : documentDate.ToDateTime();
			DocumentDateSpecified = !documentDate.IsEmpty;
		}
		protected readonly CusSupportingInfo document;

		public ZString IssuingAuthorityName => document.CSI_AdditionalDescription;

		public DateTime DocumentDate { get; }

		public ZBool DocumentDateSpecified { get; }
	}
}
