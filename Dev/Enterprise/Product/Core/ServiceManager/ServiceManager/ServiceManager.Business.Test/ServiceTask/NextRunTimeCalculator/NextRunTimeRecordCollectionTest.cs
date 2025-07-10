using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(NextRunTimeRecordCollection))]
	sealed class NextRunTimeRecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NextRunTimeRecordCollection>
	{
		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestLoad()
		{
			// Arrange
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.ConfigControlTypeAssemblyName == "A" &&
				a.ConfigControlTypeName == "B" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var serviceTask = Factory.New<StmServiceTask>();
				serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;
				serviceTask.NextRunTimeCalculator =
					new NextRunTimeCalculatorDays { Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) };

				var collection = new NextRunTimeRecordCollection();

				// Act
				collection.Load(serviceTask);
				AssertEquals(20, collection.Count);

				// Assert
				CombineAssertions(() =>
				{
					for (var i = 0; i < collection.Count; i++)
					{
						AssertEquals(ZDateTime.UtcNow.AddDays(i + 1), collection[i].NextRunTime);
					}
				});
			}
		}

		protected override NextRunTimeRecordCollection GetCollectionToTest()
		{
			return new NextRunTimeRecordCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return NextRunTimeRecordTest.GetNewNextRunTimeRecord();
		}
	}
}
