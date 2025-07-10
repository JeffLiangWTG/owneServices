using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;
using static WTG.TestHelpers.Xml.Test.XmlConstants;

namespace WTG.TestHelpers.Xml.Test
{
	public class XmlAssertionsTest : TestCase
	{
		public void TestJustAnXml()
		{
			AssertIsXml(ValidComplexXml);
		}

		public void TestAssertingTheWholeContentOfComplexXml()
		{
			AssertIsXml(ValidComplexXmlWithNamespaces)
				.WithName("A")
				.WithName(name => name.Length == 1 && name != "B")
				.WithNamespace(TestSchema)
				.WithNamespace(ns => ns != Test2Schema)
				.WithXName(xn => xn.NamespaceName == TestSchema && xn.LocalName == "A")
				.WithAttribute("Z", "z")
				.WithAttribute(a => a.WithName(n => n.Length == 1 && n.Contains("Z")))
				.HavingExactlyOneDescendantNode(node =>
					node.WithName("B")
						.WithNamespace(Test2Schema)
						.WithValue("TEST")
						.WithAttribute(a =>
							a.WithName("X")
							 .WithValue("1")
						)
				)
				.HavingExactlyOneChildNode(node =>
					node.WithName("CS")
						.HavingAllChildNodes(child =>
							child.WithName("C")
								 .WithAttribute(a =>
									a.WithName("Y")
									 .WithValue(v => v == "1" || v == "2" || v == "3")
								 )
						)
				)
				.HavingAtLeastOneChildNode(node =>
					node.WithName("B")
				)
				.HavingAtLeastOneDescendantNode(desc =>
					desc.WithName("C")
						 .WithAttribute(a =>
							a.WithName("Y")
						)
				);

			AssertIsXml(ValidComplexXmlWithNamespaces)
				.HavingExactlyOneChildNode("CS/C",
					node => node.WithAttribute(a =>
						a.WithName("Y")
						 .WithValue(v => v == "1")
					)
				)
				.HavingAtLeastOneChildNode("CS/C",
					node => node.WithAttribute(a =>
						a.WithName("Y")
						 .WithValue(v => v == "1" || v == "2" || v == "3")
					)
				);
		}
	}
}
