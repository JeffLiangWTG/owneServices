namespace Enterprise.Customs.CA.Business
{
	public class WeightUQCalculator : Customs.Business.WeightUQCalculator
	{
		public WeightUQCalculator(CusEntryHeader entryHeader) : base(entryHeader) { }

		protected override int MaximumEntryHeaderCount
		{
			get { return 2; }
		}
	}
}
