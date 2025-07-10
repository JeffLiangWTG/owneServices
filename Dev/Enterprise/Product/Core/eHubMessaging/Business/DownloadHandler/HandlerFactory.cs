using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Application.Exceptions;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public static class HandlerFactory
	{
		public static IMessageHandler GetHandler(string schemaName)
		{
			IMessageHandler handler = null;
			try
			{
				handler = string.IsNullOrEmpty(schemaName) ? null : ObjectFactory.Get(schemaName) as IMessageHandler;
			}
			catch (NoSuchObjectDefinitionException)
			{
			}

			return handler ?? new UnknownTypeMessageHandler();
		}

#if DEBUG
		public static Dictionary<string, Type> DebugOnlyGetHandlerTypes()
		{
			var handlers = new Dictionary<string, Type>();
			Assembly assembly = Assembly.GetExecutingAssembly();

			foreach (Type type in assembly.GetTypes())
			{
				if (typeof(IMessageHandler).IsAssignableFrom(type))
				{
					var attributes = type.GetCustomAttributes(typeof(SupportedSchemaNameAttribute), false);

					foreach (var attr in attributes)
					{
						string fieldValue = ((SupportedSchemaNameAttribute)attr).SchemaName;
						handlers.Add(fieldValue, type);
					}
				}
			}

			return handlers;
		}

		public static IMessageHandler GetHandlerForTest(string schemaName, TimeSpan lockTimeout)
		{
			var handler = GetHandler(schemaName);
			if (handler is MessageStatusHandler messageStatusHandler)
			{
				messageStatusHandler.LockTimeout = lockTimeout;
			}
			return handler;
		}

#endif

	}
}
