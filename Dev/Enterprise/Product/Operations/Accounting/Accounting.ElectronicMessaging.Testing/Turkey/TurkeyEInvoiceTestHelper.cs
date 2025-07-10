using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;
using static Enterprise.MasterFiles.Business.TurkeyOrgCusCodeInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class TurkeyEInvoiceTestHelper
	{
		public DisposableList SetUpForTestingEInvoicingTurkeyWithControlAccounts() => new DisposableList(new IDisposable[] {
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()),
			TestObjectCreator.SetTemporaryControlAccounts(),
			GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey),
			Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK)
		});

		public AccEInvoicingBatch CreateTestARInvoiceBatch(AccChargeCode chargeCode, string chargeDescription, ZGuid gstRatePK, string transactionNumber, decimal sellAmount, RefCurrency currency, bool isEArchive = false, bool hasPayment = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			SetDebtorAddressForTurkey(debtor);
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeDescription, currency, 1000, TestObjectCreator.Creditor1, currency, sellAmount, debtor);
			charge.JR_AT_SellGSTRate = gstRatePK;
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNumber, currency, 1m, debtor);
			arInvoice.AH_TransactionReference = "ABC2020000000001";
			var arInvoiceeDoc = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestInvoiceFile", "INV");
			var arInvoiceLine = TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK);
			arInvoice.Lines.Add(arInvoiceLine);
			var complienceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;

			SetCommonValues(hasPayment, arInvoice, complienceSubType);

			arInvoiceeDoc.IsPublished = true;
			Factory.Save();

			return CreateInvoiceBatch(arInvoice);
		}

		public AccEInvoicingBatch CreateTestARInvoiceBatch(AccChargeCode chargeCode, string chargeDescription, Guid gstRatePK, string transactionNumber, decimal sellAmount, RefCurrency currency, string invoiceDescription)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			SetDebtorAddressForTurkey(debtor);
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeDescription, TestObjectCreator.TRY, 1000, TestObjectCreator.Creditor1, currency, sellAmount, debtor);
			charge.JR_AT_SellGSTRate = gstRatePK;
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNumber, TestObjectCreator.TRY, 1m, debtor);
			arInvoice.AH_Desc = invoiceDescription;
			var arInvoiceLine = TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK);
			arInvoice.Lines.Add(arInvoiceLine);
			SetCommonValues(false, arInvoice, ComplianceSubTypeCodes.EIN);
			Factory.Save();

			return CreateInvoiceBatch(arInvoice);
		}

		public InvoicingBase CreateTestARInvoice(AccChargeCode chargeCode, string chargeDescription, ZGuid gstRatePK, string transactionNumber, decimal sellAmount, RefCurrency currency, bool isEArchive = false)
		{
			var exchangeRate = currency.Code != Constants.CurrencyCodes.Turkey ? 6m : 1m;
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			SetDebtorAddressForTurkey(debtor);
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeDescription, TestObjectCreator.TRY, 1000, TestObjectCreator.Creditor1, currency, sellAmount, debtor);
			charge.JR_AT_SellGSTRate = gstRatePK;
			charge.JR_OSSellExRate = exchangeRate;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNumber, currency, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";
			var complienceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoiceLine = TestObjectCreator.CreateRevenueLine(charge, invoice.PK);
			invoiceLine.AL_AT = gstRatePK;
			invoice.Lines.Add(invoiceLine);

			SetCommonValues(false, invoice, complienceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateTestAPCreditNote(AccChargeCode chargeCode, string chargeDescription, ZGuid gstRatePK, string transactionNumber, decimal returnAmount, RefCurrency currency, string complianceSubType, bool createVTE = false, bool createVTC = false, bool hasTaxMessage = false)
		{
			var exchangeRate = currency.Code != CurrencyCodes.Turkey ? 6m : 1m;
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR, createVTE: createVTE, createVTC: createVTC);
			SetDebtorAddressForTurkey(debtor);
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeDescription, TestObjectCreator.TRY, returnAmount, TestObjectCreator.Creditor1, currency, returnAmount, debtor);
			charge.JR_AT_CostGSTRate = gstRatePK;
			charge.JR_AT_SellGSTRate = gstRatePK;
			charge.JR_OSSellExRate = exchangeRate;
			Factory.Save();

			var creditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), transactionNumber, currency, 1m, debtor);
			creditNote.AH_TransactionReference = "ABC2020000000001";
			var creditNoteLine = CreateAPCreditNoteLine(charge, creditNote.PK);
			if (hasTaxMessage)
			{
				creditNoteLine.AL_A9_VATClass = TestObjectCreator.TaxMsg606.PK;
			}
			creditNoteLine.AL_AT = gstRatePK;
			creditNote.Lines.Add(creditNoteLine);

			creditNote.AH_OriginalTransactionNum = "ORG" + transactionNumber;
			creditNote.AH_OriginalInvoiceDate = (ZDate)creditNote.AH_InvoiceDate.AddDays(-5);

			SetCommonValues(false, creditNote, complianceSubType);
			Factory.Save();

			return creditNote;
		}

		APCreditNoteLine CreateAPCreditNoteLine(BaseCharge relatedCharge, ZGuid transactionHeaderPK)
		{
			var aPCreditNoteLine = Factory.New<APCreditNoteLine>();
			aPCreditNoteLine.AL_AH = transactionHeaderPK;
			aPCreditNoteLine.AL_JH = relatedCharge.JR_JH;
			aPCreditNoteLine.AL_PlaceOfSupply = relatedCharge.JR_CostPlaceOfSupply;
			aPCreditNoteLine.AL_AC = relatedCharge.JR_AC;
			aPCreditNoteLine.AL_Desc = relatedCharge.JR_Desc;
			aPCreditNoteLine.AL_OSExTaxAmount = relatedCharge.JR_OSCostAmt;
			aPCreditNoteLine.AL_GB = relatedCharge.JR_GB;
			aPCreditNoteLine.AL_GE = relatedCharge.JR_GE;
			aPCreditNoteLine.AL_RX_NKTransactionCurrency = relatedCharge.JR_RX_NKCostCurrency;
			aPCreditNoteLine.AL_AT = relatedCharge.JR_AT_CostGSTRate;
			if (relatedCharge.Accrual != null && !relatedCharge.Accrual.IsReversed)
			{
				relatedCharge.ReverseAccrual(ZDateTime.Now);
			}

			aPCreditNoteLine.AL_ExchangeRate = relatedCharge.JR_OSCostExRate;
			aPCreditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			return aPCreditNoteLine;
		}

		public InvoicingBase CreateTestARInvoiceWithCommentLine(AccChargeCode chargeCode, string chargeDescription, Guid gstRatePK, string transactionNumber, decimal sellAmount, RefCurrency currency, bool isEArchive = false, string complianceNumber = "")
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			SetDebtorAddressForTurkey(debtor);

			var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeDescription, TestObjectCreator.TRY, 1000, TestObjectCreator.Creditor1, currency, sellAmount, debtor);
			charge.JR_AT_SellGSTRate = gstRatePK;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNumber, currency, 1m, debtor);
			var complienceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			invoice.AH_TransactionReference = "ABC2020000000001";

			var invoiceLine = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job, TestObjectCreator.CC14, TestObjectCreator.TRY, 1, "InvoiceLine", 100m);
			TestObjectCreator.CreateJobCharge(invoiceLine, job, TestObjectCreator.CC14);
			invoice.Lines.Add(invoiceLine);

			var commentLine = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job, TestObjectCreator.CommentChargeCode, TestObjectCreator.TRY, 1, "CommentInvoiceLine", 100m);
			TestObjectCreator.CreateJobCharge(commentLine, job, TestObjectCreator.CommentChargeCode);
			invoice.Lines.Add(commentLine);
			Factory.Save();

			SetCommonValues(false, invoice, complienceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateMultilineForeignCurrencyInvoice(bool isEArchive = false, bool addCommentLines = false)
		{
			var job1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var forwardingShipment1 = (ForwardingShipment)job1.Parent;
			SetJobParentData(forwardingShipment1);

			var job2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var forwardingShipment2 = (ForwardingShipment)job2.Parent;
			SetJobParentDataDifferentValues(forwardingShipment2);

			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			debtor.CompanyData.OB_ARExternalDebtorCode = "Test-External-Debtor-Code";
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", TestObjectCreator.TRY, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			var invoiceLine1 = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job1, TestObjectCreator.CC14, TestObjectCreator.TRY, 1, "ARInvoiceLine1", 1543.95m);
			TestObjectCreator.CreateJobCharge(invoiceLine1, job1, TestObjectCreator.CC1);
			invoice.Lines.Add(invoiceLine1);

			var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job1, TestObjectCreator.CC14, TestObjectCreator.USD, 2, "ARInvoiceLine2", 2000m);
			TestObjectCreator.CreateJobCharge(invoiceLine2, job1, TestObjectCreator.CC1);
			invoice.Lines.Add(invoiceLine2);

			var invoiceLine3 = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job2, TestObjectCreator.CC14, TestObjectCreator.EUR, 3, "ARInvoiceLine3", 3000m);
			TestObjectCreator.CreateJobCharge(invoiceLine3, job2, TestObjectCreator.CC1);
			invoice.Lines.Add(invoiceLine3);

			if (addCommentLines)
			{
				var invoiceLine4 = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job2, TestObjectCreator.CommentChargeCode, TestObjectCreator.EUR, 0, "INVOICE NOTE 1", 0m);
				TestObjectCreator.CreateJobCharge(invoiceLine4, job2, TestObjectCreator.CommentChargeCode);
				invoice.Lines.Add(invoiceLine4);

				var invoiceLine5 = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job2, TestObjectCreator.CommentChargeCode, TestObjectCreator.EUR, 0, "INVOICE NOTE 2", 0m);
				TestObjectCreator.CreateJobCharge(invoiceLine5, job2, TestObjectCreator.CommentChargeCode);
				invoice.Lines.Add(invoiceLine5);

				var invoiceLine6 = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job2, TestObjectCreator.CommentChargeCode, TestObjectCreator.EUR, 0, "INVOICE NOTE 3", 0m);
				TestObjectCreator.CreateJobCharge(invoiceLine6, job2, TestObjectCreator.CommentChargeCode);
				invoice.Lines.Add(invoiceLine6);
			}

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateMultilineInvoiceWithVATandWithholdingandExempt(RefCurrency currency, decimal currencyRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2000m, TestObjectCreator.KDV18W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3000m, TestObjectCreator.KDV18W7.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1500m, TestObjectCreator.KDV8.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2500m, TestObjectCreator.KDV8W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3500m, TestObjectCreator.KDV8W3.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 4000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 4500m, TestObjectCreator.KDV18W7.PK));
			TestObjectCreator.GSTFREE1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg301.PK;
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine1", 4100m, TestObjectCreator.GSTFREE1.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine3", 4600m, TestObjectCreator.GSTFREE2.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateMultilineInvoiceWithVATandWithholding(RefCurrency currency, decimal currencyRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2000m, TestObjectCreator.KDV18W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3000m, TestObjectCreator.KDV18W7.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1500m, TestObjectCreator.KDV8.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2500m, TestObjectCreator.KDV8W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3500m, TestObjectCreator.KDV8W3.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 4000m, TestObjectCreator.KDV18.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateMixedMultilineInvoiceWithExemption(RefCurrency currency, decimal currencyRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 1500m, TestObjectCreator.KDV8.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 4000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine4", 4100m, TestObjectCreator.FREEVAT.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine5", 4600m, TestObjectCreator.GSTFREE1.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateMultilineInvoiceWithOnlyWithholding(RefCurrency currency, decimal currencyRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2000m, TestObjectCreator.KDV18W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3000m, TestObjectCreator.KDV18W7.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2500m, TestObjectCreator.KDV8W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3500m, TestObjectCreator.KDV8W3.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 4500m, TestObjectCreator.KDV18W7.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateMultilineInvoiceWithVATandDifferentWHTandEXEMPTandFREEVAT(RefCurrency currency, decimal currencyRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, 1m, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2000m, TestObjectCreator.KDV18W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3000m, TestObjectCreator.KDV18W7.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1500m, TestObjectCreator.KDV8.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine2", 2500m, TestObjectCreator.KDV8W5.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 3500m, TestObjectCreator.KDV8W3.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 4000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine3", 4500m, TestObjectCreator.KDV18W7.PK));
			TestObjectCreator.GSTFREE1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg301.PK;
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine1", 4100m, TestObjectCreator.GSTFREE1.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine3", 4600m, TestObjectCreator.GSTFREE2.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 1500m, TestObjectCreator.KDV8.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, currencyRate, "ARInvoiceLine1", 4000m, TestObjectCreator.KDV18.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine1", 4100m, TestObjectCreator.FREEVAT.PK));
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC1, currency, currencyRate, "ARInvoiceLine3", 4600m, TestObjectCreator.FREEVAT.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		RefContainer fContainerType20T1;
		public RefContainer ContainerType20T1
		{
			get
			{
				if (fContainerType20T1 == null)
				{
					fContainerType20T1 = Factory.New<RefContainer>();
					fContainerType20T1.RC_Code = "20T1";
					fContainerType20T1.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
				}
				return fContainerType20T1;
			}
		}

		RefContainer fContainerType40T2;
		public RefContainer ContainerType40T2
		{
			get
			{
				if (fContainerType40T2 == null)
				{
					fContainerType40T2 = Factory.New<RefContainer>();
					fContainerType40T2.RC_Code = "40T2";
					fContainerType40T2.RC_ContainerType = Core.Constants.ContainerTypes.OpenTop;
				}
				return fContainerType40T2;
			}
		}

		RefContainer fContainerType40T3;
		public RefContainer ContainerType40T3
		{
			get
			{
				if (fContainerType40T3 == null)
				{
					fContainerType40T3 = Factory.New<RefContainer>();
					fContainerType40T3.RC_Code = "40T3";
					fContainerType40T3.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
				}
				return fContainerType40T3;
			}
		}

		void SetJobParentData(ForwardingShipment shipment, string consolNumber = "C0001", bool setOrderItems = true, string orderItems = "ABC0001,DBC0002,EDB0003", bool attachOrder = false)
		{
			var consol = TestObjectCreator.CreateConsol("AUMEL", "AUMEL", consolNumber);
			consol.Shipments.Add(shipment);
			ForwardingContainer container1 = consol.Containers.AddNew();
			ForwardingContainer container2 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "ABCD1234564";
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;
			container1.JC_RC = ContainerType20T1.PK;

			container2.JC_ContainerNum = "DCBA2345675";
			container2.JC_ContainerMode = Constants.ContainerModes.LCL;
			container2.JC_RC = ContainerType40T2.PK;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUPER";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_Vessel = "MILLENIUM FALCON";

			shipment.JS_HouseBill = "HouseBill1";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_RL_NKDischargePort = "USORD";
			shipment.JS_RL_NKFreightRateDestination = "NLRTM";
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKHouseBillIssuePlace = "AUSYD";
			shipment.JS_RL_NKLoadPort = "AUPER";
			shipment.JS_RL_NKOrigin = "MXCAN";
			shipment.JS_RL_NKPlaceOfDischarge = "MXCAN";
			shipment.JS_RL_NKPlaceOfReceipt = "AUPER";
			shipment.JS_GoodsDescription = "PENCIL";

			shipment.JS_ActualWeight = 15265m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 536m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_OuterPacks = 1200;
			shipment.JS_F3_NKPackType = "CTN";
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_HouseBill = "ShipmentHouseBill";
			shipment.JS_TotalPackageCount = 2;
			shipment.JS_ActualChargeable = 1.7M;
			shipment.ConsigneePK = TestObjectCreator.DebtorTR.PK;
			shipment.ConsignorPK = TestObjectCreator.Debtor1.PK;

			shipment.Containers.Append(new ForwardingContainer[] { container1, container2 });

			if (setOrderItems)
			{
				shipment.DocsAndCartage.JP_OrderItemsAsString = orderItems;
			}
			if (attachOrder)
			{
				var order = shipment.AttachedOrders.AddNew();
				order.JD_OrderNumber = "ORDER001";
				order.JD_OrderDate = ZDateTime.Now.AddMonths(-1);
			}
		}

		void SetJobParentDataDifferentValues(ForwardingShipment shipment)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "AUSYD", "C0002");
			consol.Shipments.Add(shipment);
			ForwardingContainer container1 = consol.Containers.AddNew();
			ForwardingContainer container2 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "BCDD1234564";
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;
			container1.JC_RC = ContainerType40T2.PK;

			container2.JC_ContainerNum = "EDCA2345675";
			container2.JC_ContainerMode = Constants.ContainerModes.LCL;
			container2.JC_RC = ContainerType40T3.PK;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUPER";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_Vessel = "BLACK PEARL";

			shipment.JS_HouseBill = "HouseBill2";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKDischargePort = "AUSYD";
			shipment.JS_RL_NKFreightRateDestination = "USCHI";
			shipment.JS_RL_NKFreightRateOrigin = "CHBRL";
			shipment.JS_RL_NKHouseBillIssuePlace = "AUSYD";
			shipment.JS_RL_NKLoadPort = "FRCDG";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKPlaceOfDischarge = "HKHKC";
			shipment.JS_RL_NKPlaceOfReceipt = "FRCDG";
			shipment.JS_GoodsDescription = "NOTEBOOK";

			shipment.JS_ActualWeight = 1345m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 360m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_OuterPacks = 256;
			shipment.JS_F3_NKPackType = "CTN";
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_HouseBill = "ShipmentHouseBill";
			shipment.JS_TotalPackageCount = 5;
			shipment.JS_ActualChargeable = 2.5M;
			shipment.ConsigneePK = TestObjectCreator.DebtorTR.PK;
			shipment.ConsignorPK = TestObjectCreator.Debtor1.PK;

			shipment.Containers.Append(new ForwardingContainer[] { container1, container2 });
		}

		public InvoicingBase CreateForeignCurrencyInvoice(RefCurrency currency, decimal exhangeRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			SetJobParentData((ForwardingShipment)job.Parent);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, exhangeRate, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";

			var invoiceLine = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, exhangeRate, "ARInvoiceLine1", 1543.95m);
			TestObjectCreator.CreateJobCharge(invoiceLine, job, TestObjectCreator.CC1);
			invoice.Lines.Add(invoiceLine);

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateForeignCurrencyInvoiceWithWithholding(RefCurrency currency, decimal exhangeRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, exhangeRate, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, exhangeRate, "ARInvoiceLine", 2000m, TestObjectCreator.KDV18W5.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public InvoicingBase CreateForeignCurrencyInvoiceWithExemption(RefCurrency currency, decimal exhangeRate, bool isEArchive = false)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var debtor = AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR);
			var complianceSubType = isEArchive ? ComplianceSubTypeCodes.EAR : ComplianceSubTypeCodes.EIN;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", currency, exhangeRate, debtor);
			invoice.AH_TransactionReference = "ABC2020000000001";
			invoice.Lines.Add(TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, TestObjectCreator.CC14, currency, exhangeRate, "ARInvoiceLine", 2000m, TestObjectCreator.FREEVAT.PK));

			SetCommonValues(false, invoice, complianceSubType);
			Factory.Save();

			return invoice;
		}

		public void SetOrgHeaderAddressForTurkey(OrgHeader header)
		{
			header.OH_FullName = "Ulukom Test Company Name";
			header.MainAddress.OA_Address1 = "Reşitpaşa Mah. Katar Cad. İTÜ Ayazağa Kamüsü";
			header.MainAddress.OA_Address2 = "Teknokent ARI 1 Binası No:2/5/7 Maslak";
			header.MainAddress.OA_City = "Sarıyer";
			header.MainAddress.OA_State = "34";
			header.MainAddress.OA_Phone = "+90 333 222 11 00";
			header.MainAddress.OA_Fax = "+90 222 333 22 22";
			header.MainAddress.OA_Email = "istanbul@wisetech.com";
			header.MainAddress.OA_PostCode = "12345";
			header.OH_RL_NKClosestPort = "TRIST";
			header.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Turkey;
			header.MainWebURL.PU_URL = "www.istanbul-sariyer.com";
			header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			header.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			header.AddressForSendingARDocuments.OA_Email = "istanbul@wisetech.com";

			if (header.Contacts != null)
			{
				var orgContact = header.Contacts[0];
				if (orgContact == null)
				{
					orgContact = header.Contacts.AddNew();
					orgContact.OC_OH = header.PK;
				}
				orgContact.OC_Email = "istanbul@wisetech.com";
				orgContact.OC_Phone = "+90 333 222 11 00";
				orgContact.OC_Fax = "+90 222 333 22 22";
			}
		}

		public void SetDebtorAddressForTurkey(OrgHeader header, bool isGovernmentOrganisation = false)
		{
			header.OH_FullName = "Istanbul Debtor Test Company";
			header.MainAddress.OA_Address1 = "Mecidiyeköy Mah. 1. Taş Ocağı Cad.";
			header.MainAddress.OA_Address2 = "Burç Sok. No:10 Kat 4";
			header.MainAddress.OA_City = "Şişli";
			header.MainAddress.OA_State = "İstanbul";
			header.MainAddress.OA_Phone = "+90 212 212 26 92";
			header.MainAddress.OA_Fax = "+90 212 212 26 93";
			header.MainAddress.OA_Email = "sisli@istanbul.com";
			header.MainWebURL.PU_URL = "www.istanbul-sisli.com";
			header.OH_RL_NKClosestPort = "TRIST";
			header.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Turkey;
			header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			header.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			header.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			if (isGovernmentOrganisation)
			{
				header.OH_Category = OrgConstants.Category.Government;
			}
			header.AddressForSendingARDocuments.OA_Email = "sisli@istanbul.com";

			if (header.Contacts != null)
			{
				var orgContact = header.Contacts[0];
				if (orgContact == null)
				{
					orgContact = header.Contacts.AddNew();
					orgContact.OC_OH = header.PK;
				}
				orgContact.OC_Email = "sisli@istanbul.com";
				orgContact.OC_Phone = "+90 212 212 26 92";
				orgContact.OC_Fax = "+90 212 212 26 93";
			}
		}

		public InvoicingBase CreateARInvoiceWithShipment(OrgHeader orgTurkey, string invoiceRef = "AR001", string complianceSubType = ComplianceSubTypeCodes.EIN, bool createDebtorCustomeCodes = true, string consolNumber = "C0001")
		{
			return CreateARInvoiceWithShipment(orgTurkey, TestObjectCreator.TRY, invoiceRef, complianceSubType, createDebtorCustomeCodes, true, false, consolNumber);
		}

		public InvoicingBase CreateARInvoiceWithShipment(
			OrgHeader orgTurkey,
			RefCurrency refCurrency,
			string invoiceRef = "AR001",
			string complianceSubType = ComplianceSubTypeCodes.EIN,
			bool createDebtorCustomeCodes = true,
			bool createVTE = true,
			bool isGovernmentOrganization = false,
			string consolNumber = "C0001",
			bool addCommentCharges = false,
			bool setOrderItems = true,
			string orderItems = "ABC0001,DBC0002,EDB0003",
			bool attachOrder = false)
		{
			AddCustomsCodesForSender(TestObjectCreator.CreditorTR);

			var exchangeRate = refCurrency.Code != Constants.CurrencyCodes.Turkey ? 6m : 1m;
			TestObjectCreator.GSTFREE1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg301.PK;

			var debtor = createDebtorCustomeCodes ? AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR, createVTE, isGovernmentOrganization) : TestObjectCreator.DebtorTR;
			SetDebtorAddressForTurkey(debtor, isGovernmentOrganization);
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			SetJobParentData((ForwardingShipment)job.Parent, consolNumber, setOrderItems: setOrderItems, orderItems: orderItems, attachOrder: attachOrder);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC14, "charge1", refCurrency, 100.3m, orgTurkey, refCurrency, 100.3m, debtor);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC15, "charge2", refCurrency, 200.5m, orgTurkey, refCurrency, 200.5m, debtor);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC7, "charge3", refCurrency, 300.7m, orgTurkey, refCurrency, 300.7m, debtor);
			var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CommentChargeCode, "INVOICE NOTE 1", refCurrency, 0m, orgTurkey, refCurrency, 0m, debtor);
			var charge5 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CommentChargeCode, "INVOICE NOTE 2", refCurrency, 0m, orgTurkey, refCurrency, 0m, debtor);
			var charge6 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CommentChargeCode, "INVOICE NOTE 3", refCurrency, 0m, orgTurkey, refCurrency, 0m, debtor);
			charge1.JR_OSSellExRate = exchangeRate;
			charge1.JR_AT_SellGSTRate = TestObjectCreator.KDV18.PK;
			charge2.JR_OSSellExRate = exchangeRate;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.KDV8.PK;
			charge3.JR_OSSellExRate = exchangeRate;
			charge3.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge3.JR_A9_SellVATClass = TestObjectCreator.TaxMsg301.PK;
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceRef, refCurrency, exchangeRate, debtor);
			arInvoice.AH_TransactionReference = "ABC2020000000001";
			var arInvoiceLine1 = TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK);
			arInvoiceLine1.AL_Sequence = 1;

			var arInvoiceLine2 = TestObjectCreator.CreateRevenueLine(charge2, arInvoice.PK);
			arInvoiceLine2.AL_Sequence = 2;

			var arInvoiceLine3 = TestObjectCreator.CreateRevenueLine(charge3, arInvoice.PK);
			arInvoiceLine3.AL_Sequence = 3;
			arInvoice.Lines.Add(arInvoiceLine1);
			arInvoice.Lines.Add(arInvoiceLine2);
			arInvoice.Lines.Add(arInvoiceLine3);
			if (addCommentCharges)
			{
				var arInvoiceLine4 = TestObjectCreator.CreateRevenueLine(charge4, arInvoice.PK);
				arInvoiceLine4.AL_Sequence = 4;
				arInvoice.Lines.Add(arInvoiceLine4);
				var arInvoiceLine5 = TestObjectCreator.CreateRevenueLine(charge5, arInvoice.PK);
				arInvoiceLine5.AL_Sequence = 5;
				arInvoice.Lines.Add(arInvoiceLine5);
				var arInvoiceLine6 = TestObjectCreator.CreateRevenueLine(charge6, arInvoice.PK);
				arInvoiceLine6.AL_Sequence = 6;
				arInvoice.Lines.Add(arInvoiceLine6);
			}
			SetCommonValues(true, arInvoice, complianceSubType);

			var arInvoiceeDoc = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestInvoiceFile", "INV");

			arInvoiceeDoc.IsPublished = true;
			Factory.Save();

			return arInvoice;
		}

		public InvoicingBase CreateARInvoiceWithTax(OrgHeader orgTurkey, RefCurrency refCurrency, string invoiceRef = "AR001", string complianceSubType = ComplianceSubTypeCodes.EIN, bool createVTE = true, bool createVTP = true, bool createVTC = false)
		{
			AddCustomsCodesForSender(TestObjectCreator.CreditorTR);

			var exchangeRate = refCurrency.Code != Constants.CurrencyCodes.Turkey ? 6m : 1m;
			TestObjectCreator.GSTFREE1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg301.PK;

			var createDebtorCustomCodes = createVTE || createVTP || createVTC;

			var debtor = createDebtorCustomCodes ? AddCustomsCodesForDebtor(TestObjectCreator.DebtorTR, createVTE: createVTE, createVTP: createVTP, createVTC: createVTC) : TestObjectCreator.DebtorTR;
			SetDebtorAddressForTurkey(debtor);
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC14, "charge1", refCurrency, 100.3m, orgTurkey, refCurrency, 100.3m, debtor);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC15, "charge2", refCurrency, 200.5m, orgTurkey, refCurrency, 200.5m, debtor);
			charge1.JR_OSSellExRate = exchangeRate;
			charge1.JR_AT_SellGSTRate = TestObjectCreator.KDV18.PK;
			charge2.JR_OSSellExRate = exchangeRate;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.KDV8.PK;
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceRef, refCurrency, exchangeRate, debtor);
			arInvoice.AH_TransactionReference = "ABC2020000000001";
			var arInvoiceLine1 = TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK);
			arInvoiceLine1.AL_Sequence = 1;

			var arInvoiceLine2 = TestObjectCreator.CreateRevenueLine(charge2, arInvoice.PK);
			arInvoiceLine2.AL_Sequence = 2;

			arInvoice.Lines.Add(arInvoiceLine1);
			arInvoice.Lines.Add(arInvoiceLine2);
			SetCommonValues(true, arInvoice, complianceSubType);

			var arInvoiceeDoc = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestInvoiceFile", "INV");

			arInvoiceeDoc.IsPublished = true;
			Factory.Save();

			return arInvoice;
		}

		public AccEInvoicingBatch CreateInvoiceBatch(InvoicingBase invoice, int batchNumber = 1, string pivotActionType = EInvoicingPivotActionType.Submit, string governmentAllocatedNumber = "", string pivotState = EInvoicingPivotState.Batched)
		{
			var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(batchNumber, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			invoicingBatch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, invoice, pivotState, pivotActionType);
			Factory.Save();

			return invoicingBatch;
		}

		public AccEInvoicingBatch CreateBatch(int batchNumber = 1)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(batchNumber, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			Factory.Save();

			return batch;
		}

		public (AccEInvoicingBatch, AccEInvoicingTransactionPivot) CreateInvoiceBatchAndPivot(InvoicingBase invoice)
		{
			var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, invoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			return (invoicingBatch, pivot);
		}

		public AccEInvoicingBatch CreateMultilineTestARInvoiceBatch(AccChargeCode chargeCode, string chargeDescription, AccTaxRateCollection gstRatePK, string transactionNumber, decimal sellAmount, RefCurrency refCurrency)
		{
			var exchangeRate = refCurrency.Code != Constants.CurrencyCodes.Turkey ? 6m : 1m;
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNumber, refCurrency, exchangeRate, TestObjectCreator.DebtorTR);
			arInvoice.AH_TransactionReference = "ABC2020000000001";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var lines = new List<InvoicingLineBase>();

			foreach (var taxRate in gstRatePK)
			{
				var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeDescription, refCurrency, 1000, TestObjectCreator.Creditor1, refCurrency, sellAmount, TestObjectCreator.DebtorTR);
				charge.JR_AT_SellGSTRate = taxRate.PK;
				charge.JR_OSSellExRate = exchangeRate;
				var arInvoiceLine = TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK);
				arInvoiceLine.AL_AT = taxRate.PK;
				lines.Add(arInvoiceLine);
			}

			arInvoice.Lines.AddRange(lines);
			Factory.Save();

			return CreateInvoiceBatch(arInvoice);
		}

		public AccEInvoicingBatch CreateInvoiceWithExemption(RefCurrency refCurrency, bool includeTaxMessages = true)
		{
			var exchangeRate = refCurrency.Code != Constants.CurrencyCodes.Turkey ? 6m : 1m;
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC7, "charge3", TestObjectCreator.TRY, 5450m, TestObjectCreator.Creditor1, refCurrency, 5450m, TestObjectCreator.DebtorTR);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge.JR_OSSellExRate = exchangeRate;

			if (includeTaxMessages) // These are mandatory for Turkey, but this requirement could be turned off via registry.
			{
				TestObjectCreator.GSTFREE1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg606.PK;
				charge.JR_A9_SellVATClass = TestObjectCreator.TaxMsg606.PK;
			}

			Factory.Save();

			var arInvoiceWithExemption = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", refCurrency, 1m, TestObjectCreator.DebtorTR);
			arInvoiceWithExemption.AH_TransactionReference = "ABC2020000000001";
			arInvoiceWithExemption.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoiceWithExemption.PK));
			SetCommonValues(true, arInvoiceWithExemption, ComplianceSubTypeCodes.EIN);
			Factory.Save();

			return CreateInvoiceBatch(arInvoiceWithExemption);
		}

		public efatura.uyumsoft.com.tr.InvoiceInfo SetTransaction(TransactionBatch batch, TransactionInfo uInvoice)
		{
			batch.TransactionCollection.Clear();
			batch.TransactionCollection.Add(uInvoice);
			var invoiceInfoTag = new InvoiceInfoTag(uInvoice, GlbCompany.CurrentCompany);
			var eInvoice = invoiceInfoTag.BuildInvoiceInfo();

			return eInvoice;
		}

		public efatura.uyumsoft.com.tr.InvoiceInfo EInvoiceInitializer(TransactionBatch batch, bool setShipmentData = false, bool isMoreThanOneShipment = false, bool setShipmentWithOrderAndPickUpDate = false, string orderRef = null, ZDateTime? pickupDate = null)
		{
			var uInvoice = batch.TransactionCollection.First();
			if (setShipmentData)
			{
				if (setShipmentWithOrderAndPickUpDate)
				{
					SetShipmentWithOrderAndPickUpDate(uInvoice, orderRef, pickupDate);
				}
				else
				{
					SetTransactionShipment1(uInvoice);
					if (isMoreThanOneShipment)
					{
						SetTransactionShipment2(uInvoice);
					}
				}
			}
			var invoiceInfoTag = new InvoiceInfoTag(uInvoice, GlbCompany.CurrentCompany);
			var eInvoice = invoiceInfoTag.BuildInvoiceInfo();

			return eInvoice;
		}

		public void SetTransactionShipment1(TransactionInfo uInvoice)
		{
			var shipment = uInvoice.ShipmentCollection.FirstOrDefault();
			shipment.WayBillNumber = "MAWBNUMBER1";
			shipment.WayBillType = new WayBillType() { Code = "MWB", Description = "Master Waybill" };
			shipment.GoodsDescription = "GOODS DESCRIPTION 1";
			shipment.PortOfFirstArrival = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			shipment.PortOfDestination = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			shipment.PortOfOrigin = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			if (shipment.SubShipmentCollection != null)
			{
				var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
				subShipment.PortOfFirstArrival = new UNLOCO() { Code = "SGSIN", Name = "Singapore" };
				subShipment.PortOfDestination = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
				subShipment.PortOfOrigin = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
				subShipment.WayBillNumber = "800-43567890";
				subShipment.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
				subShipment.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm()
				{
					Code = "FOB",
					Description = "Free On Board"
				};
				subShipment.CarrierDocumentsOverride = new CarrierDocumentsOverride()
				{
					AWBHeader = new UniversalDataBuss.DataObjects.Universal.AWB.AWBHeader()
					{
						Consignee = new UniversalDataBuss.DataObjects.Universal.AWB.AWBParty() { Name = "CONSIGNEE 1" },
						Shipper = new UniversalDataBuss.DataObjects.Universal.AWB.AWBParty() { Name = "SHIPPER 1" },
						AWBIssueDate = ZDateTime.Now,
						AWBIssuePlace = "SYDNEY",
						AWBNumber = "AWBNumber 1"
					}
				};
			}
		}

		public void SetTransactionShipment2(TransactionInfo uInvoice)
		{
			var shipment = uInvoice.ShipmentCollection.LastOrDefault();
			shipment.WayBillNumber = "MAWBNUMBER2";
			shipment.WayBillType = new WayBillType() { Code = "MWB", Description = "Master Waybill" };
			shipment.GoodsDescription = "GOODS DESCRIPTION 2";
			shipment.PortOfFirstArrival = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			shipment.PortOfDestination = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			shipment.PortOfOrigin = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			if (shipment.SubShipmentCollection != null)
			{
				var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
				subShipment.PortOfFirstArrival = new UNLOCO() { Code = "MXCAN", Name = "Canatlan" };
				subShipment.PortOfDestination = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
				subShipment.PortOfOrigin = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
				subShipment.WayBillNumber = "695-12349990";
				subShipment.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
				subShipment.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm()
				{
					Code = "FOB",
					Description = "Free On Board"
				};
				subShipment.CarrierDocumentsOverride = new CarrierDocumentsOverride()
				{
					AWBHeader = new UniversalDataBuss.DataObjects.Universal.AWB.AWBHeader()
					{
						Consignee = new UniversalDataBuss.DataObjects.Universal.AWB.AWBParty() { Name = "CONSIGNEE 2" },
						Shipper = new UniversalDataBuss.DataObjects.Universal.AWB.AWBParty() { Name = "SHIPPER 2" },
						AWBIssueDate = ZDateTime.Now,
						AWBIssuePlace = "MELBOURNE",
						AWBNumber = "AWBNumber 2"
					}
				};
			}
		}

		public void SetShipmentWithOrderAndPickUpDate(TransactionInfo uInvoice, string orderRef, ZDateTime? pickupDate)
		{
			var shipment = uInvoice.ShipmentCollection.FirstOrDefault();
			shipment.WayBillNumber = "MAWBNUMBER1";
			shipment.WayBillType = new WayBillType() { Code = "MWB", Description = "Master Waybill" };
			shipment.GoodsDescription = "GOODS DESCRIPTION 1";
			shipment.PortOfFirstArrival = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			shipment.PortOfDestination = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			shipment.PortOfOrigin = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			if (shipment.SubShipmentCollection != null)
			{
				var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
				subShipment.PortOfFirstArrival = new UNLOCO() { Code = "SGSIN", Name = "Singapore" };
				subShipment.PortOfDestination = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
				subShipment.PortOfOrigin = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
				subShipment.WayBillNumber = "800-43567890";
				subShipment.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
				subShipment.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm()
				{
					Code = "FOB",
					Description = "Free On Board"
				};
				subShipment.CarrierDocumentsOverride = new CarrierDocumentsOverride()
				{
					AWBHeader = new UniversalDataBuss.DataObjects.Universal.AWB.AWBHeader()
					{
						Consignee = new UniversalDataBuss.DataObjects.Universal.AWB.AWBParty() { Name = "CONSIGNEE 1" },
						Shipper = new UniversalDataBuss.DataObjects.Universal.AWB.AWBParty() { Name = "SHIPPER 1" },
						AWBIssueDate = ZDateTime.Now,
						AWBIssuePlace = "SYDNEY",
						AWBNumber = "AWBNumber 1"
					}
				};
			}

			shipment.Order = new Order();

			if (orderRef != null)
			{
				shipment.Order.OrderNumber = orderRef;
			}

			if (pickupDate != null)
			{
				shipment.DateCollection.Insert(0, Date.New(UniversalDataBuss.DataObjects.Universal.DateType.LocalTransportDelivery, true, (ZDateTime)pickupDate));
				shipment.DateCollection.Insert(0, Date.New(UniversalDataBuss.DataObjects.Universal.DateType.LocalTransportCompleted, true, (ZDateTime)pickupDate));
				shipment.DateCollection.Insert(0, Date.New(UniversalDataBuss.DataObjects.Universal.DateType.LocalTransportPickup, true, (ZDateTime)pickupDate));
			}
		}

		public InvoicingBase CreateARINVTransactions(GlbBranch branch, AccTaxRate taxRate, string complianceSubType = ComplianceSubTypeCodes.EIN, string complianceNumber = "ABC2020000000001")
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "charge1", branch.Company.LocalCurrency, 10m, TestObjectCreator.CreditorTR, branch.Company.LocalCurrency, 10m, TestObjectCreator.DebtorTR);
				charge1.JR_AT_SellGSTRate = taxRate.PK;

				var job2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "charge3", branch.Company.LocalCurrency, 10m, TestObjectCreator.CreditorTR, branch.Company.LocalCurrency, 10m, TestObjectCreator.DebtorTR);
				charge2.JR_AT_SellGSTRate = taxRate.PK;

				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), branch.Company.LocalCurrency, 1m, 10m, 0M, 10m, 0m, TestObjectCreator.DebtorTR, TestObjectCreator.CC1.PK);
				arInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
				arInvoice.AH_TransactionReference = complianceNumber;
				arInvoice.Lines[0].AL_AT = charge1.JR_AT_SellGSTRate;
				SetCommonValues(true, arInvoice, complianceSubType);
				Factory.Save();

				return arInvoice;
			}
		}

		public ARCreditNote CreateReverseTransaction(InvoicingBase invoice, string transactionNumber = "0002")
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC14, "charge1", invoice.Company.LocalCurrency, 100.3m, TestObjectCreator.CreditorTR, invoice.Company.LocalCurrency, 100m, TestObjectCreator.DebtorTR);
			charge1.JR_OSSellExRate = 1m;
			charge1.JR_AT_SellGSTRate = TestObjectCreator.KDV18.PK;
			Factory.Save();

			var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine(transactionNumber, TestObjectCreator.DebtorTR, invoice.Company.LocalCurrency, 1m, "Desc", null, charge1.ChargeCode, 100m, ZDateTime.Today, false);
			arCreditNote.OriginalTransaction = invoice;
			arCreditNote.AH_TransactionBelongsToGroup = invoice.PK;
			arCreditNote.AH_ComplianceSubType = ComplianceSubTypeCodes.ICN;
			Factory.Save();

			return arCreditNote;
		}

		public GlbCompanySignatureCredential CreateCompanySignatureCredential(GlbCompany company, string userName = "user", string password = "12365478")
		{
			var companySignatureCredential = company.Factory.NewWithValidTestData<GlbCompanySignatureCredential>();
			companySignatureCredential.GP_GC = company.PK;
			companySignatureCredential.GP_UserID = userName;
			companySignatureCredential.GP_PasswordStatus = Core.Constants.PasswordOK;
			companySignatureCredential.CurrentDecryptedPassword = password;
			company.SignatureCredentials.Add(companySignatureCredential);
			company.Factory.Save();

			return companySignatureCredential;
		}

		public OrgHeader GetOrgHeader(OrganizationAddress transactionAddress)
		{
			OrgHeader result = null;
			var orgCode = transactionAddress?.OrganizationCode?.SourceValue;
			if (orgCode.HasValue)
			{
				result = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode.Value));
			}
			return result;
		}

		public IDisposable EnableEInvoicingFunctionalityForCompany(GlbCompany company, bool enableEInvoicing = true)
		{
			return AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, enableEInvoicing);
		}

		public IDisposable SetEReportingComplianceDateForCompany(GlbCompany company, DateTime date)
		{
			return AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, date);
		}

		public AccEInvoicingBatch CreateEInvoicingBatch(ZInt batchNumber, ZString batchState, InvoicingBase invoice, ZString governmentAllocatedNumber, ZString actionType, ZString pivotState)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = batchNumber;
			batch.AIB_Status = batchState;
			batch.AIB_GC = invoice.Company.PK;
			batch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow;
			batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			batch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;

			var pivot = CreateAccEInvoicingTransactionPivot(batch, invoice, pivotState, actionType);
			pivot.AIP_ActionType = actionType;

			batch.TransactionPivots.Add(pivot);

			return batch;
		}

		public AccEInvoicingTransactionPivot CreateAccEInvoicingTransactionPivot(AccEInvoicingBatch batch, InvoicingBase invoice, ZString pivotState, string actionType = Core.Constants.EInvoicingPivotActionType.Submit)
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = pivotState;
			pivot.SetCompanyAndCountryCode(batch.Company);
			pivot.AIP_ActionType = actionType;
			Factory.Save();

			return pivot;
		}

		public AccTemplateFileStorage CreateTemplateFile(ZGuid companyPk, ZString templateCode, ZBool isActive, ZBlob fileData, ZGuid externalReference, string fileName = "Default.xslt", string description = "Default")
		{
			var templateFile = Factory.New<AccTemplateFileStorage>();

			if (fileData == ZBlob.Empty)
			{
				fileData = ZBlob.FromUTF8("This is a test script.");
			}

			templateFile.TFS_GC = companyPk;
			templateFile.TFS_Ledger = LedgerTypes.AccountsReceivable;
			templateFile.TFS_Code = templateCode;
			templateFile.TFS_IsActive = isActive;
			templateFile.TFS_FileName = fileName;
			templateFile.TFS_FileData = fileData;
			templateFile.TFS_Description = description;
			templateFile.TFS_ExternalReference = externalReference;
			Factory.Save();

			return templateFile;
		}

		public AccEInvoicingTemplateFileView CreateNewTemplateFileConfiguration(AccTemplateFileStorage accTemplateFile, ZString jobType, ZString transportMode, ZGuid parentId, ZString parentTableCode)
		{
			var configuration = Factory.New<AccEInvoicingTemplateFileView>();

			configuration.ETF_ConfigType = "TES";
			configuration.ETF_GC = accTemplateFile.TFS_GC;
			configuration.ETF_JobType = jobType;
			configuration.ETF_Ledger = accTemplateFile.TFS_Ledger;
			configuration.ETF_ParentID = parentId;
			configuration.ETF_ParentTableCode = parentTableCode;
			configuration.ETF_ServiceDirection = "ALL";
			configuration.ETF_TemplateCode = accTemplateFile.TFS_Code;
			configuration.ETF_TransportMode = transportMode;
			Factory.Save();

			return configuration;
		}

		OrgHeader AddCustomsCodesForDebtor(OrgHeader debtor, bool createVTE = true, bool createVTP = true, bool createVTC = false)
		{
			CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.VDM, "KURUMLAR");
			CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.TCK, "12345678901");
			if (createVTE)
			{
				CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.VTE, "12345678901");
			}
			if (createVTC)
			{
				CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.VTC, "12345678901");
			}
			CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.PEC, "debtor@testmailaddress.com");
			CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.TradeRegistryNumber, "123456789-987654322");
			if (createVTP)
			{
				CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "4567890123");
				CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCodes.VTP, "1357997532");
			}
			else
			{
				CommonHelper.AddCustomsCodeForCountryIfMissing(debtor, Constants.CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "34567890123");
			}

			return debtor;
		}

		OrgHeader AddCustomsCodesForSender(OrgHeader sender)
		{
			CommonHelper.AddCustomsCodeForCountryIfMissing(sender, Constants.CountryCodes.Turkey, OrgCusCodes.VDM, "KURUMLAR");
			CommonHelper.AddCustomsCodeForCountryIfMissing(sender, Constants.CountryCodes.Turkey, OrgCusCodes.MER, "1098765432123456");
			CommonHelper.AddCustomsCodeForCountryIfMissing(sender, Constants.CountryCodes.Turkey, OrgCusCodes.TradeRegistryNumber, "123456789-987654321");
			CommonHelper.AddCustomsCodeForCountryIfMissing(sender, Constants.CountryCodes.Turkey, OrgCusCodes.TCK, "12345678901");
			CommonHelper.AddCustomsCodeForCountryIfMissing(sender, Constants.CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "34567890123");
			CommonHelper.AddCustomsCodeForCountryIfMissing(sender, Constants.CountryCodes.Turkey, OrgCusCodes.VTE, "5534534626");

			return sender;
		}

		void SetCommonValues(bool hasPayment, InvoicingBase arInvoice, string complienceSubType)
		{
			arInvoice.AH_ComplianceSubType = complienceSubType;
			arInvoice.AH_ReceiptType = ReceiptTypes.Cash;
			if (hasPayment)
			{
				TestObjectCreator.AUDBankAccount.AB_AccountNumber = "TR6211111111111111";
				arInvoice.AH_AB = TestObjectCreator.AUDBankAccount.PK;
				arInvoice.SubmittedFromInvoicingForm = true;
				((ARInvoice)arInvoice).IsInvoiceReceiptPayment = true;
			}
		}

		GlbBranch CreateBranchForTurkey()
		{
			SetOrgHeaderAddressForTurkey(TestObjectCreator.CreditorTR);

			var company = TestObjectCreator.CreateNewCompany("DTR", Constants.CountryCodes.Turkey, orgProxy: TestObjectCreator.CreditorTR);
			var branch = TestObjectCreator.CreateNewBranch(company, "IST");
			company.GC_IsReciprocal = true;
			branch.GB_RL_NKHomePort = "TRIST";
			branch.Factory.Save();

			return branch;
		}

		internal string GovermentAllocatedNumberForTest => "9ECA3A5A-DEAF-4DFD-80D7-680C85176302";

		internal string RetryIdentifierForTest => "Unexpected Error - Attempting Retry #";

		public static string XmlFilesEmbeddedLocation => "Enterprise.Accounting.ElectronicMessaging.Testing.Turkey.EInvoiceXmlWriter.Xml.";

		public static string GetEmbeddedResourceAsString(string resourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String(resourceName, XmlFilesEmbeddedLocation).Replace("\t", "  ");

		internal string GetTurkeyEInvoiceGEIMessageXml(string messageType)
			=> GetEmbeddedResourceAsString("TurkeyEInvoice" + messageType + "GEIMessageXml.xml");

		internal GlbBranch TurkeyBranch => turkeyBranch ?? (turkeyBranch = CreateBranchForTurkey());
		GlbBranch turkeyBranch;

		internal TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory, true));
		TestObjectCreator testObjectCreator;

		internal BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public EInvoicingTestHelper CommonHelper => commonHelper ?? (commonHelper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper commonHelper;
	}
}
