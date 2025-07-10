using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	[TestedType(typeof(HostedServiceBusinessObjectBindingAttribute))]
	sealed class HostedServiceBusinessObjectBindingAttributeTests : AssemblyMetaDataAttributeTestCase<HostedServiceBusinessObjectBindingAttribute>
	{
		sealed class HostedServiceBusinessObjectBindingAttribute_AsHostedBusinessObjectBindingTest : TestCase
		{
			public void TestTable()
			{
				AssertEquals("TestTable", hostedServiceBusinessObjectBinding.Table);
			}

			public void TestQueueName()
			{
				AssertEquals("Queue1", hostedServiceBusinessObjectBinding.QueueName);
			}

			public void TestServiceTaskCode()
			{
				AssertEquals("SVC", hostedServiceBusinessObjectBinding.ServiceTaskCode);
			}

			public void TestPredicates()
			{
				var predicates = hostedServiceBusinessObjectBinding.Predicates;
				AssertNotNull(predicates);
				AssertContainsExactElementsInAnyOrder(new[] { "A=B", "A <> Q", "C=ABC" }, predicates);
			}

			protected override void SetUp()
			{
				base.SetUp();
				hostedServiceBusinessObjectBinding = new HostedServiceBusinessObjectBindingAttribute("SVC",
					"TestTable",
					new[] { "A=B", "A <> Q", "C=ABC" },
					"Queue1");
			}

			//Remove the = null! after upgrading from NUnitCore to NUnit4, as CS8618 gets suppressed by NUnit3002 https://docs.nunit.org/articles/nunit-analyzers/NUnit3002.html
			IHostedServiceBusinessObjectBinding hostedServiceBusinessObjectBinding = null!;
		}

		sealed class HostedServiceBusinessObjectBindingAttributeTest : TestCase
		{
			public void TestTable()
			{
				AssertEquals("TestTable", hostedServiceBusinessObjectBindingAttribute.Table);
			}

			public void TestQueueName()
			{
				AssertEquals("Queue1", hostedServiceBusinessObjectBindingAttribute.QueueName);
			}

			public void TestServiceTaskCode()
			{
				AssertEquals("SVC", hostedServiceBusinessObjectBindingAttribute.ServiceTaskCode);
			}

			public void TestPredicates()
			{
				var predicates = hostedServiceBusinessObjectBindingAttribute.Predicates;
				AssertNotNull(predicates);
				AssertContainsExactElementsInAnyOrder(new[] { "A=B", "A <> Q", "C=ABC" }, predicates);
			}

			protected override void SetUp()
			{
				base.SetUp();
				hostedServiceBusinessObjectBindingAttribute = new HostedServiceBusinessObjectBindingAttribute("SVC",
					"TestTable",
					new[] { "A=B", "A <> Q", "C=ABC" },
					"Queue1");
			}

			//Remove the = null! after upgrading from NUnitCore to NUnit4, as CS8618 gets suppressed by NUnit3002 https://docs.nunit.org/articles/nunit-analyzers/NUnit3002.html
			HostedServiceBusinessObjectBindingAttribute hostedServiceBusinessObjectBindingAttribute = null!;
		}

		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.ServiceTaskCode = "ServiceTaskCode";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ServiceTaskCode = "ServiceTaskCode";
			Assert(attribute1.Equals(attribute2));

			attribute1.Table = "Table";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Table = "Table";
			Assert(attribute1.Equals(attribute2));

			attribute1.Predicates = ["Predicates"];
			Assert(!attribute1.Equals(attribute2));

			attribute2.Predicates = ["Predicates"];
			Assert(attribute1.Equals(attribute2));

			attribute1.QueueName = "QueueName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.QueueName = "QueueName";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
