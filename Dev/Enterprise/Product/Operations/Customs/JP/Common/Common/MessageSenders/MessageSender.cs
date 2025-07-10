using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common
{
	public abstract class MessageSender : Business.MessageSender
	{
		protected MessageSender(IMessageSenderSupporter job) : base(job)
		{
		}

		protected override bool SaveJob()
		{
			if (Job == null)
			{
				return false;
			}

			if (Job.HasChanges)
			{
				try
				{
					Job.Factory.Save();
				}
				catch (ZSaveException ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
					return false;
				}
			}

			return true;
		}

		protected override bool Prepare() => true;
	}
}
