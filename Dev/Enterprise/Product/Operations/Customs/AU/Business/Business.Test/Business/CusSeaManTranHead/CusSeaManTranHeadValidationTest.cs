using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManTranHeadValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBT_VoyageNum()
		{
			CusSeaManTranHead head = GetNewHead();

			head.Validation.ValidateBT_VoyageNum();
			AssertHasErrors("by default", head.BT_VoyageNumInfo);

			head.BT_VoyageNum = "123";
			AssertNoNotifications("when filled", head.BT_VoyageNumInfo);

			head.BT_VoyageNum = "1234567";
			AssertHasMessageErrors("when over 6 charactors", head.BT_VoyageNumInfo);
		}

		public void TestValidateBT_VesselName()
		{
			CusSeaManTranHead head = GetNewHead();

			head.Validation.ValidateBT_VesselName();
			AssertHasErrors("by default", head.BT_VesselNameInfo);

			head.BT_VesselName = (Factory.NewWithValidTestData<RefVessel>()).RV_Code;
			AssertNoNotifications("when valid", head.BT_VesselNameInfo);
		}

		public void TestCheckBT_VesselName_MessageErrorOnDuplicate()
		{
			const string expectedMessageError = "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel.";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var cusManifestHeader = Factory.NewWithValidTestData<CusSeaManTranHead>();
			cusManifestHeader.BT_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNoMessageError("No duplicate", cusManifestHeader.BT_VesselNameInfo, expectedMessageError);

				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				cusManifestHeader.Validation.ValidateBT_VesselName();
				AssertHasMessageError("Has Duplicate Vessel Name", cusManifestHeader.BT_VesselNameInfo, expectedMessageError);
			});
		}

		public void TestCheckBT_VesselName_LlyodsNumberValidation()
		{
			const string expectedMessageError = "Lloyds number must be 7 characters in length";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HYOGO MARU";
			vessel.RV_LloydsNumber = "111111";

			var cusManifestHeader = Factory.NewWithValidTestData<CusSeaManTranHead>();
			cusManifestHeader.BT_VesselName = vessel.RV_Name;
			CombineAssertions(() =>
			{
				AssertHasMessageError("Invalid Lloyds No", cusManifestHeader.BT_VesselNameInfo, expectedMessageError);

				cusManifestHeader.BT_LloydsIMO = "1111117";
				cusManifestHeader.Validation.ValidateBT_VesselName();
				AssertNoMessageError("Valid Lloyds No", cusManifestHeader.BT_VesselNameInfo, expectedMessageError);
			});
		}

		public void TestValidateBT_RL_NKPortOfLastForeignPort()
		{
			var head = GetNewHead();

			head.Validation.ValidateBT_RL_NKPortOfLastForeignPort();
			AssertHasMessageErrors("by default", head.BT_RL_NKPortOfLastForeignPortInfo);

			head.BT_RL_NKPortOfLastForeignPort = "~~~";
			AssertHasErrors("when invalid", head.BT_RL_NKPortOfLastForeignPortInfo);

			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_R3 = ZGuid.Empty;
			head.BT_RL_NKPortOfLastForeignPort = port.RL_Code;
			AssertHasMessageError(head.BT_RL_NKPortOfLastForeignPortInfo, $"Please choose a valid time zone on {head.BT_RL_NKPortOfLastForeignPortInfo.HumanReadableName}");

			port.RL_R3 = Factory.NewWithValidTestData<RefTimeZoneSet>().PK;
			head.BT_RL_NKPortOfLastForeignPort = port.RL_Code;
			AssertNoMessageErrors("when valid", head.BT_RL_NKPortOfLastForeignPortInfo);
			AssertNoErrors("when valid", head.BT_RL_NKPortOfLastForeignPortInfo);
		}

		public void TestValidateBT_PortOfLastForeignPortATD()
		{
			CusSeaManTranHead head = GetNewHead();

			head.Validation.ValidateBT_PortOfLastForeignPortATD();
			AssertHasMessageErrors("by default", head.BT_PortOfLastForeignPortATDInfo);

			head.BT_PortOfLastForeignPortATD = ZDateTime.Now;
			AssertNoMessageErrors("when valid", head.BT_PortOfLastForeignPortATDInfo);
			AssertNoErrors("when valid", head.BT_PortOfLastForeignPortATDInfo);
		}

		#region Implementation

		CusSeaManTranHead GetNewHead()
		{
			return Factory.New<CusSeaManTranHead>();
		}

		#endregion
	}
}
