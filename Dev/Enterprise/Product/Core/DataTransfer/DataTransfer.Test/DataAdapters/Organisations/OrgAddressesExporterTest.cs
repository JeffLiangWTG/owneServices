using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class OrgAddressesExporterTest : TransactionedTestCase
	{
		public void TestExportLightWeight_SelectedAddressIsNull()
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();

			var result = new Organisation();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			context.SimplifiedXML = true;
			new OrgAddressesExporter(orgHeader, null).Export(result, string.Empty, context);
			AssertEquals(1, result.OrganisationDetails.Addresses.Count);
			AssertEquals(orgHeader.MainAddress.OA_Code, result.OrganisationDetails.Addresses[0].AddressCode);
		}

		public void TestExportLightWeight_SelectedAddressNotNull()
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Address1";
			address.OA_Code = "Address 1 SYD";
			address.OA_City = "Burwood";
			factory.Save();

			var result = new Organisation();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			context.SimplifiedXML = true;

			new OrgAddressesExporter(address).Export(result, string.Empty, context);

			AssertEquals(1, result.OrganisationDetails.Addresses.Count);
			AssertEquals(address.OA_Code, result.OrganisationDetails.Addresses[0].AddressCode);
		}

		public void TestExportVerbose()
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();

			var result = new Organisation();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			context.SimplifiedXML = false;

			new OrgAddressesExporter(orgHeader, null).Export(result, string.Empty, context);
			AssertEquals(orgHeader.Addresses.Count, result.OrganisationDetails.Addresses.Count);

			AssertEquals(orgHeader.MainAddress.OA_Code, result.OrganisationDetails.Addresses[0].AddressCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
		}

		BusinessObjectFactory factory;
	}
}
