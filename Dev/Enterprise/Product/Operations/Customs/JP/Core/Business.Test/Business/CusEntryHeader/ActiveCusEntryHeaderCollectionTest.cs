using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			return new ActiveCusEntryHeaderCollection((JobDeclaration)Declaration);
		}

		public override void TestAreAllEntriesCleared()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "Ref1";
			newFactory.Save();

			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			entry1 = Factory.Load<CusEntryHeader>(entry1.PK);
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry1.CH_EntryStatus = "XXX";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry1.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "Ref2";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry2.CH_EntryStatus = "XXX";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry2.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
		}
	}
}
