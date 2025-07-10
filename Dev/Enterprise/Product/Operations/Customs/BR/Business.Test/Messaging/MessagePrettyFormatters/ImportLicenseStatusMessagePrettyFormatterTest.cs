using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseStatusMessagePrettyFormatterTest : TestCaseWithFactory
	{
		[TestDate(2023, 1, 1)]
		public void TestGetFormattedMessageText()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "JOBTEST";
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var li = XmlObjectSerializer.Deserialize<licompletatype>(BRCImportLicenseStatusMessageProcessorTest.GetMessageBody("17", ZDateTime.Today.ToString(Constants.DataFormat), ZDateTime.Today.ToString(Constants.DataFormat), ZDateTime.Today.ToString(Constants.DataFormat), ZDateTime.Today.ToString("HH:mm:ss")));

			var actualHtml = new ImportLicenseStatusMessagePrettyFormatter(entry, li).GetFormattedMessageText();
			var expectedHtml = BRCImportLicenseStatusMessageProcessorTest.GetExpectedHtml(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference));

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML", expectedHtml, actualHtml);
		}
	}
}
