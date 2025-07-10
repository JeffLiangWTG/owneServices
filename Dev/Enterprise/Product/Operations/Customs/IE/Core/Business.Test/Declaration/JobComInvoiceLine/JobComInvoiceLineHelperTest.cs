using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing.Declaration
{
	sealed class JobComInvoiceLineHelperTest : TestCaseWithFactory
	{
		public void TestHasProcedureCodeConcessionF15_JobDeclaration_Procedure() => AssertHasProcedureCodeConcessionF15_Procedure(jobDeclaration.InvoiceLines.Cast<JobComInvoiceLine>());

		public void TestHasProcedureCodeConcessionF15_CusEntryInstruction_Procedure() => AssertHasProcedureCodeConcessionF15_Procedure(cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>());

		void AssertHasProcedureCodeConcessionF15_Procedure(IEnumerable<JobComInvoiceLine> invoiceLines) => CombineAssertions(() =>
		{
			AssertEquals("No Procedures", false, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			invoiceLine1fromInvoice1.JI_Procedure = ProcedureCodeConcessionF15;
			AssertEquals("F15 for Procedure on invoice1 invoiceLine1", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			invoiceLine1fromInvoice1.JI_Procedure = ProcedureCodeConcessionF99;
			invoiceLine1fromInvoice2.JI_Procedure = ProcedureCodeConcessionF15;
			AssertEquals("F15 for Procedure on invoice1 invoiceLine2", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			invoiceLine1fromInvoice2.JI_Procedure = ProcedureCodeConcessionF99;
			invoiceLine2fromInvoice1.JI_Procedure = ProcedureCodeConcessionF15;
			AssertEquals("F15 for Procedure on invoice2 invoiceLine1", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			invoiceLine2fromInvoice1.JI_Procedure = ProcedureCodeConcessionF99;
			invoiceLine2fromInvoice2.JI_Procedure = ProcedureCodeConcessionF15;
			AssertEquals("F15 for Procedure on invoice2 invoiceLine2", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			invoiceLine2fromInvoice2.JI_Procedure = ProcedureCodeConcessionF99;
			AssertEquals("F15 for Procedure is not present", false, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
		});

		public void TestHasProcedureCodeConcessionF15_JobDeclaration_AdditionalProcedure() => AssertHasProcedureCodeConcessionF15_AdditionalProcedure(jobDeclaration.InvoiceLines.Cast<JobComInvoiceLine>());

		public void TestHasProcedureCodeConcessionF15_CusEntryInstruction_AdditionalProcedure() => AssertHasProcedureCodeConcessionF15_AdditionalProcedure(cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>());

		void AssertHasProcedureCodeConcessionF15_AdditionalProcedure(IEnumerable<JobComInvoiceLine> invoiceLines) => CombineAssertions(() =>
		{
			AssertEquals("No Additional Procedures", false, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			var additionalProcedure = invoiceLine1fromInvoice1.AdditionalProcedureCodes.AddNew(ProcedureCodeConcessionF15);
			AssertEquals("F15 for Additional Procedure on invoice1 invoiceLine1", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			additionalProcedure.CY_Code = ProcedureCodeConcessionF99;
			additionalProcedure = invoiceLine1fromInvoice2.AdditionalProcedureCodes.AddNew(ProcedureCodeConcessionF15);
			AssertEquals("F15 for Additional Procedure on invoice1 invoiceLine2", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			additionalProcedure.CY_Code = ProcedureCodeConcessionF99;
			additionalProcedure = invoiceLine2fromInvoice1.AdditionalProcedureCodes.AddNew(ProcedureCodeConcessionF15);
			AssertEquals("F15 for Additional Procedure on invoice2 invoiceLine1", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			additionalProcedure.CY_Code = ProcedureCodeConcessionF99;
			additionalProcedure = invoiceLine2fromInvoice2.AdditionalProcedureCodes.AddNew(ProcedureCodeConcessionF15);
			AssertEquals("F15 for Additional Procedure on invoice2 invoiceLine2", true, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
			additionalProcedure.CY_Code = ProcedureCodeConcessionF99;
			AssertEquals("F15 for Additional Procedure is not present", false, JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(invoiceLines));
		});

		public void TestHasAddtionalInfoINF00200()
		{
			CombineAssertions(() =>
			{
				var addInfo1 = invoiceLine1fromInvoice1.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				addInfo1.CSI_Code = "00000";
				var addInfo2 = invoiceLine1fromInvoice1.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo2.CSI_Code = "00200";
				AssertEquals("Not contain CSI_Code is 00200 when CSI_SubType is INF", false, invoiceLine1fromInvoice1.HasAddtionalInfoINF00200());

				addInfo1.CSI_Code = "00200";
				AssertEquals("Contain CSI_Code is 00200 when CSI_SubType is INF", true, invoiceLine1fromInvoice1.HasAddtionalInfoINF00200());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var invoice1 = jobDeclaration.Invoices.AddNew();
			invoiceLine1fromInvoice1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1fromInvoice1.JI_CEI = cusEntryInstruction.PK;
			invoiceLine2fromInvoice1 = invoice1.InvoiceLines.AddNew();
			invoiceLine2fromInvoice1.JI_CEI = cusEntryInstruction.PK;
			var invoice2 = jobDeclaration.Invoices.AddNew();
			invoiceLine1fromInvoice2 = invoice2.InvoiceLines.AddNew();
			invoiceLine1fromInvoice2.JI_CEI = cusEntryInstruction.PK;
			invoiceLine2fromInvoice2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2fromInvoice2.JI_CEI = cusEntryInstruction.PK;
		}

		JobDeclaration jobDeclaration;
		CusEntryInstruction cusEntryInstruction;
		JobComInvoiceLine invoiceLine1fromInvoice1;
		JobComInvoiceLine invoiceLine2fromInvoice1;
		JobComInvoiceLine invoiceLine1fromInvoice2;
		JobComInvoiceLine invoiceLine2fromInvoice2;

		const string ProcedureCodeConcessionF15 = "4021F15";
		const string ProcedureCodeConcessionF99 = "4021F99";
	}
}
