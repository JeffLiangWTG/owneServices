using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDesc))]
	sealed class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		public void TestBY_Description_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals(512, goodsItemOnBill.BY_DescriptionInfo.MaxLength);
			});
		}

		public void TestBY_Description_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(280, goodsItemOnBill.BY_DescriptionInfo.MaxLength);
			});
		}

		public void TestValidation()
		{
			AssertType<NctsDepartureCargoDescValidation>(goodsItemOnBill.Validation);
		}

		public void TestPackages()
		{
			AssertType<NctsPackageCollection>(goodsItemOnBill.Packages);
		}

		public void TestPreviousProcedureMaster()
		{
			AssertSame(goodsItemOnBill, goodsItemOnBill.PreviousProcedureMaster.Parent);
		}

		public void TestINctsPreviousProcedureParentProvider()
		{
			var previousProcedureParentProvider = goodsItemOnBill as INctsPreviousProcedureParentProvider;
			CombineAssertions(() =>
			{
				AssertSame("PreviousProcedures", goodsItemOnBill.PreviousProcedures, previousProcedureParentProvider.PreviousProcedures);
				AssertSame("PreviousDocuments", goodsItemOnBill.PreviousDocuments, previousProcedureParentProvider.PreviousDocuments);
				AssertSame("NctsHeader", goodsItemOnBill.Header, previousProcedureParentProvider.NctsHeader);
			});
		}

		public void TestPreviousProcedures()
		{
			var previousProcedures = goodsItemOnBill.PreviousProcedures;
			CombineAssertions(() =>
			{
				AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>("Type", previousProcedures);
				AssertEquals("IsRegisteredEditableChildObject", true, goodsItemOnBill.IsRegisteredEditableChildObject(previousProcedures));
			});
		}

		public void TestPreviousDocuments()
		{
			var previousDocuments = goodsItemOnBill.PreviousDocuments;
			CombineAssertions(() =>
			{
				AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>("Type", previousDocuments);
				AssertEquals("IsRegisteredEditableChildObject", true, goodsItemOnBill.IsRegisteredEditableChildObject(previousDocuments));
			});
		}

		public void TestDeclarationTypeEffective()
		{
			CombineAssertions(() =>
			{
				departureMovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
				AssertEquals("BM_InBondEntryType is T2", NctsDeclarationTypeList.Codes.T2, goodsItemOnBill.DeclarationTypeEffective);

				departureMovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T;
				AssertEquals("BM_InBondEntryType is T", NctsDeclarationTypeList.Codes.T, goodsItemOnBill.DeclarationTypeEffective);

				goodsItemOnBill.BY_Type = NctsDeclarationTypeList.Codes.T2F;
				AssertEquals("BM_InBondEntryType is T and BY_Type is T2F", NctsDeclarationTypeList.Codes.T2F, goodsItemOnBill.DeclarationTypeEffective);
			});
		}

		public void TestAdditionalInfos()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>("Type", goodsItemOnBill.AdditionalInfos);
				AssertEquals("IsRegisteredEditableChildObject", true, goodsItemOnBill.IsRegisteredEditableChildObject(goodsItemOnBill.AdditionalInfos));
			});
		}

		public void TestEffectiveMethodOfPayment_Null()
		{
			AssertEffectiveStringValueNull(goodsItemOnBill.BY_TransportChargesMethodOfPaymentInfo, bill.B0_TransportPaymentMethodInfo, departureMovementHeader.BM_MethodOfPaymentInfo, (x) => x.EffectiveMethodOfPayment);
		}

		public void TestEffectiveMethodOfPayment_CargoDesc()
		{
			AssertEffectiveStringValueGoodsItem(goodsItemOnBill.BY_TransportChargesMethodOfPaymentInfo, bill.B0_TransportPaymentMethodInfo, departureMovementHeader.BM_MethodOfPaymentInfo, (x) => x.EffectiveMethodOfPayment);
		}

		public void TestEffectiveMethodOfPayment_Bill()
		{
			AssertEffectiveStringValueBill(goodsItemOnBill.BY_TransportChargesMethodOfPaymentInfo, bill.B0_TransportPaymentMethodInfo, departureMovementHeader.BM_MethodOfPaymentInfo, (x) => x.EffectiveMethodOfPayment);
		}

		public void TestEffectiveMethodOfPayment_Header()
		{
			AssertEffectiveStringValueHeader(goodsItemOnBill.BY_TransportChargesMethodOfPaymentInfo, bill.B0_TransportPaymentMethodInfo, departureMovementHeader.BM_MethodOfPaymentInfo, (x) => x.EffectiveMethodOfPayment);
		}

		public void TestEffectiveMethodOfPayment_Cached()
		{
			AssertEffectiveStringValueCached(goodsItemOnBill.BY_TransportChargesMethodOfPaymentInfo, (x) => x.EffectiveMethodOfPayment);
		}

		public void TestEffectiveCountryOfDestination_Null()
		{
			AssertEffectiveStringValueNull(goodsItemOnBill.BY_RN_NKCountryOfDestinationInfo, bill.B0_RN_NKCountryOfDestinationInfo, departureMovementHeader.BM_RL_NKDestinationPortInfo, (x) => x.EffectiveCountryOfDestination);
		}

		public void TestEffectiveCountryOfDestination_CargoDesc()
		{
			AssertEffectiveStringValueGoodsItem(goodsItemOnBill.BY_RN_NKCountryOfDestinationInfo, bill.B0_RN_NKCountryOfDestinationInfo, departureMovementHeader.BM_RL_NKDestinationPortInfo, (x) => x.EffectiveCountryOfDestination);
		}

		public void TestEffectiveCountryOfDestination_Bill()
		{
			AssertEffectiveStringValueBill(goodsItemOnBill.BY_RN_NKCountryOfDestinationInfo, bill.B0_RN_NKCountryOfDestinationInfo, departureMovementHeader.BM_RL_NKDestinationPortInfo, (x) => x.EffectiveCountryOfDestination);
		}

		public void TestEffectiveCountryOfDestination_Header()
		{
			AssertEffectiveStringValueHeader(goodsItemOnBill.BY_RN_NKCountryOfDestinationInfo, bill.B0_RN_NKCountryOfDestinationInfo, departureMovementHeader.BM_RL_NKDestinationPortInfo, (x) => x.EffectiveCountryOfDestination);
		}

		public void TestEffectiveCountryOfDestination_Cached()
		{
			AssertEffectiveStringValueCached(goodsItemOnBill.BY_RN_NKCountryOfDestinationInfo, (x) => x.EffectiveCountryOfDestination);
		}

		public void TestEffectiveCountryOfDispatch_Null()
		{
			AssertEffectiveStringValueNull(goodsItemOnBill.BY_RN_NKCountryOfDispatchInfo, bill.B0_RN_NKCountryOfExportInfo, departureMovementHeader.BM_RN_NKCountryOfDispatchInfo, (x) => x.EffectiveCountryOfDispatch);
		}

		public void TestEffectiveCountryOfDispatch_CargoDesc()
		{
			AssertEffectiveStringValueGoodsItem(goodsItemOnBill.BY_RN_NKCountryOfDispatchInfo, bill.B0_RN_NKCountryOfExportInfo, departureMovementHeader.BM_RN_NKCountryOfDispatchInfo, (x) => x.EffectiveCountryOfDispatch);
		}

		public void TestEffectiveCountryOfDispatch_Bill()
		{
			AssertEffectiveStringValueBill(goodsItemOnBill.BY_RN_NKCountryOfDispatchInfo, bill.B0_RN_NKCountryOfExportInfo, departureMovementHeader.BM_RN_NKCountryOfDispatchInfo, (x) => x.EffectiveCountryOfDispatch);
		}

		public void TestEffectiveCountryOfDispatch_Header()
		{
			AssertEffectiveStringValueHeader(goodsItemOnBill.BY_RN_NKCountryOfDispatchInfo, bill.B0_RN_NKCountryOfExportInfo, departureMovementHeader.BM_RN_NKCountryOfDispatchInfo, (x) => x.EffectiveCountryOfDispatch);
		}

		public void TestEffectiveCountryOfDispatch_Cached()
		{
			AssertEffectiveStringValueCached(goodsItemOnBill.BY_RN_NKCountryOfDispatchInfo, (x) => x.EffectiveCountryOfDispatch);
		}

		public void TestEffectiveReferenceNumberUCR_Null()
		{
			AssertEffectiveStringValueNull(goodsItemOnBill.BY_CommercialReferenceNumberInfo, bill.B0_ReferenceIDInfo, departureMovementHeader.BM_UniqueConsignmentReferenceInfo, (x) => x.EffectiveReferenceNumberUCR);
		}

		public void TestEffectiveReferenceNumberUCR_CargoDesc()
		{
			AssertEffectiveStringValueGoodsItem(goodsItemOnBill.BY_CommercialReferenceNumberInfo, bill.B0_ReferenceIDInfo, departureMovementHeader.BM_UniqueConsignmentReferenceInfo, (x) => x.EffectiveReferenceNumberUCR);
		}

		public void TestEffectiveReferenceNumberUCR_Bill()
		{
			AssertEffectiveStringValueBill(goodsItemOnBill.BY_CommercialReferenceNumberInfo, bill.B0_ReferenceIDInfo, departureMovementHeader.BM_UniqueConsignmentReferenceInfo, (x) => x.EffectiveReferenceNumberUCR);
		}

		public void TestEffectiveReferenceNumberUCR_Header()
		{
			AssertEffectiveStringValueHeader(goodsItemOnBill.BY_CommercialReferenceNumberInfo, bill.B0_ReferenceIDInfo, departureMovementHeader.BM_UniqueConsignmentReferenceInfo, (x) => x.EffectiveReferenceNumberUCR);
		}

		public void TestEffectiveReferenceNumberUCR_Cached()
		{
			AssertEffectiveStringValueCached(goodsItemOnBill.BY_CommercialReferenceNumberInfo, (x) => x.EffectiveReferenceNumberUCR);
		}

		public void TestEffectiveConsignee_Null()
		{
			AssertNull(goodsItemOnBill.EffectiveConsignee);
		}

		public void TestEffectiveConsignee_CargoDesc()
		{
			var consigneeCargoDesc = goodsItemOnBill.DocAddresses.CreateWithRequirement(goodsItemOnBill.ConsigneeJobDocAddressRequirement);
			consigneeCargoDesc.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;
			var consigneeBill = bill.DocAddresses.CreateWithRequirement(bill.ConsigneeJobDocAddressRequirement);
			consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
			var consigneeHeader = header.DocAddresses.CreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			CombineAssertions(() =>
			{
				var result = goodsItemOnBill.EffectiveConsignee;
				AssertEquals("result", "ConsigneeCargoDesc", result.CompanyName);
				AssertSame("Cached", result, goodsItemOnBill.EffectiveConsignee);
			});
		}

		public void TestEffectiveConsignee_Bill()
		{
			var consigneeBill = bill.DocAddresses.CreateWithRequirement(bill.ConsigneeJobDocAddressRequirement);
			consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
			var consigneeHeader = header.DocAddresses.CreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			AssertEquals("ConsigneeBill", goodsItemOnBill.EffectiveConsignee.CompanyName);
		}

		public void TestEffectiveConsignee_Header()
		{
			var consigneeHeader = header.DocAddresses.CreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			header.Consignee.E2_OA_Address = consigneeHeader.E2_OA_Address;
			AssertEquals("ConsigneeHeader", goodsItemOnBill.EffectiveConsignee.CompanyName);
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			var previousProcedure1 = goodsItemOnBill.PreviousProcedures.AddNew();
			var previousProcedure2 = goodsItemOnBill.PreviousProcedures.AddNew();

			Factory.Save();
			Assert("PreReq: PreviousProcedure1 deleted", previousProcedure1.IsDeleted);
			Assert("PreReq: PreviousProcedure2 deleted", previousProcedure2.IsDeleted);
			CombineAssertions(() => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_CSI_Tariff_ProductWithTariff()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			var previousProcedure = goodsItemOnBill.PreviousProcedures.AddNew();
			previousProcedure.CSI_Tariff = "22441133";

			Factory.Save();
			Assert("PreReq: PreviousProcedure deleted", previousProcedure.IsDeleted);
			CombineAssertions(() => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes, expectedTariff: "11223344"));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_CSI_Tariff_ProductWithoutTariff()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345", linkToProduct: true, importTariff: string.Empty);
			var previousProcedure = goodsItemOnBill.PreviousProcedures.AddNew();
			previousProcedure.CSI_Tariff = "22441133";

			Factory.Save();
			Assert("PreReq: PreviousProcedure deleted", previousProcedure.IsDeleted);
			CombineAssertions(() => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes, expectedTariff: "22441133"));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_CSI_Tariff_NoProduct()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345", linkToProduct: false);
			var previousProcedure = goodsItemOnBill.PreviousProcedures.AddNew();
			previousProcedure.CSI_Tariff = "22441133";

			Factory.Save();
			Assert("PreReq: PreviousProcedure deleted", previousProcedure.IsDeleted);
			CombineAssertions(() => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes, expectedTariff: "22441133"));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_AuthorizationNumber_ExistsBefore()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			var previousProcedureMaster = goodsItemOnBill.PreviousProcedureMaster;
			var previousProcedure = goodsItemOnBill.PreviousProcedures.AddNew();
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			Factory.Save();
			CombineAssertions("AuthorizationNumber was not set before with Procedure 9DEZ", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes));

			previousProcedureMaster.AuthorizationNumber = "Auth123";
			Factory.Save();
			CombineAssertions("AuthorizationNumber was set before with Procedure 9DEZ", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, "Auth123", "ATC710123456789012345", YesNoList.Codes.Yes));

			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			previousProcedureMaster.AuthorizationNumber = "Auth123";
			Factory.Save();
			CombineAssertions("AuthorizationNumber was set before with Procedure != 9DEZ", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_AuthorizationNumber_LookupResult()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			var previousProcedureMaster = goodsItemOnBill.PreviousProcedureMaster;
			var previousProcedure = goodsItemOnBill.PreviousProcedures.AddNew();

			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			previousProcedureMaster.AuthorizationNumber = "Auth123";
			CreateConsignorWithAuthorization();
			Factory.Save();
			CombineAssertions("AuthorizationNumber was set before with Procedure 9DEZ; 1 Lookup result", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, "Auth123", "ATC710123456789012345", YesNoList.Codes.Yes));

			previousProcedureMaster.AuthorizationNumber = ZString.Empty;
			Factory.Save();
			CombineAssertions("AuthorizationNumber was not set before; 1 Lookup result => consignor", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, "AUTH_NUMBER_CONSIGNOR", "ATC710123456789012345", YesNoList.Codes.Yes));

			previousProcedureMaster.AuthorizationNumber = ZString.Empty;
			CreatePrincipalWithAuthorization();
			Factory.Save();
			CombineAssertions("AuthorizationNumber was not set before; 2 Lookup results", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "ATC710123456789012345", YesNoList.Codes.Yes));

			header.Consignor.E2_OA_Address = ZGuid.Empty;
			Factory.Save();
			CombineAssertions("AuthorizationNumber was not set before; 1 Lookup result => principal", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, "AUTH_NUMBER_PRINCIPAL", "ATC710123456789012345", YesNoList.Codes.Yes));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_EntryViaAtlasFlag()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("XXXXXX");
			Factory.Save();
			CombineAssertions($"BY_WarehouseEntryNumber != PreviousEntryIsATLASSet", () => AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, ZString.Empty, "XXXXXX", YesNoList.Codes.No));
			var testcase = (ZString warehouseEntryNumber) =>
			{
				goodsItemOnBill.BY_WarehouseEntryNumber = warehouseEntryNumber;
				Factory.Save();
				return (bool)goodsItemOnBill.PreviousProcedures.Single().Status;
			};

			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForBondedWarehouse_MRN(testcase);
			PreviousDocumentHelperTest.AssertIsValidAtlasReferenceForBondedWarehouse(testcase);
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_BY_WarehouseEntryNumber_Empty()
		{
			PrepareBusinessObjectsForOnFactorySavingTest(ZString.Empty);
			var previousProcedure1 = goodsItemOnBill.PreviousProcedures.AddNew();
			var previousProcedure2 = goodsItemOnBill.PreviousProcedures.AddNew();
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { previousProcedure1.PK, previousProcedure2.PK }, goodsItemOnBill.PreviousProcedures.Select(x => x.PK));
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_HasChanges()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			var previousProcedure1 = goodsItemOnBill.PreviousProcedures.AddNew();
			var previousProcedure2 = goodsItemOnBill.PreviousProcedures.AddNew();

			CombineAssertions(() =>
			{
				goodsItemOnBill.HasChanges = false;
				Factory.Save();
				AssertContainsExactElementsInAnyOrder("GoodsItem HasChanges == false", new[] { previousProcedure1.PK, previousProcedure2.PK }, goodsItemOnBill.PreviousProcedures.Select(x => x.PK));

				goodsItemOnBill.HasChanges = true;
				Factory.Save();
				AssertEquals("GoodsItem HasChanges == true before saving, PreviousProcedure1 deleted", expected: true, previousProcedure1.IsDeleted);
				AssertEquals("GoodsItem HasChanges == true before saving, PreviousProcedure2 deleted", expected: true, previousProcedure2.IsDeleted);
				AssertEquals("GoodsItem HasChanges == false after save", expected: false, goodsItemOnBill.HasChanges);

				var newPreviousProcedure = goodsItemOnBill.PreviousProcedures.Single();
				newPreviousProcedure.CSI_Code = "123";
				AssertEquals("GoodsItem HasChanges == true if PreviousProcedure modified", expected: true, goodsItemOnBill.HasChanges);

				Factory.Save();
				AssertEquals("NewPreviousProcedure deleted", expected: true, newPreviousProcedure.IsDeleted);
				AssertNotNull("A new PreviousProcedure created again", goodsItemOnBill.PreviousProcedures.Count == 1);
			});
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_Consignor_HasChanges()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			Factory.Save();

			CombineAssertions(() =>
			{
				bill.IsOutwardOrderImported = true;
				CreateConsignorWithAuthorization();
				AssertEquals("GoodsItem HasChanges = true", expected: false, goodsItemOnBill.HasChanges);
				AssertEquals("Consignor HasChanges = true", expected: true, header.Consignor.HasChanges);
				Factory.Save();
				AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, "AUTH_NUMBER_CONSIGNOR", "ATC710123456789012345", YesNoList.Codes.Yes);
			});
		}

		public void TestOnFactorySaving_ReplacePreviousProcedures_Principal_HasChanges()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			Factory.Save();

			CombineAssertions(() =>
			{
				bill.IsOutwardOrderImported = true;
				CreatePrincipalWithAuthorization();
				AssertEquals("GoodsItem HasChanges = false", expected: false, goodsItemOnBill.HasChanges);
				AssertEquals("Principal HasChanges = true", expected: true, header.Principal.HasChanges);
				Factory.Save();
				AssertSinglePreviousProcedureCreatedDuringSaving(goodsItemOnBill, "AUTH_NUMBER_PRINCIPAL", "ATC710123456789012345", YesNoList.Codes.Yes);
			});
		}

		public void TestOnFactorySaving_WhenGoodsItemHeaderIsNull()
		{
			PrepareBusinessObjectsForOnFactorySavingTest("ATC710123456789012345");
			Factory.Save();

			header.Bills.DeleteAll();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestCalculateVAT_NoTaxTypeSet()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");

			helper.CreateTaxOrFee("VATS", 0.01m, Core.Constants.CountryCodes.Germany, new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Germany, "VATS", "", new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
			Factory.Save();

			goodsItemOnBill.BY_HarmonisedTariff = "0304798001";
			goodsItemOnBill.BY_MonetaryValue = 100.0m;
			goodsItemOnBill.BY_ZZF_NKTaxType = ZString.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("VAT is calculated with 0 Rate when no Tax Type is set.", ZDecimal.Zero, goodsItemOnBill.VatAmount);

				goodsItemOnBill.BY_ZZF_NKTaxType = "VATS";

				AssertEquals("VAT is calculated from the Tax Type when set.", 1.0m, goodsItemOnBill.VatAmount);
			});
		}

		public void TestBondAmountIsUpdatedByLinePrice()
		{
			var guarantee1 = departureMovementHeader.Guarantees.AddNew();
			guarantee1.PW_Override = false;
			var guarantee2 = departureMovementHeader.Guarantees.AddNew();
			guarantee2.PW_Override = true;

			SetDataGoodsItem(goodsItemOnBill);

			CombineAssertions(() =>
			{
				goodsItemOnBill.BY_LinePrice = 8000;
				AssertEquals("Override=false", 2000M, guarantee1.PW_BondAmount);
				AssertEquals("Override=true", 0M, guarantee2.PW_BondAmount);

				var goodsItemOnBill2 = bill.GoodsItems.AddNew();
				SetDataGoodsItem(goodsItemOnBill2);
				AssertEquals("Add goodsItem, Override=false", 5000M, guarantee1.PW_BondAmount);
				AssertEquals("Add goodsItem, Override=true", 0M, guarantee2.PW_BondAmount);

				goodsItemOnBill2.Delete();
				AssertEquals("Delete goodsItem, Override=false", 2000M, guarantee1.PW_BondAmount);
				AssertEquals("Delete goodsItem, Override=true", 0M, guarantee2.PW_BondAmount);
			});

			void SetDataGoodsItem(NctsDepartureCargoDesc goodsItem)
			{
				goodsItem.BY_GrossWeight = 130;
				goodsItem.BY_NetWeight = 120;
				goodsItem.BY_HarmonisedTariff = "0304798001";
				goodsItem.BY_CustomsQuantity = 120;
				goodsItem.BY_LinePrice = 12000;
				goodsItem.BY_RX_NKLinePriceCurrency = "EUR";
				goodsItem.BY_ZZF_NKTaxType = "REG";
			}
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => goodsItemOnBill;

		protected override BusinessObject GetNewBusinessObject() => goodsItemOnBill;

		protected override ZString CountryCode => Core.Constants.CountryCodes.Germany;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovementHeader = header.MovementHeader;
			bill = header.Bills.AddNew();
			goodsItemOnBill = bill.GoodsItems.AddNew();
		}
		NctsHeader header;
		NctsDepartureMovementHeader departureMovementHeader;
		NctsBill bill;
		NctsDepartureCargoDesc goodsItemOnBill;

		void AssertEffectiveStringValueNull(ZPropertyInfo cargoDescTargetInfo, ZPropertyInfo billTargetInfo, ZPropertyInfo headerTargetInfo, Func<NctsDepartureCargoDesc, string> propertyCall)
		{
			cargoDescTargetInfo.Value = ZString.Empty;
			billTargetInfo.Value = ZString.Empty;
			headerTargetInfo.Value = ZString.Empty;
			AssertNull(propertyCall(goodsItemOnBill));
		}

		void AssertEffectiveStringValueGoodsItem(ZPropertyInfo cargoDescTargetInfo, ZPropertyInfo billTargetInfo, ZPropertyInfo headerTargetInfo, Func<NctsDepartureCargoDesc, string> propertyCall)
		{
			headerTargetInfo.Value = new ZString("3");
			billTargetInfo.Value = new ZString("2");
			cargoDescTargetInfo.Value = new ZString("1");
			AssertEquals("1", propertyCall(goodsItemOnBill));
		}

		void AssertEffectiveStringValueBill(ZPropertyInfo cargoDescTargetInfo, ZPropertyInfo billTargetInfo, ZPropertyInfo headerTargetInfo, Func<NctsDepartureCargoDesc, string> propertyCall)
		{
			cargoDescTargetInfo.Value = ZString.Empty;
			billTargetInfo.Value = new ZString("2");
			headerTargetInfo.Value = new ZString("3");
			AssertEquals("2", propertyCall(goodsItemOnBill));
		}

		void AssertEffectiveStringValueHeader(ZPropertyInfo cargoDescTargetInfo, ZPropertyInfo billTargetInfo, ZPropertyInfo headerTargetInfo, Func<NctsDepartureCargoDesc, string> propertyCall)
		{
			cargoDescTargetInfo.Value = ZString.Empty;
			billTargetInfo.Value = ZString.Empty;
			headerTargetInfo.Value = new ZString("3");
			AssertEquals("3", propertyCall(goodsItemOnBill));
		}

		void AssertEffectiveStringValueCached(ZPropertyInfo cargoDescTargetInfo, Func<NctsDepartureCargoDesc, string> propertyCall)
		{
			cargoDescTargetInfo.Value = new ZString("1");
			var result = propertyCall(goodsItemOnBill);
			AssertSame(result, propertyCall(goodsItemOnBill));
		}

		void PrepareBusinessObjectsForOnFactorySavingTest(ZString warehouseEntryNumber, bool linkToProduct = true, string importTariff = "11223344")
		{
			goodsItemOnBill.BY_BondedWHSOrderNumber = "LocalReference";
			goodsItemOnBill.BY_WarehouseEntryNumber = warehouseEntryNumber;
			goodsItemOnBill.BY_WarehouseEntryLineNo = 2;
			goodsItemOnBill.BY_BondedWhsQuantity = 1.234;
			goodsItemOnBill.BY_BondedWhsUnitQty = "KGM";
			if (linkToProduct)
			{
				var part = Factory.NewWithValidTestData<DE.Business.OrgSupplierPart>();
				var pivotIMP = part.PivotsForBinding.AddNew();
				pivotIMP.CI_TariffNum = importTariff;
				pivotIMP.CI_ChildType = Common.Shared.ClassificationTypeList.Codes.Import;
				var pivotEXP = part.PivotsForBinding.AddNew();
				pivotEXP.CI_TariffNum = "44332211";
				pivotEXP.CI_ChildType = Common.Shared.ClassificationTypeList.Codes.Export;
				goodsItemOnBill.BY_OP_Part = part.PK;
			}
		}

		void CreateConsignorWithAuthorization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CONSI";
			var authorization = org.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "AUTH_NUMBER_CONSIGNOR");
			authorization.CPH_OH_PermitHolder = org.PK;
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
		}

		void CreatePrincipalWithAuthorization()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINC";
			var authorization = principal.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "AUTH_NUMBER_PRINCIPAL");
			authorization.CPH_OH_PermitHolder = principal.PK;
			header.Principal.E2_OA_Address = principal.MainAddress.PK;
		}

		void AssertSinglePreviousProcedureCreatedDuringSaving(NctsDepartureCargoDesc goodsItem, ZString expectedAuthorizationNumber, ZString expectedReference, ZString expectedStatus, string expectedTariff = "11223344")
		{
			var previousProcedure = goodsItem.PreviousProcedures.Single();
			AssertEquals("Previous Procedure", "9DEZ", previousProcedure.CSI_Procedure);
			AssertEquals("Local Reference", "LocalReference", previousProcedure.CSI_ReferenceNumber2);
			AssertEquals("Authorization No.", expectedAuthorizationNumber, previousProcedure.AuthorizationNumber);
			AssertEquals("Reference", expectedReference, previousProcedure.CSI_ReferenceNumber);
			AssertEquals("Line No.", 2, previousProcedure.CSI_LineNo);
			AssertEquals("Commodity Code", expectedTariff, previousProcedure.CSI_Tariff);
			AssertEquals("Entry via ATLAS?", expectedStatus, previousProcedure.CSI_Status);
			AssertEquals("Debit Qty", (ZDecimal)1.234, previousProcedure.CSI_Quantity2);
			AssertEquals("Debit Unit Qty", "KGM", previousProcedure.CSI_UnitOfQuantity2);
		}
	}
}
