using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineQuotaCollection
{
	public IMLineQuotaCollection(IEnumerable<ZString> quotas)
	{
		Quotas = Argument.NotNull(quotas, nameof(quotas));
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldImportRules("R")]
	public ZInt NumberOfOccurences => Quotas.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 6, false)]
	[MessageFieldImportRules("R")]
	public IEnumerable<ZString> Quotas { get; }
}
