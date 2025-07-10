using System;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[Serializable]
	class BasherTestNotificationType : CargoWise.ComponentModel.NotificationType
	{
		public static readonly BasherTestNotificationType UniqueFooterMessage = new BasherTestNotificationType();

		protected BasherTestNotificationType()
			: base(CargoWise.EntityFramework.NotificationType.Warning.Severity, false)
		{
		}
	}
}
