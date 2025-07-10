namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreVisualisationLookups : AutoMENTAgedScoreVisualisationLookups
	{
		public MENTAgedScoreVisualisationLookups(AutoMENTAgedScoreVisualisation parent)
			: base(parent)
		{
		}

		public GraphTypes GraphTypes
		{
			get { return Factory.GetCachedValue<GraphTypes>(); }
		}
	}
}
