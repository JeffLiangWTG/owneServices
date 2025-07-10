using System;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.Web.Test
{
	public class ShipamaxServiceForTest : ShipamaxService
	{
		public EDocsShipamaxMessage GetEDocsShipamaxMessageExposed(Guid docPK, string docToken) => GetEDocsShipamaxMessage(docPK, docToken);

		public bool ValidateDocTokenExposed(Guid docPK, string docToken, out EDocsShipamaxMessage shipamaxMessage) => ValidateDocToken(docPK, docToken, out shipamaxMessage);
	}
}
