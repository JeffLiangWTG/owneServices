using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class DailySequenceNumberFormatter
{
	public ZString Encode(ZInt input)
	{
		if (input < Constants.MinValue || input > Constants.MaxValue)
		{
			throw new ArgumentException(FormattableString.Invariant($"{nameof(input)} must be [{Constants.MinValue}, {Constants.MaxValue}]"));
		}

		var result = new ZString();
		while (input != 0)
		{
			result = Constants.Charset[input % Constants.CharBase] + result;
			input /= Constants.CharBase;
		}
		return result.PadLeft(Constants.NumberOfDigits, '0');
	}

	public ZInt Decode(ZString input)
	{
		var effectiveInput = (string)input;
		if (effectiveInput.Length != Constants.NumberOfDigits)
		{
			throw new ArgumentException(FormattableString.Invariant($"{nameof(input)} must be a 2 chars length string"));
		}
		else if (effectiveInput.Any(x => !Constants.Charset.ToArray().Contains(x)))
		{
			throw new ArgumentException(FormattableString.Invariant($"{nameof(input)} must only contains allowed chars"));
		}

		var result = 0;
		var currentIndex = 0;
		foreach (var reversedChar in effectiveInput.Reverse())
		{
			result += Constants.Charset.IndexOf(reversedChar) * (int)Math.Pow(Constants.CharBase, currentIndex);
			currentIndex++;
		}
		return result;
	}

	public static class Constants
	{
		public const int NumberOfDigits = 2;
		public const int MinValue = 0;
		public const int MaxValue = 3843;
		public const string Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		public const int CharBase = 62;
	}
}
