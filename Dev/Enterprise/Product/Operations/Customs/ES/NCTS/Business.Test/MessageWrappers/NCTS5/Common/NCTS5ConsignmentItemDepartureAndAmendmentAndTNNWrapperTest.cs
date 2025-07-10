using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapperTest : WrapperHelperTest<NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper>
	{
		public void TestDeclarationType()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				AssertEquals("Expected empty DeclarationType when shouldDeclareDeclarationTypeInItem flag is false", ZString.Empty, wrapper.DeclarationType);

				wrapper = GetWrapper(goodsItem, shouldDeclareDeclarationTypeInItem: true);
				AssertEquals("Expected filled DeclarationType when shouldDeclareDeclarationTypeInItem flag is true", "T1", wrapper.DeclarationType);
			});
		}

		public void TestCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_RL_NKDestinationPort = "FR";
				nctsBill.B0_RN_NKCountryOfDestination = "ES";
				goodsItem.BY_RN_NKCountryOfDestination = "DE";
				AssertEquals("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInItem flag is false", ZString.Empty, wrapper.CountryOfDestination);

				wrapper = GetWrapper(goodsItem, shouldDeclareCountryOfDestinationInItem: true);
				AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInItem flag is true with value in goodsitem when not empty", "DE", wrapper.CountryOfDestination);

				goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
				AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInItem flag is true with value in bill when value in gooditem is empty", "ES", wrapper.CountryOfDestination);

				nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
				AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInItem flag is true with value in header when values in gooditem and bill are empty", "FR", wrapper.CountryOfDestination);
			});
		}

		public void TestReferenceNumberUCR()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_UniqueConsignmentReference = "reference2";
				nctsBill.B0_ReferenceID = "reference1";
				goodsItem.BY_CommercialReferenceNumber = "reference";
				AssertEquals("Expected empty ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is false", ZString.Empty, wrapper.ReferenceNumberUCR);

				wrapper = GetWrapper(goodsItem, shouldDeclareReferenceNumberUCRInItem: true);
				AssertEquals("Expected filled ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true with value in goodsitem when not empty", "reference", wrapper.ReferenceNumberUCR);

				goodsItem.BY_CommercialReferenceNumber = ZString.Empty;
				AssertEquals("Expected filled ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true with value in bill when value in gooditem is empty", "reference1", wrapper.ReferenceNumberUCR);

				nctsBill.B0_ReferenceID = ZString.Empty;
				AssertEquals("Expected filled ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true with value in header when values in gooditem and bill are empty", "reference2", wrapper.ReferenceNumberUCR);
			});
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalInformation list", 0, wrapper.AdditionalInformation.Count);

				var addInfo1 = goodsItem.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "TRA";
				addInfo1.CSI_ReferenceNumber = "Ref1TRA";
				addInfo1.CSI_Description = "Desc1TRA";
				addInfo1.CSI_LineNo = 1;

				var addInfo1INF = goodsItem.AdditionalInfos.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_ReferenceNumber = "Ref1";
				addInfo1INF.CSI_Description = "Desc1";
				addInfo1INF.CSI_LineNo = 3;

				var addInfo2 = goodsItem.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "REF";
				addInfo2.CSI_ReferenceNumber = "Ref2REF";
				addInfo2.CSI_Description = "Desc2REF";
				addInfo2.CSI_LineNo = 2;

				var addInfo2INF = goodsItem.AdditionalInfos.AddNew();
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

				var addInfo4INF = nctsHeader.AdditionalDocuments.AddNew();
				addInfo4INF.CSI_Code = "Y004";
				addInfo4INF.CSI_SubType = "INF";
				addInfo4INF.CSI_ReferenceNumber = "Ref4";
				addInfo4INF.CSI_Description = "Desc4";
				addInfo4INF.CSI_LineNo = 6;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.AdditionalInformation;

					AssertEquals("TransitionalPeriod: Expected filled AdditionalInformation (Included those that have subType INF)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y003", "Y002", "Y001", "Y004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled AdditionalInformation correct and ordered Number", new ZString[] { "Desc3", "Desc2", "Desc1", "Desc4" }, documents.Select(x => x.Number));
					AssertSame("TransitionalPeriod: Cached TransportDocumAdditionalInformationent", wrapper.AdditionalInformation, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.AdditionalInformation;

					AssertEquals("FinalPeriod: Expected filled AdditionalInformation", 2, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y002", "Y001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "2", "3" }, documents.Select(x => x.SequenceNumber));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation correct and ordered Number", new ZString[] { "Desc2", "Desc1" }, documents.Select(x => x.Number));
				}
			});
		}

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

				AssertEquals("Expected filled Packaging", 2, packaging.Count);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);
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
		NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper wrapper;

		NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper GetWrapper(NctsDepartureCargoDesc item, bool shouldDeclareDeclarationTypeInItem = false, bool shouldDeclareCountryOfDestinationInItem = false, bool shouldDeclareReferenceNumberUCRInItem = false) => new NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper(item, shouldDeclareDeclarationTypeInItem, shouldDeclareCountryOfDestinationInItem, shouldDeclareReferenceNumberUCRInItem);

		protected override NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper GetProvider() => wrapper;
	}
}
