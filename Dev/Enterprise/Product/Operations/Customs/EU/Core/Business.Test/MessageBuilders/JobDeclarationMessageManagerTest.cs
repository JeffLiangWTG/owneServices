using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.MessageBuilders.Testing
{
	public class JobDeclarationMessageManagerTest : TestCaseWithFactory
	{
		public void TestCustomsCommencedLoggedWhenSendingMessageExport()
		{
			TestCustomsCommencedShared("EXP", Events.ExportCustomsCommenced);
		}

		public void TestCustomsCommencedLoggedWhenSendingMessageImport()
		{
			TestCustomsCommencedShared("IMP", Events.CustomsCommenced);
		}

		void TestCustomsCommencedShared(string messageDirection, Event eventCode)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = messageDirection;
			var manager = new JobDeclarationMessageManager(dec, null);
			manager.DeclareDeclaration(new SendsMessagesToCustomsShutterUpperer());

			// Let us assert that the event is written to DB, not just to factory.  
			var dbQuery = new ZDBOnlyQuery(typeof(StmALog));
			dbQuery.AddToFilter(StmALogSchema.SL_Parent, dec.PK);
			dbQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode.Code);
			AssertEquals("Customs commenced event must be saved to DB, not just written to factory, so as to remove risk of user rejecting save and losing new event", 1, Factory.Load<StmALog>(dbQuery).Length);
		}
	}
}
