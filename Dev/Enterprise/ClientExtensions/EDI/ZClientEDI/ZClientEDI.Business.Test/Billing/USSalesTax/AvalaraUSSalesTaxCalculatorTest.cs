using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax.Test
{
	public class AvalaraUSSalesTaxCalculatorTest : TestCaseWithFactory
	{
		public void TestName()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			AssertEquals("Avalara Web Service", calculator.Name);
		}

		public void TestGetConfiguration_And_IsEnabled_IsBasedOnRegistry()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			var calculator = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Off)
			)
			{
				var config = calculator.GetConfiguration(branch);
				var isEnabled = calculator.IsEnabled(branch);
				AssertEquals("Configuration should be based on registry code OFF", ConfigurationStatus.Off, config);
				AssertEquals("IsEnabled should be based on registry code OFF", false, isEnabled);
			}

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var config = calculator.GetConfiguration(branch);
				var isEnabled = calculator.IsEnabled(branch);
				AssertEquals("Configuration should be based on registry code SND", ConfigurationStatus.Sandbox, config);
				AssertEquals("IsEnabled should be based on registry code SND", true, isEnabled);
			}

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Production)
			)
			{
				var config = calculator.GetConfiguration(branch);
				var isEnabled = calculator.IsEnabled(branch);
				AssertEquals("Configuration should be based on registry code PRD", ConfigurationStatus.Production, config);
				AssertEquals("IsEnabled should be based on registry code PRD", true, isEnabled);
			}
		}

		public void TestGetChargeCode_IsBasedOnRegistry()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC2.PK.ToGuid()))
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var (currentBranchChargePK, currentBranchChargeCode) = calculator.GetChargeCode(GlbBranch.CurrentBranch);
				AssertEquals("Current branch Charge Code should be based on registry showing CC2 PK", TestObjectCreator.CC2.PK, currentBranchChargePK);
				AssertEquals("Current branch Charge Code should be based on registry showing CC2 code", "ZZCC2", currentBranchChargeCode);

				var (otherBranchChargePK, otherBranchChargeCode) = calculator.GetChargeCode(TestObjectCreator.NonCurrentCompanyBranch);
				AssertEquals("Other branch Charge Code should be based on registry showing CC3 PK", TestObjectCreator.CC3.PK, otherBranchChargePK);
				AssertEquals("Other branch Charge Code should be based on registry showing CC3 code", "ZZCC3", otherBranchChargeCode);
			}

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Guid.NewGuid()))
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var (currentBranchChargePK, currentBranchChargeCode) = calculator.GetChargeCode(GlbBranch.CurrentBranch);
				AssertEquals("Current branch Charge Code should be empty guid when no AccChargeCode can be found", ZGuid.Empty, currentBranchChargePK);
				AssertEquals("Current branch Charge Code should be empty string when no AccChargeCode can be found", ZString.Empty, currentBranchChargeCode);

				var (otherBranchChargePK, otherBranchChargeCode) = calculator.GetChargeCode(TestObjectCreator.NonCurrentCompanyBranch);
				AssertEquals("Other branch Charge Code should be empty guid when no AccChargeCode can be found", ZGuid.Empty, otherBranchChargePK);
				AssertEquals("Other branch Charge Code should be empty string when no AccChargeCode can be found", ZString.Empty, otherBranchChargeCode);
			}
		}

		public void TestShouldShowMenuItemsOnInvoiceForm_ReturnsTrueForArLedger_AndInvCrdTransactionTypes()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
			var arAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("ARADJ001", 1m, 1m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("APINV001", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			var apCreditNote = TestObjectCreator.CreateAPCreditNote("APCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AP Credit Note");
			var apAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("APADJ001", 1m, 1m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);

			var pendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("PA001", TestObjectCreator.AALSHI, 10, 0m, TestObjectCreator.AUD, 1m);
			AssertEquals("Precondition", "PA", pendingAllocation.AH_Ledger);

			var calculator = new AvalaraUSSalesTaxCalculator();
			Assert("AR INV should show menu item", calculator.ShouldShowMenuItemsOnInvoiceForm(arInvoice));
			Assert("AR CRD should show menu item", calculator.ShouldShowMenuItemsOnInvoiceForm(arCreditNote));
			Assert("AR ADJ should not show menu item", !calculator.ShouldShowMenuItemsOnInvoiceForm(arAdjustmentNote));
			Assert("AP INV should not show menu item", !calculator.ShouldShowMenuItemsOnInvoiceForm(apInvoice));
			Assert("AP CRD should not show menu item", !calculator.ShouldShowMenuItemsOnInvoiceForm(apCreditNote));
			Assert("AP ADJ should not show menu item", !calculator.ShouldShowMenuItemsOnInvoiceForm(apAdjustmentNote));
			Assert("non AR ledger should not show menu item", !calculator.ShouldShowMenuItemsOnInvoiceForm(pendingAllocation));
		}

		public void TestMenuItemText()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			AssertNull("Generic text for Calculate menu item is good enough", calculator.CalculateMenuItemText);
			AssertEquals("Generic text for Submit menu item is Avalara specific and should be overriden", "Submit Transaction to Avalara as Committed", calculator.SubmitMenuItemText);
		}

		public void TestCheckpointForCalculationMenuItem()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var apCreditNote = TestObjectCreator.CreateAPCreditNote("APCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AP Credit Note");
			var pendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("PA001", TestObjectCreator.AALSHI, 10, 0m, TestObjectCreator.AUD, 1m);
			AssertEquals("Precondition", "PA", pendingAllocation.AH_Ledger);

			var calculator = new AvalaraUSSalesTaxCalculator();
			var checkpointForNull = calculator.CheckpointForCalculationMenuItem(null);
			var checkpointForAR = calculator.CheckpointForCalculationMenuItem(arInvoice);
			var checkpointForAP = calculator.CheckpointForCalculationMenuItem(apCreditNote);
			var checkpointForPA = calculator.CheckpointForCalculationMenuItem(pendingAllocation);

			AssertEquals("Null should return Access Denied checkpoint", "Access Denied", checkpointForNull.DisplayTextPathToSecurityRight);
			AssertEquals("AR transaction should return AR checkpoint", "Manage -> Receivables -> Receivables Transactions -> Avalara US Sales Tax Integration -> Request Sales Tax Calculation", checkpointForAR.DisplayTextPathToSecurityRight);
			AssertEquals("AP transaction should return Access Denied checkpoint", "Access Denied", checkpointForAP.DisplayTextPathToSecurityRight);
			AssertEquals("PA transaction should return Access Denied checkpoint", "Access Denied", checkpointForPA.DisplayTextPathToSecurityRight);
		}

		public void TestCheckpointForSubmitMenuItem()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var apCreditNote = TestObjectCreator.CreateAPCreditNote("APCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AP Credit Note");
			var pendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("PA001", TestObjectCreator.AALSHI, 10, 0m, TestObjectCreator.AUD, 1m);
			AssertEquals("Precondition", "PA", pendingAllocation.AH_Ledger);

			var calculator = new AvalaraUSSalesTaxCalculator();
			var checkpointForNull = calculator.CheckpointForSubmitMenuItem(null);
			var checkpointForAR = calculator.CheckpointForSubmitMenuItem(arInvoice);
			var checkpointForAP = calculator.CheckpointForSubmitMenuItem(apCreditNote);
			var checkpointForPA = calculator.CheckpointForSubmitMenuItem(pendingAllocation);

			AssertEquals("Null should return Access Denied checkpoint", "Access Denied", checkpointForNull.DisplayTextPathToSecurityRight);
			AssertEquals("AR transaction should return AR checkpoint", "Manage -> Receivables -> Receivables Transactions -> Avalara US Sales Tax Integration -> Resubmit Sales Tax Transaction to Avalara", checkpointForAR.DisplayTextPathToSecurityRight);
			AssertEquals("AP transaction should return Access Denied checkpoint", "Access Denied", checkpointForAP.DisplayTextPathToSecurityRight);
			AssertEquals("PA transaction should return Access Denied checkpoint", "Access Denied", checkpointForPA.DisplayTextPathToSecurityRight);
		}

		public void TestCalculationMenuItemTroubleshootingHint()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			AssertEquals("Further diagnostic information can be found in Help > Diagnostics > Trace Monitor > Accounting > HTTP Communication.", calculator.MenuItemTroubleshootingHint.ToString());
		}

		public void TestGetCurrentSalesTaxAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			AssertEquals("Current Sales Tax Amount should be zero when null invoice", 0m, calculator.GetCurrentSalesTaxAmount(null));

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC2.PK.ToGuid()))
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1.5m, TestObjectCreator.AALSHI);
				AssertEquals("Current Sales Tax Amount should be zero when no line items", 0m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				AddLine(arInvoice, TestObjectCreator.CC1, 11m, 1, description: "Not Sales Tax");
				AssertEquals("Current Sales Tax Amount should be zero when nominated charge code is not present on any line item", 0m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var salesTaxLine = AddLine(arInvoice, TestObjectCreator.CC2, 17m, 1, description: "Sales Tax");
				AssertNotEquals("Precondition: local and OS amounts are different", salesTaxLine.AL_OSAmount, salesTaxLine.AL_LocalTotalAmount);
				AssertNotEquals("Precondition: local and OS amounts are different", salesTaxLine.AL_OSExTaxAmount, salesTaxLine.AL_LocalExTaxAmount);
				AssertEquals("Current Sales Tax Amount should be read from line item OS Amount when nominated charge code present on a line item", 17m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var salesTaxLine2 = AddLine(arInvoice, TestObjectCreator.CC2, 23m, 1, description: "Sales Tax");
				AssertEquals("Current Sales Tax Amount should be summed from all matching line items", 40m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var salesTaxLineOtherCompanyChargeCode = AddLine(arInvoice, TestObjectCreator.CC3, 5m, 1, description: "Sales Tax, but for another company");
				AssertEquals("Current Sales Tax Amount should be summed using transaction Branch/Company to determine charge code", 40m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
				arInvoice.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
				AssertEquals("Current Sales Tax Amount should be summed using transaction Branch/Company to determine charge code", 5m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC2, 11m, 1);
				AssertEquals("Current Sales Tax Amount should be negative for AR CRD", -11m, calculator.GetCurrentSalesTaxAmount(arCreditNote));
			}

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV002", TestObjectCreator.AUD, 1.5m, TestObjectCreator.AALSHI);
				var line = AddLine(arInvoice, TestObjectCreator.CC1, 37m, 1, description: "Not Sales Tax");
				line.AL_AC = ZGuid.Empty;

				AssertEquals("Current Sales Tax Amount should be zero when no charge code nominated for sales tax in registry", 0m, calculator.GetCurrentSalesTaxAmount(arInvoice));
			}
		}

		public void TestGetBaseUrl()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			var calculator = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Off)
			)
			{
				var url = calculator.GetBaseRestUrl(branch);
				AssertNull("When configuration is Off, no url should be returned", url);
			}

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var url = calculator.GetBaseRestUrl(branch);
				AssertEquals("When configuration is Sandbox, the Sandbox url should be returned", new Uri("https://sandbox-rest.avatax.com/api/v2/"), url);
			}

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Production)
			)
			{
				var url = calculator.GetBaseRestUrl(branch);
				AssertEquals("When configuration is Production, the Production url should be returned", new Uri("https://rest.avatax.com/api/v2/"), url);
			}
		}

		public void TestShouldSetSalesTaxOnPost()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var result = calculator.ShouldSetSalesTaxOnPost(null);
				AssertEquals("Null transaction should not set sales tax on post (duh!)", false, result);
				AssertExceptionThrown<ArgumentException>("SetSalesTaxLineItem() should throw when ShouldSetSalesTaxOnPost() returns false", () => calculator.SetSalesTaxLineItem(null, 1m));

				arInvoice.AH_OH = ZGuid.Empty;
				result = calculator.ShouldSetSalesTaxOnPost(arInvoice);
				AssertEquals("Transaction with no Debtor should not set sales tax on post", false, result);
				AssertExceptionThrown<ArgumentException>("SetSalesTaxLineItem() should throw when ShouldSetSalesTaxOnPost() returns false", () => calculator.SetSalesTaxLineItem(arInvoice, 1m));

				arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
				PopulateAUOrgAddressForMapping(TestObjectCreator.AALSHI);

				result = calculator.ShouldSetSalesTaxOnPost(arInvoice);
				AssertEquals("Transaction with Debtor outside of USA should not set sales tax on post", false, result);
				AssertExceptionThrown<ArgumentException>("SetSalesTaxLineItem() should throw when ShouldSetSalesTaxOnPost() returns false", () => calculator.SetSalesTaxLineItem(arInvoice, 1m));
			}

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty))
			{
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

				var result = calculator.ShouldSetSalesTaxOnPost(arInvoice);
				AssertEquals("When registry item is not configured should not set sales tax on post", false, result);
				AssertExceptionThrown<ArgumentException>("SetSalesTaxLineItem() should throw when ShouldSetSalesTaxOnPost() returns false", () => calculator.SetSalesTaxLineItem(arInvoice, 1m));
			}

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var result = calculator.ShouldSetSalesTaxOnPost(arInvoice);
				AssertEquals("AR INV should set sales tax on post", true, result);
				AssertNoExceptionThrown("SetSalesTaxLineItem() should not throw when ShouldSetSalesTaxOnPost() returns true", () => calculator.SetSalesTaxLineItem(arInvoice, 1m));

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				result = calculator.ShouldSetSalesTaxOnPost(arCreditNote);
				AssertEquals("AR CRD should set sales tax on post", true, result);
				AssertNoExceptionThrown("SetSalesTaxLineItem() should not throw when ShouldSetSalesTaxOnPost() returns true", () => calculator.SetSalesTaxLineItem(arCreditNote, 1m));

				var arAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("ARADJ001", 1m, 1m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
				result = calculator.ShouldSetSalesTaxOnPost(arAdjustmentNote);
				AssertEquals("AR ADJ should not set sales tax on post", false, result);
				AssertExceptionThrown<ArgumentException>("SetSalesTaxLineItem() should throw when ShouldSetSalesTaxOnPost() returns false", () => calculator.SetSalesTaxLineItem(arAdjustmentNote, 1m));

				var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("APINV001", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
				result = calculator.ShouldSetSalesTaxOnPost(apInvoice);
				AssertEquals("AP transactions should not set sales tax on post", false, result);
				AssertExceptionThrown<ArgumentException>("SetSalesTaxLineItem() should throw when ShouldSetSalesTaxOnPost() returns false", () => calculator.SetSalesTaxLineItem(apInvoice, 1m));
			}
		}

		public void TestGetTransactionDetailsForIssueManager()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			var actualDetails = calculator.GetTransactionDetailsForIssueManager(invoice);
			var expectedDetails =
