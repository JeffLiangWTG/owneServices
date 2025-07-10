using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitInedibleMeatHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateAdditionalInformation()
		{
			var additionalInfotNote = declaration.Notes.AddNew();
			additionalInfotNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description;
			additionalInfotNote.ST_NoteText = "LAWSY, 123456 HELL WHAT'S GOIN ON. JUST WANT TO PUT MY TO BOB'S IN 1234567 CUT OFF AGAIN";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Additional Information Test", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+ACB+++LAWSY, 123456 HELL WHAT S GOIN ON. JUST WANT TO PUT MY TO BOB S IN 123:4567 CUT OFF AGAIN'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTranshipmentStorageTemperatureForAbsolute()
		{
			quarantineHeader.QH_AbsoluteTemperature = 12;
			quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Absolute Temperature Test", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+CEL:12.00'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTranshipmentStorageTemperatureForMinMax()
		{
			quarantineHeader.QH_AbsoluteTemperature = ZDecimal.Zero;
			quarantineHeader.QH_MinimumTemperature = 12.3m;
			quarantineHeader.QH_MaximumTemperature = 34.4m;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Minimum and Maximum Temperature Test", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MEA+TE+ADE+::12.30:34.40'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
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

			builder = new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		JobDeclaration declaration;
		RequestForPermitInedibleMeatHeaderMessageBuilder builder;
		QuarantineExDocHeader quarantineHeader;
	}
}
