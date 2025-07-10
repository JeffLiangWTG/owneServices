using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGAHeaderValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateLPCOs_MandatoryLPCOs()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_WRMProgramInd = "Y";
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "WRM");
			AssertEquals(messageError, "LPCO(s): [8000: Movement Type and 8001: Hazardous Waste/Hazardous Recyclable (HW/HRM) Permit] should be added for program: Waste Reduction And Management Division.");

			pgaHeader.CA_WRMProgramIndInfo.ClearAllNotifications();

			var lpco1 = pgaHeader.LPCOViews.AddNew();
			lpco1.CLP_Type = "8000";
			var lpco2 = pgaHeader.LPCOViews.AddNew();
			lpco2.CLP_Type = "8001";

			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "WRM");
			AssertEquals(messageError, string.Empty);
		}

		public void TestValidateLPCOs_AlternativeLPCOs()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_OCSProgramInd = "Y";
			pgaHeader.CA_IntendedUseCodeOCS = HCIntendedUseCode.Codes.HC01;
			pgaHeader.CA_CategoryOCS = HCCategories.Codes.HC18;
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals("5018 and [5044 or 5017] are required", messageError, "LPCO(s): [[5018: Import Permit] and [5044: Controlled Drugs and Substances Act Licence or 5017: Marihuana for Medical Purposes Regulations Producer Licence]] should be added for program: Office of Controlled Substances.");

			var lpco1 = pgaHeader.LPCOViews.AddNew();
			lpco1.CLP_Type = "5018";

			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals("5018 and [5044 or 5017] are required", messageError, "LPCO(s): [5044: Controlled Drugs and Substances Act Licence or 5017: Marihuana for Medical Purposes Regulations Producer Licence] should be added for program: Office of Controlled Substances.");

			var lpco2 = pgaHeader.LPCOViews.AddNew();
			lpco2.CLP_Type = "5044";

			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals(messageError, string.Empty);

			var lpco3 = pgaHeader.LPCOViews.AddNew();
			lpco3.CLP_Type = "5017";
			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals("5018 and [5044 or 5017] are required", messageError, "LPCO(s): [5044: Controlled Drugs and Substances Act Licence or 5017: Marihuana for Medical Purposes Regulations Producer Licence] should be added for program: Office of Controlled Substances.");

			pgaHeader.CA_IntendedUseCodeOCS = HCIntendedUseCode.Codes.HC15;
			pgaHeader.CA_CategoryOCS = HCCategories.Codes.HC22;
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals("[5018 and 5044] or 5015 are required", messageError, "LPCO(s): [[5018: Import Permit and 5044: Controlled Drugs and Substances Act Licence] or [5015: Test Kit Registration ]] should be added for program: Office of Controlled Substances.");

			var lpco4 = pgaHeader.LPCOViews.AddNew();
			lpco4.CLP_Type = "5018";

			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals("[5018 and 5044] or 5015 are required", messageError, "LPCO(s): [[5018: Import Permit and 5044: Controlled Drugs and Substances Act Licence] or [5015: Test Kit Registration ]] should be added for program: Office of Controlled Substances.");

			var lpco5 = pgaHeader.LPCOViews.AddNew();
			lpco5.CLP_Type = "5044";

			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals(messageError, string.Empty);

			var lpco6 = pgaHeader.LPCOViews.AddNew();
			lpco6.CLP_Type = "5015";
			messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(pgaHeader, "OCS");
			AssertEquals("[5018 and 5044] or 5015 are required", messageError, "LPCO(s): [[5018: Import Permit and 5044: Controlled Drugs and Substances Act Licence] or [5015: Test Kit Registration ]] should be added for program: Office of Controlled Substances.");
		}
	}
}
