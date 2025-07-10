using System;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery
{
	public class EmailNotifier : MasterFiles.Business.MessageDelivery.EmailNotifier
	{
		protected override string BuildHTMLExceptionMessage(Exception exception)
		{
			if (exception is KnownErrorException)
			{
				return base.BuildHTMLExceptionMessage(exception, true);
			}
			else
			{
				return base.BuildHTMLExceptionMessage(exception, false);
			}
		}
	}
}
