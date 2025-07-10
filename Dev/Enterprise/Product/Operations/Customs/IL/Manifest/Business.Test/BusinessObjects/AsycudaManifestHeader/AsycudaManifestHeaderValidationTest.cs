using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_ManifestNumber_IsMandatory()
		{
			var targetPropertyInfo = header.AMA_ManifestNumberInfo;
			header.AMA_ManifestNumber = ZString.Empty;
			AssertHasMessageErrorContaining("When Manifest Number is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_ManifestNumber = "12345";
			AssertNoMessageErrorContaining("When Manifest Number is not empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_ManifestNumber_SeaManifestNumberFormatMust6digits()
		{
			const string expectedMessageError = "Import Ocean manifest length must be 6 starting with 2 digits of the year (Previous/current/next year).";
			var targetPropertyInfo = header.AMA_ManifestNumberInfo;
			header.AMA_Nature = "IMP";
			header.AMA_ManifestNumber = "1";
			AssertNoMessageError("When TransportMode is not Sea and Manifest Number digit count is not 6", targetPropertyInfo, expectedMessageError);

			header.AMA_TransportMode = "SEA";
			header.Validation.ValidateAMA_ManifestNumber();
			AssertHasMessageError("When TransportMode is Sea and Manifest Number digit count is not 6", targetPropertyInfo, expectedMessageError);

			header.AMA_ManifestNumber = "A23456";
			AssertHasMessageError("When the TransportMode is Sea and not all six characters in the Manifest Number are digits.", targetPropertyInfo, expectedMessageError);

			header.AMA_ManifestNumber = "231456";
			AssertHasMessageError("When the TransportMode is Sea, and the Manifest Number has 6 digits, but the first 2 characters do not match the current year, the previous year, or the next year.", targetPropertyInfo, expectedMessageError);

			header.AMA_ManifestNumber = ZDateTime.Today.Year.ToString().Substring(2) + "3456";
			AssertNoMessageError("When the TransportMode is Sea, and the Manifest Number has 6 digits, and the first 2 characters match the current year.", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckAMA_ManifestNumber_ImportInlandManifestMust16varchars()
		{
			const string expectedMessageError = "Import Inland manifest length must be 16 starting with \"I\".";
			var targetPropertyInfo = header.AMA_ManifestNumberInfo;
			CombineAssertions("When import Manifest, but not Road", () =>
			{
				header.AMA_Nature = "IMP";
				header.AMA_ManifestNumber = "Y24ABCDEFGHIJKLM";
				AssertNoMessageError("When TransportMode is not Road and Manifest Number is 16 characters but does not start with 'I'", targetPropertyInfo, expectedMessageError);

				header.AMA_ManifestNumber = "I";
				AssertNoMessageError("When TransportMode is not Road and Manifest Number is not 16 characters", targetPropertyInfo, expectedMessageError);
			});

			CombineAssertions("When Road Manifest, and import", () =>
			{
				header.AMA_TransportMode = "ROA";
				header.Validation.ValidateAMA_ManifestNumber();
				AssertHasMessageError("When TransportMode is Road and Manifest Number is not 16 characters", targetPropertyInfo, expectedMessageError);

				header.AMA_ManifestNumber = "I08ABCDEFGHIJKLM";
				AssertNoMessageError("When TransportMode is Road and Manifest Number is 16 characters starting with 'I'", targetPropertyInfo, expectedMessageError);

				header.AMA_ManifestNumber = "Y24ABCDEFGHIJKLM";
				AssertHasMessageError("When TransportMode is Road and Manifest Number is 16 characters but does not start with 'I'", targetPropertyInfo, expectedMessageError);
			});

			CombineAssertions("When Road Manifest, but not import", () =>
			{
				header.AMA_Nature = "EXP";
				header.Validation.ValidateAMA_ManifestNumber();
				AssertNoMessageError("When TransportMode is Road and Manifest Number is 16 characters but does not start with 'I'", targetPropertyInfo, expectedMessageError);

				header.AMA_ManifestNumber = "I";
				AssertNoMessageError("When TransportMode is Road and Manifest Number is not 16 characters", targetPropertyInfo, expectedMessageError);
			});
		}

		public void TestCheckAMA_OA_Carrier_IsMandatory()
		{
			var targetPropertyInfo = header.AMA_OA_CarrierInfo;
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasMessageErrorContaining("When Carrier is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "CRR";
			header.AMA_OA_Carrier = orgHeader.PK;
			AssertNoMessageErrorContaining("When Carrier is not empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_Carrier_IsraelVat()
		{
			const string expectedMessageError = "VAT Number is missing for Carrier - Update organization config tab.";
			var targetPropertyInfo = header.AMA_OA_CarrierInfo;
			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "CRR";
			orgAddress.OA_OH = orgHeader.PK;
			CombineAssertions("When Inland transport", () =>
			{
				header.AMA_TransportMode = "ROA";
				AssertNoMessageError("When carrier is empty", targetPropertyInfo, expectedMessageError);

				header.AMA_OA_Carrier = orgAddress.PK;
				AssertHasMessageError("When carrier without VAT", targetPropertyInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew("VAT", "1", "IN");
				header.Validation.ValidateAMA_OA_Carrier();
				AssertHasMessageError("When carrier with VAT but not for Israel", targetPropertyInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew("VAT", "", "IL");
				header.Validation.ValidateAMA_OA_Carrier();
				AssertHasMessageError("When carrier with VAT for Israel without Reg No", targetPropertyInfo, expectedMessageError);
				orgHeader.CustomsCodes.RemoveAndDeleteAll();

				orgHeader.CustomsCodes.AddNew("VAT", "520017146", "IL");
				header.Validation.ValidateAMA_OA_Carrier();
				AssertNoMessageError("When carrier with VAT for Israel with Reg No", targetPropertyInfo, expectedMessageError);
			});

			CombineAssertions("When not Inland transport", () =>
			{
				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				header.AMA_OA_Carrier = ZGuid.Empty;
				header.AMA_TransportMode = "SEA";

				AssertNoMessageError("When carrier is empty", targetPropertyInfo, expectedMessageError);

				header.AMA_OA_Carrier = orgAddress.PK;
				AssertNoMessageError("When carrier without VAT", targetPropertyInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew("VAT", "1", "IN");
				header.Validation.ValidateAMA_OA_Carrier();
				AssertNoMessageError("When carrier with VAT but not for Israel", targetPropertyInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew("VAT", "", "IL");
				header.Validation.ValidateAMA_OA_Carrier();
				AssertNoMessageError("When carrier with VAT for Israel without Reg No", targetPropertyInfo, expectedMessageError);
			});
		}

		public void TestCheckAMA_OA_Declarant_IsMandatory()
		{
			var targetPropertyInfo = header.AMA_OA_DeclarantInfo;
			header.AMA_OA_Declarant = ZGuid.Empty;
			AssertHasMessageErrorContaining("When Declarant is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DEC";

			header.AMA_OA_Declarant = orgHeader.PK;
			AssertNoMessageErrorContaining("When Declarant is not empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_Declarant_ManifestProviderID()
		{
			const string expectedMessageError = "Customs Manifest Provider Code is missing for Declarant - Update organization Config tab";
			var targetPropertyInfo = header.AMA_OA_DeclarantInfo;
			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "DEC";
			orgAddress.OA_OH = orgHeader.PK;

			AssertNoMessageError("When Declarant is empty", targetPropertyInfo, expectedMessageError);

			header.AMA_OA_Declarant = orgAddress.PK;
			AssertHasMessageError("When Declarant without CMP", targetPropertyInfo, expectedMessageError);

			orgHeader.CustomsCodes.AddNew("CMP", "1", "IN");
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError("When Declarant with CMP but not for Israel", targetPropertyInfo, expectedMessageError);

			orgHeader.CustomsCodes.AddNew("CMP", "", "IL");
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError("When Declarant with CMP for Israel without Reg No", targetPropertyInfo, expectedMessageError);
			orgHeader.CustomsCodes.RemoveAndDeleteAll();

			orgHeader.CustomsCodes.AddNew("CMP", "520017146", "IL");
			header.Validation.ValidateAMA_OA_Declarant();
			AssertNoMessageError("When Declarant with CMP for Israel with Reg No", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckAMA_OA_ShippingAgent_IsMandatory()
		{
			const string expectedMessageError = "Shipping Agent is missing, verify Carrier Organization >> Carrier >> Agencies tab.";
			var targetPropertyInfo = header.AMA_OA_ShippingAgentInfo;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_OA_ShippingAgent = ZGuid.Empty;
			AssertHasMessageError("When Shipping Agent is empty and transport mode is SEA", targetPropertyInfo, expectedMessageError);

			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "SA";

			header.AMA_OA_ShippingAgent = orgHeader.PK;
			AssertNoMessageError("When Shipping Agent is not empty and transport mode is SEA", targetPropertyInfo, expectedMessageError);

			header.AMA_TransportMode = TransportTypeList.Codes.Road;
			header.AMA_OA_ShippingAgent = ZGuid.Empty;
			AssertNoMessageError("When Shipping Agent is empty and transport mode is ROA", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckAMA_OA_ShippingAgent_CarrierCode()
		{
			const string expectedMessageError = "Customs Carrier Code is missing for Shipping Agent - Update organization Config tab";
			var targetPropertyInfo = header.AMA_OA_ShippingAgentInfo;
			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "SA";
			orgAddress.OA_OH = orgHeader.PK;

			AssertNoMessageError("When Shipping Agent is empty", targetPropertyInfo, expectedMessageError);

			header.AMA_OA_ShippingAgent = orgAddress.PK;
			AssertHasMessageError("When Shipping Agent without CCC", targetPropertyInfo, expectedMessageError);

			orgHeader.CustomsCodes.AddNew("CCC", "1", "IN");
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertHasMessageError("When Shipping Agent with CMP but not for Israel", targetPropertyInfo, expectedMessageError);

			orgHeader.CustomsCodes.AddNew("CCC", "", "IL");
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertHasMessageError("When Shipping Agent with CCC for Israel without Reg No", targetPropertyInfo, expectedMessageError);
			orgHeader.CustomsCodes.RemoveAndDeleteAll();

			orgHeader.CustomsCodes.AddNew("CCC", "520017146", "IL");
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertNoMessageError("When Shipping Agent with CCC for Israel with Reg No", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckAMA_VehicleRegistration()
		{
			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			header2.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header2.AMA_VehicleRegistration = "";
			AssertHasMessageErrorContaining("When the Manifest header is not Israel", header2.AMA_VehicleRegistrationInfo, "Vehicle Registration Number is mandatory for Transport Mode Road");

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_VehicleRegistration = "";
			AssertNoMessageErrorContaining("When the Manifest header is Israel", header.AMA_VehicleRegistrationInfo, "Vehicle Registration Number is mandatory for Transport Mode Road");
		}

		public void TestValidateAtLeastOnTransportMeansRecordIsRequired()
		{
			const string expectedMessageError = "At Least one Transport Means record is required in road manifest.";
			var header = Factory.New<AsycudaManifestHeader>();
			header.Validation.ValidateAll();
			AssertNoRowMessageError("When TransportMeans.Count == 0 and TransportMode is not Road", header, expectedMessageError);
			header.AMA_TransportMode = TransportTypeList.Codes.Road;
			header.Validation.ValidateAll();
			AssertHasRowMessageError("When TransportMeans.Count == 0 and TransportMode is Road", header, expectedMessageError);
			header.TransportMeans.AddNew();
			header.Validation.ValidateAll();
			AssertNoRowMessageError("When TransportMeans.Count > 0 and TransportMode is Road", header, expectedMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader header;
	}
}
