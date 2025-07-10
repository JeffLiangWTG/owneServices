using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Messages.CONTRL;
using Enterprise.Edifact.D23A.Segments;

namespace Enterprise.Customs.AE.Business;

sealed class CONTRLMessageResponseProvider : ICONTRLMessageResponseProvider
{
	public CONTRLMessageResponseProvider(SegmentGroup1 segmentGroup1)
	{
		this.segmentGroup1 = Argument.NotNull(segmentGroup1, nameof(segmentGroup1));
		this.uCMSegment = segmentGroup1.UCM[0];
	}

	readonly SegmentGroup1 segmentGroup1;
	readonly UCMSegment uCMSegment;

	public ZString OutgoingReference => uCMSegment.MessageReferenceNumber;

	public ZString ActionCode => uCMSegment.ActionCoded.ToString();

	public ZString SyntaxErrorCode => uCMSegment.SyntaxErrorCoded.ToString();

	public ZString ErrorDataElementPosition => uCMSegment.DataElementIdentification.ErroneousDataElementPositionInSegment;

	public ZString ErrorDataElementComponentPosition => uCMSegment.DataElementIdentification.ErroneousComponentDataElementPosition;

	public IReadOnlyCollection<ISegmentErrorProvider> SegmentErrors => segmentErrors
		??= segmentGroup1.Group2.Cast<SegmentGroup2>().Select(segmentGroup2 => new SegmentErrorProvider(segmentGroup2)).ToList();
	IReadOnlyCollection<ISegmentErrorProvider> segmentErrors;
}
