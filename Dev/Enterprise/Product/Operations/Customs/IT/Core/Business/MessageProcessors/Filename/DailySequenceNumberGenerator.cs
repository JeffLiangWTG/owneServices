using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public class DailySequenceNumberGenerator
{
	public DailySequenceNumberGenerator(Account account, BusinessObjectFactory factory)
	{
		this.account = Argument.NotNull(account, nameof(account));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	readonly Account account;
	readonly BusinessObjectFactory factory;

	public ZString Generate()
	{
		var zeroBasedCurrentDailyNumber = ShiftFountainOneBasedToCustomsZeroBasedValue((ZInt)GetNumberFountain().GetNext(factory));
		return Formatter.Encode(zeroBasedCurrentDailyNumber);
	}

	public ZBool CanGenerate()
	{
		var oneBasedNextDailyNumber = GetNumberFountain().PeekPreliminary(factory);
		return oneBasedNextDailyNumber <= OneBasedMaxValue;
	}

	#region Implementation

	ZInt OneBasedMinValue => oneBasedMinValueCache ?? (oneBasedMinValueCache = ConvertZeroBasedToOneBasedAccountRangeValue(account.AccountRangeStart, DefaultAccountRangeStart)).Value;
	int? oneBasedMinValueCache;

	ZInt OneBasedMaxValue => oneBasedMaxValueCache ?? (oneBasedMaxValueCache = ConvertZeroBasedToOneBasedAccountRangeValue(account.AccountRangeEnd, DefaultAccountRangeEnd)).Value;
	int? oneBasedMaxValueCache;

	int ConvertZeroBasedToOneBasedAccountRangeValue(ZString accountRangeValue, ZString defaultAccountRangeValue)
	{
		var zeroBasedValue = Formatter.Decode(!accountRangeValue.IsEmpty ? accountRangeValue : defaultAccountRangeValue);
		return ShiftCustomsZeroBasedToFountainOneBasedValue(zeroBasedValue);
	}

	INumberFountainProxy GetNumberFountain() => Env.NumberFountains.GetITCustomsMessageFilenameNumberFountain(GetFountainKey(), OneBasedMinValue, OneBasedMaxValue);

	ZString GetFountainKey() => FormattableString.Invariant($"{FountainPrefix}_{account.AccountNode}_{ZDate.Today.ToString(ShortDateFormat, CultureInfo.InvariantCulture)}");

	ZInt ShiftCustomsZeroBasedToFountainOneBasedValue(ZInt inputValue) => inputValue + 1;
	ZInt ShiftFountainOneBasedToCustomsZeroBasedValue(ZInt inputValue) => inputValue - 1;

	DailySequenceNumberFormatter Formatter => formatter ?? (formatter = new DailySequenceNumberFormatter());
	DailySequenceNumberFormatter formatter;

	const string FountainPrefix = "ITCustomsMessageFilename";
	const string ShortDateFormat = "yyyyMMdd";
	const string DefaultAccountRangeStart = "00";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default Range End")]
	const string DefaultAccountRangeEnd = "zz";

	#endregion
}
