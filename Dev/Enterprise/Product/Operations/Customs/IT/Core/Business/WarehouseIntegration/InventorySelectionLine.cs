using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

sealed class InventorySelectionLine : EU.Business.InventorySelectionLine
{
	public InventorySelectionLine(InventorySelectionHeader header) : base(header)
	{
		this.header = header;
	}

	protected override void ValidateUS_ProductQtyToDrawCore()
	{
		base.ValidateUS_ProductQtyToDrawCore();

		if (!HasDrawQty)
		{
			return;
		}

		var customsEntryKeys = GetActualCustomsEntryKeys();
		if (customsEntryKeys.Any(x => IsPendingCustomsResponse(x)))
		{
			US_ProductQtyToDrawInfo.AddError(ValidationCaptions.InventorySelectionLine.CustomsEntryKeyIsPendingForResponse);
		}
		else if (customsEntryKeys.Any(x => !IsValidCustomsEntryKey(x)))
		{
			US_ProductQtyToDrawInfo.AddError(ValidationCaptions.InventorySelectionLine.CustomsEntryKeyIsNotValid);
		}
	}

	ZString[] GetActualCustomsEntryKeys()
	{
		Func<WhsInventoryWrapper, ZString> getEntryKey = x => x.ReceiveLine?.CustomsData?.WB_EntryKey ?? ZString.Empty;
		var eligibleKeys = InventoryWrappers.Select(getEntryKey);
		var selectedKeys = header.SelectedLines.Select(getEntryKey);
		return eligibleKeys.Intersect(selectedKeys).ToArray();
	}

	static bool IsPendingCustomsResponse(ZString customsEntryKey) => customsEntryKey.Contains(DataTransfer.Universal.Constants.EntryNumberPlaceHolder, StringComparison.OrdinalIgnoreCase);

	static bool IsValidCustomsEntryKey(ZString customsEntryKey) => IsValidMRN(customsEntryKey) || customsEntryKey.StartsWith(PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito);

	static bool IsValidMRN(ZString customsEntryKey) => customsEntryKey.Length == 18 && customsEntryKey.SubstringSafe(2, 2) == Core.Constants.CountryCodes.Italy;

	readonly InventorySelectionHeader header;
}
