using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitInedibleMeatLineMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateProductSourceState()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_AUState = "NSW";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Source State", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'LOC+ZZZ+NSW'", MessageTextForTesting, '\'');
		}

		public void TestGenerateBatchCode()
		{
			quarantineLine.QL_BatchCode = "BATCHCODE";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Batch Code", "LIN+1'PIA+5+X       :CC'GIN+BX+BATCHCODE'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			sancrtMessage = new SANCRTMessage();
			builder = new RequestForPermitInedibleMeatLineMessageBuilder(sancrtMessage, EXDOCMessageTypeCodes.Codes.LDG);
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
		RequestForPermitInedibleMeatLineMessageBuilder builder;
		QuarantineExDocLine quarantineLine;
	}
}
