using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public interface ICONTRLDataProvider : IInboundMessageDataProvider
{
	ICONTRLInterchangeResponseProvider InterchangeResponse { get; }

	ICONTRLMessageResponseProvider MessageResponse { get; }
}

public interface ICONTRLInterchangeResponseProvider : ISyntaxErrorProvider
{
	ZString OutgoingReference { get; }

	ZString ActionCode { get; }
}

public interface ICONTRLMessageResponseProvider : ICONTRLInterchangeResponseProvider
{
	IReadOnlyCollection<ISegmentErrorProvider> SegmentErrors { get; }
}

public interface ISegmentErrorProvider
{
	ZString SegmentPosition { get; }

	ZString SegmentSyntaxErrorCode { get; }

	IReadOnlyCollection<ISyntaxErrorProvider> DataElementErrors { get; }
}

public interface ISyntaxErrorProvider
{
	ZString SyntaxErrorCode { get; }

	ZString ErrorDataElementPosition { get; }

	ZString ErrorDataElementComponentPosition { get; }
}
