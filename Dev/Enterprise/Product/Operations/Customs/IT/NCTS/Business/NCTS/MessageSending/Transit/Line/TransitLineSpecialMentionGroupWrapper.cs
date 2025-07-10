using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitLineSpecialMentionGroupWrapper : NctsSADLineSpecialMentionGroupWrapper
{
	public TransitLineSpecialMentionGroupWrapper(NctsDepartureCargoDesc goodsItem) : base(goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
	}
	readonly NctsDepartureCargoDesc goodsItem;

	protected override ISpecialMentionUnloadingDataInfo UnloadingDataCore
	{
		get
		{
			var singlePreviousDocument = GetSinglePreviousDocument();
			if (singlePreviousDocument != null)
			{
				return new SADSpecialMentionUnloadingDataInfoWrapper(singlePreviousDocument.CSI_Tariff, singlePreviousDocument.CSI_Quantity, singlePreviousDocument.CSI_Quantity2);
			}
			return base.UnloadingDataCore;
		}
	}

	#region Implementation

	NctsPreviousDocument GetSinglePreviousDocument()
	{
		var previousDocuments = goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>();
		return !previousDocuments.Skip(1).Any() ? previousDocuments.SingleOrDefault() : null;
	}

	#endregion
}
