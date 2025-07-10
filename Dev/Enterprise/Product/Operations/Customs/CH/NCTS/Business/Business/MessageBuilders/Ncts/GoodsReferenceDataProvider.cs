using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class GoodsReferenceDataProvider : IGoodsReference
{
	public static IEnumerable<GoodsReferenceDataProvider> NewCollection(NctsDepartureHeaderContainer headerContainer)
	{
		return headerContainer?.Header.DepartureGoodsItems.Cast<NctsDepartureCargoDesc>().Where(goodsItem => goodsItem.Packages.Cast<NctsPackage>()
					.SelectMany(x => x.ContainersPivot.Containers).Any(p => p.PK == headerContainer.PK))
					.Select((goodsItem, index) => new GoodsReferenceDataProvider(goodsItem, index + 1));
	}

	GoodsReferenceDataProvider(NctsDepartureCargoDesc goodsItem, int sequenceNumber)
	{
		this.goodsItem = goodsItem;
		this.sequenceNumber = sequenceNumber;
	}
	readonly NctsDepartureCargoDesc goodsItem;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public int DeclarationGoodsItemNumber => goodsItem.BY_DeclarationGoodsItemNumber;
}
