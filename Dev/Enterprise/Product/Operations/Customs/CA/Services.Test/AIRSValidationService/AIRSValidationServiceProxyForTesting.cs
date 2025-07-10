using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Customs.CA.Services.AIRSValidationService;

namespace Enterprise.Customs.CA.Services.Testing
{
	public sealed class AIRSValidationServiceProxyForTesting : AIRSValidationServiceProxy
	{
		public AIRSValidationServiceProxyForTesting(IAIRSValidationServiceSettings settings)
			: base(settings)
		{
		}

		public int TaskDelay;

		public Exception ValidateTransactionException;

		public Exception ValidateException;

		public ValidateTransactionResult ValidateTransactionResult => validateTransactionResult ?? (validateTransactionResult = new ValidateTransactionResult());

		public void SetCommunicationException() => ValidateException = new CommunicationException();

		internal override int MessageSizeLimit => 200 * 1024;

		protected override Task<byte[]> ValidateRequirementsCore(BrokerValidationServiceClient bvsClient, byte[] xmlContent, IValidateTransaction requestObject, IEnumerable<IAIRSValidationQueriedLine> lines, CancellationTokenSource cancellationTokenSource)
		{
			if (ValidateException != null)
			{
				throw ValidateException;
			}

			var task = Task.Run(() =>
			{
				if (ValidateTransactionException != null)
				{
					throw ValidateTransactionException;
				}

				return ValidateTransactionResult.Serialize();
			});

			if (TaskDelay > 0)
			{
				var alreadyDelay = 0;
				while (alreadyDelay < TaskDelay)
				{
					Thread.Sleep(1);
					alreadyDelay++;

					if (cancellationTokenSource.IsCancellationRequested)
					{
						throw new OperationCanceledException();
					}
				}
			}

			task.Wait();
			return task;
		}

		ValidateTransactionResult validateTransactionResult;
	}
}
