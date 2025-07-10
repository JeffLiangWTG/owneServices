using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using EUNctsHeader = Enterprise.Customs.EU.NCTS.Business.NctsHeader;
using JobDeclaration = Enterprise.Customs.CH.Business.JobDeclaration;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
sealed class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
{
	public void TestCorrectTypeDecided()
	{
		_ = Header;
		Factory.Save();
		var loadedHeader = new BusinessObjectFactory().Load<EUNctsHeader>(Header.PK);
		AssertType<NctsHeader>(loadedHeader);
	}

	public void TestConfiguration()
	{
		AssertType<NctsConfiguration>(Header.Configuration);
	}

	public void TestGetLookups()
	{
		AssertType<NctsHeaderLookups>(Header.Lookups);
	}

	public void TestGetValidation()
	{
		AssertType<NctsHeaderPhase5Validation>(Header.Validation);
	}

	public void TestGetCustomsOfficeRequirementHelper()
	{
		AssertType<NctsHeaderCustomsOfficeRequirementHelper>(Header.CustomsOfficeRequirementHelper);
	}

	public void TestDefaults()
	{
		AssertEquals(Factory.New<NctsHeader>().BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS5);
	}

	public void TestDefaultDataGroupingCode() => CombineAssertions(() =>
	{
		AssertEquals("Data Grouping Code in CH", Core.Constants.CountryCodes.Switzerland, Header.DefaultDataGroupingCode);
		Factory.Save();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
		{
			var header = Factory.Load<NctsHeader>(Header.PK);
			AssertEquals("Data Grouping Code in DE", Core.Constants.CountryCodes.Switzerland, header.DefaultDataGroupingCode);
		}

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
		{
			var header = Factory.New<NctsHeader>();

			AssertEquals("Country Code LI -> LI", Core.Constants.CountryCodes.Liechtenstein, header.CountryCode);
			AssertEquals("Data Grouping Code LI -> CH", Core.Constants.CountryCodes.Switzerland, header.DefaultDataGroupingCode);
		}
	});

	public void TestBills()
	{
		AssertType<NctsBillCollection>(Header.Bills);
	}

	public void TestCusSupplyChainActors()
	{
		AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(Header.CusSupplyChainActors);
	}

