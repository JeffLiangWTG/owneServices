using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IBox44ImportMessageDataProvider : IImportCommonDataProvider
	{
		IReadOnlyCollection<IBox44Line> Lines { get; }
	}

	public interface IBox44Line
	{
		ZInt LineNumber { get; }
		IReadOnlyCollection<IImportCommonC44CertificateDocument> DocumentsAndCertificates { get; }
	}
}
