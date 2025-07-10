using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationItemValidation : ZValidation
	{
		public ExitNotificationItemValidation(ExitNotificationItem parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(ExitNotificationItemValidation);

		public override void ValidateAll()
		{
		}
	}
}
