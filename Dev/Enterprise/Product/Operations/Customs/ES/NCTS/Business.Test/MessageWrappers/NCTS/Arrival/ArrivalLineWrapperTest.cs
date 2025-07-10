using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class ArrivalLineWrapperTest : WrapperHelperTest<ArrivalLineWrapper>
	{
		public void TestGoodsCustomsProcedureCategory2()
		{
			goodsItem.BY_Type = "AH";
			AssertEquals("Expected filled GoodsCustomsProcedureCategory2", "AH", wrapper.GoodsCustomsProcedureCategory2);
		}

		public void TestGoodsCustomsProcedureCategory3()
		{
			CombineAssertions(() =>
			{
				goodsItem.HasDifferences = false;
				goodsItem.IsNew = false;
				goodsItem.IsMissing = false;
				AssertEquals("Expected empty GoodsCustomsProcedureCategory3 with false HasDifferences, IsNew and IsMissing", ZString.Empty, wrapper.GoodsCustomsProcedureCategory3);

				goodsItem.HasDifferences = true;
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with HasDifferences equals to true", "M" + 1, wrapper.GoodsCustomsProcedureCategory3);

				goodsItem.IsNew = true;
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with HasDifferences and IsNew equals to true", "MN" + 1, wrapper.GoodsCustomsProcedureCategory3);

				goodsItem.IsMissing = true; //This makes HasDifferences = false
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with IsNew and IsMissing equals to true", "NB" + 1, wrapper.GoodsCustomsProcedureCategory3);
			});
		}

		public void TestGoodsCustomsProcedureCategory4()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration = ZString.Empty;
				goodsItem.BillOfLadingItem = "HB12345678";
				AssertEquals("Expected empty GoodsCustomsProcedureCategory4 with empty PreviousSummary", ZString.Empty, wrapper.GoodsCustomsProcedureCategory4);

				nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration = "SUMMARY";
				AssertEquals("Expected filled GoodsCustomsProcedureCategory4 without separator", "HB12345678", wrapper.GoodsCustomsProcedureCategory4);

				goodsItem.BillOfLadingItem = "MB87654321/000239";
				AssertEquals("Expected filled GoodsCustomsProcedureCategory4 with separator", "MB87654321", wrapper.GoodsCustomsProcedureCategory4);
			});
		}

		public void TestGoodsCustomsProcedureCategory5()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration = ZString.Empty;
				goodsItem.BillOfLadingItem = "HB12345678";
				AssertEquals("Expected empty GoodsCustomsProcedureCategory5 with empty PreviousSummary", ZString.Empty, wrapper.GoodsCustomsProcedureCategory5);

				nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration = "SUMMARY";
				AssertEquals("Expected empty GoodsCustomsProcedureCategory5 without BillOfLadingItem separator", ZString.Empty, wrapper.GoodsCustomsProcedureCategory5);

				goodsItem.BillOfLadingItem = "MB87654321/000239";
				AssertEquals("Expected filled GoodsCustomsProcedureCategory5 with BillOfLadingItem separator", "000239", wrapper.GoodsCustomsProcedureCategory5);
			});
		}

		public void TestNotSubmittedC44Documents()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 0 NotSubmittedC44Documents with no SupportingDocuments", false, wrapper.NotSubmittedC44Documents.Any());

				AddDocument(goodsItem, 1);
				AddDocument(goodsItem, 2);
				AddDocument(goodsItem, 4);
				goodsItem.HasDifferences = true;

				var moveDetail2 = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();

				AddDocument(moveDetail2, 1);
				AddDocument(moveDetail2, 2);
				AddDocument(moveDetail2, 3);
				AddDocument(moveDetail2, 5);

				wrapper = new ArrivalLineWrapper(goodsItem);
				AssertEquals("Expected filled NotSubmittedC44Documents", 2, wrapper.NotSubmittedC44Documents.Count);
				AssertContainsExactElementsInAnyOrder("Expected filled NotSubmittedC44Documents", new ZString[] { "3", "5" }, wrapper.NotSubmittedC44Documents);

				goodsItem.IsNew = true;
				goodsItem.HasDifferences = false;
				wrapper = new ArrivalLineWrapper(goodsItem);
				AssertEquals("Expected no NotSubmittedC44Documents if OBS is not M", 0, wrapper.NotSubmittedC44Documents.Count);
			});

			void AddDocument(NctsArrivalAndUnloadingCargoDesc line, ZShort number)
			{
				var doc = line.SupportingDocuments.AddNew();
				doc.CSI_LineNo = number;
				doc.CSI_ReferenceNumber = string.Format("DOC{0}", number.ToString());
			}
		}

		public void TestGrossWeightInKG()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItem.BY_GrossWeight = 1.1;
				AssertEquals("Expected filled GrossWeightInKG with weight more than 1 rounded to the upper integer unit", new ZDecimal(2), wrapper.GrossWeightInKG);

				goodsItem.BY_GrossWeight = 0.01;
				AssertEquals("Expected filled GrossWeightInKG with weight less than 1", new ZDecimal(1), wrapper.GrossWeightInKG);

				goodsItem.BY_GrossWeight = 100.01;
				AssertEquals("Expected filled GrossWeightInKG round to the upper integer unit", new ZDecimal(101), wrapper.GrossWeightInKG);

				goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.ShortTons;
				goodsItem.BY_GrossWeight = 9;
				AssertEquals("Expected filled GrossWeightInKG with conversion from ShortTons to KG", new ZDecimal(8165), wrapper.GrossWeightInKG);
			});
		}

		public void TestNetWeightInKG()
		{
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.ShortTons;
			goodsItem.BY_NetWeight = 9;
			AssertEquals("Expected filled NetWeightInKG", new ZDecimal(8164.664963), wrapper.NetWeightInKG);
		}

		public void TestTotalGoodValueInEuros()
		{
			goodsItem.BY_MonetaryValue = new ZDecimal(23.23);
			AssertEquals("Expected filled TotalGoodValueInEuros", new ZDecimal(23.23), wrapper.TotalGoodValueInEuros);
		}

		public void TestDocuments()
		{
			goodsItem.IsNew = true;
			goodsItem.SupportingDocuments.AddNew();
			goodsItem.SupportingDocuments.AddNew();
			var documents = wrapper.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Documents", 2, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);

				goodsItem.IsNew = false;
				goodsItem.IsMissing = true;
				wrapper = new ArrivalLineWrapper(goodsItem);
				documents = wrapper.Documents;
				AssertEquals("Expected filled Documents with OBS type different to New", 0, documents.Count);
			});
		}

		public void TestGetExternalPackagesInfoCommonWrapper()
		{
			foreach (string tag in ContainerTags)
			{
				AddContainerForTest(tag);
			}
			var externalPackages = wrapper.ExternalPackages;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled ExternalPackages", ContainerTags.Length, externalPackages.NumberOfPackages);
				AssertSame("Cached ExternalPackages", wrapper.ExternalPackages, externalPackages);
			});

			void AddContainerForTest(ZString containerTag)
			{
				var container = goodsItem.Containers.AddNew();
				container.ContainerNumber = containerTag;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			wrapper = new ArrivalLineWrapper(goodsItem);
		}

		NctsHeader nctsHeader;
		NctsArrivalAndUnloadingCargoDesc goodsItem;
		ArrivalLineWrapper wrapper;

		protected override ArrivalLineWrapper GetProvider() => wrapper;
	}
}