@"Number: 
Debtor: 
Invoice Date: 
Transaction Type: INV
Total OS Amount: 0.00
Count of Non-Zero Lines: 0";

			invoice.AH_TransactionNum = "ARINV000111";
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_InvoiceDate = new ZDateTime(2022, 07, 23);
			actualDetails = calculator.GetTransactionDetailsForIssueManager(invoice);
			expectedDetails =
@"Number: ARINV000111
Debtor: AALSHI
Invoice Date: 23-Jul-22 00:00:00
Transaction Type: INV
Total OS Amount: 0.00
Count of Non-Zero Lines: 0";
			AssertEquals(expectedDetails, actualDetails);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_TransactionNum = "ARCRD000222";
			creditNote.AH_OH = TestObjectCreator.ABIGAS.PK;
			creditNote.AH_InvoiceDate = new ZDateTime(2022, 03, 2);
			var blankLine = creditNote.Lines.AddNew();
			var lineWithAmount = creditNote.Lines.AddNew();
			lineWithAmount.AL_OSExTaxAmount = 10.21m;
			actualDetails = calculator.GetTransactionDetailsForIssueManager(creditNote);
			expectedDetails =
@"Number: ARCRD000222
Debtor: ABIGAS
Invoice Date: 02-Mar-22 00:00:00
Transaction Type: CRD
Total OS Amount: 10.21
Count of Non-Zero Lines: 1";
			AssertEquals(expectedDetails, actualDetails);
		}

		#region MapTransactionToRequestJson() Tests

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_ARInvoice()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "935 Pennsylvania Avenue NW",
					line2: "Something for line 2",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20530");
				AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObjectCalculate = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJsonCalculate = JsonConvert.SerializeObject(mappedObjectCalculate, Formatting.Indented);
				var expectedJsonCalculate = ReadEmbeddedJsonResourceAsUtf8String("ARInvoiceCalculate.json");
				AssertMultilineASCIIEquals(expectedJsonCalculate, mappedJsonCalculate);

				var mappedObjectSubmit = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Submit);
				var mappedJsonSubmit = JsonConvert.SerializeObject(mappedObjectSubmit, Formatting.Indented);
				var expectedJsonSubmit = ReadEmbeddedJsonResourceAsUtf8String("ARInvoiceSubmit.json");
				AssertMultilineASCIIEquals(expectedJsonSubmit, mappedJsonSubmit);
			}
		}

		[TestDate(2022, 05, 02)]
		public void TestMapTransactionToRequestJson_ARCreditNote()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.ABIGAS, TestObjectCreator.USD, 1m, "AR Credit Note");
				arCreditNote.AH_Desc = "The description of the AR Credit Note";
				PopulateUSOrgAddressForMappingWithoutGeoCoordinates(TestObjectCreator.ABIGAS);
				AddLine(arCreditNote, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arCreditNote, TestObjectCreator.CC2, 6543.21m, 2);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObjectCalculate = calculator.MapTransactionToObject(arCreditNote, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJsonCalculate = JsonConvert.SerializeObject(mappedObjectCalculate, Formatting.Indented);
				var expectedJsonCalculate = ReadEmbeddedJsonResourceAsUtf8String("ARCreditNoteCalculate.json");
				AssertMultilineASCIIEquals(expectedJsonCalculate, mappedJsonCalculate);

				var mappedObjectSubmit = calculator.MapTransactionToObject(arCreditNote, AvalaraUSSalesTaxCalculator.MappingType.Submit);
				var mappedJsonSubmit = JsonConvert.SerializeObject(mappedObjectSubmit, Formatting.Indented);
				var expectedJsonSubmit = ReadEmbeddedJsonResourceAsUtf8String("ARCreditNoteSubmit.json");
				AssertMultilineASCIIEquals(expectedJsonSubmit, mappedJsonSubmit);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_ForSelfHostedLicense()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "935 Pennsylvania Avenue NW",
					line2: "Something for line 2",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20530");
				PopulateOrgLicenseForMapping(TestObjectCreator.AALSHI, "NCW");
				AddLine(arInvoice, TestObjectCreator.CC1, 1m, 1);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("SelfHostedLicense.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_ForWiseCloudLicense()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "935 Pennsylvania Avenue NW",
					line2: "Something for line 2",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20530");
				PopulateOrgLicenseForMapping(TestObjectCreator.AALSHI, "CHI");
				AddLine(arInvoice, TestObjectCreator.CC1, 1m, 1);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("WiseCloudLicense.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_ExcludesSalesTaxChargeCode()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC10.PK.ToGuid())
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "935 Pennsylvania Avenue NW",
					line2: "Something for line 2",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20530");
				AddLine(arInvoice, TestObjectCreator.CC1, 1m, 1);
				AddLine(arInvoice, TestObjectCreator.CC10, 2m, 2, description: "The nominated sales tax charge code should not be included in mapping");

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("ExcludeSalesTaxChargeCode.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_ExcludesZeroValueLines()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "935 Pennsylvania Avenue NW",
					line2: "Something for line 2",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20530");
				AddLine(arInvoice, TestObjectCreator.CC1, 1m, 1);
				AddLine(arInvoice, TestObjectCreator.CC2, 0m, 2, description: "Zero value lines are excluded, no matter what type of charge code");
				AddLine(arInvoice, TestObjectCreator.CommentChargeCode, 0m, 3, description: "Comment lines are excluded (because they are zero value)");
				AddLine(arInvoice, TestObjectCreator.CC3, 2m, 4);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("ZeroValueLines.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_FallsBackToGLNumberWhenNoChargeCode()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "935 Pennsylvania Avenue NW",
					line2: "Something for line 2",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20530");
				AddLine(arInvoice, TestObjectCreator.CashOnHandAccount, 1m, 1, description: "Direct GL Account, not charge code");

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("GLNumber.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_AddressIncludesLatitudeAndLongitude()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
					line1: "420 L'Enfant Plaza SW",
					city: "Washington",
					state: "DC",
					countryCode: "US",
					postCode: "20024",
					latitude: 38.88390m,
					longitude: -77.02561m);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("LatitudeAndLongitude.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_CRDWithOriginalReferenceInvoice()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				PopulateUSOrgAddressForMappingWithoutGeoCoordinates(TestObjectCreator.AALSHI);
				arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
				arInvoice.AH_Desc = "The description of the AR Invoice";
				arInvoice.AH_InvoiceDate = new ZDateTime(2022, 03, 21);
				AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);
				Factory.Save();

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.ABIGAS, TestObjectCreator.USD, 1m, "AR Credit Note");
				arCreditNote.OriginalTransactionReference = arInvoice.PK;

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arCreditNote, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("ARCreditNoteWithOriginalReference.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_CRDWithOriginalReferenceDate()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.ABIGAS, TestObjectCreator.USD, 1m, "AR Credit Note");
				arCreditNote.AH_Desc = "The description of the AR Credit Note";
				PopulateUSOrgAddressForMappingWithoutGeoCoordinates(TestObjectCreator.ABIGAS);
				AddLine(arCreditNote, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arCreditNote, TestObjectCreator.CC2, 6543.21m, 2);
				arCreditNote.AH_OriginalTransactionNum = "07219332";
				arCreditNote.AH_OriginalInvoiceDate = new ZDate(2022, 02, 14);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arCreditNote, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("ARCreditNoteWithOriginalDate.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_CRDWithOriginalReferenceDateOnly()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.ABIGAS, TestObjectCreator.USD, 1m, "AR Credit Note");
				arCreditNote.AH_Desc = "The description of the AR Credit Note";
				PopulateUSOrgAddressForMappingWithoutGeoCoordinates(TestObjectCreator.ABIGAS);
				AddLine(arCreditNote, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arCreditNote, TestObjectCreator.CC2, 6543.21m, 2);
				arCreditNote.AH_OriginalInvoiceDate = new ZDate(2022, 02, 13);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(arCreditNote, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("ARCreditNoteWithOriginalDateOnly.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMapTransactionToRequestJson_Reversal()
		{
			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "SAMPLECOMPANY")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				arInvoice.AH_InvoiceDate = new ZDateTime(2022, 03, 20);
				arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
				PopulateUSOrgAddressForMappingWithoutGeoCoordinates(TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);
				Factory.Save();

				var reversingFactory = new Accounting.Business.Base.Reversing.ReversingFactory();
				var reversing = reversingFactory.NewReversing(arInvoice);
				reversing.Reverse();
				var reversal = (InvoicingBase)reversing.ReverseTransaction;

				var calculator = new AvalaraUSSalesTaxCalculator();
				var mappedObject = calculator.MapTransactionToObject(reversal, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				var mappedJson = JsonConvert.SerializeObject(mappedObject, Formatting.Indented);
				var expectedJson = ReadEmbeddedJsonResourceAsUtf8String("ARReversal.json");
				AssertMultilineASCIIEquals(expectedJson, mappedJson);
			}
		}

		#endregion

		#region GetOrCreateHttpClient() Tests

		public void TestGetOrCreateHttpClient_ReturnsNullWhenConfiguredOff()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			var calculator = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Off)
			)
			{
				var httpClient = calculator.GetOrCreateHttpClient(branch);
				AssertNull("When configuration is Off, no http client should be returned", httpClient);
			}
		}

		public void TestGetOrCreateHttpClient_SetsAuthorizationCredentials()
		{
			var branch1 = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			var branch2 = TestObjectCreator.CreateBranchWithCompany("NZAKL");
			var calculator = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraAuthenticationSandboxUserId
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "1")
			)
			using (EDIDataRegistry.Instance.AvalaraAuthenticationSandboxPassword
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SandPass")
			)
			using (EDIDataRegistry.Instance.AvalaraAuthenticationProductionUserId
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "99999999")
			)
			using (EDIDataRegistry.Instance.AvalaraAuthenticationProductionPassword
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TheProductionPassword")
			)
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch1.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch2.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Production)
			)
			{
				var httpClientForBranch1 = calculator.GetOrCreateHttpClient(branch1);
				AssertNotNullOrEmpty("Authorization is set for Sandbox configured company", httpClientForBranch1.DefaultRequestHeaders.Authorization.ToString());

				var httpClientForBranch2 = calculator.GetOrCreateHttpClient(branch2);
				AssertNotNullOrEmpty("Authorization is set for Production configured company", httpClientForBranch2.DefaultRequestHeaders.Authorization.ToString());

				AssertNotEquals("Authorization is different for Sandbox vs Production companies", httpClientForBranch1.DefaultRequestHeaders.Authorization.ToString(), httpClientForBranch2.DefaultRequestHeaders.Authorization.ToString());
			}
		}

		public void TestGetOrCreateHttpClient_ReturnsSameInstanceWhenConfigured()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			var calculator1 = new AvalaraUSSalesTaxCalculator();
			var calculator2 = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var httpClientTheFirst = calculator1.GetOrCreateHttpClient(branch);
				var httpClientTheSecond = calculator1.GetOrCreateHttpClient(branch);
				AssertSame("Http client is cached for lifetime of AvalaraUSSalesTaxCalculator", httpClientTheFirst, httpClientTheSecond);

				var httpClientTheThird = calculator2.GetOrCreateHttpClient(branch);
				Assert("Http client is cached for within an AvalaraUSSalesTaxCalculator instance", !object.ReferenceEquals(httpClientTheFirst, httpClientTheThird));
			}
		}

		public void TestGetOrCreateHttpClient_ReturnsDifferentInstanceForProductionAndSandbox()
		{
			var branch1 = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			var branch2 = TestObjectCreator.CreateBranchWithCompany("NZAKL");
			var calculator = new AvalaraUSSalesTaxCalculator();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch1.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(branch2.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Production)
			)
			{
				var httpClientForSandbox1 = calculator.GetOrCreateHttpClient(branch1);
				var httpClientForSandbox2 = calculator.GetOrCreateHttpClient(branch1);
				AssertSame("Precondition: same http client for sandbox", httpClientForSandbox1, httpClientForSandbox2);

				var httpClientForProduction1 = calculator.GetOrCreateHttpClient(branch2);
				var httpClientForProduction2 = calculator.GetOrCreateHttpClient(branch2);
				AssertSame("Precondition: same http client for production", httpClientForProduction1, httpClientForProduction2);

				Assert("Http client is different for sandbox vs production configured companies", !object.ReferenceEquals(httpClientForSandbox1, httpClientForProduction1));
			}
		}

		[ExpectNoExceptions]
		public void TestGetOrCreateHttpClient_UsesTimeoutFromRegistry()
		{
			var expectedTimeout = TimeSpan.FromSeconds(5);
			using (EDIDataRegistry.Instance.AvalaraWebTimeoutSeconds
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (decimal)expectedTimeout.TotalSeconds)
			)
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var httpClient = new HttpClient();
				var mockHttpFactory = new Mock<IHttpClientFactory>();
				mockHttpFactory.Setup(x => x.Create(It.IsAny<TimeSpan>())).Returns(httpClient);
				using (ObjectFactory.Substitute(mockHttpFactory.Object))
				{
					var calculator = new AvalaraUSSalesTaxCalculator();
					calculator.GetOrCreateHttpClient(GlbBranch.CurrentBranch);
					mockHttpFactory.Verify(x => x.Create(expectedTimeout), "IHttpClient.Create(TimeSpan) should be called with argument " + expectedTimeout);
					mockHttpFactory.VerifyNoOtherCalls();
				}
			}
		}

		#endregion

		#region MapResponseToResultObject() Tests

		public void TestMapResponseToResultObject_ReturnsNetworkException()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var webException = new WebException("Boom!");
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var prettyJson = ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json");

			var (result, error) = calculator.MapResponseToResultObject(webException, url, response, prettyJson, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertNull("When a network exception occurs, no result should be returned", result);
			AssertSame("When a network exception occurs, it should be returned", webException, error);
		}

		public void TestMapResponseToResultObject_ReturnsExceptionWhenUnsuccessful_ValidErrorJson()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			var jsonResponse = ReadEmbeddedJsonResourceAsUtf8String("InvalidResponseError.json");

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertNull("When invalid response, no result should be returned", result);
			CombineAssertions("Error from web service is indicated by an exception object", () =>
			{
				AssertNotNull(error);
				AssertType<WebException>(error);
				AssertEquals("ValueRequiredError - Field customerCode is required. (400 Bad Request).", error.Message);
			});
		}

		public void TestMapResponseToResultObject_ReturnsExceptionWhenUnsuccessful_InvalidErrorJson()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			var jsonResponse = "<html><body>this is not JSON</body></html>";

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertNull("When invalid response, no result should be returned", result);
			CombineAssertions("Error from web service is indicated by an exception object", () =>
			{
				AssertNotNull(error);
				AssertType<WebException>(error);
				AssertEquals("400 Bad Request.", error.Message);
			});
		}

		public void TestMapResponseToResultObject_ReturnsExceptionWhenUnsuccessful_IncompleteErrorJson()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			var jsonResponse = ReadEmbeddedJsonResourceAsUtf8String("InvalidResponseBrokenError.json");

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertNull("When invalid response, no result should be returned", result);
			CombineAssertions("Error from web service is indicated by an exception object", () =>
			{
				AssertNotNull(error);
				AssertType<WebException>(error);
				AssertEquals("400 Bad Request.", error.Message);
			});
		}

		public void TestMapResponseToResultObject_ReturnsExceptionWhenInvalidResponseJson()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var jsonResponse = "<html><body>this is not JSON</body></html>";

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertNull("When invalid response, no result should be returned", result);
			CombineAssertions("Unparsable JSON from web service is indicated by an exception object", () =>
			{
				AssertNotNull(error);
				AssertType<JsonReaderException>(error);
				AssertEquals("Unexpected character encountered while parsing value: <. Path '', line 0, position 0.", error.Message);
			});
		}

		public void TestMapResponseToResultObject_ReturnsExceptionWhenMissingTotalTax()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var jsonResponse = ReadEmbeddedJsonResourceAsUtf8String("InvalidResponseMissingTotalTax.json");

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertNull("When invalid response, no result should be returned", result);
			CombineAssertions("Missing field is indicated by an exception object", () =>
			{
				AssertNotNull(error);
				AssertType<WebException>(error);
				AssertEquals("Web response is missing 'totalTax' field.", error.Message);
			});
		}

		public void TestMapResponseToResultObject_ReturnsCalculationResultForSalesTaxOnly_Calculate()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var jsonResponse = ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json");

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertEquals("When successful response, the sales tax amount should be returned", 123.45m, result.TotalSalesTaxAmount);
			AssertEquals("The invoice amount should always be zero", 0m, result.TotalInvoiceAmountExcludingSalesTax);
			AssertEquals("The warning message should be empty when no messages are returned in response", string.Empty, result.WarningMessage);
			AssertEquals("The submission status should always be empty for MappingType Calculate", string.Empty, result.SubmissionStatus);
			AssertNull("No errors", error);
		}

		public void TestMapResponseToResultObject_ReturnsCalculationResultForSalesTaxOnly_Submit()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var jsonResponse = ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json");

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Submit);
			AssertEquals("When successful response, the sales tax amount should be returned", 123.45m, result.TotalSalesTaxAmount);
			AssertEquals("The invoice amount should always be zero", 0m, result.TotalInvoiceAmountExcludingSalesTax);
			AssertEquals("The warning message should be empty when no messages are returned in response", string.Empty, result.WarningMessage);
			AssertEquals("The submission status should always be set for MappingType Submit", "Committed", result.SubmissionStatus);
			AssertNull("No errors", error);
		}

		public void TestMapResponseToResultObject_ReturnsWarningMessageWhenMessagesInResponse()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var url = new Uri("https://some.server.com/api/transactions");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var jsonResponse = ReadEmbeddedJsonResourceAsUtf8String("ValidResponseWithMessages.json");

			var (result, error) = calculator.MapResponseToResultObject(null, url, response, jsonResponse, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertEquals("When successful response, the sales tax amount should be returned", 123.45m, result.TotalSalesTaxAmount);
			AssertEquals("The invoice amount should always be zero", 0m, result.TotalInvoiceAmountExcludingSalesTax);
			var expectedWarning = @"No HSCode provided. Import Duty could not be calculated.
Something went badly wrong.";
			AssertEquals("The warning message should be set when messages are returned in response", expectedWarning, result.WarningMessage);
			AssertNull("No errors", error);
		}

		public void TestMapResponseToResultObject_AlwaysTracesDetail()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			var webException = new WebException("Boom!");
			AssertEquals("Precondition: no trace messages", 0, DummyTracer.Traces.Count);

			try
			{
				calculator.MapResponseToResultObject(webException, null, null, null, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				CombineAssertions("Network errors are logged to Tracer", () =>
				{
					AssertEquals(1, DummyTracer.Traces.Count);
					AssertContains("WebException", string.Join("", DummyTracer.Traces));
				});

				var url = new Uri("https://some.server.com/api/transactions");
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				var prettyJson = ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json");
				calculator.MapResponseToResultObject(null, url, response, prettyJson, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				CombineAssertions("Successful responses are logged to Tracer", () =>
				{
					AssertEquals(2, DummyTracer.Traces.Count);
					AssertContains("https://some.server.com/api/transactions", string.Join("", DummyTracer.Traces));
				});
				url = new Uri("https://api.other.server.com/v1/invoices");
				response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
				calculator.MapResponseToResultObject(null, url, response, prettyJson, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				CombineAssertions("Error responses are logged to Tracer", () =>
				{
					AssertEquals(3, DummyTracer.Traces.Count);
					AssertContains("https://api.other.server.com/v1/invoices", string.Join("", DummyTracer.Traces));
				});
			}
			finally
			{
				DummyTracer.Traces.Clear();
			}
		}

		#endregion

		#region GetValidationError() Tests

		public void TestGetValidationError_WhenNotEnabled()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);

			var calculator = new AvalaraUSSalesTaxCalculator();
			var error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
			AssertType<ApplicationException>(error);
			AssertEquals("Avalara integration for branch/company BNE/EDI is not enabled. Please see registry WiseTech Global Client Extensions -> Avalara US Sales Tax Integration -> Integration Setting Status.", error.Message);
		}

		public void TestGetValidationError_SubmitRequiresTransactionInDatabase()
		{
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

				var calculator = new AvalaraUSSalesTaxCalculator();
				var error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);

				AssertNull("Calculate should be valid for posted or unposted transactions", error);

				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Submit);

				AssertType<InvalidOperationException>(error);
				AssertEquals("Transaction must be posted before US Sales Tax can be committed.", error.Message);

				Factory.Save();
				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Submit);

				AssertNull("Submit should be valid for posted transactions only", error);
			}
		}

		public void TestGetValidationError_RequiresValidDebtorWithUSMainOfficeAddress()
		{
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_OH = Guid.Empty;
				Factory.Save();

				var calculator = new AvalaraUSSalesTaxCalculator();
				var error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				AssertType<System.IO.InvalidDataException>(error);
				AssertEquals("US Sales Tax cannot be calculated without a valid Debtor.", error.Message);

				arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
				PopulateAUOrgAddressForMapping(TestObjectCreator.AALSHI);

				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				AssertType<System.IO.InvalidDataException>(error);
				AssertEquals("US Sales Tax can only be calculated for Debtors with an active main office address in the US.", error.Message);

				var addr = TestObjectCreator.AALSHI.MainAddress;
				foreach (var a in TestObjectCreator.AALSHI.Addresses.Where(ad => ad.PK != addr.PK).ToList())
				{
					TestObjectCreator.AALSHI.Addresses.Remove(a);
				}
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				addr.OA_IsActive = false;

				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				AssertType<System.IO.InvalidDataException>(error);
				AssertEquals("US Sales Tax can only be calculated for Debtors with an active main office address in the US.", error.Message);
				addr.OA_IsActive = true;

				var mainOfficeAddressCapability = addr.AddressCapability.Cast<OrgAddressCapabilityWrapper>().First(a => a.Main == true && a.AddressCapabilityType == OrgAddressType.Office.Code);
				foreach (OrgAddressCapabilityWrapper ac in addr.AddressCapability)
				{
					ac.SetMainSilently(false);
					ac.AddressCapabilityType = ZString.Empty;
				}

				mainOfficeAddressCapability.SetMainSilently(false);
				mainOfficeAddressCapability.AddressCapabilityType = OrgAddressType.Office.Code;

				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				AssertType<System.IO.InvalidDataException>(error);
				AssertEquals("US Sales Tax can only be calculated for Debtors with an active main office address in the US.", error.Message);

				mainOfficeAddressCapability.SetMainSilently(true);
				mainOfficeAddressCapability.AddressCapabilityType = ZString.Empty;

				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				AssertType<System.IO.InvalidDataException>(error);
				AssertEquals("US Sales Tax can only be calculated for Debtors with an active main office address in the US.", error.Message);

				mainOfficeAddressCapability.SetMainSilently(true);
				addr.AddressCapability[0].AddressCapabilityType = OrgAddressType.Office.Code;

				error = calculator.GetValidationError(arInvoice, AvalaraUSSalesTaxCalculator.MappingType.Calculate);
				AssertNull(error);
			}
		}

		#endregion

		#region GetResponseValidation() Tests

		public void TestGetResponseValidation_Calculate_IsAlwaysValid()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			var result = new CalculationResult(1m);

			var calculator = new AvalaraUSSalesTaxCalculator();
			var error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Calculate);

			AssertNull("There should be no response validation rules for Calculate mapping", error);
		}

		public void TestGetResponseValidation_Submit_RequiresSameSalesTaxOnTransactionAndResponse()
		{
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC2.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC2, 16.34m, 1, description: "Sales Tax");
				var result = new CalculationResult(1m, submissionStatus: "Committed");

				var calculator = new AvalaraUSSalesTaxCalculator();
				var error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Submit);

				AssertNotNull(error);
				AssertEquals(error.Message, "Mismatch between current sales tax on transaction (16.34) and Avalara committed sales tax (1.00), difference of -15.34.");

				result = new CalculationResult(20.11m, submissionStatus: "Committed");
				error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Submit);

				AssertNotNull(error);
				AssertEquals(error.Message, "Mismatch between current sales tax on transaction (16.34) and Avalara committed sales tax (20.11), difference of 3.77.");

				result = new CalculationResult(16.34m, submissionStatus: "Committed");
				error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Submit);

				AssertNull(error);
			}
		}

		public void TestGetResponseValidation_Submit_RequiresCommittedStatus()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			var result = new CalculationResult(0m, submissionStatus: "");

			var calculator = new AvalaraUSSalesTaxCalculator();
			var error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Submit);

			AssertNotNull(error);
			AssertEquals("Avalara returned successful web response, but status was ''; expected 'Committed'.", error.Message);

			result = new CalculationResult(0m, submissionStatus: "Something");
			error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Submit);

			AssertNotNull(error);
			AssertEquals("Avalara returned successful web response, but status was 'Something'; expected 'Committed'.", error.Message);

			result = new CalculationResult(0m, submissionStatus: "Committed");
			error = calculator.GetResponseValidation(arInvoice, result, AvalaraUSSalesTaxCalculator.MappingType.Submit);

			AssertNull(error);
		}

		#endregion

		#region SetSalesTaxLineItem() Tests

		public void TestSetSalesTaxLineItem_NoSalesTaxLine_PositiveAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				calculator.SetSalesTaxLineItem(arInvoice, 1.8m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arInvoice, TestObjectCreator.CC3, 1.8m, 1.8m);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				calculator.SetSalesTaxLineItem(arCreditNote, 2.9m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3, -2.9m, 2.9m);
			}
		}

		public void TestSetSalesTaxLineItem_NoSalesTaxLine_NegativeAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				calculator.SetSalesTaxLineItem(arInvoice, -1.8m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arInvoice, TestObjectCreator.CC3, -1.8m, -1.8m);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				calculator.SetSalesTaxLineItem(arCreditNote, -2.9m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3, 2.9m, -2.9m);
			}
		}

		public void TestSetSalesTaxLineItem_OneSalesTaxLine_ZeroAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arInvoice, 0m);
				AssertZeroSalesTaxLinesAfterSetter(calculator, arInvoice, TestObjectCreator.CC3);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arCreditNote, 0m);
				AssertZeroSalesTaxLinesAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3);
			}
		}

		public void TestSetSalesTaxLineItem_OneSalesTaxLine_PositiveAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arInvoice, 5.1m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arInvoice, TestObjectCreator.CC3, 5.1m, 5.1m);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arCreditNote, 7.2m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3, -7.2m, 7.2m);
			}
		}

		public void TestSetSalesTaxLineItem_OneSalesTaxLine_NegativeAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arInvoice, -5.1m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arInvoice, TestObjectCreator.CC3, -5.1m, -5.1m);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arCreditNote, -7.2m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3, 7.2m, -7.2m);
			}
		}

		public void TestSetSalesTaxLineItem_OneSalesTaxLine_ExactAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arInvoice, 12m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arInvoice, TestObjectCreator.CC3, 12m, 12m);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arCreditNote, -12m);
				AssertSingleSalesTaxLineAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3, 12m, -12m);
			}
		}

		public void TestSetSalesTaxLineItem_TwoSalesTaxLines_ZeroAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arInvoice, TestObjectCreator.CC3, 13m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arInvoice, 0m);
				AssertZeroSalesTaxLinesAfterSetter(calculator, arInvoice, TestObjectCreator.CC3);

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arCreditNote, TestObjectCreator.CC3, 13m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arCreditNote, 0m);
				AssertZeroSalesTaxLinesAfterSetter(calculator, arCreditNote, TestObjectCreator.CC3);
			}
		}

		public void TestSetSalesTaxLineItem_TwoSalesTaxLines_LargerAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arInvoice, TestObjectCreator.CC3, 13m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arInvoice, 50m);
				var allLines = arInvoice.Lines.Cast<InvoicingLineBase>().Select(l => new { l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				var expectedLines = new[]
				{
					new { AL_OSExTaxAmount = (ZDecimal)12m, AL_Sequence = (ZShort)1 },
					new { AL_OSExTaxAmount = (ZDecimal)13m, AL_Sequence = (ZShort)2 },
					new { AL_OSExTaxAmount = (ZDecimal)25m, AL_Sequence = (ZShort)3 },
				};
				AssertContainsExactElementsInExactOrder("When (due to reasons beyond our control) many existing tax lines are on the invoice, a new balancing tax line should be added", expectedLines, allLines);
				AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", 50m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arCreditNote, TestObjectCreator.CC3, 13m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arCreditNote, -50m);
				allLines = arCreditNote.Lines.Cast<InvoicingLineBase>().Select(l => new { l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				expectedLines = new[]
				{
					new { AL_OSExTaxAmount = (ZDecimal)12m, AL_Sequence = (ZShort)1 },
					new { AL_OSExTaxAmount = (ZDecimal)13m, AL_Sequence = (ZShort)2 },
					new { AL_OSExTaxAmount = (ZDecimal)25m, AL_Sequence = (ZShort)3 },
				};
				AssertContainsExactElementsInExactOrder("When (due to reasons beyond our control) many existing tax lines are on the invoice, a new balancing tax line should be added", expectedLines, allLines);
				AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", -50m, calculator.GetCurrentSalesTaxAmount(arCreditNote));
			}
		}

		public void TestSetSalesTaxLineItem_TwoSalesTaxLines_SmallerAmount()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arInvoice, TestObjectCreator.CC3, 13m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arInvoice, 5m);
				var allLines = arInvoice.Lines.Cast<InvoicingLineBase>().Select(l => new { l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				var expectedLines = new[]
				{
					new { AL_OSExTaxAmount = (ZDecimal)12m,    AL_Sequence = (ZShort)1 },
					new { AL_OSExTaxAmount = (ZDecimal)13m,    AL_Sequence = (ZShort)2 },
					new { AL_OSExTaxAmount = (ZDecimal)(-20m), AL_Sequence = (ZShort)3 },
				};
				AssertContainsExactElementsInExactOrder("When (due to reasons beyond our control) many existing tax lines are on the invoice, a new balancing tax line should be added", expectedLines, allLines);
				AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", 5m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arCreditNote, TestObjectCreator.CC3, 13m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arCreditNote, -5m);
				allLines = arCreditNote.Lines.Cast<InvoicingLineBase>().Select(l => new { l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				expectedLines = new[]
				{
					new { AL_OSExTaxAmount = (ZDecimal)12m,    AL_Sequence = (ZShort)1 },
					new { AL_OSExTaxAmount = (ZDecimal)13m,    AL_Sequence = (ZShort)2 },
					new { AL_OSExTaxAmount = (ZDecimal)(-20m), AL_Sequence = (ZShort)3 },
				};
				AssertContainsExactElementsInExactOrder("When (due to reasons beyond our control) many existing tax lines are on the invoice, a new balancing tax line should be added", expectedLines, allLines);
				AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", -5m, calculator.GetCurrentSalesTaxAmount(arCreditNote));
			}
		}

		public void TestSetSalesTaxLineItem_TwoSalesTaxLines_SumToZero()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arInvoice, TestObjectCreator.CC3, -12m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arInvoice, 5m);
				var allLines = arInvoice.Lines.Cast<InvoicingLineBase>().Select(l => new { l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				var expectedLines = new[]
				{
					new { AL_OSExTaxAmount = (ZDecimal)12m,    AL_Sequence = (ZShort)1 },
					new { AL_OSExTaxAmount = (ZDecimal)(-12m), AL_Sequence = (ZShort)2 },
					new { AL_OSExTaxAmount = (ZDecimal)5m,     AL_Sequence = (ZShort)3 },
				};
				AssertContainsExactElementsInExactOrder("When (due to reasons beyond our control) many existing tax lines are on the invoice, a new balancing tax line should be added", expectedLines, allLines);
				AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", 5m, calculator.GetCurrentSalesTaxAmount(arInvoice));

				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				AddLine(arCreditNote, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line 1");
				AddLine(arCreditNote, TestObjectCreator.CC3, -12m, 2, description: "Pre-existing sales tax line 2");

				calculator.SetSalesTaxLineItem(arCreditNote, -5m);
				allLines = arCreditNote.Lines.Cast<InvoicingLineBase>().Select(l => new { l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				expectedLines = new[]
				{
					new { AL_OSExTaxAmount = (ZDecimal)12m,    AL_Sequence = (ZShort)1 },
					new { AL_OSExTaxAmount = (ZDecimal)(-12m), AL_Sequence = (ZShort)2 },
					new { AL_OSExTaxAmount = (ZDecimal)5m,     AL_Sequence = (ZShort)3 },
				};
				AssertContainsExactElementsInExactOrder("When (due to reasons beyond our control) many existing tax lines are on the invoice, a new balancing tax line should be added", expectedLines, allLines);
				AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", -5m, calculator.GetCurrentSalesTaxAmount(arCreditNote));
			}
		}

		public void TestSetSalesTaxLineItem_WithOtherLines()
		{
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			AddLine(arInvoice, TestObjectCreator.CC1, 13m, 1, description: "Some line item 1");
			AddLine(arInvoice, TestObjectCreator.CC2, 19m, 2, description: "Some line item 2");
			AddLine(arInvoice, TestObjectCreator.CC5, 23m, 3, description: "Some line item N");

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var calculator = new AvalaraUSSalesTaxCalculator();
				calculator.SetSalesTaxLineItem(arInvoice, 4.99m);

				AssertSingleSalesTaxLineAfterSetter(calculator, arInvoice, TestObjectCreator.CC3, 4.99m, 4.99m, expectedSequence: 4);

				var allLines = arInvoice.Lines.Cast<InvoicingLineBase>().Select(l => new { l.ChargeCode.AC_Code, l.AL_OSExTaxAmount, l.AL_Sequence }).ToArray();
				var expectedLines = new[]
				{
					new { AC_Code = (ZString)"ZZCC1", AL_OSExTaxAmount = (ZDecimal)13m,   AL_Sequence = (ZShort)1 },
					new { AC_Code = (ZString)"ZZCC2", AL_OSExTaxAmount = (ZDecimal)19m,   AL_Sequence = (ZShort)2 },
					new { AC_Code = (ZString)"ZZCC5", AL_OSExTaxAmount = (ZDecimal)23m,   AL_Sequence = (ZShort)3 },
					new { AC_Code = (ZString)"ZZCC3", AL_OSExTaxAmount = (ZDecimal)4.99m, AL_Sequence = (ZShort)4 },
				};
				AssertContainsExactElementsInExactOrder("Existing line items should not be removed or changed", expectedLines, allLines);
			}
		}

		public void TestSetSalesTaxLineItem_WithZeroAmountAndPreviousSalesTaxLine_DoesNotTriggerCriticalValidation()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);

			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC3, 12m, 1, description: "Pre-existing sales tax line");

				calculator.SetSalesTaxLineItem(arInvoice, 0m);
				AssertZeroSalesTaxLinesAfterSetter(calculator, arInvoice, TestObjectCreator.CC3);

				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		#region TestSetSalesTaxLineItem Assert Helpers

		static void AssertSingleSalesTaxLineAfterSetter(IUSSalesTaxCalculator calculator, InvoicingBase transaction, AccChargeCode chargeCode, decimal expectedOsExTaxAmount, decimal expectedTotalSalesTaxAmount, short expectedSequence = 1)
		{
			var salesTaxLine = transaction.Lines.Cast<InvoicingLineBase>().SingleOrDefault(l => l.AL_AC == chargeCode.PK);
			CombineAssertions("When non-zero amount is passed, there should be one line present to represent sales tax", () =>
			{
				AssertNotNull(salesTaxLine);
				AssertEquals(expectedOsExTaxAmount, salesTaxLine.AL_OSExTaxAmount);
				AssertEquals(expectedSequence, salesTaxLine.AL_Sequence);
			});
			AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", expectedTotalSalesTaxAmount, calculator.GetCurrentSalesTaxAmount(transaction));

			Assert("Line ReverseDate must not be empty", !salesTaxLine.AL_ReverseDate.IsEmpty);
			AssertEquals("Line PostDate should match Header PostDate", transaction.AH_PostDate, salesTaxLine.AL_PostDate);
			AssertEquals("Line ReverseDate should match Line PostDate for immediate revenue recognition", salesTaxLine.AL_PostDate, salesTaxLine.AL_ReverseDate);
			AssertEquals("Line Revenue Recognition should be immediate", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, salesTaxLine.AL_RevRecognitionType);
		}

		static void AssertZeroSalesTaxLinesAfterSetter(IUSSalesTaxCalculator calculator, InvoicingBase transaction, AccChargeCode chargeCode)
		{
			var salesTaxLineCount = transaction.Lines.Cast<InvoicingLineBase>().Count(l => l.AL_AC == chargeCode.PK);
			AssertEquals("When zero amount is passed, all sales tax line items should be removed", 0, salesTaxLineCount);
			AssertEquals("GetCurrentSalesTaxAmount() must always match value passed to SetSalesTaxLineItem()", 0m, calculator.GetCurrentSalesTaxAmount(transaction));
		}

		#endregion

		#endregion

		#region Tracing Function Tests

		public void TestRequestTraceDetail_PrettyPrintsValidJson()
		{
			var url = new Uri("https://api.blah.com/v2/transaction");
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			var prettyJson = ReadEmbeddedJsonResourceAsUtf8String("ARInvoiceCalculate.json");
			var minifiedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(prettyJson), Formatting.None);
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "V0lTRVRFQ0hHTE9CQUxBVVBUWUxURA");

			var traceString = AvalaraUSSalesTaxCalculator.RequestTraceDetail(url, request, minifiedJson, httpClient, "SomeUniqueIdentifier");
			AssertContains("INTERNAL ID: SomeUniqueIdentifier", traceString);
			AssertContains("URL: https://api.blah.com/v2/transaction", traceString);
			AssertContains("AUTH: Basic V0lTRVRFQ0hHTE9CQUxBVVBUWUxURA", traceString);
			AssertContains("METHOD: POST", traceString);
			AssertContains("REQUEST BODY:" + System.Environment.NewLine + prettyJson, traceString);
		}

		public void TestRequestTraceDetail_IncludesInvalidJson()
		{
			var url = new Uri("https://api.blah.com/v8/car");
			var request = new HttpRequestMessage(HttpMethod.Put, url);
			var notJson = "<html><head></head><p>this is not JSON</p></html>";
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "QUNDR1JFUEVS");

			var traceString = AvalaraUSSalesTaxCalculator.RequestTraceDetail(url, request, notJson, httpClient, "DifferentIdentifier");
			AssertContains("INTERNAL ID: DifferentIdentifier", traceString);
			AssertContains("URL: https://api.blah.com/v8/car", traceString);
			AssertContains("AUTH: Basic QUNDR1JFUEVS", traceString);
			AssertContains("METHOD: PUT", traceString);
			AssertContains("REQUEST BODY:" + System.Environment.NewLine + notJson, traceString);
		}

		public void TestResponseTraceDetail_PrettyPrintsValidJson()
		{
			var url = new Uri("https://api.blah.com/v2/transaction");
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var prettyJson = ReadEmbeddedJsonResourceAsUtf8String("ARInvoiceCalculate.json");
			var minifiedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(prettyJson), Formatting.None);

			var traceString = AvalaraUSSalesTaxCalculator.ResponseTraceDetail(url, response, minifiedJson, "AnyOldStringWillDo");
			AssertContains("INTERNAL ID: AnyOldStringWillDo", traceString);
			AssertContains("URL: https://api.blah.com/v2/transaction", traceString);
			AssertContains("RESPONSE CODE: 200 OK", traceString);
			AssertContains("RESPONSE BODY:" + System.Environment.NewLine + prettyJson, traceString);
		}

		public void TestResponseTraceDetail_IncludesInvalidJson()
		{
			var url = new Uri("https://api.blah.com/v8/car");
			var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
			var notJson = "<html><head></head><p>this is not JSON</p></html>";

			var traceString = AvalaraUSSalesTaxCalculator.ResponseTraceDetail(url, response, notJson, "");
			AssertContains("INTERNAL ID: ", traceString);
			AssertContains("URL: https://api.blah.com/v8/car", traceString);
			AssertContains("RESPONSE CODE: 503 Service Unavailable", traceString);
			AssertContains("RESPONSE BODY:" + System.Environment.NewLine + notJson, traceString);
		}

		public void TestNetworkErrorTraceDetail_ShowsFullExceptionDetail()
		{
			var url = new Uri("https://api.blah.com/v2/transaction");
			var ex = new WebException("Some web exception", new HttpRequestException("Some more detailed error message"));

			var traceString = AvalaraUSSalesTaxCalculator.NetworkErrorTraceDetail(url, ex, "08a20fbd-7c38-40e1-9203-c86284ce793c");
			AssertContains("INTERNAL ID: 08a20fbd-7c38-40e1-9203-c86284ce793c", traceString);
			AssertContains("URL: https://api.blah.com/v2/transaction", traceString);
			var expectedNetworkError = @"NETWORK ERROR:
System.Net.WebException: Some web exception ---> System.Net.Http.HttpRequestException: Some more detailed error message
   --- End of inner exception stack trace ---";
			AssertContains(expectedNetworkError, traceString);
		}

		#endregion

		#region Main Interface Tests CalculateSalesTax() and SubmitSalesTax() (integration tests)

		[DeveloperOnlyTest]
		public void TestCalculateSalesTax_WithRealWebRequest()
			=> RunRealWebRequest(
				(calculator, transaction) => calculator.CalculateSalesTax(transaction),
				"ARINV_CALC_" + ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")
			);

		[DeveloperOnlyTest]
		public void TestSubmitSalesTax_WithRealWebRequest()
			=> RunRealWebRequest(
				(calculator, transaction) => calculator.SubmitSalesTax(transaction),
				"ARINV_SUB_" + ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")
			);

		void RunRealWebRequest(
			Func<AvalaraUSSalesTaxCalculator, InvoicingBase, (CalculationResult, Exception)> mainMethod,
			string transactionNumber)
		{
			if (NUnit.Framework.TestingState.IsRunningOnDAT)
			{
				Assert("Live web requests must be run on developer workstations only", true);
				return;
			}

			using (EDIDataRegistry.Instance.AvalaraCompanyCode
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "WISETECHGLOBALAUPTYLTD")
			)
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraAuthenticationSandboxUserId
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "2001658914")
			)
			using (EDIDataRegistry.Instance.AvalaraAuthenticationSandboxPassword
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "B8BA8798EF8E9538")
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNumber, TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_Desc = "The description of the AR Invoice";
				arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);
				Factory.Save();

				var calculator = new AvalaraUSSalesTaxCalculator();
				var (result, error) = mainMethod(calculator, arInvoice);
				CombineAssertions(() =>
				{
					AssertNotNull("A successful web request should return a result", result);
					AssertNull("A successful web request should not return an error", error);
				});
			}
		}

		public void TestMainInterface_ReturnsValidationError()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Off)
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				var (result, error) = calculator.CalculateSalesTax(arInvoice);
				CombineAssertions(() =>
				{
					AssertNull("CalculateSalesTax(): Any error should not return a result", result);
					AssertType<ApplicationException>("CalculateSalesTax(): Errors should be returned as an exception object", error);
				});

				(result, error) = calculator.SubmitSalesTax(arInvoice);
				CombineAssertions(() =>
				{
					AssertNull("SubmitSalesTax(): Any error should not return a result", result);
					AssertType<ApplicationException>("SubmitSalesTax(): Errors should be returned as an exception object", error);
				});
			}
		}

		public void TestMainInterface_ReturnsZeroSalesTaxWithWarning_WhenNoMappedLineItems()
		{
			var calculator = new AvalaraUSSalesTaxCalculator();
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
				AddLine(arInvoice, TestObjectCreator.CommentChargeCode, 0m, 1, description: "Some comment line which won't be mapped because it has zero value");

				var (result, error) = calculator.CalculateSalesTax(arInvoice);
				CombineAssertions(() =>
				{
					AssertNull("CalculateSalesTax(): Should not return an error for zero tax result", error);
					AssertEquals("CalculateSalesTax(): Zero value invoice should have zero sales tax amount", 0m, result.TotalSalesTaxAmount);
					AssertEquals("CalculateSalesTax(): Zero value invoice is indicated in result", 0m, result.TotalInvoiceAmountExcludingSalesTax);
					AssertEquals("CalculateSalesTax(): Zero value invoice should include a warning", "No Line Items are eligible for transmission to Avalara as all mapped lines have zero amounts.", result.WarningMessage);
				});

				Factory.Save();
				(result, error) = calculator.SubmitSalesTax(arInvoice);
				CombineAssertions(() =>
				{
					AssertNull("SubmitSalesTax(): Should not return an error for zero tax result", error);
					AssertEquals("SubmitSalesTax(): Zero value invoice should have zero sales tax amount", 0m, result.TotalSalesTaxAmount);
					AssertEquals("SubmitSalesTax(): Zero value invoice is indicated in result", 0m, result.TotalInvoiceAmountExcludingSalesTax);
					AssertEquals("SubmitSalesTax(): Zero value invoice should include a warning", "No Line Items are eligible for transmission to Avalara as all mapped lines have zero amounts.", result.WarningMessage);
				});
			}
		}

		public void TestMainInterface_ReturnsNetworkRelatedExceptions()
		{
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC1, 1m, 1);
				Factory.Save();

				var handleableNetworkExceptions = new (Exception actual, Type expected)[]
				{
					(new TaskCanceledException(),                            typeof(TaskCanceledException)),
					(new TimeoutException(),                                 typeof(TimeoutException)),
					(new HttpRequestException(),                             typeof(HttpRequestException)),
					(new WebException(),                                     typeof(WebException)),
					(new WebException("The operation has timed out"), typeof(TimeoutException)),
				};

				foreach (var (actualEx, expectedType) in handleableNetworkExceptions)
				{
					var fakeHandler = TestHandler.CreateException(actualEx);
					using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
					{
						var calculator = new AvalaraUSSalesTaxCalculator();
						var (result, error) = calculator.CalculateSalesTax(arInvoice);
						CombineAssertions(() =>
						{
							AssertNull("CalculateSalesTax(): Any network error should not return a result", result);
							AssertType("CalculateSalesTax(): Network errors should be returned as an exception object", expectedType, error);
						});

						(result, error) = calculator.SubmitSalesTax(arInvoice);
						CombineAssertions(() =>
						{
							AssertNull("SubmitSalesTax(): Any network error should not return a result", result);
							AssertType("SubmitSalesTax(): Network errors should be returned as an exception object", expectedType, error);
						});
					}
				}
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMainInterface_WhenSuccessful()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			arInvoice.AH_Desc = "The description of the AR Invoice";
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
			AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
			AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);
			AddLine(arInvoice, TestObjectCreator.CC3, 123.45m, 3, description: "Sales Tax");
			Factory.Save();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var fakeHandler = TestHandler.CreateResponse(HttpStatusCode.OK, ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json"));
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
				{
					var calculator = new AvalaraUSSalesTaxCalculator();
					var (result, error) = calculator.CalculateSalesTax(arInvoice);
					CombineAssertions("CalculateSalesTax(): Result detail", () =>
					{
						AssertNotNull("On success, a result object should be returned", result);
						AssertEquals("Result object sales tax amount should match response message", 123.45m, result.TotalSalesTaxAmount);
						AssertEquals("Result object sales tax amount should never return submission status for CalculateSalesTax()", string.Empty, result.SubmissionStatus);
						AssertNull("On success, no exception object should be returned", error);
					});

					CombineAssertions("CalculateSalesTax(): Web Request", () => AssertWebRequest("ExpectedWebRequestSuccessCalculate.json"));

					(result, error) = calculator.SubmitSalesTax(arInvoice);
					CombineAssertions("SubmitSalesTax(): Result detail", () =>
					{
						AssertNotNull("On success, a result object should be returned", result);
						AssertEquals("Result object sales tax amount should match response message", 123.45m, result.TotalSalesTaxAmount);
						AssertEquals("Result object sales tax amount should always return submission status for SubmitSalesTax()", "Committed", result.SubmissionStatus);
						AssertNull("On success, no exception object should be returned", error);
					});

					CombineAssertions("SubmitSalesTax(): Web Request", () => AssertWebRequest("ExpectedWebRequestSuccessSubmit.json"));
				}

				void AssertWebRequest(string expectedWebRequestFile)
				{
					AssertEquals("Web request method should always be POST", HttpMethod.Post, fakeHandler.LastRequestMethod);
					AssertEquals("Web request URL should match Sandbox", "https://sandbox-rest.avatax.com/api/v2/transactions/create", fakeHandler.LastRequestUrl?.ToString()?.ToLowerInvariant());
					var expectedWebRequest = ReadEmbeddedJsonResourceAsUtf8String(expectedWebRequestFile);
					var actualWebRequest = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(fakeHandler.LastRequestContent), Formatting.Indented);
					AssertMultilineASCIIEquals("Web request content should match expected", expectedWebRequest, actualWebRequest);
				}
			}
		}

		[TestDate(2022, 04, 28)]
		public void TestMainInterface_RunValidationAfterResponse()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			arInvoice.AH_Desc = "The description of the AR Invoice";
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
			AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
			AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);
			AddLine(arInvoice, TestObjectCreator.CC3, 1.23m, 3, description: "Sales Tax");
			Factory.Save();

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid()))
			{
				var fakeHandler = TestHandler.CreateResponse(HttpStatusCode.OK, ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json"));
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
				{
					var calculator = new AvalaraUSSalesTaxCalculator();
					var (result, error) = calculator.CalculateSalesTax(arInvoice);
					CombineAssertions("CalculateSalesTax(): Result detail", () =>
					{
						AssertNotNull("On success, a result object should be returned", result);
						AssertNull("On success, no exception object should be returned, as Calculate has no ", error);
					});

					(result, error) = calculator.SubmitSalesTax(arInvoice);
					CombineAssertions("SubmitSalesTax(): Result detail", () =>
					{
						AssertNull("On response validation failure, no result object should be returned", result);
						AssertNotNull("On response validation failure, an exception object should be returned", error);
					});
				}
			}
		}

		public void TestMainInterface_AlwaysTracesDetail()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
			AddLine(arInvoice, TestObjectCreator.CC3, 123.45m, 3, description: "Sales Tax");
			Factory.Save();

			var fakeHandler = TestHandler.CreateResponse(HttpStatusCode.OK, ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json"));
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
			{
				AssertEquals("Precondition: no trace messages", 0, DummyTracer.Traces.Count);

				try
				{
					var calculator = new AvalaraUSSalesTaxCalculator();

					calculator.CalculateSalesTax(arInvoice);
					AssertEquals("Two trace messages should be logged for CalculateSalesTax(): for request and response", 2, DummyTracer.Traces.Count);

					calculator.SubmitSalesTax(arInvoice);
					AssertEquals("Two trace messages should be logged for SubmitSalesTax(): for request and response", 4, DummyTracer.Traces.Count);
				}
				finally
				{
					DummyTracer.Traces.Clear();
				}
			}
		}

		public void TestPostARInvoice_WithSuccessfulResponse()
		{
			var fakeHandler = TestHandler.CreateResponse(HttpStatusCode.OK, ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json"));
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid())
			)
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
			{
				TestObjectCreator.CC3.AC_AT_GSTRate = TestObjectCreator.FREEVAT.PK;
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_PostDate = new ZDateTime(2022, 07, 02);
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				AddLine(arInvoice, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arInvoice, TestObjectCreator.CC2, 6543.21m, 2);
				Factory.Save();

				var allLines = arInvoice.Lines.Cast<InvoicingLineBase>()
					.Select(l => new
					{
						l.AL_OSAmount,
						l.ChargeCode.AC_Code,
						l.AL_Sequence,
						l.AL_ReverseDate,
						l.AL_RevRecognitionType
					})
					.ToArray();
				var expectedLines = new[]
				{
					new { AL_OSAmount = (ZDecimal)1234.56m, AC_Code = (ZString)"ZZCC1", AL_Sequence = (ZShort)1 },
					new { AL_OSAmount = (ZDecimal)6543.21m, AC_Code = (ZString)"ZZCC2", AL_Sequence = (ZShort)2 },
					new { AL_OSAmount = (ZDecimal)123.45m,  AC_Code = (ZString)"ZZCC3", AL_Sequence = (ZShort)3 },
				}.Select(x => new
				{
					x.AL_OSAmount,
					x.AC_Code,
					x.AL_Sequence,
					AL_ReverseDate = new ZDateTime(2022, 07, 02),
					AL_RevRecognitionType = (ZString)RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate,
				});
				AssertContainsExactElementsInExactOrder("Posting a US Debtor AR Invoice should automatically add a sales tax line", expectedLines, allLines);
			}
		}

		public void TestPostARCreditNote_WithSuccessfulResponse()
		{
			var fakeHandler = TestHandler.CreateResponse(HttpStatusCode.OK, ReadEmbeddedJsonResourceAsUtf8String("ValidResponseWithNegativeAmount.json"));
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid())
			)
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
			{
				TestObjectCreator.CC3.AC_AT_GSTRate = TestObjectCreator.FREEVAT.PK;
				var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "AR Credit Note");
				arCreditNote.AH_PostDate = new ZDateTime(2022, 07, 02);
				PopulateUSOrgAddressForMapping(TestObjectCreator.AALSHI);
				AddLine(arCreditNote, TestObjectCreator.CC1, 1234.56m, 1);
				AddLine(arCreditNote, TestObjectCreator.CC2, 6543.21m, 2);
				Factory.Save();

				var allLines = arCreditNote.Lines.Cast<InvoicingLineBase>()
					.Select(l => new
					{
						l.AL_OSAmount,
						l.ChargeCode.AC_Code,
						l.AL_Sequence,
						l.AL_ReverseDate,
						l.AL_RevRecognitionType
					})
					.ToArray();
				var expectedLines = new[]
				{
					new { AL_OSAmount = (ZDecimal)(-1234.56m), AC_Code = (ZString)"ZZCC1", AL_Sequence = (ZShort)1 },
					new { AL_OSAmount = (ZDecimal)(-6543.21m), AC_Code = (ZString)"ZZCC2", AL_Sequence = (ZShort)2 },
					new { AL_OSAmount = (ZDecimal)(-123.45m),  AC_Code = (ZString)"ZZCC3", AL_Sequence = (ZShort)3 },
				}.Select(x => new
				{
					x.AL_OSAmount,
					x.AC_Code,
					x.AL_Sequence,
					AL_ReverseDate = new ZDateTime(2022, 07, 02),
					AL_RevRecognitionType = (ZString)RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate,
				});
				AssertContainsExactElementsInExactOrder("Posting a US Debtor AR Credit Note should automatically add a sales tax line", expectedLines, allLines);
			}
		}

		#endregion

		#region Dispose() Tests

		public void TestDispose()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.USD, 1m, TestObjectCreator.AALSHI);
			AddLine(arInvoice, TestObjectCreator.CC1, 1m, 1);
			AddLine(arInvoice, TestObjectCreator.CC3, 123.45m, 2, description: "Sales Tax");
			PopulateOrgAddressForMapping(TestObjectCreator.AALSHI,
				line1: "935 Pennsylvania Avenue NW",
				line2: "Something for line 2",
				city: "Washington",
				state: "DC",
				countryCode: "US",
				postCode: "20530");
			Factory.Save();

			var fakeHandler = TestHandler.CreateResponse(HttpStatusCode.OK, ReadEmbeddedJsonResourceAsUtf8String("ValidResponse.json"));

			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			using (EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
				.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid())
			)
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => fakeHandler)))
			{
				var calculator = new AvalaraUSSalesTaxCalculator();
				var (_, error) = calculator.CalculateSalesTax(arInvoice);
				AssertNull(error);
				(_, error) = calculator.CalculateSalesTax(arInvoice);
				AssertNull(error);
				(_, error) = calculator.SubmitSalesTax(arInvoice);
				AssertNull(error);
				(_, error) = calculator.SubmitSalesTax(arInvoice);
				AssertNull(error);

				AssertNoExceptionThrown("Dispose() should execute successfully after one or more web requests", () => calculator.Dispose());
				AssertNoExceptionThrown("Dispose() should execute multiple times", () => calculator.Dispose());
				AssertExceptionThrown<ObjectDisposedException>("Exception should be thrown from CalculateSalesTax() after Dispose() is called", () => calculator.CalculateSalesTax(arInvoice));
				AssertExceptionThrown<ObjectDisposedException>("Exception should be thrown from SubmitSalesTax() after Dispose() is called", () => calculator.SubmitSalesTax(arInvoice));
			}
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator
			=> testObjectCreatorValue ?? (testObjectCreatorValue = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreatorValue;

		#region Helpers for Mapping / Invoice Data

		void PopulateOrgAddressForMapping(OrgHeader org,
			string line1 = null,
			string line2 = null,
			string city = null,
			string state = null,
			string countryCode = null,
			string postCode = null,
			decimal? latitude = null,
			decimal? longitude = null)
		{
			var address = org.Addresses.MainAddress;
			address.OA_Address1 = line1;
			address.OA_Address2 = line2;
			address.OA_City = city;
			address.OA_State = state;
			address.OA_RN_NKCountryCode = countryCode;
			address.OA_PostCode = postCode;
			if (latitude != null)
			{
				address.OA_Latitude = latitude.Value;
			}
			if (longitude != null)
			{
				address.OA_Longitude = longitude.Value;
			}
		}

		void PopulateUSOrgAddressForMapping(OrgHeader org) =>
			PopulateOrgAddressForMapping(org,
				line1: "1818 H St NW",
				city: "Washington",
				state: "DC",
				countryCode: "US",
				postCode: "20433",
				latitude: 38.89895m,
				longitude: -77.043036m);

		void PopulateUSOrgAddressForMappingWithoutGeoCoordinates(OrgHeader org) =>
			PopulateOrgAddressForMapping(org,
				line1: "1818 H St NW",
				city: "Washington",
				state: "DC",
				countryCode: "US",
				postCode: "20433");

		void PopulateAUOrgAddressForMapping(OrgHeader org) =>
			PopulateOrgAddressForMapping(org,
				line1: "74 O'Riordan St",
				city: "Alexandria",
				state: "NSW",
				countryCode: "AU",
				postCode: "2015",
				latitude: -33.916325m,
				longitude: 151.195151m);

		void PopulateOrgLicenseForMapping(OrgHeader org, string hostedLocation)
		{
			var ediOrg = (EDIOrgHeader)org;
			var licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnt.LE_OH = ediOrg.PK;

			var licCom = licEnt.Companies.AddNew();
			licCom.LC_OH = ediOrg.PK;
			licCom.LC_LE = licEnt.PK;

			var cw1PrdDb = licEnt.Databases.AddNew();
			cw1PrdDb.LD_Product = ProductTypes.Codes.CargoWiseOne;
			cw1PrdDb.LD_LicenceType = DatabaseTypes.Codes.Production;
			cw1PrdDb.LD_HostedLocation = hostedLocation;

			var cwnPrdDb = licEnt.Databases.AddNew();
			cwnPrdDb.LD_Product = ProductTypes.Codes.CargoWiseNext;
			cwnPrdDb.LD_LicenceType = DatabaseTypes.Codes.Production;
			cwnPrdDb.LD_HostedLocation = hostedLocation;

			var cgwPrdDb = licEnt.Databases.AddNew();
			cgwPrdDb.LD_Product = ProductTypes.Codes.CargoWise;
			cgwPrdDb.LD_LicenceType = DatabaseTypes.Codes.Production;
			cgwPrdDb.LD_HostedLocation = hostedLocation;

			var nonCw1Db = ediOrg.LicCompany.LicEnterprise.Databases.AddNew();
			nonCw1Db.LD_Product = ProductTypes.Codes.WTGInternal;
			nonCw1Db.LD_LicenceType = DatabaseTypes.Codes.Production;
			nonCw1Db.LD_HostedLocation = "XXX";

			var nonPrdDb = ediOrg.LicCompany.LicEnterprise.Databases.AddNew();
			nonPrdDb.LD_Product = ProductTypes.Codes.CargoWiseOne;
			nonPrdDb.LD_LicenceType = DatabaseTypes.Codes.Training;
			nonPrdDb.LD_HostedLocation = "XXX";

			var inactiveCw1Db = ediOrg.LicCompany.LicEnterprise.Databases.AddNew();
			inactiveCw1Db.LD_Product = ProductTypes.Codes.CargoWiseOne;
			inactiveCw1Db.LD_LicenceType = DatabaseTypes.Codes.Production;
			inactiveCw1Db.LD_HostedLocation = "XXX";
			inactiveCw1Db.LD_IsActive = false;
		}

		InvoicingLineBase AddLine(
			InvoicingBase transaction,
			ChargeCodeOrGLHeader genericCharge,
			decimal amount,
			short sequence,
			string description = null)
		{
			var lineType = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;

			var line = (InvoicingLineBase)transaction.Lines.AddNew();
			line.AL_LineType = lineType;
			line.GenericCharge = genericCharge.PK;
			line.AL_Desc = description ?? ("Description for " + genericCharge.Code);
			line.AL_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
			line.AL_OSExTaxAmount = amount;
			line.AL_OSTaxAmount = 0m;
			line.AL_Sequence = sequence;
			return line;
		}

		class ChargeCodeOrGLHeader
		{
			public readonly AccChargeCode ChargeCode;
			public readonly AccGLHeader GLHeader;

			ChargeCodeOrGLHeader(AccChargeCode chargeCode)
			{
				ChargeCode = chargeCode;
			}
			ChargeCodeOrGLHeader(AccGLHeader glHeader)
			{
				GLHeader = glHeader;
			}

			public ZGuid PK => ChargeCode?.PK
							?? GLHeader?.PK
							?? ZGuid.Empty;

			public ZString Code => ChargeCode?.AC_Code
								?? GLHeader?.AG_AccountNum
								?? ZString.Empty;

			public static implicit operator ChargeCodeOrGLHeader(AccChargeCode cc) => new ChargeCodeOrGLHeader(cc);
			public static implicit operator ChargeCodeOrGLHeader(AccGLHeader gl) => new ChargeCodeOrGLHeader(gl);
		}

		#endregion

		#region Helpers for Tracing

		DummyTracer DummyTracer;

		#endregion

		#region Helpers for HttpClient

		sealed class TestHandler : DelegatingHandler
		{
			TestHandler(HttpResponseMessage mockResponseMessage)
			{
				this.mockResponseMessage = mockResponseMessage;
			}
			TestHandler(Exception error)
			{
				this.mockError = error;
			}

			readonly HttpResponseMessage mockResponseMessage;
			readonly Exception mockError;

			public static TestHandler CreateResponse(HttpStatusCode statusCode, string content = null)
			{
				var response = new HttpResponseMessage
				{
					StatusCode = statusCode
				};

				if (!string.IsNullOrWhiteSpace(content))
				{
					response.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
				}

				return new TestHandler(response);
			}

			public static TestHandler CreateException(Exception ex)
			{
				return new TestHandler(ex);
			}

			public HttpMethod LastRequestMethod { get; private set; }
			public Uri LastRequestUrl { get; private set; }
			public string LastRequestContent { get; private set; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				LastRequestMethod = request.Method;
				LastRequestUrl = request.RequestUri;
				LastRequestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

				if (this.mockError != null)
				{
					return Task.FromException<HttpResponseMessage>(mockError);
				}
				else
				{
					return Task.FromResult(mockResponseMessage);
				}
			}
		}

		#endregion

		string ReadEmbeddedJsonResourceAsUtf8String(string testFile)
		{
			using (var stream = typeof(AvalaraUSSalesTaxCalculatorTest).Assembly.GetManifestResourceStream("ZClientEDI.Business.Test.Billing.USSalesTax.TestData." + testFile))
			using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8, false, 32 * 1024, true))
			{
				return reader.ReadToEnd().Trim().Replace("\t", "  ");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyTracer = new DummyTracer(AccountingTraceSourceCodes.Http);
			ObjectFactory.Substitute<ITracer>(DummyTracer);
		}

		#endregion
	}
}
