using System.Collections.Generic;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.Testing;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

sealed class FRDocSADHPageTest : DocSADHPageTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return DocSADHPage.New(Factory, null, false, true);
	}

	public void TestShowSecondBox44()
	{
		var page = DocSADHPage.New(Factory, null, false, true);
		Assert(!page.ShowEnlargedBox44);

		page = DocSADHPage.New(Factory, null, true, true);
		Assert(page.ShowEnlargedBox44);
	}

	public void TestNewWithSingleEntryLine()
	{
		var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
		var pageWrapper = DocSADHPage.New(Factory, entryLine);
		AssertNotNull("Line1 should be created based on entryLine.", pageWrapper.Line1);
		AssertNull("Line2 should be null as only one entryLine taken in.", pageWrapper.Line2);
		AssertNull("Line3 should be null as only one entryLine taken in.", pageWrapper.Line3);
	}

	public void TestNewWithTripleEntryLine()
	{
		var entryLine1 = Factory.NewWithValidTestData<CusEntryLine>();
		var entryLine2 = Factory.NewWithValidTestData<CusEntryLine>();
		var pageWrapper = DocSADHPage.New(Factory, new List<CusEntryLine> { entryLine1, entryLine2, null });
		AssertNotNull("Line1 should be created based on entryLine1.", pageWrapper.Line1);
		AssertNotNull("Line2 should be created based on entryLine2.", pageWrapper.Line2);
		AssertNull("Line3 should be null as it accepts an null.", pageWrapper.Line3);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.France);
	}
}
