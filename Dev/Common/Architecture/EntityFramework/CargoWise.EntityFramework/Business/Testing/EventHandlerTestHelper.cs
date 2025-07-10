#if DEBUG

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	public class EventHandlerTestHelper : Assertion, IDisposable
	{
		public EventHandlerTestHelper(BusinessObject targetObject)
		{
			TargetObject = targetObject;
			RegisterEventHandlersForZProperties();
		}

		public void Dispose()
		{
			UnregisterEventHandlers();
		}

		public object TargetObject { get; protected set; }

		public BusinessObject TargetBusinessObject
		{
			get { return (BusinessObject)TargetObject; }
		}

		public void AssertValueChangedEventFired(string message, string propertyName)
		{
			AssertEquals(message, true, CheckValueChangedEventHasBeenFired(propertyName));
		}

		public void AssertValueChangedEventFired(string propertyName)
		{
			AssertEquals(true, CheckValueChangedEventHasBeenFired(propertyName));
		}

		public void AssertValueChangedEventNotFired(string message, string propertyName)
		{
			AssertEquals(message, false, CheckValueChangedEventHasBeenFired(propertyName));
		}

		public void AssertValueChangedEventNotFired(string propertyName)
		{
			AssertEquals(false, CheckValueChangedEventHasBeenFired(propertyName));
		}

		#region Implementation

		readonly List<EventHandlerInfo> handlers = new List<EventHandlerInfo>();

		void RegisterEventHandlersForZProperties()
		{
			if (TargetBusinessObject != null)
			{
				foreach (var propertyInfo in TargetBusinessObject.GetType().GetProperties())
				{
					if (propertyInfo.PropertyType == typeof(ZPropertyInfo))
					{
						var zPropertyInfo = (ZPropertyInfo)propertyInfo.GetValue(TargetBusinessObject);
						RegisterEventHandler(zPropertyInfo, zPropertyInfo.Name, "ValueChanged");
					}
				}
			}
		}

		class EventHandlerInfo
		{
			public object Raiser;
			public string RaiserKey;
			public EventInfo Event;
			public EventHandler Handler;
			public bool HasBeenFired;
		}

		void RegisterEventHandler(object raiser, string raiserKey, string eventName)
		{
			var handler = new EventHandlerInfo()
			{
				Raiser = raiser,
				RaiserKey = raiserKey,
				Event = raiser.GetType().GetEvent(eventName)
			};
			handler.Handler = delegate
			{ handler.HasBeenFired = true; };
			handler.Event.AddEventHandler(raiser, handler.Handler);
			handlers.Add(handler);
		}

		void UnregisterEventHandlers()
		{
			foreach (var handler in handlers)
			{
				handler.Event.RemoveEventHandler(handler.Raiser, handler.Handler);
			}
		}

		bool CheckValueChangedEventHasBeenFired(string propertyName)
		{
			if (TargetBusinessObject == null)
			{
				throw new Exception("This assertion cannot be made when TargetObject is not a BusinessObject");
			}
			var propertyInfo = TargetBusinessObject.GetType().GetProperties().Single(p => p.Name == propertyName);
			if (propertyInfo.PropertyType != typeof(ZPropertyInfo))
			{
				throw new Exception(String.Format(CultureInfo.InvariantCulture, "This assertion can be made for ZProperties only, but the {0} property is not a ZProperty", propertyName));
			}
			var zPropertyInfo = (ZPropertyInfo)propertyInfo.GetValue(TargetBusinessObject);
			return CheckEventHasBeenFired(zPropertyInfo, zPropertyInfo.Name, "ValueChanged");
		}

		bool CheckEventHasBeenFired(object raiser, string raiserKey, string eventName)
		{
			var handler = handlers.Find(h => h.RaiserKey == raiserKey && h.Event.Name == eventName)
				?? throw new Exception(String.Format(CultureInfo.InvariantCulture, "This assertion cannot be made: event {0} for the {1} object has not been registered", eventName, raiser));
			return handler.HasBeenFired;
		}

		#endregion
	}
}

#endif
