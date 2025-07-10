using System.Linq;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class ConsignmentItemPackagingDataProviderTest : BaseDepartureDataProviderTest<ConsignmentItemPackagingDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection() => AssertNull("null", ConsignmentItemPackagingDataProvider.NewCollection(null));

	public void TestSequenceNumber() => CombineAssertions(() =>
	{
		DepartureGoodsItem.Packages.AddNew();
		DepartureGoodsItem.Packages.AddNew();

		var dataProviders = ConsignmentItemPackagingDataProvider.NewCollection(DepartureGoodsItem.Packages);

		AssertEquals("Sequence at 1 index", 1, dataProviders.ElementAt(0).SequenceNumber);
		AssertEquals("Sequence at 2 index", 2, dataProviders.ElementAt(1).SequenceNumber);
	});

	public void TestProperties()
	{
		const string typeOfPackage = "CT";
		Package.B5_UnitType = typeOfPackage;
		const int numberOfPackages = 10;
		Package.B5_UnitCount = numberOfPackages;
		const string shippingMarks = "MarksAndNumbers";
		Package.B5_MarksAndNumbers = shippingMarks;

		CombineAssertions(() =>
		{
			AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
			AssertEquals("TypeOfPackage", typeOfPackage, DataProvider.TypeOfPackages);
			AssertEquals("NumberOfPackages", numberOfPackages, DataProvider.NumberOfPackages);
			AssertEquals("ShippingMarks", shippingMarks, DataProvider.ShippingMarks);
			AssertNull("BypackCorrelationIdentifier (unused)", DataProvider.BypackCorrelationIdentifier);
			AssertNull("BypackType (unused)", DataProvider.BypackType);
		});
	}

	public void TestNumberOfPackages() => CombineAssertions(() =>
	{
		var helper = new RefDataTestHelper(Factory);
		var codeList = helper.CreateCodeList(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, dataGrouping: RefDataGrouping.Codes.UnitedNationsRecommendations);
		var typeOfPackageNoBulk = codeList.CreateCode("P1");
		var typeOfPackageBulk = codeList.CreateCode("P2").WithAttribute(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);
		var typeOfPackageBreakBulk = codeList.CreateCode("P3").WithAttribute(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);
		Factory.Save();

		Package.B5_UnitType = "P1";
		AssertNotNull("No bulk", DataProvider.NumberOfPackages);

		Package.B5_UnitType = "P2";
		ResetDataProvider();
		AssertNull("Bulk", DataProvider.NumberOfPackages);

		Package.B5_UnitType = "P3";
		ResetDataProvider();
		AssertNotNull("BreakBulk", DataProvider.NumberOfPackages);
	});

	NctsPackage Package => package ??= (NctsPackage)DepartureGoodsItem.Packages.AddNew();
	NctsPackage package;

	protected override ConsignmentItemPackagingDataProvider CreateDataProvider() => ConsignmentItemPackagingDataProvider.NewCollection(DepartureGoodsItem.Packages).First();
}
