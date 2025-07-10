using System.Linq;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public static class AllowedTriggerEvents
	{
		public static bool IsAllowedEventType(Event eventType)
		{
			var result = !Events.ChangeLogs.Contains(eventType)
				&& !Events.Exceptions.Contains(eventType)
				&& !Events.InactiveEvents.Contains(eventType)
				&& !Events.Customizables.Contains(eventType)
				&& eventType != Events.WorkflowTriggerEvent
				|| eventType == Events.SetToActive
				|| eventType == Events.SetToInactive;

			if (result && DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				result = Events.ProductivityWiseEvents.Contains(eventType);
			}

			return result;
		}
	}
}
