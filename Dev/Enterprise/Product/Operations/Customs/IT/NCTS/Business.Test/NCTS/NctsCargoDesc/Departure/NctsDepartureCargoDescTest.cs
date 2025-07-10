using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureCargoDesc))]
sealed class NctsDepartureCargoDescTest : NctsDepartureCargoDescAbstractTest<NctsHeader>
{
	public override int CountSupportingInfoTypes => base.CountSupportingInfoTypes + 1;

	public void TestGetCusSupportingInfoTypes_Remarks()
	{
		AssertEquals(typeof(CusSupportingInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)goodsItem).GetCusSupportingInfoTypes()[ITCusSupportingInfoTypeList.Codes.Remarks]);
	}

	public void TestClearCustomsDeletionStatus()
	{
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem.BY_Status = "DEL";
		goodsItem.ClearCustomsDeletionStatus();
		AssertNullOrEmpty("When status is DEL", goodsItem.BY_Status);

		goodsItem.BY_Status = "NBA";
		goodsItem.ClearCustomsDeletionStatus();
		AssertEquals("When status is not DEL", "", goodsItem.BY_Status);
	}

	public void TestLookups_Phase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		AssertType<NctsDepartureCargoDescPhase4Lookups>("When NCTS Phase 4, Lookups", goodsItem.Lookups);
	}

	public void TestLookups_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		AssertType<NctsDepartureCargoDescPhase5Lookups>("When NCTS Phase 5, Lookups", goodsItem.Lookups);
	}

	public void TestAdditionalInfoLineAllowedValidation_NonPhase5Departure()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		AssertNoRowErrorContaining(additionalInfo, ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowed);
		var additionalInfo2 = goodsItem.AdditionalInfos.AddNew();
		AssertHasRowErrorContaining(additionalInfo2, ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowed);
	}

	public void TestAdditionalInfoLineAllowedValidation_Phase5Departure()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		AssertNoRowErrorContaining(additionalInfo, ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowed);
		var additionalInfo2 = goodsItem.AdditionalInfos.AddNew();
		AssertNoRowErrorContaining(additionalInfo2, ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowed);
	}

	public void TestValidationType_Phase4()
	{
		nctsHeader.BH_ApplicationCode = "NCT";
		AssertType<NctsDepartureCargoDescPhase4Validation>(goodsItem.Validation);
	}

	public void TestValidationType_Phase5()
	{
		nctsHeader.BH_ApplicationCode = "NC5";
		AssertType<NctsDepartureCargoDescPhase5Validation>(goodsItem.Validation);
	}

	public void TestPreviousDocuments()
	{
		AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>(goodsItem.PreviousDocuments);
	}

	public void TestAeoCertificateManager()
	{
		var aeoCertificateManager = goodsItem.AeoCertificateManager;
		AssertNotNull("AeoCertificateManager must be never null", aeoCertificateManager);
		AssertSame("AeoCertificateManager must be cached", aeoCertificateManager, goodsItem.AeoCertificateManager);

		goodsItem.BY_ParentID = ZGuid.Empty;
		AssertNotNull("AeoCertificateManager must be never null also when GoodItem parent is null", aeoCertificateManager);
	}

	public void TestISupportingDocumentsProviderMembers()
	{
		goodsItem.SupportingDocuments.AddNew();

		var goodItemasISupportingDocumentsProvider = (ISupportingDocumentsProvider)goodsItem;

		AssertNotNull("ISupportingDocumentsProvider.SupportingDocuments must be not null", goodItemasISupportingDocumentsProvider.SupportingDocuments);
		AssertSame("ISupportingDocumentsProvider.SupportingDocuments must be the same of SupportingDocuments", goodsItem.SupportingDocuments, goodItemasISupportingDocumentsProvider.SupportingDocuments);
	}

	public void TestFees()
	{
		AssertNotNull("Fees collection not null", goodsItem.Fees);
		AssertType<NctsCargoDescFeeCollection>("Fees collection type", goodsItem.Fees);
	}

	public void TestDeleteNctsCargoDescWipeOutAlsoFeesCollection()
	{
		goodsItem.Fees.AddNew();
		goodsItem.Fees.AddNew();
		AssertEquals("PRE-CONDITION", 2, goodsItem.Fees.Count);

		goodsItem.Delete();
		AssertEquals("POST-CONDITION", 0, goodsItem.Fees.Count);
	}

	public void TestTotalTaxedAmount()
	{
		AssertEquals(nameof(goodsItem.TotalTaxedAmount), 0m, goodsItem.TotalTaxedAmount);

		goodsItem.Fees.AddNew().BFE_ChargeAmount = 10m;
		goodsItem.Fees.AddNew().BFE_ChargeAmount = 12.99m;
		AssertEquals(nameof(goodsItem.TotalTaxedAmount), 22.99m, goodsItem.TotalTaxedAmount);
	}

	public void TestIPackageProviderMembers()
	{
		CombineAssertions("When no packages found", () => AssertProperties(ZString.Empty, ZInt.Zero, ZString.Empty));

		var package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "PT";
		package.B5_MarksAndNumbers = "MARKS AND NOS";
		package.B5_UnitCount = 100;
		CombineAssertions("When single package found", () => AssertProperties("MARKS AND NOS", 100, "PT"));

		goodsItem.Packages.AddNew();
		CombineAssertions("When two packages, first is considered", () => AssertProperties("MARKS AND NOS", 100, "PT"));

		void AssertProperties(ZString marksAndNos, ZInt numberOfPackages, ZString packageType)
		{
			var packageProvider = (IPackageProvider)goodsItem;
			AssertEquals(nameof(packageProvider.MarksAndNumbers), marksAndNos, packageProvider.MarksAndNumbers);
			AssertEquals(nameof(packageProvider.NumberOfPackages), numberOfPackages, packageProvider.NumberOfPackages);
			AssertEquals(nameof(packageProvider.PackageType), packageType, packageProvider.PackageType);
		}
	}

	public void TestGroupedPreviousDocuments()
	{
		AssertNotNull(nameof(NctsDepartureCargoDesc.GroupedPreviousDocuments), goodsItem.GroupedPreviousDocuments);

		var previousDocument1 = goodsItem.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		AssertEquals($"{nameof(NctsDepartureCargoDesc.GroupedPreviousDocuments)} Count()", 0, goodsItem.GroupedPreviousDocuments.Count);

		var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "MRN";
		AssertEquals($"{nameof(NctsDepartureCargoDesc.GroupedPreviousDocuments)} Count()", 2, goodsItem.GroupedPreviousDocuments.Count);
	}

	public void TestIMergedPreviousDocumentsProviderMembers()
	{
		goodsItem.BY_LineNo = 2;
		goodsItem.PreviousDocuments.AddNew();
		goodsItem.PreviousDocuments.AddNew();
		goodsItem.BY_Status = "NBS";

		var mergedPreviousDocumentsProvider = goodsItem as IMergedPreviousDocumentsProvider;
		AssertNotNull($"{nameof(NctsDepartureCargoDesc)} must implement {nameof(IMergedPreviousDocumentsProvider)}", mergedPreviousDocumentsProvider);

		CombineAssertions($"Assert {nameof(IMergedPreviousDocumentsProvider)} properties", () =>
		{
			AssertEquals(nameof(IMergedPreviousDocumentsProvider.LineNumber), 2, mergedPreviousDocumentsProvider.LineNumber);
			AssertEquals($"{nameof(IMergedPreviousDocumentsProvider.MergedPreviousDocuments)} Count()", 2, mergedPreviousDocumentsProvider.MergedPreviousDocuments.Count());
			AssertEquals(nameof(IMergedPreviousDocumentsProvider.NBStatus), "NBS", mergedPreviousDocumentsProvider.NBStatus);
			AssertEquals(nameof(IMergedPreviousDocumentsProvider.IsExport), true, mergedPreviousDocumentsProvider.IsExport);
		});
	}

	public void TestCusProcedure()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "8000");
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "8071");
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "8071F51");

		goodsItem.BY_Procedure = "8000";
		var cusProcedure = goodsItem.CusProcedure;
		AssertEquals("ProcedureCode", "80", cusProcedure.ZZ6_ProcedureCode);
		AssertEquals("PreviousProcedureCode", "00", cusProcedure.ZZ6_PreviousProcedureCode);
		AssertEquals("Concession", "", cusProcedure.ZZ6_Concession);

		goodsItem.BY_Procedure = "8071";
		cusProcedure = goodsItem.CusProcedure;
		AssertEquals("ProcedureCode", "80", cusProcedure.ZZ6_ProcedureCode);
		AssertEquals("PreviousProcedureCode", "71", cusProcedure.ZZ6_PreviousProcedureCode);
		AssertEquals("Concession", "", cusProcedure.ZZ6_Concession);

		goodsItem.BY_Procedure = "8071F51";
		cusProcedure = goodsItem.CusProcedure;
		AssertEquals("ProcedureCode", "80", cusProcedure.ZZ6_ProcedureCode);
		AssertEquals("PreviousProcedureCode", "71", cusProcedure.ZZ6_PreviousProcedureCode);
		AssertEquals("Concession", "F51", cusProcedure.ZZ6_Concession);

		goodsItem.BY_Procedure = "XXYY";
		AssertNull(goodsItem.CusProcedure);

		goodsItem.BY_Procedure = "807";
		AssertNull(goodsItem.CusProcedure);

		goodsItem.BY_Procedure = "8";
		AssertNull(goodsItem.CusProcedure);

		goodsItem.BY_Procedure = "";
		AssertNull(goodsItem.CusProcedure);

		goodsItem.BY_Procedure = "8071F61";
		AssertNull(goodsItem.CusProcedure);
	}

	public void TestIsImport()
	{
		AssertEquals("NctsDepartureCargoDesc isImport should be false", false, goodsItem.IsImport);
	}

	public void TestIsExport()
	{
		AssertEquals("NctsDepartureCargoDesc isImport should be true", true, goodsItem.IsExport);
	}

	public void TestLineNumber()
	{
		goodsItem.BY_LineNo = ZShort.Zero;
		AssertEquals("LineNumber has the value zero", ZShort.Zero, goodsItem.LineNumber);

		goodsItem.BY_LineNo = 1234;
		AssertEquals("LineNumber has the value 1234", 1234, goodsItem.LineNumber);
	}

	public void TestEntryNumberWrapper()
	{
		CombineAssertions("EntryLine's EntryNumberWrapper is empty", () =>
		{
			var registrationInfoWrapper = goodsItem.EntryNumberWrapper;
			AssertEquals(nameof(registrationInfoWrapper.Register), ZString.Empty, registrationInfoWrapper.Register);
			AssertEquals(nameof(registrationInfoWrapper.RegistrationNumber), ZString.Empty, registrationInfoWrapper.RegistrationNumber);
			AssertEquals(nameof(registrationInfoWrapper.IssueDate), ZDate.Empty, registrationInfoWrapper.IssueDate);
			AssertEquals(nameof(registrationInfoWrapper.Series), ZString.Empty, registrationInfoWrapper.Series);
		});

		Factory.NewCusEntryNumber(goodsItem.Header, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "4 T-2343G", issueDate: new ZDateTime(2020, 01, 01));
		CombineAssertions("EntryLine's EntryNumberWrapper is present", () =>
		{
			var registrationInfoWrapper = goodsItem.EntryNumberWrapper;
			AssertEquals(nameof(registrationInfoWrapper.Register), "4", registrationInfoWrapper.Register);
			AssertEquals(nameof(registrationInfoWrapper.RegistrationNumber), "2343G", registrationInfoWrapper.RegistrationNumber);
			AssertEquals(nameof(registrationInfoWrapper.IssueDate), new ZDateTime(2020, 01, 01), registrationInfoWrapper.IssueDate);
			AssertEquals(nameof(registrationInfoWrapper.Series), "T", registrationInfoWrapper.Series);
		});
	}

	public void TestNBGroupedPreviousDocuments()
	{
		AssertNotNull(nameof(NctsDepartureCargoDesc.GroupedPreviousDocuments), goodsItem.NBGroupedPreviousDocuments);

		var previousDocument1 = goodsItem.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		AssertEquals($"{nameof(NctsDepartureCargoDesc.NBGroupedPreviousDocuments)} Count()", 0, goodsItem.NBGroupedPreviousDocuments.Count());

		var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "MRN";
		AssertEquals($"{nameof(NctsDepartureCargoDesc.NBGroupedPreviousDocuments)} Count()", 2, goodsItem.NBGroupedPreviousDocuments.Count());
	}

	public void TestAdditionalInfoCollectionType()
	{
		AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>($"{nameof(goodsItem.AdditionalInfos)} type", goodsItem.AdditionalInfos);
		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		AssertType<NctsAdditionalInfo>($"{nameof(goodsItem.AdditionalInfos)} elements type", additionalInfo);
	}

	public void TestAdditionalInfoCollectionMaxCount_NonPhase5Departure()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;

		AssertEquals(nameof(goodsItem.AdditionalInfos.MaxCount), 1, goodsItem.AdditionalInfos.MaxCount);
	}

	public void TestAdditionalInfoCollectionMaxCount_Phase5Departure()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		AssertEquals(nameof(goodsItem.AdditionalInfos.MaxCount), -1, goodsItem.AdditionalInfos.MaxCount);
	}

	public void TestRemark()
	{
		goodsItem.Remarks = "AAA";
		Factory.Save();

		var anotherFactory = new BusinessObjectFactory();
		var goodsItemReloaded = anotherFactory.Load<NctsDepartureCargoDesc>(goodsItem.PK);
		AssertEquals("Remarks", "AAA", goodsItemReloaded.Remarks);

		goodsItem.Remarks = "";
		Factory.Save();

		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK)
			.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix)
			.AddToFilter(CusSupportingInfoSchema.CSI_Type, "REM");

		var remSupportingInfo = Factory.LoadTop1<CusSupportingInfo>(query);
		AssertNull("When Remarks is empty, no related REM Supporting Info is expected", remSupportingInfo);
	}

	public void TestGetNewTariffFormatter()
	{
		var departureCargoDescForTest = Factory.New<NctsDepartureCargoDescForTest>();
		AssertType<EU.Business.TariffFormatterTen>(departureCargoDescForTest.GetNewTariffFormatterExposed());
	}

	public void TestSetFormattedTariffSetUnformattedTariff()
	{
		CombineAssertions(() =>
		{
			goodsItem.BY_FormattedHarmonisedTariff = "8007.00.10 00";
			AssertEquals(nameof(goodsItem.BY_HarmonisedTariff), "8007001000", goodsItem.BY_HarmonisedTariff);

			goodsItem.BY_FormattedHarmonisedTariff = "";
			AssertEquals(nameof(goodsItem.BY_HarmonisedTariff), "", goodsItem.BY_HarmonisedTariff);

			goodsItem.BY_FormattedHarmonisedTariff = "0106130000";
			AssertEquals(nameof(goodsItem.BY_HarmonisedTariff), "0106130000", goodsItem.BY_HarmonisedTariff);
		});
	}

	public void TestCloneInternal_CopyBY_Status()
	{
		goodsItem.BY_Status = "NBS";
		var clonedGoodsItem = (NctsDepartureCargoDesc)goodsItem.Clone();
		AssertNullOrEmpty("BY_Status should be empty", clonedGoodsItem.BY_Status);
	}

	public void TestBY_Status_Phase5CaptionAndDescription()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(goodsItem.BY_StatusInfo, "Status", "", "", "Goods item status");
	}

	public void TestBY_StatusReadOnly()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		AssertEquals("status.ReadOnly", true, goodsItem.BY_StatusInfo.ReadOnly);
	}

	public void TestActualCountryOfDispatch()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		AssertEquals(nameof(goodsItem.ActualCountryOfDispatch), "", goodsItem.ActualCountryOfDispatch);

		nctsHeader.BH_RL_NKImportLoadPort = "AU";
		AssertEquals(nameof(goodsItem.ActualCountryOfDispatch), "AU", goodsItem.ActualCountryOfDispatch);

		nctsHeader.BH_RL_NKImportLoadPort = "";
		goodsItem.BY_RN_NKCountryOfDispatch = "IT";
		AssertEquals(nameof(goodsItem.ActualCountryOfDispatch), "IT", goodsItem.ActualCountryOfDispatch);

		nctsHeader.BH_RL_NKImportLoadPort = "FR";
		AssertEquals(nameof(goodsItem.ActualCountryOfDispatch), "IT", goodsItem.ActualCountryOfDispatch);
	}

	public void TestPackage()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		goodsItem.Packages.RemoveAndDeleteAll();
		AssertNull("Package", goodsItem.Package);

		var package1 = goodsItem.Packages.AddNew();
		AssertSame("Package", package1, goodsItem.Package);

		goodsItem.Packages.AddNew();
		goodsItem.Packages.AddNew();
		AssertSame("Package", package1, goodsItem.Package);
	}

	public void TestAttachmentPrintingSupporter()
	{
		var attachmentPrintingSupporter = goodsItem.AttachmentPrintingSupporter;
		AssertType<NctsDepartureCargoDescAttachmentPrintingSupporter>(attachmentPrintingSupporter);
		AssertSame("Cached", attachmentPrintingSupporter, goodsItem.AttachmentPrintingSupporter);
	}

	public void TestCommodityCodeReadOnly()
	{
		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			AssertEquals("When Registry configuration is enabled, CommodityCodeReadOnly", false, goodsItem.CommodityCodeReadOnly);
		}

		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			AssertEquals("When Registry configuration is disabled, CommodityCodeReadOnly", true, goodsItem.CommodityCodeReadOnly);
		}
	}

	public void TestDefaultThirdQuantityUOMFromTariff_ExistingThirdUOM()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
		Factory.Save();

		var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
		Factory.Save();

		goodsItem.BY_CustomsThirdUnitQty = "AAA";
		goodsItem.BY_HarmonisedTariff = "2222222222";

		AssertEquals("Customs Third Unit Qty (BY_CustomsThirdUnitQty) change when it already has one set", "SSS", goodsItem.BY_CustomsThirdUnitQty);
	}

	public void TestDefaultFourthQuantityUOMFromTariff_ExistingThirdUOM()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
		Factory.Save();

		var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM4Type, "CCC");
		Factory.Save();

		goodsItem.BY_CustomsFourthUnitQty = "AAA";
		goodsItem.BY_HarmonisedTariff = "2222222222";

		AssertEquals("Customs Fourth Unit Qty change when it already has one set", "CCC", goodsItem.BY_CustomsFourthUnitQty);
	}

	public void TestCanDelete()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsDepartureMovementHeader = nctsHeader.MovementHeader;
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			nctsDepartureMovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("In declaration state", true, goodsItem.CanDelete);

			nctsDepartureMovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			AssertEquals("In amendment state", false, goodsItem.CanDelete);
		});
	}

	public void TestReasonForNotAbleToDelete()
	{
		const string expectedMessage = "It is not possible to delete a Goods Item in Amendment phase. You can request a deletion of the Goods Item setting its Status to DLR (deletion request)";

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsDepartureMovementHeader = nctsHeader.MovementHeader;
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

		nctsDepartureMovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
		AssertEquals(expectedMessage, goodsItem.ReasonForNotAbleToDelete);

		nctsDepartureMovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
		AssertNotEquals(expectedMessage, goodsItem.ReasonForNotAbleToDelete);
	}

	public void TestIsCustomsStatusDeleted()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.MovementHeader.Header.Bills.AddNew().GoodsItems.AddNew();

			goodsItem.BY_Status = ZString.Empty;
			AssertEquals("When BY_Status = ZString.Empty, IsCustomsStatusDeleted", false, goodsItem.IsCustomsStatusDeleted);

			goodsItem.BY_Status = "DLR";
			AssertEquals("When BY_Status = DLR, IsCustomsStatusDeleted", false, goodsItem.IsCustomsStatusDeleted);

			goodsItem.BY_Status = "DEL";
			AssertEquals("When BY_Status = DEL, IsCustomsStatusDeleted", true, goodsItem.IsCustomsStatusDeleted);
		});
	}

	public void TestIsCustomsStatusDeletionRequested()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.MovementHeader.Header.Bills.AddNew().GoodsItems.AddNew();

			goodsItem.BY_Status = ZString.Empty;
			AssertEquals("When BY_Status = ZString.Empty, IsCustomsStatusDeletionRequested", false, goodsItem.IsCustomsStatusDeletionRequested);

			goodsItem.BY_Status = "DEL";
			AssertEquals("When BY_Status = DEL, IsCustomsStatusDeletionRequested", false, goodsItem.IsCustomsStatusDeletionRequested);

			goodsItem.BY_Status = "DLR";
			AssertEquals("When BY_Status = DLR, IsCustomsStatusDeletionRequested", true, goodsItem.IsCustomsStatusDeletionRequested);
		});
	}

	public void TestSetAsCustomsDeletionRequest()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var goodsItem = nctsHeader.MovementHeader.Header.Bills.AddNew().GoodsItems.AddNew();

		goodsItem.SetAsCustomsDeletionRequest();
		AssertEquals("After calling SetAsCustomsDeletionRequest(), BY_Status", "DLR", goodsItem.BY_Status);

		goodsItem.BY_Status = "DEL";
		goodsItem.SetAsCustomsDeletionRequest();
		AssertEquals("After calling SetAsCustomsDeletionRequest(), BY_Status", "DLR", goodsItem.BY_Status);
	}

	public void TestResetBY_DeclarationGoodsItemNumberOnSaving_Phase5()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var bill1 = nctsHeader.Bills.AddNew();
		var goodItem11 = bill1.GoodsItems.AddNew();
		goodItem11.BY_DeclarationGoodsItemNumber = 1;
		var goodItem12 = bill1.GoodsItems.AddNew();
		goodItem12.BY_DeclarationGoodsItemNumber = 2;

		var bill2 = nctsHeader.Bills.AddNew();
		var goodItem21 = bill2.GoodsItems.AddNew();
		goodItem21.BY_DeclarationGoodsItemNumber = 3;
		var goodItem22 = bill2.GoodsItems.AddNew();
		goodItem22.BY_DeclarationGoodsItemNumber = 4;

		CombineAssertions(() =>
		{
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 before saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 before saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 before saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 before saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			Factory.Save();
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			var goodItem23 = bill2.GoodsItems.AddNew();
			Factory.Save();
			AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
			AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem23.BY_DeclarationGoodsItemNumber is 0 after saving, new item", 0, goodItem23.BY_DeclarationGoodsItemNumber);

			goodItem23.BY_DeclarationGoodsItemNumber = 5;
			Factory.Save();
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after second saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after second saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after second saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after second saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
			AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem23.BY_DeclarationGoodsItemNumber is not 0 after second saving", 5, goodItem23.BY_DeclarationGoodsItemNumber);

			goodItem21.BY_DeclarationGoodsItemNumber = 0;
			Factory.Save();
			AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem21.BY_DeclarationGoodsItemNumber is 0 after saving, the item changed", 0, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
			AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem23.BY_DeclarationGoodsItemNumber is not 0 after saving", 5, goodItem23.BY_DeclarationGoodsItemNumber);
		});
	}

	public void TestResetBY_DeclarationGoodsItemNumberOnDelete_Phase5()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var bill1 = nctsHeader.Bills.AddNew();
		var goodItem11 = bill1.GoodsItems.AddNew();
		goodItem11.BY_DeclarationGoodsItemNumber = 1;
		var goodItem12 = bill1.GoodsItems.AddNew();
		goodItem12.BY_DeclarationGoodsItemNumber = 2;

		var bill2 = nctsHeader.Bills.AddNew();
		var goodItem21 = bill2.GoodsItems.AddNew();
		goodItem21.BY_DeclarationGoodsItemNumber = 3;
		var goodItem22 = bill2.GoodsItems.AddNew();
		goodItem22.BY_DeclarationGoodsItemNumber = 4;

		CombineAssertions(() =>
		{
			AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			goodItem11.Delete();
			AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			Factory.Save();
			goodItem12.Delete();
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem12, when in database", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem12, when in database", 4, goodItem22.BY_DeclarationGoodsItemNumber);
		});
	}

	public void TestBill()
	{
		var goodItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		AssertType<NctsBill>(goodItem.Bill);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
	}

	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;

	protected override ZString CountryCode => Core.Constants.CountryCodes.Italy;

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		var goodsItem = (NctsDepartureCargoDesc)base.GetNewBusinessObjectForDefaultLightValidationTest();
		goodsItem.Consignor.OrganisationPK = Factory.New<OrgHeader>().PK;
		return goodsItem;
	}
}

sealed class NctsCargoDescTraderValidationTest : NctsTraderValidationTest
{
	protected override JobDocAddress GetConsignorAddress()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.Bills.AddNew().GoodsItems.AddNew().Consignor;
	}
}

class NctsDepartureCargoDescForTest : NctsDepartureCargoDesc
{
	public NctsDepartureCargoDescForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public TariffFormatter GetNewTariffFormatterExposed() => GetNewTariffFormatter();
}
