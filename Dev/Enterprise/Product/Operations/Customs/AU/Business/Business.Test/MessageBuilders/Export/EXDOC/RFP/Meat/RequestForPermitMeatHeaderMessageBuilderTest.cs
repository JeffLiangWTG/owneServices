using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitMeatHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateQuotaType()
		{
			quarantineHeader.QH_QuotaType = "ABCDE";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Quota Type in Order and Lodge message", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+QUT+++ABCDE'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+N::AQ:ACS'UNT+12+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTranshipmentStorageTemperature()
		{
			quarantineHeader.QH_AbsoluteTemperature = 12;
			quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Absolute Temperature Test", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+CEL:12.00'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+N::AQ:ACS'UNT+12+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateInspectionRequestedDate()
		{
			quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2006, 12, 12);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Inspection Requested Date", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+N::AQ:ACS'PRC+IN:PP:AQ'DTM+318:20061212:102'UNT+13+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateAvAnimalAgeText()
		{
			quarantineHeader.QH_AvAnimalAge = "more than 1 year";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Average Animal Age", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+ACF+++MORE THAN 1 YEAR'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+N::AQ:ACS'UNT+12+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateApprovedCertifier()
		{
			quarantineHeader.QH_ApprovedCertifier = "H1234";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Approved Certifier", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+N::AQ:ACS'PNA+PQ+H1234'UNT+12+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateShipStoresIndicator()
		{
			quarantineHeader.QH_ShipsStores = true;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Ship Stores Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:SST'GIS+N::AQ:QI'GIS+N::AQ:ACS'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
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

			builder = new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		JobDeclaration declaration;
		RequestForPermitMeatHeaderMessageBuilder builder;
		QuarantineExDocHeader quarantineHeader;
	}
}
