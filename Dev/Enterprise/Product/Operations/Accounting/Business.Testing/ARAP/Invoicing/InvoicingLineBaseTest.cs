using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;
using static Enterprise.Core.Constants;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;
using AccGenericJobHeader = Enterprise.Accounting.Business.GenericJob.GenericJob;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract partial class InvoicingLineBaseTest : DependentTransactionLineTest
	{
		public void TestLocalPaidAmount()
		{
			MasterHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.IDR.RX_Code;
			MasterHeader.AH_ExchangeRate = 14000m;
			InvoicingLine.AL_RX_NKTransactionCurrency = TestObjectCreator.IDR.RX_Code;
			InvoicingLine.AL_OSExTaxAmount = 3000000m;
			InvoicingLine.AL_OSTaxAmount = 300000m;
			InvoicingLine.AL_AT = GST10Rate.PK;

			var lineMatching = InvoicingLine as ILineMatching;
			lineMatching.SetDefaultValues();
			AssertEquals("PreCondition, LocalOutstandingAmount",
				235.72m,
				lineMatching.LocalOutstandingAmount * InvoicingLine.Multiplier_ForTestOnly
			);
			AssertEquals("PreCondition, OutstandingAmount",
				3300000m,
				lineMatching.OutstandingAmount * InvoicingLine.Multiplier_ForTestOnly
			);

			lineMatching.PaidAmount = 3300000m * InvoicingLine.Multiplier_ForTestOnly;
			AssertEquals("LocalPaidAmount should be equaled to LocalOutstandingAmount when fully paid.",
				235.72m,
				lineMatching.LocalPaidAmount * InvoicingLine.Multiplier_ForTestOnly
			);

			lineMatching.PaidAmount = (3300000m - 1m) * InvoicingLine.Multiplier_ForTestOnly;
			AssertEquals("LocalPaidAmount should be calculated via exchange rate when partially paid.",
				235.71m,
				lineMatching.LocalPaidAmount * InvoicingLine.Multiplier_ForTestOnly
			);
		}

		public void TestLocalPaidAmountWhenFullPaidInSecondMatch()
		{
			MasterHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.IDR.RX_Code;
			MasterHeader.AH_ExchangeRate = 14000m;
			InvoicingLine.AL_RX_NKTransactionCurrency = TestObjectCreator.IDR.RX_Code;
			InvoicingLine.AL_OSExTaxAmount = 3000000m;
			InvoicingLine.AL_OSTaxAmount = 300000m;
			InvoicingLine.AL_AT = GST10Rate.PK;

			var lineMatching = InvoicingLine as ILineMatching;
			lineMatching.SetDefaultValues();

			InvoicingLine.SetOutstandingAmount_ForTestOnly(1500000m * InvoicingLine.Multiplier_ForTestOnly);
			InvoicingLine.SetLocalOutstandingAmount_ForTestOnly(107.15m * InvoicingLine.Multiplier_ForTestOnly);

			lineMatching.PaidAmount = 1500000m * InvoicingLine.Multiplier_ForTestOnly;
			AssertEquals($"LocalPaidAmount, {lineMatching.OutstandingAmount}/14000 => 107.14, but it should be 107.15 since it is fully paid.",
				107.15m,
				lineMatching.LocalPaidAmount * InvoicingLine.Multiplier_ForTestOnly
			);
		}

		[SuspendCriticalValidation]
		public void TestAL_PostToGLAndAL_ReverseToGLIsAlwaysYesAfterSavingOfCommentLine()
		{
			Line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			AssertEquals("Precondition before saving: AL_PostToGL", "N", Line.AL_PostToGL);
			AssertEquals("Precondition before saving: AL_ReverseToGL", "N", Line.AL_ReverseToGL);

			Factory.Save();

			var isCSTorREVLine = Line.AL_LineType == TransactionLineTypes.Revenue || Line.AL_LineType == TransactionLineTypes.Cost;
			var expectedValue = isCSTorREVLine ? "Y" : "N";
			AssertEquals("After saving: AL_PostToGL", expectedValue, Line.AL_PostToGL);
			AssertEquals("After saving: AL_ReverseToGL", expectedValue, Line.AL_ReverseToGL);
		}

		[SuspendCriticalValidation]
		public void TestAL_PostToGLAndAL_ReverseToGLIsAlwaysNoAfterSavingOfNonCommentLine()
		{
			Line.AL_AC = TestObjectCreator.CC1.PK;
			AssertEquals("Precondition before saving: AL_PostToGL", "N", Line.AL_PostToGL);
			AssertEquals("Precondition before saving: AL_ReverseToGL", "N", Line.AL_ReverseToGL);
			Factory.Save();
			AssertEquals("After saving: AL_PostToGL", "N", Line.AL_PostToGL);
			AssertEquals("After saving: AL_ReverseToGL", "N", Line.AL_ReverseToGL);
		}

		[TestDate(2021, 02, 02)]
		public void TestAL_TaxDateIsDefaultedBasedOnTaxDateDefaultingOptionRegistryWhenAL_ATIsSetForJobRelatedLines()
		{
			var shipmentDepartureDate = ZDate.Today.AddDays(5);
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			shipment.JS_E_DEP = shipmentDepartureDate;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.DepartureDate;

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
				invoice.SubmittedFromInvoicingForm = true;

				var firstInvoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				Assert(firstInvoiceLine.AL_JH.IsEmpty);
				Assert(firstInvoiceLine.AL_AC.IsEmpty);
				Assert(firstInvoiceLine.AL_AT.IsEmpty);
				AssertEquals("Tax date should default to empty date because AL_JH is empty and AL_AT is not valid.", ZDateTime.Empty, firstInvoiceLine.AL_TaxDate);

				firstInvoiceLine.AL_AT = GSTExemptRate.PK;
				AssertEquals("Tax date should default to today because AL_JH is empty and AL_AT is valid.", ZDateTime.Today, firstInvoiceLine.AL_TaxDate);

				firstInvoiceLine.AL_AC = TestObjectCreator.CC1.PK;
				Assert(firstInvoiceLine.AL_JH.IsEmpty);
				AssertEquals(TestObjectCreator.CC1.AC_AT_GSTRate, firstInvoiceLine.AL_AT);
				AssertEquals("Tax date should default to today because AL_JH is empty and AL_AT is valid.", ZDateTime.Today, firstInvoiceLine.AL_TaxDate);

				firstInvoiceLine.AL_JH = job.PK;
				AssertEquals(TestObjectCreator.CC1.AC_AT_GSTRate, firstInvoiceLine.AL_AT);
				AssertEquals("Tax date should default to shipment departure date because AL_JH and AL_AT are valid.", shipmentDepartureDate, firstInvoiceLine.AL_TaxDate);

				var secondInvoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				AssertEquals(job.PK, secondInvoiceLine.AL_JH);
				Assert(secondInvoiceLine.AL_AC.IsEmpty);
				Assert(secondInvoiceLine.AL_AT.IsEmpty);
				Assert(secondInvoiceLine.AL_TaxDate.IsEmpty);

				secondInvoiceLine.AL_AC = TestObjectCreator.CC2.PK;
				AssertEquals(TestObjectCreator.CC1.AC_AT_GSTRate, secondInvoiceLine.AL_AT);
				AssertEquals("Tax date should default to shipment departure date because AL_JH and AL_AT are valid.", shipmentDepartureDate, secondInvoiceLine.AL_TaxDate);

				secondInvoiceLine.AL_TaxDate = ZDate.Today;
				AssertEquals("Precondition : Tax date is manually set to today.", ZDate.Today, secondInvoiceLine.AL_TaxDate);

				secondInvoiceLine.AL_AC = TestObjectCreator.CC3.PK;
				AssertEquals(TestObjectCreator.CC3.AC_AT_GSTRate, secondInvoiceLine.AL_AT);
				AssertEquals("Postcondition : Tax date should default to shipment departure date because AL_JH and AL_AT are valid.", shipmentDepartureDate, secondInvoiceLine.AL_TaxDate);

				secondInvoiceLine.AL_TaxDate = ZDate.Today;
				AssertEquals("Precondition : Tax date is manually set to today.", ZDate.Today, secondInvoiceLine.AL_TaxDate);

				AssertEquals(TestObjectCreator.CC4.AC_AT_GSTRate, TestObjectCreator.CC3.AC_AT_GSTRate);
				secondInvoiceLine.AL_AC = TestObjectCreator.CC4.PK;
				AssertEquals(TestObjectCreator.CC4.AC_AT_GSTRate, secondInvoiceLine.AL_AT);
				AssertEquals("Postcondition : Tax date is not defaulted again because CC3 and CC4 has the same tax rate.", ZDate.Today, secondInvoiceLine.AL_TaxDate);
			}
		}

		[TestDate(2021, 02, 02)]
		public void TestSetAPLineJobDefaultTaxDateBasedOnRegistry_NoJob_RegistryIsTodaysDate()
		{
			AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(TaxDateDefaultingOption.Code.Today, false);
		}

		[TestDate(2021, 02, 02)]
		public void TestSetAPLineJobDefaultTaxDateBasedOnRegistry_NoJob_RegistryIsInvoiceDate()
		{
			AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(TaxDateDefaultingOption.Code.InvoiceDate, false);
		}

		[TestDate(2021, 02, 02)]
		public void TestSetAPLineJobDefaultTaxDateBasedOnRegistry_NoJob_RegistryIsJobOperationalDate()
		{
			AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(TaxDateDefaultingOption.Code.ArrivalDate, false);
		}

		[TestDate(2021, 02, 02)]
		public void TestSetAPLineJobDefaultTaxDateBasedOnRegistry_HasJob_RegistryIsTodaysDate()
		{
			AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(TaxDateDefaultingOption.Code.Today);
		}

		[TestDate(2021, 02, 02)]
		public void TestSetAPLineJobDefaultTaxDateBasedOnRegistry_HasJob_RegistryIsInvoiceDate()
		{
			AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		[TestDate(2021, 02, 02)]
		public void TestSetAPLineJobDefaultTaxDateBasedOnRegistry_HasJob_RegistryIsJobOperationalDate()
		{
			AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(TaxDateDefaultingOption.Code.ArrivalDate);
		}

		void AssertSetAPLineJobDefaultTaxDateBasedOnRegistry(ZString dateOption, bool hasJob = true)
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = dateOption; // TaxDateDefaultingOption.Code.ArrivalDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				InvoicingBase invoice = (InvoicingBase)MasterHeader;
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.AH_InvoiceDate = new ZDate(2021, 2, 1);
				invoice.Lines.Add(InvoicingLine);
				Assert(InvoicingLine.AL_TaxDate.IsEmpty);
				InvoicingLine.AL_AT = TestObjectCreator.GST1.PK;
				if (hasJob)
				{
					InvoicingLine.AL_JH = job.PK;
				}
				else
				{
					InvoicingLine.AL_JH = ZGuid.Invalid;
				}
				if (InvoicingLine.TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable
					|| InvoicingLine.TransactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions
					|| InvoicingLine.TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					if (hasJob)
					{
						if (dateOption == TaxDateDefaultingOption.Code.Today)
						{
							AssertEquals("Should use today's date", new ZDate(2021, 2, 2), InvoicingLine.AL_TaxDate);
						}
						else if (dateOption == TaxDateDefaultingOption.Code.InvoiceDate)
						{
							AssertEquals("Should use invoice date", new ZDate(2021, 2, 1), InvoicingLine.AL_TaxDate);
						}
						else if (dateOption == TaxDateDefaultingOption.Code.ArrivalDate)
						{
							AssertEquals("Should use shipment's ARV date", new ZDate(2021, 2, 7), InvoicingLine.AL_TaxDate);
						}
					}
					else
					{
						AssertEquals("Should use empty date", ZDate.Empty, InvoicingLine.AL_TaxDate);
					}
				}
				else
				{
					Assert("Not applicable", InvoicingLine.AL_TaxDate.IsEmpty);
				}
			}
		}

		public void TestSetDefaultPeriodApportionmentMethodClearOtherFields()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			AssertEquals("DEF", invoiceLine.PeriodApportionmentMethod);
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodStartDate = ZDate.Today;
			invoiceLine.PeriodEndDate = ZDate.Today.AddMonths(1);
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			Assert(!invoiceLine.PeriodStartDate.IsEmpty);
			Assert(!invoiceLine.PeriodEndDate.IsEmpty);
			Assert(!invoiceLine.PeriodClearingGLAccountPK.IsEmpty);
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
			Assert(invoiceLine.PeriodStartDate.IsEmpty);
			Assert(invoiceLine.PeriodEndDate.IsEmpty);
			Assert(invoiceLine.PeriodClearingGLAccountPK.IsEmpty);
		}

		public void TestIsRelatedJobReadyForFinancialClosureWithoutPostSecurity()
		{
			var cacheValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;

			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			job.JH_Status = JobHeaderStatus.Working.Code;
			line.AL_JH = ZGuid.Empty;
			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be false as relatd job statue is not JFC", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be false as relatd job statue is not JFC", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			line.AL_JH = job.PK;
			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be true as relatd job statue is not JFC", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be false as relatd job statue is not JFC", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			line.AL_JH = ZGuid.Empty;
			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be false as relatd job is null", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be false as relatd job is null", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			line.AL_JH = job.PK;
			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be true as user didn't have security right", line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsRelatedJobReadyForFinancialClosureWithoutPostSecurity Should be false as user have security right", !line.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);
			}
		}

		public void TestReadonlynessForPeriodApportionmentFields()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			Assert(invoiceLine.PeriodApportionmentMethodInfo.ReadOnly);
			Assert(invoiceLine.PeriodStartDateInfo.ReadOnly);
			Assert(invoiceLine.PeriodEndDateInfo.ReadOnly);
			Assert(invoiceLine.PeriodClearingGLAccountPKInfo.ReadOnly);

			invoiceLine.AL_JH = ZGuid.Empty;
			AssertEquals(PeriodApportionmentMethods.Codes.Default, invoiceLine.PeriodApportionmentMethod);
			Assert(!invoiceLine.PeriodApportionmentMethodInfo.ReadOnly);
			Assert(invoiceLine.PeriodStartDateInfo.ReadOnly);
			Assert(invoiceLine.PeriodEndDateInfo.ReadOnly);
			Assert(invoiceLine.PeriodClearingGLAccountPKInfo.ReadOnly);

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			Assert(!invoiceLine.PeriodApportionmentMethodInfo.ReadOnly);
			Assert(!invoiceLine.PeriodStartDateInfo.ReadOnly);
			Assert(!invoiceLine.PeriodEndDateInfo.ReadOnly);
			Assert(!invoiceLine.PeriodClearingGLAccountPKInfo.ReadOnly);
		}

		public void TestPeriodApportionmentMethodTypeList()
		{
			var expected = new[]
			{
					"DEF - Recognize in a Post Period",
					"PER - Split Equally over Multiple Periods",
					"MAN - Manually Enter Apportionment Amounts",
					"DAY - Split based on Number of Days in Period"
				};

			AssertContainsExactElementsInAnyOrder(expected, InvoicingLine.PeriodApportionmentMethodsList.Cast<ICodeDescription>().Select(x => Invariant($"{x.Code} - {x.Description}")));
		}

		public void TestAL_Calc_RelatedJob()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			Factory.Save();

			var shipmentJob = TestObjectCreator.CreateJob(shipment1, createWithMutex: false);
			var shipmentCharge = TestObjectCreator.CreateCharge(shipmentJob, TestObjectCreator.CC1, 10m, 10m);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var gatewayCharge = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1, 15m, 15m);
			gatewayCharge.JR_Calc_RelatedJobNumber = shipment1.JS_UniqueConsignRef;
			Factory.Save();

			var forwardingConsol = TestObjectCreator.CreateConsol(consolNum: "C0002");
			TestObjectCreator.CreateShipment("S00002", forwardingConsol);
			var consolCost = TestObjectCreator.CreateConsolCost(forwardingConsol, TestObjectCreator.CC2, 20m, TestObjectCreator.Creditor1);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			invoice.IsConvertedFromARInvoice = true;
			invoice.SubmittedFromInvoicingForm = true;

			var invoiceLineImportedFromGatewayCharge = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.ImportJobChargesIntoInvoice(new[] { gatewayCharge, shipmentCharge }, invoiceLineImportedFromGatewayCharge, false);
			var invoiceLineImportedFromShipmentCharge = invoice.Lines[1];
			AssertNotNull(invoiceLineImportedFromShipmentCharge);
			Assert(invoiceLineImportedFromGatewayCharge.IsPopulatedFromImportedJobCharge);
			Assert(invoiceLineImportedFromShipmentCharge.IsPopulatedFromImportedJobCharge);

			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(forwardingConsol);
			var importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
			importer.ImportCostsIntoCosting(new BusinessObject[] { consolCost });
			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			invoice.ImportAllApportionmentsFromCosting();
			var invoiceLineImportedFromApportionCharge = invoice.Lines[2];
			AssertNotNull(invoiceLineImportedFromApportionCharge);
			Assert(invoiceLineImportedFromApportionCharge.IsPopulatedFromImportedJobCharge);

			var invoiceLineManuallyEntered = TestObjectCreator.CreateInvoiceLine(invoice, gatewayJob, TestObjectCreator.CC3, 30m);
			var invoiceLineFromIntercompanyInvoiceImport = TestObjectCreator.CreateInvoiceLine(invoice, gatewayJob, TestObjectCreator.CC4, 40m);
			invoiceLineFromIntercompanyInvoiceImport.RelatedJobFromIntercompanyInvoiceImport = (shipment1.PK, shipment1.JS_UniqueConsignRef);

			AssertEquals(shipment1.PK, invoiceLineImportedFromGatewayCharge.AL_Calc_RelatedJobPK);
			AssertEquals(shipment1.JS_UniqueConsignRef, invoiceLineImportedFromGatewayCharge.AL_Calc_RelatedJobNumber);

			AssertEquals(shipment1.PK, invoiceLineFromIntercompanyInvoiceImport.AL_Calc_RelatedJobPK);
			AssertEquals(shipment1.JS_UniqueConsignRef, invoiceLineFromIntercompanyInvoiceImport.AL_Calc_RelatedJobNumber);

			AssertEquals(ZGuid.Empty, invoiceLineImportedFromApportionCharge.AL_Calc_RelatedJobPK);
			AssertEquals(ZString.Empty, invoiceLineImportedFromApportionCharge.AL_Calc_RelatedJobNumber);

			AssertEquals(ZGuid.Empty, invoiceLineImportedFromShipmentCharge.AL_Calc_RelatedJobPK);
			AssertEquals(ZString.Empty, invoiceLineImportedFromShipmentCharge.AL_Calc_RelatedJobNumber);

			AssertEquals(ZGuid.Empty, invoiceLineManuallyEntered.AL_Calc_RelatedJobPK);
			AssertEquals(ZString.Empty, invoiceLineManuallyEntered.AL_Calc_RelatedJobNumber);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedInvoice = newFactory.Load<APInvoice>(invoice.PK);
			AssertNotNull(reloadedInvoice);
			var reloadedLines = reloadedInvoice.Lines.Cast<InvoicingLineBase>();

			invoiceLineImportedFromGatewayCharge = reloadedLines.First(l => l.PK == invoiceLineImportedFromGatewayCharge.PK);
			AssertEquals(shipment1.PK, invoiceLineImportedFromGatewayCharge.AL_Calc_RelatedJobPK);
			AssertEquals(shipment1.JS_UniqueConsignRef, invoiceLineImportedFromGatewayCharge.AL_Calc_RelatedJobNumber);

			invoiceLineFromIntercompanyInvoiceImport = reloadedLines.First(l => l.PK == invoiceLineFromIntercompanyInvoiceImport.PK);
			AssertEquals(shipment1.PK, invoiceLineFromIntercompanyInvoiceImport.AL_Calc_RelatedJobPK);
			AssertEquals(shipment1.JS_UniqueConsignRef, invoiceLineFromIntercompanyInvoiceImport.AL_Calc_RelatedJobNumber);

			invoiceLineImportedFromApportionCharge = reloadedLines.First(l => l.PK == invoiceLineImportedFromApportionCharge.PK);
			AssertEquals(ZGuid.Empty, invoiceLineImportedFromApportionCharge.AL_Calc_RelatedJobPK);
			AssertEquals(ZString.Empty, invoiceLineImportedFromApportionCharge.AL_Calc_RelatedJobNumber);

			invoiceLineImportedFromShipmentCharge = reloadedLines.First(l => l.PK == invoiceLineImportedFromShipmentCharge.PK);
			AssertEquals(ZGuid.Empty, invoiceLineImportedFromShipmentCharge.AL_Calc_RelatedJobPK);
			AssertEquals(ZString.Empty, invoiceLineImportedFromShipmentCharge.AL_Calc_RelatedJobNumber);

			invoiceLineManuallyEntered = reloadedLines.First(l => l.PK == invoiceLineManuallyEntered.PK);
			AssertEquals(ZGuid.Empty, invoiceLineManuallyEntered.AL_Calc_RelatedJobPK);
			AssertEquals(ZString.Empty, invoiceLineManuallyEntered.AL_Calc_RelatedJobNumber);
		}

		public void TestAL_Calc_RelatedJobForIncompleteInvoice()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			Factory.Save();

			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var gatewayCharge = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1, 15m, 15m);
			gatewayCharge.JR_Calc_RelatedJobNumber = shipment1.JS_UniqueConsignRef;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			invoice.SubmittedFromInvoicingForm = true;
			var invoiceLineImportedFromGatewayCharge = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.ImportJobChargesIntoInvoice(new[] { gatewayCharge }, invoiceLineImportedFromGatewayCharge, false);
			Assert(invoiceLineImportedFromGatewayCharge.IsPopulatedFromImportedJobCharge);
			AssertEquals(shipment1.PK, invoiceLineImportedFromGatewayCharge.AL_Calc_RelatedJobPK);
			AssertEquals(shipment1.JS_UniqueConsignRef, invoiceLineImportedFromGatewayCharge.AL_Calc_RelatedJobNumber);

			Assert(!invoice.IsIncompleteInvoice);
			invoice.SaveAsIncomplete();
			Assert(invoice.IsIncompleteInvoice);

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			AssertNotNull(incompleteInvoice);
			incompleteInvoice.RestoreSavedData();
			AssertEquals(shipment1.PK, incompleteInvoice.Lines[0].AL_Calc_RelatedJobPK);
			AssertEquals(shipment1.JS_UniqueConsignRef, incompleteInvoice.Lines[0].AL_Calc_RelatedJobNumber);
		}

		public void TestRelatedJobIdAndRelatedJobNumberResetsWhenInvoicingJobChanges()
		{
			var originalGatewayInvoicingJobPk = new ZGuid("eee11dd9-c320-4615-b3e5-f3b585ebfff3");
			InvoicingLine.InvoiceBase.IsConvertedFromARInvoice = true;
			InvoicingLine.AL_JH = originalGatewayInvoicingJobPk;

			AssertEquals("Precondition", ZGuid.Empty, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobPk);
			AssertEquals("Precondition", ZString.Empty, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobNumber);
			AssertEquals("Precondition", ZString.Empty, InvoicingLine.AL_Calc_RelatedJobNumber);

			bool refreshBindingCalledOnAL_Calc_RelatedJobNumber = false;
			InvoicingLine.AL_Calc_RelatedJobNumberInfo.ValueChanged += (sender, e) =>
			{
				refreshBindingCalledOnAL_Calc_RelatedJobNumber = true;
			};

			InvoicingLine.RelatedJobFromIntercompanyInvoiceImport = (ZGuid.BrettsGuid, "S00001234");

			AssertEquals(ZGuid.BrettsGuid, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobPk);
			AssertEquals("S00001234", InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobNumber);
			Assert(refreshBindingCalledOnAL_Calc_RelatedJobNumber);
			AssertEquals("S00001234", InvoicingLine.AL_Calc_RelatedJobNumber);

			refreshBindingCalledOnAL_Calc_RelatedJobNumber = false;
			InvoicingLine.AL_JH = new ZGuid("dc77f393-c80e-489b-a6e4-e4cd2d988f6e");

			AssertEquals(ZGuid.Empty, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobPk);
			AssertEquals(ZString.Empty, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobNumber);
			Assert(refreshBindingCalledOnAL_Calc_RelatedJobNumber);
			AssertEquals(ZString.Empty, InvoicingLine.AL_Calc_RelatedJobNumber);

			refreshBindingCalledOnAL_Calc_RelatedJobNumber = false;
			InvoicingLine.AL_JH = originalGatewayInvoicingJobPk;

			AssertEquals(ZGuid.BrettsGuid, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobPk);
			AssertEquals("S00001234", InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobNumber);
			Assert(refreshBindingCalledOnAL_Calc_RelatedJobNumber);
			AssertEquals("S00001234", InvoicingLine.AL_Calc_RelatedJobNumber);
		}

		public void TestJobChargeTargetFieldsForSisterCompanyInvoiceImport()
		{
			AssertEquals("Precondition", ZGuid.Empty, InvoicingLine.TargetJobIDFromIntercompanyInvoiceImport);
			AssertEquals("Precondition", ZGuid.Empty, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobPk);
			AssertEquals("Precondition", ZString.Empty, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobNumber);

			Assert(!InvoicingLine.IsConvertedFromARInvoice);
			AssertJobChargeTargetFieldsForSisterCompanyInvoiceImport(ZGuid.Empty, ZGuid.Empty, ZString.Empty);

			InvoicingLine.InvoiceBase.IsConvertedFromARInvoice = true;
			Assert(InvoicingLine.IsConvertedFromARInvoice);
			AssertJobChargeTargetFieldsForSisterCompanyInvoiceImport(ZGuid.BrettsGuid, ZGuid.BrettsGuid, "S00001234");
		}

		void AssertJobChargeTargetFieldsForSisterCompanyInvoiceImport(ZGuid expectedTargetJobId, ZGuid expectedRelatedJobId, ZString expectedRelatedJobNumber)
		{
			InvoicingLine.TargetJobIDFromIntercompanyInvoiceImport = ZGuid.BrettsGuid;
			InvoicingLine.RelatedJobFromIntercompanyInvoiceImport = (ZGuid.BrettsGuid, "S00001234");

			AssertEquals(GetDeveloperMessage("TargetJobIDFromIntercompanyInvoiceImport"), expectedTargetJobId, InvoicingLine.TargetJobIDFromIntercompanyInvoiceImport);
			AssertEquals(GetDeveloperMessage("RelatedJobIDFromIntercompanyInvoiceImport"), expectedRelatedJobId, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobPk);
			AssertEquals(GetDeveloperMessage("RelatedJobNumberFromIntercompanyInvoiceImport"), expectedRelatedJobNumber, InvoicingLine.RelatedJobFromIntercompanyInvoiceImport.JobNumber);

			ZString GetDeveloperMessage(ZString propertyName)
			{
				var shouldBeOrNotBe = (InvoicingLine.IsConvertedFromARInvoice ? "" : " NOT");
				return $"{propertyName} should " + shouldBeOrNotBe + " be set when invoice is" + shouldBeOrNotBe + " imported from sister company.";
			}
		}

		public virtual void TestIsStampDutyChargeLine()
		{
			Assert(!InvoicingLine.IsStampDutyChargeLine());
		}

		public virtual void TestSetLocalLineDescriptionAndNoErrors()
		{
			var stampDutyChargeCode = TestObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "NON", 1m, TestObjectCreator.SVAT2, TestObjectCreator.WHTFREE1);
			stampDutyChargeCode.AC_LocalLanguageDescription = "Local Description Stamp Duty";
			var registry = AccountingConfigurationRegistry.Instance;
			var currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();
			using (registry.StampDutyChargeCode.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, stampDutyChargeCode.PK.ToGuid()))
			using (registry.StampDutyFixedAmount.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, 2m))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var line0 = TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 100m, TestObjectCreator.GST1.PK);
				var line1 = arInvoice.AddStampDutyLine();
				AssertEquals("AL_Desc is Stamp Duty", "Stamp Duty", line1.AL_Desc);
				Assert("Stamp Duty has no errors", !line1.HasErrors());

				using (registry.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				using (registry.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is true", true, registry.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);

					var line4 = arInvoice.AddStampDutyLine();
					AssertEquals("AL_Desc is Local Description Stamp Duty", "Local Description Stamp Duty", line4.AL_Desc);
				}
			}
		}

		public void TestIsARIsAP()
		{
			Assert(InvoicingLine.IsAP() != InvoicingLine.IsAR());

			switch (InvoicingLine)
			{
				case UACreditNoteLine _:
				case UAInvoiceLine _:
				case APAdjustmentNoteLine _:
				case APCreditNoteLine _:
				case APInvoiceLine _:
					Assert(InvoicingLine.IsAP());
					break;

				case ARAdjustmentNoteLine _:
				case ARCreditNoteLine _:
				case ARInvoiceLine _:
					Assert(InvoicingLine.IsAR());
					break;

				default:
					Fail(Invariant($"Please extend this test for {InvoicingLine.GetType().Name}"));
					break;
			}
		}

		public void TestOriginalJobChargePK()
		{
			var jobCharge = Factory.New<JobCharge>();
			InvoicingLine.OriginalJobCharge = jobCharge;
			AssertEquals(jobCharge.PK, InvoicingLine.OriginalJobChargePK);

			InvoicingLine.OriginalJobCharge = null;
			AssertEquals(ZGuid.Empty, InvoicingLine.OriginalJobChargePK);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesInvoicingLineBase()
		{
			var invoice = (InvoicingBase)MasterHeader;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			AssertNotNull("Company should not be null", line.Company);

			var localList = new List<string>
				{
					nameof(line.TransLinePaysTotalAmount)
				};

			var osList = new List<string>
				{
					nameof(line.CachedCalculatedTaxAmount),
					nameof(line.GSTInclusiveAmount)
				};

			var tester = new DecimalPlacesAttributeTester(line, line.Company);
			tester.CheckLocalCurrency(localList, nameof(line.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(line.CurrencyDecimals), nameof(line.AL_RX_NKTransactionCurrency), line);
		}

		public void TestTaxReportingBasisHumanReadableName()
		{
			InvoicingLine.AL_GSTVATBasis = "";
			AssertEquals("", InvoicingLine.TaxReportingBasisHumanReadableName);

			InvoicingLine.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
			AssertEquals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Description, InvoicingLine.TaxReportingBasisHumanReadableName);

			InvoicingLine.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertEquals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Description, InvoicingLine.TaxReportingBasisHumanReadableName);
		}

		public void TestLocalTaxRateCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Chile);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Chile IVA", 19);

			Factory.Save();

			var invoice = (InvoicingBase)MasterHeader;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_ExchangeRate = 664.9865M;
			line.AL_OSExTaxAmount = 10495.13M;
			line.AL_RX_NKTransactionCurrency = "USD";

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			line.AL_AT = taxRate.PK;

			AssertEquals("Local ex tax amount", 6979120M, line.AL_LocalExTaxAmount);
			AssertEquals("Local tax is calculated applying 19% tax rate to local ex tax amount", 1326033m, line.AL_LocalTaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			line.AL_AT = Guid.Empty;
			line.AL_AT = taxRate.PK;

			AssertEquals("OS tax amount", 1994.07M, line.AL_OSTaxAmount);
			AssertEquals("Local tax is calculated by converting os tax amount to local tax amount using exchange rate", 1326030M, line.AL_LocalTaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			line.AL_AT = Guid.Empty;
			line.AL_AT = taxRate.PK;

			AssertEquals("Local ex tax amount", 6979120M, line.AL_LocalExTaxAmount);
			AssertEquals("Local tax is calculated applying 19% tax rate to local ex tax amount", 1326033m, line.AL_LocalTaxAmount);
			AssertEquals("OS tax amount", 1994.07M, line.AL_OSTaxAmount);

			line.AL_OSTaxAmount = 1994.08M; //OS tax amount change manually, local tax amount should be calculated after applying exchange rate on OS tax amount
											//because if local tax amount still is calculated from local ex tax amount then after saving the invoice we will not be able to reconstruct the OS tax amount

			AssertEquals("Local tax is calculated by converting os tax amount to local tax amount using exchange rate", 1326036M, line.AL_LocalTaxAmount);
		}

		public void TestSetAL_ATUpdatesAL_A9_VatClass()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			AssertEquals(ZGuid.Empty, line.AL_A9_VATClass);
			TestObjectCreator.GST1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			AssertEquals("line's tax message should be copied from tax rate", TestObjectCreator.TaxMsg1.PK, line.AL_A9_VATClass);
			line.AL_AT = ZGuid.Empty;
			AssertEquals("line's tax message should be reset to empty if tax rate is missing", ZGuid.Empty, line.AL_A9_VATClass);
		}

		public void TestChargeTypeWithOverride()
		{
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			AccChargeTypeOverride chargeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			chargeOverride.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			chargeOverride.AN_JobType = "ALL";
			chargeOverride.AN_JobDirection = "ALL";
			Factory.Save();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001");
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			AssertEquals("ChargeType", ZString.Empty, line.ChargeTypeWithOverride);

			line.GenericCharge = chargeCode.PK;
			AssertEquals("ChargeType", "MRG", line.ChargeTypeWithOverride);

			line.AL_JH = TestObjectCreator.CreateJob(shipment, false).PK;
			AssertEquals("ChargeType", "MJA", line.ChargeTypeWithOverride);
		}

		public void TestComplianceRelatedProperties()
		{
			var type = GetExpectedBusinessObjectType();
			if (type == typeof(ARInvoiceLine) || type == typeof(APInvoiceLine) || type == typeof(ARCreditNoteLine) || type == typeof(APCreditNoteLine))
			{
				var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
				vat3.AT_PostingGroupId = 0;

				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;

				var testInv1 = (InvoicingBase)MasterHeader;
				var line = (InvoicingLineBase)testInv1.Lines.AddNew();
				line.AL_JH = TestObjectCreator.Job1.PK;
				line.AL_AC = ac1.PK;
				line.AL_AT = vat3.PK;

				var charge = TestObjectCreator.CreateJobCharge(line, TestObjectCreator.Job1, ac1);

				var complianceHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv1.AH_Ledger, "desc", "00001014", "ABC", "lineDesc", line);

				Factory.Save();

				AssertEquals("ABC", line.ComplianceSubType);
				AssertEquals("00001014", line.ComplianceDocumentNumber);

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				line.ComplianceSubType = "ALL";
				line.ComplianceDocumentNumber = "Document Number";
				line.ComplianceDocumentOrganization = org.PK;
				line.ComplianceDocumentVATRegistrationNum = "VAT Registration Num";
				line.ComplianceDocumentDate = new ZDateTime(2008, 1, 2, 8, 8, 8);
				line.ComplianceDocumentReportingPeriod = 20180201;
				line.ComplianceDocumentSupportingReason = "AAA";
				line.ComplianceSupportingDocumentType = "XXX";
				line.ComplianceSupportingDocumentNumber = "Supporting Document Number";
				line.CreateComplianceDocumentRecordOnPosting = true;

				AssertEquals("ALL", line.ComplianceSubType);
				AssertEquals("Document Number", line.ComplianceDocumentNumber);
				AssertEquals(org.PK, line.ComplianceDocumentOrganization);
				AssertEquals("VAT Registration Num", line.ComplianceDocumentVATRegistrationNum);
				AssertEquals(new ZDateTime(2008, 1, 2, 8, 8, 8), line.ComplianceDocumentDate);
				AssertEquals(20180201, line.ComplianceDocumentReportingPeriod);
				AssertEquals("AAA", line.ComplianceDocumentSupportingReason);
				AssertEquals("XXX", line.ComplianceSupportingDocumentType);
				AssertEquals("Supporting Document Number", line.ComplianceSupportingDocumentNumber);
				Assert(line.CreateComplianceDocumentRecordOnPosting);

				var cusCode = TestObjectCreator.AALSHI.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "ABN";
				cusCode.OK_CustomsRegNo = "VATREG123";
				cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				Factory.Save();
				line.ComplianceDocumentOrganization = TestObjectCreator.AALSHI.PK;
				AssertEquals("VATREG123", line.ComplianceDocumentVATRegistrationNum);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestComplianceDocumentHeader()
		{
			var type = GetExpectedBusinessObjectType();
			if (type == typeof(ARInvoiceLine) || type == typeof(APInvoiceLine) || type == typeof(ARCreditNoteLine) || type == typeof(APCreditNoteLine))
			{
				var invoice = (InvoicingBase)MasterHeader;
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_AT = TestObjectCreator.GST1.PK;

				var complianceHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(invoice.AH_Ledger, "desc", "00001014", "ABC", "lineDesc", line);
				Factory.Save();
				AssertNotNull(line.ComplianceDocumentHeader);
				AssertEquals(complianceHeader1.PK, line.ComplianceDocumentHeader.PK);

				complianceHeader1.Void();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				invoice = newFactory.Load<InvoicingBase>(invoice.PK);
				line = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.PK == line.PK);
				AssertNull(line.ComplianceDocumentHeader);

				var complianceHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(invoice.AH_Ledger, "desc", "00001015", "ABC", "lineDesc", line);
				Factory.Save();
				invoice = newFactory.Load<InvoicingBase>(invoice.PK);
				line = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.PK == line.PK);
				AssertNotNull(line.ComplianceDocumentHeader);
				AssertEquals(complianceHeader2.PK, line.ComplianceDocumentHeader.PK);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestClearAndEnableComplianceRelatedProperties()
		{
			var testInv1 = (InvoicingBase)MasterHeader;
			var line = (InvoicingLineBase)testInv1.Lines.AddNew();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			line.ComplianceSubType = "ALL";
			line.ComplianceDocumentNumber = "Document Number";
			line.ComplianceDocumentOrganization = org.PK;
			line.ComplianceDocumentVATRegistrationNum = "VAT Registration Num";
			line.ComplianceDocumentDate = new ZDateTime(2008, 1, 2, 8, 8, 8);
			line.ComplianceDocumentReportingPeriod = 20180201;
			line.ComplianceDocumentSupportingReason = "AAA";
			line.ComplianceSupportingDocumentType = "XXX";
			line.ComplianceSupportingDocumentNumber = "Supporting Document Number";
			line.CreateComplianceDocumentRecordOnPosting = true;

			AssertEquals("ALL", line.ComplianceSubType);
			AssertEquals("Document Number", line.ComplianceDocumentNumber);
			AssertEquals(org.PK, line.ComplianceDocumentOrganization);
			AssertEquals("VAT Registration Num", line.ComplianceDocumentVATRegistrationNum);
			AssertEquals(new ZDateTime(2008, 1, 2, 8, 8, 8), line.ComplianceDocumentDate);
			AssertEquals(20180201, line.ComplianceDocumentReportingPeriod);
			AssertEquals("AAA", line.ComplianceDocumentSupportingReason);
			AssertEquals("XXX", line.ComplianceSupportingDocumentType);
			AssertEquals("Supporting Document Number", line.ComplianceSupportingDocumentNumber);

			line.CreateComplianceDocumentRecordOnPosting = false;

			AssertEquals(ZString.Empty, line.ComplianceSubType);
			AssertEquals(ZString.Empty, line.ComplianceDocumentNumber);
			AssertEquals(ZGuid.Empty, line.ComplianceDocumentOrganization);
			AssertEquals(ZString.Empty, line.ComplianceDocumentVATRegistrationNum);
			AssertEquals(ZDateTime.Empty, line.ComplianceDocumentDate);
			AssertEquals(ZInt.Zero, line.ComplianceDocumentReportingPeriod);
			AssertEquals(ZString.Empty, line.ComplianceDocumentSupportingReason);
			AssertEquals(ZString.Empty, line.ComplianceSupportingDocumentType);
			AssertEquals(ZString.Empty, line.ComplianceSupportingDocumentNumber);
		}

		public void TestSettingChargeCodeDoesntBlowUpForSingleDepartmentChargeCodes()
		{
			TestObjectCreator.CC1.AC_DepartmentFilterList = "ABC";
			TestObjectCreator.Factory.Save();

			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.CC1.PK;
			AssertEquals("Department should remain as current department", GlbDepartment.CurrentDepartment.PK, line.AL_GE);
		}

		public void TestChargeListNotLoaded()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.Lines.Add(InvoicingLine);
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			InvoicingLine.AL_JH = job.PK;
			Assert(InvoicingLine.ChargeList.Count == 0);
		}

		public void TestShowJobChargesForImportEventIsNotRaisedWhenAL_JHHasErrors()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			bool showJobChargesForImportEventWasRaised = false;

			invoice.Lines.ShowJobChargesForImportEvent += delegate
			{ showJobChargesForImportEventWasRaised = true; };
			line.AL_JH = job.PK;
			AssertHasError(line.AL_JHInfo, "This type of job cannot be used.");
			Assert("ShowJobChargesForImportEvent shouldn't be raised", !showJobChargesForImportEventWasRaised);
		}

		public void TestShowJobChargesForImportEventIsNotRaisedWhenImportingData()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.Lines.Add(InvoicingLine);
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			bool showJobChargesForImportEventWasRaised = false;
			invoice.Lines.ShowJobChargesForImportEvent += delegate
			{ showJobChargesForImportEventWasRaised = true; };
			((ISupportDataImporting)InvoicingLine).IsImportingData = true;
			InvoicingLine.AL_JH = job.PK;
			Assert("ShowJobChargesForImportEvent shouldn't be raised", !showJobChargesForImportEventWasRaised);
		}

		public void TestAL_OverseasTotal_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var invoice = (InvoicingBase)MasterHeader;

				if (invoice is ARInvoice || invoice is ARCreditNote)
				{
					Assert("PreCondition", invoice.IsSourceReferenceEnabled);
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					invoice.Lines.Add(InvoicingLine);

					AssertEquals("PreCondition", string.Empty, invoice.AH_ComplianceSubType);
					Assert(InvoicingLine.AL_OverseasTotalInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCM;
					Assert(!InvoicingLine.AL_OverseasTotalInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					Assert(!InvoicingLine.AL_OverseasTotalInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.LCR;
					Assert(!InvoicingLine.AL_OverseasTotalInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.LTX;
					Assert(!InvoicingLine.AL_OverseasTotalInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.SBC;
					Assert(InvoicingLine.AL_OverseasTotalInfo.ReadOnly);
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
		}

		public void TestAL_OSTaxAmount_ReadOnlyWithSourceReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var invoice = (InvoicingBase)MasterHeader;

				if (invoice is ARInvoice || invoice is ARCreditNote)
				{
					Assert("PreCondition", invoice.IsSourceReferenceEnabled);
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
					invoice.Lines.Add(InvoicingLine);

					SetupInvoiceAndLineGSTApplicability(true, false, false, false);
					AssertNotEquals(invoice.Company.GC_RX_NKLocalCurrency, invoice.AH_RX_NKTransactionCurrency);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCM;
					Assert("tax fields are not editable because the line doesn't have TAXID", InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
					Assert("tax fields are not editable because the line doesn't have TAXID", InvoicingLine.AL_LocalTaxAmountInfo.ReadOnly);
					Assert("Local Amount should be editable", !InvoicingLine.AL_LocalExTaxAmountInfo.ReadOnly);

					SetupInvoiceAndLineGSTApplicability(true, false, true, true);

					InvoicingLine.AL_AT = GST10Rate.PK;
					Assert(!InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
					Assert(!InvoicingLine.AL_LocalTaxAmountInfo.ReadOnly);
					Assert(!InvoicingLine.AL_LocalExTaxAmountInfo.ReadOnly);
					InvoicingLine.AL_AT = GSTExemptRate.PK;
					Assert(!InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
					Assert(!InvoicingLine.AL_LocalTaxAmountInfo.ReadOnly);
					Assert(!InvoicingLine.AL_LocalExTaxAmountInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					Assert(!InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
					Assert(!InvoicingLine.AL_LocalTaxAmountInfo.ReadOnly);
					Assert(!InvoicingLine.AL_LocalExTaxAmountInfo.ReadOnly);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.SBC;
					Assert(InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
					Assert(InvoicingLine.AL_LocalTaxAmountInfo.ReadOnly);
					Assert(InvoicingLine.AL_LocalExTaxAmountInfo.ReadOnly);
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
		}

		public virtual void TestAL_JH_ReadOnly()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";

			Factory.Save();

			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = invoiceLine.PK;

			Assert("Precondition: Job is not applicable", !invoiceLine.IsJobApplicable);
			Assert("AL_JH_ReadOnly_ForTestOnly must be true because IsInvoicingBaseApproving is true", invoiceLine.AL_JH_ReadOnly_ForTestOnly);
		}

		public void TestAL_JH_ReadOnly_ImportedFromJob()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			if (invoice is APInvoice || invoice is APCreditNote)
			{
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.Lines.Add(InvoicingLine);
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				InvoicingLine.AL_JH = job.PK;
				InvoicingLine.OriginalJobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10M, 12M);

				Assert("Precondition: Is imported from job charge", InvoicingLine.IsPopulatedFromImportedJobCharge);
				Assert("Precondition: Is Job applicable", InvoicingLine.IsJobApplicable);

				Assert("AL_JH is readonly", InvoicingLine.AL_JHInfo.ReadOnly);

				InvoicingLine.OriginalJobCharge = null;

				Assert("Precondition: not imported from job charge", !InvoicingLine.IsPopulatedFromImportedJobCharge);
				Assert("Precondition: If job is applicable does not depend on whether the line is imported from job charge or not", InvoicingLine.IsJobApplicable);

				Assert("AL_JH is readonly", !InvoicingLine.AL_JHInfo.ReadOnly);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestIsJobApplicable()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			if (invoice is UAInvoice || invoice is UACreditNote)
			{
				invoice = Factory.NewWithValidTestData<UAInvoice>();
				invoice.AH_Ledger = "AP";
				invoice.AH_TransactionType = "INV";

				Factory.Save();

				InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				invoiceLine.AL_Desc = "Test 002";
				invoiceLine.AL_OSExTaxAmount = 100m;
				invoiceLine.AL_IsFinalCharge = false;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				invoiceLine.AL_JH = job.PK;

				var charge = TestObjectCreator.CreateCharge(invoiceLine);
				Assert("Precondition", invoiceLine.IsInvoicingBaseApproving);

				Assert("Job not applicable as IsInvoicingBaseApproving = true", !invoiceLine.IsJobApplicable);
			}
			else if (invoice is ARInvoice || invoice is ARCreditNote || invoice is ARAdjustmentNote)
			{
				if (invoice is IAmending amending && invoice is IBadDebtWritingOff badDebt)
				{
					Assert("Precondition: IsAmendingTransaction = false", !amending.IsAmendingTransaction);
					Assert("Precondition: IsAmendingOriginal = false", !InvoicingLine.IsAmendingOriginal);
					Assert("Precondition: Job is not applicable", !InvoicingLine.IsJobApplicable);

					InvoicingLineBase originalLine = (InvoicingLineBase)this.CreateNewLine();
					InvoicingBase originalTransaction = originalLine.InvoiceBase;

					InvoicingLine.TransactionHeader.AH_TransactionBelongsToGroup = originalLine.TransactionHeader.PK;
					amending.FlagAsCreatedAmending();
					Assert("IsAmendingTransaction", amending.IsAmendingTransaction);
					Assert("IsAmendingOriginal", InvoicingLine.IsAmendingOriginal);

					badDebt.IsWritingOff = true;
					Assert("Precondition: Job is not applicable", !InvoicingLine.IsJobApplicable);
					badDebt.IsWritingOff = false;
					Assert("Job is applicable", InvoicingLine.IsJobApplicable);
				}
				else
				{
					Assert("Job is not applicable", !InvoicingLine.IsJobApplicable);
				}
			}
			else
			{
				Assert("Job is applicable", InvoicingLine.IsJobApplicable);
			}
		}

		public void TestShouldValidateJob()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			if (invoice is UAInvoice || invoice is UACreditNote)
			{
				invoice = Factory.NewWithValidTestData<UAInvoice>();
				invoice.AH_Ledger = "AP";
				invoice.AH_TransactionType = "INV";

				Factory.Save();

				InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				invoiceLine.AL_Desc = "Test 002";
				invoiceLine.AL_OSExTaxAmount = 100m;
				invoiceLine.AL_IsFinalCharge = false;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				invoiceLine.AL_JH = job.PK;

				var charge = TestObjectCreator.CreateCharge(invoiceLine);
				Assert("Precondition: Job not applicable as IsInvoicingBaseApproving = true", !invoiceLine.IsJobApplicable);
				Assert("Precondition: IsConvertedUAInvoiceOrCRD = true", invoice.IsConvertedUAInvoiceOrCRD);
				Assert("Should validate job as IsConvertedUAInvoiceOrCRD = true", invoiceLine.ShouldValidateJob);
			}
			else if (invoice is ARInvoice || invoice is ARCreditNote || invoice is ARAdjustmentNote)
			{
				if (invoice is IAmending amending && invoice is IBadDebtWritingOff badDebt)
				{
					badDebt.IsWritingOff = true;
					Assert("Precondition: Job is not applicable", !InvoicingLine.IsJobApplicable);
					Assert("Should not validate job as IsJobApplicable = false", !InvoicingLine.ShouldValidateJob);

					InvoicingLineBase originalLine = (InvoicingLineBase)this.CreateNewLine();
					InvoicingBase originalTransaction = originalLine.InvoiceBase;
					badDebt.IsWritingOff = false;
					InvoicingLine.TransactionHeader.AH_TransactionBelongsToGroup = originalLine.TransactionHeader.PK;
					amending.FlagAsCreatedAmending();

					Assert("Job is applicable", InvoicingLine.IsJobApplicable);
					Assert("Should validate job as IsJobApplicable = true", InvoicingLine.ShouldValidateJob);
				}
				else
				{
					Assert("Precondition: Job is not applicable", !InvoicingLine.IsJobApplicable);
					Assert("Precondition: IsConvertedUAInvoiceOrCRD = false", !invoice.IsConvertedUAInvoiceOrCRD);
					Assert("Precondition: IsConvertedFromARInvoice = false", !invoice.IsConvertedFromARInvoice);
					Assert("Should not validate job", !InvoicingLine.ShouldValidateJob);

					invoice.IsConvertedFromARInvoice = true;
					Assert("Should validate job as IsConvertedFromARInvoice = true", InvoicingLine.ShouldValidateJob);
				}
			}
			else
			{
				Assert("Precondition: Job is applicable", InvoicingLine.IsJobApplicable);
				Assert("Should validate job as IsJobApplicable = true", InvoicingLine.ShouldValidateJob);
			}
		}

		public void TestAL_OSExTaxAmount_ReadOnly()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";

			Factory.Save();

			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			Assert("AL_OSExTaxAmount_ReadOnly_ForTestOnly must be false because IsConvertedFromARInvoice is false", !invoiceLine.AL_OSExTaxAmount_ReadOnly_ForTestOnly);

			invoiceLine.InvoiceBase.IsConvertedFromARInvoice = true;
			Assert("AL_OSExTaxAmount_ReadOnly_ForTestOnly must be true because IsConvertedFromARInvoice is true", invoiceLine.AL_OSExTaxAmount_ReadOnly_ForTestOnly);
		}

		public void TestAL_A9_VATClass_ReadOnly()
		{
			InvoicingLine.AL_AT = ZGuid.Empty;
			Assert("tax message is readonly if tax rate is missing", InvoicingLine.AL_A9_VATClass_ReadOnly);

			InvoicingLine.AL_AT = GST10Rate.PK;
			var invoice = InvoicingLine.InvoiceBase;
			if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var oldSecurity = Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed;
				var oldRegistry = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				try
				{
					Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = false;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Assert(InvoicingLine.AL_A9_VATClass_ReadOnly);

					Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Assert(InvoicingLine.AL_A9_VATClass_ReadOnly);

					Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = false;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Assert(InvoicingLine.AL_A9_VATClass_ReadOnly);

					Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Assert(!InvoicingLine.AL_A9_VATClass_ReadOnly);
				}
				finally
				{
					Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = oldSecurity;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldRegistry);
				}
			}
			else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = false;
					Assert(InvoicingLine.AL_A9_VATClass_ReadOnly);
					Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = true;
					Assert(InvoicingLine.AL_A9_VATClass_ReadOnly);
				}

				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = false;
					Assert(InvoicingLine.AL_A9_VATClass_ReadOnly);
					Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = true;
					Assert(!InvoicingLine.AL_A9_VATClass_ReadOnly);
				}
			}
			else
			{
				Assert(!InvoicingLine.AL_A9_VATClass_ReadOnly);
			}
		}

		public void TestAL_A9_VATClass_ReadOnlyWithoutHeader()
		{
			var arLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			arLine.AL_AT = ZGuid.Empty;
			AssertNull(arLine.InvoiceBase);
			Assert("tax message is readonly if tax rate is missing", arLine.AL_A9_VATClass_ReadOnly);

			AssertEquals("PreCondition", TransactionLineTypes.Revenue, arLine.AL_LineType);

			arLine.AL_AT = GST10Rate.PK;

			var oldSecurity = Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed;
			var oldRegistry = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			try
			{
				Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = false;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				Assert(arLine.AL_A9_VATClass_ReadOnly);

				Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = true;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				Assert(arLine.AL_A9_VATClass_ReadOnly);

				Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = false;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				Assert(arLine.AL_A9_VATClass_ReadOnly);

				Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = true;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				Assert(!arLine.AL_A9_VATClass_ReadOnly);
			}
			finally
			{
				Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed = oldSecurity;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldRegistry);
			}

			var apLine = Factory.NewWithValidTestData<APInvoiceLine>();
			apLine.AL_AT = ZGuid.Empty;
			AssertNull(apLine.InvoiceBase);
			Assert("tax message is readonly if tax rate is missing", apLine.AL_A9_VATClass_ReadOnly);

			apLine.AL_AT = GST10Rate.PK;
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = false;
				Assert(apLine.AL_A9_VATClass_ReadOnly);
				Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = true;
				Assert(!apLine.AL_A9_VATClass_ReadOnly);
			}
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = false;
				Assert(apLine.AL_A9_VATClass_ReadOnly);
				Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = true;
				Assert(apLine.AL_A9_VATClass_ReadOnly);
			}
		}

		public void TestImportedApportionmentID()
		{
			InvoicingLineBase invoiceLine = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals("ImportedApportionmentID must be empty", ZGuid.Empty, invoiceLine.ImportedApportionmentID);
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge.JR_E6 = ZGuid.NewZGuid();
			invoiceLine.ApportionmentChargeImportedFrom = charge;
			AssertEquals("ImportedApportionmentID must be equal to charge.JR_E6", charge.JR_E6, invoiceLine.ImportedApportionmentID);
		}

		public void TestImportFromApportionSplitCharge()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			invoice.AH_RX_NKTransactionCurrency = "EUR";
			invoice.AH_ExchangeRate = 2.6m;

			Factory.Save();

			string expectedRevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			var revRecognitionCollection = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
			revRecognitionCollection[0].RecognitionDateOptionCode = expectedRevRecognitionType;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecognitionCollection);

			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_Desc = "Test 002";
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_IsFinalCharge = false;

			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostExRate = 2.6m;
			charge.JR_Desc = "Test 001";
			charge.JR_OSCostAmt = 421.79m;
			charge.JR_CostGovtChargeCode = "C009";
			charge.JR_SellGovtChargeCode = "S009";
			AssertEquals("JR_LocalCostAmt calculated from OS", 162.23m, charge.JR_LocalCostAmt);
			charge.JR_LocalCostAmt = 162.24m; // Simulate rounding error correction on local currency amount
			AssertEquals("JR_LocalCostAmt assigned as ajustment", 162.24m, charge.JR_LocalCostAmt);
			charge.IsFinal = true;
			charge.JR_AL_APLine = invoiceLine.PK;

			AssertEquals("RevenueRecognitionType by charge Code should not be empty", expectedRevRecognitionType, charge.InvoicingJob.GetRevenueRecognitionType(charge.ChargeCode));

			AssertNotEquals("Precondition: line and change currency", invoiceLine.AL_RX_NKTransactionCurrency, charge.JR_RX_NKCostCurrency);
			AssertEquals("IsInvoicingBaseApproving", true, invoiceLine.IsInvoicingBaseApproving);
			invoiceLine.ImportFromApportionSplitCharge(charge);

			AssertEquals("AL_RX_NKTransactionCurrency must be stay non changed because IsInvoicingBaseApproving true", "EUR", invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("AL_Desc must be stay non changed because IsInvoicingBaseApproving true", "Test 002", invoiceLine.AL_Desc);
			AssertNotEquals("AL_OSExTaxAmount must be changed because it not depends on IsInvoicingBaseApproving", 100m, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("AL_OSExTaxAmount should be as per charge", 421.79m, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("AL_LineAmount should be as assigned on charge except changing sign", -162.24m, invoiceLine.AL_LineAmount);
			AssertEquals("AL_IsFinal must be stay non changed because IsInvoicingBaseApproving true", false, invoiceLine.AL_IsFinalCharge);
			AssertEquals("AL_RevRecognitionType is populated", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, invoiceLine.AL_RevRecognitionType);
			AssertEquals("AL_GovtChargeCode is populated", "C009", invoiceLine.AL_GovtChargeCode);

			charge.JR_AL_APLine = ZGuid.Empty;
			invoice.Lines.RemoveAndDelete(invoiceLine);

			invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_Desc = "Test 003";
			invoiceLine.AL_OSExTaxAmount = 200m;
			charge.JR_CostGovtChargeCode = "C010";
			charge.JR_SellGovtChargeCode = "S010";
			invoiceLine.AL_IsFinalCharge = false;

			AssertNotEquals("Precondition: line and change currency", invoiceLine.AL_RX_NKTransactionCurrency, charge.JR_RX_NKCostCurrency);
			AssertEquals("IsInvoicingBaseApproving", false, invoiceLine.IsInvoicingBaseApproving);
			invoiceLine.ImportFromApportionSplitCharge(charge);

			AssertEquals("AL_RX_NKTransactionCurrency must be populated from the charge because IsInvoicingBaseApproving false", "USD", invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("AL_Desc must be populated from the charge because IsInvoicingBaseApproving false", "Test 001", invoiceLine.AL_Desc);
			AssertEquals("AL_OSExTaxAmount should be as per charge", 421.79m, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("AL_LineAmount should be as assigned on charge except changing sign", -162.24m, invoiceLine.AL_LineAmount);
			AssertEquals("AL_IsFinal must be  as per charge", true, invoiceLine.AL_IsFinalCharge);
			AssertEquals("AL_RevRecognitionType must be populated from the charge because IsInvoicingBaseApproving false", expectedRevRecognitionType, invoiceLine.AL_RevRecognitionType);
			AssertEquals("AL_GovtChargeCode is populated", "C010", invoiceLine.AL_GovtChargeCode);
		}

		[TestDate(2021, 02, 08)]
		public void TestImportFromApportionSplitCharge_RespectTaxDateRegistryByDefault()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObjectForDeleteTest(Factory);
			if (!(invoiceLine is APInvoiceLine || invoiceLine is APCreditNoteLine))
			{
				Assert("Not applicable", true);
				return;
			}
			AssertNotNull(invoiceLine.InvoiceBase);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var invoiceDate = new ZDateTime(2020, 02, 05);
				invoiceLine.InvoiceBase.AH_InvoiceDate = invoiceDate;
				var consolCost1 = invoiceLine.InvoiceBase.ConsolCosting.ConsolCosts.AddNew();

				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);

				consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
				consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
				consolCost1.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				consolCost1.E6_IsTaxAmountOverridden = true;
				consolCost1.E6_OSGSTAmount_Calc = 11;

				invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
				AssertEquals("AL_AT", TestObjectCreator.GST1.PK, invoiceLine.AL_AT);
				AssertEquals("AL_TaxDate", invoiceDate, invoiceLine.AL_TaxDate);
				AssertEquals("E6_TaxDate", invoiceDate, consolCost1.E6_TaxDate);
			}
		}

		[TestDate(2021, 02, 08)]
		public void TestImportFromApportionSplitCharge_IgnoreTaxDateRegistryIfCostHasTaxDate()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObjectForDeleteTest(Factory);
			if (!(invoiceLine is APInvoiceLine || invoiceLine is APCreditNoteLine))
			{
				Assert("Not applicable", true);
				return;
			}
			AssertNotNull(invoiceLine.InvoiceBase);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var invoiceDate = new ZDateTime(2020, 02, 05);
				invoiceLine.InvoiceBase.AH_InvoiceDate = invoiceDate;
				var consolCost1 = invoiceLine.InvoiceBase.ConsolCosting.ConsolCosts.AddNew();

				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);

				consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
				consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
				consolCost1.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				var manuallyEnteredDate = ZDate.Today.AddDays(2);
				consolCost1.E6_TaxDate = manuallyEnteredDate;
				consolCost1.E6_IsTaxAmountOverridden = true;
				consolCost1.E6_OSGSTAmount_Calc = 11;

				invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
				AssertEquals("AL_AT", TestObjectCreator.GST1.PK, invoiceLine.AL_AT);
				AssertEquals("AL_TaxDate", manuallyEnteredDate, invoiceLine.AL_TaxDate);
				AssertEquals("E6_TaxDate", manuallyEnteredDate, consolCost1.E6_TaxDate);
			}
		}

		[TestDate(2021, 02, 08)]
		public void TestImportFromApportionSplitCharge_FallbackToTodayIfCostTaxDateIsEmpty()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObjectForDeleteTest(Factory);
			if (!(invoiceLine is APInvoiceLine || invoiceLine is APCreditNoteLine))
			{
				Assert("Not applicable", true);
				return;
			}
			AssertNotNull(invoiceLine.InvoiceBase);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var invoiceDate = new ZDateTime(2020, 02, 05);
				invoiceLine.InvoiceBase.AH_InvoiceDate = invoiceDate;
				var consolCost1 = invoiceLine.InvoiceBase.ConsolCosting.ConsolCosts.AddNew();

				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);

				consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
				consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
				consolCost1.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				consolCost1.E6_TaxDate = ZDate.Empty;
				consolCost1.E6_IsTaxAmountOverridden = true;
				consolCost1.E6_OSGSTAmount_Calc = 11;

				invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
				AssertEquals("AL_AT", TestObjectCreator.GST1.PK, invoiceLine.AL_AT);
				AssertEquals("AL_TaxDate", ZDate.Today, invoiceLine.AL_TaxDate);
				AssertEquals("E6_TaxDate", ZDate.Today, consolCost1.E6_TaxDate);
			}
		}

		public void TestImportFromApportionSplitCharge_TaxDate()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObject();
			if (!(invoiceLine is APInvoiceLine || invoiceLine is APCreditNoteLine))
			{
				Assert("Not applicable", true);
				return;
			}

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
			consolCost1.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			var expectedDate = ZDate.Today.AddDays(2);
			consolCost1.E6_TaxDate = expectedDate;
			consolCost1.E6_IsTaxAmountOverridden = true;
			consolCost1.E6_OSGSTAmount_Calc = 11;

			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
			consolCost2.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			consolCost2.E6_TaxDate = ZDate.Empty;
			consolCost2.E6_IsTaxAmountOverridden = true;
			consolCost2.E6_OSGSTAmount_Calc = 14;

			invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
			AssertEquals("AL_AT", TestObjectCreator.GST1.PK, invoiceLine.AL_AT);
			AssertEquals("AL_TaxDate", expectedDate, invoiceLine.AL_TaxDate);
			AssertEquals("E6_TaxDate", expectedDate, consolCost1.E6_TaxDate);
			AssertEquals("E6_OSGSTAmount_Calc", 11m, consolCost1.E6_OSGSTAmount_Calc);
			AssertEquals("JR_OSCostGSTAmt_Calc", 11m, consolCost1.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);

			invoiceLine = (InvoicingLineBase)this.GetNewBusinessObject();
			invoiceLine.ImportFromApportionSplitCharge(consolCost2.ApportionmentCharges[0]);
			AssertEquals("AL_AT", TestObjectCreator.GST1.PK, invoiceLine.AL_AT);
			AssertEquals("AL_TaxDate", ZDate.Today, invoiceLine.AL_TaxDate);
			AssertEquals("E6_TaxDate", ZDate.Today, consolCost2.E6_TaxDate);
			AssertEquals("E6_OSGSTAmount_Calc", 14m, consolCost2.E6_OSGSTAmount_Calc);
			AssertEquals("JR_OSCostGSTAmt_Calc", 14m, consolCost2.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
		}

		public void TestImportFromApportionSplitCharge_PlaceOfSupply_NonEmpty()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var fposList = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);
				var fposCode1 = fposList[0].Code;
				var fposCodeType1 = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, fposCode1);

				var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObject();
				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				TestObjectCreator.CreateJob(shipment, false);
				var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
				consolCost1.E6_PlaceOfSupply = fposCode1;

				AssertEquals("cost place of supply", fposCode1, consolCost1.E6_PlaceOfSupply);
				AssertEquals("cost place of supply Type", fposCodeType1, consolCost1.E6_PlaceOfSupplyType);

				invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
				AssertEquals("line place of supply", fposCode1, invoiceLine.AL_PlaceOfSupply);
				AssertEquals("line place of supply Type", fposCodeType1, invoiceLine.AL_PlaceOfSupplyType);
			}
		}

		public void TestImportFromApportionSplitCharge_PlaceOfSupply_Empty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObject();
				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				TestObjectCreator.CreateJob(shipment, false);
				var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
				consolCost1.E6_PlaceOfSupply = ZString.Empty;

				AssertEquals("cost place of supply", ZString.Empty, consolCost1.E6_PlaceOfSupply);
				AssertEquals("cost place of supply Type", ZString.Empty, consolCost1.E6_PlaceOfSupplyType);

				invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
				AssertEquals("line place of supply", ZString.Empty, invoiceLine.AL_PlaceOfSupply);
				AssertEquals("line place of supply Type", ZString.Empty, invoiceLine.AL_PlaceOfSupplyType);
			}
		}

		public void TestImportFromApportionSplitCharge_SupplyType_NonEmpty()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty,Guid.Empty, true))
			{
				var supplyTypeCode = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;

				var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObject();
				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				TestObjectCreator.CreateJob(shipment, false);
				var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
				consolCost1.E6_SupplyType = supplyTypeCode;

				AssertEquals("cost supply type", supplyTypeCode, consolCost1.E6_SupplyType);

				invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
				AssertEquals("line supply type", supplyTypeCode, invoiceLine.AL_SupplyType);
			}
		}

		public void TestImportFromApportionSplitCharge_SupplyType_ReadOnly()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var supplyTypeCode = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;
			var invoiceLine1 = (InvoicingLineBase)this.GetNewBusinessObject();
			invoiceLine1.AL_SupplyType = supplyTypeCode;

			AssertEquals("invoiceLine1.AL_SupplyType is not readonly.", false, invoiceLine1.AL_SupplyTypeInfo.ReadOnly);

			var invoiceLine2 = (InvoicingLineBase)this.GetNewBusinessObject();
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
			consolCost1.E6_SupplyType = supplyTypeCode;

			AssertEquals("cost supply type", supplyTypeCode, consolCost1.E6_SupplyType);

			invoiceLine2.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
			AssertEquals("line supply type", supplyTypeCode, invoiceLine2.AL_SupplyType);
			AssertEquals("invoiceLine2.AL_SupplyType is readonly after ImportFromApportionSplitCharge.", true, invoiceLine2.AL_SupplyTypeInfo.ReadOnly);
		}

		public void TestImportFromApportionSplitCharge_SupplyType_Empty()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObject();
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
			consolCost1.E6_SupplyType = ZString.Empty;

			AssertEquals("cost supply type", ZString.Empty, consolCost1.E6_SupplyType);

			invoiceLine.ImportFromApportionSplitCharge(consolCost1.ApportionmentCharges[0]);
			AssertEquals("line supply type", ZString.Empty, invoiceLine.AL_SupplyType);
		}

		public void TestSetTaxBranchTriggerSetTaxRate()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			var testTaxBranch = TestObjectCreator.CreateBranch("TST", GlbCompany.CurrentCompany);
			var taxRateTaxBranch = newFactory.NewWithValidTestData<AccTaxRate>();
			var taxRateCurrentBranch = newFactory.NewWithValidTestData<AccTaxRate>();
			TestObjectCreator.CreateTaxOverrides(chargeCode
				, taxOverride => {
					taxOverride.AO_JobType = "NJR";
					taxOverride.AO_GB = GlbBranch.CurrentBranch.PK;
					taxOverride.AO_AT = taxRateCurrentBranch.PK;
				}
				, taxOverride => {
					taxOverride.AO_JobType = "NJR";
					taxOverride.AO_GB = testTaxBranch.PK;
					taxOverride.AO_AT = taxRateTaxBranch.PK;
				});
			newFactory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.SetAPTaxApplicable(true);
			org.CompanyData.SetARTaxApplicable(true);
			var invoice = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
			invoice.AH_OH = org.PK;
			var invoiceLine = (TransactionLine)invoice.Lines.AddNew();
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_AT = ZGuid.Empty;

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations & EnableTaxBranchReporting are not enabled, AL_GB_TaxBranch should not trigger AL_AT resetting.", () => {
				invoiceLine.AL_GB_TaxBranch = testTaxBranch.PK;
				AssertEquals(ZGuid.Empty, invoiceLine.AL_AT);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled, AL_GB_TaxBranch should not trigger AL_AT resetting.", () => {
				invoiceLine.AL_GB_TaxBranch = testTaxBranch.PK;
				AssertEquals(ZGuid.Empty, invoiceLine.AL_AT);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CombineAssertions("When EnableTaxBranchReporting is not enabled,AL_GB_TaxBranch should not trigger AL_AT resetting.", () => {
				invoiceLine.AL_GB_TaxBranch = testTaxBranch.PK;
				AssertEquals(ZGuid.Empty, invoiceLine.AL_AT);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations & EnableTaxBranchReporting are enabled, AL_GB should not trigger AL_AT resetting.", () => {
				invoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
				AssertEquals(ZGuid.Empty, invoiceLine.AL_AT);
			});

			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations & EnableTaxBranchReporting are enabled, AL_GB_TaxBranch should trigger AL_AT resetting.", () => {
				invoiceLine.AL_GB_TaxBranch = testTaxBranch.PK;
				AssertEquals(taxRateTaxBranch.PK, invoiceLine.AL_AT);
			});
		}

		[TestDate(2018, 7, 31)]
		public void TestJobChargeImportedIntoAPInvoiceWithCorrectExRate_InvoiceDateExRateOption()
		{
			AssertJobChargeImportedIntoAPInvoiceWithCorrectExRateByExRateOption(ExRateOption.ExchangeRateBasedOnInvoiceDate);
		}

		[TestDate(2018, 7, 31)]
		public void TestJobChargeImportedIntoAPInvoiceWithCorrectExRate_PostDateExRateOption()
		{
			AssertJobChargeImportedIntoAPInvoiceWithCorrectExRateByExRateOption(ExRateOption.ExchangeRateBasedOnPostDate);
		}

		[TestDate(2018, 7, 31)]
		public void TestJobChargeImportedIntoAPInvoiceWithCorrectExRate_TodayExRateOption()
		{
			AssertJobChargeImportedIntoAPInvoiceWithCorrectExRateByExRateOption(ExRateOption.TodayExchangeRate);
		}

		void AssertJobChargeImportedIntoAPInvoiceWithCorrectExRateByExRateOption(CodeDescriptionPair exRateOption)
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObjectForDeleteTest(Factory);
			var postingExRateRegistry = invoiceLine.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

			if (!(invoiceLine is APInvoiceLine || invoiceLine is APCreditNoteLine))
			{
				Assert("Not applicable", true);
				return;
			}

			AssertNotNull(invoiceLine.InvoiceBase);
			AssertEquals("Invoice date is Today", ZDateTime.Today, invoiceLine.InvoiceBase.AH_InvoiceDate.Date);
			AssertEquals("Post Date is Today", ZDateTime.Today, invoiceLine.InvoiceBase.AH_PostDate.Date);

			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption.Code);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.USD, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.USD, 100m, TestObjectCreator.Debtor);
			AssertEquals(2, job.ExchangeRates.Count);
			var crdExRate = job.ExchangeRates.Cast<JobInvoicing.ExchangeRate>().First(x => x.JF_OrgType == ExchangeRateOrgTypeEnum.Creditor.ToCode());
			AssertEquals("From system's USD Ex Rate", 2m, crdExRate.JF_TodayRate);
			AssertEquals("Defaulted from system's today Ex Rate", 2m, crdExRate.JF_BaseRate);
			crdExRate.JF_BaseRate = 1.5m;
			AssertNotEquals("Now Base Rate is different to Today's Rate", crdExRate.JF_TodayRate, crdExRate.JF_BaseRate);
			AssertEquals("Charge has updated Cost Ex Rate", 1.5m, jobCharge.JR_OSCostExRate);

			invoiceLine.InvoiceBase.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoiceLine.InvoiceBase.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			invoiceLine.InvoiceBase.AH_PostedToEFT = true;  // Use Job Exchange Rate
			AssertEquals("Invoice Rate defaulted to 1 until we add lines", 1m, invoiceLine.InvoiceBase.AH_ExchangeRate);

			invoiceLine.InvoiceBase.ImportJobChargesIntoInvoice(new Charge[] { jobCharge }, invoiceLine, false);
			AssertEquals("Line has correct Exchange Rate", 2m, invoiceLine.AL_ExchangeRate);
			AssertEquals("Invoice has correct Exchange Rate", 2m, invoiceLine.InvoiceBase.AH_ExchangeRate);
		}

		[TestDate(2022, 10, 15)]
		public void TestInvoiceUseCorrectExRateByExRateOption()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObjectForDeleteTest(Factory);
			var postingExRateRegistry = invoiceLine.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

			var collection = new InvoicePostingExRateOptionCollection();
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ExRateOption.Default.Code, 0));
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, 0));
			postingExRateRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var postDate = ZDateTime.Today.AddDays(-1);
			var invoiceDate = ZDateTime.Today.AddDays(-2);
			var rateForPostDate = 2m;
			var rateForInvoiceDate = 3m;
			var rateForToday = 4m;
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2022, 01, 01));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, rateForPostDate, postDate, postDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, rateForInvoiceDate, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, rateForToday, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, rateForPostDate, postDate, postDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, rateForInvoiceDate, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, rateForToday, ZDateTime.Today, ZDateTime.Today);

			Factory.Save();

			InvoicingLine.InvoiceBase.AH_InvoiceDate = invoiceDate;
			InvoicingLine.InvoiceBase.AH_PostDate = postDate;

			invoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("PreCond: company local currency is AUD", TestObjectCreator.AUD.RX_Code, invoiceLine.InvoiceBase.Company.GC_RX_NKLocalCurrency);
			AssertEquals("PreCond: invoice currency is local", TestObjectCreator.AUD.RX_Code, invoiceLine.InvoiceBase.AH_RX_NKTransactionCurrency);
			AssertEquals("PreCond: invoice line currency is foreign", TestObjectCreator.USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);

			AssertEquals("invoice line exchange rate should use the invoice date because we use the invoice currency to get the InvoicePostingExRateOption",
				3m, invoiceLine.AL_ExchangeRate);
		}

		public void TestShouldSkipValidateForEmptyTaxAmount()
		{
			var consol = TestObjectCreator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var header = TestObjectCreator.CreateOrgHeader("Header", true, true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var oms = Factory.NewWithValidTestData<OrgMiscServ>();
			oms.OM_OH = header.PK;

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			companyData.OB_OH = header.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			invoice.AH_OH = header.PK;
			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = TestObjectCreator.CreateChargeCode("TEST").PK;
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 1m;
			cost.E6_AT_TaxRate = TestObjectCreator.CreateTaxRate("TEST", "test", 1).PK;

			invoice.ImportAllApportionmentsFromCosting();
			invoice.Lines.Cast<InvoiceLine>().ForEach(x => x.Validation.ValidateAll());

			var line1 = invoice.Lines.Cast<InvoiceLine>().First(x => x.AL_OSTaxAmount == 0.01m);
			var line2 = invoice.Lines.Cast<InvoiceLine>().First(x => x.AL_OSTaxAmount.IsEmpty);

			AssertNoErrors(line1.AL_OSTaxAmountInfo);
			AssertNoErrors(line2.AL_OSTaxAmountInfo);

			cost.CalculationStrategy.ReleaseMutexes();
		}

		public void TestIsInvoicingBaseApproving()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";

			Factory.Save();

			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = invoiceLine.PK;

			Assert("IsInvoicingBaseApproving must be true", invoiceLine.IsInvoicingBaseApproving);

			charge.SetAPLineForcedForTest(Line.PK);
			AssertEquals("charge.JR_AL_APLine was set", Line.PK, charge.JR_AL_APLine);

			Assert("IsInvoicingBaseApproving must be false because RelatedJobChargeExists is false", !invoiceLine.IsInvoicingBaseApproving);
		}

		public void TestSecurityOverrideProviderCore()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			AssertEquals(true, SecurityOverrideProviderSource.Get(invoiceLine).Provider is DefaultAccessSecurityProvider);

			SecurityOverrideProviderSource.Get(invoice).Provider = new NonInteractiveSecurityOverrideProvider();
			AssertEquals(false, SecurityOverrideProviderSource.Get(invoiceLine).Provider is DefaultAccessSecurityProvider);
			AssertEquals(true, SecurityOverrideProviderSource.Get(invoiceLine).Provider is NonInteractiveSecurityOverrideProvider);
		}

		public void TestDeleteLineForNewlyCreatedJobWithOtherNonJobLines()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(testDataFactory);
			ForwardingShipment shipment = creator.CreateShipment("S00001234");
			Job job = creator.CreateJob(shipment);
			ZGuid gLAccount = creator.CreateGLHeader().PK;
			ZGuid jobChargeCode = creator.MRG100.PK;
			testDataFactory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase gLInvoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBase jobInvoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			gLInvoiceLine.GenericCharge = gLAccount;
			jobInvoiceLine.AL_JH = job.PK;

			invoice.Lines.RemoveAndDelete(jobInvoiceLine);

			AssertEquals("Should be only one line left", 1, invoice.Lines.Count);
		}

		public void TestLineJobCharges()
		{
			AccChargeCode accChargeCode = TestObjectCreator.CC1;
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.Lines.AddNew();
			invoice.Lines[0].GenericCharge = accChargeCode.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_LineType = TransactionLineTypes.UnapprovedCost;
			invoice.AH_Ledger = "UA";
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			TestObjectCreator.CreateJobCharge(invoice.Lines[0], job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			AssertEquals("Line has job related charges", 0, invoice.Lines[0].LineCharges.Count);

			var accrual = TestObjectCreator.CreateAccrual(job, accChargeCode, 1, "", 10);
			Factory.Save();
			invoice.Lines[0].LineCharges.Load();

			AssertEquals("Line has job related charges", 1, invoice.Lines[0].LineCharges.Count);

			var wip = TestObjectCreator.CreateWIP(job, accChargeCode, 1, "", 10);
			Factory.Save();
			invoice.Lines[0].LineCharges.Load();

			AssertEquals("Line has job related charges", 2, invoice.Lines[0].LineCharges.Count);

			invoice.Lines[0].GenericCharge = ZGuid.Empty;
			invoice.Lines[0].AL_JH = ZGuid.Empty;

			var hits = Factory.DatabaseLoadCount;

			invoice.Lines[0].GenericCharge = accChargeCode.PK;
			AssertEquals("Should be no additional hits when charge code is set to empty", hits, Factory.DatabaseLoadCount);
			invoice.Lines[0].GenericCharge = ZGuid.Empty;
			invoice.Lines[0].AL_AC = accChargeCode.PK;
			hits = Factory.DatabaseLoadCount;
			invoice.Lines[0].ReloadLineCharges();
			AssertEquals("Still should be no additional hits when charge code is valid and method is called explicitly", hits, Factory.DatabaseLoadCount);

			invoice.Lines[0].GenericCharge = ZGuid.Empty;
			hits = Factory.DatabaseLoadCount;
			((INeedRow)invoice.Lines[0]).Row[AccTransactionLinesSchema.Constants.AL_JH] = job.PK.ToGuid();
			invoice.Lines[0].ReloadLineCharges();
			AssertEquals("Still should be no additional hits when charge code is empty but job isn't, even if method called explicitly", hits, Factory.DatabaseLoadCount);
		}

		public void TestSuspendAndResumeGenericJobSettingDefaults()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				InvoicingLineBase invoicingLine = Line as InvoicingLineBase;
				invoicingLine.AL_GB = ZGuid.Empty;
				invoicingLine.AL_GE = ZGuid.Empty;

				invoicingLine.SuspendAL_JHSettingDefaults();
				invoicingLine.AL_JH = job.PK;

				Assert("Branch field should not be defaulted", invoicingLine.AL_GB.IsEmpty);
				Assert("Department field should not be defaulted", invoicingLine.AL_GE.IsEmpty);

				invoicingLine.AL_JH = ZGuid.Empty;
				invoicingLine.ResumeAL_JHSettingDefaults();

				invoicingLine.AL_JH = job.PK;
				Assert("Department should not be empty", !invoicingLine.AL_GB.IsEmpty);
			}
		}

		public void TestSuspendAndResumeGenericChargeSettingDefaults()
		{
			TestObjectCreator creator = new TestObjectCreator(new BusinessObjectFactory());
			AccChargeCode chargeCode1 = creator.CC1;

			creator.Factory.Save();

			InvoicingLineBase invoicingLine = Line as InvoicingLineBase;
			invoicingLine.GenericCharge = chargeCode1.PK;
			AssertEquals("Charge Code field on line should be filled", chargeCode1.PK, invoicingLine.AL_AC);
			invoicingLine.AL_AC = ZGuid.Empty;
			invoicingLine.GenericCharge = ZGuid.Empty;

			invoicingLine.SuspendGenericChargeSettingDefaults();
			invoicingLine.GenericCharge = chargeCode1.PK;
			Assert("Charge code field should be empty", invoicingLine.AL_AC.IsEmpty);
			invoicingLine.GenericCharge = ZGuid.Empty;

			invoicingLine.ResumeGenericChargeSettingDefaults();

			invoicingLine.GenericCharge = chargeCode1.PK;
			AssertEquals("Charge Code field on line should be filled", chargeCode1.PK, invoicingLine.AL_AC);
		}

		public void TestDepartmentDefaultingForShipmentJobDoesNotFailIfJH_GEIsNotSetYet()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			var chargeCode = TestObjectCreator.CreateChargeCode("CCC1");
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			Factory.Save();

			job.JH_GE = ZGuid.Empty;

			InvoicingLineBase invoicingLine = Line as InvoicingLineBase;
			invoicingLine.GenericCharge = chargeCode.PK;

			AssertNull("Precondition", job.Department);
			try
			{
				invoicingLine.AL_JH = job.PK;
			}
			catch
			{
				Fail("Should not throw any exception");
			}
		}

		public void TestSuspendGenericChargeSettingDefaultsDoesNotEffectValidateDepartment()
		{
			AccChargeCode fEADepartmentChargeCodeInDB = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, null, null, "FEA");
			Factory.Save();

			InvoicingLineBase line = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			line.SuspendGenericChargeSettingDefaults();

			AssertNull(line.GenericTransactionCharge);
			line.GenericCharge = fEADepartmentChargeCodeInDB.PK;
			AssertNotNull(line.GenericTransactionCharge);

			line.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "CIA")).PK;
			line.Validation.ValidateAL_GE();
			AssertEquals("Department should have error since selected department is not in charge department filter list",
				"The department CIA is not contained in the department filter list for the entered charge." + System.Environment.NewLine + "The list is: FEA",
				line.AL_GEInfo.GetErrors().GetFirstMessage());
			line.ResumeGenericChargeSettingDefaults();
		}

		public void TestJobIsNotDeletedIfOtherLineReferenceSameJob()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Factory.Save();

			InvoicingBase invoice = Factory.NewWithValidTestData(typeof(APInvoice)) as InvoicingBase;

			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				InvoicingLineBase invoicingLineBase1 = invoice.Lines.AddNew() as InvoicingLineBase;
				InvoicingLineBase invoicingLineBase2 = invoice.Lines.AddNew() as InvoicingLineBase;

				invoicingLineBase1.AL_JH = job.PK;
				invoicingLineBase2.AL_JH = job.PK;

				AssertNotNull(invoicingLineBase1.TransactionJob);
				AssertNotNull(invoicingLineBase2.TransactionJob);

				AssertEquals(invoicingLineBase1.TransactionJob.PK, invoicingLineBase1.TransactionJob.PK);

				invoicingLineBase1.Delete();
				Assert(!invoicingLineBase1.TransactionJob.IsDeleted);
			}
		}

		public void TestLineJobChargesForReversedWIPsACRs()
		{
			AccChargeCode accChargeCode = TestObjectCreator.CC1;
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);

			var reversedWIP = TestObjectCreator.CreateWIP(job, accChargeCode, 1, "", 10);
			reversedWIP.RelatedJobCharge.JR_LocalSellAmt = 0;
			reversedWIP.RelatedJobCharge.JR_OSSellAmt = 0;
			Factory.Save();

			Assert("Precondition: reversedWIP.IsReversed", reversedWIP.IsReversed);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.Lines.AddNew();
			invoice.Lines[0].GenericCharge = accChargeCode.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_LineType = TransactionLineTypes.UnapprovedCost;
			invoice.AH_Ledger = "UA";
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			TestObjectCreator.CreateJobCharge(invoice.Lines[0], job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			AssertEquals("Line has job related charges", 0, invoice.Lines[0].LineCharges.Count);

			var accrual = TestObjectCreator.CreateAccrual(job, accChargeCode, 1, "", 10);
			Factory.Save();
			invoice.Lines[0].LineCharges.Load();

			AssertEquals("Line has job related charges", 1, invoice.Lines[0].LineCharges.Count);

			var wip = TestObjectCreator.CreateWIP(job, accChargeCode, 1, "", 10);
			Factory.Save();
			invoice.Lines[0].LineCharges.Load();

			AssertEquals("Line has job related charges", 2, invoice.Lines[0].LineCharges.Count);
		}

		public void TestLineJobChargesWithCancelledHeader()
		{
			AccChargeCode accChargeCode = TestObjectCreator.CC1;
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);

			APInvoice cancelledInvoice = Factory.NewWithValidTestData<APInvoice>();
			cancelledInvoice.Lines.AddNew();
			cancelledInvoice.Lines[0].AL_AC = accChargeCode.PK;
			cancelledInvoice.Lines[0].AL_JH = job.PK;
			cancelledInvoice.Lines[0].AL_LineType = TransactionLineTypes.Cost;
			cancelledInvoice.AH_IsCancelled = ZBool.True;
			TransactionMatchLink matchLink = ((IMatching)cancelledInvoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = cancelledInvoice.PK;
			matchLink.AP_Amount = cancelledInvoice.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);

			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.Lines.AddNew();
			invoice.Lines[0].GenericCharge = accChargeCode.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_LineType = TransactionLineTypes.UnapprovedCost;
			invoice.AH_Ledger = "UA";
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			TestObjectCreator.CreateJobCharge(invoice.Lines[0], job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			AssertEquals("Line has job related charges", 0, invoice.Lines[0].LineCharges.Count);

			var accrual = TestObjectCreator.CreateAccrual(job, accChargeCode, 1, "", 10);
			Factory.Save();
			invoice.Lines[0].LineCharges.Load();

			AssertEquals("Line has job related charges", 1, invoice.Lines[0].LineCharges.Count);

			var wip = TestObjectCreator.CreateWIP(job, accChargeCode, 1, "", 10);
			Factory.Save();
			invoice.Lines[0].LineCharges.Load();

			AssertEquals("Line has job related charges", 2, invoice.Lines[0].LineCharges.Count);
		}

		public void TestFindParentLinesCollection()
		{
			AssertNotNull("Should find Parent Lines Collection", InvoicingLine.GetParentLinesCollection_ForTestOnly());
		}

		public void TestFindParentLinesCollectionThrowsExceptionOnMultipleInvoicingLinesCollectionParents()
		{
			InvoicingLineBaseCollection collection2 = new InvoicingLineBaseCollection(InvoicingLine.InvoiceBase);
			collection2.Add(InvoicingLine);
			try
			{
				InvoicingLineBaseCollection collectionThatShouldNeverBeAssigned = InvoicingLine.GetParentLinesCollection_ForTestOnly();
				Fail("Should never reach here as an invoice line should only have one parent lines collection");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Should throw exception if there is more than one parent lines collection",
					"Invoice line should only have one parent invoice lines collection.", ex.Message);
			}
		}

		public void TestAL_DescDefaultNew()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Code Desc";
			InvoicingLine.AL_AC = chargeCode.PK;
			AssertEquals("Desc should default from Charge Code", chargeCode.AC_Desc, InvoicingLine.AL_Desc);
		}

		public void TestGenericChargeCollectionNullOnDepartment()
		{
			AssertNotNull("ChargeList should never be null", ((InvoicingLineBase)Line).ChargeList);

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();

			AccGenericJobHeader newJob = Factory.LoadGenericJob<AccGenericJobHeader>(shipment.PK, JobShipmentSchema.Constants.Prefix);
			InvoicingLine.AL_JH = newJob.PK;
			InvoicingLine.AL_GE = NonCurrentDepartment.PK;

			AssertNotNull("ChargeList should be instantiated on calling", InvoicingLine.ChargeList);
		}

		public void TestGenericChargeCollection()
		{
			AssertNotNull("Charge Collection should be instantiated", InvoicingLine.ChargeList);
		}

		public virtual void TestGenericJobCollection()
		{
			AssertNotNull("Job Collection should be instantiated", InvoicingLine.JobList);

			ForwardingConsol testConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			testConsol.JK_IsForwarding = true;
			ForwardingShipment testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			InvoicingLine.JobList.Load();
			Assert("The shipment should be in the job list", InvoicingLine.JobList.Contains(testShipment.PK));
		}

		public void TestWithHoldingTaxField_ReadOnlyness()
		{
			SetupInvoiceAndLineWHTApplicability(isCurrentOrgWHTApplicable: true);
			Assert("Readonly as company is not WHT registered and user is not allowed to modify", InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isCurrentOrgWHTApplicable: true, canUserModifyWHT: true);
			Assert("Readonly as company is not WHT registered", InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isCurrentOrgWHTApplicable: true, canUserModifyWHT: true, isCurrentCompanyWHTRegistered: true);
			Assert("Finally! not read only", !InvoicingLine.AL_AWInfo.ReadOnly);

			InvoicingLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			Assert("Readonly as comment charge is selected", InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isCurrentOrgWHTApplicable: true, canUserModifyWHT: false, isCurrentCompanyWHTRegistered: true);
			Assert("Readonly as user is not allowed to modify WHT", InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isGLAccount: true);
			Assert(InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isGLAccount: true, isCurrentCompanyWHTRegistered: true);
			Assert(InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isGLAccount: true, isCurrentOrgWHTApplicable: true, isCurrentCompanyWHTRegistered: true);
			Assert(!InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isCurrentCompanyWHTRegistered: true);
			Assert(InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability(isCurrentOrgWHTApplicable: true, isGLAccount: true, canUserModifyWHT: true, isCurrentCompanyWHTRegistered: true);
			Assert(!InvoicingLine.AL_AWInfo.ReadOnly);

			InvoicingLine.ApportionmentChargeImportedFrom = Factory.New<ApportionSplitCharge>();
			Assert("Readonly as imported from Apportionment split charge", InvoicingLine.AL_AWInfo.ReadOnly);

			SetupInvoiceAndLineWHTApplicability();
			Assert(InvoicingLine.AL_AWInfo.ReadOnly);
		}

		public void TestAL_AWDefaultedFromChargeCode()
		{
			if (InvoicingLine.InvoiceBase.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
				var charge = TestObjectCreator.FRT;
				var withHoldingTax = TestObjectCreator.WHT1;
				charge.AC_AW_WithholdingTaxRate = withHoldingTax.PK;

				var whtRegisteredOrg = TestObjectCreator.CreateOrgHeader("WHTREG", true, false, false, true, false, false);
				InvoicingLine.InvoiceBase.AH_OH = whtRegisteredOrg.PK;

				InvoicingLine.AL_AC = charge.PK;
				AssertEquals("Withholding tax defaulted", withHoldingTax.PK, InvoicingLine.AL_AW);

				InvoicingLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				AssertEquals("Withholding tax does not apply for comment Charge code", ZGuid.Empty, InvoicingLine.AL_AW);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestJobCollectionForAmendingTransaction()
		{
			IAmending amending = InvoicingLine.InvoiceBase as IAmending;
			if (amending != null && ((InvoicingBase)amending).HasImplementedGenerateAmendingTransaction && !(InvoicingLine.InvoiceBase is APInvoice))
			{
				InvoicingLineBase originalLine = (InvoicingLineBase)this.CreateNewLine();
				InvoicingBase originalTransaction = originalLine.InvoiceBase;
				Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				originalLine.AL_JH = job.PK;
				originalTransaction.Lines.RemoveAndDelete(originalTransaction.Lines.AddNew()); // To force recalculating Jobs collection

				InvoicingLine.TransactionHeader.AH_TransactionBelongsToGroup = originalLine.TransactionHeader.PK;
				amending.FlagAsCreatedAmending();
				Assert("IsAmendingTransaction", amending.IsAmendingTransaction);
				AssertEquals("Should be 1 Original Transaction Job PK", 1, amending.OriginalTransactionJobPKs.Length);
				AssertNotNull("JobCollection", InvoicingLine.JobCollection);
				InvoicingLine.JobCollection.Load();
				AssertEquals("JobCollection.Count", 1, InvoicingLine.JobCollection.Count);
				AssertEquals("Should be job from the original transaction line", originalLine.AL_JH, InvoicingLine.JobCollection[0].PK);
			}
			else
			{
				Assert("Not applicable because TransactionHeader does not implement IAmending", true);
			}
		}

		public void TestAL_JHIsNotReadOnlyForAmendingTransaction()
		{
			IAmending amending = InvoicingLine.InvoiceBase as IAmending;
			if (amending != null && InvoicingLine.InvoiceBase.HasImplementedGenerateAmendingTransaction && !(InvoicingLine.InvoiceBase is APInvoice))
			{
				Assert("Precondition: IsAmendingTransaction = false", !amending.IsAmendingTransaction);
				Assert("Precondition: IsAmendingOriginal = false", !InvoicingLine.IsAmendingOriginal);
				Assert("Precondition: Job is not applicable", !InvoicingLine.IsJobApplicable);
				Assert("Precondition: Job Cell should be readonly", InvoicingLine.AL_JHInfo.ReadOnly);

				InvoicingLineBase originalLine = (InvoicingLineBase)this.CreateNewLine();
				InvoicingBase originalTransaction = originalLine.InvoiceBase;

				InvoicingLine.TransactionHeader.AH_TransactionBelongsToGroup = originalLine.TransactionHeader.PK;
				amending.FlagAsCreatedAmending();
				Assert("IsAmendingTransaction", amending.IsAmendingTransaction);
				Assert("IsAmendingOriginal", InvoicingLine.IsAmendingOriginal);
				Assert("Job is applicable", InvoicingLine.IsJobApplicable);
				Assert("Job Cell should not be readonly", !InvoicingLine.AL_JHInfo.ReadOnly);
			}
			else
			{
				Assert("Not applicable because TransactionHeader does not implement IAmending or is not AR", true);
			}
		}

		public void TestGSTReadOnlyWithCompleteGSTApplicability()
		{
			SetupInvoiceAndLineGSTApplicability(true, false, true, true);
			Assert("GST Cell should not be readonly", !InvoicingLine.AL_ATInfo.ReadOnly);
		}

		public void TestGSTAmountFieldWritableWhenRateGreaterThanZero()
		{
			SetupInvoiceAndLineGSTApplicability(true, false, false, false);
			Assert("Amount field should not be writable by default", InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
			SetupInvoiceAndLineGSTApplicability(true, false, true, true);

			InvoicingLine.AL_AT = GST10Rate.PK;
			Assert("Amount field should now be writable", !InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);

			InvoicingLine.AL_AT = GSTExemptRate.PK;
			Assert("Amount field should not be writable", InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);

			InvoicingLine.AL_AT = GST10Rate.PK;
			Assert("Amount field should now be writable", !InvoicingLine.AL_OSTaxAmountInfo.ReadOnly);
		}

		public void TestTaxRate()
		{
			InvoicingLineBase lineToTest = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as InvoicingLineBase;
			lineToTest.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			lineToTest.AL_OSExTaxAmount = 123.45678m;
			lineToTest.AL_AT = GST10Rate.PK;

			AssertEquals(Utilities.Round(12.345678m, lineToTest.TransactionCurrency.Decimals), lineToTest.AL_OSTaxAmount);
		}

		public void TestCachedTaxRateCalculation()
		{
			InvoicingLine.AL_AT = GST10Rate.PK;
			AssertEquals("Cached Calculated tax amount should be 0", 0m, InvoicingLine.CachedCalculatedTaxAmount);
			InvoicingLine.AL_OSExTaxAmount = 234m;
			AssertEquals("Cached Calculated tax amount should be 23.4", 23.4m, InvoicingLine.CachedCalculatedTaxAmount);
			Assert("Flag to reset Calculated amount should be false", !InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);

			InvoicingLine.ResetCachedCalculatedTaxAmount_ForTestOnly();
			InvoicingLine.AL_TaxRateNumerator = 20;
			AssertEquals("Cached Calculated tax amount", 46.8m, InvoicingLine.CachedCalculatedTaxAmount);

			AccTaxRate gST5Rate = TestObjectCreator.CreateTaxRate("GST5", "GST RATE 5", 5);
			InvoicingLine.AL_AT = gST5Rate.PK;
			Assert("Flag to reset Calculated tax amount should be true", InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);
			AssertEquals("Cached Calculated tax amount should be 11.7", 11.7m, InvoicingLine.CachedCalculatedTaxAmount);
			Assert("Flag to reset Calculated tax amount should be false", !InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);

			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_SubUnitRatio = 100;
			InvoicingLine.AL_RX_NKTransactionCurrency = currency.RX_Code;
			Assert("Flag to reset calculated tax amount should be true", InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);
			AssertEquals("Cached calculated tax amount should be 11.7", 11.7m, InvoicingLine.CachedCalculatedTaxAmount);
			Assert("Flag to reset calculated tax amount should be false", !InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);

			InvoicingLine.AL_OSExTaxAmount = 0.04m;
			Assert("Flag to reset Calculated amount should be true", InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);
			AssertEquals("Cached Calculated tax amount should be 0", 0m, InvoicingLine.CachedCalculatedTaxAmount);
			Assert("Flag to reset Calculated amount should be false", !InvoicingLine.IsCachedTaxAmountDirty_ForTestOnly);
		}

		public void TestGSTNotReadOnlyWithApplicableCompanyOrgGLAccountAndDisabledUser()
		{
			SetupInvoiceAndLineGSTApplicability(true, true, false, true);
			Assert("GST Cell should not be readonly with GL Account " +
				"even if the user can't modify GST under normal (i.e. Charge Code) circumstances",
				!InvoicingLine.AL_ATInfo.ReadOnly);
		}

		public void TestGSTNotReadOnlyWithApplicableCompanyOrgChargeCodeAndEnabledUser()
		{
			SetupInvoiceAndLineGSTApplicability(true, false, true, true);
			Assert("GST Cell should not be readonly when registry entry dictates user is allowed to modify", !InvoicingLine.AL_ATInfo.ReadOnly);
		}

		public void TestGSTReadOnlyOnStandardSettings()
		{
			SetupInvoiceAndLineGSTApplicability(true, false, false, true);
			Assert("GST should be readonly", InvoicingLine.AL_ATInfo.ReadOnly);
		}

		public void TestGSTReadOnlyWhenOrgIsNotGSTApplicable()
		{
			SetupInvoiceAndLineGSTApplicability(false, false, false, true);
			Assert("GST should be readonly", InvoicingLine.AL_ATInfo.ReadOnly);
		}

		public void TestGSTReadOnlyWhenOrgIsNotGSTApplicableAndUserCanOverride()
		{
			SetupInvoiceAndLineGSTApplicability(false, false, true, true);
			Assert("GST should be readonly", InvoicingLine.AL_ATInfo.ReadOnly);
		}

		public void TestIsGSTMandatory()
		{
			SetupInvoiceAndLineGSTApplicability(true, false, false, true);
			Assert("GST should be mandatory", InvoicingLine.IsGSTMandatory);

			SetupInvoiceAndLineGSTApplicability(false, false, false, true);
			Assert("GST should not be mandatory", !InvoicingLine.IsGSTMandatory);

			SetupInvoiceAndLineGSTApplicability(true, false, false, false);
			Assert("GST should not be mandatory", !InvoicingLine.IsGSTMandatory);

			SetupInvoiceAndLineGSTApplicability(false, false, false, false);
			Assert("GST should not be mandatory", !InvoicingLine.IsGSTMandatory);
		}

		public void TestGSTDefaultedWhenCurrentCompanyAndOrganisationGSTApplicableWithCharge()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			SetupInvoiceAndLineGSTApplicability(true, false, false, true);
			chargeCode.AC_AT_GSTRate = GST10Rate.PK;
			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertEquals("Line Tax Rate should default GST code - organisation and company are applicable", GST10Rate.PK, InvoicingLine.AL_AT);

			SetCreditorOrDebtorOnLedger(false, true);
			AssertEquals("Line Tax Rate should NOT default GST code - organisation is NOT applicable", ZGuid.Empty, InvoicingLine.AL_AT);
		}

		public void TestGSTNotDefaultedWhenCurrentCompanyNotApplicableAndOrgIs()
		{
			SetupInvoiceAndLineGSTApplicability(true, false, false, false);

			TestStandardCharge.VC_GSTRate = GST10Rate.PK;
			InvoicingLine.GenericCharge = TestStandardCharge.PK;
			AssertEquals("Line Tax Rate should NOT default GST code - company is not applicable", ZGuid.Empty, InvoicingLine.AL_AT);

			SetupInvoiceAndLineGSTApplicability(true, false, false, true);
			AssertEquals("Line Tax Rate should default GST code - company is now applicable", GST10Rate.PK, InvoicingLine.AL_AT);
		}

		public void TestGenericChargeForGLAccount()
		{
			InvoicingLine.GenericCharge = TestGLCharge.PK;

			Assert(InvoicingLine.AL_AC.IsEmpty);
			Assert(InvoicingLine.AL_AG.IsValid);
		}

		public void TestGenericChargeForChargeCode()
		{
			InvoicingLine.GenericCharge = TestStandardCharge.PK;

			Assert(InvoicingLine.AL_AG.IsEmpty);
			Assert(InvoicingLine.AL_AC.IsValid);
		}

		public void TestAL_JHSetsCorrectDefaultsWhenChangedWithNoExtraData()
		{
			var differentFactory = new BusinessObjectFactory();
			ForwardingShipment shipment = differentFactory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			var rate = 5m;

			GlbCompany.CurrentCompany.SetCurrency(CurrencyCodes.Australia);

			RefExchangeRate exRate = differentFactory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = CurrencyCodes.UnitedStates;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = rate;
			differentFactory.Save();

			var invoice = TestObjectCreator.CreateInvoice(MasterHeaderType, TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
			invoice.UseJobExchangeRate = true;

			var line = invoice.Lines.AddNew() as InvoicingLineBase;
			AssertEquals(CurrencyCodes.UnitedStates, line.TransactionCurrency.Code);
			AssertEquals(CurrencyCodes.Australia, GlbCompany.CurrentCompany.LocalCurrency.Code);
			Assert(!line.AL_JHSettingDefaultsSuspended_ForTestOnly);
			line.AL_JH = job.PK;
			AssertEquals("Exchange Rate should be set correctly when defaulting values in AL_JH Setter", rate, line.AL_ExchangeRate);
		}

		public void TestJobIsNotDeletedIfAlreadyInDatabase()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			InvoicingLineBase invoicingLineBase = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			invoicingLineBase.AL_JH = job.PK;
			AssertNotNull(invoicingLineBase.Job);
			AssertEquals(invoicingLineBase.Job.PK, job.PK);
			AssertNotNull(invoicingLineBase.TransactionJob);
			invoicingLineBase.Delete();
			Assert(!job.IsDeleted);
		}

		public void TestCommentChargeCode()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			InvoicingLineBase invoicingLineBase = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			invoicingLineBase.AL_AC = chargeCode.PK;
			AssertEquals(ZGuid.Empty, invoicingLineBase.AL_AT);
			AssertEquals(ZGuid.Empty, invoicingLineBase.AL_AW);
			AssertEquals(0M, invoicingLineBase.AL_OSTaxAmount);
			AssertEquals(0M, invoicingLineBase.AL_OSExTaxAmount);
			Assert(invoicingLineBase.AL_ATInfo.ReadOnly);
			Assert(invoicingLineBase.AL_AWInfo.ReadOnly);
			Assert(invoicingLineBase.AL_OSExTaxAmountInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestCopyValuesFrom()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;

			InvoicingLineBase invoicingLineBase1 = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			InvoicingLineBase invoicingLineBase2 = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			invoicingLineBase2.AL_JH = testJob.PK;
			invoicingLineBase2.AL_GB = branch.PK;
			invoicingLineBase2.AL_GE = department.PK;

			invoicingLineBase1.CopyValuesFrom(invoicingLineBase2);
			AssertEquals("Branch should be equal", branch.PK, invoicingLineBase1.AL_GB);
			AssertEquals("Department should be equal", department.PK, invoicingLineBase1.AL_GE);
		}

		public void TestSetGSTReadOnlyState()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			InvoicingLineBase testLine = InvoicingLine;
			InvoicingBase invoicingBase = (InvoicingBase)MasterHeader;
			invoicingBase.Lines.Add(testLine);

			SetGSTRegisteredClient(invoicingBase);
			SetUserModifyRegistryFlag(invoicingBase, false);

			testLine.UpdateGSTReadOnlyState();
			AssertEquals(!testLine.AllowUserGSTOverride_ForTestOnly, testLine.AL_ATInfo.ReadOnly);

			SetUserModifyRegistryFlag(invoicingBase, true);

			testLine.UpdateGSTReadOnlyState();
			AssertEquals(!testLine.AllowUserGSTOverride_ForTestOnly, testLine.AL_ATInfo.ReadOnly);

			AccChargeCode commentChargeCode = new TestObjectCreator(Factory).CreateChargeCode("CMT", "Comment", Constants.ChargeType.Comment, 0, null, null, "ALL");
			testLine.GenericCharge = commentChargeCode.PK;
			Factory.Save();
			testLine.UpdateGSTReadOnlyState();
			AssertEquals(true, testLine.AL_ATInfo.ReadOnly);
		}

		public void TestCalculateGST()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			InvoicingLineBase testLine = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			InvoicingBase invoicingBase = (InvoicingBase)Factory.New(MasterHeaderType);
			invoicingBase.Lines.Add(testLine);

			SetGSTRegisteredClient(invoicingBase);

			testLine.AL_AT = GST10Rate.PK;
			testLine.GenericTransactionCharge = TestGLCharge;

			testLine.CalculateGST(testLine.IsGSTMandatory);
			AssertEquals(GST10Rate.PK, testLine.AL_AT);

			testLine.AL_AT = GST10Rate.PK;
			chargeCode.AC_AT_GSTRate = GSTExemptRate.PK;
			testLine.GenericCharge = chargeCode.PK;

			testLine.CalculateGST(testLine.IsGSTMandatory);
			AssertEquals(GSTExemptRate.PK, testLine.AL_AT);
		}

		[SuspendCriticalValidation]
		public void TestRegistryDefaultNotReportTaxId()
		{
			foreach (var registryValue in new[] { true, false })
			{
				foreach (var registryValueForBranchOrgProxy in new[] { true, false })
				{
					AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORT.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue);
					AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValueForBranchOrgProxy); //this registry shouldn't change the behaviour

					var organisation = Factory.NewWithValidTestData<OrgHeader>();
					GlbCompany.CurrentCompany.GC_OH_OrgProxy = organisation.PK;

					var testLine = (InvoicingLineBase)Line;
					testLine.InvoiceBase.AH_OH = organisation.PK;

					var testGSTChargeCode = TestObjectCreator.CreateChargeCode("XXX", "DESCRIPTION", "MRG", 100m, TestObjectCreator.GST1, null);
					testGSTChargeCode.Factory.Save();
					testLine.GenericCharge = testGSTChargeCode.PK;

					AssertNotReportableTaxDefaulting(testLine, registryValue);
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestInterBranchTransactionTaxDefaulting()
		{
			var testLine = SetupInterBranchTransactionTaxDefaulting();

			foreach (var registryValue in new[] { true, false })
			{
				AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORT.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue); //this registry value should not change the tax id defaulting behaviour
				AssertNotReportableTaxDefaulting(testLine, true, "If Org is OrgProxy of any branch of the current company and if tax number are the same for the org and the branch, Then we apply NOT REPORT taxID");
			}
		}

		[SuspendCriticalValidation]
		public void TestInterBranchTransactionTaxDefaulting_RegistryItem()
		{
			var testLine = SetupInterBranchTransactionTaxDefaulting();
			AssertNotReportableTaxDefaulting(testLine, true, "PreCond: All conditions are met to set the NOT taxID on the testLine");

			AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertNotReportableTaxDefaulting(testLine, false, "The registry OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy must be true to set NOT taxID");
		}

		[SuspendCriticalValidation]
		public void TestInterBranchTransactionTaxDefaulting_MatchingTaxRegistrationNumber()
		{
			var testLine = SetupInterBranchTransactionTaxDefaulting();
			AssertNotReportableTaxDefaulting(testLine, true, "PreCond: All conditions are met to set the NOT taxID on the testLine");

			var org = testLine.InvoiceBase.Header;
			var orgTaxNumber = org.CustomsCodes.Cast<OrgCusCode>().First(x => x.OK_CustomsRegNo == "123456789");
			var branchTaxNumber = testLine.Branch.OrgProxy.CustomsCodes.Cast<OrgCusCode>().First(x => x.OK_CustomsRegNo == "123456789");

			org.ResetCodeForTaxRegistration_ForTestOnly();
			orgTaxNumber.OK_CustomsRegNo = "456987";
			AssertEquals("PreCond: The org RawTaxRegistrationNumber is linked to the org customCode value", "456987", org.RawTaxRegistrationNumber);
			Factory.Save();
			AssertNotReportableTaxDefaulting(testLine, false, "Tax Details must be equals to set NOT TaxID");

			org.ResetCodeForTaxRegistration_ForTestOnly();
			testLine.Branch.OrgProxy.ResetCodeForTaxRegistration_ForTestOnly();
			orgTaxNumber.OK_CustomsRegNo = "ABC 1234";
			AssertEquals("ABC 1234", org.RawTaxRegistrationNumber);
			branchTaxNumber.OK_CustomsRegNo = "abc1234";
			AssertEquals("abc1234", testLine.Branch.OrgProxy.RawTaxRegistrationNumber);
			Factory.Save();
			AssertNotReportableTaxDefaulting(testLine, true, "Tax Details comparison ignores whitespace and case");
		}

		[SuspendCriticalValidation]
		public void TestInterBranchTransactionTaxDefaulting_MustBeOrgProxyOfActiveBranch()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testLine = SetupInterBranchTransactionTaxDefaulting();
			AssertNotReportableTaxDefaulting(testLine, true, "PreCond: All conditions are met to set the NOT taxID on the testLine");

			var branchesWithInvoiceOrgAsOrgProxy = testLine.Company.Branches.Where(x => x.GB_OH_OrgProxy == testLine.InvoiceBase.Header.PK);
			AssertEquals("Only one branch is using the invoice org as org proxy", 1, branchesWithInvoiceOrgAsOrgProxy.Count());
			var branchWithInvoiceOrgAsOrgProxy = branchesWithInvoiceOrgAsOrgProxy.First();
			AssertNotEquals(testLine.Branch.PK, branchWithInvoiceOrgAsOrgProxy.PK);

			branchWithInvoiceOrgAsOrgProxy.GB_OH_OrgProxy = objectCreator.ABIGAS.PK;
			Factory.Save();
			AssertNotReportableTaxDefaulting(testLine, false, "organisation must be an org proxy of an active branch of the invoice company to set NOT TaxID");

			branchWithInvoiceOrgAsOrgProxy.GB_OH_OrgProxy = testLine.InvoiceBase.Header.PK;
			Factory.Save();
			Assert(branchWithInvoiceOrgAsOrgProxy.GB_IsActive);
			AssertNotReportableTaxDefaulting(testLine, true, "PreCond: all conditions are met to set the NOT taxID");
			branchWithInvoiceOrgAsOrgProxy.GB_IsActive = false;
			AssertNotReportableTaxDefaulting(testLine, false, "The branch that has the invoice org as org proxy must be active to set the taxID to NOT");
		}

		[SuspendCriticalValidation]
		public void TestInterBranchTransactionTaxDefaultingIsEnforcedWhenBothRegistriesAreEnabled()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testLine = SetupInterBranchTransactionTaxDefaulting();

			AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORT.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = testLine.InvoiceBase.Header.PK;
			orgRelatedParty.PR_OH_RelatedParty = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			orgRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertNotReportableTaxDefaulting(testLine, true, "Invoice Org is part of InterOffice billing group so we set NOT taxID");

			AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			AssertNotReportableTaxDefaulting(testLine, true,
				"When both registries are true and the invoice org is Proxy of any active branch, then we use the InterBranchTransactionTaxDefaulting condition. Here the condition is True so we set NOT taxID");

			var invoiceOrg = testLine.InvoiceBase.Header;
			invoiceOrg.ResetCodeForTaxRegistration_ForTestOnly();
			invoiceOrg.CustomsCodes.Cast<OrgCusCode>().First(x => x.OK_CustomsRegNo == "123456789").OK_CustomsRegNo = "456987";
			Factory.Save();

			AssertNotReportableTaxDefaulting(testLine, false,
				"When both registries are true and the invoice org is Proxy of any active branch and the InterBranchTransactionTaxDefaulting condition is false, we do not apply NOT taxID");
		}

		InvoicingLineBase SetupInterBranchTransactionTaxDefaulting()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var company = objectCreator.CreateCompanyAndBranch("AUSYD");
			var branch = company.FirstActiveBranch;
			AssertNotNull(branch);
			Factory.Save();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var testLine = (InvoicingLineBase)Line;
			testLine.AL_GB = branch.PK;
			AssertEquals(company.PK, testLine.AL_GC);
			AssertNotEquals(testLine.Branch.PK, GlbBranch.CurrentBranch.PK);

			testLine.InvoiceBase.AH_OH = organisation.PK;
			var testGSTChargeCode = objectCreator.CreateChargeCode("XXX", "DESCRIPTION", "MRG", 100m, objectCreator.GST1, null);
			Factory.Save();
			testLine.GenericCharge = testGSTChargeCode.PK;

			AssertNotReportableTaxDefaulting(testLine, false, "The default tax will be applied");

			objectCreator.CreateBranch("TB1", company, organisation);
			Factory.Save();

			var codeType = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCodes.Australia);
			AssertNotNullOrEmpty(codeType);

			organisation.ResetCodeForTaxRegistration_ForTestOnly();
			objectCreator.CreateCustomsCodes(organisation, CountryCodes.Australia, codeType, "123456789");
			AssertEquals("The org RawTaxRegistrationNumber is linked to the org customCode value", "123456789", organisation.RawTaxRegistrationNumber);

			//Add the same custom code with a diferent code type, it shouldn't be used later
			objectCreator.CreateCustomsCodes(organisation, CountryCodes.Australia, OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "123456789");

			testLine.Branch.OrgProxy.ResetCodeForTaxRegistration_ForTestOnly();
			objectCreator.CreateCustomsCodes(testLine.Branch.OrgProxy, CountryCodes.Australia, codeType, "123456789");
			AssertEquals("123456789", testLine.Branch.OrgProxy.RawTaxRegistrationNumber);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORT.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			return testLine;
		}

		void AssertNotReportableTaxDefaulting(InvoicingLineBase testLine, bool isTaxNotReportable, string message = "")
		{
			if (isTaxNotReportable)
			{
				AssertEquals(message, AccTaxRate.Types.NotReportable, testLine.GetFallbackTaxRate_ForTestOnly(out _).AT_Type);
			}
			else
			{
				AssertNotEquals(AccTaxRate.Types.NotReportable, testLine.GetFallbackTaxRate_ForTestOnly(out _).AT_Type);
				AssertEquals(message, "ZZGST1", testLine.GetFallbackTaxRate_ForTestOnly(out _).AT_Code);
			}
		}

		public void TestJobDepartmentDefaulting()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbBranch branch = newFactory.NewWithValidTestData<GlbBranch>();
			GlbDepartment dept = newFactory.NewWithValidTestData<GlbDepartment>();
			newFactory.Save();

			TestObjectCreator.Job1.JH_GB = branch.PK;
			TestObjectCreator.Job1.JH_GE = dept.PK;

			Assert("Precondition: Line department is not the JobDept", dept.PK != InvoicingLine.AL_GE);
			Assert("Precondition: Line branch is not the JobBranch", branch.PK != InvoicingLine.AL_GB);
			InvoicingLine.AL_JH = TestObjectCreator.Job1.PK;
			AssertEquals("Line department should be Dept", dept.PK, InvoicingLine.AL_GE);
			AssertEquals("Line branch should be Branch", branch.PK, InvoicingLine.AL_GB);
		}

		public void TestDecimalPlaces()
		{
			RefCurrency currency2DP = Factory.New<RefCurrency>();
			currency2DP.RX_Code = "ZZ1";
			currency2DP.RX_SubUnitRatio = 100;

			RefCurrency currency0DP = Factory.New<RefCurrency>();
			currency0DP.RX_Code = "ZZ2";
			currency0DP.RX_SubUnitRatio = 0;

			InvoicingLine.AL_RX_NKTransactionCurrency = currency0DP.RX_Code;
			AssertEquals("Decimals should be 0", 0, InvoicingLine.Decimals);
			InvoicingLine.AL_RX_NKTransactionCurrency = currency2DP.RX_Code;
			AssertEquals("Decimals should be 2", 2, InvoicingLine.Decimals);
		}

		public void TestTaxAmountIsRoundedWhenRecalculating()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRateNumerator_ForTestOnly(10);

			AccWithholding wHTRate = Factory.NewWithValidTestData<AccWithholding>();
			wHTRate.AW_Rate = 15m;

			RefCurrency currency = TestObjectCreator.USD;

			Line = Factory.New(GetExpectedBusinessObjectType()) as TransactionLine;

			Line.AL_OSExTaxAmount = 123.45m;
			Line.AL_AT = taxRate.PK;
			AssertEquals("Precondition: Tax amount should be 12.35", 12.35m, Line.AL_OSTaxAmount);
			Line.AL_RX_NKTransactionCurrency = currency.RX_Code;    // causes recalculation of tax amount in InvoicingLineBase
			AssertEquals("Tax amount should be 12.35", 12.35m, Line.AL_OSTaxAmount);

			Line = Factory.New(GetExpectedBusinessObjectType()) as TransactionLine;

			Line.AL_OSExTaxAmount = 543.21m;
			Line.AL_AW = wHTRate.PK;
			AssertEquals("Precondition: Withholding tax amount should be 81.48", 81.48m, Line.AL_OSWHTAmount);
			Line.AL_RX_NKTransactionCurrency = currency.RX_Code;    // causes recalculation of tax amount in InvoicingLineBase
			AssertEquals("Withholding tax amount should be 81.48", 81.48m, Line.AL_OSWHTAmount);
		}

		public override void TestGetNewValidation()
		{
			base.TestGetNewValidation();

			InvoicingBase invBase = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
			InvoicingLineBase invLine = (InvoicingLineBase)invBase.Lines.AddNew();
			Assert("Validation should not be TransactionLineEmptyValidation", !typeof(TransactionLineEmptyValidation).IsAssignableFrom(invLine.Validation.GetType()));

			invBase.AH_IsCancelled = true;
			Assert("Validation should be TransactionLineEmptyValidation", typeof(TransactionLineEmptyValidation).IsAssignableFrom(invLine.Validation.GetType()));

			invBase.AH_IsCancelled = false;
			new IMatchingCollection(Factory) { invBase };

			Assert("Invoice should be in matching context", invBase.IsInMatchingContext);
			Assert("Validation should be LineMatchingValidation", typeof(LineMatchingValidation).IsAssignableFrom(invLine.Validation.GetType()));

			if (invBase is APInvoice || invBase is APCreditNote)
			{
				invBase.AH_IsCancelled = true;
				AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Assert("Validation should be TransactionLineReversalValidation", typeof(TransactionLineEmptyValidation).IsAssignableFrom(invLine.Validation.GetType()));
			}
		}

		#region Tax Rate Calculation Test
		public virtual void TestChargeCodeSettingDepartment()
		{
			AccChargeCode chargeCodeAll = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeAll.AC_Code = "CCODE1";
			chargeCodeAll.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeAll.AC_DepartmentFilterList = "ALL";

			AccChargeCode chargeCodeOne = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeOne.AC_Code = "CCODE2";
			chargeCodeOne.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeOne.AC_DepartmentFilterList = "FEA";

			AccChargeCode chargeCodeMany = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeMany.AC_Code = "CCODE3";
			chargeCodeMany.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeMany.AC_DepartmentFilterList = "FEA, FIA, FES, FIS, CEA, CIA, CES, CIS";

			Factory.Save();

			var invBase = TestObjectCreator.CreateInvoiceWithLine(MasterHeaderType, new ZString("1"), TestObjectCreator.AUD, 1, 1, 1, 1, 1);
			InvoicingLineBase invLine = invBase.Lines[0];

			GlbDepartment currDept = GlbDepartment.CurrentDepartment;

			invLine.GenericCharge = chargeCodeAll.PK;
			AssertEquals("Department should equal Current Department", invLine.AL_GE, currDept.PK);

			GlbDepartment fEADepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

			invLine.GenericCharge = chargeCodeOne.PK;
			AssertEquals("Department should be equal to FEA Department", invLine.AL_GE, fEADepartment.PK);

			invLine.GenericCharge = chargeCodeMany.PK;

			AssertEquals("Department should equal Last set by One charge code", invLine.AL_GE, fEADepartment.PK);

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);

			invLine.AL_JH = job.PK;
			AssertEquals("Department should equal to the one set by the job", job.JH_GE, invLine.AL_GE);

			invLine.AL_JH = ZGuid.Empty;
			invLine.AL_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB").AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")).PK;
			invLine.AL_JH = job.PK;
			AssertEquals("Department should equal to the CIA Department", "CIA", invLine.Department.GE_Code);

			invLine.GenericCharge = chargeCodeOne.PK;
			AssertEquals("Department should equal to the one set by single filter list charge code", invLine.AL_GE, fEADepartment.PK);

			invLine.GenericCharge = chargeCodeMany.PK;
			AssertEquals("Department should equal to the one set by the job", job.JH_GE, invLine.AL_GE);

			invLine.GenericCharge = chargeCodeOne.PK;
			AssertEquals("Department should equal to the one set by single filter list charge code", invLine.AL_GE, fEADepartment.PK);

			invLine.GenericCharge = chargeCodeAll.PK;
			AssertEquals("Department should equal to the one set by the job", job.JH_GE, invLine.AL_GE);
		}

		public virtual void TestFallbackTaxRate()
		{
			BusinessObjectFactory accFactory = new BusinessObjectFactory();

			AccTaxRate taxRate1 = accFactory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccInvMsg taxMessage1 = accFactory.NewWithValidTestData<AccInvMsg>();
			taxMessage1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccInvMsg taxMessage2 = accFactory.NewWithValidTestData<AccInvMsg>();
			taxMessage2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			taxRate1.AT_A9_DefaultVatClass = taxMessage1.PK;

			accFactory.Save();

			AccChargeCode chargeCode = accFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AT_GSTRate = taxRate1.PK;
			chargeCode.AC_Code = "CCODE1";
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride.AO_Direction = ALL;
			taxOverride.AO_IncoTerm = ALL;
			taxOverride.AO_JobType = ALL;
			taxOverride.AO_Origin = ALL;
			taxOverride.AO_Destination = ALL;
			taxOverride.AO_TaxRegCntryOrGroup = ALL;

			AccTaxRate taxRate2 = accFactory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TAX2";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_AT = taxRate2.PK;
			taxOverride.AO_A9_DefaultVATClass = taxMessage2.PK;
			accFactory.Save();

			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.TestOrganisation.PK;

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);

			Job job = TestObjectCreator.CreateJob(shipment, false);
			InvoicingLine.AL_JH = job.PK;

			ZGuid overrideInvTaxMsg = ZGuid.Empty;
			InvoicingLine.AL_AC = chargeCode.PK;
			AssertEquals("Fallback TaxRate should be the TaxRate override - TaxRate2", taxRate2.PK, InvoicingLine.GetFallbackTaxRate_ForTestOnly(out overrideInvTaxMsg).PK);
			AssertEquals("Fallback TaxMessage should be the TaxMessage override - TaxMessage2", taxMessage2.PK, overrideInvTaxMsg);

			AccChargeCode chargeCodeWithoutTaxOverride = accFactory.NewWithValidTestData<AccChargeCode>();
			chargeCodeWithoutTaxOverride.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeWithoutTaxOverride.AC_Code = "CCODE2";
			AccTaxRate taxRate3 = accFactory.NewWithValidTestData<AccTaxRate>();
			taxRate3.AT_Code = "TAX3";
			taxRate3.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccInvMsg taxMessage3 = accFactory.NewWithValidTestData<AccInvMsg>();
			taxRate3.AT_A9_DefaultVatClass = taxMessage3.PK;
			chargeCodeWithoutTaxOverride.AC_AT_GSTRate = taxRate3.PK;
			accFactory.Save();

			InvoicingLine.AL_AC = chargeCodeWithoutTaxOverride.PK;
			AssertEquals("Fallback TaxRate should be the TaxRate on the ChargeCode - TaxRate3", taxRate3.PK, InvoicingLine.GetFallbackTaxRate_ForTestOnly(out overrideInvTaxMsg).PK);
			AssertEquals("Fallback TaxMessage should be the TaxMessage on the TaxRate - TaxMessage3", taxMessage3.PK, overrideInvTaxMsg);
		}

		public virtual void TestSettingJobSetsFallbackTaxRate()
		{
			BusinessObjectFactory accFactory = new BusinessObjectFactory();
			AccChargeCode chargeCode = accFactory.NewWithValidTestData<AccChargeCode>();
			accFactory.Save();

			AccTaxRate taxRateOriginal = accFactory.NewWithValidTestData<AccTaxRate>();
			chargeCode.AC_AT_GSTRate = taxRateOriginal.PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_JobType = ALL;
			taxOverride.AO_Direction = ALL;
			taxOverride.AO_IncoTerm = ALL;
			taxOverride.AO_Origin = ALL;
			taxOverride.AO_Destination = ALL;
			taxOverride.AO_TaxRegCntryOrGroup = ALL;
			taxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			AccTaxRate taxRateOverride = accFactory.NewWithValidTestData<AccTaxRate>();
			taxOverride.AO_AT = taxRateOverride.PK;

			accFactory.Save();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.SetAPTaxApplicable(true);
			org.CompanyData.SetARTaxApplicable(true);
			InvoicingBase invBase = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
			Line = invBase.Lines.AddNew();
			invBase.AH_OH = org.PK;
			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertEquals("Tax Rate should be set to the original Tax", taxRateOriginal.PK, InvoicingLine.AL_AT);
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);

			Job job = TestObjectCreator.CreateJob(shipment, false);
			InvoicingLine.AL_JH = job.PK;

			AssertEquals("Tax Rate should be set to the Tax Override not the original Tax Rate", taxRateOverride.PK, InvoicingLine.AL_AT);
		}

		public virtual void TestSetBranchTriggerSetTaxRate()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			newFactory.Save();

			var taxRate1 = newFactory.NewWithValidTestData<AccTaxRate>();
			var taxOverride1 = chargeCode.TaxOverrides.AddNew();
			taxOverride1.AO_JobType = ALL;
			taxOverride1.AO_Direction = ALL;
			taxOverride1.AO_IncoTerm = ALL;
			taxOverride1.AO_Origin = ALL;
			taxOverride1.AO_Destination = ALL;
			taxOverride1.AO_TaxRegCntryOrGroup = ALL;
			taxOverride1.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride1.AO_AT = taxRate1.PK;
			taxOverride1.AO_GB = GlbBranch.CurrentBranch.PK;

			var taxRate2 = newFactory.NewWithValidTestData<AccTaxRate>();
			var taxOverride2 = chargeCode.TaxOverrides.AddNew();
			taxOverride2.AO_JobType = ALL;
			taxOverride2.AO_Direction = ALL;
			taxOverride2.AO_IncoTerm = ALL;
			taxOverride2.AO_Origin = ALL;
			taxOverride2.AO_Destination = ALL;
			taxOverride2.AO_TaxRegCntryOrGroup = ALL;
			taxOverride2.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride2.AO_AT = taxRate2.PK;
			taxOverride2.AO_GB = ZGuid.Empty;

			newFactory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CompanyData.SetAPTaxApplicable(true);
				org.CompanyData.SetARTaxApplicable(true);
				var invoice = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
				Line = invoice.Lines.AddNew();
				invoice.AH_OH = org.PK;
				InvoicingLine.GenericCharge = chargeCode.PK;

				var shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);
				Job job = TestObjectCreator.CreateJob(shipment, false);
				InvoicingLine.AL_JH = job.PK;

				AssertEquals(taxRate1.PK, InvoicingLine.AL_AT);

				InvoicingLine.AL_GB = ZGuid.Empty;
				AssertEquals("Set the branch will trigger to set the tax rate", taxRate2.PK, InvoicingLine.AL_AT);
			}
		}

		public virtual void TestSetSupplyTypeTriggerSetTaxRate()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			newFactory.Save();

			var taxRate1 = newFactory.NewWithValidTestData<AccTaxRate>();
			var taxOverride1 = chargeCode.TaxOverrides.AddNew();
			taxOverride1.AO_JobType = ALL;
			taxOverride1.AO_Direction = ALL;
			taxOverride1.AO_IncoTerm = ALL;
			taxOverride1.AO_Origin = ALL;
			taxOverride1.AO_Destination = ALL;
			taxOverride1.AO_TaxRegCntryOrGroup = ALL;
			taxOverride1.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride1.AO_AT = taxRate1.PK;
			taxOverride1.AO_GB = GlbBranch.CurrentBranch.PK;
			taxOverride1.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX;

			var taxRate2 = newFactory.NewWithValidTestData<AccTaxRate>();
			var taxOverride2 = chargeCode.TaxOverrides.AddNew();
			taxOverride2.AO_JobType = ALL;
			taxOverride2.AO_Direction = ALL;
			taxOverride2.AO_IncoTerm = ALL;
			taxOverride2.AO_Origin = ALL;
			taxOverride2.AO_Destination = ALL;
			taxOverride2.AO_TaxRegCntryOrGroup = ALL;
			taxOverride2.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride2.AO_AT = taxRate2.PK;
			taxOverride2.AO_GB = GlbBranch.CurrentBranch.PK;
			taxOverride2.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;

			newFactory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CompanyData.SetAPTaxApplicable(true);
				org.CompanyData.SetARTaxApplicable(true);
				var invoice = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
				Line = invoice.Lines.AddNew();
				invoice.AH_OH = org.PK;
				InvoicingLine.GenericCharge = chargeCode.PK;

				var shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);
				Job job = TestObjectCreator.CreateJob(shipment, false);
				InvoicingLine.AL_JH = job.PK;

				InvoicingLine.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX;
				AssertEquals(taxRate1.PK, InvoicingLine.AL_AT);

				InvoicingLine.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				AssertEquals("Set the supply type will trigger to set the tax rate", taxRate2.PK, InvoicingLine.AL_AT);
			}
		}

		public virtual void TestChargeCodeTaxRateOverrideAndChargeCodeTaxOverride()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			var taxRateOriginal = newFactory.NewWithValidTestData<AccTaxRate>();
			chargeCode.AC_AT_GSTRate = taxRateOriginal.PK;

			var taxOverrideSTD = chargeCode.TaxOverrides.AddNew();
			taxOverrideSTD.AO_JobType = ALL;
			taxOverrideSTD.AO_IncoTerm = ALL;
			taxOverrideSTD.AO_Direction = ALL;
			taxOverrideSTD.AO_Origin = ALL;
			taxOverrideSTD.AO_Destination = ALL;
			taxOverrideSTD.AO_TaxRegCntryOrGroup = ALL;
			taxOverrideSTD.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;

			var taxOverrideForINT = chargeCode.TaxOverrides.AddNew();
			taxOverrideForINT.AO_JobType = ALL;
			taxOverrideForINT.AO_IncoTerm = ALL;
			taxOverrideForINT.AO_Direction = ALL;
			taxOverrideForINT.AO_Origin = ALL;
			taxOverrideForINT.AO_Destination = ALL;
			taxOverrideForINT.AO_TaxRegCntryOrGroup = ALL;
			taxOverrideForINT.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverrideForINT.AO_TransactionContext = "INT";
			taxOverrideForINT.AO_DefaultingRule = "SUM";

			var taxRateOverrideForSTD = newFactory.NewWithValidTestData<AccTaxRate>();
			taxOverrideSTD.AO_AT = taxRateOverrideForSTD.PK;
			var taxRateOverrideForINT = newFactory.NewWithValidTestData<AccTaxRate>();
			taxOverrideForINT.AO_AT = taxRateOverrideForINT.PK;
			newFactory.Save();

			AssertEquals("Pre-condition", TaxOverrideTransactionContext.Codes.Standard, taxOverrideSTD.AO_TransactionContext);

			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertNull("TaxRate should be null because GenericJob is not set", InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _));
			InvoicingLine.TaxRateOverrideCalculator.ReCalculate();
			AssertNull("TaxOverride should be null because GenericJob is not set", InvoicingLine.TaxRateOverrideCalculator.CachedTaxRateOverride);

			var shipment = TestObjectCreator.CreateShipment("S00001001", saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, false);
			InvoicingLine.AL_JH = job.PK;
			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.TestOrganisation.PK;

			AssertEquals("TaxRate should be the overriding tax rate on the charge code", taxRateOverrideForSTD.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _).PK);
			InvoicingLine.TaxRateOverrideCalculator.ReCalculate();
			AssertEquals("TaxOverride should be the tax override of the charge code", taxOverrideSTD.PK, InvoicingLine.TaxRateOverrideCalculator.CachedTaxRateOverride.PK);

			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			AssertEquals("TaxRate should be the overriding tax rate for INT transaction context on the charge code", taxRateOverrideForINT.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _).PK);
			InvoicingLine.TaxRateOverrideCalculator.ReCalculate();
			AssertEquals("TaxOverride should be the tax override for INT transaction context of the charge code", taxOverrideForINT.PK, InvoicingLine.TaxRateOverrideCalculator.CachedTaxRateOverride.PK);

			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			AssertEquals("TaxRate should be the overriding tax rate for INT transaction context on the charge code", taxRateOverrideForSTD.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _).PK);
			InvoicingLine.TaxRateOverrideCalculator.ReCalculate();
			AssertEquals("TaxOverride should null when relative registry is false", null, InvoicingLine.TaxRateOverrideCalculator.CachedTaxRateOverride);
		}

		public virtual void TestChargeCodeTaxRateOverride_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				var newBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BR1");
				newBranch.GB_OH_OrgProxy = TestObjectCreator.ActiveOrg.PK;
				GlbCompany.CurrentCompany.Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					TestObjectCreator.ActiveOrg.MainAddress.OA_RL_NKRelatedPortCode = TestObjectCreator.Creditor1.MainAddress.OA_RL_NKRelatedPortCode = "INDEL";
					TestObjectCreator.ActiveOrg.MainAddress.OA_RN_NKCountryCode = TestObjectCreator.Creditor1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
					TestObjectCreator.ActiveOrg.MainAddress.OA_State = TestObjectCreator.Creditor1.MainAddress.OA_State = "DL";
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					Factory.Save();

					var taxRateOriginal = Factory.NewWithValidTestData<AccTaxRate>();
					chargeCode.AC_AT_GSTRate = taxRateOriginal.PK;
					var overrideInvTaxMsg = ZGuid.Empty;

					var taxOverride = chargeCode.TaxOverrides.AddNew();
					taxOverride.AO_JobType = ALL;
					taxOverride.AO_IncoTerm = ALL;
					taxOverride.AO_Direction = ALL;
					taxOverride.AO_Origin = ALL;
					taxOverride.AO_Destination = ALL;
					taxOverride.AO_TaxRegCntryOrGroup = ALL;
					taxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
					taxOverride.AO_HomeCountryOrZone = "BSX";
					var taxRateOverride = Factory.NewWithValidTestData<AccTaxRate>();
					taxOverride.AO_AT = taxRateOverride.PK;

					InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.Creditor1.PK;
					InvoicingLine.GenericCharge = chargeCode.PK;
					AssertEquals("Default TaxRate", taxRateOriginal.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out overrideInvTaxMsg).PK);

					ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_IsCancelled = false;
					shipment.JS_IsForwardRegistered = true;
					shipment.JS_UniqueConsignRef = "S00001001";

					var job = JobInvoicing.Job.CreateWithMutex(Factory, shipment);
					job.JH_ParentID = shipment.PK;
					job.JH_ParentTableCode = "JS";
					job.JH_JobNum = "S00001001";
					job.JH_GB = GlbBranch.CurrentBranch.PK;
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					InvoicingLine.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(InvoicingLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

					Factory.Save();

					InvoicingLine.AL_PlaceOfSupply = "KL";
					AssertEquals("TaxRate should be the overriding tax rate on the charge code", taxRateOverride.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out overrideInvTaxMsg).PK);

					InvoicingLine.AL_PlaceOfSupply = "";
					AssertEquals("TaxRate should be the original tax rate on the charge code", taxRateOriginal.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out overrideInvTaxMsg).PK);
				}
			}
		}

		public void TestChargeCodeDefaultsPlaceOfSupplyForNonJobRelatedCostOrSellLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				SetUpInvoicingLineForIndiaPOS();

				Assert(InvoicingLine.IsGSTMandatory);
				Assert(InvoicingLine.InvoiceBase.NeedPlaceOfSupplyAtLineLevel);
				Assert(InvoicingLine.AL_JH.IsEmpty);
				Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);

				var isCostOrSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) || AccTransactionLines.RevenueLineTypes.Contains(InvoicingLine.AL_LineType);
				Assert(isCostOrSell);

				var costSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) ? CostSell.Cost : CostSell.Revenue;
				var posList = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);
				var posCode1 = posList[0].Code;
				var posCode2 = posList[1].Code;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, costSell, PlaceOfSupplyTypes.State.Code, posCode1, InvoicingLine.InvoiceBase.Header, InvoicingLine.Branch);

				var chargeCodeToAssign = InvoicingLine.AL_AC == TestObjectCreator.CC1.PK ? TestObjectCreator.CC10 : TestObjectCreator.CC1;
				InvoicingLine.AL_AC = chargeCodeToAssign.PK;
				AssertEquals(posCode1, InvoicingLine.AL_PlaceOfSupply);

				InvoicingLine.AL_PlaceOfSupply = posCode2;
				InvoicingLine.AL_AC = InvoicingLine.AL_AC;
				AssertEquals(posCode1, InvoicingLine.AL_PlaceOfSupply);

				chargeCodeToAssign = InvoicingLine.AL_AC == TestObjectCreator.CC1.PK ? TestObjectCreator.CC10 : TestObjectCreator.CC1;
				InvoicingLine.AL_AC = chargeCodeToAssign.PK;
				AssertEquals(posCode1, InvoicingLine.AL_PlaceOfSupply);
			}
		}

		public void TestClearPlaceOfSupplyWhenNoMatchingResult()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: branch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: GlbBranch.CurrentBranch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "WA", org: TestObjectCreator.Debtor1);
			PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code);

			var isCostOrSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) || AccTransactionLines.RevenueLineTypes.Contains(InvoicingLine.AL_LineType);
			Assert(isCostOrSell);
			var costSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) ? CostSell.Cost : CostSell.Revenue;

			SetupInvoiceAndLineGSTApplicability(true, false, false, true);
			Assert(InvoicingLine.IsGSTMandatory);
			Assert(InvoicingLine.InvoiceBase.NeedPlaceOfSupplyAtLineLevel);
			Assert(InvoicingLine.AL_JH.IsEmpty);
			Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);

			InvoicingLine.AL_AC = TestObjectCreator.CC1.PK;
			InvoicingLine.AL_OH = PickupOrg().PK;
			InvoicingLine.AL_GB = branch.PK;
			AssertPosNotEmpty();
			AssertPosMatchingWithChargeCodeAndOrgHeader();

			InvoicingLine.AL_AC = ZGuid.Empty;
			AssertPosMatchingWithoutChargeCode();

			InvoicingLine.AL_AC = TestObjectCreator.CC1.PK;
			InvoicingLine.AL_OH = ZGuid.Empty;
			if (InvoicingLine.InvoiceBase != null)
			{
				InvoicingLine.InvoiceBase.AH_OH = ZGuid.Empty;
			}
			AssertPosMatchingWithoutOrgHeader();

			InvoicingLine.AL_OH = PickupOrg().PK;
			InvoicingLine.InvoiceBase.AH_OH = PickupOrg().PK;
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "StateNotExisted", branch: branch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "StateNotExisted", branch: GlbBranch.CurrentBranch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "StateNotExisted", org: TestObjectCreator.Debtor1);
			AssertPostmatchingWithInvalidState();

			void AssertPosMatchingWithChargeCodeAndOrgHeader()
			{
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_AC);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_OH);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine?.InvoiceBase.AH_OH);
				AssertPosNotEmpty("PreCondition");

				InvoicingLine.AL_GB = ZGuid.Empty;
				if (costSell == CostSell.Cost)
				{
					AssertEquals("Should not be empty when matching result not empty.(it will match result with loged-in branch)", "NSW", InvoicingLine.AL_PlaceOfSupply);
				}
				else
				{
					AssertEquals("Should not be empty when matching result not empty", "WA", InvoicingLine.AL_PlaceOfSupply);
				}
				InvoicingLine.AL_GB = branch.PK;
				AssertPosNotEmpty();
			}

			void AssertPostmatchingWithInvalidState()
			{
				AssertNull("PreCondition", branch.OrgProxy.MainAddress.RelatedState);
				AssertNull("PreCondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.RelatedState);
				AssertNull("PreCondition", TestObjectCreator.Debtor1.MainAddress.RelatedState);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_AC);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_OH);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine?.InvoiceBase.AH_OH);
				AssertPosNotEmpty("PreCondition");

				InvoicingLine.AL_GB = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, InvoicingLine.AL_PlaceOfSupply);
				InvoicingLine.AL_GB = branch.PK;
				AssertEquals("Should be empty when matching result empty, when state is not existed", ZString.Empty, InvoicingLine.AL_PlaceOfSupply);
			}

			void AssertPosMatchingWithoutChargeCode()
			{
				AssertEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_AC);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_OH);
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine?.InvoiceBase.AH_OH);
				AssertPosNotEmpty("PreCondition");

				InvoicingLine.AL_GB = ZGuid.Empty;
				AssertPosNotEmpty("Will not do POS matching when Charge Code empty");
				InvoicingLine.AL_GB = branch.PK;
				AssertPosNotEmpty("Will not do POS matching when Charge Code empty");
			}

			void AssertPosMatchingWithoutOrgHeader()
			{
				AssertNotEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_AC);
				AssertEquals("PreCondition", ZGuid.Empty, InvoicingLine.AL_OH);
				AssertEquals("PreCondition", ZGuid.Empty, InvoicingLine?.InvoiceBase.AH_OH);
				AssertPosNotEmpty("PreCondition");

				InvoicingLine.AL_GB = ZGuid.Empty;
				AssertPosNotEmpty("Will not do POS matching when OrgHeader empty");
				InvoicingLine.AL_GB = branch.PK;
				AssertPosNotEmpty("Will not do POS matching when OrgHeader empty");
			}

			void AssertPosNotEmpty(string commnet = null)
			{
				var expectedPOS = costSell == CostSell.Cost ? "NSW" : "WA";
				AssertEquals(commnet ?? "Should be empty when matching result empty", expectedPOS, InvoicingLine.AL_PlaceOfSupply);
			}

			OrgHeader PickupOrg()
				=> costSell == CostSell.Cost ? TestObjectCreator.Creditor1 : TestObjectCreator.Debtor1;
		}

		public void TestChargeCodeDoesNotDefaultPlaceOfSupplyForJobRelatedLine()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLine.AL_JH = job.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				SetUpInvoicingLineForIndiaPOS();

				Assert(InvoicingLine.IsGSTMandatory);
				Assert(InvoicingLine.InvoiceBase.NeedPlaceOfSupplyAtLineLevel);
				Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);

				var isCostOrSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) || AccTransactionLines.RevenueLineTypes.Contains(InvoicingLine.AL_LineType);
				Assert(isCostOrSell);

				var costSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) ? CostSell.Cost : CostSell.Revenue;
				var posCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, costSell, PlaceOfSupplyTypes.State.Code, posCode, InvoicingLine.InvoiceBase.Header, InvoicingLine.Branch);

				var chargeCodeToAssign = InvoicingLine.AL_AC == TestObjectCreator.CC1.PK ? TestObjectCreator.CC10 : TestObjectCreator.CC1;
				InvoicingLine.AL_AC = chargeCodeToAssign.PK;
				Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);
			}
		}

		public void TestBranchDefaultsPlaceOfSupplyForNonJobRelatedCostOrSellLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				SetUpInvoicingLineForIndiaPOS();

				Assert(InvoicingLine.IsGSTMandatory);
				Assert(InvoicingLine.InvoiceBase.NeedPlaceOfSupplyAtLineLevel);
				Assert(InvoicingLine.AL_JH.IsEmpty);
				InvoicingLine.AL_AC = TestObjectCreator.CC1.PK;
				Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);

				var isCostOrSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) || AccTransactionLines.RevenueLineTypes.Contains(InvoicingLine.AL_LineType);
				Assert(isCostOrSell);

				var costSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) ? CostSell.Cost : CostSell.Revenue;
				var posList = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);
				var posCode1 = posList[0].Code;
				var posCode2 = posList[1].Code;
				var posCode3 = posList[2].Code;

				var currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
				currentBranch.OrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;
				var nonCurrentBranch = TestObjectCreator.NonCurrentBranch;
				nonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("OBR", true, true).PK;
				nonCurrentBranch.OrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, costSell, PlaceOfSupplyTypes.State.Code, posCode1, InvoicingLine.InvoiceBase.Header, nonCurrentBranch);

				InvoicingLine.AL_GB = nonCurrentBranch.PK;
				AssertEquals(posCode1, InvoicingLine.AL_PlaceOfSupply);

				InvoicingLine.AL_PlaceOfSupply = posCode2;
				InvoicingLine.AL_GB = InvoicingLine.AL_GB;
				AssertEquals(posCode1, InvoicingLine.AL_PlaceOfSupply);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, costSell, PlaceOfSupplyTypes.State.Code, posCode3, InvoicingLine.InvoiceBase.Header, currentBranch);

				InvoicingLine.AL_GB = currentBranch.PK;
				AssertEquals(posCode3, InvoicingLine.AL_PlaceOfSupply);
			}
		}

		public void TestBranchDoesNotDefaultPlaceOfSupplyForJobRelatedLine()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLine.AL_JH = job.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				SetUpInvoicingLineForIndiaPOS();

				Assert(InvoicingLine.IsGSTMandatory);
				Assert(InvoicingLine.InvoiceBase.NeedPlaceOfSupplyAtLineLevel);
				InvoicingLine.AL_AC = TestObjectCreator.CC1.PK;
				Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);

				var isCostOrSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) || AccTransactionLines.RevenueLineTypes.Contains(InvoicingLine.AL_LineType);
				Assert(isCostOrSell);

				var costSell = AccTransactionLines.CostLineTypes.Contains(InvoicingLine.AL_LineType) ? CostSell.Cost : CostSell.Revenue;
				var posCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var nonCurrentBranch = TestObjectCreator.NonCurrentBranch;
				nonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("OBR", true, true).PK;
				nonCurrentBranch.OrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, costSell, PlaceOfSupplyTypes.State.Code, posCode, InvoicingLine.InvoiceBase.Header, nonCurrentBranch);

				InvoicingLine.AL_GB = nonCurrentBranch.PK;
				Assert(InvoicingLine.AL_PlaceOfSupply.IsEmpty);
			}
		}

		void SetUpInvoicingLineForIndiaPOS()
		{
			TestObjectCreator.ABIGAS.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.ABIGAS.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;
			InvoicingLine.Branch.OrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.India;

			AssertNotNull("InvoicingLine.InvoiceBase", InvoicingLine.InvoiceBase);
			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.ABIGAS.PK;
		}

		public virtual void TestTaxRateOverrideWorksForAPInvoices()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			var taxRateOriginal = newFactory.NewWithValidTestData<AccTaxRate>();
			chargeCode.AC_AT_GSTRate = taxRateOriginal.PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_JobType = ALL;
			taxOverride.AO_IncoTerm = ALL;
			taxOverride.AO_Direction = ALL;
			taxOverride.AO_Origin = "US";
			taxOverride.AO_Destination = "ONTZ";
			taxOverride.AO_TaxRegCntryOrGroup = ALL;
			taxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			AccTaxRate taxRateOverride = newFactory.NewWithValidTestData<AccTaxRate>();
			taxOverride.AO_AT = taxRateOverride.PK;
			newFactory.Save();

			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertNull("TaxRate should be null because GenericJob is not set", InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _));

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001001", "US2TT", "CAACT", saveIt: true);
			Job job = TestObjectCreator.CreateJob(shipment, false);
			InvoicingLine.AL_JH = job.PK;
			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.TestOrganisation.PK;
			AssertEquals("TaxRate should be the overriding tax rate on the charge code", taxRateOverride.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _).PK);

			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0M);
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1.0M, 100M);
			line.AL_JH = job.PK;
			line.AL_AC = chargeCode.PK;
			AssertEquals("TaxRate should be the overriding tax rate on the charge code", taxRateOverride.PK, line.AL_AT);
		}

		public virtual void TestChargeCodeTaxRateOverrideWithCustomsStatus()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			var taxRateOriginal = newFactory.NewWithValidTestData<AccTaxRate>();
			chargeCode.AC_AT_GSTRate = taxRateOriginal.PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_JobType = ALL;
			taxOverride.AO_IncoTerm = ALL;
			taxOverride.AO_Direction = ALL;
			taxOverride.AO_Origin = ALL;
			taxOverride.AO_Destination = ALL;
			taxOverride.AO_TaxRegCntryOrGroup = ALL;
			taxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride.AO_CustomsStatus = "T1";
			AccTaxRate taxRateOverride = newFactory.NewWithValidTestData<AccTaxRate>();
			taxOverride.AO_AT = taxRateOverride.PK;
			newFactory.Save();

			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertNull("TaxRate should be null because GenericJob is not set", InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _));

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001001", saveIt: true);
			Job job = TestObjectCreator.CreateJob(shipment, false);
			InvoicingLine.AL_JH = job.PK;
			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.TestOrganisation.PK;
			AssertNotEquals("TaxRate should be not expected because CustomsStatus is not set", taxRateOverride.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _).PK);

			shipment.CustomsEntryNumberType = "T1";
			AssertEquals("TaxRate should be the overriding tax rate on the charge code", taxRateOverride.PK, InvoicingLine.GetChargeCodeTaxRateOverride_ForTestOnly(InvoicingLine.ChargeCode, out _).PK);
		}

		#endregion

		public virtual void TestSetDefaultAL_JH()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(newFactory);
			ForwardingShipment shipment = testObjectCreator.CreateShipment("S00001234");
			Job job = testObjectCreator.CreateJob(shipment);
			newFactory.Save();

			((InvoicingBase)InvoicingLine.MasterTransactionHeader).SubmittedFromInvoicingForm = true;
			((InvoicingBase)InvoicingLine.MasterTransactionHeader).SetIsReversing(false);
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("InvoicingLine's AL_JH should be empty", InvoicingLine.AL_JH.IsEmpty);
		}

		public void TestShouldDefaultGenericJob()
		{
			InvoicingBase invBase = (InvoicingBase)InvoicingLine.MasterTransactionHeader;
			invBase.SetIsReversing(true);
			invBase.SubmittedFromInvoicingForm = false;
			Assert("ShouldDefaultGenericJob should be false", !InvoicingLine.ShouldDefaultAL_JH_ForTestOnly);
			invBase.SetIsReversing(false);
			Assert("ShouldDefaultGenericJob should be false", !InvoicingLine.ShouldDefaultAL_JH_ForTestOnly);
			invBase.SubmittedFromInvoicingForm = true;
			Assert("ShouldDefaultGenericJob should be true", InvoicingLine.ShouldDefaultAL_JH_ForTestOnly);
		}

		public virtual void TestDefaultPreviousLineJobToNextLine()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ForwardingShipment shipment = newFactory.NewWithValidTestData<ForwardingShipment>();
			newFactory.Save();

			InvoicingBase invBase = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
			invBase.SubmittedFromInvoicingForm = true;
			invBase.SetIsReversing(false);
			InvoicingLineBase invLine = invBase.Lines.AddNew() as InvoicingLineBase;

			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				invLine.AL_JH = job.PK;
			}

			InvoicingLineBase invLine2 = invBase.Lines.AddNew() as InvoicingLineBase;
			Assert("Generic Job should not be copied to the next line", invLine2.AL_JH.IsEmpty);
		}

		public void TestCalculateGSTOnlySetsFieldWhenGenericChargeIsValid()
		{
			TestObjectCreator.AALSHI.CompanyData.SetARTaxApplicable(ZBool.True);
			AccChargeCode testGSTChargeCode = TestObjectCreator.CreateChargeCode("XXX", "DESCRIPTION", "MRG", 100m, TestObjectCreator.GST1, null);
			AccChargeCode testGSTFREEChargeCode = TestObjectCreator.CreateChargeCode("YYY", "DESCRIPTION", "MRG", 100m, TestObjectCreator.GSTFREE1, null);
			testGSTChargeCode.Factory.Save();

			InvoicingLine.InvoiceBase.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLine.AL_JH = TestObjectCreator.Job1.PK;
			Assert("Should be no error on GST field as yet", !InvoicingLine.AL_ATInfo.HasErrors());

			InvoicingLine.GenericCharge = testGSTChargeCode.PK;
			InvoicingLine.AL_JH = TestObjectCreator.Job2.PK;
			AssertEquals("Should set correct GST ID", TestObjectCreator.GST1.PK, InvoicingLine.AL_AT);

			InvoicingLine.GenericCharge = ZGuid.Empty;
			InvoicingLine.AL_AT = GST10Rate.PK;
			InvoicingLine.AL_JH = TestObjectCreator.Job1.PK;
			AssertEquals("Should not reset the GST ID", GST10Rate.PK, InvoicingLine.AL_AT);

			InvoicingLine.GenericCharge = testGSTFREEChargeCode.PK;
			AssertEquals("Should reset correct GST ID", TestObjectCreator.GSTFREE1.PK, InvoicingLine.AL_AT);
		}

		public void TestGSTInclusiveAmountReadOnly_ForSisterCompanyAPInvoice()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 50M);

			invoice.IsConvertedFromARInvoice = false;

			invoice.GSTInclusiveAmounts = true;
			Assert("GSTInclusiveAmount should be editable as GST inclusive checkbox ticked", !invoice.Lines[0].GSTInclusiveAmountInfo.ReadOnly);

			invoice.GSTInclusiveAmounts = false;
			Assert("GSTInclusiveAmount should be readonly as GST inclusive checkbox unticked", invoice.Lines[0].GSTInclusiveAmountInfo.ReadOnly);

			invoice.IsConvertedFromARInvoice = true;

			invoice.GSTInclusiveAmounts = true;
			Assert("GSTInclusiveAmount should be readonly as invoice posted from sister company, though checkbox ticked", invoice.Lines[0].GSTInclusiveAmountInfo.ReadOnly);

			invoice.GSTInclusiveAmounts = false;
			Assert("GSTInclusiveAmount should be readonly as invoice posted from sister company", invoice.Lines[0].GSTInclusiveAmountInfo.ReadOnly);
		}

		public void TestGSTInclusiveAmountInfo()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(MasterHeaderType) as InvoicingBase;
			AssertNotNull(newInvoice);

			newInvoice.Lines.AddNew();
			newInvoice.Lines.AddNew();
			newInvoice.Lines[0].AL_OSExTaxAmount = 120m;
			newInvoice.Lines[1].AL_OSExTaxAmount = 33m;
			newInvoice.GSTInclusiveAmounts = false;

			Assert("GSTInclusiveAmount should be readonly.", newInvoice.Lines[0].GSTInclusiveAmountInfo.ReadOnly);
			Assert("GSTInclusiveAmount should be readonly.", newInvoice.Lines[1].GSTInclusiveAmountInfo.ReadOnly);

			newInvoice.GSTInclusiveAmounts = true;

			Assert("GSTInclusiveAmount shouldn't be readonly.", !newInvoice.Lines[0].GSTInclusiveAmountInfo.ReadOnly);
			Assert("GSTInclusiveAmount shouldn't be readonly.", !newInvoice.Lines[1].GSTInclusiveAmountInfo.ReadOnly);
		}

		public void TestGSTInclusiveAmount()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(MasterHeaderType) as InvoicingBase;
			AssertNotNull(newInvoice);

			newInvoice.Lines.AddNew();
			InvoicingLineBase testInvoiceLine = newInvoice.Lines[0];
			testInvoiceLine.AL_AT = TestObjectCreator.CreateTaxRate("TestGST", "", 10).PK;
			testInvoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			testInvoiceLine.AL_OSExTaxAmount = 120m;

			newInvoice.GSTInclusiveAmounts = false;
			testInvoiceLine.GSTInclusiveAmount = 77m;
			AssertEquals("AL_OSExTaxAmount should not be changed.", 120m, testInvoiceLine.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount should not be changed.", 12m, testInvoiceLine.AL_OSTaxAmount);

			newInvoice.GSTInclusiveAmounts = true;
			testInvoiceLine.GSTInclusiveAmount = 11m;
			AssertEquals("AL_OSExTaxAmount should be changed.", 10m, testInvoiceLine.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount should be changed.", 1m, testInvoiceLine.AL_OSTaxAmount);

			testInvoiceLine.GSTInclusiveAmount = 11.333m;
			AssertEquals("GSTInclusiveAmount should be rounded.", 11.33m, testInvoiceLine.GSTInclusiveAmount);
			AssertEquals("AL_OSExTaxAmount should be rounded.", 10.30m, testInvoiceLine.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount should be rounded.", 1.03m, testInvoiceLine.AL_OSTaxAmount);

			newInvoice.GSTInclusiveAmountNeedUpdate = false;
			newInvoice.GSTInclusiveAmounts = false;
			testInvoiceLine.AL_OSExTaxAmount = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 12.1m, testInvoiceLine.GSTInclusiveAmount);
			testInvoiceLine.AL_OSTaxAmount = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 22m, testInvoiceLine.GSTInclusiveAmount);

			newInvoice.GSTInclusiveAmounts = true;
			testInvoiceLine.GSTInclusiveAmount = 333m;
			AssertEquals("GSTInclusiveAmount should be updated.", 333m, testInvoiceLine.GSTInclusiveAmount);
			testInvoiceLine.AL_OSExTaxAmount = 22m;
			AssertEquals("GSTInclusiveAmount should not be updated.", 333m, testInvoiceLine.GSTInclusiveAmount);
			testInvoiceLine.AL_OSTaxAmount = 22m;
			AssertEquals("GSTInclusiveAmount should not be updated.", 333m, testInvoiceLine.GSTInclusiveAmount);

			newInvoice.GSTInclusiveAmountNeedUpdate = true;
			newInvoice.GSTInclusiveAmounts = false;
			testInvoiceLine.AL_OSExTaxAmount = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 12.1m, testInvoiceLine.GSTInclusiveAmount);
			testInvoiceLine.AL_OSTaxAmount = 11m;
			AssertEquals("GSTInclusiveAmount should be updated.", 22m, testInvoiceLine.GSTInclusiveAmount);

			newInvoice.GSTInclusiveAmounts = true;
			testInvoiceLine.GSTInclusiveAmount = 333m;
			AssertEquals("GSTInclusiveAmount should be updated.", 333m, testInvoiceLine.GSTInclusiveAmount);
			testInvoiceLine.AL_OSExTaxAmount = 22m;
			AssertEquals("GSTInclusiveAmount should be updated.", 24.2m, testInvoiceLine.GSTInclusiveAmount);
			testInvoiceLine.AL_OSTaxAmount = 22m;
			AssertEquals("GSTInclusiveAmount should be updated.", 44m, testInvoiceLine.GSTInclusiveAmount);
		}

		#region TestGSTInclusiveAmountCanChangeLineValues

		public void TestGSTInclusiveAmountCanChangeLineValues()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(MasterHeaderType) as InvoicingBase;
			AssertNotNull(newInvoice);

			newInvoice.Lines.ApportionedInvoiceLineModified += new ApportionedInvoiceLineModifiedEventHandler(Lines_ApportionedInvoiceLineModified);
			newInvoice.Lines.AddNew();
			InvoicingLineBase testInvoiceLine = newInvoice.Lines[0];

			testInvoiceLine.ApportionmentChargeImportedFrom = Factory.New<ApportionSplitCharge>();
			Assert(!AppLineModifiedRaised);
			testInvoiceLine.GSTInclusiveAmount = 123m;
			Assert("Should have raised apportioned line modified", AppLineModifiedRaised);
		}

		bool AppLineModifiedRaised;
		void Lines_ApportionedInvoiceLineModified(InvoicingLineBase sender, EventArgs e)
		{
			AppLineModifiedRaised = true;
		}

		#endregion

		public void TestAL_ExchangeRateNotRaiseApportionedLineModified()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(MasterHeaderType) as InvoicingBase;
			newInvoice.AH_ExchangeRate = 2.0;
			AssertNotNull(newInvoice);

			newInvoice.Lines.ApportionedInvoiceLineModified += new ApportionedInvoiceLineModifiedEventHandler(Lines_ApportionedInvoiceLineModified);
			newInvoice.Lines.AddNew();
			InvoicingLineBase testInvoiceLine = newInvoice.Lines[0];

			testInvoiceLine.ApportionmentChargeImportedFrom = Factory.New<ApportionSplitCharge>();
			AssertEquals(2m, testInvoiceLine.AL_ExchangeRate);
			AssertEquals(false, AppLineModifiedRaised);
			testInvoiceLine.AL_ExchangeRate = 1.5;
			AssertEquals(2m, testInvoiceLine.AL_ExchangeRate);
			AssertEquals(true, AppLineModifiedRaised);
		}

		public void TestReadOnlyForInvoiceLinesWhenApproving()
		{
			if (InvoicingLine.LineType_ForTestOnly == TransactionLineTypes.UnapprovedCost)
			{
				AssertEquals("Before conversion", false, InvoicingLine.GenericChargeInfo.ReadOnly);
				AssertEquals("Before conversion", false, InvoicingLine.AL_DescInfo.ReadOnly);
				AssertEquals("Before conversion", false, InvoicingLine.AL_JHInfo.ReadOnly);
				AssertEquals("Before conversion", false, InvoicingLine.AL_GBInfo.ReadOnly);
				AssertEquals("Before conversion", false, InvoicingLine.AL_GEInfo.ReadOnly);
				AssertEquals("Before conversion", false, InvoicingLine.AL_IsFinalChargeInfo.ReadOnly);

				UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
				invoice.AH_Ledger = "AP";
				invoice.AH_TransactionType = "INV";

				Factory.Save();

				InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

				JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_AL_APLine = invoiceLine.PK;

				InvoicingLine.RefreshBinding();
				AssertEquals("After conversion", true, InvoicingLine.GenericChargeInfo.ReadOnly);
				AssertEquals("After conversion", true, InvoicingLine.AL_DescInfo.ReadOnly);
				AssertEquals("After conversion", true, InvoicingLine.AL_JHInfo.ReadOnly);
				AssertEquals("After conversion", true, InvoicingLine.AL_GBInfo.ReadOnly);
				AssertEquals("After conversion", true, InvoicingLine.AL_GEInfo.ReadOnly);
				AssertEquals("After conversion", true, InvoicingLine.AL_IsFinalChargeInfo.ReadOnly);
			}
			else
			{
				InvoicingLine.RefreshBinding();
				AssertEquals("For not UA lines", false, InvoicingLine.GenericChargeInfo.ReadOnly);
				AssertEquals("For not UA lines", false, InvoicingLine.AL_DescInfo.ReadOnly);
				AssertEquals("For not UA lines", MasterHeader.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable,
														InvoicingLine.AL_JHInfo.ReadOnly);
				AssertEquals("For not UA lines", false, InvoicingLine.AL_GBInfo.ReadOnly);
				AssertEquals("For not UA lines", false, InvoicingLine.AL_GEInfo.ReadOnly);
				AssertEquals("For not UA lines", false, InvoicingLine.AL_IsFinalChargeInfo.ReadOnly);
			}
		}

		public void TestAL_Desc_ReadOnly()
		{
			var isRevenueLine = InvoicingLine.LineType_ForTestOnly == ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = true;
			InvoicingLine.AL_AC = TestObjectCreator.CC1.PK;
			InvoicingLine.RefreshBinding();

			var securityCheckPoint = InvoicingLine.ModifyDefaultChargeCodeDescription_ForTestOnly;
			Assert(securityCheckPoint.IsAllowed);
			Assert(!InvoicingLine.AL_Desc_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = false;
			AssertEquals(!isRevenueLine, securityCheckPoint.IsAllowed);
			AssertEquals(isRevenueLine, InvoicingLine.AL_Desc_ReadOnly_ForTestOnly);

			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = false;
			securityCheckPoint.IsAllowed = true;
			Assert(securityCheckPoint.IsAllowed);
			AssertEquals(isRevenueLine, InvoicingLine.AL_Desc_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = false;
			AssertEquals(!isRevenueLine, securityCheckPoint.IsAllowed);
			AssertEquals(isRevenueLine, InvoicingLine.AL_Desc_ReadOnly_ForTestOnly);
		}

		public void TestAL_Sequence_ReadOnly()
		{
			AssertEquals("Pre-Condition: Not InDatabase", false, InvoicingLine.IsInDatabase);
			AssertNotNull("Pre-Condition: SHould have Header", InvoicingLine.TransactionHeader);

			AssertEquals(false, InvoicingLine.AL_Sequence_ReadOnly);

			Factory.Save();
			AssertEquals("InDatabase", true, InvoicingLine.IsInDatabase);
			bool expected = InvoicingLine.TransactionHeader.AH_Ledger != LedgerTypes.IncompleteTransactions &&
				InvoicingLine.TransactionHeader.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions;
			AssertEquals(expected, InvoicingLine.AL_Sequence_ReadOnly);
		}

		public void TestTransLinePaysTotalAmount()
		{
			AccTransLinePay line1 = InvoicingLine.TransLinePays.AddNew();
			AccTransLinePay line2 = InvoicingLine.TransLinePays.AddNew();
			line1.A7_Amount = 10m;
			line2.A7_Amount = 20m;
			AssertEquals(30m, InvoicingLine.TransLinePaysTotalAmount);
		}

		#region IDescriptionSetter Tests

		public void TestIDescriptionSetterImplementation_Description()
		{
			var descriptionSetter = InvoicingLine as IDescriptionSetter;

			descriptionSetter.Description = "TEST-DESCRIPTIONTEXT-AAAA";
			AssertEquals("TEST-DESCRIPTIONTEXT-AAAA", InvoicingLine.AL_Desc);

			descriptionSetter.Description = ZString.Empty;
			AssertEquals(ZString.Empty, InvoicingLine.AL_Desc);
		}

		#endregion

		#region ILineMatching Tests

		public void TestIsFullyPay()
		{
			ILineMatching lineMatching = InvoicingLine;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", true, lineMatching.IsFullyPay);

			InvoicingLine.SetOutstandingAmount_ForTestOnly(15.33m);
			lineMatching.IsFullyPay = false;
			AssertEquals(ZDecimal.Zero, lineMatching.PaidAmount);

			lineMatching.IsFullyPay = true;
			AssertEquals(15.33m, lineMatching.PaidAmount);
		}

		public void TestIsFullyPaidResetWhenPaidAmountIsNotEqualOustandingAmount()
		{
			if (InvoicingLine.TransactionHeader.AH_Ledger != "UA")
			{
				JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_JH = jobHeader.PK;
				jobCharge.JR_AL_APLine = InvoicingLine.PK;
				jobCharge.JR_RX_NKCostCurrency = "AUD";
				InvoicingLine.AL_JH = jobHeader.PK;
				ILineMatching lineMatching = InvoicingLine;
				lineMatching.SetDefaultValues();
				AssertEquals("Default value", true, lineMatching.IsFullyPay);

				InvoicingLine.SetOutstandingAmount_ForTestOnly(15.33m);
				lineMatching.IsFullyPay = true;
				AssertEquals(15.33m, lineMatching.PaidAmount);
				lineMatching.PaidAmount = 12.22m;
				Assert("IsFullyPay should be reset to false", !lineMatching.IsFullyPay);

				lineMatching.PaidAmountInChargeCurrency = 15.33m;
				Assert("IsFullyPay should be reset to true", lineMatching.IsFullyPay);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSetDefaultValues()
		{
			InvoicingLine.AL_LineType = "CST";
			AccTransLinePay line1 = InvoicingLine.TransLinePays.AddNew();
			AccTransLinePay line2 = InvoicingLine.TransLinePays.AddNew();
			line1.A7_Amount = 10m;
			line2.A7_Amount = 20m;
			InvoicingLine.AL_OSAmount = 40;
			ILineMatching lineMatching = InvoicingLine;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", 10m, lineMatching.OriginalPaidAmount);
			AssertEquals("Default value", 10m, lineMatching.PaidAmount);
			AssertEquals("Default value", 10m, lineMatching.OutstandingAmount);
			AssertEquals("Default value", true, lineMatching.IsFullyPay);
			AssertEquals("Default value", 0m, lineMatching.PaidAmountInChargeCurrency);

			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AL_APLine = InvoicingLine.PK;
			jobCharge.JR_RX_NKCostCurrency = currency.RX_Code;
			InvoicingLine.AL_JH = jobHeader.PK;
			InvoicingLine.AL_OSAmount = 0;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", -30m, lineMatching.PaidAmountInChargeCurrency);

			line1.A7_Amount = 0m;
			line2.A7_Amount = 0m;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", 0m, lineMatching.PaidAmountInChargeCurrency);

			InvoicingLine.AL_OSExTaxAmount = 30;
			InvoicingLine.AL_OSTaxAmount = 10;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", "AUD", lineMatching.AL_RX_NKTransactionCurrency);
			AssertEquals("Default value", 1m, lineMatching.AL_ExchangeRate);
			AssertEquals("Default value", 40m, lineMatching.AL_OSAmount * InvoicingLine.Multiplier_ForTestOnly);
			AssertEquals("Default value", 30m, lineMatching.AL_OSExTaxAmount);
			AssertEquals("Default value", 10m, lineMatching.AL_OSTaxAmount);

			InvoicingLine.AL_RX_NKTransactionCurrency = "USD";
			InvoicingLine.AL_ExchangeRate = 0.5m;
			InvoicingLine.AL_OSExTaxAmount = 30;
			InvoicingLine.AL_OSTaxAmount = 10;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", "AUD", lineMatching.AL_RX_NKTransactionCurrency);
			AssertEquals("Default value", 1m, lineMatching.AL_ExchangeRate);
			AssertEquals("Default value", 80m, lineMatching.AL_OSAmount * InvoicingLine.Multiplier_ForTestOnly);
			AssertEquals("Default value", 60m, lineMatching.AL_OSExTaxAmount);
			AssertEquals("Default value", 20m, lineMatching.AL_OSTaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingLine.AL_AT = Factory.New<AccTaxRate>().PK;
			InvoicingLine.AL_TaxRateNumerator = 10;
			InvoicingLine.AL_TaxRateDenominator = 2;
			InvoicingLine.AL_TaxExtraRateNumerator = 6;
			InvoicingLine.AL_TaxExtraRateDenominator = 3;
			InvoicingLine.AL_OSTaxAmount = 10;
			lineMatching.SetDefaultValues();
			AssertEquals("Default value", "AUD", lineMatching.AL_RX_NKTransactionCurrency);
			AssertEquals("Default value", 1m, lineMatching.AL_ExchangeRate);
			AssertEquals("Default value", 80m, lineMatching.AL_OSAmount * InvoicingLine.Multiplier_ForTestOnly);
			AssertEquals("Default value", 60m, lineMatching.AL_OSExTaxAmount);
			AssertEquals("Default value", 4.26m, lineMatching.AL_OSTaxAmount);
		}

		public void TestUpdateOriginalAmounts()
		{
			ILineMatching lineMatching = InvoicingLine;
			lineMatching.PaidAmount = 10m;
			AssertEquals(0m, lineMatching.OriginalPaidAmount);
			lineMatching.UpdateOriginalAmounts();
			AssertEquals(10m, lineMatching.OriginalPaidAmount);
		}

		public void TestResetAmounts()
		{
			ILineMatching lineMatching = InvoicingLine;
			lineMatching.PaidAmount = 10m;
			lineMatching.PaidAmountInChargeCurrency = 20m;
			lineMatching.ResetAmounts();
			AssertEquals(0m, lineMatching.PaidAmount);
			AssertEquals(0m, lineMatching.PaidAmountInChargeCurrency);
		}

		public void TestCharge()
		{
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AL_APLine = InvoicingLine.PK;
			InvoicingLine.AL_JH = jobHeader.PK;
			ILineMatching lineMatching = InvoicingLine;
			AssertEquals(lineMatching.Charge.PK, jobCharge.PK);
		}

		public void TestChargeCurrency()
		{
			RefCurrency currencyAUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			RefCurrency currencyUSD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AL_APLine = InvoicingLine.PK;
			jobCharge.JR_RX_NKCostCurrency = currencyAUD.RX_Code;
			jobCharge.JR_RX_NKSellCurrency = currencyUSD.RX_Code;
			InvoicingLine.AL_JH = jobHeader.PK;
			ILineMatching lineMatching = InvoicingLine;

			InvoicingLine.AL_LineType = "CST";
			AssertEquals(currencyAUD.RX_Code, lineMatching.ChargeCurrency);

			InvoicingLine.AL_LineType = "REV";
			AssertEquals(currencyUSD.RX_Code, lineMatching.ChargeCurrency);

			InvoicingLine.AL_LineType = "TST";
			AssertEquals(ZString.Empty, lineMatching.ChargeCurrency);
		}

		public void TestChargeExRate()
		{
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AL_APLine = InvoicingLine.PK;
			jobCharge.JR_RX_NKSellCurrency = "USD";
			jobCharge.JR_RX_NKCostCurrency = "USD";
			jobCharge.JR_OSCostExRate = 3m;
			((Charge)jobCharge).RevenueExchangeRate.SetBuyRate_ForTestOnly(2m);
			InvoicingLine.AL_JH = jobHeader.PK;
			ILineMatching lineMatching = InvoicingLine;

			InvoicingLine.AL_LineType = "CST";
			AssertEquals(3m, lineMatching.ChargeExRate);

			InvoicingLine.AL_LineType = "REV";
			AssertEquals(2m, lineMatching.ChargeExRate);

			InvoicingLine.AL_LineType = "TST";
			AssertEquals(ZDecimal.Zero, lineMatching.ChargeExRate);
		}

		public void TestChargeAmount()
		{
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AL_APLine = InvoicingLine.PK;
			jobCharge.JR_OSCostAmt = 10m;
			jobCharge.JR_OSCostGSTAmt_Calc = 5m;
			jobCharge.JR_OSSellAmt = 20m;
			jobCharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			InvoicingLine.AL_JH = jobHeader.PK;
			ILineMatching lineMatching = InvoicingLine;

			ZDecimal creditNoteMultiplier = InvoicingLine.InvoiceBase.AH_TransactionType == TransactionTypes.CreditNote ? -1.0m : 1.0m;
			InvoicingLine.AL_LineType = "CST";
			AssertEquals(15m * InvoicingLine.Multiplier_ForTestOnly * creditNoteMultiplier, lineMatching.ChargeAmount);

			InvoicingLine.AL_LineType = "REV";
			AssertEquals(22m * InvoicingLine.Multiplier_ForTestOnly * creditNoteMultiplier, lineMatching.ChargeAmount);

			InvoicingLine.AL_LineType = "TST";
			AssertEquals(ZDecimal.Zero, lineMatching.ChargeAmount);
		}

		public void TestPaidAmountInChargeCurrency()
		{
			if (InvoicingLine.TransactionHeader.AH_Ledger != "UA")
			{
				JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_JH = jobHeader.PK;
				jobCharge.JR_AL_APLine = InvoicingLine.PK;
				jobCharge.JR_OSCostAmt = 10m;
				jobCharge.JR_OSCostGSTAmt_Calc = 5m;
				jobCharge.JR_OSSellAmt = 20m;
				jobCharge.JR_RX_NKCostCurrency = "USD";
				jobCharge.JR_OSCostExRate = 2m;
				jobCharge.JR_RX_NKSellCurrency = "USD";
				((Charge)jobCharge).RevenueExchangeRate.SetBuyRate_ForTestOnly(2m);
				InvoicingLine.AL_JH = jobHeader.PK;
				ILineMatching lineMatching = InvoicingLine;
				lineMatching.SetDefaultValues();

				lineMatching.PaidAmountInChargeCurrency = 100;
				AssertEquals(50m, lineMatching.PaidAmount);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestPaidAmount()
		{
			if (InvoicingLine.TransactionHeader.AH_Ledger != "UA")
			{
				JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_JH = jobHeader.PK;
				jobCharge.JR_AL_APLine = InvoicingLine.PK;
				jobCharge.JR_OSCostAmt = 10m;
				jobCharge.JR_OSCostGSTAmt_Calc = 5m;
				jobCharge.JR_OSSellAmt = 20m;
				jobCharge.JR_RX_NKCostCurrency = "USD";
				jobCharge.JR_OSCostExRate = 2m;
				jobCharge.JR_RX_NKSellCurrency = "USD";
				((Charge)jobCharge).RevenueExchangeRate.SetBuyRate_ForTestOnly(2m);
				InvoicingLine.AL_JH = jobHeader.PK;
				ILineMatching lineMatching = InvoicingLine;
				InvoicingLine.AL_OSAmount = 200m;
				InvoicingLine.AL_LineAmount = 200m;
				lineMatching.SetDefaultValues();

				Assert(lineMatching.IsFullyPay);
				lineMatching.PaidAmount = 100;
				Assert(!lineMatching.IsFullyPay);
				lineMatching.PaidAmount = 200;
				Assert(lineMatching.IsFullyPay);
				lineMatching.PaidAmount = 100;
				AssertEquals(200m, lineMatching.PaidAmountInChargeCurrency);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestAL_JHRaiseOnJobChanged

		public void TestAL_JHRaiseOnJobChanged()
		{
			OnJobChangedWasRaised = false;
			ExpectedNewJob = null;

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				InvoicingLine.AL_JH = job.PK;
				Assert("Event is not subscribed.", !OnJobChangedWasRaised);

				InvoicingLine.InvoiceBase.OnJobChanged += new EventHandler<ChangedBizoEventArgs>(InvoiceBase_OnJobChanged);
				InvoicingLine.AL_JH = job.PK;
				Assert("Job is not changed.", !OnJobChangedWasRaised);

				InvoicingLine.AL_JH = ZGuid.NewZGuid();
				Assert("Job does not exist.", !OnJobChangedWasRaised);

				InvoicingLine.AL_JH = job.PK;
				Assert("Event should be raised.", OnJobChangedWasRaised);
				AssertEquals("Correct Job should be passed to event handler.", job, ExpectedNewJob);
			}
		}

		void InvoiceBase_OnJobChanged(object sender, ChangedBizoEventArgs e)
		{
			OnJobChangedWasRaised = true;
			ExpectedNewJob = e.NewBusinessObject as Job;
		}

		bool OnJobChangedWasRaised;
		Job ExpectedNewJob;

		#endregion

		#region TestApportionedLineModifiedIsRaisedIfNotCanChangeLineValues

		public void TestApportionedLineModifiedIsRaised_AL_A9_VATClass()
		{
			var invoice = (InvoicingBase)MasterHeader;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.Lines.Add(InvoicingLine);
			InvoicingLine.AL_OSExTaxAmount = 100m;
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = TestObjectCreator.CC1.PK;

			invoice.Lines.ApportionedInvoiceLineModified += OnApportionedInvoiceLineModified;
			HasApportionedInvoiceLineModifiedEventBeenRaised = false;
			ApportionedInvoiceLineModifiedEventBeenRaisedCount = 0;

			charge.JR_E6 = ZGuid.NewZGuid();
			InvoicingLine.ApportionmentChargeImportedFrom = charge;
			AssertEquals("Pre-condition: CanChangeLineValues_ForTestOnly should be false", false, InvoicingLine.CanChangeLineValues_ForTestOnly);
			var originalVATClass = InvoicingLine.AL_A9_VATClass;
			InvoicingLine.AL_A9_VATClass = Guid.NewGuid();
			AssertEquals("VAT Class was not modify", originalVATClass, InvoicingLine.AL_A9_VATClass);
			AssertEquals("CanChangeLineValues_ForTestOnly", false, InvoicingLine.CanChangeLineValues_ForTestOnly);
			Assert("Should have raised event", HasApportionedInvoiceLineModifiedEventBeenRaised);
			AssertEquals("Should have raised event once", 1, ApportionedInvoiceLineModifiedEventBeenRaisedCount);
		}

		public void TestApportionedLineModifiedIsRaised_Currency()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.Lines.Add(InvoicingLine);
			InvoicingLine.AL_OSExTaxAmount = 100m;
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			invoice.Lines.ApportionedInvoiceLineModified += OnApportionedInvoiceLineModified;
			HasApportionedInvoiceLineModifiedEventBeenRaised = false;
			ApportionedInvoiceLineModifiedEventBeenRaisedCount = 0;

			charge.JR_E6 = ZGuid.NewZGuid();
			InvoicingLine.ApportionmentChargeImportedFrom = charge;
			InvoicingLine.ExchangeRate.Currency = "USD";
			AssertEquals("CanChangeLineValues_ForTestOnly", false, InvoicingLine.CanChangeLineValues_ForTestOnly);
			Assert("Should have raised event", HasApportionedInvoiceLineModifiedEventBeenRaised);
			AssertEquals("Should have raised event once", 1, ApportionedInvoiceLineModifiedEventBeenRaisedCount);
		}

		public void TestApportionedLineModifiedIsRaised_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				var fposList = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);
				var fposCode1 = fposList[0].Code;

				var invoice = (InvoicingBase)MasterHeader;
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.Lines.Add(InvoicingLine);
				InvoicingLine.AL_OSExTaxAmount = 100m;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				var charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				charge.JR_JH = job.PK;
				charge.JR_LocalCostAmt = 10m;
				charge.JR_OSCostAmt = 10m;
				charge.JR_AC = TestObjectCreator.CC1.PK;
				Factory.Save();

				invoice.Lines.ApportionedInvoiceLineModified += OnApportionedInvoiceLineModified;
				HasApportionedInvoiceLineModifiedEventBeenRaised = false;
				ApportionedInvoiceLineModifiedEventBeenRaisedCount = 0;

				charge.JR_E6 = ZGuid.NewZGuid();
				InvoicingLine.ApportionmentChargeImportedFrom = charge;
				InvoicingLine.AL_PlaceOfSupply = fposCode1;
				AssertEquals("CanChangeLineValues_ForTestOnly", false, InvoicingLine.CanChangeLineValues_ForTestOnly);
				Assert("Should have raised event", HasApportionedInvoiceLineModifiedEventBeenRaised);
				AssertEquals("Should have raised event once", 1, ApportionedInvoiceLineModifiedEventBeenRaisedCount);
			}
		}

		public void TestApportionedLineModifiedIsRaised_GovtChargeCode()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.Lines.Add(InvoicingLine);
			InvoicingLine.AL_OSExTaxAmount = 100m;
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_CostGovtChargeCode = "Hello";
			Factory.Save();

			invoice.Lines.ApportionedInvoiceLineModified += OnApportionedInvoiceLineModified;
			HasApportionedInvoiceLineModifiedEventBeenRaised = false;
			ApportionedInvoiceLineModifiedEventBeenRaisedCount = 0;

			charge.JR_E6 = ZGuid.NewZGuid();
			InvoicingLine.ApportionmentChargeImportedFrom = charge;
			InvoicingLine.AL_GovtChargeCode = "World";
			AssertEquals("CanChangeLineValues_ForTestOnly", false, InvoicingLine.CanChangeLineValues_ForTestOnly);
			Assert("Should have raised event", HasApportionedInvoiceLineModifiedEventBeenRaised);
			AssertEquals("Should have raised event once", 1, ApportionedInvoiceLineModifiedEventBeenRaisedCount);
		}

		public void TestApportionedLineModifiedIsRaisedIfNotCanChangeLineValues()
		{
			InvoicingBase invoice = (InvoicingBase)MasterHeader;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.Lines.Add(InvoicingLine);
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			AssertNotNull("InvoicingLine.TransactionHeader", InvoicingLine.TransactionHeader);
			invoice.Lines.ApportionedInvoiceLineModified += OnApportionedInvoiceLineModified;
			HasApportionedInvoiceLineModifiedEventBeenRaised = false;
			AssertEquals("CanChangeLineValues_ForTestOnly", true, InvoicingLine.CanChangeLineValues_ForTestOnly);
			InvoicingLine.AL_JH = job.PK;
			Assert("Should not have raised event", !HasApportionedInvoiceLineModifiedEventBeenRaised);

			charge.JR_E6 = ZGuid.NewZGuid();
			InvoicingLine.ApportionmentChargeImportedFrom = charge;

			AssertEquals("CanChangeLineValues_ForTestOnly", false, InvoicingLine.CanChangeLineValues_ForTestOnly);
			InvoicingLine.AL_JH = ZGuid.Empty;
			Assert("Should have raised event", HasApportionedInvoiceLineModifiedEventBeenRaised);
		}

		bool HasApportionedInvoiceLineModifiedEventBeenRaised;
		int ApportionedInvoiceLineModifiedEventBeenRaisedCount;

		void OnApportionedInvoiceLineModified(object sender, EventArgs e)
		{
			HasApportionedInvoiceLineModifiedEventBeenRaised = true;
			ApportionedInvoiceLineModifiedEventBeenRaisedCount++;
		}

		#endregion

		public void TestGSTandExtraTaxForGSTInclusiveAmount()
		{
			InvoicingLine.InvoiceBase.GSTInclusiveAmounts = true;
			InvoicingLine.AL_AT = GSTAndEDU.PK;
			InvoicingLine.GSTInclusiveAmount = 110.3m;

			AssertEquals("OS Ex Tax Amount on Load", 100m, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 10.3m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 10m, InvoicingLine.AL_OSGSTAmount);
			AssertEquals("OS EDU Amount on Load", 0.3m, InvoicingLine.AL_OSExtraTaxAmount);

			InvoicingLine.AL_AT = GSTAndQST.PK;
			InvoicingLine.GSTInclusiveAmount = 112.88m;

			AssertEquals("OS Ex Tax Amount on Load", 100m, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 12.88m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 5m, InvoicingLine.AL_OSGSTAmount);
			AssertEquals("OS QST Amount on Load", 7.88m, InvoicingLine.AL_OSExtraTaxAmount);

			InvoicingLine.AL_AT = RET.PK;
			InvoicingLine.GSTInclusiveAmount = 112m;

			AssertEquals("OS Ex Tax Amount on Load", 100m, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 12m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 16m, InvoicingLine.AL_OSGSTAmount);
			AssertEquals("OS RET Amount on Load", -4m, InvoicingLine.AL_OSExtraTaxAmount);

			InvoicingLine.AL_AT = GSTAndQSTBasedOnQCT.PK;
			InvoicingLine.GSTInclusiveAmount = 114.98m;

			AssertEquals("OS Ex Tax Amount on Load", 100m, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 14.98m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 5m, InvoicingLine.AL_OSGSTAmount);
			AssertEquals("OS QCT Amount on Load", 9.98m, InvoicingLine.AL_OSExtraTaxAmount);

			InvoicingLine.AL_AT = OTO6.PK;
			InvoicingLine.GSTInclusiveAmount = 106m;

			AssertEquals("OS Ex Tax Amount on Load", 100m, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 6m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount on Load", 0m, InvoicingLine.AL_OSGSTAmount);
			AssertEquals("OS OTO Amount on Load", 6m, InvoicingLine.AL_OSExtraTaxAmount);

			InvoicingLine.AL_TaxRateNumerator = 10;
			InvoicingLine.AL_TaxRateDenominator = 2;
			InvoicingLine.AL_TaxExtraRateNumerator = 6;
			InvoicingLine.AL_TaxExtraRateDenominator = 3;
			InvoicingLine.GSTInclusiveAmount = 107m;
			AssertEquals("OS Ex Tax Amount", 100m, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 7m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals("OS GST Amount", 5m, InvoicingLine.AL_OSGSTAmount);
			AssertEquals("OS OTO Amount", 2m, InvoicingLine.AL_OSExtraTaxAmount);
		}

		public void TestOSTaxRoundingOnHeader()
		{
			RefCurrency currency0DP = Factory.NewWithValidTestData<RefCurrency>();
			currency0DP.RX_SubUnitRatio = 0;

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRateNumerator_ForTestOnly(10);

			TransactionHeaderWithLines headerWithLines = (TransactionHeaderWithLines)Factory.NewWithValidTestData(MasterHeaderType);
			Line = headerWithLines.Lines.AddNew();

			Line.AL_OSExTaxAmount = 385493.34m;
			Line.AL_AT = taxRate.PK;

			AssertEquals(424042.67m, Line.AL_OverseasTotal);

			headerWithLines.AH_RX_NKTransactionCurrency = currency0DP.RX_Code;
			AssertEquals(385493m, Line.AL_OSExTaxAmount);
			AssertEquals(38549m, Line.AL_OSTaxAmount);
			AssertEquals(424042m, Line.AL_OverseasTotal);

			AssertEquals(424042m, headerWithLines.AH_OSTotalAmount);
		}

		public void TestCalculateBranch()
		{
			var organisation = TestObjectCreator.Creditor1;
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Test Branch 1", GlbCompany.CurrentCompany);
			branch1.GB_OH_OrgProxy = organisation.PK;
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Test Branch 2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", "Test Branch 3", GlbCompany.CurrentCompany);

			var chargeCode = TestObjectCreator.CC1;
			var anotherChargeCode = TestObjectCreator.CC2;
			var chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.All;
			chargeBranchOverride.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			chargeBranchOverride.YA_GB_SpecificBranch = branch2.PK;

			chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.Import;
			chargeBranchOverride.YA_TransportMode = Constants.TransportModes.Air;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker;

			var shipment = TestObjectCreator.CreateShipment("S001", "NZAKL", "AUSYD");
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OH_ImportBroker = organisation.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GB = branch3.PK;
			Factory.Save();

			InvoicingLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			AssertEquals("Precondition: AL_GB", TestObjectCreator.NonCurrentBranch.PK, InvoicingLine.AL_GB);
			AssertEquals("Precondition: AL_AC.IsEmpty", true, InvoicingLine.AL_AC.IsEmpty);
			AssertEquals("Precondition: AL_JH.IsEmpty", true, InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.GenericCharge = anotherChargeCode.PK;
			AssertEquals("Branch should not be changed as job is not set", TestObjectCreator.NonCurrentBranch.PK, InvoicingLine.AL_GB);

			InvoicingLine.AL_JH = job.PK;
			AssertEquals("Branch should be set to job charge as the charge code doesn't have branch override setup.", branch3.PK, InvoicingLine.AL_GB);

			InvoicingLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			InvoicingLine.GenericCharge = ZGuid.Empty;
			AssertEquals("Precondition: JR_GB", TestObjectCreator.NonCurrentBranch.PK, InvoicingLine.AL_GB);
			InvoicingLine.GenericCharge = anotherChargeCode.PK;
			AssertEquals("Branch should be set to job charge as the charge code doesn't have branch override setup.", branch3.PK, InvoicingLine.AL_GB);

			AssertNotNull("Precondition: Line should have Invoice.", InvoicingLine.InvoiceBase);
			AssertEquals("Precondition: InvoiceBase.SubmittedFromInvoicingForm", false, InvoicingLine.InvoiceBase.SubmittedFromInvoicingForm);
			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertEquals("Branch should not be change because invoice in not SubmittedFromInvoicingForm", branch3.PK, InvoicingLine.AL_GB);

			InvoicingLine.GenericCharge = ZGuid.Empty;
			InvoicingLine.InvoiceBase.SubmittedFromInvoicingForm = true;
			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertEquals("Branch should be set to branch defined in the charge code branch override setup.", branch1.PK, InvoicingLine.AL_GB);

			InvoicingLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			InvoicingLine.AL_JH = ZGuid.Empty;
			AssertEquals("Precondition: JR_GB", TestObjectCreator.NonCurrentBranch.PK, InvoicingLine.AL_GB);
			InvoicingLine.AL_JH = job.PK;
			AssertEquals("Branch should be set to branch defined in the charge code branch override setup.", branch1.PK, InvoicingLine.AL_GB);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			InvoicingLine.GenericCharge = ZGuid.Empty;
			InvoicingLine.GenericCharge = chargeCode.PK;
			AssertEquals("Branch should be set to branch defined in the charge code branch override setup for ALL transport mode.", branch2.PK, InvoicingLine.AL_GB);
		}

		GlbCompany differentCompany;
		GlbBranch differentBranch1;
		OrgHeader differentCompanyOrgProxy;
		OrgHeader differentBranchOrgProxy;
		GlbDepartment originalDepartment;
		AccountingPeriodTestHelper PeriodManagementTestHelper;

		void SetupCompanyAndBranch()
		{
			originalDepartment = GlbDepartment.CurrentDepartment;

			differentCompany = TestObjectCreator.CreateNewCompany("ABC", "CN");
			differentCompany.GC_RX_NKLocalCurrency = "USD";

			differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			differentCompanyOrgProxy.OH_RL_NKClosestPort = differentBranch1.GB_RL_NKHomePort;
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;

			differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			differentBranchOrgProxy.OH_RL_NKClosestPort = differentBranch1.GB_RL_NKHomePort;
			differentBranch1.GB_OH_OrgProxy = differentBranchOrgProxy.PK;

			Factory.Save();
		}

		public void TestSetExchangeRateUsesJobExRateForDefaultOption() => AssertSetExchangeRateUsesExRateConfiguration(ExRateOption.Default.Code);
		public void TestSetExchangeRateUsesExRateConfigForInvoiceDateOption() => AssertSetExchangeRateUsesExRateConfiguration(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		public void TestSetExchangeRateUsesExRateConfigForPostDateOption() => AssertSetExchangeRateUsesExRateConfiguration(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		public void TestSetExchangeRateUsesExRateConfigForTodayOption() => AssertSetExchangeRateUsesExRateConfiguration(ExRateOption.TodayExchangeRate.Code);

		void AssertSetExchangeRateUsesExRateConfiguration(ZString exRateOption)
		{
			var today = ZDateTime.Today;

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, today.AddDays(-2), today.AddDays(2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 3m, today.AddDays(-2), today.AddDays(2));
			ExchangeRateReader.GetReaderInstance().ClearCache();

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			exRate.JF_OH_Org = TestObjectCreator.ABIGAS.PK;
			exRate.JF_BaseRate = 5m;

			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(ledgerCode: "AR", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.SellRate);

			Factory.Save();
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var expectedRate = exRateOption == ExRateOption.Default.Code ? exRate.JF_BaseRate : ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, ExchangeRateType.Sell, today.ToDateTime());

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD, 4m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.UseJobExchangeRate = true;

			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100m, TestObjectCreator.USD);
			line.AL_JH = job.PK;
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 100m, debtor: invoice.Header);

			line.SetExchangeRate();
			AssertEquals(expectedRate, line.AL_ExchangeRate);
		}

		public void TestSetExchangeRateUsesExRateConfigurationForNonJobLines_DefaultOption() => AssertSetExchangeRateUsesExRateConfigurationForNonJobLines(ExRateOption.Default.Code);
		public void TestSetExchangeRateUsesExRateConfigurationForNonJobLines_InvoiceDateOption() => AssertSetExchangeRateUsesExRateConfigurationForNonJobLines(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		public void TestSetExchangeRateUsesExRateConfigurationForNonJobLines_PostDateOption() => AssertSetExchangeRateUsesExRateConfigurationForNonJobLines(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		public void TestSetExchangeRateUsesExRateConfigurationForNonJobLines_TodayOption() => AssertSetExchangeRateUsesExRateConfigurationForNonJobLines(ExRateOption.TodayExchangeRate.Code);

		[TestDate(2020, 2, 2)]
		void AssertSetExchangeRateUsesExRateConfigurationForNonJobLines(ZString exRateOption)
		{
			var defaultDate = ZDateTime.Today;
			var overrideDate = ZDateTime.Today.AddDays(1);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.C01Rate, 2m, defaultDate, defaultDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.C01Rate, 3m, overrideDate, overrideDate);
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, AccountingMasterFilesConstants.JobTypes.NonJobRelated, TransportModes.All, FreightShipmentDirection.Code.All, exRateType: ExchangeRateTypes.Code.C01Rate);
			Factory.Save();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0001", TestObjectCreator.USD, 4m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = overrideDate;
			invoice.AH_InvoiceDate = overrideDate;
			invoice.UseJobExchangeRate = true;

			var dateToCheck = exRateOption == ExRateOption.Default.Code || exRateOption == ExRateOption.TodayExchangeRate.Code
				 ? defaultDate.ToDateTime()
				 : overrideDate.ToDateTime();

			var expectedRate = ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, ZArchitecture.Environment.ExchangeRate.GetExchangeRateType(ExchangeRateTypes.Code.C01Rate), dateToCheck);

			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100m, TestObjectCreator.USD);
			line.SetExchangeRate();
			AssertEquals("Non Job line should use NJR rate type when UseJobExchangeRate is true", expectedRate, line.AL_ExchangeRate);
		}

		public void TestSetExchangeRate()
		{
			SetupCompanyAndBranch();

			TestObjectCreator.CreateBranch("PRX", GlbCompany.CurrentCompany, TestObjectCreator.Agent);

			SetupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			RefExchangeRate exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = differentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = 0.8m;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = "SEA";
			ForwardingShipment forwardingShipment1 = consol.Shipments.AddNew();
			forwardingShipment1.JS_UniqueConsignRef = "S00002000";

			Job shipmentJob1 = TestObjectCreator.CreateJob(forwardingShipment1, false);
			TestObjectCreator.SetExchangeRate(shipmentJob1, TestObjectCreator.USD, 1.2M);
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			ZDecimal consolCostAmount = 2000.00m;
			cost.E6_OSCostAmount = consolCostAmount;
			Factory.Save();

			ZDecimal jobCharge1Amount = 1.00m;
			InvoicingBase transaction = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);
				agent.CompanyData.SetAPTaxApplicable(false);

				consol = factory2.Load<ForwardingConsol>(consol.PK);

				ForwardingShipment shipment1 = consol.Shipments[0];

				OrgHeader localClient = factory2.NewWithValidTestData<OrgHeader>();
				Job job1 = factory2.NewJobForTesting<Job>();
				job1.JH_JobNum = "J00001000";
				job1.JH_GB = differentBranch1.PK;
				job1.JH_GE = originalDepartment.PK;
				job1.LocalChargesPK = localClient != null ? localClient.PK : ZGuid.Empty;
				job1.AgentCollectPK = agent != null ? agent.PK : ZGuid.Empty;
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";
				factory2.Save();

				Charge jobCharge1 = job1.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode2.PK;
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_OSCostAmt = jobCharge1Amount;
				jobCharge1.JR_OSSellAmt = jobCharge1Amount;
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				var jobs = new[] { job1 };
				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				factory2.Save();
			}
			Factory.Save();
			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);
			convertedInvoice.Factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			apps = new ApportionmentListing(Factory, consol);
			AssertEquals("One Consol Cost should exist on Consol", 1, apps.CostsCollection.Count);
			cost = apps.CostsCollection[0];

			AssertNotEquals(null, convertedInvoice.Lines[0].TransactionJob);
			AssertEquals(true, convertedInvoice.IsConvertedFromARInvoice);
			AssertEquals(true, convertedInvoice.Lines[0].IsPopulatedFromImportedApportionment);
			AssertEquals(false, convertedInvoice.UseJobExchangeRate);

			convertedInvoice.Lines[0].ApportionmentChargeImportedFrom.JR_OSCostExRate = 2.0m;

			convertedInvoice.UseJobExchangeRate = true;
			AssertEquals("ExchangeRate of apportionment line", 2.0m, convertedInvoice.Lines[0].ExchangeRate.Rate);
			AssertEquals("ExchangeRate of invoice line", 2.0m, convertedInvoice.Lines[0].ExchangeRate.Rate);

			convertedInvoice.UseJobExchangeRate = false;
			convertedInvoice.AH_ExchangeRate = 1.2m;
			AssertEquals("ExchangeRate of apportionment line", 1.2m, convertedInvoice.Lines[0].ExchangeRate.Rate);
			AssertEquals("ExchangeRate of invoice line", 1.2m, convertedInvoice.Lines[0].ExchangeRate.Rate);

			convertedInvoice.UseJobExchangeRate = true;
			AssertEquals("ExchangeRate of apportionment line", 1.2m, convertedInvoice.Lines[0].ExchangeRate.Rate);
			AssertEquals("ExchangeRate of invoice line", 1.2m, convertedInvoice.Lines[0].ExchangeRate.Rate);
		}

		public void TestSetExchangeRate_ShouldSetOneAsLocalCurrency()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = 0.8m;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0001", TestObjectCreator.AUD, 4m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.Lines.AddNew();
			invoice.AH_PostedToEFT = true;
			invoice.UseJobExchangeRate = true;

			AssertEquals("Local currency exchange rate should equal 1", 1.0m, invoice.Lines[0].ExchangeRate.Rate);
		}

		void SetupPeriodManagement(int year)
		{
			if (PeriodManagementTestHelper == null)
			{
				PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			}
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, differentCompany.PK);
		}

		public void TestARGlobalChargeCodesQueryPerformance()
		{
			// Arrange
			const int NumberOfLines = 3;

			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			for (int i = 0; i < NumberOfLines; i++)
			{
				var chargeCode = TestObjectCreator.CreateChargeCode(string.Format("MyCode{0}", i));
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 10);
				line.AL_AC = chargeCode.PK;
				var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
					string.Format("MyCode{0}", i), "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);
			}

			Factory.Save();

			// Act
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var dbHitsBefore = factory2.GetTableHitCount(AccGlobalChargeCodeMapSchema.Constants.TableName);
			var invoiceReloaded = factory2.Load<InvoicingBase>(invoice.PK);
			var dbHitsAfterInvoiceLoad = factory2.GetTableHitCount(AccGlobalChargeCodeMapSchema.Constants.TableName);

			// Assert
			AssertEquals(0, dbHitsAfterInvoiceLoad - dbHitsBefore);

			// Act again
			dbHitsBefore = factory2.GetTableHitCount(AccGlobalChargeCodeMapSchema.Constants.TableName);
			for (int i = 0; i < NumberOfLines; i++)
			{
				var globalCodes = invoiceReloaded.Lines[i].ARGlobalChargeCodes;
				AssertEquals(1, globalCodes.Count());
			}

			// Assert
			var dbHitsAfterAccessChargeCodes = factory2.GetTableHitCount(AccGlobalChargeCodeMapSchema.Constants.TableName);
			AssertEquals(1, dbHitsAfterAccessChargeCodes - dbHitsBefore);
		}

		public void TestARGlobalChargeCodes_0() { TestARGlobalChargeCodes(0); }
		public void TestARGlobalChargeCodes_1() { TestARGlobalChargeCodes(1); }
		public void TestARGlobalChargeCodes_2() { TestARGlobalChargeCodes(2); }

		void TestARGlobalChargeCodes(int numberOfCodes)
		{
			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("MyCC");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			for (int i = 0; i < numberOfCodes; i++)
			{
				var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
					string.Format("MyCode{0}", i), "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);
			}

			Factory.Save();

			// Act
			var globalCodes = line.ARGlobalChargeCodes.ToList();

			// Assert
			AssertEquals(numberOfCodes, globalCodes.Count);

			for (int i = 0; i < numberOfCodes; i++)
			{
				AssertEquals(string.Format("MyCode{0}", i), globalCodes[i].YG_Code);
				AssertEquals("DescriptionOfChargeCode", globalCodes[i].YG_Desc);
			}
		}

		public void TestIsDataVersionsAutoLogged()
		{
			var line = CreateNewLine();
			Assert("Precondition - new line", !line.IsInDatabase);
			AssertEquals("A line that should not log all it's data on the first save. This is because there is no need and can cause read and write performance problems.", false, ((IDataVersionLoggingSupported)line).IsDataVersionsAutoLogged);
			line.Factory.Save();
			AssertEquals("Changes to saved lines are rare, so we can log these without causeing those performace problems. Lets do autologging.", true, ((IDataVersionLoggingSupported)line).IsDataVersionsAutoLogged);
		}

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				AccTransactionLinesSchema.PK,
				AccTransactionLinesSchema.AL_Desc
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, AccTransactionLinesSchema.PK.TableSchema.SqlSchemaName, AccTransactionLinesSchema.PK.TableName, columns);
		}

		public void TestLoggingOfEditingTransactionLine()
		{
			if (Line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				Factory.Save();

				AssertEquals("No logs yet", 0, GetEditLogs(Line).Count());
				Line.AL_Desc = "Different Description";
				AssertEquals("No logs unit save", 0, GetEditLogs(Line).Count());
				Factory.Save();
				AssertEquals("Now has logged the change", 1, GetEditLogs(Line).Count());
				AssertEquals("Log message correct", $"Line Description edited for line with: Job: (none), Charge: {((InvoicingLineBase)Line).GenericChargeBizO.VC_Code}, Local Amount: 0.00.", GetEditLogs(Line).First().SL_Reference);

				var trigger = Factory.New<TransactionHeaderProcessTask>();
				Assert("Should not trigger on EDT as EDT trigger is not configured on invoice", trigger.P9_ActualDate.IsEmpty);

				var shipment = TestObjectCreator.CreateShipment("S001001");
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
				Line = CreateNewLine();
				Line.AL_JH = job.PK;
				Line.AL_AC = TestObjectCreator.CC1.PK;
				Line.AL_LocalExTaxAmount = 999.1m;
				TestObjectCreator.CreateCharge(Line);
				Factory.Save();

				var invoice = Factory.Load<InvoicingBase>(Line.AL_AH);

				trigger.P9_ParentID = invoice.PK;
				trigger.P9_ParentTableCode = invoice.TablePrefix;
				trigger.TriggerConditions.TriggerEventCode = "EDT";
				trigger.P9_Type = "TRG";
				trigger.P9_GC = ZGuid.Empty;
				Factory.Save();

				Line.AL_Desc = "Different Description";
				Factory.Save();
				AssertEquals("Now has logged the change", 1, GetEditLogs(Line).Count());
				AssertEquals("Log message correct", "Line Description edited for line with: Job: S001001, Charge: ZZCC1, Local Amount: 999.10.", GetEditLogs(Line).First().SL_Reference);

				Assert("Should trigger on EDT as EDT trigger is configured", !trigger.P9_ActualDate.IsEmpty);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestLoggingGovtChargeCodeWhenChanged()
		{
			var govtChargeCode = "GVTCC1";
			var newGovtChargeCode = "New GVTCC1";

			var logReference = string.Format("Government Charge Code changed from Default value: '{0}'. New Value: '{1}'", govtChargeCode, newGovtChargeCode);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GovtChargeCode = govtChargeCode;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			Line = CreateNewLine();
			Line.AL_JH = job.PK;
			Line.AL_AC = chargeCode.PK;
			Line.AL_LocalExTaxAmount = 999.1m;
			Line.AL_GovtChargeCode = newGovtChargeCode;
			Line.AL_Sequence = 1;
			Line.AL_GC = TestObjectCreator.NonCurrentCompany.PK;
			TestObjectCreator.CreateCharge(Line);
			Factory.Save();

			var logs = GetEditLogs(Line);
			AssertEquals("1 log added", 1, logs.Count());
			AssertEquals(logReference, logs.First().SL_Reference);

			Line.AL_Sequence = 2;
			Factory.Save();

			logs = GetEditLogs(Line);
			AssertEquals("No new logs added", 1, logs.Count());

			Line.AL_GovtChargeCode = govtChargeCode;
			Factory.Save();

			logs = GetEditLogs(Line);
			AssertEquals("No new logs added", 1, logs.Count());

			Line.AL_GovtChargeCode = newGovtChargeCode;
			Factory.Save();

			logs = GetEditLogs(Line);
			AssertEquals("A new log is added", 2, logs.Count());
			AssertEquals(logReference, logs.First().SL_Reference);
			AssertEquals(logReference, logs.Skip(1).First().SL_Reference);
		}

		public void TestAL_GovtChargeCode_ReadOnly()
		{
			var line = (InvoicingLineBase)CreateNewLine();

			foreach (var securityCheckPointValue in new[] { false, true })
			{
				if (line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable
					|| line.TransactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions
					|| line.TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					Env.Security.NewPayablesOverrideGovtCCodeAllows.IsAllowed = securityCheckPointValue;
				}
				else if (line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					Env.Security.NewReceivablesOverrideGovtCCodeAllows.IsAllowed = securityCheckPointValue;
				}

				AssertEquals(!securityCheckPointValue, line.AL_GovtChargeCode_ReadOnly_ForTestOnly);
			}
		}

		public void TestAL_GovtChargeCode_ReadOnly_GLAccount()
		{
			Env.Security.NewPayablesOverrideGovtCCodeAllows.IsAllowed = false;
			Env.Security.NewReceivablesOverrideGovtCCodeAllows.IsAllowed = false;

			var line = (InvoicingLineBase)CreateNewLine();
			line.GenericCharge = TestObjectCreator.GLHeader1.PK;

			AssertEquals("If gl account is linked to line then AL_GovtChargeCode should not be readonly", false, line.AL_GovtChargeCode_ReadOnly_ForTestOnly);
		}

		IEnumerable<StmALog> GetEditLogs(TransactionLine line)
		{
			var invoice = Factory.Load<InvoicingBase>(line.AL_AH);
			if (invoice != null)
			{
				return invoice.Logs.GetAllLogs().Cast<StmALog>().Where(s => s.SL_SE_NKEvent == Events.EditedARecord.Code);
			}
			else
			{
				return null;
			}
		}

		public void TestChangeChargeTypeWithAnotherFactory()
		{
			var newFactory = new BusinessObjectFactory();
			var testFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, Constants.ChargeType.Margin);
			testFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var chargeCode = newFactory.LoadTop1(typeof(AccChargeCode), testFilter) as AccChargeCode;
			chargeCode.AC_DepartmentFilterList = "ALL";

			var invoice = TestObjectCreator.CreateInvoice(MasterHeaderType, TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = chargeCode.PK;
			AssertEquals(chargeCode.AC_ChargeType, line.GenericChargeBizO.VC_Type);

			chargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			newFactory.Save();

			AssertEquals(chargeCode.AC_ChargeType, line.GenericChargeBizO.VC_Type);
		}

		public void TestOnLineAmountChanged()
		{
			var invoice = TestObjectCreator.CreateInvoice(MasterHeaderType, TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var invoiceLineCollection = invoice.Lines;
			var filteredCollection = invoice.FilteredLines;

			var uncommittedLine = (filteredCollection as IBindingList).AddNew() as InvoicingLineBase;
			Assert(filteredCollection.IsNonCommittedCollectionElement(uncommittedLine));
			Assert(filteredCollection.Contains(uncommittedLine));
			AssertEquals(1, filteredCollection.Count);
			Assert(!invoiceLineCollection.Contains(uncommittedLine));
			AssertEquals(0, invoiceLineCollection.Count);
			AssertEquals(0m, uncommittedLine.InvoiceBase.AH_OSTotalAmount);

			uncommittedLine.AL_LineAmount = 1;

			AssertEquals(1m, Math.Abs(uncommittedLine.InvoiceBase.AH_OSTotalAmount));
			Assert(filteredCollection.Contains(uncommittedLine));
			AssertEquals(1, filteredCollection.Count);
			Assert(invoiceLineCollection.Contains(uncommittedLine));
			AssertEquals(1, invoiceLineCollection.Count);

			invoiceLineCollection.RemoveAndDeleteAll();
			AssertEquals(0, filteredCollection.Count);
			AssertEquals(0, invoiceLineCollection.Count);
			AssertEquals(0m, uncommittedLine.InvoiceBase.AH_OSTotalAmount);
		}

		public void TestApportionmentChargeImportedFromReturnNullWhileChargeIsDeleted()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);

			var list = new ApportionmentListing(Factory, consol);
			var consolCost = list.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			consolCost.E6_RX_NKCurrency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			consolCost.E6_ExchangeRate = 1m;
			consolCost.E6_OSCostAmount = 10m;
			//consolCost.E6_PPDCLT = Enterprise.Core.Constants.DomesticPaymentTerms.Prepaid;
			consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			consolCost.E6_InvoiceNum = "I001";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentDate = ZDateTime.Now;

			Factory.Save();

			AssertEquals(1, job.Charges.Count);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.ImportSingleCostAndRevalidateLines(consolCost, line);
			AssertNotNull(line.ApportionmentChargeImportedFrom);
			AssertEquals(TestObjectCreator.CC1.PK, line.ApportionmentChargeImportedFrom.JR_AC);

			var chargePK = job.Charges[0].PK;
			using (invoice.GetReportingDeletedApportionmentChargesSuspender())
			{
				job.Charges[0].Delete();
			}
			AssertEquals("Deleting charge but not the line should report error", ErrorReporter.LastKeyReported, "ApportionmentChargeImportedFromIsDeleted_4");
			AssertEquals("Should be no other reports", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			AssertNull("Should be set to null while charge is deleted", line.ApportionmentChargeImportedFrom);
		}

		public void TestTaxIdAndMessageDefaultedForNonJobRelatedChargeCode()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("NONJOB");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("NONJOB2");
			var taxCode = TestObjectCreator.CreateTaxRate("EXEMPT", "Exempt", 0);
			var taxCode2 = TestObjectCreator.CreateTaxRate("EXEMPT2", "Exempt2", 0);
			var taxMsg = TestObjectCreator.TaxMsg1;
			var taxMsg2 = TestObjectCreator.TaxMsg2;
			TestObjectCreator.CreateTaxOverride(chargeCode2, taxCode2.PK, taxMsg2.PK);
			TestObjectCreator.CreateTaxOverride(chargeCode, taxCode.PK, taxMsg.PK, "OTH", "ALL", "ALL", "NJR");

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), new ZString("1"), TestObjectCreator.AUD, 1, 1, 1, 1, 1);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = invoice.Lines[0];

			line.GenericCharge = chargeCode2.PK;

			AssertNotEquals("The Tax ID should not have been defaulted", taxCode2.PK, line.AL_AT);
			AssertNotEquals("The Tax message should not have been defaulted", taxMsg2.PK, line.AL_A9_VATClass);
			AssertNotEquals("The Tax ID should not have been defaulted", taxCode.PK, line.AL_AT);
			AssertNotEquals("The Tax message should not have been defaulted", taxMsg.PK, line.AL_A9_VATClass);

			line.GenericCharge = chargeCode.PK;

			AssertEquals("The Tax ID should have been defaulted", taxCode.PK, line.AL_AT);
			AssertEquals("The Tax message should have been defaulted", taxMsg.PK, line.AL_A9_VATClass);
		}

		public void TestTaxIdAndMessageDefaultedForNonJobRelatedChargeCodeOnIncompleteInvoice()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("NONJOB");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("NONJOB2");
			var taxCode = TestObjectCreator.CreateTaxRate("EXEMPT", "Exempt", 0);
			var taxCode2 = TestObjectCreator.CreateTaxRate("EXEMPT2", "Exempt2", 0);
			var taxMsg = TestObjectCreator.TaxMsg1;
			var taxMsg2 = TestObjectCreator.TaxMsg2;
			TestObjectCreator.CreateTaxOverride(chargeCode2, taxCode2.PK, taxMsg2.PK, "OTH", "REV", "ALL", "NJR");
			TestObjectCreator.CreateTaxOverride(chargeCode, taxCode.PK, taxMsg.PK, "OTH", "COS", "ALL", "NJR");

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), new ZString("1"), TestObjectCreator.AUD, 1, 1, 1, 1, 1);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = invoice.Lines[0];

			line.GenericCharge = chargeCode2.PK;
			invoice.SaveAsIncomplete();

			var incompleteInvoice = Factory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.Lines[0].GenericCharge = chargeCode.PK;

			AssertEquals("The Tax ID should have been defaulted", taxCode.PK, incompleteInvoice.Lines[0].AL_AT);
			AssertEquals("The Tax message should have been defaulted", taxMsg.PK, incompleteInvoice.Lines[0].AL_A9_VATClass);
		}

		public void TestValidateIfLineCanBeMarkedAsImported()
		{
			var invoiceLine = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			if (invoiceLine == typeof(APInvoiceLine) || invoiceLine == typeof(APCreditNoteLine))
			{
				invoiceLine.AL_AC = TestObjectCreator.CC1.PK;
				invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
				invoiceLine.AL_GE = GlbDepartment.CurrentDepartment.PK;

				var charge = Factory.NewWithValidTestData<JobCharge>();
				invoiceLine.OriginalJobCharge = charge;

				bool isImported = invoiceLine.ValidateIfLineCanBeMarkedAsImported(charge);
				Assert(!isImported);

				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_JH = TestObjectCreator.Job1.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;

				isImported = invoiceLine.ValidateIfLineCanBeMarkedAsImported(charge);
				Assert(isImported);

				charge.JR_E6 = ZGuid.NewZGuid();
				isImported = invoiceLine.ValidateIfLineCanBeMarkedAsImported(charge);
				Assert(!isImported);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestOriginalChargeAfterImportApportionCharge()
		{
			var invoiceLine = (InvoicingLineBase)Factory.New(GetExpectedBusinessObjectType());
			if (invoiceLine == typeof(APInvoiceLine) || invoiceLine == typeof(APCreditNoteLine))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				var apportionCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				apportionCharge.RelatedApportionChargeFromDB = charge;

				invoiceLine.ImportFromApportionSplitCharge(apportionCharge);

				AssertNotNull(invoiceLine.OriginalJobCharge);
				AssertEquals(charge.PK, invoiceLine.OriginalJobCharge.PK);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestTaxCalculationOnAL_ATChange()
		{
			InvoicingLine.AL_OSExTaxAmount = 8M;
			InvoicingLine.AL_AT = CreateTaxRate().PK;
			AssertEquals(ZDate.Today, InvoicingLine.AL_TaxDate);
			AssertAmounts(8M, 0.8M, 8.8M);

			InvoicingLine.AL_AT = TestObjectCreator.GSTFREE1.PK;
			AssertAmounts(8M, 0M, 8M);
		}

		public void TestTaxCalculationOnAL_TaxDateChange()
		{
			InvoicingLine.AL_OSExTaxAmount = 8M;
			InvoicingLine.AL_AT = CreateTaxRate().PK;
			AssertEquals(ZDate.Today, InvoicingLine.AL_TaxDate);
			AssertAmounts(8M, 0.8M, 8.8M);

			InvoicingLine.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertEquals(ZDate.Today.AddDays(1), InvoicingLine.AL_TaxDate);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxRateNumeratorChange()
		{
			InvoicingLine.AL_OSExTaxAmount = 8M;
			InvoicingLine.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 0.8M, 8.8M);

			InvoicingLine.AL_TaxRateNumerator = 3;
			AssertAmounts(8M, 0.24M, 8.24M);

			InvoicingLine.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxRateDenominatorChange()
		{
			InvoicingLine.AL_OSExTaxAmount = 8M;
			InvoicingLine.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 0.8M, 8.8M);

			InvoicingLine.AL_TaxRateDenominator = 3;
			AssertAmounts(8M, 0.27M, 8.27M);

			InvoicingLine.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxExtraRateNumeratorChange()
		{
			InvoicingLine.AL_OSExTaxAmount = 8M;
			InvoicingLine.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 0.8M, 8.8M);

			InvoicingLine.AL_TaxExtraRateNumerator = 3;
			AssertAmounts(8M, 1.06M, 9.06M);

			InvoicingLine.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxExtraRateDenominatorChange()
		{
			InvoicingLine.AL_OSExTaxAmount = 8M;
			InvoicingLine.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 0.8M, 8.8M);

			InvoicingLine.AL_TaxExtraRateNumerator = 5;
			InvoicingLine.AL_TaxExtraRateDenominator = 1;
			AssertAmounts(8M, 1.24M, 9.24M);

			InvoicingLine.AL_TaxExtraRateDenominator = 5;
			InvoicingLine.AL_TaxExtraRateDenominator = 5;
			AssertAmounts(8M, 0.89M, 8.89M);

			InvoicingLine.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestIsLinkedChargeDeleted()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 258.456M, TestObjectCreator.AALSHI);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var invoiceCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 258.456M);
			apInvoice.ImportAllApportionmentsFromCosting();

			AssertEquals(false, apInvoice.Lines[0].IsLinkedChargeDeleted);
			invoiceCost.ApportionmentCharges[0].Delete();
			AssertEquals(true, apInvoice.Lines[0].IsLinkedChargeDeleted);

			ErrorReporter.Clear();
		}

		public void TestApportionmentChargeImportedFrom_ErrorIsReported()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 258.456M, TestObjectCreator.AALSHI);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var invoiceCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 258.456M);
			apInvoice.ImportAllApportionmentsFromCosting();

			invoiceCost.ApportionmentCharges[0].Delete();
			var charge = apInvoice.Lines[0].ApportionmentChargeImportedFrom;
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("ApportionmentChargeLinkedToInvoiceLineDeleted", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdjustOsTaxAmountAdjustOSTaxonly()
		{
			var taxRate = TestObjectCreator.GST1;
			InvoicingLine.AL_AT = TestObjectCreator.GST1.PK;
			InvoicingLine.AL_RX_NKTransactionCurrency = "USD";
			InvoicingLine.AL_ExchangeRate = 10;
			InvoicingLine.AL_OSExTaxAmount = 100;

			AssertEquals(10m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals(1m, InvoicingLine.AL_LocalTaxAmount);

			var line = InvoicingLine as IReceivablesTaxAmountCalculation;
			line.AdjustOsTaxAmount(5, true);
			AssertEquals(15m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals(1m, InvoicingLine.AL_LocalTaxAmount);

			line.AdjustOsTaxAmount(-10, false);
			AssertEquals(5m, InvoicingLine.AL_OSTaxAmount);
			AssertEquals(0.5m, InvoicingLine.AL_LocalTaxAmount);
		}

		public void TestAL_AT_ReadOnlyForReceivableRegistryAndSecurity()
		{
			AssertNotNull("PreCondition", InvoicingLine.MasterTransactionHeader);

			if (InvoicingLine.MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				InvoicingLine.MasterTransactionHeader.AH_OH = TestObjectCreator.TestOrganisation.PK;

				AssertNull("PreCondition", InvoicingLine.ChargeCode);
				Assert("PreCondition", InvoicingLine.IsGSTMandatory);
				AssertNotNull("PreCondition", InvoicingLine.MasterTransactionHeader.Header);
				AssertNotNull("PreCondition", InvoicingLine.MasterTransactionHeader.Header.MiscServ);
				Assert("PreCondition", InvoicingLine.IsGSTMandatory);
				Assert("PreCondition", !InvoicingLine.IsCurrentChargeGLAccount_ForTestOnly);

				var oldSecurity = Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed;
				var oldRegistry = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				try
				{
					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = false;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Assert(!InvoicingLine.AllowUserGSTOverride_ForTestOnly);
					Assert(InvoicingLine.AL_AT_ReadOnly_ForTestOnly);

					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Assert(!InvoicingLine.AllowUserGSTOverride_ForTestOnly);
					Assert(InvoicingLine.AL_AT_ReadOnly_ForTestOnly);

					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = false;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					Assert(!InvoicingLine.AllowUserGSTOverride_ForTestOnly);
					Assert(InvoicingLine.AL_AT_ReadOnly_ForTestOnly);

					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					Assert(InvoicingLine.AllowUserGSTOverride_ForTestOnly);
					Assert(!InvoicingLine.AL_AT_ReadOnly_ForTestOnly);
				}
				finally
				{
					Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed = oldSecurity;
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldRegistry);
				}
			}
		}

		void AssertAmounts(decimal lineAmount, decimal taxAmount, decimal totalAmount)
		{
			AssertEquals(lineAmount, InvoicingLine.AL_OSExTaxAmount);
			AssertEquals(taxAmount, InvoicingLine.AL_OSTaxAmount);
			AssertEquals(totalAmount, InvoicingLine.AL_OverseasTotal);
		}

		public void TestSetAL_SupplyType_AL_ACChange()
		{
			SetupSupplyTypeOverride();

			var shipmentDepartureDate = ZDate.Today.AddDays(5);
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			shipment.JS_E_DEP = shipmentDepartureDate;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			invoice.SubmittedFromInvoicingForm = true;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			AssertEquals(string.Empty, line.AL_SupplyType);

			line.AL_AC = TestObjectCreator.FRT.PK;
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, line.AL_SupplyType);
		}

		public void TestSetAL_GB_TaxBranch()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var testTaxBranch1 = TestObjectCreator.CreateBranch("TS1", GlbCompany.CurrentCompany);
			var testTaxBranch2 = TestObjectCreator.CreateBranch("TS2", GlbCompany.CurrentCompany);

			var list = new ApportionmentListing(Factory, consol);
			var consolCost = list.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			consolCost.E6_RX_NKCurrency = CurrencyCodes.Australia;
			consolCost.E6_ExchangeRate = 1m;
			consolCost.E6_OSCostAmount = 10m;
			consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			consolCost.E6_InvoiceNum = "I001";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentDate = ZDateTime.Now;

			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.ImportSingleCostAndRevalidateLines(consolCost, line1);
			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();

			AssertNotNull(line1.ApportionmentChargeImportedFrom);
			AssertNull(line2.ApportionmentChargeImportedFrom);

			invoice.AH_GB_TaxBranch = testTaxBranch1.PK;
			AssertEquals(testTaxBranch1.PK, line1.AL_GB_TaxBranch);
			AssertEquals(testTaxBranch1.PK, line2.AL_GB_TaxBranch);

			invoice.AH_GB_TaxBranch = testTaxBranch2.PK;
			AssertEquals(testTaxBranch2.PK, line1.AL_GB_TaxBranch);
			AssertEquals(testTaxBranch2.PK, line2.AL_GB_TaxBranch);
		}

		public void TestSetAL_SupplyType_AL_GEChange()
		{
			SetupSupplyTypeOverride();

			var shipmentDepartureDate = ZDate.Today.AddDays(5);
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			shipment.JS_E_DEP = shipmentDepartureDate;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			invoice.SubmittedFromInvoicingForm = true;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			AssertEquals(string.Empty, line.AL_SupplyType);

			line.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;

			line.AL_AC = TestObjectCreator.FRT.PK;
			AssertEquals(string.Empty, line.AL_SupplyType);

			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, line.AL_SupplyType);
		}

		public void TestSetAL_SupplyType_AL_JHChange()
		{
			SetupSupplyTypeOverride();

			var shipmentDepartureDate = ZDate.Today.AddDays(5);
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			shipment.JS_E_DEP = shipmentDepartureDate;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			invoice.SubmittedFromInvoicingForm = true;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.FRT.PK;
			AssertEquals(string.Empty, line.AL_SupplyType);

			line.AL_JH = job.PK;
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, line.AL_SupplyType);
		}

		[TestDate(2022, 10, 15)]
		public void TestExchangeRateOnAL_TaxDateChangeWhenSetUseJobExchRate_WithForeignCurrency()
		{
			var (invoiceLine, rateForToday, rateForYesterday) = CreateInvoiceLineAndExchangeRate();

			invoiceLine.InvoiceBase.AH_PostedToEFT = false;
			invoiceLine.AL_TaxDate = ZDate.Today.AddDays(-1);
			AssertEquals(rateForYesterday, invoiceLine.AL_ExchangeRate);
			AssertEquals(rateForYesterday, invoiceLine.InvoiceBase.AH_ExchangeRate);
			invoiceLine.AL_TaxDate = ZDate.Today;
			AssertEquals(rateForToday, invoiceLine.AL_ExchangeRate);
			AssertEquals(rateForToday, invoiceLine.InvoiceBase.AH_ExchangeRate);
		}

		[TestDate(2022, 10, 15)]
		public void TestExchangeRateOnAL_TaxDateChangeWhenSetUseJobExchRate_WithForeignCurrency_UseJobExchangeRate()
		{
			var (invoiceLine, rateForToday, rateForYesterday) = CreateInvoiceLineAndExchangeRate();

			invoiceLine.InvoiceBase.AH_PostedToEFT = true;
			invoiceLine.AL_TaxDate = ZDate.Today.AddDays(-1);
			AssertEquals(rateForYesterday, invoiceLine.AL_ExchangeRate);
			AssertEquals(rateForYesterday, invoiceLine.InvoiceBase.AH_ExchangeRate);
			invoiceLine.AL_TaxDate = ZDate.Today;
			AssertEquals(rateForToday, invoiceLine.AL_ExchangeRate);
			AssertEquals(rateForToday, invoiceLine.InvoiceBase.AH_ExchangeRate);
		}

		[TestDate(2022, 10, 15)]
		public void TestExchangeRateOnAL_TaxDateChangeWhenSetUseJobExchRate_WithLocalCurrency()
		{
			var (invoiceLine, rateForToday, rateForYesterday) = CreateInvoiceLineAndExchangeRate();

			invoiceLine.InvoiceBase.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			invoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			invoiceLine.AL_TaxDate = ZDate.Today.AddDays(-1);
			AssertEquals(1m, invoiceLine.InvoiceBase.AH_ExchangeRate);
			AssertEquals(rateForYesterday, invoiceLine.AL_ExchangeRate);
			invoiceLine.AL_TaxDate = ZDate.Today;
			AssertEquals(1m, invoiceLine.InvoiceBase.AH_ExchangeRate);
			AssertEquals(rateForToday, invoiceLine.AL_ExchangeRate);
		}

		(InvoicingLineBase, decimal, decimal) CreateInvoiceLineAndExchangeRate()
		{
			var invoiceLine = (InvoicingLineBase)this.GetNewBusinessObjectForDeleteTest(Factory);
			var postingExRateRegistry = invoiceLine.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

			var collection = new InvoicePostingExRateOptionCollection();
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, 0));
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, 0));
			postingExRateRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			invoiceLine.InvoiceBase.AH_InvoiceDate = ZDateTime.Today;
			invoiceLine.InvoiceBase.AH_PostDate = ZDateTime.Today;
			invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;

			var rateForToday = 0.1m;
			var rateForYesterday = 0.2m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, rateForToday, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, rateForYesterday, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, rateForToday, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, rateForYesterday, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			invoiceLine.AL_OSExTaxAmount = 10m;
			AssertNotNull(invoiceLine.AL_LocalExTaxAmount);
			Factory.Save();

			invoiceLine.InvoiceBase.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("PreCond: invoice line currency is foreign", TestObjectCreator.USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);

			return (invoiceLine, rateForToday, rateForYesterday);
		}

		void SetupSupplyTypeOverride()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var supplyTypeOverride = TestObjectCreator.FRT.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_TransportMode = Constants.FreightShipmentDirection.Code.All;
			supplyTypeOverride.ACS_Direction = Constants.TransportModes.All;
			supplyTypeOverride.ACS_IncoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			supplyTypeOverride.ACS_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			Factory.Save();
		}

		public void TestSetAL_SupplyTypeWithChargeTypeOverride_DSB()
		{
			SetupSupplyTypeOverride();

			AssertEquals("Precondition", Core.Constants.ChargeType.Margin, TestObjectCreator.FRT.AC_ChargeType);

			var typeOverride = TestObjectCreator.FRT.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobType = "SHP";
			typeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			typeOverride.AN_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			typeOverride.AN_JobDirection = Constants.FreightShipmentDirection.Code.All;
			Factory.Save();

			var shipmentDepartureDate = ZDate.Today.AddDays(5);
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			shipment.JS_E_DEP = shipmentDepartureDate;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			invoice.SubmittedFromInvoicingForm = true;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			AssertEquals(string.Empty, line.AL_SupplyType);

			line.AL_AC = TestObjectCreator.FRT.PK;
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB, line.AL_SupplyType);
		}

		public void TestApportionedLineModifiedStackTrace()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			invoice.SubmittedFromInvoicingForm = true;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.RaiseApportionedLineModified_ForTestOnly();
			line.RaiseApportionedLineModified_ForTestOnly();

			AssertEquals(2, line.ApportionedLineModifiedStackTrace.Count);
			AssertContains("LastMessageReported contains stack trace", "at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified_ForTestOnly()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.Testing.InvoicingLineBaseTest.TestApportionedLineModifiedStackTrace()", line.ApportionedLineModifiedStackTrace[0].ToString());
			AssertContains("LastMessageReported contains stack trace", "at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified_ForTestOnly()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.Testing.InvoicingLineBaseTest.TestApportionedLineModifiedStackTrace()", line.ApportionedLineModifiedStackTrace[1].ToString());
		}

		AccTaxRate CreateTaxRate()
		{
			var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
			rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
			return rate;
		}

		[TestDate(2024, 9, 2)]
		public void TestNotifyTransactionLineTaxDateChanges()
		{
			var invoice = InvoicingLine.InvoiceBase;
			invoice.Lines.RemoveAll();

			var taxDate = ZDate.Today;
			var invoiceDate = new ZDate(2024, 9, 1);
			invoice.AH_InvoiceDate = invoiceDate;
			var line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_TaxDate = taxDate;

			AssertEquals(taxDate, invoice.InvoiceTaxDate);
			line.AL_TaxDate = ZDate.Empty;
			AssertEquals("Using AH_InvoiceDate due to AL_TaxDate change to empty.", invoiceDate, invoice.InvoiceTaxDate);

			line.AL_AT = ZGuid.Empty;
			AssertEquals("Empty due to AL_AT change to empty.", ZDateTime.Empty, invoice.InvoiceTaxDate);
			line.AL_AT = TestObjectCreator.GST1.PK;
			AssertEquals(taxDate, invoice.InvoiceTaxDate);

			TestObjectCreator.CC2.AC_ChargeType = ChargeType.Comment;
			line.AL_AC = TestObjectCreator.CC2.PK;
			AssertEquals("Empty due to Charge Code change to CMT", ZDateTime.Empty, invoice.InvoiceTaxDate);
		}

		protected override bool IsExpectMultiSubAccountsSupported => true;

		protected override string ARLineType { get { return TransactionLineTypes.Revenue; } }
		protected override string APLineType { get { return TransactionLineTypes.Cost; } }

		#region Implementation

		protected void SetupInvoiceAndLineGSTApplicability(bool isCurrentOrgGSTApplicable, bool isGLAccount, bool canUserModifyGST, bool isCurrentCompanyGSTRegistered)
		{
			InvoicingLine.GenericCharge = isGLAccount ? TestGLCharge.PK : TestStandardCharge.PK;
			InvoicingLine.AL_AT = GST10Rate.PK;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = isCurrentCompanyGSTRegistered;
			SetCreditorOrDebtorOnLedger(isCurrentOrgGSTApplicable, true);
			SetGSTOverrideBasedOnLedger(canUserModifyGST, true);
			InvoicingLine.UpdateGSTReadOnlyState();
		}

		protected void SetCreditorOrDebtorOnLedger(bool isApplicable, bool isGst)
		{
			if (InvoicingLine.MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				InvoicingLine.MasterTransactionHeader.AH_OH = isApplicable ? (isGst ? GSTRegisteredDebtor.PK : WHTRegisteredDebtor.PK) : NonGSTRegisteredDebtor.PK;
			}
			else
			{
				InvoicingLine.MasterTransactionHeader.AH_OH = isApplicable ? (isGst ? GSTRegisteredCreditor.PK : WHTRegisteredCreditor.PK) : NonGSTRegisteredCreditor.PK;
			}
		}

		protected void SetGSTOverrideBasedOnLedger(bool canModify, bool isGst)
		{
			if (InvoicingLine.MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (isGst)
				{
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, canModify);
				}
				else
				{
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyWHTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, canModify);
				}
			}
			else
			{
				if (isGst)
				{
					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, canModify);
				}
				else
				{
					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, canModify);
				}
			}
		}

		protected void SetupInvoiceAndLineWHTApplicability(bool isCurrentOrgWHTApplicable = false, bool isGLAccount = false, bool canUserModifyWHT = false, bool isCurrentCompanyWHTRegistered = false)
		{
			InvoicingLine.GenericCharge = isGLAccount ? TestGLCharge.PK : TestStandardCharge.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = isCurrentCompanyWHTRegistered;
			SetCreditorOrDebtorOnLedger(isCurrentOrgWHTApplicable, false);
			SetGSTOverrideBasedOnLedger(canUserModifyWHT, false);
			InvoicingLine.UpdateWHTReadOnlyState();
		}

		protected AccGenericCharge TestGLCharge
		{
			get
			{
				if (fTestGLCharge == null)
				{
					fTestGLCharge = Factory.New<AccGenericCharge>();
					fTestGLCharge.VC_Code = "TSTGLC";
					fTestGLCharge.VC_IsGLAccount = true;
				}
				return fTestGLCharge;
			}
		}

		protected AccGenericCharge TestStandardCharge
		{
			get
			{
				if (fTestStandardCharge == null)
				{
					fTestStandardCharge = Factory.New<AccGenericCharge>();
					fTestStandardCharge.VC_Code = "TSTCCC";
					fTestStandardCharge.VC_IsGLAccount = false;
				}
				return fTestStandardCharge;
			}
		}

		protected AccTaxRate GST10Rate
		{
			get
			{
				if (fGST10Rate == null)
				{
					fGST10Rate = TestObjectCreator.CreateTaxRate("TSTGST", "TEST GST RATE", 10);
				}
				return fGST10Rate;
			}
		}

		protected AccTaxRate GSTExemptRate
		{
			get
			{
				if (fGSTExemptRate == null)
				{
					fGSTExemptRate = TestObjectCreator.CreateTaxRate("TSTEXE", "TEST EXEMPT GST RATE", 0);
				}
				return fGSTExemptRate;
			}
		}

		protected OrgHeader GSTRegisteredDebtor
		{
			get
			{
				if (fGSTRegisteredDebtor == null)
				{
					fGSTRegisteredDebtor = TestObjectCreator.CreateOrgHeader("TSTREGDBT", false, true, false, false, true, false);
				}
				return fGSTRegisteredDebtor;
			}
		}

		protected OrgHeader NonGSTRegisteredDebtor
		{
			get
			{
				if (fNonGSTRegisteredDebtor == null)
				{
					fNonGSTRegisteredDebtor = TestObjectCreator.CreateOrgHeader("TSTNONDBT", false, true, false, false, false, false);
				}
				return fNonGSTRegisteredDebtor;
			}
		}

		protected OrgHeader GSTRegisteredCreditor
		{
			get
			{
				if (fGSTRegisteredCreditor == null)
				{
					fGSTRegisteredCreditor = TestObjectCreator.CreateOrgHeader("TSTREGDBT", true, false, true, false, false, false);
				}
				return fGSTRegisteredCreditor;
			}
		}

		protected OrgHeader NonGSTRegisteredCreditor
		{
			get
			{
				if (fNonGSTRegisteredCreditor == null)
				{
					fNonGSTRegisteredCreditor = TestObjectCreator.CreateOrgHeader("TSTNONDBT", true, false, false, false, false, false);
				}
				return fNonGSTRegisteredCreditor;
			}
		}

		protected OrgHeader WHTRegisteredDebtor
		{
			get
			{
				if (fGSTRegisteredDebtor == null)
				{
					fGSTRegisteredDebtor = TestObjectCreator.CreateOrgHeader("TSTWHTDBT", false, true, false, false, false, true);
				}
				return fGSTRegisteredDebtor;
			}
		}

		protected OrgHeader WHTRegisteredCreditor
		{
			get
			{
				if (fGSTRegisteredCreditor == null)
				{
					fGSTRegisteredCreditor = TestObjectCreator.CreateOrgHeader("TSTWHTCRT", true, false, false, true, false, false);
				}
				return fGSTRegisteredCreditor;
			}
		}

		protected InvoicingLineBase InvoicingLine
		{
			get { return (InvoicingLineBase)base.Line; }
		}

		AccGenericCharge fTestGLCharge;
		AccGenericCharge fTestStandardCharge;
		AccTaxRate fGST10Rate;
		AccTaxRate fGSTExemptRate;
		OrgHeader fGSTRegisteredDebtor;
		OrgHeader fNonGSTRegisteredDebtor;
		OrgHeader fGSTRegisteredCreditor;
		OrgHeader fNonGSTRegisteredCreditor;

		protected bool PreTestCurrentCompanyGSTRegisteredValue;
		protected const string ALL = "ALL";

		protected override void SetUp()
		{
			base.SetUp();
			PreTestCurrentCompanyGSTRegisteredValue = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return true; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return true; }
		}

		void SetGSTRegisteredClient(InvoicingBase invoicingBase)
		{
			if (invoicingBase.AH_Ledger == LedgerTypes.AccountsPayable || invoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				invoicingBase.AH_OH = GSTRegisteredCreditor.PK;
			}
			else
			{
				invoicingBase.AH_OH = GSTRegisteredDebtor.PK;
			}
		}

		void SetUserModifyRegistryFlag(InvoicingBase invoicingBase, bool editable)
		{
			if (invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, editable);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, editable);
			}
		}

		#endregion
	}
}
