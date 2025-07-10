using System;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class PackageProviderTest : Customs.Business.Testing.DataProviderTestCase<PackageProvider>
	{
		public void TestConstructor_ReportItemNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new PackageProvider(null, ""));
		}

		public void TestConstructor_PackageNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new PackageProvider(Factory.New<CusExitReportItem>(), ""));
		}

		public void TestQuantity() => CombineAssertions(() =>
		{
			reportItem.ERI_Quantity = 10;
			AssertEquals("Quantity when ShipmentType != 'FP'", 10, GetProvider().Quantity);

			shipmentType = "FP";
			AssertNull("Quantity when ShipmentType = 'FP'", GetProvider().Quantity);
		});

		public void TestIsSupportEmptyPackType()
		{
			CombineAssertions(() =>
			{
				package.CXP_PackageType = "1A";
				AssertEquals("Not bulk", true, Provider.IsSupportEmptyPackType);

				package.CXP_PackageType = "VG";
				AssertEquals("Bulk", false, Provider.IsSupportEmptyPackType);
			});
		}

		public void TestKind() => CombineAssertions(() =>
		{
			package.CXP_PackageType = "1A";
			AssertEquals("Kind when ShipmentType != 'FP'", "1A", GetProvider().Kind);

			shipmentType = "FP";
			AssertEquals("Kind when ShipmentType = 'FP'", string.Empty, GetProvider().Kind);
		});

		public void TestMarksNumbers() => CombineAssertions(() =>
		{
			package.CXP_MarksAndNumbers = "xyz";
			AssertEquals("MarksNumbers when ShipmentType != 'FP'", "xyz", GetProvider().MarksNumbers);

			shipmentType = "FP";
			AssertEquals("MarksNumbers when ShipmentType = 'FP'", string.Empty, GetProvider().Kind);
		});

		public void TestPositionNumber()
		{
			shipmentType = "FP";
			package.CXP_Sequence = 2;
			AssertEquals(2, GetProvider().PositionNumber);
		}

		protected override PackageProvider GetProvider() => new PackageProvider(reportItem, shipmentType);

		protected override void SetUp()
		{
			base.SetUp();
			package = Factory.New<CusExitConsignmentPackage>();
			reportItem = Factory.New<CusExitReportItem>();
			reportItem.ERI_CXP_Package = package.PK;
			shipmentType = "";
		}
		CusExitReportItem reportItem;
		CusExitConsignmentPackage package;
		string shipmentType;
	}
}
