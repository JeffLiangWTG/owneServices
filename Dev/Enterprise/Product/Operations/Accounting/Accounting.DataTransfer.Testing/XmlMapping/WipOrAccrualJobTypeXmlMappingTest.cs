using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.XmlMapping.Testing
{
	[TestedType(typeof(WipOrAccrualJobTypeXmlMapping))]
	public class WipOrAccrualJobTypeXmlMappingTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(JobInvoicingConsumerTypes) }; }
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
					var fieldValueObj = field.GetValue(null);

					if (fieldValueObj is string)
					{
						continue;
					}

					string fieldValue = ((JobInvoicingConsumerType)fieldValueObj).Code;
					if (!((IList)GetEnterpriseCodesToExcludeCheckingAgainst()).Contains(fieldValue))
					{
						string message =
							"Missing mapping for enterprise code " + fieldValue +
							@". If you recently added a code to this list, you must manually add it to ~Dev\Enterprise\Product\Core\DataTransfer\DataTransfer\DataFileDefinitions\Xml\Version1\FinancialWipOrAccrual.xsd, then " +
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
			WipOrAccrualJobTypeXmlMappingTestClass mappings = new WipOrAccrualJobTypeXmlMappingTestClass();
			AssertEquals("Name", "WIP or Accrual Job Type", mappings.Name);
		}

		class WipOrAccrualJobTypeXmlMappingTestClass : WipOrAccrualJobTypeXmlMapping
		{
			public new string Name
			{
				get { return base.Name; }
			}
		}
	}
}
