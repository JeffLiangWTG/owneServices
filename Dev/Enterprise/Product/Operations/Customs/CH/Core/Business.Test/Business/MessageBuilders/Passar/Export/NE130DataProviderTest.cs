using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE130DataProvider))]
sealed class NE130DataProviderTest : BasePassarMessageDataProviderTest<NE130DataProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NE130DataProvider(null));
	}

	public new void TestMessageProperties()
	{
		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123456");

		CombineAssertions(() =>
		{
			AssertNull("CorrelationIdentifier is not used", DataProvider.CorrelationIdentifier);
			AssertEquals("MessageSender", "123456", DataProvider.MessageSender);
		});
	}

	public void TestExportOperation()
	{
		AssertNotNull(DataProvider.ExportOperation);
		AssertSame("cached", DataProvider.ExportOperation, DataProvider.ExportOperation);
	}

	public void TestTraderAtDeparture()
	{
		AssertNotNull(DataProvider.TraderAtDeparture);
		AssertSame("cached", DataProvider.TraderAtDeparture, DataProvider.TraderAtDeparture);
	}

	public void TestEdecSelectionAndTransit()
	{
		AssertNotNull(DataProvider.EdecSAT);
		AssertSame("cached", DataProvider.EdecSAT, DataProvider.EdecSAT);
	}

	protected override NE130DataProvider CreateDataProvider() => new NE130DataProvider(SendingObject);
}
