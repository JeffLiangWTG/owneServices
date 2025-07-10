using System.Linq;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC044CConsignmentItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CConsignmentItemWrapper>
	{
		public void TestGoodsItemNumber()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			bill.MovementDetail.B9_SeqNo = "99";
			var item = bill.ArrivalGoodsItems.AddNew();
			var package1 = item.Packages.AddNew();
			package1.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;

			var wrapper = CC044CConsignmentItemWrapper.New(item);
			AssertEquals("GoodsItemNumber should return HouseConsignement item sequence number.", "99", wrapper.GoodsItemNumber);
		}

		public void TestCommodity() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var bill = nctsHeader.Bills.AddNew();
			var item = bill.ArrivalGoodsItems.AddNew();
			item.BY_NetWeight = 10;
			item.BY_NetWeightUnit = "KG";
			item.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;

			var wrapper =  CC044CConsignmentItemWrapper.New(item);
			AssertEquals("Commodity should looked at Item when unloadedState is NEW", 10m, wrapper.Commodity.GoodsMeasure.NetMass.Value);

			item.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			unloadedItem.BY_NetWeight = 20;
			unloadedItem.BY_NetWeightUnit = "KG";
			item.BY_BY_Commodity = unloadedItem.PK;
			wrapper = CC044CConsignmentItemWrapper.New(item);
			AssertEquals("Commidity should looked at unloaded Item when unloadedState is DIF", 20m, wrapper.Commodity.GoodsMeasure.NetMass.Value);

			item.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			wrapper = CC044CConsignmentItemWrapper.New(item);
			AssertNull("Commidity should be null when unloadedState is MIS", wrapper.Commodity);
		});

		public void TestPackaging()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var bill = nctsHeader.Bills.AddNew();
			var item = bill.ArrivalGoodsItems.AddNew();

			var package = item.Packages.AddNew();
			package.B5_TypeOfDifference = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var wrapper = CC044CConsignmentItemWrapper.New(item);
			AssertEquals("Package with NEW state should be mapped.", 1, wrapper.Packaging.Count);

			package.B5_TypeOfDifference = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			wrapper = CC044CConsignmentItemWrapper.New(item);
			AssertEquals("Package with DEC state should not be mapped.", 0, wrapper.Packaging.Count);

			package.B5_TypeOfDifference = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
			wrapper = CC044CConsignmentItemWrapper.New(item);
			AssertEquals("Package with DIF state should be mapped.", 1, wrapper.Packaging.Count);
		}

		public void TestSupportingDocument()
		{
			AssertContainsExactElementsInAnyOrder("Only supporting documents with NEW and MIS state should be mapped using CC044CSupportingDocumentWrapper.", new string[] { "SUP1", "" }, Provider.SupportingDocument.Select(x => x.Type));
		}

		public void TestTransportDocument()
		{
			AssertContainsExactElementsInAnyOrder("Only transport documents with NEW and MIS state should be mapped using CC044CDocumentWrapper.", new string[] { "TRA1", "" }, Provider.TransportDocument.Select(x => x.Type));
		}

		public void TestAdditionalReference()
		{
			AssertContainsExactElementsInAnyOrder("Only additional references with NEW and MIS state should be mapped using CC044CDocumentWrapper.", new string[] { "REF1", "" }, Provider.AdditionalReference.Select(x => x.Type));
		}

		protected override CC044CConsignmentItemWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var bill = nctsHeader.Bills.AddNew();
			var item = bill.ArrivalGoodsItems.AddNew();
			var supportingDocument1 = item.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "SUP1";
			supportingDocument1.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var supportingDocument2 = item.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "SUP2";
			supportingDocument2.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var supportingDocument3 = item.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "SUP3";
			supportingDocument3.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;

			var transportDocument1 = item.AdditionalInfos.AddNew();
			transportDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument1.CSI_Code = "TRA1";
			transportDocument1.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var transportDocument2 = item.AdditionalInfos.AddNew();
			transportDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument2.CSI_Code = "TRA2";
			transportDocument2.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var transportDocument3 = item.AdditionalInfos.AddNew();
			transportDocument3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument3.CSI_Code = "TRA3";
			transportDocument3.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;

			var additionalReference1 = item.AdditionalInfos.AddNew();
			additionalReference1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference1.CSI_Code = "REF1";
			additionalReference1.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var additionalReference2 = item.AdditionalInfos.AddNew();
			additionalReference2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference2.CSI_Code = "REF2";
			additionalReference2.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var additionalReference3 = item.AdditionalInfos.AddNew();
			additionalReference3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference3.CSI_Code = "REF3";
			additionalReference3.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;

			var package1 = item.Packages.AddNew();
			package1.B5_UnitType = "CT";
			package1.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var package2 = item.Packages.AddNew();
			package2.B5_UnitType = "BX";
			package2.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			return CC044CConsignmentItemWrapper.New(item);
		}
	}
}
