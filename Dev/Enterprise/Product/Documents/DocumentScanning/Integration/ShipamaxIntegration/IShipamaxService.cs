using System;
using System.Collections.Generic;

namespace Enterprise.DocumentScanning.Integration
{
	public interface IShipamaxService
	{
		void SaveParseResult(Guid docPK, string docToken, ShipamaxParseResult parseResult);

		IEnumerable<ShipamaxEDocsChange> CheckEDocsChanges(Guid docPK, string docToken);
	}
}
