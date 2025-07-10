using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADAttributeList
	{
		#region Singleton Pattern

		public static ADAttributeList Instance
		{
			get { return instance ?? (instance = new ADAttributeList()); }
		}

		[ThreadStatic]
		static ADAttributeList instance;

		ADAttributeList()
		{
		}

		#endregion

		public CodeDescriptionPairList UserAttributes
			=> (userAttributes == null || !userAttributes.Cast<CodeDescriptionPair>().Any()) ? (userAttributes = GetAttributesPairList(GetAllAttributesFromDomain((NoResString)"User"))) : userAttributes;
		CodeDescriptionPairList userAttributes;

		public CodeDescriptionPairList GroupAttributes
			=> (groupAttributes == null || !groupAttributes.Cast<CodeDescriptionPair>().Any()) ? (groupAttributes = GetAttributesPairList(GetAllAttributesFromDomain((NoResString)"Group"))) : groupAttributes;
		CodeDescriptionPairList groupAttributes;

		public CodeDescriptionPairList PredefinedAttributes => predefinedAttributes ?? (predefinedAttributes = GetAttributesPairList(GetPredefinedAttributes()));
		CodeDescriptionPairList predefinedAttributes;

		CodeDescriptionPairList GetAttributesPairList(IEnumerable<string> attributes)
		{
			var result = new CodeDescriptionPairList();
			foreach (var attribute in attributes)
			{
				result.Add(new CodeDescriptionPair(attribute, string.Empty));// we only use the code fields
			}
			return result;
		}

		IEnumerable<string> GetAllAttributesFromDomain(string classType)
		{
			try
			{
				var domainCredentials = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials;
				var searcher = DirectorySearcherFactory.GetDirectorySearcher(domainCredentials);
				return Sanitise(searcher.GetAllAttributes(classType));
			}
			catch (UnableToConnectToDomainException)
			{
				return Enumerable.Empty<string>();
			}
		}

		public IEnumerable<string> GetPredefinedAttributes()
		{
			var attributeFields = typeof(ADAttributes).GetFields()
					.Where(f => f.IsLiteral && !ExcludedAttributes.Contains(f.Name, StringComparer.OrdinalIgnoreCase))
					.OrderBy(f => f.Name);

			foreach (var attributeField in attributeFields)
			{
				yield return (string)attributeField.GetValue(null);
			}
		}

		readonly string[] ExcludedAttributes =
		{
			(NoResString)"cn",
			"distinguishedName",
			"objectGUID",
			"sAMAccountName",
			"whenChanged",
			"whenCreated"
		};

		IEnumerable<string> Sanitise(IEnumerable<string> attributes) => attributes.Where(a => !ExcludedAttributes.Contains(a, StringComparer.OrdinalIgnoreCase)).OrderBy(a => a);
	}
}
