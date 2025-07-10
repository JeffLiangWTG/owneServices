using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		public override void TestLinePriceCurr()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceLine = GetNewInvoiceLine();
			AssertEquals("LinePriceCurr", InvoiceLineWrapper.LinePriceCurr.ToString(), "NZD");
		}

		public override void TestOriginCode()
		{
			InvoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertEquals("InvoiceLineWrapperInternal.OriginCode", "AU", InvoiceLineWrapper.OriginCode);
			InvoiceLine.JI_CountryOfOrigin = "ZA";
			AssertEquals("InvoiceLineWrapperInternal.OriginCode", "ZA", InvoiceLineWrapper.OriginCode);
		}

		public override void TestCountryOfOrigin()
		{
			AssertNull("InvoiceLineWrapperInternal.CountryOfOrigin", InvoiceLineWrapper.CountryOfOrigin);
			InvoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertNotNull("InvoiceLineWrapperInternal.CountryOfOrigin", InvoiceLineWrapper.CountryOfOrigin);
			AssertEquals("InvoiceLineWrapperInternal.OriginCode", "AU", InvoiceLineWrapper.CountryOfOrigin.Code);
			InvoiceLine.JI_CountryOfOrigin = "ZA";
			AssertNotNull("InvoiceLineWrapperInternal.CountryOfOrigin", InvoiceLineWrapper.CountryOfOrigin);
			AssertEquals("InvoiceLineWrapperInternal.OriginCode", "ZA", InvoiceLineWrapper.CountryOfOrigin.Code);
		}

		public void TestRequiresPermitCodes()
		{
			CusEntryLine cusline = InvoiceLine.CusEntryLine;
			DocJobComInvoiceLine docLine = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			Assert(!docLine.RequiresPermitCodes);

			InvoiceLine.JI_Tariff = "0401.20.00.00C";
			InvoiceLine.Declaration.DoMerge(new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer());
			docLine = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			InvoiceLine.Declaration.ResumeApportionment();
			Assert(docLine.RequiresPermitCodes);
		}

		public void TestRequiresPermitCodesDoesntBlowUpIfTheresNoEntryLine()
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceLine = jobDeclaration.InvoiceLines.AddNew();
			DocJobComInvoiceLine docLine = DocJobComInvoiceLine.New(invoiceLine, Factory);
			jobDeclaration.ResumeApportionment();
			Assert(!docLine.RequiresPermitCodes);
			invoiceLine.JI_Tariff = "0401.20.00.00C";
			jobDeclaration.ResumeApportionment();
			Assert(docLine.RequiresPermitCodes);
		}

		public override void TestDutyRateDescriptionBase()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_DutyPercent = 0.5m;
			InvoiceLine.JI_CL = entryLine.PK;
			// NZ description is calculated in the NZ duty calculator, it is not within the scope
			// of this test to test this calculation.  Testing for empty string indicates that the 
			// NZ calculator was called, and not the base property.
			AssertEquals("Duty rate description", ZString.Empty, InvoiceLineWrapper.DutyRateDescription);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.NewZealand; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
