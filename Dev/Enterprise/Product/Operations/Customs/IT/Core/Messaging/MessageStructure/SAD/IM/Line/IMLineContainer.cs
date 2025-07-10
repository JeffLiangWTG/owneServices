using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineContainer
{
	public IMLineContainer(ZString container)
	{
		Container = Argument.NotNullOrEmpty(container, nameof(container));
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldImportRules("D", "C55")]
	[MessageFieldDepositoRules("D", "C55")]
	public ZString Container { get; }

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	public ZString TotalPartialUnloadContainerIndicator => ZString.Empty;
}
