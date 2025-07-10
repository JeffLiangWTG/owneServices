using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class MessageProviderHelperTest : TestCaseWithFactory
	{
		public void TestStatusIsNewOrMissing()
		{
			CombineAssertions(() =>
			{
				Assert("Status New in upper case", MessageProviderHelper.StatusIsNewOrMissing("new"));
				Assert("Status NEW in lower case", MessageProviderHelper.StatusIsNewOrMissing("NEW"));
				Assert("Status Missing in lower case", MessageProviderHelper.StatusIsNewOrMissing("mis"));
				Assert("Status Missing in upper case", MessageProviderHelper.StatusIsNewOrMissing("MIS"));
				Assert("Other Status", !MessageProviderHelper.StatusIsNewOrMissing("DIF"));
			});
		}

		public void TestGetRegCodeFromCustomsCodes()
		{
			SetPrincipal("0123456789000");
			CombineAssertions(() =>
			{
				var organisation = nctsHeader.Principal.Organisation;
				AssertEquals("Code type = EOR", "IE0123456789000", MessageProviderHelper.GetRegCodeFromCustomsCodes(organisation));

				var codes = organisation.CustomsCodes.Find((OrgCusCode x) => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var code = codes.First();
				code.OK_CodeType = "XXX";
				AssertNull("Should be null when code type does not match EOR or TCU", MessageProviderHelper.GetRegCodeFromCustomsCodes(organisation));

				code.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
				AssertEquals("Code type = TCU", "IE0123456789000", MessageProviderHelper.GetRegCodeFromCustomsCodes(organisation));
			});
		}

		public void TestFindAdditionalDocuments_IsOrdered()
		{
			var additionalDocument1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_SubType = "INF";
			additionalDocument1.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 20);
			var additionalDocument2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_SubType = "INF";
			additionalDocument2.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 18);
			var additionalDocument3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalDocument3.CSI_SubType = "INF";
			additionalDocument3.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 19);

			AssertContainsExactElementsInExactOrder("should order by create time",
				expected: new NctsAdditionalInfo[] { additionalDocument2, additionalDocument3, additionalDocument1 },
				actual: MessageProviderHelper.FindAdditionalDocuments(nctsHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation)
				);

			var bill = nctsHeader.Bills.AddNew();
			var billTransportDocument1 = bill.AdditionalDocuments.AddNew();
			billTransportDocument1.CSI_SubType = "TRA";
			billTransportDocument1.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 19);
			var billTransportDocument2 = bill.AdditionalDocuments.AddNew();
			billTransportDocument2.CSI_SubType = "TRA";
			billTransportDocument2.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 18);
			var billTransportDocument3 = bill.AdditionalDocuments.AddNew();
			billTransportDocument3.CSI_SubType = "TRA";
			billTransportDocument3.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 20);

			AssertContainsExactElementsInExactOrder("should order by create time",
				expected: new NctsBillAdditionalDocument[] { billTransportDocument2, billTransportDocument1, billTransportDocument3 },
				actual: MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.TransportDocument)
				);

			var goodsItem = bill.GoodsItems.AddNew();
			var goodsItemAdditionalDocument1 = goodsItem.AdditionalInfos.AddNew();
			goodsItemAdditionalDocument1.CSI_SubType = "TRA";
			goodsItemAdditionalDocument1.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 19);
			var goodsItemAdditionalDocument2 = goodsItem.AdditionalInfos.AddNew();
			goodsItemAdditionalDocument2.CSI_SubType = "TRA";
			goodsItemAdditionalDocument2.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 18);
			var goodsItemAdditionalDocument3 = goodsItem.AdditionalInfos.AddNew();
			goodsItemAdditionalDocument3.CSI_SubType = "TRA";
			goodsItemAdditionalDocument3.CSI_SystemCreateTimeUtc = new ZDateTime(2023, 4, 20);

			AssertContainsExactElementsInExactOrder("should order by create time",
				expected: new NctsAdditionalInfo[] { goodsItemAdditionalDocument2, goodsItemAdditionalDocument1, goodsItemAdditionalDocument3 },
				actual: MessageProviderHelper.FindAdditionalDocuments(goodsItem, AdditionalInfoSubTypeList.Codes.TransportDocument)
				);
		}

		public void TestFindAdditionalDocuments()
		{
			var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = "INF";
			var transportDocument = nctsHeader.AdditionalDocuments.AddNew();
			transportDocument.CSI_SubType = "TRA";
			var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
			additionalReference.CSI_SubType = "REF";

			var bill = nctsHeader.Bills.AddNew();
			var billAdditionalDocument = bill.AdditionalDocuments.AddNew();
			billAdditionalDocument.CSI_SubType = "INF";
			var billTransportDocument = bill.AdditionalDocuments.AddNew();
			billTransportDocument.CSI_SubType = "TRA";
			var billAdditionalReference = bill.AdditionalDocuments.AddNew();
			billAdditionalReference.CSI_SubType = "REF";

			var goodsItem = bill.GoodsItems.AddNew();
			var goodsItemAdditionalDocument = goodsItem.AdditionalInfos.AddNew();
			goodsItemAdditionalDocument.CSI_SubType = "INF";
			var goodsItemTransportDocument = goodsItem.AdditionalInfos.AddNew();
			goodsItemTransportDocument.CSI_SubType = "TRA";
			var goodsItemAdditionalReference = goodsItem.AdditionalInfos.AddNew();
			goodsItemAdditionalReference.CSI_SubType = "REF";

			CombineAssertions(() =>
			{
				AssertSame("Find REF", additionalReference, MessageProviderHelper.FindAdditionalDocuments(nctsHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference).FirstOrDefault());
				AssertSame("Find TRA", transportDocument, MessageProviderHelper.FindAdditionalDocuments(nctsHeader, AdditionalInfoSubTypeList.Codes.TransportDocument).FirstOrDefault());
				AssertSame("Find INF", additionalDocument, MessageProviderHelper.FindAdditionalDocuments(nctsHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation).FirstOrDefault());
				AssertSame("Find REF", billAdditionalReference, MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.AdditionalReference).FirstOrDefault());
				AssertSame("Find TRA", billTransportDocument, MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.TransportDocument).FirstOrDefault());
				AssertSame("Find INF", billAdditionalDocument, MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.AdditionalInformation).FirstOrDefault());
				AssertSame("Find REF", goodsItemAdditionalReference, MessageProviderHelper.FindAdditionalDocuments(goodsItem, AdditionalInfoSubTypeList.Codes.AdditionalReference).FirstOrDefault());
				AssertSame("Find TRA", goodsItemTransportDocument, MessageProviderHelper.FindAdditionalDocuments(goodsItem, AdditionalInfoSubTypeList.Codes.TransportDocument).FirstOrDefault());
				AssertSame("Find INF", goodsItemAdditionalDocument, MessageProviderHelper.FindAdditionalDocuments(goodsItem, AdditionalInfoSubTypeList.Codes.AdditionalInformation).FirstOrDefault());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;

		void SetPrincipal(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}
	}
}
