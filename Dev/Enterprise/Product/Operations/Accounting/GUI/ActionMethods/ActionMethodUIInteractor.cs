using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class ActionMethodUIInteractor : ILoggerWithSession
	{
		public ActionMethodUIInteractor(IOperationalActionSectionLog log)
		{
			Argument.NotNull(log, "log");
			this.log = log;
		}

		protected readonly IOperationalActionSectionLog log;

		protected readonly List<string> warningsEncountered = new List<string>();
		protected readonly List<string> messagesEncountered = new List<string>();

		public IDisposable StartRatingSession()
		{
			return new RatingSession(log);
		}

		protected class RatingSession : IDisposable
		{
			public RatingSession(IOperationalActionSectionLog log)
			{
				this.log = log;
			}

			readonly IOperationalActionSectionLog log;

			public void Dispose()
			{
				log.BumpSectionProgress();
			}
		}

		public void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell)
		{
			if (warningsEncountered.Any())
			{
				var sb = new ZStringBuilder(warningsEncountered);
				sb.Prepend(Res.GetString("c4b8dd5a-5705-42b2-9c70-e45e05646adb", "Warnings Encountered:"));
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0}: {1}", ActionMethodsHelper.TargetReference(RatingCache.LocalSession.Target), sb.ToStringWithNewLineBetweenAppends());
				warningsEncountered.Clear();
			}
		}

		public void Log(LogType type, string message)
		{
			Log(type, message, null);
		}

		public virtual void Log(LogType type, string message, Exception ex)
		{
			var logLevel = OperationalActionLogErrorLevel.Informational;
			switch (type)
			{
				case LogType.Error:
					logLevel = OperationalActionLogErrorLevel.Error;
					break;

				case LogType.Warning:
					warningsEncountered.Add(message);
					break;

				case LogType.Information:
					messagesEncountered.Add(message);
					return;

				case LogType.Debug:
					if (RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.Value)
					{
						messagesEncountered.Add(message);
					}
					break;
			}

			if (type != LogType.Debug || RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.Value)
			{
				log.NotifyFormat(logLevel, "{0}: {1}", ActionMethodsHelper.TargetReference(RatingCache.LocalSession.Target), message);
			}
		}
	}
}
