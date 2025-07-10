using System.Threading;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class TestTransferRuleRunner : TransferRuleRunner
	{
		public TestTransferRuleRunner(IPAVESystem system, ILogger logger, ITransferRuleRunnerParams transferRuleRunnerParams, ITransferRuleRunnerDataAccessor dataAccessor = null)
			: this(system, new TransferRuleRunnerLogger(logger, system), dataAccessor, transferRuleRunnerParams)
		{
		}

		public TestTransferRuleRunner(IPAVESystem system, ILogger logger, ITransferRuleRunnerDataAccessor dataAccessor = null, ITransferRuleRunnerParams transferRuleRunnerParams = null)
			: this(system, new TransferRuleRunnerLogger(logger, system), dataAccessor, transferRuleRunnerParams ?? new TransferRuleRunnerParams())
		{
		}

		TestTransferRuleRunner(IPAVESystem system, ITransferRuleRunnerLogger logger, ITransferRuleRunnerDataAccessor dataAccessor, ITransferRuleRunnerParams transferRuleRunnerParams)
			: this(system, dataAccessor ?? new TransferRuleRunnerDataAccessor(logger), logger, transferRuleRunnerParams)
		{
		}

		TestTransferRuleRunner(IPAVESystem system, ITransferRuleRunnerDataAccessor dataAccessor, ITransferRuleRunnerLogger logger, ITransferRuleRunnerParams transferRuleRunnerParams)
			: base(system, dataAccessor, logger, transferRuleRunnerParams)
		{
		}

		public void Process_ForTest()
		{
			using (Env.Instance.TemporaryServiceTaskContext(TransferRuleRunnerServiceTask.Code, canRunInAnyBranch: true))
			{
				Process(CancellationToken.None);
			}
		}

		protected override void OnTransferWorkflows()
		{
			TestDateAttribute.AddSeconds(1);
			base.OnTransferWorkflows();
		}
	}
}
