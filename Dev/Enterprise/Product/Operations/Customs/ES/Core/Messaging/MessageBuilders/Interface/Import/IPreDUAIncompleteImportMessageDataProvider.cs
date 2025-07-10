using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IPreDUAIncompleteImportMessageDataProvider : IImportCommonDataProvider
	{
		IPDIHeader Header { get; }

		IReadOnlyCollection<IImportCommonLine> Lines { get; }
	}

	public interface IPDIHeader : IImportCommonHeader
	{
		ZString CustomsOffice { get; }
	}
}
