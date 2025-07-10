using System;
using System.Reflection;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseCusEntryLine))]
	public abstract class DocBaseCusEntryLineAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : CusEntryLine
			where TWrapper : DocBaseCusEntryLine
	{
		public void TestDutyRateDescription()
		{
			EntryLineInternal.CL_DutyPercent = 22.5m;
			EntryLineInternal.CL_FlatAmount = 44.66m;
			EntryLineInternal.CL_FlatAmountUQ = "FF";
			AssertEquals("DutyRateDescription", EntryLineInternal.DutyRateDescription, EntryLineWrapperInternal.DutyRateDescription);
		}

		public void TestToString()
		{
			EntryLineInternal.CL_LineNumber = Convert.ToByte(1);
			AssertEquals("ToString()", EntryLineInternal.CL_LineNumber.ToString(), EntryLineWrapperInternal.ToString());
		}

		#region Abstract

		protected abstract TWrapper CreateEntryLineWrapper(ICusEntryLine entryLineInternal);

		#endregion

		#region ZDecimal Fields

		public virtual void TestLinePriceInLocalCurrencyEqualsTheRelatedValueInBizObj()
		{
			AssertEquals(DocEntryLineMergeOfTwoInvoiceLines.LinePriceInLocalCurrency, ((CusEntryLine)EntryLineMergeOfTwoInvoiceLines).TotalLinePriceInLocalCurrency);
		}

		public virtual void TestCustomsValue()
		{
			EntryLineInternal.CL_CustomsValue = 12.34M;
			AssertEquals("CustomsValue", EntryLineInternal.CL_CustomsValue, EntryLineWrapperInternal.CustomsValue);
		}

		public void TestCustomsValueInLocalCurrency()
		{
			EntryLineInternal.CL_CustomsValue = 12.34M;
			AssertEquals("CustomsValueInLocalCurrency", EntryLineInternal.CustomsValue.Amount, EntryLineWrapperInternal.CustomsValueInLocalCurrency);
		}

		public void TestDutyAmount()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.34m);
			AssertEquals("DutyAmount", EntryLineInternal.DutyAmount, EntryLineWrapperInternal.DutyAmount);
		}

		public void TestDutyPercent()
		{
			EntryLineInternal.CL_DutyPercent = 12.34M;
			AssertEquals("DutyPercent", EntryLineInternal.CL_DutyPercent, EntryLineWrapperInternal.DutyPercent);
		}

		public void TestGSTVATAmount()
		{
			EntryLineInternal.Fees.AddOrUpdate(EntryLineInternal.Declaration.GSTOrVATCode, 12.34m);
			AssertEquals("GSTVATAmount", EntryLineInternal.GSTVATAmount, EntryLineWrapperInternal.GSTVATAmount);
		}

		public void TestGSTVATDeferred()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 12.34m);
			AssertEquals("GSTVATDeferred", EntryLineInternal.GSTVATDeferred, EntryLineWrapperInternal.GSTVATDeferred);
		}

		public void TestWarehouseUnitValue()
		{
			EntryLineInternal.CL_WarehouseUnitValue = 12.34M;
			AssertEquals("WarehouseUnitValue", EntryLineInternal.CL_WarehouseUnitValue, EntryLineWrapperInternal.WarehouseUnitValue);
		}

		public virtual void TestDutyAmountRounded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Tunisia))
			{
				EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
				AssertEquals("DutyAmount", 12.345M, EntryLineWrapperInternal.DutyAmountRounded);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AssertEquals("DutyAmount", 12M, EntryLineWrapperInternal.DutyAmountRounded);
			}
		}

		public virtual void TestGSTVATAmountRounded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Tunisia))
			{
				EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12.3453M);
				AssertEquals("DutyAmount", 12.345M, EntryLineWrapperInternal.GSTVATAmountRounded);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AssertEquals("DutyAmount", 12M, EntryLineWrapperInternal.GSTVATAmountRounded);
			}
		}

		#endregion

		#region ZShort Fields

		public void TestLineNumber()
		{
			EntryLineInternal.CL_LineNumber = Convert.ToByte(1);
			AssertEquals("LineNumber", EntryLineInternal.CL_LineNumber, EntryLineWrapperInternal.LineNumber);
		}

		#endregion

		#region ZString Fields

		public abstract void TestLinePricesWithCurrency();

		public void TestParentTrailer()
		{
			EntryLineInternal.CL_ParentTrailer = "P";
			AssertEquals("ParentTrailer", EntryLineInternal.CL_ParentTrailer, EntryLineWrapperInternal.ParentTrailer);
		}

		public void TestCountryOfOriginCodeCanBeSet()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			BaseJobComInvoiceHeader invHeader = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_CountryOfOrigin = "AU";
			invLine.JI_Tariff = "111";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			entryLine.MergeInvoiceLine(invLine);

			DocBaseCusEntryLineTestClass docEntryLine = DocBaseCusEntryLineTestClass.New(entryLine, Factory);
			AssertEquals("Country of Origin is not passed properly into DocCusEntryLine", docEntryLine.CountryOfOriginCode, "AU");
		}

		public void TestTariffCanBeSet()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_AdValoremTariff = "1234.5";
			DocBaseCusEntryLineTestClass docEntryLine = DocBaseCusEntryLineTestClass.New(entryLine, Factory);
			AssertEquals("Tariff is not passed properly into DocCusEntryLine", entryLine.CL_AdValoremTariff, docEntryLine.Tariff);
		}

		public void TestFormattedTariff()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			BaseJobComInvoiceHeader invHeader = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_Tariff = "1234.5";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			DocBaseCusEntryLineTestClass docEntryLine = DocBaseCusEntryLineTestClass.New(entryLine, Factory);
			AssertEquals("FormattedTariff", entryLine.FormattedTariff, docEntryLine.FormattedTariff);
		}

		#endregion

		#region Implementation Fields Tests

		public void TestInvoiceLineInteral()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			DocBaseCusEntryLineTestClass entryLineClassWrapper = DocBaseCusEntryLineTestClass.New(EntryLineInternal, Factory);

			AssertNotNull("Invoice line is not empty", entryLineClassWrapper.InvoiceLineInternalTestMethod);
		}

		#endregion

		#region Sub Class Tests

		public void TestInvoiceLine()
		{
			PropertyInfo property = EntryLineWrapperInternal.GetType().GetProperty("InvoiceLine");
			AssertNotNull("You must implement a property call InvoiceLine", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceLine result = (DocBaseJobComInvoiceLine)method.Invoke(EntryLineWrapperInternal, Array.Empty<object>());
			AssertNotNull("InvoiceLine is not null", result);
			Assert("InvoiceLine is of type DocJobComInvoiceLine", result.GetType().ToString().EndsWith("DocJobComInvoiceLine"));
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				EntryLineWrapperInternal
			};
		}

		#region Implementation

		protected T EntryLineInternal;
		protected TWrapper EntryLineWrapperInternal
		{
			get { return CreateEntryLineWrapper(EntryLineInternal); }
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			EntryLineInternal = GetNewEntryLine();
			base.SetUp();
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateEntryLineWrapper(EntryLineInternal);
		}

		protected virtual T GetNewEntryLine()
		{
			BaseJobDeclaration declaration = GetNewDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return (T)entryLine;
		}

		protected virtual ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get { return null; }
		}

		protected virtual TWrapper DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return null; }
		}

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
		#endregion
	}
}
