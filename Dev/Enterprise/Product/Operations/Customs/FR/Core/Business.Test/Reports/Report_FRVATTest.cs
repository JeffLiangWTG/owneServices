using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	[TemplateName("FR VAT Report")]
	class Report_FRVATTest : FRReportFunctionalTestCase
	{
		[TestDate(2022, 05, 11)]
		public new void TestRows()
		{
			base.TestRows();
		}

		protected override void PrepareTestData()
		{
			_ = CreateDeclarationAtMinimumRequirement("0001");

			var (declaration2, invoiceHeader2, invoiceLine2, entryHeader2, entryLine2) = CreateDeclarationAtMinimumRequirement("0002");
			invoiceHeader2.SupportingDocuments.AddNew("N380", "Invoice NO. 1");
			entryLine2.CL_InvoiceAmount = 300m;
			entryLine2.CL_RX_NKInvoiceAmountCurrency = "USD";
			invoiceHeader2.SupportingDocuments.AddNew("N325", "Invoice NO. 2");
			var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "70P";
			entryInstruction2.CEI_SubStyle = "A";
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.JI_Procedure = "7041000";
			invoiceLine2.ZG_CountryOfSupply = "US";
			var invoiceLine2B = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2B.ZG_CountryOfSupply = "CN";
			var entryLine2B = entryHeader2.MergedLines.AddNew();
			invoiceLine2B.JI_CL = entryLine2B.PK;
			invoiceLine2B.JI_Procedure = "7041000";
			entryLine2B.CL_InvoiceAmount = 400m;
			entryLine2B.CL_RX_NKInvoiceAmountCurrency = "USD";
			var b00B = entryLine2B.ConfirmedFees.AddNew();
			b00B.CF_ChargeType = "B00";
			b00B.CF_BaseValue = 200m;
			b00B.CF_ChargeAmount = 20m;
			b00B.CF_MethodOfPayment = "6";

			declaration2.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
			var item21 = declaration2.DocsAndCartage.OrderItems.AddNew();
			item21.JT_OrderReference = "xxx";
			var item22 = declaration2.DocsAndCartage.OrderItems.AddNew();
			item22.JT_OrderReference = "yyy";

			var (declaration3, invoiceHeader3, invoiceLine3, entryHeader3, entryLine3) = CreateDeclarationAtMinimumRequirement("0003");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0003";
			declaration3.JE_JS = shipment.PK;

			shipment.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
			var item31 = shipment.DocsAndCartage.OrderItems.AddNew();
			item31.JT_OrderReference = "aaa";
			var item32 = shipment.DocsAndCartage.OrderItems.AddNew();
			item32.JT_OrderReference = "bbb";
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusEntryHeader entryHeader, CusEntryLine entryLine) CreateDeclarationAtMinimumRequirement(string reference)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = "F";
			declaration.JE_OH_Importer = importer1PK;
			declaration.JE_OH_Supplier = supplier1PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "IM";
			declaration.JE_DeclarationReference = "B" + reference;
			declaration.JE_UCR = "UCR" + reference;
			declaration.ZG_VATDeferType = "2";

			var vatNumberDocument = declaration.SupportingDocuments.AddNew("1008", "VAT NO.");

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.ZG_CountryOfSupply = "US";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryReleaseDate = ZDateTime.Today;
			entryHeader.CH_BGMReference = "BGM" + reference;
			entryHeader.EntryNumber = "EntryNum" + reference;

			var entryLine = entryHeader.MergedLines.AddNew();
			var b00 = entryLine.ConfirmedFees.AddNew();
			b00.CF_ChargeType = "B00";
			b00.CF_BaseValue = 40m;
			b00.CF_ChargeAmount = 4m;
			b00.CF_MethodOfPayment = "6";

			invoiceLine.JI_CL = entryLine.PK;

			return (declaration, invoiceHeader, invoiceLine, entryHeader, entryLine);
		}

		protected override ZString ObjectName => "Report_FRVAT";

		protected override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
		{
			yield return (typeof(string), Report_FRVATReturnedParameters.JobNumber);
			yield return (typeof(string), Report_FRVATReturnedParameters.DUCR);
			yield return (typeof(string), Report_FRVATReturnedParameters.EntryNumber);
			yield return (typeof(string), Report_FRVATReturnedParameters.EntryReference);
			yield return (typeof(DateTime), Report_FRVATReturnedParameters.BAEDate);
			yield return (typeof(string), Report_FRVATReturnedParameters.CPC);
			yield return (typeof(string), Report_FRVATReturnedParameters.CountriesOfSupply);
			yield return (typeof(string), Report_FRVATReturnedParameters.InvoiceNumbers);
			yield return (typeof(decimal), Report_FRVATReturnedParameters.TotalInvoiceAmount);
			yield return (typeof(string), Report_FRVATReturnedParameters.Currency);
			yield return (typeof(decimal), Report_FRVATReturnedParameters.Rate);
			yield return (typeof(string), Report_FRVATReturnedParameters.ImporterCode);
			yield return (typeof(string), Report_FRVATReturnedParameters.ImporterName);
			yield return (typeof(string), Report_FRVATReturnedParameters.SupplierName);
			yield return (typeof(string), Report_FRVATReturnedParameters.EORI);
			yield return (typeof(string), Report_FRVATReturnedParameters.EORISuffix);
			yield return (typeof(string), Report_FRVATReturnedParameters.VATNumber);
			yield return (typeof(decimal), Report_FRVATReturnedParameters.TotalVATBaseAmount);
			yield return (typeof(decimal), Report_FRVATReturnedParameters.TotalVATAmount);
			yield return (typeof(string), Report_FRVATReturnedParameters.VATProcedureCode);
			yield return (typeof(string), Report_FRVATReturnedParameters.VATProcedureDescription);
			yield return (typeof(string), Report_FRVATReturnedParameters.OrderRefs);
		}

		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new[]
			{
				(Report_FRVATReturnedParameters.JobNumber, "B0001"),
				(Report_FRVATReturnedParameters.DUCR, "UCR0001"),
				(Report_FRVATReturnedParameters.EntryNumber, "EntryNum0001"),
				(Report_FRVATReturnedParameters.EntryReference, "BGM0001"),
				(Report_FRVATReturnedParameters.BAEDate, ZDateTime.Today.ToString("s")),
				(Report_FRVATReturnedParameters.CPC, "   (IMF/    )"),
				(Report_FRVATReturnedParameters.CountriesOfSupply, "US"),
				(Report_FRVATReturnedParameters.InvoiceNumbers, ""),
				(Report_FRVATReturnedParameters.TotalInvoiceAmount, "0.0000"),
				(Report_FRVATReturnedParameters.Currency, "EUR"),
				(Report_FRVATReturnedParameters.Rate, "1.640000000"),
				(Report_FRVATReturnedParameters.ImporterCode, "IMP1"),
				(Report_FRVATReturnedParameters.ImporterName, "Importer 1"),
				(Report_FRVATReturnedParameters.SupplierName, "Supplier 1"),
				(Report_FRVATReturnedParameters.EORI, "FR331597005"),
				(Report_FRVATReturnedParameters.EORISuffix, "00064"),
				(Report_FRVATReturnedParameters.VATNumber, "004477"),
				(Report_FRVATReturnedParameters.TotalVATBaseAmount, "40.000000"),
				(Report_FRVATReturnedParameters.TotalVATAmount, "4.0000"),
				(Report_FRVATReturnedParameters.VATProcedureCode, "2"),
				(Report_FRVATReturnedParameters.VATProcedureDescription, "AI2"),
				(Report_FRVATReturnedParameters.OrderRefs, "")
			};

			yield return new[]
			{
				(Report_FRVATReturnedParameters.JobNumber, "B0002"),
				(Report_FRVATReturnedParameters.DUCR, "UCR0002"),
				(Report_FRVATReturnedParameters.EntryNumber, "EntryNum0002"),
				(Report_FRVATReturnedParameters.EntryReference, "BGM0002"),
				(Report_FRVATReturnedParameters.BAEDate, ZDateTime.Today.ToString("s")),
				(Report_FRVATReturnedParameters.CPC, "70P(IMA/7041)"),
				(Report_FRVATReturnedParameters.CountriesOfSupply, "CN/US"),
				(Report_FRVATReturnedParameters.InvoiceNumbers, "Invoice NO. 1/Invoice NO. 2"),
				(Report_FRVATReturnedParameters.TotalInvoiceAmount, "700.0000"),
				(Report_FRVATReturnedParameters.Currency, "USD"),
				(Report_FRVATReturnedParameters.Rate, "1.390000000"),
				(Report_FRVATReturnedParameters.ImporterCode, "IMP1"),
				(Report_FRVATReturnedParameters.ImporterName, "Importer 1"),
				(Report_FRVATReturnedParameters.SupplierName, "Supplier 1"),
				(Report_FRVATReturnedParameters.EORI, "FR331597005"),
				(Report_FRVATReturnedParameters.EORISuffix, "00064"),
				(Report_FRVATReturnedParameters.VATNumber, "004477"),
				(Report_FRVATReturnedParameters.TotalVATBaseAmount, "240.000000"),
				(Report_FRVATReturnedParameters.TotalVATAmount, "24.0000"),
				(Report_FRVATReturnedParameters.VATProcedureCode, "2"),
				(Report_FRVATReturnedParameters.VATProcedureDescription, "AI2"),
				(Report_FRVATReturnedParameters.OrderRefs, "xxx,yyy")
			};

			yield return new[]
			{
				(Report_FRVATReturnedParameters.JobNumber, "S0003"),
				(Report_FRVATReturnedParameters.DUCR, "UCR0003"),
				(Report_FRVATReturnedParameters.EntryNumber, "EntryNum0003"),
				(Report_FRVATReturnedParameters.EntryReference, "BGM0003"),
				(Report_FRVATReturnedParameters.BAEDate, ZDateTime.Today.ToString("s")),
				(Report_FRVATReturnedParameters.CPC, "   (IMF/    )"),
				(Report_FRVATReturnedParameters.CountriesOfSupply, "US"),
				(Report_FRVATReturnedParameters.InvoiceNumbers, ""),
				(Report_FRVATReturnedParameters.TotalInvoiceAmount, "0.0000"),
				(Report_FRVATReturnedParameters.Currency, "EUR"),
				(Report_FRVATReturnedParameters.Rate, "1.640000000"),
				(Report_FRVATReturnedParameters.ImporterCode, "IMP1"),
				(Report_FRVATReturnedParameters.ImporterName, "Importer 1"),
				(Report_FRVATReturnedParameters.SupplierName, "Supplier 1"),
				(Report_FRVATReturnedParameters.EORI, "FR331597005"),
				(Report_FRVATReturnedParameters.EORISuffix, "00064"),
				(Report_FRVATReturnedParameters.VATNumber, "004477"),
				(Report_FRVATReturnedParameters.TotalVATBaseAmount, "40.000000"),
				(Report_FRVATReturnedParameters.TotalVATAmount, "4.0000"),
				(Report_FRVATReturnedParameters.VATProcedureCode, "2"),
				(Report_FRVATReturnedParameters.VATProcedureDescription, "AI2"),
				(Report_FRVATReturnedParameters.OrderRefs, "aaa,bbb")
			};
		}

		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return (Report_FRVATParameter.CompanyPK, GlbCompany.CurrentCompany.PK.ToString());
			yield return (Report_FRVATParameter.ImporterPK, importer1PK.ToString());
			yield return (Report_FRVATParameter.SupplierPK, supplier1PK.ToString());
			yield return (Report_FRVATParameter.VATProcedureCode, "2");
			yield return (Report_FRVATParameter.BAEDateFrom, ZDateTime.Today.AddMonths(-1).ToString("s"));
			yield return (Report_FRVATParameter.BAEDateTo, ZDateTime.Today.AddMonths(1).ToString("s"));
		}

		protected override IEnumerable<string> GetParameterNameList()
		{
			yield return Report_FRVATParameter.CompanyPK;
			yield return Report_FRVATParameter.ImporterPK;
			yield return Report_FRVATParameter.SupplierPK;
			yield return Report_FRVATParameter.VATProcedureCode;
			yield return Report_FRVATParameter.BAEDateFrom;
			yield return Report_FRVATParameter.BAEDateTo;
		}

		protected override void SetUp()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMP1";
			importer1.OH_FullName = "Importer 1";
			importer1.CustomsCodes.AddNew(EuropeanUnionSharedCodeTypes.Eori, "331597005", Core.Constants.CountryCodes.France);
			importer1.CustomsCodes.AddNew(FranceCodeTypes.EoriBranchSuffix, "00064", Core.Constants.CountryCodes.France);
			importer1.CustomsCodes.AddNew("TVA", "004477", "FR");
			importer1PK = importer1.PK.ToGuid();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "Supplier 1";
			supplier1PK = supplier1.PK.ToGuid();
		}

		Guid importer1PK;
		Guid supplier1PK;
	}

	class Report_FRVATParameter
	{
		public const string CompanyPK = "@CompanyPK";
		public const string ImporterPK = "@ImporterPK";
		public const string SupplierPK = "@SupplierPK";
		public const string VATProcedureCode = "@VATProcedureCode";
		public const string BAEDateFrom = "@BAEDateFrom";
		public const string BAEDateTo = "@BAEDateTo";
	}

	class Report_FRVATReturnedParameters
	{
		public const string JobNumber = "JobNumber";
		public const string DUCR = "DUCR";
		public const string EntryNumber = "EntryNumber";
		public const string EntryReference = "EntryReference";
		public const string BAEDate = "BAEDate";
		public const string CPC = "CPC";
		public const string CountriesOfSupply = "CountriesOfSupply";
		public const string InvoiceNumbers = "InvoiceNumbers";
		public const string TotalInvoiceAmount = "TotalInvoiceAmount";
		public const string Currency = "Currency";
		public const string Rate = "Rate";
		public const string ImporterCode = "ImporterCode";
		public const string ImporterName = "ImporterName";
		public const string SupplierName = "SupplierName";
		public const string EORI = "EORI";
		public const string EORISuffix = "EORISuffix";
		public const string VATNumber = "VATNumber";
		public const string TotalVATBaseAmount = "TotalVATBaseAmount";
		public const string TotalVATAmount = "TotalVATAmount";
		public const string VATProcedureCode = "VATProcedureCode";
		public const string VATProcedureDescription = "VATProcedureDescription";
		public const string OrderRefs = "OrderRefs";
	}
}
