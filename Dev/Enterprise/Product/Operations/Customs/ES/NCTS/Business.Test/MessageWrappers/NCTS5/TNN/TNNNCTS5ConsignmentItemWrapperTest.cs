using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class TNNNCTS5ConsignmentItemWrapperTest : WrapperHelperTest<TNNNCTS5ConsignmentItemWrapper>
	{
		public void TestCommodity()
		{
			var commodity = wrapper.Commodity;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Commodity", commodity);
				AssertSame("Cached Commodity", wrapper.Commodity, commodity);
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
			nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.GoodsItems.AddNew();
			wrapper = GetWrapper(goodsItem);
		}

		NctsDepartureCargoDesc goodsItem;
		NctsBill nctsBill;
		NctsHeader nctsHeader;
		TNNNCTS5ConsignmentItemWrapper wrapper;

		TNNNCTS5ConsignmentItemWrapper GetWrapper(NctsDepartureCargoDesc item, bool shouldDeclareDeclarationTypeInItem = false, bool shouldDeclareCountryOfDestinationInItem = false, bool shouldDeclareReferenceNumberUCRInItem = false) => new TNNNCTS5ConsignmentItemWrapper(item, shouldDeclareDeclarationTypeInItem, shouldDeclareCountryOfDestinationInItem, shouldDeclareReferenceNumberUCRInItem);

		protected override TNNNCTS5ConsignmentItemWrapper GetProvider() => wrapper;
	}
}
