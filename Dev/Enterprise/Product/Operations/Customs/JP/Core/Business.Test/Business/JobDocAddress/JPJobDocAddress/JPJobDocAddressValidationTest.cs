using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JPJobDocAddressValidation))]
	public class JPJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_OA_Address()
		{
			var expectedDepotMessage = "The selected Customs Depot address does not have a Customs Controlled Premises Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Numbers and add a JP-CCP code with the selected Customs Depot address as the premises address.";
			var expectedBondedWarehouseMessage = "The selected Bonded Warehouse address does not have a Customs Controlled Premises Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Numbers and add a JP-CCP code with the selected Bonded Warehouse address as the premises address.";
			var expectedInspectionWitnessMessage = "The selected Inspection Witness address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected Inspection Witness address.";
			var expectedExternalBrokerMessage = "The selected External Broker address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected External Broker address.";
			var expectedForwarderMessage = "The selected Freight Forwarder address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected Freight Forwarder address.";
			var expectedAirCargoAgentMessage1 = "The selected Air Cargo Agent address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected Air Cargo Agent address.";
			var expectedAirCargoAgentMessage2 = "The selected Air Cargo Agent address does not have a Location Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is AAL, and Premises Address is the selected Air Cargo Agent address.";

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.MainAddress.CustomsCodes.AddNew();

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "OrgTest2";
			header2.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2HDN8", Core.Constants.CountryCodes.Japan);
			header2.MainAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "12345", Core.Constants.CountryCodes.Japan);
			header2.MainAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.AAL, "AAL45", Core.Constants.CountryCodes.Japan);

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var depotAddressInfo = jobDeclaration.DepotDocAddress.E2_OA_AddressInfo;
			var bondedWarehouseAddressInfo = jobDeclaration.WarehouseDocAddress.E2_OA_AddressInfo;
			var inspectionWitnessAddressInfo = jobDeclaration.InspectionWitness.E2_OA_AddressInfo;
			var externalBrokerAddressInfo = jobDeclaration.ExternalBrokerAddress.E2_OA_AddressInfo;
			var forwarderAddressInfo = jobDeclaration.ForwarderAddress.E2_OA_AddressInfo;
			var airCargoAgentInfo = jobDeclaration.AirCargoAgent.E2_OA_AddressInfo;

			jobDeclaration.DepotDocAddress.OrganisationPK = header.PK;
			jobDeclaration.WarehouseDocAddress.OrganisationPK = header.PK;
			jobDeclaration.InspectionWitness.OrganisationPK = header.PK;
			jobDeclaration.ExternalBrokerAddress.OrganisationPK = header.PK;
			jobDeclaration.ForwarderAddress.OrganisationPK = header.PK;
			jobDeclaration.AirCargoAgent.OrganisationPK = header.PK;

			AssertHasMessageError(depotAddressInfo, expectedDepotMessage);
			AssertHasMessageError(bondedWarehouseAddressInfo, expectedBondedWarehouseMessage);
			AssertHasMessageError(inspectionWitnessAddressInfo, expectedInspectionWitnessMessage);
			AssertHasMessageError(externalBrokerAddressInfo, expectedExternalBrokerMessage);
			AssertHasMessageError(forwarderAddressInfo, expectedForwarderMessage);
			AssertHasMessageError(airCargoAgentInfo, expectedAirCargoAgentMessage1);
			AssertHasMessageError(airCargoAgentInfo, expectedAirCargoAgentMessage2);

			jobDeclaration.DepotDocAddress.OrganisationPK = header2.PK;
			jobDeclaration.WarehouseDocAddress.OrganisationPK = header2.PK;
			jobDeclaration.InspectionWitness.OrganisationPK = header2.PK;
			jobDeclaration.ExternalBrokerAddress.OrganisationPK = header2.PK;
			jobDeclaration.DepotDocAddress.E2_OA_Address = header2.MainAddress.PK;
			jobDeclaration.WarehouseDocAddress.E2_OA_Address = header2.MainAddress.PK;
			jobDeclaration.InspectionWitness.E2_OA_Address = header2.MainAddress.PK;
			jobDeclaration.ExternalBrokerAddress.E2_OA_Address = header2.MainAddress.PK;
			jobDeclaration.ForwarderAddress.E2_OA_Address = header2.MainAddress.PK;
			jobDeclaration.AirCargoAgent.E2_OA_Address = header2.MainAddress.PK;

			AssertNoMessageError(depotAddressInfo, expectedDepotMessage);
			AssertNoMessageError(bondedWarehouseAddressInfo, expectedBondedWarehouseMessage);
			AssertNoMessageError(inspectionWitnessAddressInfo, expectedInspectionWitnessMessage);
			AssertNoMessageError(externalBrokerAddressInfo, expectedExternalBrokerMessage);
			AssertNoMessageError(forwarderAddressInfo, expectedForwarderMessage);
			AssertNoMessageError(airCargoAgentInfo, expectedAirCargoAgentMessage1);
			AssertNoMessageError(airCargoAgentInfo, expectedAirCargoAgentMessage2);
		}

		public void TestCheckE2_GovRegNumType()
		{
			var expectedErrorMessage = "The code you have selected is not in the list.";
			var info = DocAddress.E2_GovRegNumTypeInfo;

			DocAddress.E2_GovRegNumType = "xxx";
			AssertHasMessageError(info, expectedErrorMessage);

			DocAddress.E2_GovRegNumType = OrgCusCode.JapanCodeTypes.JAS;
			AssertNoMessageError(info, expectedErrorMessage);
		}

		public void TestCheckE2_GovRegNum_JAS()
		{
			DocAddress.E2_GovRegNumType = OrgCusCode.JapanCodeTypes.JAS;

			var errorMessage = "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z].";
			var targetInfo = DocAddress.E2_GovRegNumInfo;

			DocAddress.E2_GovRegNum = "12345678901";
			AssertHasMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "123456789012";
			AssertNoMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "12345678901b";
			AssertHasMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "12345678901L";
			AssertNoMessageError(targetInfo, errorMessage);
		}

		public void TestCheckE2_GovRegNum_LPC()
		{
			DocAddress.E2_GovRegNumType = OrgCusCode.JapanCodeTypes.LPC;

			var targetInfo = DocAddress.E2_GovRegNumInfo;
			var errorMessage = "Please enter 13 digits, or 17 digits.";

			DocAddress.E2_GovRegNum = "123456789012";
			AssertHasMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "123456789012L";
			AssertHasMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum  = "1234567890123";
			AssertNoMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "12345678901234567";
			AssertNoMessageError(targetInfo, errorMessage);
		}

		public void TestCheckE2_GovRegNum_CIE()
		{
			DocAddress.E2_GovRegNumType = OrgCusCode.JapanCodeTypes.CIE;

			var targetInfo = DocAddress.E2_GovRegNumInfo;
			var errorMessage = "Please enter C0000 followed by 8 or 12 digits, or 1 followed by 7 or 11 digits.";

			DocAddress.E2_GovRegNum = "1234567";
			AssertHasMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "1234567890123";
			AssertHasMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "123456789012";
			AssertNoMessageError(targetInfo, errorMessage);

			DocAddress.E2_GovRegNum = "C0000123456789012";
			AssertNoMessageError(targetInfo, errorMessage);
		}

		public void TestCheckPropertiesRequireMessageErrorIfNotWesternEuropean()
		{
			CombineAssertions(() =>
			{
				CheckMessageErrorIfNotWesternEuropean(x => x.E2_CompanyNameInfo);
				CheckMessageErrorIfNotWesternEuropean(x => x.E2_AdditionalAddressInformationInfo);
				CheckMessageErrorIfNotWesternEuropean(x => x.E2_Address1Info);
				CheckMessageErrorIfNotWesternEuropean(x => x.E2_Address2Info);
			});
		}

		void CheckMessageErrorIfNotWesternEuropean(Func<JobDocAddress, ZPropertyInfo> propertyInfSelector)
		{
			var targetInfo = propertyInfSelector.Invoke(DocAddress);
			var humanReadableName = targetInfo.HumanReadableName;
			var message = $"{humanReadableName} only accepts Western European languages characters.";
			targetInfo.Value = (ZString)"ゐゐゐ";
			AssertHasMessageError($"{humanReadableName} has message error.", targetInfo, message);

			targetInfo.Value = (ZString)"A11";
			AssertNoMessageError($"{humanReadableName} has no message error.", targetInfo, message);
		}

		public void TestCheckLocationCode()
		{
			DocAddress.E2_AddressOverride = true;
			ValidationTestHelper.AssertInvalidCodeMessageError(DocAddress.LocationCodeInfo, "a12", "ABC", "Please enter no longer than 3 characters consisting solely of digits and capital letters [A-Z].");
		}

		public void TestCheckEnglishAddress()
		{
			CheckEnglishAddressInDifferentLanguages(DocAddress, [Core.Constants.Languages.EnglishAmerican, Core.Constants.Languages.EnglishBritish, Core.Constants.Languages.English], Core.Constants.Languages.Japanese, Core.Constants.Languages.ChineseTraditional);
		}

		void CheckEnglishAddressInDifferentLanguages(JPJobDocAddress docAddress, string[] validEnglishLanguages, params string[] invalidEnglishLanguages)
		{
			var message = "NACCS only accepts addresses in English. The entered address is not in English. Please override it to English or add an English translation to the address.";
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "org1";
			var addresses = header.Addresses;
			CombineAssertions(() =>
			{
				invalidEnglishLanguages.ForEach(l =>
				{
					var address = addresses.AddNew();
					address.OA_Language = l;
					docAddress.E2_OA_Address = address.PK;
					AssertHasMessageError($"Language code: {l} has message error.", docAddress.E2_OA_AddressInfo, message);
				});

				validEnglishLanguages.ForEach(l =>
				{
					var address = addresses.AddNew();
					address.OA_Language = l;
					docAddress.E2_OA_Address = address.PK;
					AssertNoMessageError($"Language code: {l} has no message error.", docAddress.E2_OA_AddressInfo, message);
				});
			});
		}

		JPJobDocAddress DocAddress => docAddress ??= Factory.New<JPJobDocAddress>();
		JPJobDocAddress docAddress;
	}
}
