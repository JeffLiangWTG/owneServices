using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsSupportingDocument))]
sealed class NctsSupportingDocumentTest : CusSupportingInfoTest<NctsSupportingDocument>
{
	public void TestLookups()
	{
		AssertType<NctsSupportingDocumentLookups>(SupportingDocument.Lookups);
	}

		protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			yield return nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		}

	protected override BusinessObject GetNewBusinessObject() => SupportingDocument;

	NctsSupportingDocument SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument(Factory));
	NctsSupportingDocument supportingDocument;

	NctsSupportingDocument GetSupportingDocument(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader.SupportingDocuments.AddNew();
	}
}
