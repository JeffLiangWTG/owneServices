using System;
using System.Reflection;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	static class LogInfoAttributeExtension
	{
		public static LogInfoAttribute LogInfo(this Enum value)
		{
			return value.GetType()
						.GetField(value.ToString())
						.GetCustomAttribute<LogInfoAttribute>(false)
					?? throw new NotImplementedException();
		}
	}
}
