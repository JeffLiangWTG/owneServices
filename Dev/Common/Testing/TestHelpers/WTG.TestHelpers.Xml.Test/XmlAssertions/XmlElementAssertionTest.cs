using System;
using System.Xml.Linq;
using NUnit.Framework;
using static WTG.TestHelpers.Xml.Test.XmlConstants;

namespace WTG.TestHelpers.Xml.Test
{
	public class XmlElementAssertionTest : TestCase
	{
		void TestAssertionFailedWith(string xml, string message, string expectedAssertionFailedMessage, Action<IXmlAssertion> test)
		{
			var element = XElement.Parse(xml);
			var errorMessageSuffix = $"{Environment.NewLine}Actual Value:{Environment.NewLine}{element}";

			var xmlAssertionsWithoutCustomMessage = new XmlElementAssertion(element);
			var xmlAssertionsWithCustomMessage = new XmlElementAssertion(element, message: "CUSTOM MESSAGE");

			AssertExceptionThrown<AssertionFailedError>(message, Html($"{expectedAssertionFailedMessage}{errorMessageSuffix}"), () => test(xmlAssertionsWithoutCustomMessage));
			AssertExceptionThrown<AssertionFailedError>(message, Html($"CUSTOM MESSAGE{Environment.NewLine}{expectedAssertionFailedMessage}{errorMessageSuffix}"), () => test(xmlAssertionsWithCustomMessage));

			var xmlAssertionWithDelayedValidation = new XmlElementAssertion(xml, withDelayedChecks: true);

			test(xmlAssertionWithDelayedValidation);

			var result = xmlAssertionWithDelayedValidation.Check();

			Assert(message, !result.IsSuccessful);
			AssertEquals(message, $"{expectedAssertionFailedMessage}{errorMessageSuffix}", result.ValidationMessage.GetMessage());
		}

