using System.Collections.Generic;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDUASimplifiedImportMessageDataProvider : IDUAImportDataProvider
	{
		IDUAImportCommonHeader Header { get; }

		IReadOnlyCollection<IDUAImportCommonLine> Lines { get; }
	}
}
