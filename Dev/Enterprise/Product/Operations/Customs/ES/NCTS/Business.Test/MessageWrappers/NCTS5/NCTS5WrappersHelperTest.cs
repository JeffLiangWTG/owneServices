using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public sealed class NCTS5WrappersHelperTest : TestCaseWithFactory
	{
		public void TestIsUnloadingStateNEWorMISorDIF()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateNEWorMISorDIF is true when given string is NEW", true, NCTS5WrappersHelper.IsUnloadingStateNEWorMISorDIF("NEW"));
				AssertEquals("IsUnloadingStateNEWorMISorDIF is false when given string is AAA (not NEW, MIS or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateNEWorMISorDIF("AAA"));
				AssertEquals("IsUnloadingStateNEWorMISorDIF is true when given string is MIS", true, NCTS5WrappersHelper.IsUnloadingStateNEWorMISorDIF("MIS"));
				AssertEquals("IsUnloadingStateNEWorMISorDIF is false when given string is DEC (not NEW, MIS or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateNEWorMISorDIF("DEC"));
				AssertEquals("IsUnloadingStateNEWorMISorDIF is true when given string is DIF", true, NCTS5WrappersHelper.IsUnloadingStateNEWorMISorDIF("DIF"));
			});
		}

		public void TestIsUnloadingStateNEWorDIF()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateNEWorDIF is false when given string is MIS (not NEW or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateNEWorDIF("MIS"));
				AssertEquals("IsUnloadingStateNEWorDIF is true when given string is NEW", true, NCTS5WrappersHelper.IsUnloadingStateNEWorDIF("NEW"));
				AssertEquals("IsUnloadingStateNEWorDIF is false when given string is DEC (not NEW or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateNEWorDIF("DEC"));
				AssertEquals("IsUnloadingStateNEWorDIF is true when given string is DIF", true, NCTS5WrappersHelper.IsUnloadingStateNEWorDIF("DIF"));
				AssertEquals("IsUnloadingStateNEWorDIF is false when given string is AAA (not NEW or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateNEWorDIF("AAA"));
			});
		}

		public void TestIsUnloadingStateMISorDIF()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateMISorDIF is false when given string is NEW (not MIS or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateMISorDIF("NEW"));
				AssertEquals("IsUnloadingStateMISorDIF is true when given string is MIS", true, NCTS5WrappersHelper.IsUnloadingStateMISorDIF("MIS"));
				AssertEquals("IsUnloadingStateMISorDIF is false when given string is DEC (not MIS or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateMISorDIF("DEC"));
				AssertEquals("IsUnloadingStateMISorDIF is true when given string is DIF", true, NCTS5WrappersHelper.IsUnloadingStateMISorDIF("DIF"));
				AssertEquals("IsUnloadingStateMISorDIF is false when given string is AAA (not MIS or DIF)", false, NCTS5WrappersHelper.IsUnloadingStateMISorDIF("AAA"));
			});
		}

		public void TestIsUnloadingStateMISorNEW()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateMISorNEW is false when given string is DIF (not MIS or NEW)", false, NCTS5WrappersHelper.IsUnloadingStateMISorNEW("DIF"));
				AssertEquals("IsUnloadingStateMISorNEW is true when given string is MIS", true, NCTS5WrappersHelper.IsUnloadingStateMISorNEW("MIS"));
				AssertEquals("IsUnloadingStateMISorNEW is false when given string is DEC (not MIS or NEW)", false, NCTS5WrappersHelper.IsUnloadingStateMISorNEW("DEC"));
				AssertEquals("IsUnloadingStateMISorNEW is true when given string is NEW", true, NCTS5WrappersHelper.IsUnloadingStateMISorNEW("NEW"));
				AssertEquals("IsUnloadingStateMISorNEW is false when given string is AAA (not MIS or NEW)", false, NCTS5WrappersHelper.IsUnloadingStateMISorNEW("AAA"));
			});
		}

		public void TestIsUnloadingStateNEW()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateNEW is false when given string is MIS (not NEW)", false, NCTS5WrappersHelper.IsUnloadingStateNEW("MIS"));
				AssertEquals("IsUnloadingStateNEW is false when given string is DEC (not NEW)", false, NCTS5WrappersHelper.IsUnloadingStateNEW("DEC"));
				AssertEquals("IsUnloadingStateNEW is true when given string is NEW", true, NCTS5WrappersHelper.IsUnloadingStateNEW("NEW"));
				AssertEquals("IsUnloadingStateNEW is false when given string is DIF (not NEW)", false, NCTS5WrappersHelper.IsUnloadingStateNEW("DIF"));
			});
		}

		public void TestIsUnloadingStateDIF()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateDIF is false when given string is MIS (not DIF)", false, NCTS5WrappersHelper.IsUnloadingStateDIF("MIS"));
				AssertEquals("IsUnloadingStateDIF is false when given string is DEC (not DIF)", false, NCTS5WrappersHelper.IsUnloadingStateDIF("DEC"));
				AssertEquals("IsUnloadingStateDIF is true when given string is DIF", true, NCTS5WrappersHelper.IsUnloadingStateDIF("DIF"));
				AssertEquals("IsUnloadingStateDIF is false when given string is NEW (not DIF)", false, NCTS5WrappersHelper.IsUnloadingStateDIF("NEW"));
			});
		}

		public void TestIsUnloadingStateMIS()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsUnloadingStateMIS is false when given string is NEW (not MIS)", false, NCTS5WrappersHelper.IsUnloadingStateMIS("NEW"));
				AssertEquals("IsUnloadingStateMIS is false when given string is DEC (not MIS)", false, NCTS5WrappersHelper.IsUnloadingStateMIS("DEC"));
				AssertEquals("IsUnloadingStateMIS is true when given string is MIS", true, NCTS5WrappersHelper.IsUnloadingStateMIS("MIS"));
				AssertEquals("IsUnloadingStateMIS is false when given string is DIF (not MIS)", false, NCTS5WrappersHelper.IsUnloadingStateMIS("DIF"));
			});
		}

		public void TestHasNctsBillDifferences()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var bill = nctsHeader.Bills.AddNew();
				bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState not DIF", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF but no Diferences", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				bill.B0_Weight = 1;
				var billGoodItem = bill.ArrivalGoodsItems.AddNew();
				billGoodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				billGoodItem.BY_GrossWeight = 2m;
				AssertEquals("HasNctsBillDifferences is true if UnloadedState is DIF and weight is different", true, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				billGoodItem.BY_GrossWeight = 1m;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF and weight is not different", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				var supDoc = bill.SupportingDocuments.AddNew();
				supDoc.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				AssertEquals("HasNctsBillDifferences is true if UnloadedState is DIF and there is Supporting Document with no DEC", true, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				supDoc.CSI_Status = SupportingDocumentStatusList.Codes.DEC;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF and there is no Supporting Document with no DEC", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				var additionalDoc = bill.AdditionalDocuments.AddNew();
				additionalDoc.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
				AssertEquals("HasNctsBillDifferences is true if UnloadedState is DIF and there is Additional Document with no DEC", true, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				additionalDoc.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF and there is no Additional Document with no DEC", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				var prevDoc = bill.PreviousDocuments.AddNew();
				prevDoc.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsBillDifferences is true if UnloadedState is DIF and there is Previous Document with no DEC", true, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				prevDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF and there is no Previous Document with no DEC", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				billGoodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsBillDifferences is true if UnloadedState is DIF and there is Good Item with no DEC", true, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				billGoodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF and there is no Good Item with no DEC", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				var transportInfo = bill.ArrivalTransportInfos.AddNew();
				transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsBillDifferences is true if UnloadedState is DIF and there is Transport Info with no DEC", true, NCTS5WrappersHelper.HasNctsBillDifferences(bill));

				transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("HasNctsBillDifferences is false if UnloadedState is DIF and there is no Transport Info with no DEC", false, NCTS5WrappersHelper.HasNctsBillDifferences(bill));
			});
		}

		public void TestHasNctsArrivalGoodItemDifferences()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				var bill = nctsHeader.Bills.AddNew();

				var goodItem = bill.ArrivalGoodsItems.AddNew();
				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState not DIF", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF but no Diferences", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.BY_Description = "DESC1";
				goodItem.UnloadedGoodsItem.BY_Description = "DESC2";
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and description is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_Description = "DESC1";
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and description is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.BY_CusC4Number = "CUS1";
				goodItem.UnloadedGoodsItem.BY_CusC4Number = "CUS2";
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and Cus Code is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_CusC4Number = "CUS1";
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and Cus Code is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.BY_GrossWeight = 5m;
				goodItem.UnloadedGoodsItem.BY_GrossWeight = 2m;
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and Gross Weight is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_GrossWeight = 5m;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and Gross Weight is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.BY_NetWeight = 5m;
				goodItem.UnloadedGoodsItem.BY_NetWeight = 2m;
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and Net Weight is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_NetWeight = 5m;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and Net Weight is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.BY_HarmonisedTariff = "1234512345";
				goodItem.UnloadedGoodsItem.BY_HarmonisedTariff = "12345121";
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and Tariff Code is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_HarmonisedTariff = "12345123";
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and Tariff Code is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				var supDoc = goodItem.SupportingDocuments.AddNew();
				supDoc.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and there is Supporting Document with no DEC", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				supDoc.CSI_Status = SupportingDocumentStatusList.Codes.DEC;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and there is no Supporting Document with no DEC", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				var additionalInfo = goodItem.AdditionalInfos.AddNew();
				additionalInfo.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and there is Additional Infos with no DEC", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				additionalInfo.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and there is no Additional Infos with no DEC", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				var package = goodItem.Packages.AddNew();
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsArrivalGoodItemDifferences is true if UnloadedState is DIF and there is Packages with no DEC", true, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));

				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("HasNctsArrivalGoodItemDifferences is false if UnloadedState is DIF and there is no Packages with no DEC", false, NCTS5WrappersHelper.HasNctsArrivalGoodItemDifferences(goodItem));
			});
		}

		public void TestHasNctsArrivalCommodityDifferences()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				var bill = nctsHeader.Bills.AddNew();

				var goodItem = bill.ArrivalGoodsItems.AddNew();
				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState not DIF", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState is DIF but no Diferences", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.BY_Description = "DESC1";
				goodItem.UnloadedGoodsItem.BY_Description = "DESC2";
				AssertEquals("HasNctsArrivalCommodityDifferences is true if UnloadedState is DIF and description is different", true, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_Description = "DESC1";
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState is DIF and description is not different", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.BY_CusC4Number = "CUS1";
				goodItem.UnloadedGoodsItem.BY_CusC4Number = "CUS2";
				AssertEquals("HasNctsArrivalCommodityDifferences is true if UnloadedState is DIF and Cus Code is different", true, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_CusC4Number = "CUS1";
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState is DIF and Cus Code is not different", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.BY_GrossWeight = 5m;
				goodItem.UnloadedGoodsItem.BY_GrossWeight = 2m;
				AssertEquals("HasNctsArrivalCommodityDifferences is true if UnloadedState is DIF and Gross Weight is different", true, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_GrossWeight = 5m;
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState is DIF and Gross Weight is not different", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.BY_NetWeight = 5m;
				goodItem.UnloadedGoodsItem.BY_NetWeight = 2m;
				AssertEquals("HasNctsArrivalCommodityDifferences is true if UnloadedState is DIF and Net Weight is different", true, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_NetWeight = 5m;
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState is DIF and Net Weight is not different", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.BY_HarmonisedTariff = "1234512345";
				goodItem.UnloadedGoodsItem.BY_HarmonisedTariff = "12345121";
				AssertEquals("HasNctsArrivalCommodityDifferences is true if UnloadedState is DIF and Tariff Code is different", true, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_HarmonisedTariff = "12345123";
				AssertEquals("HasNctsArrivalCommodityDifferences is false if UnloadedState is DIF and Tariff Code is not different", false, NCTS5WrappersHelper.HasNctsArrivalCommodityDifferences(goodItem));
			});
		}

		public void TestHasNctsArrivalGoodsMeasureDifferences()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				var bill = nctsHeader.Bills.AddNew();

				var goodItem = bill.ArrivalGoodsItems.AddNew();
				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("HasNctsArrivalGoodsMeasureDifferences is false if UnloadedState not DIF", false, NCTS5WrappersHelper.HasNctsArrivalGoodsMeasureDifferences(goodItem));

				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("HasNctsArrivalGoodsMeasureDifferences is false if UnloadedState is DIF but no Diferences", false, NCTS5WrappersHelper.HasNctsArrivalGoodsMeasureDifferences(goodItem));

				goodItem.BY_GrossWeight = 5m;
				goodItem.UnloadedGoodsItem.BY_GrossWeight = 2m;
				AssertEquals("HasNctsArrivalGoodsMeasureDifferences is true if UnloadedState is DIF and Gross Weight is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodsMeasureDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_GrossWeight = 5m;
				AssertEquals("HasNctsArrivalGoodsMeasureDifferences is false if UnloadedState is DIF and Gross Weight is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodsMeasureDifferences(goodItem));

				goodItem.BY_NetWeight = 5m;
				goodItem.UnloadedGoodsItem.BY_NetWeight = 2m;
				AssertEquals("HasNctsArrivalGoodsMeasureDifferences is true if UnloadedState is DIF and Net Weight is different", true, NCTS5WrappersHelper.HasNctsArrivalGoodsMeasureDifferences(goodItem));

				goodItem.UnloadedGoodsItem.BY_NetWeight = 5m;
				AssertEquals("HasNctsArrivalGoodsMeasureDifferences is false if UnloadedState is DIF and Net Weight is not different", false, NCTS5WrappersHelper.HasNctsArrivalGoodsMeasureDifferences(goodItem));
			});
		}

		public void TestDepartureTransportMeans()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, departureTransportMeans.Count);

				departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._1_SeaTransport, "11", ZString.Empty, ZString.Empty, "Vessel", "ES", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled DepartureTransportMeans with 1 element when inland mode is not 3", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "11", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT1()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._1_SeaTransport, "10", ZString.Empty, ZString.Empty, "Vessel", "ES", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "10", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT2()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._2_RailTransport, "20", "wagon", "GB", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "20", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "wagon", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "GB", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT3()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._3_RoadTransport, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "trailer2", "ES");
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "31", departureTransportMeansElement.TransportMode);
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportId", "trailer2", departureTransportMeansElement.TransportId);
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);

				departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._3_RoadTransport, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "trailer1", "FR", "trailer2", "ES");
				AssertEquals("Expected filled DepartureTransportMeans with 2 elements", 2, departureTransportMeans.Count);

				var departureTransportMeansArray = departureTransportMeans.ToArray();
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansArray[0].SequenceNumber);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "31", departureTransportMeansArray[0].TransportMode);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled with trailer 1 ID DepartureTransportMeans[0].TransportId", "trailer1", departureTransportMeansArray[0].TransportId);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "FR", departureTransportMeansArray[0].TransportNationality);

				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].SequenceNumber", "2", departureTransportMeansArray[1].SequenceNumber);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportMode", "31", departureTransportMeansArray[1].TransportMode);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled with trailer 2 ID DepartureTransportMeans[1].TransportId", "trailer2", departureTransportMeansArray[1].TransportId);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportNationality", "ES", departureTransportMeansArray[1].TransportNationality);

				departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._3_RoadTransport, ZString.Empty, "transportID", "GB", ZString.Empty, ZString.Empty, "trailer1", "FR", "trailer2", "ES");
				AssertEquals("Expected filled DepartureTransportMeans with 3 elements", 3, departureTransportMeans.Count);

				departureTransportMeansArray = departureTransportMeans.ToArray();
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansArray[0].SequenceNumber);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "30", departureTransportMeansArray[0].TransportMode);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with transport ID DepartureTransportMeans[0].TransportId", "transportID", departureTransportMeansArray[0].TransportId);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "GB", departureTransportMeansArray[0].TransportNationality);

				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].SequenceNumber", "2", departureTransportMeansArray[1].SequenceNumber);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportMode", "31", departureTransportMeansArray[1].TransportMode);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with trailer 1 ID DepartureTransportMeans[1].TransportId", "trailer1", departureTransportMeansArray[1].TransportId);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportNationality", "FR", departureTransportMeansArray[1].TransportNationality);

				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].SequenceNumber", "3", departureTransportMeansArray[2].SequenceNumber);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].TransportMode", "31", departureTransportMeansArray[2].TransportMode);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with trailer 2 ID DepartureTransportMeans[2].TransportId", "trailer2", departureTransportMeansArray[2].TransportId);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].TransportNationality", "ES", departureTransportMeansArray[2].TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT4()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._4_AirTransport, "40", "aircraftID", "ES", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "40", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "aircraftID", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT5()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._5_PostalConsignment, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty DepartureTransportMeans when MOT is 5", 0, departureTransportMeans.Count);
			});
		}

		public void TestDepartureTransportMeans_MOT7()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._7_FixedTransportInstallations, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty DepartureTransportMeans when MOT is 7", 0, departureTransportMeans.Count);
			});
		}

		public void TestDepartureTransportMeans_MOT8()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._8_InlandWaterwayTransport, "80", "Vessel", "ES", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportMode", "80", departureTransportMeansElement.TransportMode);
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT9()
		{
			CombineAssertions(() =>
			{
				var departureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(ModeOfTransportList.Codes._9_OwnPropulsion, "81", "transportID", "ES", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "81", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "transportID", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}
	}
}
