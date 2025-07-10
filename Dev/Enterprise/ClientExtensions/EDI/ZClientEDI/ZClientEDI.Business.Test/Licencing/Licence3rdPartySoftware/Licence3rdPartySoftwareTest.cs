using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(Licence3rdPartySoftware))]
	public class Licence3rdPartySoftwareTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader testHeader = factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.CreateAndLoadLicenceForOrg();
			Licence3rdPartySoftware testLicence3rdPartySoft = testHeader.LicCompany.Licence3rdPartySoftware.AddNew();
			return testLicence3rdPartySoft;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.CreateAndLoadLicenceForOrg();
			Licence3rdPartySoftware testLicence3rdPartySoft = testHeader.LicCompany.Licence3rdPartySoftware.AddNew();
			return testLicence3rdPartySoft;
		}

		public void TestL3_OP_ProductSKU()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";

			OrgSupplierPart supplierPart = Factory.New<OrgSupplierPart>();

			OrgPartRelation relation = supplierPart.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = supplier.PK;

			Licence3rdPartySoftware testRow = (Licence3rdPartySoftware)GetNewBusinessObject();
			testRow.L3_OP_ProductSKU = supplierPart.PK;

			ZQuery query = new ZQuery(OrgPartRelationSchema.OU_OP, testRow.L3_OP_ProductSKU);
			OrgPartRelation testPartRelation2 = Factory.LoadTop1<OrgPartRelation>(query);
			AssertEquals("L3_OH_Supplier should be set to the first supplier code.", testPartRelation2.OU_OH, testRow.L3_OH_Supplier);

			testRow.L3_OP_ProductSKU = ZGuid.Empty;
			AssertEquals("L3_OH_Supplier", ZGuid.Empty, testRow.L3_OH_Supplier);

			testRow.L3_OP_ProductSKU = ZGuid.NewZGuid();
			AssertEquals("L3_OH_Supplier", ZGuid.Empty, testRow.L3_OH_Supplier);
		}

		public void TestReadOnlySecurityIsDenied()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed;
			try
			{
				EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
				testHeader.CreateAndLoadLicenceForOrg();
				Licence3rdPartySoftware testLicence3rdPartySoft = testHeader.LicCompany.Licence3rdPartySoftware.AddNew();

				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = false;
				Assert("should be read only", testLicence3rdPartySoft.L3_LicenceTypeInfo.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = oldValue;
			}
		}

		public void TestReadOnlySecurityIsAllowed()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed;

			try
			{
				EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
				testHeader.CreateAndLoadLicenceForOrg();
				Licence3rdPartySoftware testLicence3rdPartySoft = testHeader.LicCompany.Licence3rdPartySoftware.AddNew();

				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = true;
				Assert("should not be read only", !testLicence3rdPartySoft.L3_LicenceTypeInfo.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = oldValue;
			}
		}

		public void TestPartDescription()
		{
			OrgSupplierPart testPart = Factory.NewWithValidTestData<OrgSupplierPart>();

			Licence3rdPartySoftware testRow = (Licence3rdPartySoftware)GetNewBusinessObject();
			AssertEquals("", testRow.PartDescription);

			testRow.L3_OP_ProductSKU = ZGuid.Invalid;
			AssertEquals("", testRow.PartDescription);

			testRow.L3_OP_ProductSKU = ZGuid.Empty;
			AssertEquals("", testRow.PartDescription);

			testRow.L3_OP_ProductSKU = testPart.PK;
			AssertEquals(testRow.PartDescription, testPart.OP_Desc);
		}
	}
}
