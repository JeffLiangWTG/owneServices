using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public static class RegistryChangeLogger
	{
		public static string GetLogReference(IEnumerable<IRegistryWithLogs> originalElements, IEnumerable<IRegistryWithLogs> newElements)
		{
			var logItems = new List<(IRegistryWithLogs setting, EventType eventType)>();
			var result = new ZStringBuilder();

			foreach (var newElement in newElements)
			{
				var originalElement = originalElements.FirstOrDefault(item => item.IsSameItem(newElement));

				if (originalElement == null)
				{
					result.AppendLine(newElement.GetLogText(EventType.Add));
				}
				else if (!originalElement.IsEqual(newElement))
				{
					result.AppendLine(newElement.GetLogText(EventType.Edit));
				}
			}

			foreach (var originalElement in originalElements)
			{
				if (!newElements.Any(item => item.IsSameItem(originalElement)))
				{
					result.AppendLine(originalElement.GetLogText(EventType.Delete));
				}
			}

			return result.ToString();
		}

		public enum EventType
		{
			Add = 0,
			Edit = 1,
			Delete = 2
		}
	}
}
