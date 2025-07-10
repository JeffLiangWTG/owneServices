using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadCustomsStatusInformationProviderTest : TestCaseWithFactory
{
	public void TestImportCustomsStatusWithInformationOrderCollection()
	{
		var expectedValues = new CustomsStatusOrder[]
		{
			new CustomsStatusOrder(ZString.Empty, 0),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Registered, 1),
			new CustomsStatusOrder(ITEntryStatusList.Codes.NbRejected, 2),
			new CustomsStatusOrder(ITEntryStatusList.Codes.UnderControl, 3),
			new CustomsStatusOrder(ITEntryStatusList.Codes.ImportCleared, 5)
		};

		CustomsStatusOrderTestHelper.AssertCollection(expectedValues, SadCustomsStatusInformationProvider.ImportCustomsStatusWithInformationOrderCollection.ToArray());
	}

	public void TestExportCustomsStatusWithInformationOrderCollection()
	{
		var expectedValues = new CustomsStatusOrder[]
		{
			new CustomsStatusOrder(ZString.Empty, 0),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Registered, 1),
			new CustomsStatusOrder(ITEntryStatusList.Codes.NbRejected, 2),
			new CustomsStatusOrder(ITEntryStatusList.Codes.UnderControl, 3),
			new CustomsStatusOrder(ITEntryStatusList.Codes.ExportCleared, 4),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Exit, 5),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Arrival, 6)
		};

		CustomsStatusOrderTestHelper.AssertCollection(expectedValues, SadCustomsStatusInformationProvider.ExportCustomsStatusWithInformationOrderCollection.ToArray());
	}

	public void TestCompareImportCustomsStatusOrder()
	{
		CombineAssertions("CompareImportCustomsStatusOrder", () =>
		{
			AssertEquals("When values are equal", 0, SadCustomsStatusInformationProvider.CompareImportCustomsStatusOrder("REG", "REG"));
			AssertEquals("StatusB order is greater than StatusA", 1, SadCustomsStatusInformationProvider.CompareImportCustomsStatusOrder("REG", "NBR"));
			AssertEquals("StatusB order is minor than StatusA", -1, SadCustomsStatusInformationProvider.CompareImportCustomsStatusOrder("NBR", "REG"));
			AssertEquals("StatusB order is minor when is null", -1, SadCustomsStatusInformationProvider.CompareImportCustomsStatusOrder("REG", "XXX"));
			AssertEquals("StatusB order is greater when StatusA is null", 1, SadCustomsStatusInformationProvider.CompareImportCustomsStatusOrder("XXX", "REG"));
		});
	}

	public void TestCompareExportCustomsStatusOrder()
	{
		CombineAssertions("CompareExportCustomsStatusOrder", () =>
		{
			AssertEquals("When values are equal", 0, SadCustomsStatusInformationProvider.CompareExportCustomsStatusOrder("REG", "REG"));
			AssertEquals("StatusB order is greater than StatusA", 1, SadCustomsStatusInformationProvider.CompareExportCustomsStatusOrder("REG", "EXI"));
			AssertEquals("StatusB order is minor than StatusA", -1, SadCustomsStatusInformationProvider.CompareExportCustomsStatusOrder("EXI", "REG"));
			AssertEquals("StatusB order is minor when is null", -1, SadCustomsStatusInformationProvider.CompareExportCustomsStatusOrder("REG", "XXX"));
			AssertEquals("StatusB order is greater when StatusA is null", 1, SadCustomsStatusInformationProvider.CompareExportCustomsStatusOrder("XXX", "REG"));
		});
	}

	public void TestCompareOrderCustomsStatus()
	{
		var statusWithInformationOrdered = ImmutableArray.Create(new CustomsStatusOrder("A", 0), new CustomsStatusOrder("B", 1));
		CombineAssertions("CompareOrderCustomsStatus", () =>
		{
			AssertEquals("When values are equal", 0, SadCustomsStatusInformationProvider.CompareCustomsStatusOrder(statusWithInformationOrdered, "A", "A"));
			AssertEquals("StatusB order is greater than StatusA", 1, SadCustomsStatusInformationProvider.CompareCustomsStatusOrder(statusWithInformationOrdered, "A", "B"));
			AssertEquals("StatusB order is minor than StatusA", -1, SadCustomsStatusInformationProvider.CompareCustomsStatusOrder(statusWithInformationOrdered, "B", "A"));
			AssertEquals("StatusB order is minor when is null", -1, SadCustomsStatusInformationProvider.CompareCustomsStatusOrder(statusWithInformationOrdered, "A", ""));
		});
	}
}
