namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirOutturnLine : OutturnLine
	{
		public AirOutturnLine()
		{ }

		protected override int ConsignmentRef_MaxLength { get { return CusHAWB.Schema.CS_HAWBMaxLength; } }

		protected override OutturnLine GetNewOutturnLine()
		{
			return new AirOutturnLine();
		}
	}
}
