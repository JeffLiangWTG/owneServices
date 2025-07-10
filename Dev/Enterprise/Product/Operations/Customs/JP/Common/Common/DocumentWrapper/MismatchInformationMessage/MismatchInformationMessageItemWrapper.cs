using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.JP.Common;

public class MismatchInformationMessageItemWrapper(MismatchInformationHouseBillProvider item) : DocumentWrapper
{
	readonly MismatchInformationHouseBillProvider item = Argument.NotNull(item, nameof(item));

	#region Item Fields

	public ZString I_11 => item?.HouseBillNumber ?? ZString.Empty;

	public ZString I_12 => item?.IsSplitShipment ?? ZString.Empty;

	public ZString I_13 => item?.ExpectedArrivalQuantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_14 => item?.DocumentQuantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_15 => item?.DocumentQuantityMismatch ?? ZString.Empty;

	public ZString I_16 => item?.ArrivalQuantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_17 => item?.ArrivalQuantityMismatch ?? ZString.Empty;

	public ZString I_18 => item?.DocumentWeight.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_19 => item?.DocumentWeightMismatch ?? ZString.Empty;

	public ZString I_20 => item?.ArrivalWeight.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_21 => item?.ArrivalWeightMismatch ?? ZString.Empty;

	public ZString I_22 => item?.MismatchReason ?? ZString.Empty;

	public ZString I_23 => item?.SpecialCargoCode ?? ZString.Empty;

	#endregion
}
