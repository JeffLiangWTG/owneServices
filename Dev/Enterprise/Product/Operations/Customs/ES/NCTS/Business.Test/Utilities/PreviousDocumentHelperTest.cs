using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class PreviousDocumentHelperTest : TestCaseWithFactory
	{
		public void TestGetReferenceNumberToSendNctsDeparture()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var doc = SetNctsDocument("X", "SUM", "99997000002", 0, goodsItem);
				AssertEquals("For previous documents not Z reference number must not have type as prefix", "99997000002", PreviousDocumentHelper.GetReferenceNumberToSendNctsDeparture(doc));

				doc = SetNctsDocument("Z", "SUM", "99997000123", 0, goodsItem);
				AssertEquals("FFor previous documents Z reference number must have type as prefix", "SUM99997000123", PreviousDocumentHelper.GetReferenceNumberToSendNctsDeparture(doc));

				doc = SetNctsDocument("X", "SUM", "99997000456", 5, goodsItem);
				AssertEquals("For non AAE previous documents reference number must not have line number attached", "99997000456", PreviousDocumentHelper.GetReferenceNumberToSendNctsDeparture(doc));

				doc = SetNctsDocument("X", "DUA", "99997000456", 5, goodsItem);
				AssertEquals("For AAE previous documents reference number must have line number attached", "99997000456005", PreviousDocumentHelper.GetReferenceNumberToSendNctsDeparture(doc));

				doc = SetNctsDocument("Z", "DUA", "99997000456", 5, goodsItem);
				AssertEquals("For AAE previous documents that are also Z reference number must have type as prefix and line number attached", "DUA99997000456005", PreviousDocumentHelper.GetReferenceNumberToSendNctsDeparture(doc));
			});
		}

		NctsPreviousDocument SetNctsDocument(ZString subType, ZString code, ZString reference, ZShort lineNo, NctsDepartureCargoDesc goodsItemParent)
		{
			var doc = Factory.New<NctsPreviousDocument>();
			doc.CSI_ParentTableCode = goodsItemParent.TablePrefix;
			doc.CSI_ParentID = goodsItemParent.PK;
			doc.CSI_SubType = subType;
			doc.CSI_Code = code;
			doc.CSI_ReferenceNumber = reference;
			doc.CSI_LineNo = lineNo;
			return doc;
		}
	}
}

