using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ClientInformation))]
	public class ClientInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T110", ClientInformation.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ClientInformation();
		}
	}

	[TestedType(typeof(ClientInformationCollection))]
	public class ClientInformationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ClientInformationCollection>
	{
		public void TestDefaultElements()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestDebtor";
			org1.OH_IsActive = true;
			OrgCompanyData data1 = org1.CompanyData;
			data1.OB_IsDebtor = true;
			data1.OB_IsCreditor = false;
			data1.OB_GC = GlbCompany.CurrentCompany.PK;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestCreditor";
			org.OH_IsActive = true;
			OrgCompanyData data = org.CompanyData;
			data.OB_IsDebtor = false;
			data.OB_IsCreditor = true;
			data.OB_GC = GlbCompany.CurrentCompany.PK;
			OrgAddress testAddress = org1.Addresses[0];
			testAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			testAddress.OA_CompanyNameOverride = "CHINACOMPANY";
			testAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			Factory.Save();
			ClientInformationCollection collection = new ClientInformationCollection(Factory);
			ClientInformation clientInformation = collection.Cast<ClientInformation>().FirstOrDefault(var => var.ClientCode == "TestDebtor");
			AssertNotNull(clientInformation);
			AssertEquals("CHINACOMPANY", clientInformation.ClientName);
			clientInformation = collection.Cast<ClientInformation>().FirstOrDefault(var => var.ClientCode == "TestCreditor");
			AssertNull(clientInformation);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClientInformation();
		}

		protected override ClientInformationCollection GetCollectionToTest()
		{
			return new ClientInformationCollection(Factory);
		}
	}
}
