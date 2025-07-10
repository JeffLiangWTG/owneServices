using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEntryLineCollection))]
	class ExportEntryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExportEntryLineCollection>
	{
		public void TestConstructor()
		{
			_ = entryHeader.MergedLines.AddNew();
			_ = entryHeader.MergedLines.AddNew();
			collection = new ExportEntryLineCollection(entryHeader);
			AssertEquals(2, collection.Count);
		}

		public void TestConstructorNull() => AssertExceptionThrown<ArgumentNullException>(() => new ExportEntryLineCollection(null));

		public void TestAllowNew() => AssertEquals(false, collection.AllowNew);

		public void TestAllowRemove() => AssertEquals(false, collection.AllowRemove);

		protected override Type GetExpectedCollectionType() => typeof(ExportEntryLineCollection);

		protected override ExportEntryLineCollection GetCollectionToTest() => collection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExportEntryLine(Factory.New<CusEntryLine>());

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = Factory.New<CusEntryHeader>();
			collection = new ExportEntryLineCollection(entryHeader);
		}
		CusEntryHeader entryHeader;
		ExportEntryLineCollection collection;
	}
}
