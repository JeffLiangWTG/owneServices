namespace Enterprise.Customs.CN.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This will be used by CN Customs")]
	public class WeightUQCalculator : Customs.Business.WeightUQCalculator
	{
		public WeightUQCalculator(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}
	}
}
