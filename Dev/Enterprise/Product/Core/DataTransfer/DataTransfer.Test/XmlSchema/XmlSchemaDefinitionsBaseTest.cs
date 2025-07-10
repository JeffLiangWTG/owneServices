using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestsSubclassesOf(typeof(XmlSchemaDefinitionsBase))]
	public abstract class XmlSchemaDefinitionsBaseTest : TestCase
	{
		public void TestStaticInstancePropertyExistsAndCachedByWeakReference()
		{
			WeakReference instanceRef = TestStaticInstancePropertyExistsAndCachedByWeakReference_ReturnWeakReferenceToSchemaDefinitions();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertEquals(
				"Value returned from Instance should be weak referenced to allow it to collect to same memory. If this test fails, it could be an indication that the schema definitions is being cached statically somewhere (even possibly outside of this class)",
				false, instanceRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference TestStaticInstancePropertyExistsAndCachedByWeakReference_ReturnWeakReferenceToSchemaDefinitions()
		{
			Type schemaDefinitionsType = GetXmlSchemaDefinitions().GetType();
			PropertyInfo staticInstanceProperty = schemaDefinitionsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
			AssertNotNull("Static Instance property should exist so the instance can be cached", staticInstanceProperty);
			AssertEquals("Static Instance property should have the correct return type", schemaDefinitionsType, staticInstanceProperty.PropertyType);

			XmlSchemaDefinitionsBase firstInstance = (XmlSchemaDefinitionsBase)staticInstanceProperty.GetValue(null, null);
			XmlSchemaDefinitionsBase secondInstance = (XmlSchemaDefinitionsBase)staticInstanceProperty.GetValue(null, null);
			AssertEquals("Value returned from Instance should be cached", true, firstInstance == secondInstance);

			return new WeakReference(firstInstance);
		}

		public void TestAllSchemasValid_AndDoesntLeakMemory()
		{
			XmlSchemaDefinitionsBase schemas = GetXmlSchemaDefinitions();
			DoTestAllSchemasValid(schemas);
			DoTestAllSchemasValid(schemas);
			int previousSchemaCacheCount = schemas.SchemaCacheCount;
			DoTestAllSchemasValid(schemas);
			AssertEquals("Shouldnt create more cache entries than is required", previousSchemaCacheCount, schemas.SchemaCacheCount);
		}

		public void TestGetAllXsdResourceNames()
		{
			string[] resourceNames = GetXmlSchemaDefinitions().GetAllXsdResourceNames();
			AssertEquals("Should find at least 1", true, resourceNames.Length > 0);
		}

		#region Test enums are unique

		public void TestEnumNamesUnique()
		{
			foreach (XmlSchema schema in GetAllXmlSchemas())
			{
				TestEnumNamesUnique(schema);
			}
			Assert("if it didn't fail up until here, it's OK", true);
		}

		void TestEnumNamesUnique(XmlSchemaObject schemaObject)
		{
			if (schemaObject is XmlSchema)
			{
				XmlSchema schema = schemaObject as XmlSchema;

				for (int i = 0; i < schema.Items.Count; i++)
				{
					XmlSchemaObject obj = schema.Items[i];
					TestEnumNamesUnique(obj);
				}
			}
			else if (schemaObject is XmlSchemaSimpleType)
			{
				XmlSchemaSimpleType simpleType = schemaObject as XmlSchemaSimpleType;
				XmlSchemaSimpleTypeRestriction enumValues = simpleType.Content as XmlSchemaSimpleTypeRestriction;
				if (enumValues != null)
				{
					try
					{
						TestEnumNamesUnique(enumValues);
					}
					catch (DuplicateNameException e)
					{
						Assert(String.Format("value {0} is already in the enumeration {1}", e.Message, ((XmlSchemaType)schemaObject).QualifiedName.Name), false);
					}
				}
			}
		}

		void TestEnumNamesUnique(XmlSchemaSimpleTypeRestriction enumValues)
		{
			StringCollection values = new StringCollection();
			foreach (XmlSchemaObject enumValue in enumValues.Facets)
			{
				if (enumValue is XmlSchemaEnumerationFacet)
				{
					string val = ((XmlSchemaFacet)enumValue).Value;
					if (values.Contains(val))
					{
						throw new DuplicateNameException(val);
					}
					else
					{
						values.Add(val);
					}
				}
			}
		}

		#endregion

		#region TestDecimalValidationFractionDigitsFacetNotUsedInSchemas

		public virtual void TestDecimalValidationFractionDigitsFacetNotUsedInSchemas()
		{
			XmlSchemaDefinitionsBase definitions = GetXmlSchemaDefinitions();
			foreach (XmlSchema schema in definitions.AllSchemas)
			{
				TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(new XmlSchemaNavigator(schema), new List<string>());
			}
			Assert("No schema has fraction digits (expect when a complex type imherits from the simple type)", true);
		}

		void TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(XmlSchemaNavigator schemaNavigator, List<string> processedElementNames)
		{
			foreach (XmlSchemaObject schemaObject in schemaNavigator.ChildSchemaObjects)
			{
				XmlSchemaAttribute attribute = schemaObject as XmlSchemaAttribute;
				if (attribute != null)
				{
					TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(attribute.SchemaType);
				}

				XmlSchemaElement element = schemaObject as XmlSchemaElement;
				if (element != null && !processedElementNames.Contains(element.Name))
				{
					processedElementNames.Add(element.Name);
					TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(element.SchemaType);
					schemaNavigator.NavigateToElement(element.Name);
					TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(schemaNavigator, processedElementNames);
					schemaNavigator.NavigateBack();
				}
			}
		}

		void TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(XmlSchemaType type)
		{
			XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
			if (simpleType != null)
			{
				TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(simpleType);
			}
		}

		void TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(XmlSchemaSimpleType type)
		{
			XmlSchemaSimpleTypeRestriction restriction = type.Content as XmlSchemaSimpleTypeRestriction;
			if (restriction != null && restriction.BaseTypeName.Name == "xs:decimal")
			{
				foreach (XmlSchemaFacet facet in restriction.Facets)
				{
					if (facet is XmlSchemaFractionDigitsFacet)
					{
						StringWriter message = new StringWriter();
						message.WriteLine("You should not provide fractionDigits to an xs:decimal data type because they need to be rounded. Instead, consider doing the following:");
						message.WriteLine("- Define a simple type (for example MonetaryValueSimpleType) with the fractionDigits restriction");
						message.WriteLine("- Define a complex type (for example MonetaryValue) inheriting from the simple type");
						message.WriteLine("- Use the complex type wherever you want the fractionDigits to be restricted in the schema");
						message.WriteLine("- Sub-class the generated C# class and override ZDecimal Value, and round the value in the setter (and truncate the scale with ZDecimal.RoundAndTruncateScale)");
						message.WriteLine("Thie unit test will pass when the simple type is sub-classed by a complex type in this way.");
						Fail(message.GetStringBuilder().ToString());
					}
				}
			}
			if (type.BaseXmlSchemaType is XmlSchemaSimpleType)
			{
				TestDecimalValidationFractionDigitsFacetNotUsedInSchemas(type.BaseXmlSchemaType as XmlSchemaSimpleType);
			}
		}

		#endregion

		protected virtual string GetExpectedXmlNamespace()
		{
			return XmlSchemaDefinitions.EdiXmlNamespace;
		}

		protected abstract XmlSchemaDefinitionsBase GetXmlSchemaDefinitions();

		#region Implementation

		void DoTestAllSchemasValid(XmlSchemaDefinitionsBase schemas)
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(schemas))
			{
				if (typeof(XmlSchema).IsAssignableFrom(property.PropertyType))
				{
					ExpectXmlSchemaContainsRootElementAttribute attr = (ExpectXmlSchemaContainsRootElementAttribute)property.Attributes[typeof(ExpectXmlSchemaContainsRootElementAttribute)];
					if (attr == null)
					{
						Fail(
							"You should apply " + typeof(ExpectXmlSchemaContainsRootElementAttribute).FullName +
							" to property " + property.Name +
							" to specify a top-level element name that you expect to find. Put null to skip checking for the name of the top-level element.");
					}
					XmlSchema schema = (XmlSchema)property.GetValue(schemas);
					AssertNotNull("Should return a non-null schema for property " + property.Name, schema);
					AssertEquals("Schema should be compiled for property " + property.Name, true, schema.IsCompiled);
					if (!string.IsNullOrEmpty(attr.ExpectedElementName))
					{
						AssertEquals(
							"Expected to find element with name " + attr.ExpectedElementName +
							" and namespace " + GetExpectedXmlNamespace(),
							true, schema.Elements.Contains(new XmlQualifiedName(attr.ExpectedElementName, GetExpectedXmlNamespace())));
					}
				}
			}
		}

		XmlSchema[] GetAllXmlSchemas()
		{
			ArrayList result = new ArrayList();
			XmlSchemaDefinitionsBase schemas = GetXmlSchemaDefinitions();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(schemas))
			{
				if (typeof(XmlSchema).IsAssignableFrom(property.PropertyType))
				{
					result.Add((XmlSchema)property.GetValue(schemas));
				}
			}
			return (XmlSchema[])result.ToArray(typeof(XmlSchema));
		}

		#endregion
	}
}
