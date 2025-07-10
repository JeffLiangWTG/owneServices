using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ECCCProductCategoriesTest : TestCase
	{
		public void TestGetVehicleClassList()
		{
			AssertEquals("EC01", "EC05, EC06, EC07, EC08, EC09, EC10, EC11, EC12", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.EC01).CodesAsString);
			AssertEquals("XE01", "EC05, EC06, EC07, EC08, EC09, EC10, EC11, EC12", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.XE01).CodesAsString);

			AssertEquals("XE01", "EC05, EC06, EC07, EC08, EC09, EC10, EC11, EC12", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.XE01).CodesAsString);

			AssertEquals("EC02", "", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.EC02).CodesAsString);
			AssertEquals("XE02", "", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.XE02).CodesAsString);
			AssertEquals("EC03", "", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.EC03).CodesAsString);
			AssertEquals("XE03", "", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.XE03).CodesAsString);
			AssertEquals("EC04", "EC25, EC26, EC27, EC28, EC29, EC31, EC32", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.EC04).CodesAsString);
			AssertEquals("EX04", "EC25, EC26, EC27, EC28, EC29, EC31", ECCCProductCategories.GetVehicleClassList(ProcessCodes.Codes.XE04).CodesAsString);
		}

		public void TestGetEngineClassList()
		{
			AssertEquals("EC01", "EC14, EC34", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.EC01).CodesAsString);
			AssertEquals("XE01", "EC14, EC34", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.XE01).CodesAsString);
			AssertEquals("EC02", "EC15, EC16, EC0A, EC0B, EC0C, EC0D", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.EC02).CodesAsString);
			AssertEquals("XE02", "EC15, EC16, EC0A, EC0B, EC0C, EC0D", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.XE02).CodesAsString);
			AssertEquals("EC03", "EC22, EC23, EC24", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.EC03).CodesAsString);
			AssertEquals("XE03", "EC22, EC23, EC24", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.XE03).CodesAsString);
			AssertEquals("EC04", "EC30, EC33", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.EC04).CodesAsString);
			AssertEquals("EX04", "EC44, EC45, EC46", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.XE04).CodesAsString);
		}

		public void TestGetEngineClassList_IsIID402Active()
		{
			var list1 = ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.EC02);
			var list2 = ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.XE02);
			AssertEquals("EC02", "EC15, EC16, EC0A, EC0B, EC0C, EC0D", list1.CodesAsString);
			AssertEquals("XE02", "EC15, EC16, EC0A, EC0B, EC0C, EC0D", list2.CodesAsString);
			AssertEquals("EC03", "EC22, EC23, EC24", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.EC03).CodesAsString);
			AssertEquals("XE03", "EC22, EC23, EC24", ECCCProductCategories.GetEngineClassList(ProcessCodes.Codes.XE03).CodesAsString);

			AssertEquals("Off-Road Mobile Compression-ignition - Loose", list1.GetDescriptionFromCode(ECCCProductCategories.Codes.EC15_New));
			AssertEquals("Off-Road Mobile Compression-ignition - Installed", list1.GetDescriptionFromCode(ECCCProductCategories.Codes.EC16_New));
			AssertEquals("Off-Road Mobile Compression-ignition - Loose", list2.GetDescriptionFromCode(ECCCProductCategories.Codes.EC15_New));
			AssertEquals("Off-Road Mobile Compression-ignition - Installed", list2.GetDescriptionFromCode(ECCCProductCategories.Codes.EC16_New));
		}
	}
}
