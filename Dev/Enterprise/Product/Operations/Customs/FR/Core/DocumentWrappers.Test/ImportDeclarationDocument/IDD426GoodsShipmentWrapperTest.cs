using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDGoodsShipmentWrapper<IDD426ItemWrapper>))]
sealed class IDD426GoodsShipmentWrapperTest : DocBaseWrapperCollectionTest<IDDGoodsShipmentWrapper<IDD426ItemWrapper>>
{
	protected override IDDGoodsShipmentWrapper<IDD426ItemWrapper> GetNewDocumentWrapperCollection()
	{
		return new IDDGoodsShipmentWrapper<IDD426ItemWrapper>(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var item = new MGoodsShipmentItemType05FR { StatisticalValue = 1 };
		var result = IDD426ItemWrapper.New(1, item, Factory);
		collection.Add(result);
		return result;
	}
}
