using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDtbConsignment))]
	sealed class FreightWrapperFromConsignmentTest : FreightWrapperTest
	{
		#region TestGetJobNumberAndHeading

		public void TestGetJobNumberAndHeading()
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_JobID = "ABCD";
			var transportWrapper = GetFreightWrapper(consignment);

			AssertEquals("ABCD", transportWrapper.JobNumber);
			AssertEquals("Consignment ID", transportWrapper.JobNumberHeading);
		}

		#endregion

		#region TransportReferenceHeadingToTest

		protected override string TransportReferenceHeadingToTest
		{
			get { return "Connote Number"; }
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			var consignment = (DtbConsignment)GetNewBusinessObjectToWrap();
			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("Carrier Org", GlbCompany.CurrentCompany.OrgProxy, wrapper.Carrier.WrappedObject);
		}

		#endregion

		#region TestDeliveryAgent

		public void TestDeliveryAgent()
		{
			var consignment = (DtbConsignment)GetNewBusinessObjectToWrap();
			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("Carrier Org", GlbCompany.CurrentCompany.OrgProxy, wrapper.DeliveryAgent.WrappedObject);
		}

		#endregion

		#region TestCustomsEntries

		public void TestConsignmentCustomsEntries()
		{
			var now = ZDateTime.Now;
			var consignment = Helper.CreateConsignment();
			CusEntryNumber entryNumber = (CusEntryNumber)consignment.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryType = "TST";
			entryNumber.CE_EntryNum = "34345678";
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryLineReference = "Info";
			entryNumber.CE_IssueDate = now;

			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("CustomsEntries Size", 1, consignmentWrapper.CustomsEntries.Count);
			AssertEquals("EntryNumber", "34345678", consignmentWrapper.CustomsEntries[0].EntryNumber);
			AssertEquals("EntryCategory", "CUS", consignmentWrapper.CustomsEntries[0].EntryCategory);
			AssertEquals("EntryLineReference", "Info", consignmentWrapper.CustomsEntries[0].Information);
			AssertEquals("EntryNumberIssueDate", now, consignmentWrapper.CustomsEntries[0].IssueDate);
		}

		#endregion

		#region TestConsignmentNote

		public void TestConsignmentNote()
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_ConnoteNumber = "CONNOTE0000123";

			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("Consignment Note", "CONNOTE0000123", consignmentWrapper.ConNote);
		}

		#endregion

		#region TestConsignmentHazardousAndRefrigerated

		public void TestConsignmentHazardousAndRefrigerated()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("Default: Hazardous", false, consignment.LTC_IsHazardous);
			AssertEquals("Default: Refrigerated", false, consignment.LTC_RequiresRefrigeration);

			consignment.LTC_IsHazardous = true;
			consignment.LTC_RequiresRefrigeration = true;

			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("Hazardous", true, consignmentWrapper.Hazardous);
			AssertEquals("Refrigerated", true, consignmentWrapper.Refrigerated);
		}

		#endregion

		#region TestTransportReferenceAndGoodsDescription

		public void TestTransportReferenceAndGoodsDescription()
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_JobID = "C00000001";
			consignment.LTC_ConnoteNumber = "TRS00001";

			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("No Transport Reference", "TRS00001", consignmentWrapper.TransportReference);
			AssertEquals("No goods descriptions", "", consignmentWrapper.GoodsDescription);
		}

		#endregion

		#region TestJobHeaderLocalClient_WithoutJob

		public void TestJobHeaderLocalClient_WithoutJob()
		{
			var consignment = Helper.CreateConsignment("C00000001");
			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertNull(consignmentWrapper.Job);
			AssertEquals("JobHeaderLocalClient.CompanyNameAndAddress", "", consignmentWrapper.JobHeaderLocalClient.CompanyNameAndAddress);

			var billingParty = consignment.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			billingParty.CompanyName = "Honda Motorcycles";
			billingParty.Address1 = "Melbourne";

			consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertNull(consignmentWrapper.Job);
			AssertEquals("JobHeaderLocalClient.CompanyNameAndAddress", "HONDA MOTORCYCLES\nMELBOURNE", consignmentWrapper.JobHeaderLocalClient.CompanyNameAndAddress);
		}

		#endregion

		#region TestJobHeaderLocalClient_WithJob

		public void TestJobHeaderLocalClient_WithJob()
		{
			var consignment = Helper.CreateConsignment("C00000001");
			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertNull(consignmentWrapper.Job);
			AssertEquals("JobHeaderLocalClient.CompanyName should be empty", "", consignmentWrapper.JobHeaderLocalClient.CompanyName);

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = consignment.PK;
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.Addresses.AddNew(OrgAddressType.Office, true);
			orgHeader.OH_FullName = "LOCAL CLIENT COMPANY NAME";
			orgHeader.OH_Code = "OH1";
			job.LocalChargesPK = orgHeader.PK;

			consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("Precondition: Job", job, consignmentWrapper.Job);
			AssertEquals("JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME", consignmentWrapper.JobHeaderLocalClient.CompanyName);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_JobID = "C00000001";
			consignment.LTC_ConnoteNumber = "";

			var consignmentWrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("JobNumber - Must fallback to Job ID.", "C00000001", consignmentWrapper.JobNumber);

			consignment.LTC_ConnoteNumber = "TRS00001";
			AssertEquals("JobNumber - Manual Connote No.", "C00000001", consignmentWrapper.JobNumber);
		}

		#endregion

		#region TestPickupAndDeliveryRequiredFromAndTo

		[TestDate(2016, 03, 22)]
		public void TestPickupAndDeliveryRequiredFromAndTo()
		{
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);

			var wrapper = GetFreightWrapper(consignment);
			AssertEquals(ZDateTime.Empty, wrapper.PickupFrom);
			AssertEquals(ZDateTime.Empty, wrapper.PickupRequiredBy);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryFrom);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryRequiredBy);

			consignment.PickupAddress.Actions[0].LTA_RequiredFrom = ZDateTimeOffset.Now.AddDays(1);
			consignment.PickupAddress.Actions[0].LTA_RequiredTo = ZDateTimeOffset.Now.AddDays(2);
			consignment.DeliveryAddress.Actions[0].LTA_RequiredFrom = ZDateTimeOffset.Now.AddDays(3);
			consignment.DeliveryAddress.Actions[0].LTA_RequiredTo = ZDateTimeOffset.Now.AddDays(4);
			AssertEquals(ZDateTime.Now.AddDays(1), wrapper.PickupFrom);
			AssertEquals(ZDateTime.Now.AddDays(2), wrapper.PickupRequiredBy);
			AssertEquals(ZDateTime.Now.AddDays(3), wrapper.DeliveryFrom);
			AssertEquals(ZDateTime.Now.AddDays(4), wrapper.DeliveryRequiredBy);
		}

		#endregion

		#region TestConsigneeAndConsignor

		public void TestConsigneeAndConsignor()
		{
			var consignor = Helper.CreateOrganisation("Org1");
			var consignee = Helper.CreateOrganisation("Org2");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
			consignment.PickupAddress.Address.OrganisationPK = consignor.PK;
			consignment.DeliveryAddress.Address.OrganisationPK = consignee.PK;

			var wrapper = GetFreightWrapper(consignment);
			AssertEquals(consignor, wrapper.Consignor.Organisation);
			AssertEquals(consignee, wrapper.Consignee.Organisation);
		}

		#endregion

		#region TestPickupAddress

		[SetOrgAllowMixedCase(true)]
		public void TestPickupAddress()
		{
			var consignor = Helper.CreateOrganisation("Org1");
			consignor.MainAddress.OA_Address1 = "123 Test st";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			consignment.PickupAddress.Address.OrganisationPK = consignor.PK;

			var wrapper = GetFreightWrapper(consignment);
			AssertNotNull(wrapper.PickupAddress);
			AssertEquals("123 Test st", wrapper.PickupAddress.AddressLine1);
		}

		#endregion

		#region TestDeliveryAddress

		[SetOrgAllowMixedCase(true)]
		public void TestDeliveryAddress()
		{
			var consignee = Helper.CreateOrganisation("Org1");
			consignee.MainAddress.OA_Address1 = "123 Test st";
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);

			consignment.DeliveryAddress.Address.OrganisationPK = consignee.PK;

			var wrapper = GetFreightWrapper(consignment);
			AssertNotNull(wrapper.DeliveryAddress);
			AssertEquals("123 Test st", wrapper.DeliveryAddress.AddressLine1);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_RS_NKServiceLevel = "SVL";
			var wrapper = GetFreightWrapper(consignment);
			AssertEquals("SVL", wrapper.ServiceLevel.Code);

			consignment.LTC_RS_NKServiceLevel = "ABC";
			wrapper = GetFreightWrapper(consignment);
			AssertEquals("ABC", wrapper.ServiceLevel.Code);
		}

		#endregion

		#region TestReceiverReference

		public void TestReceiverReference()
		{
			var consignee = Helper.CreateOrganisation("Org1");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
			consignment.PickupAddress.Actions[0].LTA_ReferenceNumber = "123";
			consignment.DeliveryAddress.Actions[0].LTA_ReferenceNumber = "456";

			var wrapper = GetFreightWrapper(consignment);
			AssertEquals("456", wrapper.CustomerReference);
		}

		#endregion

		#region TestOwnerAndCustomerReferenceCheckForNullConfirmation

		public void TestOwnerAndCustomerReferenceCheckForNoConfirmations()
		{
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);

			consignment.PickupAddress.Actions.DeleteAll();
			consignment.DeliveryAddress.Actions.DeleteAll();

			AssertEquals(0, consignment.PickupAddress.Actions.Count);
			AssertEquals(0, consignment.DeliveryAddress.Actions.Count);

			var wrapper = GetFreightWrapper(consignment);
			AssertEquals("Shouldn't blow up", "", wrapper.OwnerReference);
			AssertEquals("Shouldn't blow up", "", wrapper.CustomerReference);
		}

		#endregion

		#region TestPackagesDetails

		public void TestPackagesDetails()
		{
			var consignment = Helper.CreateConsignment();

			var pA1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Pallet, 150, 150, 15);
			var pA2 = Helper.CreatePackage(consignment, Constants.PkgUnit.Pallet, 200, 200, 20);
			var pB1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Box, 200, 200, 20);
			var pB2 = Helper.CreatePackage(consignment, Constants.PkgUnit.Box, 300, 300, 30);
			var pB3 = Helper.CreatePackage(consignment, Constants.PkgUnit.Box, 300, 300, 30);
			var pC1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Gross, 1.5, 24, 1);
			var pC2 = Helper.CreatePackage(consignment, Constants.PkgUnit.Gross, 1.5, 24, 1);
			var pC3 = Helper.CreatePackage(consignment, Constants.PkgUnit.Gross, 1.5, 24, 1);

			pA1.KP_WeightUQ = Constants.Weight.Kilograms;
			pA1.KP_VolumeUQ = Constants.Volume.CubicFeet;

			// inner packages
			pA2.KP_WeightUQ = Constants.Weight.Pounds;
			pA2.KP_VolumeUQ = Constants.Volume.CubicMetres;
			pA2.KP_KP_ParentPackage = pA1.PK;

			pB1.KP_WeightUQ = Constants.Weight.Kilograms;
			pB1.KP_VolumeUQ = Constants.Volume.CubicInches;

			pB2.KP_WeightUQ = Constants.Weight.Pounds;
			pB2.KP_VolumeUQ = Constants.Volume.CubicMetres;

			// inner packages
			pB3.KP_WeightUQ = Constants.Weight.Kilograms;
			pB3.KP_VolumeUQ = Constants.Volume.CubicMetres;
			pB3.KP_KP_ParentPackage = pB2.PK;

			pC1.KP_WeightUQ = Constants.Weight.Pounds;
			pC1.KP_VolumeUQ = Constants.Volume.CubicMetres;

			pC2.KP_WeightUQ = Constants.Weight.Kilograms;
			pC2.KP_VolumeUQ = Constants.Volume.CubicMetres;

			pC3.KP_WeightUQ = Constants.Weight.Kilograms;
			pC3.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);

			const string expectedDetails =
