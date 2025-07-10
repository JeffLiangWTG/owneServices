using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SACJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestExportDateShouldBeValid()
		{
			testDec.JE_ExportDate = ZDateTime.Invalid;
			AssertHasErrors(testDec.JE_ExportDateInfo);
		}

		public void TestDateOfFirstArrival()
		{
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			testDec.JE_RL_NKPortOfLoading = "SGSIN";
			testDec.Validation.ValidateJE_DateOfFirstArrival();
			AssertNoMessageErrors("Date of first arrival", testDec.JE_DateOfFirstArrivalInfo);
		}

		public void TestVoyageFlightNo()
		{
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			CheckVoyageFlightNo();
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			CheckVoyageFlightNo();
		}

		void CheckVoyageFlightNo()
		{
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Is Air", true, testDec.IsAir);

			testDec.JE_VoyageFlightNo = "";
			AssertNoMessageErrors("Empty flight no for air is not an error", testDec.JE_VoyageFlightNoInfo);

			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDec.JE_VoyageFlightNo = "";
			AssertNoMessageErrors("Empty voyage number for sea is a message error", testDec.JE_VoyageFlightNoInfo);

			testDec.JE_VoyageFlightNo = "123";
			AssertNoMessageErrors("Empty voyage number for sea is a message error", testDec.JE_VoyageFlightNoInfo);
		}

		public void TestGoodsDescription()
		{
			testDec.JE_GoodsDescription = "";
			AssertEquals("Goods description is mandatory", true, testDec.JE_GoodsDescriptionInfo.HasMessageErrors());

			testDec.JE_GoodsDescription = "Description";
			AssertEquals("Goods description is not empty", false, testDec.JE_GoodsDescriptionInfo.HasMessageErrors());
		}

		public void TestDateOfArrival()
		{
			testDec.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("DateOfArrival is mandatory", true, testDec.JE_DateOfArrivalInfo.HasMessageErrors());

			testDec.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals("DateOfArrival is not empty now", false, testDec.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestPortOfArrival()
		{
			testDec.JE_RL_NKPortOfArrival = "";
			AssertEquals("Port of arrival is mandatory", true, testDec.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			testDec.JE_RL_NKPortOfArrival = "AU~~~";
			AssertEquals("Port of arrival is invalid", true, testDec.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			testDec.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("Port of arrival is invalid as this is import job and port of arrival should be AU", true, testDec.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Port of arrival valid", false, testDec.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestPortOfLoading()
		{
			testDec.JE_RL_NKPortOfLoading = "";
			AssertNoMessageErrors("Port of loading is not required", testDec.JE_RL_NKPortOfLoadingInfo);
		}

		public void TestPortOfFirstArrivalShouldNotBeCheckedWhenSAC()
		{
			testDec.JE_RL_NKPortOfFirstArrival = "";
			AssertNoNotifications("Port of first arrival should not be checked when SAC", testDec.JE_RL_NKPortOfFirstArrivalInfo);
		}

		public void TestHouseBillIsNotCheckedWhenSAC()
		{
			testDec.JE_HouseBill = "";
			AssertNoNotifications("House Bill should not be checked when SAC", testDec.JE_HouseBillInfo);
		}

		public void TestPortOfOriginShouldNotBeCheckedWhenSAC()
		{
			testDec.JE_RL_NKOrigin = "";
			AssertNoNotifications("Port of Origin should not be checked when SAC", testDec.JE_RL_NKOriginInfo);
		}

		public void TestJE_ExportDateIsNotCheckedForSAC()
		{
			testDec.JE_ExportDate = ZDateTime.Empty;
			AssertNoNotifications("Export Date should not be checked when SAC", testDec.JE_ExportDateInfo);
		}

		public void TestTestingValuesOfDeclarationObjectOfThisTestClass()
		{
			AssertEquals(true, testDec.IsImport);
			AssertEquals(true, testDec.IsSAC);
		}

		public void TestImporterValidationWhenABNAndCIDEmtpy()
		{
			OrgHeader header = OrgHeader.New(Factory);
			testDec.JE_OH_Importer = header.PK;
			AssertHasMessageErrors("Importer", testDec.JE_OH_ImporterInfo);

			header.CustomsClientID = "12345678901";
			testDec.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors("Importer has CID", testDec.JE_OH_ImporterInfo);

			header.CustomsClientID = ZString.Empty;
			testDec.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrors("Importer", testDec.JE_OH_ImporterInfo);

			header.LocalBusinessRegNo = "12345678901";
			testDec.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors("Importer has ABN", testDec.JE_OH_ImporterInfo);
		}

		#region Implementation

		JobDeclaration testDec;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals(true, testDec.IsSAC);
			AssertEquals(true, testDec.IsSACWithoutLines);
		}

		#endregion
	}
}
