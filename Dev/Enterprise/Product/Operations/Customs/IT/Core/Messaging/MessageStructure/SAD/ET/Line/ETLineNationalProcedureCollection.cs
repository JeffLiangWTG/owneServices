using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineNationalProcedureCollection
{
	readonly IEnumerable<ZString> nationalProcedures;

	public ETLineNationalProcedureCollection(IEnumerable<ZString> nationalProcedures)
	{
		this.nationalProcedures = Argument.NotNull(nationalProcedures, "nationalProcedures");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	public ZInt? NumberOfOccurences => nationalProcedures.Any() ? nationalProcedures.Count() : null;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	public IEnumerable<ZString> NationalProcedures => nationalProcedures;
}
