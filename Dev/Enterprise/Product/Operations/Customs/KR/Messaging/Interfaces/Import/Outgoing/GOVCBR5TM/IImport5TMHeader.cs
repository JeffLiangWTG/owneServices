using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5TMHeader : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		IOrganization Payer { get; }
		IEnumerable<IImport5TMLine> EntryLines { get; }
	}
}