		public void TestBH_CommunicationLanguage() => CombineAssertions(() =>
		{
			CaptionTestHelper.AssertCaptions(Header.BH_CommunicationLanguageInfo, caption: "Language", fullDescription: "Language used to communicate with customs");
			AssertEquals("MaxLength", 2, Header.BH_CommunicationLanguageInfo.MaxLength);
			AssertEquals("Default on new", SwissCustomsLanguageList.Codes.German, Header.BH_CommunicationLanguage);

		var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);

		currentUser.GS_WorkingLanguage = SharedConstants.Languages.French;
		Factory.Save();
		using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
		{
			var header = Factory.New<NctsHeader>();
			AssertEquals("Default from login", SwissCustomsLanguageList.Codes.French, header.BH_CommunicationLanguage);
		}

		currentUser.GS_WorkingLanguage = SharedConstants.Languages.Swedish;
		Factory.Save();
		using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
		{
			var header = Factory.New<NctsHeader>();
			AssertEquals("German if not a Swiss language", SwissCustomsLanguageList.Codes.German, header.BH_CommunicationLanguage);
		}
	});

	public void TestGetNewCustomsOfficesForDeparture()
	{
		AssertType<NctsEuOfficeCodeCollectionForDepartureGrid>(Header.CustomsOfficesForDeparture);
	}

	public void TestDestinationCustomsOfficeCodeForArrival()
	{
		Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
		AssertEquals("ReadOnly", true, Header.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
	}

	public void TestBH_ExportFlag()
	{
		Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
		AssertEquals("ReadOnly", true, Header.BH_ExportFlagInfo.ReadOnly);
	}

	public void TestDestinationTrader_ReadOnly() => CombineAssertions(() =>
	{
		Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
		AssertEquals("Arrival ReadOnly", true, Header.DestinationTrader.ReadOnly);

		Header.BH_HeaderType = NctsMovementType.Codes.Departure;
		AssertEquals("Departure not ReadOnly", false, Header.DestinationTrader.ReadOnly);
	});

	public void TestDestinationTrader_Default() => CombineAssertions(() =>
	{
		var branchOrgHeader = Factory.New<OrgHeader>();
		branchOrgHeader.OH_Code = "XXX";
		branchOrgHeader.Addresses.AddNewMainAddress();
		Factory.Save();

		GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgHeader.PK;
		Header.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertEquals("Branch entered", branchOrgHeader.PK, Header.DestinationTrader.Organisation.PK);

		header = null;
		GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
		AssertNotNull("Pre-condition - company has address", GlbCompany.CurrentCompany.OrgProxy?.MainAddress);
		Header.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertEquals("Branch null", GlbCompany.CurrentCompany.OrgProxy.PK, Header.DestinationTrader.Organisation.PK);

		header = null;
		Header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertNull("Departure - Destination Trader null", Header.DestinationTrader.Organisation);
	});

	public void TestPreviousDocuments()
	{
		AssertType<CommonPreviousDocument>(Header.PreviousDocuments.AddNew());
	}

	public void TestPreviousDocuments_ReadOnly()
	{
		CombineAssertions(() =>
		{
			Header.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertEquals("Arrival PreviousDocuments not ReadOnly", false, Header.PreviousDocuments.ReadOnly);

			header = null;
			Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("Arrival PreviousDocuments ReadOnly", true, Header.PreviousDocuments.ReadOnly);
		});
	}

	public void TestMovementReferenceNumberSetter() => CombineAssertions(() =>
	{
		const string mrnValue1 = "1.1";
		const string mrnValue2 = "1.2";
		var timeStamp = ZDateTime.BrettsBirthday;

		AssertNullOrEmpty("Empty", Header.MovementReferenceNumber);

		Header.MovementReferenceNumberSetter(mrnValue1, timeStamp, timeStamp.AddHours(1));
		var mrnNumber1 = Header.MovementReferenceEntryNumber;
		AssertCusEntryNumber(mrnNumber1, mrnValue1, timeStamp, timeStamp.AddHours(1));

		Header.MovementReferenceNumberSetter(mrnValue2, timeStamp.AddDays(1), timeStamp.AddDays(1).AddHours(1));
		var mrnNumber2 = Header.MovementReferenceEntryNumber;
		AssertCusEntryNumber(mrnNumber2, mrnValue2, timeStamp.AddDays(1), timeStamp.AddDays(1).AddHours(1));
		AssertEquals("Reference", mrnNumber1, mrnNumber2);
	});

	public void TestIsLinkedExport() => CombineAssertions(() =>
	{
		AssertEquals("No documents", false, Header.IsLinkedExport);

		Header.PreviousDocuments.AddNew().CSI_Code = "XXX";
		AssertEquals("Other documents", false, Header.IsLinkedExport);

		Header.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodes.Export;
		AssertEquals("Export documents", true, Header.IsLinkedExport);
	});

	public void TestIsLinkedOrRelatedExport() => CombineAssertions(() =>
	{
		Header.SetMovementType(NctsMovementType.Codes.Departure);

		Header.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodes.Export;
		AssertEquals("EXPO documents", true, Header.IsLinkedOrRelatedExport);

		Header.PreviousDocuments.RemoveAll();
		AssertEquals("No linked EntryHeaders, no EXPO documents", false, Header.IsLinkedOrRelatedExport);

		var cusEntryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		Header.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(cusEntryHeader);
		AssertEquals("Linked EntryHeaders", true, Header.IsLinkedOrRelatedExport);
	});

	protected override void PrepareArrivalMovementHeaderForIsUnloadingAllowedOrCompleteTests(EUNctsHeader header)
	{
		base.PrepareArrivalMovementHeaderForIsUnloadingAllowedOrCompleteTests(header);
		var chHeader = (NctsHeader)header;
		chHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.False;
	}

	public void TestIsUnloadingAllowedOrComplete_Arrival() => CombineAssertions(() =>
	{
		Header.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalMovementHeader = Header.ArrivalMovementHeader;
		arrivalMovementHeader.MultipleMRNIndicator = ZBool.True;

		foreach (var status in ExpectedUnloadingAllowedOrCompleteStatusListPhase5)
		{
			arrivalMovementHeader.BM_CustomsStatus = status;
			AssertEquals($"Code {status}", false, Header.IsUnloadingAllowedOrComplete);
		}
	});

	void AssertCusEntryNumber(CusEntryNumber entryNumber, string expectedEntryNum, ZDateTime expectedIssueDate, ZDateTime expectedExpiryDate)
	{
		AssertEquals("CE_EntryNum", expectedEntryNum, entryNumber.CE_EntryNum);
		AssertEquals("CE_IssueDate", expectedIssueDate, entryNumber.CE_IssueDate);
		AssertEquals("CE_ExpiryDate", expectedExpiryDate, entryNumber.CE_ExpiryDate);
	}

	public void TestArrivalReferenceEntryNumber() => CombineAssertions(() =>
	{
		AssertNotNull("ArrivalReferenceEntryNumber->Not Null", Header.ArrivalReferenceNumber);
		AssertEquals("ArrivalReferenceEntryNumber.CE_RN_NKCountryCode", Header.CountryCode, header.MovementReferenceEntryNumber.CE_RN_NKCountryCode);
		AssertEquals("ArrivalReferenceEntryNumber.CE_EntryType", CusEntryNumberTypes.EU.ArrivalReferenceNumber, Header.ArrivalReferenceEntryNumber.CE_EntryType);
	});

	public void TestArrivalReferenceNumber() => CombineAssertions(() =>
	{
		AssertEquals("Initial ARN is empty", ZString.Empty, Header.ArrivalReferenceNumber);
		AssertNull("Empty ARN not loaded", CusEntryNumber.Load(header, CusEntryNumberTypes.EU.ArrivalReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode));
		CusEntryNumber.LoadOrCreate(Header, CusEntryNumberTypes.EU.ArrivalReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode).CE_EntryNum = "ARN123";
		AssertEquals("CE_EntryNum provided", "ARN123", Header.ArrivalReferenceNumber);
	});

	public void TestIsNationalTransit() => CombineAssertions(() =>
	{
		AssertEquals("MRN empty", false, Header.IsNationalTransit);

		Header.MovementReferenceNumberSetter("23DEXXXXXXXXXXXXN1");
		AssertEquals("MRN wrong", false, Header.IsNationalTransit);

		Header.MovementReferenceNumberSetter("23CHXXXXXXXXXXXXX1");
		AssertEquals("MRN wrong", false, Header.IsNationalTransit);

		Header.MovementReferenceNumberSetter("23CHXXXXXXXXXXXXN1");
		AssertEquals("MRN valid CH", true, Header.IsNationalTransit);
	});

	public void TestSetInitialCustomsOffice() => CombineAssertions(() =>
	{
		var header = Header;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = header.MovementHeader;

		AssertEquals("Initial Customs Offices Count.", 1, movementHeader.CustomsOffices.Count);
		AssertEquals($"Customs office '{OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination}' added.", 1, movementHeader.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination).Length);
	});

	public void TestNctsOfficeOfTransitHasToBeAdded() => CombineAssertions(() =>
	{
		var header = Header;

		new RefDataTestHelper(Factory).CreateNctsDeclarationTypeList();
		header.Consignor.E2_AddressOverride = ZBool.True;
		header.Consignor.E2_RN_NKCountryCode = Constants.CountryCodes.Switzerland;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = Header.MovementHeader;

		movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		header.BH_RL_NKImportLoadPort = "CH";
		AssertEquals($"TRA unavailable if Entry Type is '{movementHeader.BM_InBondEntryType}' and BH_RL_NKImportLoadPort is '{header.BH_RL_NKImportLoadPort}'.", false, movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));

		header.BH_RL_NKImportLoadPort = ZString.Empty;
		header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		header.BH_RL_NKImportLoadPort = "CH";
		AssertEquals($"TRA available if Entry Type is not 'T-CH' and Consignor Country Code is '{header.BH_RL_NKImportLoadPort}'.", true, movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));
	});

	public void TestPopulateGuaranteeFromPrincipal() => CombineAssertions(() =>
	{
		Header.SetMovementType(NctsMovementType.Codes.Departure);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

		var cusGuaranteeHeader = CreateGuarantee("GRN1234");

		Header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;

		var movementHeader = Header.MovementHeader;

		AssertEquals("Precondition", 1, movementHeader.Guarantees.Count);

		AssertEquals("PW_BondType", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, movementHeader.Guarantees[0].PW_BondType);
		AssertEquals("PW_BondNumber", "GRN1234", movementHeader.Guarantees[0].PW_BondNumber);
		AssertEquals("PW_Password", "PIN1", movementHeader.Guarantees[0].PW_Password);

		Header.Principal.E2_OA_Address = ZGuid.Empty;
		cusGuaranteeHeader.MainAccessCode = "PIN2";
		Header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;

		AssertEquals("PW_Password updated", "PIN2", movementHeader.Guarantees[0].PW_Password);

		Header.Principal.E2_OA_Address = ZGuid.Empty;
		movementHeader.Guarantees.Clear();
		movementHeader.Guarantees.AddNew().PW_Password = "PIN0";
		Header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
		AssertEquals("Header.Guarantees not empty / no new Guarantee added", "PIN0", movementHeader.Guarantees[0].PW_Password);
		movementHeader.Guarantees.Clear();

		var cusGuaranteeHeader2 = CreateGuarantee("GRN5678", EU.Business.PermitRuleCodeList.Codes.ADD);

		Header.Principal.E2_OA_Address = ZGuid.Empty;
		Header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
		AssertEquals("Principal has multiple guarantees / nothing added", 0, movementHeader.Guarantees.Count);

		CusGuaranteeHeader CreateGuarantee(string number, string ruleCode = null)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			guaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_SubType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
			guaranteeHeader.CPH_Number = number;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			guaranteeHeader.MainAccessCode = "PIN1";
			if (ruleCode != null)
			{
				var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
				rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
				rule.CPR_ValueFrom = "#1";
			}
			return guaranteeHeader;
		}
	});

	public void TestAdditionalDocuments()
	{
		AssertType<NctsAdditionalInfo>(Header.AdditionalDocuments.AddNew());
	}

	public void TestActivationDeadline()
	{
		AssertEquals("Without CusEntryNumberMRN", ZDateTime.Empty, Header.MovementReferenceEntryNumber.CE_ExpiryDate);

		var cusEntryNumber = CusEntryNumber.LoadOrCreate(Header, CusEntryNumberTypes.Standard.ClearancePermitNumber, CountryCodes.Switzerland);
		cusEntryNumber.CE_ExpiryDate = ZDateTime.Now;
		AssertEquals("With CusEntryNumber with Type != MRN", ZDateTime.Empty, Header.MovementReferenceEntryNumber.CE_ExpiryDate);

		var expiryDate = ZDateTime.Now.AddYears(1);
		Header.MovementReferenceNumberSetter("mrn", ZDateTime.Now, expiryDate);
		AssertEquals("With CusEntryNumber with Type MRN", expiryDate, Header.MovementReferenceEntryNumber.CE_ExpiryDate);
	}

	public void TestGetLinkedNctsHeader() => CombineAssertions(() =>
	{
		var departure = CreateNctsHeader(NctsMovementType.Codes.Departure);
		var arrival = CreateNctsHeader(NctsMovementType.Codes.Arrival);

		AssertNull("Input: Null", NctsHeader.GetLinkedNctsHeader(null));
		AssertNull("Input: NctsArrivalCargoDesc", NctsHeader.GetLinkedNctsHeader(Factory.New<NctsArrivalCargoDesc>()));
		AssertNull("Input: ArrivalMovementHeader", NctsHeader.GetLinkedNctsHeader(arrival.ArrivalMovementHeader));
		AssertEquals("Input: NctsHeader (Arrival)", arrival, NctsHeader.GetLinkedNctsHeader(arrival));
		AssertEquals("Input: NctsHeader (Departure)", departure, NctsHeader.GetLinkedNctsHeader(departure));
		AssertEquals("Input: MovementHeader", departure, NctsHeader.GetLinkedNctsHeader(departure.MovementHeader));

		NctsHeader CreateNctsHeader(string movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(movementType);
			return header;
		}
	});

		public void TestExistNonDECEntry() => CombineAssertions(() =>
		{
			Header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);

		AssertEquals("No container", false, Header.ExistNonDECEntry);

		var container1 = Header.ArrivalHeaderContainers.AddNew();
		container1.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;

		var container2 = Header.ArrivalHeaderContainers.AddNew();
		container2.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals($"BC_UnloadedState={container2.BC_UnloadedState}", true, Header.ExistNonDECEntry);
		container2.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals($"BC_UnloadedState={container2.BC_UnloadedState}", true, Header.ExistNonDECEntry);
		container2.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"BC_UnloadedState={container2.BC_UnloadedState}", true, Header.ExistNonDECEntry);
		container2.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals($"BC_UnloadedState={container2.BC_UnloadedState}", false, Header.ExistNonDECEntry);

		var seal1 = container1.Seals.AddNew();
		seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;

		var seal2 = container1.Seals.AddNew();
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals($"BK_UnloadingState={seal2.BK_UnloadingState}", true, Header.ExistNonDECEntry);
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals($"BK_UnloadingState={seal2.BK_UnloadingState}", true, Header.ExistNonDECEntry);
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"BK_UnloadingState={seal2.BK_UnloadingState}", true, Header.ExistNonDECEntry);
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals($"BK_UnloadingState={seal2.BK_UnloadingState}", false, Header.ExistNonDECEntry);
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
		AssertEquals($"BK_UnloadingState={seal2.BK_UnloadingState}", false, Header.ExistNonDECEntry);

		var bill = Header.Bills.AddNew();
		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals($"B9_UnloadedState={bill.MovementDetail.B9_UnloadedState} (EU-check)", true, Header.ExistNonDECEntry);
		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;

		var supportingDocument = bill.AdditionalDocuments.AddNew();
		supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"CSI_Status={supportingDocument.CSI_Status} (supportingDocument, EU-check)", true, Header.ExistNonDECEntry);
		supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;

		var additionalDocument = bill.AdditionalDocuments.AddNew();
		additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"CSI_Status={additionalDocument.CSI_Status} (additionalDocument, EU-check)", true, Header.ExistNonDECEntry);
		additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;

		var goodsItem = bill.ArrivalGoodsItems.AddNew();
		goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"BY_UnloadedState={goodsItem.BY_UnloadedState} (EU-check)", true, Header.ExistNonDECEntry);
		goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

		var package = goodsItem.Packages.AddNew();
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"B5_TypeOfDifference={package.B5_TypeOfDifference} (EU-check)", true, Header.ExistNonDECEntry);
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

		AssertEquals($"All DEC", false, Header.ExistNonDECEntry);
	});

	public void TestSetAllUnloadedStateToDEC() => CombineAssertions(() =>
	{
		const string OrgContainerMode = "CNT";
		const string NewContainerMode = "NCT";
		const string OrgContainerNum = "ORG-NUM";
		const string NewContainerNum = "NEW-NUM";
		const string OrgSealNumber = "ORG-SEAL";
		const string NewSealNumber = "NEW-SEAL";

			Header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);

		var containerDIF = CreateContainer(NctsUnloadedStateList.Codes.DIF);
		var containerMIS = CreateContainer(NctsUnloadedStateList.Codes.MIS);
		var containerNEW = CreateContainer(NctsUnloadedStateList.Codes.NEW);
		var containerDEC = CreateContainer(NctsUnloadedStateList.Codes.DEC);

		var sealDECDIF = CreateSeal(containerDEC, NctsUnloadedStateList.Codes.DIF);
		var sealDECMIS = CreateSeal(containerDEC, NctsUnloadedStateList.Codes.MIS);
		var sealDECNEW = CreateSeal(containerDEC, NctsUnloadedStateList.Codes.NEW);
		var sealDECDEC = CreateSeal(containerDEC, NctsUnloadedStateList.Codes.DEC);
		var sealDECDAM = CreateSeal(containerDEC, NctsUnloadedStateList.Codes.DAM);

		var sealDIFDIF = CreateSeal(containerDIF, NctsUnloadedStateList.Codes.DIF);
		var sealDIFMIS = CreateSeal(containerDIF, NctsUnloadedStateList.Codes.MIS);
		var sealDIFNEW = CreateSeal(containerDIF, NctsUnloadedStateList.Codes.NEW);
		var sealDIFDEC = CreateSeal(containerDIF, NctsUnloadedStateList.Codes.DEC);
		var sealDIFDAM = CreateSeal(containerDIF, NctsUnloadedStateList.Codes.DAM);

		var sealMISDIF = CreateSeal(containerMIS, NctsUnloadedStateList.Codes.DIF);
		var sealMISMIS = CreateSeal(containerMIS, NctsUnloadedStateList.Codes.MIS);
		var sealMISNEW = CreateSeal(containerMIS, NctsUnloadedStateList.Codes.NEW);
		var sealMISDEC = CreateSeal(containerMIS, NctsUnloadedStateList.Codes.DEC);
		var sealMISDAM = CreateSeal(containerMIS, NctsUnloadedStateList.Codes.DAM);

		var bill = Header.Bills.AddNew();
		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;

		var supportingDocument = bill.AdditionalDocuments.AddNew();
		supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;

		var additionalDocument = bill.AdditionalDocuments.AddNew();
		additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;

		var goodsItem = bill.ArrivalGoodsItems.AddNew();
		goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		var package = goodsItem.Packages.AddNew();
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;

		Factory.Save();
		Update();

		Header.SetAllUnloadedStateToDEC();

		AssertUnloadedState("was DEC", containerDEC.BC_UnloadedStateInfo);
		AssertUnloadedState("was DIF", containerDIF.BC_UnloadedStateInfo, valuesRestored: true);
		AssertUnloadedState("was MIS", containerMIS.BC_UnloadedStateInfo);
		AssertDeleted("was NEW", containerNEW.BC_UnloadedStateInfo);

		AssertUnloadedState("was DEC (container was DEC)", sealDECDEC.BK_UnloadingStateInfo);
		AssertUnloadedState("was DIF (container was DEC)", sealDECDIF.BK_UnloadingStateInfo, valuesRestored: true);
		AssertUnloadedState("was MIS (container was DEC)", sealDECMIS.BK_UnloadingStateInfo);
		AssertUnloadedState("was DAM (container was DEC)", sealDECDAM.BK_UnloadingStateInfo, NctsUnloadedStateList.Codes.DAM);
		AssertDeleted("was NEW (container was DEC)", sealDECNEW.BK_UnloadingStateInfo);

		AssertUnloadedState("was DEC (container was DIF)", sealDIFDEC.BK_UnloadingStateInfo);
		AssertUnloadedState("was DIF (container was DIF)", sealDIFDIF.BK_UnloadingStateInfo, valuesRestored: true);
		AssertUnloadedState("was MIS (container was DIF)", sealDIFMIS.BK_UnloadingStateInfo);
		AssertUnloadedState("was DAM (container was DIF)", sealDIFDAM.BK_UnloadingStateInfo, NctsUnloadedStateList.Codes.DAM);
		AssertDeleted("was NEW (container was DIF)", sealDIFNEW.BK_UnloadingStateInfo);

		AssertUnloadedState("was DEC (container was MIS)", sealMISDEC.BK_UnloadingStateInfo);
		AssertUnloadedState("was DIF (container was MIS)", sealMISDIF.BK_UnloadingStateInfo, valuesRestored: true);
		AssertUnloadedState("was MIS (container was MIS)", sealMISMIS.BK_UnloadingStateInfo);
		AssertUnloadedState("was DAM (container was MIS)", sealMISDAM.BK_UnloadingStateInfo, NctsUnloadedStateList.Codes.DAM);
		AssertDeleted("was NEW (container was MIS)", sealMISNEW.BK_UnloadingStateInfo);

		AssertUnloadedState("EU", bill.MovementDetail.B9_UnloadedStateInfo);
		AssertUnloadedState("supportingDocument, EU", supportingDocument.CSI_StatusInfo);
		AssertUnloadedState("additionalDocument, EU", additionalDocument.CSI_StatusInfo);
		AssertUnloadedState("EU", goodsItem.BY_UnloadedStateInfo);
		AssertUnloadedState("EU", package.B5_TypeOfDifferenceInfo);

		NctsArrivalHeaderContainer CreateContainer(string unloadingState)
		{
			var container = Header.ArrivalHeaderContainers.AddNew();
			container.BC_UnloadedState = unloadingState;
			container.BC_Mode = OrgContainerMode;
			container.BC_ContainerNum = OrgContainerNum;
			return container;
		}

		CusSeal CreateSeal(NctsArrivalHeaderContainer container, string unloadingState)
		{
			var seal = container.Seals.AddNew();
			seal.BK_UnloadingState = unloadingState;
			seal.BK_SealNumber = OrgSealNumber;
			return seal;
		}

		void Update()
		{
			foreach (var container in Header.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>())
			{
				container.BC_Mode = NewContainerMode;
				container.BC_ContainerNum = NewContainerNum;
				foreach (var seal in container.Seals.Cast<CusSeal>())
				{
					seal.BK_SealNumber = NewSealNumber;
				}
			}
		}

		void AssertUnloadedState(string assertionMessage, ZPropertyInfo unloadedStatePropertyInfo, string expectedUnloadingState = NctsUnloadedStateList.Codes.DEC, bool valuesRestored = false)
		{
			AssertEquals($"{unloadedStatePropertyInfo.Name} {assertionMessage}", expectedUnloadingState, unloadedStatePropertyInfo.Value);
			AssertEquals($"{unloadedStatePropertyInfo.Name} {assertionMessage} not deleted", false, unloadedStatePropertyInfo.BizObj.IsDeleted);
			if (unloadedStatePropertyInfo.BizObj is NctsArrivalHeaderContainer container)
			{
				AssertEquals($"BC_Mode {assertionMessage}", valuesRestored ? OrgContainerMode : NewContainerMode, container.BC_Mode);
				AssertEquals($"BC_ContainerNum {assertionMessage}", valuesRestored ? OrgContainerNum : NewContainerNum, container.BC_ContainerNum);
			}
			else if (unloadedStatePropertyInfo.BizObj is CusSeal seal)
			{
				AssertEquals($"BK_SealNumber {assertionMessage}", valuesRestored ? OrgSealNumber : NewSealNumber, seal.BK_SealNumber);
			}
		}

		void AssertDeleted(string assertionMessage, ZPropertyInfo unloadedTstatePropertyInfo)
		{
			AssertEquals($"{unloadedTstatePropertyInfo.BizObj.TableName} {assertionMessage} deleted", true, unloadedTstatePropertyInfo.BizObj.IsDeleted);
		}
	});

	public void TestDefaultTraderAtDestinationManager() => AssertType<NctsDefaultTraderAtDestinationManager>(Header.DefaultTraderAtDestinationManager);

	public void TestIsMessageStatusSent()
	{
		foreach (var messageStatus in new CHLogicalStatusList().GetAllCodes())
		{
			Header.EffectiveMessageStatus = messageStatus;
			AssertEquals($"MessageStatus={messageStatus}", messageStatus.In(CHLogicalStatusList.Codes.Sent, CHLogicalStatusList.Codes.Acknowledged), Header.IsMessageStatusSent);
		}
	}

	public void TestIsNationalTransportSwitzerland()
	{
		Header.SetMovementType(NctsMovementType.Codes.Departure);

		Header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertEquals("NationalTransitSwitzerland false", false, Header.IsNationalTransitSwitzerland);

		Header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertEquals("NationalTransitSwitzerland true", true, Header.IsNationalTransitSwitzerland);
	}

	public void TestMovementReferenceIssueDate() => CombineAssertions(() =>
	{
		AssertEquals("Neither MRN nor ARN provided", ZDateTime.Empty, Header.MovementReferenceIssueDate);

		Header.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2024, 5, 1);
		Header.MovementReferenceEntryNumber.CE_ExpiryDate = new ZDateTime(2024, 5, 2);
		AssertEquals("Issue Date from MRN", new ZDateTime(2024, 5, 1), Header.MovementReferenceIssueDate);

		Header.ArrivalReferenceEntryNumber.CE_IssueDate = new ZDateTime(2024, 5, 3);
		AssertEquals("ARN without ExpiryDate", ZDateTime.Empty, Header.MovementReferenceIssueDate);

		Header.ArrivalReferenceEntryNumber.CE_ExpiryDate = new ZDateTime(2024, 5, 4);
		AssertEquals("Expiry Date from ARN", new ZDateTime(2024, 5, 4), Header.MovementReferenceIssueDate);
	});

	public void TestPreviousDocumentsEXPOCount() => CombineAssertions(() =>
	{
		AssertEquals("No previous documents", 0, Header.PreviousDocumentsEXPOCount);
		var previousDocument = Header.PreviousDocuments.AddNew();
		AssertEquals("No EXPO previous documents", 0, Header.PreviousDocumentsEXPOCount);
		previousDocument.CSI_Code = PreviousDocumentCodes.Export;
		AssertEquals("1 EXPO previous documents", 1, Header.PreviousDocumentsEXPOCount);
	});

	public void TestGoodsItemsCount() => CombineAssertions(() =>
	{
		Header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertEquals("No goods items", 0, Header.GoodsItemsCount);
		var bill = Header.Bills.AddNew();
		bill.GoodsItems.AddNew();
		AssertEquals("1 Bill with 1 Goods item", 1, Header.GoodsItemsCount);
		bill.GoodsItems.AddNew();
		AssertEquals("1 Bill with 2 Goods item", 2, Header.GoodsItemsCount);
		bill = Header.Bills.AddNew();
		bill.GoodsItems.AddNew();
		AssertEquals("1 Bill with 2 Goods item and 1 Bill with 1 Goods item", 3, Header.GoodsItemsCount);
	});

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Header.DepartureHeaderContainers.AddNew();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => Header.DepartureHeaderContainers.AddNew();

	NctsHeader Header => header ?? (header = Factory.New<NctsHeader>());
	NctsHeader header;
}
