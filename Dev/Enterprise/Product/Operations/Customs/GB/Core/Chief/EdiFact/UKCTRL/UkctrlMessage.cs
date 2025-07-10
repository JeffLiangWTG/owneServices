namespace Enterprise.Customs.GB.Chief.EdiFact.UKCTRL
{
	public class UkctrlMessage
	{
		public UkctrlMessage()
		{
			GroupTwos = new GroupTwoCollection();
		}

		public UkctrlMessage(UkctrlHeader hdr)
			: this()
		{
			this.Header = hdr;
		}

		public UkctrlMessage(UkctrlHeader header, GroupTwoCollection groupTwos)
			: this()
		{
			this.Header = header;
			this.GroupTwos = groupTwos;
		}

		public UkctrlHeader Header { get; private set; }
		public GroupTwoCollection GroupTwos { get; private set; }

		public bool IsOverallSuccess
		{
			get
			{
				return this.Header.UCX_ACTION_CODE == "1";
			}
		}
	}
}
