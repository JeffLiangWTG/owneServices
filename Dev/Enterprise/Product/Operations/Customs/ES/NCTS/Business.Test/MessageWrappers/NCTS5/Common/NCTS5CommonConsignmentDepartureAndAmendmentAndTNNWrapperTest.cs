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
	sealed class NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapperTest : WrapperHelperTest<NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
			});
		}

		public void TestCountryOfDispatch()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					departureMovement.BM_RN_NKCountryOfDispatch = "ES";
					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: true, shouldDeclareCountryOfDispatchInConsignment: true);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch when isTIRDeclaration and shouldDeclareCountryOfDestinationInConsignment are true", "ES", wrapper.CountryOfDispatch);

					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: false, shouldDeclareCountryOfDispatchInConsignment: true);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch when shouldDeclareCountryOfDestinationInConsignment is true but isTIRDeclaration is false", ZString.Empty, wrapper.CountryOfDispatch);

					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: true, shouldDeclareCountryOfDispatchInConsignment: false);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch when isTIRDeclaration is true but shouldDeclareCountryOfDestinationInConsignment is false", ZString.Empty, wrapper.CountryOfDispatch);

					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: false, shouldDeclareCountryOfDispatchInConsignment: false);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch when isTIRDeclaration and shouldDeclareCountryOfDestinationInConsignment are false", ZString.Empty, wrapper.CountryOfDispatch);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: true, shouldDeclareCountryOfDispatchInConsignment: true);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch when isTIRDeclaration and shouldDeclareCountryOfDestinationInConsignment are true", "ES", wrapper.CountryOfDispatch);

					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: true, shouldDeclareCountryOfDispatchInConsignment: false);
					AssertEquals("FinalPeriod: Expected empty CountryOfDispatch when isTIRDeclaration is true but shouldDeclareCountryOfDestinationInConsignment is false", ZString.Empty, wrapper.CountryOfDispatch);

					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: false, shouldDeclareCountryOfDispatchInConsignment: true);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch when shouldDeclareCountryOfDestinationInConsignment is true but isTIRDeclaration is false", "ES", wrapper.CountryOfDispatch);

					wrapper = GetWrapper(nctsHeader, isTIRDeclaration: false, shouldDeclareCountryOfDispatchInConsignment: false);
					AssertEquals("FinalPeriod: Expected empty CountryOfDispatch when isTIRDeclaration and shouldDeclareCountryOfDestinationInConsignment are false", ZString.Empty, wrapper.CountryOfDispatch);
				}
			});
		}

		public void TestCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_RL_NKDestinationPort = "ES";
				wrapper = GetWrapper(nctsHeader, shouldDeclareCountryOfDestinationInConsignment: true);
				AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInConsignment si true", "ES", wrapper.CountryOfDestination);

				wrapper = GetWrapper(nctsHeader, shouldDeclareCountryOfDestinationInConsignment: false);
				AssertEquals("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInConsignment is false", ZString.Empty, wrapper.CountryOfDestination);
			});
		}

		public void TestReferenceNumberUCR()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_UniqueConsignmentReference = "reference";
				wrapper = GetWrapper(nctsHeader, shouldDeclareUCRInConsignment: true);
				AssertEquals("Expected filled ReferenceNumberUCR when shouldDeclareUCRInConsignment is true", "reference", wrapper.ReferenceNumberUCR);

				wrapper = GetWrapper(nctsHeader, shouldDeclareUCRInConsignment: false);
				AssertEquals("Expected empty ReferenceNumberUCR when shouldDeclareUCRInConsignment is false", ZString.Empty, wrapper.ReferenceNumberUCR);
			});
		}

		public void TestNullConsignee()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignee.ToString());
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				nctsHeader.Consignee.E2_OA_Address = orgAddress.PK;

				wrapper = GetWrapper(nctsHeader, shouldDeclareConsigneeInConsignment: true);
				var consignee = wrapper.Consignee;
				AssertNotNull("Expected filled Consignee when shouldDeclareConsigneeInConsignment is true", consignee);
				AssertSame("Cached Consignee", wrapper.Consignee, consignee);

				wrapper = GetWrapper(nctsHeader, shouldDeclareConsigneeInConsignment: true, any30600AdditionalInfoInItems: true);
				AssertNull("Expected empty Consignee when shouldDeclareConsigneeInConsignment is true but any30600AdditionalInfoInItems is true", wrapper.Consignee);

				wrapper = GetWrapper(nctsHeader, shouldDeclareConsigneeInConsignment: true, any30600AdditionalInfoInItems: false);
				AssertNotNull("Expected filled Consignee when shouldDeclareConsigneeInConsignment is true and any30600AdditionalInfoInItems is false", wrapper.Consignee);

				wrapper = GetWrapper(nctsHeader, shouldDeclareConsigneeInConsignment: false);
				AssertNull("Expected empty Consignee when shouldDeclareConsigneeInConsignment is false", wrapper.Consignee);
			});
		}

		public void TestCountryOfRoutingOfConsignment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty CountryOfRoutingOfConsignment when no data declared", 0, wrapper.CountryOfRoutingOfConsignment.Count);

				nctsHeader.CountriesOfRouting.AddNew();
				wrapper = GetWrapper(nctsHeader);
				var countryOfRoutingOfConsignment = wrapper.CountryOfRoutingOfConsignment;
				AssertEquals("Expected filled CountryOfRoutingOfConsignment", 1, countryOfRoutingOfConsignment.Count);
				AssertSame("Cached CountryOfRoutingOfConsignment", wrapper.CountryOfRoutingOfConsignment, countryOfRoutingOfConsignment);
			});
		}

		public void TestMethodOfPayment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty MethodOfPayment when not declared", ZString.Empty, wrapper.MethodOfPayment);

				departureMovement.BM_MethodOfPayment = "A";
				wrapper = GetWrapper(nctsHeader, isEXISecurityType: true);
				AssertEquals("Expected filled MethodOfPayment when isEXISecurityType is true", "A", wrapper.MethodOfPayment);

				wrapper = GetWrapper(nctsHeader, isEXISecurityType: false);
				AssertEquals("Expected empty MethodOfPayment when isEXISecurityType is false", ZString.Empty, wrapper.MethodOfPayment);
			});
		}

		public void TestGrossMass()
		{
			var nctsBill1 = nctsHeader.Bills.AddNew();
			var goodsItem1 = nctsBill1.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem1.BY_GrossWeight = 200.4455m;
			departureMovement.BM_GrossWeight = 405.5336789m;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
			{
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("TransitionalPeriod: Expected filled GrossMass 3 decimals with header's gross mass and not the sum of the gross masses in bills items", 405.534m, wrapper.GrossMass);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("FinalPeriod: Expected filled GrossMass 6 decimals with header's gross mass and not the sum of the gross masses in bills items", 405.533679m, wrapper.GrossMass);
			}
		}

		public void TestSupportingDocument()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);

					var supdoc1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					supdoc1.CSI_Code = "9001";
					supdoc1.CSI_LineNo = 2;

					var supdoc2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					supdoc2.CSI_Code = "Y001";
					supdoc2.CSI_LineNo = 4;

					var supdoc3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					supdoc3.CSI_Code = "A003";
					supdoc3.CSI_LineNo = 3;

					var supdoc4 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					supdoc4.CSI_Code = "5004";
					supdoc4.CSI_LineNo = 1;

					var nctsBill = nctsHeader.Bills.AddNew();
					var supdoc5 = nctsBill.SupportingDocuments.AddNew();
					supdoc5.CSI_Code = "5005";
					supdoc5.CSI_LineNo = 1;

					var supdoc6 = nctsBill.GoodsItems.AddNew().SupportingDocuments.AddNew();
					supdoc6.CSI_Code = "5006";
					supdoc6.CSI_LineNo = 1;

					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.SupportingDocument;

					AssertEquals("FinalPeriod: Expected filled SupportingDocument", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered Name", new ZString[] { "5004", "9001", "A003", "Y001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered SequenceNumber", new ZString[] { "1", "2", "3", "4" }, documents.Select(x => x.SequenceNumber));
					AssertSame("FinalPeriod: Cached SupportingDocument", wrapper.SupportingDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.SupportingDocument;
					AssertEquals("TransitionalPeriod: Expected not filled SupportingDocument", 0, documents.Count);
				}
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty TransportDocument list", 0, wrapper.TransportDocument.Count);

					var addInfo1 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo1.CSI_Code = "9001";
					addInfo1.CSI_SubType = "TRA";
					addInfo1.CSI_LineNo = 3;

					var addInfo1INF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo1INF.CSI_Code = "Y001";
					addInfo1INF.CSI_SubType = "INF";
					addInfo1INF.CSI_LineNo = 1;

					var addInfo2 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo2.CSI_Code = "9002";
					addInfo2.CSI_SubType = "TRA";
					addInfo2.CSI_LineNo = 2;

					var addInfo2REF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo2REF.CSI_Code = "Y002";
					addInfo2REF.CSI_SubType = "REF";
					addInfo2REF.CSI_LineNo = 2;

					var addInfo3 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo3.CSI_Code = "9003";
					addInfo3.CSI_SubType = "TRA";
					addInfo3.CSI_LineNo = 1;

					var addInfo4 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo4.CSI_Code = "9004";
					addInfo4.CSI_SubType = "TRA";
					addInfo4.CSI_LineNo = 6;

					var nctsBill = nctsHeader.Bills.AddNew();
					var addInfo5 = nctsBill.AdditionalDocuments.AddNew();
					addInfo5.CSI_Code = "5005";
					addInfo5.CSI_SubType = "TRA";
					addInfo5.CSI_LineNo = 1;

					var addInfo6 = nctsBill.GoodsItems.AddNew().AdditionalInfos.AddNew();
					addInfo6.CSI_Code = "5006";
					addInfo6.CSI_SubType = "TRA";
					addInfo6.CSI_LineNo = 1;

					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.TransportDocument;

					AssertEquals("FinalPeriod: Expected filled TransportDocument (Included those that have subType TRA)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled TransportDocument ordered Name", new ZString[] { "9003", "9002", "9001", "9004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled TransportDocument ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertSame("FinalPeriod: Cached TransportDocument", wrapper.TransportDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.TransportDocument;

					AssertEquals("TransitionalPeriod: Expected not filled TransportDocument", 0, documents.Count);
				}
			});
		}

		public void TestAdditionalReference()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);

					var addInfo1 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo1.CSI_Code = "9001";
					addInfo1.CSI_SubType = "TRA";
					addInfo1.CSI_LineNo = 1;

					var addInfo1REF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo1REF.CSI_Code = "Y001";
					addInfo1REF.CSI_SubType = "REF";
					addInfo1REF.CSI_LineNo = 3;

					var addInfo2 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo2.CSI_Code = "9002";
					addInfo2.CSI_SubType = "INF";
					addInfo2.CSI_LineNo = 2;

					var addInfo2REF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo2REF.CSI_Code = "Y002";
					addInfo2REF.CSI_SubType = "REF";
					addInfo2REF.CSI_LineNo = 2;

					var addInfo3REF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo3REF.CSI_Code = "Y003";
					addInfo3REF.CSI_SubType = "REF";
					addInfo3REF.CSI_LineNo = 1;

					var addInfo4REF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo4REF.CSI_Code = "Y004";
					addInfo4REF.CSI_SubType = "REF";
					addInfo4REF.CSI_LineNo = 6;

					var nctsBill = nctsHeader.Bills.AddNew();
					var addInfo5 = nctsBill.AdditionalDocuments.AddNew();
					addInfo5.CSI_Code = "5005";
					addInfo5.CSI_SubType = "REF";
					addInfo5.CSI_LineNo = 1;

					var addInfo6 = nctsBill.GoodsItems.AddNew().AdditionalInfos.AddNew();
					addInfo6.CSI_Code = "5006";
					addInfo6.CSI_SubType = "REF";
					addInfo6.CSI_LineNo = 1;

					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.AdditionalReference;

					AssertEquals("FinalPeriod: Expected filled AdditionalReference (Included those that have subType REF)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y003", "Y002", "Y001", "Y004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertSame("FinalPeriod: Cached AdditionalReference", wrapper.AdditionalReference, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.AdditionalReference;

					AssertEquals("TransitionalPeriod: Expected not filled AdditionalReference", 0, documents.Count);
				}
			});
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty AdditionalInformation list", 0, wrapper.AdditionalInformation.Count);

					var addInfo1 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo1.CSI_Code = "9001";
					addInfo1.CSI_SubType = "TRA";
					addInfo1.CSI_ReferenceNumber = "Ref1TRA";
					addInfo1.CSI_Description = "Desc1TRA";
					addInfo1.CSI_LineNo = 1;

					var addInfo1INF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo1INF.CSI_Code = "Y001";
					addInfo1INF.CSI_SubType = "INF";
					addInfo1INF.CSI_ReferenceNumber = "Ref1";
					addInfo1INF.CSI_Description = "Desc1";
					addInfo1INF.CSI_LineNo = 3;

					var addInfo2 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo2.CSI_Code = "9002";
					addInfo2.CSI_SubType = "REF";
					addInfo2.CSI_ReferenceNumber = "Ref2TRA";
					addInfo2.CSI_Description = "Desc2TRA";
					addInfo2.CSI_LineNo = 2;

					var addInfo2INF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo2INF.CSI_Code = "Y002";
					addInfo2INF.CSI_SubType = "INF";
					addInfo2INF.CSI_ReferenceNumber = "Ref2";
					addInfo2INF.CSI_Description = "Desc2";
					addInfo2INF.CSI_LineNo = 2;

					var addInfo3INF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo3INF.CSI_Code = "Y003";
					addInfo3INF.CSI_SubType = "INF";
					addInfo3INF.CSI_ReferenceNumber = "Ref3";
					addInfo3INF.CSI_Description = "Desc3";
					addInfo3INF.CSI_LineNo = 1;

					var addInfo4INF = nctsHeader.AdditionalDocuments.AddNew();
					addInfo4INF.CSI_Code = "Y004";
					addInfo4INF.CSI_SubType = "INF";
					addInfo4INF.CSI_ReferenceNumber = "Ref4";
					addInfo4INF.CSI_Description = "Desc4";
					addInfo4INF.CSI_LineNo = 6;

					var nctsBill = nctsHeader.Bills.AddNew();
					var addInfo5 = nctsBill.AdditionalDocuments.AddNew();
					addInfo5.CSI_Code = "5005";
					addInfo5.CSI_SubType = "INF";
					addInfo5.CSI_LineNo = 1;

					var addInfo6 = nctsBill.GoodsItems.AddNew().AdditionalInfos.AddNew();
					addInfo6.CSI_Code = "5006";
					addInfo6.CSI_SubType = "INF";
					addInfo6.CSI_LineNo = 1;

					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.AdditionalInformation;

					AssertEquals("FinalPeriod: Expected filled AdditionalInformation (Included those that have subType INF)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y003", "Y002", "Y001", "Y004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation correct and ordered Number", new ZString[] { "Desc3", "Desc2", "Desc1", "Desc4" }, documents.Select(x => x.Number));
					AssertSame("FinalPeriod: Cached TransportDocumAdditionalInformationent", wrapper.AdditionalInformation, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					var documents = wrapper.AdditionalInformation;

					AssertEquals("TransitionalPeriod: Expected not filled AdditionalInformation", 0, documents.Count);
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

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper wrapper;

		NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper GetWrapper(NctsHeader header, bool isTIRDeclaration = false, bool shouldDeclareCountryOfDispatchInConsignment = false, bool shouldDeclareCountryOfDestinationInConsignment = false, bool shouldDeclareUCRInConsignment = false, bool shouldDeclareConsigneeInConsignment = false, bool any30600AdditionalInfoInItems = false, bool isEXISecurityType = false) => new NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper(header, isTIRDeclaration, shouldDeclareCountryOfDispatchInConsignment, shouldDeclareCountryOfDestinationInConsignment, shouldDeclareUCRInConsignment, shouldDeclareConsigneeInConsignment, any30600AdditionalInfoInItems, isEXISecurityType);

		protected override NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper GetProvider() => wrapper;
	}
}
