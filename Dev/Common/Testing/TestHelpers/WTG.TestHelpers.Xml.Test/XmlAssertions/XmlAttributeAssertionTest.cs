using System;
using System.Xml.Linq;
using NUnit.Framework;
using static WTG.TestHelpers.Xml.Test.XmlConstants;

namespace WTG.TestHelpers.Xml.Test
{
	public class XmlAttributeAssertionTest : TestCase
	{
		void TestAssertionFailedWith(XAttribute attribute, string message, string expectedAssertionFailedMessage, Action<IXmlAttributeAssertion> test)
		{
			var errorMessageSuffix = $"{Environment.NewLine}Actual Value: {attribute}";

			var xmlAssertion = new XmlAttributeAssertion(attribute);

			AssertExceptionThrown<AssertionFailedError>(message, Html($"{expectedAssertionFailedMessage}{errorMessageSuffix}"), () => test(xmlAssertion));

			var xmlAssertionWithDelayedValidation = new XmlAttributeAssertion(attribute, withDelayedChecks: true);

			test(xmlAssertionWithDelayedValidation);

			var result = xmlAssertionWithDelayedValidation.Check();

			Assert(message, !result.IsSuccessful);
			AssertEquals(message, $"{expectedAssertionFailedMessage}{errorMessageSuffix}", result.ValidationMessage.GetMessage());
		}

		public void TestWithNameExactMatch()
		{
			var attribute = new XAttribute("A", "a");

			AssertNoExceptionThrown(() => new XmlAttributeAssertion(attribute).WithName("A"));

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute name",
				$"damn{Environment.NewLine}Name 'A' of attribute is not equal to 'B'.",
				attr => attr.WithName("B", failedMessage: "damn")
				);
		}

		public void TestWithNameLambdaMatch()
		{
			var attribute = new XAttribute("A", "a");

			AssertNoExceptionThrown(() => new XmlAttributeAssertion(attribute).WithName(n => n.Length == 1));

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with unmatched attribute name",
				$"damn{Environment.NewLine}Name 'A' of attribute does not match the given criteria.",
				attr => attr.WithName(n => n.Length == 2, failedMessage: "damn")
				);
		}

		public void TestWithNamespaceExactMatch()
		{
			AssertNoExceptionThrown(() => new XmlAttributeAssertion(new XAttribute(XName.Get("A", TestSchema), "a")).WithNamespace(TestSchema));

			var attribute = new XAttribute("A", "a");

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute namespace",
				$"damn{Environment.NewLine}Namespace '' of attribute is not equal to '{TestSchema}'.",
				attr => attr.WithNamespace(TestSchema, failedMessage: "damn")
				);
		}

		public void TestWithNamespaceLambdaMatch()
		{
			AssertNoExceptionThrown(() => new XmlAttributeAssertion(new XAttribute(XName.Get("A", TestSchema), "a")).WithNamespace(n => n.Contains("cw1")));

			var attribute = new XAttribute("A", "a");

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with unmatched attribute namespace",
				$"damn{Environment.NewLine}Namespace '' of attribute does not match the given criteria.",
				attr => attr.WithNamespace(n => !string.IsNullOrEmpty(n), failedMessage: "damn")
				);
		}

		public void TestWithNamespaceAndName()
		{
			var attributeWithSchema = new XAttribute(XName.Get("A", TestSchema), "a");

			AssertNoExceptionThrown(() => new XmlAttributeAssertion(attributeWithSchema).WithNamespaceAndName(TestSchema, "A"));

			var attribute = new XAttribute("A", "a");

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute namespace and name",
				$"damn{Environment.NewLine}Attribute does not have namespace '{TestSchema}' and name 'A'.",
				attr => attr.WithNamespaceAndName(TestSchema, "A", failedMessage: "damn")
				);

			TestAssertionFailedWith(
				attributeWithSchema,
				"Should fail assertion with wrong attribute namespace and name",
				$"Attribute does not have namespace '{TestSchema}' and name 'B'.",
				attr => attr.WithNamespaceAndName(TestSchema, "B")
				);
		}

		public void TestWithXName()
		{
			var attribute = new XAttribute("A", "a");

			AssertNoExceptionThrown(() => new XmlAttributeAssertion(new XAttribute(XName.Get("A", TestSchema), "a")).WithXName(n => n.NamespaceName == TestSchema && n.LocalName == "A"));
			AssertNoExceptionThrown(() => new XmlAttributeAssertion(attribute).WithXName(n => string.IsNullOrEmpty(n.NamespaceName) && n.LocalName == "A"));

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with unmatched attribute xname",
				$"damn{Environment.NewLine}XName 'A' of attribute does not match the given criteria.",
				attr => attr.WithXName(n => !string.IsNullOrEmpty(n.NamespaceName), failedMessage: "damn")
				);
		}

		public void TestWithValueExactMatch()
		{
			var attribute = new XAttribute("A", "a");

			AssertNoExceptionThrown(() => new XmlAttributeAssertion(attribute).WithValue("a"));

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute value",
				$"damn{Environment.NewLine}Value 'a' of attribute is not equal to 'b'.",
				attr => attr.WithValue("b", failedMessage: "damn")
				);
		}

		public void TestWithValueLambdaMatch()
		{
			var attribute = new XAttribute("A", "a");

			AssertNoExceptionThrown(() => new XmlAttributeAssertion(attribute).WithValue(v => v.Length == 1 && v.StartsWith("a")));

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with unmatched attribute value",
				$"damn{Environment.NewLine}Value 'a' of attribute does not match the given criteria.",
				attr => attr.WithValue(v => v != "a", failedMessage: "damn")
				);
		}

		public void TestChaining()
		{
			var attribute = new XAttribute(XName.Get("A", TestSchema), "a");
			AssertNoExceptionThrown(
				() => new XmlAttributeAssertion(attribute)
						.WithName("A")
						.WithNamespace(TestSchema)
						.WithValue("a")
					);

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute name",
				"Name 'A' of attribute is not equal to 'INVALID'.",
				attr => attr
						.WithName("INVALID")
						.WithNamespace(TestSchema)
						.WithValue("a")
				);

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute namesapce",
				$"Namespace '{TestSchema}' of attribute is not equal to 'INVALID'.",
				attr => attr
						.WithName("A")
						.WithNamespace("INVALID")
						.WithValue("a")
				);

			TestAssertionFailedWith(
				attribute,
				"Should fail assertion with wrong attribute value",
				"Value 'a' of attribute is not equal to 'INVALID'.",
				attr => attr
						.WithName("A")
						.WithNamespace(TestSchema)
						.WithValue("INVALID")
				);
		}
	}
}
