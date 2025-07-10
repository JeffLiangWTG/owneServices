using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitSkinsAndHidesLineMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateProductSourceState()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_AUState = "NSW";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Source State", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'LOC+ZZZ+NSW'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCustomsWeight()
		{
			quarantineLine.QL_AqisCustomsWeightUQ = "KGM";
			quarantineLine.QL_AqisCustomsWeight = 69.68m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Customs weight", "LIN+1'MEA+AAF+SQ+KGM:69.68'PIA+5+X       :CC'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateSaltingDate()
		{
			quarantineLine.QL_SaltingDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Salting Date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'DTM+9:20061212:102'", MessageTextForTesting, '\'');
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();

			invoiceLine = helper.Line1;
			quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_SendHCDesc = true;

			sancrtMessage = new SANCRTMessage();
			builder = new RequestForPermitSkinsAndHidesLineMessageBuilder(sancrtMessage, EXDOCMessageTypeCodes.Codes.LDG);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		ZString MessageTextForTesting => sancrtMessage.ToString(new Edifact.UNOACharacterSet());

		SANCRTMessage sancrtMessage;
		JobComInvoiceLine invoiceLine;
		RequestForPermitSkinsAndHidesLineMessageBuilder builder;
		QuarantineExDocLine quarantineLine;
	}
}
