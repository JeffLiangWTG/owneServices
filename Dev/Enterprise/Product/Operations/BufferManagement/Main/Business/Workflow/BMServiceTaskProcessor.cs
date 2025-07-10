using System;
using System.Threading;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public abstract class BMServiceTaskProcessor : IPAVEProcessor
	{
		protected BMServiceTaskProcessor(ILogger logger, ServiceTaskFactoryProviderWrapper factoryProvider)
		{
			Logger = logger;
			FactoryProvider = factoryProvider;
		}

		protected ILogger Logger { get; }

		protected internal ServiceTaskFactoryProviderWrapper FactoryProvider { get; }

		public void Process(CancellationToken token)
		{
			if (!AreConditionsMetAndLogIfNot())
			{
				return;
			}

			ProcessCore(token);
		}

		bool AreConditionsMetAndLogIfNot()
		{
			if (!IsSufficientWorkflowManagementModeEnabled)
			{
				var location = BMSRegistry.Instance.WorkflowManagementMode.GetLocation();
				Logger.Information(FormattableString.Invariant($"The registry item [{location}] must be set to a higher level to use this processor."));
				return false;
			}

			return true;
		}

		protected virtual bool IsSufficientWorkflowManagementModeEnabled => true;

		public abstract void ProcessCore(CancellationToken token);
	}
}
