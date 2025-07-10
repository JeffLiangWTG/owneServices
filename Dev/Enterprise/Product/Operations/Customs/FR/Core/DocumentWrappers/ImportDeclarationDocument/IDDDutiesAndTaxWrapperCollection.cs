using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDDDutiesAndTaxWrapperCollection<T> : DocBaseWrapperCollection<T> where T : DocBaseWrapper
{
	public IDDDutiesAndTaxWrapperCollection(BusinessObjectFactory factory) : base(factory)
	{
	}
}
