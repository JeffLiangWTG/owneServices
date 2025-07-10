using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SecondStepMessageOperationalActionRunner
	{
		public SecondStepMessageOperationalActionRunner(IOperationalActionSectionLog log, IEnumerable<JobDeclaration> declarations)
		{
			logger = new OperationalActionSectionLogWrapper(log);
			targets = declarations;
		}

		public void SendSecondStepMessage()
		{
			var entries = targets.SelectMany(x => x.ActiveEntryHeaders)
								.Cast<CusEntryHeader>()
								.Where(x => x.IsDeltaDStepOneSentOK && !x.IsDeltaDStepTwoSentOK)
								.ToArray();

			var countOfEntries = entries.Length;
			if (countOfEntries > 0)
			{
				logger.SetSectionProgressMax(countOfEntries);
				logger.LogFormat(LogType.Information, (NoResString)"{0} entries found", countOfEntries);

				var processor = new FRAutoD2MMessageSender(logger);
				processor.ProcessEntries(entries);
			}
			else
			{
				logger.Log(LogType.Information, $"No Delta G2 entries with entry status BAE were found");
			}
		}

		readonly ICommonLogger logger;
		readonly IEnumerable<JobDeclaration> targets;
	}
}
