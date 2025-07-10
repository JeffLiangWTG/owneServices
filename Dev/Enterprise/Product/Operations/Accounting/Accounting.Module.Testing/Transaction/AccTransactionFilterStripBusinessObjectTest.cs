using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class AccTransactionFilterStripBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Filters

		#region TestLedgerFiltering

		public void TestLedgerFilteringForMultipleLedger()
		{
			if (!TestFilterBizO.IsSingleLedger_ForTestOnly)
			{
				APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
				ARInvoice testARInvoice = Factory.NewWithValidTestData<ARInvoice>();
				UAInvoice testUAInvoice = Factory.NewWithValidTestData<UAInvoice>();
				Factory.Save();

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				if (TestFilterBizO.GetType() == typeof(UnapprovedTransactionFilterStripBusinessObject))
				{
					Assert("There should only be the UAInvoice and ARInvoice in the collection", testTransactions.Contains(testARInvoice.PK));
					Assert("There should only be the UAInvoice and ARInvoice in the collection", testTransactions.Contains(testUAInvoice.PK));
				}
				else
				{ throw new NotSupportedException(); }
			}
			else
			{
				Assert(true);
			}
		}

		public void TestLedgerFilteringForSingleLedger()
		{
			if (TestFilterBizO.IsSingleLedger_ForTestOnly)
			{
				APJournal testAPJournal = Factory.NewWithValidTestData<APJournal>();
				ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
				Factory.Save();

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should only be the Journal in the collection", 1, testTransactions.Count);
				if (TestFilterBizO.IsPayableModule)
				{ Assert("There should only be the APJournal in the collection", testTransactions.Contains(testAPJournal.PK)); }
				else
				{ Assert("There should only be the ARJournal in the collection", testTransactions.Contains(testARJournal.PK)); }
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		public void TestUsedByActiveCollectionBatchStatusFiltering()
		{
			if (!TestFilterBizO.IsPayableModule)
			{
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();

				var bank = Factory.NewWithValidTestData<AccBankAccount>();
				var batch = Factory.New<AccCollectionBatch>();
				batch.ACB_TotalAmount = 100m;
				batch.ACB_AB = bank.PK;
				batch.ACB_GC = GlbCompany.CurrentCompany.PK;
				batch.ACB_BatchNumber = "0001000";

				var order = Factory.New<AccCollectionOrder>();
				order.ACO_ACB = batch.PK;
				order.ACO_CollectionDate = ZDate.Today;
				order.ACO_OH_Debtor = TestObjectCreator.AALSHI.PK;
				order.ACO_OrderNumber = "000001";
				order.IncludeInBatch = true;
				order.ACO_Amount = 100m;

				var orderline = Factory.New<AccCollectionOrderLine>();
				orderline.AOL_ACO = order.PK;
				orderline.AOL_AH = invoice1.PK;
				orderline.AOL_IsCancelled = false;
				orderline.IncludeInOrder = true;
				Factory.Save();

				ModuleTextFilter collectionBatchFilter = ((ModuleTextFilter)TestFilterBizO["Collection Batch"]);
				collectionBatchFilter.Property = "IAB";
				collectionBatchFilter.IsActive = true;
				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should capture 1 transactions", 1, testTransactions.Count);
				Assert(testTransactions.Contains(invoice1.PK));

				collectionBatchFilter.Property = "NAB";
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should capture 1 transactions", 1, testTransactions.Count);
				Assert(testTransactions.Contains(invoice2.PK));

				collectionBatchFilter.Property = "ALL";
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should capture 2 transactions", 2, testTransactions.Count);
				Assert(testTransactions.Contains(invoice1.PK));
				Assert(testTransactions.Contains(invoice2.PK));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAgreedPaymentMethodFiltering()
		{
			Invoice testInv = CreateNewInvoice(Factory);
			testInv.AH_AgreedPaymentMethodOverride = "CRQ";

			Invoice testInv2 = CreateNewInvoice(Factory);
			testInv2.AH_AgreedPaymentMethodOverride = "CRQ";

			Invoice testInv3 = CreateNewInvoice(Factory);
			testInv3.AH_AgreedPaymentMethodOverride = "CHK";

			Factory.Save();

			ModuleTextFilter agreedPaymentMethodFilter = ((ModuleTextFilter)TestFilterBizO["Agreed Payment Method"]);
			agreedPaymentMethodFilter.Property = "CRQ";
			agreedPaymentMethodFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should capture 2 transactions", 2, testTransactions.Count);
			Assert(testTransactions.Contains(testInv.PK));
			Assert(testTransactions.Contains(testInv2.PK));
		}

		#region TestComplianceDocumentRecordFilter

		public virtual void TestComplianceDocumentRecrodFilterVisibility()
		{
			AssertComplianceDocumentRecrodFilterVisibility(true);
			AssertComplianceDocumentRecrodFilterVisibility(false);
		}

		protected virtual void AssertComplianceDocumentRecrodFilterVisibility(bool value)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var complianceDocumentFilter = (ModuleTextFilter)(TestFilterBizO["Compliance Document Record"]);
				if (TestFilterBizO.ShouldAddComplianceDocumentRecordFilter_ForTestOnly)
				{
					AssertNotNull("complianceDocumentFilter should not be null.", complianceDocumentFilter);
				}
				else
				{
					AssertNull("complianceDocumentFilter should be null.", complianceDocumentFilter);
				}
			}
		}

		public virtual void TestComplianceDocumentRecordFilter()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				if (TestFilterBizO.ShouldAddComplianceDocumentRecordFilter_ForTestOnly)
				{
					var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "", 3);
					vat3.AT_PostingGroupId = 0;

					var ac1 = TestObjectCreator.CreateChargeCode("AC1");
					ac1.AC_AT_GSTRate = vat3.PK;
					ac1.AC_Desc = "desc";

					var invoiceWithAllLineWithCompliacneDocumentRecord = CreateNewInvoice(Factory);
					invoiceWithAllLineWithCompliacneDocumentRecord.AH_OH = TestObjectCreator.Agent.PK;
					var lineType = invoiceWithAllLineWithCompliacneDocumentRecord is ARInvoice ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;
					var lineWithCompliacneDocumentRecord = TestObjectCreator.CreateInvoiceLine(lineType, invoiceWithAllLineWithCompliacneDocumentRecord, TestObjectCreator.Job1, ac1, TestObjectCreator.AUD, 1, "desc", 0);
					lineWithCompliacneDocumentRecord.AL_LineAmount = 10m;
					lineWithCompliacneDocumentRecord.AL_OSAmount = 10m;
					var commentLine = invoiceWithAllLineWithCompliacneDocumentRecord.Lines.AddNew() as InvoiceLine;
					commentLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;

					var charge = TestObjectCreator.CreateJobCharge(lineWithCompliacneDocumentRecord, TestObjectCreator.Job1, ac1);
					charge.JR_LocalCostAmt = invoiceWithAllLineWithCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_OSCostAmt = invoiceWithAllLineWithCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_AT_CostGSTRate = lineWithCompliacneDocumentRecord.AL_AT;

					var header = TestObjectCreator.CreateComplianceDocumentHeader(invoiceWithAllLineWithCompliacneDocumentRecord.AH_Ledger, "desc", "ABC");
					header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;

					var line = TestObjectCreator.CreateComplianceDocumentLine(header, "desc", 1);
					TestObjectCreator.CreateComplianceDocumentPivot(line, lineWithCompliacneDocumentRecord);

					Factory.Save();

					var invoiceWithAllLineWithoutCompliacneDocumentRecord = CreateNewInvoice(Factory);
					invoiceWithAllLineWithoutCompliacneDocumentRecord.AH_OH = TestObjectCreator.Agent.PK;
					var lineWithoutCompliacneDocumentRecord = TestObjectCreator.CreateInvoiceLine(lineType, invoiceWithAllLineWithoutCompliacneDocumentRecord, TestObjectCreator.Job2, ac1, TestObjectCreator.AUD, 1, "desc", 0);
					lineWithoutCompliacneDocumentRecord.AL_LineAmount = 10m;
					lineWithoutCompliacneDocumentRecord.AL_OSAmount = 10m;
					commentLine = invoiceWithAllLineWithCompliacneDocumentRecord.Lines.AddNew() as InvoiceLine;
					commentLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;

					charge = TestObjectCreator.CreateJobCharge(lineWithoutCompliacneDocumentRecord, TestObjectCreator.Job2, ac1);
					charge.JR_LocalCostAmt = invoiceWithAllLineWithoutCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_OSCostAmt = invoiceWithAllLineWithoutCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_AT_CostGSTRate = lineWithoutCompliacneDocumentRecord.AL_AT;

					Factory.Save();

					var invoiceWithSomeLineWithCompliacneDocumentRecord = CreateNewInvoice(Factory);
					invoiceWithSomeLineWithCompliacneDocumentRecord.AH_OH = TestObjectCreator.Agent.PK;
					lineWithCompliacneDocumentRecord = TestObjectCreator.CreateInvoiceLine(lineType, invoiceWithSomeLineWithCompliacneDocumentRecord, TestObjectCreator.Job2, ac1, TestObjectCreator.AUD, 1, "desc", 0);
					lineWithCompliacneDocumentRecord.AL_LineAmount = 10m;
					lineWithCompliacneDocumentRecord.AL_OSAmount = 10m;

					charge = TestObjectCreator.CreateJobCharge(lineWithCompliacneDocumentRecord, TestObjectCreator.Job2, ac1);
					charge.JR_LocalCostAmt = invoiceWithSomeLineWithCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_OSCostAmt = invoiceWithSomeLineWithCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_AT_CostGSTRate = lineWithCompliacneDocumentRecord.AL_AT;

					header = TestObjectCreator.CreateComplianceDocumentHeader(invoiceWithSomeLineWithCompliacneDocumentRecord.AH_Ledger, "desc", "ABC");
					header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;

					line = TestObjectCreator.CreateComplianceDocumentLine(header, "desc", 1);
					TestObjectCreator.CreateComplianceDocumentPivot(line, lineWithCompliacneDocumentRecord);

					lineWithoutCompliacneDocumentRecord = TestObjectCreator.CreateInvoiceLine(lineType, invoiceWithSomeLineWithCompliacneDocumentRecord, TestObjectCreator.Job2, ac1, TestObjectCreator.AUD, 1, "desc", 0);
					lineWithoutCompliacneDocumentRecord.AL_LineAmount = 10m;
					lineWithoutCompliacneDocumentRecord.AL_OSAmount = 10m;

					charge = TestObjectCreator.CreateJobCharge(lineWithoutCompliacneDocumentRecord, TestObjectCreator.Job2, ac1);
					charge.JR_LocalCostAmt = invoiceWithSomeLineWithCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_OSCostAmt = invoiceWithSomeLineWithCompliacneDocumentRecord is ARInvoice ? 10m : -10m;
					charge.JR_AT_CostGSTRate = lineWithoutCompliacneDocumentRecord.AL_AT;

					commentLine = invoiceWithSomeLineWithCompliacneDocumentRecord.Lines.AddNew() as InvoiceLine;
					commentLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;

					Factory.Save();

					var commentInvoice = CreateNewInvoice(Factory);
					commentInvoice.AH_OH = TestObjectCreator.Agent.PK;
					var commentInvoiceLine = TestObjectCreator.CreateInvoiceLine(lineType, commentInvoice, TestObjectCreator.Job2, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD, 1, "desc", 0);
					charge = TestObjectCreator.CreateJobCharge(commentInvoiceLine, TestObjectCreator.Job2, TestObjectCreator.CommentChargeCode);
					charge.JR_AT_CostGSTRate = commentInvoiceLine.AL_AT;
					Factory.Save();

					var complianceDocumentFilter = (ModuleTextFilter)(TestFilterBizO["Compliance Document Record"]);

					complianceDocumentFilter.Property = "CRE";
					complianceDocumentFilter.IsActive = true;

					var filterResult = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					filterResult.Load();
					AssertEquals("Filter Compliance Document Record is 'CRE'", "CRE", complianceDocumentFilter.Property);
					AssertEquals("Should capture 1 transactions", 1, filterResult.Count);
					Assert(filterResult.Contains(invoiceWithAllLineWithCompliacneDocumentRecord.PK));

					complianceDocumentFilter.Property = "NCR";

					filterResult = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					filterResult.Load();

					AssertEquals("Filter Compliance Document Record is 'NCR'", "NCR", complianceDocumentFilter.Property);
					AssertEquals("Should capture 1 transactions", 1, filterResult.Count);
					Assert(filterResult.Contains(invoiceWithAllLineWithoutCompliacneDocumentRecord.PK));

					complianceDocumentFilter.Property = "PCR";

					filterResult = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					filterResult.Load();

					AssertEquals("Filter Compliance Document Record is 'PCR'", "PCR", complianceDocumentFilter.Property);
					AssertEquals("Should capture 1 transactions", 1, filterResult.Count);
					Assert(filterResult.Contains(invoiceWithSomeLineWithCompliacneDocumentRecord.PK));

					complianceDocumentFilter.Property = "ALL";

					filterResult = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					filterResult.Load();

					AssertEquals("Filter Compliance Document Record is 'ALL'", "ALL", complianceDocumentFilter.Property);
					AssertEquals("Should capture 4 transactions", 4, filterResult.Count);
					Assert(filterResult.Contains(invoiceWithAllLineWithCompliacneDocumentRecord.PK));
					Assert(filterResult.Contains(invoiceWithAllLineWithoutCompliacneDocumentRecord.PK));
					Assert(filterResult.Contains(invoiceWithSomeLineWithCompliacneDocumentRecord.PK));
					Assert(filterResult.Contains(commentInvoice.PK));
				}
				else
				{
					Assert(true);
				}
			}
		}

		#endregion

		#region TestEInvoicingFilter

		void AssertEInvoicingFiltersVisibility(bool shouldBeVisible)
		{
			var eInvoicingFilters = new string[]
			{ "EInvoicing Status", "EInvoicing Last Response Received UTC", "E-Reporting Batch", "E-Reporting eHub #", "E-Reporting Govt #", "E-Reporting Auth #" };

			foreach (var item in eInvoicingFilters)
			{
				var filter = TestFilterBizO.GetModuleFiltersCore_ForTestOnly()[item];
				if (shouldBeVisible)
				{
					AssertNotNull("Filter should be available", filter);
				}
				else
				{
					AssertNull("Filter should not be available", filter);
				}
			}
		}

		public virtual void TestEInvoicingFiltersVisibility()
		{
			var currCompanyGuid = GlbCompany.CurrentCompany.PK.ToGuid();
			var registry = AccountingMasterFilesRegistry.Instance;
			var regFunctionality = TestFilterBizO.IsPayableModule ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;

			var globalEInvoicingFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();

			globalEInvoicingFactoryMock.Setup(x => x.DoesCountrySupportElectronicInvoicing(It.IsAny<ZString>())).Returns(true);
			ObjectFactory.Substitute(globalEInvoicingFactoryMock.Object);
			Assert("EInvoicing is disabled", !regFunctionality.GetValueWithoutFallback(currCompanyGuid, Guid.Empty, Guid.Empty));
			AssertEInvoicingFiltersVisibility(false);
			using (regFunctionality.SetTemporaryValue(currCompanyGuid, Guid.Empty, Guid.Empty, true))
			{
				AssertEInvoicingFiltersVisibility(true);
			}

			globalEInvoicingFactoryMock.Setup(x => x.DoesCountrySupportElectronicInvoicing(It.IsAny<ZString>())).Returns(false);
			Assert("EInvoicing is disabled", !regFunctionality.GetValueWithoutFallback(currCompanyGuid, Guid.Empty, Guid.Empty));
			AssertEInvoicingFiltersVisibility(false);
			using (regFunctionality.SetTemporaryValue(currCompanyGuid, Guid.Empty, Guid.Empty, true))
			{
				AssertEInvoicingFiltersVisibility(false);
			}
		}

		InvoicingBase[] SetupEInvoicingData()
		{
			var invoice1 = CreateNewInvoice(Factory);
			var batchDiscarded = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batchDiscarded.AIB_BatchNumber = 1;
			batchDiscarded.AIB_EHubAllocatedNumber = "";
			batchDiscarded.AIB_GovernmentAllocatedNumber = "";

			var pivot1Discarded = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot1Discarded.AIP_Status = Constants.EInvoicingPivotState.Discarded;
			pivot1Discarded.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
			pivot1Discarded.AIP_LastResponseReceivedUtc = new ZDateTime(2017, 03, 02);
			pivot1Discarded.AIP_ParentTableCode = "AH";
			pivot1Discarded.AIP_AIB = batchDiscarded.PK;
			pivot1Discarded.AIP_ParentID = invoice1.PK;

			var batch1 = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch1.AIB_BatchNumber = 2;
			batch1.AIB_EHubAllocatedNumber = "EHub#1";
			batch1.AIB_GovernmentAllocatedNumber = "Govt#1";

			var pivot1 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot1.AIP_Status = Constants.EInvoicingPivotState.Batched;
			pivot1.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
			pivot1.AIP_LastResponseReceivedUtc = new ZDateTime(2017, 03, 03);
			pivot1.AIP_ParentTableCode = "AH";
			pivot1.AIP_AIB = batch1.PK;
			pivot1.AIP_ParentID = invoice1.PK;

			var batchStatusCheck = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batchStatusCheck.AIB_BatchNumber = 3;
			batchStatusCheck.AIB_EHubAllocatedNumber = "";
			batchStatusCheck.AIB_GovernmentAllocatedNumber = "";

			var pivot1ForStatusCheck = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot1ForStatusCheck.AIP_Status = Constants.EInvoicingPivotState.Batched;
			pivot1ForStatusCheck.AIP_ActionType = Constants.EInvoicingPivotActionType.StatusCheck;
			pivot1ForStatusCheck.AIP_ParentTableCode = "AH";
			pivot1ForStatusCheck.AIP_AIB = batchStatusCheck.PK;
			pivot1ForStatusCheck.AIP_ParentID = invoice1.PK;

			var invoice2 = CreateNewInvoice(Factory);
			var batch2 = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch2.AIB_BatchNumber = 4;
			batch2.AIB_EHubAllocatedNumber = "EHub#2";
			batch2.AIB_GovernmentAllocatedNumber = "Govt#2";
			var pivot2 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot2.AIP_Status = Constants.EInvoicingPivotState.BatchedWithError;
			pivot2.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
			pivot2.AIP_ErrorDescription = "The transaction is invalid";
			pivot2.AIP_ParentTableCode = "AH";
			pivot2.AIP_AIB = batch2.PK;
			pivot2.AIP_ParentID = invoice2.PK;

			var invoice3 = CreateNewInvoice(Factory);

			var invoice4 = CreateNewInvoice(Factory);
			var batch4 = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch4.AIB_BatchNumber = 5;
			batch4.AIB_EHubAllocatedNumber = "EHub#3";
			batch4.AIB_GovernmentAllocatedNumber = "Govt#3";

			var pivot4 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot4.AIP_Status = Constants.EInvoicingPivotState.Succeed;
			pivot4.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
			pivot4.AIP_LastResponseReceivedUtc = new ZDateTime(2017, 03, 09);
			pivot4.AIP_ParentTableCode = "AH";
			pivot4.AIP_AIB = batch4.PK;
			pivot4.AIP_ParentID = invoice4.PK;
			Factory.Save();

			var arCreditNote = TestObjectCreator.ReverseTransaction(invoice4, out _);
			((InvoicingBase)arCreditNote).AH_ComplianceSubType = MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
			((InvoicingBase)arCreditNote).AH_TransactionNum = "CRD0001";

			var batchCrd = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batchCrd.AIB_BatchNumber = 5;
			batchCrd.AIB_EHubAllocatedNumber = "EHub#4";
			batchCrd.AIB_GovernmentAllocatedNumber = "Govt#4";

			var pivotCrd = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivotCrd.AIP_Status = Constants.EInvoicingPivotState.Succeed;
			pivotCrd.AIP_ActionType = Constants.EInvoicingPivotActionType.Cancel;
			pivotCrd.AIP_LastResponseReceivedUtc = new ZDateTime(2017, 03, 10);
			pivotCrd.AIP_ParentTableCode = "AH";
			pivotCrd.AIP_AIB = batchCrd.PK;
			pivotCrd.AIP_ParentID = arCreditNote.PK;
			Factory.Save();

			return new InvoicingBase[] { invoice1, invoice2, invoice3, invoice4, (InvoicingBase)arCreditNote };
		}

		void AssertEInvoicingData(ModuleFilter filter, InvoicingBase[] result)
		{
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, filter.Query);
			transactions.Load();
			AssertEquals(result.Length, transactions.Count);
			Assert("Collection should contain all invoices", transactions.ContainsSameElementsInAnyOrder(result));
		}

		public virtual void TestEInvoicingFilters()
		{
			var invoices = SetupEInvoicingData();
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Spain })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					SetEInvoicingEnabled(invoices[0].AH_Ledger);

					var numberfilter = (ModuleNumberRangeFilter)(TestFilterBizO["E-Reporting Batch"]);
					numberfilter.IsActive = true;
					numberfilter.Property1 = 1;
					numberfilter.Property2 = 5;
					AssertEInvoicingData(numberfilter, new InvoicingBase[] { invoices[0], invoices[1], invoices[3], invoices[4] });

					var filter = (ModuleTextFilter)(TestFilterBizO["E-Reporting Govt #"]);
					filter.IsActive = true;
					filter.Property = "Govt#1";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[0] });
					filter.Property = "Govt#2";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[1] });
					filter.Property = "Govt#3";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[3] });
					filter.Property = "Govt#4";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[4] });

					filter = (ModuleTextFilter)(TestFilterBizO["E-Reporting eHub #"]);
					filter.IsActive = true;
					filter.Property = "EHub#1";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[0] });
					filter.Property = "EHub#2";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[1] });
					filter.Property = "EHub#3";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[3] });
					filter.Property = "EHub#4";
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[4] });

					filter = (ModuleTextFilter)(TestFilterBizO["EInvoicing Status"]);
					filter.IsActive = true;
					filter.Property = Constants.EInvoicingPivotState.Discarded;
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[0] });
					filter.IsActive = true;
					filter.Property = Constants.EInvoicingPivotState.Batched;
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[0] });
					filter.IsActive = true;
					filter.Property = Constants.EInvoicingPivotState.Succeed;
					AssertEInvoicingData(filter, new InvoicingBase[] { invoices[3], invoices[4] });
					filter.Property = ZString.Empty;
					filter.IsActive = true;
					AssertEInvoicingData(filter, invoices);

					var dateFilter = (ModuleDateFilter)(TestFilterBizO["EInvoicing Last Response Received UTC"]);
					dateFilter.IsActive = true;
					dateFilter.Property1 = new ZDateTime(2017, 02, 28);
					dateFilter.Property2 = new ZDateTime(2017, 03, 05);
					dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					AssertEInvoicingData(dateFilter, new InvoicingBase[] { invoices[0] });
				}
			}
		}

		public virtual void TestEReportingAuthNumberFilter()
		{
			var invoice1 = CreateNewInvoice(Factory);
			var authRecord1 = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord1.AHF_ParentId = invoice1.PK;
			authRecord1.AHF_Number = "Auth#1";

			var invoice2 = CreateNewInvoice(Factory);
			var authRecord2 = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord2.AHF_ParentId = invoice2.PK;
			authRecord2.AHF_Number = "Auth#2";

			Factory.Save();

			var invoices = new[] { invoice1, invoice2 };

			var countryCodeList = Country.LicenceKeyBuilderSupportedCountryCodes;
			foreach (var country in countryCodeList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					if (ElectronicInvoicingEligibilityDecider.IsSupportedCountry(country))
					{
						SetEInvoicingEnabled(invoices[0].AH_Ledger);

						var filter = (ModuleTextFilter)(TestFilterBizO["E-Reporting Auth #"]);
						filter.IsActive = true;
						filter.Property = "";
						AssertEInvoicingData(filter, new InvoicingBase[] { invoices[0], invoices[1] });
						filter.Property = "Auth#0";
						AssertEInvoicingData(filter, Array.Empty<InvoicingBase>());
						filter.Property = "Auth#1";
						AssertEInvoicingData(filter, new InvoicingBase[] { invoices[0] });
						filter.Property = "Auth#2";
						AssertEInvoicingData(filter, new InvoicingBase[] { invoices[1] });
					}
				}
			}
		}

		protected void SetEInvoicingEnabled(string ledger)
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var regFunctionality = ledger == LedgerTypes.AccountsPayable ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;
			regFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		}

		#endregion

		#region TestDebtorGroupFiltering

		public void TestDebtorGroupFiltering()
		{
			OrgDebtorGroup testDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgDebtorGroup testDebtorGroup2 = Factory.NewWithValidTestData<OrgDebtorGroup>();

			OrgCreditorGroup testCreditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			OrgCreditorGroup testCreditorGroup2 = Factory.NewWithValidTestData<OrgCreditorGroup>();

			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			if (TestFilterBizO.UseCreditor_ForTestOnly)
			{
				testOrg1.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup.PK;
			}
			else
			{
				testOrg1.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup.PK;
			}

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			if (TestFilterBizO.UseCreditor_ForTestOnly)
			{
				testOrg2.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup2.PK;
			}
			else
			{
				testOrg2.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup2.PK;
			}

			Invoice testInv = CreateNewInvoice(Factory);
			testInv.AH_OH = testOrg1.PK;

			Invoice testInv2 = CreateNewInvoice(Factory);
			testInv2.AH_OH = testOrg2.PK;

			Factory.Save();

			GlbCompany company_New = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch_New = company_New.Branches.AddNew();
			GlbStaff staff_New = Factory.NewWithValidTestData<GlbStaff>();
			staff_New.GS_GB_HomeBranch = branch_New.PK;
			staff_New.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			staff_New.GS_LoginName = "newstaff";
			Factory.Save();

			using (Env.SetTemporaryUserContext("newstaff", branch_New.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				testOrg1 = newFactory.Load<OrgHeader>(testOrg1.PK);
				if (TestFilterBizO.UseCreditor_ForTestOnly)
				{
					testOrg1.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup2.PK;
				}
				else
				{
					testOrg1.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup2.PK;
				}

				Invoice testInv3 = CreateNewInvoice(newFactory);
				testInv3.AH_OH = testOrg1.PK;

				newFactory.Save();
			}

			ModuleGuidFilter creditorDebtorFilter = ((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorGroupText_ForTestOnly]);
			if (TestFilterBizO.UseCreditor_ForTestOnly)
			{
				creditorDebtorFilter.Property = testCreditorGroup.PK;
			}
			else
			{
				creditorDebtorFilter.Property = testDebtorGroup.PK;
			}
			creditorDebtorFilter.IsActive = true;

			ZQuery filter = creditorDebtorFilter.Query;
			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory, filter);

			Factory.Save();
			headers.Load();

			AssertEquals("There should be 1 transaction from TestDebtorGroup", 1, headers.Count);
			Assert("That transaction should be TestInv", headers.Contains(testInv));
			if (TestFilterBizO.UseCreditor_ForTestOnly)
			{
				creditorDebtorFilter.Property = testCreditorGroup2.PK;
			}
			else
			{
				creditorDebtorFilter.Property = testDebtorGroup2.PK;
			}
			filter = creditorDebtorFilter.Query;
			headers.Load(filter);

			AssertEquals("There should be 1 transaction from TestDebtorGroup2", 1, headers.Count);
			Assert("That transaction should be TestInv2", headers.Contains(testInv2));
		}

		#endregion

		#region Organisation (Debtor / Creditor) Filtering

		public void TestOrganisationFilterList()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader aROrg = newFactory.NewWithValidTestData<OrgHeader>();
			aROrg.CompanyData.OB_IsDebtor = true;
			OrgHeader aPOrg = newFactory.NewWithValidTestData<OrgHeader>();
			aPOrg.CompanyData.OB_IsCreditor = true;
			OrgHeader aRAndAPOrg = newFactory.NewWithValidTestData<OrgHeader>();
			aRAndAPOrg.CompanyData.OB_IsDebtor = true;
			aRAndAPOrg.CompanyData.OB_IsCreditor = true;

			aPOrg.OH_IsActive = true;
			aROrg.OH_IsActive = true;
			aRAndAPOrg.OH_IsActive = true;

			newFactory.Save();

			ModuleGuidFilter debtorOrCreditorFilter;
			if (TestFilterBizO.UseCreditor_ForTestOnly)
			{
				debtorOrCreditorFilter = (ModuleGuidFilter)TestFilterBizO["Creditor"];
			}
			else
			{
				debtorOrCreditorFilter = (ModuleGuidFilter)TestFilterBizO["Debtor"];
			}
			TestFilterBizO.AH_OHList_ForTestOnly.Load();

			if (TestFilterBizO.UseDebtor_ForTestOnly)
			{
				Assert(TestFilterBizO.AH_OHList_ForTestOnly.Contains(aROrg));
			}
			else
			{
				Assert(TestFilterBizO.AH_OHList_ForTestOnly.Contains(aPOrg));
			}
			Assert(TestFilterBizO.AH_OHList_ForTestOnly.Contains(aRAndAPOrg));
		}

		#endregion

		#region TestBranchManagementCodeFilter

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";

			var invoice1 = CreateNewInvoice(Factory);
			invoice1.AH_GB = branch1.PK;
			var invoice2 = CreateNewInvoice(Factory);
			invoice2.AH_GB = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)TestFilterBizO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice1", new[] { invoice1 }, collection);

			branchManagementCodeFilter.Property = "BRB";
			collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice2", new[] { invoice2 }, collection);
		}

		#endregion

		#region ViewingNonLoginBranchTransactions

		public void TestViewingNonLoginBranchTransactions()
		{
			TestFilterBizO.ViewingNonLoginBranchTransactions_ForTestOnly.IsAllowed = true;
			ModuleGuidFilter branchFilter = (ModuleGuidFilter)TestFilterBizO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should not be read only", false, branchFilter.ReadOnly);
			AssertNotEquals("Should not be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertNotEquals("Should not have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);

			TestFilterBizO.ViewingNonLoginBranchTransactions_ForTestOnly.IsAllowed = false;
			branchFilter = (ModuleGuidFilter)TestFilterBizO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should be read only", true, branchFilter.ReadOnly);
			AssertEquals("Should be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertEquals("Should have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);
		}

		#endregion

		#region AllNumbers Filter Max Length Tests

		public void TestAllNumbersFilterMaxLength()
		{
			var columnsUsedInAllNumbersFilter = new HashSet<SchemaColumn>() { JobHeaderSchema.JH_JobNum, AccTransactionHeaderSchema.AH_TransactionNum, AccTransactionHeaderSchema.AH_ChequeOrReference,
					AccTransactionHeaderSchema.AH_ReceiptBatchNo, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef };
			var columnWithLargestMaxLength = columnsUsedInAllNumbersFilter.MaxBy(c => c.MaxLength);
			var errorMessage = $"All Numbers Filter Max Length should be set to {columnWithLargestMaxLength.Name} Max Length of {columnWithLargestMaxLength.MaxLength}.";
			AssertEquals(errorMessage, ModuleNumberFilter.MultiplyMaxLength(columnWithLargestMaxLength.MaxLength), TestFilterBizO["All Numbers"].MaxLength);
		}

		public void TestGetAllNumbersQueryReturnsNoResultQueryWhenFilterValueIsGreaterThanFilterMaxLength()
		{
			var filter = (ModuleNumberFilter)TestFilterBizO["All Numbers"];
			filter.IsActive = true;

			filter.Property = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength);
			Assert("GetAllNumbersQuery should NOT be a No Result Query, filter value = filter max length", !TestFilterBizO.Filter.IsNoResultQuery);
			filter.Property = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength + 1);
			Assert("GetAllNumbersQuery should be a No Result Query, filter value > filter max length", TestFilterBizO.Filter.IsNoResultQuery);
		}

		public void TestAllNumbersFilterWithColumnMaxLength()
		{
			AssertAllNumbersFilterWithColumnMaxLength(JobHeaderSchema.JH_JobNum);
			AssertAllNumbersFilterWithColumnMaxLength(AccTransactionHeaderSchema.AH_TransactionNum);
			AssertAllNumbersFilterWithColumnMaxLength(AccTransactionHeaderSchema.AH_ChequeOrReference);
			AssertAllNumbersFilterWithColumnMaxLength(AccTransactionHeaderSchema.AH_ReceiptBatchNo);
			AssertAllNumbersFilterWithColumnMaxLength(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef);
		}

		void AssertAllNumbersFilterWithColumnMaxLength(SchemaColumn columnToTest)
		{
			var filter = (ModuleNumberFilter)TestFilterBizO["All Numbers"];
			filter.IsActive = true;

			filter.Property = TestObjectCreator.GetRandomString(columnToTest.MaxLength);
			AssertContains(columnToTest.Name, TestFilterBizO.Filter.GetAsWhereClause(false));

			filter.Property = TestObjectCreator.GetRandomString(columnToTest.MaxLength + 1);
			AssertNotContains(columnToTest.Name, TestFilterBizO.Filter.GetAsWhereClause(false));
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract Invoice CreateNewInvoice(BusinessObjectFactory factory);

		protected TransactionFilterStripBusinessObject TestFilterBizO
		{
			get
			{
				if (fTestFilterBizO == null)
				{
					fTestFilterBizO = (TransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				}
				return fTestFilterBizO;
			}
		}
		protected TransactionFilterStripBusinessObject fTestFilterBizO;

		#endregion
	}
}
