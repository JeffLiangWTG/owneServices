using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryLineDetailsFor5ULFilterStripBusinessObject))]
	sealed class EntryLineDetailsFor5ULFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryLineDetailsFor5ULFilterStripBusinessObject();

		public void TestFilter()
		{
			var filter = new EntryLineDetailsFor5ULFilterStripBusinessObject();
			AssertNotNull(filter[EntryLineDetailsFor5ULFilterStripBusinessObject.Schema.LineNum]);
		}

		public void TestLineNum()
		{
			var filter = new EntryLineDetailsFor5ULFilterStripBusinessObject();
			var coll = new KREntryLineDetailsViewCollection(Factory, "1234522123450X");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(3, coll.Count);
			AssertEquals("1234522123450X", coll.Cast<KREntryLineDetailsView>().First().KEL_EntryNum);
			Assert(coll.Cast<KREntryLineDetailsView>().Any(x => x.KEL_LineNumber == 1));
			Assert(coll.Cast<KREntryLineDetailsView>().Any(x => x.KEL_LineNumber == 2));
			Assert(coll.Cast<KREntryLineDetailsView>().Any(x => x.KEL_LineNumber == 3));

			var lineNumFilter = (ModuleNumberFilter)filter[EntryLineDetailsFor5ULFilterStripBusinessObject.Schema.LineNum];
			lineNumFilter.Property = "2";
			lineNumFilter.IsActive = true;

			coll = new KREntryLineDetailsViewCollection(Factory, "1234522123451X");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123451X", coll.Cast<KREntryLineDetailsView>().First().KEL_EntryNum);
			AssertEquals("2", coll.Cast<KREntryLineDetailsView>().First().KEL_LineNumber.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entry = CreateEntry("1234522123450X");
			CreateEntryLine(entry, 1);
			CreateEntryLine(entry, 2);
			CreateEntryLine(entry, 3);

			var entry2 = CreateEntry("1234522123451X");
			CreateEntryLine(entry2, 2);
			Factory.Save();

			CusEntryHeader CreateEntry(ZString entryNum)
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = "IMP";
				declaration1.JE_ApplicationCode = "BLT";

				var entry = declaration1.CustomsEntryHeaders.AddNew();
				var entryNumber = entry.EntryNumbers.AddNew();
				entryNumber.CE_EntryType = "IMP";
				entryNumber.CE_EntryNum = entryNum;

				return entry;
			}
			void CreateEntryLine(CusEntryHeader entry, ZShort lineNum)
			{
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = lineNum;
			}
		}
	}
}
