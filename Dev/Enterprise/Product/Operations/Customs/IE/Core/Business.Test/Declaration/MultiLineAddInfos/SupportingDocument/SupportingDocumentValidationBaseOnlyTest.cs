using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CommonSupportingDocumentValidation))]
	class SupportingDocumentValidationBaseOnlyTest : SupportingDocumentValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override SupportingDocument SetupSupportingDocument() => declaration.SupportingDocuments.AddNew();
	}
}
