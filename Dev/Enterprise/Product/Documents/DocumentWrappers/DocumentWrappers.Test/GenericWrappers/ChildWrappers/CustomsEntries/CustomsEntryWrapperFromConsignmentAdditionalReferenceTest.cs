using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.TransportConsignment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperForConsignmentAdditionalReference))]
	sealed class CustomsEntryWrapperFromConsignmentAdditionalReferenceTest : CustomsEntryWrapperTest
	{
		public override void TestEntryCategory()
		{
			CustomsEntryWrapper wrapperEmpty = (CustomsEntryWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.EntryCategory", "CUS", wrapperEmpty.EntryCategory);
		}

		public override void TestWrapperMappingFull()
		{
			var consignmentBO = Factory.New<DtbBookingConsignment>();
			CusEntryNumber entryNumber = (CusEntryNumber)consignmentBO.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryType = "BPR";
			entryNumber.CE_EntryNum = "34345678";
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryLineReference = "Info";
			entryNumber.CE_IssueDate = new ZDateTime(2009, 2, 2);

			var wrapperFull = new CustomsEntryWrapperForConsignmentAdditionalReference(entryNumber, Factory);
			AssertEquals("wrapperFull.ToString()", "34345678", wrapperFull.ToString());
			AssertEquals("wrapperFull.EntryNumber", "34345678", wrapperFull.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "Booking Party Reference", wrapperFull.EntryType.Code);
			AssertEquals("wrapperFull.EntryCategory", "CUS", wrapperFull.EntryCategory);
			AssertEquals("wrapperFull.Information", "Info", wrapperFull.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 2, 2), wrapperFull.IssueDate);
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
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "TST";
			return new CustomsEntryWrapperFromCusEntryNumber(entryNumber, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CustomsEntryWrapperForConsignmentAdditionalReference(null, Factory);
		}
	}
}
