using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD.Testing
{
	public class CODWrapperTest : TestCaseWithFactory
	{
		public void TestActionCode()
		{
			AssertEquals("3", wrapper.ActionCode);

			var headerApplicator = new FrCreditCODApplicator(Factory);
			var itemApplicator = headerApplicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			wrapper = new CODWrapper(itemApplicator.ReleasingEntryHeader, "3", items, null);
			AssertEquals("3", wrapper.ActionCode);

			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			wrapper = new CODWrapper(itemApplicator.ReleasingEntryHeader, "1", items, null);
			AssertEquals("1", wrapper.ActionCode);

			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			wrapper = new CODWrapper(itemApplicator.ReleasingEntryHeader, "1", items, null);
			AssertEquals("1", wrapper.ActionCode);
		}

		public void TestFileReference()
		{
			AssertEquals("101", wrapper.FileReference);
		}

		public void TestItems()
		{
			AssertEquals(items, wrapper.Items);
		}

		public void TestGens()
		{
			AssertEquals(gens, wrapper.Gens);
		}

		public void TestMessageEnveloppe()
		{
			AssertType<CODMessageEnvelopWrapper>(wrapper.MessageEnvelope);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_BGMReference = "101";
			items = new List<IArticle>();
			gens = new List<IGen>();
			wrapper = new CODWrapper(entryHeader, "3", items, gens);
		}

		List<IArticle> items;
		List<IGen> gens;
		CODWrapper wrapper;
	}
}
