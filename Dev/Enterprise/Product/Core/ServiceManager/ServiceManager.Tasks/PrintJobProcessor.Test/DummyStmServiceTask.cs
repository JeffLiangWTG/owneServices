using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ServiceManager;
using Enterprise.ServiceManager.Business;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing;

public class DummyStmServiceTask : NonPersistentBusinessObject, IStmServiceTaskConfigControlDataProvider
{
	public DummyStmServiceTask(BusinessObjectFactory factory)
		: base(factory)
	{
		this.factory = factory;
	}

	public StmServiceTaskAdapter ConfigAdapter
	{
		get
		{
			if (adapter is null)
			{
				var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == "15minutes");

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == defaultScheduleMock);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				_ = hostedServiceProviderMock
						.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
						.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var serviceTask = factory.New<StmServiceTask>();
					adapter = new DummyStmServiceTaskAdapter(factory, serviceTask);
				}
			}

			return adapter;
		}
	}

	readonly BusinessObjectFactory factory;
	DummyStmServiceTaskAdapter adapter;

	class DummyStmServiceTaskAdapter : StmServiceTaskAdapter, IServiceTaskSchedule
	{
		public DummyStmServiceTaskAdapter(BusinessObjectFactory factory, StmServiceTask serviceTask)
			: base(factory, serviceTask)
		{
		}

		#region Schema
		public static new class Schema
		{
			public const string ConfigString = "ConfigString";
			public const string TableName = "ServiceTaskSchedule";
		}
		#endregion

		public string ConfigString { get; set; }

		public string GetBranchCountryCode()
		{
			return "";
		}

		ZGuid IStmScheduleTask.S5_GB { get; set; }
		ZString IStmScheduleTask.S5_ParentTableCode { get; set; }
		ZString IStmScheduleTask.S5_ScheduleDescription { get; set; }
		ZBool IStmScheduleTask.S5_IsActive { get; set; }
		ZString IStmScheduleTask.S5_ScheduleType { get; set; }
		ZDateTime IStmScheduleTask.S5_NextScheduledPrintRunTimeUtc { get; set; }

		public bool IsNudgeable => false;
	}
}
