using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitHorticultureHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateLotNumber()
		{
			quarantineHeader.QH_LotNumber = "324";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Lot Number", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+MKS+++324'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateAuthorisedStartDate()
		{
			quarantineHeader.QH_AuthorisedStartDate = new ZDateTime(2006, 12, 12);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Authorised Start Date", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PRC+IN:PP:AQ'DTM+194:20061212:102'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateConsigneeAddressAndPhone()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "456 HIGH ST";
			consignee.MainAddress.OA_Address2 = "GOOGLE BUTT";
			consignee.MainAddress.OA_City = "TAIPEI";
			consignee.MainAddress.OA_PostCode = "654321";
			consignee.OH_RL_NKClosestPort = "TWTPE";
			consignee.MainAddress.OA_State = "TWSTATE";
			consignee.MainAddress.OA_Phone = "02 1234 5678";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Consignee Address", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:9++13'LOC+12+TWTPE'LOC+8+TAIPEI'LOC+36+TW'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'ADR++5:456 HIGH ST:GOOGLE BUTT+TAIPEI+654321+TW+:::TWSTATE'UNT+13+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateConsigneeName()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE FULL NAME";
			consignee.CustomsClientID = "CONSIGNEE REFERENCE NUMBER";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Consignee Name", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+CN+++++10:CONSIGNEE FULL NAME'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestAmendmentText()
		{
			var amendmentTextNote = declaration.Notes.AddNew();
			amendmentTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description;
			amendmentTextNote.ST_NoteText = @"<134567890123456789012345678901234><234567890123456789012345678901234>
<334567890123456789012345678901234>
<434567890123456789012345678901234><534567890123456789012345678901234><634567890123456789012345678901234>
<734567890123456789012345678901234><8345678>xxxxxxxxxxx";
			builder = new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Amendment Text", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:9++4'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+CHG+++<134567890123456789012345678901234>:<234567890123456789012345678901234>:<334567890123456789012345678901234>:<434567890123456789012345678901234>:<534567890123456789012345678901234>:<634567890123456789012345678901234>:<734567890123456789012345678901234>:<8345678>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
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
			invoiceHeader = helper.Header1;
			invoiceHeader.JobComInvoiceLines.RemoveAndDeleteAll();
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;

			builder = new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		RequestForPermitHorticultureHeaderMessageBuilder builder;
		QuarantineExDocHeader quarantineHeader;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
