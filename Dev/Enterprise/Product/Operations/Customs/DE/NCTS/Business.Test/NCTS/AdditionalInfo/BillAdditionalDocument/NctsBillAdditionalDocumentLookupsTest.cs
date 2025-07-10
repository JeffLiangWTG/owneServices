using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsBillAdditionalDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList()
		{
			CombineAssertions(() =>
			{
				var entryStyleList = lookups.SubTypeList;
				AssertEquals("SubTypeList CodesAsString", "REF", entryStyleList.CodesAsString);
				AssertEquals("REF Description", "Additional Reference", entryStyleList.GetDescriptionFromCode(AdditionalInfoSubTypeList.Codes.AdditionalReference));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
			lookups = new NctsBillAdditionalDocumentLookups(additionalDocument);
		}
		NctsBillAdditionalDocument additionalDocument;
		NctsBillAdditionalDocumentLookups lookups;
	}
}
