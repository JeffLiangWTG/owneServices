using System;
using System.Linq;
using CargoWise.Customs.Shared.MessageDefinitions;

namespace Enterprise.Customs.DE.Messaging
{
	public static class Extensions
	{
		public static string GetEmbeddedResourcePath(this Type type) => ((EmbeddedResourceAttribute)type.GetCustomAttributes(typeof(EmbeddedResourceAttribute), false).Single()).EmbeddedResourcePath;
	}
}
