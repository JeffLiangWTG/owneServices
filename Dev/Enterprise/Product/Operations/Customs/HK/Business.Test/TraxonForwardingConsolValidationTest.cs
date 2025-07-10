using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.HK.Business.Testing
{
	class TraxonForwardingConsolValidationTest : TestCaseWithFactory
	{
		public void TestValidateTraxonOutputDirectory()
		{
			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath);
			Assert("Output directory Validation Error has occurred", TraxonForwardingConsolValidation.IsTraxonOutputDirectoryValid(GlbCompany.CurrentCompany.FirstActiveBranch));
			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZString.Empty);
			Assert("Output directory Validation Error has not occurred", !TraxonForwardingConsolValidation.IsTraxonOutputDirectoryValid(GlbCompany.CurrentCompany.FirstActiveBranch));
			HKDataRegistry.Instance.ISACFTPServerOutputAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ftp://a.com");
			Assert("Output directory Validation Error has occurred", TraxonForwardingConsolValidation.IsTraxonOutputDirectoryValid(GlbCompany.CurrentCompany.FirstActiveBranch));
		}

		public void TestValidateCOSACCode()
		{
			HKDataRegistry.Instance.CosacAgentCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342");
			Assert("COSAC Validation Error has occurred", TraxonForwardingConsolValidation.IsCOSACCodeValid(GlbCompany.CurrentCompany.FirstActiveBranch));
			HKDataRegistry.Instance.CosacAgentCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZString.Empty);
			Assert("COSAC Validation Error has not occurred", !TraxonForwardingConsolValidation.IsCOSACCodeValid(GlbCompany.CurrentCompany.FirstActiveBranch));
		}

		public void TestValidateTraxonSenderID()
		{
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			Assert("Sender ID Validation Error has occurred", TraxonForwardingConsolValidation.IsTraxonSenderIDValid(GlbCompany.CurrentCompany.FirstActiveBranch));
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZString.Empty);
			Assert("Sender ID Validation Error has not occurred", !TraxonForwardingConsolValidation.IsTraxonSenderIDValid(GlbCompany.CurrentCompany.FirstActiveBranch));
		}

		public void TestValidateTraxonRecipientReference()
		{
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");
			Assert("Recipient Reference Validation Error has occurred", TraxonForwardingConsolValidation.IsTraxonRecipientReferencePasswordValid(GlbCompany.CurrentCompany.FirstActiveBranch));
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZString.Empty);
			Assert("Recipient Reference Validation Error has not occurred", !TraxonForwardingConsolValidation.IsTraxonRecipientReferencePasswordValid(GlbCompany.CurrentCompany.FirstActiveBranch));
		}

		public void TestValidateFlightNumber()
		{
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "CX102";
			Assert("Flight Number Validation Error has occurred", testValidation.IsFlightNumberValid);
			transport.JW_VoyageFlight = ZString.Empty;
			Assert("Flight Number Validation Error has not occurred", !testValidation.IsFlightNumberValid);
		}

		public void TestValidateETA()
		{
			var transport = testConsol.Transports[0];
			transport.JW_ETA = new ZDateTime(Env.Time.CurrentLocalDate);
			Assert("Flight ETA Validation Error has occurred", testValidation.IsETAValid);
			transport.JW_ETA = ZDateTime.Empty;
			Assert("Flight ETA Validation Error has not occurred", !testValidation.IsETAValid);
		}

		public void TestValidateMAWB()
		{
			testConsol.JK_MasterBillNum = "16068974393";
			Assert("MAWB Validation Error has occurred", testValidation.IsMAWBValid);
			testConsol.JK_MasterBillNum = ZString.Empty;
			Assert("MAWB Validation Error has not occurred", !testValidation.IsMAWBValid);
		}

		public void TestValidateShipmentCount()
		{
			Assert("Shipment Count Validation Validation Error has occurred", testValidation.IsShipmentCountValid);
			ForwardingConsol testConsol2 = Factory.New<ForwardingConsol>();
			testValidation = new TraxonForwardingConsolValidation(testConsol2);
			Assert("Shipment Count Validation Error has not occurred", !testValidation.IsShipmentCountValid);
		}

		public void TestValidateHAWB()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_HouseBill = "TEST123";
			Assert("HAWB Validation Error has occurred", TraxonForwardingConsolValidation.IsHAWBValid(shipment));
			shipment.JS_HouseBill = ZString.Empty;
			Assert("HAWB Validation Error has not occurred", !TraxonForwardingConsolValidation.IsHAWBValid(shipment));
		}

		public void TestValidateGoodsDescription()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_GoodsDescription = "TEST123";
			Assert("HAWB Validation Error has occurred", TraxonForwardingConsolValidation.IsGoodsDescriptionValid(shipment));
			shipment.JS_GoodsDescription = ZString.Empty;
			Assert("HAWB Validation Error has not occurred", !TraxonForwardingConsolValidation.IsGoodsDescriptionValid(shipment));
		}

		public void TestLicenceLength()
		{
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "HKHKG";
			Assert(TraxonForwardingConsolValidation.IsHKImport(shipment));

			var importLicense = shipment.CusEntryNumbers.AddNew();
			importLicense.CE_EntryNum = "123456789012345678901";
			importLicense.CE_EntryType = Enterprise.Customs.Common.CusEntryNumberTypes.HongKong.ImportLicense;

			var sb = new StringBuilder();
			TraxonForwardingConsolValidation.ValidateLicenseNumbers(shipment, m => sb.AppendLine(m));
			AssertContains("ISAC licence number (123456789012345678901) should not be longer than 20 in length.", sb.ToString());
		}

		public void TestValidateTotalPackageCount()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_TotalPackageCount = 123;
			Assert("Package count Validation Error has occurred", !TraxonForwardingConsolValidation.IsTotalPackageCountValid(shipment));
			shipment.JS_TotalPackageCount = 0;
			shipment.JS_OuterPacks = 10;
			Assert("Pacakge Count Validation Error has occurred", TraxonForwardingConsolValidation.IsTotalPackageCountValid(shipment));
			shipment.JS_TotalPackageCount = 0;
			shipment.JS_OuterPacks = 0;
			Assert("Pacakge Count Validation Error has not occurred", !TraxonForwardingConsolValidation.IsTotalPackageCountValid(shipment));
			shipment.JS_TotalPackageCount = 999999999;
			shipment.JS_OuterPacks = 0;
			Assert("Package Validation Error has not occurred", !TraxonForwardingConsolValidation.IsTotalPackageCountValid(shipment));
			shipment.JS_TotalPackageCount = 0;
			shipment.JS_OuterPacks = 999999999;
			Assert("Package Validation Error has not occurred", !TraxonForwardingConsolValidation.IsTotalPackageCountValid(shipment));
		}

		public void TestValidateActualWeight()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_ActualWeight = 12.4m;
			shipment.JS_UnitOfWeight = ZString.Empty;
			Assert("Weight Validation Error has not occurred", !TraxonForwardingConsolValidation.IsActualWeightValid(shipment));
			shipment.JS_ActualWeight = 200m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			Assert("Weight Validation Error has occurred", TraxonForwardingConsolValidation.IsActualWeightValid(shipment));
			shipment.JS_ActualWeight = 0.01m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			Assert("Weight Validation Error has not occurred", !TraxonForwardingConsolValidation.IsActualWeightValid(shipment));
			shipment.JS_ActualWeight = 999999.6m;
			Assert("Weight Validation Error has not occurred", !TraxonForwardingConsolValidation.IsActualWeightValid(shipment));
		}

		public void TestValidateConsigneeIsNull()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			var sb = new StringBuilder();

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertContains("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789" + TraxonForwardingConsolValidation.ConsigneeError, sb.ToString());

			OrgHeader consigneeTest = GetConsigneeAddress();
			consigneeTest.OH_FullName = ZString.Empty;
			shipment.ConsigneePK = consigneeTest.PK;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertContains("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee" + TraxonForwardingConsolValidation.NameError, sb.ToString());
		}

		public void TestValidateConsigneeName()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			consigneeTest.OH_FullName = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee name may not be blank." + System.Environment.NewLine, sb.ToString());

			consigneeTest.OH_FullName = "ABC";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.ConsigneeDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee name may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			consigneeTest.OH_FullName = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee name may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.ConsigneeDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee name may not be blank." + System.Environment.NewLine, sb.ToString());

			consigneeTest.OH_FullName = "XYZ";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeAddress()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			consigneeTest.MainAddress.OA_Address1 = ZString.Empty;
			consigneeTest.MainAddress.OA_Address2 = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee address may not be blank." + System.Environment.NewLine, sb.ToString());

			consigneeTest.MainAddress.OA_Address1 = "ABC";
			consigneeTest.MainAddress.OA_Address2 = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee address may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ABC";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			consigneeTest.MainAddress.OA_Address1 = ZString.Empty;
			consigneeTest.MainAddress.OA_Address2 = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee address may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee address may not be blank." + System.Environment.NewLine, sb.ToString());

			consigneeTest.MainAddress.OA_Address1 = "ABC";
			consigneeTest.MainAddress.OA_Address2 = "DEF";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeCity()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			consigneeTest.MainAddress.OA_City = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee city may not be blank." + System.Environment.NewLine, sb.ToString());

			consigneeTest.MainAddress.OA_City = "ABC";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ABC";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "DEF";
			shipment.ConsigneeDocumentaryAddress.E2_City = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());

			consigneeTest.MainAddress.OA_City = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee city may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee city may not be blank." + System.Environment.NewLine, sb.ToString());

			consigneeTest.MainAddress.OA_City = "XYZ";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeCountry()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());

			consigneeTest.OH_RL_NKClosestPort = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.Address.OA_RN_NKCountryCode = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee country may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.ConsigneeDocumentaryAddress.E2_State = "IL";
			shipment.ConsigneeDocumentaryAddress.City = "Chicago";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());

			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignee country may not be blank." + System.Environment.NewLine, sb.ToString());
		}

		public void TestValidateUnmatchedOrgs()
		{
			var sb = new StringBuilder();

			shipment.JS_UniqueConsignRef = "123456789";
			shipment.ConsigneePK = OrgHeader.UnmatchedOrganisationPK;
			shipment.ConsignorPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.Notes.AddNew(false, "Unmatched Org Details", EmptyUnmatchedOrgDetails);

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertMultilineASCIIEquals("Consignee Validation Error has occurred",
@"Error on Shipment (ISAC): 123456789 Consignee name may not be blank.
Error on Shipment (ISAC): 123456789 Consignee address may not be blank.
Error on Shipment (ISAC): 123456789 Consignee city may not be blank.
Error on Shipment (ISAC): 123456789 Consignee country may not be blank.",
				sb.ToString());

			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertMultilineASCIIEquals("Consignor Validation Error has occurred",
@"Error on Shipment (ISAC): 123456789 Consignor name may not be blank.
Error on Shipment (ISAC): 123456789 Consignor address may not be blank.
Error on Shipment (ISAC): 123456789 Consignor city may not be blank.
Error on Shipment (ISAC): 123456789 Consignor country may not be blank.",
				sb.ToString());

			shipment.Notes.RemoveAndDeleteAll();
			shipment.Notes.AddNew(false, "Unmatched Org Details", UnmatchedOrgDetails);

			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", "", sb.ToString());

			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", "", sb.ToString());
		}

		public void TestValidateConsignorIsNull()
		{
			var sb = new StringBuilder();

			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789" + TraxonForwardingConsolValidation.ConsignorError + System.Environment.NewLine, sb.ToString());

			OrgHeader consignorTest = GetConsignorAddress();
			consignorTest.OH_FullName = ZString.Empty;
			shipment.ConsignorPK = consignorTest.PK;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor name may not be blank." + System.Environment.NewLine, sb.ToString());
		}

		public void TestValidateConsignorName()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.OH_FullName = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor name may not be blank." + System.Environment.NewLine, sb.ToString());

			consignorTest.OH_FullName = "ABC";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.ConsignorDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor name may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.OH_FullName = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor name may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.ConsignorDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor name may not be blank." + System.Environment.NewLine, sb.ToString());

			consignorTest.OH_FullName = "XYZ";
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsignorAddress()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.MainAddress.OA_Address1 = ZString.Empty;
			consignorTest.MainAddress.OA_Address2 = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor address may not be blank." + System.Environment.NewLine, sb.ToString());

			consignorTest.MainAddress.OA_Address1 = "ABC";
			consignorTest.MainAddress.OA_Address2 = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_Address1 = "";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor address may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ABC";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.MainAddress.OA_Address1 = ZString.Empty;
			consignorTest.MainAddress.OA_Address2 = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor address may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor address may not be blank." + System.Environment.NewLine, sb.ToString());

			consignorTest.MainAddress.OA_Address1 = "ABC";
			consignorTest.MainAddress.OA_Address2 = "DEF";
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsignorCity()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.MainAddress.OA_City = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor city may not be blank." + System.Environment.NewLine, sb.ToString());

			consignorTest.MainAddress.OA_City = "ABC";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_City = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor city may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_City = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.MainAddress.OA_City = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor city may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ABCD";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor city may not be blank." + System.Environment.NewLine, sb.ToString());

			consignorTest.MainAddress.OA_City = "XYZ";
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsignorCountry()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", "", sb.ToString());

			consignorTest.OH_RL_NKClosestPort = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.Address.OA_RN_NKCountryCode = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor country may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.ConsignorDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", "", sb.ToString());

			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor country may not be blank." + System.Environment.NewLine, sb.ToString());
		}

		public void TestValidateConsignorState()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "USLAX";
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			consignorTest.MainAddress.OA_State = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Consignor state may not be blank." + System.Environment.NewLine, sb.ToString());
		}

		public void TestValidateConsignorContactPhoneNumber()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "USLAX";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.EH_ShipperContactDetail = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details are not mandatory, but Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsignorContact()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "USLAX";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.EH_ShipperContactCode = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details not mandatory, but Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsignorAWBExistsUS()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "USLAX";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.Delete();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details not mandatory, but Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsignorAWBExistsCA()
		{
			var sb = new StringBuilder();

			OrgHeader consignorTest = GetConsignorAddress();
			shipment.ConsignorPK = consignorTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "CATOR";

			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignor Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.Delete();
			TraxonForwardingConsolValidation.ValidateConsignorInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details not required, but Consignor Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeState()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			consigneeTest.MainAddress.OA_State = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + shipment.JS_UniqueConsignRef + " Consignee state may not be blank." + System.Environment.NewLine, sb.ToString());
		}

		public void TestValidateConsigneeContactPhoneNumber()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.EH_ConsigneeContactDetail = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details not required, but Consignee Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeContact()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.EH_ConsigneeContactCode = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details are not required, but Consignee Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeAWBExistsUS()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.Delete();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact details are not mandatory, but Consignee Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateConsigneeAWBExistsCA()
		{
			var sb = new StringBuilder();

			OrgHeader consigneeTest = GetConsigneeAddress();
			shipment.ConsigneePK = consigneeTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "CATOR";
			shipment.JS_RL_NKOrigin = "HKHKG";

			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Consignee Validation Error has occurred", "", sb.ToString());

			shipment.AWBHeader.Delete();
			TraxonForwardingConsolValidation.ValidateConsigneeInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Contact Details are not required, but Consignee Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateNotifyPartyIsNull()
		{
			var sb = new StringBuilder();

			shipment.JS_UniqueConsignRef = "123456789";
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("Notify party is not mandatory so there should be no errors", "", sb.ToString());

			OrgContact notifyContactTest = GetNotifyAddress();
			var notifyTest = Factory.Load<OrgHeader>(notifyContactTest.OC_OH);
			notifyTest.OH_FullName = ZString.Empty;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyTest.PK;
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("There should be an error as the Notify Party name is not entered.", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party name may not be blank." + System.Environment.NewLine, sb.ToString());
		}

		public void TestValidateNotifyPartyName()
		{
			var sb = new StringBuilder();

			OrgContact notifyContactTest = GetNotifyAddress();
			var notifyTest = Factory.Load<OrgHeader>(notifyContactTest.OC_OH);
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyContactTest.OC_OH;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			notifyTest.OH_FullName = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party name may not be blank." + System.Environment.NewLine, sb.ToString());

			notifyTest.OH_FullName = "ABC";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party name may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			notifyTest.OH_FullName = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party name may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party name may not be blank." + System.Environment.NewLine, sb.ToString());

			notifyTest.OH_FullName = "XYZ";
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateNotifyPartyAddress()
		{
			var sb = new StringBuilder();

			OrgContact notifyContactTest = GetNotifyAddress();
			var notifyTest = Factory.Load<OrgHeader>(notifyContactTest.OC_OH);
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyContactTest.OC_OH;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			notifyTest.MainAddress.OA_Address1 = ZString.Empty;
			notifyTest.MainAddress.OA_Address2 = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party address may not be blank." + System.Environment.NewLine, sb.ToString());

			notifyTest.MainAddress.OA_Address1 = "ABC";
			notifyTest.MainAddress.OA_Address2 = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "";
			shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party address may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABC";
			shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			notifyTest.MainAddress.OA_Address1 = ZString.Empty;
			notifyTest.MainAddress.OA_Address2 = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party address may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party address may not be blank." + System.Environment.NewLine, sb.ToString());

			notifyTest.MainAddress.OA_Address1 = "ABCD";
			notifyTest.MainAddress.OA_Address2 = "DEF";
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateNotifyPartyCity()
		{
			var sb = new StringBuilder();

			OrgContact notifyContactTest = GetNotifyAddress();
			var notifyTest = Factory.Load<OrgHeader>(notifyContactTest.OC_OH);
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyContactTest.OC_OH;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			notifyTest.MainAddress.OA_City = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party city may not be blank." + System.Environment.NewLine, sb.ToString());

			notifyTest.MainAddress.OA_City = "ABC";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_City = "";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party city may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_City = "DEF";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());

			notifyTest.MainAddress.OA_City = ZString.Empty;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party city may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABCD";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party city may not be blank." + System.Environment.NewLine, sb.ToString());

			notifyTest.MainAddress.OA_City = "XYZ";
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", "", sb.ToString());
		}

		public void TestValidateNotifyPartyCountry()
		{
			var sb = new StringBuilder();

			OrgContact notifyContactTest = GetNotifyAddress();
			var notifyTest = Factory.Load<OrgHeader>(notifyContactTest.OC_OH);
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyTest.PK;
			shipment.JS_UniqueConsignRef = "123456789";

			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", "", sb.ToString());

			notifyTest.OH_RL_NKClosestPort = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", "", sb.ToString());

			notifyTest.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has occurred", TraxonForwardingConsolValidation.ShipmentError + "123456789 Notify Party country may not be blank." + System.Environment.NewLine, sb.ToString());

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "ABCD";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "CITY";
			sb.Clear();
			TraxonForwardingConsolValidation.ValidateNotifyPartyInformation(shipment, m => sb.AppendLine(m));
			AssertEquals("NotifyParty Validation Error has not occurred", "", sb.ToString());
		}

		public void TestValidateMOACurrencyImport()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RX_NKGoodsValueCurr = "AUD";
			Assert("MOA Currency Import Validation Error has occurred", TraxonForwardingConsolValidation.IsGoodsValueCurrencyValid(shipment));

			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			Assert("MOA Currency Import Validation Error has not occurred", !TraxonForwardingConsolValidation.IsGoodsValueCurrencyValid(shipment));
		}

		public void TestValidate_WithNullAWBHeader()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.AWBHeader.EH_Currency = "AUD";
			shipment.JS_TransportMode = "ROA";
			Assert("MOA Currency Import Validation Error has not occurred", !TraxonForwardingConsolValidation.IsMOACurrencyValid(shipment));
			Assert("Customs Insurance Export Validation Error has occurred", !TraxonForwardingConsolValidation.IsInsuranceValueValid(shipment));
			Assert("Customs Value Export Validation Error has not occurred", !TraxonForwardingConsolValidation.IsCustomsValueValid(shipment));
			Assert("Customs Carriage Export Validation Error has not occurred", !TraxonForwardingConsolValidation.IsCarriageValueValid(shipment));
		}

		public void TestValidateMOACurrencyExport()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.AWBHeader.EH_Currency = "AUD";
			Assert("MOA Currency Import Validation Error has occurred", TraxonForwardingConsolValidation.IsMOACurrencyValid(shipment));

			shipment.AWBHeader.EH_Currency = ZString.Empty;
			Assert("MOA Currency Import Validation Error has not occurred", !TraxonForwardingConsolValidation.IsMOACurrencyValid(shipment));
		}

		public void TestValidateCustomsValueExport()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.AWBHeader.EH_CustomsValue = 134.56m;
			Assert("Customs Value Export Validation Error has occurred", TraxonForwardingConsolValidation.IsCustomsValueValid(shipment));

			shipment.AWBHeader.EH_CustomsValue = 9999999999999m;
			Assert("Customs Value Export Validation Error has not occurred", !TraxonForwardingConsolValidation.IsCustomsValueValid(shipment));
		}

		public void TestValidateCustomsValueImport()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_GoodsValue = 134.56m;
			Assert("Customs Value Import Validation Error has occurred", TraxonForwardingConsolValidation.IsGoodsValueValid(shipment));

			shipment.JS_GoodsValue = 9999999999999m;
			Assert("Customs Value Import Validation Error has not occurred", !TraxonForwardingConsolValidation.IsGoodsValueValid(shipment));
		}

		public void TestValidateCustomsCarriageExport()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.AWBHeader.EH_DeclaredValue = 134.56m;
			Assert("Customs Carriage Export Validation Error has occurred", TraxonForwardingConsolValidation.IsCarriageValueValid(shipment));

			shipment.AWBHeader.EH_DeclaredValue = 9999999999999m;
			Assert("Customs Carriage Export Validation Error has not occurred", !TraxonForwardingConsolValidation.IsCarriageValueValid(shipment));
		}

		public void TestValidateInsuranceValueExport()
		{
			shipment.JS_UniqueConsignRef = "123456789";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.AWBHeader.EH_InsuranceValue = 134.56m;
			Assert("Customs Insurance Export Validation Error has occurred", TraxonForwardingConsolValidation.IsInsuranceValueValid(shipment));

			shipment.AWBHeader.EH_InsuranceValue = 9999999999999m;
			Assert("Customs Insurance Export Validation Error has not occurred", !TraxonForwardingConsolValidation.IsInsuranceValueValid(shipment));
		}

		public void TestValidate_WithAWBHeaderIsDeleted()
		{
			shipment.AWBHeader.EH_CustomsValue = 134.56m;
			var awbHeader = shipment.AWBHeader;
			Assert("Customs Value Export Validation Error has occurred", TraxonForwardingConsolValidation.IsCustomsValueValid(awbHeader));
			awbHeader.Delete();
			Assert("Customs Value Export Validation Error has not occurred", !TraxonForwardingConsolValidation.IsCustomsValueValid(awbHeader));
		}

		public void TestValidateOtherCustomsDataOnBill()
		{
			var awbHeader = shipment.AWBHeader;
			shipment.JS_UniqueConsignRef = "S00000813";

			var contactInfos = new[]
			{
					awbHeader.EH_ConsigneeContactNameInfo,
					awbHeader.EH_ConsigneeContactDetailInfo,
					awbHeader.EH_ShipperContactDetailInfo,
				};

			foreach (var info in contactInfos)
			{
				info.ClearValue();
			}

			var errors = new ZStringBuilder();
			TraxonForwardingConsolValidation.ValidateOtherCustomsDataOnBill(shipment, s => errors.AppendLine(s));

			var expectedErrors = @"Error on Shipment (ISAC): S00000813 The Consignee Contact Name on air way bill may not be empty.
Error on Shipment (ISAC): S00000813 The Consignee Contact Details on air way bill may not be empty.
Error on Shipment (ISAC): S00000813 The Shipper Contact Details on air way bill may not be empty.
";
			var actualErorrs = errors.ToString();

			AssertEquals("Should add these errors as their value are empty.", expectedErrors, actualErorrs);

			foreach (var info in contactInfos)
			{
				info.SetValueFromString("TEST INFO");
			}

			errors = new ZStringBuilder();
			TraxonForwardingConsolValidation.ValidateOtherCustomsDataOnBill(shipment, s => errors.AppendLine(s));

			Assert("Shoud not add any errors as all values are not empty.", errors.IsEmpty);
		}

		public void TestValidateACASAccountDetailsOnBill()
		{
			var awbHeader = shipment.AWBHeader;
			shipment.JS_UniqueConsignRef = "S00000813";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = "HKHKG";

			var shipment2 = testConsol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00000814";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_RL_NKOrigin = "HKHKG";
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;

			var errors = new ZStringBuilder();
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				TraxonForwardingConsolValidation.ValidateACASAccountDetails(testConsol, s => errors.AppendLine(s));
			}

			var expectedAccountHolderAndNameErrors = @"Warning - SendISAC: S00000813, S00000814 - The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline.";
			var expectedAccountIssuerAndNumberErrors = @"Warning - SendISAC: S00000813, S00000814 - The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline.";
			var actualErrors = errors.ToString().Trim();

			AssertContains("Should contain account holder and name errors as some values are missing.", expectedAccountHolderAndNameErrors, actualErrors);
			AssertContains("Should contain account issuer and number errors as some values are missing.", expectedAccountIssuerAndNumberErrors, actualErrors);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";
			controllingCustomer.OH_RL_NKClosestPort = "HKHKG";
			controllingCustomer.OH_FullName = "Customs Agents";
			controllingCustomer.MainAddress.OA_Address1 = "CC MAIN ADDRESS";
			shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

			var awbHeader1 = shipment2.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = "PPD";
			awbHeader1.EH_ShipperName = "HK Shipper";
			awbHeader1.EH_ShipperAccount = "ShipperAccount";

			errors = new ZStringBuilder();
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				TraxonForwardingConsolValidation.ValidateACASAccountDetails(testConsol, s => errors.AppendLine(s));
			}

			Assert("Should not contain any errors as all values are supplied.", errors.IsEmpty);
		}

		public void TestIsProcessingISACMessageValid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				AssertEquals("Null branch", false, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(null));

				var company = Factory.New<GlbCompany>();
				company.GC_Code = "TWC";
				var branch = company.Branches.AddNew();
				branch.GB_Code = "TWB";
				Factory.Save();

				using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
				using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "AGENT"))
				using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "SENDERID"))
				using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RECIPREF"))
				{
					AssertEquals("All Valid with Path", true, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch));

					using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
					{
						AssertEquals("All Valid via eHub", true, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch));
					}

					using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
					{
						AssertEquals("No path, no eHub", false, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch));
					}

					using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
					{
						AssertEquals("No agent", false, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch));
					}

					using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
					{
						AssertEquals("No SenderID", false, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch));
					}

					using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
					{
						AssertEquals("No password", false, TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch));
					}
				}
			}
		}

		ForwardingConsol testConsol;
		TraxonForwardingConsolValidation testValidation;
		ForwardingShipment shipment;
		protected override void SetUp()
		{
			base.SetUp();
			testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00003134";
			testConsol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport = testConsol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			testValidation = new TraxonForwardingConsolValidation(testConsol);
			shipment = testConsol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			storedBranchCode = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "HKHKG";
		}

		const string UnmatchedOrgDetails = @"
