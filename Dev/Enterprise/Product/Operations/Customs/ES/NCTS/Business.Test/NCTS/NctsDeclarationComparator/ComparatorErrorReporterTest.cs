using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

sealed class ComparatorErrorReporterTest : TestCaseWithFactory
{
	public void TestGetFormattedReport()
	{
		var errorReporter = new ComparatorErrorReporter();
		errorReporter.AddError(ComparatorErrorLevel.Header, nameof(IComparablePredeclaration.ConsignorID), ZString.Empty);
		errorReporter.AddError(ComparatorErrorLevel.Header, nameof(IComparablePredeclaration.ConsigneeID), ZString.Empty);
		errorReporter.AddError(ComparatorErrorLevel.Header, nameof(IComparablePredeclaration.DeclarationType), ZString.Empty);
		errorReporter.AddError(ComparatorErrorLevel.Consignment, nameof(IComparableHouseConsignment.HouseNumber), "1");
		errorReporter.AddError(ComparatorErrorLevel.Goods, nameof(IComparableGoodsItems.DeclarationType), "1");
		errorReporter.AddError(ComparatorErrorLevel.Goods, nameof(IComparableGoodsItems.AdditionalDocumentsAR), "1");
		errorReporter.AddError(ComparatorErrorLevel.Goods, nameof(IComparableGoodsItems.AdditionalDocumentsTD), "1");
		errorReporter.AddError(ComparatorErrorLevel.Goods, nameof(IComparableGoodsItems.CommodityCode), "1");

		var expectedReport = @"Consignment/Consignor
Consignment/Consignee
Consignment/Declaration Type
House Consignment(1)/Sequence Number
Goods Item(1)/Declaration Type
And 3 more errors.";
		AssertEquals("Expected formatted report", expectedReport, errorReporter.GetFormattedReport());
	}
}
