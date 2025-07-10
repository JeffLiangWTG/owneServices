using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class HBLCargoRegistrationInformationItemWrapper(HBLRegistrationInformationItemProvider item, int itemNumber) : DocumentEngineCore.DocWrappers.DocumentWrapper
{
	readonly HBLRegistrationInformationItemProvider item = Argument.NotNull(item, nameof(item));

	public ZString ItemNo => itemNumber.ToString().PadLeft(2, '0');

	#region Item Fields
	public ZString I_5 => item.HouseBill;

	public ZString I_6 => item.GoodsDescription;

	public ZString I_7 => item.MarksAndNumbers;

	public ZString I_8 => item.ConsigneeCode;

	public ZString I_9 => item.ConsigneeName;

	public ZString I_10 => item.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_11 => item.Quantity?.Unit ?? ZString.Empty;

	public ZString I_12 => item.TotalWeight?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_13 => item.TotalWeight?.Unit ?? ZString.Empty;

	public ZString I_14 => item.Volume?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_15 => item.Volume?.Unit ?? ZString.Empty;

	public ZString I_16 => item.SpecialCargoCode;

	public ZString I_17 => item.IsTemporaryLanding;

	public ZString I_18 => item?.StartDateForTemporaryLanding?.ToNACCSDate() ?? ZString.Empty;

	public ZString I_19 => item?.EndDateForTemporaryLanding?.ToNACCSDate() ?? ZString.Empty;

	public ZString I_20_1 => item.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString I_20_2 => item.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString I_20_3 => item.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(2) ?? ZString.Empty;

	public ZString I_20_4 => item.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(3) ?? ZString.Empty;

	public ZString I_20_5 => item.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(4) ?? ZString.Empty;
	#endregion
}
