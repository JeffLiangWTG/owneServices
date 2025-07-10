using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebPrintNudgeSuspendingWrapper : NonPersistentBusinessObject
	{
		public WebPrintNudgeSuspendingWrapper(WebPrintNudgeSuspending webPrintNudgeSuspending)
		{
			NudgeSuspending = webPrintNudgeSuspending;
		}

		internal WebPrintNudgeSuspending NudgeSuspending { get; set; }

		public ZInt MaxErrorsInMinutes
		{
			get { return NudgeSuspending.MaxErrorsInMinutes; }
			set { NudgeSuspending.MaxErrorsInMinutes = value; }
		}

		public ZInt IntervalMinutes
		{
			get { return NudgeSuspending.IntervalMinutes; }
			set { NudgeSuspending.IntervalMinutes = value; }
		}

		public ZInt SuspendMinutes
		{
			get { return NudgeSuspending.SuspendMinutes; }
			set { NudgeSuspending.SuspendMinutes = value; }
		}

		public ZInt MaxErrorsInHours
		{
			get { return NudgeSuspending.MaxErrorsInHours; }
			set { NudgeSuspending.MaxErrorsInHours = value; }
		}

		public ZInt IntervalHours
		{
			get { return NudgeSuspending.IntervalHours; }
			set { NudgeSuspending.IntervalHours = value; }
		}

		public ZInt SuspendHours
		{
			get { return NudgeSuspending.SuspendHours; }
			set { NudgeSuspending.SuspendHours = value; }
		}
	}
}
