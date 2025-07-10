using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<NctsSupportingDocument>))]
	class HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<NctsSupportingDocument>, NctsSupportingDocument, HouseConsignmentGoodItemsSupportingDocumentsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<NctsSupportingDocument> GetColumnLayoutBuilderForTesting() => new HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<NctsSupportingDocument>();
	}
}
