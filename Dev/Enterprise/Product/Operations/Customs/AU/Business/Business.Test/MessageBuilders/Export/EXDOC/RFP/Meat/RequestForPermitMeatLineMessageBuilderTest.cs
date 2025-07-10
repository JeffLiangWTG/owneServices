using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitMeatLineMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateProductSourceState()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_AUState = "NSW";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Source State", "LIN+1'PIA+5+X       :CC'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'LOC+ZZZ+NSW'", MessageTextForTesting, '\'');
		}

		public void TestGenerateLabelApprovalIndicator()
		{
			quarantineLine.QL_LabelApprovalIndicator = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Label Approval Indicator", "LIN+1'PIA+5+X       :CC'ATT+10++Y:LAI:AQ'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateUngradedProductIndicator()
		{
			quarantineLine.QL_UngradedProductIndicator = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Ungraded Product Indicator", "LIN+1'PIA+5+X       :CC'ATT+10++Y:UPI:AQ'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateHalalProductIndicator()
		{
			quarantineLine.QL_HalalProductIndicator = true;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Halal Product Indicator", "LIN+1'PIA+5+X       :CC'ATT+10++Y:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateLineImperialNetWeight()
		{
			quarantineLine.QL_ImperialNetWeight = 123.4m;
			quarantineLine.QL_ImperialNetWeightUnit = EXDOCImperialWeightUnitCodes.Codes.Pound;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Line Imperial Net Weight", "LIN+1'MEA+AAI+AAL+LBR:123.4'PIA+5+X       :CC'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateBeefVealWeight()
		{
			quarantineLine.QL_BeefVealWeightAmount = 12.3;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Beef Veal Weight", "LIN+1'MEA+AAI+ACG+KGM:12.3'PIA+5+X       :CC'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateChemicalLeanPercentage()
		{
			quarantineLine.QL_ChemicalLeanPercentage = 12;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Checmical Lean Percentage", "LIN+1'MEA+CH++P1:12'PIA+5+X       :CC'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateDominantProductAlones()
		{
			quarantineLine.QL_DominantProduct = "DPCODE";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Dominant product alone", "LIN+1'PIA+5+X       :CC'PIA+5+DPCODE:CG'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateDominantAndAdditionalProducts()
		{
			quarantineLine.QL_DominantProduct = "DPCODE";
			quarantineLine.QL_AdditionalProducts = "APCODES";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Dominant and Additional products", "LIN+1'PIA+5+X       :CC'PIA+5+DPCODE:CG'PIA+5+APCODES:SG'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateRFPLineItemDescription()
		{
			quarantineLine.QL_MeatInspectionDescription = "A DESCRIPTION THAT SCOTT MADE UP (REALLY)";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("RFP LINE ITEM DESCRIPTION", "LIN+1'PIA+5+X       :CC'IMD+++IN:::A DESCRIPTION THAT SCOTT MADE UP (R:EALLY)'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateAMLCQuotaApprovalReference()
		{
			quarantineLine.QL_QuotaApprovalRef = "TEST232";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("AMLC Quota Approval Reference", "LIN+1'PIA+5+X       :CC'RFF+RE:TEST232'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateAMLCPerformanceExporterNumber()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "AMLCTEST";
			var amlcNumber = supplier.CustomsCodes.AddNew();
			amlcNumber.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber;
			amlcNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			amlcNumber.OK_CustomsRegNo = "12345";
			Factory.Save();
			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("AMLC Export Segment", "LIN+1'PIA+5+X       :CC'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'PNA+EX+12345'", MessageTextForTesting, '\'');
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
			builder = new RequestForPermitMeatLineMessageBuilder(sancrtMessage, EXDOCMessageTypeCodes.Codes.LDG);

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
		RequestForPermitMeatLineMessageBuilder builder;
		QuarantineExDocLine quarantineLine;
	}
}
