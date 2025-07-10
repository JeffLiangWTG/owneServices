namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaOutturnLine : OutturnLine
	{
		public SeaOutturnLine()
		{ }

		protected override OutturnLine GetNewOutturnLine()
		{
			return new SeaOutturnLine();
		}

		protected override int ConsignmentRef_MaxLength { get { return CusSCAHouse.Schema.CA_HouseBillMaxLength; } }
	}
}
