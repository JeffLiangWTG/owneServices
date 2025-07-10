using System;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

sealed class ControlChannelStrategyForCodeDZero24 : IControlChannelStrategy
{
	public ControlChannelStrategyForCodeDZero24(ElectronicFolderResponseMessageWrapper responseMessageWrapper)
	{
		this.responseMessageWrapper = Argument.NotNull(responseMessageWrapper, nameof(responseMessageWrapper));
		ValidateCode();
	}

	public const string Code_D024 = "D_024";

	#region IControlChannelStrategy

	string IControlChannelStrategy.GetControlChannel() => CustomsChannelCodeList.Codes.AutomaticControl;

	#endregion

	void ValidateCode()
	{
		var code = responseMessageWrapper.ResponseStatusCode;
		if (!string.Equals(code, Code_D024, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException($"{nameof(ControlChannelStrategyForCodeDZero24)} is not applicable for code {code}");
		}
	}

	readonly ElectronicFolderResponseMessageWrapper responseMessageWrapper;
}
