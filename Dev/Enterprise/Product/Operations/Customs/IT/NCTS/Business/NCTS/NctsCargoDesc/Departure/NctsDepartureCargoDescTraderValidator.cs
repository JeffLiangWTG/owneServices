#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescTraderValidator
{
	public NctsDepartureCargoDescTraderValidator(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		header = Argument.NotNull(goodsItem.Header, nameof(goodsItem.Header));
		movementHeader = Argument.NotNull(goodsItem.Header.MovementHeader, nameof(goodsItem.Header.MovementHeader));
	}
	readonly NctsDepartureMovementHeader movementHeader;
	readonly NctsHeader header;
	readonly NctsDepartureCargoDesc goodsItem;

	public void ValidateConsignor()
	{
		var consignor = goodsItem.Consignor;
		var headerConsignor = header.Consignor;

		ValidateTraderWhenDeclarationIsGroupage(consignor, x => x.Consignor, headerConsignor);
		ValidateTraderRelatedToHeaderLevelForStandard(consignor, headerConsignor, ValidationCaptions.NctsCargoDesc.ConsignorIsEmptyAndParticipantTypeShouldBeGroupage);
	}

	public void ValidateConsignee()
	{
		var consignee = goodsItem.Consignee;
		var headerConsignee = header.Consignee;

		ValidateTraderWhenDeclarationIsGroupage(consignee, x => x.Consignee, headerConsignee);
		if (!header.IsPhase5Departure)
		{
			ValidateTraderRelatedToHeaderLevelForStandard(consignee, headerConsignee, ValidationCaptions.NctsCargoDesc.ConsigneeIsEmptyAndParticipantTypeShouldBeGroupage);
		}
	}

	#region Implementation

	void ValidateTraderWhenDeclarationIsGroupage(JobDocAddress goodsItemTrader, Func<NctsDepartureCargoDesc, JobDocAddress> getTrader, JobDocAddress headerTrader)
	{
		if (movementHeader.IsGroupage)
		{
			ValidateTraderRelatedToGoodsItemsCount(goodsItemTrader, headerTrader);
			ValidateTraderRelatedToOtherGoodsItems(goodsItemTrader, getTrader, headerTrader);
		}
	}

	void ValidateTraderRelatedToGoodsItemsCount(JobDocAddress goodsItemTrader, JobDocAddress headerTrader)
	{
		if (headerTrader.IsEmpty && !goodsItemTrader.IsEmpty && GoodsItem.Count == 1)
		{
			goodsItemTrader.OrganisationPKInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.MustHaveMoreThanOneGoodsItemToDeclareTradersAtGoodsItemLevel);
		}
	}

	void ValidateTraderRelatedToOtherGoodsItems(JobDocAddress goodsItemTrader, Func<NctsDepartureCargoDesc, JobDocAddress> getTrader, JobDocAddress headerTrader)
	{
		var goodsItems = GoodsItem;

		if (headerTrader.IsEmpty && goodsItems.Count > 1 && HaveSameOrganizationForAllGoodsItem())
		{
			goodsItemTrader.OrganisationPKInfo.AddMessageError(ValidationCaptions.NctsCargoDesc.MustHaveDifferentOrganizationsAtGoodsItemLevel);
		}

		bool HaveSameOrganizationForAllGoodsItem()
		{
			return goodsItems
				.Cast<NctsDepartureCargoDesc>()
				.Where(x => !getTrader(x).OrganisationPK.IsEmpty)
				.DistinctBy(x => getTrader(x).OrganisationPK).Count() == 1;
		}
	}

	void ValidateTraderRelatedToHeaderLevelForStandard(JobDocAddress goodsItemTrader, JobDocAddress headerTrader, string messageError)
	{
		if (movementHeader.ParticipantType == NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter && headerTrader.IsEmpty && !goodsItemTrader.IsEmpty)
		{
			goodsItemTrader.OrganisationPKInfo.AddMessageError(messageError);
		}
	}

	NctsDepartureCargoDescCollection GoodsItem => movementHeader.GoodsItems;

	#endregion
}
