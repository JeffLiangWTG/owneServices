using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDDGoodsShipmentWrapper<T> : DocBaseWrapperCollection<T> where T : DocBaseWrapper
{
	public IDDGoodsShipmentWrapper(BusinessObjectFactory factory) : base(factory)
	{
	}
}
