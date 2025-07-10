using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CPackagingWrapper))]
sealed class CC044CPackagingWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CPackagingWrapper>
{
	public void TestSequenceNumber() => CombineAssertions("SequenceNumber should be mapped to B5_SequenceNumber.", () =>
	{
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package.B5_SequenceNumber = 99;
		var provider = CC044CPackagingWrapper.New(package);
		AssertEquals("UnloadedState NEW", "99", provider.SequenceNumber);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package.PackDifference.B5_SequenceNumber = 98;
		provider = CC044CPackagingWrapper.New(package.PackDifference);
		AssertEquals("UnloadedState DIF", "98", provider.SequenceNumber);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package.B5_SequenceNumber = 97;
		provider = CC044CPackagingWrapper.New(package);
		AssertEquals("UnloadedState MIS", "97", provider.SequenceNumber);
	});

	public void TestTypeOfPackages() => CombineAssertions("TypeOfPackages should be mapped to B5_UnitType only if package is new or has differences.", () =>
	{
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package.B5_UnitType = "KG";
		var provider = CC044CPackagingWrapper.New(package);
		AssertEquals("UnloadedState NEW", "KG", provider.TypeOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package.PackDifference.B5_UnitType = "VG";
		provider = CC044CPackagingWrapper.New(package.PackDifference);
		AssertEquals("UnloadedState DIF", "VG", provider.TypeOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CPackagingWrapper.New(package);
		AssertNullOrEmpty("UnloadedState MIS", provider.TypeOfPackages);
	});

	public void TestNumberOfPackages() => CombineAssertions("NumberOfPackages should be mapped to B5_UnitCount only if package is new or has differences.", () =>
	{
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package.B5_UnitCount = 5;
		package.B5_UnitType = "VG";
		var provider = CC044CPackagingWrapper.New(package);
		AssertNull("UnloadedState MIS", provider.NumberOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		provider = CC044CPackagingWrapper.New(package);
		AssertEquals("UnloadedState NEW", "5", provider.NumberOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package.PackDifference.B5_UnitCount = 10;
		package.PackDifference.B5_UnitType = "KG";
		provider = CC044CPackagingWrapper.New(package.PackDifference);
		AssertEquals("UnloadedState DIF", "10", provider.NumberOfPackages);
	});

	public void TestShippingMarks() => CombineAssertions("ShippingMarks should be mapped to B5_MarksAndNumbers only if package is new or has differences.", () =>
	{
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package.B5_MarksAndNumbers = "Marks";
		var provider = CC044CPackagingWrapper.New(package);
		AssertEquals("UnloadedState NEW", "Marks", provider.ShippingMarks);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package.PackDifference.B5_MarksAndNumbers = "Skram";
		provider = CC044CPackagingWrapper.New(package.PackDifference);
		AssertEquals("UnloadedState DIF", "Skram", provider.ShippingMarks);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CPackagingWrapper.New(package);
		AssertNullOrEmpty("UnloadedState MIS", provider.ShippingMarks);
	});

	protected override CC044CPackagingWrapper GetProvider() => null;

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		package = header.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew();
	}

	NctsPackage package;
}
