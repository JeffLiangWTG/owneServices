using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : EU.H7.Business.Testing.SupportingDocumentTest<SupportingDocument>
{
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		return bill.SupportingDocuments.AddNew();
	}

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		yield return bill.SupportingDocuments.AddNew();

		var item = bill.PackedItems.AddNew();
		yield return item.SupportingDocuments.AddNew();
	}
}
