using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(StmServiceTaskBranchValidationAdapter))]
	class StmServiceTaskBranchValidationAdapterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			// Act
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

			// Assert
			Assert(adapter.S5_IsActive);
			AssertEquals(GlbBranch.CurrentBranch.PK, adapter.S5_GB);
		}

		public void TestGetS5_GB()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();
			var testBranch = Guid.NewGuid();

			// Act
			adapter.SST_GB_Branch = testBranch;

			// Assert
			AssertEquals(testBranch, adapter.S5_GB.ToGuid());
		}

		public void TestSetS5_GB()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();
			var testBranch1 = Guid.NewGuid();
			var testBranch2 = Guid.NewGuid();

			adapter.SST_GB_Branch = testBranch1;

			// Act
			adapter.S5_GB = testBranch2;

			// Assert
			AssertEquals(testBranch2, adapter.SST_GB_Branch.ToGuid());
		}

		public void TestGetS5_ParentTableCode()
		{
			// Act
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

			// Assert
			AssertEquals(Constants.ServiceTask.ParentTableCode, adapter.S5_ParentTableCode);
		}

		public void TestSetS5_ParentTableCode_DoesNothing()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

			// Act
			adapter.S5_ParentTableCode = "~1";

			// Assert
			AssertEquals(Constants.ServiceTask.ParentTableCode, adapter.S5_ParentTableCode);
		}

		public void TestGetS5_ScheduleDescription()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				// Act
				var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

				// Assert
				AssertEquals("some description", adapter.S5_ScheduleDescription);
			}
		}

		public void TestSetS5_ScheduleDescription_DoesNothing()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

				// Act
				adapter.S5_ScheduleDescription = "another desc";

				// Assert
				AssertEquals("some description", adapter.S5_ScheduleDescription);
			}
		}

		public void TestGetS5_ScheduleType()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

			// Act
			adapter.SST_ServiceTaskCode = "~#1";

			// Assert
			AssertEquals("~#1", adapter.S5_ScheduleType);
		}

		public void TestSetS5_ScheduleType_DoesNothing()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();
			adapter.SST_ServiceTaskCode = "~#1";

			// Act
			adapter.S5_ScheduleType = "~99";

			// Assert
			AssertEquals("~#1", adapter.S5_ScheduleType);
		}

		public void TestGetS5_IsActive()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

			// Act
			adapter.SST_Active = true;

			// Assert
			Assert(adapter.S5_IsActive);

			// Act
			adapter.SST_Active = false;

			// Assert
			Assert(!adapter.S5_IsActive);
		}

		public void TestSetS5_IsActive()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();
			adapter.SST_Active = false;

			// Act
			adapter.S5_IsActive = true;

			// Assert
			Assert(adapter.SST_Active);
		}

		[TestDate(2024, 1, 1, 1, 0, 0)]
		public void TestGetS5_NextScheduledPrintRunTimeUtc()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();

			// Act
			adapter.SST_NextRunTime = ZDateTimeOffset.UtcNow.AddHours(1);

			// Assert
			AssertEquals(ZDateTimeOffset.UtcNow.AddHours(1).ToUtcZDateTime(), adapter.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2024, 1, 1, 1, 0, 0)]
		public void TestSetS5_NextScheduledPrintRunTimeUtc_DoesNothing()
		{
			// Arrange
			var adapter = Factory.New<StmServiceTaskBranchValidationAdapter>();
			adapter.SST_NextRunTime = ZDateTimeOffset.UtcNow.AddHours(1);

			// Act
			adapter.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddDays(3).AddHours(2).AddMinutes(1);

			// Assert
			AssertEquals(ZDateTimeOffset.UtcNow.AddHours(1).ToUtcZDateTime(), adapter.S5_NextScheduledPrintRunTimeUtc);
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
