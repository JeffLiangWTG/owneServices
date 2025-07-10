using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	class HostedServiceAttributeProviderTest : TestCase
	{
		public void TestGetClientHostedServiceAttributes_WithDuplicateAttributes_ThrowsException()
		{
			// Arrange
			var metaDataReader = new Mock<IAssemblyMetaDataReader>();
			metaDataReader
				.Setup(o => o.GetAttributes<HostedServiceAttribute>(true))
				.Returns(new []
					{
						new HostedServiceAttribute("Code", "Description", "Category", typeof(HostedServiceAttribute)),
						new HostedServiceAttribute("Code", "Description", "Category", typeof(HostedServiceAttribute)),
					}
				);

			var attributeProvider = new HostedServiceAttributeProvider();

			using (ObjectFactory.Substitute(metaDataReader.Object))
			{
				// Act, Assert
				AssertExceptionThrown<ArgumentException>(() => attributeProvider.GetClientHostedServiceAttributes());
			}
		}

		public void TestGetAllServiceAttributes_ReturnsAllAttributes()
		{
			// Arrange
			var expectedServiceAttributes = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>(true);

			// Act
			var serviceAttributes = new HostedServiceAttributeProvider().GetHostedServiceAttributes();

			// Assert
			Assert(expectedServiceAttributes.All(o => serviceAttributes.Any(x => o.Code == x.Code && o.Type == x.Type)));
		}

		public void TestGetHostedServiceAttribute_WithInvalidAssemblyName_ReturnsNull()
		{
			// Arrange
			var attribute = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>().First();

			// Act
			var serviceAttribute = new HostedServiceAttributeProvider().GetHostedServiceAttribute("InvalidAssembly", attribute.Code);

			// Assert
			AssertNull(serviceAttribute);
		}

		public void TestGetHostedServiceAttribute_WithInvalidCode_ReturnsNull()
		{
			// Arrange
			var attribute = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>().First();

			// Act
			var serviceAttribute = new HostedServiceAttributeProvider().GetHostedServiceAttribute(attribute.TypeAssemblyName, "InvalidCode");

			// Assert
			AssertNull(serviceAttribute);
		}

		public void TestGetHostedServiceAttribute_WithAssemblyMetaData_GetsCorrectAttribute()
		{
			var attributes = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>(true);

			foreach (var attribute in attributes)
			{
				Test(attribute.TypeAssemblyName, attribute.Code);
			}

			void Test(string assemblyName, string code)
			{
				// Arrange, Act
				var serviceAttribute = new HostedServiceAttributeProvider().GetHostedServiceAttribute(assemblyName, code);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(serviceAttribute.TypeAssemblyName, assemblyName);
					AssertEquals(serviceAttribute.Code, code);
				});
			}
		}

		public void TestGetServiceProvidersReturnsClientSpecific()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var hostedServiceTasks = new HostedServiceAttributeProvider().GetClientHostedServiceAttributes();
				Assert("Must contain at least one CSP task", hostedServiceTasks.Any(x => x.Category == "CSP"));
			}
		}

		public void TestGetServiceProviders()
		{
			var hostedServiceTasks = new HostedServiceAttributeProvider().GetClientHostedServiceAttributes();
			var serviceConfigs = Array.ConvertAll(hostedServiceTasks.ToArray(),
				serviceConfig => new string[]
				{
					serviceConfig.Code,
					serviceConfig.Description,
					serviceConfig.TypeAssemblyName,
					ClientHookLoader.Instance.GetClientCode(serviceConfig.TypeAssemblyName)
				});

			var batchProcessors = new List<string>();

			for (var i = 0; i < serviceConfigs.Length; i++)
			{
				var clientCode1 = serviceConfigs[i][3];

				CombineAssertions("Found duplicate codes/descriptions for service tasks", () =>
				{
					for (var j = i + 1; j < serviceConfigs.Length; j++)
					{
						var clientCode2 = serviceConfigs[j][3];
						if (clientCode1 != null && clientCode2 != null && clientCode1 != clientCode2)
						{
							// don't compare different ZClient
							continue;
						}

						var message1 = string.Format("'{0}' code is used by more than one service task ('{1}' and '{2}').", serviceConfigs[i][0], serviceConfigs[i][1], serviceConfigs[j][1]);
						Assert(message1, !serviceConfigs[i][0].Equals(serviceConfigs[j][0], StringComparison.CurrentCultureIgnoreCase));

						var message2 = string.Format("'{0}' description is used by more than one service task ('{1}' and '{2}').", serviceConfigs[i][1], serviceConfigs[i][0], serviceConfigs[j][0]);
						Assert(message2, !serviceConfigs[i][1].Equals(serviceConfigs[j][1], StringComparison.CurrentCultureIgnoreCase));
					}
				});
				var message3 = string.Format("'{0}' code ('{1}') is used by batch processor.", serviceConfigs[i][0], serviceConfigs[i][1]);
				Assert(message3, !batchProcessors.Contains(serviceConfigs[i][0]));
			}
		}

		public void TestMutuallyExclusiveServiceTaskGroupsHaveAtLeastTwoTasksPerCategory()
		{
			var services = new HostedServiceAttributeProvider()
				.GetClientHostedServiceAttributes()
				.GroupBy(config => config.MutuallyExclusiveTaskGroup, config => config.Code)
				.Select(grouping => (key: grouping.Key, count: grouping.Count()))
				.Where(tuple => tuple.count < 2)
				.Select(tuple => $"{tuple.key}: {tuple.count}");

			AssertContainsExactLinesInAnyOrder("There is no need to be mutually exclusive if you're the only candidate.", string.Empty, string.Join("\r\n", services));
		}

		public void TestGetHostedServicesReturnsSameListAsGetServiceAttributesWithFilterClientAssemblies()
		{
			// Arrange
			var hostedServiceAttributeProvider = new HostedServiceAttributeProvider();
			var expected = hostedServiceAttributeProvider
				.GetClientHostedServiceAttributes()
				.Select(o => (o.Code, o.Description))
				.ToArray();

			// Act
			var result = hostedServiceAttributeProvider.GetHostedServices()
				.Select(o => (o.Code, o.Description));

			// Assert
			AssertContainsExactElementsInAnyOrder(expected, result);
		}
	}
}
