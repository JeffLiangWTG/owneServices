using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskBusinessObject))]
	public class ComplianceRiskBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ComplianceRiskBusinessObject(Factory.NewWithValidTestData<ForwardingShipment>());

		public void TestComplianceRiskBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceBizO = new ComplianceRiskBusinessObject(shipment);

			CombineAssertions(() =>
			{
				AssertNotNull(complianceBizO.ComplianceRiskStatus);
				AssertEquals(shipment.PK, complianceBizO.ComplianceRiskStatus.COR_ParentID);
				AssertEquals(shipment.TablePrefix, complianceBizO.ComplianceRiskStatus.COR_ParentTableCode);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceBizO.ComplianceRiskStatus.COR_PartyRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceBizO.ComplianceRiskStatus.COR_LocationRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceBizO.ComplianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceBizO.ComplianceRiskStatus.COR_OverallRisk);
			});
		}

		public void TestComplianceRiskObjectPartiesAndLocations()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var complianceBizO = new ComplianceRiskBusinessObject(shipment);
			complianceBizO.RefreshData();

			CombineAssertions(() =>
			{
				AssertEquals(2, complianceBizO.Parties.Count);
				AssertEquals(true, complianceBizO.Parties.Any(p => ((ComplianceRiskPartyWrapper)p).OrgCode == consignor.OH_Code));
				AssertEquals(true, complianceBizO.Parties.Any(p => ((ComplianceRiskPartyWrapper)p).OrgCode == consignee.OH_Code));
				AssertEquals(2, complianceBizO.Locations.Count);
				AssertEquals(true, complianceBizO.Locations.Any(l => ((ComplianceRiskLocationWrapper)l).Location == "AU"));
				AssertEquals(true, complianceBizO.Locations.Any(l => ((ComplianceRiskLocationWrapper)l).Location == "NZ"));
			});
		}

		public void TestReloadSafeComplianceRiskStatusWithRelatedParties()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRiskStatus);

			Factory.Save();

			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.ComplianceRiskStatus set COR_PartyRisk = 'CLR', COR_LocationRisk = 'CLR', COR_OverallRisk = 'CLR' where COR_PK = '{complianceRiskStatus.PK}'
