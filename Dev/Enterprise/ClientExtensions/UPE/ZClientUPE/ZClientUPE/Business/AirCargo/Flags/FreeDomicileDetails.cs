namespace Enterprise.Client.UPE.Business
{
	public class FreeDomicileDetails : UPECusHAWBFlagDetails
	{
		public FreeDomicileDetails(UPECusHAWB uPECusHAWB) : base(uPECusHAWB)
		{
		}

		public override string FlagName
		{
			get { return "Free Domicile"; }
		}

		protected override string NoteReference
		{
			get { return ""; }
		}
	}
}
