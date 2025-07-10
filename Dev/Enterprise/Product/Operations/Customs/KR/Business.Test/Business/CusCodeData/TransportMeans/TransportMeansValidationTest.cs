using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class TransportMeansValidationTest : CusCodeDataValidationTest
	{
		public void TestWorkingVesselName()
		{
			var vessel1 = RefVessel.New(Factory);
			vessel1.RV_Code = "CY_Code Test1";

			var vessel2 = RefVessel.New(Factory);
			vessel2.RV_Code = "CY_Code Test2";
			vessel2.RV_MalaysiaVesselId = "ABC123456";
			Factory.Save();

			transportMean.CY_Data = "CY_Data";
			transportMean.CY_Code = ZString.Empty;
			AssertNoMessageError(transportMean.CY_CodeInfo, "Either Working Vessel Name or Transport Vehicle Reg No is Mandatory. Please enter one of the two fields.");

			transportMean.CY_Data = ZString.Empty;
			transportMean.CY_Code = ZString.Empty;
			AssertHasMessageErrorContaining(transportMean.CY_CodeInfo, "Either Working Vessel Name or Transport Vehicle Reg No is Mandatory. Please enter one of the two fields.");

			transportMean.CY_Code = "CY_Code Test3";
			AssertHasMessageErrorContaining(transportMean.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			transportMean.CY_Code = "CY_Code Test1";
			AssertHasMessageErrorContaining(transportMean.CY_CodeInfo, "You have entered a vessel. Then its Agent Vessel Number must be entered. Please enter F3 in the Vessel Name column and enter its ID in the Agent Vessel Number field in the Vessel form.");

			transportMean.CY_Code = "CY_Code Test2";
			AssertNoMessageErrors(transportMean.CY_CodeInfo);
		}

		public void TestVessleID()
		{
			var vessel1 = RefVessel.New(Factory);
			vessel1.RV_Code = "CY_Code Test1";
			vessel1.RV_MalaysiaVesselId = "12345678";

			var vessel2 = RefVessel.New(Factory);
			vessel2.RV_Code = "CY_Code Test2";
			vessel2.RV_MalaysiaVesselId = "AB1234567";

			var vessel3 = RefVessel.New(Factory);
			vessel3.RV_Code = "CY_Code Test3";
			vessel3.RV_MalaysiaVesselId = "ABC12345A";

			var vessel4 = RefVessel.New(Factory);
			vessel4.RV_Code = "CY_Code Test4";
			vessel4.RV_MalaysiaVesselId = "ABC123456";
			Factory.Save();

			transportMean.CY_Code = "CY_Code Test1";
			AssertHasMessageErrorContaining(transportMean.DescriptionInfo, "The length of Vessel ID must be nine.");

			transportMean.CY_Code = "CY_Code Test2";
			AssertHasMessageErrorContaining(transportMean.DescriptionInfo, "The first three characters must be English characters and remaining characters must be digits.");

			transportMean.CY_Code = "CY_Code Test3";
			AssertHasMessageErrorContaining(transportMean.DescriptionInfo, "The first three characters must be English characters and remaining characters must be digits.");

			transportMean.CY_Code = "CY_Code Test4";
			AssertNoMessageErrors(transportMean.DescriptionInfo);
		}

		public void TestWorkingVesselLloydsNumberAndTransportVehicleRegNo()
		{
			var vessel1 = RefVessel.New(Factory);
			vessel1.RV_Code = "CY_Code Test1";
			Factory.Save();

			transportMean.CY_Code = vessel1.RV_Code;
			transportMean.CY_Data = ZString.Empty;
			AssertNoMessageErrors(transportMean.CY_DataInfo);

			transportMean.CY_Code = ZString.Empty;
			transportMean.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(transportMean.CY_DataInfo, "Either Working Vessel Name or Transport Vehicle Reg No is Mandatory. Please enter one of the two fields.");

			transportMean.CY_Data = "한글 테스트";
			AssertNoMessageErrors(transportMean.CY_DataInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			transportMean = declaration.TransportMeans.AddNew();
		}
		TransportMeans transportMean;
	}
}
