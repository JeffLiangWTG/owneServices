using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportAmendmentHeader : IMessageDataProvider
	{
		ZString ExportDeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IEnumerable<IExport5ASItem> AmendmentItems { get; }
		IOrganization Exporter { get; }
		ZString UnipassDeclarantID { get; }
	}
}
