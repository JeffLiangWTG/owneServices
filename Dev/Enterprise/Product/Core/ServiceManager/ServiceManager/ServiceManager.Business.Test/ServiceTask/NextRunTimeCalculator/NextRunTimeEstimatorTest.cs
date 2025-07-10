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
	[TestedType(typeof(NextRunTimeEstimator))]
	public class NextRunTimeEstimatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNextRunTimeEstimator()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;
			serviceTask.NextRunTimeCalculator =
				new NextRunTimeCalculatorDays { Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) };
			// Act
			var estimator = new NextRunTimeEstimator(serviceTask);

			// Assert
			AssertEquals(20, estimator.NextRunTimeList.Count);
		}

		public void TestNextRunTimeEstimator_DoesNotCalculateWhenScheduleHasErrors()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorDays { Period = 0, ScheduledRunTime = TimeSpan.FromHours(11) };

			// Act
			var estimator = new NextRunTimeEstimator(serviceTask);

			// Assert
			AssertEquals(0, estimator.NextRunTimeList.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var serviceTask = Factory.NewWithValidTestData<StmServiceTask>();
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;
			serviceTask.NextRunTimeCalculator =
				new NextRunTimeCalculatorDays { Period = 1 };

			return new NextRunTimeEstimator(serviceTask);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>());
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);
			hostedServiceAttributeProviderDisposable = ObjectFactory.Substitute(hostedServiceProviderMock);
		}

		protected override void TearDown()
		{
			hostedServiceAttributeProviderDisposable?.Dispose();
			base.TearDown();
		}

		IDisposable hostedServiceAttributeProviderDisposable;
	}
}
