using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
{
	public void TestHumanReadableName() => AssertEquals("Customs Entry Header", Factory.New<CusEntryHeader>().HumanReadableName);

	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		var oHeader = Factory.New<CusEntryHeader>();
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(oHeader,
			"CHCusEntryHeader",
			schemaTypeName: nameof(AutoCusEntryHeader.Schema));
	}

	public void TestTotalDutyAmount_Interfaced()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;

		var entry = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry.MergedLines.AddNew();
		entryLine1.Fees.GetOrAddFeeByFeeType(FeeTypeList.Codes.A00).CF_ChargeAmount = 200m;

		var entryLine2 = entry.MergedLines.AddNew();
		entryLine2.Fees.GetOrAddFeeByFeeType(FeeTypeList.Codes.A00).CF_ChargeAmount = 300m;
		entryLine2.Fees.GetOrAddFeeByFeeType(FeeTypeList.Codes.B00).CF_ChargeAmount = 100m;

		AssertEquals("Total Duty Amount", 500m, entry.TotalDutyAmount);
	}

	public override void TestTotalDutyAmount()
	{
		var entry = ImportJobDeclaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry.MergedLines.AddNew();
		entryLine1.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 200m;

		var entryLine2 = entry.MergedLines.AddNew();
		entryLine2.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 300m;
		entryLine2.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.VAT).CF_ChargeAmount = 100m;

		AssertEquals("Total Duty Amount", 500m, entry.TotalDutyAmount);
	}

	public override void TestEntriesOfHouseBillsRefreshed()
	{
		ImportJobDeclaration.FillWithValidTestData();

		var invoice1 = ImportJobDeclaration.Invoices.AddNew();
		var invoice2 = ImportJobDeclaration.Invoices.AddNew();
		SetInvoicesToResultInTwoEntries(invoice1.JobComInvoiceLines.AddNew(), invoice2.JobComInvoiceLines.AddNew());

		var bill1 = ImportJobDeclaration.Bills.AddNew();
		var bill2 = ImportJobDeclaration.Bills.AddNew();

		invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
		invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

		DoMerge(ImportJobDeclaration);

		var entry1 = (CusEntryHeader)invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
		var entry2 = (CusEntryHeader)invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
		AssertEquals(true, entry1 != entry2);
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill1.Entries).Contains(entry1));
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill2.Entries).Contains(entry2));

		invoice1.JZ_CU_RelatedHouseBill = bill2.PK;
		invoice2.JZ_CU_RelatedHouseBill = bill1.PK;

		DoMerge(ImportJobDeclaration);

		entry1 = (CusEntryHeader)invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
		entry2 = (CusEntryHeader)invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
		AssertEquals(true, entry1 != entry2);
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill1.Entries).Contains(entry2));
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill2.Entries).Contains(entry1));
	}

	public void TestEntryHeaderStatusDescription()
	{
		RefCusCodeTestHelper.CreateCustomsStatusCodeListAndFrenchLanguage(Factory);

		var entry = ImportJobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("EntryHeaderStatusDescription is set to default value", ZString.Empty, entry.EntryHeaderStatusDescription);

			entry.CH_EntryStatus = RefCusCodeTestHelper.EntryStatusCode;
			AssertEquals("EntryHeaderStatusDescription has the default english description", "english description", entry.EntryHeaderStatusDescription);

			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR";
			AssertEquals("EntryHeaderStatusDescription has the RefCusCodeListLanguage description", "description francaise", entry.EntryHeaderStatusDescription);
		});
	}

	public void TestMessageStatusDescription()
	{
		var entry = ImportJobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			entry.CH_Status = ZString.Empty;
			AssertEquals("CH_Status is empty", "Not Sent", entry.MessageStatusDescription);

			entry.CH_Status = CHLogicalStatusList.Codes.Accepted;
			AssertEquals("CH_Status not empty", CHLogicalStatusList.Descriptions.Accepted, entry.MessageStatusDescription);
		});
	}

	public void TestPhaseStatusCaption() => CaptionTestHelper.AssertCaptions(ExportJobDeclaration.CustomsEntryHeaders.AddNew().CH_PhaseStatusInfo, caption: "Phase Status", shortCaption: "Ph. Status");

	public void TestPhaseStatusDescriptionCaption() => CaptionTestHelper.AssertCaptions(ExportJobDeclaration.CustomsEntryHeaders.AddNew().PhaseStatusDescriptionInfo, caption: "Phase Status Description", fullDescription: "Description of the Phase Status", mediumCaption: "Phase Status Desc.", shortCaption: "Ph. Status Desc.");

	public void TestPhaseStatusDescription() => CombineAssertions(() =>
	{
		var entry = ExportJobDeclaration.CustomsEntryHeaders.AddNew();

		AssertEquals("CH_PhaseStatus is empty.", ZString.Empty, entry.PhaseStatusDescription);

		entry.CH_PhaseStatus = "XYZ";
		AssertEquals("Unknown CH_PhaseStatus.", ZString.Empty, entry.PhaseStatusDescription);

		entry.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
		AssertEquals($"CH_PhaseStatus is '{entry.CH_PhaseStatus}'.", PassarDeclarationPhaseList.Descriptions.Amendment, entry.PhaseStatusDescription);
	});

	public void TestSelectionResultDescription() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateSelectionResultCodeListAndFrenchLanguage(Factory);

		var entry = ImportJobDeclaration.CustomsEntryHeaders.AddNew();

		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		cusEntryNumber.CE_ParentID = entry.PK;
		cusEntryNumber.CE_ParentTable = entry.TableName;
		cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cusEntryNumber.CE_EntryNum = "123456";
		cusEntryNumber.CE_IssueDate = ZDateTime.Today;

		cusEntryNumber.CE_EntryStatus = ZString.Empty;
		AssertEquals("SelectionResultDescription should be empty", ZString.Empty, entry.SelectionResultDescription);

		cusEntryNumber.CE_EntryStatus = RefCusCodeTestHelper.EntryStatusCode;
		AssertEquals("SelectionResultDescription has the default english description", RefCusCodeTestHelper.EntryStatusDescription, entry.SelectionResultDescription);

		GlbStaff.CurrentUser.GS_WorkingLanguage = "FR";
		AssertEquals("SelectionResultDescription has the RefCusCodeListLanguage description", "description francaise", entry.SelectionResultDescription);
	});

	public void TestAccessCode()
	{
		var entry = ImportJobDeclaration.CustomsEntryHeaders.AddNew();
		AssertEquals("Access Code Field of Entry Header is empty", ZString.Empty, entry.AccessCode);

		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Switzerland.AccessCode;
		cusEntryNumber.CE_ParentID = entry.PK;
		cusEntryNumber.CE_ParentTable = entry.TableName;
		cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cusEntryNumber.CE_EntryNum = "123456";
		cusEntryNumber.CE_IssueDate = ZDateTime.Today;

		AssertEquals("Access Code Field of Entry Header has the EntryNum value of the ACC EntryLine", cusEntryNumber.CE_EntryNum, entry.AccessCode);
	}

	public void TestAccessCodeSetter()
	{
		var accessCode = "123456";
		var brettsBirthday = ZDateTime.BrettsBirthday;

		var entry = ImportJobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertNull("AccessCodeCusEntryNumber is null", entry.AccessCodeCusEntryNumber);
			entry.AccessCodeSetter(accessCode, brettsBirthday);
			AssertEquals("AccessCodeCusEntryNumber.CE_EntryNum", accessCode, entry.AccessCodeCusEntryNumber.CE_EntryNum);
			AssertEquals("AccessCodeCusEntryNumber.CE_IssueDate", brettsBirthday, entry.AccessCodeCusEntryNumber.CE_IssueDate);
		});
	}

	public void TestPopulateCH_BGMReferenceIfRequired()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();

		AssertNullOrEmpty("Before OnSaving CH_BGMReference should be empty", entryHeader1.CH_BGMReference);
		Factory.Save();
		AssertNotNullOrEmpty("After OnSaving CH_BGMReference should not be empty", entryHeader1.CH_BGMReference);
		var bgmReference = entryHeader1.CH_BGMReference;
		entryHeader1.CH_CustomsMessageRemarks = "XXX";
		Factory.Save();
		AssertNotNullOrEmpty("After OnSaving CH_BGMReference should not be empty", entryHeader1.CH_BGMReference);
		entryHeader1.CH_CustomsMessageRemarks = "YYY";
		Factory.Save();
		AssertEquals("CH_BGMReference should not change his value", bgmReference, entryHeader1.CH_BGMReference);

		var prefix = entryHeader1.CH_BGMReference.Substring(0, 9);
		AssertEquals("Prefix should contain EnterpriseCode, CompanyCode and ServerCode", GlbCompany.CurrentCompany.LicenceKeyIdentifier, prefix);

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		Factory.Save();
		var bgmReference2 = entryHeader2.CH_BGMReference;
		AssertNotEquals("EntryHeader1.CH_BGMReference should be different from EntryHeader2.CH_BGMReference", bgmReference, bgmReference2);

		var seq1 = int.Parse(entryHeader1.CH_BGMReference.Substring(9, 9));
		var seq2 = int.Parse(entryHeader2.CH_BGMReference.Substring(9, 9));
		AssertEquals("Verify that the two sequence numbers are consecutive", seq1 + 1, seq2);
	}

	public void TestLogStatusChangeIfChanged()
	{
		var status1 = "111";
		var status2 = "222";
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions(() =>
		{
			entryHeader.CH_Status = status1;
			Factory.Save();
			AssertNull(entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange));

			entryHeader.CH_Status = status2;
			Factory.Save();
			AssertEquals(1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.MessageStatusChange).Count());
			AssertEquals("SL_Reference", $"|NEW={status2}|OLD={status1}", entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange)?.SL_Reference);

			entryHeader.CH_Status = status1;
			entryHeader.Logs.AddNew(Events.MessageStatusChange, $"|NEW={status1}|OLD={status2}");
			Factory.Save();
			AssertEquals(2, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.MessageStatusChange).Count());
			AssertEquals("SL_Reference", $"|NEW={status1}|OLD={status2}", entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange)?.SL_Reference);

			entryHeader.CH_Status = status2;
			Factory.Save();
			AssertEquals(3, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.MessageStatusChange).Count());
			AssertEquals("SL_Reference", $"|NEW={status2}|OLD={status1}", entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange)?.SL_Reference);
		});
	}

	public void TestShouldLogEntryStatus()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals(true, entryHeader.ShouldLogEntryStatus);
	}

	public void TestEComMessages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = entryHeader.Messages.AddNew();

		AssertEquals("Should be empty", 0, entryHeader.EComMessages.Count);

		message.EM_MessageType = MessageTypeCodeList.Codes.ECM;
		AssertEquals("Shouldn't be empty", 1, entryHeader.EComMessages.Count);

		AssertType<EComCHEDIMessageCollectionView>(entryHeader.EComMessages);
		AssertType<CHEDIMessage>(entryHeader.EComMessages.FirstOrDefault());
	}

	public void TestNonEComMessages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = MessageTypeCodeList.Codes.ECM;
		AssertEquals("Should be empty", 0, entryHeader.NonEComMessages.Count);

		message.EM_MessageType = "IMP";
		AssertEquals("Shouldn't be empty", 1, entryHeader.NonEComMessages.Count);

		AssertType<NonEComCHEDIMessageCollectionView>(entryHeader.NonEComMessages);
		AssertType<CHEDIMessage>(entryHeader.NonEComMessages.FirstOrDefault());
	}

	public void TestEComMessageStatusDescription()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		CombineAssertions(() =>
		{
			entryHeader.CH_LastEComplaintStatus = ZString.Empty;
			AssertEquals("LastEComplaintStatus blank", "Not Sent", entryHeader.EComMessageStatusDescription);

			entryHeader.CH_LastEComplaintStatus = "XXX";
			AssertEquals("LastEComplaintStatus invalid", "Unknown", entryHeader.EComMessageStatusDescription);

			entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Sent;
			AssertEquals("LastEComplaintStatus SNT", "Sent", entryHeader.EComMessageStatusDescription);
		});
	}

	public void TestGetLatestDeclarationVersion()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.EntryNumber = "";
		AssertEquals("LatestDeclarationVersion 0", 0, entryHeader.GetLatestDeclarationVersion());

		entryHeader.EntryNumber = "21CHEI000042027074";
		AssertEquals("LatestDeclarationVersion 0", 0, entryHeader.GetLatestDeclarationVersion());

		entryHeader.EntryNumber = "21CHEI000042027074.3";
		AssertEquals("LatestDeclarationVersion 3", 3, entryHeader.GetLatestDeclarationVersion());

		entryHeader.EntryNumber = "21CHEI000042027074.3.1";
		AssertEquals("LatestDeclarationVersion 3", 3, entryHeader.GetLatestDeclarationVersion());
	}

	public void TestCloseEComplaint()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.NotSent;
			TestInvalidStatus("There is no ECom open.");

			entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Sent;
			TestInvalidStatus("Cannot close, a response is pending.");

			entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Closed;
			TestInvalidStatus("The ECom has already been closed.");

			entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Received;
			var message = entryHeader.CloseEComplaint();
			AssertEquals("Sucess: Result message", ZString.Empty, message);
			AssertEquals("Sucess: Status changed", EComplaintStatusList.Codes.Closed, entryHeader.CH_LastEComplaintStatus);
			AssertEquals("Sucess: Event count", 1, entryHeader.Logs.LogsNotInDB.Length);
			var loggedEvent = entryHeader.Logs.LogsNotInDB.LastOrDefault();
			AssertEquals("Event code", Events.EComStatusChange.Code, loggedEvent?.SL_SE_NKEvent);
			AssertEquals("Event reference", $"|NEW={EComplaintStatusList.Codes.Closed}|OLD={EComplaintStatusList.Codes.Received}", loggedEvent?.SL_Reference);
		});

		void TestInvalidStatus(string expectedMessage)
		{
			var oldStatus = entryHeader.CH_LastEComplaintStatus;
			var message = entryHeader.CloseEComplaint();
			AssertEquals($"{oldStatus}: Result message", expectedMessage, message);
			AssertEquals($"{oldStatus}: Status unchanged", oldStatus, entryHeader.CH_LastEComplaintStatus);
			AssertEquals($"{oldStatus}: Event count", 0, entryHeader.Logs.LogsNotInDB.Length);
		}
	}

	public void TestICommonGoodsItemsIntegratorProvider_CommonGoodsItemsIntegrator()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<CommonGoodsItemsIntegrator>(entryHeader.CommonGoodsItemsIntegrator);
	}

	public void TestIsMessageStatusSent() => CombineAssertions(() =>
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
		AssertEquals("IsMessageStatusSent on status sent returns false", true, entryHeader.IsMessageStatusSent);

		entryHeader.CH_Status = CHLogicalStatusList.Codes.Acknowledged;
		AssertEquals("IsMessageStatusSent on status acknowledged returns false", true, entryHeader.IsMessageStatusSent);

		entryHeader.CH_Status = CHLogicalStatusList.Codes.Failed;
		AssertEquals("IsMessageStatusSent on status failed returns false", false, entryHeader.IsMessageStatusSent);
	});

	public void TestConfirmedDuty()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		PopulateCusEntryHeaderCharges(entryHeader);
		entryHeader.ConfirmedCharges.Load();

		AssertEquals("Sum of confirmed Duties", 50.0M, entryHeader.ConfirmedDuty);
	}

	public void TestConfirmedVAT()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		PopulateCusEntryHeaderCharges(entryHeader);
		entryHeader.ConfirmedCharges.Load();

		AssertEquals("Sum of confirmed Duties", 10.0M, entryHeader.ConfirmedVAT);
	}

	public void TestCH_LastEComStatus() => CombineAssertions(() =>
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("MaxLength", 3, entryHeader.CH_LastEComplaintStatusInfo.MaxLength);
		CaptionTestHelper.AssertCaptions(entryHeader.CH_LastEComplaintStatusInfo, caption: "ECom Status", shortCaption: "ECom", fullDescription: "Last ECom Status");
	});

	public void TestGetTotalChargeValueFor() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Switzerland);
		var dtyRateType = helper.CreateNewOrGetExistingRateType(CountryCodes.Switzerland, RateTypes.Duties);
		helper.CreateCusRateCode(Factory, "290", dtyRateType.PK, countryCode: CountryCodes.Switzerland);
		helper.CreateCusRateCode(Factory, "291", dtyRateType.PK, countryCode: CountryCodes.Switzerland);
		Factory.Save();

		var dtyChargeType = new EntryChargeType(null, "DTY", ZString.Empty, true, ZString.Empty);
		var vatChargeType = new EntryChargeType(null, "VAT", ZString.Empty, true, ZString.Empty);

		var entryHeader = ImportJobDeclaration.ActiveEntryHeaders.AddNew();
		var chargeEntry = (ICustomsChargeEntry)entryHeader;
		var entryLine1 = entryHeader.AllEntryLines.AddNew();
		var entryLine2 = entryHeader.AllEntryLines.AddNew();

		AddFee(entryLine1, "290", 50m);
		AddFee(entryLine1, "291", 100m);
		AddFee(entryLine1, "VAT", 200m);
		AddFee(entryLine2, "290", 60m);
		AddFee(entryLine2, "VAT", 300m);
		AssertEquals("DTY (calculated)", 50m + 100m + 60m, chargeEntry.GetTotalChargeValueFor(dtyChargeType, ZString.Empty));
		AssertEquals("VAT (calculated)", 200m + 300m, chargeEntry.GetTotalChargeValueFor(vatChargeType, ZString.Empty));

		AddConfirmedFee(entryLine1, "290", 500m);
		AssertEquals("DTY (one confirmed)", 500m, chargeEntry.GetTotalChargeValueFor(dtyChargeType, ZString.Empty));
		AssertEquals("VAT (one confirmed)", 0m, chargeEntry.GetTotalChargeValueFor(vatChargeType, ZString.Empty));

		AddConfirmedFee(entryLine1, "291", 1000m);
		AddConfirmedFee(entryLine1, "VAT", 2000m);
		AddConfirmedFee(entryLine2, "290", 600m);
		AddConfirmedFee(entryLine2, "VAT", 3000m);
		AssertEquals("DTY (confirmed)", 500m + 1000m + 600m, chargeEntry.GetTotalChargeValueFor(dtyChargeType, ZString.Empty));
		AssertEquals("VAT (confirmed)", 2000m + 3000m, chargeEntry.GetTotalChargeValueFor(vatChargeType, ZString.Empty));

		void AddFee(CusEntryLine entryLine, ZString chargeType, ZDecimal chargeAmount)
		{
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = chargeType;
			fee.CF_ChargeAmount = chargeAmount;
		}

		void AddConfirmedFee(CusEntryLine entryLine, ZString chargeType, ZDecimal chargeAmount)
		{
			var fee = entryLine.ConfirmedFees.AddNew();
			fee.CF_ChargeType = chargeType;
			fee.CF_ChargeAmount = chargeAmount;
		}
	});

	public void TestIsFeePaidByBroker()
	{
		var entryHeader = ImportJobDeclaration.ActiveEntryHeaders.AddNew();

		AssertIsFeePaidByBroker(true, CusEntryFeeTypes.VAT, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Declarant);
		AssertIsFeePaidByBroker(true, RateTypes.Duties, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Declarant);
		AssertIsFeePaidByBroker(true, RateTypes.AdditionalTaxes, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Declarant);
		AssertIsFeePaidByBroker(true, RateTypes.AdditionalFees, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Declarant);

		AssertIsFeePaidByBroker(true, CusEntryFeeTypes.VAT, DeclarationPayerList.Codes.Forwarder, DeclarationPayerList.Codes.Declarant);
		AssertIsFeePaidByBroker(false, RateTypes.Duties, DeclarationPayerList.Codes.Forwarder, DeclarationPayerList.Codes.Declarant);
		AssertIsFeePaidByBroker(false, RateTypes.AdditionalTaxes, DeclarationPayerList.Codes.Forwarder, DeclarationPayerList.Codes.Declarant);
		AssertIsFeePaidByBroker(false, RateTypes.AdditionalFees, DeclarationPayerList.Codes.Forwarder, DeclarationPayerList.Codes.Declarant);

		AssertIsFeePaidByBroker(false, CusEntryFeeTypes.VAT, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Forwarder);
		AssertIsFeePaidByBroker(true, RateTypes.Duties, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Forwarder);
		AssertIsFeePaidByBroker(true, RateTypes.AdditionalTaxes, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Forwarder);
		AssertIsFeePaidByBroker(true, RateTypes.AdditionalFees, DeclarationPayerList.Codes.Declarant, DeclarationPayerList.Codes.Forwarder);

		void AssertIsFeePaidByBroker(bool expectedResult, string feeCode, string paymentMethod, string vadPaidBy)
		{
			ImportJobDeclaration.JE_PaymentMethod = paymentMethod;
			ImportJobDeclaration.JE_VATPaidBy = vadPaidBy;
			AssertEquals($"FeeCode={feeCode} JE_PaymentMethod={paymentMethod} JE_VATPaidBy={vadPaidBy}", expectedResult, entryHeader.IsFeePaidByBroker(feeCode, ZString.Empty, null));
		}
	}

	void PopulateCusEntryHeaderCharges(CusEntryHeader entryHeader)
	{
		PopulateCharges(Core.Constants.Customs.CusEntryFeeTypes.VAT, 10.0M);
		PopulateCharges(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 20.0M);
		PopulateCharges("150", 30.0M);
		PopulateCharges("XXX", 40.0M, string.Empty);

		void PopulateCharges(ZString chargeType, ZDecimal chargeAmount, string source = CusEntryHeaderChargesSourceCodeList.Codes.CUS)
		{
			var newCharge = entryHeader.Charges.AddNew();
			newCharge.C1_Source = source;
			newCharge.C1_ChargeType = chargeType;
			newCharge.C1_ChargeAmount = chargeAmount;
		}
	}

	protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

	protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

	new JobDeclaration ImportJobDeclaration => importJobDeclaration ??= CreateNewDeclaration(JobMessageTypeList.Codes.Import);
	JobDeclaration importJobDeclaration;

	JobDeclaration ExportJobDeclaration => exportJobDeclaration ??= CreateNewDeclaration(JobMessageTypeList.Codes.Export);
	JobDeclaration exportJobDeclaration;

	JobDeclaration CreateNewDeclaration(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return declaration;
	}
}
