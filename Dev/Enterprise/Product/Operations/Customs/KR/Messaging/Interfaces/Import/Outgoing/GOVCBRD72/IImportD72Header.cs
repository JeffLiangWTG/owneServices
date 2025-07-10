using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportD72Header : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZInt SequenceNo { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IOrganization Declarant { get; }
		ZString DeclarantType { get; }
		ZDate BeforeReExportScheduledDate { get; }
		ZDate AfterReExportScheduledDate { get; }
		ZString ReasonDescription { get; }
		IEnumerable<IImportD72Line> Lines { get; }
	}
}
