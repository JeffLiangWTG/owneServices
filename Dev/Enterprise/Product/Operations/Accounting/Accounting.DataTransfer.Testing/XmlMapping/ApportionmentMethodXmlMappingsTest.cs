using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.XmlMapping.Testing
{
	[TestedType(typeof(ApportionmentMethodXmlMapping))]
	public class ApportionmentMethodXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(AllocationMethod) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		public override void TestNoNewCodesAdded()
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
				foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					triedAtLeastOneComparison = true;
					string fieldValue = field.GetValue(null).ToString();
					if (!((IList)GetEnterpriseCodesToExcludeCheckingAgainst()).Contains(fieldValue))
					{
						if (fieldValue == "MAN")
						{
							continue; //MAN is intentionally not added in FinancialInvoiceLine as an apportionmentMethod
						}

						string message =
							"Missing mapping for enterprise code " + fieldValue +
							@". If you recently added a code to this list, you must manually add it to .\DataTransfer\DataTransfer\DataFileDefinitions\Xml\Version1\FinancialInvoiceLine.xsd, then " +
							"Right-click on the XSD file and 'Run Custom Tool', and " +
							"add the mapping to the class " + MappingsType.FullName + ".";
						AssertEquals(message, true, mappings.ContainsEnterpriseCode(fieldValue));
					}
				}
			}
			if (triedAtLeastOneComparison)
			{
				Assert(true);
			}
			else
			{
				Fail("No fields were found to test - the test must find some information to be considered valid.");
			}
		}

		public void TestName()
		{
			ApportionmentMethodXmlMappingTestClass mappings = new ApportionmentMethodXmlMappingTestClass();
			AssertEquals("Name", "Apportionment Method", mappings.Name);
		}

		class ApportionmentMethodXmlMappingTestClass : ApportionmentMethodXmlMapping
		{
			public new string Name
			{
				get { return base.Name; }
			}
		}
	}
}
