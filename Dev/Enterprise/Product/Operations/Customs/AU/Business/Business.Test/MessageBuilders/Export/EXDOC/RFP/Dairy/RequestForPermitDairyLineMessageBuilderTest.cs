using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitDairyLineMessageBuilderTest : TestCaseWithFactory
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

		public void TestGeneratePercentageOfMilkProtein()
		{
			quarantineLine.QL_PercentOfMilkProtein = 45.4m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Milk Protein Percentage Segment", "LIN+1'MEA+MP++P1:45.40'PIA+5+X       :CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGeneratePercentageOfMilkFat()
		{
			quarantineLine.QL_PercentOfMilkFat = 45.4m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Milk Fat Percentage Segment", "LIN+1'MEA+MF++P1:45.40'PIA+5+X       :CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		public void TestGenerateTotalWeightOfMilkProteinInMixtures()
		{
			quarantineLine.QL_TotalWeightOfMilkProteinInMixtures = 4500;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Milk Fat Percentage Segment", "LIN+1'MEA+MP+AAL+KGM:4500.00'PIA+5+X       :CC'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateTotalWeightOfMilkFatInMixtures()
		{
			quarantineLine.QL_TotalWeightOfMilkFatInMixtures = 1000;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Total weight of milk fat in mixtures", "LIN+1'MEA+MF+AAL+KGM:1000.00'PIA+5+X       :CC'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateAMLCPerformanceExporterNumber()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "AMLCTEST";
			var aMLCNumber = supplier.CustomsCodes.AddNew();
			aMLCNumber.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber;
			aMLCNumber.OK_RN_NKCodeCountry = "AU";
			aMLCNumber.OK_CustomsRegNo = "12345";
			Factory.Save();
			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("AMLC Export Segment", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PNA+EX+12345'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCustomsWeight()
		{
			quarantineLine.QL_AqisCustomsWeightUQ = "KGM";
			quarantineLine.QL_AqisCustomsWeight = 69.68m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Customs weight", "LIN+1'MEA+AAF+SQ+KGM:69.68'PIA+5+X       :CC'ATT+10++N:FCI:AQ", MessageTextForTesting, '\'');
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			sancrtMessage = new SANCRTMessage();
			builder = new RequestForPermitDairyLineMessageBuilder(sancrtMessage, EXDOCMessageTypeCodes.Codes.LDG);
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
		RequestForPermitDairyLineMessageBuilder builder;
		QuarantineExDocLine quarantineLine;
	}
}
