namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public abstract class BaseLine
	{
		public BaseLine(FortuneShippingDataRow row)
		{
			this.Row = row;
		}

		protected readonly FortuneShippingDataRow Row;
	}
}
