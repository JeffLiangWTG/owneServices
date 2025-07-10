using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class OppositeInformationDataProvider : IOppositeInformation
{
	public static OppositeInformationDataProvider New(NctsHeader nctsHeader, ZString? referenceNumberInput = null)
	{
		OppositeInformationDataProvider dataProvider = null;
		if (nctsHeader != null)
		{
			var movementHeader = (NctsCommonMovementHeader)nctsHeader.MovementHeader ?? nctsHeader.ArrivalMovementHeader;
			if (movementHeader != null)
			{
				dataProvider = new OppositeInformationDataProvider(movementHeader, referenceNumberInput ?? ZString.Empty);
			}
		}
		return dataProvider;
	}

	OppositeInformationDataProvider(NctsCommonMovementHeader movementHeader, ZString referenceNumberInput)
	{
		this.movementHeader = movementHeader;
		this.referenceNumberInput = referenceNumberInput;
	}
	readonly NctsCommonMovementHeader movementHeader;
	readonly ZString referenceNumberInput;

	public string ReferenceNumber => referenceNumberInput.IsEmpty ? movementHeader.BM_PaperlessInbondNum.ReturnNullIfEmpty() : referenceNumberInput;

	public string Detail => null;

	public string Text => CustomsMessageHelper.PartnerTopic;
}
