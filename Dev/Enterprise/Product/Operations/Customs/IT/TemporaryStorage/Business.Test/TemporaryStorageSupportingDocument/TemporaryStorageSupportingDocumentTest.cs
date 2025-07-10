using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageSupportingDocument))]
sealed class TemporaryStorageSupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<TemporaryStorageSupportingDocument>
{
	public void TestCSI_ReferenceNumber_MaxLength()
	{
		var supportingDocument = GetNewBusinessObject(Factory);
		AssertEquals(35, supportingDocument.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestValidationType()
	{
		var supportingDocument = GetNewBusinessObject(Factory);
		AssertType<TemporaryStorageSupportingDocumentValidation>(supportingDocument.Validation);
	}

	protected override IEnumerable<TemporaryStorageSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewBusinessObject(factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	TemporaryStorageSupportingDocument GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var supportingDocument = packedItem.SupportingDocuments.AddNew();
		return supportingDocument;
	}
}
