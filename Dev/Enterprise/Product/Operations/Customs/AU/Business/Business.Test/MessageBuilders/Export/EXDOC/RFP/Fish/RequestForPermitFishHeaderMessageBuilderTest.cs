using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitFishHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateOriginCatchingZone()
		{
			quarantineHeader.QH_OriginCatchZone = "TESTING THE ORIGIN CATCH ZONE";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Origin Catch Zone Segment", OriginCatchMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTranshipmentStorageTemperatureForAbsolute()
		{
			quarantineHeader.QH_AbsoluteTemperature = 12;
			quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Absolute Temperature Test", TranshipmentTempAbsoluteMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTranshipmentStorageTemperatureForMinMax()
		{
			quarantineHeader.QH_AbsoluteTemperature = ZDecimal.Zero;
			quarantineHeader.QH_MinimumTemperature = 12.3m;
			quarantineHeader.QH_MaximumTemperature = 34.4m;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Minimum and Maximum Temperature Test", TranshipmentTempMaxMinMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateInspectionRequestedDate()
		{
			quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2006, 12, 11);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Inspection Requested Date", InspectionRequestedDateMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateNoteExclusion()
		{
			var letterOfCreditNote = declaration.Notes.AddNew();
			letterOfCreditNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";

			var notifyTextNote = declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";

			builder.GenerateRFPMessage();
			var message = builder.MessageTextForTesting;
			AssertContains("RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>", message);
			AssertNotContains("ATTN MR BANKER, BANK OF NEW ZEALAND", message);
			AssertNotContains("NEDDY SEAGOON", message);
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

			builder = new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		JobDeclaration declaration;
		RequestForPermitFishHeaderMessageBuilder builder;
		QuarantineExDocHeader quarantineHeader;

		const string OriginCatchMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+OCZ+++TESTING THE ORIGIN CATCH ZONE'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+11+<<MSGNO PLACEHOLDER>>'";
		const string TranshipmentTempAbsoluteMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+CEL:12.00'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+11+<<MSGNO PLACEHOLDER>>'";
		const string TranshipmentTempMaxMinMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+::12.30:34.40'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+11+<<MSGNO PLACEHOLDER>>'";
		const string InspectionRequestedDateMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+::AQ:TAC'PRC+IN:PP:AQ'DTM+318:20061211:102'UNT+12+<<MSGNO PLACEHOLDER>>'";
	}
}
