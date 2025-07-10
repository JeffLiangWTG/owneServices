using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	public class ComplianceDocumentHelperTest : TestCaseWithFactory
	{
		[TestDate(2019, 04, 16, 15, 35, 00)]
		public void TestCreateVoidedComplianceDocumentHeader()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var sequence = new ComplianceDocumentCreatorTest().CreateComplianceSequence("AAA", "NTC", ComplianceBookAllocationLevel.Company);
				sequence.XD_Prefix = "AD";
				sequence.XD_MaximumNumberDigits = 8;

				var bo = new VoidingSequenceNumberBusinessObject(sequence);
				bo.VoidingToNumber = "00000010";
				bo.VoidNumbersInRange();
				sequence.Factory.Save();

				var query = new ZQuery();
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentStatus, ComplianceDocumentStatus.Voided);
				query.OrderBy = AccComplianceDocumentHeaderSchema.Constants.ADH_DocumentNumber + " ASC";
				var voidedHeaders = Factory.Load<ARComplianceDocumentHeader>(query);
				var query1 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);

				AssertEquals("Should create 10 voided compliance document headers", 10, voidedHeaders.Length);

				for (int i = 0; i < voidedHeaders.Length; i++)
				{
					var documentNumber = sequence.XD_Prefix + (i + 1).ToString().PadLeft(sequence.XD_MaximumNumberDigits, '0');

					AssertEquals(LedgerTypes.AccountsReceivable, voidedHeaders[i].ADH_Ledger);
					AssertEquals(ComplianceDocumentStatus.Voided, voidedHeaders[i].ADH_DocumentStatus);
					AssertEquals(documentNumber, voidedHeaders[i].ADH_DocumentNumber);
					AssertEquals(sequence.PK, voidedHeaders[i].ADH_XD_ComplianceBook);
					AssertEquals(GlbCompany.CurrentCompany.PK, voidedHeaders[i].ADH_GC_Company);
					AssertEquals("VAT", voidedHeaders[i].ADH_DocumentType);
					AssertEquals("NTC", voidedHeaders[i].ADH_ComplianceSubType);
					AssertEquals(new ZDateTime(2019, 04, 16, 15, 35, 00), voidedHeaders[i].ADH_DocumentDate);
					AssertEquals(201904, voidedHeaders[i].ADH_ReportingPeriod);
					AssertEquals("Sequence number voided via compliance invoice book", voidedHeaders[i].ADH_Description);
					Assert(voidedHeaders[i].Logs.HasLogWith(query1));
				}
			}
		}

		public void TestAllocateComplianceDocuments()
		{
			var complianceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceBook.XD_IsActive = ZBool.True;
			complianceBook.XD_EndNumber = 100;
			complianceBook.XD_NextNumber = 1;
			complianceBook.XD_StartDate = ZDate.Today.AddYears(-1);
			complianceBook.XD_ExpiryDate = ZDateTime.Today.AddYears(1);
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
			arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine1.AL_OSExTaxAmount = 100m;
			arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
			arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
			var arInvLine2 = (ARInvoiceLine)arInvoice.Lines.AddNew();
			arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine2.AL_OSExTaxAmount = 100m;
			arInvLine2.AL_AC = TestObjectCreator.DSBChargeCode1.PK;
			arInvLine2.AL_AT = TestObjectCreator.GST2.PK;
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			Factory.Save();
			var query = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK);
			var aRComplianceDocumentHeaders = Factory.Load<ARComplianceDocumentHeader>(query);
			AssertEquals("should be 2 compliancedocumentheaders", 2, aRComplianceDocumentHeaders.Length);

			var complianceDocumentDates = new ZDateTime[] { ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(1) };
			for (int i = 0; i < 2; i++)
			{
				aRComplianceDocumentHeaders[i].ADH_DocumentDate = complianceDocumentDates[i];
				aRComplianceDocumentHeaders[i].ADH_XD_ComplianceBook = complianceBook.PK;
				aRComplianceDocumentHeaders[i].ADH_ReportingPeriod = aRComplianceDocumentHeaders[i].ADH_DocumentDate.Year * 100 + aRComplianceDocumentHeaders[i].ADH_DocumentDate.Month;
			}
			Assert(aRComplianceDocumentHeaders[0].ADH_DocumentDate > aRComplianceDocumentHeaders[1].ADH_DocumentDate);
			ComplianceDocumentHelper.AllocateComplianceDocuments(Factory, aRComplianceDocumentHeaders);
			Factory.Save();
			Assert(aRComplianceDocumentHeaders[0].ADH_DocumentNumber > aRComplianceDocumentHeaders[1].ADH_DocumentNumber);

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine3 = (ARInvoiceLine)arInvoice2.Lines.AddNew();
			arInvLine3.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine3.AL_OSExTaxAmount = 100m;
			arInvLine3.AL_AC = TestObjectCreator.DSBChargeCode.PK;
			arInvLine3.AL_AT = TestObjectCreator.GST1.PK;
			var arInvLine4 = (ARInvoiceLine)arInvoice2.Lines.AddNew();
			arInvLine4.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine4.AL_OSExTaxAmount = 100m;
			arInvLine4.AL_AC = TestObjectCreator.DSBChargeCode.PK;
			arInvLine4.AL_AT = TestObjectCreator.GST2.PK;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnforceReceivablesComplianceDocumentDateandNumberSequencing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			new ComplianceDocumentCreator(new[] { arInvoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			Factory.Save();

			var query2 = new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.AALSHI.PK);
			var aRComplianceDocumentHeaders2 = Factory.Load<ARComplianceDocumentHeader>(query2);
			AssertEquals("should be 2 compliancedocumentheaders", 2, aRComplianceDocumentHeaders2.Length);

			var complianceDocumentDates2 = new ZDateTime[] { ZDateTime.Today, ZDateTime.Today.AddDays(3) };
			for (int i = 0; i < 2; i++)
			{
				aRComplianceDocumentHeaders2[i].ADH_DocumentDate = complianceDocumentDates2[i];
				aRComplianceDocumentHeaders2[i].ADH_XD_ComplianceBook = complianceBook.PK;
				aRComplianceDocumentHeaders2[i].ADH_ReportingPeriod = aRComplianceDocumentHeaders2[i].ADH_DocumentDate.Year * 100 + aRComplianceDocumentHeaders2[i].ADH_DocumentDate.Month;
			}

			ZString[] complianceBooks1 = ComplianceDocumentHelper.AllocateComplianceDocuments(Factory, aRComplianceDocumentHeaders2);
			Factory.Save();

			AssertEquals(1, complianceBooks1.Length);
			AssertEquals(ZString.Empty, aRComplianceDocumentHeaders2[0].ADH_DocumentNumber);
			AssertEquals(ZString.Empty, aRComplianceDocumentHeaders2[1].ADH_DocumentNumber);
		}

		public void TestPrintComplianceDocument()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line1WithCC1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			var line2WithCC1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M, 20M, 0M, TestObjectCreator.CC1.PK);
			var line3WithCC2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 300M, 30M, 0M, TestObjectCreator.CC2.PK);
			var line4WithGlHeader = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 400M, 40M, TestObjectCreator.GLHeader1.PK);
			var line5WithGlHeader = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 500M, 50M, TestObjectCreator.GLHeader1.PK);
			line1WithCC1.AL_AT = TestObjectCreator.GST1.PK;
			line2WithCC1.AL_AT = TestObjectCreator.GST1.PK;
			line3WithCC2.AL_AT = TestObjectCreator.GST1.PK;
			line4WithGlHeader.AL_AT = TestObjectCreator.GST2.PK;
			line5WithGlHeader.AL_AT = TestObjectCreator.GST2.PK;

			var query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, "Triplicate Cash Register GUI");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "ARComplianceDocument");
			var stmMenu = Factory.Load<StmMenuItem>(query).First();

			StmPrintQueue printer = Factory.NewWithValidTestData<StmPrintQueue>();
			var sequence = new ComplianceDocumentCreatorTest().CreateComplianceSequence("AAA", "NTC", ComplianceBookAllocationLevel.Company, factory: Factory);
			sequence.XD_SU_MenuItem = stmMenu.PK;
			sequence.XD_MaxChargesPerTransaction = 40;
			sequence.XD_RollupBehaviourWhenMaxExceeded = Enterprise.Core.Constants.ComplianceRollupBehaviourType.SinglePageSummarize;
			sequence.XD_SQ_DocumentPrintQueue = printer.PK;

			var newSequence = new ComplianceDocumentCreatorTest().CreateComplianceSequence("AAA", "NTC", ComplianceBookAllocationLevel.Company, factory: Factory);
			newSequence.XD_SU_MenuItem = stmMenu.PK;
			newSequence.XD_MaxChargesPerTransaction = 40;
			newSequence.XD_RollupBehaviourWhenMaxExceeded = Enterprise.Core.Constants.ComplianceRollupBehaviourType.SinglePageSummarize;

			new ComplianceDocumentCreator(new InvoicingBase[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();

			var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
			AssertEquals(2, headers.Length);
			headers.ForEach(x => x.ADH_XD_ComplianceBook = sequence.PK);

			Factory.Save();

			headers.ForEach(x => Assert("Print count should be 0 before print.", x.ADH_PrintCount == 0));
			ComplianceDocumentHelper.PrintComplianceDocument(headers);
			headers.ForEach(x => Assert("Print count should be 1 after print.", x.ADH_PrintCount == 1));
			var stmPrintJob = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, headers.Select(y => y.PK)));
			AssertEquals(2, stmPrintJob.Length);
			AssertNull("NOT show delivery form when printer is specific in sequence.", ZFormModaliser.LastFormShownDialogForTest);

			headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
			AssertEquals(2, headers.Length);
			headers.ForEach(x => x.ADH_XD_ComplianceBook = newSequence.PK);

			Factory.Save();

			ComplianceDocumentHelper.PrintComplianceDocument(headers);
			AssertEquals("Show delivery form when printer is not specific in sequence.", "DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
		}

		public void TestGetCreateComplianceDocumentsSecurity()
		{
			AssertNotNull(ComplianceDocumentHelper.GetCreateComplianceDocumentsSecurity(LedgerTypes.AccountsPayable));
			AssertNotNull(ComplianceDocumentHelper.GetCreateComplianceDocumentsSecurity(LedgerTypes.AccountsPayable));
			AssertExceptionThrown<NotSupportedException>(() => ComplianceDocumentHelper.GetCreateComplianceDocumentsSecurity("XX"));
		}

		public void TestProxyRegistrationNumber()
		{
			var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "VAT";
			cusCode1.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusCode1.OK_CustomsRegNo = "12345675";

			var cusCode2 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "GTX";
			cusCode2.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusCode2.OK_CustomsRegNo = "565566675";

			AssertEquals("ProxyVATRegistrationNumber", "12345675", ComplianceDocumentHelper.GetCompanyProxyVATNumber());
			AssertEquals("ProxyGTXRegistrationNumber", "565566675", ComplianceDocumentHelper.GetCompanyProxyGTXNumber());
		}

		public void TestGetFormatCodeForExport()
		{
			#region AP

			var ledger = LedgerTypes.AccountsPayable;
			var subType = "TXI";
			AssertEquals("21", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXP";
			AssertEquals("21", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TDI";
			AssertEquals("22", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TDC";
			AssertEquals("22", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TDP";
			AssertEquals("22", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TCR";
			AssertEquals("23", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TCE";
			AssertEquals("23", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TCD";
			AssertEquals("24", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXE";
			AssertEquals("25", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXC";
			AssertEquals("25", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TSX";
			AssertEquals("26", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TSD";
			AssertEquals("27", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXS";
			AssertEquals("28", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "XXX";
			AssertEquals("  ", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			#endregion

			#region AR

			ledger = LedgerTypes.AccountsReceivable;
			subType = "TXI";
			AssertEquals("31", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXP";
			AssertEquals("31", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TDI";
			AssertEquals("32", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TDC";
			AssertEquals("32", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TDP";
			AssertEquals("32", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TCR";
			AssertEquals("33", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TCE";
			AssertEquals("33", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TCD";
			AssertEquals("34", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXE";
			AssertEquals("35", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TXC";
			AssertEquals("35", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			subType = "TSX";
			AssertEquals("  ", ComplianceDocumentHelper.GetFormatCode(subType, ledger));

			#endregion
		}

		public void TestJoinAndAttachLineNumber()
		{
			List<string> messages = null;
			AssertEquals(null, ComplianceDocumentHelper.JoinAndAttachLineNumber(messages));

			messages = new List<string>();
			AssertEquals(string.Empty, ComplianceDocumentHelper.JoinAndAttachLineNumber(messages));

			messages.Add("aa");
			AssertEquals("aa", ComplianceDocumentHelper.JoinAndAttachLineNumber(messages));

			messages.Add("bb");
			AssertEquals(@"1. aa
2. bb", ComplianceDocumentHelper.JoinAndAttachLineNumber(messages));

			messages.Add("cc");
			AssertEquals(@"1. aa
2. bb
3. cc", ComplianceDocumentHelper.JoinAndAttachLineNumber(messages));
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.GST1.AT_PostingGroupId = 1;
			TestObjectCreator.GST2.AT_PostingGroupId = 2;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
