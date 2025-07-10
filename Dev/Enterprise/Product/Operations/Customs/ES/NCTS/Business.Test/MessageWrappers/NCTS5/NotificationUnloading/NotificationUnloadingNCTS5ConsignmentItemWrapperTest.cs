using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NotificationUnloadingNCTS5ConsignmentItemWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5ConsignmentItemWrapper>
	{
		public void TestPackaging()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Packaging list", 0, wrapper.Packaging.Count);

				var package1 = goodsItem.Packages.AddNew();
				package1.B5_UnitType = "CT";

				var package2 = goodsItem.Packages.AddNew();
				package2.B5_UnitType = "FR";

				wrapper = GetWrapper(goodsItem);
				var packaging = wrapper.Packaging;

				AssertEquals("Expected filled Packaging when item status is NEW", 2, packaging.Count);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);

				goodsItem.BY_UnloadedState = "MIS";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected empty Packaging when item status is MIS", 0, wrapper.Packaging.Count);

				goodsItem.BY_UnloadedState = "DIF";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected filled Packaging when item status is DIF", 2, wrapper.Packaging.Count);
			});
		}

		public void TestCommodity()
		{
			var commodity = wrapper.Commodity;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Commodity when item status is NEW", commodity);
				AssertSame("Cached Commodity", wrapper.Commodity, commodity);

				goodsItem.BY_UnloadedState = "MIS";
				wrapper = GetWrapper(goodsItem);
				AssertNull("Expected empty Commodity when item status is MIS", wrapper.Commodity);

				goodsItem.BY_UnloadedState = "DIF";
				goodsItem.BY_HarmonisedTariff = "12345";
				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "12345";
				wrapper = GetWrapper(goodsItem);
				AssertNull("Expected null Commodity when item status is DIF but no Commodity differences", wrapper.Commodity);

				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "67890";
				wrapper = GetWrapper(goodsItem);
				AssertNotNull("Expected filled Commodity when item status is DIF and Commodity has differences", wrapper.Commodity);
			});
		}

		public void TestSupportingDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);

				var supdoc1 = goodsItem.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_ReferenceNumber = "reference1";
				supdoc1.CSI_ReferenceNumber2 = "additionalData1";
				supdoc1.CSI_LineNo = 2;
				supdoc1.CSI_Status = "NEW";

				var supdoc2 = goodsItem.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "Y001";
				supdoc2.CSI_ReferenceNumber = "reference2";
				supdoc2.CSI_ReferenceNumber2 = "additionalData2";
				supdoc2.CSI_LineNo = 4;
				supdoc2.CSI_Status = "DIF";

				var supdoc3 = goodsItem.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "A003";
				supdoc3.CSI_ReferenceNumber = "reference3";
				supdoc3.CSI_ReferenceNumber2 = "additionalData3";
				supdoc3.CSI_LineNo = 3;
				supdoc3.CSI_Status = "MIS";

				var supdoc4 = goodsItem.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "5004";
				supdoc4.CSI_ReferenceNumber = "reference4";
				supdoc4.CSI_ReferenceNumber2 = "additionalData4";
				supdoc4.CSI_LineNo = 1;
				supdoc4.CSI_Status = "NEW";

				var supdoc5 = nctsBill.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "A005";
				supdoc5.CSI_ReferenceNumber = "reference5";
				supdoc5.CSI_ReferenceNumber2 = "additionalData5";
				supdoc5.CSI_LineNo = 5;
				supdoc5.CSI_Status = "NEW";

				var supdoc6 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				supdoc6.CSI_Code = "5006";
				supdoc6.CSI_ReferenceNumber = "reference6";
				supdoc6.CSI_ReferenceNumber2 = "additionalData6";
				supdoc6.CSI_LineNo = 6;
				supdoc6.CSI_Status = "NEW";

				AssertEquals("Prereq: supdoc1.CSI_LineNo", (ZInt)2, supdoc1.CSI_LineNo);
				AssertEquals("Prereq: supdoc2.CSI_LineNo", (ZInt)4, supdoc2.CSI_LineNo);
				AssertEquals("Prereq: supdoc3.CSI_LineNo", (ZInt)3, supdoc3.CSI_LineNo);
				AssertEquals("Prereq: supdoc4.CSI_LineNo", (ZInt)1, supdoc4.CSI_LineNo);
				AssertEquals("Prereq: supdoc5.CSI_LineNo", (ZInt)5, supdoc5.CSI_LineNo);
				AssertEquals("Prereq: supdoc6.CSI_LineNo", (ZInt)6, supdoc6.CSI_LineNo);

				wrapper = GetWrapper(goodsItem);
				var documents = wrapper.SupportingDocument;

				AssertEquals("Expected filled SupportingDocument (only item docs) when item status is NEW", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for SupportingDocument (Name, Number, ComplementaryInformation, SequenceNumber)",
															new (ZString, ZString, ZString, ZString)[] {
																("5004", "reference4", "additionalData4", "1"),
																("9001", "reference1", "additionalData1", "2"),
																(ZString.Empty, ZString.Empty, ZString.Empty, "3"),
																("Y001", "reference2", "additionalData2", "4")
															}, documents.Select(x => (x.Name, x.Number, x.ComplementaryInformation, x.SequenceNumber)).ToArray());
				AssertSame("Cached SupportingDocument", wrapper.SupportingDocument, documents);

				goodsItem.BY_UnloadedState = "MIS";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected empty SupportingDocument when item status is MIS", 0, wrapper.SupportingDocument.Count);

				goodsItem.BY_UnloadedState = "DIF";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected filled SupportingDocument when item status is DIF", 4, wrapper.SupportingDocument.Count);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportDocument list", 0, wrapper.TransportDocument.Count);

				var addInfo1 = goodsItem.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "TRA";
				addInfo1.CSI_ReferenceNumber = "reference1";
				addInfo1.CSI_LineNo = 3;
				addInfo1.CSI_Status = "NEW";

				var addInfo1INF = goodsItem.AdditionalInfos.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_ReferenceNumber = "reference1INF";
				addInfo1INF.CSI_LineNo = 1;
				addInfo1INF.CSI_Status = "NEW";

				var addInfo2 = goodsItem.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "TRA";
				addInfo2.CSI_ReferenceNumber = "reference2";
				addInfo2.CSI_LineNo = 2;
				addInfo2.CSI_Status = "DIF";

				var addInfo2INF = goodsItem.AdditionalInfos.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "INF";
				addInfo2INF.CSI_ReferenceNumber = "reference2INF";
				addInfo2INF.CSI_LineNo = 2;
				addInfo2INF.CSI_Status = "NEW";

				var addInfo3 = goodsItem.AdditionalInfos.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";
				addInfo3.CSI_ReferenceNumber = "reference3";
				addInfo3.CSI_LineNo = 1;
				addInfo3.CSI_Status = "MIS";

				var addInfo4 = goodsItem.AdditionalInfos.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "TRA";
				addInfo4.CSI_ReferenceNumber = "reference4";
				addInfo4.CSI_LineNo = 6;
				addInfo4.CSI_Status = "NEW";

				var addInfo5 = nctsBill.AdditionalDocuments.AddNew();
				addInfo5.CSI_Code = "A005";
				addInfo5.CSI_SubType = "TRA";
				addInfo5.CSI_ReferenceNumber = "reference5";
				addInfo5.CSI_LineNo = 7;
				addInfo5.CSI_Status = "NEW";

				var addInfo6 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo6.CSI_Code = "5006";
				addInfo6.CSI_SubType = "TRA";
				addInfo6.CSI_ReferenceNumber = "reference6";
				addInfo6.CSI_LineNo = 8;
				addInfo6.CSI_Status = "NEW";

				AssertEquals("Prereq: addInfo1.CSI_LineNo", (ZInt)3, addInfo1.CSI_LineNo);
				AssertEquals("Prereq: addInfo1INF.CSI_LineNo", (ZInt)1, addInfo1INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo2.CSI_LineNo", (ZInt)2, addInfo2.CSI_LineNo);
				AssertEquals("Prereq: addInfo2INF.CSI_LineNo", (ZInt)2, addInfo2INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo3.CSI_LineNo", (ZInt)1, addInfo3.CSI_LineNo);
				AssertEquals("Prereq: addInfo4.CSI_LineNo", (ZInt)6, addInfo4.CSI_LineNo);
				AssertEquals("Prereq: addInfo5.CSI_LineNo", (ZInt)7, addInfo5.CSI_LineNo);
				AssertEquals("Prereq: addInfo6.CSI_LineNo", (ZInt)8, addInfo6.CSI_LineNo);

				wrapper = GetWrapper(goodsItem);
				var documents = wrapper.TransportDocument;

				AssertEquals("Expected filled TransportDocument (Included those that have subType TRA and are item docs) when item status is NEW", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for TransportDocument (Name, Number, SequenceNumber)",
															new (ZString, ZString, ZString)[] {
																(ZString.Empty, ZString.Empty, "1"),
																("9002", "reference2", "2"),
																("9001", "reference1", "3"),
																("9004", "reference4", "6")
															}, documents.Select(x => (x.Name, x.Number, x.SequenceNumber)).ToArray());
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, documents);

				goodsItem.BY_UnloadedState = "MIS";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected empty TransportDocument when item status is MIS", 0, wrapper.TransportDocument.Count);

				goodsItem.BY_UnloadedState = "DIF";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected filled TransportDocument when item status is DIF", 4, wrapper.TransportDocument.Count);
			});
		}

		public void TestAdditionalReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);

				var addInfo1 = goodsItem.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "REF";
				addInfo1.CSI_Status = "NEW";
				addInfo1.CSI_ReferenceNumber = "reference1";

				var addInfo1INF = goodsItem.AdditionalInfos.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_Status = "NEW";
				addInfo1INF.CSI_ReferenceNumber = "reference1INF";

				var addInfo2 = goodsItem.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "REF";
				addInfo2.CSI_Status = "DIF";
				addInfo2.CSI_ReferenceNumber = "reference2";

				var addInfo2INF = goodsItem.AdditionalInfos.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "INF";
				addInfo2INF.CSI_Status = "NEW";
				addInfo2INF.CSI_ReferenceNumber = "reference2INF";

				var addInfo3 = goodsItem.AdditionalInfos.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "REF";
				addInfo3.CSI_Status = "MIS";
				addInfo3.CSI_ReferenceNumber = "reference3";

				var addInfo4 = goodsItem.AdditionalInfos.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "REF";
				addInfo4.CSI_Status = "NEW";
				addInfo4.CSI_ReferenceNumber = "reference4";

				var addInfo5 = nctsBill.AdditionalDocuments.AddNew();
				addInfo5.CSI_Code = "A005";
				addInfo5.CSI_SubType = "REF";
				addInfo5.CSI_Status = "NEW";
				addInfo5.CSI_ReferenceNumber = "reference5";

				var addInfo6 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo6.CSI_Code = "5006";
				addInfo6.CSI_SubType = "REF";
				addInfo6.CSI_Status = "NEW";
				addInfo6.CSI_ReferenceNumber = "reference6";

				addInfo1.CSI_LineNo = 3;
				addInfo1INF.CSI_LineNo = 1;
				addInfo2.CSI_LineNo = 2;
				addInfo2INF.CSI_LineNo = 2;
				addInfo3.CSI_LineNo = 1;
				addInfo4.CSI_LineNo = 6;
				addInfo5.CSI_LineNo = 7;
				addInfo6.CSI_LineNo = 8;

				AssertEquals("Prereq: addInfo1.CSI_LineNo", (ZInt)3, addInfo1.CSI_LineNo);
				AssertEquals("Prereq: addInfo1INF.CSI_LineNo", (ZInt)1, addInfo1INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo2.CSI_LineNo", (ZInt)2, addInfo2.CSI_LineNo);
				AssertEquals("Prereq: addInfo2INF.CSI_LineNo", (ZInt)2, addInfo2INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo3.CSI_LineNo", (ZInt)1, addInfo3.CSI_LineNo);
				AssertEquals("Prereq: addInfo4.CSI_LineNo", (ZInt)6, addInfo4.CSI_LineNo);
				AssertEquals("Prereq: addInfo5.CSI_LineNo", (ZInt)7, addInfo5.CSI_LineNo);
				AssertEquals("Prereq: addInfo6.CSI_LineNo", (ZInt)8, addInfo6.CSI_LineNo);

				wrapper = GetWrapper(goodsItem);
				var documents = wrapper.AdditionalReference;

				AssertEquals("Expected filled AdditionalReference (Included those that have subType REF and are item docs) when item status is NEW", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for AdditionalReference (Name, Number, SequenceNumber)",
															new (ZString, ZString, ZString)[] {
																(ZString.Empty, ZString.Empty, "1"),
																("9002", "reference2", "2"),
																("9001", "reference1", "3"),
																("9004", "reference4", "6")
															}, documents.Select(x => (x.Name, x.Number, x.SequenceNumber)).ToArray());
				AssertSame("Cached AdditionalReference", wrapper.AdditionalReference, documents);

				goodsItem.BY_UnloadedState = "MIS";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected empty AdditionalReference when item status is MIS", 0, wrapper.AdditionalReference.Count);

				goodsItem.BY_UnloadedState = "DIF";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected filled AdditionalReference when item status is DIF", 4, wrapper.AdditionalReference.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = "NEW";
			wrapper = GetWrapper(goodsItem);
		}

		NctsArrivalCargoDesc goodsItem;
		NctsBill nctsBill;
		NctsHeader nctsHeader;
		NotificationUnloadingNCTS5ConsignmentItemWrapper wrapper;

		NotificationUnloadingNCTS5ConsignmentItemWrapper GetWrapper(NctsArrivalCargoDesc item) => new NotificationUnloadingNCTS5ConsignmentItemWrapper(item);

		protected override NotificationUnloadingNCTS5ConsignmentItemWrapper GetProvider() => wrapper;
	}
}
