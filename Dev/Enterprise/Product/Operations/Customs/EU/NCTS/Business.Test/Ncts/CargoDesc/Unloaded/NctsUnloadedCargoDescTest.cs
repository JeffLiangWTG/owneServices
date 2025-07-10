using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUnloadedCargoDesc))]
	sealed class NctsUnloadedCargoDescTest : NctsCommonCargoDescAbstractTest<NctsHeader>
	{
		public void TestHeader()
		{
			var (unloadedGoodsItem, arrivalCargoDesc) = GetNewBusinessObject(Factory);
			AssertSame(arrivalCargoDesc.Header, unloadedGoodsItem.Header);
		}

		public void TestMoveHeaderOrBillParent()
		{
			var unloadedGoodsItem = GetNewBusinessObject(Factory).UnloadedGoodsItem;
			AssertNull("Use ArrivalCargoDescParent instead", unloadedGoodsItem.MoveHeaderOrBillParent);
		}

		public void TestArrivalCargoDescParent()
		{
			var (unloadedGoodsItem, arrivalCargoDesc) = GetNewBusinessObject(Factory);
			AssertSame(arrivalCargoDesc, unloadedGoodsItem.ArrivalCargoDescParent);
		}

		public void TestBY_BY_Commodity()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("BY_BY_Commodity is always empty", ZGuid.Empty, unloadedCargoDesc.BY_BY_Commodity);
				AssertExceptionThrown<System.NotSupportedException>("Setting BY_BY_Commodity throws NotSupportedException", () => unloadedCargoDesc.BY_BY_Commodity = ZGuid.BrettsGuid);
			});
		}

		public void TestLookups()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertType<NctsUnloadedCargoDescLookups>(unloadedCargoDesc.Lookups);
		}

		public void TestValidation()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertType<NctsUnloadedCargoDescValidation>(unloadedCargoDesc.Validation);
		}

		public void TestLoadOrCreate_CreateNew()
		{
			var parent = Factory.New<NctsArrivalCargoDesc>();
			parent.BY_GrossWeightUnit = "KG";
			parent.BY_NetWeightUnit = "G";
			parent.ClearHasChanges();
			CombineAssertions(() =>
			{
				AssertEquals("Initially parent.BY_BY_Commodity is empty", ZGuid.Empty, parent.BY_BY_Commodity);
				var unloadedCargoDesc = NctsUnloadedCargoDesc.LoadOrCreate(parent);
				AssertEquals("New NctsUnloadedCargoDesc is created", false, unloadedCargoDesc.IsInDatabase);
				AssertEquals("parent.BY_BY_Commodity is set to unloadedCargoDesc.PK", unloadedCargoDesc.PK, parent.BY_BY_Commodity);
				AssertEquals("parent.HasChanges=True after creating new NctsUnloadedCargoDesc", true, parent.HasChanges);

				AssertEquals("BY_ParentID is set to parent.PK", parent.PK, unloadedCargoDesc.BY_ParentID);
				AssertEquals("BY_ParentTableCode is set to 'BY'", CusInBondCargoDescSchema.Constants.Prefix, unloadedCargoDesc.BY_ParentTableCode);

				AssertEquals("BY_GrossWeightUnit has the same value as parent", parent.BY_GrossWeightUnit, unloadedCargoDesc.BY_GrossWeightUnit);
				AssertEquals("BY_NetWeightUnit has the same value as parent", parent.BY_NetWeightUnit, unloadedCargoDesc.BY_NetWeightUnit);
			});
		}

		public void TestLoadOrCreate_LoadExisting()
		{
			var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
			var parent = Factory.New<NctsArrivalCargoDesc>();
			parent.BY_BY_Commodity = unloadedCargoDesc.PK;
			parent.ClearHasChanges();
			CombineAssertions(() =>
			{
				var unloadedCargoDesc2 = NctsUnloadedCargoDesc.LoadOrCreate(parent);
				AssertEquals("Existing NctsUnloadedCargoDesc is loaded", unloadedCargoDesc.PK, unloadedCargoDesc2.PK);
				AssertEquals("parent.BY_BY_Commodity not changed", unloadedCargoDesc.PK, parent.BY_BY_Commodity);
				AssertEquals("parent.HasChanges=False after loading existing NctsUnloadedCargoDesc", false, parent.HasChanges);
			});
		}

		public new void TestCorrectTypeDecideForLoad()
		{
			Assert(@"This test is temporarily disabled because NctsArrivalCargoDesc is not connected to NctsArrivalMovementHeader", true);
		}

		public void TestBY_Cusc4Number_ReadOnly()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Cusc4Number ReadOnly", false, unloadedCargoDesc.BY_CusC4NumberInfo.ReadOnly);
		}

		public void TestBY_NetWeightUnit_ReadOnly()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Net weight unit ReadOnly", true, unloadedCargoDesc.BY_NetWeightUnitInfo.ReadOnly);
		}

		public void TestBY_NetWeight_ReadOnly()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Net weight ReadOnly", false, unloadedCargoDesc.BY_NetWeightInfo.ReadOnly);
		}

		public void TestBY_GrossWeightUnit_ReadOnly()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Gross weight unit ReadOnly", true, unloadedCargoDesc.BY_GrossWeightUnitInfo.ReadOnly);
		}

		public void TestBY_GrossWeight_ReadOnly()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Gross weight ReadOnly", false, unloadedCargoDesc.BY_GrossWeightInfo.ReadOnly);
		}

		public void TestBY_Description_ReadOnly_()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Description ReadOnly", false, unloadedCargoDesc.BY_DescriptionInfo.ReadOnly);
		}

		public void TestBY_HarmonisedTariff_ReadOnly()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals("Harmonised Tariff ReadOnly", false, unloadedCargoDesc.BY_HarmonisedTariffInfo.ReadOnly);
		}

		public void TestBY_Cusc4Number_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(UnloadedCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsUnloadedCargoDesc)x).BY_CusC4NumberInfo.ReadOnly, UnloadedCargoDescForTestLockedDeclaration);
		}

		public void TestBY_NetWeightUnit_ReadOnly_Locked()
		{
			AssertEquals("Net Weight Unit is always readonly", true, UnloadedCargoDescForTestLockedDeclaration.BY_GrossWeightUnitInfo.ReadOnly);
		}

		public void TestBY_NetWeight_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(UnloadedCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsUnloadedCargoDesc)x).BY_NetWeightInfo.ReadOnly, UnloadedCargoDescForTestLockedDeclaration);
		}

		public void TestBY_GrossWeightUnit_ReadOnly_Locked()
		{
			AssertEquals("Gross Weight Unit is always readonly", true, UnloadedCargoDescForTestLockedDeclaration.BY_GrossWeightUnitInfo.ReadOnly);
		}

		public void TestBY_GrossWeight_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(UnloadedCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsUnloadedCargoDesc)x).BY_GrossWeightInfo.ReadOnly, UnloadedCargoDescForTestLockedDeclaration);
		}

		public void TestBY_Description_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(UnloadedCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsUnloadedCargoDesc)x).BY_DescriptionInfo.ReadOnly, UnloadedCargoDescForTestLockedDeclaration);
		}

		public void TestBY_HarmonisedTariff_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(UnloadedCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsUnloadedCargoDesc)x).BY_HarmonisedTariffInfo.ReadOnly, UnloadedCargoDescForTestLockedDeclaration);
		}

		public void TestTariffType()
		{
			var (unloadedCargoDesc, _) = GetNewBusinessObject(Factory);
			AssertEquals(TariffTypes.Export, unloadedCargoDesc.TariffType);
		}

		public void TestValuationDate()
		{
			var (unloadedGoodsItem, arrivalCargoDesc) = GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("DateTime Today", ZDateTime.Today, unloadedGoodsItem.ValuationDate);
				AssertEquals("Equal to arrival", arrivalCargoDesc.ValuationDate, unloadedGoodsItem.ValuationDate);
			});
		}

		public void TestSetTariffDefaultsGoodsDescriptionWhenEmpty()
		{
			var (unloadedGoodsItem, _) = GetNewBusinessObject(Factory);
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);
			NCTSTestHelper.AssertCargoDescSetting_BY_HarmonisedTariff_Synchronizes_BY_Description(unloadedGoodsItem);
		}

		public void TestSetTariffDefaultsTruncatedGoodsDescriptionWhenExceedsMaxLength()
		{
			var (unloadedGoodsItem, _) = GetNewBusinessObject(Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EXP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));

			unloadedGoodsItem.BY_HarmonisedTariff = "9999999999";
			var descriptionMaxLength = unloadedGoodsItem.BY_DescriptionInfo.MaxLength;
			CombineAssertions(() =>
			{
				AssertEquals("Truncated when Exceeds", descriptionMaxLength, unloadedGoodsItem.BY_Description.Length);
				AssertEquals("Correct value in Description", new string('0', descriptionMaxLength), unloadedGoodsItem.BY_Description);
			});
		}

		public void TestSetTariffDoesNotDefaultGoodsDescriptionWhenAlreadyFilledWithUnofficialDescription()
		{
			var (unloadedGoodsItem, _) = GetNewBusinessObject(Factory);
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);
			unloadedGoodsItem.BY_Description = "I'M NOT EMPTY SO I STAY HERE!";
			unloadedGoodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			AssertEquals("When setting BY_HarmonisedTariff and BY_Description is already filled with an unofficial description, BY_Description", "I'M NOT EMPTY SO I STAY HERE!", unloadedGoodsItem.BY_Description);
		}

		public void TestITariffDescriptionSynchronizerSupporterMembers()
		{
			var (unloadedGoodsItem, _) = GetNewBusinessObject(Factory);
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);
			NCTSTestHelper.AssertCargoDescITariffDescriptionSynchronizerSupporterMembers(unloadedGoodsItem);
		}

		public void TestLiabilityFormattedTariff()
		{
			CombineAssertions(() =>
			{
				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					var header = Factory.New<NctsHeader>();
					header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					header.SetMovementType(NctsMovementType.Codes.Arrival);
					NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
					var bill = header.Bills.AddNew();
					var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
					var unloadedGoodsItem = arrivalCargoDesc.UnloadedGoodsItem;
					unloadedGoodsItem.BY_HarmonisedTariff = "3333333333";
					AssertEquals("LiabilityFormattedTariff should be as other FormattedTariff", "3333.33.33 33", arrivalCargoDesc.LiabilityFormattedTariff);

					arrivalCargoDesc.LiabilityFormattedTariff = "3333.33.70 60";
					AssertEquals("LiabilityTariff should not be set from LiabilityFormattedTariff if 8 first chrs does not match with unloaded", "3333333333", arrivalCargoDesc.LiabilityTariff);

					arrivalCargoDesc.LiabilityFormattedTariff = "3333.33.33 60";
					AssertEquals("LiabilityTariff should be set from LiabilityFormattedTariff", "3333333360", arrivalCargoDesc.LiabilityTariff);
				}
			});
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).UnloadedGoodsItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).UnloadedGoodsItem;

		NctsUnloadedCargoDesc UnloadedCargoDescForTestLockedDeclaration
		{
			get
			{
				if (unloadedCargoDescForTestLockedDeclaration == null)
				{
					var (unloadedGoodsItem, arrivalCargoDesc) = GetNewBusinessObject(Factory);
					unloadedCargoDescForTestLockedDeclaration = unloadedGoodsItem;
				}
				return unloadedCargoDescForTestLockedDeclaration;
			}
		}
		NctsUnloadedCargoDesc unloadedCargoDescForTestLockedDeclaration;

		(NctsUnloadedCargoDesc UnloadedGoodsItem, NctsArrivalCargoDesc ArrivalCargoDesc) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			return (arrivalCargoDesc.UnloadedGoodsItem, arrivalCargoDesc);
		}
	}
}
