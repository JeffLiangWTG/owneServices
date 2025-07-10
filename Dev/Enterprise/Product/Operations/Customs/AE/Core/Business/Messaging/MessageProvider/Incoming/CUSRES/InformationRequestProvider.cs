using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Messages.CUSRES;
using Enterprise.Edifact.D23A.Segments;

namespace Enterprise.Customs.AE.Business;

sealed class InformationRequestProvider : IInformationRequest
{
	public InformationRequestProvider(SegmentGroup4 segmentGroup4)
	{
		this.segmentGroup4 = Argument.NotNull(segmentGroup4, nameof(segmentGroup4));
	}
	readonly SegmentGroup4 segmentGroup4;

	public ZString ErrorSegment => segmentGroup4.ERP[0].ErrorSegmentPointDetails.SegmentTagIdentifier;

	public ZString RequestType => segmentGroup4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode;

	public IReadOnlyCollection<ZString> ResponseDetails => responseDetails
		??= segmentGroup4.FTX.Cast<FTXSegment>().Select(x => (ZString)x.TextLiteral.FreeText1).ToList();
	IReadOnlyCollection<ZString> responseDetails;
}
