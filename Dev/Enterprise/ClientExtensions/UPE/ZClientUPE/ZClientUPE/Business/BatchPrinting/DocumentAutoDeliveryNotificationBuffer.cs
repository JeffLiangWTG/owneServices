
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business
{
	public class DocumentAutoDeliveryNotificationBuffer : NotificationBuffer
	{
		public DocumentAutoDeliveryNotificationBuffer(ZString heading)
			: this(heading, null)
		{
		}

		public DocumentAutoDeliveryNotificationBuffer(ZString heading, INotifications inner)
			: base(inner)
		{
			this.Heading = heading;
		}

		public readonly ZString Heading;

		protected override string EmailBodyHeader
		{
			get { return Heading + "; Generated " + ZDateTime.Now.ToString(); }
		}

		protected override string EmailBodyFooter
		{
			get { return ""; }
		}
	}
}
