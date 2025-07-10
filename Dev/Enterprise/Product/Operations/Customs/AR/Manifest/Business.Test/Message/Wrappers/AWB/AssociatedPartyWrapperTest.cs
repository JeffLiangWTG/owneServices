using System.Linq;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	public class AssociatedPartyWrapperTest : TestCaseWithFactory
	{
		public void TestAssociatedPartyWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateHouseBill(header);

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], MessageSubTypeCodes.Codes.Original);
			IParty associated = wrapper.MasterConsignment.IncludedHouseConsignment.AssociatedParty.ElementAt(0);
			IPostalStructuredAddress postalStructuredAddress = associated.PostalStructuredAddress;
			ITradeContact tradeContact = associated.DefinedTradeContact;

			CombineAssertions(() =>
			{
				AssertEquals("123456", associated.PrimaryID);
				AssertEquals(ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, associated.SchemeAgencyID);
				AssertEquals("210696", associated.AdditionalID);
				AssertEquals("NotifyParty", associated.Name);
				AssertEquals(ZString.Empty, associated.AccountID);

				AssertEquals("H3A 2R4", postalStructuredAddress.PostcodeCode);
				AssertEquals("Address1 Address2", postalStructuredAddress.StreetName);
				AssertEquals("City", postalStructuredAddress.CityName);
				AssertEquals(Core.Constants.CountryCodes.Argentina, postalStructuredAddress.CountryID);
				AssertEquals("Argentina", postalStructuredAddress.CountryName);
				AssertEquals(ZString.Empty, postalStructuredAddress.CityID);
				AssertEquals("NYC", postalStructuredAddress.PostOfficeBox);

				AssertEquals("Bruno Garcia", tradeContact.PersonName);
				AssertEquals("Phone", tradeContact.DirectTelephoneCommunicationCompleteNumber);
				AssertEquals("Fax", tradeContact.FaxCommunicationCompleteNumber);
				AssertEquals("Email", tradeContact.EmailCommunicationID);
			});
		}

		void CreateAndPopulateHouseBill(AsycudaManifestHeader header)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "123456";
			orgHeader.OH_FullName = "NotifyParty";
			orgHeader.OH_RL_NKClosestPort = "NYC";

			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "Address1";
			orgAddress.Address2 = "Address2";
			orgAddress.City = "City";
			orgAddress.OA_Email = "Email";
			orgAddress.OA_Fax = "Fax";
			orgAddress.OA_Phone = "Phone";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			orgAddress.Postcode = "H3A 2R4";

			OrgStaffAssignments staffAssignment = orgHeader.StaffAssignments.AddNew();
			GlbStaff salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "BGA";
			salesRep.GS_FullName = "Bruno Garcia";
			staffAssignment.O8_GS_NKPersonResponsible = salesRep.GS_Code;
			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.CustomsAgent;

			AsycudaBill bill = header.Bills.AddNew();

			bill.NotifyPartyOrgPK = orgHeader.PK;
			bill.ABL_NotifyPartyRegNo = "210696";
			bill.ABL_NotifyPartyRegNoType = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;
		}
	}
}
