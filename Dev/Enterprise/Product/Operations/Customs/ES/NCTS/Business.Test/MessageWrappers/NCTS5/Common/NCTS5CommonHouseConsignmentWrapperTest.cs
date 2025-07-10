using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonHouseConsignmentWrapperTest : WrapperHelperTest<NCTS5CommonHouseConsignmentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if consignment is null", typeof(System.ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "houseConsignment"), () => new NCTS5CommonHouseConsignmentWrapper(null));
		}

		public void TestGrossMass()
		{
			CombineAssertions(() =>
			{
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItem1.BY_GrossWeight = 200.4455m;
				nctsBill.B0_Weight = 202.7666666m;
				nctsBill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
					AssertEquals("TransitionalPeriod: Expected filled GrossMass 3 decimals with grossMass from bill and not the sum of the item's gross masses", 202.767m, wrapper.GrossMass);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
					AssertEquals("FinalPeriod: Expected filled GrossMass 6 decimals with grossMass from bill and not the sum of the item's gross masses", 202.766667m, wrapper.GrossMass);
				}
			});
		}

		public void TestSequenceNumberDeparture()
		{
			AssertEquals("Expected filled SequenceNumber with value in nctsBill.SequenceNumber", nctsBill.SequenceNumber.ToString(), wrapper.SequenceNumber);
		}

		public void TestSequenceNumberArrival()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var nctsBill = nctsHeader.Bills.AddNew();
				nctsBill.MovementDetail.B9_SeqNo = "5";
				var wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);

				AssertEquals("nctsBill.SequenceNumber is 1 for arrival", (ZShort)1, nctsBill.SequenceNumber);
				AssertEquals("Expected filled SequenceNumber with value in nctsBill.MovementDetail.B9_SeqNo", "5", wrapper.SequenceNumber);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty TransportDocument list", 0, wrapper.TransportDocument.Count);

					var addInfo1 = nctsBill.AdditionalDocuments.AddNew();
					addInfo1.CSI_Code = "9001";
					addInfo1.CSI_SubType = "TRA";
					addInfo1.CSI_LineNo = 3;

					var addInfo1INF = nctsBill.AdditionalDocuments.AddNew();
					addInfo1INF.CSI_Code = "Y001";
					addInfo1INF.CSI_SubType = "INF";
					addInfo1INF.CSI_LineNo = 1;

					var addInfo2 = nctsBill.AdditionalDocuments.AddNew();
					addInfo2.CSI_Code = "9002";
					addInfo2.CSI_SubType = "TRA";
					addInfo2.CSI_LineNo = 2;

					var addInfo2REF = nctsBill.AdditionalDocuments.AddNew();
					addInfo2REF.CSI_Code = "Y002";
					addInfo2REF.CSI_SubType = "REF";
					addInfo2REF.CSI_LineNo = 2;

					var addInfo3 = nctsBill.AdditionalDocuments.AddNew();
					addInfo3.CSI_Code = "9003";
					addInfo3.CSI_SubType = "TRA";
					addInfo3.CSI_LineNo = 1;

					var addInfo4 = nctsBill.AdditionalDocuments.AddNew();
					addInfo4.CSI_Code = "9004";
					addInfo4.CSI_SubType = "TRA";
					addInfo4.CSI_LineNo = 6;

					var addInfo5 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo5.CSI_Code = "5005";
					addInfo5.CSI_SubType = "TRA";
					addInfo5.CSI_LineNo = 1;

					var addInfo6 = nctsBill.GoodsItems.AddNew().AdditionalInfos.AddNew();
					addInfo6.CSI_Code = "5006";
					addInfo6.CSI_SubType = "TRA";
					addInfo6.CSI_LineNo = 1;

					wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
					var documents = wrapper.TransportDocument;

					AssertEquals("FinalPeriod: Expected filled TransportDocument (Included those that have subType TRA)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled TransportDocument ordered Name", new ZString[] { "9003", "9002", "9001", "9004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled TransportDocument ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertSame("FinalPeriod: Cached TransportDocument", wrapper.TransportDocument, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
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

					var addInfo1 = nctsBill.AdditionalDocuments.AddNew();
					addInfo1.CSI_Code = "9001";
					addInfo1.CSI_SubType = "TRA";
					addInfo1.CSI_LineNo = 1;

					var addInfo1REF = nctsBill.AdditionalDocuments.AddNew();
					addInfo1REF.CSI_Code = "Y001";
					addInfo1REF.CSI_SubType = "REF";
					addInfo1REF.CSI_LineNo = 3;

					var addInfo2 = nctsBill.AdditionalDocuments.AddNew();
					addInfo2.CSI_Code = "9002";
					addInfo2.CSI_SubType = "INF";
					addInfo2.CSI_LineNo = 2;

					var addInfo2REF = nctsBill.AdditionalDocuments.AddNew();
					addInfo2REF.CSI_Code = "Y002";
					addInfo2REF.CSI_SubType = "REF";
					addInfo2REF.CSI_LineNo = 2;

					var addInfo3REF = nctsBill.AdditionalDocuments.AddNew();
					addInfo3REF.CSI_Code = "Y003";
					addInfo3REF.CSI_SubType = "REF";
					addInfo3REF.CSI_LineNo = 1;

					var addInfo4REF = nctsBill.AdditionalDocuments.AddNew();
					addInfo4REF.CSI_Code = "Y004";
					addInfo4REF.CSI_SubType = "REF";
					addInfo4REF.CSI_LineNo = 6;

					var addInfo5 = nctsHeader.AdditionalDocuments.AddNew();
					addInfo5.CSI_Code = "5005";
					addInfo5.CSI_SubType = "REF";
					addInfo5.CSI_LineNo = 1;

					var addInfo6 = nctsBill.GoodsItems.AddNew().AdditionalInfos.AddNew();
					addInfo6.CSI_Code = "5006";
					addInfo6.CSI_SubType = "REF";
					addInfo6.CSI_LineNo = 1;

					wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
					var documents = wrapper.AdditionalReference;

					AssertEquals("FinalPeriod: Expected filled AdditionalReference (Included those that have subType REF)", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered Name", new ZString[] { "Y003", "Y002", "Y001", "Y004" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInformation ordered SequenceNumber", new ZString[] { "1", "2", "3", "6" }, documents.Select(x => x.SequenceNumber));
					AssertSame("FinalPeriod: Cached AdditionalReference", wrapper.AdditionalReference, documents);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
					var documents = wrapper.AdditionalReference;

					AssertEquals("TransitionalPeriod: Expected not filled AdditionalReference", 0, documents.Count);
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
			wrapper = new NCTS5CommonHouseConsignmentWrapper(nctsBill);
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NCTS5CommonHouseConsignmentWrapper wrapper;

		protected override NCTS5CommonHouseConsignmentWrapper GetProvider() => wrapper;
	}
}
