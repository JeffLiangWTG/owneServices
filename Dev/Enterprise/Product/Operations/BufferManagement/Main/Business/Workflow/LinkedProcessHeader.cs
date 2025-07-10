namespace Enterprise.BufferManagement.Business
{
	public class LinkedProcessHeader
	{
		public LinkedProcessHeader(ProcessHeader processHeader, ProcessHeaderLink linkToProcessHeader)
		{
			ProcessHeader = processHeader;
			LinkToProcessHeader = linkToProcessHeader;
		}

		public ProcessHeader ProcessHeader { get; private set; }
		public ProcessHeaderLink LinkToProcessHeader { get; private set; }
	}
}
