using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapperTest : WrapperHelperTest<NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper>
	{
		public void TestReferenceNumberUCR()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_UniqueConsignmentReference = "Reference3";
				nctsBill.B0_ReferenceID = "Reference";
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertEquals("TransitionalPeriod: Expected not filled ReferenceNumberUCR", ZString.Empty, wrapper.ReferenceNumberUCR);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_CommercialReferenceNumber = "Reference";
					wrapper = GetWrapper(nctsBill, shouldDeclareReferenceUCRInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected filled ReferenceNumberUCR when shouldDeclareReferenceUCRInHouseOrItem flag is true and Item ReferenceNumberUCR is the same", "Reference", wrapper.ReferenceNumberUCR);

					goodsItem1.BY_CommercialReferenceNumber = "Reference2";
					wrapper = GetWrapper(nctsBill, shouldDeclareReferenceUCRInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected not filled ReferenceNumberUCR when shouldDeclareReferenceUCRInHouseOrItem flag is true but Item ReferenceNumberUCR is not the same", ZString.Empty, wrapper.ReferenceNumberUCR);

					nctsBill.B0_ReferenceID = ZString.Empty;
					goodsItem1.BY_CommercialReferenceNumber = ZString.Empty;
					wrapper = GetWrapper(nctsBill, shouldDeclareReferenceUCRInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected filled ReferenceNumberUCR from Header when shouldDeclareReferenceUCRInHouseOrItem flag is true and Bill ReferenceNumberUCR is Empty", "Reference3", wrapper.ReferenceNumberUCR);
				}
			});
		}

		public void TestSupportingDocument()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);

					var supdoc1 = nctsBill.SupportingDocuments.AddNew();
					supdoc1.CSI_Code = "9001";
					supdoc1.CSI_LineNo = 2;

					var supdoc2 = nctsBill.SupportingDocuments.AddNew();
					supdoc2.CSI_Code = "Y001";
					supdoc2.CSI_LineNo = 4;

					var supdoc3 = nctsBill.SupportingDocuments.AddNew();
					supdoc3.CSI_Code = "A003";
					supdoc3.CSI_LineNo = 3;

					var supdoc4 = nctsBill.SupportingDocuments.AddNew();
					supdoc4.CSI_Code = "5004";
					supdoc4.CSI_LineNo = 1;

					var supdoc5 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					supdoc5.CSI_Code = "5005";
					supdoc5.CSI_LineNo = 1;

					var supdoc6 = nctsBill.GoodsItems.AddNew().SupportingDocuments.AddNew();
					supdoc6.CSI_Code = "5006";
					supdoc6.CSI_LineNo = 1;

					wrapper = GetWrapper(nctsBill);
					var documents = wrapper.SupportingDocument;

					AssertEquals("FinalPeriod: Expected filled SupportingDocument", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered Name", new ZString[] { "5004", "9001", "A003", "Y001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered SequenceNumber", new ZString[] { "1", "2", "3", "4" }, documents.Select(x => x.SequenceNumber));
					AssertSame("FinalPeriod: Cached SupportingDocument", wrapper.SupportingDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsBill);
					var documents = wrapper.SupportingDocument;
					AssertEquals("TransitionalPeriod: Expected not filled SupportingDocument", 0, documents.Count);
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

					var addInfo1 = nctsBill.AdditionalDocuments.AddNew();
					addInfo1.CSI_Code = "9001";
					addInfo1.CSI_SubType = "TRA";
					addInfo1.CSI_ReferenceNumber = "Ref1TRA";
					addInfo1.CSI_Description = "Desc1TRA";
					addInfo1.CSI_LineNo = 1;

					var addInfo1INF = nctsBill.AdditionalDocuments.AddNew();
					addInfo1INF.CSI_Code = "Y001";
					addInfo1INF.CSI_SubType = "INF";
					addInfo1INF.CSI_ReferenceNumber = "Ref1";
					addInfo1INF.CSI_Description = "Desc1";
					addInfo1INF.CSI_LineNo = 3;

					var addInfo2 = nctsBill.AdditionalDocuments.AddNew();
					addInfo2.CSI_Code = "9002";
					addInfo2.CSI_SubType = "REF";
					addInfo2.CSI_ReferenceNumber = "Ref2TRA";
					addInfo2.CSI_Description = "Desc2TRA";
					addInfo2.CSI_LineNo = 2;

					var addInfo2INF = nctsBill.AdditionalDocuments.AddNew();
					addInfo2INF.CSI_Code = "Y002";
					addInfo2INF.CSI_SubType = "INF";
					addInfo2INF.CSI_ReferenceNumber = "Ref2";
					addInfo2INF.CSI_Description = "Desc2";
					addInfo2INF.CSI_LineNo = 2;

					var addInfo3INF = nctsBill.AdditionalDocuments.AddNew();
					addInfo3INF.CSI_Code = "Y003";
					addInfo3INF.CSI_SubType = "INF";
					addInfo3INF.CSI_ReferenceNumber = "Ref3";
					addInfo3INF.CSI_Description = "Desc3";
					addInfo3INF.CSI_LineNo = 1;

					var addInfo4INF = nctsBill.AdditionalDocuments.AddNew();
					addInfo4INF.CSI_Code = "Y004";
					addInfo4INF.CSI_SubType = "INF";
					addInfo4INF.CSI_ReferenceNumber = "Ref4";
					addInfo4INF.CSI_Description = "Desc4";
					addInfo4INF.CSI_LineNo = 6;

					var addInfo5 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo5.CSI_Code = "5005";
					addInfo5.CSI_SubType = "INF";
					addInfo5.CSI_LineNo = 1;

					var addInfo6 = nctsBill.GoodsItems.AddNew().AdditionalInfos.AddNew();
					addInfo6.CSI_Code = "5006";
					addInfo6.CSI_SubType = "INF";
					addInfo6.CSI_LineNo = 1;

					wrapper = GetWrapper(nctsBill);
					var documents = wrapper.AdditionalInformation;

					AssertEquals("FinalPeriod: Expected filled AdditionalInformation (Included those that have subType INF)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y003", "Y002", "Y001", "Y004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation correct and ordered Number", new ZString[] { "Desc3", "Desc2", "Desc1", "Desc4" }, documents.Select(x => x.Number));
					AssertSame("FinalPeriod: Cached TransportDocumAdditionalInformationent", wrapper.AdditionalInformation, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsBill);
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
			nctsBill = nctsHeader.Bills.AddNew();
			wrapper = GetWrapper(nctsBill);
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper wrapper;

		NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper GetWrapper(NctsBill bill, bool shouldDeclareReferenceUCRInHouseOrItem = false) => new NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper(bill, shouldDeclareReferenceUCRInHouseOrItem);

		protected override NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper GetProvider() => wrapper;
	}
}
