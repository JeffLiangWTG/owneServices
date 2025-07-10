using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDGoodsShipmentWrapper<IDD429ItemWrapper>))]
sealed class IDD429GoodsShipmentWrapperTest : DocBaseWrapperCollectionTest<IDDGoodsShipmentWrapper<IDD429ItemWrapper>>
{
	protected override IDDGoodsShipmentWrapper<IDD429ItemWrapper> GetNewDocumentWrapperCollection()
	{
		return new IDDGoodsShipmentWrapper<IDD429ItemWrapper>(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var item = new MGoodsShipmentItemType04FR { StatisticalValue = 1 };
		var result = IDD429ItemWrapper.New(1, item, Factory);
		collection.Add(result);
		return result;
	}
}
