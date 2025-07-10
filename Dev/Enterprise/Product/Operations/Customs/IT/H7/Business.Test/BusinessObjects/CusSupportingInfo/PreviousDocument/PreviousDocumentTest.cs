using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(PreviousDocument))]
sealed class PreviousDocumentTest : EU.H7.Business.Testing.PreviousDocumentTest<PreviousDocument>
{
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		return bill.PreviousDocuments.AddNew();
	}

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		yield return bill.PreviousDocuments.AddNew();

		var item = bill.PackedItems.AddNew();
		yield return item.PreviousDocuments.AddNew();
	}
}
