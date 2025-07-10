using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.AuditDataServices.Subscription.Common;

namespace Enterprise.AuditDataServices.Subscription
{
	public class SubscriberLoader
	{
		#region SuppressResourceStringsCheckRegion

		// Subscriber assembly names
		public const string AccountingSubscriberCode = "Accounting";
		public const string BorderWiseSubscriberCode = "BorderWise";
		public const string DataScienceSubscriberCode = "DataScience";
		public const string DevToolsSubscriberCode = "DevTools";
		public const string DopSubscriberCode = "ArchiveManager";
		public const string EServicesSubscriberCode = "EServices";
		public const string GlowSubscriberCode = "Glow";
		public const string HvlvSubscriberCode = "HVLV";
		public const string MdmSubscriberCode = "MDM";
		public const string PaveSubscriberCode = "PAVE";
		public const string TelematicsSubscriberCode = "Telematics";
		public const string TransportBookingSubscriberCode = "TransportBooking";
		public const string XtSubscriberCode = "XT";
		public const string CoreSubscriberCode = "Core";

		// Subscription assembly name
		public const string SubscriberCode = "Subscription";
		public const string SubscribersNamespaceSuffix = ".Subscribers";

		// assembly prefixes / suffixes for subscribers
		public const string AdsNamespacePrefix = "Enterprise.AuditDataServices.";

		public const string ZClientEdiBusinessAssemblyName = "ZClientEDI.Business";
		public const string DataScienceNamespace = "Enterprise.Client.EDI.DataScience.Business.AuditSubscribers";
		public const string DeviceManagementSubscriberNamespace = "Enterprise.Client.EDI.DeviceManagement.Business.Subscribers";
		public const string DevToolsNamespace = "Enterprise.Client.EDI.DevTools.Business.Audit.Subscribers";
		public const string EServicesNamespace = "Enterprise.Client.EDI.EServices.LicenceSync.AuditSubscribers";

		public const string ZClientEdiAssemblyName = "ZClientEDI";
		public const string XtSubscriberNamespace = "Enterprise.Client.EDI.ServiceTasks.XT.Subscribers";

		// Array of subscriber codes inside this assembly
		internal readonly string[] SubscriberCodes = new string[]
		{
			SubscriberCode,
			AccountingSubscriberCode,
			BorderWiseSubscriberCode,
			DataScienceSubscriberCode,
			DevToolsSubscriberCode,
			DopSubscriberCode,
			EServicesSubscriberCode,
			GlowSubscriberCode,
			HvlvSubscriberCode,
			MdmSubscriberCode,
			PaveSubscriberCode,
			TelematicsSubscriberCode,
			TransportBookingSubscriberCode,
			CoreSubscriberCode,
			XtSubscriberCode
		};

		#endregion

		public IEnumerable<AuditSubscriberTask> EnumerateAllSubscriberTasks()
		{
			var query = new List<Type>();

			var enterpriseCode = ObjectFactory.Get<IClientHookLoader>()?.ClientHook?.UniqueId;
			var includeEdiSubscriberTasks = (enterpriseCode == "EDI");

			foreach (var subscriber in SubscriberCodes)
			{
				var assemblyName = GetSubscriberNamespacePrefix(subscriber);
				foreach (var type in RetrieveTypesFromAssembly(assemblyName, assemblyName, typeof(AuditSubscriberTask)))
				{
					if (includeEdiSubscriberTasks)
					{
						query.Add(type);
					}
					else if (!typeof(ClientSpecificAuditSubscriberTask).IsAssignableFrom(type))
					{
						query.Add(type);
					}
				}
			}

			var subscriberList = query.AsEnumerable().Distinct();
			foreach (var subscriberType in subscriberList)
			{
				yield return (AuditSubscriberTask)Activator.CreateInstance(subscriberType);
			}
		}

		public IEnumerable<IAuditSubscriber> EnumerateAllSubscriberTypes()
		{
			var query = new List<Type>();

			foreach (var subscriber in SubscriberCodes)
			{
				query.AddRange(RetrieveTypesFromAssembly(GetSubscriberNamespacePrefix(subscriber), GetSubscriberNamespace(subscriber), typeof(IAuditSubscriber)));
			}

			var enterpriseCode = ObjectFactory.Get<IClientHookLoader>()?.ClientHook?.UniqueId;
			if (enterpriseCode == "EDI")
			{
				// Add ALL EDI namespaces here
				query.AddRange(RetrieveTypesFromAssembly(ZClientEdiBusinessAssemblyName, DataScienceNamespace, typeof(IAuditSubscriber)));
				query.AddRange(RetrieveTypesFromAssembly(ZClientEdiBusinessAssemblyName, DeviceManagementSubscriberNamespace, typeof(IAuditSubscriber)));
				query.AddRange(RetrieveTypesFromAssembly(ZClientEdiBusinessAssemblyName, DevToolsNamespace, typeof(IAuditSubscriber)));
				query.AddRange(RetrieveTypesFromAssembly(ZClientEdiAssemblyName, XtSubscriberNamespace, typeof(IAuditSubscriber)));
			}

			var subscriberList = query.AsEnumerable().Distinct();
			foreach (var subscriberType in subscriberList)
			{
				yield return (IAuditSubscriber)Activator.CreateInstance(subscriberType);
			}
		}

		/// <summary>
		/// Enumarates all subscribers in the given namespace where:
		///   - It implements IAuditSubscriber
		///   - It's a concrete class
		///   - Its ends with the ".Subscribers" suffix
		/// </summary>
		public IEnumerable<IAuditSubscriber> EnumerateSubscribersOfType(string subscriberAssemblyName, string subscriberNamespace)
		{
			foreach (var subscriberType in RetrieveTypesFromAssembly(subscriberAssemblyName, subscriberNamespace, typeof(IAuditSubscriber)))
			{
				yield return (IAuditSubscriber)Activator.CreateInstance(subscriberType);
			}
		}

		public string GetSubscriberNamespacePrefix(string subscriberCode)
		{
			return AdsNamespacePrefix + subscriberCode;
		}

		public string GetSubscriberNamespace(string subscriberCode)
		{
			return GetSubscriberNamespacePrefix(subscriberCode) + SubscribersNamespaceSuffix;
		}

		IEnumerable<Type> RetrieveTypesFromAssembly(string assemblyName, string subscriberNamespace, Type baseClassType)
		{
			try
			{
				return from t in AssemblyLoader.LoadAssembly(assemblyName).GetTypes()
					 where
						t.IsClass
						&& !t.IsAbstract
						&& baseClassType.IsAssignableFrom(t)
						&& !string.IsNullOrWhiteSpace(t.Namespace)
						&& t.Namespace.StartsWith(subscriberNamespace, StringComparison.OrdinalIgnoreCase)
						&& (t.GetConstructor(Array.Empty<Type>()) is ConstructorInfo ctor && ctor.IsPublic && !ctor.IsStatic)
					select t;
			}
			catch (ReflectionTypeLoadException)
			{
				return new List<Type>();
			}
		}
	}
}
