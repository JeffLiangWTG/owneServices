using System;
using System.ComponentModel;
using System.Reflection;

namespace Enterprise.ZArchitecture.ComponentModel
{
	internal class ControlEventDescriptorCollection : EventDescriptorCollection
	{
		readonly Type controlType;

		public ControlEventDescriptorCollection(
					Type controlType)
			: this(controlType, GetDefaultEventCollection(controlType))
		{
		}

		public ControlEventDescriptorCollection(
					Type controlType,
					EventDescriptor[] defaultDescriptors)
			: base(defaultDescriptors)
		{
			this.controlType = controlType;
		}

		public override EventDescriptor Find(string name, bool ignoreCase)
		{
			var result = base.Find(name, ignoreCase);

			if (result == null)
			{
				result = CreateEventDescriptor(controlType, name);
				Add(result);
			}

			return result;
		}

		static EventDescriptor CreateEventDescriptor(Type componentType, string name)
		{
			var info = componentType.GetEvent(name);

			return info != null
						? CreateEventDescriptor(componentType, info)
						: null;
		}

		static EventDescriptor CreateEventDescriptor(Type componentType, EventInfo info)
		{
			return TypeDescriptor.CreateEvent(componentType, info.Name, info.EventHandlerType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event descriptor")]
		static EventDescriptor[] GetDefaultEventCollection(Type controlType)
		{
			var validatingEvent = CreateEventDescriptor(controlType, "Validating");
			return (validatingEvent != null) ? new EventDescriptor[] { validatingEvent } : Array.Empty<EventDescriptor>();
		}
	}
}
