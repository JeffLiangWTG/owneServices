using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonConsignmentItemWrapperTest : WrapperHelperTest<NCTS5CommonConsignmentItemWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if item is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "item"), () => GetWrapper(null));
		}

		public void TestGoodsItemNumber()
		{
			goodsItem.BY_LineNo = 3;
			AssertEquals("Expected filled GoodsItemNumber", "3", wrapper.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			goodsItem.BY_DeclarationGoodsItemNumber = 4;
			AssertEquals("Expected filled DeclarationGoodsItemNumber", "4", wrapper.DeclarationGoodsItemNumber);
		}

		public void TestPackaging()
		{
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_UnitType = "CT";

			var package2 = goodsItem.Packages.AddNew();
			package2.B5_UnitType = "FR";

			wrapper = GetWrapper(goodsItem);
			var packaging = wrapper.Packaging;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Packaging in base class even when there are packages in the item, should be inherited", 0, packaging.Count);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);
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
				addInfo1.CSI_LineNo = 3;

				var addInfo1INF = goodsItem.AdditionalInfos.AddNew();
				addInfo1INF.CSI_Code = "Y001";
				addInfo1INF.CSI_SubType = "INF";
				addInfo1INF.CSI_LineNo = 1;

				var addInfo2 = goodsItem.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "TRA";
				addInfo2.CSI_LineNo = 2;

				var addInfo2INF = goodsItem.AdditionalInfos.AddNew();
				addInfo2INF.CSI_Code = "Y002";
				addInfo2INF.CSI_SubType = "REF";
				addInfo2INF.CSI_LineNo = 2;

				var addInfo3 = nctsBill.AdditionalDocuments.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";
				addInfo3.CSI_LineNo = 1;

				var addInfo4 = nctsHeader.AdditionalDocuments.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "TRA";
				addInfo4.CSI_LineNo = 6;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.TransportDocument;

					AssertEquals("TransitionalPeriod: Expected filled TransportDocument (Included those that have subType TRA)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled TransportDocument ordered Name", new ZString[] { "9003", "9002", "9001", "9004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled TransportDocument ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertSame("TransitionalPeriod: Cached TransportDocument", wrapper.TransportDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.TransportDocument;

					AssertEquals("FinalPeriod: Expected not filled TransportDocument for final period", 0, documents.Count);
				}
			});
		}

		public void TestAdditionalReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);

				var addInfo1 = goodsItem.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "TRA";
				addInfo1.CSI_LineNo = 1;

				var addInfo1REF = goodsItem.AdditionalInfos.AddNew();
				addInfo1REF.CSI_Code = "Y001";
				addInfo1REF.CSI_SubType = "REF";

				var addInfo2 = goodsItem.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "INF";
				addInfo2.CSI_LineNo = 2;

				var addInfo2REF = goodsItem.AdditionalInfos.AddNew();
				addInfo2REF.CSI_Code = "Y002";
				addInfo2REF.CSI_SubType = "REF";
				addInfo2REF.CSI_LineNo = 2;

				var addInfo3REF = nctsBill.AdditionalDocuments.AddNew();
				addInfo3REF.CSI_Code = "Y003";
				addInfo3REF.CSI_SubType = "REF";
				addInfo3REF.CSI_LineNo = 1;

				var addInfo4REF = nctsHeader.AdditionalDocuments.AddNew();
				addInfo4REF.CSI_Code = "Y004";
				addInfo4REF.CSI_SubType = "REF";
				addInfo4REF.CSI_LineNo = 6;

				addInfo1REF.CSI_LineNo = 3;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.AdditionalReference;

					AssertEquals("TransitionalPeriod: Expected filled AdditionalReference (Included those that have subType REF)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y003", "Y002", "Y001", "Y004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("TransitionalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertSame("TransitionalPeriod: Cached AdditionalReference", wrapper.AdditionalReference, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(goodsItem);
					var documents = wrapper.AdditionalReference;

					AssertEquals("FinalPeriod: Expected filled AdditionalReference (Included those that have subType REF)", 2, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y002", "Y001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "2", "3" }, documents.Select(x => x.SequenceNumber));
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
			goodsItem = nctsBill.GoodsItems.AddNew();
			wrapper = GetWrapper(goodsItem);
		}

		NctsDepartureCargoDesc goodsItem;
		NctsBill nctsBill;
		NctsHeader nctsHeader;
		NCTS5CommonConsignmentItemWrapper wrapper;

		NCTS5CommonConsignmentItemWrapper GetWrapper(NctsDepartureCargoDesc item) => new NCTS5CommonConsignmentItemWrapper(item);

		protected override NCTS5CommonConsignmentItemWrapper GetProvider() => wrapper;
	}
}
