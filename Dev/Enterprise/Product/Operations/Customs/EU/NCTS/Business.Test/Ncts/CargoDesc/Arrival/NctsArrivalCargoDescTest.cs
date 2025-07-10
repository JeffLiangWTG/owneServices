using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsArrivalCargoDesc))]
	public abstract class NctsArrivalCargoDescAbstractTest<TMaster> : NctsCommonCargoDescAbstractTest<TMaster>
		where TMaster : NctsHeader
	{
		protected override void AssertAdditionalTypes(BusinessObject goodsItem)
		{
			AssertType("EU NctsArrivalCargoDesc", goodsItem.GetType(), new BusinessObjectFactory().Load<NctsArrivalCargoDesc>(goodsItem.PK));
		}
	}
	
	[TestedType(typeof(NctsArrivalCargoDesc))]
	sealed class NctsArrivalCargoDescTest : NctsArrivalCargoDescAbstractTest<NctsHeader>
	{
		public void TestConsigneeDocAddressRequirementType() => AssertType<JobDocAddressRequirement>(arrivalCargoDesc.ConsigneeDocAddressRequirement);

		public void TestPreviousDocuments()
		{
			var previousDocument = arrivalCargoDesc.PreviousDocuments.AddNew();
			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS;
			previousDocument.CSI_Code = "PD1";
			previousDocument.CSI_ReferenceNumber = "PD123";
			AssertEquals("PD123", arrivalCargoDesc.PreviousDocuments.Cast<NctsPreviousDocument>().Single().CSI_ReferenceNumber);
		}

		public void TestUNDGs()
		{
			arrivalCargoDesc.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("0004a", arrivalCargoDesc.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
		}

		public void TestConsigneeDocAddress()
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE2", arrivalCargoDesc.ConsigneeDocAddress, "2");
			NCTSTestHelper.AssertJobDocAddress(arrivalCargoDesc.ConsigneeDocAddress, DocAddressType.ConsigneeAddress, "2");
		}

		public void TestBY_Cusc4Number_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_CusC4NumberInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_UnloadedState_ReadOnly_Locked()
		{
			ArrivalCargoDescForTestLockedDeclaration.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_UnloadedStateInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_UnloadedState_Readonly_WhenIsMISAndMovementDetailIsMIS()
		{
			var bill = cusInBondMoveDetail.Bill;
			
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();

			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
			goodsItem1.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
			goodsItem2.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;
			CombineAssertions("Parent is not MIS", () =>
			{
				AssertEquals("BY_UnloadedState is 'DEC'", false, goodsItem1.BY_UnloadedStateInfo.ReadOnly);
				AssertEquals("BY_UnloadedState is 'MIS'", false, goodsItem2.BY_UnloadedStateInfo.ReadOnly);
			});

			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;
			goodsItem1.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
			goodsItem2.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;
			CombineAssertions("Parent is MIS", () =>
			{
				AssertEquals("BY_UnloadedState is 'DEC'", false, goodsItem1.BY_UnloadedStateInfo.ReadOnly);
				AssertEquals("BY_UnloadedState is 'MIS'", true, goodsItem2.BY_UnloadedStateInfo.ReadOnly);
			});
		}

		public void TestBY_NetWeightUnit_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_NetWeightUnitInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_NetWeight_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_NetWeightInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_GrossWeightUnit_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_GrossWeightUnitInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_GrossWeight_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_GrossWeightInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_Description_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_DescriptionInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_HarmonisedTariff_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_HarmonisedTariffInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_FormattedHarmonisedTariff_ReadOnly_Locked()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(ArrivalCargoDescForTestLockedDeclaration.Header.ArrivalMovementHeader, x => ((NctsArrivalCargoDesc)x).BY_FormattedHarmonisedTariffInfo.ReadOnly, ArrivalCargoDescForTestLockedDeclaration);
		}

		public void TestBY_Cusc4Number_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_CusC4NumberInfo.ReadOnly);
		}

		public void TestBY_UnloadedState_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_UnloadedStateInfo.ReadOnly);
		}

		public void TestBY_NetWeightUnit_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_NetWeightUnitInfo.ReadOnly);
		}

		public void TestBY_NetWeight_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_NetWeightInfo.ReadOnly);
		}

		public void TestBY_GrossWeightUnit_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_GrossWeightUnitInfo.ReadOnly);
		}

		public void TestBY_GrossWeight_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_GrossWeightInfo.ReadOnly);
		}

		public void TestBY_Description_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_DescriptionInfo.ReadOnly);
		}

		public void TestBY_HarmonisedTariff_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_HarmonisedTariffInfo.ReadOnly);
		}

		public void TestBY_FormattedHarmonisedTariff_ReadOnly_SentToCustoms()
		{
			AssertEquals(true, ArrivalCargoDescSentToCustoms.BY_FormattedHarmonisedTariffInfo.ReadOnly);
		}

		public void TestBY_CusC4Number_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_CusC4NumberInfo, true);
		}

		public void TestCY_Data_MaxLength()
		{
			AssertEquals(9, arrivalCargoDesc.BY_CusC4NumberInfo.MaxLength);
		}

		public void TestBY_BY_Commodity_RelatedBusinessObject()
		{
			AssertHasCustomAttribute<RelatedBusinessObjectAttribute>(typeof(NctsArrivalCargoDesc), NctsArrivalCargoDesc.Schema.BY_BY_Commodity, false, x => x.RelatedBizObjName == nameof(NctsArrivalCargoDesc.UnloadedGoodsItem));
		}

		public void TestUnloadedGoodsItemIsRegisteredEditableChildObject()
		{
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(true, arrivalCargoDesc.IsRegisteredEditableChildObject(arrivalCargoDesc.UnloadedGoodsItem));
		}

		public void TestUnloadedGoodsItem_UnloadedStateDIF_CreateNew()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Initially BY_BY_Commodity is empty", ZGuid.Empty, arrivalCargoDesc.BY_BY_Commodity);
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedGoodsItem = arrivalCargoDesc.UnloadedGoodsItem;
				AssertNotNull("Calling UnloadedGoodsItem creates NctsUnloadedCargoDesc", unloadedGoodsItem);
				AssertEquals("BY_BY_Commodity stores PK of created NctsUnloadedCargoDesc", unloadedGoodsItem.PK, arrivalCargoDesc.BY_BY_Commodity);
			});
		}

		public void TestUnloadedGoodsItem_UnloadedStateDIF_LoadExisting()
		{
			var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
			arrivalCargoDesc.BY_BY_Commodity = unloadedCargoDesc.PK;
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("Existing NctsUnloadedCargoDesc is loaded", unloadedCargoDesc.PK, arrivalCargoDesc.UnloadedGoodsItem.PK);
		}

		public void TestUnloadedGoodsItem_UnloadedStateDIF_CreateNewWhenExistingGoodsItemIsDeleted()
		{
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedGoodsItem1 = arrivalCargoDesc.UnloadedGoodsItem;
				AssertEquals("BY_BY_Commodity stores unloadedGoodsItem1.PK", unloadedGoodsItem1.PK, arrivalCargoDesc.BY_BY_Commodity);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("unloadedGoodsItem1 deleted", true, unloadedGoodsItem1.IsDeleted);
				AssertEquals("BY_BY_Commodity is empty", ZGuid.Empty, arrivalCargoDesc.BY_BY_Commodity);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedGoodsItem2 = arrivalCargoDesc.UnloadedGoodsItem;
				AssertNotEquals("New NctsUnloadedCargoDesc is created", unloadedGoodsItem1.PK, unloadedGoodsItem2.PK);
				AssertEquals("BY_BY_Commodity stores new NctsUnloadedCargoDesc", unloadedGoodsItem2.PK, arrivalCargoDesc.BY_BY_Commodity);
			});
		}

		public void TestUnloadedGoodsItem_UnloadedStateNotDIF()
		{
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertNull("UnloadedGoodsItem is null", arrivalCargoDesc.UnloadedGoodsItem);
				AssertEquals("BY_BY_Commodity is empty", ZGuid.Empty, arrivalCargoDesc.BY_BY_Commodity);
			});
		}

		public void TestChangingUnloadedStateToNonDIFDeletesExistingUnloadedGoodsItem()
		{
			AssertChangingUnloadedStateToNonDIFDeletesExistingUnloadedGoodsItem(NctsUnloadedStateList.Codes.DEC);
		}

		public void TestChangingUnloadedStateToInvalidStateDeletesExistingUnloadedGoodsItem()
		{
			AssertChangingUnloadedStateToNonDIFDeletesExistingUnloadedGoodsItem("XYZ");
		}

		public void TestDeletingBizoDeletesExistingUnloadedGoodsItem()
		{
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = ZString.Empty;
				arrivalCargoDesc.Delete();
				AssertEquals("Deleting BO when BY_UnloadedState is empty, no NctsUnloadedCargoDesc created", false, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Any(x => x is NctsUnloadedCargoDesc));

				var arrivalCargoDesc2 = Factory.New<NctsArrivalCargoDesc>();
				arrivalCargoDesc2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedGoodsItem = arrivalCargoDesc2.UnloadedGoodsItem;
				AssertNotNull("UnloadedGoodsItem not null", unloadedGoodsItem);
				AssertEquals("Only 1 NctsUnloadedCargoDesc in Factory", 1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Count(x => x is NctsUnloadedCargoDesc));

				arrivalCargoDesc2.Delete();
				AssertEquals("UnloadedGoodsItem is deleted", true, unloadedGoodsItem.IsDeleted);
				AssertEquals("Deleting BO, no NctsUnloadedCargoDesc created", 1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Count(x => x is NctsUnloadedCargoDesc));
			});
		}

		public void TestBY_UnloadedState_Editable()
		{
			CombineAssertions(() =>
			{
				foreach (var state in new NctsUnloadedStateList().GetAllCodes())
				{
					arrivalCargoDesc.BY_UnloadedState = state;
					AssertEquals($"BY_UnloadedState = '{state}'", state == NctsUnloadedStateList.Codes.NEW, arrivalCargoDesc.BY_UnloadedStateInfo.ReadOnly);
				}
			});
		}

		public void TestBY_LineNo_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_LineNoInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Sequence No.", shortCaption: "Seq.No.", fullDescription: "Sequence Number");
		}

		public void TestBY_CusC4Number_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_CusC4NumberInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "CUS Code", shortCaption: "CUS Cd");
		}

		public void TestBY_Description_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_DescriptionInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Description", shortCaption: "Descr.", fullDescription: "Description of goods");
		}

		public void TestBY_GrossWeight_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_GrossWeightInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Gross Weight", shortCaption: "Gross Wgt.", fullDescription: "Gross Weight of the Goods");
		}

		public void TestBY_NetWeight_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_NetWeightInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Net Weight", shortCaption: "Net Wgt.", fullDescription: "Net Weight of the Goods");
		}

		public void TestBY_GrossWeightUnit_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_GrossWeightUnitInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Units", fullDescription: "Gross Weight Unit qualifier");
		}

		public void TestBY_NetWeightUnit_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_NetWeightUnitInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Units", fullDescription: "Net Weight Unit qualifier");
		}

		public void TestBY_CommodityCode_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_CommodityCodeInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Commodity", shortCaption: "Cmdty.", fullDescription: "Commodity Code");
		}

		public void TestBY_HarmonisedTariff_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(ArrivalCargoDescForCaptionTest.BY_HarmonisedTariffInfo, ArrivalCargoDescForCaptionTest.MultipleKeysToUse, caption: "Commodity", shortCaption: "Cmdty.", mediumCaption: "Commodity Code", fullDescription: "Commodity Code");
		}

		public void TestBY_UnloadedState_Caption()
		{
			AssertEquals("Unloaded State", DataBoundResourceStrings.GetDataForProperty(ArrivalCargoDescForCaptionTest.BY_UnloadedStateInfo).Caption);
		}

		public void TestLiabilityFormattedTariff_Caption()
		{
			NCTSTestHelper.AssertCaptions(ArrivalCargoDescForCaptionTest.LiabilityFormattedTariffInfo, "Commodity Code", "Commodity", "Cmdty.");
		}

		public void TestLiabilityTariff_Caption()
		{
			NCTSTestHelper.AssertCaptions(ArrivalCargoDescForCaptionTest.LiabilityTariffInfo, "Commodity Code", "Commodity", "Cmdty.");
		}

		public void TestBY_HarmonisedTariff_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_HarmonisedTariffInfo, false);
		}

		public void TestBY_FormattedHarmonisedTariff_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_FormattedHarmonisedTariffInfo, false);
		}

		public void TestBY_Description_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_DescriptionInfo, true);
		}

		public void TestBY_GrossWeight_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_GrossWeightInfo, true);
		}

		public void TestBY_GrossWeightUnit_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_GrossWeightUnitInfo, true);
		}

		public void TestBY_NetWeight_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_NetWeightInfo, true);
		}

		public void TestBY_NetWeightUnit_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(x => x.BY_NetWeightUnitInfo, true);
		}

		public void TestPreviousDocuments_ReadOnly_Phase4()
		{
			arrivalCargoDesc.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("PreviousDocuments not ReadOnly for Arrival NCTS4", false, arrivalCargoDesc.PreviousDocuments.ReadOnly);
		}

		public void TestPreviousDocuments_ReadOnly_Phase5()
		{
			AssertEquals("PreviousDocuments ReadOnly for Arrival NCTS5", true, arrivalCargoDesc.PreviousDocuments.ReadOnly);
		}

		public void TestSetTariffDefaultsGoodsDescriptionWhenEmpty()
		{
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);
			NCTSTestHelper.AssertCargoDescSetting_BY_HarmonisedTariff_Synchronizes_BY_Description(arrivalCargoDesc);
		}

		public void TestSetTariffDefaultsTruncatedGoodsDescriptionWhenExceedsMaxLength()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EXP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));

			arrivalCargoDesc.BY_HarmonisedTariff = "9999999999";
			var descriptionMaxLength = arrivalCargoDesc.BY_DescriptionInfo.MaxLength;
			CombineAssertions(() =>
			{
				AssertEquals("Truncated when Exceeds", descriptionMaxLength, arrivalCargoDesc.BY_Description.Length);
				AssertEquals("Correct value in Description", new string('0', descriptionMaxLength), arrivalCargoDesc.BY_Description);
			});
		}

		public void TestSetTariffDoesNotDefaultGoodsDescriptionWhenAlreadyFilledWithUnofficialDescription()
		{
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);
			arrivalCargoDesc.BY_Description = "I'M NOT EMPTY SO I STAY HERE!";
			arrivalCargoDesc.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			AssertEquals("When setting BY_HarmonisedTariff and BY_Description is already filled with an unofficial description, BY_Description", "I'M NOT EMPTY SO I STAY HERE!", arrivalCargoDesc.BY_Description);
		}

		public void TestITariffDescriptionSynchronizerSupporterMembers()
		{
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);
			NCTSTestHelper.AssertCargoDescITariffDescriptionSynchronizerSupporterMembers(arrivalCargoDesc);
		}

		public void TestLookups()
		{
			AssertType<NctsArrivalCargoDescLookups>(arrivalCargoDesc.Lookups);
		}

		public void TestValidation()
		{
			AssertType<NctsArrivalCargoDescValidation>(arrivalCargoDesc.Validation);
		}

		public new void TestCorrectTypeDecideForLoad()
		{
			Assert(@"This test is temporarily disabled because NctsArrivalCargoDesc is not connected to NctsArrivalMovementHeader", true);
		}

		public void TestDeclaredNewLabel()
		{
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("New", "New Value", arrivalCargoDesc.DeclaredNewLabel);

			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("Declared", "Declared Value", arrivalCargoDesc.DeclaredNewLabel);
		}

		public void TestUnloadedColumnsVisible()
		{
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = "NEW";
				AssertEquals("When unloaded state <> DIF then the unloaded columns should not be visible", false, arrivalCargoDesc.UnloadedColumsVisible);
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertEquals("When unloaded state = DIF then the unloaded columns should be visible", true, arrivalCargoDesc.UnloadedColumsVisible);
			});
		}

		public void TestCopyDeclaredValuesToUnloadingValues()
		{
			arrivalCargoDesc.BY_HarmonisedTariff = "Commodity";
			arrivalCargoDesc.BY_CusC4Number = "CUSC";
			arrivalCargoDesc.BY_Description = "Description";
			arrivalCargoDesc.BY_GrossWeight = 4857.87;
			arrivalCargoDesc.BY_GrossWeightUnit = "KG";
			arrivalCargoDesc.BY_NetWeight = 1446.78;
			arrivalCargoDesc.BY_NetWeightUnit = "KG";
			arrivalCargoDesc.BY_UnloadedState = "DIF";

			CombineAssertions(() =>
			{
				AssertEquals("BY_HarmonisedTariff", "Commodity", arrivalCargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff);
				AssertEquals("BY_CusC4Number", "CUSC", arrivalCargoDesc.UnloadedGoodsItem.BY_CusC4Number);
				AssertEquals("BY_Description", "Description", arrivalCargoDesc.UnloadedGoodsItem.BY_Description);
				AssertEquals("BY_GrossWeight", new ZDecimal(4857.87), arrivalCargoDesc.UnloadedGoodsItem.BY_GrossWeight);
				AssertEquals("BY_GrossWeightUnit", "KG", arrivalCargoDesc.UnloadedGoodsItem.BY_GrossWeightUnit);
				AssertEquals("BY_NetWeight", new ZDecimal(1446.78), arrivalCargoDesc.UnloadedGoodsItem.BY_NetWeight);
				AssertEquals("BY_NetWeightUnit", "KG", arrivalCargoDesc.UnloadedGoodsItem.BY_NetWeightUnit);
			});
		}

		public void TestTariffType()
		{
			AssertEquals(TariffTypes.Export, arrivalCargoDesc.TariffType);
		}

		public void TestValuationDate()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.ArrivalMovementHeader.BM_ValuationDate = new ZDateTime(2023, 1, 1);
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc2 = bill.ArrivalGoodsItems.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.Today, arrivalCargoDesc.ValuationDate);
				AssertEquals(header.ArrivalMovementHeader.BM_ValuationDate, arrivalCargoDesc2.ValuationDate);
			});
		}

		public void TestCanDelete()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var billGoodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
			CombineAssertions(() =>
			{
				billGoodsItem.BY_UnloadedState = ZString.Empty;
				AssertEquals("BY_UnloadedState empty", false, billGoodsItem.CanDelete);

				billGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("BY_UnloadedState = 'DEC'", false, billGoodsItem.CanDelete);

				billGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("BY_UnloadedState = 'NEW'", true, billGoodsItem.CanDelete);
			});
		}

		public void TestIsUnloadedCommodityCodeRequired()
		{
			AssertEquals(expected: true, arrivalCargoDesc.IsUnloadedCommodityCodeRequired);
		}

		public void TestSetAllUnloadedStateToDEC_whenDIF()
		{
			AssertAllUnloadedStateToDEC("DIF");
		}

		public void TestSetAllUnloadedStateToDEC_whenMIS()
		{
			AssertAllUnloadedStateToDEC("MIS");
		}

		public void TestSetAllUnloadedStateToDEC_whenDEC()
		{
			AssertAllUnloadedStateToDEC("DEC");
		}

		void AssertAllUnloadedStateToDEC(string unloadedState)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();
			bill.MovementDetail.B9_UnloadedState = "DEC";

			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = unloadedState;
			var supportingDoc_GoodsItem = goodsItem.SupportingDocuments.AddNew();
			supportingDoc_GoodsItem.CSI_Status = "DIF";
			var additionalDoc_GoodsItem = goodsItem.AdditionalInfos.AddNew();
			additionalDoc_GoodsItem.CSI_Status = "NEW";
			var package = goodsItem.Packages.AddNew();
			package.B5_TypeOfDifference = "DIF";

			goodsItem.SetAllUnloadedStateToDEC();
			CombineAssertions(() =>
			{
				AssertEquals("UnloadedState of goodsItem has been changed to or is DEC", "DEC", goodsItem.BY_UnloadedState);
				AssertEquals("When unload state is NEW, additionalDocument will be delete", 0, bill.AdditionalDocuments.Count);
				AssertEquals("Unload State of supportingDocument under goodsItem has been update from 'DIF' to 'DEC'", "DEC", supportingDoc_GoodsItem.CSI_Status);
				AssertEquals("When unload state is NEW, additionalDocument under goodsItem will be delete", 0, goodsItem.AdditionalInfos.Count);
				AssertEquals("Unload State of package under goodsItem has been update from 'DIF' to 'DEC'", "DEC", package.B5_TypeOfDifference);
			});
		}

		public void TestLiabilityTariff_GenAddOnColumn()
		{
			CombineAssertions(() =>
			{
				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					AssertEquals("LiabilityTariff should not change if arrival.BY_HarmonisedTariff is empty", ZString.Empty, arrivalCargoDesc.LiabilityTariff);

					arrivalCargoDesc.BY_HarmonisedTariff = "2222222222";
					AssertEquals("LiabilityTariff should change to be arrival.BY_HarmonisedTariff when change and not DIF", "2222222222", arrivalCargoDesc.LiabilityTariff);

					arrivalCargoDesc.LiabilityTariff = "2222222290";
					AssertEquals("LiabilityTariff should be be set if value is set in LiabilityTariff", "2222222290", arrivalCargoDesc.LiabilityTariff);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
					var unloadedGoodsItem = arrivalCargoDesc.UnloadedGoodsItem;
					unloadedGoodsItem.BY_HarmonisedTariff = "3333333333";
					AssertEquals("LiabilityTariff should change to be unloaded.BY_HarmonisedTariff when change and DIF", "3333333333", arrivalCargoDesc.LiabilityTariff);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
					AssertEquals("LiabilityTariff should be arrival.BY_HarmonisedTariff when status change and no DIF", "2222222222", arrivalCargoDesc.LiabilityTariff);
				}
			});
		}

		public void TestLiabilityTariffPersistance()
		{
			var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "LiabilityTariff");

			arrivalCargoDesc.LiabilityTariff = "1234567890";

			CombineAssertions(() =>
			{
				AssertEquals("LiabilityTariff", "1234567890", arrivalCargoDesc.LiabilityTariff);
				AssertNotNull("LiabilityTariff is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
			});
		}

		public void TestLiabilityFormattedTariff()
		{
			CombineAssertions(() =>
			{
				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDesc.BY_HarmonisedTariff = "2222222222";
					AssertEquals("LiabilityFormattedTariff should be as other FormattedTariff", "2222.22.22 22", arrivalCargoDesc.LiabilityFormattedTariff);

					arrivalCargoDesc.LiabilityFormattedTariff = "2222.22.50 50";
					AssertEquals("LiabilityTariff should not be set from LiabilityFormattedTariff if 8 first chrs does not match declared", "2222222222", arrivalCargoDesc.LiabilityTariff);

					arrivalCargoDesc.LiabilityFormattedTariff = "2222.22.22 60";
					AssertEquals("LiabilityTariff should be set from LiabilityFormattedTariff", "2222222260", arrivalCargoDesc.LiabilityTariff);
				}
			});
		}

		public void TestLiabilityFormattedTariffMaxLength()
		{
			AssertEquals(22, arrivalCargoDesc.LiabilityFormattedTariffInfo.MaxLength);
		}

		public void TestUpdateAllFeesFromTariffRatesWhenLiabilityTariffChange()
		{
			NCTSTestHelper.SetUpTariff(Factory, tariffTypeCode: TariffTypes.Export);

			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_MonetaryValue = 1_000m;
				AssertEquals("[PreReq] Duty is zero", ZDecimal.Zero, arrivalCargoDesc.DutyAmount);
				arrivalCargoDesc.LiabilityTariff = NCTSTestHelper.TestTariffCode;
				AssertEquals("Duty is calculated when LiabilityTariff change", 120m, arrivalCargoDesc.DutyAmount);
			});
		}

		public void TestRemoveDataFromLiabilityCalculationTabWhenUnloadedStatusChange()
		{
			NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
			CombineAssertions(() =>
			{
				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					arrivalCargoDesc.BY_RN_NKCountryOfOrigin = "LV";
					arrivalCargoDesc.BY_HarmonisedTariff = "2222222222";
					arrivalCargoDesc.BY_CustomsSecondQuantity = 1m;
					arrivalCargoDesc.BY_CustomsSecondUnitQty = "NAR";
					arrivalCargoDesc.BY_CustomsThirdQuantity = 1m;
					arrivalCargoDesc.BY_CustomsThirdUnitQty = "NAR";
					arrivalCargoDesc.BY_CustomsFourthQuantity = 1m;
					arrivalCargoDesc.BY_CustomsFourthUnitQty = "NAR";
					arrivalCargoDesc.BY_MonetaryValue = 1m;
					arrivalCargoDesc.AdditionalSupplementaryCodes.AddNew();

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

					AssertEquals("BY_RN_NKCountryOfOrigin is not removed when isLiabilityCalculationForArrivalSupported is true", "LV", arrivalCargoDesc.BY_RN_NKCountryOfOrigin);
					AssertEquals("LiabilityTariff is not removed when isLiabilityCalculationForArrivalSupported is true", "2222222222", arrivalCargoDesc.LiabilityTariff);
					AssertEquals("BY_CustomsSecondQuantity is not removed when isLiabilityCalculationForArrivalSupported is true", 1m, arrivalCargoDesc.BY_CustomsSecondQuantity);
					AssertEquals("BY_CustomsSecondUnitQty is not removed when isLiabilityCalculationForArrivalSupported is true", "NAR", arrivalCargoDesc.BY_CustomsSecondUnitQty);
					AssertEquals("BY_CustomsThirdQuantity is not removed when isLiabilityCalculationForArrivalSupported is true", 1m, arrivalCargoDesc.BY_CustomsThirdQuantity);
					AssertEquals("BY_CustomsThirdUnitQty is not removed when isLiabilityCalculationForArrivalSupported is true", "NAR", arrivalCargoDesc.BY_CustomsThirdUnitQty);
					AssertEquals("BY_CustomsFourthQuantity is not removed when isLiabilityCalculationForArrivalSupported is true", 1m, arrivalCargoDesc.BY_CustomsFourthQuantity);
					AssertEquals("BY_CustomsFourthUnitQty is not removed when isLiabilityCalculationForArrivalSupported is true", "NAR", arrivalCargoDesc.BY_CustomsFourthUnitQty);
					AssertEquals("BY_MonetaryValue is not removed when isLiabilityCalculationForArrivalSupported is true", 1m, arrivalCargoDesc.BY_MonetaryValue);
					AssertEquals("AdditionalSupplementaryCodes are not removed when isLiabilityCalculationForArrivalSupported is true", 1, arrivalCargoDesc.AdditionalSupplementaryCodes.Count);
				}

				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: false))
				{
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

					AssertEquals("BY_RN_NKCountryOfOrigin is removed when isLiabilityCalculationForArrivalSupported is false", ZString.Empty, arrivalCargoDesc.BY_RN_NKCountryOfOrigin);
					AssertEquals("LiabilityTariff is removed when isLiabilityCalculationForArrivalSupported is false", ZString.Empty, arrivalCargoDesc.LiabilityTariff);
					AssertEquals("BY_CustomsSecondQuantity is removed when isLiabilityCalculationForArrivalSupported is false", ZDecimal.Zero, arrivalCargoDesc.BY_CustomsSecondQuantity);
					AssertEquals("BY_CustomsSecondUnitQty is removed when isLiabilityCalculationForArrivalSupported is false", ZString.Empty, arrivalCargoDesc.BY_CustomsSecondUnitQty);
					AssertEquals("BY_CustomsThirdQuantity is removed when isLiabilityCalculationForArrivalSupported is false", ZDecimal.Zero, arrivalCargoDesc.BY_CustomsThirdQuantity);
					AssertEquals("BY_CustomsThirdUnitQty is removed when isLiabilityCalculationForArrivalSupported is false", ZString.Empty, arrivalCargoDesc.BY_CustomsThirdUnitQty);
					AssertEquals("BY_CustomsFourthQuantity is removed when isLiabilityCalculationForArrivalSupported is false", ZDecimal.Zero, arrivalCargoDesc.BY_CustomsFourthQuantity);
					AssertEquals("BY_CustomsFourthUnitQty is removed when isLiabilityCalculationForArrivalSupported is false", ZString.Empty, arrivalCargoDesc.BY_CustomsFourthUnitQty);
					AssertEquals("BY_MonetaryValue is removed when isLiabilityCalculationForArrivalSupported is false", ZDecimal.Zero, arrivalCargoDesc.BY_MonetaryValue);
					AssertEquals("AdditionalSupplementaryCodes are removed when isLiabilityCalculationForArrivalSupported is false", 0, arrivalCargoDesc.AdditionalSupplementaryCodes.Count);
				}
			});
		}

		public void TestIsLiabilityCalculationForArrivalSupported()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovementHeader = Factory.New<NCTSTestHelper.NctsArrivalMovementHeaderForLiabilityTest>();

			var goodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();

			CombineAssertions(() =>
			{
				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					AssertEquals("IsLiabilityCalculationForArrivalSupported is false when ShouldGuaranteeForArrivalBeVisible is false", false, goodsItem.IsLiabilityCalculationForArrivalSupported);
					header.MovementHeaders.RemoveAllFromRelationship();
					header.MovementHeaders.Add(arrivalMovementHeader);

					goodsItem.BY_UnloadedState = "MIS";
					AssertEquals("IsLiabilityCalculationForArrivalSupported is false when Unloaded State MIS and configuration.IsLiabilityCalculationForArrivalSupported is true", false, goodsItem.IsLiabilityCalculationForArrivalSupported);

					goodsItem.BY_UnloadedState = "NEW";
					AssertEquals("IsLiabilityCalculationForArrivalSupported is true when Unloaded State not MIS, ShouldGuaranteeForArrivalBeVisible is true and configuration.IsLiabilityCalculationForArrivalSupported is true", true, goodsItem.IsLiabilityCalculationForArrivalSupported);
				}

				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: false))
				{
					AssertEquals("IsLiabilityCalculationForArrivalSupported is false when configuration.IsLiabilityCalculationForArrivalSupported is false", false, goodsItem.IsLiabilityCalculationForArrivalSupported);
				}
			});
		}

		public void TestBY_RX_NKCurrency_Attributes() => CombineAssertions(() =>
			AssertEntity<NctsArrivalCargoDesc>()
				.HasProperty(x => x.BY_RX_NKCurrency)
				.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly));

		protected override ZString CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			return bill.ArrivalGoodsItems.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();

			arrivalCargoDesc.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
			return arrivalCargoDesc;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			cusInBondMoveDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS5);
		}
		NctsHeader header;
		NctsArrivalCargoDesc arrivalCargoDesc;
		CusInBondMoveDetail cusInBondMoveDetail;

		static CusInBondMoveDetail CreateMoveDetail(BusinessObjectFactory factory, string applicationCode, string declarationType = NctsMovementType.Codes.Arrival)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = applicationCode;
			header.SetMovementType(declarationType);
			return applicationCode == CusInBondApplicationCodeList.Codes.NCTS5
				? header.Bills.AddNew().MovementDetail
				: header.ArrivalMovementHeader.MovementDetails.AddNew();
		}

		NctsArrivalCargoDesc ArrivalCargoDescForTestLockedDeclaration
		{
			get
			{
				if (arrivalCargoDescForTestLockedDeclaration == null)
				{
					var header = Factory.New<NctsHeader>();
					header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					header.SetMovementType("A");
					var moveHeader = header.ArrivalMovementHeader;
					moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
					var bill = header.Bills.AddNew();
					var cargoDesc = bill.ArrivalGoodsItems.AddNew();
					cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDescForTestLockedDeclaration = cargoDesc;
				}
				return arrivalCargoDescForTestLockedDeclaration;
			}
		}
		NctsArrivalCargoDesc arrivalCargoDescForTestLockedDeclaration;

		NctsArrivalCargoDesc ArrivalCargoDescSentToCustoms
		{
			get
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType("A");
				header.ArrivalMovementHeader.BM_MessageStatus = "SNT";
				var bill = header.Bills.AddNew();
				var cargoDesc = bill.ArrivalGoodsItems.AddNew();
				return cargoDesc;
			}
		}

		NctsArrivalCargoDesc ArrivalCargoDescForCaptionTest
		{
			get
			{
				if (arrivalCargoDescForCaptionTest == null)
				{
					arrivalCargoDescForCaptionTest = Factory.New<NctsArrivalCargoDesc>();
				}
				return arrivalCargoDescForCaptionTest;
			}
		}
		NctsArrivalCargoDesc arrivalCargoDescForCaptionTest;

		void AssertPropertyIsReadOnlyWhenBY_UnloadedStateIsNotNEW(Func<NctsArrivalCargoDesc, ZPropertyInfo> getPropertyInfo, bool readOnlyWhenEmpty)
		{
			var propertyInfo = getPropertyInfo.Invoke(arrivalCargoDesc);
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = ZString.Empty;
				AssertEquals("BY_UnloadedState empty", readOnlyWhenEmpty, propertyInfo.ReadOnly);

				foreach (var state in new NctsUnloadedStateList().GetAllCodes())
				{
					arrivalCargoDesc.BY_UnloadedState = state;
					AssertEquals($"BY_UnloadedState = '{state}'", state != NctsUnloadedStateList.Codes.NEW, propertyInfo.ReadOnly);
				}
			});
		}

		void AssertChangingUnloadedStateToNonDIFDeletesExistingUnloadedGoodsItem(string newUnloadedState)
		{
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = ZString.Empty;
				AssertNull("BY_UnloadedState empty, UnloadedGoodsItem null", arrivalCargoDesc.UnloadedGoodsItem);
				arrivalCargoDesc.BY_UnloadedState = newUnloadedState;
				AssertEquals($"Changing BY_UnloadedState to '{newUnloadedState}', no NctsUnloadedCargoDesc created", false, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Any(x => x is NctsUnloadedCargoDesc));

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedGoodsItem = arrivalCargoDesc.UnloadedGoodsItem;
				AssertNotNull("BY_UnloadedState='DIF', UnloadedGoodsItem is not null", unloadedGoodsItem);
				AssertEquals("BY_UnloadedState='DIF', BY_BY_Commodity stores UnloadedGoodsItem.PK", unloadedGoodsItem.PK, arrivalCargoDesc.BY_BY_Commodity);
				AssertEquals("Only 1 NctsUnloadedCargoDesc in Factory", 1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Count(x => x is NctsUnloadedCargoDesc));

				arrivalCargoDesc.BY_UnloadedState = newUnloadedState;
				AssertEquals($"Changing BY_UnloadedState to '{newUnloadedState}', NctsUnloadedCargoDesc is deleted", true, unloadedGoodsItem.IsDeleted);
				AssertNull($"Changing BY_UnloadedState to '{newUnloadedState}', UnloadedGoodsItem is null", arrivalCargoDesc.UnloadedGoodsItem);
				AssertEquals($"Changing BY_UnloadedState to '{newUnloadedState}', BY_BY_Commodity is empty", ZGuid.Empty, arrivalCargoDesc.BY_BY_Commodity);
				AssertEquals($"Changing BY_UnloadedState to '{newUnloadedState}', no NctsUnloadedCargoDesc created", 1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Count(x => x is NctsUnloadedCargoDesc));
			});
		}
	}
}
