namespace Enterprise.Client.UPE.Business
{
	public class AbandonDetails : UPECusHAWBFlagDetails
	{
		public AbandonDetails(UPECusHAWB uPECusHAWB) : base(uPECusHAWB)
		{
		}

		public override string FlagName
		{
			get { return "Abandon"; }
		}

		protected override string NoteReference
		{
			get { return ""; }
		}
	}
}
