using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecContainerDataProvider : IEdecContainer
{
	public static EdecContainerDataProvider New(CusContainer cusContainer) => cusContainer == null ? null : new EdecContainerDataProvider(cusContainer);

	EdecContainerDataProvider(CusContainer cusContainer)
	{
		container = Argument.NotNull(cusContainer, nameof(cusContainer));
	}

	readonly CusContainer container;

	public string ContainerNumber => container.CO_ContainerNumber;
}