<UnmatchOrgRecords>
	<UnmatchOrgRecord>
		<OrganisationType>Consignee</OrganisationType>
		<OrganisationSubType>Consignee</OrganisationSubType>
		<OwnerCode>CONSIGNEE</OwnerCode>
		<EDICode>CONSIGNEE</EDICode>
		<OrganisationName>CONSIGNEE NAME</OrganisationName>
		<AddressLine1>TEST CONSIGNEE ADDRESS 1</AddressLine1>
		<AddressLine2>TEST CONSIGNEE ADDRESS 2</AddressLine2>
		<City>TEST CONSIGNEE CITY</City>
		<PostCode>9999</PostCode>
		<Country>HK</Country>
	</UnmatchOrgRecord>
	<UnmatchOrgRecord>
		<OrganisationType>Consignor</OrganisationType>
		<OrganisationSubType>Consignor</OrganisationSubType>
		<OwnerCode>CONSIGNOR</OwnerCode>
		<EDICode>CONSIGNOR</EDICode>
		<OrganisationName>CONSIGNOR NAME</OrganisationName>
		<AddressLine1>TEST CONSIGNOR ADDRESS 1</AddressLine1>
		<AddressLine2/>
		<City>TEST CONSIGNOR CITY</City>
		<PostCode>2222</PostCode>
		<StateOrProvince>NSW</StateOrProvince>
		<Country>AU</Country>
	</UnmatchOrgRecord>
