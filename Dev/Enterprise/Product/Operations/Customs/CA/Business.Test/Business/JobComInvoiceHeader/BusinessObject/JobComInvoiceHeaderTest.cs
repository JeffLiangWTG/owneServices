using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageBuilders.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestJZ_InvoiceDisplaySequence_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			cadEntry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			var entryLine = cadEntry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();
			Assert(invoice.JZ_InvoiceDisplaySequenceInfo.ReadOnly);

			cadEntry.CH_EntryStatus = CADEntryStatusList.Codes.Rejected;
			Assert(!invoice.JZ_InvoiceDisplaySequenceInfo.ReadOnly);

			cadEntry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Assert(invoice.JZ_InvoiceDisplaySequenceInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(invoiceHeader,
				"CAJobComInvoiceHeader",
				schemaTypeName: nameof(AutoJobComInvoiceHeader.Schema));
		}

		[TestDate(2023, 02, 13)]
		public void TestSetCustomsCommenceInfoWhenAttachLVS()
		{
			var lvs = Factory.New<JobDeclaration>();
			var lvx = Factory.New<JobDeclaration>();
			var invoice = lvx.Invoices.AddNew();
			AssertEquals(ZDateTime.Empty, lvx.JE_CustomsCommencedDate);
			AssertEquals(ZString.Empty, lvx.JE_GS_NKCustomsCommencedUser);

			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice, lvs);
			AssertEquals("13-Feb-23", lvx.JE_CustomsCommencedDate.ToShortDateString());
			AssertEquals(GlbStaff.CurrentUser.GS_Code, lvx.JE_GS_NKCustomsCommencedUser);

			LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice, lvs);
			AssertEquals(ZString.Empty, lvx.JE_CustomsCommencedDate.ToShortDateString());
			AssertEquals(ZString.Empty, lvx.JE_GS_NKCustomsCommencedUser);
		}

		public void TestCanDetach()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 3;
			var invoice = declaration.Invoices.AddNew();
			Assert(invoice.CanDetach);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Assert("Can not detach when there is CAD Goods Shipment Sequence", !invoice.CanDetach);
		}

		public void TestGoodsShipmentSequence()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 3;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			AssertEquals((ZShort)3, invoice.GoodsShipmentSequence);
		}

		protected override IEnumerable<Func<BaseJobDeclaration, JobComInvoiceHeader, BaseJobComInvoiceLine, (ZPropertyInfo, IZType)>> GetJZ_Calc_LinesEnteredRelatedProperties()
		{
			foreach (var item in base.GetJZ_Calc_LinesEnteredRelatedProperties())
			{
				yield return item;
			}
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobComInvoiceLine)invoiceLine).CA_CalculationMethodInfo, new ZString("Z"));
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobComInvoiceLine)invoiceLine).JI_ParentIDInfo, ZGuid.NewZGuid());
		}

		public void TestRecalculatePageNumberWhenJZ_InvoiceDisplaySequenceChanged()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.CA_PageNumber = 1;
			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.CA_PageNumber = 2;

			var invoice2 = jobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var line3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.CA_PageNumber = 3;
			var line4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line4.CA_PageNumber = 4;

			var invoice3 = jobDeclaration.Invoices.AddNew();
			invoice3.JZ_InvoiceDisplaySequence = 3;
			var line5 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line5.CA_PageNumber = 5;
			var line6 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line6.CA_PageNumber = 6;

			AssertEquals(1, line1.CA_PageNumber);
			AssertEquals(2, line2.CA_PageNumber);
			AssertEquals(3, line3.CA_PageNumber);
			AssertEquals(4, line4.CA_PageNumber);
			AssertEquals(5, line5.CA_PageNumber);
			AssertEquals(6, line6.CA_PageNumber);

			invoice.JZ_InvoiceDisplaySequence = 4;

			AssertEquals(5, line1.CA_PageNumber);
			AssertEquals(6, line2.CA_PageNumber);
			AssertEquals(1, line3.CA_PageNumber);
			AssertEquals(2, line4.CA_PageNumber);
			AssertEquals(3, line5.CA_PageNumber);
			AssertEquals(4, line6.CA_PageNumber);
		}

		public void TestRecalculatePageNumberWhenJZ_JEChanged()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.CA_PageNumber = 9;

			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.CA_PageNumber = 8;

			JobComInvoiceHeader invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var line3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.CA_PageNumber = 7;
			var line4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line4.CA_PageNumber = 7;

			JobComInvoiceHeader invoice3 = Factory.New<JobComInvoiceHeader>();
			invoice3.JZ_InvoiceDisplaySequence = 3;
			var line5 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line5.CA_PageNumber = 1;
			var line6 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line6.CA_PageNumber = 2;
			jobDeclaration.Invoices.Add(invoice);
			jobDeclaration.Invoices.Add(invoice2);
			jobDeclaration.Invoices.Add(invoice3);

			AssertEquals(2, line1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(3, line3.CA_PageNumber);
			AssertEquals(3, line4.CA_PageNumber);
			AssertEquals(4, line5.CA_PageNumber);
			AssertEquals(5, line6.CA_PageNumber);
		}

		public void TestRefreshDeclarationInvoiceLinesWhenJZ_JEChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			AssertEquals(true, invoiceHeader.HasInvoiceLinesWithCFIAPGA);
			AssertEquals(false, declaration.HasInvoiceLinesWithCFIAPGA);

			invoiceHeader.JZ_JE = declaration.PK;

			AssertEquals(true, declaration.HasInvoiceLinesWithCFIAPGA);
		}

		public void TestJZ_InvoiceNumber()
		{
			AssertSetJZ_InvoiceNumberForB2B3X(JobMessageTypeList.Codes.B2Adjustments);
			AssertSetJZ_InvoiceNumberForB2B3X(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertSetJZ_InvoiceNumberForB2B3X(ZString messagetype)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = messagetype;
			var invoice = jobDeclaration.B2AsAccountedForInvoices.AddNew();

			invoice.JZ_InvoiceNumber = "1";
			AssertEquals("1", invoice.JZ_InvoiceNumber);

			invoice.JZ_InvoiceNumber = "01";
			AssertEquals("01", invoice.JZ_InvoiceNumber);

			invoice.JZ_InvoiceNumber = "INV1";
			AssertEquals("1", invoice.JZ_InvoiceNumber);

			invoice.JZ_InvoiceNumber = "INV01";
			AssertEquals("01", invoice.JZ_InvoiceNumber);

			invoice.JZ_InvoiceNumber = "NS";
			AssertEquals("NS1", invoice.JZ_InvoiceNumber);
		}

		public void TestJZ_InvoiceNumber_ReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = Declaration.B2AsAccountedForInvoices.AddNew();
			Assert(!invoice.JZ_InvoiceNumberInfo.ReadOnly);

			invoice.JZ_InvoiceNumber = "1";
			Assert(invoice.JZ_InvoiceNumberInfo.ReadOnly);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(invoice.JZ_InvoiceNumberInfo.ReadOnly);

			invoice.JZ_InvoiceNumber = "";
			Assert(!invoice.JZ_InvoiceNumberInfo.ReadOnly);
		}

		public void TestCA_RN_NKSource()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.CA_StateOfSource = USStatesList.Codes.NewYork;
			InvoiceHeader.CA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("CA_StateOfSource", USStatesList.Codes.NewYork, InvoiceHeader.CA_StateOfSource);

			InvoiceHeader.CA_RN_NKSource = Core.Constants.CountryCodes.Canada;
			AssertEquals("CA_StateOfSource", string.Empty, InvoiceHeader.CA_StateOfSource);
		}

		public void TestCA_ReadyForConsolidationTick()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CompanyData.OB_IsDebtor = true;
			importer.MiscServ.OM_ARGlobalCreditApproved = true;
			importer.MiscServ.OM_ARGlobalOnCreditHold = true;
			importer.CompanyData.OB_IsDebtor = true;
			importer.CompanyData.OB_AROnCreditHold = true;
			importer.OH_Code = "TSTOH";
			Factory.Save();

			var lvxJob = Factory.NewWithValidTestData<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvxJob.JE_OH_Importer = importer.PK;
			var lvxInvoice = lvxJob.LVXInvoiceHeader;
			lvxInvoice.JZ_OH_Buyer = importer.PK;
			lvxInvoice.CA_ReadyForConsolidation = false;
			Factory.Save();
			AssertNull("No declaration event", lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.ReadyForConsolidation));
			AssertNull("No Credit Check event", lvxInvoice.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CreditCheckFailed));
			lvxInvoice.CA_ReadyForConsolidation = true;
			Factory.Save();
			AssertNotNull("Event logged", lvxJob.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.ReadyForConsolidation));
			var jobDeclarationLog = lvxJob.Logs.Find(o => o.SL_SE_NKEvent == Events.ReadyForConsolidation.Code).FirstOrDefault();
			var exceptedSource = "This Declaration " + lvxJob.JobNumber;
			AssertEquals("Event source table", "JobDeclaration", jobDeclarationLog.SL_Table);
			AssertContains("Event source", exceptedSource, jobDeclarationLog.SL_TableFriendlyName);

			AssertNotNull("Event logged", lvxInvoice.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CreditCheckFailed));
			lvxInvoice.CA_ReadyForConsolidation = false;
			Factory.Save();
			var log = lvxJob.Logs.Find(o => o.SL_SE_NKEvent == Events.ReadyForConsolidation.Code).FirstOrDefault();
			AssertNotNull("Event logged", log);
			Assert("Event logged cancelled", log.IsCancelled);

			importer.CompanyData.OB_IsDebtor = false;
			importer.MiscServ.OM_ARGlobalCreditApproved = false;
			importer.MiscServ.OM_ARGlobalOnCreditHold = false;
			lvxInvoice.CA_ReadyForConsolidation = true;
			Factory.Save();
			log = lvxInvoice.Logs.Find(o => o.SL_SE_NKEvent == Events.CreditCheckFailed.Code).FirstOrDefault();
			AssertNotNull("Event logged", log);
			Assert("Event logged cancelled", log.IsCancelled);

			lvxInvoice.JZ_OH_Buyer = Guid.Empty;
			AssertEquals(false, lvxInvoice.CA_ReadyForConsolidation);
		}

		public void TestJZ_NoOfPacks_ReadOnly()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();

			Assert("Default to false", !invoiceHeader.JZ_NoOfPacksInfo.ReadOnly);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("Should be true as the parent declaration is Import and IID.", invoiceHeader.JZ_NoOfPacksInfo.ReadOnly);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Should be false as the parent declaration is not Import.", !invoiceHeader.JZ_NoOfPacksInfo.ReadOnly);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			Assert("Should be false as the parent declaration is not IID.", !invoiceHeader.JZ_NoOfPacksInfo.ReadOnly);
		}

		public void TestRefreshActualTotalPacksCount()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			jobDeclaration.JE_MasterBill = "X";
			jobDeclaration.Packages.RemoveAndDeleteAll();

			var bill = jobDeclaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();

			var package1 = packageGroup.Packages.AddNew();
			var package2 = packageGroup.Packages.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			linePackageCollection.RemoveAndDeleteAll();

			var lineLinkPackage = linePackageCollection.AddNew();
			lineLinkPackage.Package = package1;
			lineLinkPackage.IsLinked = true;
			lineLinkPackage.PackQty = 13;

			var headerPackageCollection = (InvoiceHeaderCusLinkPackageCollection)invoiceHeader.PackagesForInvoicesForBindingOnly;
			headerPackageCollection.RemoveAndDeleteAll();

			var headerLinkPackage = headerPackageCollection.AddNew();
			headerLinkPackage.Package = package2;
			headerLinkPackage.IsLinked = true;
			headerLinkPackage.PackQty = 26;

			invoiceHeader.JZ_NoOfPacks = 0;
			invoiceHeader.RefreshActualTotalPacksCount();

			AssertEquals("Should calculate the total linked packages count from the invoice and all child invoice lines.", 39m, invoiceHeader.JZ_NoOfPacks);
		}

		public void TestLVXIsReadyForConsolidationReadOnly()
		{
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxInvoice = lvxJob.LVXInvoiceHeader;
			Assert(lvxInvoice.CA_ReadyForConsolidationInfo.ReadOnly);
			Factory.Save();
			Assert(lvxInvoice.CA_ReadyForConsolidationInfo.ReadOnly);
			lvxInvoice.JZ_OH_Buyer = Factory.New<OrgHeader>().PK;
			Assert(!lvxInvoice.CA_ReadyForConsolidationInfo.ReadOnly);
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvsInvoice = lvsJob.Invoices.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxInvoice, lvsJob);
			Assert(lvxInvoice.CA_ReadyForConsolidationInfo.ReadOnly);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CAChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestJZ_OA_ManufacturerAddress()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			OrgAddress address1 = manufacturer.MainAddress;
			address1.Address1 = "testAddress";

			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OA_ManufacturerAddress = address1.PK;
			AssertEquals(manufacturer.PK, invoice.JZ_OA_ManufacturerAddress_ZAddress.OrgPK);
			AssertEquals("testAddress", invoice.JZ_OA_ManufacturerAddress_ZAddress.AddressFull);
		}

		public void TestHasHCPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithHCPGA);

			invoiceLine1.CA_HCInd = YesNoList.Codes.Yes;
			var hcHeader = invoiceLine1.HCPGAHeader;
			AssertEquals(true, invoice.HasInvoiceLinesWithHCPGA);

			AssertEquals(false, invoice.HasInvoiceLinesWithCPRIndOnHCPGA);
			hcHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.HasInvoiceLinesWithCPRIndOnHCPGA);
			hcHeader.CA_CPRProgramInd = YesNoList.Codes.No;
			AssertEquals(false, invoice.HasInvoiceLinesWithCPRIndOnHCPGA);

			AssertEquals(false, invoice.HasInvoiceLinesWithPESIndOnHCPGA);
			hcHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.HasInvoiceLinesWithPESIndOnHCPGA);
			hcHeader.CA_PESProgramInd = YesNoList.Codes.No;
			AssertEquals(false, invoice.HasInvoiceLinesWithPESIndOnHCPGA);
		}

		public void TestHasTCPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithTCPGA);

			invoiceLine1.CA_TCInd = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.HasInvoiceLinesWithTCPGA);
		}

		public void TestHasPHACPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithPHACPGA);

			invoiceLine1.CA_PHACInd = YesNoList.Codes.Yes;
			var phacHeader = invoiceLine1.PHACPGAHeader;
			AssertEquals(true, invoice.HasInvoiceLinesWithPHACPGA);
		}

		public void TestHasCFIAPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithCFIAPGA);

			invoiceLine1.CA_CFIAInd = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.HasInvoiceLinesWithCFIAPGA);
		}

		public void TestHasECCCPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithECCCPGA);

			invoiceLine1.CA_ECCCInd = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.HasInvoiceLinesWithECCCPGA);

			AssertEquals(false, invoice.HasInvoiceLinesWithWENIndOnECCCPGA);
			invoiceLine1.ECCCPGAHeader.CA_WENProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.HasInvoiceLinesWithWENIndOnECCCPGA);
			invoiceLine1.ECCCPGAHeader.CA_WENProgramInd = YesNoList.Codes.No;
			AssertEquals(false, invoice.HasInvoiceLinesWithWENIndOnECCCPGA);
		}

		public void TestHasInvoiceLinesWithPGARequirements()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3824600001");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithPGARequirements);

			invoiceLine1.JI_Tariff = "3824600001";
			AssertEquals(true, invoice.HasInvoiceLinesWithPGARequirements);
		}

		public void TestJZ_ValuationDateOverride()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			inv.JZ_InvoiceDate = ZDateTime.Empty;
			inv.JZ_ValuationDateOverride = new ZDateTime(2016, 3, 3);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2016, 3, 3), inv.JZ_InvoiceDate);

			inv.JZ_InvoiceDate = new ZDateTime(2016, 3, 5);
			inv.JZ_ValuationDateOverride = new ZDateTime(2016, 3, 4);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2016, 3, 4), inv.JZ_InvoiceDate);

			inv.JZ_InvoiceDate = new ZDateTime(2016, 3, 6);
			inv.JZ_ValuationDateOverride = new ZDateTime(2016, 3, 7);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2016, 3, 6), inv.JZ_InvoiceDate);

			inv.JZ_InvoiceDate = new ZDateTime(2016, 3, 5);
			inv.JZ_ValuationDateOverride = new ZDateTime(2016, 3, 4, 15, 56, 30);
			AssertEquals("JZ_ValuationDateOverride", new ZDateTime(2016, 3, 4), inv.JZ_ValuationDateOverride);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2016, 3, 4), inv.JZ_InvoiceDate);
		}

		[ExpectNoExceptions]
		public void TestCommercialInvoiceOriginatorIsDeleted()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Test Adress 1";
			inv.CommercialInvoiceOriginator.OrganisationPK = org.PK;
			var adress = new AddressFormatter(Factory, inv.CommercialInvoiceOriginator, GlbCompany.CurrentCompany, false).PostalAddress();
			AssertEquals("TEST ADRESS 1", adress);
			inv.CommercialInvoiceOriginator.Delete();
			adress = new AddressFormatter(Factory, inv.CommercialInvoiceOriginator, GlbCompany.CurrentCompany, false).PostalAddress();
			AssertEquals(ZString.Empty, adress);
		}

		[ExpectNoExceptions]
		public void TestJZ_RW_NKOriginStateMaxLength()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			header.JZ_RW_NKOriginState = "MEX";
			AssertEquals(2, header.JZ_RW_NKOriginStateInfo.MaxLength);
			AssertEquals("ME", header.JZ_RW_NKOriginState);
		}

		public void TestEffectiveImportClearanceProvince()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "0497", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ON");

			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0821", "0821", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "BC");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0001", "0001", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals(ZString.Empty, invoiceHeader.EffectiveImportClearanceProvince);
			declaration.JE_CustomsOffice = "0497";
			AssertEquals("ON", invoiceHeader.EffectiveImportClearanceProvince);
			invoiceHeader.CA_PortOfClearance = "0821";
			AssertEquals("BC", invoiceHeader.EffectiveImportClearanceProvince);
			invoiceHeader.CA_PortOfClearance = "0001";
			AssertEquals(ZString.Empty, invoiceHeader.EffectiveImportClearanceProvince);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Canada;

		public void TestTotalValueForDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.CA_CustomsValue = 1.0m;

			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.CA_CustomsValue = 2.0m;

			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.CA_CustomsValue = 3.0m;

			AssertEquals(6.0m, invoiceHeader.TotalValueForDuty);
		}

		public void TestCountryOfOrigin()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = "IL";
			Assert(!invoice.JZ_RW_NKOriginStateInfo.ReadOnly);
			AssertEquals("UIL", invoice.CountryOfOrigin);
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			Assert(invoice.JZ_RW_NKOriginStateInfo.ReadOnly);
			AssertEquals(Core.Constants.CountryCodes.Australia, invoice.CountryOfOrigin);
		}

		public void TestSynchroniser()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var asClaimedInvoice = invoice.CorrespondingAsClaimedForInvoice;
			AssertNotNull(asClaimedInvoice);
			AssertEquals("1", asClaimedInvoice.JZ_InvoiceNumber);

			AssertNotEquals("02", asClaimedInvoice.CA_TreatmentCode);
			invoice.CA_TreatmentCode = "02";
			AssertEquals("02", asClaimedInvoice.CA_TreatmentCode);
			Factory.Save();

			var factoryReload = new BusinessObjectFactory();
			var decReload = factoryReload.Load<JobDeclaration>(declaration.PK);
			AssertEquals(1, decReload.B2AsAccountedForInvoices.Count);
			AssertEquals(1, decReload.B2AsClaimedForInvoices.Count);
			var asAccountInvoiceReload = decReload.B2AsAccountedForInvoices[0];
			var asClaimedInvoiceReload = decReload.B2AsClaimedForInvoices[0];
			AssertNotEquals("D", asClaimedInvoiceReload.CA_TimeLimitCode);
			asAccountInvoiceReload.CA_TimeLimitCode = "D";
			AssertEquals("D", asClaimedInvoiceReload.CA_TimeLimitCode);
		}

		public void TestTimeLimitRange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 1, 1);
			var header = declaration.Invoices.AddNew();
			header.CA_TimeLimit = 10;
			AssertEquals((new ZDateTime(2024, 1, 1), ZDateTime.Empty), header.TimeLimitRange);

			header.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			AssertEquals((new ZDateTime(2024, 1, 1), new ZDateTime(2024, 1, 11)), header.TimeLimitRange);

			header.CA_TimeLimit = 1;
			header.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			AssertEquals((new ZDateTime(2024, 1, 1), new ZDateTime(2024, 1, 8)), header.TimeLimitRange);

			header.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertEquals((new ZDateTime(2024, 1, 1), new ZDateTime(2024, 2, 1)), header.TimeLimitRange);

			header.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Year;
			AssertEquals((new ZDateTime(2024, 1, 1), new ZDateTime(2025, 1, 1)), header.TimeLimitRange);

			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 2, 1);
			header.CA_TimeLimitCode = "XX";
			AssertEquals((new ZDateTime(2024, 2, 1), ZDateTime.Empty), header.TimeLimitRange);
		}

		public void TestGetWarningBeforeBeingDeleted()
		{
			var b2Declaration = Factory.New<JobDeclaration>();
			b2Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var asAccountedInvoice = b2Declaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = asAccountedInvoice.CorrespondingAsClaimedForInvoice;
			AssertEquals("Deleting a Sub-header in 'As Claimed' will delete the corresponding Sub-header and  lines in 'As Accounted'.", asClaimedInvoice.GetWarningBeforeBeingDeleted());

			var asClaimedInvoice1 = b2Declaration.B2AsClaimedForInvoices.AddNew();
			asClaimedInvoice1.JZ_InvoiceNumber = "INV1";
			AssertNotEquals("Deleting a Sub-header in 'As Claimed' will delete the corresponding Sub-header and  lines in 'As Accounted'.", asClaimedInvoice1.GetWarningBeforeBeingDeleted());
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = invoice.CorrespondingAsClaimedForInvoice;
			AssertNotNull(asClaimedInvoice);
			var asClaimedInvoicePK = asClaimedInvoice.PK;
			Factory.Save();

			invoice.Delete();
			var asClaimedInvoice1 = Factory.Load<JobComInvoiceHeader>(asClaimedInvoicePK);
			AssertNull(asClaimedInvoice1);
		}

		public void TestCanBeSynchronizedAfterReload()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var declarationPK = declaration.PK;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var declaration1 = factory.Load<JobDeclaration>(declarationPK);
			var invoice1 = declaration1.B2AsAccountedForInvoices[0];
			var asClaimedInvoice = invoice1.CorrespondingAsClaimedForInvoice;
			AssertNotEquals("02", asClaimedInvoice.CA_TreatmentCode);
			invoice1.CA_TreatmentCode = "02";
			AssertEquals("02", asClaimedInvoice.CA_TreatmentCode);
		}

		public void TestB2InvoiceAndInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			var asAccountedInvoice = dec.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice.JZ_InvoiceNumber = "INV1";
			var asAccountedInvoiceLine1 = asAccountedInvoice.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLine1.CA_OriginalLineNo = "1";
			var asAccountedInvoiceLine2 = asAccountedInvoice.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLine2.CA_OriginalLineNo = "2";

			var asClaimedInvoiceLine1 = asAccountedInvoiceLine1.CorrespondingAsClaimedForInvoiceLine;
			var asClaimedInvoiceLine2 = asAccountedInvoiceLine2.CorrespondingAsClaimedForInvoiceLine;
			var asClaimedInvoice = asClaimedInvoiceLine1.InvoiceHeader;

			AssertEquals(asAccountedInvoice, asClaimedInvoice.CorrespondingAsAccountedForInvoice);
			AssertEquals(asClaimedInvoice, asAccountedInvoice.CorrespondingAsClaimedForInvoice);
			AssertEquals(2, asAccountedInvoice.AsAccountForFilteredInvoiceLines.Count);
			AssertEquals(2, asClaimedInvoice.AsClaimForFilteredInvoiceLines.Count);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			B2JobComInvoiceLineViewCollection asAccountForFilteredInvoiceLines = null;
			AssertNoExceptionThrown(() =>
			{
				asAccountForFilteredInvoiceLines = standAloneInvoice.AsAccountForFilteredInvoiceLines;
			});
			AssertNull(asAccountForFilteredInvoiceLines);
		}

		public void TestIsB2AsAccountForSeededHeader()
		{
			AssertIsB2AsAccountForSeededHeader(JobMessageTypeList.Codes.B2Adjustments);
			AssertIsB2AsAccountForSeededHeader(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertIsB2AsAccountForSeededHeader(ZString messageType)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = messageType;

			var invoice = dec.B2AsAccountedForInvoices.AddNew();
			invoice.CA_IsSeeded = false;
			Assert(!invoice.IsB2AsAccountForSeededHeader);
			Assert(!invoice.ReadOnly);

			invoice.CA_IsSeeded = true;
			Assert(invoice.IsB2AsAccountForSeededHeader);
			Assert(invoice.ReadOnly);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var invoice1 = factory.Load<JobComInvoiceHeader>(invoice.PK);
			Assert(invoice1.ReadOnly);
		}

		public void TestIsB2AsClaimedForSeededHeader()
		{
			AssertIsB2AsClaimedForSeededHeader(JobMessageTypeList.Codes.B2Adjustments);
			AssertIsB2AsClaimedForSeededHeader(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertIsB2AsClaimedForSeededHeader(ZString messageType)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = messageType;

			var invoice = dec.B2AsClaimedForInvoices.AddNew();
			invoice.CA_IsSeeded = false;
			Assert(!invoice.IsB2AsClaimedForSeededHeader);
			Assert(!invoice.ReadOnly);

			invoice.CA_IsSeeded = true;
			Assert(invoice.IsB2AsClaimedForSeededHeader);
		}

		public const decimal VFDOverLimit = JobComInvoiceHeader.VFDLimit + 1;

		public void TestCopyB3SubHeaderToInvoice()
		{
			var subHeader = new ExpectedB3SubHeaderForTesting();
			subHeader.B3SubHeaderNumber = 1;
			subHeader.CountryOfOrigin = "UIL";
			subHeader.PlaceOfExport = "UAL";
			subHeader.TariffTreatmentCode = TariffTreatmentCodes.Codes.Chile;
			subHeader.DateOfDirectShipment = new ZDateTime(2012, 07, 02);
			subHeader.CurrencyCode = Core.Constants.CurrencyCodes.Australia;
			subHeader.B3TimeLimits = 2;
			subHeader.TimeLimitUnit = TimeLimitUnitCodes.Codes.Day;
			subHeader.TradeZone = "44";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			InvoiceHeader.CopyB3SubHeaderToInvoice(subHeader);
			AssertEquals("1", InvoiceHeader.JZ_InvoiceNumber);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, InvoiceHeader.JZ_RN_NKDefaultOrigin);
			AssertEquals("IL", InvoiceHeader.JZ_RW_NKOriginState);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, InvoiceHeader.CA_RN_NKExport);
			AssertEquals("AL", InvoiceHeader.CA_USStateOfExport);
			AssertEquals(TariffTreatmentCodes.Codes.Chile, InvoiceHeader.CA_TreatmentCode);
			AssertEquals(new ZDateTime(2012, 07, 02), InvoiceHeader.JZ_ValuationDateOverride);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, InvoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals(2, InvoiceHeader.CA_TimeLimit);
			AssertEquals(TimeLimitUnitCodes.Codes.Day, InvoiceHeader.CA_TimeLimitCode);
			AssertEquals("44", InvoiceHeader.CA_TradeZone);

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var asClaimedInvoice = dec.B2AsClaimedForInvoices.AddNew();
			asClaimedInvoice.GetDetailsFrom(InvoiceHeader);
			AssertEquals("1", asClaimedInvoice.JZ_InvoiceNumber);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, asClaimedInvoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("IL", asClaimedInvoice.JZ_RW_NKOriginState);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, asClaimedInvoice.CA_RN_NKExport);
			AssertEquals("AL", asClaimedInvoice.CA_USStateOfExport);
			AssertEquals(TariffTreatmentCodes.Codes.Chile, asClaimedInvoice.CA_TreatmentCode);
			AssertEquals(new ZDateTime(2012, 07, 02), asClaimedInvoice.JZ_ValuationDateOverride);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, asClaimedInvoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(2, asClaimedInvoice.CA_TimeLimit);
			AssertEquals(TimeLimitUnitCodes.Codes.Day, asClaimedInvoice.CA_TimeLimitCode);
			AssertEquals("44", asClaimedInvoice.CA_TradeZone);
		}

		public void TestCA_USPortOfExit_ReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("is read_only", InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
			InvoiceHeader.CA_TradeZone = "101";
			Assert("is NOT read_only", !InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
			InvoiceHeader.CA_TradeZone = ZString.Empty;
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Assert("is NOT read_only", !InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.PuertoRico;
			Assert("is NOT read_only", !InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStatesMinorIslands;
			Assert("is NOT read_only", !InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.VirginIslands;
			Assert("is NOT read_only", !InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.Australia;
			Assert("is read_only", InvoiceHeader.CA_USPortOfExitInfo.ReadOnly);
		}

		public void TestCA_IsCasualImportValues_ReadOnly()
		{
			InvoiceHeader.CA_IsCasualImport = true;
			Assert("is not read_only", !InvoiceHeader.CA_CasualImportCommodityInfo.ReadOnly);
			Assert("is not read_only", !InvoiceHeader.CA_CasualImportDestinationProvinceInfo.ReadOnly);
			InvoiceHeader.CA_IsCasualImport = false;
			Assert("is read_only", InvoiceHeader.CA_CasualImportCommodityInfo.ReadOnly);
			Assert("is read_only", InvoiceHeader.CA_CasualImportDestinationProvinceInfo.ReadOnly);
		}

		public void TestDefaultingCA_CasualImportDestinationProvince()
		{
			InvoiceHeader.CA_IsCasualImport = false;

			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			importer.MainAddress.OA_City = "TORONTO";
			importer.MainAddress.OA_State = "ON";

			var delivery = Factory.New<OrgHeader>();
			delivery.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			delivery.MainAddress.OA_City = "VANCOUVER";
			delivery.MainAddress.OA_State = "BC";

			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CAYUL";
			consignee.MainAddress.OA_City = "MONTREAL";
			consignee.MainAddress.OA_State = "QC";

			Declaration.JE_OH_Importer = importer.PK;

			InvoiceHeader.CA_IsCasualImport = true;
			AssertEquals("CA_CasualImportDestinationProvince should default to importer", "ON", InvoiceHeader.CA_CasualImportDestinationProvince);

			InvoiceHeader.CA_IsCasualImport = false;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = delivery.MainAddress.PK;

			InvoiceHeader.CA_IsCasualImport = true;
			AssertEquals("CA_CasualImportDestinationProvince should default to delivery address", "BC", InvoiceHeader.CA_CasualImportDestinationProvince);

			InvoiceHeader.CA_IsCasualImport = false;
			InvoiceHeader.FinalConsigneeAddress.E2_OA_Address = consignee.MainAddress.PK;

			InvoiceHeader.CA_IsCasualImport = true;
			AssertEquals("CA_CasualImportDestinationProvince should default to consignee", "QC", InvoiceHeader.CA_CasualImportDestinationProvince);
		}

		public void TestCA_IsCasualImportDefaultsInvoiceLineLevelDestProvince()
		{
			InvoiceHeader.CA_IsCasualImport = false;

			var headerConsignee = Factory.New<OrgHeader>();
			headerConsignee.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			headerConsignee.MainAddress.OA_City = "VANCOUVER";
			headerConsignee.MainAddress.OA_State = "BC";

			InvoiceHeader.FinalConsigneeAddress.E2_OA_Address = headerConsignee.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			consignee.MainAddress.OA_City = "TORONTO";
			consignee.MainAddress.OA_State = "ON";

			var line = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			line.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;

			Assert("CA_IsCasualImport initially false", !line.CA_IsCasualImport);
			Assert("CA_CasualImportDestinationProvince should be empty", line.CA_CasualImportDestinationProvince.IsEmpty);

			var line2 = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			line2.CA_IsCasualImport = true;
			line2.CA_CasualImportDestinationProvince = "QC";

			var line3 = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			Assert("CA_IsCasualImport initially false", !line3.CA_IsCasualImport);
			Assert("CA_CasualImportDestinationProvince should be empty", line3.CA_CasualImportDestinationProvince.IsEmpty);

			InvoiceHeader.CA_IsCasualImport = true;

			Assert("CA_IsCasualImport should return true", line.CA_IsCasualImport);
			AssertEquals("CA_CasualImportDestinationProvince should be defaulted from consignee", "ON", line.CA_CasualImportDestinationProvince);

			Assert("CA_IsCasualImport should return true", line2.CA_IsCasualImport);
			AssertEquals("CA_CasualImportDestinationProvince should not change", "QC", line2.CA_CasualImportDestinationProvince);

			Assert("CA_IsCasualImport should return true", line3.CA_IsCasualImport);
			AssertEquals("CA_CasualImportDestinationProvince should return invoice header value", "BC", line3.CA_CasualImportDestinationProvince);
		}

		public void TestISUSCountryOfExport()
		{
			ValidationTestHelper.SetTotalValueForDuty(JobComInvoiceHeader.VFDLimit, Declaration);
			InvoiceHeader.CA_TradeZone = "101";
			Assert("Trade Zone is specified", InvoiceHeader.IsUSCountryOfExport);
			InvoiceHeader.CA_TradeZone = ZString.Empty;
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.PuertoRico;
			Assert("PR", InvoiceHeader.IsUSCountryOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStatesMinorIslands;
			Assert("US Minor Islands", InvoiceHeader.IsUSCountryOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.VirginIslands;
			Assert("Virgin Islands", InvoiceHeader.IsUSCountryOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Assert("US states", InvoiceHeader.IsUSCountryOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.Australia;
			Assert("AU", !InvoiceHeader.IsUSCountryOfExport);
		}

		public void TestPlaceOfExport()
		{
			InvoiceHeader.CA_RN_NKExport = "AU";
			InvoiceHeader.CA_TradeZone = "101";
			AssertEquals("101", InvoiceHeader.PlaceOfExport);
			InvoiceHeader.CA_TradeZone = ZString.Empty;
			AssertEquals("AU", InvoiceHeader.PlaceOfExport);
		}

		public void TestIsUSPlaceOfExport()
		{
			ValidationTestHelper.SetTotalValueForDuty(VFDOverLimit, Declaration);
			InvoiceHeader.CA_TradeZone = "101";
			Assert("Trade Zone is specified", InvoiceHeader.IsUSPlaceOfExport);
			InvoiceHeader.CA_TradeZone = ZString.Empty;
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.PuertoRico;
			Assert("PR", InvoiceHeader.IsUSPlaceOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStatesMinorIslands;
			Assert("US Minor Islands", InvoiceHeader.IsUSPlaceOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.VirginIslands;
			Assert("Virgin Islands", InvoiceHeader.IsUSPlaceOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			InvoiceHeader.CA_USStateOfExport = "AL";
			Assert("US states", InvoiceHeader.IsUSPlaceOfExport);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.Australia;
			Assert("AU", !InvoiceHeader.IsUSPlaceOfExport);
		}

		public void TestVendorStateAndZip()
		{
			ValidationTestHelper.SetTotalValueForDuty(VFDOverLimit, Declaration);

			var vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("No exception occured with null data", ZString.Empty, vendorStateAndZip.State);
			AssertEquals("No exception occured with null data", ZString.Empty, vendorStateAndZip.Zip);

			var vendor = Factory.New<OrgHeader>();
			vendor.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			vendor.MainAddress.OA_City = "IL";
			vendor.MainAddress.OA_PostCode = "12345";

			InvoiceHeader.JZ_OH_Supplier = vendor.PK;
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State always set for a US vendor", "UIL", vendorStateAndZip.State);
			AssertEquals("Zip always set for a US vendor", "12345", vendorStateAndZip.Zip);

			vendor.MainAddress.OA_RL_NKRelatedPortCode = "PRXXX";
			InvoiceHeader.CA_RN_NKExport = "AU";
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State is blank for non US Place of Export", ZString.Empty, vendorStateAndZip.State);
			AssertEquals("Zip is blank for non US Place of Export", ZString.Empty, vendorStateAndZip.Zip);

			// all the folling have a US Place of Export
			vendor.MainAddress.OA_RL_NKRelatedPortCode = "PRXXX";
			InvoiceHeader.CA_TradeZone = "101";
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State set to vendor country/region for a non-US vendor", ZString.Empty, vendorStateAndZip.State);
			AssertEquals("Zip blank for a non-US vendor", ZString.Empty, vendorStateAndZip.Zip);

			var exporter = Factory.New<OrgHeader>();
			var exporterAddress = exporter.Addresses.AddNew();
			exporterAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			InvoiceHeader.ExporterDocumentaryAddress.E2_OA_Address = exporterAddress.PK;
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State set to vendor country/region for a non-US vendor and exporter", ZString.Empty, vendorStateAndZip.State);
			AssertEquals("Zip blank for a non-US vendor and exporter", ZString.Empty, vendorStateAndZip.Zip);

			exporterAddress.OA_RL_NKRelatedPortCode = "VISJN";
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State set to exporter country/region for a US territory exporter", "VI", vendorStateAndZip.State);
			AssertEquals("Zip blank for a US territory exporter", ZString.Empty, vendorStateAndZip.Zip);
			exporterAddress.OA_RL_NKRelatedPortCode = "UMJAR";
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State set to exporter country/region for a US territory exporter", "UM", vendorStateAndZip.State);
			AssertEquals("Zip blank for a US territory exporter", ZString.Empty, vendorStateAndZip.Zip);
			exporterAddress.OA_RL_NKRelatedPortCode = "PRPNU";
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State set to exporter country/region for a US territory exporter", "PR", vendorStateAndZip.State);
			AssertEquals("Zip blank for a US territory exporter", ZString.Empty, vendorStateAndZip.Zip);

			exporterAddress.OA_RL_NKRelatedPortCode = "USCHI";
			exporterAddress.OA_State = "CA";
			exporterAddress.OA_PostCode = "54321";
			vendorStateAndZip = InvoiceHeader.VendorStateAndZip;
			AssertEquals("State always set for a US exporter", "UCA", vendorStateAndZip.State);
			AssertEquals("Zip always set for a US exporter", "54321", vendorStateAndZip.Zip);
		}

		public void TestSetDefaultsFromDeclaration()
		{
			using (Declaration.SuspendValidationTesting())
			{
				invoice = Declaration.Invoices.AddNew();
				Declaration.JE_ExportDate = ZDateTime.Now;
				AssertEquals("CA_RL_NKLastPort", string.Empty, invoice.CA_RL_NKLastPort);
				AssertEquals("JZ_ValuationDateOverride", ZDateTime.Empty, invoice.JZ_ValuationDateOverride);

				Declaration.JE_RL_NKPortOfLoading = "AUBNE";
				Declaration.JE_ExportDate = ZDateTime.Now;
				invoice = Declaration.Invoices.AddNew();
				AssertEquals("CA_RL_NKLastPort", Declaration.JE_RL_NKPortOfLoading, invoice.CA_RL_NKLastPort);
				AssertEquals("JZ_ValuationDateOverride", ZDateTime.TruncateToDay(Declaration.JE_ExportDate), invoice.JZ_ValuationDateOverride);
			}
		}

		public void TestIncludedConstruction()
		{
			var charge = InvoiceHeader.GroupCharges.AddNew(CAChargeTypeList.Codes.Construction, 100m, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsIncludedInITOT = true;
			var actualAmount = (Money)InvoiceHeader["IncludedConstruction"];
			AssertEquals("IncludedConstruction", 100m, actualAmount.Amount);
			AssertEquals("IncludedConstruction", JobDeclaration.LocalCurrencyConstantCode, actualAmount.Currency.Code);
		}

		public void TestInvoiceAmountInCAD()
		{
			helper = new DeclarationTestHelper(Factory, false);
			InvoiceHeader.JZ_InvoiceAmount = 1234.50m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = helper.AUD.RX_Code;
			AssertEquals("InvoiceAmountInCAD", 605.15m, InvoiceHeader.InvoiceAmountInCAD);
		}

		public void TestJZ_IncoTerm()
		{
			var line = Declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.NoRemission, line.CA_CalculationMethod);
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Declaration.FilteredInvoiceLines.AddNew();

			AssertEquals("InvoiceLines.Count", 2, InvoiceHeader.InvoiceLines.Count);
			foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.InvoiceLines)
			{
				AssertEquals("Calculation method should be set to DDP if inco term is DDP", CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine.CA_CalculationMethod);
			}

			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.InvoiceLines)
			{
				AssertEquals("Calculation method should be reset to No Remission if inco term changed from DDP", CalculationMethods.Codes.NoRemission, invoiceLine.CA_CalculationMethod);
			}

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			line.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			AssertEquals("Repair line should be added", 3, InvoiceHeader.InvoiceLines.Count);

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("Calculation method should NOT be reset if inco term changed but NOT to DDP", 3, InvoiceHeader.InvoiceLines.Count);

			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("Repair line should be removed if inco term changed to DDP", 2, InvoiceHeader.InvoiceLines.Count);
			foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.InvoiceLines)
			{
				AssertEquals("Calculation method should be reset to DDP if inco term changed to DDP", CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine.CA_CalculationMethod);
			}
		}

		public void TestJZ_RN_NKDefaultOrigin()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RW_NKOriginState = USStatesList.Codes.NewYork;
			InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("JZ_RW_NKOriginState", USStatesList.Codes.NewYork, InvoiceHeader.JZ_RW_NKOriginState);
			InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals("JZ_RW_NKOriginState", string.Empty, InvoiceHeader.JZ_RW_NKOriginState);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RW_NKOriginState = CanadianProvinceList.Codes.YukonTerritory;
			InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals("JZ_RW_NKOriginState", CanadianProvinceList.Codes.YukonTerritory, InvoiceHeader.JZ_RW_NKOriginState);
			InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("JZ_RW_NKOriginState", string.Empty, InvoiceHeader.JZ_RW_NKOriginState);
		}

		public void TestOriginCountryAndStateChangesOnIID()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = "IID";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1701121000";
			invoiceLine.CA_CFIAInd = "Y";
			var nonCFIAInvoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = "IL";

			AssertEquals(Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKSource);
			AssertEquals("IL", invoiceLine.CA_StateOfSource);
			AssertEquals(ZString.Empty, nonCFIAInvoiceLine.CA_RN_NKSource);
			AssertEquals(ZString.Empty, nonCFIAInvoiceLine.CA_StateOfSource);
		}

		public void TestCA_RN_NKExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0452", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPortOfExit, "3801");
			Factory.Save();

			InvoiceHeader.CA_USStateOfExport = USStatesList.Codes.NewYork;
			InvoiceHeader.CA_USPortOfExit = "2813";
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("CA_USStateOfExport", USStatesList.Codes.NewYork, InvoiceHeader.CA_USStateOfExport);
			AssertEquals("CA_USPortOfExit", "2813", InvoiceHeader.CA_USPortOfExit);
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CA_USStateOfExport", string.Empty, InvoiceHeader.CA_USStateOfExport);
			AssertEquals("CA_USPortOfExit", string.Empty, InvoiceHeader.CA_USPortOfExit);

			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			InvoiceHeader.CA_USPortOfExit = "";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			Declaration.JE_CustomsOffice = "0452";
			AssertEquals("CA_USPortOfExit", "3801", InvoiceHeader.CA_USPortOfExit);

			Declaration.JE_CustomsOffice = "0705";
			AssertEquals("CA_USPortOfExit", "3801", InvoiceHeader.CA_USPortOfExit);

			InvoiceHeader.CA_USPortOfExit = ZString.Empty;
			Declaration.JE_CustomsOffice = "0495";
			AssertEquals("CA_USPortOfExit", ZString.Empty, InvoiceHeader.CA_USPortOfExit);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Declaration.JE_CustomsOffice = "0452";
			AssertEquals("CA_USPortOfExit", ZString.Empty, InvoiceHeader.CA_USPortOfExit);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP;
			Declaration.JE_CustomsOfficeInfo.RefreshBinding();
			AssertEquals("CA_USPortOfExit", ZString.Empty, InvoiceHeader.CA_USPortOfExit);

			Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedKingdom;
			Declaration.JE_CustomsOfficeInfo.RefreshBinding();
			AssertEquals("CA_USPortOfExit", ZString.Empty, InvoiceHeader.CA_USPortOfExit);
		}

		public void TestJZ_RX_NKInvoice_CurrencyForExport()
		{
			helper = new DeclarationTestHelper(Factory, false);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.CA_RX_DeclaredCurr = helper.CAD.PK;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			AssertEquals(helper.USD.RX_Code, InvoiceHeader.JZ_RX_NKInvoice_Currency);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			AssertEquals("Should not have been cleared", helper.USD.RX_Code, InvoiceHeader.JZ_RX_NKInvoice_Currency);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertEquals(helper.CAD.RX_Code, InvoiceHeader.JZ_RX_NKInvoice_Currency);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;
			AssertEquals(helper.CAD.RX_Code, InvoiceHeader.JZ_RX_NKInvoice_Currency);
		}

		public void TestBuyerReadOnlyForLVSConsolidatedByImporter()
		{
			var declaration = Declaration;
			helper = new DeclarationTestHelper(Factory, false);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			InvoiceHeader.JZ_OH_Buyer = helper.CreateOrganisation("A", "CAAAA").PK;
			AssertNotNull("Buyer", InvoiceHeader.Buyer);
			AssertEquals("JZ_OH_Buyer", false, InvoiceHeader.JZ_OH_BuyerInfo.ReadOnly);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertNull("Buyer", InvoiceHeader.Buyer);
			AssertEquals("JZ_OH_Buyer", true, InvoiceHeader.JZ_OH_BuyerInfo.ReadOnly);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.LVXInvoiceHeader.JZ_OH_Buyer = helper.CreateOrganisation("B", "CABBB").PK;
			AssertNotNull("Buyer", declaration2.LVXInvoiceHeader.Buyer);
			AssertEquals("JZ_OH_Buyer", false, declaration2.LVXInvoiceHeader.JZ_OH_BuyerInfo.ReadOnly);

			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration2.LVXInvoiceHeader, declaration);
			AssertEquals("JZ_OH_Buyer", true, declaration2.LVXInvoiceHeader.JZ_OH_BuyerInfo.ReadOnly);
		}

		public override void TestMarkApportionmentDirtyOnInvoiceCurrencyChanged()
		{
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.CA_RX_DeclaredCurr = ZGuid.Empty;
			invoice = declaration1.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			declaration1.ApportionmentDirty = false;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals("Apportionment dirty", true, declaration1.ApportionmentDirty);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestValidation()
		{
			invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EXP Validation type", typeof(ExportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("EXP AddInfoValidation type", typeof(ExportAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IMP Validation type", typeof(ImportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("IMP AddInfoValidation type", typeof(ImportAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
			invoice.JZ_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS Validation type", typeof(ImportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("LVS AddInfoValidation type", typeof(ImportAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("MSC Validation type", typeof(JobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("MSC AddInfoValidation type", typeof(AddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());

			declaration1 = Factory.New<JobDeclaration>();
			invoice = declaration1.Invoices.AddNew();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EXP Validation type", typeof(ExportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("EXP AddInfoValidation type", typeof(ExportAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IMP Validation type", typeof(ImportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("IMP AddInfoValidation type", typeof(ImportAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS Validation type", typeof(ImportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("LVS AddInfoValidation type", typeof(ImportAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("MSC Validation type", typeof(JobComInvoiceHeaderValidation), invoice.Validation.GetType());
			AssertEquals("MSC AddInfoValidation type", typeof(AddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
		}

		public void TestDefaultValueforDutyCodeFromSupplierConsigneeLink_ConsigneeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);

			OrgHeader consignee1 = OrgHeader.New(Factory);
			consignee1.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgHeader consignor = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link1 = consignee1.SupplierLinks.AddNew(consignor);
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link1.OL_ValuationBasis = "13";

			invoice.JZ_OH_Supplier = consignor.PK;
			invoice.JZ_OH_Consignee = consignee1.PK;
			AssertEquals("Precondition:", link1, invoice.SupplierConsigneeLink);
			AssertEquals("ValueForDutyCode should default from SupplierConsigneeLink", "13", invoice.CA_ValueForDutyCode);

			OrgHeader consignee2 = OrgHeader.New(Factory);
			consignee2.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgSupplierBuyerLink link2 = consignee2.SupplierLinks.AddNew(consignor);
			link2.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link2.OL_ValuationBasis = "23";

			invoice.CA_ValueForDutyCode = string.Empty;
			invoice.JZ_OH_Consignee = consignee2.PK;
			AssertEquals("Precondition:", link2, invoice.SupplierConsigneeLink);
			AssertEquals("ValueForDutyCode should default from SupplierConsigneeLink", "23", invoice.CA_ValueForDutyCode);

			invoice.JZ_OH_Consignee = consignee1.PK;
			AssertEquals("ValueForDutyCode should not default from SupplierConsigneeLink", "23", invoice.CA_ValueForDutyCode);
		}

		public void TestDefaultValueforDutyCodeFromSupplierConsigneeLink_SupplierChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);

			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgHeader consignor1 = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link1 = consignee.SupplierLinks.AddNew(consignor1);
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link1.OL_ValuationBasis = "13";

			invoice.JZ_OH_Supplier = consignor1.PK;
			invoice.JZ_OH_Consignee = consignee.PK;
			AssertEquals("Precondition:", link1, invoice.SupplierConsigneeLink);
			AssertEquals("ValueForDutyCode should default from SupplierConsigneeLink", "13", invoice.CA_ValueForDutyCode);

			OrgHeader consignor2 = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link2 = consignee.SupplierLinks.AddNew(consignor2);
			link2.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			link2.OL_ValuationBasis = "23";

			invoice.CA_ValueForDutyCode = string.Empty;
			invoice.JZ_OH_Supplier = consignor2.PK;
			AssertEquals("Precondition:", link2, invoice.SupplierConsigneeLink);
			AssertEquals("ValueForDutyCode should default from SupplierConsigneeLink", "23", invoice.CA_ValueForDutyCode);

			invoice.JZ_OH_Supplier = consignor1.PK;
			AssertEquals("ValueForDutyCode should not default from SupplierConsigneeLink", "23", invoice.CA_ValueForDutyCode);
		}

		public void TestDoNotDefaultExportCountryAndStateFromOrigin()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(ZString.Empty, invoice.CA_RN_NKExport);
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(ZString.Empty, invoice.CA_RN_NKExport);
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = "IL";
			AssertEquals(ZString.Empty, invoice.CA_USStateOfExport);
			invoice.JZ_RW_NKOriginState = "AL";
			AssertEquals(ZString.Empty, invoice.CA_USStateOfExport);
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			AssertEquals("", invoice.CA_USStateOfExport);
			invoice.JZ_RW_NKOriginState = "IL";
			AssertEquals("", invoice.CA_USStateOfExport);
		}

		#region TestCopyValuesToDeclarationForLVX

		public void TestCopyValuesToDeclarationForLVX()
		{
			var universalhelper = new UniversalReferenceTestDataHelper(Factory);
			universalhelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = universalhelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0821", "0821", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalhelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "BC");

			var officeCode2 = universalhelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0009", "0009", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalhelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "NS");
			Factory.Save();

			helper = new DeclarationTestHelper(Factory, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = helper.CreateOrganisation("A", "CAAAA").PK;
			AssertEquals("Copy JZ_OH_Buyer", invoice.JZ_OH_Buyer, declaration.JE_OH_Importer);
			invoice.JZ_OH_Supplier = helper.CreateOrganisation("B", "CNBBB").PK;
			AssertEquals("Copy JZ_OH_Supplier", invoice.JZ_OH_Supplier, declaration.JE_OH_Supplier);
			invoice.CA_PortOfClearance = "0821";
			AssertEquals("Copy JE_CustomsOffice", "0821", declaration.JE_CustomsOffice);
			AssertEquals("Copy CA_ProvinceOfClearance", "BC", declaration.CA_ProvinceOfClearance);
			invoice.CA_PortOfClearance = "0009";
			AssertEquals("Copy JE_CustomsOffice", "0009", declaration.JE_CustomsOffice);
			AssertEquals("Copy CA_ProvinceOfClearance", "NS", declaration.CA_ProvinceOfClearance);
			invoice.CA_PortOfClearance = "";
			AssertEquals("Copy JE_CustomsOffice", "", declaration.JE_CustomsOffice);
			AssertEquals("Copy CA_ProvinceOfClearance", "", declaration.CA_ProvinceOfClearance);
			invoice.JZ_IncoTerm = "CIF";
			AssertEquals("Copy JZ_IncoTerm", "CIF", declaration.JE_ShipmentIncoTerm);
		}

		#endregion

		public void TestCanDelete()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = declaration1.Invoices.AddNew();
			Assert("Can be deleted", invoice1.CanDelete);
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader1.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			Assert("Shipment may not be deleted because this B3 has already been reported, or is waiting for a response.", !invoice1.CanDelete);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.Invoices.AddNew();
			Assert("Can be deleted", invoice2.CanDelete);
			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration2.LVXInvoiceHeader, declaration3);
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader3.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Assert("Shipment may not be deleted because this B3 has already been reported, or is waiting for a response.", !invoice2.CanDelete);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var invoice4 = declaration4.Invoices.AddNew();
			var entryLine = entryHeader4.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 1;
			var invoiceLine = invoice4.JobComInvoiceLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();
			Assert("Invoice not allowed to be deleted because there is CAD response for it.", !invoice4.CanDelete);
		}

		public void TestAttachToAndDetachFromConsolidatedLVSDeclaration()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Assert("Precondition: CA_RequiresMerge is false", !declaration1.CA_RequiresMerge);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var declaration2Entry = declaration2.CustomsEntryHeaders.AddNew();
			var declaration2EntryLine1 = declaration2Entry.MergedLines.AddNew();
			var declaration2EntryLine2 = declaration2Entry.MergedLines.AddNew();
			var invoice2 = declaration2.LVXInvoiceHeader;
			invoice2.JobComInvoiceLines.AddNew().JI_CL = declaration2EntryLine1.PK;
			invoice2.JobComInvoiceLines.AddNew().JI_CL = declaration2EntryLine2.PK;
			invoice2.JZ_InvoiceNumber = "INV111";
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration1);
			Assert("invoice2 attached to declaration1", declaration1.Invoices.Contains(invoice2));
			var groupHeader = (JobComInvoiceGroupHeader)declaration1.JobComInvoiceGroupHeaders.FindByPK(invoice2.GroupHeader.PK);
			AssertNotNull("invoice2.GroupHeader attached to declaration1", groupHeader);
			AssertEquals("All Invoices", groupHeader.JZ_InvoiceNumber);
			Assert("CA_RequiresMerge should be set to false", !declaration1.CA_RequiresMerge);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var declaration3Entry = declaration3.CustomsEntryHeaders.AddNew();
			var declaration3EntryLine1 = declaration3Entry.MergedLines.AddNew();
			var declaration3EntryLine2 = declaration3Entry.MergedLines.AddNew();
			var invoice3 = declaration3.LVXInvoiceHeader;
			invoice3.JobComInvoiceLines.AddNew().JI_CL = declaration3EntryLine1.PK;
			invoice3.JobComInvoiceLines.AddNew().JI_CL = declaration3EntryLine2.PK;
			invoice3.JZ_InvoiceNumber = "INV222";
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice3, declaration1);
			Assert("invoice3 attached to declaration1", declaration1.Invoices.Contains(invoice3));
			var groupHeader2 = (JobComInvoiceGroupHeader)declaration1.JobComInvoiceGroupHeaders.FindByPK(invoice3.GroupHeader.PK);
			AssertNotNull("invoice3.GroupHeader attached to declaration1", groupHeader2);
			AssertEquals("All Invoices", groupHeader2.JZ_InvoiceNumber);
			Assert("CA_RequiresMerge should be set to false", !declaration1.CA_RequiresMerge);

			var entry = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			AdditionalInvoiceLineEntryLineLink link11 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link11.BU_JI = invoice2.JobComInvoiceLines[0].PK;
			link11.BU_CL = entryLine1.PK;
			AdditionalInvoiceLineEntryLineLink link12 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link12.BU_JI = invoice2.JobComInvoiceLines[0].PK;
			link12.BU_CL = entryLine2.PK;
			AdditionalInvoiceLineEntryLineLink link2 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link2.BU_JI = invoice2.JobComInvoiceLines[1].PK;
			link2.BU_CL = entryLine1.PK;
			declaration1.CA_RequiresMerge = true;
			Factory.Save();

			LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice2, declaration1);
			Assert("invoice2 detached from declaration1", !declaration1.Invoices.Contains(invoice2));
			Assert("invoice2.GroupHeader detached from declaration1", !declaration1.JobComInvoiceGroupHeaders.Contains(invoice2.GroupHeader));
			AssertEquals("All Invoices", groupHeader.JZ_InvoiceNumber);
			Assert("CA_RequiresMerge should be set to false", !declaration1.CA_RequiresMerge);
			Assert("Multiple CusUnderbondDec, the first one should be deleted", link11.IsDeleted && link12.IsDeleted);
			Assert("The CusUnderbondDec should be deleted", link2.IsDeleted);

			LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice3, declaration1);
			Assert("invoice3 detached from declaration1", !declaration1.Invoices.Contains(invoice3));
			Assert("invoice3.GroupHeader detached from declaration1", !declaration1.JobComInvoiceGroupHeaders.Contains(invoice3.GroupHeader));
			Assert("CA_RequiresMerge should be set to false", !declaration1.CA_RequiresMerge);
		}

		public void TestDeleteMultiEntryLinesFromSameInvoiceLineWithoutException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var entry = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var link1 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link1.BU_JI = invoiceLine.PK;
			link1.BU_CL = entryLine1.PK;
			var link2 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link2.BU_JI = invoiceLine.PK;
			link2.BU_CL = entryLine2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var decInDiffFactory = newFactory.Load<JobDeclaration>(declaration2.PK);
			var invoiceInDiffFactory = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var hits = new Dictionary<string, int> { { JobComInvoiceHeader.Schema.TableName, 8 } };
			using (AssertDbHitsWithUsefulQueryInformation(hits, newFactory, ignoreHitsFromTablesCachedInUberFactory: true, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				AssertNoExceptionThrown(() =>
				{
					LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoiceInDiffFactory, decInDiffFactory);
				});
			}
			newFactory.Save();
			Assert(link1.IsDeleted);
			Assert(link2.IsDeleted);
		}

		public void TestFirstAdditionalOrOnlyDeclaration()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = declaration1.Invoices.AddNew();
			AssertNotNull(invoice1.FirstAdditionalOrOnlyDeclaration);
			AssertEquals(declaration1.PK, invoice1.FirstAdditionalOrOnlyDeclaration.PK);
			Assert(invoice1.IsAttachedToPersistentConsolidatedLVS);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.LVXInvoiceHeader;
			AssertNotNull(invoice2.FirstAdditionalOrOnlyDeclaration);
			AssertEquals(declaration2.PK, invoice2.FirstAdditionalOrOnlyDeclaration.PK);
			Assert(!invoice2.IsAttachedToPersistentConsolidatedLVS);
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration1);
			AssertNotNull(invoice2.FirstAdditionalOrOnlyDeclaration);
			AssertEquals(declaration1.PK, invoice2.FirstAdditionalOrOnlyDeclaration.PK);
			Assert(invoice2.IsAttachedToPersistentConsolidatedLVS);
		}

		public void TestCA_LVSCarrier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = declaration.Invoices.AddNew();
			invoice.CA_LVSCarrier = "3001";
			AssertEquals("3001", invoice.CA_LVSCarrier);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", invoice.CA_LVSCarrier);
		}

		public void TestNoExceptionSupportsRelatedBillWithoutJobDeclaration()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("No Exception", true, invoice.SupportsRelatedBill);
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("No Exception", false, invoice.SupportsRelatedBill);
		}

		public void TestSupportsRelatedBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(false, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, invoice.SupportsRelatedBill);
		}

		public void TestUpdateAVSEventRequired()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			new FakeDeclarationCreatorForInvoice(invoice);
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("Precondition: AVSEventRequired", !declaration.AVSEventRequired);
			declaration.Invoices.Add(invoice);
			Assert("AVSEventRequired should be set", declaration.AVSEventRequired);

			declaration.AVSEventRequired = false;
			declaration.Invoices.RemoveFromRelationship(invoice);
			Assert("AVSEventRequired should be set", declaration.AVSEventRequired);
		}

		public void TestSupplierDocumentaryAddress()
		{
			var otherFactory = new BusinessObjectFactory();
			var consignor1 = otherFactory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress1 = consignor1.Addresses.MainAddress;
			var consignor2 = otherFactory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress2 = consignor2.Addresses.MainAddress;
			var declaration = otherFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice[JobComInvoiceHeaderSchema.JZ_OH_Supplier.Name] = consignor1.PK;
			otherFactory.Save();
			invoice = Factory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("SupplierDocumentaryAddress.OrganisationPK", consignor1.PK, invoice.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("SupplierDocumentaryAddress.E2_OA_Address", consignorMainAddress1.PK, invoice.SupplierDocumentaryAddress.E2_OA_Address);

			invoice.JZ_OH_Supplier = consignor2.PK;
			AssertEquals("SupplierDocumentaryAddress.OrganisationPK", consignor2.PK, invoice.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("SupplierDocumentaryAddress.E2_OA_Address", consignorMainAddress2.PK, invoice.SupplierDocumentaryAddress.E2_OA_Address);

			invoice.SupplierDocumentaryAddress.OrganisationPK = consignor1.PK;
			AssertEquals("JZ_OH_Supplier", consignor1.PK, invoice.JZ_OH_Supplier);

			AssertEquals("DefaultAddressType", ZArchitecture.Business.AddressType.NoDefault, invoice.SupplierDocumentaryAddress.DefaultAddressType);

			var standAloneInvoice = otherFactory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_InvoiceDisplaySequence = 1;
			standAloneInvoice.JZ_OH_Supplier = consignor1.PK;
			AssertEquals("SupplierDocumentaryAddress.OrganisationPK", consignor1.PK, standAloneInvoice.SupplierDocumentaryAddress.OrganisationPK);

			standAloneInvoice.JZ_OH_Supplier = consignor2.PK;
			AssertEquals("SupplierDocumentaryAddress.OrganisationPK", consignor2.PK, standAloneInvoice.SupplierDocumentaryAddress.OrganisationPK);
		}

		public void TestSupplierPickupDeliveryAddress()
		{
			var otherFactory = new BusinessObjectFactory();
			var shipper1 = otherFactory.NewWithValidTestData<OrgHeader>();
			var shipper1MainAddress = shipper1.Addresses.MainAddress;
			var shipper2 = otherFactory.NewWithValidTestData<OrgHeader>();
			var shipper2MainAddress = shipper2.Addresses.MainAddress;
			var declaration = otherFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipper1.PK;
			otherFactory.Save();
			invoice = Factory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("SupplierPickupDeliveryAddress.OrganisationPK", shipper1.PK, invoice.SupplierPickupDeliveryAddress.OrganisationPK);
			AssertEquals("SupplierPickupDeliveryAddress.E2_OA_Address", shipper1MainAddress.PK, invoice.SupplierPickupDeliveryAddress.E2_OA_Address);

			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipper2.PK;
			AssertEquals("SupplierPickupDeliveryAddress.OrganisationPK", shipper2.PK, invoice.SupplierPickupDeliveryAddress.OrganisationPK);
			AssertEquals("SupplierPickupDeliveryAddress.E2_OA_Address", shipper2MainAddress.PK, invoice.SupplierPickupDeliveryAddress.E2_OA_Address);

			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipper1.PK;
			AssertEquals("SupplierPickupDeliveryAddress.OrganisationPK", shipper1.PK, invoice.SupplierPickupDeliveryAddress.OrganisationPK);
			AssertEquals("SupplierPickupDeliveryAddress.E2_OA_Address", shipper1MainAddress.PK, invoice.SupplierPickupDeliveryAddress.E2_OA_Address);

			AssertEquals("DefaultAddressType", ZArchitecture.Business.AddressType.NoDefault, invoice.SupplierPickupDeliveryAddress.DefaultAddressType);
		}

		public void TestDefaultCountryOfOriginAndExport()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress = consignor.Addresses.MainAddress;
			consignorMainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var consignorAddress = consignor.Addresses.AddNew();
			consignorAddress.OA_Address1 = "ADDRESS";
			consignorAddress.OA_RL_NKRelatedPortCode = "USCHI";
			consignorAddress.OA_State = "IL";
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.Addresses.MainAddress.OA_RL_NKRelatedPortCode = "CABLO";
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.Addresses.MainAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Country/Region of Origin should be defaulted", "AU", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("Country/Region of Export should be defaulted", "AU", invoice.CA_RN_NKExport);

			invoice.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
			AssertEquals("Country/Region of Origin should be defaulted", "US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should be defaulted", "IL", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should be defaulted", "US", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", "IL", invoice.CA_USStateOfExport);

			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipper.PK;
			AssertEquals("Country/Region of Origin should be defaulted", "CA", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should be defaulted", "", invoice.JZ_RW_NKOriginState);

			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter.PK;
			AssertEquals("Country/Region of Export should be defaulted", "CN", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", "", invoice.CA_USStateOfExport);

			invoice.SupplierPickupDeliveryAddress.OrganisationPK = ZGuid.Empty;
			invoice.ExporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("Country/Region of Origin should be defaulted", "US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should be defaulted", "IL", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should be defaulted", "US", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", "IL", invoice.CA_USStateOfExport);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("Country/Region of Origin should not be defaulted", "US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should not be defaulted", "IL", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should not be defaulted", "US", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should not be defaulted", "IL", invoice.CA_USStateOfExport);

			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			invoice.SupplierDocumentaryAddress.E2_State = "AL";

			AssertEquals("Country/Region of Origin should not be defaulted", "US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should not be defaulted", "AL", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should not be defaulted", "US", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should not be defaulted", "AL", invoice.CA_USStateOfExport);

			invoice.SupplierPickupDeliveryAddress.E2_AddressOverride = true;
			invoice.SupplierPickupDeliveryAddress.E2_RN_NKCountryCode = "AU";

			AssertEquals("Country/Region of Origin should not be defaulted", "AU", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should not be defaulted", "", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should not be defaulted", "US", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should not be defaulted", "AL", invoice.CA_USStateOfExport);

			Factory.Save();

			invoice.SupplierPickupDeliveryAddress.E2_RN_NKCountryCode = "NZ";
			invoice.SupplierPickupDeliveryAddress.E2_State = ZString.Empty;
			invoice.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "JP";
			invoice.SupplierDocumentaryAddress.E2_State = ZString.Empty;
			AssertEquals("Country/Region of Origin should not change", "AU", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should not change", "", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should not change", "US", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should not change", "AL", invoice.CA_USStateOfExport);

			invoice.JZ_RN_NKDefaultOrigin = ZString.Empty;
			invoice.CA_RN_NKExport = ZString.Empty;
			invoice.SupplierPickupDeliveryAddress.E2_RN_NKCountryCode = "GB";
			invoice.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "MX";
			AssertEquals("Country/Region of Origin should change", "GB", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should change", "", invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should change", "MX", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should change", "", invoice.CA_USStateOfExport);
		}

		public void TestDefaultCountryOfOriginAndExportWhenOverride()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress = consignor.Addresses.MainAddress;
			consignorMainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var consignorAddress = consignor.Addresses.AddNew();
			consignorAddress.OA_Address1 = "ADDRESS";
			consignorAddress.OA_RL_NKRelatedPortCode = "USCHI";
			consignorAddress.OA_State = "IL";
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.Addresses.MainAddress.OA_RL_NKRelatedPortCode = "CABLO";
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.Addresses.MainAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Country/Region of Origin should be defaulted", "AU", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("Country/Region of Export should be defaulted", "AU", invoice.CA_RN_NKExport);

			invoice.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "CA";
			invoice.SupplierDocumentaryAddress.E2_State = "XO";
			AssertEquals("Country/Regionof Origin should be defaulted", "CA", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should be defaulted", ZString.Empty, invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should be defaulted", "CA", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", ZString.Empty, invoice.CA_USStateOfExport);

			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter.PK;
			AssertEquals("Country/Region of Export should be defaulted", "CN", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", "", invoice.CA_USStateOfExport);

			invoice.ExporterDocumentaryAddress.E2_AddressOverride = true;
			invoice.ExporterDocumentaryAddress.E2_RN_NKCountryCode = "CA";
			invoice.ExporterDocumentaryAddress.E2_State = "XO";
			AssertEquals("Country/Region of Export should be defaulted", "CA", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", ZString.Empty, invoice.CA_USStateOfExport);

			invoice.SupplierPickupDeliveryAddress.OrganisationPK = ZGuid.Empty;
			invoice.ExporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("Country/Region of Origin should be defaulted", "CA", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("State of Origin should be defaulted", ZString.Empty, invoice.JZ_RW_NKOriginState);
			AssertEquals("Country/Region of Export should be defaulted", "CA", invoice.CA_RN_NKExport);
			AssertEquals("State of Export should be defaulted", ZString.Empty, invoice.CA_USStateOfExport);
		}

		public void TestDefaultValueFromBuyerSupplierLink()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer2";
			importer.OH_FullName = "Importer2";
			var vendor = Factory.New<OrgHeader>();
			vendor.OH_Code = "Vendor";
			vendor.OH_FullName = "Vendor";
			var link = importer.SupplierLinks.AddNew(vendor);
			link.OL_RN_NKImporterCountry = "CA";
			link.OL_RX_NKDefaultCurrency = "CNY";
			var linkMode = link.OrgSupBuyLinkTrnModes.AddNew();
			linkMode.PF_TransportMode = "ALL";
			linkMode.PF_IncoTerm = "CIF";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			invoice.JZ_IncoTerm = ZString.Empty;
			Assert("PreventBuyerSupplierRelationships", ((IBuyerSupplierRelationshipConsumer)invoice).PreventBuyerSupplierRelationships);
			invoice.JZ_OH_Buyer = importer.PK;
			AssertEquals("JZ_OH_Supplier should not be set", ZGuid.Empty, invoice.JZ_OH_Supplier);
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.JZ_OH_Supplier = vendor.PK;
			AssertEquals("JZ_OH_Buyer should not be set", ZGuid.Empty, invoice.JZ_OH_Buyer);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("PreventBuyerSupplierRelationships", !((IBuyerSupplierRelationshipConsumer)invoice).PreventBuyerSupplierRelationships);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Assert("PreventBuyerSupplierRelationships", !((IBuyerSupplierRelationshipConsumer)invoice).PreventBuyerSupplierRelationships);

			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.JZ_OH_Buyer = importer.PK;
			AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoice.JZ_OH_Supplier);
			AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("JZ_IncoTerm should be set", "CIF", invoice.JZ_IncoTerm);
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			invoice.JZ_IncoTerm = ZString.Empty;
			invoice.JZ_OH_Supplier = vendor.PK;
			AssertEquals("JZ_OH_Buyer should be set", importer.PK, invoice.JZ_OH_Buyer);
			AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("JZ_IncoTerm should be set", "CIF", invoice.JZ_IncoTerm);
		}

		public void TestFinalConsigneeAddress()
		{
			var otherFactory = new BusinessObjectFactory();
			var consignee1 = otherFactory.NewWithValidTestData<OrgHeader>();
			var consigneeMainAddress1 = consignee1.Addresses.MainAddress;
			var consignee2 = otherFactory.NewWithValidTestData<OrgHeader>();
			var consigneeMainAddress2 = consignee2.Addresses.MainAddress;
			var declaration = otherFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Consignee = consignee1.PK;
			otherFactory.Save();
			invoice = Factory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("FinalConsigneeAddress.OrganisationPK", consignee1.PK, invoice.FinalConsigneeAddress.OrganisationPK);
			AssertEquals("FinalConsigneeAddress.E2_OA_Address", consigneeMainAddress1.PK, invoice.FinalConsigneeAddress.E2_OA_Address);

			invoice.JZ_OH_Consignee = consignee2.PK;
			AssertEquals("FinalConsigneeAddress.OrganisationPK", consignee2.PK, invoice.FinalConsigneeAddress.OrganisationPK);
			AssertEquals("FinalConsigneeAddress.E2_OA_Address", consigneeMainAddress2.PK, invoice.FinalConsigneeAddress.E2_OA_Address);

			invoice.FinalConsigneeAddress.OrganisationPK = consignee1.PK;
			AssertEquals("JZ_OH_Consignee", consignee1.PK, invoice.JZ_OH_Consignee);
		}

		public void TestDoNotSetFinalConsigneeAddressWhenOrgPKIsNotEmpty()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "JOEYYIN";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			Factory.Save();

			AssertEquals(true, invoice1.FinalConsigneeAddress.OrganisationPK.IsEmpty);

			invoice1.FinalConsigneeAddress.Delete();
			AssertEquals(true, invoice1.FinalConsigneeAddress.OrganisationPK.IsEmpty);

			invoice1.FinalConsigneeAddress.Delete();
			Factory.Save();

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = consignee.MainAddress.PK;
			docAddress.E2_ParentID = invoice1.PK;
			docAddress.E2_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			docAddress.E2_AddressType = "FCA";
			docAddress.E2_AddressSequence = 0;
			docAddress.E2_AddressOverride = false;
			invoice1.DocAddresses.Reload(true);
			AssertEquals(consignee.PK, invoice1.FinalConsigneeAddress.OrganisationPK);
		}

		public void TestBuyerDocumentaryAddress()
		{
			var otherFactory = new BusinessObjectFactory();
			var buyer1 = otherFactory.NewWithValidTestData<OrgHeader>();
			var buyerMainAddress1 = buyer1.Addresses.MainAddress;
			var buyer2 = otherFactory.NewWithValidTestData<OrgHeader>();
			var buyerMainAddress2 = buyer2.Addresses.MainAddress;
			var declaration = otherFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = buyer1.PK;
			otherFactory.Save();
			invoice = Factory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("BuyerDocumentaryAddress.OrganisationPK", buyer1.PK, invoice.BuyerDocumentaryAddress.OrganisationPK);
			AssertEquals("BuyerDocumentaryAddress.E2_OA_Address", buyerMainAddress1.PK, invoice.BuyerDocumentaryAddress.E2_OA_Address);

			invoice.JZ_OH_Buyer = buyer2.PK;
			AssertEquals("BuyerDocumentaryAddress.OrganisationPK", buyer2.PK, invoice.BuyerDocumentaryAddress.OrganisationPK);
			AssertEquals("BuyerDocumentaryAddress.E2_OA_Address", buyerMainAddress2.PK, invoice.BuyerDocumentaryAddress.E2_OA_Address);

			invoice.BuyerDocumentaryAddress.OrganisationPK = buyer1.PK;
			AssertEquals("JZ_OH_Buyer", buyer1.PK, invoice.JZ_OH_Buyer);

			var standAloneInvoice = otherFactory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_InvoiceDisplaySequence = 1;
			standAloneInvoice.JZ_OH_Buyer = buyer1.PK;
			AssertEquals("BuyerDocumentaryAddress.OrganisationPK", buyer1.PK, standAloneInvoice.BuyerDocumentaryAddress.OrganisationPK);

			standAloneInvoice.JZ_OH_Buyer = buyer2.PK;
			AssertEquals("BuyerDocumentaryAddress.OrganisationPK", buyer2.PK, standAloneInvoice.BuyerDocumentaryAddress.OrganisationPK);
		}

		public void TestExportBrokerDocumentaryAddress()
		{
			var otherFactory = new BusinessObjectFactory();
			var exporter1 = otherFactory.NewWithValidTestData<OrgHeader>();
			var exporterMainAddress1 = exporter1.Addresses.MainAddress;
			var exporter2 = otherFactory.NewWithValidTestData<OrgHeader>();
			var exporterMainAddress2 = exporter2.Addresses.MainAddress;
			var declaration = otherFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter1.PK;
			otherFactory.Save();
			invoice = Factory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("ExportBrokerDocumentaryAddress.OrganisationPK", exporter1.PK, invoice.ExporterDocumentaryAddress.OrganisationPK);
			AssertEquals("ExportBrokerDocumentaryAddress.E2_OA_Address", exporterMainAddress1.PK, invoice.ExporterDocumentaryAddress.E2_OA_Address);

			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter2.PK;
			AssertEquals("ExportBrokerDocumentaryAddress.OrganisationPK", exporter2.PK, invoice.ExporterDocumentaryAddress.OrganisationPK);
			AssertEquals("ExportBrokerDocumentaryAddress.E2_OA_Address", exporterMainAddress2.PK, invoice.ExporterDocumentaryAddress.E2_OA_Address);

			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter1.PK;
			AssertEquals("ExporterDocumentaryAddress", exporter1.PK, invoice.ExporterDocumentaryAddress.OrganisationPK);
		}

		public void TestCA_USStateOfExport_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			Assert("Readonly", invoice.CA_USStateOfExportInfo.ReadOnly);
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Assert("Not Readonly", !invoice.CA_USStateOfExportInfo.ReadOnly);
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Mexico;
			Assert("Readonly", invoice.CA_USStateOfExportInfo.ReadOnly);
		}

		public void TestCA_StateOfSource_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			Assert("Readonly", invoice.CA_StateOfSourceInfo.ReadOnly);
			invoice.CA_RN_NKSource = "US";
			Assert("Not Readonly", !invoice.CA_StateOfSourceInfo.ReadOnly);
		}

		public void TestUseRecentOneYearExchangeRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 1, 3);

			var lessThanOneYearcurrency = Factory.New<RefCurrency>();
			lessThanOneYearcurrency.RX_Code = "AAA";

			var lessThanOneYearExchangeRate = Factory.New<RefExchangeRate>();
			lessThanOneYearExchangeRate.RE_ExRateType = "CUS";
			lessThanOneYearExchangeRate.RE_GC = Env.CurrentCompany.PK;
			lessThanOneYearExchangeRate.RE_StartDate = new ZDateTime(2018, 1, 4);
			lessThanOneYearExchangeRate.RE_ExpiryDate = new ZDateTime(2019, 1, 5);
			lessThanOneYearExchangeRate.RE_RX_NKExCurrency = lessThanOneYearcurrency.RX_Code;
			lessThanOneYearExchangeRate.RE_SellRate = 0.6m;

			var lessThanOneYearOutOfRangecurrency = Factory.New<RefCurrency>();
			lessThanOneYearOutOfRangecurrency.RX_Code = "BBB";

			var lessThanOneYearOutOfRangeExchangeRate = Factory.New<RefExchangeRate>();
			lessThanOneYearOutOfRangeExchangeRate.RE_ExRateType = "CUS";
			lessThanOneYearOutOfRangeExchangeRate.RE_GC = Env.CurrentCompany.PK;
			lessThanOneYearOutOfRangeExchangeRate.RE_StartDate = new ZDateTime(2018, 1, 4);
			lessThanOneYearOutOfRangeExchangeRate.RE_ExpiryDate = new ZDateTime(2018, 1, 5);
			lessThanOneYearOutOfRangeExchangeRate.RE_RX_NKExCurrency = lessThanOneYearOutOfRangecurrency.RX_Code;
			lessThanOneYearOutOfRangeExchangeRate.RE_SellRate = 0.8m;

			var moreThanOneYearcurrency = Factory.New<RefCurrency>();
			moreThanOneYearcurrency.RX_Code = "CCC";

			var moreThanOneYearExchangeRate = Factory.New<RefExchangeRate>();
			moreThanOneYearExchangeRate.RE_ExRateType = "CUS";
			moreThanOneYearExchangeRate.RE_GC = Env.CurrentCompany.PK;
			moreThanOneYearExchangeRate.RE_StartDate = new ZDateTime(2018, 1, 1);
			moreThanOneYearExchangeRate.RE_ExpiryDate = new ZDateTime(2018, 1, 2);
			moreThanOneYearExchangeRate.RE_RX_NKExCurrency = moreThanOneYearcurrency.RX_Code;
			moreThanOneYearExchangeRate.RE_SellRate = 1.2m;

			invoice.JZ_RX_NKInvoice_Currency = lessThanOneYearOutOfRangecurrency.RX_Code;
			CombineAssertions(() =>
			{
				AssertEquals(lessThanOneYearOutOfRangeExchangeRate.RE_SellRate, invoice.JZ_InvoiceCurrExRate);
				AssertEquals(lessThanOneYearOutOfRangeExchangeRate.RE_SellRate, invoice.JZ_InvoiceCurrLandedCostExRate);
				AssertHasWarning(invoice.JZ_RX_NKInvoice_CurrencyInfo, string.Format("There is no exchange rate on file for 3/01/2019. The closest match of the exchange rate for BBB is the rate for the date 5/01/2018. This exchange rate will be used.{0}", ExternalMessageValidation.AdviceHowToFixNoValidExchangeRates));

				invoice.JZ_RX_NKInvoice_Currency = lessThanOneYearcurrency.RX_Code;
				AssertEquals(lessThanOneYearExchangeRate.RE_SellRate, invoice.JZ_InvoiceCurrExRate);
				AssertEquals(lessThanOneYearExchangeRate.RE_SellRate, invoice.JZ_InvoiceCurrLandedCostExRate);
				AssertNoWarning(invoice.JZ_RX_NKInvoice_CurrencyInfo, string.Format("There is no exchange rate on file for 3/01/2019. The closest match of the exchange rate for BBB is the rate for the date 5/01/2018. This exchange rate will be used.{0}", ExternalMessageValidation.AdviceHowToFixNoValidExchangeRates));
			});

			invoice.JZ_RX_NKInvoice_Currency = moreThanOneYearcurrency.RX_Code;
			CombineAssertions(() =>
			{
				AssertEquals(0m, invoice.JZ_InvoiceCurrExRate);
				AssertEquals(0m, invoice.JZ_InvoiceCurrLandedCostExRate);
				AssertHasMessageError(invoice.JZ_RX_NKInvoice_CurrencyInfo, string.Format("There is no valid currency exchange rate for this currency on file for the past year.{0}", ExternalMessageValidation.AdviceHowToFixNoValidExchangeRates));
			});
		}

		public void TestICurrencyConverterDataProvider()
		{
			var inv = Factory.New<JobComInvoiceHeader>();
			var invAsProvider = inv as ICurrencyConverterDataProvider;
			AssertEquals("Fall back days", 365, invAsProvider.MaximumDaysToFallback);

			var dec = Factory.New<JobDeclaration>();
			var inv1 = dec.Invoices.AddNew();
			var inv1AsProvider = inv1 as ICurrencyConverterDataProvider;
			AssertEquals("Fall back days", 365, inv1AsProvider.MaximumDaysToFallback);
		}

		public void TestCA_DDPDeductDutyOnly()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ExciseTax, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CustomsDuty, rateType2.PK);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode3 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E90", rateType3.PK);
			var rateCode4 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E91", rateType3.PK);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB1", rateType3.PK);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456123", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate1 = universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(tariff1, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(tariff1, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);

			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			universalHelper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TNE");
			var countervailingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123");
			var countervailingRate = universalHelper.CreateRate(countervailingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			Factory.Save();
			JobComInvoiceLineTestHelper.CreateCAGSTRateCode(Factory, 5m);

			ZDateTime currentDate = ZDateTime.Today;
			ZDateTime effectiveDate = currentDate.AddDays(-1);
			ZDateTime expiryDate = currentDate.AddDays(+1);
			string customsUnit = CustomsUnitOfMeasureList.Codes.Litre;

			#region Populate Ref Numbers
			CACClassHeader classHeader1Test = Factory.New<CACClassHeader>();
			classHeader1Test.ZA_ClassificationNumber = "0789456123";
			classHeader1Test.ZA_EffectiveDate = effectiveDate;
			classHeader1Test.ZA_ExpiryDate = expiryDate;
			classHeader1Test.ZA_AreaCode = "900";

			var refNumHeader = classHeader1Test.RefNumbers.AddNew();
			refNumHeader.ZD_EffectiveDate = effectiveDate;
			refNumHeader.ZD_ExpiryDate = expiryDate;
			var refNum = refNumHeader.RefNumbers.AddNew();
			refNum.ZE_GSTRefNumber = "AA3";
			refNum.ZE_ExciseTaxRefNumber = "BB3";

			var exciseRates = new CACTaxRateCollection(Factory, CACTaxRate.TaxType.Excise);
			var taxRate = exciseRates.AddNew();
			taxRate.ZH_TaxRefNumber = refNumHeader.RefNumbers[0].ZE_ExciseTaxRefNumber;
			taxRate.ZH_UnitOfMeasure = customsUnit;
			taxRate.ZH_Title = "Excise TAX";
			taxRate.ZH_RateType = RateTypes.Codes.AcceptT;
			taxRate.ZH_Rate = 12;
			taxRate.ZH_EffectiveDate = effectiveDate;
			taxRate.ZH_ExpiryDate = expiryDate;
			#endregion

			#region  Populate Tariff 
			var tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = "4901";
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = currentDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = currentDate;

			var rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			var rateLine = rate.RateLines.AddNew();
			rateLine.ZR_DutyRateMax = 0;
			rateLine.ZR_DutyRateMin = 0;
			rateLine.ZR_DutyRateRegular = 0;
			rateLine.ZR_DutyRateType = RateTypes.Codes.Free;
			#endregion

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.CA_TreatmentCode = "02";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoiceHeader.JZ_InvoiceAmount = 50;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CAD";
			invoiceHeader.JZ_InvoiceCurrExRate = 1;

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_99TariffCode = "4901";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnit;
			invoiceLine.JI_FormattedTariff = "0789456123";
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;

			invoiceHeader.CA_DDPDeductDutyOnly = false;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);

			var duty0 = invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate;
			invoiceHeader.CA_DDPDeductDutyOnly = true;
			var duty1 = invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate;
			AssertNotEquals("Duty value automatically reculated after CA_DDPDeductDutyOnly is selected", duty0, duty1);

			AssertEquals("CA_DDPDeductDutyOnly", true, invoiceHeader.CA_DDPDeductDutyOnly);
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			var duty2 = invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate;
			AssertEquals("CA_DDPDeductDutyOnly", false, invoiceHeader.CA_DDPDeductDutyOnly);
			AssertNotEquals("Duty value automatically reculated after CA_DDPDeductDutyOnly is automatically cleared", duty1, duty2);
		}

		void PrepareGlobalTariffData(BusinessObjectFactory factory, string tariffNumber)
		{
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(factory, DutyAndTaxTypes.Codes.ExciseTax, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(factory, DutyAndTaxTypes.Codes.CustomsDuty, rateType2.PK);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode3 = universalHelper.LoadOrCreateNewCusRateCode(factory, "E90", rateType3.PK);
			var rateCode4 = universalHelper.LoadOrCreateNewCusRateCode(factory, "E91", rateType3.PK);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(factory, "BB1", rateType3.PK);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, tariffNumber, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate1 = universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(tariff1, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(tariff1, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			factory.Save();
		}

		public void TestE2_ContactShouldNotHasInvalidCharacters()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			ContactNameValidataionHelperForTest.AssertE2_Contact(invoice.SupplierDocumentaryAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(invoice.SupplierPickupDeliveryAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(invoice.CommercialInvoiceOriginator);
			ContactNameValidataionHelperForTest.AssertE2_Contact(invoice.FinalConsigneeAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(invoice.BuyerDocumentaryAddress);
			ContactNameValidataionHelperForTest.AssertE2_Contact(invoice.ExporterDocumentaryAddress);
		}

		protected override Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (fExpectedDocAddressTypes == null)
				{
					fExpectedDocAddressTypes = base.ExpectedDocAddressTypes;
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.OriginatingConsignorAddress, DocAddressType.OriginatingConsignorAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.SupplierDocumentaryAddress, DocAddressType.SupplierDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.SupplierPickupDeliveryAddress, DocAddressType.SupplierPickupDeliveryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.CommercialInvoiceOriginator, DocAddressType.CommercialInvoiceOriginator);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.FinalConsigneeAddress, DocAddressType.FinalConsigneeAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.BuyerDocumentaryAddress, DocAddressType.BuyerDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.Exporter, DocAddressType.Exporter);
				}
				return fExpectedDocAddressTypes;
			}
		}
		Hashtable fExpectedDocAddressTypes;

		#region ICustomsCustomLabelsConfigOrgProvider

		public void TestICustomsCustomLabelsConfigOrgProvider()
		{
			var customsCustomLabelsConfigOrgProvider = InvoiceHeader as ICustomsCustomLabelsConfigOrgProvider;
			AssertEquals(JobComInvoiceLine.Schema.JI_PartAttrib1, customsCustomLabelsConfigOrgProvider.PartAttribute1);
			AssertEquals(JobComInvoiceLine.Schema.JI_PartAttrib2, customsCustomLabelsConfigOrgProvider.PartAttribute2);
			AssertEquals(JobComInvoiceLine.Schema.JI_PartAttrib3, customsCustomLabelsConfigOrgProvider.PartAttribute3);
			AssertEquals(JobComInvoiceLine.Schema.JI_SerialNumber, customsCustomLabelsConfigOrgProvider.SerialNumber);
		}

		#endregion

		protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO)
		{
			return base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO) || bO is JobDocAddress;
		}

		protected override IEnumerable<string> MessageTypesForDefaultCurrencyToLocalCurrency(BaseJobDeclaration declaration)
		{
			return base.MessageTypesForDefaultCurrencyToLocalCurrency(declaration).Except(JobMessageTypeList.Codes.Export);
		}

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.groupHeader; }
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.invoiceHeader; }
		}

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		public void TestEDIInvoiceOGDProperties()
		{
			SetUpEDIInvoiceOGD();
			var supplier2 = helper.CreateOrganisation("SUP", "ALTERNATE SUPPLIER NAME", "AUMEL", "ALT SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567");
			IEDIInvoiceOGD invoiceHeader = invoice;
			//Min
			AssertEquals("InvoiceNumber", invoice.JZ_InvoiceNumber, invoiceHeader.InvoiceNumber);
			AssertEquals("Parent Vendor", declaration1.SupplierDocumentaryAddress, invoiceHeader.Vendor);
			AssertEquals("Purchaser", invoice.BuyerDocumentaryAddress, invoiceHeader.Purchaser);
			AssertEquals("Consignee", invoice.FinalConsigneeAddress, invoiceHeader.Consignee);
			AssertEquals("Exporter", invoice.ExporterDocumentaryAddress, invoiceHeader.Exporter);

			AssertEquals("CommonCountryOfOrigin is US", (ZString)("U" + invoice.JZ_RW_NKOriginState), invoiceHeader.CommonCountryOfOrigin);
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CommonCountryOfOrigin", invoice.JZ_RN_NKDefaultOrigin, invoiceHeader.CommonCountryOfOrigin);
			invoice.JZ_RN_NKDefaultOrigin = ZString.Empty;
			AssertEquals("CommonCountryOfOrigin is blank", "VAR", invoiceHeader.CommonCountryOfOrigin);

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = "CA";
			AssertEquals("CommonCountryOfOrigin is VAR if there are override origins that don’t match the header", "VAR", invoiceHeader.CommonCountryOfOrigin);
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = "IL";
			AssertEquals("CommonCountryOfOrigin is VAR if there are override origins that don’t match the header", "VAR", invoiceHeader.CommonCountryOfOrigin);
			invoiceLine.JI_StateOrRegionOfOrigin = "IL";
			AssertEquals("CommonCountryOfOrigin is US if all override origins match the header", (ZString)("U" + invoice.JZ_RW_NKOriginState), invoiceHeader.CommonCountryOfOrigin);

			AssertEquals("CommonCountryOfExport", (ZString)("U" + invoice.CA_USStateOfExport), invoiceHeader.CommonCountryOfExport);
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			AssertEquals("CommonCountryOfExport", invoice.CA_RN_NKExport, invoiceHeader.CommonCountryOfExport);

			AssertNotNull("InvoiceLines", invoiceHeader.InvoiceLines);
			//AQ
			AssertEquals("InvoiceDate", invoice.JZ_InvoiceDate, invoiceHeader.InvoiceDate);
			AssertEquals("InvoiceAmount", invoice.JZ_InvoiceAmount, invoiceHeader.InvoiceAmount);
			AssertEquals("InvoiceCurrency", invoice.JZ_RX_NKInvoice_Currency, invoiceHeader.InvoiceCurrency);

			ZDecimal expectedIncludedOFTandONS = GetAmountInCAD(invoice.IncludedOverseasFreight) + GetAmountInCAD(invoice.IncludedOverseasInsurance);
			AssertEquals("IncludedOFTAndONS", expectedIncludedOFTandONS, invoiceHeader.IncludedOFTAndONS);
			AssertEquals("IncludedConstruction", GetAmountInCAD(invoice.IncludedConstruction), invoiceHeader.IncludedConstruction);
			AssertEquals("IncludedPacking", GetAmountInCAD(invoice.IncludedPackingCosts), invoiceHeader.IncludedPacking);
			ZDecimal expectedExcludedOFTandONS = GetAmountInCAD(invoice.ExcludedOverseasFreight) + GetAmountInCAD(invoice.ExcludedOverseasInsurance);
			AssertEquals("ExcludedOFTAndONS", expectedExcludedOFTandONS, invoiceHeader.ExcludedOFTAndONS);
			AssertEquals("ExcludedCommission", GetAmountInCAD(invoice.ExcludedCommission), invoiceHeader.ExcludedCommission);
			AssertEquals("ExcludedPacking", GetAmountInCAD(invoice.ExcludedPackingCosts), invoiceHeader.ExcludedPacking);

			AssertEquals("OtherReference", invoice.CA_OtherReference, invoiceHeader.OtherReference);
			AssertEquals("Shipper", invoice.SupplierPickupDeliveryAddress, invoiceHeader.Shipper);
			AssertEquals("DepartmentRuling", invoice.CA_DepartmentRuling, invoiceHeader.DepartmentRuling);

			AssertEquals("LastPortName", invoice.LastPort.RL_PortName.Left(25), invoiceHeader.LastPortName);
			AssertEquals("LastPortDate", invoice.JZ_ValuationDateOverride, invoiceHeader.LastPortDate);
			invoice.CA_RL_NKLastPort = "XXXXX";
			AssertEquals("LastPortName", declaration1.PortOfLoading.RL_PortName.Left(25), invoiceHeader.LastPortName);
			invoice.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertEquals("LastPortDate", declaration1.JE_ExportDate, invoiceHeader.LastPortDate);

			AssertEquals("TranshipmentCountry", invoice.CA_RN_NKTranshipment, invoiceHeader.TranshipmentCountry);
			AssertEquals("ConditionsOfSale", invoice.CA_ConditionsOfSale, invoiceHeader.ConditionsOfSale);
			AssertEquals("TermsOfPayment", invoice.CA_TermsOfPayment, invoiceHeader.TermsOfPayment);
			AssertEquals("ServicesInd", invoice.CA_ServicesInd, invoiceHeader.ServicesInd);
			AssertEquals("RoyaltyInd", invoice.CA_RoyaltyInd, invoiceHeader.RoyaltyInd);
			//OGD
			AssertEquals("Manufacturer", invoice.ManufacturerAddress, invoiceHeader.Manufacturer);

			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoice.CA_LVSCarrier = "3001";
			invoice.JZ_InvoiceDate = ZDateTime.Empty;
			AssertEquals("OtherReference", "OTHER REFERENCE\r\nCarrier: 3001 Invoice Date: ", invoiceHeader.OtherReference);

			invoice.JZ_InvoiceDate = new ZDateTime(2015, 9, 10);
			AssertEquals("OtherReference", "OTHER REFERENCE\r\nCarrier: 3001 Invoice Date: " + new ZDateTime(2015, 9, 10).ToString("dd MMM yyyy", CultureInfo.CurrentCulture), invoiceHeader.OtherReference);

			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertEquals("Vendor", invoice.SupplierDocumentaryAddress, invoiceHeader.Vendor);
			invoice.SupplierPickupDeliveryAddress.OrganisationPK = supplier2.PK;
			AssertNull("Send the shipper address only when it is different to the vendor address", invoiceHeader.Shipper);
			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierPickupDeliveryAddress.E2_AddressOverride = true;
			AssertEquals("Overriden Vendor", invoice.SupplierDocumentaryAddress, invoiceHeader.Vendor);
			AssertEquals("Overriden Shipper", invoice.SupplierPickupDeliveryAddress, invoiceHeader.Shipper);

			invoice.FinalConsigneeAddress.OrganisationPK = declaration1.JE_OH_Importer;
			AssertNull("Consignee", invoiceHeader.Consignee);

			declaration1.ImporterOfRecordAddress.OrganisationPK = helper.CreateOrganisation("IOR", "IMPORTER OF RECORD NAME", "CATOR", "IMPORTER OF RECORD ADDRESS", "TORONTO", "ON", "K1C1C1", "123 4567").PK;
			AssertEquals("Consignee", invoice.FinalConsigneeAddress, invoiceHeader.Consignee);

			invoice.FinalConsigneeAddress.E2_OA_Address = declaration1.ImporterOfRecordAddress.E2_OA_Address;
			AssertNull("Consignee", invoiceHeader.Consignee);
		}

		[StressTest]
		public void TestEDIInvoiceOGDInvoiceLines()
		{
			SetUpEDIInvoiceOGD();
			var expectedLines = new List<JobComInvoiceLine>();
			invoice = declaration1.Invoices.AddNew();
			for (var i = 0; i < 6; i++)
			{
				var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				line.JI_Tariff = i.ToString();
				line.JI_LineNo = (ZShort)(i + 1);
				expectedLines.Add(line);
			}

			expectedLines.OrderBy(x => x.JI_LineNo);
			for (var i = 0; i < 6; i++)
			{
				var line = expectedLines[i];
				using (line.SuspendRecalculatePageNumbers())
				{
					line.CA_PageNumber = i % 3 + 1;
				}
			}

			declaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration1.DoMerge();

			var lines = new List<IEDIInvoiceLineOGD>(((IEDIInvoiceMin)invoice).InvoiceLines);

			AssertEquals("Line Count", invoice.InvoiceLines.Count, lines.Count);
			AssertIEDIInvoiceLine(lines[0], expectedLines[0], 1, 1);
			AssertIEDIInvoiceLine(lines[1], expectedLines[3], 1, 2);
			AssertIEDIInvoiceLine(lines[2], expectedLines[1], 2, 1);
			AssertIEDIInvoiceLine(lines[3], expectedLines[4], 2, 2);
			AssertIEDIInvoiceLine(lines[4], expectedLines[2], 3, 1);
			AssertIEDIInvoiceLine(lines[5], expectedLines[5], 3, 2);
		}

		void AssertIEDIInvoiceLine(IEDIInvoiceLineOGD line, JobComInvoiceLine expectedLine, int expectedPageNumber, int expectedLineNumber)
		{
			AssertEquals("Line", expectedLine, ((CusEntryLine)line).RandomLine);
			AssertEquals("PageNumber", expectedPageNumber, line.PageNumber);
			AssertEquals("LineNumber", expectedLineNumber, line.LineNumber);
		}

		#region Implementation

		ZDecimal GetAmountInCAD(Money money)
		{
			return invoice.CurrencyConverter.ConvertExact(money, cad).Amount;
		}

		void SetUpEDIInvoiceOGD()
		{
			helper = new DeclarationTestHelper(Factory, true);
			cad = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Canada);
			var supplier = helper.CreateOrganisation("SUP", "ALTERNATE SUPPLIER NAME", "AUMEL", "ALT SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567");
			var buyer = helper.CreateOrganisation("BUY", "PURCHASER NAME", "CATOR", "PURCHASER ADDRESS", "TORONTO", "ON", "K1C1C1", "123 4567");
			var consignee = helper.CreateOrganisation("CNE", "CONSIGNEE NAME", "CABLO", "CONSIGNEE ADDRESS", "BEAVER LODGE", "AB", "K1D1D1", "123 4567");
			var shipper = helper.CreateOrganisation("SHP", "SHIPPER NAME", "NZAKL", "SHIPPER ADDRESS", "AUCKLAND", "123 4567");
			var exporter = helper.CreateOrganisation("EXP", "EXPORTER NAME", "NZAKL", "EXPORTER ADDRESS", "AUCKLAND", "123 4567");
			var manufacturer = helper.CreateOrganisation("MNF", "MANUFACTURER NAME", "NZAKL", "MANUFACTURER ADDRESS", "AUCKLAND", "123 4567");
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "TORONTO", "ON", "K1C1C1", "123 4567");

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "CA###";
			unloco.RL_PortName = "123456789012345678901234567890";

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_OH_Supplier = supplier.PK;
			declaration1.JE_RL_NKPortOfLoading = unloco.RL_Code;
			declaration1.JE_ExportDate = new ZDateTime(2010, 6, 29);
			declaration1.JE_OH_Importer = importer.PK;
			invoice = declaration1.Invoices.AddNew();
			//Min
			invoice.JZ_InvoiceNumber = "INV987654321";

			invoice.BuyerDocumentaryAddress.E2_OA_Address = buyer.Addresses.AddNew().PK;
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.FinalConsigneeAddress.E2_OA_Address = consignee.Addresses.AddNew().PK;
			invoice.ExporterDocumentaryAddress.E2_OA_Address = exporter.Addresses.AddNew().PK;
			invoice.InvoiceLines.AddNew();
			//AQ
			invoice.JZ_InvoiceDate = new ZDateTime(2009, 6, 30);
			invoice.JZ_InvoiceAmount = 1234.50m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;

			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m);
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasInsurance, 50m);
			invoice.Charges.AddNew(CAChargeTypeList.Codes.Construction, 200m);
			invoice.Charges.AddNew(CAChargeTypeList.Codes.PackingCost, 300.60m);
			invoice.Charges.AddNew(CAChargeTypeList.Codes.Commission, 150m);

			invoice.CA_OtherReference = "OTHER REFERENCE";
			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipper.PK;
			invoice.CA_DepartmentRuling = "DEPARTMENTAL RULINGS";
			invoice.CA_RL_NKLastPort = "AUBNE";
			invoice.JZ_ValuationDateOverride = new ZDateTime(2009, 4, 28);
			invoice.CA_RN_NKTranshipment = "SG";
			invoice.CA_ConditionsOfSale = "CONDITIONS OF SALE";
			invoice.CA_TermsOfPayment = "TERMS OF PAYMENT";
			invoice.CA_ServicesInd = true;
			invoice.CA_RoyaltyInd = true;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = USStatesList.Codes.NewYork;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.CA_USStateOfExport = USStatesList.Codes.NewYork;

			//OGD
			invoice.JZ_OA_ManufacturerAddress = manufacturer.Addresses.AddNew().PK;
		}

		JobDeclaration declaration1;
		DeclarationTestHelper helper;
		JobComInvoiceHeader invoice;
		RefCurrency cad;

		#endregion

		public void TestGetNewValidation()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var declaration = invoiceHeader.JobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Misc Validation", typeof(JobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Validation", typeof(ImportJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals("IM2 Validation", typeof(ImportJobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Validation", typeof(B2JobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Validation", typeof(B2JobComInvoiceHeaderValidation), invoiceHeader.Validation.GetType());
		}

		public void TestFirstCusLinkPackage_ShouldRecalculate_WhenItemAdded()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();

			var item = invoiceHeader.PackagesForInvoicesForBindingOnly[0];
			AssertEquals(item.PK, invoiceHeader.FirstCusLinkPackage.PK);

			invoiceHeader.PackagesForInvoicesForBindingOnly.AddNew();
			AssertEquals(item.PK, invoiceHeader.FirstCusLinkPackage.PK);
		}

		public void TestFirstCusLinkPackage_ShouldRecalculate_WhenItemDeleted()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var item1 = invoiceHeader.PackagesForInvoicesForBindingOnly[0];
			var item2 = invoiceHeader.PackagesForInvoicesForBindingOnly.AddNew();
			AssertEquals(item1.PK, invoiceHeader.FirstCusLinkPackage.PK);

			invoiceHeader.PackagesForInvoicesForBindingOnly.Remove(item1);
			AssertEquals(item2.PK, invoiceHeader.FirstCusLinkPackage.PK);
		}

		public void TestFirstCusLinkPackage_ShouldRecalculate_ValueChanged()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			AssertEquals(100, invoiceHeader.FirstPackageQty);

			invoiceHeader.PackagesForInvoicesForBindingOnly[0].PackQty = 200;
			AssertEquals(200, invoiceHeader.FirstPackageQty);
		}

		public void TestFirstCusLinkPackageLineValue()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var cusLinkPackage1 = invoiceHeader.PackagesForInvoicesForBindingOnly[0];
			AssertEquals(cusLinkPackage1.PackQty, invoiceHeader.FirstCusLinkPackage.PackQty);

			var cusLinkPackage2 = new InvoiceHeaderCusLinkPackage(invoiceHeader);
			cusLinkPackage2.IsLinked = true;
			cusLinkPackage2.Package = invoiceHeader.JobDeclaration.Packages[0];
			cusLinkPackage2.PackQty = 200;
			invoiceHeader.PackagesForInvoicesForBindingOnly.Add(cusLinkPackage2);
			AssertEquals(cusLinkPackage1.PackQty, invoiceHeader.FirstCusLinkPackage.PackQty);

			invoiceHeader.PackagesForInvoicesForBindingOnly.Sort("PackQty", ListSortDirection.Descending);
			AssertEquals(cusLinkPackage2.PackQty, invoiceHeader.FirstCusLinkPackage.PackQty);

			invoiceHeader.PackagesForInvoicesForBindingOnly.RemoveAndDelete(invoiceHeader.PackagesForInvoicesForBindingOnly[0]);
			AssertEquals(cusLinkPackage1.PackQty, invoiceHeader.FirstCusLinkPackage.PackQty);
		}

		public void TestIsPackQtyAndIsLinked_ReadOnly()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			Assert(!invoiceHeader.FirstPackageIsLinkedInfo.ReadOnly);
			Assert(!invoiceHeader.FirstPackageQtyInfo.ReadOnly);
			invoiceHeader.PackagesForInvoicesForBindingOnly[0].IsLinked = false;
			Assert(!invoiceHeader.FirstPackageIsLinkedInfo.ReadOnly);
			Assert(invoiceHeader.FirstPackageQtyInfo.ReadOnly);
			invoiceHeader.PackagesForInvoicesForBindingOnly.RemoveAll();
			Assert(invoiceHeader.FirstPackageIsLinkedInfo.ReadOnly);
			Assert(invoiceHeader.FirstPackageQtyInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			base.SetUp();
			var package = invoiceHeader.JobDeclaration.Packages.AddNew();
			invoiceHeader.PackagesPivot.AddPivotFor(package);
			var cusLinkPackage = new InvoiceHeaderCusLinkPackage((JobComInvoiceHeader)invoiceHeader);
			cusLinkPackage.IsLinked = true;
			cusLinkPackage.Package = package;
			cusLinkPackage.PackQty = 100;
			invoiceHeader.PackagesForInvoicesForBindingOnly.Add(cusLinkPackage);
			return invoiceHeader;
		}
	}
}
