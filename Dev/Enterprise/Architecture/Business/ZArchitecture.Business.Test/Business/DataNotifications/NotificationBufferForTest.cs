namespace Enterprise.ZArchitecture.Business.Testing
{
	class NotificationBufferForTest : NotificationBuffer
	{
		public new string EmailBody
		{
			get { return base.EmailBody; }
		}
	}
}
