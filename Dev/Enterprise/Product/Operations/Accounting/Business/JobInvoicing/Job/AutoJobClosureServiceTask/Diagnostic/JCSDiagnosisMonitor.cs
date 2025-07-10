using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic
{
	public class JCSDiagnosisMonitor : TraceMonitor
	{
		public JCSDiagnosisMonitor(IEnumerable<ZGuid> jobPKs) : base()
		{
			JobPKs = jobPKs;
			LogCallStack = ZBool.False;
			LogDateTime = ZBool.False;
			LogThreadId = ZBool.False;
			LogProcessId = ZBool.False;
		}
		IEnumerable<ZGuid> JobPKs { get; }

		public int JobCount => JobPKs.Count();

		public ZString Log
		{
			get { return logText; }
			set { SetNonPersistentPropertyValue(LogInfo, ref logText, value); }
		}
		ZString logText;

		public ZPropertyInfo<ZString> LogInfo => (ZPropertyInfo<ZString>)GetZPropertyInfo(nameof(Log));

		protected override TraceSourceSettingsCollection GetTraceSourceSettingsCollectionCore()
		{
			return new TraceSourceSettingsCollection()
			{
				new TraceSourceSettings() { TraceSourceCategory = AccountingTraceSourceCodes.CategoryName, TraceSourceName = AccountingTraceSourceCodes.JCS, TraceSourceDescription = string.Empty, TraceLevel = TraceSourceLevels.Codes.Verbose, TraceFilter_ReadOnly = true }
			};
		}

		public void QueueJobsForDiagnosisAndMonitor()
		{
			if (JobPKs?.Any() ?? false)
			{
				var processor = new AutoJobStatusUpdateProcessor(new JCSDiagnosisLogger(), new JCSJobProviderForDiagnostics(JobPKs), new BusinessObjectFactoryProviderForJCSDiagnosis(new ReadOnlyBusinessObjectFactory()));
				processor.Process(CancellationToken.None);
			}
		}

		public IMessageWriter GetMessageWriter() => new JCSTraceMessageWriter(LogInfo);
	}
}
