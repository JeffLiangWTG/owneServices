using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(InvoiceHeaderActiveCollection))]
sealed class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
{
	public void TestSetDefaultsForNewElementCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.MainAddress.OA_RN_NKCountryCode = "IN";
		var addressIN = org.Addresses.AddNew();
		OrgCusCode cusCode = org.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, "1234");
		cusCode.OK_OA_PremisesAddress = addressIN.PK;
		var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
		supplierDocumentaryAddress.OrganisationPK = ZGuid.BrettsGuid;
		supplierDocumentaryAddress.E2_OA_Address = addressIN.PK;

		var collection = new InvoiceHeaderActiveCollection(declaration);
		var newMember = collection.AddNew();
		AssertEquals("When AEO empty and MainSupplier field with org of AEO cusCode, Export declaration", supplierDocumentaryAddress.OrganisationPK, newMember.AuthorizedEconomicOperatorOrgPK);
	}
}

