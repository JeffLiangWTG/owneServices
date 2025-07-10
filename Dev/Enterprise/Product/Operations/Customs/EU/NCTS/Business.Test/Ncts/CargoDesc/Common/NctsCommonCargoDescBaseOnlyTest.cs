using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCommonCargoDesc))]
	sealed class NctsCommonCargoDescBaseOnlyTest : NctsCommonCargoDescAbstractTest<NctsHeader>
	{
		public void TestValidationDecider()
		{
			AssertNull(goodsItem.ValidationDecider);
		}

		public void TestRegistryCompanyPK()
		{
			AssertEquals(company.PK, goodsItem.RegistryCompanyPK);
		}

		public void TestRegistryBranchPK()
		{
			AssertEquals(branch.PK, goodsItem.RegistryBranchPK);
		}

		public void TestFeeType()
		{
			AssertEquals(typeof(NctsCargoDescFee), goodsItem.FeeType);
		}

		public void TestICusInvPackTypeSupporter_PackType()
		{
			var provider = (ICusInvPackTypeSupporter)goodsItem;
			AssertEquals(typeof(NctsPackage), provider.PackType);
		}

		public void TestICusInvPackTypeSupporter()
		{
			var provider = (ICusInvPackTypeSupporter)goodsItem;
			AssertType<NctsPackageTypeDecider>(provider.PackTypeDecider);
		}

		public void TestValidation()
		{
			AssertEquals(true, goodsItem.Validation is NctsCommonCargoDescValidation);
		}

		public void TestLookups()
		{
			AssertEquals(true, goodsItem.Lookups is NctsCommonCargoDescLookups);
		}

		public void TestBill()
		{
			AssertEquals(bill.PK, goodsItemDeparture.Bill.PK);
		}

		public void TestMoveHeaderOrBillParent()
		{
			CombineAssertions(() =>
			{
				Assert("Parent is MoveHeader", goodsItem.MoveHeaderOrBillParent is NctsCommonMovementHeader);
				Assert("Parent is Bill", goodsItemDeparture.MoveHeaderOrBillParent is NctsBill);
			});
		}

		public void TestBY_Description_MaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Max Length (NCTS4)", 280, goodsItem.BY_DescriptionInfo.MaxLength);
				header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Max Length (NCTS5)", 512, goodsItem.BY_DescriptionInfo.MaxLength);
			});
		}

		public void TestBY_ParentID_RelatedBusinessObject()
		{
			AssertEquals(nameof(NctsCommonCargoDesc.MoveHeaderOrBillParent), RelatedBusinessObjectAttribute.GetRelatedBizObjName(goodsItem.BY_ParentIDInfo));
		}

		public void TestDataGroupingCode()
		{
			AssertEquals(Core.Constants.CountryCodes.Latvia, goodsItem.DataGroupingCode);
		}

		public void TestTariffType()
		{
			AssertEquals(TariffTypes.Import, goodsItem.TariffType);
		}

		public void TestBY_LineNo_ReadOnly()
		{
			AssertEquals(true, goodsItem.BY_LineNoInfo.ReadOnly);
		}

		public void TestBY_DeclarationGoodsItemNumber_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_DeclarationGoodsItemNumberInfo, goodsItem.MultipleKeysToUse, caption: "Decl. Goods Item No.", shortCaption: "Decl. Item No.", fullDescription: "Declaration Goods Item Number");
		}

		public void TestBY_DeclarationGoodsItemNumber_ReadOnly()
		{
			AssertEquals(true, goodsItem.BY_DeclarationGoodsItemNumberInfo.ReadOnly);
		}

		public void TestConditionC075()
		{
			var sm = AddSpecialMentionForTest("hello", "AA", false, "");
			AssertNoMessageErrorContaining(sm.CSI_RN_NKCountryCodeInfo, "C075");
			sm = AddSpecialMentionForTest("help danger", "DG0", false, "");
			AssertHasMessageErrorContaining(sm.CSI_RN_NKCountryCodeInfo, "C075");
		}

		public void TestGrossWeightInKilograms()
		{
			goodsItem.BY_GrossWeight = new ZDecimal(2000);
			goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals(907.18474m, goodsItem.GrossMassInKilograms);
		}

		public void TestAutomaticSequenceNumberEnabled()
		{
			CombineAssertions(() =>
			{
				AssertEquals("First Line", 1, (int)goodsItem.BY_LineNo);
				var secondLine = header.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("Second Line", 2, (int)secondLine.BY_LineNo);
				var thirdLine = header.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("Third Line", 3, (int)thirdLine.BY_LineNo);
				secondLine.Delete();
				AssertEquals("First Line same as second deleted", 1, (int)goodsItem.BY_LineNo);
				AssertEquals("Third Line renumbered as second deleted", 2, (int)thirdLine.BY_LineNo);
				var newThirdLine = header.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("New Third added", 3, (int)newThirdLine.BY_LineNo);
			});
		}

		public void TestAutomaticSequenceNumberDisabled()
		{
			CombineAssertions(() =>
			{
				var firstLine = Factory.New<NctsArrivalAndUnloadingCargoDescForTest>();
				firstLine.AttachToParent(header.ArrivalMovementHeader);
				AssertEquals("First Line", ZShort.Zero, firstLine.BY_LineNo);
				firstLine.BY_LineNo = 20;
				var secondLine = Factory.New<NctsArrivalAndUnloadingCargoDescForTest>();
				secondLine.AttachToParent(header.ArrivalMovementHeader);
				AssertEquals("Second Line", ZShort.Zero, secondLine.BY_LineNo);
				secondLine.BY_LineNo = 35;
				firstLine.Delete();
				AssertEquals("Second Line remains the same as the first line is deleted", 35, (int)secondLine.BY_LineNo);
			});
		}

		public void TestPackages()
		{
			var package = goodsItem.Packages.AddNew();
			package.B5_MarksAndNumbers = "PK1234";
			package.B5_UnitType = "BX";
			package.B5_UnitCount = 5;
			AssertEquals(1, goodsItem.Packages.Count);
			AssertEquals("PK1234", goodsItem.Packages[0].B5_MarksAndNumbers);
			AssertEquals("BX", goodsItem.Packages[0].B5_UnitType);
			AssertEquals(5, goodsItem.Packages[0].B5_UnitCount);
		}

		public void TestSgiCodes()
		{
			AddSgisAndAdditionalInfosForTest();
			AssertEquals(2, goodsItem.SgiCodes.Count());
			var sgiCode = goodsItem.SgiCodes.First();
			AssertEquals("XX", sgiCode.Code);
			AssertEquals(12.1m, sgiCode.Qty);
		}

		public void TestSpecialMentions()
		{
			AddSgisAndAdditionalInfosForTest();
			AssertEquals(2, goodsItem.SpecialMentions.Count());
			var sm = goodsItem.SpecialMentions.First();
			AssertEquals("Special Mention", sm.StatementText);
			AssertEquals("XXXXX", sm.Statement);
			AssertEquals(true, sm.ExportFromEC);
			AssertEquals("GB", sm.ExportFromCountry);
		}

		public void TestSupportingDocuments()
		{
			var sd = goodsItem.SupportingDocuments.AddNew();
			sd.CSI_Code = "316";
			sd.CSI_ReferenceNumber = "SD001";
			sd.CSI_Description = "Pre Entry Lin=9999";
			AssertEquals(1, goodsItem.SupportingDocuments.Count);
			AssertEquals("316", sd.CSI_Code);
			AssertEquals("SD001", sd.CSI_ReferenceNumber);
			AssertEquals("Pre Entry Lin=9999", sd.CSI_Description);
		}

		public void TestICanBeImportOrExport()
		{
			EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport importOrExport = goodsItem;

			AssertEquals("CountryCode", goodsItem.DataGroupingCode, importOrExport.TrueCountryCode);
			AssertEquals("Data Grouping", goodsItem.DataGroupingCode, importOrExport.DataGroupingCode);
		}

		public void TestWipeBY_CommercialReferenceNumber()
		{
			goodsItem.BY_CommercialReferenceNumber = "12345";
			AssertEquals("Check Commercial Reference Number", "12345", goodsItem.BY_CommercialReferenceNumber);

			goodsItem.WipeCommercialReferenceNumber();
			AssertEquals("Commercial Reference Number should have been wiped", string.Empty, goodsItem.BY_CommercialReferenceNumber);
		}

		public void TestAdditionalInfoCollection()
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>($"{nameof(goodsItem.AdditionalInfos)} type", goodsItem.AdditionalInfos);

			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			AssertType<NctsAdditionalInfo>($"{nameof(goodsItem.AdditionalInfos)} elements type", additionalInfo);
		}

		public void TestGetNewTariffFormatter()
		{
			var commonCargoDescForTest = Factory.New<NctsCommonCargoDescForTest>();
			AssertType<TariffFormatterThirteen>("Default TariffFormatter", commonCargoDescForTest.GetNewTariffFormatterExposed());

			var countryCodeAndRelatedFormatterTestingList = new Dictionary<ZString, Type>()
			{
				{ Core.Constants.CountryCodes.Italy, typeof(TariffFormatterTen) },
				{ Core.Constants.CountryCodes.France, typeof(TariffFormatterTen) },
				{ Core.Constants.CountryCodes.Germany, typeof(TariffFormatterEleven) },
			};

			CombineAssertions("TariffFormatter is CountryCode dependant", () =>
			{
				foreach (var countryCodeAndRelatedFormatter in countryCodeAndRelatedFormatterTestingList)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeAndRelatedFormatter.Key))
					{
						AssertType($"TariffFormatter for {countryCodeAndRelatedFormatter.Key}", countryCodeAndRelatedFormatter.Value, commonCargoDescForTest.GetNewTariffFormatterExposed());
					}
				}
			});
		}

		public void TestTariffPropertiesMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BY_HarmonisedTariffInfo MaxLength", 22, goodsItem.BY_HarmonisedTariffInfo.MaxLength);
				AssertEquals("BY_FormattedHarmonisedTariff MaxLength", 22, goodsItem.BY_FormattedHarmonisedTariffInfo.MaxLength);
			});
		}

		public void TestMultipleKeysToUse()
		{
			CombineAssertions(() =>
			{
				AssertSequencesEqual("Header.BH_ApplicationCode is NCT", new[] { NctsHeader.Phase4CaptionKey }, goodsItem.MultipleKeysToUse);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertSequencesEqual("Header.BH_ApplicationCode is NC5", new[] { NctsHeader.Phase5CaptionKey }, goodsItem.MultipleKeysToUse);

				AssertSequencesEqual("Header is null", Array.Empty<string>(), Factory.New<NctsDepartureCargoDesc>().MultipleKeysToUse);
			});
		}

		public void TestBY_LineNo_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_LineNoInfo, "Item Number", "Item No.", "Item#");
		}

		public void TestBY_Description_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_DescriptionInfo, NctsHeader.Phase4CaptionKey, "[31] Description of Goods", "[31] Description", "Desc.");
		}

		public void TestBY_Description_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_DescriptionInfo, NctsHeader.Phase5CaptionKey, "Goods Description", "Description", "Desc.");
		}

		public void TestBY_Description_Phase4MaxLength()
		{
			AssertEquals("MaxLength", 280, goodsItem.BY_DescriptionInfo.MaxLength);
		}

		public void TestBY_Description_Phase5MaxLength()
		{
			AssertEquals("MaxLength", 512, goodsItemDeparture.BY_DescriptionInfo.MaxLength);
		}

		public void TestBY_GrossWeight_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_GrossWeightInfo, NctsHeader.Phase4CaptionKey, "[35] Gross Weight", string.Empty, "Gross Wgt.");
		}

		public void TestBY_GrossWeight_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItemDeparture.BY_GrossWeightInfo, NctsHeader.Phase5CaptionKey, "Gross Weight", "Gross Wgt.", "Gross");
		}

		public void TestGrossMassInKilograms_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.GetType(), nameof(goodsItem.GrossMassInKilograms), NctsHeader.Phase4CaptionKey, "[35] Gross Weight (kg)", "Gross Wgt. (kg)", "Gross (kg)");
		}

		public void TestGrossMassInKilograms_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItemDeparture.GetType(), nameof(goodsItem.GrossMassInKilograms), NctsHeader.Phase5CaptionKey, "Gross Weight (kg)", "Gross Wgt. (kg)", "Gross (kg)");
		}

		public void TestBY_NetWeight_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_NetWeightInfo, NctsHeader.Phase4CaptionKey, "[38] Net Weight", string.Empty, "Net Wgt.");
		}

		public void TestBY_NetWeight_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItemDeparture.BY_NetWeightInfo, NctsHeader.Phase5CaptionKey, "Net Weight", "Net Wgt.", "Net");
		}

		public void TestNetMassInKilograms_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.GetType(), nameof(goodsItem.NetMassInKilograms), NctsHeader.Phase4CaptionKey, "[38] Net Weight", "Net Wgt. (kg)", "Net (kg)");
		}

		public void TestNetMassInKilograms_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItemDeparture.GetType(), nameof(goodsItem.NetMassInKilograms), NctsHeader.Phase5CaptionKey, "Net Weight (kg)", "Net Wgt. (kg)", "Net (kg)");
		}

		public void TestBY_HarmonisedTariff_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_HarmonisedTariffInfo, NctsHeader.Phase4CaptionKey, "[33] Commodity Code", "Commodity", "Cmdty.");
		}

		public void TestBY_HarmonisedTariff_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItemDeparture.BY_HarmonisedTariffInfo, NctsHeader.Phase5CaptionKey, "Commodity Code", "Commodity", "Cmdty.");
		}

		public void TestBY_FormattedHarmonisedTariff_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_FormattedHarmonisedTariffInfo, NctsHeader.Phase4CaptionKey, "[33] Commodity Code", "Commodity", "Cmdty.");
		}

		public void TestBY_FormattedHarmonisedTariff_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItemDeparture.BY_FormattedHarmonisedTariffInfo, NctsHeader.Phase5CaptionKey, "Commodity Code", "Commodity", "Cmdty.");
		}

		public void TestBY_RN_NKCountryOfOrigin_Phase4Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_RN_NKCountryOfOriginInfo, NctsHeader.Phase4CaptionKey, "Origin Country/Region", "Origin.", "Origin. Ctry./Rgn.", "Country/Region of Origin");
		}

		public void TestBY_RN_NKCountryOfOrigin_Phase5Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItemDeparture.BY_RN_NKCountryOfOriginInfo, NctsHeader.Phase5CaptionKey, "Origin Country/Region", "Origin", "Origin Ctry./Rgn.", "Country/Region of Origin");
		}

		public void TestBY_CustomsSecondQuantity_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_CustomsSecondQuantityInfo, header.MultipleKeysToUse, caption: "Supplementary Quantity", shortCaption: "Sup. Qty", mediumCaption: "Supplementary Qty");
		}

		public void TestBY_CustomsSecondUnitQty_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_CustomsSecondUnitQtyInfo, header.MultipleKeysToUse, caption: "Supplementary Unit Quantity", shortCaption: "Sup. Unit Qty", mediumCaption: "Supplementary Unit Qty");
		}

		public void TestBY_CustomsThirdQuantity_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_CustomsThirdQuantityInfo, header.MultipleKeysToUse, caption: "[31] Third Qty", shortCaption: "Third Qty", mediumCaption: "Third Qty", fullDescription: "Third Additional Quantity for Liability Amount Calculation");
		}

		public void TestBY_CustomsFourthQuantity_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_CustomsFourthQuantityInfo, header.MultipleKeysToUse, caption: "Fourth Quantity", shortCaption: "Fourth Qty", mediumCaption: "Fourth Qty", fullDescription: "Fourth Additional Quantity for Liability Amount Calculation");
		}

		public void TestBY_Supplements_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_SupplementsInfo, header.MultipleKeysToUse, caption: "Supplementary Codes", shortCaption: "Sup. Codes", mediumCaption: "Sup. Codes");
		}

		public void TestDutyAmount_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.DutyAmountInfo, header.MultipleKeysToUse, caption: "Duty Amount");
		}

		public void TestAntiDumpingDutyAmount_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.AntiDumpingDutyAmountInfo, header.MultipleKeysToUse, caption: "Antidumping Duty Amount", shortCaption: "Antidumping Duty");
		}

		public void TestCountervailingDutyAmount_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.CountervailingDutyAmountInfo, header.MultipleKeysToUse, caption: "Countervailing Duty Amount", shortCaption: "Countervailing Duty");
		}

		public void TestTariff_Phase5andWcoGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Universal.Constants.TariffTypes.HarmonizedSystem);
			tariffType2.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, tariffType2.PK, "680291", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Marble, travertine and alabaster", compositeKey: "13.68..02.9.1");
			Factory.Save();

			goodsItemDeparture.BY_HarmonisedTariff = "680291";
			var tariff = goodsItemDeparture.UniversalTariff;
			CombineAssertions(() =>
			{
				AssertEquals("WCO Tariff is loaded", "680291", tariff.ZZ1_TariffCode);
				AssertEquals("WCO Tariff description is correct", "Marble, travertine and alabaster", tariff.ZZ1_Description);
				AssertEquals("UniversalTariffDescription is loaded", "Marble, travertine and alabaster", goodsItemDeparture.UniversalTariffDescription);
			});
		}

		public void TestContainersPivots()
		{
			CombineAssertions(() =>
			{
				var containersPivots = goodsItem.ContainersPivots;
				AssertType<NonPersistentDepartureContainerPivotCollection>("Type", containersPivots);
				AssertEquals("IsRegisteredEditableChildObject", true, goodsItem.IsRegisteredEditableChildObject(containersPivots));
				AssertSame("Cached", containersPivots, goodsItem.ContainersPivots);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("Cannot delete Goods Item from Customs.", goodsItemArrival.ReasonForNotAbleToDelete);
		}

		public void TestBY_UnloadedState_ReadOnly_SentToCustoms()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType("A");
			header.ArrivalMovementHeader.BM_MessageStatus = "SNT";
			var bill = header.Bills.AddNew();
			var cargoDesc = bill.ArrivalGoodsItems.AddNew();

			Assert(cargoDesc.BY_UnloadedStateInfo.ReadOnly);
		}

		public void TestBY_RX_NKCurrency()
		{
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, header.LocalCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, goodsItem.BY_RX_NKCurrency);
			AssertEquals(true, goodsItem.BY_RX_NKCurrencyInfo.ReadOnly);
		}

		#region Default UOMs

		public void TestDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "SSS");
			Factory.Save();

			AssertDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsSecondUnitQty = "AAA";
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Second Qty Existing suplementary UOM", assertionText), "SSS", goodsItem.BY_CustomsSecondUnitQty);
			}
		}

		public void TestDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "SSS");
			Factory.Save();

			AssertDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryQuantity("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryQuantity("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultSecondQuantityUOMFromTariff_ExistingSuplementaryQuantity(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsSecondQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Second Qty Existing suplementary Qty", assertionText), "SSS", goodsItem.BY_CustomsSecondUnitQty);
			}
		}

		public void TestDefaultSecondQuantityUOMFromTariff_InvalidTariff()
		{
			AssertDefaultSecondQuantityUOMFromTariff_InvalidTariff("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultSecondQuantityUOMFromTariff_InvalidTariff("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultSecondQuantityUOMFromTariff_InvalidTariff(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "4444444444";
				AssertEquals(string.Format("{0}: Second Qty invalid Tariff", assertionText), ZString.Empty, goodsItem.BY_CustomsSecondUnitQty);
			}
		}

		public void TestDefaultSecondQuantityUOMFromTariff_MultipleUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithMultipleUnits = helper.CreateTariff(CountryCode, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "XXX");
			Factory.Save();

			AssertDefaultSecondQuantityUOMFromTariff_MultipleUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultSecondQuantityUOMFromTariff_MultipleUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultSecondQuantityUOMFromTariff_MultipleUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "1111111111";
				AssertCollectionContains(string.Format("{0}: Second Qty multiple UOMS", assertionText), goodsItem.BY_CustomsSecondUnitQty, new ZString[] { "NAR", "XXX" });
			}
		}

		public void TestDefaultSecondQuantityUOMFromTariff_NoUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			helper.CreateTariff(CountryCode, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertDefaultSecondQuantityUOMFromTariff_NoUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultSecondQuantityUOMFromTariff_NoUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultSecondQuantityUOMFromTariff_NoUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsSecondUnitQty = "AAA";
				goodsItem.BY_HarmonisedTariff = "3333333333";
				AssertEquals(string.Format("{0}: Second Qty no UOM", assertionText), ZString.Empty, goodsItem.BY_CustomsSecondUnitQty);
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

			AssertDefaultThirdQuantityUOMFromTariff_ExistingThirdUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_ExistingThirdUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_ExistingThirdUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsThirdUnitQty = "AAA";
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Third Unit Qty (BY_CustomsThirdUnitQty) change when it already has one set", assertionText), "SSS", goodsItem.BY_CustomsThirdUnitQty);
			}
		}

		public void TestDefaultThirdQuantityUOMFromTariff_ExistingThirdQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
			Factory.Save();

			AssertDefaultThirdQuantityUOMFromTariff_ExistingThirdQuantity("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_ExistingThirdQuantity("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_ExistingThirdQuantity(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsThirdQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Third Unit Qty (BY_CustomsThirdUnitQty) is set when it already has quantity set", assertionText), "SSS", goodsItem.BY_CustomsThirdUnitQty);
			}
		}

		public void TestDefaultThirdQuantityUOMFromTariff_InvalidTariff()
		{
			AssertDefaultThirdQuantityUOMFromTariff_InvalidTariff("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_InvalidTariff("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_InvalidTariff(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "4444444444";
				AssertEquals(string.Format("{0}: Customs Third Unit (BY_CustomsThirdUnitQty) is not set for invalid tariff", assertionText), ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
			}
		}

		public void TestDefaultThirdQuantityUOMFromTariff_MultipleUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithMultipleUnits = helper.CreateTariff(CountryCode, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM3Type, "NAR");
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM3Type, "XXX");
			Factory.Save();

			AssertDefaultThirdQuantityUOMFromTariff_MultipleUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_MultipleUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_MultipleUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "1111111111";
				AssertCollectionContains(string.Format("{0}: Customs Third Unit (BY_CustomsThirdUnitQty) has the collection if has multiple UOMS", assertionText), goodsItem.BY_CustomsThirdUnitQty, new ZString[] { "NAR", "XXX" });
			}
		}

		public void TestDefaultThirdQuantityUOMFromTariff_NoUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			helper.CreateTariff(CountryCode, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertDefaultThirdQuantityUOMFromTariff_NoUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_NoUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_NoUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsThirdUnitQty = "AAA";
				goodsItem.BY_HarmonisedTariff = "3333333333";
				AssertEquals(string.Format("{0}: Customs Third Unit (BY_CustomsThirdUnitQty) is set to empty if the tariff has no UOMS", assertionText), ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
			}
		}

		public void TestDefaultThirdQuantityUOMFromTariff_RateUOM()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "MIL", "");

			AssertDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsThirdQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Third Unit Qty (BY_CustomsThirdUnitQty) is the UOM of the rate Formula when there is no CU3", assertionText), "MIL", goodsItem.BY_CustomsThirdUnitQty);
			}
		}

		public void TestDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "DTN", "NAR", true);

			AssertDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsThirdQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Third Unit Qty (BY_CustomsThirdUnitQty) is NAR cause DTN is a conversion of KGM", assertionText), "NAR", goodsItem.BY_CustomsThirdUnitQty);
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_ExistingFourthUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM4Type, "CCC");
			Factory.Save();

			AssertDefaultFourthQuantityUOMFromTariff_ExistingFourthUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_ExistingFourthUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_ExistingFourthUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsFourthUnitQty = "AAA";
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Fourth Unit Qty (BY_CustomsFourthUnitQty) change when it already has one set", assertionText), "CCC", goodsItem.BY_CustomsFourthUnitQty);
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_ExistingFourthQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM4Type, "CCC");
			Factory.Save();

			AssertDefaultFourthQuantityUOMFromTariff_ExistingFourthQuantity("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_ExistingFourthQuantity("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_ExistingFourthQuantity(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsFourthQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Fourth Unit Qty is set when it already has quantity set", assertionText), "CCC", goodsItem.BY_CustomsFourthUnitQty);
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_InvalidTariff()
		{
			AssertDefaultFourthQuantityUOMFromTariff_InvalidTariff("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_InvalidTariff("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_InvalidTariff(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "4444444444";
				AssertEquals(string.Format("{0}: Customs Fourth Unit is not set for invalid tariff", assertionText), ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_MultipleUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithMultipleUnits = helper.CreateTariff(CountryCode, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM3Type, "NAR");
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM3Type, "XXX");
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM4Type, "SSS");
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM4Type, "CCC");
			Factory.Save();

			AssertDefaultFourthQuantityUOMFromTariff_MultipleUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_MultipleUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_MultipleUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "1111111111";
				AssertCollectionContains(string.Format("{0}: Customs Fourth Unit has the collection if has multiple UOMS", assertionText), goodsItem.BY_CustomsFourthUnitQty, new ZString[] { "NAR", "XXX", "SSS", "CCC" });
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_NoUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			helper.CreateTariff(CountryCode, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertDefaultFourthQuantityUOMFromTariff_NoUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_NoUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_NoUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsFourthUnitQty = "AAA";
				goodsItem.BY_HarmonisedTariff = "3333333333";
				AssertEquals(string.Format("{0}: Customs Fourth Unit is set to empty if the tariff has no UOMS", assertionText), ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_RateUOM()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "MIL", "NAR");

			AssertDefaultFourthQuantityUOMFromTariff_RateUOM("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_RateUOM("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_RateUOM(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsFourthQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Fourth Unit Qty is the UOM of the rate Formula when there is no CU4", assertionText), "NAR", goodsItem.BY_CustomsFourthUnitQty);
			}
		}

		public void TestDefaultFourthQuantityUOMFromTariff_RateUOMIsConvertable()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "DTN", "NAR", true, "MIL");

			AssertDefaultFourthQuantityUOMFromTariff_RateUOMIsConvertable("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultFourthQuantityUOMFromTariff_RateUOMIsConvertable("Arrival Good Item", goodsItemArrival);
			}

			void AssertDefaultFourthQuantityUOMFromTariff_RateUOMIsConvertable(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_CustomsFourthQuantity = 2.4m;
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals(string.Format("{0}: Customs Fourth Unit Qty is MIL or NAR cause DTN is a conversion of KGM and third Qty will be set before", assertionText), true, new ZString[] { "NAR", "MIL" }.Contains(goodsItem.BY_CustomsFourthUnitQty));
			}
		}

		public void TestSetDefaultTariffUnitOfMeasuresFromRatesView_Duty()
		{
			SetUpRatesViews("DTY", "A00");
			AssertSetDefaultTariffUnitOfMeasuresFromRatesView("Departure: HighestDuty", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertSetDefaultTariffUnitOfMeasuresFromRatesView("Arrival: HighestDuty", goodsItemArrival);
			}
		}

		public void TestSetDefaultTariffUnitOfMeasuresFromRatesView_Antidumping()
		{
			SetUpRatesViews(Customs.Universal.Constants.RateTypes.AntiDumping, "RC1");
			AssertSetDefaultTariffUnitOfMeasuresFromRatesView("Departure: HighestAntiDumpingDuty", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertSetDefaultTariffUnitOfMeasuresFromRatesView("Arrival: HighestAntiDumpingDuty", goodsItemArrival);
			}
		}

		public void TestSetDefaultTariffUnitOfMeasuresFromRatesView_Countervailing()
		{
			SetUpRatesViews(Customs.Universal.Constants.RateTypes.Countervailing, "RC1");
			AssertSetDefaultTariffUnitOfMeasuresFromRatesView("Departure: HighestCountervailingDuty", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertSetDefaultTariffUnitOfMeasuresFromRatesView("Arrival: HighestCountervailingDuty", goodsItemArrival);
			}
		}

		void AssertSetDefaultTariffUnitOfMeasuresFromRatesView(string assertionName, NctsCommonCargoDesc goodsItem)
		{
			CombineAssertions(assertionName, () =>
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				goodsItem.BY_HarmonisedTariff = ZString.Empty;
				if (goodsItem is NctsDepartureCargoDesc departureGoodItem)
				{
					AssertEquals("PreReq: First Unit filled", "KGM", departureGoodItem.CustomsFirstUnitQtyKilograms);
				}
				AssertEquals("BY_CustomsThirdUnitQty: When no tariff, nothing change", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When no tariff, nothing change", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_HarmonisedTariff = "1111222222";
				AssertEquals("BY_CustomsThirdUnitQty: When unit is set in First or Second Qty, no unit set", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When unit is set in First or Second Qty, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_HarmonisedTariff = "1111222223";
				AssertEquals("BY_CustomsThirdUnitQty: When there is unit in First and Second Qty, unit is set in Third", "LPA", goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When Third is empty, no unit set in Fourth", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_HarmonisedTariff = "1111222224";
				AssertEquals("BY_CustomsFourthUnitQty: When unit is set in First, Second Qty or Third, unit is set on Fourth", "MIL", goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_HarmonisedTariff = "1111222225";
				AssertEquals("BY_CustomsFourthUnitQty: If a related unit is set in First, Second Qty, Third or Fourth, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_RN_NKCountryOfOrigin = ZString.Empty;
				goodsItem.BY_HarmonisedTariff = "1111222226";
				AssertEquals("BY_CustomsThirdUnitQty: When no country of Origin, no unit set", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When no country of Origin, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				AssertEquals("BY_CustomsThirdUnitQty: When country of Origin is set, no unit set", "HG", goodsItem.BY_CustomsThirdUnitQty);
			});
		}

		void SetUpRatesViews(string rateTypeCode, string rateCodeText)
		{
			(var helper, var tariffs) = SetUpTariffRateFormula(Factory, rateTypeCode, rateCodeText, new Dictionary<string, string>()
			{
				{ "1111222222", "0.5*[HG]" },
				{ "1111222223", "0.5*[LPA]" },
				{ "1111222224", "0.5*[MIL]" },
				{ "1111222225", "0.5*[LPA]" },
				{ "1111222226", "0.5*[HG]" },
			});

			helper.CreateTariffUOM(tariffs[0], UnitOfMeasureTypes.AdditionalUOMType, "HG");
			helper.CreateTariffUOM(tariffs[2], UnitOfMeasureTypes.CustomsUOM3Type, "AAA");
			helper.CreateTariffUOM(tariffs[3], UnitOfMeasureTypes.CustomsUOM3Type, "ASVX");

			Factory.Save();
		}

		public void TestConvertibleUnitsOfMeasure()
		{
			AssertSupplementaryCodes("Departure Good Item", goodsItemDeparture);
			AssertSupplementaryCodes("Arrival Good Item", goodsItemArrival);

			void AssertSupplementaryCodes(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					var convertibleUnitsOfMeasure = goodsItem.ConvertibleUnitsOfMeasureSets;
					AssertEquals("Convertible Units Of Measure List Count", 3, convertibleUnitsOfMeasure.Count);
					AssertContainsExactElementsInAnyOrder(new[] { "DTN", "GRM", "KG", "KGM", "TNE" }, convertibleUnitsOfMeasure.ElementAt(0).ToArray());
					AssertContainsExactElementsInAnyOrder(new[] { "LTR", "HLT", "KLT" }, convertibleUnitsOfMeasure.ElementAt(1).ToArray());
					AssertContainsExactElementsInAnyOrder(new[] { "LPA", "ASVX" }, convertibleUnitsOfMeasure.ElementAt(2).ToArray());
				});
			}
		}

		#endregion

		#region Supplementary Codes

		public void TestAddAdditionalCodesWithOutTariff()
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("ADDAdditionalCodes without Tariff for Departure Ncts5", Array.Empty<ZString>(), goodsItemDeparture.ADDAdditionalCodes);
				AssertContainsExactElementsInAnyOrder("ADDAdditionalCodes without Tariff for Arrival Ncts5", Array.Empty<ZString>(), goodsItemArrival.ADDAdditionalCodes);
			});
		}

		public void TestGetCountryCodeForSupplementaryCodeProvider()
		{
			AssertEquals(Core.Constants.CountryCodes.Latvia, header.CountryCode);
			AssertEquals(Core.Constants.CountryCodes.Latvia, goodsItemDeparture.GetCountryCodeForCodeProvider());
		}

		public void TestAdditionalSupplementaryCodesCodeList_Departure()
		{
			var loader = new BaseSupplementaryCode.Loader(Factory);
			var code = loader.LoadOrCreate<SupplementaryCode, NctsCommonCargoDesc>(goodsItemDeparture, 1);
			code.CY_Code = "SUP";
			NCTSTestHelper.SetupTariffAndRateForAdditionalSupplementaryCodes(Factory);
			goodsItemDeparture.BY_HarmonisedTariff = "1234512345";
			goodsItemDeparture.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			var list = code.Lookups.CY_CodeList;
			AssertEquals("Departure: list count", 1, list.Count);
			Assert("Departure: list has code", list.ContainsCode("additionalcode"));
			AssertEquals("Departure: Description from Code", "Additional Code 1 Descriptions", list.GetDescriptionFromCode("additionalcode"));
		}

		public void TestAdditionalSupplementaryCodesCodeList_Arrival()
		{
			var loader = new BaseSupplementaryCode.Loader(Factory);
			var code = loader.LoadOrCreate<SupplementaryCode, NctsCommonCargoDesc>(goodsItemArrival, 1);
			code.CY_Code = "SUP";
			NCTSTestHelper.SetupTariffAndRateForAdditionalSupplementaryCodes(Factory);

			goodsItemArrival.BY_HarmonisedTariff = "1234512345";
			goodsItemArrival.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			var list = code.Lookups.CY_CodeList;
			AssertEquals("Arrival: list count", 1, list.Count);
			Assert("Arrival: list has code", list.ContainsCode("additionalcode"));
			AssertEquals("Arrival: Description from Code", "Additional Code 1 Descriptions", list.GetDescriptionFromCode("additionalcode"));
		}

		public void TestSupplementaryCodes()
		{
			AssertSupplementaryCodes("Departure Good Item", goodsItemDeparture);
			AssertSupplementaryCodes("Arrival Good Item", goodsItemArrival);

			void AssertSupplementaryCodes(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					var collection = goodsItem.AdditionalSupplementaryCodes;
					AssertEquals("No Supplementary code captured.", 0, goodsItem.SupplementaryCodes.Count());

					collection.AddNew("ABCD");
					collection.AddNew("EFGH");
					AssertEquals("Two supplementary codes captured.", 2, goodsItem.SupplementaryCodes.Count());
					AssertContainsExactElementsInAnyOrder("", new ZString[] { "ABCD", "EFGH" }, goodsItem.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
				});
			}
		}

		public void TestBY_Supplements_ReadOnly()
		{
			AssertEquals(true, goodsItemDeparture.BY_SupplementsInfo.ReadOnly);
		}

		public void TestAdditionalSupplementaryCodesCalculatedField()
		{
			AssertAdditionalSupplementaryCodesCalculatedField("Departure Good Item", goodsItemDeparture);
			AssertAdditionalSupplementaryCodesCalculatedField("Arrival Good Item", goodsItemArrival);

			void AssertAdditionalSupplementaryCodesCalculatedField(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					var collection = goodsItem.AdditionalSupplementaryCodes;
					AssertEquals("", goodsItem.BY_Supplements);

					collection.AddNew("ABCD");
					collection.AddNew("EFGH");
					AssertEquals("ABCD,EFGH", goodsItem.BY_Supplements);
				});
			}
		}

		public void TestGetCountryCodeFromAdditionalCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("GetCountryCodeFromAdditionalCode for Departure Ncts5", goodsItemDeparture.GetCountryCodeFromAdditionalCode("AD"), ZString.Empty);
				AssertEquals("GetCountryCodeFromAdditionalCode for Arrival Ncts5", goodsItemArrival.GetCountryCodeFromAdditionalCode("AD"), ZString.Empty);
			});
		}

		#endregion

		#region Duty Amount

		public void TestDutiableAmount()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			AssertDutiableAmount("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmount("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmount(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.BY_MonetaryValue = 1_000m;
				AssertEquals(string.Format("{0}: Duty Amount Calculation", assertionText), 120m, goodsItem.DutyAmount);
				AssertEquals($"{assertionText}: Fees should have {NctsCommonCargoDesc.ChargeType.Duty} record", 120m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.Duty).BFE_ChargeAmount);
			}
		}

		public void TestDutiableAmountWithSecondUnitNARxValue()
		{
			NCTSTestHelper.SetUpTariffSecondUnit(Factory);

			AssertDutiableAmountWithSecondUnitNARxValue("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmountWithSecondUnitNARxValue("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmountWithSecondUnitNARxValue(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111100000";
				goodsItem.BY_CustomsSecondUnitQty = "NAR";
				goodsItem.BY_MonetaryValue = 200m;
				goodsItem.BY_CustomsSecondQuantity = 15.00m;

				AssertEquals(string.Format("{0}: Calculate by NAR of the second unit and apply by price", assertionText), 7.50m, goodsItem.DutyAmount);
			}
		}

		public void TestDutiableAmountWithSecondUnitNARMax()
		{
			NCTSTestHelper.SetUpTariffSecondUnit(Factory);

			AssertDutiableAmountWithSecondUnitNARMax("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmountWithSecondUnitNARMax("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmountWithSecondUnitNARMax(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111100000";
				goodsItem.BY_CustomsSecondUnitQty = "NAR";
				goodsItem.BY_MonetaryValue = 200m;
				goodsItem.BY_CustomsSecondQuantity = 50.00m;

				AssertEquals(string.Format("{0}: Calculate by NAR of the second unit and apply by MAX", assertionText), 9.20m, goodsItem.DutyAmount);
			}
		}

		public void TestDutiableAmountWithSecondUnitNARMin()
		{
			NCTSTestHelper.SetUpTariffSecondUnit(Factory);

			AssertDutiableAmountWithSecondUnitNARMin("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmountWithSecondUnitNARMin("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmountWithSecondUnitNARMin(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111100000";
				goodsItem.BY_CustomsSecondUnitQty = "NAR";
				goodsItem.BY_MonetaryValue = 200m;
				goodsItem.BY_CustomsSecondQuantity = 1.00m;

				AssertEquals(string.Format("{0}: Calculate by NAR of the second unit and apply by MIN", assertionText), 5.40m, goodsItem.DutyAmount);
			}
		}

		public void TestDutiableAmountWithCustomsUnit()
		{
			NCTSTestHelper.SetUpTariffSecondUnit(Factory);

			AssertDutiableAmountWithCustomsUnit("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmountWithCustomsUnit("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmountWithCustomsUnit(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111100000";
				goodsItem.BY_CustomsUnitQty = "NAR";
				goodsItem.BY_MonetaryValue = 200m;
				goodsItem.BY_CustomsQuantity = 15.00m;

				AssertEquals(string.Format("{0}: Calculate by NAR of the customs unit and apply by price", assertionText), 7.50m, goodsItem.DutyAmount);
			}
		}

		public void TestDutiableAmountWithThirdUnit_Phase5()
		{
			goodsItemDeparture = headerDeparture.Bills.AddNew().GoodsItems.AddNew();
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);
			}

			NCTSTestHelper.SetUpTariffSecondUnit(Factory);
			AssertDutiableAmountWithThirdUnit_Phase5("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmountWithThirdUnit_Phase5("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmountWithThirdUnit_Phase5(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111100000";
				goodsItem.BY_CustomsThirdUnitQty = "NAR";
				goodsItem.BY_MonetaryValue = 200m;
				goodsItem.BY_CustomsThirdQuantity = 15.00m;

				AssertEquals(string.Format("{0}: Calculate by NAR of the third unit and apply by price", assertionText), 7.50m, goodsItem.DutyAmount);
			}
		}

		public void TestDutiableAmountWithFourthUnit_Phase4()
		{
			NCTSTestHelper.SetUpTariffSecondUnit(Factory);
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			goodsItemDeparture.BY_HarmonisedTariff = "9111100000";
			goodsItemDeparture.BY_CustomsFourthUnitQty = "NAR";
			goodsItemDeparture.BY_MonetaryValue = 200m;
			goodsItemDeparture.BY_CustomsFourthQuantity = 15.00m;

			AssertEquals("Calculate by NAR of the fourth unit and Not apply by price", 5.400m, goodsItemDeparture.DutyAmount);
		}

		public void TestDutiableAmountWithFourthUnit_Phase5()
		{
			goodsItemDeparture = headerDeparture.Bills.AddNew().GoodsItems.AddNew();
			goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);

			NCTSTestHelper.SetUpTariffSecondUnit(Factory);

			AssertDutiableAmountWithFourthUnit_Phase5("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDutiableAmountWithFourthUnit_Phase5("Arrival Good Item", goodsItemArrival);
			}

			void AssertDutiableAmountWithFourthUnit_Phase5(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111100000";
				goodsItem.BY_CustomsFourthUnitQty = "NAR";
				goodsItem.BY_MonetaryValue = 200m;
				goodsItem.BY_CustomsFourthQuantity = 15.00m;

				AssertEquals(string.Format("{0}: Calculate by NAR of the fourth unit and apply by price", assertionText), 7.50m, goodsItem.DutyAmount);
			}
		}

		public void TestDutiableAmountWithAllUnits_Phase5()
		{
			goodsItemDeparture = headerDeparture.Bills.AddNew().GoodsItems.AddNew();
			goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);

			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);

			AssertDutiableAmountWithAllUnits_Phase5("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDutiableAmountWithAllUnits_Phase5("Arrival Good Item", goodsItemArrival);
			}

			void AssertDutiableAmountWithAllUnits_Phase5(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111200000";
				goodsItem.BY_CustomsFourthUnitQty = "LTR";
				goodsItem.BY_CustomsFourthQuantity = 15.00m;
				goodsItem.BY_CustomsThirdUnitQty = "NAR";
				goodsItem.BY_CustomsThirdQuantity = 15.00m;
				goodsItem.BY_CustomsUnitQty = "KGM";
				goodsItem.BY_CustomsQuantity = 15.00m;
				goodsItem.BY_CustomsSecondUnitQty = "DTN";
				goodsItem.BY_CustomsSecondQuantity = 15.00m;

				AssertEquals(string.Format("{0}: Calculate DutyAmount with the three Customs fields", assertionText), 30.00m, goodsItem.DutyAmount);
			}
		}

		public void TestRecalculateDutyAmount_Phase5()
		{
			goodsItemDeparture = headerDeparture.Bills.AddNew().GoodsItems.AddNew();
			goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);

			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);

			AssertRecalculateDutyAmount_Phase5("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertRecalculateDutyAmount_Phase5("Arrival Good Item", goodsItemArrival);
			}

			void AssertRecalculateDutyAmount_Phase5(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "9111200000";
				goodsItem.BY_CustomsFourthUnitQty = "LTR";
				goodsItem.BY_CustomsFourthQuantity = 15.00m;
				goodsItem.BY_CustomsThirdUnitQty = "NAR";
				goodsItem.BY_CustomsThirdQuantity = 15.00m;
				goodsItem.BY_CustomsUnitQty = "KGM";
				goodsItem.BY_CustomsQuantity = 15.00m;
				goodsItem.BY_CustomsSecondUnitQty = "DTN";
				goodsItem.BY_CustomsSecondQuantity = 15.00m;

				CombineAssertions(assertionText, () =>
				{
					AssertEquals("[Precondition] DutyAmount value", 30.00m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsQuantity = 10.00m;
					AssertEquals("DutyAmount is recalculated when Customs value change", 27.50m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsSecondQuantity = 10.00m;
					AssertEquals("DutyAmount is recalculated when Customs Second value change", 25.0m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsThirdQuantity = 10.00m;
					AssertEquals("DutyAmount is recalculated when Customs Third value change", 22.50m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsFourthQuantity = 10.00m;
					AssertEquals("DutyAmount is recalculated when Customs Fourth value change", 20.00m, goodsItem.DutyAmount);
				});
			}
		}

		public void TestDutiableAmountWithSupplementaryCodes()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			AssertDutiableAmountWithSupplementaryCodes("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmountWithSupplementaryCodes("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmountWithSupplementaryCodes(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
					goodsItem.BY_MonetaryValue = 1_000m;
					AssertEquals(120m, goodsItem.DutyAmount);

					var supCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supCode.CY_Code = "ABCD";
					AssertEquals(80m, goodsItem.DutyAmount);
				});
			}
		}

		public void TestHighestDutyIfAdditionalCodesAndNoMatchingRate()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			AssertHighestDutyIfAdditionalCodesAndNoMatchingRate("Departure Good Item", goodsItemDeparture);
			AssertHighestDutyIfAdditionalCodesAndNoMatchingRate("Arrival Good Item", goodsItemArrival);

			void AssertHighestDutyIfAdditionalCodesAndNoMatchingRate(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.AdditionalSupplementaryCodes.AddNew("EFGH");
				AssertEquals(string.Format("{0}: In case no rate matches the additional code, default rate is selected.", assertionText), "VFD * 0.12", goodsItem.HighestDuty.ZZ2_RateFormula);
			}
		}

		public void TestHighestDutyIfAdditionalCodesAndMatchingRate()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			AssertHighestDutyIfAdditionalCodesAndMatchingRate("Departure Good Item", goodsItemDeparture);
			AssertHighestDutyIfAdditionalCodesAndMatchingRate("Arrival Good Item", goodsItemArrival);

			void AssertHighestDutyIfAdditionalCodesAndMatchingRate(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.AdditionalSupplementaryCodes.AddNew("ABCD");
				AssertEquals(string.Format("{0}: Rate matching the additional code should always be selected over default rate.", assertionText), "VFD * 0.08", goodsItem.HighestDuty.ZZ2_RateFormula);
			}
		}

		public void TestHighestDutyIfNoAdditionalCode()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			AssertHighestDutyIfNoAdditionalCode("Departure Good Item", goodsItemDeparture);
			AssertHighestDutyIfNoAdditionalCode("Arrival Good Item", goodsItemArrival);

			void AssertHighestDutyIfNoAdditionalCode(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.BY_RN_NKCountryOfOrigin = "EU";
					goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
					AssertEquals("VFD * 0.12", goodsItem.HighestDuty.ZZ2_RateFormula);

					goodsItem.BY_HarmonisedTariff = "0304798001";
					AssertNull(goodsItem.HighestDuty);
				});
			}
		}

		public void TestHighestDutyWithTwoEmptyAdditionalCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(1000));
			Factory.Save();

			AssertHighestDutyWithTwoEmptyAdditionalCode("Departure Good Item", goodsItemDeparture);
			AssertHighestDutyWithTwoEmptyAdditionalCode("Arrival Good Item", goodsItemArrival);

			void AssertHighestDutyWithTwoEmptyAdditionalCode(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.BY_MonetaryValue = 100;

				AssertNoExceptionThrown(assertionText, () =>
				{
					var highestDuty = goodsItem.HighestDuty;
					AssertEquals("The highest duty should return the rate2. ", "VFD * 0.12", highestDuty.ZZ2_RateFormula);
				});
			}
		}

		public void TestHighestDutyWithOneNotEmptyAdditionalCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "2501");
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(1000));
			Factory.Save();

			AssertHighestDutyWithOneNotEmptyAdditionalCode("Departure Good Item", goodsItemDeparture);
			AssertHighestDutyWithOneNotEmptyAdditionalCode("Arrival Good Item", goodsItemArrival);

			void AssertHighestDutyWithOneNotEmptyAdditionalCode(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.BY_MonetaryValue = 100;

				AssertNoExceptionThrown(assertionText, () =>
				{
					var highestDuty = goodsItem.HighestDuty;
					AssertEquals("The highest duty should return the rate2 since it is the one with empty additional code. ", "VFD * 0.12", highestDuty.ZZ2_RateFormula);
				});
			}
		}

		public void TestHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "2501");
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(1000), "2500");

			var preference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.06", preference2.PK);
			helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "2505");
			Factory.Save();

			AssertHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes("Departure Good Item", goodsItemDeparture);
			AssertHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes("Arrival Good Item", goodsItemArrival);

			void AssertHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "EU";
				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.BY_MonetaryValue = 100;

				AssertNoExceptionThrown(assertionText, () =>
				{
					var highestDuty = goodsItem.HighestDuty;
					AssertEquals("The highest duty should return the rate1 since it is the one with the highest additional code for preference 100. ", "VFD * 0.08", highestDuty.ZZ2_RateFormula);
				});
			}
		}

		public void TestDutyAmount_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DutyAmount Readonly for Departure Ncts5", true, goodsItemDeparture.DutyAmountInfo.ReadOnly);
				AssertEquals("DutyAmount Readonly for Arrival Ncts5", true, goodsItemArrival.DutyAmountInfo.ReadOnly);
			});
		}

		public void TestHighestDutyWithIncompleteHarmonisedTariff()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			AssertDutiableAmount("Departure Good Item", goodsItemDeparture);
			AssertDutiableAmount("Arrival Good Item", goodsItemArrival);

			void AssertDutiableAmount(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "0304";
				goodsItem.BY_MonetaryValue = 1_000m;
				AssertNull(string.Format("{0}: Highest Duty Amount Calculation with incomplete harmonised tariff", assertionText), goodsItem.HighestDuty);

				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				AssertNotNull(string.Format("{0}: Highest Duty Amount Calculation with complete harmonised tariff", assertionText), goodsItem.HighestDuty);
			}
		}

		#endregion

		#region VAT Amnout

		public void TestVatAmount()
		{
			NCTSTestHelper.SetUpTariffAndAntidumpingAndCountervailing(Factory);

			goodsItemDeparture.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItemDeparture.BY_MonetaryValue = 1_000m;

			var supplementaryCode1 = goodsItemDeparture.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode1.CY_Code = "AC01";

			CombineAssertions(() =>
			{
				AssertEquals("DutyAmount", 120m, goodsItemDeparture.DutyAmount);
				AssertEquals("VatAmount calculation (1000+120)*0.2", 224m, goodsItemDeparture.VatAmount);
				AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.VAT} record", 224m, goodsItemDeparture.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.VAT).BFE_ChargeAmount);
			});
		}

		#endregion

		#region Antidumping

		public void TestAntiDumpingDutyAmount()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.AntiDumping);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Departure Good Item", goodsItemDeparture);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Arrival Good Item", goodsItemArrival);

			void AssertAntiDumpingDutyAmountWithNoRateMatched(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
				goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

				var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC02";
				CombineAssertions(assertionText, () =>
				{
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.ADDAdditionalCodes);
					AssertEquals("AC02 matched, amount should be 4.3", (ZDecimal)4.3, goodsItem.AntiDumpingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.AntiDumpingDuty} record", 4.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.AntiDumpingDuty).BFE_ChargeAmount);

					supplementaryCode1.CY_Code = "AC04";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC01", "AC03" }, goodsItem.ADDAdditionalCodes);
					AssertEquals("AC01 matched, amount should be 24.3", (ZDecimal)24.3, goodsItem.AntiDumpingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.AntiDumpingDuty} record", 24.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.AntiDumpingDuty).BFE_ChargeAmount);

					var supplementaryCode2 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode2.CY_Code = "AC02";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02", "AC04" }, goodsItem.ADDAdditionalCodes);
					AssertEquals("AC02 matched, amount should equal 4.3", (ZDecimal)4.3, goodsItem.AntiDumpingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.AntiDumpingDuty} record", 4.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.AntiDumpingDuty).BFE_ChargeAmount);

					goodsItem.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC01", "AC03" }, goodsItem.ADDAdditionalCodes);
					AssertEquals("AC01 matched, amount should be 24.3", (ZDecimal)24.3, goodsItem.AntiDumpingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.AntiDumpingDuty} record", 24.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.AntiDumpingDuty).BFE_ChargeAmount);
				});
			}
		}

		public void TestAntiDumpingDutyAmountWithNoRateMatched()
		{
			SetupDataForGetAdditionalCodesForRateTypeWithNoRateMached(Customs.Universal.Constants.RateTypes.AntiDumping);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Departure Good Item", goodsItemDeparture);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Arrival Good Item", goodsItemArrival);

			void AssertAntiDumpingDutyAmountWithNoRateMatched(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

					var supplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode.CY_Code = "AC00";

					AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), goodsItem.ADDAdditionalCodes);
					AssertEquals("No matched ,amount should be 0", ZDecimal.Zero, goodsItem.AntiDumpingDutyAmount);
					AssertEquals($"Fees should have no {NctsCommonCargoDesc.ChargeType.AntiDumpingDuty} record", 0, goodsItem.Fees.Count(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.AntiDumpingDuty));
				});
			}
		}

		public void TestAntiDumpingDutyAmount_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AntiDumpingDutyAmount Readonly for Departure Ncts5", true, goodsItemDeparture.AntiDumpingDutyAmountInfo.ReadOnly);
				AssertEquals("AntiDumpingDutyAmount Readonly for Arrival Ncts5", true, goodsItemArrival.AntiDumpingDutyAmountInfo.ReadOnly);
			});
		}

		public void TestAntiDumpingWithIncompleteHarmonisedTariff()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.AntiDumping);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Departure Good Item", goodsItemDeparture);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Arrival Good Item", goodsItemArrival);

			void AssertAntiDumpingDutyAmountWithNoRateMatched(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
				goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

				var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC02";
				CombineAssertions(assertionText, () =>
				{
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.ADDAdditionalCodes);

					goodsItem.BY_HarmonisedTariff = "DUMMY";
					AssertEquals("When Harmonised Tariff is incomplete, there should be no AntiDumping Additional Codes", 0, goodsItem.ADDAdditionalCodes.Count());
				});
			}
		}

		#endregion

		#region Countervailing

		public void TestCountervailingDutyAmount()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.Countervailing);
			AssertCountervailingDutyAmount("Departure Good Item", goodsItemDeparture);
			AssertCountervailingDutyAmount("Arrival Good Item", goodsItemArrival);

			void AssertCountervailingDutyAmount(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

					var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode1.CY_Code = "AC02";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.CVDAdditionalCodes);
					AssertEquals("AC02 matched, amount should be 4.3", (ZDecimal)4.3, goodsItem.CountervailingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.CountervailingDuty} record", 4.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.CountervailingDuty).BFE_ChargeAmount);

					supplementaryCode1.CY_Code = "AC04";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC01", "AC03" }, goodsItem.CVDAdditionalCodes);
					AssertEquals("AC01 matched, amount should be 24.3", (ZDecimal)24.3, goodsItem.CountervailingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.CountervailingDuty} record", 24.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.CountervailingDuty).BFE_ChargeAmount);

					var supplementaryCode2 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode2.CY_Code = "AC02";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02", "AC04" }, goodsItem.CVDAdditionalCodes);
					AssertEquals("AC02 matched, amount should equal 4.3", (ZDecimal)4.3, goodsItem.CountervailingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.CountervailingDuty} record", 4.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.CountervailingDuty).BFE_ChargeAmount);

					goodsItem.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC01", "AC03" }, goodsItem.CVDAdditionalCodes);
					AssertEquals("AC01 matched, amount should be 24.3", (ZDecimal)24.3, goodsItem.CountervailingDutyAmount);
					AssertEquals($"Fees should have {NctsCommonCargoDesc.ChargeType.CountervailingDuty} record", 24.3m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.CountervailingDuty).BFE_ChargeAmount);
				});
			}
		}

		public void TestCountervailingDutyAmountWithNoRateMatched()
		{
			SetupDataForGetAdditionalCodesForRateTypeWithNoRateMached(Customs.Universal.Constants.RateTypes.Countervailing);
			AssertCountervailingDutyAmountWithNoRateMatched("Departure Good Item", goodsItemDeparture);
			AssertCountervailingDutyAmountWithNoRateMatched("Arrival Good Item", goodsItemArrival);

			void AssertCountervailingDutyAmountWithNoRateMatched(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

					var supplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode.CY_Code = "AC00";

					AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), goodsItem.CVDAdditionalCodes);
					AssertEquals("No matched ,amount should be 0", ZDecimal.Zero, goodsItem.CountervailingDutyAmount);
					AssertEquals($"Fees should have no {NctsCommonCargoDesc.ChargeType.CountervailingDuty} record", 0, goodsItem.Fees.Count(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.CountervailingDuty));
				});
			}
		}

		public void TestCountervailingDutyAmount_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CountervailingDutyAmount Readonly for Departure Ncts5", true, goodsItemDeparture.CountervailingDutyAmountInfo.ReadOnly);
				AssertEquals("CountervailingDutyAmount Readonly for Arrival Ncts5", true, goodsItemArrival.CountervailingDutyAmountInfo.ReadOnly);
			});
		}

		public void TestCountervailingWithIncompleteHarmonisedTariff()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.Countervailing);
			AssertCountervailingDutyAmount("Departure Good Item", goodsItemDeparture);
			AssertCountervailingDutyAmount("Arrival Good Item", goodsItemArrival);

			void AssertCountervailingDutyAmount(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

					var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode1.CY_Code = "AC02";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.CVDAdditionalCodes);

					goodsItem.BY_HarmonisedTariff = "DUMMY";
					AssertEquals("When Harmonised Tariff is incomplete, there should be no Countervailing additional codes", 0, goodsItem.CVDAdditionalCodes.Count());
				});
			}
		}

		#endregion

		#region Liability

		public void TestLiabilityAmount_Phase4()
		{
			_ = SetupLiabilityRates();
			AssertLiabilityAmount("Departure Good Item", goodsItemDeparture);
			AssertLiabilityAmount("Arrival Good Item", goodsItemArrival);
			return;

			static void AssertLiabilityAmount(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					goodsItem.BY_HarmonisedTariff = "DUMMY_TARIFF";
					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
					goodsItem.BY_MonetaryValue = 1_000m;
					var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					var supplementaryCode2 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode1.CY_Code = "AC01";
					supplementaryCode2.CY_Code = "AC02";
					AssertEquals("[PRE-CONDITION] DutyAmount", 420m, goodsItem.DutyAmount);
					AssertEquals("[PRE-CONDITION] AntiDumpingDutyAmount", 9001m, goodsItem.AntiDumpingDutyAmount);
					AssertEquals("[PRE-CONDITION] CountervailingDutyAmount", 60_000m, goodsItem.CountervailingDutyAmount);
					AssertEquals($"{assertionText}: In Phase4, LiabilityAmount should be the sum of DutyAmount, AntiDumpingDutyAmount and CountervailingDutyAmount.", 69_421m, goodsItem.LiabilityAmount);
				});
			}
		}

		public void TestLiabilityAmount_Phase5()
		{
			var principal = SetUpGuarantee();
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			SetupLiabilityRates();

			CombineAssertions("In Phase5, LiabilityAmount should be the sum of DutyLiabilityAmount, VATLiabilityAmount and OtherLiabilityAmount.", () =>
			{
				goodsItemDeparture.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItemDeparture.BY_HarmonisedTariff = "DUMMY_TARIFF";
				goodsItemDeparture.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
				goodsItemDeparture.BY_MonetaryValue = 1_000m;
				var supplementaryCode1 = goodsItemDeparture.AdditionalSupplementaryCodes.AddNew();
				var supplementaryCode2 = goodsItemDeparture.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC01";
				supplementaryCode2.CY_Code = "AC02";
				AssertEquals("[PRE-CONDITION] DutyAmount", 420m, goodsItemDeparture.DutyAmount);
				AssertEquals("[PRE-CONDITION] VATAmount", 14084.2m, goodsItemDeparture.VatAmount);
				AssertEquals("[PRE-CONDITION] AntiDumpingDutyAmount", 9001m, goodsItemDeparture.AntiDumpingDutyAmount);
				AssertEquals("[PRE-CONDITION] CountervailingDutyAmount", 60_000m, goodsItemDeparture.CountervailingDutyAmount);
				AssertEquals("By default the rates for LiabilityAmount calculation is 100.", 83505.2m, goodsItemDeparture.LiabilityAmount);

				goodsItemDeparture.Header.Principal.E2_OA_Address = principal.MainAddress.PK;
				goodsItemDeparture.Header.MovementHeader.Guarantees.RemoveAndDeleteAll();
				goodsItemDeparture.Header.MovementHeader.Guarantees.AddNew().PW_BondNumber = "GUA#";
				AssertEquals("When guarantee rule configured, calculation should be done as DutyAmount * 0.5 + VATAmount * 0.7 + OtherFees * 0.3.", 30769.24m, goodsItemDeparture.LiabilityAmount);
			});

			OrgHeader SetUpGuarantee()
			{
				var principal = Factory.NewWithValidTestData<OrgHeader>();
				var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader.CPH_OH_PermitHolder = principal.PK;
				guaranteeHeader.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				guaranteeHeader.CPH_Number = "GUA#";
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

				var rulePCP = guaranteeHeader.CusGuaranteeRules.AddNew();
				rulePCP.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.PCP;
				rulePCP.CPR_ValueFrom = "30";
				var rulePCD = guaranteeHeader.CusGuaranteeRules.AddNew();
				rulePCD.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.PCD;
				rulePCD.CPR_ValueFrom = "50";
				var rulePCV = guaranteeHeader.CusGuaranteeRules.AddNew();
				rulePCV.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.PCV;
				rulePCV.CPR_ValueFrom = "70";
				Factory.Save();
				return principal;
			}
		}

		#endregion

		public void TestCanLoadUsingBaseClass() => CombineAssertions(() =>
		{
			AssertCanLoadUsingBaseClass<NctsDepartureCargoDesc>(NctsMovementType.Codes.Departure);
			AssertCanLoadUsingBaseClass<NctsArrivalCargoDesc>(NctsMovementType.Codes.Arrival);

			void AssertCanLoadUsingBaseClass<T>(string movementType) where T : NctsCommonCargoDesc
			{
				var assertionInfo = typeof(T).Name;
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(movementType);
				var nctsBill = nctsHeader.Bills.AddNew();
				var cargoDesc = Factory.New<T>();
				cargoDesc.BY_ParentTableCode = nctsBill.TablePrefix;
				cargoDesc.BY_ParentID = nctsBill.PK;
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var reloadedCargoDesc = newFactory.Load<T>(cargoDesc.PK);
				AssertNotNull($"{assertionInfo}: Reloaded CargoDesc", reloadedCargoDesc);
				AssertSame($"{assertionInfo}: Reloaded using table prefix", reloadedCargoDesc, newFactory.Load(reloadedCargoDesc.TablePrefix, reloadedCargoDesc.PK));
				AssertSame($"{assertionInfo}: Reloaded using type", reloadedCargoDesc, newFactory.Load<NctsCommonCargoDesc>(reloadedCargoDesc.PK));
			}
		});

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			company = Factory.New<GlbCompany>();
			branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_GB = branch.PK;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = header.ArrivalMovementHeader.GoodsItems.AddNew();

			headerDeparture = Factory.New<NctsHeader>();
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			bill = headerDeparture.Bills.AddNew();
			goodsItemDeparture = bill.GoodsItems.AddNew();
			goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);

			Factory.Save();
		}
		GlbCompany company;
		GlbBranch branch;
		NctsHeader header;
		NctsHeader headerDeparture;
		NctsBill bill;
		NctsCommonCargoDesc goodsItem;
		NctsCommonCargoDesc goodsItemDeparture;
		NctsCommonCargoDesc goodsItemArrival;

		protected override ZString CountryCode => Core.Constants.CountryCodes.Latvia;

		void AddSgisAndAdditionalInfosForTest()
		{
			AddSgiForTest("SGIXX", 12.1m);
			AddSgiForTest("SGIYY", 12.2m);
			AddSpecialMentionForTest("Special Mention", "XXXXX", true, "GB");
			AddSpecialMentionForTest("Special Mention", "YYYYY", true, "GB");
		}

		void AddSgiForTest(ZString sensitiveGoodsCode, ZDecimal sensitiveGoodsQuantity)
		{
			var sgi = goodsItem.AdditionalInfos.AddNew();
			sgi.CSI_Code = sensitiveGoodsCode;
			sgi.CSI_Description = sensitiveGoodsQuantity.ToString();
		}

		NctsAdditionalInfo AddSpecialMentionForTest(ZString additionalInfo, ZString additionalInfoType, ZBool exportFromEC, ZString exportFromCountry)
		{
			var sm = goodsItem.AdditionalInfos.AddNew();
			sm.CSI_Description = additionalInfo;
			sm.CSI_Code = additionalInfoType;
			sm.CSI_NctsExportFromEC = exportFromEC;
			sm.CSI_RN_NKCountryCode = exportFromCountry;
			return sm;
		}

		public void TestAdditionalInfoSequenceGenerator()
		{
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Documents type REF", goodsItem.RefSequenceNumberGenerator);
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Documents type INF", goodsItem.InfSequenceNumberGenerator);
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Documents type TRA", goodsItem.TraSequenceNumberGenerator);
		}

		void SetupDataForGetAdditionalCodesForRateType(string rateTypeCode)
		{
			(var helper, var testRate1, var testRate2, var tradeGroup) = SetupRatesForGetAdditionalCodes(rateTypeCode);

			helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

			NCTSTestHelper.SetupCusCodeForGetAdditionalCodes(helper, Factory);
		}

		void SetupDataForGetAdditionalCodesForRateTypeWithNoRateMached(string rateTypeCode)
		{
			(var helper, var testRate1, var testRate2, var tradeGroup) = SetupRatesForGetAdditionalCodes(rateTypeCode);

			helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");
			helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC04");

			NCTSTestHelper.SetupCusCodeForGetAdditionalCodes(helper, Factory);
		}

		(RateView dutyRate, RateView antiDumpingRate, RateView countervailingRate) SetupLiabilityRates()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			_ = helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");

			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "LV", ensureDataGroupingExists: false);
			Factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, tariffType.PK, "DUMMY_TARIFF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");

			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, ensureDataGroupingExists: false);
			var rateType1 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.Duty);
			var rateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.AntiDumping);
			var rateType3 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.Countervailing);
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", rateType1.PK);
			var rateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "RC2", rateType2.PK);
			var rateCode3 = helper.LoadOrCreateNewCusRateCode(Factory, "RC3", rateType3.PK);

			var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "  420.0 * [FLAT]");
			var testRate2 = helper.CreateRate(cusTariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, " 9001.0 * [FLAT]");
			var testRate3 = helper.CreateRate(cusTariff, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "60000.0 * [FLAT]");
			_ = helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "AC01");
			_ = helper.CreateCusApplicability(testRate3, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "AC02");

			helper.CreateTaxOrFee("ORD", 0.2m, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingVATApplicability(cusTariff, Core.Constants.CountryCodes.Latvia, "ORD");
			Factory.Save();

			return (testRate1, testRate2, testRate3);
		}

		(Universal.Testing.UniversalReferenceTestDataHelper, RateView, RateView, CusRefTradeGroupView) SetupRatesForGetAdditionalCodes(string rateTypeCode)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);
			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "LV", ensureDataGroupingExists: false);
			Factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, s1p1TariffType.PK, "DUMMYTRF1", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Latvia);
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, rateTypeCode, ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			var testRate1 = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");
			var testRate2 = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

			return (helper, testRate1, testRate2, tradeGroup);
		}

		(Universal.Testing.UniversalReferenceTestDataHelper, List<TariffView>) SetUpTariffRateFormula(BusinessObjectFactory factory, string rateTypeCode, string rateCodeText, Dictionary<string, string> tariffCodeRateFormula)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			factory.Save();

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");

			var rateType = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, rateTypeCode);
			var rateCode = helper.CreateCusRateCode(factory, rateCodeText, rateType.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var tariffsView = new List<TariffView> { };

			foreach (var item in tariffCodeRateFormula)
			{
				var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, item.Key, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
				var rate = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, item.Value, preference.PK);
				helper.CreateCusApplicability(rate.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				tariffsView.Add(tariff);
			}

			factory.Save();

			return (helper, tariffsView);
		}

		class NctsCommonCargoDescForTest : NctsCommonCargoDesc
		{
			public NctsCommonCargoDescForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Customs.Business.TariffFormatter GetNewTariffFormatterExposed() => GetNewTariffFormatter();
		}

		class NctsArrivalAndUnloadingCargoDescForTest : NctsArrivalAndUnloadingCargoDesc
		{
			public NctsArrivalAndUnloadingCargoDescForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void AttachToParent(NctsCommonMovementHeader parent)
			{
				BY_ParentTableCode = parent.TablePrefix;
				BY_ParentID = parent.PK;
			}

			protected override bool AutomaticSequenceNumberEnabled => false;
		}
	}
}
