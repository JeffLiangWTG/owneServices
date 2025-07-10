using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDtbBookingConsignment))]
	sealed class FreightWrapperFromBookingConsignmentTest : FreightWrapperFromDtbTransportTest<DtbBookingConsignment, FreightWrapperFromDtbBookingConsignment>
	{
		#region JobNumberHeadingToTest

		protected override string JobNumberHeadingToTest
		{
			get
			{
				return "Consignment ID";
			}
		}

		#endregion

		#region TransportReferenceHeadingToTest

		protected override string TransportReferenceHeadingToTest
		{
			get
			{
				return "Connote Number";
			}
		}

		#endregion

		#region TestBookingReference

		public void TestBookingReference()
		{
			var consignmentWithBooking = Helper.CreateBookingConsignment();
			var consignmentWithoutBooking = Helper.CreateBookingConsignment();
			var booking = Helper.CreateBooking("B1", consignmentWithBooking);
			booking.KM_JobID = "REF";
			var wrapperForConsignmentWithBooking = new FreightWrapperFromDtbBookingConsignment(consignmentWithBooking, Factory);
			var wrapperForConsignmentWithoutBooking = new FreightWrapperFromDtbBookingConsignment(consignmentWithoutBooking, Factory);
			AssertEquals("REF", wrapperForConsignmentWithBooking.BookingReference);
			AssertEquals("", wrapperForConsignmentWithoutBooking.BookingReference);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			var consignment = (DtbBookingConsignment)GetNewBusinessObjectToWrap();
			var wrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("Carrier Org", GlbCompany.CurrentCompany.OrgProxy, wrapper.Carrier.WrappedObject);
		}

		#endregion

		#region TestDeliveryAgent

		public void TestDeliveryAgent()
		{
			var consignment = (DtbBookingConsignment)GetNewBusinessObjectToWrap();
			var wrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("Carrier Org", GlbCompany.CurrentCompany.OrgProxy, wrapper.DeliveryAgent.WrappedObject);
		}

		#endregion

		#region TestCustomsEntries

		public void TestConsignmentCustomsEntries()
		{
			var consignment = GetTransportBizO();
			CusEntryNumber entryNumber = (CusEntryNumber)consignment.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryType = "BPR";
			entryNumber.CE_EntryNum = "34345678";
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryLineReference = "Info";
			entryNumber.CE_IssueDate = new ZDateTime(2009, 2, 2);

			var consignmentWrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("CustomsEntries Size", 1, consignmentWrapper.CustomsEntries.Count);
			AssertEquals("EntryType", "Booking Party Reference", consignmentWrapper.CustomsEntries[0].EntryType.ToString());
			AssertEquals("EntryNumber", "34345678", consignmentWrapper.CustomsEntries[0].EntryNumber);
			AssertEquals("EntryCategory", "CUS", consignmentWrapper.CustomsEntries[0].EntryCategory);
			AssertEquals("EntryLineReference", "Info", consignmentWrapper.CustomsEntries[0].Information);
			AssertEquals("EntryNumberIssueDate", new ZDateTime(2009, 2, 2), consignmentWrapper.CustomsEntries[0].IssueDate);
		}

		#endregion

		#region TestConsignmentNote

		public void TestConsignmentNote()
		{
			var consignment = GetTransportBizO();
			consignment.KM_TransportReference = "CONNOTE0000123";

			var consignmentWrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("Consignment Note", "CONNOTE0000123", consignmentWrapper.ConNote);
		}

		#endregion

		#region TestConsignmentHazardousAndRefrigerated

		public void TestConsignmentHazardousAndRefrigerated()
		{
			var consignment = GetTransportBizO();
			AssertEquals("Default: Hazardous", false, consignment.KM_IsHazardous);
			AssertEquals("Default: Refrigerated", false, consignment.KM_RequiresRefrigeration);

			consignment.KM_IsHazardous = true;
			consignment.KM_RequiresRefrigeration = true;

			var consignmentWrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("Hazardous", true, consignmentWrapper.Hazardous);
			AssertEquals("Refrigerated", true, consignmentWrapper.Refrigerated);
		}

		#endregion

		#region TestTransportReferenceAndGoodsDescription

		public void TestTransportReferenceAndGoodsDescription()
		{
			var consignment = GetTransportBizO();
			consignment.KM_JobID = "C00000001";
			consignment.KM_TransportReference = "TRS00001";

			var consignmentWrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("Transport Reference", "TRS00001", consignmentWrapper.TransportReference);
			AssertEquals("No goods descriptions", "", consignmentWrapper.GoodsDescription);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			var consignment = GetTransportBizO();
			consignment.KM_JobID = "C00000001";

			var consignmentWrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);
			AssertEquals("JobNumberHeading", JobNumberHeadingToTest, consignmentWrapper.JobNumberHeading);

			AssertEquals("Precondition", "", consignment.KM_TransportReference);
			AssertEquals("JobNumber - Must fallback to Job ID.", "C00000001", consignmentWrapper.JobNumber);

			consignment.KM_TransportReference = "TRS00001";
			AssertEquals("JobNumber - Manual Connote No.", "TRS00001", consignmentWrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryJobNumber

		public void TestSecondaryJobNumber()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var transportBookingWithJobID = consolidation.Bookings.AddNew();
			var transportBookingWithoutJobID = consolidation.Bookings.AddNew();
			transportBookingWithJobID.KM_JobID = "ABCD";
			var consignmentConsolidation = Helper.CreateConsolidation();
			var consignment = Helper.CreateBookingConsignment();
			consignment.KM_KB_Booking = consignmentConsolidation.PK;

			var consignmentWrapper = GetFreightWrapper(consignment);
			consignmentConsolidation.KB_ParentID = transportBookingWithJobID.PK;
			consignmentConsolidation.KB_ParentTableCode = transportBookingWithJobID.TablePrefix;
			AssertEquals("ABCD", consignmentWrapper.SecondaryNumber);
			AssertEquals("Booking ID", consignmentWrapper.SecondaryHeading);

			consignmentConsolidation.KB_ParentID = transportBookingWithoutJobID.PK;
			consignmentWrapper = GetFreightWrapper(consignment);
			AssertEquals("", consignmentWrapper.SecondaryNumber);
			AssertEquals("Booking ID", consignmentWrapper.SecondaryHeading);

			var wrapperForConsignmentWithoutParent = GetFreightWrapper(Factory.New<DtbBookingConsignment>());
			AssertEquals("", wrapperForConsignmentWithoutParent.SecondaryNumber);
			AssertEquals("", wrapperForConsignmentWithoutParent.SecondaryHeading);
		}

		#endregion

		#region TestPickupAndDeliveryRequiredFromAndTo

		[TestDate(2013, 07, 24)]
		public void TestPickupAndDeliveryRequiredFromAndTo()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var wrapper = GetFreightWrapper(consignment);
			AssertEquals(ZDateTime.Empty, wrapper.PickupFrom);
			AssertEquals(ZDateTime.Empty, wrapper.PickupRequiredBy);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryFrom);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryRequiredBy);

			consignment.PickupInstruction.Confirmations[0].KK_RequiredFrom = ZDateTime.Now.AddDays(1);
			consignment.PickupInstruction.Confirmations[0].KK_RequiredTo = ZDateTime.Now.AddDays(2);
			consignment.DeliveryInstruction.Confirmations[0].KK_RequiredFrom = ZDateTime.Now.AddDays(3);
			consignment.DeliveryInstruction.Confirmations[0].KK_RequiredTo = ZDateTime.Now.AddDays(4);
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
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.PickupInstruction.Address.OrganisationPK = consignor.PK;
			consignment.DeliveryInstruction.Address.OrganisationPK = consignee.PK;

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
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.PickupInstruction.Address.OrganisationPK = consignor.PK;

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
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.DeliveryInstruction.Address.OrganisationPK = consignee.PK;

			var wrapper = GetFreightWrapper(consignment);
			AssertNotNull(wrapper.DeliveryAddress);
			AssertEquals("123 Test st", wrapper.DeliveryAddress.AddressLine1);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.KM_RS_NKServiceLevel = "SVL";
			var wrapper = GetFreightWrapper(consignment);
			AssertEquals("SVL", wrapper.ServiceLevel.Code);

			consignment.KM_RS_NKServiceLevel = "ABC";
			wrapper = GetFreightWrapper(consignment);
			AssertEquals("ABC", wrapper.ServiceLevel.Code);
		}

		#endregion

		#region TestReceiverReference

		public void TestReceiverReference()
		{
			var consignee = Helper.CreateOrganisation("Org1");
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			consignment.PickupInstruction.Confirmations[0].KK_ReferenceNum = "123";
			consignment.DeliveryInstruction.Confirmations[0].KK_ReferenceNum = "456";

			var wrapper = GetFreightWrapper(consignment);
			AssertEquals("456", wrapper.CustomerReference);
		}

		#endregion

		#region TestOwnerAndCustomerReferenceCheckForNullConfirmation

		public void TestOwnerAndCustomerReferenceCheckForNoConfirmations()
		{
			var consignment = GetTransportBizO();
			consignment.PickupInstruction.Confirmations.DeleteAll();
			consignment.DeliveryInstruction.Confirmations.DeleteAll();

			AssertEquals(0, consignment.PickupInstruction.Confirmations.Count);
			AssertEquals(0, consignment.DeliveryInstruction.Confirmations.Count);

			var wrapper = GetFreightWrapper(consignment);
			AssertEquals("Shouldn't blow up", "", wrapper.OwnerReference);
			AssertEquals("Shouldn't blow up", "", wrapper.CustomerReference);
		}

		#endregion

		#region GetTransportBizO

		protected override DtbBookingConsignment GetEmptyTransportBizO()
		{
			return Helper.CreateBookingConsignment();
		}

		protected override DtbBookingConsignment GetTransportBizO()
		{
			return Helper.CreateBookingConsignmentWithTemplate();
		}

		#endregion

		#region GetFreightWrapper

		protected override FreightWrapperFromDtbBookingConsignment GetFreightWrapper(DtbBookingConsignment transport)
		{
			return new FreightWrapperFromDtbBookingConsignment(transport, Factory);
		}

		#endregion

		#region IsCarrierUsed

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Consignment ID" },
					{ "TransportReferenceHeading" , "Connote Number" }
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

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var org = Helper.CreateOrganisation("Org1");
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			Factory.Save();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			return base.GetSetupWrapperForDefaultFormatting();
		}

		#region TestBarcodeTextForFont

		protected override void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
			var consignment = (DtbBookingConsignment)bizO;
			consignment.KM_TransportReference = "ConnoteID";
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^DMC=ConnoteID;CAD;|FÊ";
		}

		#endregion

		#region TestPackagesDetails

		public void TestPackagesDetails()
		{
			var consignment = GetTransportBizO();

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

			var wrapper = new FreightWrapperFromDtbBookingConsignment(consignment, Factory);

			const string expectedDetails =
@"15x PLT, Wgt: 240.718 KG, Vol: 150.000 CF
20x BOX, Wgt: 200.000 KG, Vol: 200.000 CI
30x BOX, Wgt: 961.387 LB, Vol: 300.000 M3
1x GRS, Wgt: 1.500 LB, Vol: 24.000 M3
2x GRS, Wgt: 3.000 KG, Vol: 48.000 M3";
			AssertEquals(expectedDetails, wrapper.PackagesDetails);
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
