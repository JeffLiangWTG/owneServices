using System.Linq;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureHeaderContainerPhase5Validation : EU.NCTS.Business.NctsDepartureHeaderContainerPhase5Validation
{
	public NctsDepartureHeaderContainerPhase5Validation(NctsDepartureHeaderContainer parent) : base(parent)
	{
	}

	NctsDepartureHeaderContainer HeaderContainer => (NctsDepartureHeaderContainer)Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckAtLeastOneGoodsItemIsLinkedToThisContainer();
	}

	void CheckAtLeastOneGoodsItemIsLinkedToThisContainer()
	{
		Parent.RemoveRowMessageError(ValidationCaptions.NctsHeaderContainer.NoGoodsItemIsLinkedToThisContainer);

		var isContainerLinkedToAnyGoodsItem = HeaderContainer
			.Header
			?.Bills.SelectMany(x => x.GoodsItems)
			.Cast<NctsDepartureCargoDesc>()
			.Any(goodsItem => goodsItem.ContainersSelected.Contains(Parent.BC_ContainerNum)) ?? false;

		if (!isContainerLinkedToAnyGoodsItem)
		{
			Parent.AddRowMessageError(ValidationCaptions.NctsHeaderContainer.NoGoodsItemIsLinkedToThisContainer);
		}
	}
}
