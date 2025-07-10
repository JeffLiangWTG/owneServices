using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IBox40AmendmentImportMessageDataProvider : IImportCommonDataProvider
	{
		IReadOnlyCollection<IBox40AmendmentLine> Lines { get; }
	}

	public interface IBox40AmendmentLine
	{
		ZInt LineNumber { get; }
		ZString PrecedentDocumentType { get; }
		ZString PrecedentDocumentClass { get; }
		ZString PrecedentDocumentReference { get; }
	}
}
