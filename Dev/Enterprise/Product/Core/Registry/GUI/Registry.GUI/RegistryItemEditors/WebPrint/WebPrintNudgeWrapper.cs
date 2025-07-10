using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebPrintNudgeWrapper : NonPersistentBusinessObject
	{
		public WebPrintNudgeWrapper(WebPrintNudge webPrintNudge)
		{
			Nudge = webPrintNudge;
		}

		internal WebPrintNudge Nudge { get; set; }

		public ZBool EnableIPAddress
		{
			get { return Nudge.EnableIPAddress; }
			set { Nudge.EnableIPAddress = value; }
		}

		public ZBool EnableURLAddress
		{
			get { return !Nudge.EnableIPAddress; }
			set { Nudge.EnableIPAddress = !value; }
		}

		public ZInt SwtichBackToIPAddressIntervalInHours
		{
			get { return Nudge.SwtichBackToIPAddressIntervalInHours; }
			set { Nudge.SwtichBackToIPAddressIntervalInHours = value; }
		}
	}
}
