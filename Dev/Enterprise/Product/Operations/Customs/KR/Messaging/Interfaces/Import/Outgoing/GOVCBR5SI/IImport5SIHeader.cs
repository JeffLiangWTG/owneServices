using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SIHeader : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IEnumerable<IImport5SILine> MailItemIDs { get; }
	}
}
