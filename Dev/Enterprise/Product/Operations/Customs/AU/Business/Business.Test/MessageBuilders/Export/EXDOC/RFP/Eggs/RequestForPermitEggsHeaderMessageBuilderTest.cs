using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitEggsHeaderMessageBuilderTest : TestCaseWithFactory
	{
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

			builder = new RequestForPermitEggsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		JobDeclaration declaration;
		RequestForPermitEggsHeaderMessageBuilder builder;
		QuarantineExDocHeader quarantineHeader;

		const string TranshipmentTempAbsoluteMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+E::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+CEL:12.00'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+11+<<MSGNO PLACEHOLDER>>'";
		const string TranshipmentTempMaxMinMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+E::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+::12.30:34.40'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+::AQ:TAC'UNT+11+<<MSGNO PLACEHOLDER>>'";
	}
}
