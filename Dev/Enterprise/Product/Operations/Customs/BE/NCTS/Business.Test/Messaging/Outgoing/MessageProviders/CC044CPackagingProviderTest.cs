using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC044CPackagingProvider))]
	sealed class CC044CPackagingProviderTest : PackagingProviderAbstractTest<CC044CPackagingProvider>
	{
		public override void TestSequenceNumber()
		{
			package.B5_SequenceNumber = 2;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public override void TestTypeOfPackages()
		{
			package.B5_UnitType = "KG";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("KG", Provider.TypeOfPackages);
		}

		public void TestTypeOfPackages_Conditional()
		{
			package.B5_UnitType = "KG";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			AssertNullOrEmpty(Provider.TypeOfPackages);
		}

		public override void TestNumberOfPackages()
		{
			package.B5_UnitCount = 5;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.NumberOfPackages);
		}

		public void TestNumberOfPackages_New()
		{
			package.B5_UnitCount = 5;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(5, Provider.NumberOfPackages);
		}

		public override void TestNumberOfPackages_Bulk()
		{
			package.B5_UnitCount = 10;
			package.B5_UnitType = "VG";
			AssertNull(Provider.NumberOfPackages);
		}

		public void TestNumberOfPackages_New_Bulk()
		{
			package.B5_UnitCount = 5;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package.B5_UnitType = "VG";
			AssertNull(Provider.NumberOfPackages);
		}

		public override void TestShippingMarks()
		{
			package.B5_MarksAndNumbers = "marks";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("marks", Provider.ShippingMarks);
		}

		public void TestShippingMarks_Conditional()
		{
			package.B5_MarksAndNumbers = "marks";
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			AssertNullOrEmpty(Provider.ShippingMarks);
		}
	}
}
