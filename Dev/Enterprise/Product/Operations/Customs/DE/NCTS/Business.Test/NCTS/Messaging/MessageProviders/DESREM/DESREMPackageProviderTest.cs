using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DESREMPackageProviderTest : Customs.Business.Testing.DataProviderTestCase<DESREMPackageProvider>
	{
		public void TestConstructor_NullArgument() => AssertExceptionThrown<ArgumentNullException>(() => new DESREMPackageProvider(null));

		public void TestSequenceNumber()
		{
			package.B5_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestKind_MIS()
		{
			package.B5_UnitType = "KG";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.Kind);
		}

		public void TestKind_NEW()
		{
			package.B5_UnitType = "KG";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("KG", Provider.Kind);
		}

		public void TestQuantity_MIS()
		{
			package.B5_UnitCount = 4;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.Quantity);
		}

		public void TestQuantity_NEW()
		{
			package.B5_UnitCount = 4;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(4L, Provider.Quantity);
		}

		public void TestMarksNumber_MIS()
		{
			package.B5_MarksAndNumbers = "MarksNumber";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.MarksNumber);
		}

		public void TestMarksNumber_NEW()
		{
			package.B5_MarksAndNumbers = "MarksNumber";
			AssertEquals("MarksNumber", Provider.MarksNumber);
		}

		protected override DESREMPackageProvider GetProvider() => new DESREMPackageProvider(package);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			package = header.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew();
		}
		NctsPackage package;

		new IDESREMPackage Provider => base.Provider;
	}
}
