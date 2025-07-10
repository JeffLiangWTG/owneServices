using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GlbCompanyExtensionsTest : TestCaseWithFactory
	{
		public void TestModes()
		{
			var testOrgHeader = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			var testAddress = testOrgHeader.Addresses.AddNew();
			testAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress.OA_Address1 = "Address1";
			testAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			ObjectCreator.CreateNewCompany("CM1", orgProxy: ObjectCreator.AALSHI);

			Factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrgHeader.PK;

			var ediCommunication1 = testOrgHeader.EDICommunicationsModes.AddNew();
			ediCommunication1.EK_Module = "GEI";
			ediCommunication1.EK_FileFormat = "XML";
			ediCommunication1.EK_CommunicationsTransport = "EDP";
			ediCommunication1.EK_Destination = "ITTST";
			ediCommunication1.EK_CommsDirection = "TRX";

			var ediCommunication2 = testOrgHeader.EDICommunicationsModes.AddNew();
			ediCommunication2.EK_Module = "GEI";
			ediCommunication2.EK_FileFormat = "XML";
			ediCommunication2.EK_CommunicationsTransport = "HUB";
			ediCommunication2.EK_Destination = "IT";
			ediCommunication2.EK_CommsDirection = "TRX";

			var ediCommunication4 = testOrgHeader.EDICommunicationsModes.AddNew();
			ediCommunication4.EK_Module = "INV";
			ediCommunication4.EK_FileFormat = "CSV";
			ediCommunication4.EK_CommunicationsTransport = "EDP";
			ediCommunication4.EK_Destination = "IT";
			ediCommunication4.EK_CommsDirection = "TRX";

			var ediCommunication5 = ObjectCreator.AALSHI.EDICommunicationsModes.AddNew();
			ediCommunication5.EK_Module = "GEI";
			ediCommunication5.EK_FileFormat = "XML";
			ediCommunication5.EK_CommunicationsTransport = "HUB";
			ediCommunication5.EK_Destination = "IT";
			ediCommunication5.EK_CommsDirection = "TRX";

			Factory.Save();

			var comms = GlbCompany.CurrentCompany.LoadGEICommuncationModes();
			AssertEquals("Modes", 2, comms.Length);
			AssertContainsExactElementsInAnyOrder(comms.Cast<EDICommunicationsMode>().Select(c => c.PK), new[] { ediCommunication1.PK, ediCommunication2.PK });
		}

		protected TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
