using System;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Development.Common
{
	public class BiCustomLogger
	{
		#region Singleton Pattern

		BiCustomLogger(Action<string> loggerFunction, bool logSubTask)
		{
			this.loggerFunction = loggerFunction;
			this.logSubTask = logSubTask;
			RegisterEvents();
		}

		public static void Initialize(Action<string> loggerFunction, bool logSubTask = false)
		{
			instance = new BiCustomLogger(loggerFunction, logSubTask);
		}

		public static BiCustomLogger Instance
		{
			get
			{
				return instance;
			}
		}
		[ThreadSafe]
		static BiCustomLogger instance;

		#endregion

		void RegisterEvents()
		{
			BiLogger.Instance.OnStartTask += new BiLoggerEvent(Log);
			BiLogger.Instance.OnCompleted += new BiLoggerEvent(Log);
			BiLogger.Instance.OnStartSubtask += new BiLoggerEvent((message, time) => Log("\t" + message, time));
			BiLogger.Instance.OnFailed += new BiLoggerEvent(Log);

			if (logSubTask)
			{
				BiLogger.Instance.OnStartSubtask += new BiLoggerEvent(Log);
			}
		}

		void Log(string message, DateTime time)
		{
			if (!string.IsNullOrEmpty(message))
			{
				try
				{
					loggerFunction(message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		readonly Action<string> loggerFunction;
		readonly bool logSubTask;
	}
}
