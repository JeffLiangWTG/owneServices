using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousDocument))]
	sealed class NctsPreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<NctsPreviousDocument>
	{
		public void TestLookups_Phase4()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsPreviousDocumentPhase4Lookups>(previousDocument.Lookups);
		}

		public void TestLookups_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsPreviousDocumentPhase5Lookups>(previousDocument.Lookups);
		}

		public void TestValidation_Phase4()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsPreviousDocumentPhase4Validation>(previousDocument.Validation);
		}

		public void TestValidation_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsPreviousDocumentPhase5Validation>(previousDocument.Validation);
		}

		public void TestCSI_Code_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_CodeInfo, "Document Type", "Doc. Type", "Type");
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_ReferenceNumberInfo, "Reference Number", "Reference No.", "Reference");
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Phase 5 Departure in Transition Period", 35, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Phase 5 Departure outside Transition Period", 70, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
				}

				var arrivalPreviousDocument = CreatePhase5ArrivalDocument();
				AssertEquals("Phase 5 Arrival", 35, arrivalPreviousDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ItemNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_ItemNumberInfo, "Item Number", "Item No.", "Item");
		}

		public void TestCSI_Quantity2_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_Quantity2Info, "Number of Packages", "No. of Packages", "Pack Qty");
		}

		public void TestCSI_UnitOfQuantity2_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_UnitOfQuantity2Info, "Package Type", "Pack Type", "Pack Type");
		}

		public void TestCSI_Quantity_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_QuantityInfo, "Quantity", "Quantity", "Qty.");
		}

		public void TestCSI_UnitOfQuantity_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_UnitOfQuantityInfo, "Unit Of Quantity", "Unit Qty.", "UOM");
		}

		public void TestCSI_ReferenceNumber2_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_ReferenceNumber2Info, "Complement of Information", "Complement Info.", "Complement");
		}

		public void TestCSI_QuantityDecimalType()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(NctsPreviousDocument), nameof(NctsPreviousDocument.CSI_Quantity), true, x => x.DecimalPlaces == 3);
		}

		public void TestCSI_Quantity2DecimalType()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(NctsPreviousDocument), nameof(NctsPreviousDocument.CSI_Quantity2), true, x => x.DecimalPlaces == 3);
		}

		public void TestGoodsItem()
		{
			AssertEquals(previousDocument.GoodsItem.PK, nctsHeader.MovementHeader.GoodsItems[0].PK);
		}

		public void TestSequenceNumber_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				var previousDocument = goodsItem.PreviousDocuments.AddNew();
				AssertEquals("SequenceNumber of previousDocument is 0", 0, previousDocument.CSI_LineNo);

				var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
				AssertEquals("SequenceNumber of previousDocument2 is 0", 0, previousDocument2.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				var previousDocument = goodsItem.PreviousDocuments.AddNew();
				AssertEquals("SequenceNumber of previousDocument is 1", 1, previousDocument.CSI_LineNo);

				var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
				AssertEquals("SequenceNumber of previousDocument2 is 2", 2, previousDocument2.CSI_LineNo);

				previousDocument.Delete();
				AssertEquals("previousDocument is deleted, SequenceNumber of previousDocument2 is 1", 1, previousDocument2.CSI_LineNo);
			});
		}

		public void TestRefCusCode_Phase5_NoAttribute()
		{
			CreateRefCusCodeListForTest();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			previousDocument.CSI_Code = "ABC";
			AssertNull(previousDocument.RefCusCode);
		}

		public void TestRefCusCode_Phase5_HasAttribute()
		{
			(var refCusCodeList, var helper) = CreateRefCusCodeListForTest();

			helper.CreateCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			Factory.Save();

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				previousDocument.CSI_Code = "ABC";
				AssertEquals("Has Attribute", "ABC", previousDocument.RefCusCode.ZZD_Code);

				previousDocument.CSI_Code = "DEF";
				AssertNull("Recalculated", previousDocument.RefCusCode);
			});
		}

		public void TestIsPhase5Departure()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Phase5, Departure", false, previousDocument.IsPhase5Departure);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Is Phase5, Departure", true, previousDocument.IsPhase5Departure);

				var arrivalPreviousDocument = CreatePhase5ArrivalDocument();
				AssertEquals("Arrival", false, arrivalPreviousDocument.IsPhase5Departure);
			});
		}

		public void TestIsPhase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Phase5", false, previousDocument.IsPhase5);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Is Phase5", true, previousDocument.IsPhase5);
			});
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Previous Document", previousDocument.HumanReadableName);
		}

		public void TestIsNCTSPreviousDocument() => CombineAssertions(() =>
		{
			previousDocument.CSI_Code = ZString.Empty;
			AssertEquals($"CSI_Code={previousDocument.CSI_Code}", false, previousDocument.IsNCTSPreviousDocument);
			previousDocument.CSI_Code = "N123";
			AssertEquals($"CSI_Code={previousDocument.CSI_Code}", true, previousDocument.IsNCTSPreviousDocument);
			previousDocument.CSI_Code = "X123";
			AssertEquals($"CSI_Code={previousDocument.CSI_Code}", false, previousDocument.IsNCTSPreviousDocument);
		});

		public void TestValidationDeciderPhase4Departure()
		{
			AssertNull(previousDocument.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Departure()
		{
			var phase5DepartureDocument = CreatePhase5DepartureDocument();
			AssertType<NctsPreviousDocumentDeparturePhase5ValidationDecider>(phase5DepartureDocument.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Arrival()
		{
			var phase5ArrivalDocument = CreatePhase5ArrivalDocument();
			AssertType<NctsPreviousDocumentArrivalPhase5ValidationDecider>(phase5ArrivalDocument.ValidationDecider);
		}

		public void TestCSI_LineNo_RecalculatedWhenPreviousDocumentIsDeleted()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			var doc1 = goodsItem.PreviousDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Doc 1";
			doc1.CSI_ItemNumber = 1;

			var doc2 = goodsItem.PreviousDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Doc 2";
			doc2.CSI_ItemNumber = 2;

			var doc3 = goodsItem.PreviousDocuments.AddNew();
			doc3.CSI_ReferenceNumber = "Doc 3";
			doc3.CSI_ItemNumber = 3;

			AssertEquals("Doc 1 CSI_LineNo", 1, doc1.CSI_LineNo);
			AssertEquals("Doc 2 CSI_LineNo", 2, doc2.CSI_LineNo);
			AssertEquals("Doc 3 CSI_LineNo", 3, doc3.CSI_LineNo);

			goodsItem.PreviousDocuments.RemoveAndDelete(doc2);

			AssertEquals("Doc 1 CSI_LineNo, when Doc 2 is deleted", 1, doc1.CSI_LineNo);
			AssertEquals("Doc 3 CSI_LineNo, when Doc 2 is deleted", 2, doc3.CSI_LineNo);

			goodsItem.PreviousDocuments.RemoveAndDelete(doc1);

			AssertEquals("Doc 3 CSI_LineNo, when Doc 1 is deleted", 1, doc3.CSI_LineNo);
		}

		protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			yield return goodsItem.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => previousDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsPreviousDocument previousDocument;

		(RefCusCodeList, UniversalReferenceTestDataHelper) CreateRefCusCodeListForTest()
		{
			var nctsCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingEU = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var dataGroupingLV = helper.CreateNewOrGetExistingDataGrouping("LV", "Latvia", dataGroupingEU);
			helper.CreateNewOrGetExistingCusCodeType(nctsCodeType, "Ncts Previous Document Type");
			var refCusCodeList = helper.CreateCusCodeList(dataGroupingLV.ZZZ_DataGrouping, nctsCodeType, "ABC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			return (refCusCodeList, helper);
		}

		NctsPreviousDocument CreatePhase5ArrivalDocument()
		{
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return arrivalNctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().PreviousDocuments.AddNew();
		}

		NctsPreviousDocument CreatePhase5DepartureDocument()
		{
			var departureNctsHeader = Factory.New<NctsHeader>();
			departureNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return departureNctsHeader.Bills.AddNew().GoodsItems.AddNew().PreviousDocuments.AddNew();
		}
	}
}
