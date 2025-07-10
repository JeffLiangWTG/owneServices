using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperFromWarehouseReference))]
	sealed class CustomsEntryWrapperFromWarehouseReferenceTest : CustomsEntryWrapperTest
	{
		public override void TestEntryCategory()
		{
			CustomsEntryWrapperFromWarehouseReference wrapperEmpty = (CustomsEntryWrapperFromWarehouseReference)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.EntryCategory", "", wrapperEmpty.EntryCategory);
		}

		public override void TestWrapperMappingFull()
		{
			WhsDocketReference entryNumber = Factory.New<WhsDocketReference>();
			entryNumber.WX_RefType = "INV";
			entryNumber.WX_Reference = "34345678";

			var wrapperFull = new CustomsEntryWrapperFromWarehouseReference(entryNumber, Factory);
			AssertEquals("wrapperFull.ToString()", "34345678", wrapperFull.ToString());
			AssertEquals("wrapperFull.EntryNumber", "34345678", wrapperFull.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "INV", wrapperFull.EntryType.Code);
			AssertEquals("wrapperFull.EntryCategory", "", wrapperFull.EntryCategory);
			AssertEquals("wrapperFull.Information", "", wrapperFull.Information);
			AssertEquals("wrapperFull.IssueDate", ZDateTime.Empty, wrapperFull.IssueDate);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
EntryType : TST
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			WhsDocketReference entryNumber = Factory.New<WhsDocketReference>();
			entryNumber.WX_RefType = "TST";
			return new CustomsEntryWrapperFromWarehouseReference(entryNumber, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CustomsEntryWrapperFromWarehouseReference(null, Factory);
		}
	}
}
