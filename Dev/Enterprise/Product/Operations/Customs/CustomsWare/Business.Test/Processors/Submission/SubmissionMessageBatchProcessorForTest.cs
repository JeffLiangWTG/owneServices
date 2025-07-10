using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class SubmissionMessageBatchProcessorForTest : SubmissionMessageBatchProcessor
	{
		public SubmissionMessageBatchProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString PreProcessMessages(NonDependentEDIMessageCollection readyMessages)
		{
			void FactorySaving(BusinessObjectFactory factory)
			{
				var currentBranchPK = Env.CurrentBranchPK;
				if (readyMessages.Cast<EDIMessage>().Any(x => x.EM_GB != currentBranchPK))
				{
					factory.Saving -= FactorySaving;
					throw new InvalidOperationException("The save can happen in the wrong branch,");
				}
			}

			void FactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				factory.Saving -= FactorySaving;
				factory.Saved -= FactorySaved;
			}

			readyMessages.Factory.Saving += FactorySaving;
			readyMessages.Factory.Saved += FactorySaved;
			return base.PreProcessMessages(readyMessages);
		}
	}
}
