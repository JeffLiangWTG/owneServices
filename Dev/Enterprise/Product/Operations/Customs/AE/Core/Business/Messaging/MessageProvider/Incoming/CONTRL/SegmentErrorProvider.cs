using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Messages.CONTRL;
using Enterprise.Edifact.D23A.Segments;

namespace Enterprise.Customs.AE.Business;

sealed class SegmentErrorProvider : ISegmentErrorProvider
{
	public SegmentErrorProvider(SegmentGroup2 segmentGroup2)
	{
		this.segmentGroup2 = Argument.NotNull(segmentGroup2, nameof(segmentGroup2));
	}

	readonly SegmentGroup2 segmentGroup2;

	public ZString SegmentPosition => segmentGroup2.UCS[0].SegmentPositionInMessageBody;

	public ZString SegmentSyntaxErrorCode => segmentGroup2.UCS[0].SyntaxErrorCoded.ToString();

	public IReadOnlyCollection<ISyntaxErrorProvider> DataElementErrors => dataElementErrors ??= GetDataElementErrors();
	IReadOnlyCollection<ISyntaxErrorProvider> dataElementErrors;

	List<SyntaxErrorProvider> GetDataElementErrors()
	{
		var dataElementErrors = new List<SyntaxErrorProvider>();
		foreach (UCDSegment uCDSegment in segmentGroup2.UCD)
		{
			var errorCode = uCDSegment.SyntaxErrorCoded.ToString();
			var errorDataElementPosition = uCDSegment.DataElementIdentification.ErroneousDataElementPositionInSegment;
			var errorDataElementComponentPosition = uCDSegment.DataElementIdentification.ErroneousComponentDataElementPosition;

			dataElementErrors.Add(new SyntaxErrorProvider(errorCode, errorDataElementPosition, errorDataElementComponentPosition));
		}

		return dataElementErrors;
	}
}
