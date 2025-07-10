using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineAdditionalCodeCollection
{
	public IMLineAdditionalCodeCollection(IEnumerable<ZString> additionalCodes)
	{
		AdditionalCodes = Argument.NotNull(additionalCodes, nameof(additionalCodes));
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZInt NumberOfOccurences => AdditionalCodes.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IEnumerable<ZString> AdditionalCodes { get; }
}
