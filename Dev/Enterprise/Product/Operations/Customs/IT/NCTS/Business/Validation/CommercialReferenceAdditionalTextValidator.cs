using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class CommercialReferenceAdditionalTextValidator
{
	readonly NctsDepartureMovementHeader movementHeader;

	public CommercialReferenceAdditionalTextValidator(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
	}

	ZBool ShouldValidate => movementHeader.Header.BH_FTZMove && !movementHeader.BM_AdditionalText.IsEmpty;

	public void ValidateCommercialReferenceNumber(ZPropertyInfoString commercialReferencePropertyInfoString)
	{
		if (ShouldValidate && !commercialReferencePropertyInfoString.Value.IsEmpty)
		{
			commercialReferencePropertyInfoString.AddMessageError(ValidationCaptions.CommercialReferenceNumber.CommercialReferenceCannotBeSetHeaderAndGoodsItems);
		}
	}

	public void ValidateAdditionalInfo()
	{
		var goodsItems = movementHeader.GoodsItems;
		if (ShouldValidate && goodsItems.Any(obj => !obj.BY_CommercialReferenceNumber.IsEmpty))
		{
			movementHeader.BM_AdditionalTextInfo.AddMessageError(ValidationCaptions.CommercialReferenceNumber.CommercialReferenceCannotBeSetHeaderAndGoodsItems);
		}
	}
}
