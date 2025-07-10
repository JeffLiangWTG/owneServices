using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusEntryHeaderInternalTest : TestCaseWithFactory
	{
		public void TestDetailsNotToBeAmended()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("Nature", "N10", entryLine.NatureTypeForCMR);

			entry.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals("DetailsNotToBeAmended", "", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);

			entry.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("DetailsNotToBeAmended", "", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);

			entry.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("DetailsNotToBeAmended", "", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);

			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entry.CH_HighestLineNumber = 1;
			entry.EntryNumber = "1";
			AssertEquals("DetailsNotToBeAmended", "N10", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "N10", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_IsPackToBondForLine = true;

			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("DetailsNotToBeAmended", "N10", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", true, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "N10", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "", entryLine2.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine2.HasNonAmendableChanges);

			entry.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("DetailsNotToBeAmended", "N10", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", true, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "N10", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "", entryLine2.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine2.HasNonAmendableChanges);

			invoiceLine2.JI_IsPackToBondForLine = false;
			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("DetailsNotToBeAmended", "N10", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "N10", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "", entryLine2.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine2.HasNonAmendableChanges);

			entry.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entry.CH_HighestLineNumber = 2;
			AssertEquals("DetailsNotToBeAmended", "N10", entry.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "N10", entryLine.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine.HasNonAmendableChanges);
			AssertEquals("DetailsNotToBeAmended", "N10", entryLine2.ZA_DetailsNotToBeAmended);
			AssertEquals("HasNonAmendableChanges", false, entryLine2.HasNonAmendableChanges);
		}

		public void TestUsedCurrenciesIsResetByResetCachedValues()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertNull("Preconditions: fUsedCurrencies expected to be null initially", entryHeader.fUsedCurrencies);
			ICurrency[] temp = entryHeader.UsedCurrencies;
			AssertNotNull("fUsedCurrencies should not be null after invoking the property.", entryHeader.fUsedCurrencies);
			entryHeader.ResetTotalsAndCachedValues();
			AssertNull("fUsedCurrencies expected to be null again after calling ResetCachedValues", entryHeader.fUsedCurrencies);
		}

		public void TestEndToEndTestForUsedCurrencies()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			declaration.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Only one currency should be in the list.", 1, entryHeader.UsedCurrencies.Length);
			AssertEquals("The used currency is that of the current company.", JobDeclaration.LocalCurrencyConstantCode, EntryHeader.UsedCurrencies[0].Code);

			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_Amount = 10m; //Note: it sets the currency to that of the invoice header
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;
			AssertNull("No NZD currency expected to be found in the UsedCurrencies list because the list is cached and should not recollect currencies until next merge.",
				Array.Find(entryHeader.UsedCurrencies, (ICurrency currency) =>
				{ return currency.Code == Core.Constants.CurrencyCodes.NewZealand; }));

			declaration.DoMerge();

			AssertEquals("Two currencies should be in the list.", 2, entryHeader.UsedCurrencies.Length);
			AssertNotNull("Looking for NZD currency.",
				Array.Find(entryHeader.UsedCurrencies, (ICurrency currency) =>
				{ return currency.Code == Core.Constants.CurrencyCodes.NewZealand; }));
			AssertNotNull("Looking for the currency company currency.",
				Array.Find(entryHeader.UsedCurrencies, (ICurrency currency) =>
				{ return currency.Code == JobDeclaration.LocalCurrencyConstantCode; }));
		}

		public void TestTAndI()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, "AUD");

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 4000m;
			line1.AddInfo.ZA_TILV = "1.23AUD";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 6000m;
			line2.AddInfo.ZA_TILV = "2.46AUD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var cusEntryLine1 = line1.CusEntryLine;
			var cusEntryLine2 = line2.CusEntryLine;
			AssertEquals("TILV equals calculated", 1.23m, cusEntryLine1.TILV);
			AssertEquals("TILV equals calculated", 2.46m, cusEntryLine2.TILV);

			var cusEntryHeader = cusEntryLine1.Header;
			AssertEquals("TAndI equals sum of calculated", 3.69m, cusEntryHeader.TAndI);
			AssertEquals("TAndI equals sum of calculated", 3.69m, cusEntryHeader.TransportAndInsuranceInLocalCurrency.Amount.Round(2));

			cusEntryHeader.UpdateTILV("10.20AUD");
			cusEntryLine1.UpdateTILV("1.10AUD");
			cusEntryLine2.UpdateTILV("2.20AUD");
			AssertEquals("TAndI equals Customs Total TILV Response Value", 10.20m, cusEntryHeader.TAndI);
			AssertEquals("TAndI equals Customs Total TILV Response Value", 10.20m, cusEntryHeader.TransportAndInsuranceInLocalCurrency.Amount.Round(2));

			Factory.Save();

			var cusEntryHeaderCopy = Factory.Load<CusEntryHeader>(cusEntryHeader.PK);
			AssertEquals("TAndI persists", 10.20m, cusEntryHeaderCopy.TAndI);
			AssertEquals("TAndI persists", 10.20m, cusEntryHeaderCopy.TransportAndInsuranceInLocalCurrency.Amount.Round(2));

			cusEntryHeaderCopy.UpdateTILV("");  // Declarations on a Consolidated Entry have line values but no header value
			AssertEquals("TAndI equals sum of Line TILV Response Values", 3.30m, cusEntryHeaderCopy.TAndI);
			AssertEquals("TAndI equals sum of Line TILV Response Values", 3.30m, cusEntryHeaderCopy.TransportAndInsuranceInLocalCurrency.Amount.Round(2));
		}

		public void TestTAndITransmitConditionChecker()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertNotNull("TAndITransmitConditionChecker", entryHeader.TAndITransmitConditionChecker);
			AssertEquals("Is Calculated", false, entryHeader.TAndITransmitConditionChecker.IsCalculationUpToDateExposedForTesting);

			bool accessed = entryHeader.TAndITransmitConditionChecker.ShouldTransmitTAndIForHeader;
			AssertEquals("Is now calculated", true, entryHeader.TAndITransmitConditionChecker.IsCalculationUpToDateExposedForTesting);

			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("Is calculated reset", false, entryHeader.TAndITransmitConditionChecker.IsCalculationUpToDateExposedForTesting);
		}

		public void TestTransportAndInsuranceInARightCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 0.75m, helper.USDCurrency);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 0.5m, helper.EURCurrency);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ExportDate = ZDateTime.Today;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 5000m;
			line1.JI_CL = entryLine.PK;
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.JI_CL = entryLine2.PK;

			BaseJobComInvHeaderCharge charge1 = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "USD");
			BaseJobComInvHeaderCharge charge2 = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10m, "EUR");
			entryHeader.ResetTotalsAndCachedValues();
			entryLine.ResetTotalsAndCachedValues();
			entryLine2.ResetTotalsAndCachedValues();

			AssertEquals("TAndI should be in AUD as there are more than one currency involved", "AUD", entryHeader.TransportAndInsuranceCurrencyForMessage.RX_Code);
			AssertEquals("TransportAndInsuranceInACalcualtedCurrency Amount", "AUD", entryHeader.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TransportAndInsuranceInACalcualtedCurrency currency", 153.33m, entryHeader.TransportAndInsuranceForMessage.Amount);

			charge1.J7_RX_NKCurrency = "USD";
			charge2.J7_RX_NKCurrency = "USD";
			entryHeader.ResetTotalsAndCachedValues();
			entryLine.ResetTotalsAndCachedValues();
			entryLine2.ResetTotalsAndCachedValues();

			AssertEquals("TAndI should be in USD now", "USD", entryHeader.TransportAndInsuranceCurrencyForMessage.RX_Code);
			AssertEquals("TransportAndInsuranceInACalculatedCurrency Amount", "USD", entryHeader.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TransportAndInsuranceInACalculatedCurrency currency", 110.00m, entryHeader.TransportAndInsuranceForMessage.Amount.Round(2));
		}

		public void TestTransportAndInsuranceForMessageWhenNoInvoiceIsSplitIntoMultipleEntries()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.Charges.AddNew("OFT", 50m, "AUD");
			invoice.AddInfo.ZA_TILV = "20AUD";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 5000m;

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.AddInfo.ZA_TILV = "0AUD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("TransportAndInsuranceFormessage should return TILV in invoice header", 20m, declaration.CustomsEntryHeaders[0].TransportAndInsuranceForMessage.Amount);

			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;

			OrgHeader warehouse = Factory.New<OrgHeader>();
			OrgAddress address1 = warehouse.Addresses.AddNew();
			OrgCusCode premiseID = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1");
			premiseID.OK_OA_PremisesAddress = address1.PK;

			OrgAddress address2 = warehouse.Addresses.AddNew();

			OrgCusCode premiseID2 = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2");
			premiseID2.OK_OA_PremisesAddress = address2.PK;

			line1.AddInfo.ZA_OA_WarehouseAddress_Hidden = address1.PK;
			line2.AddInfo.ZA_OA_WarehouseAddress_Hidden = address2.PK;
			line2.AddInfo.ZA_TILV = "10AUD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry1 = line1.CusEntryLine.Header;
			AssertEquals("TILV apportioned for the other invoice line which is attached to this entry", 10m, entry1.TransportAndInsuranceForMessage.Amount);

			CusEntryHeader entry2 = line2.CusEntryLine.Header;
			AssertEquals("it should aggregate line's TILV to return as this invoice is split", 10m, entry2.TransportAndInsuranceForMessage.Amount);
		}

		public void TestIsSAC()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals("IsSAC", false, testDec.IsSAC);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("IsSAC", false, entryHeader.IsSAC);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSAC", true, testDec.IsSAC);
			AssertEquals("IsSAC", true, entryHeader.IsSAC);
		}

		public void TestIMessageAttachee()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B";
			entryHeader.EntryNumber = "A";

			AssertEquals("User friendly code", "Entry No:A/Ref No:B", ((IMessageAttachee)entryHeader).UserFriendlyCode);
			AssertEquals("IsValid to send an original", true, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Original));
			AssertEquals("Is not Valid to send an Amendment", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Amend));
			AssertEquals("Is not Valid to send a withdrawal", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw));

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Is not Valid to send an original", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Original));
			AssertEquals("Is Valid to send an amendment", true, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Amend));
			AssertEquals("Is Valid to send a withdrawal", true, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw));

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("IsPostLodge", true, entryHeader.IsStatusPostLodge);
			AssertEquals("Is not Valid to send an original", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Original));
			AssertEquals("Is not Valid to send an amendment", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Amend));
			AssertEquals("Is not Valid to send a withdrawal", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw));

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("IsPostLodge", true, entryHeader.IsStatusPostLodge);
			AssertEquals("Is not Valid to send an original", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Original));
			AssertEquals("Is not Valid to send an amendment", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Amend));
			AssertEquals("Is not Valid to send a withdrawal", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw));

			entryHeader.CH_Status = CMRBaseStatuses.Codes.NotSent;
			AssertEquals("IsPostLodge", false, entryHeader.IsStatusPostLodge);
			AssertEquals("Is not Valid to send an original", true, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Original));
			AssertEquals("Is not Valid to send an amendment", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Amend));
			AssertEquals("Is not Valid to send a withdrawal", false, ((IMessageAttachee)entryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw));
		}

		public void TestHasALineWithPUPIndicator()
		{
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var mockMergedLines = new Mock<CusEntryLineCollection>(mockEntry.Object, Factory);

			mockEntry.Setup(m => m.MergedLines).Returns(mockMergedLines.Object);
			mockMergedLines.Setup(m => m.HasALineWithPUPIndicator).Returns(true);
			AssertEquals("HasALineWithPUPIndicator", true, mockEntry.Object.HasALineWithPUPIndicator);

			mockMergedLines.Reset();
			mockMergedLines.Setup(m => m.HasALineWithPUPIndicator).Returns(false);
			AssertEquals("HasALineWithPUPIndicator", false, mockEntry.Object.HasALineWithPUPIndicator);
		}

		public void TestHaveAmendmentsBeenMadeAndNotYetClearedByCustoms()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("HaveAmendmentsBeenMadeAndNotYetClearedByCustoms", false, entryHeader.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);
			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("HaveAmendmentsBeenMadeAndNotYetClearedByCustoms", true, entryHeader.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("HaveAmendmentsBeenMadeAndNotYetClearedByCustoms", true, entryHeader.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			StmALog oustandingAmendmentLog = testDec.Logs.AddNew(Events.DeclarationAmendmentQueued, "TEST");
			AssertEquals("PreCondition:There is an oustanding amendment", true, testDec.HasOutstandingAmendment);
			AssertEquals("HaveAmendmentsBeenMadeAndNotYetClearedByCustoms", true, entryHeader.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);

			oustandingAmendmentLog.Cancel();
			AssertEquals("PreCondition:There is no oustanding amendment", false, testDec.HasOutstandingAmendment);
			AssertEquals("HaveAmendmentsBeenMadeAndNotYetClearedByCustoms", false, entryHeader.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);
		}

		public void TestEntryFOBWhenItIsOverridenForCFRInvoices()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.AutoCreateChargesBasedOnIncoTerm = false;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			InvoiceCharge fIFT = invoice1.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 200m;
			fIFT.J7_RX_NKCurrency = "AUD";
			fIFT.J7_IsDutiable = false;
			fIFT.J7_IsIncludedInITOT = true;

			InvoiceCharge oFT1 = invoice1.Charges.AddNew();
			oFT1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT1.J7_Amount = 500m;
			oFT1.J7_RX_NKCurrency = "AUD";
			oFT1.J7_IsIncludedInITOT = true;

			AssertEquals("JZ_Calc_FOBAmount", 9300m, invoice1.JZ_Calc_FOBAmount);
			invoice1.JZ_OverrideFOB = true;
			invoice1.EffectiveFOBAmount = 9500m;
			AssertEquals("Effective FOB amount", 9500m, invoice1.EffectiveFOBAmount);

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;

			InvoiceCharge oFT2 = invoice2.Charges.AddNew();
			oFT2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT2.J7_Amount = 800m;
			oFT2.J7_RX_NKCurrency = "AUD";
			oFT2.J7_IsIncludedInITOT = true;

			AssertEquals("Effective FOB amount for Invoice2", 19200m, invoice2.EffectiveFOBAmount);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			testDec.ResumeApportionment();
			AssertEquals("FOB for the entry", 28700m, entryHeader.FOB.Amount);
			AssertEquals("Transport And Insurance should include Non-dutiable FIFT", 1500m, entryHeader.TransportAndInsurance.Amount);
			AssertEquals("CIF should be FOB + OFT + ONS", 30000m, entryHeader.CIF.Amount);
			AssertEquals("overseas Freight should be only OverseasFreight not including non-dutiable FIFT", 1300m, entryHeader.OverseasFreight.Amount);
			AssertEquals("Deduction should be zero", 0m, entryHeader.OtherCharges2.Amount);
			AssertEquals("ForeignInland Freight should be zero", 0m, entryHeader.ForeignInlandFreight.Amount);
		}

		public void TestFOBFiguresForFOBInvoices()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.AutoCreateChargesBasedOnIncoTerm = false;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			InvoiceCharge fIFT = invoice.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 200m;
			fIFT.J7_RX_NKCurrency = "AUD";
			fIFT.J7_IsDutiable = false;//not dutiable
			fIFT.J7_IsIncludedInITOT = true;

			InvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = "AUD";

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			testDec.ResumeApportionment();
			AssertEquals("Overseas Freight for the entry does not include ForeignInlandFreight", 500m, entryHeader.OverseasFreight.Amount);
			AssertEquals("Deduction should be there to reduce Customs value as this is non-dutiable and included in ITOT", -200m, entryHeader.OtherCharges2.Amount);
			AssertEquals("Transport and insurance should include non-dutiable foreign inland freight", 700m, entryHeader.TransportAndInsurance.Amount);
			AssertEquals("FOB for the entry", 9800m, entryHeader.FOB.Amount);
			AssertEquals("CIF for the entry", 10300m, entryHeader.CIF.Amount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AssertEquals("EffectiveFOBAmount", 9800m, invoice.EffectiveFOBAmount);
			invoice.JZ_OverrideFOB = true;
			invoice.EffectiveFOBAmount = 10000m;
			AssertEquals("Deduction should not be expressed as negative", 200m, entryHeader.OtherCharges2.Amount);
			AssertEquals("EffectiveFOBAmount", 10000m, invoice.EffectiveFOBAmount);
			AssertEquals("FOB for the entry", 10000m, entryHeader.FOB.Amount);
			AssertEquals("CIF for the entry", 10500m, entryHeader.CIF.Amount);
		}

		public void TestRefreshNormalisationCheckerWhenMerging()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = invoiceLine.PK;

			NormalisationConditionChecker checkerBeforeMerge = entryHeader.NormalisationConditionChecker;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			CusEntryHeader entryAfterRemerge = testDec.CustomsEntryHeaders[0];
			AssertEquals("Checker should be refreshed", false, checkerBeforeMerge == entryAfterRemerge.NormalisationConditionChecker);
		}

		public void TestWarehouseCCPOnLineN20()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			MergedDeclarationCreator2Line creator = new MergedDeclarationCreator2Line(Factory, ZDateTime.Now);
			creator.InvoiceLine1.JI_IsPackToBondForLine = false;
			creator.InvoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).Addresses[0].PK;
			creator.InvoiceLine2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "def";
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			AssertEquals("Entry1 CCP", "DEF", creator.Entry1.WarehouseCCP.ToUpper());
		}

		public void TestWarehouseCCPComesFromDeclarationN20()
		{
			MergedDeclarationCreator2Line creator = new MergedDeclarationCreator2Line(Factory);
			creator.InvoiceLine1.JI_IsPackToBondForLine = false;
			creator.Declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).Addresses[0].PK;
			creator.Declaration.WarehouseAddress.LocalControlledPremisesID = "def";
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			AssertEquals("Entry1 CCP", "DEF", creator.Entry1.WarehouseCCP.ToUpper());
		}

		public void TestLineCCPOverridesDeclarationCCPN20()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			MergedDeclarationCreator2Line creator = new MergedDeclarationCreator2Line(Factory, ZDateTime.Now);
			creator.InvoiceLine1.JI_IsPackToBondForLine = false;
			creator.Declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).Addresses[0].PK;
			creator.Declaration.WarehouseAddress.LocalControlledPremisesID = "def";
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			creator.InvoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).Addresses[0].PK;
			creator.InvoiceLine2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "abc";
			AssertEquals("Entry1 CCP", "ABC", creator.Entry1.WarehouseCCP.ToUpper());
		}

		public void TestWarehouseCCPOnLineForN30()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory, ZDateTime.Now);
			creator.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			creator.InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).Addresses[0].PK;
			creator.InvoiceLine1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "def";
			AssertEquals("Entry1 CCP", "DEF", creator.Entry1.WarehouseCCP.ToUpper());
		}

		public void TestWarehouseCCPComesFromDeclarationForN30()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			creator.Declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).Addresses[0].PK;
			creator.Declaration.WarehouseAddress.LocalControlledPremisesID = "def";
			AssertEquals("Entry1 CCP", "DEF", creator.Entry1.WarehouseCCP.ToUpper());
		}

		public void TestLineCCPOverridesDeclarationCCPForN30()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory, ZDateTime.Now);
			creator.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			creator.Declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).Addresses[0].PK;
			creator.Declaration.WarehouseAddress.LocalControlledPremisesID = "def";
			creator.InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).Addresses[0].PK;
			creator.InvoiceLine1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "abc";
			AssertEquals("Entry1 CCP", "ABC", creator.Entry1.WarehouseCCP.ToUpper());
		}

		public void TestIsSubjectToDutyOrGSTAndRelatedFlags()
		{
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);

			Env.Registry.CMRTestMode = true;
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = deminimus;

			AssertEquals("IsOverThreshold", false, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("not subject to duty and tax as this is under threshold for N10", false, entryHeader.IsSubjectToDutyAndTax);

			testDec.JE_MessageType = "EXW";
			AssertEquals("IsOverThreshold", false, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("IsNature 30", true, entryHeader.IsNature30);
			AssertEquals("subject to duty and tax as this is N30", true, entryHeader.IsSubjectToDutyAndTax);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryLine.CL_CustomsValue = deminimus + 1;
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("Is over threshold", true, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("SAC with lines", false, entryHeader.IsSACWithLine);
			AssertEquals("Subject to duty and tax", true, entryHeader.IsSubjectToDutyAndTax);

			testDec.JE_MessageSubType = "SAC";
			entryLine.CL_CustomsValue = deminimus;
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("Is not over threshold", false, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("SAC with lines", false, entryHeader.IsSACWithLine);
			AssertEquals("SAC without lines", true, entryHeader.IsSACWithoutLine);
			AssertEquals("not Subject to duty and tax as this is SAC", false, entryHeader.IsSubjectToDutyAndTax);

			entryHeader.fQuestions = null;
			CMRCusEntryCPDec question375 = entryHeader.Questions.AddNew();
			question375.ON_CPDecNum = 375;
			question375.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			entryLine.CL_CustomsValue = deminimus;
			entryHeader.ResetTotalsAndCachedValues();

			AssertEquals("IsOverThreshold", false, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("not subject to duty and tax as this is under threshold for N10", false, entryHeader.IsSubjectToDutyAndTax);

			entryHeader.fQuestions = null;
			question375.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			AssertEquals("IsOverThreshold", false, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("Is subject to duty and tax as this is under threshold for N10 but override cp dec question specified", true, entryHeader.IsSubjectToDutyAndTax);

			testDec.AddInfo.ZA_UPEIndicator_Hidden = true;
			AssertEquals("Entry is over the Threshold", false, entryHeader.IsGoodsValueOfEntryOverThreshold);
			AssertEquals("Entry is now subject to duty and tax as the entry is for Unaccompanied Personal Effects", true, entryHeader.IsSubjectToDutyAndTax);
		}

		public void TestTotalAmountPayableForThisSession()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_TotalPaid = 10000m;

			AssertEquals("Total payable in the message", 191.90m, new OutstandingAmountRetriever(iMDRMessage).OutstandingAmount);
			AssertEquals("IsStatusPostLodge", true, entryHeader.IsStatusPostLodge);
			AssertEquals("TotalAmountPayableForThisSession", 191.90m, entryHeader.TotalAmountPayableForThisSession);

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals("IsStatusPostLodge", false, entryHeader.IsStatusPostLodge);
			AssertEquals("No lodgement is done", entryHeader.CH_TotalPaid, entryHeader.TotalAmountPayableForThisSession);
		}

		public void TestTotalAmountPayableForCMR()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 100m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 110m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 120m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 130m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 140m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 150m);

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 200m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 210m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 220m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 230m);

			AssertEquals("All Entry fees", 650m, entryHeader.AllEntryFees);
			AssertEquals("Total Line payable", 860m, entryHeader.MergedLines.TotalLinePayable);
			AssertEquals("Total payable", 1610m, entryHeader.TotalAmountPayable);
		}

		public void TestTotalPayableAdvisedWhenPaid()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals("Total payable in the message", 191.90m, new OutstandingAmountRetriever(iMDRMessage).OutstandingAmount);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			entryHeader.isTotalPayableAdvisedInLastClearanceMessageCalculated = false;
			AssertEquals("IsPaymentMessageSentOrCleared", true, CMREntryPaymentStatusList.IsPaymentMessageSentOrCleared(entryHeader.AddInfo.ZA_PaymentStatus_Hidden));
			AssertEquals("Total payable in the message", 191.90m, entryHeader.TotalPayableAdvisedInLastClearanceMessage);
			AssertEquals("TotalPayableAdvised from Entry Header", 0m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Refunded;
			entryHeader.isTotalPayableAdvisedInLastClearanceMessageCalculated = false;
			AssertEquals("IsPaymentMessageSentOrCleared", true, CMREntryPaymentStatusList.IsPaymentMessageSentOrCleared(entryHeader.AddInfo.ZA_PaymentStatus_Hidden));
			AssertEquals("Total payable in the message", 191.90m, entryHeader.TotalPayableAdvisedInLastClearanceMessage);
			AssertEquals("TotalPayableAdvised from Entry Header", 0m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			entryHeader.isTotalPayableAdvisedInLastClearanceMessageCalculated = false;
			AssertEquals("IsPaymentMessageSentOrCleared", true, CMREntryPaymentStatusList.IsPaymentMessageSentOrCleared(entryHeader.AddInfo.ZA_PaymentStatus_Hidden));
			AssertEquals("Total payable in the message", 191.90m, entryHeader.TotalPayableAdvisedInLastClearanceMessage);
			AssertEquals("TotalPayableAdvised from Entry Header", 0m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayRejected;
			entryHeader.isTotalPayableAdvisedInLastClearanceMessageCalculated = false;
			AssertEquals("IsPaymentMessageSentOrCleared", false, CMREntryPaymentStatusList.IsPaymentMessageSentOrCleared(entryHeader.AddInfo.ZA_PaymentStatus_Hidden));
			AssertEquals("Total payable in the message", 191.90m, entryHeader.TotalPayableAdvisedInLastClearanceMessage);
			AssertEquals("TotalPayableAdvised from Entry Header", 191.90m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);
		}

		public void TestCalculatedNetTotalLinesAmountDue()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ExportDate = ZDateTime.Today;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine3 = (CusEntryLine)entryHeader.PendingDeletionEntryLines.AddNew();

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 200m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 210m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 220m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 230m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 900m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 500m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 600m);
			entryLine3.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 100m);

			AssertEquals("CalculatedNetTotalLinesAmountDue", -240m, entryHeader.CalculatedNetTotalLinesAmountDue);
			Assert("IsARefundDue", entryHeader.IsARefundDue);

			entryHeader.ResetCachedValuesForDutyCalculation();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 200m);
			Assert("IsARefundDue", !entryHeader.IsARefundDue);
		}

		public void TestIsStatusPostLodge()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.CH_Status = "";
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.FailPreLodge.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals(false, header.IsStatusPostLodge);

			header.CH_Status = CustomsEntryStatus.FailWithdrawal.Code;
			AssertEquals(true, header.IsStatusPostLodge);

			header.CH_Status = "ZZZ";
			AssertEquals(true, header.IsStatusPostLodge);

			header.CH_Status = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(false, header.IsStatusPostLodge);
		}

		public void TestCPDecQuestionViewType()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			CMRCusEntryCPDec questionAnswered = entryLine.Questions.AddNew();
			questionAnswered.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec questionUnAnswered = entryLine.Questions.AddNew();

			entryHeader.CPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.All;
			AssertEquals("there should be two questions", 2, entryHeader.CPDecQuestionsViewCollection.Count);

			entryHeader.CPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.Unanswered;
			AssertEquals("there should be One questions", 1, entryHeader.CPDecQuestionsViewCollection.Count);
			AssertEquals("there should be One questions", questionUnAnswered, entryHeader.CPDecQuestionsViewCollection[0]);

			entryHeader.CPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.Answered;
			AssertEquals("there should be One questions", 1, entryHeader.CPDecQuestionsViewCollection.Count);
			AssertEquals("there should be One questions", questionAnswered, entryHeader.CPDecQuestionsViewCollection[0]);
		}

		public void TestIsGoodsDelivered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question14 = entryHeader.Questions.AddNew();
			question14.ON_CPDecNum = 14;
			question14.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("IsGoodsDelivered", !entryHeader.IsGoodsDelivered);
			question14.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			Assert("IsGoodsDelivered", entryHeader.IsGoodsDelivered);
		}

		public void TestIsFeePaidByBroker()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);

			AssertEquals("XXX is paid by broker.", true, entry.IsFeePaidByBroker("XXX", "", null));
			AssertEquals("GSD is not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.GSTDeferred, "", null));
			AssertEquals("DTD is not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyDeferredAmount, "", null));

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			AssertEquals("DTY is paid by broker cuz no duty is deferred.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 99m);
			AssertEquals("DTY is still paid by broker as not all duty is deferred.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));

			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			AssertEquals("DTY is still 'paid by broker'. A zero value charge is ignored.", true, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));

			entry.Declaration.JE_PaymentMethod = "ZZZ";
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 0m);
			entry.Charges.SetAmount(CusEntryChargeTypeList.Codes.GSTDeferred, 0m);
			AssertEquals("XXX is not paid by broker cuz the payment method is not BRK.", false, entry.IsFeePaidByBroker("XXX", "", null));
			AssertEquals("DTY is not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyAmount, "", null));
			AssertEquals("DTD is not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.DutyDeferredAmount, "", null));
			AssertEquals("GSD is not paid by broker.", false, entry.IsFeePaidByBroker(CusEntryChargeTypeList.Codes.GSTDeferred, "", null));
		}

		public void TestStatusChangedToClearedSinceLoadingUnderCMRWithATD()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MergedDeclarationCreator creator = new MergedDeclarationCreator(factory);
			creator.Declaration.FillWithValidTestData();
			creator.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var entry = creator.Entry1;
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.NotPaid;
			var statusChangedToClearedSinceLoadingProperty = entry.GetType().GetProperty(
				"StatusChangedToClearedSinceLoading",
				BindingFlags.GetProperty |
				BindingFlags.NonPublic |
				BindingFlags.Instance);
			AssertEquals("StatusChangedToCleared", false, statusChangedToClearedSinceLoadingProperty.GetValue(entry));
			factory.Save();

			AssertEquals("StatusChangedToCleared", false, statusChangedToClearedSinceLoadingProperty.GetValue(entry));
			creator.Entry1.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			AssertEquals("StatusChangedToCleared", true, statusChangedToClearedSinceLoadingProperty.GetValue(entry));
		}

		public void TestStatusChangedToClearedSinceLoadingUnderCMRWithPayResponse()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MergedDeclarationCreator creator = new MergedDeclarationCreator(factory);
			creator.Declaration.FillWithValidTestData();
			creator.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var entry = creator.Entry1;
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.NotPaid;
			var statusChangedToClearedSinceLoadingProperty = entry.GetType().GetProperty(
				"StatusChangedToClearedSinceLoading",
				BindingFlags.GetProperty |
				BindingFlags.NonPublic |
				BindingFlags.Instance);

			AssertEquals("StatusChangedToCleared", false, statusChangedToClearedSinceLoadingProperty.GetValue(entry));
			factory.Save();

			AssertEquals("StatusChangedToCleared", false, statusChangedToClearedSinceLoadingProperty.GetValue(entry));
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			AssertEquals("StatusChangedToCleared", true, statusChangedToClearedSinceLoadingProperty.GetValue(entry));

			factory.Save();
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			AssertEquals("StatusChangedToCleared", false, statusChangedToClearedSinceLoadingProperty.GetValue(entry));

			factory.Save();
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			AssertEquals("StatusChangedToCleared", true, statusChangedToClearedSinceLoadingProperty.GetValue(entry));
		}

		public void TestIsStatusChangingToClearedPreCMR()
		{
			EntryHeader.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			AssertEquals("IsStatusChangingToCleared()", true, EntryHeader.IsStatusChangingToClearedInternal(EntryHeaderStatus.NotSent.Code, EntryHeaderStatus.PayMessageCleared.Code));
			AssertEquals("IsStatusChangingToCleared()", false, EntryHeader.IsStatusChangingToClearedInternal(EntryHeaderStatus.NotSent.Code, EntryHeaderStatus.ReadyForPayment.Code));
		}

		public void TestStatusChangedToHeldSinceLoading()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MergedDeclarationCreator creator = new MergedDeclarationCreator(factory);
			creator.Declaration.FillWithValidTestData();
			creator.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var entry = creator.Entry1;
			entry.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			var statusChangedToHeldSinceLoadingProperty = entry.GetType().GetProperty(
				"StatusChangedToHeldSinceLoading",
				BindingFlags.GetProperty |
				BindingFlags.NonPublic |
				BindingFlags.Instance);
			AssertEquals("Status set to Clear before save", false, statusChangedToHeldSinceLoadingProperty.GetValue(entry));
			factory.Save();
			AssertEquals("Status is Clear after save", false, statusChangedToHeldSinceLoadingProperty.GetValue(entry));
			creator.Entry1.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("Status changed to Held", true, statusChangedToHeldSinceLoadingProperty.GetValue(entry));
			factory.Save();
			AssertEquals("Status is Held after save", false, statusChangedToHeldSinceLoadingProperty.GetValue(entry));
		}

		public void TestCustomsFactorForNormalised()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 12);

			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.7326m, helper.USDCurrency);

			testDec.JE_AddInfo = "ForcePrimeEnclosureIfMultipleEntries_Hidden=Y*NumberOfEntryPrints_Hidden=2*EFTReceiptPrinter_Hidden=06976*PrinterNumber_Hidden=06976*ClearanceAdvicePrinter_Hidden=06976";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.AIR;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2004, 10, 30);
			testDec.JE_DateAtOrigin = new ZDateTime(2004, 10, 12);
			testDec.JE_DateOfArrival = new ZDateTime(2004, 10, 30);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2004, 10, 30);
			testDec.JE_DeclarationReference = "S00040547";
			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 4, 8);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "DIVING EQUIPMENT";
			testDec.JE_MasterBill = "08155555555";
			testDec.JE_MergeBy = "TRF";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OwnerRef = "45165";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "HKHKG";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "HKHKG";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2005, 4, 7);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2005, 4, 8);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 422;
			testDec.JE_TotalNoOfPacksPackType = "CTN";
			testDec.JE_TotalVolume = 27.950m;
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 2537.200m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF455";

			JobComInvoiceHeader testHeader45165 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader45165.JZ_AddInfo = "PRF=X*ORG=CN*PackCountForNature10_Hidden=422*ValuationBasis_Hidden=UT";
			testHeader45165.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader45165.JZ_InvoiceAmount = 1328.7600m;
			testHeader45165.JZ_InvoiceCurrExRate = 0.732600000m;
			testHeader45165.JZ_InvoiceDate = new ZDateTime(2004, 10, 27);
			testHeader45165.JZ_InvoiceNumber = "45165";
			testHeader45165.JZ_RX_NKInvoice_Currency = "USD";
			testHeader45165.JZ_Weight = 2537.200m;
			testHeader45165.JZ_WeightUQ = "KG";

			JobComInvoiceHeader testHeaderA = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeaderA.JZ_AddInfo = "ORG=CN*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=UT";
			testHeaderA.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeaderA.JZ_InvoiceAmount = 20622.7000m;
			testHeaderA.JZ_InvoiceCurrExRate = 0.732600000m;
			testHeaderA.JZ_InvoiceDate = new ZDateTime(2005, 4, 7);
			testHeaderA.JZ_InvoiceNumber = "A";
			testHeaderA.JZ_RX_NKInvoice_Currency = "USD";
			testHeaderA.JZ_Weight = 10.000m;
			testHeaderA.JZ_WeightUQ = "KG";

			JobComInvoiceLine testLine1 = testHeader45165.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "InstrumentCode_Hidden=346957*InstrumentType_Hidden=MD1*PRF=X*ORG=CN*TreatmentCode_Hidden=821*ValuationBasis_Hidden=TV";
			testLine1.JI_ConcessionOrder = "MD1|346957";
			testLine1.JI_Description = "BAGS";
			testLine1.JI_InvoiceUQ = "PKG";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 306.0000m;
			testLine1.JI_CountryOfOrigin = "CN";
			testLine1.JI_Tariff = "4202.92.90 42";
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader45165.JobComInvoiceLines.AddNew();
			testLine2.JI_AddInfo = "InstrumentCode_Hidden=346957*InstrumentType_Hidden=MD1*PRF=X*ORG=CN*TreatmentCode_Hidden=821*ValuationBasis_Hidden=TV";
			testLine2.JI_ConcessionOrder = "MD1|346957";
			testLine2.JI_Description = "DIVE FINS";
			testLine2.JI_InvoiceUQ = "NO";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 976.8000m;
			testLine2.JI_CountryOfOrigin = "CN";
			testLine2.JI_Tariff = "9506.29.00 49";
			testLine2.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine3 = testHeaderA.JobComInvoiceLines.AddNew();
			testLine3.JI_AddInfo = "PRF=X*ORG=CN*ValuationBasis_Hidden=TV";
			testLine3.JI_CustomsQuantity = 16.0000m;
			testLine3.JI_CustomsUnitQty = "KG";
			testLine3.JI_Description = "BOXES OF OTHER PLASTICS";
			testLine3.JI_InvoiceQuantity = 16.00000m;
			testLine3.JI_InvoiceUQ = "KG";
			testLine3.JI_LineNo = (short)2;
			testLine3.JI_LinePrice = 881.0000m;
			testLine3.JI_CountryOfOrigin = "CN";
			testLine3.JI_Tariff = "3923.10.00 58";
			testLine3.JI_Weight = 16.000m;
			testLine3.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeaderA.JobComInvoiceLines.AddNew();
			testLine4.JI_AddInfo = "InstrumentCode_Hidden=346957*InstrumentType_Hidden=MD1*PRF=X*ORG=CN*TreatmentCode_Hidden=821*ValuationBasis_Hidden=TV";
			testLine4.JI_ConcessionOrder = "MD1|346957";
			testLine4.JI_Description = "DIVE MASKS";
			testLine4.JI_LineNo = (short)1;
			testLine4.JI_LinePrice = 2901.5000m;
			testLine4.JI_CountryOfOrigin = "CN";
			testLine4.JI_Tariff = "9506.29.00 49";
			testLine4.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine5 = testHeaderA.JobComInvoiceLines.AddNew();
			testLine5.JI_AddInfo = "InstrumentCode_Hidden=346957*InstrumentType_Hidden=MD1*PRF=X*ORG=CN*TreatmentCode_Hidden=821*ValuationBasis_Hidden=TV";
			testLine5.JI_ConcessionOrder = "MD1|346957";
			testLine5.JI_Description = "DIVE MASKS & SNORKELS";
			testLine5.JI_LineNo = (short)3;
			testLine5.JI_LinePrice = 16840.2000m;
			testLine5.JI_CountryOfOrigin = "CN";
			testLine5.JI_Tariff = "9506.29.00 49";
			testLine5.JI_WeightUQ = "KG";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("Customs Factor", 1.36500137m, entryHeader.CustomsFactor);
		}

		public void TestTotalCustomsValueAndCustomsFactorWithAdjustment()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 14173m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 25994.79m, "AUD");
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 35.43m, "AUD");

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 14173m;
			invoiceLine.JI_Tariff = "7616990029";
			invoiceLine.JI_CountryOfOrigin = "ZA";
			invoiceLine.AddInfo.ZA_ADJ = "200AUD";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];

			AssertEquals("Customs value for line", 14373m, entryLine.CL_CustomsValue);
			AssertEquals("Customs value for header", 14373m, entryHeader.CustomsValueInAUD.Amount);
			AssertEquals("Customs factor", 1m, entryHeader.CustomsFactor);
		}

		//ToDo: Deb
		//			[ToDo("Joo, Deb", "29-04-05", "Work In Progress")]
		//			public void TestRecalculate_AreThereInvoicesWithDifferentIncoTermOrCurrencyOrInvoiceLevelCharge()
		//			{
		//				Assert("If we recycle EntryHeader, I need to set the flag Recalculate_AreThereInvoicesWithDifferentIncoTermOrCurrencyOrInvoiceLevelCharge to true again before sending an amendment message and Deb needs to put in some code to remerge when the flag needs to be set", false);
		//			}

		public void TestHasGST()
		{
			Assert("Initial state", !EntryHeader.HasGST);
			EntryHeader.MergedLines.AddNew().Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 5);

			Assert("Has gst", EntryHeader.HasGST);
			EntryHeader.MergedLines.RemoveAll();
			Assert("No gst", !EntryHeader.HasGST);
		}

		public void TestHasDeferredGST()
		{
			Assert("Initial state", !EntryHeader.HasDeferredGST);
			EntryHeader.MergedLines.AddNew().Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTDeferred, 5);

			Assert("Has gst", EntryHeader.HasDeferredGST);
			EntryHeader.MergedLines.RemoveAll();
			Assert("No gst", !EntryHeader.HasDeferredGST);
		}

		public void TestErrorLineNumbersFromLastResponseMessageWhenNoResponse()
		{
			var errors = (short[])EntryHeader.GetType()
						.InvokeMember("ErrorLineNumbersFromLastResponseMessage",
						BindingFlags.InvokeMethod |
						BindingFlags.NonPublic |
						BindingFlags.Instance,
						null,
						EntryHeader,
						null);
			AssertEquals(0, errors.Length);
		}

		public void TestErrorLineNumbersFromLastResponseMessageWhenCMRCUSRES()
		{
			EntryHeader.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			EDIMessage responseWithOneLineError = GetCreateCMRRejectionMessage();
			EntryHeader.Messages.Add(responseWithOneLineError);
			AssertEquals("Line 2 has error", true, EntryHeader.HasMessageResponseError(2));
			AssertEquals("Line 1 has no error", false, EntryHeader.HasMessageResponseError(1));
		}

		public void TestNoCMRLineInError_NoInvoiceLineWillBeHighlighted()
		{
			EntryHeader.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			EDIMessage responseWithNoLineError = GetCreateCMRRejectionMessageWithNoLineInError();
			EntryHeader.Messages.Add(responseWithNoLineError);
			AssertEquals("No Line has error", false, EntryHeader.HasMessageResponseError(1));
			AssertEquals("No Line has error", false, EntryHeader.HasMessageResponseError(2));
		}

		public void TestStatusProvider()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Status Provider should be an CMR one", true, entryHeader.StatusProvider is CMRStatusProvider);
		}

		public void TestCusEntryNumber()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			entryHeader.EntryNumber = "TST001";
			AssertEquals("CusEntryNumber type is CAN", "CAN", entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("CH_MessageType matches entry type", "CAN", entryHeader.CH_MessageType);
			AssertEquals("CH_MessageTypeDescription matches entry type", "Customs Authority Number", entryHeader.CH_MessageTypeDescription);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader.EntryNumber = "TST002";
			AssertEquals("CusEntryNumber type matches message type", "IMP", entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("CH_MessageType", "", entryHeader.CH_MessageType);
			AssertEquals("CH_MessageTypeDescription", "", entryHeader.CH_MessageTypeDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
		}

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Factory.New<CusEntryHeader>();
					fEntryHeader.SetDeclarationForTesting(JobDeclaration.New(Factory));
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		EDIMessage GetCreateCMRRejectionMessage()
		{
			EDIMessage result = GetCreateResponse();
			result.EM_MessageText = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("CMRCUSRESRejection.txt")).Replace("\r\n", "");
			result.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			return result;
		}

		EDIMessage GetCreateResponse()
		{
			EDIMessage result = GetResponse();
			result.EM_MessageType = "CMR";  //HACK: Check?
			return result;
		}

		EDIMessage GetResponse()
		{
			EDIMessage result = Factory.New<EDIMessage>();
			result.EM_Status = EDIMessage.Status.Received;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			return result;
		}

		EDIMessage GetCreateCMRRejectionMessageWithNoLineInError()
		{
			EDIMessage result = GetCreateResponse();
			result.EM_MessageText = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("CMRCUSRESRejectionWithNoLineInError.txt")).Replace("\r\n", "");
			result.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			return result;
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.CMR.TestFiles." + fileName;
	}
}
