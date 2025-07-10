using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobSupplierBooking))]
	public class DocJobSupplierBookingTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocJobSupplierBooking.New(Booking, Factory),
				DocJobSupplierBooking.New(Booking.Factory, Booking.PK)
			};
		}

		public void TestBasicProperties()
		{
			Booking.JSB_BookingId = "JSB002";
			Booking.JSB_Status = "CNV";
			Booking.JSB_LoadMode = "CFS";
			Booking.JSB_TransportMode = "SEA";
			Booking.JSB_IncoTerm = "APP";
			Booking.JSB_RL_NKLoadPort = "CNSHA";
			Booking.JSB_RL_NKDischargePort = "AUSYD";
			Booking.JSB_BookedOnDate = new ZDate(2021, 1, 2);
			Booking.JSB_CargoAvailableDate = new ZDate(2022, 2, 4);
			Booking.JSB_OH_BookingParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			Booking.JSB_ContainerMode = "FCL";
			Booking.JSB_GoodsDescription = "good description";
			Booking.JSB_MarksAndNumbers = "marks & number";
			Booking.JSB_OA_CFSAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			Booking.JSB_RL_NKDestination = "SGSIN";
			Booking.JSB_RL_NKOrigin = "xxxx";

			AssertEquals("BookingId", Booking.JSB_BookingId, BookingWrapper.BookingId);
			AssertEquals("Status", Booking.JSB_Status, BookingWrapper.Status);
			AssertEquals("LoadMode", Booking.JSB_LoadMode, BookingWrapper.LoadMode);
			AssertEquals("TransportMode", Booking.JSB_TransportMode, BookingWrapper.TransportMode);
			AssertEquals("IncoTerm", Booking.JSB_IncoTerm, BookingWrapper.IncoTerm);
			AssertEquals("LoadPort", Booking.JSB_RL_NKLoadPort, BookingWrapper.LoadPort);
			AssertEquals("DischargePort", Booking.JSB_RL_NKDischargePort, BookingWrapper.DischargePort);
			AssertEquals("BookingParty", Booking.BookingParty.OH_Code, BookingWrapper.BookingParty.Code);
			AssertEquals("BookedOnDate", Booking.JSB_BookedOnDate, BookingWrapper.BookedOnDate);
			AssertEquals("CargoAvailableDate", Booking.JSB_CargoAvailableDate, BookingWrapper.CargoAvailableDate);
			AssertEquals("ContainerMode", Booking.JSB_ContainerMode, BookingWrapper.ContainerMode);
			AssertEquals("GoodsDescription", Booking.JSB_GoodsDescription, BookingWrapper.GoodsDescription);
			AssertEquals("MarksAndNumbers", Booking.JSB_MarksAndNumbers, BookingWrapper.MarksAndNumbers);
			AssertEquals("CFSAddress", Booking.CFSAddress.AddressCode, BookingWrapper.CFSAddress.Code);
			AssertEquals("Destination", Booking.JSB_RL_NKDestination, BookingWrapper.Destination);
			AssertEquals("Origin", Booking.JSB_RL_NKOrigin, BookingWrapper.Origin);
		}

		public void TestSupplierAddress()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg.Contacts.AddNew().FillWithValidTestData();
			supplierOrg.MainAddress.Address1 = "xxxx 0001";
			Booking.SupplierNameOrPK = supplierOrg.PK.ToString();
			AssertEquals("SupplierAddress", Booking.SupplierAddress.E2_Address1, BookingWrapper.SupplierAddress.Address1);
		}

		public void TestControllingCustomerAddress()
		{
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.AddNew().FillWithValidTestData();
			controllingCustomerOrg.MainAddress.Address1 = "xxxx 0002";
			Booking.ControllingCustomerNameOrPK = controllingCustomerOrg.PK.ToString();
			AssertEquals("ControllingCustomerAddress", Booking.ControllingCustomerAddress.E2_Address1, BookingWrapper.ControllingCustomerAddress.Address1);
		}

		public void TestConsigneeDocumentaryAddress()
		{
			var consigneeDocumentaryOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeDocumentaryOrg.Contacts.AddNew().FillWithValidTestData();
			consigneeDocumentaryOrg.MainAddress.Address1 = "xxxx 0003";
			Booking.ConsigneeDocumentaryNameOrPK = consigneeDocumentaryOrg.PK.ToString();
			AssertEquals("ConsigneeDocumentaryAddress", Booking.ConsigneeDocumentaryAddress.E2_Address1, BookingWrapper.ConsigneeDocumentaryAddress.Address1);
		}

		public void TestJobContainers()
		{
			var container1 = Factory.NewWithValidTestData<ForwardingContainer>();
			container1.JC_ContainerNum = "CON1";
			container1.JC_JSB_SupplierBooking = Booking.PK;
			var container2 = Factory.NewWithValidTestData<ForwardingContainer>();
			container2.JC_ContainerNum = "CON2";
			container2.JC_JSB_SupplierBooking = Booking.PK;

			Booking.Containers.Add(container1);
			Booking.Containers.Add(container2);

			AssertEquals("container 1", "CON1", BookingWrapper.JobContainers.OfType<IDocContainer>().Select(container => container.ContainerNumber).First());
			AssertEquals("container 2", "CON2", BookingWrapper.JobContainers.OfType<IDocContainer>().Select(container => container.ContainerNumber).Last());
		}

		public void TestPlannedContainers()
		{
			var container1 = Factory.NewWithValidTestData<JobSupplierBookingPlannedContainer>();
			container1.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.J1_ParentID = Booking.PK;
			var container2 = Factory.NewWithValidTestData<JobSupplierBookingPlannedContainer>();
			container2.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.J1_ParentID = Booking.PK;

			Booking.PlannedContainers.Add(container1);
			Booking.PlannedContainers.Add(container2);

			AssertEquals("container 1", "20GP", BookingWrapper.PlannedContainers.OfType<IDocContainer>().Select(container => container.Container.Code).First());
			AssertEquals("container 2", "40GP", BookingWrapper.PlannedContainers.OfType<IDocContainer>().Select(container => container.Container.Code).Last());
		}

		public void TestNotifyParties()
		{
			var notifyPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			notifyPartyOrg.MainAddress.Address1 = "notify party 0001";

			var notifyParty2Org = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2Org.MainAddress.Address1 = "notify party 0002";

			var notifyParty3Org = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3Org.MainAddress.Address1 = "notify party 0003";

			Booking.DocAddresses.AddNew(notifyPartyOrg.MainAddress, DocAddressType.NotifyParty);
			Booking.DocAddresses.AddNew(notifyParty2Org.MainAddress, DocAddressType.NotifyParty2);
			Booking.DocAddresses.AddNew(notifyParty3Org.MainAddress, DocAddressType.NotifyParty3);

			AssertEquals("NotifyParty", "notify party 0001", BookingWrapper.NotifyPartyDocumentaryAddress.Address1);
			AssertEquals("NotifyParty2", "notify party 0002", BookingWrapper.NotifyParty2DocumentaryAddress.Address1);
			AssertEquals("NotifyParty3", "notify party 0003", BookingWrapper.NotifyParty3DocumentaryAddress.Address1);
		}

		#region Implementation

		JobSupplierBooking Booking;
		DocJobSupplierBooking BookingWrapper;

		protected override void SetUp()
		{
			Booking = Factory.New<JobSupplierBooking>();
			BookingWrapper = DocJobSupplierBooking.New(Booking, Factory);
			base.SetUp();
		}

		#endregion
	}
}
