using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	[TestedType(typeof(TriggerActionWithOptionalFactorySaveLogSubscriber))]
	class TriggerActionWithOptionalFactorySaveLogSubscriberPostLocalSisterCompanyChargeTest : TriggerActionWithOptionalFactorySaveLogSubscriberTest
	{
		protected override string WorkflowTriggerActionType => WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly;

		protected override void SetupTriggerProcessorMock(IWorkflowProvider provider, IProcessor processor)
		{
			var mock = new Mock<ILocalSisterCompanyChargePosterCreator>();
			mock.Setup(x => x.CreateLocalSisterCompanyChargePoster(It.Is<IWorkflowProvider>(p => p.PK == provider.PK))).Returns(processor);
			ObjectFactory.Substitute(mock.Object);
		}
	}
}
