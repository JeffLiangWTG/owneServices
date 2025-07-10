using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class IJobWithTransportCompanyExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestGetCarrierName

		public void TestGetCarrierName()
		{
			var order = Factory.New<WhsOrder>();
			AssertEquals("", order.GetCarrierName());

			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_FullName = "TRANSPORT";
			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			AssertEquals("TRANSPORT", order.GetCarrierName());
		}

		#endregion

		#region TestGetTransportCoAddress

		public void TestGetTransportCoAddress()
		{
			AssertEquals(AddressWrapper.Empty(Factory).Address, ((IJobWithTransportCompany)null).GetTransportCoAddress(Factory).Address);

			var order = Factory.New<WhsOrder>();
			var address = order.GetTransportCoAddress(Factory);
			AssertNotNull("Precondition", order.TransportCoDocAddress);
			AssertEquals(order.TransportCoDocAddress, address.WrappedObject);
			AssertEquals(Factory, address.Factory);
		}

		#endregion

		#region TestGetTransportCoAddressLegacy

		public void TestGetTransportCoAddressLegacy()
		{
			var order = Factory.New<WhsOrder>();
			var address = order.GetTransportCoAddressLegacy(Factory);
			AssertNotNull("Precondition", order.TransportCoDocAddress);
			AssertEquals(order.TransportCoDocAddress, address.WrappedObject);
			AssertEquals(Factory, address.Factory);
		}

		#endregion

		#region TestGetTransportCompany

		public void TestGetTransportCompany()
		{
			var order = Factory.New<WhsOrder>();
			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_FullName = "TRANSPORT";
			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;

			var organisation = order.GetTransportCompany(Factory);
			AssertEquals(transportCo, organisation.Organisation);
			AssertEquals(Factory, organisation.Factory);
			AssertEquals("Transport Co", organisation.TypeDescription);
		}

		#endregion

		#region TestGetTransportCompanyLegacy

		public void TestGetTransportCompanyLegacy()
		{
			var order = Factory.New<WhsOrder>();
			order.TransportCoDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;

			var organisation = order.GetTransportCompanyLegacy(Factory);
			AssertEquals(order.TransportCoDocAddress, organisation.SelectedAddress.WrappedObject);
			AssertEquals(Factory, organisation.Factory);
		}

		#endregion
	}
}
