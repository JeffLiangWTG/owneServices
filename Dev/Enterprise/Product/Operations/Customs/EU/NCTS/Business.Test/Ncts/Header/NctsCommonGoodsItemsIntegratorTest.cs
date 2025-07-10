using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsCommonGoodsItemsIntegratorTest : TestCaseWithFactory
	{
		public void TestGetCommonGoodsItemsForIntegration_Phase4()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.MovementHeader.GoodsItems.AddNew();
			nctsHeader.MovementHeader.GoodsItems.AddNew();

			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
			var collection = commonGoodsItemsIntegrator.GetCommonGoodsItemsForIntegration();
			AssertEquals(2, collection.Count());
		}

		public void TestGetCommonGoodsItemsForIntegration_Phase5()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill1 = nctsHeader.Bills.AddNew();
			bill1.GoodsItems.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			bill2.GoodsItems.AddNew();
			bill2.GoodsItems.AddNew();

			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
			var collection = commonGoodsItemsIntegrator.GetCommonGoodsItemsForIntegration();
			AssertEquals(3, collection.Count());
		}

		public void TestConvertToCommonGoodsItem()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_JobReference = "123ABC";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";

			var houseBill = nctsHeader.Bills.AddNew();
			houseBill.B0_ReferenceID = "HB:HB-1234";

			var item = houseBill.GoodsItems.AddNew();
			item.BY_Description = "Goods Description";
			item.BY_GrossWeight = 1.5m;
			item.BY_NetWeight = 2.9m;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			item.BY_FormattedHarmonisedTariff = "1202.30.00 00";
			item.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Portugal;
			item.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;
			item.BY_MonetaryValue = 100.00m;
			item.BY_CustomsSecondQuantity = 8.3m;
			item.BY_CustomsSecondUnitQty = "ASV";

			var package1 = item.Packages.AddNew();
			package1.B5_UnitType = "BG";
			package1.B5_UnitCount = 123;
			package1.B5_MarksAndNumbers = "M-1234";
			var package2 = item.Packages.AddNew();
			package2.B5_UnitType = "BX";
			package2.B5_UnitCount = 234;
			package2.B5_MarksAndNumbers = "M-2345";

			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
			var goodsItem = commonGoodsItemsIntegrator.ConvertToCommonGoodsItem(item);

			CombineAssertions("Properties of the CommonGoodsItem should be copied from the NctsCommonCargoDesc object", () =>
			{
				AssertEquals("GoodsDescription", "Goods Description", goodsItem.GoodsDescription);
				AssertEquals("GrossMass", 1.5m, goodsItem.GrossMass);
				AssertEquals("NetMass", 2.9m, goodsItem.NetMass);
				AssertEquals("GrossMassUnit", Core.Constants.Weight.Kilograms, goodsItem.GrossMassUnit);
				AssertEquals("NetMassUnit", Core.Constants.Weight.Kilograms, goodsItem.NetMassUnit);
				AssertEquals("CommodityCode", "1202.30.00 00", goodsItem.CommodityCode);
				AssertEquals("DispatchCountry", Core.Constants.CountryCodes.Portugal, goodsItem.DispatchCountry);
				AssertEquals("DestinationCountry", Core.Constants.CountryCodes.UnitedKingdom, goodsItem.DestinationCountry);
				AssertEquals("Value", 100.00m, goodsItem.Value);
				AssertEquals("JobReference", "123ABC", goodsItem.JobReference);
				AssertEquals("EntryNumber", ((ZString)"Z", (ZString)"N830", (ZString)"MRN123", (ZInt?)null), goodsItem.EntryNumber);

				var packages = goodsItem.Packages.ToArray();
				AssertEquals(2, packages.Length);

				AssertEquals("HB:HB-1234", packages[0].BillOrReferenceNumber);
				AssertEquals("BG", packages[0].PackageType);
				AssertEquals(123, packages[0].PackageCount);
				AssertEquals("M-1234", packages[0].MarksAndNumbers);

				AssertEquals("HB:HB-1234", packages[1].BillOrReferenceNumber);
				AssertEquals("BX", packages[1].PackageType);
				AssertEquals(234, packages[1].PackageCount);
				AssertEquals("M-2345", packages[1].MarksAndNumbers);
			});
		}

		public void TestCopyCommonGoodsItems_Phase4()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);

			var goodsItems = new CommonGoodsItem[]
			{
				new CommonGoodsItem(),
				new CommonGoodsItem()
			};
			commonGoodsItemsIntegrator.CopyCommonGoodsItems(goodsItems, 0);
			AssertEquals(2, nctsHeader.MovementHeader.GoodsItems.Count);
		}

		public void TestCopyCommonGoodsItems_Phase5()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Bills.AddNew();
			var targetBill = nctsHeader.Bills.AddNew();
			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);

			var goodsItems = new CommonGoodsItem[]
			{
				new CommonGoodsItem(),
				new CommonGoodsItem()
			};
			commonGoodsItemsIntegrator.CopyCommonGoodsItems(goodsItems, 1);
			AssertEquals(2, targetBill.GoodsItems.Count);
		}

		public void TestCopyCommonGoodsItems_WhenActiveCopyFromPreviousLine() => CombineAssertions(() =>
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var targetBill = nctsHeader.Bills.AddNew();
				var goodItem = targetBill.GoodsItems;
				var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);

				var goodsItems = new CommonGoodsItem[]
				{
					SetUpGoodsItem(),
					SetUpGoodsItem()
				};
				commonGoodsItemsIntegrator.CopyCommonGoodsItems(goodsItems, 0);
				AssertEquals("Expected 2 packages in the first Good Item", 2, goodItem[0].Packages.Count);
				AssertEquals("Expected 2 packages in the second Good Item", 2, goodItem[1].Packages.Count);
			}
		});

		public void TestCopyFromCommonGoodsItem()
		{
			var goodsItem = SetUpGoodsItem();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
			var target = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			commonGoodsItemsIntegrator.CopyFromCommonGoodsItem(goodsItem, target);

			CombineAssertions("Value should have been copied to the goodsitem.", () =>
			{
				AssertEquals("GoodsDescription", "description", target.BY_Description);
				AssertEquals("GrossMass", 12000m, target.BY_GrossWeight);
				AssertEquals("NetMass", 10m, target.BY_NetWeight);
				AssertEquals("commodity code", "1202.30.00 01", target.BY_FormattedHarmonisedTariff);
				AssertEquals("GrossMassUnit", "G", target.BY_GrossWeightUnit);
				AssertEquals("NetMassUnit", "KG", target.BY_NetWeightUnit);
				AssertEquals("DestinationCountry", "US", target.BY_RN_NKCountryOfDestination);
				AssertEquals("DispatchCountry", "FR", target.BY_RN_NKCountryOfDispatch);
				AssertEquals("Value", 8m, target.BY_MonetaryValue);
				AssertEquals("SupplementaryQuantity", 1.1m, target.BY_CustomsSecondQuantity);
				AssertEquals("SupplementaryUnitQuantity", "NAR", target.BY_CustomsSecondUnitQty);
				AssertEquals(2, target.Packages.Count);

				var package1 = target.Packages[0];
				AssertEquals("BG", package1.B5_UnitType);
				AssertEquals(123, package1.B5_UnitCount);
				AssertEquals("M-1234", package1.B5_MarksAndNumbers);

				var package2 = target.Packages[1];
				AssertEquals("BX", package2.B5_UnitType);
				AssertEquals(234, package2.B5_UnitCount);
				AssertEquals("M-2345", package2.B5_MarksAndNumbers);
			});
		}

		public void TestTheOtherCollectionToAttach()
		{
			var header = Factory.New<NctsHeader>();
			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(header);
			AssertType<CusEntryHeadersToAttachCollection>(commonGoodsItemsIntegrator.TheOtherCollectionToAttach());
		}

		public void TestCopyEntryNumberFromCommonGoodsItem_WhenPhase5AndTransitionPeriodIsOn()
		{
			var commonGoodsItem = new CommonGoodsItem()
			{
				EntryNumber = (PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.Other, "Ent123", 1),
			};

			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var bill = nctsHeader.Bills.AddNew();
				var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
				var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				commonGoodsItemsIntegrator.CopyFromCommonGoodsItem(commonGoodsItem, goodsItem);

				AssertEntryNumberHasBeenImportedInPreviousDocument(goodsItem.PreviousDocuments, "Ent123", "ZZZ", "Z", 1);
				AssertEquals("House Consignment Previous Documents count", 0, bill.PreviousDocuments.Count);
			}
		}

		public void TestCopyEntryNumberFromCommonGoodsItem_WhenPhase5AndTransitionPeriodIsOff()
		{
			var commonGoodsItem = new CommonGoodsItem()
			{
				EntryNumber = (PreviousDocumentClassList.Codes.SummaryDeclaration, SupportingDocumentTypes.TirCarnet, "Ent123", 1),
			};

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				commonGoodsItemsIntegrator.CopyFromCommonGoodsItem(commonGoodsItem, goodsItem);

				AssertEntryNumberHasBeenImportedInPreviousDocument(bill.PreviousDocuments, "Ent123", "952", "X", 0);
				AssertEquals("Goods Item Previous Documents count", 0, goodsItem.PreviousDocuments.Count);
			}
		}

		void AssertEntryNumberHasBeenImportedInPreviousDocument(IEnumerable<PreviousDocument> previousDocumentCollection, string entryNumber, string expectedCode, string expectedSubType, int expectedItemNumber)
		{
			AssertEquals("Previous Document count", 1, previousDocumentCollection.Count());
			var entryNumberDoc = previousDocumentCollection.FirstOrDefault(x => x.CSI_ReferenceNumber == entryNumber);
			AssertNotNull($"PreviousDocuments with reference '{entryNumber}'", entryNumberDoc);

			CombineAssertions("Only 1 Previous document should have been created.", () =>
			{
				AssertEquals("Previous Document CSI_Code", expectedCode, entryNumberDoc.CSI_Code);
				AssertEquals("Previous Document CSI_SubType", expectedSubType, entryNumberDoc.CSI_SubType);
				AssertEquals("Previous Document CSI_ReferenceNumber", entryNumber, entryNumberDoc.CSI_ReferenceNumber);
				AssertEquals("Previous Document CSI_ItemNumber", expectedItemNumber, entryNumberDoc.CSI_ItemNumber);
			});
		}

		IDisposable TemporarilySetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

		CommonGoodsItem SetUpGoodsItem()
		{
			return new CommonGoodsItem()
			{
				GoodsDescription = "description",
				GrossMass = 12000m,
				NetMass = 10m,
				GrossMassUnit = "G",
				NetMassUnit = "KG",
				CommodityCode = "1202.30.00 01",
				Value = 8m,
				JobReference = "jobreference",
				DispatchCountry = "FR",
				DestinationCountry = "US",
				SupplementaryQuantity = 1.1m,
				SupplementaryQuantityUnit = "NAR",
				Packages = new[]
				{
					new CommonPackage
					{
						BillOrReferenceNumber = "HB-1234",
						PackageType = "BG",
						PackageCount = 123,
						MarksAndNumbers = "M-1234"
					},
					new CommonPackage
					{
						BillOrReferenceNumber = "HB-1234",
						PackageType = "BX",
						PackageCount = 234,
						MarksAndNumbers = "M-2345"
					}
				}
			};
		}
	}
}
