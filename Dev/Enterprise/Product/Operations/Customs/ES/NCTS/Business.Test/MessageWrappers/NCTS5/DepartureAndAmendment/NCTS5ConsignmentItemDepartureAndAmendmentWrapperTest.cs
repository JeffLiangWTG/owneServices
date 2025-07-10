using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5ConsignmentItemDepartureAndAmendmentWrapperTest : WrapperHelperTest<NCTS5ConsignmentItemDepartureAndAmendmentWrapper>
	{
		public void TestCountryOfDispatch()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_RN_NKCountryOfDispatch = "FR";
				nctsBill.B0_RN_NKCountryOfExport = "ES";
				goodsItem.BY_RN_NKCountryOfDispatch = "DE";
				AssertEquals("Expected empty CountryOfDispatch when shouldDeclareCountryOfDispatchInItem flag is false", ZString.Empty, wrapper.CountryOfDispatch);

				wrapper = GetWrapper(goodsItem, shouldDeclareCountryOfDispatchInItem: true);
				AssertEquals("Expected filled CountryOfDispatch when shouldDeclareCountryOfDispatchInItem flag is true with value in goodsitem when not empty", "DE", wrapper.CountryOfDispatch);

				goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
				AssertEquals("Expected filled CountryOfDispatch when shouldDeclareCountryOfDispatchInItem flag is true with value in bill when value in gooditem is empty", "ES", wrapper.CountryOfDispatch);

				nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
				AssertEquals("Expected filled CountryOfDispatch when shouldDeclareCountryOfDispatchInItem flag is true with value in header when values in gooditem and bill are empty", "FR", wrapper.CountryOfDispatch);
			});
		}

		public void TestNullConsignee()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInItem flag is false so Consignee is null", () => wrapper.Consignee.ToString());

				wrapper = GetWrapper(goodsItem, shouldDeclareConsigneeInItem: true);
				AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInItem flag is true but consignee is not declared so Consignee is null", () => wrapper.Consignee.ToString());
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgAddress1 = Factory.New<OrgAddress>();
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader1.OH_FullName = "Name1";
				orgAddress1.OA_OH = orgHeader1.PK;
				nctsHeader.Consignee.E2_OA_Address = orgAddress1.PK;

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_FullName = "Name2";
				orgAddress2.OA_OH = orgHeader2.PK;
				nctsBill.Consignee.E2_OA_Address = orgAddress2.PK;

				var orgAddress3 = Factory.New<OrgAddress>();
				var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader3.OH_FullName = "Name3";
				orgAddress3.OA_OH = orgHeader3.PK;
				goodsItem.Consignee.E2_OA_Address = orgAddress3.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInItem flag is false so Consignee is null", () => wrapper.Consignee.ToString());

					wrapper = GetWrapper(goodsItem, shouldDeclareConsigneeInItem: true);
					var consignee = wrapper.Consignee;
					AssertNotNull("TransitionalPeriod: Expected filled Consignee when shouldDeclareConsigneeInItem flag is true", consignee);
					AssertEquals("TransitionalPeriod: Expected filled Consignee when shouldDeclareConsigneeInItem flag is true with value in goodsitem when not empty, Name", "Name3", consignee.Name);
					AssertSame("TransitionalPeriod: Cached Consignee", wrapper.Consignee, consignee);

					goodsItem.Consignee.E2_OA_Address = ZGuid.Empty;
					wrapper = GetWrapper(goodsItem, shouldDeclareConsigneeInItem: true);
					AssertEquals("TransitionalPeriod: Expected filled Consignee when shouldDeclareConsigneeInItem flag is true with value in bill when value in gooditem is empty, Name", "Name2", wrapper.Consignee.Name);

					nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
					wrapper = GetWrapper(goodsItem, shouldDeclareConsigneeInItem: true);
					AssertEquals("TransitionalPeriod: Expected filled Consignee when shouldDeclareConsigneeInItem flag is true with value in header when values in gooditem and bill are empty, Name", "Name1", wrapper.Consignee.Name);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem, shouldDeclareConsigneeInItem: true);
					AssertNull("FinalPeriod: Expected not filled Consignee when final period", wrapper.Consignee);
				}
			});
		}

		public void TestAdditionalSupplyChainActor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalSupplyChainActor when no data declared", 0, wrapper.AdditionalSupplyChainActor.Count);

				goodsItem.CusSupplyChainActorReferences.AddNew();
				wrapper = GetWrapper(goodsItem);
				var additionalSupplyChainActor = wrapper.AdditionalSupplyChainActor;
				AssertEquals("Expected filled AdditionalSupplyChainActor", 1, additionalSupplyChainActor.Count);
				AssertSame("Cached AdditionalSupplyChainActor", wrapper.AdditionalSupplyChainActor, additionalSupplyChainActor);
			});
		}

		public void TestCommodity()
		{
			var commodity = wrapper.Commodity;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Commodity", commodity);
				AssertSame("Cached Commodity", wrapper.Commodity, commodity);
			});
		}

		public void TestPreviousDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousDocument list", 0, wrapper.PreviousDocument.Count);

				var prevdoc1 = goodsItem.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";

				var prevdoc2 = goodsItem.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";

				wrapper = GetWrapper(goodsItem);
				var documents = wrapper.PreviousDocument;

				AssertEquals("Expected filled PreviousDocument with only 1", 1, documents.Count);
				AssertSame("Cached PreviousDocument", wrapper.PreviousDocument, documents);
			});
		}

		public void TestPreviousDocument_FromItem()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousDocument list", 0, wrapper.PreviousDocument.Count);

				var prevdoc1 = goodsItem.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";
				prevdoc1.CSI_LineNo = 4;

				var prevdoc2 = nctsBill.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";
				prevdoc2.CSI_LineNo = 2;

				var prevdoc3 = nctsHeader.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9003";
				prevdoc3.CSI_LineNo = 3;

				wrapper = GetWrapper(goodsItem);
				var documents = wrapper.PreviousDocument;

				AssertEquals("Expected filled PreviousDocument with only 1", 1, documents.Count);
				var doc = documents.First();
				AssertEquals("Expected filled with the correct Name", "9001", doc.Name);
				AssertEquals("Expected filled with the correct SequenceNumber", "1", doc.SequenceNumber);
				AssertSame("Cached PreviousDocument", wrapper.PreviousDocument, documents);
			});
		}

		public void TestPreviousDocument_FromBill()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousDocument list", 0, wrapper.PreviousDocument.Count);

				var prevdoc2 = nctsBill.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";
				prevdoc2.CSI_LineNo = 2;

				var prevdoc3 = nctsHeader.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9003";
				prevdoc3.CSI_LineNo = 3;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.PreviousDocument;

					AssertEquals("TransitionalPeriod: Expected filled PreviousDocument with only 1", 1, documents.Count);
					var doc = documents.First();
					AssertEquals("TransitionalPeriod: Expected filled with the correct Name", "9002", doc.Name);
					AssertEquals("TransitionalPeriod: Expected filled with the correct SequenceNumber", "1", doc.SequenceNumber);
					AssertSame("TransitionalPeriod: Cached PreviousDocument", wrapper.PreviousDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.PreviousDocument;

					AssertEquals("FinalPeriod: Expected filled PreviousDocument with Header", 1, documents.Count);
					var doc = documents.First();
					AssertEquals("FinalPeriod: Expected filled with the correct Name", "9003", doc.Name);
					AssertEquals("FinalPeriod: Expected filled with the correct SequenceNumber", "1", doc.SequenceNumber);
					AssertSame("FinalPeriod: Cached PreviousDocument", wrapper.PreviousDocument, documents);
				}
			});
		}

		public void TestPreviousDocument_FromHeader()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousDocument list", 0, wrapper.PreviousDocument.Count);

				var prevdoc3 = nctsHeader.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9003";
				prevdoc3.CSI_LineNo = 3;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.PreviousDocument;

					AssertEquals("TransitionalPeriod: Expected filled PreviousDocument with only 1", 1, documents.Count);
					var doc = documents.First();
					AssertEquals("TransitionalPeriod: Expected filled with the correct Name", "9003", doc.Name);
					AssertEquals("TransitionalPeriod: Expected filled with the correct SequenceNumber", "1", doc.SequenceNumber);
					AssertSame("TransitionalPeriod: Cached PreviousDocument", wrapper.PreviousDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.PreviousDocument;

					AssertEquals("FinalPeriod: Expected not filled PreviousDocument with Header", 1, documents.Count);
					var doc = documents.First();
					AssertEquals("FinalPeriod: Expected filled with the correct Name", "9003", doc.Name);
					AssertEquals("FinalPeriod: Expected filled with the correct SequenceNumber", "1", doc.SequenceNumber);
					AssertSame("FinalPeriod: Cached PreviousDocument", wrapper.PreviousDocument, documents);
				}
			});
		}

		public void TestPreviousDocumentQuantityVehicles()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = true;
				goodsItem.Packages.AddNew();
				goodsItem.Packages.AddNew();

				var prevdoc1 = goodsItem.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.IsVehicles = true;
				goodsItem2.Packages.AddNew();

				var prevdoc2 = goodsItem2.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";

				wrapper = GetWrapper(goodsItem);
				var wrapper2 = GetWrapper(goodsItem2);
				var documents = wrapper.PreviousDocument;
				var documents2 = wrapper2.PreviousDocument;

				AssertEquals("Expected filled in GoodItem1 PreviousDocument Quantity with 2 Vehicles", 2.0m, documents.FirstOrDefault().Quantity);
				AssertEquals("Expected filled in GoodItem2 PreviousDocument Quantity with 1 Vehicles", 1.0m, documents2.FirstOrDefault().Quantity);
			});
		}

		public void TestPreviousDocumentQuantityNoVehicles()
		{
			CombineAssertions(() =>
			{
				var prevdoc1 = goodsItem.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";
				prevdoc1.CSI_Quantity = 2;
				prevdoc1.CSI_UnitOfQuantity = "A";

				var goodsItem2 = nctsBill.GoodsItems.AddNew();

				var prevdoc2 = goodsItem2.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9001";
				prevdoc2.CSI_Quantity = 1;
				prevdoc2.CSI_UnitOfQuantity = "A";

				wrapper = GetWrapper(goodsItem);
				var wrapper2 = GetWrapper(goodsItem2);
				var documents = wrapper.PreviousDocument;
				var documents2 = wrapper2.PreviousDocument;

				AssertEquals("Expected filled in GoodItem1 PreviousDocument Quantity with 2", 2.0m, documents.FirstOrDefault().Quantity);
				AssertEquals("Expected filled in GoodItem2 PreviousDocument Quantity with 1", 1.0m, documents2.FirstOrDefault().Quantity);
			});
		}

		public void TestPreviousDocumentQuantityTwoGoodsOneVehiclesOneNot()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = true;
				goodsItem.Packages.AddNew();
				goodsItem.Packages.AddNew();
				goodsItem.Packages.AddNew();
				var prevdoc1 = goodsItem.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";

				var goodsItem2 = nctsBill.GoodsItems.AddNew();

				var prevdoc2 = goodsItem2.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9001";
				prevdoc2.CSI_Quantity = 1;
				prevdoc2.CSI_UnitOfQuantity = "A";

				wrapper = GetWrapper(goodsItem);
				var wrapper2 = GetWrapper(goodsItem2);
				var documents = wrapper.PreviousDocument;
				var documents2 = wrapper2.PreviousDocument;

				AssertEquals("Expected filled in GoodItem1 PreviousDocument Quantity with 3 Vehicles", 3.0m, documents.FirstOrDefault().Quantity);
				AssertEquals("Expected filled in GoodItem2 PreviousDocument Quantity with CSI_Quantity value 1", 1.0m, documents2.FirstOrDefault().Quantity);
			});
		}

		public void TestSupportingDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);

				var supdoc1 = goodsItem.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_LineNo = 2;

				var supdoc2 = goodsItem.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "Y001";
				supdoc2.CSI_LineNo = 4;

				var supdoc3 = nctsBill.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "A003";
				supdoc3.CSI_LineNo = 3;

				var supdoc4 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "5004";
				supdoc4.CSI_LineNo = 1;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.SupportingDocument;

					AssertEquals("TransitionalPeriod: Expected filled SupportingDocument", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled SupportingDocuments ordered Name", new ZString[] { "5004", "9001", "A003", "Y001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled SupportingDocuments ordered SequenceNumber", new ZString[] { "1", "2", "3", "4" }, documents.Select(x => x.SequenceNumber));
					AssertSame("TransitionalPeriod: Cached SupportingDocument", wrapper.SupportingDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.SupportingDocument;

					AssertEquals("FinalPeriod: Expected filled SupportingDocument", 2, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered Name", new ZString[] { "9001", "Y001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered SequenceNumber", new ZString[] { "2", "4" }, documents.Select(x => x.SequenceNumber));
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
			nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.GoodsItems.AddNew();
			wrapper = GetWrapper(goodsItem);
		}

		NctsDepartureCargoDesc goodsItem;
		NctsBill nctsBill;
		NctsDepartureMovementHeader departureMovement;
		NctsHeader nctsHeader;
		NCTS5ConsignmentItemDepartureAndAmendmentWrapper wrapper;

		NCTS5ConsignmentItemDepartureAndAmendmentWrapper GetWrapper(NctsDepartureCargoDesc item, bool shouldDeclareDeclarationTypeInItem = false, bool shouldDeclareCountryOfDispatchInItem = false, bool shouldDeclareCountryOfDestinationInItem = false, bool shouldDeclareReferenceNumberUCRInItem = false, bool shouldDeclareConsigneeInItem = false) => new NCTS5ConsignmentItemDepartureAndAmendmentWrapper(item, shouldDeclareDeclarationTypeInItem, shouldDeclareCountryOfDispatchInItem, shouldDeclareCountryOfDestinationInItem, shouldDeclareReferenceNumberUCRInItem, shouldDeclareConsigneeInItem);

		protected override NCTS5ConsignmentItemDepartureAndAmendmentWrapper GetProvider() => wrapper;
	}
}
