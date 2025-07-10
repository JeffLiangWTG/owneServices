using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Segments;

namespace Enterprise.Customs.AE.Business;

sealed class CONTRLInterchangeResponseProvider : ICONTRLInterchangeResponseProvider
{
	public CONTRLInterchangeResponseProvider(UCISegment uCISegment)
	{
		this.uCISegment = Argument.NotNull(uCISegment, nameof(uCISegment));
	}

	readonly UCISegment uCISegment;

	public ZString OutgoingReference => uCISegment.InterchangeControlReference;

	public ZString ActionCode => uCISegment.ActionCoded.ToString();

	public ZString SyntaxErrorCode => uCISegment.SyntaxErrorCoded.ToString();

	public ZString ErrorDataElementPosition => uCISegment.DataElementIdentification.ErroneousDataElementPositionInSegment;

	public ZString ErrorDataElementComponentPosition => uCISegment.DataElementIdentification.ErroneousComponentDataElementPosition;
}
