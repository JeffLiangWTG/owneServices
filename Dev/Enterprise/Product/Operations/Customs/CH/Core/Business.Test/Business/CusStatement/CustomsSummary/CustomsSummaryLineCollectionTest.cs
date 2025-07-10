using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsSummaryLineCollection))]
sealed class CustomsSummaryCollectionLineTest : ActiveBusinessObjectCollectionTestCase<CustomsSummaryLineCollection>
{
	public void TestAllowNew()
	{
		var collection = GetCollectionToTest();
		collection.SetReadOnlyIncludingChildren(false);
		AssertEquals(false, ((IBindingList)collection).AllowNew);
	}

	public void TestCompanyFilter()
	{
		var otherCompany = Factory.New<GlbCompany>();
		var line1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		line1.StatementHeader.B2_GC = GlbCompany.CurrentCompany.PK;
		var line2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		line2.StatementHeader.B2_GC = otherCompany.PK;
		Factory.Save();
		var collection = new CustomsSummaryLineCollection(new BusinessObjectFactory());
		AssertEquals("Count", 1, collection.Count);
		AssertEquals("Line of company", line1.PK, collection[0].PK);
	}

	protected override CustomsSummaryLineCollection GetCollectionToTest()
	{
		var header = (CustomsSummaryHeader)CustomsSummaryTestHelper.CreateSummaryLine(Factory).StatementHeader;
		Factory.Save();
		return new CustomsSummaryLineCollection(header);
	}
}
