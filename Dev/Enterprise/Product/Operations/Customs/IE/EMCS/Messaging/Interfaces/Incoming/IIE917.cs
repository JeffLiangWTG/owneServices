using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE917 : IEMCSInboundProvider
	{
		IReadOnlyCollection<IXMLError> Errors { get; }

		ZString AdministrativeReferenceCode { get; }
	}

	public interface IXMLError
	{
		ZString ErrorLineNumber { get; }

		ZString ErrorColumnNumber { get; }

		ZString ErrorReason { get; }

		ZString ErrorLocation { get; }

		ZString OriginalAttributeValue { get; }
	}
}
