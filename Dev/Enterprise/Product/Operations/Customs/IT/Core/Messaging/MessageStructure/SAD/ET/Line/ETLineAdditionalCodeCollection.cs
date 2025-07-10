using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineAdditionalCodeCollection
{
	readonly IEnumerable<ZString> additionalCodes;

	public ETLineAdditionalCodeCollection(IEnumerable<ZString> additionalCodes)
	{
		this.additionalCodes = Argument.NotNull(additionalCodes, "additionalCodes");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZInt NumberOfOccurences => additionalCodes.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public IEnumerable<ZString> AdditionalCodes => additionalCodes;
}
