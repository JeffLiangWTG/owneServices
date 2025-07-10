using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescCollection : EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>
{
	public NctsDepartureCargoDescCollection(EU.NCTS.Business.NctsCommonMovementHeader master) : base(master)
	{
	}

	public NctsDepartureCargoDescCollection(NctsBill parent) : base(parent)
	{
	}

	protected override void SetDefaultsForNewElementCore(NctsDepartureCargoDesc newElement)
	{
		base.SetDefaultsForNewElementCore(newElement);

		newElement.AeoCertificateManager.AddAeoCertificatesIfNeeded();
	}

	public ZBool AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem() => AtLeastOneOfGoodItemsPropertyIsFilledButNotAllOfThem(x => x.BY_RN_NKCountryOfDispatch);

	public ZBool AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem() => AtLeastOneOfGoodItemsPropertyIsFilledButNotAllOfThem(x => x.BY_RN_NKCountryOfDestination);

	bool AtLeastOneOfGoodItemsPropertyIsFilledButNotAllOfThem(Func<EU.NCTS.Business.NctsCommonCargoDesc, ZString> propertyFunc)
	{
		return this.Any(x => !propertyFunc(x).IsEmpty) && this.Any(x => propertyFunc(x).IsEmpty);
	}

	public ZBool IsLastGoodsItem(ZInt lineNumber)
	{
		var lastGoodsItem = this.Cast<NctsDepartureCargoDesc>().OrderBy(x => x.BY_LineNo).LastOrDefault();
		return lastGoodsItem != null && lineNumber == lastGoodsItem.BY_LineNo;
	}

	public IEnumerable<NctsDepartureCargoDesc> GetItemsWithSendableGroupedPreviousDocuments() => this.Cast<NctsDepartureCargoDesc>().Where(x => x.GroupedPreviousDocuments.IsSendableInNbMessage);

	public bool HasAnyItemWithGroupedPreviousDocuments => this.Cast<NctsDepartureCargoDesc>().Any(x => x.GroupedPreviousDocuments.Any());
}
