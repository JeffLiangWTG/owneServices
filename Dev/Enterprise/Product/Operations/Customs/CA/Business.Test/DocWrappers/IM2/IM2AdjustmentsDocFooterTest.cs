using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(IM2AdjustmentsDocFooter))]
	sealed class IM2AdjustmentsDocFooterTest : NonPersistentBusinessObjectTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestIM2AdjustmentsDocumentFooterMembers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = "TRF";
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.CA_IsDocAttached = true;
			declaration.CA_JustificationForRequest = "JUSTIFICATION FOR REQUEST";
			declaration.CA_Under = "UNDER";
			declaration.CA_B2Explanation = "EXPLANATION";
			declaration.CA_ClaimedInterestAmount = 11m;
			declaration.CA_AnySightDepositAmount = 12m;
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "MY COMPANY";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MY BRANCH";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_WorkPhone = "MY PHONE";
			staff.GS_FullName = "MY NAME";
			staff.GS_GB_HomeBranch = branch.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 1000m;
			line1.JI_Tariff = "0000000001";
			var sima = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			var duty = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 40m;
			var excise = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 3m;
			var gst = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 50m;
			declaration.ResumeApportionment();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var im2 = declaration.GetNewCopyToB2Declaration();
			var line2 = im2.InvoiceLines[0];
			line2.JI_Tariff = "0000000002";
			var sima2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			sima2.C1_Amount = 600;
			var duty2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 50;
			var excise2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			excise2.C1_Amount = 4m;
			var gst2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			gst2.C1_Amount = 60m;
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();
			im2.ResumeApportionment();
			var docPages = new IM2AdjustmentsDocPage().GetPages(null, im2);
			var footer = new IM2AdjustmentsDocFooter(im2, docPages);
			declaration.ResumeApportionment();

			AssertEquals("X", footer.DocAttached);
			AssertEquals("JUSTIFICATION FOR REQUEST", footer.JustificationForRequest);
			AssertEquals("UNDER", footer.Under);
			AssertEquals("EXPLANATION", footer.Explanation);
			AssertEquals("MY NAME", footer.BrokerName);
			AssertEquals("MY PHONE", footer.BrokerPhone);
			AssertEquals(@"MY COMPANY
MY BRANCH", footer.BrokerAgent);
			AssertNull("Broker Signature Image", footer.BrokerSignatureImage);

			AssertEquals("22.00", footer.TotalCustomDuties);
			AssertEquals("100.00", footer.TotalSIMAAssessment);
			AssertEquals("1.00", footer.TotalExciseTax);
			AssertEquals("123.00", footer.SubTotal);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals("(60-50)", "10.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals("(60-50=10>0 result:10)", "10.00", footer.TotalGST);

			AssertEquals("133.00", footer.AmountDueReceiverGeneralCanada);
			AssertEquals(ZString.Empty, footer.AmountDueClaimant);

			im2.CA_AnySightDepositAmount = 50m;
			sima2.C1_Amount = 400;
			duty2.C1_Amount = 30;
			excise2.C1_Amount = 2;
			gst2.C1_Amount = 40m;
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();
			declaration.ResumeApportionment();
			var docPages2 = new IM2AdjustmentsDocPage().GetPages(null, im2);
			var footer2 = new IM2AdjustmentsDocFooter(im2, docPages2);
			declaration.ResumeApportionment();

			AssertEquals("40.00", footer2.TotalCustomDuties);
			AssertEquals("-100.00", footer2.TotalSIMAAssessment);
			AssertEquals("-1.00", footer2.TotalExciseTax);
			AssertEquals("-61.00", footer2.SubTotal);
			AssertEquals("11.00", footer2.Interest);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer2 = new IM2AdjustmentsDocFooter(im2, docPages2);
			AssertEquals("(40-50)", "-10.00", footer2.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer2 = new IM2AdjustmentsDocFooter(im2, docPages2);
			AssertEquals("(40-50=-10<0 result:0)", ZString.Empty, footer2.TotalGST);

			AssertEquals(ZString.Empty, footer2.AmountDueReceiverGeneralCanada);
			AssertEquals("61.00", footer2.AmountDueClaimant);

			sima.C1_ExemptCode = SIMACodes.Codes.C32;
			sima2.C1_ExemptCode = SIMACodes.Codes.C32;
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();
			var docPages3 = new IM2AdjustmentsDocPage().GetPages(null, im2);
			var footer3 = new IM2AdjustmentsDocFooter(im2, docPages3);
			declaration.ResumeApportionment();
			AssertEquals(ZString.Empty, footer3.TotalSIMAAssessment);
			AssertEquals("-61.00", footer2.SubTotal);
			AssertEquals("61.00", footer2.AmountDueClaimant);
		}

		public void TestB2TotalOnFormAndDocumentShouldBeEqual()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 1000m;
			line1.JI_Tariff = "0000000001";
			var sima = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0000000002";
			var duty = line2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 40m;
			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0000000003";
			var excise = line3.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 3m;
			var line4 = header.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "0000000004";
			var gst = line4.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 50m;
			var line5 = header.JobComInvoiceLines.AddNew();
			line5.JI_Tariff = "0000000005";
			var line6 = header.JobComInvoiceLines.AddNew();
			line6.JI_Tariff = "0000000006";
			var line7 = header.JobComInvoiceLines.AddNew();
			line7.JI_Tariff = "0000000007";
			var line8 = header.JobComInvoiceLines.AddNew();
			line8.JI_Tariff = "0000000008";
			var line9 = header.JobComInvoiceLines.AddNew();
			line9.JI_Tariff = "0000000009";
			var line10 = header.JobComInvoiceLines.AddNew();
			line10.JI_Tariff = "0000000010";
			var sima10 = line10.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			sima10.C1_ExemptCode = SIMACodes.Codes.C51;
			sima10.C1_Override = true;
			sima10.C1_Amount = 500m;
			var line11 = header.JobComInvoiceLines.AddNew();
			line11.JI_Tariff = "0000000011";
			declaration.ResumeApportionment();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(11, declaration.B3EntryHeader.AllEntryLines.Count);
			Factory.Save();

			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.ResumeApportionment();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			im2.CalculateIM2Total();
			Factory.Save();
			AssertEquals("B2 Total On Form", 0m, im2.CA_B2Total);
			var docPages = new IM2AdjustmentsDocPage().GetPages(null, im2);
			var footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals(11, im2.B3EntryHeader.AllEntryLines.Count);
			AssertEquals(0, docPages.Count());

			im2 = declaration.GetNewCopyToB2Declaration();
			var im2invoice = (JobComInvoiceHeader)im2.Invoices.First();
			im2invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var im2line = (JobComInvoiceLine)im2invoice.InvoiceLines.First();

			var im2sima = im2line.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty)
				?? im2line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			im2sima.C1_ExemptCode = SIMACodes.Codes.C51;
			im2sima.C1_Amount = 1000m;
			im2.ResumeApportionment();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			im2.CalculateIM2Total();
			Factory.Save();
			AssertEquals(11, im2.B3EntryHeader.AllEntryLines.Count);
			AssertEquals("B2 Total On Form", 1000m, im2.CA_B2Total);
			docPages = new IM2AdjustmentsDocPage().GetPages(null, im2);
			footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals(6, docPages.Count());
			AssertEquals("B2 Total On Form", footer.AmountDueReceiverGeneralCanada, im2.CA_B2Total.ToString(2));
		}

		public void TestTotalGSTWithSplitLine()
		{
			#region Create Test Data
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";
			var gst = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 100m;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			var asClaimedInvoice = im2.Invoices[0];
			var asClaimedInvoiceLine1 = asClaimedInvoice.JobComInvoiceLines[0];
			asClaimedInvoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			var asClaimedGST1 = asClaimedInvoiceLine1.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			asClaimedGST1.C1_Amount = 80m;
			var asClaimedInvoiceLine2 = asClaimedInvoice.JobComInvoiceLines.AddNew();
			asClaimedInvoiceLine2.JI_Tariff = "0301100000";
			asClaimedInvoiceLine2.JI_CustomsQuantity = 20;
			asClaimedInvoiceLine2.JI_CustomsUnitQty = "KGM";
			asClaimedInvoiceLine2.JI_InvoiceQuantity = 10;
			asClaimedInvoiceLine2.JI_InvoiceUQ = "NMB";
			asClaimedInvoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			asClaimedInvoiceLine2.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			asClaimedInvoiceLine2.JI_LineNo = 2;
			asClaimedInvoiceLine2.CA_PageNumber = 1;
			asClaimedInvoiceLine2.JI_LinePrice = 100m;
			asClaimedInvoiceLine2.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			asClaimedInvoiceLine2.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			asClaimedInvoiceLine2.CA_TreatmentCode = "03";
			asClaimedInvoiceLine2.CA_PreviousB3SubHeaderNo = 1;
			asClaimedInvoiceLine2.CA_PreviousB3LineNo = 1;
			var asClaimedGST2 = asClaimedInvoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			asClaimedGST2.C1_Override = true;
			asClaimedGST2.C1_Amount = 30m;
			im2.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			im2.CA_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			Factory.Save();
			#endregion

			var docPages = new IM2AdjustmentsDocPage().GetPages(null, im2);

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals("(80+30-100)", "10.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals("(80+30-100>0 result:10)", "10.00", footer.TotalGST);

			asClaimedGST2.C1_Amount = 10m;
			im2.DoMerge();
			Factory.Save();
			docPages = new IM2AdjustmentsDocPage().GetPages(null, im2);
			footer = new IM2AdjustmentsDocFooter(im2, docPages);
			AssertEquals("80 + 10 - 100 < 0", ZString.Empty, footer.TotalGST);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = "TRF";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.ApportionmentDirty = false;
			im2.DoMerge();
			var docPages = new IM2AdjustmentsDocPage().GetPages(null, im2);
			return new IM2AdjustmentsDocFooter(im2, docPages);
		}
	}
}
