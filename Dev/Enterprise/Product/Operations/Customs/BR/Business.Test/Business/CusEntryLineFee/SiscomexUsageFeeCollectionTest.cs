using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SiscomexUsageFeeCollection))]
	class SiscomexUsageFeeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader1CDI = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1CDI.CH_MessageType = MessageTypeList.Codes.CDI;
			entryHeader1CDI.CH_CEI_Instruction = instruction1.PK;

			var entryHeader1SUF = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1SUF.CH_MessageType = MessageTypeList.Codes.SUF;
			entryHeader1SUF.CH_CEI_Instruction = instruction1.PK;

			var entryLine1SUF = entryHeader1SUF.MergedLines.AddNew();
			var fee1 = entryLine1SUF.Fees.AddOrUpdate("SUF", 10.0m);
			var collection = new SiscomexUsageFeeCollection(entryHeader1CDI);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { fee1 }, collection);

			var entryHeader2SUF = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2SUF.CH_MessageType = MessageTypeList.Codes.SUF;
			entryHeader2SUF.CH_CEI_Instruction = instruction2.PK;

			var entryLine2SUF = entryHeader2SUF.MergedLines.AddNew();
			entryLine2SUF.Fees.AddOrUpdate("SUF", 10.0m);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { fee1 }, collection);

			var entryLine3SUF = entryHeader1SUF.MergedLines.AddNew();
			var fee3 = entryLine3SUF.Fees.AddOrUpdate("SUF", 10.0m);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { fee1, fee3 }, collection);

			collection = new SiscomexUsageFeeCollection(entryHeader1SUF);
			collection.Load();
			AssertEquals(0, collection.Count);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var entryHeaderInDatabase = factory.Load<CusEntryHeader>(entryHeader1CDI.PK);
			collection = new SiscomexUsageFeeCollection(entryHeaderInDatabase);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { fee1.PK, fee3.PK }, collection.Select(x => x.PK));
		}

		public void TestReadOnly()
		{
			var collection = new SiscomexUsageFeeCollection(Factory.NewWithValidTestData<CusEntryHeader>());
			Assert(collection.ReadOnly);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			return new SiscomexUsageFeeCollection(entryHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusEntryLineFee>();
		}
	}
}
