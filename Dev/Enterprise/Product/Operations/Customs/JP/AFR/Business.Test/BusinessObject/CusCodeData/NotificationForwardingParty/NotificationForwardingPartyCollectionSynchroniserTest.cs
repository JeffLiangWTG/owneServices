using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class NotificationForwardingPartyCollectionSynchroniserTest : AFRSynchroniserTestCase
	{
		public void TestSynchronisation()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			Factory.Save();

			var synchroniser = new NotificationForwardingPartyCollectionSynchroniser(shipment, bill);
			synchroniser.Synchronise();
			AssertEquals(0, bill.NotificationForwardingParties.Count);

			var newConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newConsigneeOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.SetCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Japan), "VICTA");
			newConsigneeOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery);
			Factory.Save();

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = newConsigneeOrg.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(1, bill.NotificationForwardingParties.Count);
				AssertEquals((short)1, bill.NotificationForwardingParties.FirstOrDefault().CY_Order);
				AssertEquals("VICTA", bill.NotificationForwardingParties.FirstOrDefault().CY_Data);
			});

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.NewZGuid();
			CombineAssertions(() =>
			{
				AssertEquals(1, bill.NotificationForwardingParties.Count);
				AssertEquals((short)1, bill.NotificationForwardingParties.FirstOrDefault().CY_Order);
				AssertEquals("", bill.NotificationForwardingParties.FirstOrDefault().CY_Data);
			});

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = newConsigneeOrg2.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(0, bill.NotificationForwardingParties.Count);
			});

			var relatedOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg2.SetCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Japan), "VICTB");
			newConsigneeOrg.AddRelatedParty(relatedOrg2.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery, string.Empty, string.Empty, testCompany);
			Factory.Save();

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = newConsigneeOrg.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(1, bill.NotificationForwardingParties.Count);
				AssertEquals((short)1, bill.NotificationForwardingParties.FirstOrDefault().CY_Order);
				AssertEquals("VICTA", bill.NotificationForwardingParties.FirstOrDefault().CY_Data);
			});
			header.JPH_GB_Branch = testBranch.PK;
			CombineAssertions(() =>
			{
				AssertEquals(1, bill.NotificationForwardingParties.Count);
				AssertEquals((short)1, bill.NotificationForwardingParties.FirstOrDefault().CY_Order);
				AssertEquals("VICTB", bill.NotificationForwardingParties.FirstOrDefault().CY_Data);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			shipment = consol.Shipments.AddNew();

			header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;

			bill = header.Bills.AddNew();
		}
		ForwardingShipment shipment;
		JPAFRBills bill;
	}
}
