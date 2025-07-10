using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitHorticultureLineMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateCustomsWeight()
		{
			quarantineLine.QL_AqisCustomsWeightUQ = "KGM";
			quarantineLine.QL_AqisCustomsWeight = 69.68m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Customs weight", "LIN+1'MEA+AAF+SQ+KGM:69.68'PIA+5+X       :CC'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateProductSourceState()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_AUState = "NSW";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Source State", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'LOC+ZZZ+NSW'", MessageTextForTesting, '\'');
		}

		public void TestGenerateAdditionalDeclarationInformation()
		{
			quarantineLine.QL_AddtionalDeclarationComments = "HERE ARE SOME ADDITIONAL COMMENTS ON THE CHECKIN FIASCO. WHAT A CROCK OF ZXCV PEOPLE WHO DO IT SHOULD BE SHOT";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Container Inspection Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+AAZ+++HERE ARE SOME ADDITIONAL COMMENTS ON THE CHECKIN FIASCO. WHAT A CROCK :OF ZXCV PEOPLE WHO DO IT SHOULD BE SHOT'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCodedStatementAll()
		{
			quarantineLine.QL_StatementNumber1 = 123;
			quarantineLine.QL_StatementNumber2 = 456;
			quarantineLine.QL_StatementNumber3 = 789;
			quarantineLine.QL_StatementNumber4 = 389;
			quarantineLine.QL_StatementNumber5 = 873;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("CodedStatement All", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+ZZZ+++123:456:789:389:873'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCodedStatementFour()
		{
			quarantineLine.QL_StatementNumber1 = 123;
			quarantineLine.QL_StatementNumber2 = 456;
			quarantineLine.QL_StatementNumber3 = 789;
			quarantineLine.QL_StatementNumber4 = 389;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("CodedStatement first 4", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+ZZZ+++123:456:789:389'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCodedStatementThree()
		{
			quarantineLine.QL_StatementNumber1 = 123;
			quarantineLine.QL_StatementNumber2 = 456;
			quarantineLine.QL_StatementNumber3 = 789;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("CodedStatement first 3", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+ZZZ+++123:456:789'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCodedStatementTwo()
		{
			quarantineLine.QL_StatementNumber1 = 123;
			quarantineLine.QL_StatementNumber2 = 456;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("CodedStatement first 2", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+ZZZ+++123:456'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCodedStatementOne()
		{
			quarantineLine.QL_StatementNumber1 = 123;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("CodedStatement first 1", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+ZZZ+++123'", MessageTextForTesting, '\'');
		}

		public void TestGenerateFreeTextStatement()
		{
			quarantineLine.QL_StatementText = "HAD ENOUGH OF WRITING TEXT FOR STATEMENTS TESTS WORK";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Free Text Statement", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+AAY+++HAD ENOUGH OF WRITING TEXT FOR STATEMENTS TESTS WORK'", MessageTextForTesting, '\'');
		}

		public void TestGenerateGrowerNumber()
		{
			quarantineLine.QL_GrowerNumber = "2131";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Grower Number", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'FTX+MKS+++2131'", MessageTextForTesting, '\'');
			quarantineLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			RequestForPermitHeaderMessageBuilderTest.AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineLine.QuarantineExDocHeader, "GrowerNumber", "FTX+MKS+++2131'", () => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceLine.InvoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			sancrtMessage = new SANCRTMessage();
			builder = new RequestForPermitHorticultureLineMessageBuilder(sancrtMessage, EXDOCMessageTypeCodes.Codes.LDG);
			invoiceLine = helper.Line1;
			quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_SendHCDesc = true;
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		ZString MessageTextForTesting => sancrtMessage.ToString(new Edifact.UNOACharacterSet());

		JobComInvoiceLine invoiceLine;
		SANCRTMessage sancrtMessage;
		RequestForPermitHorticultureLineMessageBuilder builder;
		QuarantineExDocLine quarantineLine;
	}
}
