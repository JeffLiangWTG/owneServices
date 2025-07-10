using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2AdjustmentsDocPage))]
	sealed class B2AdjustmentsDocPageTest : AdjustmentsDocPageTest
	{
		public override void TestSubHeaderWithNoLines()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsClaimedForInvoices.AddNew();

			var docPage = new B2AdjustmentsDocPage().GetPages(subHeader);
			AssertEquals(1, docPage.Count());
		}

		public override void TestGetPages()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var tranastionNumber = b2.TransactionNumber;
			tranastionNumber.AccountSecurityCode = "12345";
			tranastionNumber.SequentialNumber = "87654321";
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			var duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;

			var docPage = new B2AdjustmentsDocPage().GetPages(subHeader.CorrespondingAsClaimedForInvoice);
			AssertEquals(2, docPage.Count());

			AssertEquals("12345876543219", docPage.ElementAt(0).TransactionNumber);
			AssertEquals(ZString.Empty, docPage.ElementAt(1).TransactionNumber);
		}

		public override void TestAdjustmentsDocFirstPage()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "NS1";
			var asAccountedLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine.CA_OriginalLineNo = "1";
			subHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			subHeader.JZ_RW_NKOriginState = "AL";
			subHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			subHeader.CA_USStateOfExport = "IL";
			subHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			subHeader.JZ_ValuationDateOverride = new ZDateTime(2012, 3, 15);
			subHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			subHeader.CA_TimeLimit = 3;
			subHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			subHeader.MarkAsNeedingValidation();

			var docPage = new B2AdjustmentsDocPage().GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(0);
			AssertEquals("NS", docPage.SubHeaderNo);
			AssertEquals("UAL", docPage.CountryOfOrigin);
			AssertEquals("UIL", docPage.PlaceOfExport);
			AssertEquals(TariffTreatmentCodes.Codes.UnitedStates, docPage.TariffTreatment);
			AssertEquals("03", docPage.DirectShipmentMonth);
			AssertEquals("15", docPage.DirectShipmentDay);
			AssertEquals("2012", docPage.DirectShipmentYear);
			AssertEquals(Core.Constants.CurrencyCodes.Canada, docPage.CurrencyCode);
			AssertEquals("3", docPage.TimeLimit);
			AssertEquals(TimeLimitUnitCodes.Codes.Week, docPage.TimeCode);
		}

		public override void TestSortLinesNumerically()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asAccountedLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine1.CA_OriginalLineNo = "1";

			var asAccountedLine2 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine2.CA_OriginalLineNo = "10";

			var asAccountedLine3 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine3.CA_OriginalLineNo = "1/SL";

			var asAccountedLine4 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine4.CA_OriginalLineNo = "1.1";

			var asAccountedLine5 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine5.CA_OriginalLineNo = "2";

			var asAccountedLine6 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine6.CA_OriginalLineNo = "20";

			var tempPage = new B2AdjustmentsDocPage();
			var docPage1 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(0);
			AssertEquals("1", docPage1.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("1/SL", docPage1.AsAccountForDocLine2.OriginalLineNo);
			var docPage2 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(1);
			AssertEquals("1.1", docPage2.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("2", docPage2.AsAccountForDocLine2.OriginalLineNo);
			var docPage3 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(2);
			AssertEquals("10", docPage3.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("20", docPage3.AsAccountForDocLine2.OriginalLineNo);
		}

		public override void TestSplitLineShouldBeAtTheBackOfEachGroup()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asAccountedLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine1.CA_OriginalLineNo = "1";

			var asClaimedSubHeader = subHeader.CorrespondingAsClaimedForInvoice;
			var splitLine = asClaimedSubHeader.AsClaimForFilteredInvoiceLines.AddNew();
			splitLine.CA_OriginalLineNo = "1";
			asClaimedSubHeader.AsClaimForFilteredInvoiceLines.Sort((IComparer)new CAInvoiceLineComparer(true, JobComInvoiceLine.Schema.CA_OriginalLineNo));

			var tempPage = new B2AdjustmentsDocPage();
			var docPage1 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(0);
			AssertEquals("1", docPage1.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("1", docPage1.AsClaimForDocLine1.OriginalLineNo);
			AssertEquals("", docPage1.AsAccountForDocLine2.OriginalLineNo);
			AssertEquals("1/SL", docPage1.AsClaimForDocLine2.OriginalLineNo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = b2.Invoices.AddNew();
			subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			return new B2AdjustmentsDocPage().GetPages(subHeader).ElementAt(0);
		}
	}
}
