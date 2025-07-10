using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestExistence()
		{
			AssertNotNull("I Don't Think", testValidation);
		}

		public override void TestPackagesActualPackageCount_Validation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			AssertEquals("Should Have Message Error", false, declaration.PackagesActualPackageCountInfo.HasMessageErrors());
		}

		public void TestAirWayBillValidation()
		{
			//			Assert("PreCondition", !Declaration.JE_MasterBillInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "081-11111112";

			if (declaration.IsImport)
			{
				Assert("Bad Masterbill CheckDigit did not show error", declaration.JE_MasterBillInfo.HasMessageErrors());
			}
			else
			{
				Assert("Bad Masterbill CheckDigit did not show error", declaration.JE_MasterBillInfo.HasWarnings());
			}

			declaration.JE_MasterBill = "081-11111111";
			Assert("Good Masterbill CheckDigit should not show error", !declaration.JE_MasterBillInfo.HasWarnings());
		}

		public void TestOceanBillValidation()
		{
			//			Assert("PreCondition", !Declaration.JE_MasterBillInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "Blah";
			Assert("No error should show", !declaration.JE_MasterBillInfo.HasWarnings());
		}

		public void TestLloydsNumberValidation()
		{
			//			Assert("Precondition: No error", !Declaration.JE_VesselNameInfo.HasMessageErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			RefVessel testVessel = RefVessel.New(Factory);
			testVessel.RV_Code = "SORA BUM";
			testVessel.RV_LloydsNumber = "3493920";
			declaration.JE_VesselName = testVessel.RV_Code;
			AssertEquals("Should have invalid lloyds number message error", true, declaration.JE_VesselNameInfo.HasMessageErrors());
			testVessel.RV_LloydsNumber = "8610033";
			declaration.Validation.ValidateJE_VesselName();
		}

		public void TestCheckJE_DateOfFirstArrival()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertHasMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, "Date of Arrival can not be before the Export Date");

			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
			AssertNoMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, "Date of Arrival can not be before the Export Date");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertHasWarning(declaration.JE_DateOfFirstArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");

			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
			AssertNoWarning(declaration.JE_DateOfFirstArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, "Date of Arrival can not be before the Export Date");

			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 09, 23, 10, 10);
			AssertNoWarning(declaration.JE_DateOfFirstArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");
			AssertHasMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, "Date of Arrival can not be before the Export Date");
		}

		#region Implementation

		protected new JobDeclaration declaration;

		protected JobDeclarationValidation testValidation;

		protected abstract JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			testValidation = GetNewValidationProvider(declaration);
		}

		protected RefVessel GetValidVessel()
		{
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "1234567";
			return vessel;
		}

		protected OrgHeader GetValidImporter()
		{
			ZQuery sQLFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
			sQLFilter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.NotEqual, "");
			return Factory.LoadTop1<OrgHeader>(sQLFilter);
		}

		#endregion
	}
}
