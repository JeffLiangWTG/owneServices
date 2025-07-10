using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDocAddressForSimplifiedLVS))]
	sealed class JobDocAddressForSimplifiedLVSTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNew()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			var docAddress = JobDocAddressForSimplifiedLVS.New(simplifiedLVS, DocAddressTypes.Codes.SupplierDocumentaryAddress);
			AssertType<SimplifiedLVSJobDocAddressValidation>(docAddress.AdditionalValidation);
			AssertEquals(simplifiedLVS, docAddress.SimplifiedLVS);
			AssertEquals(DocAddressTypes.Codes.SupplierDocumentaryAddress, docAddress.E2_AddressType);
			AssertEquals(ZArchitecture.Business.AddressType.NoDefault, docAddress.DefaultAddressType);
		}

		public void TestReadOnly()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			var supplierDocumentaryAddress = simplifiedLVS.SupplierDocumentaryAddress;
			Assert("ReadOnly", !supplierDocumentaryAddress.ReadOnly);

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			var invoice = declaration.Invoices.AddNew();

			simplifiedLVS.Invoices.Add(invoice);
			Assert("ReadOnly", supplierDocumentaryAddress.ReadOnly);
		}

		public void TestValidation()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			var supplierDocumentaryAddress = simplifiedLVS.SupplierDocumentaryAddress;
			Assert(supplierDocumentaryAddress.Validation.ContainsPiggybackedValidation(typeof(SimplifiedLVSJobDocAddressValidation)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			return JobDocAddressForSimplifiedLVS.New(simplifiedLVS, DocAddressTypes.Codes.SupplierDocumentaryAddress);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			return JobDocAddressForSimplifiedLVS.New(simplifiedLVS, DocAddressTypes.Codes.SupplierDocumentaryAddress);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("JobDocAddressForSimplifiedLVS is not supposed to be saved by factory by default", true);
		}

		public override bool EnableAllowSpatialTypesAttributeTest => false;
	}
}
