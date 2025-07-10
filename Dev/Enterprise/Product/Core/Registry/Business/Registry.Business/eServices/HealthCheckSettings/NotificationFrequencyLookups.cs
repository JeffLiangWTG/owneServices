using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.eServices.HealthCheckSettings
{
	public class NotificationFrequencyLookups : ZLookups
	{
		public NotificationFrequencyLookups(NotificationFrequency parent)
			: base(parent) { }

		public CodeDescriptionPairList Settings
		{
			get { return HealthCheckConstants.GetFailedEDIInterchangeNotificationFrequencyOptions(); }
		}
	}
}
