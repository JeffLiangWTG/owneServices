using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Definitions;

namespace Enterprise.MailManager
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[Serializable, XmlSerializerAssembly("MailManager.XmlSerializers")]
	public sealed class MailSubscriberAttribute : AssemblyMetaDataAttributeWithType, IMailFilterProvider
	{
		[Obsolete("Use the constructor that accepts a Type. This is only here for the Deserialiser.")]
		public MailSubscriberAttribute() { }

		public MailSubscriberAttribute(Type type)
			: base(type)
		{
			// We could serialise the Code <-> MethodName pairs to skip loading assemblies at run time...
		}

		public IEnumerable<IMailFilter> GetFilters()
		{
			try
			{
				return FindFilterMethods(Type).Select(m => m.Invoke(null, null)).Cast<IMailFilter>();
			}
			catch (FileNotFoundException)
			{
				//caused by a ZClient dll not being present. We can safely ignore filters from missing assemblies since client isn't using them anyway.
				return Array.Empty<IMailFilter>();
			}
		}

		static IEnumerable<MethodInfo> FindFilterMethods(Type t)
			=> t.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(IsMailFilter);

		static bool IsMailFilter(MethodInfo m)
			=> m.GetCustomAttribute<MailFilterAttribute>() != null;

		public bool TryGetFilter(string code, out IMailFilter filter)
		{
			try
			{
				var filterMethod = Type.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.GetCustomAttribute<MailFilterAttribute>()?.Code == code);
				filter = (IMailFilter)filterMethod?.Invoke(null, null);
				return filter != null;
			}
			catch (FileNotFoundException)
			{
				//caused by a ZClient dll not being present. We can safely ignore filters from missing assemblies since client isn't using them anyway.
				filter = null;
				return false;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class MailFilterAttribute : Attribute
	{
		public string Code { get; }

		public MailFilterAttribute(string code)
			=> Code = code;
	}
}
