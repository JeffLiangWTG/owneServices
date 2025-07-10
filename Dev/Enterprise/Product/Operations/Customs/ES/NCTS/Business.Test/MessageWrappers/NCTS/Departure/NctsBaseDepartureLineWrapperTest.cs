using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsBaseDepartureLineWrapperTest : WrapperHelperTest<NctsDepartureBaseDepartureLineWrapper>
	{
		public void TestOtherUnitsNumber()
		{
			goodsItem.BY_CustomsThirdQuantity = GoodsItemDataNCTS.ThirdQuantity;
			AssertEquals("Expected filled OtherUnitsNumber", GoodsItemDataNCTS.ThirdQuantity, wrapper.OtherUnitsNumber);
		}

		public void TestOtherUnitsQualifier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				goodsItem.BY_CustomsThirdUnitQty = GoodsItemDataNCTS.SecondUnitQtyCW1;
				AssertEquals("Expected filled OtherUnitsQualifier with mapped value", GoodsItemDataNCTS.SecondUnitQtyCustoms, wrapper.OtherUnitsQualifier);

				goodsItem.BY_CustomsThirdUnitQty = GoodsItemDataNCTS.SecondUnitQtyNotMapped;
				AssertEquals("Expected filled OtherUnitsQualifier with original value because the value is not mapped", GoodsItemDataNCTS.SecondUnitQtyNotMapped, wrapper.OtherUnitsQualifier);
			});
		}

		public void TestDangerousGoodsCode()
		{
			goodsItem.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, GoodsItemDataNCTS.DangerousGoodsCode, "", "").First().PK;
			goodsItem.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code = GoodsItemDataNCTS.DangerousGoodsCode;
			AssertEquals("Expected filled DangerousGoodsCode", GoodsItemDataNCTS.DangerousGoodsCode, wrapper.DangerousGoodsCode);
		}

		public void TestDocumentTypeCode()
		{
			CombineAssertions(() =>
			{
				var prevDoc = goodsItem.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "OTH";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentTypeCode 1", "ACG", wrapper.DocumentTypeCode);

				prevDoc.CSI_Code = "DUA";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentTypeCode 2", "AAE", wrapper.DocumentTypeCode);

				prevDoc.CSI_Code = "SUM";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentTypeCode 3", "AEI", wrapper.DocumentTypeCode);

				prevDoc.CSI_ReferenceNumber = "docrefaaaaaaaaaaa";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentTypeCode 4", "AFB", wrapper.DocumentTypeCode);
			});
		}

		public void TestDocumentReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Document", ZString.Empty, wrapper.DocumentReferenceNumber);

				var prevDoc = goodsItem.PreviousDocuments.AddNew();

				prevDoc.CSI_Code = "SUM";
				prevDoc.CSI_SubType = "AAA";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentReferenceNumber without type", "docref", wrapper.DocumentReferenceNumber);

				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentReferenceNumber with type", "SUMdocref", wrapper.DocumentReferenceNumber);

				prevDoc.CSI_SubType = "AAA";
				prevDoc.CSI_LineNo = 2;
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentReferenceNumber without line number", "docref", wrapper.DocumentReferenceNumber);

				prevDoc.CSI_Code = "DUA";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentReferenceNumber with line number", "docref002", wrapper.DocumentReferenceNumber);

				prevDoc.CSI_LineNo = 0;
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentReferenceNumber without line number (becuase it's zero)", "docref", wrapper.DocumentReferenceNumber);

				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_LineNo = 2;
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentReferenceNumber with type and line number", "DUAdocref002", wrapper.DocumentReferenceNumber);
			});
		}

		public void TestDocumentLineNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Document", ZString.Empty, wrapper.DocumentLineNo);

				var prevDoc = goodsItem.PreviousDocuments.AddNew();

				prevDoc.CSI_LineNo = 2;
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected empty DocumentLineNo for documentType code empty", ZString.Empty, wrapper.DocumentLineNo);

				prevDoc.CSI_Code = "OTH";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected empty DocumentLineNo for documentType code ACG", ZString.Empty, wrapper.DocumentLineNo);

				prevDoc.CSI_Code = "DUA";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected empty DocumentLineNo for documentType code AAE", ZString.Empty, wrapper.DocumentLineNo);

				prevDoc.CSI_Code = "SUM";
				prevDoc.CSI_ReferenceNumber = "docref";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentLineNo for documentType code AEI", "2", wrapper.DocumentLineNo);

				prevDoc.CSI_LineNo = 0;
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected empty DocumentLineNofor documentType code AEI when LineNo is 0", ZString.Empty, wrapper.DocumentLineNo);

				prevDoc.CSI_ReferenceNumber = "docrefaaaaaaaaaaa";
				prevDoc.CSI_LineNo = 2;
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected empty DocumentLineNo for documentType code AFB", ZString.Empty, wrapper.DocumentLineNo);
			});
		}

		public void TestDocumentClass()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Document", ZString.Empty, wrapper.DocumentClass);

				var prevDoc = goodsItem.PreviousDocuments.AddNew();
				prevDoc.CSI_SubType = "AAA";
				wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
				AssertEquals("Expected filled DocumentClass", "AAA", wrapper.DocumentClass);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			wrapper = new NctsDepartureBaseDepartureLineWrapper(goodsItem);
		}
		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsDepartureBaseDepartureLineWrapper wrapper;

		protected override NctsDepartureBaseDepartureLineWrapper GetProvider() => wrapper;
	}
}
