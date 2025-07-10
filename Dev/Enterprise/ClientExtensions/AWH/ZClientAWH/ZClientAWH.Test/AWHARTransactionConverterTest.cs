using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AWH.Testing
{
	public class AWHARTransactionConverterTest : TestCaseWithFactory
	{
		public void TestMapExport()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			AWHARTransactionConverterForTest converter = new AWHARTransactionConverterForTest(buffer, Factory);
			Xsd.TxnHeader header = CreateHeader(Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			Xsd.Organisation organisation = CreateOrganisation(true);
			header.DebtorOrCreditor = organisation;
			FlatFileDataRowCollection rows = converter.MapExport(header);
			AssertEquals("Number of rows created should be 1", 1, rows.Count);
			FlatFileDataRow dataRow = rows[0];
			AssertEquals("Code", AWHConstants.TransType, dataRow[AWHFlatFileDataRow.Schema.Code.Name]);
			AssertEquals("Zero1", AWHConstants.FixedZero, dataRow[AWHFlatFileDataRow.Schema.Zero1.Name]);
			AssertEquals("Zero2", AWHConstants.FixedZero, dataRow[AWHFlatFileDataRow.Schema.Zero2.Name]);
			AssertEquals("One", AWHConstants.FixedOne, dataRow[AWHFlatFileDataRow.Schema.One.Name]);
			AssertEquals("A", AWHConstants.FixedA, dataRow[AWHFlatFileDataRow.Schema.A.Name]);
			AssertEquals("ClientAccount", "12345", dataRow[AWHFlatFileDataRow.Schema.ClientAccount.Name]);
			AssertEquals("GLAccount", "ABC1111111", dataRow[AWHFlatFileDataRow.Schema.GLAccount.Name]);
			AssertEquals("Date", "20070101", dataRow[AWHFlatFileDataRow.Schema.Date.Name]);
			AssertEquals("Desc", "093 charge code  1 2 3 4", dataRow[AWHFlatFileDataRow.Schema.Desc.Name]);
			AssertEquals("Amount", "92387423", dataRow[AWHFlatFileDataRow.Schema.Amount.Name]);
			AssertEquals("Year", "2007", dataRow[AWHFlatFileDataRow.Schema.Year.Name]);
			AssertEquals("Invoice", "INVOICE", dataRow[AWHFlatFileDataRow.Schema.Invoice.Name]);
			AssertEquals("GST Code", "G01I", dataRow[AWHFlatFileDataRow.Schema.TaxCode.Name]);
		}

		public void TestExportThrowLSCCodeNotSetException()
		{
			Xsd.TxnHeader header = CreateHeader(Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			Xsd.Organisation organisation = CreateOrganisation(false);
			header.DebtorOrCreditor = organisation;
			AssertThrowCorrectException(AWHException.AWHExceptionType.LegacySystemCodeNotSet, header);
		}

		public void TestExportThrowBranchNotSetException()
		{
			Xsd.TxnHeader header = CreateHeader("123", Env.CurrentDepartment.Code);
			Xsd.Organisation organisation = CreateOrganisation(true);
			header.DebtorOrCreditor = organisation;
			AssertThrowCorrectException(AWHException.AWHExceptionType.BranchCodeMappingNotSet, header);
		}

		public void TestExportThrowDeptNotSetException()
		{
			Xsd.TxnHeader header = CreateHeader(Env.CurrentBranch.Code, "NOT");
			Xsd.Organisation organisation = CreateOrganisation(true);
			header.DebtorOrCreditor = organisation;
			AssertThrowCorrectException(AWHException.AWHExceptionType.DeptCodeMappingNotSet, header);
		}

		void AssertThrowCorrectException(AWHException.AWHExceptionType expectedType, Xsd.TxnHeader header)
		{
			NotificationBuffer buffer = new NotificationBuffer();
			AWHARTransactionConverterForTest converter = new AWHARTransactionConverterForTest(buffer, Factory);
			try
			{
				converter.MapExport(header);
			}
			catch (Exception e)
			{
				AssertEquals("Exception is an AWHException", typeof(AWHException), e.GetType());
				AWHException ex = (AWHException)e;
				AssertEquals("Exception Type", expectedType, ex.Type);
			}
		}

		Xsd.TxnHeader CreateHeader(ZString branch, ZString dept)
		{
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.Ledger = Xsd.TxnLedgerType.AR;
			header.TxnType = Xsd.TxnType.INV;
			header.PostDate = new ZDateTime(2007, 1, 1, 12, 0, 0);
			header.InvoiceDate = new ZDateTime(2007, 1, 1, 12, 0, 0);
			header.Description = "Description";
			header.DisbursementFlag = true;
			header.Branch = branch;
			header.Department = dept;
			header.TxnNumber = "INVOICE";
			header.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("92847.423"), Core.Constants.CurrencyCodes.Australia);
			header.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("3214.2389"), Core.Constants.CurrencyCodes.Australia);
			header.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("3000077"), Core.Constants.CurrencyCodes.AmericanSamoa);
			header.JobInvoiceNo = "0943ad45/0dfjlksda";
			header.ChequeOrReference = "CHK0002398723947";
			header.TxnLines.Add(CreateLine(branch, dept));
			return header;
		}

		Xsd.TxnLine CreateLine(ZString branch, ZString dept)
		{
			Xsd.TxnLine line = new Xsd.TxnLine();
			line.Branch = branch;
			line.Department = dept;
			line.ChargeCode = "093";
			line.Description = "093 charge code \r\n1,2,3,4";
			line.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			line.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("923874.23"), Core.Constants.CurrencyCodes.Australia);
			line.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("5276.54"), Core.Constants.CurrencyCodes.Australia);
			line.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("9785.08"), Core.Constants.CurrencyCodes.Australia);
			line.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("74956.3723"), Core.Constants.CurrencyCodes.NewZealand);
			line.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("182.4389"), Core.Constants.CurrencyCodes.NewZealand);
			line.OsTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Parse("234.74"), Core.Constants.CurrencyCodes.NewZealand);
			line.GLAccount = "AB.C1.11.11.11";
			line.ChargeGroup = "JobGroup";
			line.TaxCode = "GST";
			line.IsFinalCharge = false;
			return line;
		}

		Xsd.Organisation CreateOrganisation(bool setLSC)
		{
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.OrganisationDetails.Name = "Eagle Datamation International";
			organisation.OrganisationDetails.Location.Value = "AUSYD";
			Xsd.OrgContact contact = organisation.OrganisationDetails.Contacts.AddNew();
			contact.Name = "John Doe";
			Xsd.OrgAddress address = organisation.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "Level 2";
			address.AddressLine2 = "184 Bourke Road";
			Xsd.AddressCapability capability = address.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			address.CityOrSuburb = "Alexandria";
			address.Email = "users@edi.com.au";
			address.Location.Value = "AUMEL";
			address.Location.Country = "Australia";
			address.PostCode = "2015";
			address.StateOrProvince = "NSW";
			Xsd.TelephoneNumber number = address.TelephoneNumbers.AddNew();
			number.Value = "02 9025 1100";
			number.NumberType = Xsd.TelephoneNumberNumberType.Business;
			number = address.TelephoneNumbers.AddNew();
			number.Value = "02 9025 1199";
			number.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			organisation.EDICode = "EDI";
			if (setLSC)
			{
				Xsd.RegistrationNumber registrationNumber = organisation.OrganisationDetails.RegistrationNumbers.AddNew();
				registrationNumber.Number = "12345";
				registrationNumber.NumberType = Xsd.RegistrationNumberTypes.LSC;
				registrationNumber.CountryOfRegistration = Env.CurrentCompany.Country.Code;
			}

			return organisation;
		}

#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			SetupRegistryItems();
		}

		void SetupRegistryItems()
		{
			AWHDataRegistry.Instance.ARTransExportDirectory = Env.TempPath;
			AWHDataRegistry.Instance.ExportFilePrefix = "Prefix";
			CodeDescriptionPairList brhList = new CodeDescriptionPairList();
			brhList.Add(new CodeDescriptionPair(Env.CurrentBranch.Code, "ABC"));
			AWHDataRegistry.Instance.BranchList = brhList;
			CodeDescriptionPairList deptList = new CodeDescriptionPairList();
			deptList.Add(new CodeDescriptionPair(Env.CurrentDepartment.Code, "1"));
			AWHDataRegistry.Instance.DepartmentList = deptList;
		}

		class AWHARTransactionConverterForTest : AWHARTransactionConverter
		{
			public AWHARTransactionConverterForTest(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
#endregion
	}
}
