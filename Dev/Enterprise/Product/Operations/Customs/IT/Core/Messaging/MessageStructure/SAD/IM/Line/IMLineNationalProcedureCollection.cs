using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineNationalProcedureCollection
{
	public IMLineNationalProcedureCollection(IEnumerable<ZString> nationalProcedures)
	{
		NationalProcedures = Argument.NotNull(nationalProcedures, nameof(nationalProcedures));
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZInt NumberOfOccurences => NationalProcedures.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IEnumerable<ZString> NationalProcedures { get; }
}
