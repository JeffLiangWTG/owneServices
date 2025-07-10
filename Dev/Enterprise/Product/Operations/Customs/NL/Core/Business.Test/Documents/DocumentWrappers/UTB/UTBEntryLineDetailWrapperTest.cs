using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(UTBEntryLineDetailWrapper))]
class UTBEntryLineDetailWrapperTest : NonPersistentBusinessObjectTestCase
{
	BusinessObjectCollectionWrapper<UTBEntryLineDetailWrapper> entryLineDetails;
	CusEntryHeader entryHeader;
	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = DocumentWrapperTestHelper.GetEntryHeaderForTest(new BusinessObjectFactory());
		entryLineDetails = new UTBDocumentWrapper(entryHeader).CusEntryLineDetails;
	}
	public void TestLineDetails()
	{
		AssertEquals("Amount detail lines", 3, entryLineDetails.Count);

		var entryLine1 = entryLineDetails[0];
		var entryLine2 = entryLineDetails[1];
		var entryLine3 = entryLineDetails[2];

		CombineAssertions(() =>
		{
			AssertEquals("LineDetails - linenumber line 1", 1, entryLine1.ArticleSequenceNumber);
			AssertEquals("LineDetails - linenumber line 2", 2, entryLine2.ArticleSequenceNumber);
			AssertEquals("LineDetails - linenumber line 3", 3, entryLine3.ArticleSequenceNumber);

			AssertEquals("LineDetails - MRN line 1", "MRN1234567890", entryLine1.MovementReferenceNumber);
			AssertEquals("LineDetails - MRN line 2", "MRN1234567890", entryLine2.MovementReferenceNumber);
			AssertEquals("LineDetails - MRN line 3", "MRN1234567890", entryLine3.MovementReferenceNumber);

			AssertEquals("LineDetails - TotalDuties line 1", 96m + 24m, entryLine1.TotalAmountDuties);
			AssertEquals("LineDetails - TotalDuties line 2", 48m, entryLine2.TotalAmountDuties);
			AssertEquals("LineDetails - TotalDuties line 3", 1020m, entryLine3.TotalAmountDuties);

			AssertEquals("LineDetails - TotalTax line 1", 252m, entryLine1.TotalAmountTax);
			AssertEquals("LineDetails - TotalTax line 2", 16800m, entryLine2.TotalAmountTax);
			AssertEquals("LineDetails - TotalTax line 3", 3570m, entryLine3.TotalAmountTax);

			AssertEquals("LineDetails - TotalDutiesAndTax line 1", 96m + 24m, entryLine1.TotalAmountDutiesAndTax);
			AssertEquals("LineDetails - TotalDutiesAndTax line 2", 48m, entryLine2.TotalAmountDutiesAndTax);
			AssertEquals("LineDetails - TotalDutiesAndTax line 3", 1020m, entryLine3.TotalAmountDutiesAndTax);

			AssertEquals("LineDetails Fees on line 1", 3, entryLine1.FeeSummary.Count);
			AssertEquals("LineDetails Fees on line 2", 2, entryLine2.FeeSummary.Count);
			AssertEquals("LineDetails Fees on line 3", 2, entryLine3.FeeSummary.Count);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => entryLineDetails[0];
}
