using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.ServiceManager.Host;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.CW;
using ServiceManager.Integration.Abstractions;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.ServiceManager.Business.Testing
{
	sealed class HostedServiceQueuesProviderTest : TestCase
	{
		public void TestQueuesIsEnumerable()
		{
			// Arrange
			var hostedServiceQueuesSubProviders = GetHostedServiceQueuesSubProviders();
			var hostedServiceQueuesProvider = new HostedServiceQueuesProvider(hostedServiceQueuesSubProviders);

			// Act
			var queuesEnumerable = hostedServiceQueuesProvider.Queues;

			// Assert
			Assert("Type doesn't match", queuesEnumerable is IEnumerable<IHostedServiceQueue>);
		}

		public void TestQueuesHasCustomsQueues()
		{
			var testList = new ListObject()
			{
				Mock.Of<IHostedServiceQueuesSubProvider>(q => q.Queues == new[]
				{
					Mock.Of<IHostedServiceQueue>(sq => sq.ServiceTaskCode == "ABC" && sq.Name == "ABC _T#_ Name" && sq.QueueResult == new QueueResult(10, TimeSpan.Zero)),
					Mock.Of<IHostedServiceQueue>(sq => sq.ServiceTaskCode == "DEF" && sq.Name == "DEF _T#_ Name" && sq.QueueResult == new QueueResult(20, TimeSpan.Zero)),
					Mock.Of<IHostedServiceQueue>(sq => sq.ServiceTaskCode == "GHI" && sq.Name == "GHI _T#_ Name" && sq.QueueResult == new QueueResult(30, TimeSpan.Zero)),
				})
			};
			using (ObjectFactory.Substitute("HostedServiceQueuesSubProviders", testList))
			{
				var hostedServiceQueuesSubProviders = GetHostedServiceQueuesSubProviders();
				CombineAssertions(() =>
				{
					var provider = new HostedServiceQueuesProvider(hostedServiceQueuesSubProviders);
					var queues = provider.Queues.Where(x => x.Name.Contains(" _T#_ ")).ToArray();
					AssertEquals("queues.Length", 3, queues.Length);
					AssertIHostedServiceQueue(queues[0], "ABC", "ABC _T#_ Name");
					AssertIHostedServiceQueue(queues[1], "DEF", "DEF _T#_ Name");
					AssertIHostedServiceQueue(queues[2], "GHI", "GHI _T#_ Name");
				});
			}

			void AssertIHostedServiceQueue(IHostedServiceQueue hostedServiceQueue, string serviceTaskCode, string name)
			{
				AssertEquals("ServiceTaskCode", serviceTaskCode, hostedServiceQueue.ServiceTaskCode);
				AssertEquals("Name", name, hostedServiceQueue.Name);
			}
		}

		IHostedServiceQueuesSubProvider[] GetHostedServiceQueuesSubProviders()
		{
			return new ServiceCollection()
					.AddRegistrations([])
					.BuildServiceProvider()
					.GetRequiredService<IHostedServiceQueuesSubProvider[]>();
		}
	}
}
