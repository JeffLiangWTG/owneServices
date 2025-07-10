using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CA;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2AdjustmentsDocLine))]
	sealed class B2AdjustmentsDocLineTest : AdjustmentsDocLineTest
	{
		public override void TestAdjustmentsDocumentLineMembers()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.Invoices.AddNew();
			subHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			line.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 30 CHAR. A LONG GOODS DESCRIPTION WHICH EXCEEDS 30 CHAR. ";
			line.CA_AuthorityNumber = "AUTHO";
			line.JI_Tariff = "2402.10.00 10";
			line.CA_99TariffCode = "9960";
			line.JI_CustomsQuantity = 5m;
			line.JI_CustomsUnitQty = "MIL";
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods;
			line.CA_CVforCurrConv = 1000m;

			var sima = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			sima.C1_ExemptCode = SIMACodes.Codes.C31;

			var excise = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 50m;
			excise.C1_Rate = 5.0m;
			excise.C1_RateType = RateTypes.Codes.Specific;

			var duty = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 300m;
			duty.C1_Rate = 6.5m;
			duty.C1_UnitOfMeasure = "AG";
			duty.C1_RateType = RateTypes.Codes.AdValorem;

			duty = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 100m;
			duty.C1_Rate = 5.5m;
			duty.C1_RateType = RateTypes.Codes.Specific;

			var gst = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 60m;
			gst.C1_Rate = 5m;
			gst.C1_RateType = RateTypes.Codes.AdValorem;
			b2.ResumeApportionment();

			var tempLine = new B2AdjustmentsDocLine();
			var docLine = tempLine.GetLinesOrderedByDuty(line).ElementAt(0);
			AssertEquals("1", docLine.OriginalLineNo);
			AssertEquals("A LONG GOODS DESCRIPTION WHICH\r\n EXCEEDS 30 CHAR. A LONG GOODS", docLine.Description);
			AssertEquals("2402.10.00 10", docLine.ClassificationNumber);
			AssertEquals("9960", docLine.TariffCode);
			AssertEquals("5", docLine.Quantity);
			AssertEquals("MIL", docLine.UM);
			AssertEquals(ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods, docLine.VFDCode);
			AssertEquals(SIMACodes.Codes.C31, docLine.SIMACode);
			AssertEquals("5.50", docLine.CustomsDutyRate);
			AssertEquals(1000m, docLine.ValueForCurrencyConversion);
			AssertEquals(1000m, docLine.ValueForDuty);
			AssertEquals(100m, docLine.CustomsDuties);
			AssertEquals(500m, docLine.SIMAAssessment);
			AssertEquals(50m, docLine.ExciseTax);
			AssertEquals(1950m, docLine.ValueForTax);
			AssertEquals(60m, docLine.GST);

			docLine = tempLine.GetLinesOrderedByDuty(line).ElementAt(1);
			AssertEquals("6.5", docLine.CustomsDutyRate);
			AssertEquals(300m, docLine.CustomsDuties);
			AssertEquals("AG", docLine.UM);

			gst.C1_ExemptCode = GSTStatusCodes.Codes.C57;
			b2.ResumeApportionment();

			docLine = tempLine.GetLinesOrderedByDuty(line).ElementAt(0);
			AssertEquals(GSTStatusCodes.Codes.C57, docLine.GSTRate);

			b2.CA_B2Type = B2TypeList.Codes.Blanket;
			b2.ResumeApportionment();

			docLine = tempLine.GetLinesOrderedByDuty(line).ElementAt(0);
			Assert(docLine.OriginalLineNo.IsEmpty);
			Assert(docLine.ValueForCurrencyConversion.IsEmpty);
			Assert(docLine.ValueForDuty.IsEmpty);
			Assert(docLine.CustomsDuties.IsEmpty);
			Assert(docLine.ExciseTax.IsEmpty);
		}

		public override void TestPopulateCalculatedValueForTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_IsCasualImport = true;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_ValueForTax = 100.26m;
			invoiceLine.JI_CustomsQuantity = 20;

			var docLine = new B2AdjustmentsDocLine().GetLinesOrderedByDuty(invoiceLine).ElementAt(0);
			AssertEquals(100.26m, docLine.ValueForTax);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = b2.Invoices.AddNew();
			var line = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			return new B2AdjustmentsDocLine().GetLinesOrderedByDuty(line).ElementAt(0);
		}
	}
}
