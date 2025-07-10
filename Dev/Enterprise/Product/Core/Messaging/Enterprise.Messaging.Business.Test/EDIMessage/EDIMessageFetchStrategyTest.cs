using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.FetchStrategy.Testing
{
	public class EDIMessageFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			EDIInterchange ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_BodyText = ediInterchange.PK.ToString();

			EDIMessage ediMessage1 = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage1.EM_ApplicationReference = "EDIMessageFetchStrategyTest";
			ediMessage1.EM_EI = ediInterchange.PK;

			EDIMessage ediMessage2 = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage2.EM_ApplicationReference = "EDIMessageFetchStrategyTest";
			ediMessage2.EM_EI = ediInterchange.PK;

			EDIMessageAttach attach1 = ediMessage1.MessageAttachments.AddNew();
			attach1.EG_FileName = "dummy1.txt";
			EDIMessageAttach attach2 = ediMessage1.MessageAttachments.AddNew();
			attach2.EG_FileName = "dummy2.txt";

			EDIMessageAttach attach3 = ediMessage2.MessageAttachments.AddNew();
			attach3.EG_FileName = "dummy3.txt";
			EDIMessageAttach attach4 = ediMessage2.MessageAttachments.AddNew();
			attach3.EG_FileName = "dummy4.txt";

			EDIInterchange unRelatedEDIInterchange = Factory.NewWithValidTestData<EDIInterchange>();

			Factory.Save();

			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();

			ediMessage1 = cleanFactory.Load<EDIMessage>(ediMessage1.PK);
			unRelatedEDIInterchange = cleanFactory.Load<EDIInterchange>(unRelatedEDIInterchange.PK);

			ZQuery ediInterchangeFilter = new ZQuery(EDIInterchangeSchema.PK, ediInterchange.PK);
			ediInterchangeFilter.FetchOnlyFromLocalCache = true;

			ediInterchange = cleanFactory.LoadTop1<EDIInterchange>(ediInterchangeFilter);
			AssertEquals(ediInterchange.PK.ToString(), ediInterchange.EI_BodyText);

			cleanFactory = new BusinessObjectFactory();
			int dbHits = cleanFactory.DatabaseLoadCount;
			ZQuery ediMessageFilter = new ZQuery(EDIMessageSchema.EM_EI, ediInterchange.PK);
			EDIMessage[] messages = cleanFactory.Load<EDIMessage>(ediMessageFilter);
			AssertEquals(dbHits + 1, cleanFactory.DatabaseLoadCount);
			AssertEquals(2, messages[0].MessageAttachments.Count);
			AssertEquals(dbHits + 2, cleanFactory.DatabaseLoadCount);
			AssertEquals(2, messages[1].MessageAttachments.Count);
			AssertEquals(dbHits + 2, cleanFactory.DatabaseLoadCount);
		}
	}
}
