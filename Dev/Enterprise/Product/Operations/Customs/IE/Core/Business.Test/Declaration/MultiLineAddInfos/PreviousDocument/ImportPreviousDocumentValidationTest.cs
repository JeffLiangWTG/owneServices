using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(ImportPreviousDocumentValidation))]
	sealed class ImportPreviousDocumentValidationTest : PreviousDocumentValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;
		protected override PreviousDocument SetupPreviousDocument() => jobDeclaration.PreviousDocuments.AddNew();

		public void TestCSI_ReferenceNumber()
		{
			string message = "Previous Document Reference Number can have up to 35 alpha numeric characters.";

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(jobDeclaration, true))
			{
				previousDocument.CSI_ReferenceNumber = new ZString('A', 36);
				AssertHasMessageError("UCC5, 36 characters", previousDocument.CSI_ReferenceNumberInfo, message);
				previousDocument.CSI_ReferenceNumber = new ZString('A', 35);
				AssertNoMessageError("UCC5, 35 characters", previousDocument.CSI_ReferenceNumberInfo, message);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				previousDocument.CSI_ReferenceNumber = new ZString('A', 36);
				AssertNoMessageError("UCC6, 36 characters", previousDocument.CSI_ReferenceNumberInfo, message);
			}
		}
	}
}
