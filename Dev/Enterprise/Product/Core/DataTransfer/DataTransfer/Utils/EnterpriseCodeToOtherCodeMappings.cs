using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Business
{
	[Immutable]
	public abstract class EnterpriseCodeExternalCodeMappings : IEnumerable<EnterpriseCodeExternalCodeMappings.Mapping>
	{
		protected EnterpriseCodeExternalCodeMappings()
		{
			list = new Lazy<ImmutableArray<Mapping>>(() => GetMappings().ToImmutableArray());
		}

		public string GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetExternalCodeCore(enterpriseCode, errorContext, notifications);
		}

		public string GetEnterpriseCode(Enum externalCode, string errorContext, INotifications notifications)
		{
			return GetEnterpriseCode(externalCode == null ? null : externalCode.ToString(), errorContext, notifications);
		}

		public string GetEnterpriseCode(string externalCode, string errorContext, INotifications notifications)
		{
			string result = null;
			var messagePostfix = string.IsNullOrEmpty(errorContext) ? "" : ("; " + errorContext);
			foreach (var mapping in this)
			{
				if (mapping.ExternalCode == externalCode)
				{
					result = mapping.EnterpriseCode;
					break;
				}
			}

			if (!string.IsNullOrEmpty(externalCode) && result == null && notifications != null)
			{
				notifications.Notify(new WarningNotification(WarningType.Warning, Name + " '" + externalCode + "'" + messagePostfix));
			}

			return result;
		}

		public bool ContainsEnterpriseCode(string enterpriseCode)
		{
			return ContainsEnterpriseCode(this, enterpriseCode);
		}

		public IEnumerator<Mapping> GetEnumerator()
		{
			return ((IEnumerable<Mapping>)list.Value).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)list.Value).GetEnumerator();
		}

		#region Implementation

		public Mapping this[int i]
		{
			get { return list.Value[i]; }
		}

		protected T GetEnumExternalCode<T>(string enterpriseCode, T defaultValue, string errorContext, INotifications notifications)
			where T : struct
		{
			var externalCode = GetExternalCodeImplementation(enterpriseCode, errorContext, notifications);
			T result = defaultValue;
			return Enum.TryParse(externalCode, out result) ? result : defaultValue;
		}

		protected virtual string GetExternalCodeCore(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetExternalCodeImplementation(enterpriseCode, errorContext, notifications);
		}

		protected abstract string Name { get; }

		protected abstract IEnumerable<Mapping> GetMappings();

		protected static string GetExternalCode(IEnumerable<Mapping> mappings, string enterpriseCode)
		{
			var mapping = string.IsNullOrEmpty(enterpriseCode)
				? null
				: mappings.FirstOrDefault(m => m.EnterpriseCode == enterpriseCode);
			return mapping != null ? mapping.ExternalCode : null;
		}

		protected static bool ContainsEnterpriseCode(IEnumerable<Mapping> mappings, string enterpriseCode)
		{
			return GetExternalCode(mappings, enterpriseCode) != null;
		}

		string GetExternalCodeImplementation(string enterpriseCode, string errorContext, INotifications notifications)
		{
			var result = GetExternalCode(this, enterpriseCode);
			if (result == null && !string.IsNullOrEmpty(enterpriseCode) && notifications != null)
			{
				var messagePostfix = string.IsNullOrEmpty(errorContext) ? "" : ("; " + errorContext);
				notifications.Notify(new WarningNotification(WarningType.Warning, Res.GetString("d388a506-34b9-4f71-9b31-65bd089feb10", "Unsupported {0} '{1}'{2}", Name, enterpriseCode, messagePostfix)));
			}
			return result;
		}

		[Immutable]
		public class Mapping
		{
			public Mapping(string enterpriseCode, string externalCode)
			{
				EnterpriseCode = enterpriseCode;
				ExternalCode = externalCode;
			}

			public readonly string EnterpriseCode;
			public readonly string ExternalCode;
		}

		readonly Lazy<ImmutableArray<Mapping>> list;

		#endregion
	}
}
