using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JobDocAddressSynchroniserWithFallbackTest : TestCaseWithFactory
	{
		public void TestSynchroniser()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var supplierDoc = declaration.SupplierDocumentaryAddress;
			supplierDoc.E2_AddressOverride = ZBool.True;

			var synchroniser = new JobDocAddressSynchroniserWithFallback(supplierDoc, shipment.NotifyPartyDocumentaryAddress, shipment.ConsigneeDocumentaryAddress);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(ZGuid.Empty, supplierDoc.E2_OA_Address);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org1.MainAddress.PK, supplierDoc.E2_OA_Address);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org2.MainAddress.PK, supplierDoc.E2_OA_Address);

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org3.PK;
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org3.MainAddress.PK, supplierDoc.E2_OA_Address);

			synchroniser.SetEnabled(false, false);
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(false, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org3.MainAddress.PK, supplierDoc.E2_OA_Address);
		}

		public void TestSynchroniserSourceIsDeleted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var supplierDoc = declaration.SupplierDocumentaryAddress;

			var synchroniser = new JobDocAddressSynchroniserWithFallback(supplierDoc, shipment.NotifyPartyDocumentaryAddress, shipment.ConsigneeDocumentaryAddress);
			shipment.NotifyPartyDocumentaryAddress.Delete();
			synchroniser.SetEnabled(true, false);
			AssertNoExceptionThrown(() => synchroniser.Synchronise(true));
		}
	}
}
