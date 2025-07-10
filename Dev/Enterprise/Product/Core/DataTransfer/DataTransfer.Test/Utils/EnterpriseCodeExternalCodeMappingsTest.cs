using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestsSubclassesOf(typeof(EnterpriseCodeExternalCodeMappings))]
	public abstract class EnterpriseCodeExternalCodeMappingsTest : TransactionedTestCase
	{
		public virtual void TestNoNewCodesAdded()
		{
			EnterpriseCodeExternalCodeMappings mappings = GetNewMappings();
			var types = new List<Type>();
			types.AddRange(EnterpriseCodeDefinitionsClassesToCheckAgainst);
			foreach (Type type in EnterpriseCodeDefinitionsClassesToCheckAgainst)
			{
				Type codeType = type.GetNestedType("Codes");
				if (codeType != null)
				{
					types.Add(codeType);
				}
			}
			bool triedAtLeastOneComparison = false;
			foreach (Type type in types)
			{
				List<MemberInfo> members = new List<MemberInfo>();
				members.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Static));
				members.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Static));

				foreach (MemberInfo member in members)
				{
					if ((member.MemberType == MemberTypes.Field &&
						(((FieldInfo)member).FieldType == typeof(string) || typeof(Enum).IsAssignableFrom(((FieldInfo)member).FieldType)))
						||
						(member.MemberType == MemberTypes.Property &&
						(((PropertyInfo)member).PropertyType == typeof(string) || typeof(Enum).IsAssignableFrom(((PropertyInfo)member).PropertyType))))
					{
						triedAtLeastOneComparison = true;
						string memberValue = member.MemberType == MemberTypes.Field ? ((FieldInfo)member).GetValue(null).ToString() : ((PropertyInfo)member).GetValue(null, null).ToString();
						if (!((IList)GetEnterpriseCodesToExcludeCheckingAgainst()).Contains(memberValue))
						{
							var message =
$@"Missing mapping for enterprise code [{memberValue}] from list Type [{type.FullName}].
If you recently added a code to this list, in order to properly map it to the Legacy XML schema, you must:
	• Add the same value to the relevant element within .\Enterprise\Product\Core\DataTransfer\DataTransfer\DataFileDefinitions\Xml\Version1\Elements.xsd
	• Right-click on the XSD file and 'Run Custom Tool'
	• Add the mapping to the class {MappingsType.FullName}.

NOTE: this mapping is only required if the relevant business object or module supports Legacy XML. It's probably OK to just add the mapping as Legacy XML is mostly replaced with Native XML these days.";
							Assert(message, mappings.ContainsEnterpriseCode(memberValue));
						}
					}
				}
			}

			Assert("No fields were found to test - the test must find some information to be considered valid.", triedAtLeastOneComparison);
		}

		public void TestAllCodesUnique()
		{
			EnterpriseCodeExternalCodeMappings mappings = GetNewMappings();

			Hashtable enterpriseCodesSoFar = new Hashtable();
			Hashtable externalCodesSoFar = new Hashtable();
			foreach (EnterpriseCodeExternalCodeMappings.Mapping mapping in mappings)
			{
				if (enterpriseCodesSoFar.Contains(mapping.EnterpriseCode))
				{
					Fail("Enterprise code " + mapping.EnterpriseCode + " defined more than once");
				}
				if (externalCodesSoFar.Contains(mapping.ExternalCode))
				{
					Fail("External code " + mapping.ExternalCode + " defined more than once");
				}

				enterpriseCodesSoFar[mapping.EnterpriseCode] = null;
				externalCodesSoFar[mapping.ExternalCode] = null;
			}
			Assert(true);
		}

		public void TestEnterpriseAndExternalCodeShouldBeSameIfRequired()
		{
			if (EnterpriseAndExternalCodeShouldBeSame)
			{
				foreach (EnterpriseCodeExternalCodeMappings.Mapping mapping in GetNewMappings())
				{
					if (!ShouldExcludeMapping(mapping))
					{
						string externalCode = mapping.ExternalCode;
						if (externalCode.StartsWith("Item") && char.IsNumber(externalCode[4]))
						{
							externalCode = externalCode.Substring(4);
						}
						AssertEquals(
							"All enterprise codes should be the same as their external codes. " +
							"The xsd schema is defined this way, and if the enterprise codes change thought must be given to how it " +
							"affects the xsd schema.",
							mapping.EnterpriseCode, externalCode);
					}
				}
			}
			Assert(true);
		}

		protected Type MappingsType => TestedTypeHelper.GetTestedType(GetType());

		protected virtual string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return Array.Empty<string>();
		}

		protected virtual bool ShouldExcludeMapping(EnterpriseCodeExternalCodeMappings.Mapping mapping)
		{
			return false;
		}

		protected virtual Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return Array.Empty<Type>(); }
		}

		protected virtual bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		protected virtual EnterpriseCodeExternalCodeMappings GetNewMappings()
		{
			EnterpriseCodeExternalCodeMappings result = null;
			if (MappingsType.GetConstructor(Array.Empty<Type>()) != null)
			{
				result = (EnterpriseCodeExternalCodeMappings)Activator.CreateInstance(MappingsType);
			}
			else if (MappingsType.GetConstructor(new[] { typeof(BusinessObjectFactory) }) != null)
			{
				result = (EnterpriseCodeExternalCodeMappings)Activator.CreateInstance(MappingsType, new BusinessObjectFactory());
			}
			else
			{
				FieldInfo staticInstanceField = MappingsType.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
				PropertyInfo staticInstanceProperty = MappingsType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
				if (staticInstanceField != null)
				{
					result = (EnterpriseCodeExternalCodeMappings)staticInstanceField.GetValue(null);
				}
				if (staticInstanceProperty != null)
				{
					result = (EnterpriseCodeExternalCodeMappings)staticInstanceProperty.GetValue(null, null);
				}
			}
			return result;
		}
	}
}
