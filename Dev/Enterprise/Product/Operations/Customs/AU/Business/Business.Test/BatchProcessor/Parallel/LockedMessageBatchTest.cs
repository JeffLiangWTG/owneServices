using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LockedMessageBatchTest : TestCaseWithFactory
	{
		public void TestDisposeTwice()
		{
			int disposeCount = 0;

			var batch = new LockedMessageBatch(System.Array.Empty<ZGuid>(), new DisposableAction(() => disposeCount++));
			AssertEquals(0, disposeCount);

			batch.Dispose();
			AssertEquals(1, disposeCount);

			batch.Dispose();
			AssertEquals(1, disposeCount);
		}

		public void TestLoadMessages()
		{
			var message1 = Factory.New<TestEDIMessage>();
			message1.EM_MessageNum = "0001";
			var message2 = Factory.New<TestEDIMessage>();
			message2.EM_MessageNum = "0002";

			int disposeCount = 0;

			var messageThatNoLongerExists = ZGuid.NewZGuid();
			using (var batch = new LockedMessageBatch(new[] { message1.PK, messageThatNoLongerExists, message2.PK }, new DisposableAction(() => disposeCount++)))
			{
				AssertEquals(0, disposeCount);

				var messages = batch.LoadMessages(Factory);
				AssertArrayEqualsByElements(new ZString[] { "0001", "0002" }, messages.Select(m => m.EM_MessageNum).ToArray());
				AssertEquals("loading should not cause dispose", 0, disposeCount);
			}

			AssertEquals(1, disposeCount);

			// same test reversed order of messages
			using (var batch = new LockedMessageBatch(new[] { message2.PK, messageThatNoLongerExists, message1.PK }, null))
			{
				var messages = batch.LoadMessages(Factory);
				AssertArrayEqualsByElements(new ZString[] { "0002", "0001" }, messages.Select(m => m.EM_MessageNum).ToArray());
			}
		}

		sealed class TestEDIMessage : EDIMessage
		{
			public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum;
		}
	}
}
