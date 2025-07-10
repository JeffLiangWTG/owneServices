using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ControllingCustomerRetrieverTest : TestCaseWithFactory
	{
		public void TestGetControllingCustomer()
		{
			AssertExceptionThrown<ArgumentNullException>(() => ControllingCustomerRetriever.GetControllingCustomer(null));

			var shipment = Factory.New<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			OrgHeader controllingCustomer;

			// Freight charge with sell account
			var freightCharge = job.Charges.AddNew();
			freightCharge.JR_AC = Env.Registry.FreightChargeCode;
			freightCharge.JR_OH_SellAccount = CreateOrgWithControllingAgentCustomer(out controllingCustomer).PK;

			var actualCustomer = ControllingCustomerRetriever.GetControllingCustomer(shipment.InvoicingSupporter);
			AssertEquals("Result when freight charge with sell account exists", controllingCustomer.PK, actualCustomer.PK);

			// Local client controlling customer
			job.LocalChargesPK = CreateOrgWithControllingAgentCustomer(out controllingCustomer).PK;
			actualCustomer = ControllingCustomerRetriever.GetControllingCustomer(shipment.InvoicingSupporter);
			AssertEquals("Result when local client exists", controllingCustomer.PK, actualCustomer.PK);

			// Controlling customer on a shipment
			var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			controllingAgentAddress.E2_OA_Address = Factory.New<OrgHeader>().MainAddress.PK;
			actualCustomer = ControllingCustomerRetriever.GetControllingCustomer(shipment.InvoicingSupporter);
			AssertEquals("Result when controlling customer exists", controllingAgentAddress.Organisation.PK, actualCustomer.PK);
		}

		OrgHeader CreateOrgWithControllingAgentCustomer(out OrgHeader controllingCustomer)
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(
				customer.PK,
				RelatedPartyTypeList.Codes.ControllingCustomer,
				RelatedPartyDirectionList.Codes.PickupAndDelivery,
				ZString.Empty,
				ZString.Empty,
				GlbCompany.CurrentCompany);

			controllingCustomer = customer;
			return org;
		}
	}
}
