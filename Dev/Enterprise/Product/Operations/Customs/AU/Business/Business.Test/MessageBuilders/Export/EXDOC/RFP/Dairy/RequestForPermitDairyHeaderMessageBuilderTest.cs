using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitDairyHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateAdditionalInformation()
		{
			var additionalInfotNote = declaration.Notes.AddNew();
			additionalInfotNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description;
			additionalInfotNote.ST_NoteText = "LAWSY, 123456 HELL WHAT'S GOIN ON. JUST WANT TO PUT MY TO BOB'S IN 1234567 CUT OFF AGAIN";
			quarantineHeader.QH_ImportedProductFlag = ZString.Empty;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Additional Information Test", AdditionalInformationMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTranshipmentStorageTemperature()
		{
			quarantineHeader.QH_AbsoluteTemperature = 12;
			quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			quarantineHeader.QH_ImportedProductFlag = ZString.Empty;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Absolute Temperature Test", TranshipmentTempMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateDeclarationOfComplianceIndicator()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ImportedProductFlag = ZString.Empty;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Declaration Of Compliance Indicator", DeclarationOfComplianceIndicatorMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateImportedProductFlag()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.Yes;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Declaration Of Compliance Indicator", ImportedProductFlagMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTrueAndCompleteIndicator()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ImportedProductFlag = ZString.Empty;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Declaration Of Compliance Indicator", TrueAndCompleteIndicatorMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateNoteExclusion()
		{
			var letterOfCreditNote = declaration.Notes.AddNew();
			letterOfCreditNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";

			builder.GenerateRFPMessage();
			var message = builder.MessageTextForTesting;
			AssertContains("RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>", message);
			AssertNotContains("ATTN MR BANKER, BANK OF NEW ZEALAND", message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			var invoiceHeader = helper.Header1;
			invoiceHeader.JobComInvoiceLines.RemoveAndDeleteAll();
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			builder = new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
		}
		JobDeclaration declaration;
		RequestForPermitDairyHeaderMessageBuilder builder;
		QuarantineExDocHeader quarantineHeader;

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		const string AdditionalInformationMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+ACB+++LAWSY, 123456 HELL WHAT S GOIN ON. JUST WANT TO PUT MY TO BOB S IN 123:4567 CUT OFF AGAIN'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:QI'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+12+<<MSGNO PLACEHOLDER>>'";
		const string TranshipmentTempMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+CEL:12.00'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:QI'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+12+<<MSGNO PLACEHOLDER>>'";
		const string DeclarationOfComplianceIndicatorMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:QI'GIS+N::AQ:ACS'GIS+Y::AQ:DOC'GIS+::AQ:TAC'UNT+12+<<MSGNO PLACEHOLDER>>'";
		const string ImportedProductFlagMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:QI'GIS+N::AQ:ACS'GIS+::AQ:TAC'GIS+Y::AQ:IPF'UNT+12+<<MSGNO PLACEHOLDER>>'";
		const string TrueAndCompleteIndicatorMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:QI'GIS+N::AQ:ACS'GIS+Y::AQ:TAC'UNT+11+<<MSGNO PLACEHOLDER>>'";
	}
}
