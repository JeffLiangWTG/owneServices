using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	class StmServiceTaskExtendedConfigurationControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return StmServiceTaskFormTest.GetFormToBashWithExcludedCaptions(Factory.New<StmServiceTask>());
		}

		public void TestExtendedConfigurationTextboxesFitWord()
		{
			// Arrange
			ObjectFactory.DisposeSubstitutions();
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "ARC" &&
				a.CanRunInAnyBranch &&
				a.AllowsMultipleInstances &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "10minutes"));
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);

			using var hostedServiceAttributeProvider = ObjectFactory.Substitute(hostedServiceProviderMock);
			var taskSchedule = Factory.New<StmServiceTask>();
			taskSchedule.SST_ServiceTaskCode = "ARC";
			int expectedWidth = 300;

			// Act
			using var form = new StmServiceTaskForm(taskSchedule);
			form.Show();

			// Assert
			var dropEditSecondaryProcessesMaxCount = form.Controls.Find("dropEditSecondaryProcessesMaxCount", true)[0];
			AssertEquals(expectedWidth, dropEditSecondaryProcessesMaxCount.Width);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ObjectFactory.DisposeSubstitutions();
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "~01" &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));
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
