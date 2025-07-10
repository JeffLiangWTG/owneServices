using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ARComplianceDocumentHeader))]
	public class ARComplianceDocumentHeaderTest : AccComplianceDocumentHeaderTest
	{
		protected override AccComplianceDocumentHeader GetComplianceDocumentHeader()
		{
			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			header.ADH_Ledger = LedgerTypes.AccountsReceivable;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			return header;
		}

		[TestDate(2019, 1, 1)]
		public override void TestQueueForComplianceReports_ACQDate()
		{
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(201901, new ZDateTime(2019, 01, 01), new ZDateTime(2019, 01, 31));
			testHelper.SetupSinglePeriod(201902, new ZDateTime(2019, 02, 01), new ZDateTime(2019, 02, 28));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
			newFactory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var aRInvoice = creator.CreateInvoice(typeof(ARInvoice), "INV002", creator.TWD, 1M, creator.ABIGAS);
				var line2 = creator.CreateInvoiceLine(aRInvoice, creator.TWD, 1M, 200M, 20M, 0M, creator.CC1.PK);
				line2.AL_AT = creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { aRInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocument = newFactory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 1, complianceDocument.Length);

				complianceDocument[0].ADH_ComplianceSubType = "TXC";
				complianceDocument[0].ADH_DocumentNumber = "001";
				complianceDocument[0].ADH_DocumentDate = ZDateTime.Today;
				complianceDocument[0].ADH_ReportingPeriod = 201902;
				newFactory.Save();

				var queueEntries = new DynamicBusinessObjectCollection(newFactory);
				queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
				AssertEquals("only one compliance document is created in AccTransactionComplianceReportQueue table", 1, queueEntries.Count);
				AssertEquals(new ZDateTime(2019, 01, 01), (ZDateTime)queueEntries[0]["ACQ_Date"]);
				AssertEquals(complianceDocument[0].PK, (ZGuid)queueEntries[0]["ACQ_ParentID"]);
			}
		}

		[TestDate(2019, 06, 01)]
		public override void TestCheckCanVoid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var complianceDocument = GetComplianceDocumentHeader();
				complianceDocument.ADH_DocumentDate = ZDateTime.Today;
				complianceDocument.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
				complianceDocument.ADH_DocumentNumber = "001";
				complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
				complianceDocument.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, complianceDocument.PK));
				pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
				Factory.Save();

				AssertEquals("Compliance Document with sub type TXE and TCE cannot be voided until the original document has been successfully uploaded.", complianceDocument.CheckCanVoid());

				complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
				AssertEquals("Compliance Document with sub type TXE and TCE cannot be voided until the original document has been successfully uploaded.", complianceDocument.CheckCanVoid());

				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				AssertEquals(ZString.Empty, complianceDocument.CheckCanVoid());

				complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
				pivot.AIP_Status = EInvoicingPivotState.Batched;
				AssertEquals(ZString.Empty, complianceDocument.CheckCanVoid());
			}
		}

		#region E-Invoicing

		public void TestCreateEInvoicingPivot()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1.0m, TestObjectCreator.Debtor);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 1.0m, "desc", 10m);
				invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				new ComplianceDocumentCreator(new[] { invoice }, Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should not be queued into pivot table", 0, pivots.Length);

				var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK)).First();
				header.ADH_DocumentNumber = "AA00000001";
				header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
				Factory.Save();

				AssertEquals(Core.Constants.ComplianceDocumentStatus.NumberSet, header.ADH_DocumentStatus);
				pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should queued into pivot table", 1, pivots.Length);

				header.ADH_Description = "compliance document already queued";
				Factory.Save();

				pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should not queued into pivot table again", 1, pivots.Length);

				AssertEquals(header.PK, pivots.First().AIP_ParentID);
				AssertEquals(AccComplianceDocumentHeaderSchema.Constants.Prefix, pivots.First().AIP_ParentTableCode);
				AssertEquals(header.ADH_GC_Company, pivots.First().AIP_GC);

				header.Void();
				Factory.Save();
				Assert(header.IsVoided);

				pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should not queued into pivot table again", 1, pivots.Length);
			}
		}

		public void TestNoEInvoicingPivotCreatedForVoidingSequenceNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_MaximumNumberDigits = 8;
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 100;
				sequence.XD_NextNumber = 5;
				sequence.XD_IsActive = true;
				sequence.XD_SequenceClass = "TXE";

				VoidingSequenceNumberBusinessObject bo = new VoidingSequenceNumberBusinessObject(sequence);
				bo.VoidingToNumber = "20";
				bo.VoidNumbersInRange();
				Factory.Save();
				var pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should not be queued into pivot table", 0, pivots.Length);
			}
		}

		public void TestCreateEInvoicingPivotWhenVoiding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1.0m, TestObjectCreator.Debtor);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 1.0m, "desc", 10m);
				invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				new ComplianceDocumentCreator(new[] { invoice }, Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK)).First();
				Assert(header.IsAdded);

				var pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should not be queued into pivot table", 0, pivots.Length);

				header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
				header.ADH_DocumentNumber = "AA00000001";
				header.Void();
				Factory.Save();

				Assert(header.IsVoided);
				pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH"));
				AssertEquals("Compliance document should not queued into pivot table again", 1, pivots.Length);
			}
		}

		public void TestRequeueWhenVoiding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1.0m, TestObjectCreator.Debtor);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 1.0m, "desc", 10m);
				invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK)).First();
				Assert(header.IsAdded);

				header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
				header.ADH_DocumentNumber = "AA00000001";
				Factory.Save();

				var pivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, "ADH")).FirstOrDefault();
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				header.Void();
				Factory.Save();

				AssertEquals(ZGuid.Empty, pivot.AIP_AIB);
				AssertEquals(EInvoicingPivotState.Queued, pivot.AIP_Status);
				AssertEquals(ZString.Empty, pivot.AIP_ErrorDescription);
				AssertEquals(false, pivot.AIP_IsNotifiedByEmail);
				AssertEquals(ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals(ZDateTime.Empty, pivot.AIP_LastSentTimeUtc);
			}
		}

		#endregion

		public void TestComplianceNumberSequence()
		{
			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arInv }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

			var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK);
			var arComplianceDocumentHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);

			AssertNotNull(arComplianceDocumentHeader);
			Assert(!arComplianceDocumentHeader.IsCorrected);
			AssertEquals(arComplianceDocumentHeader.ADH_TransactionType, arComplianceDocumentHeader.ComplianceTransactionType);
			AssertEquals(arComplianceDocumentHeader.ADH_ComplianceSubType, arComplianceDocumentHeader.ComplianceSubType);
			AssertEquals(arComplianceDocumentHeader.ADH_DocumentDate, arComplianceDocumentHeader.ComplianceDocumentDate);
			AssertEquals(ZDateTime.Empty, arComplianceDocumentHeader.PostDate);
			AssertEquals(ZDateTime.Empty, arComplianceDocumentHeader.InvoiceDate);
		}

		public void TestAllocateComplianceSequenceNumberWithNumberFormat()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Indonesia);
			var configurationCollection = ComplianceNumberSequenceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "NTC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = ZDateTime.Now.AddMonths(1);
			complianceSequence.XD_NumberFormat = "AAA";
			Factory.Save();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arInv }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

			var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK);
			var invHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
			invHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			invHeader.ADH_XD_ComplianceBook = complianceSequence.PK;

			invHeader.SetComplianceDocumentNumber();
			var expectedNumber = invHeader.ComplianceDocumentDate.Day.ToString("00") + "aC abc-0010025";

			Factory.Save();
			AssertEquals(expectedNumber, invHeader.ADH_DocumentNumber);
		}

		public void TestADH_XD_ComplianceBook_ReadOnlyAndADH_DocumentNumber_ReadOnly()
		{
			var header = GetComplianceDocumentHeader();
			header.ADH_Ledger = LedgerTypes.AccountsReceivable;
			header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			Assert(!header.ADH_XD_ComplianceBookInfo.ReadOnly);
			Assert(header.ADH_DocumentNumberInfo.ReadOnly);

			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			header.TransactionHeaders.Add(arCreditNote);
			header.ADH_TransactionType = TransactionTypes.CreditNote;

			Assert(header.ADH_XD_ComplianceBookInfo.ReadOnly);

			header.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.NumberSet;
			Assert(header.ADH_DocumentNumberInfo.ReadOnly);

			header.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;
			Assert(!header.ADH_DocumentNumberInfo.ReadOnly);
		}

		public override void TestSetComplianceDocumentNumber()
		{
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "NTC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = ZDateTime.Now.AddMonths(1);
			Factory.Save();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			var creditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
			creditNote.AH_TransactionBelongsToGroup = arInv.PK;
			var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
			creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { creditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

			var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
			var crdHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
			crdHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			crdHeader.ADH_XD_ComplianceBook = complianceSequence.PK;

			crdHeader.SetComplianceDocumentNumber();
			Factory.Save();

			AssertEquals("abc-0010025", crdHeader.ADH_DocumentNumber);

			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				crdHeader.ADH_DocumentNumber = ZString.Empty;
				crdHeader.SetComplianceDocumentNumber();
				AssertEquals(ZString.Empty, crdHeader.ADH_DocumentNumber);
			}
		}

		public void TestSetComplianceDocumentNumber_AllowToSetNumberOnComplianceSubType()
		{
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "TXE";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = ZDateTime.Now.AddMonths(1);
			Factory.Save();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arInv }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

			var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK);
			var invHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
			invHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			invHeader.ADH_XD_ComplianceBook = complianceSequence.PK;

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			invHeader.SetComplianceDocumentNumber();
			AssertEquals(string.Empty, invHeader.ADH_DocumentNumber);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var mockIComplianceDocumentNumberProvider = new Mock<IComplianceDocumentNumberProvider>();
			mockIComplianceDocumentNumberProvider.Setup(x => x.AllocateComplianceDocumentNumberErrorMessage(It.IsAny<IEnumerable<ARComplianceDocumentHeader>>())).Returns("TEST");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentNumberProvider>>().Setup(x => x.Get()).Returns(mockIComplianceDocumentNumberProvider.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				invHeader.SetComplianceDocumentNumber();
				AssertEquals(string.Empty, invHeader.ADH_DocumentNumber);

				mockIComplianceDocumentNumberProvider.Setup(x => x.AllocateComplianceDocumentNumberErrorMessage(It.IsAny<IEnumerable<ARComplianceDocumentHeader>>())).Returns(string.Empty);
				invHeader.SetComplianceDocumentNumber();
				Factory.Save();
				AssertEquals("abc-0010025", invHeader.ADH_DocumentNumber);
			}
		}

		public void TestSetComplianceSequenceBook()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "TXC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = ZDateTime.Now.AddMonths(1);
			Factory.Save();

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TCR";
			complianceSequence1.XD_Prefix = "abc-001";
			complianceSequence1.XD_StartNumber = 1;
			complianceSequence1.XD_EndNumber = 99;
			complianceSequence1.XD_MaximumNumberDigits = 4;
			complianceSequence1.XD_NextNumber = 25;
			complianceSequence1.XD_IsActive = true;
			complianceSequence1.XD_ExpiryDate = ZDateTime.Now.AddMonths(1);
			Factory.Save();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = TestObjectCreator.CreateARInvoiceLine(arInv, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 0.655m, "Test", 0);
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arInv }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			Factory.Save();

			var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK);
			var invHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
			invHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			invHeader.SetComplianceSequenceBook();
			AssertEquals(complianceSequence.PK, invHeader.ADH_XD_ComplianceBook);

			var arCrd = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
			var arCrdLine = TestObjectCreator.CreateARCreditNoteLine(arCrd, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
			arCrdLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arCrd }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			Factory.Save();

			query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
			var crdHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
			crdHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			crdHeader.SetComplianceSequenceBook();
			AssertEquals(complianceSequence1.PK, crdHeader.ADH_XD_ComplianceBook);

			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				crdHeader.ADH_XD_ComplianceBook = Guid.Empty;
				crdHeader.SetComplianceDocumentNumber();
				AssertEquals(Guid.Empty, crdHeader.ADH_XD_ComplianceBook);
			}
		}

		public void TestPropertiesReadOnly()
		{
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var invHeader = GetComplianceDocumentHeader() as ARComplianceDocumentHeader;
				invHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;

				Assert(invHeader.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
				Assert(!invHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(!invHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(!invHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);

				invHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.NumberSet;

				Assert(invHeader.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);

				var creditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
				var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
				creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				new ComplianceDocumentCreator(new[] { creditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
				var crdHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);

				crdHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;

				Assert(crdHeader.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
				Assert(!crdHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(!crdHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(!crdHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);

				crdHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.NumberSet;

				Assert(crdHeader.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);
			}

			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invHeader = GetComplianceDocumentHeader() as ARComplianceDocumentHeader;
				invHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;

				Assert(invHeader.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
				Assert(!invHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(!invHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(!invHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);

				invHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.NumberSet;

				Assert(invHeader.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(invHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);

				var creditNote = TestObjectCreator.CreateARCreditNote("CRD002", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
				var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
				creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				new ComplianceDocumentCreator(new[] { creditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
				var crdHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);

				crdHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;

				Assert(!crdHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(!crdHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(!crdHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(!crdHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);

				crdHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.NumberSet;

				Assert(crdHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_DocumentDate_ReadOnly_ForTestOnly);
				Assert(crdHeader.ADH_DocumentNumber_ReadOnly_ForTestOnly);
			}
		}

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("This record can not be deleted, because it's document status is not ADD.", GetComplianceDocumentHeader().ReasonForNotAbleToDelete);
		}

		public void TestInternalReference_ShareSequentialARComplianceDocumentsReferenceNumbers_False()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			using (Db.Connection.BeginTransactionWithManager())
			{
				Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, companyPK).SetNext(Db.Connection, 1000);
				Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, companyPK).SetNext(Db.Connection, 1000);

				AccountingConfigurationRegistry.Instance.ShareSequentialARComplianceDocumentsReferenceNumbers.SetValue(companyPK, Guid.Empty, Guid.Empty, ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(false));

				var complianceDocumentHeader1 = SetupARComplianceDocumentHeader(TransactionTypes.Invoice, "AA001");
				AssertEquals("00001000", complianceDocumentHeader1.ADH_InternalReference);

				var complianceDocumentHeader2 = SetupARComplianceDocumentHeader(TransactionTypes.Invoice, "AA002");
				AssertEquals("00001001", complianceDocumentHeader2.ADH_InternalReference);

				var complianceDocumentHeader3 = SetupARComplianceDocumentHeader(TransactionTypes.CreditNote, "AA003");
				AssertEquals("00001000", complianceDocumentHeader3.ADH_InternalReference);
			}
		}

		public void TestInternalReference_ShareSequentialARComplianceDocumentsReferenceNumbers_True()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			using (Db.Connection.BeginTransactionWithManager())
			{
				Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, companyPK).SetNext(Db.Connection, 1000);
				Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, companyPK).SetNext(Db.Connection, 1000);

				AccountingConfigurationRegistry.Instance.ShareSequentialARComplianceDocumentsReferenceNumbers.SetValue(companyPK, Guid.Empty, Guid.Empty, ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true));

				var complianceDocumentHeader1 = SetupARComplianceDocumentHeader(TransactionTypes.Invoice, "AA001");
				AssertEquals("00001000", complianceDocumentHeader1.ADH_InternalReference);

				var complianceDocumentHeader2 = SetupARComplianceDocumentHeader(TransactionTypes.Invoice, "AA002");
				AssertEquals("00001001", complianceDocumentHeader2.ADH_InternalReference);

				var complianceDocumentHeader3 = SetupARComplianceDocumentHeader(TransactionTypes.CreditNote, "AA003");
				AssertEquals("00001002", complianceDocumentHeader3.ADH_InternalReference);
			}
		}

		AccComplianceDocumentHeader SetupARComplianceDocumentHeader(ZString transactionType, ZString documentNumber)
		{
			var factory = new BusinessObjectFactory();

			var complianceDocumentHeader = factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocumentHeader.ADH_TransactionType = transactionType;
			complianceDocumentHeader.ADH_DocumentNumber = documentNumber;
			complianceDocumentHeader.ADH_QRCode1 = ZString.Empty;
			complianceDocumentHeader.ADH_QRCode2 = ZString.Empty;
			factory.Save();

			return complianceDocumentHeader;
		}
	}
}
