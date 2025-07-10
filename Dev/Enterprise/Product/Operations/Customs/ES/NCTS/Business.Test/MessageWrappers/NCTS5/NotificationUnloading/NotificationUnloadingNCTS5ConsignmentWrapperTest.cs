using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NotificationUnloadingNCTS5ConsignmentWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5ConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestGrossMass()
		{
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			arrivalMovement.BM_GrossWeightUnloaded = 1.23m;
			AssertEquals(1.23m, wrapper.GrossMass);
		}

		public void TestGrossMassSpecified()
		{
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				arrivalMovement.BM_GrossWeightUnloaded = 1;
				arrivalMovement.BM_GrossWeight = 2;
				AssertEquals("Values are not equal", expected: true, wrapper.GrossMassSpecified);

				arrivalMovement.BM_GrossWeightUnloaded = 2;
				AssertEquals("Values are equal", expected: false, wrapper.GrossMassSpecified);
			});
		}

		public void TestSupportingDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);

				var supdoc1 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_ReferenceNumber = "reference1";
				supdoc1.CSI_ReferenceNumber2 = "additionalData1";
				supdoc1.CSI_LineNo = 2;
				supdoc1.CSI_Status = "NEW";

				var supdoc2 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "Y001";
				supdoc2.CSI_ReferenceNumber = "reference2";
				supdoc2.CSI_ReferenceNumber2 = "additionalData2";
				supdoc2.CSI_LineNo = 4;
				supdoc2.CSI_Status = "DIF";

				var supdoc3 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "A003";
				supdoc3.CSI_ReferenceNumber = "reference3";
				supdoc3.CSI_ReferenceNumber2 = "additionalData3";
				supdoc3.CSI_LineNo = 3;
				supdoc3.CSI_Status = "MIS";

				var supdoc4 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "5004";
				supdoc4.CSI_ReferenceNumber = "reference4";
				supdoc4.CSI_ReferenceNumber2 = "additionalData4";
				supdoc4.CSI_LineNo = 1;
				supdoc4.CSI_Status = "NEW";

				AssertEquals("Prereq: supdoc1.CSI_LineNo", (ZInt)2, supdoc1.CSI_LineNo);
				AssertEquals("Prereq: supdoc2.CSI_LineNo", (ZInt)4, supdoc2.CSI_LineNo);
				AssertEquals("Prereq: supdoc3.CSI_LineNo", (ZInt)3, supdoc3.CSI_LineNo);
				AssertEquals("Prereq: supdoc4.CSI_LineNo", (ZInt)1, supdoc4.CSI_LineNo);

				wrapper = GetWrapper(nctsHeader);
				var documents = wrapper.SupportingDocument;

				AssertEquals("Expected filled SupportingDocument (only arrivalMovement's docs)", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for SupportingDocument (Name, Number, ComplementaryInformation, SequenceNumber)",
															new (ZString, ZString, ZString, ZString)[] {
																("5004", "reference4", "additionalData4", "1"),
																("9001", "reference1", "additionalData1", "2"),
																(ZString.Empty, ZString.Empty, ZString.Empty, "3"),
																("Y001", "reference2", "additionalData2", "4")
															}, documents.Select(x => (x.Name, x.Number, x.ComplementaryInformation, x.SequenceNumber)).ToArray());
				AssertSame("Cached SupportingDocument", wrapper.SupportingDocument, documents);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportDocument list", 0, wrapper.TransportDocument.Count);

				var addInfo1 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "TRA";
				addInfo1.CSI_ReferenceNumber = "reference1";
				addInfo1.CSI_LineNo = 3;
				addInfo1.CSI_Status = "NEW";

				var addInfo1INF = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_ReferenceNumber = "reference1INF";
				addInfo1INF.CSI_LineNo = 1;
				addInfo1INF.CSI_Status = "NEW";

				var addInfo2 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "TRA";
				addInfo2.CSI_ReferenceNumber = "reference2";
				addInfo2.CSI_LineNo = 2;
				addInfo2.CSI_Status = "DIF";

				var addInfo2INF = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "INF";
				addInfo2INF.CSI_ReferenceNumber = "reference2INF";
				addInfo2INF.CSI_LineNo = 2;
				addInfo2INF.CSI_Status = "NEW";

				var addInfo3 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";
				addInfo3.CSI_ReferenceNumber = "reference3";
				addInfo3.CSI_LineNo = 1;
				addInfo3.CSI_Status = "MIS";

				var addInfo4 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "TRA";
				addInfo4.CSI_ReferenceNumber = "reference4";
				addInfo4.CSI_LineNo = 6;
				addInfo4.CSI_Status = "NEW";

				AssertEquals("Prereq: addInfo1.CSI_LineNo", (ZInt)3, addInfo1.CSI_LineNo);
				AssertEquals("Prereq: addInfo1INF.CSI_LineNo", (ZInt)1, addInfo1INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo2.CSI_LineNo", (ZInt)2, addInfo2.CSI_LineNo);
				AssertEquals("Prereq: addInfo2INF.CSI_LineNo", (ZInt)2, addInfo2INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo3.CSI_LineNo", (ZInt)1, addInfo3.CSI_LineNo);
				AssertEquals("Prereq: addInfo4.CSI_LineNo", (ZInt)6, addInfo4.CSI_LineNo);

				wrapper = GetWrapper(nctsHeader);
				var documents = wrapper.TransportDocument;

				AssertEquals("Expected filled TransportDocument (Included those that have subType TRA and are arrivalMovement docs)", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for TransportDocument (Name, Number, SequenceNumber)",
															new (ZString, ZString, ZString)[] {
																(ZString.Empty, ZString.Empty, "1"),
																("9002", "reference2", "2"),
																("9001", "reference1", "3"),
																("9004", "reference4", "6")
															}, documents.Select(x => (x.Name, x.Number, x.SequenceNumber)).ToArray());
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, documents);
			});
		}

		public void TestAdditionalReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);

				var addInfo1 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "REF";
				addInfo1.CSI_Status = "NEW";
				addInfo1.CSI_ReferenceNumber = "reference1";

				var addInfo1INF = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_Status = "NEW";
				addInfo1INF.CSI_ReferenceNumber = "reference1INF";

				var addInfo2 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "REF";
				addInfo2.CSI_Status = "DIF";
				addInfo2.CSI_ReferenceNumber = "reference2";

				var addInfo2INF = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "INF";
				addInfo2INF.CSI_Status = "NEW";
				addInfo2INF.CSI_ReferenceNumber = "reference2INF";

				var addInfo3 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "REF";
				addInfo3.CSI_Status = "MIS";
				addInfo3.CSI_ReferenceNumber = "reference3";

				var addInfo4 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "REF";
				addInfo4.CSI_Status = "NEW";
				addInfo4.CSI_ReferenceNumber = "reference4";

				addInfo1.CSI_LineNo = 3;
				addInfo1INF.CSI_LineNo = 1;
				addInfo2.CSI_LineNo = 2;
				addInfo2INF.CSI_LineNo = 2;
				addInfo3.CSI_LineNo = 1;
				addInfo4.CSI_LineNo = 6;

				AssertEquals("Prereq: addInfo1.CSI_LineNo", (ZInt)3, addInfo1.CSI_LineNo);
				AssertEquals("Prereq: addInfo1INF.CSI_LineNo", (ZInt)1, addInfo1INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo2.CSI_LineNo", (ZInt)2, addInfo2.CSI_LineNo);
				AssertEquals("Prereq: addInfo2INF.CSI_LineNo", (ZInt)2, addInfo2INF.CSI_LineNo);
				AssertEquals("Prereq: addInfo3.CSI_LineNo", (ZInt)1, addInfo3.CSI_LineNo);
				AssertEquals("Prereq: addInfo4.CSI_LineNo", (ZInt)6, addInfo4.CSI_LineNo);

				wrapper = GetWrapper(nctsHeader);
				var documents = wrapper.AdditionalReference;

				AssertEquals("Expected filled AdditionalReference (Included those that have subType REF and are arrivalMovement docs)", 4, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected correct values for AdditionalReference (Name, Number, SequenceNumber)",
															new (ZString, ZString, ZString)[] {
																(ZString.Empty, ZString.Empty, "1"),
																("9002", "reference2", "2"),
																("9001", "reference1", "3"),
																("9004", "reference4", "6")
															}, documents.Select(x => (x.Name, x.Number, x.SequenceNumber)).ToArray());
				AssertSame("Cached AdditionalReference", wrapper.AdditionalReference, documents);
			});
		}

		public void TestHouseConsignment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty HouseConsignment when no data declared", 0, wrapper.HouseConsignment.Count);

				var bill1 = nctsHeader.Bills.AddNew();
				bill1.MovementDetail.B9_UnloadedState = "NEW";
				var bill2 = nctsHeader.Bills.AddNew();
				bill2.MovementDetail.B9_UnloadedState = "MIS";
				var bill3 = nctsHeader.Bills.AddNew();
				bill3.MovementDetail.B9_UnloadedState = "DIF";
				var bill4 = nctsHeader.Bills.AddNew();
				bill4.MovementDetail.B9_UnloadedState = "DEC";
				wrapper = GetWrapper(nctsHeader);
				var houseConsignment = wrapper.HouseConsignment;
				AssertEquals("Expected filled HouseConsignment only those with unloaded state MIS or NEW", 2, houseConsignment.Count);
				AssertSame("Cached HouseConsignment", wrapper.HouseConsignment, houseConsignment);
			});
		}

		public void TestHouseConsigmentDIF()
		{
			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] Expected empty HouseConsignment when no data declared", 0, wrapper.HouseConsignment.Count);

				var bill1 = GetHouseConsigmentDIF();

				var bill2 = GetHouseConsigmentDIF();
				bill2.B0_Weight = 1;
				var bill2GoodItem = bill2.ArrivalGoodsItems.AddNew();
				bill2GoodItem.BY_GrossWeight = 2;

				var bill3 = GetHouseConsigmentDIF();
				bill3.SupportingDocuments.AddNew().CSI_Status = SupportingDocumentStatusList.Codes.DEC;

				var bill4 = GetHouseConsigmentDIF();
				bill4.SupportingDocuments.AddNew().CSI_Status = SupportingDocumentStatusList.Codes.NEW;

				var bill5 = GetHouseConsigmentDIF();
				bill5.AdditionalDocuments.AddNew().CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;

				var bill6 = GetHouseConsigmentDIF();
				bill6.AdditionalDocuments.AddNew().CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;

				var bill7 = GetHouseConsigmentDIF();
				bill7.PreviousDocuments.AddNew().CSI_Status = NctsUnloadedStateList.Codes.DEC;

				var bill8 = GetHouseConsigmentDIF();
				bill8.PreviousDocuments.AddNew().CSI_Status = NctsUnloadedStateList.Codes.NEW;

				var bill9 = GetHouseConsigmentDIF();
				bill9.ArrivalGoodsItems.AddNew().BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

				var bill10 = GetHouseConsigmentDIF();
				bill10.ArrivalGoodsItems.AddNew().BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

				var bill11 = GetHouseConsigmentDIF();
				bill11.ArrivalTransportInfos.AddNew().TPM_TransportState = NctsUnloadedStateList.Codes.DEC;

				var bill12 = GetHouseConsigmentDIF();
				bill12.ArrivalTransportInfos.AddNew().TPM_TransportState = NctsUnloadedStateList.Codes.NEW;

				wrapper = GetWrapper(nctsHeader);
				var houseConsignment = wrapper.HouseConsignment;
				AssertEquals("Expected filled HouseConsignment only those with unloaded state DIF has changes", 6, houseConsignment.Count);
				AssertSame("Cached HouseConsignment", wrapper.HouseConsignment, houseConsignment);
			});

			NctsBill GetHouseConsigmentDIF()
			{
				var bill = nctsHeader.Bills.AddNew();
				bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				return bill;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = GetWrapper(nctsHeader);
		}

		NotificationUnloadingNCTS5ConsignmentWrapper wrapper;
		NctsHeader nctsHeader;

		NotificationUnloadingNCTS5ConsignmentWrapper GetWrapper(NctsHeader header) => new NotificationUnloadingNCTS5ConsignmentWrapper(header);

		protected override NotificationUnloadingNCTS5ConsignmentWrapper GetProvider() => wrapper;
	}
}
