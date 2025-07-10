using System;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

sealed class ControlChannelStrategyForCodeZero : IControlChannelStrategy
{
	public ControlChannelStrategyForCodeZero(ElectronicFolderResponseMessageWrapper responseMessageWrapper)
	{
		this.responseMessageWrapper = Argument.NotNull(responseMessageWrapper, nameof(responseMessageWrapper));
		ValidateCode();
	}

	public const string Code_Zero = "0";

	#region IControlChannelStrategy

	string IControlChannelStrategy.GetControlChannel()
	{
		var codes = responseMessageWrapper.ArticleCdcCodes;
		if (codes.Count == 0)
		{
			return null;
		}

		var distinctCodes = codes.Distinct().ToArray();
		return GetControlChannelCode(distinctCodes);
	}

	#endregion

	string GetControlChannelCode(string[] codes)
	{
		var prioritizedAllowedCodes = new string[3];

		foreach (var code in codes)
		{
			if (string.Equals(code, CustomsChannelCodeList.Codes.Inspection, StringComparison.OrdinalIgnoreCase))
			{
				prioritizedAllowedCodes[0] = code;
			}
			else if (string.Equals(code, CustomsChannelCodeList.Codes.DocumentControl, StringComparison.OrdinalIgnoreCase))
			{
				prioritizedAllowedCodes[1] = code;
			}
			else if (string.Equals(code, CustomsChannelCodeList.Codes.ScannerControl, StringComparison.OrdinalIgnoreCase))
			{
				prioritizedAllowedCodes[2] = code;
			}
		}

		return prioritizedAllowedCodes.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));
	}

	void ValidateCode()
	{
		var code = responseMessageWrapper.ResponseStatusCode;
		if (!string.Equals(code, Code_Zero, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException($"{nameof(ControlChannelStrategyForCodeDZero24)} is not applicable for code {code}");
		}
	}

	readonly ElectronicFolderResponseMessageWrapper responseMessageWrapper;
}
