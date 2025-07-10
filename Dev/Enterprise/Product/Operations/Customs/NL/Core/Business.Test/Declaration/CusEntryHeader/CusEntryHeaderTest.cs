using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
{
	public void TestDMSFallbackIsActive()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Default value", false, entryHeader.DMSFallbackIsActive);

			entryHeader.DMSFallbackIsActive = true;
			AssertEquals("Update DMSFallbackIsActive to true, will update CH_PhaseStatus to FBK", CustomsEntryPhaseStatusList.Codes.FBK, entryHeader.CH_PhaseStatus);

			entryHeader.DMSFallbackIsActive = false;
			AssertEquals("Update DMSFallbackIsActive to false, will update CH_PhaseStatus to 515", CustomsEntryPhaseStatusList.Codes._515, entryHeader.CH_PhaseStatus);
		});
	}

	public void TestDMSFallbackIsActive_AddOrRemoveAdditionalInfos()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			entryHeader.DMSFallbackIsActive = true;
			AssertNotNull("DMSFallbackIsActive is true and MessageHasBeenSentBeforeFallback is false", entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.IsAnAdditionalInformation && x.CSI_Code == "NP500"));

			entryHeader.DMSFallbackIsActive = false;
			AssertNull("Target AdditionalInfo has been deleted", entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.IsAnAdditionalInformation && x.CSI_Code == "NP500"));

			var fallbackConfig = new FallbackConfiguration();
			fallbackConfig.Start = new ZDateTime(2024, 10, 25, 10, 38, 00);
			var companyPK = entryHeader.RegistryCompanyPK;
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);

			var message = Factory.New<NLEDIMessage>();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 25, 07, 38, 00);
			entryHeader.Messages.Add(message);
			AssertEquals("MessageHasBeenSentBeforeFallback is true", true, entryHeader.MessageHasBeenSentBeforeFallback);

			entryHeader.DMSFallbackIsActive = true;
			AssertNull("DMSFallbackIsActive is true and MessageHasBeenSentBeforeFallback is true", entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.IsAnAdditionalInformation && x.CSI_Code == "NP500"));
		});
	}

	[TestDate(2025, 03, 21, 11, 15, 00)]
	public void TestDMSFallbackIsActive_ChangeFBKMessageAfterFallback()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.DMSFallbackIsActive = true;

		var fbkMessage = Factory.New<NLEDIMessage>();
		fbkMessage.EM_MessageSubType = "FBK";
		fbkMessage.EM_ReceiveTransmit = "TRX";
		fbkMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 30);
		fbkMessage.EM_HeldUntilDate = ZDateTime.MaxSmallDateTime;
		entryHeader.Messages.Add(fbkMessage);

		CombineAssertions(() =>
		{
			var companyPK = entryHeader.RegistryCompanyPK;
			var fallbackConfig = new FallbackConfiguration();
			fallbackConfig.Start = new ZDateTime(2024, 10, 25);
			fallbackConfig.End = new ZDateTime(2025, 04, 01);
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			entryHeader.DMSFallbackIsActive = false;
			AssertEquals("During Fallback, EM_HeldUntilDate not change", ZDateTime.MaxSmallDateTime, fbkMessage.EM_HeldUntilDate);
			AssertEquals("During Fallback, EM_MessageSubType not change", "FBK", fbkMessage.EM_MessageSubType);

			fallbackConfig.End = new ZDateTime(2024, 12, 27);
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			entryHeader.DMSFallbackIsActive = false;
			AssertEquals("After Fallback, EM_HeldUntilDate has been set to time of sending", new ZDateTime(2025, 03, 21, 11, 15, 00), fbkMessage.EM_HeldUntilDate);
			AssertEquals("After Fallback, MessageSubType has been set to DEC", "DEC", fbkMessage.EM_MessageSubType);
		});
	}

	public void TestDMSFallbackIsActive_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var companyPK = entryHeader.RegistryCompanyPK;
		entryHeader.DMSFallbackIsActive = true;

		var fallbackConfig = new FallbackConfiguration();
		CombineAssertions(() =>
		{
			AssertEquals("DMSFallbackIsActive shouldn't be readonly when DMSFallbackIsActive is true", false, entryHeader.DMSFallbackIsActiveInfo.ReadOnly);
			entryHeader.DMSFallbackIsActive = false;
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			AssertEquals("DMSFallbackIsActive should be readonly for Start Date empty", true, entryHeader.DMSFallbackIsActiveInfo.ReadOnly);

			fallbackConfig.Start = ZDateTime.Now.AddYears(-1);
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			AssertEquals("DMSFallbackIsActive should not be readonly when Start Date is past and End Date is Empty", false, entryHeader.DMSFallbackIsActiveInfo.ReadOnly);

			fallbackConfig.Start = ZDateTime.Now.AddYears(1);
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			AssertEquals("DMSFallbackIsActive should be readonly when Start Date is future", true, entryHeader.DMSFallbackIsActiveInfo.ReadOnly);

			fallbackConfig.Start = ZDateTime.Now.AddYears(-1);
			fallbackConfig.End = ZDateTime.Now.AddYears(1);
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			AssertEquals("DMSFallbackIsActive should not be readonly when Start Date is past and End Date is future", false, entryHeader.DMSFallbackIsActiveInfo.ReadOnly);

			declaration.JE_MessageType = "IMP";
			AssertEquals("DMSFallbackIsActive should be readonly when Import, even Start Date is past and End Date is future", true, entryHeader.DMSFallbackIsActiveInfo.ReadOnly);
		});
	}

	public void TestMessageHasBeenSentBeforeFallback()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var fallbackConfig = new FallbackConfiguration();
			fallbackConfig.Start = new ZDateTime(2024, 10, 25, 10, 38, 00);
			var companyPK = entryHeader.RegistryCompanyPK;
			NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);
			AssertEquals("Start.ToUniversalBranchTime", new ZDateTime(2024, 10, 25, 08, 38, 00), fallbackConfig.Start.ToUniversalBranchTime());

			var message = Factory.New<NLEDIMessage>();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 25, 09, 38, 00);
			entryHeader.Messages.Add(message);
			AssertEquals("EM_SystemCreateTimeUtc is after universal branch time", false, entryHeader.MessageHasBeenSentBeforeFallback);

			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 25, 07, 38, 00);
			AssertEquals("EM_SystemCreateTimeUtc is before universal branch time", true, entryHeader.MessageHasBeenSentBeforeFallback);

			message.EM_ReceiveTransmit = "RCV";
			AssertEquals("EM_ReceiveTransmit isn't TRX", false, entryHeader.MessageHasBeenSentBeforeFallback);
		});
	}

	public void TestRandomHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertType<JobComInvoiceHeader>(entryHeader.RandomHeader);
	}

	public void TestEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertType<AllCusEntryLineCollection<CusEntryLine>>(entryHeader.AllEntryLines);
	}

	public void TestMergedLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertType<CusEntryLineCollection<CusEntryLine>>(entryHeader.MergedLines);
	}

	public void TestMessages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.AddNew();
		AssertEquals(typeof(EDIMessageCollection), entryHeader.Messages.GetType());
	}

	public void TestEntryLinesHasCosts()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		CombineAssertions(() =>
		{
			AssertEquals(false, entryHeader.EntryLinesHasCosts);

			entryLine.Fees.AddNew();
			AssertEquals(true, entryHeader.EntryLinesHasCosts);
		});
	}

	public void TestHasRelatedExitControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123");

		CombineAssertions(() =>
		{
			AssertEquals("No 'Exit control'-declaration is linked to the declaration with the same MRN as the CusEntryHeader", false, entryHeader.HasRelatedExitControl);

			var exitHeader = Factory.New<CusExitHeader>();
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var exitReport = exitHeader.CusExitReports.AddNew();

			exitConsignment.CXC_MovementReference = "MRN123";
			exitReport.CER_CXC_Consignment = exitConsignment.PK;
			exitHeader.Parent = declaration;

			AssertEquals("'Exit control'-declaration is linked to the declaration with the same MRN as the CusEntryHeader", true, entryHeader.HasRelatedExitControl);
		});
	}

	public void TestCH_PhaseStatus_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var phaseStatusInfo = entryHeader.CH_PhaseStatusInfo;

		AssertEquals(true, phaseStatusInfo.ReadOnly);
	}

	public void TestCH_PhaseStatus_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(entryHeader.CH_PhaseStatusInfo);

		CombineAssertions(() =>
		{
			AssertEquals("CH_PhaseStatus Short Caption", "Ph. Status", captionResourceString.ShortCaption);
			AssertEquals("CH_PhaseStatus Caption", "Phase Status", captionResourceString.Caption);
		});
	}

	public void TestCH_PhaseStatusDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("CH_PhaseStatus is empty.", ZString.Empty, entryHeader.CH_PhaseStatusDescription);

			entryHeader.CH_PhaseStatus = "XXX";
			AssertEquals("Phase status description for code not in list", ZString.Empty, entryHeader.CH_PhaseStatusDescription);

			entryHeader.CH_PhaseStatus = "CRE";
			AssertEquals("Phase status description for code in list", CustomsEntryPhaseStatusList.Descriptions.CRE, entryHeader.CH_PhaseStatusDescription);
		});
	}

	public void TestCH_PhaseStatusDescription_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(entryHeader.CH_PhaseStatusDescriptionInfo);

		CombineAssertions(() =>
		{
			AssertEquals("CH_PhaseStatusDescription Short Caption", "Ph. Status Desc.", captionResourceString.ShortCaption);
			AssertEquals("CH_PhaseStatusDescription Caption", "Phase Status Description", captionResourceString.Caption);
		});
	}

	public void TestCH_EntryStatus_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryStatusInfo = entryHeader.CH_EntryStatusInfo;

		AssertEquals(true, entryStatusInfo.ReadOnly);
		entryHeader.DMSFallbackIsActive = true;

		List<(ZString entryHeaderStatus, ZString entryStatus)> validStatusCombinationsAfterFallbackIsSent =
		[
			(StatusNew.Accepted, ZString.Empty),
			(StatusNew.Accepted, EntryStatusNew.NotReceiveResponseBeforeEndOfFallback),
			(StatusNew.Error, ZString.Empty),
			(StatusNew.SentToCustoms, ZString.Empty),
			(StatusNew.Accepted, EntryStatusNew.PhysicalInspection),
		];

		CombineAssertions(() =>
		{
			foreach (var item in validStatusCombinationsAfterFallbackIsSent)
			{
				entryHeader.CH_Status = item.entryHeaderStatus;
				entryHeader.CH_EntryStatus = item.entryStatus;
				AssertEquals($"CH_Status is {item.entryHeaderStatus}, CH_EntryStatus is {item.entryStatus}, CH_PhaseStatus is FBK and Fallback is active, EntryStatus shouldn't be readonly", false, entryStatusInfo.ReadOnly);
			}

			entryHeader.CH_Status = "INV";
			AssertEquals("CH_Status is INV, CH_EntryStatus is Empty, and Fallback is active, Status should be readonly", true, entryStatusInfo.ReadOnly);

			entryHeader.CH_EntryStatus = "ERR";
			entryHeader.CH_Status = ZString.Empty;
			entryHeader.DMSFallbackIsActive = false;
			AssertEquals("Fallback isn't active, EntryStatus should be readonly", true, entryStatusInfo.ReadOnly);
		});
	}

	public void TestEntryNumber_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryNumberInfo = entryHeader.EntryNumberInfo;

		AssertEquals(true, entryNumberInfo.ReadOnly);
	}

	public void TestCH_Status_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(entryHeader.CH_StatusInfo);

		CombineAssertions(() =>
		{
			AssertEquals("CH_Status Short Caption", "Status", captionResourceString.ShortCaption);
			AssertEquals("CH_Status Medium Caption", "Msg. Status", captionResourceString.MediumCaption);
			AssertEquals("CH_Status Caption", "Message Status", captionResourceString.Caption);
		});
	}

	public void TestCH_Status_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var statusInfo = entryHeader.CH_StatusInfo;

		AssertEquals(true, statusInfo.ReadOnly);

		var companyPK = entryHeader.RegistryCompanyPK;
		NLCustomsRegistry.Instance.FallbackEmail.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, "MNL");

		CombineAssertions(() =>
		{
			entryHeader.DMSFallbackIsActive = true;
			entryHeader.CH_Status = "SNT";
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("CH_Status is SNT, CH_EntryStatus is Empty, and Fallback is active, Status shouldn't be readonly", false, statusInfo.ReadOnly);

			entryHeader.CH_Status = "ERR";
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("CH_Status is ACC, CH_EntryStatus is Empty, and Fallback is active, Status shouldn't be readonly", false, statusInfo.ReadOnly);

			entryHeader.DMSFallbackIsActive = false;
			AssertEquals("Fallback isn't active, Status should be readonly", true, statusInfo.ReadOnly);

			entryHeader.DMSFallbackIsActive = true;
			entryHeader.CH_Status = "SNT";
			entryHeader.CH_EntryStatus = ZString.Empty;
			NLCustomsRegistry.Instance.FallbackEmail.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, "AUT");
			AssertEquals("Automatic Email, Status should be readonly", true, statusInfo.ReadOnly);
		});
	}

	public void TestCH_EntryReleaseDate_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryReleaseDateInfo = entryHeader.CH_EntryReleaseDateInfo;

		AssertEquals(true, entryReleaseDateInfo.ReadOnly);
	}

	public void TestDefaultStatusDescription()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals(ZString.Empty, entryHeader.DefaultStatusDescription);
	}

	#region Test LRN
	protected override ZBool IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber => true;

	public void TestLocalReferenceNumberGenerated()
	{
		entryHeader = CreateEntryHeader();
		AssertNotNullOrEmpty(GetLocalReferenceNumberAfterSave());
	}

	public void TestLocalReferenceNumberCurrentYearPart()
	{
		entryHeader = CreateEntryHeader();
		var firstTwoCharactersForCurrentYear = ZDateTime.Now.Year.ToString().Substring(2);
		AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the current year.", firstTwoCharactersForCurrentYear, GetLocalReferenceNumberAfterSave());
	}

	[TestDate(2071, 01, 01)]
	public void TestLocalReferenceNumberDifferentYearPart()
	{
		entryHeader = CreateEntryHeader();
		var firstTwoCharactersForMockedCurrentYear = "71";
		AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the Mocked \"current\" year.", firstTwoCharactersForMockedCurrentYear, GetLocalReferenceNumberAfterSave());
	}

	public void TestLocalReferenceNumberEoriNumberPart()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("123456789");
		var partThatShouldStartWithEORI = GetLocalReferenceNumberAfterSave().Substring(2);

		AssertStartsWith("Expected Local Reference Number, after the first 2 characters, to contain the EORI of the Declarant.", "123456789", partThatShouldStartWithEORI);
	}

	public void TestLocalReferenceNumberNumberFountainPart()
	{
		entryHeader = CreateEntryHeader();
		var partThatShouldBe9RandomDigits = GetLocalReferenceNumberAfterSave().Substring(2 + 9);
		AssertMatch("Expected Local Reference Number, after the first 11 characters, to be 9 random digits.", new Regex(@"\d{9}"), partThatShouldBe9RandomDigits);

		entryHeader.CH_BGMReference = null;
		var partThatShouldBe9DifferentRandomDigits = GetLocalReferenceNumberAfterSave().Substring(2 + 9);
		AssertNotEquals("Expected Local Reference Number, after the first 11 characters, to be 9 random digits. Yet they were the same after generating the LRN twice.", partThatShouldBe9DifferentRandomDigits, partThatShouldBe9RandomDigits);
	}

	public void TestLocalReferenceNumberAlreadySet()
	{
		entryHeader = CreateEntryHeader();
		var localReferenceNumberAtStart = GetLocalReferenceNumberAfterSave();
		var localReferenceNumberAfterAnotherSave = GetLocalReferenceNumberAfterSave();

		AssertEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
	}

	public void TestLocalReferenceNumberDeclarantChange()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("123456789");
		var localReferenceNumberAtStart = GetLocalReferenceNumberAfterSave();
		SetCompanyEoriNumber("987654321");
		var localReferenceNumberAfterAnotherSave = GetLocalReferenceNumberAfterSave();

		AssertNotEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
	}

	public void TestLocalReferenceNumberAfterDeclarationUCRChange()
	{
		entryHeader = CreateEntryHeader();
		var localReferenceNumberAtStart = GetLocalReferenceNumberAfterSave();
		entryHeader.Declaration.JE_UCR = "123456789";
		var localReferenceNumberAfterAnotherSave = GetLocalReferenceNumberAfterSave();

		AssertEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
	}

	public void TestLocalReferenceNumberSubmitted() => CombineAssertions(() =>
	{
		TestLocalReferenceNumberSubmitted(false, false, true);
		TestLocalReferenceNumberSubmitted(true, false, false);
		TestLocalReferenceNumberSubmitted(false, true, false);
		TestLocalReferenceNumberSubmitted(true, true, false);
	});

	void TestLocalReferenceNumberSubmitted(bool isWaitingForResponse, bool hasBeenLodgedAtCustoms, bool changeExpected)
	{
		var declaration = Factory.New<JobDeclaration>();

		entryHeader = Factory.New<DummyCusEntryHeader>();
		entryHeader.SetDeclarationForTesting(declaration);

		var declarant = entryHeader.DeclarantOrganisation;
		((DummyCusEntryHeader)entryHeader).IsWaitingForResponseReturns = isWaitingForResponse;
		((DummyCusEntryHeader)entryHeader).HasBeenLodgedAtCustomsReturns = hasBeenLodgedAtCustoms;
		SetCompanyEoriNumber("123456789");
		var localReferenceNumberAtStart = entryHeader.CH_BGMReference;

		SetCompanyEoriNumber("987654321");
		var localReferenceNumberAfterAnotherSave = entryHeader.CH_BGMReference;

		if (changeExpected)
		{
			AssertNotEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
		}
		else
		{
			AssertEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
		}
	}

	public void TestLocalReferenceNumberLength()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("BEDE9668822669288");
		var localReferenceNumber = GetLocalReferenceNumberAfterSave();
		CombineAssertions(() =>
		{
			AssertEquals("Length of LocalReferenceNumber should be 22 characters, long EORI-nr", 22, localReferenceNumber.Length);
			SetCompanyEoriNumber("BE123");
			localReferenceNumber = GetLocalReferenceNumberAfterSave();
			AssertEquals("Length of LocalReferenceNumber should be 22 characters, short EORI-nr", 22, localReferenceNumber.Length);
			SetCompanyEoriNumber("BEDE966882266928812345678901234567");
			localReferenceNumber = GetLocalReferenceNumberAfterSave();
			AssertEquals("Length of LocalReferenceNumber should be 0 characters, Invalid (very long) EORI-nr", 0, localReferenceNumber.Length);
		});
	}

	public void TestLocalReferenceNumberCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(entryHeader.CH_BGMReferenceInfo);

		AssertEquals("LocalReferenceNumber Caption", "Reference No", captionResourceString.Caption);
		AssertEquals("LocalReferenceNumber Short Caption", "Ref No.", captionResourceString.ShortCaption);
	}

	void SetCompanyEoriNumber(string value)
	{
		var company = entryHeader.Declaration.Branch.OrgProxy;
		company.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, value, "BE");
		Factory.Save();
	}

	ZString GetLocalReferenceNumberAfterSave()
	{
		Factory.Save();
		return entryHeader.CH_BGMReference;
	}

	CusEntryHeader entryHeader;
	CusEntryHeader CreateEntryHeader()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();

		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.SetDeclarationForTesting(declaration);

		var declarant = entryHeader.DeclarantOrganisation;
		declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123987465", "BE");

		return entryHeader;
	}

	#endregion

	#region Implement

	protected override BaseJobDeclaration GetNewDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return dec;
	}

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = base.ImportJobDeclaration;
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}
	}

	#endregion

	public void TestCusEntryNumberIssueDate()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("CusEntryNumber is null", ZDateTime.Empty, entryHeader.CusEntryNumberIssueDate);

			entryHeader.EntryNumber = "EntryNumber333";
			var issueDate = new ZDateTime(2023, 11, 13);
			entryHeader.CusEntryNumber.CE_IssueDate = issueDate;
			AssertEquals("CusEntryNumber isn't null", issueDate, entryHeader.CusEntryNumberIssueDate);
		});
	}

	public void TestFallbackEntryNumberIssueDate()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("FallbackEntryNumberIssueDate is empty", ZDateTime.Empty, entryHeader.FallbackEntryNumberIssueDate);
			AssertNull("FallbackEntryNumber was not created during the access", CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.EU.Fallback, entryHeader.CountryCode));

			entryHeader.FallbackEntryNumberIssueDate = new ZDateTime(2023, 11, 13);
			var fallbackEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.EU.Fallback, entryHeader.CountryCode);
			AssertEquals("CE_IssueDate", new ZDateTime(2023, 11, 13), fallbackEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", true, fallbackEntryNumber.CE_EntryIsSystemGenerated);
			AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, fallbackEntryNumber.CE_Category);
		});
	}

	public void TestCusEntryNumberIssueDate_Caption()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var resStringData = entryHeader.CusEntryNumberIssueDateInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Acceptance Date", resStringData.Caption);
			AssertEquals("MediumCaption", "Accept. Date", resStringData.MediumCaption);
			AssertEquals("ShortCaption", "Acc. Date", resStringData.ShortCaption);
		});
	}

	public void TestCH_ExitDate_Caption()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var resStringData = entryHeader.CH_ExitDateInfo.GetAttribute<ResourceStringDataAttribute>();
		AssertEquals("Exit Date", resStringData.Caption);
	}

	public void TestCH_ExitDate_ReadOnly()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var exitDateInfo = entryHeader.CH_ExitDateInfo;

		AssertEquals(true, exitDateInfo.ReadOnly);
	}

	public void TestCustomsRemarks()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.AddNew();

		AssertEquals("1 message without notes", 0, entryHeader.CustomsMessageRemarks.Count);

		var messageWithNote = entryHeader.Messages.AddNew();
		messageWithNote.CustomsMessageRemarks = "CAN|Cancelled";

		AssertEquals("1 message without notes and 1 with notes", 1, entryHeader.CustomsMessageRemarks.Count);
	}

	public void TestShouldLogEntryStatus()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals(true, entryHeader.ShouldLogEntryStatus);
	}

	public void TestShouldLogStatus()
	{
		var entryHeader = Factory.New<DummyCusEntryHeader>();
		AssertEquals(true, entryHeader.ShouldLogStatusExposed);
	}

	public void TestShouldLogPhaseStatus()
	{
		var entryHeader = Factory.New<DummyCusEntryHeader>();
		AssertEquals(true, entryHeader.ShouldLogPhaseStatusExposed);
	}

	[TestDate(2025, 04, 08, 13, 39, 00)]
	public void TestGetMostRecentStatusChangeEventTime()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "515", ZDateTimeOffset.Now);
		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "515", ZDateTimeOffset.Now.AddDays(1));

		CombineAssertions(() =>
		{
			AssertEquals("When no SL_SE_NKEvent match", ZDateTime.Empty, entryHeader.GetMostRecentStatusChangeEventTime(Events.CustomsEntryStatus, "515"));
			AssertEquals("When no SL_Reference match", ZDateTime.Empty, entryHeader.GetMostRecentStatusChangeEventTime(Events.PhaseStatusChange, "513"));
			AssertEquals("When multiple logs exist", new ZDateTime(2025, 04, 09, 13, 39, 00), entryHeader.GetMostRecentStatusChangeEventTime(Events.PhaseStatusChange, "515"));
		});
	}

	[TestDate(2025, 04, 03, 15, 36, 00)]
	public void TestRetrievePreviousValueFromLog()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "515", ZDateTimeOffset.Now);
		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "514", ZDateTimeOffset.Now.AddDays(1));

		CombineAssertions(() =>
		{
			var dateTimeInBetween = new ZDateTime(2025, 4, 3, 23, 0, 0);
			AssertEquals("When no SL_SE_NKEvent match", ZString.Empty, entryHeader.RetrievePreviousValueFromLog(Events.CustomsEntryStatus, dateTimeInBetween));
			AssertEquals("Previous Value when using a time after setting the first status but before setting the last status", "515", entryHeader.RetrievePreviousValueFromLog(Events.PhaseStatusChange, dateTimeInBetween));

			var dateTimeAfter = new ZDateTime(2025, 4, 5, 1, 0, 0);
			AssertEquals("Previous Value when using a time after setting the last status", "514", entryHeader.RetrievePreviousValueFromLog(Events.PhaseStatusChange, dateTimeAfter));
		});
	}

	public void TestIsMrnEntryNumberTheOneWeWantToShow()
	{
		var entryHeader = Factory.New<DummyCusEntryHeader>();
		AssertEquals("IsMrnEntryNumberTheOneWeWantToShow is false.", true, entryHeader.IsMrnEntryNumberTheOneWeWantToShow_Exposed);
	}

	sealed class DummyCusEntryHeader : CusEntryHeader
	{
		public DummyCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsWaitingForResponseReturns { get; set; }

		public bool HasBeenLodgedAtCustomsReturns { get; set; }

		public override bool IsWaitingForResponse => IsWaitingForResponseReturns;

		public override bool HasBeenLodgedAtCustoms => HasBeenLodgedAtCustomsReturns;

		public ZBool ShouldLogStatusExposed => ShouldLogStatus;

		public ZBool ShouldLogPhaseStatusExposed => ShouldLogPhaseStatus;

		public bool IsMrnEntryNumberTheOneWeWantToShow_Exposed => base.IsMrnEntryNumberTheOneWeWantToShow;
	}
}
