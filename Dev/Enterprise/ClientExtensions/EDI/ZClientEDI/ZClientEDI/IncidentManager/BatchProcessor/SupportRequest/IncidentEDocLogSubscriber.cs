using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	[Serializable]
	public class IncidentEDocLogSubscriber : LogSubscriber
	{
		public override string Name
		{
			get { return "IncidentEDocLogSubscriber"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { IncidentRequestSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.DocumentImported.Code }; }
		}

		public override bool IsClientSpecificSubscriber => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var processor = GetProcessor();
			foreach (var logsPerParent in queuedLogs.Where(x => x.SJ_GS_NKUser == User.WebUserCode)
				.GroupBy(x => x.SJ_ParentID))
			{
				var factory = queuedLogs[0].Factory;
				processor.ProcessNewEdocs(factory, logsPerParent.Key, logsPerParent.Select(x => PkFromLogReference(x.SJ_Reference)).Where(x => !x.IsEmpty));
			}
		}

		ISupportRequestProcessor GetProcessor()
		{
#if DEBUG
			if (Globals.IsTest && !CreateRealProcessor)
			{
				var result = CargoWise.Application.ObjectFactory.Get<ISupportRequestProcessor>();
				if (result != null)
				{
					return result;
				}
			}
#endif
			return new SupportRequestProcessor(DefaultLogger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		static ZGuid PkFromLogReference(ZString logRef)
		{
			return BaseStmALog.GetGuid(logRef);
		}

#if DEBUG
		public override bool EnableFactorySaveAlerterInTesting => true;

		[ThreadSafe]
		static bool CreateRealProcessor = false;

		internal static IDisposable CreateRealProcessorOverride()
		{
			return new Override();
		}

		sealed class Override : IDisposable
		{
			public Override()
			{
				CreateRealProcessor = true;
			}

			public void Dispose()
			{
				CreateRealProcessor = false;
			}
		}
#endif
	}
}
