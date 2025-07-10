using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(SupplierInformation))]
	public class SupplierInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T109", SupplierInformation.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SupplierInformation();
		}
	}

	[TestedType(typeof(SupplierInformationCollection))]
	public class SupplierInformationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SupplierInformationCollection>
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
			OrgAddress testAddress = org.Addresses[0];
			testAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress.OA_CompanyNameOverride = "CHINACOMPANY";
			testAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			Factory.Save();
			SupplierInformationCollection collection = new SupplierInformationCollection(Factory);
			SupplierInformation supplierInformation = collection.Cast<SupplierInformation>().FirstOrDefault(var => var.SupplierCode == "TestDebtor");
			AssertNull(supplierInformation);
			supplierInformation = collection.Cast<SupplierInformation>().FirstOrDefault(var => var.SupplierCode == "TestCreditor");
			AssertNotNull(supplierInformation);
			AssertEquals("CHINACOMPANY", supplierInformation.SupplierName);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupplierInformation();
		}

		protected override SupplierInformationCollection GetCollectionToTest()
		{
			return new SupplierInformationCollection(Factory);
		}
	}
}