UPDATE dbo.OrgHeader set OH_ScreeningStatus = 'CLR' where OH_PK = '{consignor.PK}'
UPDATE dbo.OrgHeader set OH_ScreeningStatus = 'CLR' where OH_PK = '{consignee.PK}'");

			complianceBizO.ReloadSafeComplianceRiskStatusWithRelatedParties();

			AssertEquals("CLR", consignor.OH_ScreeningStatus);
			AssertEquals("CLR", consignee.OH_ScreeningStatus);
			AssertEquals("CLR", complianceBizO.ComplianceRiskStatus.COR_PartyRisk);
			AssertEquals("CLR", complianceBizO.ComplianceRiskStatus.COR_LocationRisk);
			AssertEquals("CLR", complianceBizO.ComplianceRiskStatus.COR_OverallRisk);
		}

		public void TestCommercialInvoiceGoodOriginMaterialChanged_ForwardingShipment()
		{
			var shipment = GlobalCommercialInvoiceHelperTest.CreateNewShipment(Factory) as ForwardingShipment;
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			var invoiceHeader = GlobalCommercialInvoiceHelperTest.CreateInvoiceHeader(Factory, shipment);
			invoiceHeader.GIH_RN_NKCountryOrigin = "US";
			var line = Factory.New<GlobalCommercialInvoiceLine>();
			line.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			line.GIL_GIH_Header = invoiceHeader.PK;
			line.GIL_Tariff1 = "222222";
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRiskStatus);
				var commodities = complianceBizO.ComplianceRiskStatus.CommodityDetailCollection.ToList<ComplianceCommodityDetail>();
				AssertEquals(1, commodities.Count);
				var commodity = commodities.Single();
				AssertEquals("US", commodity.CCD_RN_NKOrigin);

				invoiceHeader.GIH_RN_NKCountryOrigin = "AU";
				complianceBizO.RefreshData();
				commodities = complianceBizO.ComplianceRiskStatus.CommodityDetailCollection.ToList<ComplianceCommodityDetail>();
				AssertEquals(1, commodities.Count);
				commodity = commodities.Single();
				AssertEquals("AU", commodity.CCD_RN_NKOrigin);
			}
		}

		public void TestCommercialInvoiceGoodOriginMaterialChanged_QuotedBooking()
		{
			var booking = GlobalCommercialInvoiceHelperTest.CreateNewBookingQuick(Factory);
			var invoiceHeader = GlobalCommercialInvoiceHelperTest.CreateInvoiceHeader(Factory, ((QuotedBooking)booking).Booking);
			invoiceHeader.GIH_RN_NKCountryOrigin = "US";
			var line = Factory.New<GlobalCommercialInvoiceLine>();
			line.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, booking.ViewPK, "TH");
			line.GIL_GIH_Header = invoiceHeader.PK;
			line.GIL_Tariff1 = "111111";
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var complianceBizO = new ComplianceRiskPlugInBusinessObject(booking as BusinessObject);
				var commodities = complianceBizO.ComplianceRiskStatus.CommodityDetailCollection.ToList<ComplianceCommodityDetail>();
				AssertEquals(1, commodities.Count);
				var commodity = commodities.Single();
				AssertEquals("US", commodity.CCD_RN_NKOrigin);

				invoiceHeader.GIH_RN_NKCountryOrigin = "AU";
				complianceBizO.RefreshData();
				commodities = complianceBizO.ComplianceRiskStatus.CommodityDetailCollection.ToList<ComplianceCommodityDetail>();
				AssertEquals(1, commodities.Count);
				commodity = commodities.Single();
				AssertEquals("AU", commodity.CCD_RN_NKOrigin);
			}
		}

		public void TestReloadSafeComplianceRiskStatusWithRelatedPartiesAndOverrideDocAddress()
		{
			var parentHeader = Factory.NewWithValidTestData<OrgHeader>();
			parentHeader.OH_FullName = "PARENT ORG";
			parentHeader.MainAddress.OA_Address1 = "Parent address";
			parentHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "TEST COMNPAY";
			header.OH_Code = "JOBDOC";
			header.OH_IsActive = false;
			parentHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Iran;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_OH_ImportBroker = parentHeader.PK;

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_ParentID = shipment.PK;
			docAddress.E2_ParentTableCode = shipment.TablePrefix;
			docAddress.E2_CompanyName = "TEST COMPANY";
			docAddress.E2_OA_Address = header.MainAddress.PK;
			docAddress.E2_AddressOverride = false;

			Factory.Save();

			var dummyBizObject = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory);
			dummyBizObject.AddPartiesForTest(new ScreeningParty(parentHeader, "Doc Address", docAddress));

			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Desc = "Country";
			dummyBizObject.AddCountriesForTest(new ScreeningParty(country, "Country", country));

			var location = dummyBizObject.Locations.Single();
			var party = dummyBizObject.Parties.Single() as ScreeningParty;

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(country.RN_Code, location.Code);
				AssertEquals(header.OH_Code, party.OrgCode);
			});

			party.DocAddress.E2_AddressOverride = true;
			party.DocAddress.E2_CompanyName = "OVERRIDE COMPANY";
			party.DocAddress.E2_Address1 = "OVERRIDE Address1";
			party.DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(dummyBizObject);
			plugInBizO.RefreshData();
			plugInBizO.ReloadSafeComplianceRiskStatusWithRelatedParties();

			Factory.Save();

			AssertEquals(true, docAddress.E2_AddressOverride);
			AssertEquals("OVERRIDE COMPANY", docAddress.E2_CompanyName);
			AssertEquals("OVERRIDE Address1", docAddress.E2_Address1);
			AssertEquals(Core.Constants.CountryCodes.Australia, party.DocAddress.E2_RN_NKCountryCode);
		}

		public void TestReloadSafeComplianceRiskStatusWithRelatedParties_NonPersistentBizONotReloaded()
		{
			var booking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>()
				.InvokeMember(
					"New",
					System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
					null,
					null,
					new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });
			Factory.Save();

			var bookingScreeningParties = (booking as IScreeningPartyProvider).ScreeningParties;
			var quotedBookingScreeningParty = bookingScreeningParties.FirstOrDefault();

			Assert("ScreeningEntity is a NonPersistent BizO", quotedBookingScreeningParty.ScreeningEntity is NonPersistentBusinessObject);
		}
	}
}
