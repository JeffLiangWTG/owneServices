using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CIQRequiredDocumentAddInfo))]
	class CIQRequiredDocumentAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var document = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().CIQRequiredDocuments.AddNew();
			var result = new CIQRequiredDocumentAddInfo(document.B7_AddInfoDataInfo);
			return result;
		}
	}
}
