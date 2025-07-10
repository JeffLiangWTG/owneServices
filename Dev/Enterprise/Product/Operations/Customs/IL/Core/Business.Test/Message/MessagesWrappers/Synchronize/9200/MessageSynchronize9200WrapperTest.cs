using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.GEN;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class MessageSynchronize9200WrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageSynchronize9200>
	{
		public void TestNewOrNull()
		{
			AssertNull("When listOfCorrelationIDs is empty", MessageSynchronize9200Wrapper.NewOrNull(Enumerable.Empty<ZString>()));
			AssertNotNull("When listOfCorrelationIDs is not empty", Provider);
		}

		public void TestBatchId()
		{
			AssertEquals("", Provider.BatchId);
		}

		public void TestListOfCorrelationIDs()
		{
			AssertEquals(2, Provider.ListOfCorrelationIDs.Count);
			var correlationID1 = Provider.ListOfCorrelationIDs.FirstOrDefault();
			AssertEquals(guid1.ToString(), correlationID1.CorrelationIDs);

			var correlationID2 = Provider.ListOfCorrelationIDs.LastOrDefault();
			AssertEquals(guid2.ToString(), correlationID2.CorrelationIDs);
		}

		public void TestRequestContentHeader()
		{
			AssertNotNull(Provider.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>(Provider.RequestContentHeader);
		}

		protected override IMessageSynchronize9200 GetProvider()
		{
			return MessageSynchronize9200Wrapper.NewOrNull(listOfCorrelationIDs);
		}

		protected override void SetUp()
		{
			base.SetUp();
			guid1 = new ZGuid();
			guid2 = new ZGuid();
			listOfCorrelationIDs = new ZString[] { guid1.ToString(), guid2.ToString() };
		}
		ZGuid guid1;
		ZGuid guid2;
		IEnumerable<ZString> listOfCorrelationIDs;
	}
}
