using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperFromCusEntryHeader))]
	sealed class CustomsEntryWrapperFromCusEntryHeaderTest : CustomsEntryWrapperTest
	{
		public override void TestWrapperMappingFull()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "34345678";
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			entryHeader.CH_MessageType = "ZXZ";
			entryHeader.CH_BGMReference = "INFO";
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2009, 1, 1);
			var wrapperFull = new CustomsEntryWrapperFromCusEntryHeader(entryHeader, Factory);
			AssertEquals("wrapperFull.ToString()", "34345678", wrapperFull.ToString());
			AssertEquals("wrapperFull.EntryNumber", "34345678", wrapperFull.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "ZXZ", wrapperFull.EntryType.Code);
			AssertEquals("wrapperFull.EntryType.Description", "ZXZ", wrapperFull.EntryType.Description);
			AssertEquals("wrapperFull.EntryType.CodeAndDescription", "ZXZ", wrapperFull.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", "CUS", wrapperFull.EntryCategory);
			AssertEquals("wrapperFull.Information", "INFO", wrapperFull.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 1, 1), wrapperFull.IssueDate);

			var entryHeader2 = Factory.New<CusEntryHeader>();
			entryHeader2.EntryNumber = "11111111";
			entryHeader2.CusEntryNumber.CE_Category = "CUS";
			entryHeader2.CH_MessageType = "MSC";
			entryHeader2.CH_BGMReference = "INFO";
			entryHeader2.CH_EntryReleaseDate = new ZDateTime(2009, 1, 1);
			var wrapperFull2 = new CustomsEntryWrapperFromCusEntryHeader(entryHeader2, Factory);
			AssertEquals("wrapperFull.ToString()", "11111111", wrapperFull2.ToString());
			AssertEquals("wrapperFull.EntryNumber", "11111111", wrapperFull2.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "MSC", wrapperFull2.EntryType.Code);
			AssertEquals("wrapperFull.EntryType.Description", "Miscellaneous Customs", wrapperFull2.EntryType.Description);
			AssertEquals("wrapperFull.EntryType.CodeAndDescription", "MSC - Miscellaneous Customs", wrapperFull2.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", "CUS", wrapperFull2.EntryCategory);
			AssertEquals("wrapperFull.Information", "INFO", wrapperFull2.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 1, 1), wrapperFull2.IssueDate);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CustomsEntryWrapperFromCusEntryHeader(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_MessageType = "ZXZ";
			return new CustomsEntryWrapperFromCusEntryHeader(entryHeader, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
EntryType : ZXZ
Registry : (No Default Field Value Available on Registry)
";
			}
		}
	}
}
