namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionAcceptabilityBandResult
	{
		public BoardSectionAcceptabilityBandResult(IAcceptabilityBandOverride sectionBand, AcceptabilityBandResult result)
		{
			SectionBand = sectionBand;
			Result = result;
		}

		public IAcceptabilityBandOverride SectionBand { get; }
		public AcceptabilityBandResult Result { get; }

		public static BoardSectionAcceptabilityBandResult Empty(BoardSectionAcceptabilityBand sectionBand)
		{
			var result = AcceptabilityBandResult.CreateEmptyResultForPendingCalculation();
			return new BoardSectionAcceptabilityBandResult(sectionBand, result);
		}
	}
}