</UnmatchOrgRecords>";

		const string EmptyUnmatchedOrgDetails = @"
<UnmatchOrgRecords>
	<UnmatchOrgRecord>
		<OrganisationType>Consignee</OrganisationType>
		<OrganisationSubType>Consignee</OrganisationSubType>
		<OwnerCode></OwnerCode>
		<EDICode></EDICode>
		<OrganisationName></OrganisationName>
		<AddressLine1></AddressLine1>
		<AddressLine2></AddressLine2>
		<City></City>
		<PostCode></PostCode>
		<Country></Country>
	</UnmatchOrgRecord>
	<UnmatchOrgRecord>
		<OrganisationType>Consignor</OrganisationType>
		<OrganisationSubType>Consignor</OrganisationSubType>
		<OwnerCode></OwnerCode>
		<EDICode></EDICode>
		<OrganisationName></OrganisationName>
		<AddressLine1></AddressLine1>
		<AddressLine2/>
		<City></City>
		<PostCode></PostCode>
		<StateOrProvince></StateOrProvince>
		<Country></Country>
	</UnmatchOrgRecord>
</UnmatchOrgRecords>";

		ZString storedBranchCode;
		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = storedBranchCode;
		}

		OrgHeader GetConsigneeAddress()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "ACO SEWINGART LTD";
			result.MainAddress.OA_Address1 = "RM 833 METRO CENTER 2, 21 KING";
			result.MainAddress.OA_Address2 = "ST., CHICAGO";
			result.MainAddress.OA_City = "CHICAGO";
			result.MainAddress.OA_State = "IL";
			result.OH_RL_NKClosestPort = "USCHI";
			AddAWB();
			return result;
		}

		OrgHeader GetConsignorAddress()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "ACO HELLO GEOFF";
			result.MainAddress.OA_Address1 = "RM 435 THE POINT, 25 OXFORD";
			result.MainAddress.OA_Address2 = "ST., NEW YORK";
			result.MainAddress.OA_City = "NEW YORK";
			result.MainAddress.OA_State = "WA";
			result.OH_RL_NKClosestPort = "USNYC";
			AddAWB();
			return result;
		}

		OrgContact GetNotifyAddress()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "LESPORTSAC INC";
			result.MainAddress.OA_Address1 = "320 FIFTH AVE";
			result.MainAddress.OA_City = "NEW YORK NY 10001";
			result.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Scott Test";
			contact.OC_OH = result.PK;
			return contact;
		}

		void AddAWB()
		{
			shipment.JS_OverrideWaybillDefaults = ZBool.True;
			shipment.AWBHeader.EH_ConsigneeContactCode = "TE";
			shipment.AWBHeader.EH_ConsigneeContactDetail = "08 8932 2740";
			shipment.AWBHeader.EH_ShipperContactCode = "TE";
			shipment.AWBHeader.EH_ShipperContactDetail = "08 8932 3456";
		}
	}
}
