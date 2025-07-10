using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	public class InterchangeResender
	{
		public static InterchangeResender GetInstance(EDIInterchange interchange)
		{
			if (interchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.CMR)
			{
				return new CMRInterchangeResender(interchange);
			}
			else
			{
				return new InterchangeResender(interchange);
			}
		}

		public bool Resend(bool force = false)
		{
			bool result = false;
			if (force || ShouldSetToQueued(fInterchange.EI_Status))
			{
				result = SetInterchangeToQueued();
			}
			return result;
		}

		#region Implementation

		protected InterchangeResender(EDIInterchange interchange)
		{
			fInterchange = interchange;
		}

		protected EDIInterchange fInterchange;

		protected bool SetInterchangeToQueued()
		{
			bool result = false;

			var dBFactory = new BusinessObjectFactory();
			dBFactory.RefreshEnabled = false;
			var dBInterchange = dBFactory.Load<EDIInterchange>(fInterchange.PK);
			if (dBInterchange != null)
			{
				SetToQueued(dBInterchange);
				dBFactory.Save();
				result = true;
			}

			return result;
		}

		protected virtual bool ShouldSetToQueued(ZString status) => status == EDIInterchange.Status.Sent;

		protected virtual void SetToQueued(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Queued;
		}

		#endregion
	}

	public class CMRInterchangeResender : InterchangeResender
	{
		protected internal CMRInterchangeResender(EDIInterchange interchange) : base(interchange)
		{
		}

		protected override bool ShouldSetToQueued(ZString status) => base.ShouldSetToQueued(status) || status == EDIInterchange.Status.Failed;

		protected override void SetToQueued(EDIInterchange interchange)
		{
			if (interchange.EI_Status == EDIInterchange.Status.Failed)
			{
				interchange.EI_Status = EDIInterchange.Status.SendPending;
			}
			else
			{
				base.SetToQueued(interchange);
			}

			if (interchange.EI_RetryCount > 0)
			{
				interchange.EI_RetryCount--;
			}

			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				if (message.EM_Status == EDIMessage.Status.Failed)
				{
					message.EM_Status = EDIMessage.Status.Sent;
				}
			}
		}
	}
}
