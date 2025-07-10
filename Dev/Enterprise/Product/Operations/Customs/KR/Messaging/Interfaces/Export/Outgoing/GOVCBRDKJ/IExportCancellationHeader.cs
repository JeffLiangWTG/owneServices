using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportCancellationHeader : IMessageDataProvider
	{
		ZString ExportDeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IOrganization Exporter { get; }
		ZString UnipassDeclarantID { get; }
	}
}
