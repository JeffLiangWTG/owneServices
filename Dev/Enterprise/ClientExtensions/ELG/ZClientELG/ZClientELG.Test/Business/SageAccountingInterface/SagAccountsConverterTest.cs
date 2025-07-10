using CargoWise.Types;
using Enterprise.ClientSharedComponents.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ELG.Testing
{
	public abstract class SagAccountsConverterTest : AccountsConverterARAPTest
	{
		public void TestTaxIndicatorContValues()
		{
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.EXEMPT == "0");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.FREEVAT == "1");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.CAPVAT == "2");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.VAT == "2");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.LOWVAT == "2");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.MIDVAT == "2");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.MIDVATREV == "4");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.LOWVATREV == "4");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.AR_FREEVATREV == "4");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.AP_FREEVATREV == "7");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.AP_VATREV == "8");
			Assert("Tax indicator values", Enterprise.Client.ELG.SagAccountsConverter.TaxIndicator.AR_VATREV == "9");
		}

		[ExpectException(typeof(ELGException))]
		public void TestInvalidBranchDepartmentMapping()
		{
			TestHelper.SetValidRegistrySagDataExportEnabled();
			TestHelper.SetInvalidRegistryBranchDepartmentCodeCollectionItem();
			TestHelper.SetValidRegistryTransportAndChargeCodeCollectionItem();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem();
			Xsd.TxnHeader sourceXml = GetSource();
			SetActual(sourceXml);
		}

		[ExpectException(typeof(ELGException))]
		public void TestInvalidTransportChargeCodeMapping()
		{
			TestHelper.SetValidRegistrySagDataExportEnabled();
			TestHelper.SetValidRegistryBranchDepartmentCodeCollectionItem();
			TestHelper.SetInvalidRegistryTransportAndChargeCodeCollectionItem();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem();
			Xsd.TxnHeader sourceXml = GetSource();
			SetActual(sourceXml);
		}

		[ExpectException(typeof(ELGException))]
		public void TestInvalidSageAccountMapping()
		{
			TestHelper.SetValidRegistrySagDataExportEnabled();
			TestHelper.SetValidRegistryBranchDepartmentCodeCollectionItem();
			TestHelper.SetValidRegistryTransportAndChargeCodeCollectionItem();
			TestHelper.SetInvalidRegistrySageAccountCodeCollectionItem();
			Xsd.TxnHeader sourceXml = GetSource();
			SetActual(sourceXml);
		}

		[ExpectNoExceptions]
		[TestDate(2007, 5, 23)]
		public void TestMapExport()
		{
			TestHelper.SetValidRegistryAll();
			Xsd.TxnHeader sourceXml = GetSource();
			FlatFileDataRowCollection expectedRows = SetExpectedRows();
			FlatFileDataRowCollection actualRows = SetActual(sourceXml);
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
		}

		protected virtual Xsd.TxnHeader GetSource()
		{
			Xsd.TxnHeader result = new Xsd.TxnHeader();
			result.Branch = TestHelper.FindOrCreateBranch("SYD").GB_Code;
			result.DebtorOrCreditor.EDICode = "ABC";
			result.Department = TestHelper.FindOrCreateDepartment("BRN").GE_Code;
			result.Description = "Shipment";
			result.DueDate = new ZDateTime(2007, 5, 11);
			result.GlAccount = "1111.22.33";
			result.InvTermDays = "30";
			result.InvoiceDate = new ZDateTime(2007, 5, 1);
			result.JobInvoiceNo = "B00001041";
			result.LocalInvoiceAmtInclTax.Value = 500.25m;
			result.LocalInvoiceAmtExclTax.Value = 450.25m;
			result.OsInvoiceAmtInclTax.Value = 500.25m;
			result.OsInvoiceAmtExclTax.Value = 450.25m;
			result.OsInvoiceAmtExclTax.CurrencyCode = "AUD";
			result.LocalTaxAmount.Value = 20.025m;
			result.OsTaxAmount.Value = 20.025m;
			result.TxnNumber = "000002053";
			result.PostDate = new ZDateTime(2007, 5, 10);
			Xsd.RegistrationNumber regoNum = new Xsd.RegistrationNumber();
			regoNum.CountryOfRegistration = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			regoNum.NumberType = Xsd.RegistrationNumberTypes.LSC;
			regoNum.Number = "LSC123";
			result.DebtorOrCreditor.OrganisationDetails.RegistrationNumbers.Add(regoNum);
			Xsd.TxnLine detailValue = result.TxnLines.AddNew();
			detailValue.GLAccount = "4444.22.33";
			detailValue.Description = "Line Description";
			detailValue.LocalInvoiceAmtInclTax.Value = 123.45m;
			detailValue.Branch = TestHelper.FindOrCreateBranch("SYD").GB_Code;
			detailValue.Department = TestHelper.FindOrCreateDepartment("BRN").GE_Code;
			detailValue.ModeOfTransport = Xsd.TransportMode.SEA;
			detailValue.ChargeCode = "FRT";
			detailValue.LocalInvoiceAmtInclTax.Value = 500.25m;
			detailValue.LocalInvoiceAmtExclTax.Value = 450.225m;
			detailValue.OsInvoiceAmtInclTax.Value = 500.25m;
			detailValue.OsInvoiceAmtExclTax.Value = 450.225m;
			detailValue.LocalTaxAmount.Value = 20.025m;
			detailValue.OsTaxAmount.Value = 20.025m;
			detailValue.ConsolOrJobNo = "S00001046";
			detailValue.TaxCode = "EXEMPT";
			return result;
		}

		protected NotificationBuffer Notifications
		{
			get
			{
				return notificaitons ?? (notificaitons = new NotificationBuffer());
			}
		}

		NotificationBuffer notificaitons;
		protected ELGTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new ELGTestHelper(Factory));
			}
		}

		ELGTestHelper testHelper;
		protected SagAccountsFlatFileFormat Format
		{
			get
			{
				return format ?? (format = new SagAccountsFlatFileFormat());
			}
		}

		SagAccountsFlatFileFormat format;
		protected abstract FlatFileDataRowCollection SetExpectedRows();
		protected abstract FlatFileDataRowCollection SetActual(Xsd.TxnHeader xmlHeader);
	}
}