@"15x PLT, Wgt: 240.718 KG, Vol: 150.000 CF
20x BOX, Wgt: 200.000 KG, Vol: 200.000 CI
30x BOX, Wgt: 961.387 LB, Vol: 300.000 M3
1x GRS, Wgt: 1.500 LB, Vol: 24.000 M3
2x GRS, Wgt: 3.000 KG, Vol: 48.000 M3";
			AssertEquals(expectedDetails, wrapper.PackagesDetails);
		}

		#endregion

		#region TestShipmentOuterPacksQty

		#region TestShipmentOuterPacksQty_DifferentUnits

		public void TestShipmentOuterPacksQty_DifferentUnits()
		{
			var consignment = Helper.CreateConsignment();

			var pA1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Pallet, 10, 10, 10);
			var pB1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Box, 20, 20, 20);
			var pC1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Carton, 30, 30, 30);

			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("pA(10) + pB(20) + pC(30)", 60m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals(Constants.PkgUnit.Package, wrapper.ShipmentOuterPacksQty.Unit.Code);
		}

		#endregion

		#region TestShipmentOuterPacksQty_SameUnit

		public void TestShipmentOuterPacksQty_SameUnit()
		{
			var consignment = Helper.CreateConsignment();

			var pA1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Pallet, 10, 10, 10);
			var pB1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Pallet, 20, 20, 20);
			var pC1 = Helper.CreatePackage(consignment, Constants.PkgUnit.Pallet, 30, 30, 30);

			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("pA(10) + pB(20) + pC(30)", 60m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals(Constants.PkgUnit.Pallet, wrapper.ShipmentOuterPacksQty.Unit.Code);
		}

		public void TestShipmentOuterPacksQty_WithNoPackage()
		{
			var consignment = Helper.CreateConsignment();

			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);
			AssertEquals("Should be 0", 0m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals(Constants.PkgUnit.Package, wrapper.ShipmentOuterPacksQty.Unit.Code);
		}

		#endregion

		#endregion

		#region TestGetBookingInstructions

		public void TestGetBookingInstructions()
		{
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
			var multiConsignmentAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			Helper.CreateConsignmentAction(multiConsignmentAddress, ActionTypes.Codes.Delivery);
			Helper.CreateConsignmentAction(multiConsignmentAddress, ActionTypes.Codes.PickUp);

			var wrapper = new FreightWrapperFromDtbConsignment(consignment, Factory);

			AssertEquals("Consignment should have 4 instructions.", 4, wrapper.BookingInstructions.Count);
		}

		#endregion

		#region Overrides

		#region IsCarrierUsed

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ExpectedCarrierTypeDescription

		protected override ZString ExpectedCarrierTypeDescription
		{
			get { return "Transport Company"; }
		}

		#endregion

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var org = Helper.CreateOrganisation("Org1");
			org.MainAddress.OA_RN_NKCountryCode = "AU";

			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);

			Factory.Save();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			return GetFreightWrapper(consignment);
		}

		#region TestBarcodeTextForFont

		protected override void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
			var consignment = (DtbConsignment)bizO;
			consignment.LTC_JobID = "ConnoteID";
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^LTC=ConnoteID;CAD;|kÊ";
		}

		#endregion

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "LTC001" },
					{ "JobNumberBarcodeText", "^LTC=LTC001;;|" },
					{ "JobNumberBarcodeTextForFont", "È^LTC=LTC001;;|HÊ" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈLTC001xÊ" },
					{ "JobNumberHeading", "Consignment ID" },
					{ "SecondaryHeading", "Job ID" },
					{ "SecondaryNumber", "LTC001" },
					{ "TransportReferenceHeading", "Connote Number" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Carrier : MAIN ST\nAUSTRALIA
DeliveryAddress :  is null
DeliveryAgent : MAIN ST\nAUSTRALIA
PickupAddress :  is null";
			}
		}

		#region GetNewBusinessObjectToWrap

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Helper.CreateConsignment();
		}

		#endregion

		#endregion

		#region GetFreightWrapper

		FreightWrapperFromDtbConsignment GetFreightWrapper(DtbConsignment consignment)
		{
			return new FreightWrapperFromDtbConsignment(consignment, Factory);
		}

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
