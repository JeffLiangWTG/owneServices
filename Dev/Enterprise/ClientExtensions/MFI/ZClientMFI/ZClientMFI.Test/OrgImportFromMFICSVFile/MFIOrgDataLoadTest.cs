using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.OrgImportFromMFICSVFile.Testing
{
	class MFIOrgDataLoadTest : TestCaseWithFactory
	{
		public void TestImportData()
		{
			int noOfOrganisation = Factory.GetDatabaseCount(typeof(OrgHeader));
			MFIOrgDataLoad dataLoad = new MFIOrgDataLoad();
			AssertNull("org with legacy code '914228' doesn't exist", GetOrgHeader("914228", OrgCusCode.CodeTypes.LegacySystemCode));
			AssertNull("org with legacy code '1129273' doesn't exist", GetOrgHeader("1129273", OrgCusCode.CodeTypes.LegacySystemCode));
			AssertNull("org with legacy code '205390' doesn't exist", GetOrgHeader("205390", OrgCusCode.CodeTypes.LegacySystemCode));

			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("OrgImportFromMFICSFile.Organisation Sample File.csv");
				dataLoad.ImportData(testFilePath, "Organisation");
				AssertEquals("3 organisations are created", noOfOrganisation + 3, Factory.GetDatabaseCount(typeof(OrgHeader)));
				OrgHeader org914228 = GetOrgHeader("914228", OrgCusCode.CodeTypes.LegacySystemCode);
				OrgHeader org1129273 = GetOrgHeader("1129273", OrgCusCode.CodeTypes.LegacySystemCode);
				OrgHeader org205390 = GetOrgHeader("205390", OrgCusCode.CodeTypes.LegacySystemCode);
				AssertNotNull("org with legacy code '914228' is created", org914228);
				AssertNotNull("org with legacy code '1129273' is created", org1129273);
				AssertNotNull("org with legacy code '205390' is created", org205390);
				AssertNotNull("branch", org914228.Branch);
				AssertEquals("branch code", "BNE", org914228.Branch.GB_Code);
				AssertNotNull("Staff assignment shouldn't be null", org914228.StaffAssignments.OverallSalesRepStaff);
				AssertEquals("Overall Sales Rep", StaffABC.GS_Code, org914228.StaffAssignments.OverallSalesRepStaff.GS_Code);
				AssertNotNull("Staff assignment shouldn't be null", org914228.StaffAssignments.OverallCustomerServiceRepStaff);
				AssertEquals("Overall customer service Rep", StaffDEF.GS_Code, org914228.StaffAssignments.OverallCustomerServiceRepStaff.GS_Code);
				OrgHeader controllingCustomer = org914228.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.PickupAndDelivery);
				AssertNotNull("Controlling customer", controllingCustomer);
				AssertEquals("Controlling customer organisation", org205390.PK, controllingCustomer.PK);
				OrgHeader invoiceFrtToOrg = org914228.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.PickupAndDelivery);
				AssertNotNull("invoice freight to Org is not null", invoiceFrtToOrg);
				AssertEquals("invoice freight to org", org1129273.PK, invoiceFrtToOrg.PK);
				OrgCusCode aPC = org914228.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, GlbCompany.CurrentCompany.Country);
				AssertNotNull("APC", aPC);
				AssertEquals("APC code", "205390", aPC.OK_CustomsRegNo);
				OrgCusCode uOC = org914228.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.UniversalOfficeCode, GlbCompany.CurrentCompany.Country);
				AssertNotNull("UOC", uOC);
				AssertEquals("UOC code", "UOC#", uOC.OK_CustomsRegNo);
				OrgCusCode dUN = org914228.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, GlbCompany.CurrentCompany.Country);
				AssertNotNull("DUN", dUN);
				AssertEquals("DUN code", "DUN#", dUN.OK_CustomsRegNo);
				OrgCusCode eIN = org914228.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, GlbCompany.CurrentCompany.Country);
				AssertNotNull("EIN", eIN);
				AssertEquals("EIN code", "EIN#", eIN.OK_CustomsRegNo);
				AssertEquals("Known Shipper Details", 1, org914228.MainAddress.KnownShipperDetails.Count);
				AssertEquals("known shipper 's approved number", "KSMS121908", org914228.MainAddress.KnownShipperDetails[0].OV_EXApprovalNumber);
				AssertEquals("known shipper - is approved", AviationSecuritySchemeMembershipEx.Codes.Yes, org914228.MainAddress.KnownShipperDetails[0].OV_EXApprovedOrMajorExporter);
				AssertEquals("Last inspect Date", new ZDateTime(2008, 11, 25), org914228.MainAddress.KnownShipperDetails[0].OV_EXSiteInspectionDate);
			}
		}

		OrgHeader GetOrgHeader(ZString code, ZString codeType)
		{
			OrgHeader result = null;
			ZQuery cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, code);
			OrgCusCode cusCode = Factory.LoadTop1<OrgCusCode>(cusCodeFilter);
			if (cusCode != null)
			{
				result = Factory.Load<OrgHeader>(cusCode.OK_OH);
			}

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			StaffABC = SetupStaff("ABC");
			StaffDEF = SetupStaff("DEF");
		}

		GlbStaff StaffABC, StaffDEF;
		GlbStaff SetupStaff(ZString code)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			staff.GS_Code = code;
			staff.GS_LoginName = code + " Name";
			Factory.Save();
			return staff;
		}
	}
}