		public void TestXmlValidationOnConstruction()
		{
			AssertExceptionThrown<AssertionFailedError>(
				"Should fail assertion on invalid XML",
				Html($"Invalid XML string.{Environment.NewLine}Actual Value:{Environment.NewLine}{InvalidXml}"),
				() => new XmlElementAssertion(InvalidXml));

			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidSimpleXml));
		}

		public void TestWithNameExactMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidSimpleXml).WithName("A"));

			TestAssertionFailedWith(
				ValidSimpleXml,
				"Should fail assertion with wrong element name",
				$"damn{Environment.NewLine}Name 'A' of element <A> is not equal to 'B'.",
				xml => xml.WithName("B", failedMessage: "damn")
				);
		}

		public void TestWithNameLambdaMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidSimpleXml).WithName(name => name != "B"));

			TestAssertionFailedWith(
				ValidSimpleXml,
				"Should fail assertion with wrong unmatched name",
				$"damn{Environment.NewLine}Name 'A' of element <A> does not match the given criteria.",
				xml => xml.WithName(name => name != "A", failedMessage: "damn")
				);
		}

		public void TestWithNamespaceExactMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithDefaultNamespace).WithNamespace(TestSchema));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithNonDefaultNamespace).WithNamespace(Test2Schema));

			TestAssertionFailedWith(
				ValidXmlWithDefaultNamespace,
				"Should fail assertion with wrong element namespace",
				$"Namespace '{TestSchema}' of element <{{{TestSchema}}}A> is not equal to 'http://schemas.cw1.com/invalid'.",
				xml => xml.WithNamespace("http://schemas.cw1.com/invalid")
				);

			TestAssertionFailedWith(
				ValidXmlWithNonDefaultNamespace,
				"Should fail assertion with wrong element namespace",
				$"damn{Environment.NewLine}Namespace '{Test2Schema}' of element <{{{Test2Schema}}}A> is not equal to 'http://schemas.cw1.com/invalid2'.",
				xml => xml.WithNamespace("http://schemas.cw1.com/invalid2", failedMessage: "damn")
				);
		}

		public void TestWithNamspaceLambdaMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithDefaultNamespace).WithNamespace(n => n.StartsWith("http")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithNonDefaultNamespace).WithNamespace(n => !n.StartsWith("https")));

			TestAssertionFailedWith(
				ValidXmlWithDefaultNamespace,
				"Should fail assertion with unmatched element namespace",
				$"Namespace '{TestSchema}' of element <{{{TestSchema}}}A> does not match the given criteria.",
				xml => xml.WithNamespace(n => n.StartsWith("https"))
				);

			TestAssertionFailedWith(
				ValidXmlWithNonDefaultNamespace,
				"Should fail assertion with unmatched element namespace",
				$"damn{Environment.NewLine}Namespace '{Test2Schema}' of element <{{{Test2Schema}}}A> does not match the given criteria.",
				xml => xml.WithNamespace(n => !n.StartsWith("http"), failedMessage: "damn")
				);
		}

		public void TestWithNamespaceAndName()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithDefaultNamespace).WithNamespaceAndName(TestSchema, "A"));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithNonDefaultNamespace).WithNamespaceAndName(Test2Schema, "A"));

			TestAssertionFailedWith(
				ValidXmlWithDefaultNamespace,
				"Should fail assertion with wrong element namespace and name",
				$"Element <{{{TestSchema}}}A> does not have namespace 'http://schemas.cw1.com/invalid' and name 'A'.",
				xml => xml.WithNamespaceAndName("http://schemas.cw1.com/invalid", "A")
				);

			TestAssertionFailedWith(
				ValidXmlWithNonDefaultNamespace,
				"Should fail assertion with wrong element namespace and name",
				$"damn{Environment.NewLine}Element <{{{Test2Schema}}}A> does not have namespace 'http://schemas.cw1.com/test2' and name 'B'.",
				xml => xml.WithNamespaceAndName("http://schemas.cw1.com/test2", "B", failedMessage: "damn")
				);
		}

		public void TestWithXName()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithDefaultNamespace).WithXName(xname => xname.NamespaceName == TestSchema && xname.LocalName == "A"));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidXmlWithNonDefaultNamespace).WithXName(xname => xname.NamespaceName == Test2Schema && xname.LocalName == "A"));

			TestAssertionFailedWith(
				ValidXmlWithDefaultNamespace,
				"Should fail assertion with unmatched element xname",
				$"damn{Environment.NewLine}XName '{{{TestSchema}}}A' of element <{{{TestSchema}}}A> does not match the given criteria.",
				xml => xml.WithXName(xname => xname.NamespaceName == "http://schemas.cw1.com/invalid", failedMessage: "damn")
				);
		}

		public void TestWithValueExactMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidSimpleXml).WithValue("TEST"));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element value",
				"Element <A> is expected to have inner text but has nested elements instead.",
				xml => xml.WithValue("TEST")
				);

			TestAssertionFailedWith(
				ValidSimpleXml,
				"Should fail assertion with wrong element value",
				$"damn{Environment.NewLine}Value 'TEST' of element <A> is not equal to 'INVALID'.",
				xml => xml.WithValue("INVALID", failedMessage: "damn")
				);
		}

		public void TestWithValueLambdaMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidSimpleXml).WithValue(v => v.Contains("TEST")));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong unmatched value",
				"Element <A> is expected to have inner text but has nested elements instead.",
				xml => xml.WithValue(v => v.StartsWith("TEST"))
				);

			TestAssertionFailedWith(
				ValidSimpleXml,
				"Should fail assertion with wrong unmatched value",
				$"damn{Environment.NewLine}Value 'TEST' of element <A> does not match the given criteria.",
				xml => xml.WithValue(v => !v.StartsWith("TEST"), failedMessage: "damn")
				);
		}

		public void TestWithAttributeExactMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).WithAttribute("Z", "z"));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong attribute name and value",
				"Element <A> does not have an attribute 'Z' with value '1'.",
				xml => xml.WithAttribute("Z", "1")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong attribute name and value",
				$"damn{Environment.NewLine}Element <A> does not have an attribute 'X' with value 'z'.",
				xml => xml.WithAttribute("X", "z", failedMessage: "damn")
				);
		}

		public void TestWithAttributeLambdaMatch()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).WithAttribute(a => a.WithName("Z").WithValue("z")));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched attribute",
				"Element <A> does not have an attribute matching the given criteria.",
				xml => xml.WithAttribute(a => a.WithName("Z").WithValue("1"))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched attribute",
				$"damn{Environment.NewLine}Element <A> does not have an attribute matching the given criteria.",
				xml => xml.WithAttribute(a => a.WithName("X").WithValue("z"), failedMessage: "damn")
				);
		}

		public void TestHavingAtLeastOneChildNode()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneChildNode(node => node.WithName("B")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneChildNode(node => node.WithName("CS")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneChildNode("CS/C"));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "1"))));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "1" || v == "2" || v == "3"))));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingAtLeastOneChildNode(node => node.WithName("C"))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"damn{Environment.NewLine}Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingAtLeastOneChildNode(node => node.WithName("INVALID"), failedMessage: "damn")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingAtLeastOneChildNode("C")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"damn{Environment.NewLine}Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingAtLeastOneChildNode("INVALID", failedMessage: "damn")
				); 

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingAtLeastOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "4")))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"damn{Environment.NewLine}Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingAtLeastOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "4")), failedMessage: "damn")
				);
		}

		public void TestHavingExactlyOneChildNode()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneChildNode(node => node.WithName("CS")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneChildNode(node => node.WithName("B").WithAttribute("X", "1")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneChildNode("CS"));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "1"))));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> has more than one child elements matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode(node => node.WithName("B"))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode(node => node.WithName("C"))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"damn{Environment.NewLine}Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode(node => node.WithName("INVALID"), failedMessage: "damn")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> has more than one child elements matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode("B")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode("C")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"damn{Environment.NewLine}Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode("INVALID", failedMessage: "damn")
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> has more than one child elements matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "1" || v == "2")))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "4")))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched child node",
				$"damn{Environment.NewLine}Element <A> does not have any child element matching the given criteria.",
				xml => xml.HavingExactlyOneChildNode("CS/C", node => node.WithAttribute(a => a.WithName("Y").WithValue(v => v == "4")), failedMessage: "damn")
				);
		}

		public void TestHavingAllChildNodes()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAllChildNodes(node => node.WithName(n => n == "B" || n == "CS")));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element name",
				$"damn{Environment.NewLine}Element <A> has child element <CS> not matching the given criteria.{Environment.NewLine}Actual child Element that failed validation:{Environment.NewLine}{XElement.Parse(ValidComplexXml).Element("CS")}",
				xml => xml.HavingAllChildNodes(node => node.WithName("B"), failedMessage: "damn")
				);
		}

		public void TestHavingAtLeastOneDescendantNode()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneDescendantNode(node => node.WithName("B")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneDescendantNode(node => node.WithName("CS")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute("Y", "2")));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element name",
				$"damn{Environment.NewLine}Element <A> does not have any descendant element matching the given criteria.",
				xml => xml.HavingAtLeastOneDescendantNode(node => node.WithName("INVALID"), failedMessage: "damn")
				);
		}

		public void TestHavingExactlyOneDescendantNode()
		{
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneDescendantNode(node => node.WithName("CS")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneDescendantNode(node => node.WithName("B").WithAttribute("X", "1")));
			AssertNoExceptionThrown(() => new XmlElementAssertion(ValidComplexXml).HavingExactlyOneDescendantNode(node => node.WithName("C").WithAttribute("Y", "2")));

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element name",
				"Element <A> has more than one descendant elements matching the given criteria.",
				xml => xml.HavingExactlyOneDescendantNode(node => node.WithName("B"))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element name",
				"Element <A> has more than one descendant elements matching the given criteria.",
				xml => xml.HavingExactlyOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("Y")))
				);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element name",
				$"damn{Environment.NewLine}Element <A> does not have any descendant element matching the given criteria.",
				xml => xml.HavingExactlyOneDescendantNode(node => node.WithName("INVALID"), failedMessage: "damn")
				);
		}

		public void TestChaining()
		{
			AssertNoExceptionThrown(
				() => new XmlElementAssertion(ValidComplexXml)
						.WithName("A")
						.WithAttribute(a => a.WithName("Z").WithValue("z"))
						.HavingAtLeastOneChildNode(node => node.WithName("B"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("Y")))
					);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with wrong element name",
				"Name 'A' of element <A> is not equal to 'INVALID'.",
				xml => xml
						.WithName("INVALID")
						.WithAttribute(a => a.WithName("Z").WithValue("z"))
						.HavingAtLeastOneChildNode(node => node.WithName("B"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("Y")))
					);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched element attribute",
				"Element <A> does not have an attribute matching the given criteria.",
				xml => xml
						.WithName("A")
						.WithAttribute(a => a.WithName("INVALID").WithValue("z"))
						.HavingAtLeastOneChildNode(node => node.WithName("B"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("Y")))
					);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched element attribute",
				"Element <A> does not have an attribute matching the given criteria.",
				xml => xml
						.WithName("A")
						.WithAttribute(a => a.WithName("Z").WithValue("INVALID"))
						.HavingAtLeastOneChildNode(node => node.WithName("B"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("Y")))
					);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched element child",
				"Element <A> does not have any child element matching the given criteria.",
				xml => xml
						.WithName("A")
						.WithAttribute(a => a.WithName("Z").WithValue("z"))
						.HavingAtLeastOneChildNode(node => node.WithName("INVALID"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("Y")))
					);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched element descendant",
				"Element <A> does not have any descendant element matching the given criteria.",
				xml => xml
						.WithName("A")
						.WithAttribute(a => a.WithName("Z").WithValue("z"))
						.HavingAtLeastOneChildNode(node => node.WithName("B"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("INVALID").WithAttribute(a => a.WithName("Y")))
					);

			TestAssertionFailedWith(
				ValidComplexXml,
				"Should fail assertion with unmatched element descendant",
				"Element <A> does not have any descendant element matching the given criteria.",
				xml => xml
						.WithName("A")
						.WithAttribute(a => a.WithName("Z").WithValue("z"))
						.HavingAtLeastOneChildNode(node => node.WithName("B"))
						.HavingAtLeastOneDescendantNode(node => node.WithName("C").WithAttribute(a => a.WithName("INVALID")))
					);
		}
	}
}
