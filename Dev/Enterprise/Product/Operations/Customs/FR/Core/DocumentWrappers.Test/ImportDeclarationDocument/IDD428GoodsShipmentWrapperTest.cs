using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDGoodsShipmentWrapper<IDD428ItemWrapper>))]
sealed class IDD428GoodsShipmentWrapperTest : DocBaseWrapperCollectionTest<IDDGoodsShipmentWrapper<IDD428ItemWrapper>>
{
	protected override IDDGoodsShipmentWrapper<IDD428ItemWrapper> GetNewDocumentWrapperCollection()
	{
		return new IDDGoodsShipmentWrapper<IDD428ItemWrapper>(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var item = new MGoodsShipmentItemType05FR { StatisticalValue = 1 };
		var result = IDD428ItemWrapper.New(1, item, Factory);
		collection.Add(result);
		return result;
	}
}
