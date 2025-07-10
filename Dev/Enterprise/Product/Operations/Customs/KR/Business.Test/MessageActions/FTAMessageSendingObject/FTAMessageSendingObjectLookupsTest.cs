using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class FTAMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFTARelationArticleCodeList()
		{
			AssertEquals("1, 2, 3", sendingObject.Lookups.FTARelationArticleCodeList.CodesAsString);
		}

		public void TestYNCodeList()
		{
			AssertEquals("N, Y", sendingObject.Lookups.YNCodeList.CodesAsString);
		}

		public void TestOtherList()
		{
			AssertType<ConsignorCollection>("ConsignorList", sendingObject.Lookups.ConsignorList);
			AssertType<ConsigneeCollection>("ConsigneeList", sendingObject.Lookups.ConsigneeList);
			AssertType<RefUNLOCOCollection>("PortOfLoadings", sendingObject.Lookups.PortOfLoadings);
			AssertType<RefCountryCollection>("Countries", sendingObject.Lookups.Countries);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5SC;

			sendingObject = new FTAMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SC);
		}
		FTAMessageSendingObject sendingObject;
	}
}
