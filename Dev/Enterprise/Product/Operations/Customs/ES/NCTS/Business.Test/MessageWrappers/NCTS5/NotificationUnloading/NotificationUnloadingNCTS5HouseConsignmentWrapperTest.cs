using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	sealed class NotificationUnloadingNCTS5HouseConsignmentWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5HouseConsignmentWrapper>
	{
		public void TestGrossMass()
		{
			nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			nctsBill.B0_GrossWeightUnloaded = 1.2345678;
			AssertEquals(1.234568m, wrapper.GrossMass);
		}

		public void TestGrossMassSpecified()
		{
			CombineAssertions(() =>
			{
				nctsBill.B0_Weight = 200;
				nctsBill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
				nctsBill.B0_GrossWeightUnloaded = 199;
				AssertEquals("Expected true: UnloadedState NEW; weights differ", expected: true, wrapper.GrossMassSpecified);

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected true: UnloadedState DIF; weights differ", expected: true, wrapper.GrossMassSpecified);

				nctsBill.MovementDetail.B9_UnloadedState = "MIS";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected false: UnloadedState != DIF, NEW; weights differ", expected: false, wrapper.GrossMassSpecified);

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				nctsBill.B0_GrossWeightUnloaded = 200;
				AssertEquals("Expected false: UnloadedState DIF; weights are equal", expected: false, wrapper.GrossMassSpecified);
			});
		}

		public void TestDepartureTransportMeans()
		{
			AssertEquals("Expected empty DepartureTransportMeans for now (provisional period)", 0, wrapper.DepartureTransportMeans.Count);
		}

		public void TestSupportingDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);

				var supdoc1 = nctsBill.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_ReferenceNumber = "reference1";
				supdoc1.CSI_ReferenceNumber2 = "additionalData1";
				supdoc1.CSI_LineNo = 2;
				supdoc1.CSI_Status = "NEW";

				var supdoc2 = nctsBill.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "Y001";
				supdoc2.CSI_ReferenceNumber = "reference2";
				supdoc2.CSI_ReferenceNumber2 = "additionalData2";
				supdoc2.CSI_LineNo = 4;
				supdoc2.CSI_Status = "DIF";

				var supdoc3 = nctsBill.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "A003";
				supdoc3.CSI_ReferenceNumber = "reference3";
				supdoc3.CSI_ReferenceNumber2 = "additionalData3";
				supdoc3.CSI_LineNo = 3;
				supdoc3.CSI_Status = "MIS";

				var supdoc4 = nctsBill.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "5004";
				supdoc4.CSI_ReferenceNumber = "reference4";
				supdoc4.CSI_ReferenceNumber2 = "additionalData4";
				supdoc4.CSI_LineNo = 1;
				supdoc4.CSI_Status = "NEW";

				var supdoc5 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "A005";
				supdoc5.CSI_ReferenceNumber = "reference5";
				supdoc5.CSI_ReferenceNumber2 = "additionalData5";
				supdoc5.CSI_LineNo = 5;
				supdoc5.CSI_Status = "NEW";

				AssertEquals("Prereq: supdoc1.CSI_LineNo", (ZInt)2, supdoc1.CSI_LineNo);
				AssertEquals("Prereq: supdoc2.CSI_LineNo", (ZInt)4, supdoc2.CSI_LineNo);
				AssertEquals("Prereq: supdoc3.CSI_LineNo", (ZInt)3, supdoc3.CSI_LineNo);
				AssertEquals("Prereq: supdoc4.CSI_LineNo", (ZInt)1, supdoc4.CSI_LineNo);
				AssertEquals("Prereq: supdoc5.CSI_LineNo", (ZInt)5, supdoc5.CSI_LineNo);

				wrapper = GetWrapper(nctsBill);
				var documents = wrapper.SupportingDocument;

				AssertEquals("Expected filled SupportingDocument (only bill docs) when bill's unloaded state is NEW", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for SupportingDocument (Name, Number, ComplementaryInformation, SequenceNumber)",
															new (ZString, ZString, ZString, ZString)[] {
																("5004", "reference4", "additionalData4", "1"),
																("9001", "reference1", "additionalData1", "2"),
																(ZString.Empty, ZString.Empty, ZString.Empty, "3"),
																("Y001", "reference2", "additionalData2", "4")
															}, documents.Select(x => (x.Name, x.Number, x.ComplementaryInformation, x.SequenceNumber)).ToArray());
				AssertSame("Cached SupportingDocument", wrapper.SupportingDocument, documents);

				nctsBill.MovementDetail.B9_UnloadedState = "MIS";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected empty SupportingDocument when bill's unloaded state is MIS", 0, wrapper.SupportingDocument.Count);

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected filled SupportingDocument when bill's unloaded state is DIF", 4, wrapper.SupportingDocument.Count);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportDocument list", 0, wrapper.TransportDocument.Count);

				var addInfo1 = nctsBill.AdditionalDocuments.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "TRA";
				addInfo1.CSI_ReferenceNumber = "reference1";
				addInfo1.CSI_LineNo = 3;
				addInfo1.CSI_Status = "NEW";

				var addInfo1INF = nctsBill.AdditionalDocuments.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_ReferenceNumber = "reference1INF";
				addInfo1INF.CSI_LineNo = 1;
				addInfo1INF.CSI_Status = "NEW";

				var addInfo2 = nctsBill.AdditionalDocuments.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "TRA";
				addInfo2.CSI_ReferenceNumber = "reference2";
				addInfo2.CSI_LineNo = 2;
				addInfo2.CSI_Status = "DIF";

				var addInfo2INF = nctsBill.AdditionalDocuments.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "INF";
				addInfo2INF.CSI_ReferenceNumber = "reference2INF";
				addInfo2INF.CSI_LineNo = 2;
				addInfo2INF.CSI_Status = "NEW";

				var addInfo3 = nctsBill.AdditionalDocuments.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";
				addInfo3.CSI_ReferenceNumber = "reference3";
				addInfo3.CSI_LineNo = 1;
				addInfo3.CSI_Status = "MIS";

				var addInfo4 = nctsBill.AdditionalDocuments.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "TRA";
				addInfo4.CSI_ReferenceNumber = "reference4";
				addInfo4.CSI_LineNo = 6;
				addInfo4.CSI_Status = "NEW";

				var addInfo5 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo5.CSI_Code = "A005";
				addInfo5.CSI_SubType = "TRA";
				addInfo5.CSI_ReferenceNumber = "reference5";
				addInfo5.CSI_LineNo = 7;
				addInfo5.CSI_Status = "NEW";

				AssertEquals("Prereq: addInfo1.CSI_LineNo", (ZInt)3, addInfo1.CSI_LineNo);
				AssertEquals("Prereq: addInfo1INF.CSI_LineNo", (ZInt)1, addInfo1INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo2.CSI_LineNo", (ZInt)2, addInfo2.CSI_LineNo);
				AssertEquals("Prereq: addInfo2INF.CSI_LineNo", (ZInt)2, addInfo2INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo3.CSI_LineNo", (ZInt)1, addInfo3.CSI_LineNo);
				AssertEquals("Prereq: addInfo4.CSI_LineNo", (ZInt)6, addInfo4.CSI_LineNo);
				AssertEquals("Prereq: addInfo5.CSI_LineNo", (ZInt)7, addInfo5.CSI_LineNo);

				wrapper = GetWrapper(nctsBill);
				var documents = wrapper.TransportDocument;

				AssertEquals("Expected filled TransportDocument (Included those that have subType TRA and are bill docs) when bill's unloaded state is NEW", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for TransportDocument (Name, Number, SequenceNumber)",
															new (ZString, ZString, ZString)[] {
																(ZString.Empty, ZString.Empty, "1"),
																("9002", "reference2", "2"),
																("9001", "reference1", "3"),
																("9004", "reference4", "6")
															}, documents.Select(x => (x.Name, x.Number, x.SequenceNumber)).ToArray());
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, documents);

				nctsBill.MovementDetail.B9_UnloadedState = "MIS";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected empty TransportDocument when bill's unloaded state is MIS", 0, wrapper.TransportDocument.Count);

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected filled TransportDocument when bill's unloaded state is DIF", 4, wrapper.TransportDocument.Count);
			});
		}

		public void TestAdditionalReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);

				var addInfo1 = nctsBill.AdditionalDocuments.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "REF";
				addInfo1.CSI_Status = "NEW";
				addInfo1.CSI_ReferenceNumber = "reference1";

				var addInfo1INF = nctsBill.AdditionalDocuments.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_Status = "NEW";
				addInfo1INF.CSI_ReferenceNumber = "reference1INF";

				var addInfo2 = nctsBill.AdditionalDocuments.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "REF";
				addInfo2.CSI_Status = "DIF";
				addInfo2.CSI_ReferenceNumber = "reference2";

				var addInfo2INF = nctsBill.AdditionalDocuments.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "INF";
				addInfo2INF.CSI_Status = "NEW";
				addInfo2INF.CSI_ReferenceNumber = "reference2INF";

				var addInfo3 = nctsBill.AdditionalDocuments.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "REF";
				addInfo3.CSI_Status = "MIS";
				addInfo3.CSI_ReferenceNumber = "reference3";

				var addInfo4 = nctsBill.AdditionalDocuments.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "REF";
				addInfo4.CSI_Status = "NEW";
				addInfo4.CSI_ReferenceNumber = "reference4";

				var addInfo5 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo5.CSI_Code = "A005";
				addInfo5.CSI_SubType = "REF";
				addInfo5.CSI_Status = "NEW";
				addInfo5.CSI_ReferenceNumber = "reference5";

				addInfo1.CSI_LineNo = 3;
				addInfo1INF.CSI_LineNo = 1;
				addInfo2.CSI_LineNo = 2;
				addInfo2INF.CSI_LineNo = 2;
				addInfo3.CSI_LineNo = 1;
				addInfo4.CSI_LineNo = 6;
				addInfo5.CSI_LineNo = 7;

				AssertEquals("Prereq: addInfo1.CSI_LineNo", (ZInt)3, addInfo1.CSI_LineNo);
				AssertEquals("Prereq: addInfo1INF.CSI_LineNo", (ZInt)1, addInfo1INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo2.CSI_LineNo", (ZInt)2, addInfo2.CSI_LineNo);
				AssertEquals("Prereq: addInfo2INF.CSI_LineNo", (ZInt)2, addInfo2INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo3.CSI_LineNo", (ZInt)1, addInfo3.CSI_LineNo);
				AssertEquals("Prereq: addInfo4.CSI_LineNo", (ZInt)6, addInfo4.CSI_LineNo);
				AssertEquals("Prereq: addInfo5.CSI_LineNo", (ZInt)7, addInfo5.CSI_LineNo);

				wrapper = GetWrapper(nctsBill);
				var documents = wrapper.AdditionalReference;

				AssertEquals("Expected filled AdditionalReference (Included those that have subType REF and are bill docs) when bill's unloaded state is NEW", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for AdditionalReference (Name, Number, SequenceNumber)",
															new (ZString, ZString, ZString)[] {
																(ZString.Empty, ZString.Empty, "1"),
																("9002", "reference2", "2"),
																("9001", "reference1", "3"),
																("9004", "reference4", "6")
															}, documents.Select(x => (x.Name, x.Number, x.SequenceNumber)).ToArray());
				AssertSame("Cached AdditionalReference", wrapper.AdditionalReference, documents);

				nctsBill.MovementDetail.B9_UnloadedState = "MIS";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected empty AdditionalReference when bill's unloaded state is MIS", 0, wrapper.AdditionalReference.Count);

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected filled AdditionalReference when bill's unloaded state is DIF", 4, wrapper.AdditionalReference.Count);
			});
		}

		public void TestConsignmentItem()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ConsignmentItem when no goodsItem declared, when bill's unloaded state is NEW", 0, wrapper.ConsignmentItem.Count);

				var goodsItem1 = nctsBill.ArrivalGoodsItems.AddNew();
				goodsItem1.BY_UnloadedState = "NEW";
				var goodsItem2 = nctsBill.ArrivalGoodsItems.AddNew();
				goodsItem2.BY_UnloadedState = "MIS";
				var goodsItem3 = nctsBill.ArrivalGoodsItems.AddNew();
				goodsItem3.BY_UnloadedState = "DIF";
				var goodsItem4 = nctsBill.ArrivalGoodsItems.AddNew();
				goodsItem4.BY_UnloadedState = "DEC";

				wrapper = GetWrapper(nctsBill);
				var consignmentItem = wrapper.ConsignmentItem;
				AssertEquals("Expected filled with 2 ConsignmentItem, only those with unloaded state MIS or NEW when bill's unloaded state is NEW", 2, consignmentItem.Count);
				AssertSame("Cached ConsignmentItem", wrapper.ConsignmentItem, consignmentItem);

				nctsBill.MovementDetail.B9_UnloadedState = "MIS";
				wrapper = GetWrapper(nctsBill);
				AssertNull("Expected null ConsignmentItem when bill's unloaded state is MIS", wrapper.ConsignmentItem);

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				goodsItem1.BY_UnloadedState = "NEW";
				goodsItem2.BY_UnloadedState = "MIS";
				goodsItem3.BY_UnloadedState = "DIF";
				goodsItem4.BY_UnloadedState = "DEC";

				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected filled with 2 ConsignmentItem, only those with unloaded state MIS or NEW, when bill's unloaded state is DIF", 2, wrapper.ConsignmentItem.Count);
			});
		}

		public void TestConsignmentItemDIF()
		{
			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] Expected empty HouseConsignment when no data declared", 0, wrapper.ConsignmentItem.Count);

				var goodItem1 = GetGoodItemDIF();

				var goodItem2 = GetGoodItemDIF();
				goodItem2.BY_Description = "DESC1";
				goodItem2.UnloadedGoodsItem.BY_Description = "DESC2";

				var goodItem3 = GetGoodItemDIF();
				goodItem3.BY_CusC4Number = "CUS1";
				goodItem3.UnloadedGoodsItem.BY_CusC4Number = "CUS2";

				var goodItem4 = GetGoodItemDIF();
				goodItem4.BY_GrossWeight = 1m;
				goodItem4.UnloadedGoodsItem.BY_GrossWeight = 2m;

				var goodItem5 = GetGoodItemDIF();
				goodItem5.BY_NetWeight = 1m;
				goodItem5.UnloadedGoodsItem.BY_NetWeight = 2m;

				var goodItem6 = GetGoodItemDIF();
				goodItem6.BY_HarmonisedTariff = "1234512345";
				goodItem6.UnloadedGoodsItem.BY_HarmonisedTariff = "12345121";

				var goodItem7 = GetGoodItemDIF();
				goodItem7.SupportingDocuments.AddNew().CSI_Status = SupportingDocumentStatusList.Codes.DEC;

				var goodItem8 = GetGoodItemDIF();
				goodItem8.SupportingDocuments.AddNew().CSI_Status = SupportingDocumentStatusList.Codes.NEW;

				var goodItem9 = GetGoodItemDIF();
				goodItem9.AdditionalInfos.AddNew().CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;

				var goodItem10 = GetGoodItemDIF();
				goodItem10.AdditionalInfos.AddNew().CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;

				var goodItem11 = GetGoodItemDIF();
				goodItem11.Packages.AddNew().B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

				var goodItem12 = GetGoodItemDIF();
				goodItem12.Packages.AddNew().B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

				wrapper = GetWrapper(nctsBill);
				var consignmentItem = wrapper.ConsignmentItem;
				AssertEquals("Expected filled with ConsignmentItem only those with unloaded state DIF has changes", 8, consignmentItem.Count);
				AssertSame("Cached ConsignmentItem", wrapper.ConsignmentItem, consignmentItem);
			});

			NctsArrivalCargoDesc GetGoodItemDIF()
			{
				var goodItem = nctsBill.ArrivalGoodsItems.AddNew();
				goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				return goodItem;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsBill = nctsHeader.Bills.AddNew();
			nctsBill.MovementDetail.B9_UnloadedState = "NEW";
			wrapper = GetWrapper(nctsBill);
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NotificationUnloadingNCTS5HouseConsignmentWrapper wrapper;

		NotificationUnloadingNCTS5HouseConsignmentWrapper GetWrapper(NctsBill bill) => new NotificationUnloadingNCTS5HouseConsignmentWrapper(bill);

		protected override NotificationUnloadingNCTS5HouseConsignmentWrapper GetProvider() => wrapper;
	}
}
