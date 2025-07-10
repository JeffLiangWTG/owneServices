using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportSupportingDocumentValidation))]
	class ExportSupportingDocumentValidationTest : SupportingDocumentValidationAbstractTest
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			CombineAssertions(() =>
			{
				supportingDocument.CSI_ReferenceNumber = "TestRef";
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is mandatory in IE", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				supportingDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is mandatory in IE", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_ReferenceNumber_TransitionPeriod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decDoc = declaration.SupportingDocuments.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invDoc = invoice.SupportingDocuments.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			var lineDoc = invLine.SupportingDocuments.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var instructionDoc = instruction.SupportingDocuments.AddNew();

			string message = "Reference Number of Supporting Document can have up to 35 alpha numeric characters.";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 36);
				CombineAssertions("TransitionPeriodAES30, Export, 36 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertHasMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertHasMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertHasMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});

				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 35);
				CombineAssertions("35 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 36);
				CombineAssertions("TransitionPeriodAES30, Import, 36 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 37);
				CombineAssertions("Not TransitionPeriodAES30, Export, 37 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});
			}
		}

		protected override string MessageType => MessageTypeList.Codes.Export;
		protected override SupportingDocument SetupSupportingDocument() => declaration.SupportingDocuments.AddNew();
	}
}
