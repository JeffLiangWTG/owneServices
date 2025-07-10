using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RepairLineSynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchronise_Warranty()
		{
			destination.Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			destination.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			new RepairLineSynchroniser(destination, source).Synchronise(true);
			AssertEquals(0m, destination.JI_CustomsQuantity);
			AssertEquals(0m, destination.JI_CustomsSecondQuantity);
			AssertEquals(0m, destination.JI_CustomsThirdQuantity);
			AssertEquals(0m, destination.CA_ADJValue);
			AssertEquals("Warranty Repairs Remission -", destination.JI_Description);
		}

		public void TestSynchronise()
		{
			destination.Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			source.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			destination.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			new RepairLineSynchroniser(destination, source).Synchronise(true);

			AssertNotEquals("PK", destination.PK, source.PK);
			AssertEquals("JI_Tariff", destination.JI_Tariff, source.JI_Tariff);
			AssertEquals("JI_Description", destination.JI_Description, "Repairs Remission -");
			AssertEquals("CA_99TariffCode", destination.CA_99TariffCode, source.CA_99TariffCode);
			AssertEquals("CA_TreatmentCode", destination.CA_TreatmentCode, source.CA_TreatmentCode);
			AssertEquals("CA_ValueForDutyCode", destination.CA_ValueForDutyCode, "29");
			AssertEquals("CA_ADJCode", destination.CA_ADJCode, source.CA_ADJCode);
			AssertEquals("CA_ADJValue", destination.CA_ADJValue, source.CA_ADJValue);
			AssertEquals("JI_CustomsQuantity", destination.JI_CustomsQuantity, source.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", destination.JI_CustomsUnitQty, source.JI_CustomsUnitQty);
			AssertEquals("JI_InvoiceQuantity", destination.JI_InvoiceQuantity, source.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", destination.JI_InvoiceUQ, source.JI_InvoiceUQ);
			AssertEquals("JI_CustomsSecondQuantity", destination.JI_CustomsSecondQuantity, source.JI_CustomsSecondQuantity);
			AssertEquals("JI_CustomsSecondUnitQty", destination.JI_CustomsSecondUnitQty, source.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdQuantity", destination.JI_CustomsThirdQuantity, source.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdUnitQty", destination.JI_CustomsThirdUnitQty, source.JI_CustomsThirdUnitQty);
			AssertEquals("JI_CountryOfOrigin", destination.JI_CountryOfOrigin, source.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", destination.JI_StateOrRegionOfOrigin, source.JI_StateOrRegionOfOrigin);
			AssertEquals("CA_RN_NKExport", destination.CA_RN_NKExport, ZString.Empty);
			AssertEquals("CA_USStateOfExport", destination.CA_USStateOfExport, ZString.Empty);
			AssertEquals("CA_RequirementID", destination.CA_RequirementID, source.CA_RequirementID);
			AssertEquals("CA_RequirementVer", destination.CA_RequirementVer, source.CA_RequirementVer);
			AssertEquals("CA_AirsCode", destination.CA_AirsCode, source.CA_AirsCode);
			AssertEquals("CA_DestinationProvince", destination.CA_DestinationProvince, source.CA_DestinationProvince);
			AssertEquals("CA_RN_NKCFIAOrigin", destination.CA_RN_NKCFIAOrigin, source.CA_RN_NKCFIAOrigin);
			AssertEquals("CA_CFIAUSStateOfOrigin", destination.CA_CFIAUSStateOfOrigin, source.CA_CFIAUSStateOfOrigin);
			AssertEquals("CA_EndUse", destination.CA_EndUse, source.CA_EndUse);
			AssertEquals("CA_MiscID", destination.CA_MiscID, source.CA_MiscID);
			AssertEquals("CA_ImportReasonCode", destination.CA_ImportReasonCode, source.CA_ImportReasonCode);
			AssertEquals("CA_Model", destination.CA_Model, source.CA_Model);
			AssertEquals("CA_ModelNumber", destination.CA_ModelNumber, source.CA_ModelNumber);
			AssertEquals("JI_BrandName", destination.JI_BrandName, source.JI_BrandName);
			AssertEquals("CA_TypeSize", destination.CA_TypeSize, source.CA_TypeSize);
			AssertEquals("CA_TIIN", destination.CA_TIIN, source.CA_TIIN);
			AssertEquals("CA_CompliantCompletion", destination.CA_CompliantCompletion, source.CA_CompliantCompletion);
			AssertEquals("CA_CompliantImportDate", destination.CA_CompliantImportDate, source.CA_CompliantImportDate);
			AssertEquals("CA_AuthorityNumber should not be synchronized", destination.CA_AuthorityNumber, "");//WI00233229

			AssertEquals("2 CFIA Numbers", 2, destination.CFIARegistrationNumbers.Count);
			AssertEquals("1st CFIA Number code", "1", destination.CFIARegistrationNumbers[0].CY_Code);
			AssertEquals("1st CFIA Number data", "AAAAAA", destination.CFIARegistrationNumbers[0].CY_Data);
			AssertEquals("2nd CFIA Number code", "2", destination.CFIARegistrationNumbers[1].CY_Code);
			AssertEquals("2nd CFIA Number data", "BBBBBB", destination.CFIARegistrationNumbers[1].CY_Data);
			AssertEquals("2 SITT Numbers", 2, destination.SITTCertificationNumbers.Count);
			AssertEquals("1st SITT Number data", "CCCCCC", destination.SITTCertificationNumbers[0].CY_Data);
			AssertEquals("2nd SITT Number data", "DDDDDD", destination.SITTCertificationNumbers[1].CY_Data);

			source.CA_TIIN = "X";
			source.CA_CompliantCompletion = false;
			AssertEquals("CA_TIIN", destination.CA_TIIN, source.CA_TIIN);
			AssertEquals("CA_CompliantCompletion", destination.CA_CompliantCompletion, source.CA_CompliantCompletion);

			source.CFIARegistrationNumbers[0].CY_Code = "3";
			source.CFIARegistrationNumbers[0].CY_Data = "ZZZZZZ";
			AssertEquals("2 CFIA Numbers", 2, destination.CFIARegistrationNumbers.Count);
			AssertEquals("1st CFIA Number code", "3", destination.CFIARegistrationNumbers[0].CY_Code);
			AssertEquals("1st CFIA Number data", "ZZZZZZ", destination.CFIARegistrationNumbers[0].CY_Data);
			source.CFIARegistrationNumbers[0].Delete();
			AssertEquals("1 CFIA Numbers", 1, destination.CFIARegistrationNumbers.Count);
			AssertEquals("1st CFIA Number code", "2", destination.CFIARegistrationNumbers[0].CY_Code);
			AssertEquals("1st CFIA Number data", "BBBBBB", destination.CFIARegistrationNumbers[0].CY_Data);
			source.CFIARegistrationNumbers.AddNew("4", "QQQQQQ");
			AssertEquals("2 CFIA Numbers", 2, destination.CFIARegistrationNumbers.Count);
			AssertEquals("2nd CFIA Number code", "4", destination.CFIARegistrationNumbers[1].CY_Code);
			AssertEquals("2nd CFIA Number data", "QQQQQQ", destination.CFIARegistrationNumbers[1].CY_Data);

			source.SITTCertificationNumbers[0].CY_Data = "RRRRRR";
			AssertEquals("2 SITT Numbers", 2, destination.SITTCertificationNumbers.Count);
			AssertEquals("1st SITT Number data", "RRRRRR", destination.SITTCertificationNumbers[0].CY_Data);
			source.SITTCertificationNumbers[0].Delete();
			AssertEquals("1 SITT Numbers", 1, destination.SITTCertificationNumbers.Count);
			AssertEquals("1st SITT Number data", "DDDDDD", destination.SITTCertificationNumbers[0].CY_Data);
			source.SITTCertificationNumbers.AddNew("", "SSSSSS");
			AssertEquals("2 CFIA Numbers", 2, destination.SITTCertificationNumbers.Count);
			AssertEquals("2nd SITT Number data", "SSSSSS", destination.SITTCertificationNumbers[1].CY_Data);
			source.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsIdenticalGoods;
			AssertEquals("CA_ValueForDutyCode", destination.CA_ValueForDutyCode, "19");

			source.Declaration.JE_MessageType = CA.Business.JobMessageTypeList.Codes.LowValueShipments;
			new RepairLineSynchroniser(destination, source).Synchronise(true);
			AssertEquals("CA_RN_NKExport", destination.CA_RN_NKExport, source.CA_RN_NKExport);
			AssertEquals("CA_USStateOfExport", destination.CA_USStateOfExport, source.CA_USStateOfExport);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Canada;

			source = declaration.FilteredInvoiceLines.AddNew();
			source.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			source.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			source.JI_CountryOfOrigin = Constants.CountryCodes.Canada;
			source.JI_StateOrRegionOfOrigin = CanadianProvinceList.Codes.Alberta;
			source.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			source.CA_USStateOfExport = USStatesList.Codes.Arkansas;
			source.CA_ADJCode = AmountTypes.Codes.Percent;
			source.CA_ADJValue = 10;
			source.JI_InvoiceQuantity = 123m;
			source.JI_InvoiceUQ = "NMB";
			JobComInvoiceLineTestHelper.FillInvoiceLine(source, 2, 2, 600, 100, 90, 80, Constants.Weight.Kilograms);

			source.CA_RequirementID = "A";
			source.CA_RequirementVer = "B";
			source.CA_AirsCode = "C";
			source.CA_DestinationProvince = "D";
			source.CA_RN_NKCFIAOrigin = "E";
			source.CA_CFIAUSStateOfOrigin = "US";
			source.CA_EndUse = "CA";
			source.CA_MiscID = "F";
			source.CA_ImportReasonCode = "G";
			source.CA_Model = "H";
			source.CA_ModelNumber = "I";
			source.JI_BrandName = "J";
			source.CA_TypeSize = "K";
			source.CA_TIIN = "L";
			source.CA_CompliantCompletion = true;
			source.CA_CompliantImportDate = true;
			source.CA_AuthorityNumber = "12345";
			source.CFIARegistrationNumbers.AddNew("1", "AAAAAA");
			source.CFIARegistrationNumbers.AddNew("2", "BBBBBB");
			source.SITTCertificationNumbers.AddNew("", "CCCCCC");
			source.SITTCertificationNumbers.AddNew("", "DDDDDD");

			destination = declaration.FilteredInvoiceLines.AddNew();
			destination.JI_ParentID = source.PK;
		}

		JobComInvoiceLine source;
		JobComInvoiceLine destination;
	}
}
