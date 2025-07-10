using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDJPImportMessageDataProvider : IImportCommonDataProvider
	{
		IReadOnlyCollection<IDJPDocument> Documents { get; }
		IReadOnlyCollection<IDJPDeclaration> Declarations { get; }
	}

	public interface IDJPDocument : IDocumentsCommon
	{
		ZDateTime Date { get; }
		ZString Indicator { get; }
	}

	public interface IDJPDeclaration
	{
		ZString MRN { get; }
		IReadOnlyCollection<IDJPDocument> Documents { get; }
		IReadOnlyCollection<IDJPLine> Lines { get; }
	}

	public interface IDJPLine
	{
		ZInt LineNumber { get; }
		IReadOnlyCollection<IDJPDocument> Documents { get; }
	}
}
