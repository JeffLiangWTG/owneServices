using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5BBHeader : IImport5BAHeader
	{
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		new IEnumerable<IImport5BBLine> EntryLines { get; }
	}
}
