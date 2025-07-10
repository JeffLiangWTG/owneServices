using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

[TestedType(typeof(SupportingDocument))]
public class SupportingDocumentTest : EU.H7.Business.Testing.SupportingDocumentTest<SupportingDocument>
{
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var bill = Factory.NewWithValidTestData<AsycudaBill>();
		return bill.SupportingDocuments.AddNew();
	}

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		yield return (SupportingDocument)bill.SupportingDocuments.AddNew();
	}

	public void TestGetValidation()
	{
		var bill = Factory.NewWithValidTestData<AsycudaBill>();
		var supportingDocument = bill.SupportingDocuments.AddNew();
		Assert(supportingDocument.Validation is SupportingDocumentValidation);
	}
}
