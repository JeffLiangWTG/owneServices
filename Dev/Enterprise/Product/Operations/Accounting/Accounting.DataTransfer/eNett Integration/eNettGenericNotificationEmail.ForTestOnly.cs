#if DEBUG

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	public partial class eNettGenericNotificationEmail
	{
		public string GetBody_ForTestOnly()
		{
			return GetBody();
		}
	}
}

#endif
