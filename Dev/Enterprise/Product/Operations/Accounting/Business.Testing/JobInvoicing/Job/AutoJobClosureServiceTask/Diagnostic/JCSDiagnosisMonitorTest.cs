using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic.Testing
{
	[TestedType(typeof(JCSDiagnosisMonitor))]
	public class JCSDiagnosisMonitorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTraceSourceSettingsList()
		{
			var tracer = new JCSDiagnosisMonitor(new ZGuid[] { Guid.NewGuid() });
			AssertEquals(1, tracer.TraceSourceSettingsList.Count);
			AssertEquals(AccountingTraceSourceCodes.JCS, tracer.TraceSourceSettingsList[0].TraceSourceName);
		}

		public void TestBizoProperties()
		{
			var msgWriter = new DummyMessageWriter();
			var tracer = new JCSDiagnosisMonitor(new ZGuid[] { Guid.NewGuid() });
			tracer.MessageWriter = msgWriter;

			AssertEquals(nameof(tracer.LogCallStack), ZBool.False, tracer.LogCallStack);
			AssertEquals(nameof(tracer.LogDateTime), ZBool.False, tracer.LogDateTime);
			AssertEquals(nameof(tracer.LogThreadId), ZBool.False, tracer.LogThreadId);
			AssertEquals(nameof(tracer.LogProcessId), ZBool.False, tracer.LogProcessId);
			AssertEquals(nameof(tracer.MessageWriter), msgWriter, tracer.MessageWriter);
		}

		public void TestIntitializeAndClearTraceSources()
		{
			var traceMonitor = new TraceMonitor();
			traceMonitor.MessageWriter = new DummyMessageWriter();
			traceMonitor.IntitializeTraceSources();
			Assert("Trace sources successfully initialized", traceMonitor.IsTracing);
		}

		[TestDate(2024, 6, 18)]
		public void TestQueueJobsForDiagnosisAndMonitor()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var config = objectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true);
			var regValue = objectCreator.CreateJobClosureConfiguration(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var shipment = objectCreator.CreateShipment("S00987", "AUSYD", "USLAX");
			var job = objectCreator.CreateJob(shipment);
			job.JH_A_JOP = new ZDateTime(TestDateAttribute.Date.AddDays(-30));
			Factory.Save();

			var tracer = new JCSDiagnosisMonitor(new ZGuid[] { job.PK });
			tracer.MessageWriter = new DummyMessageWriter();
			tracer.IntitializeTraceSources();
			tracer.QueueJobsForDiagnosisAndMonitor();

			var companyCode = GlbCompany.CurrentCompany.GC_Code;
			var branchCode = GlbBranch.CurrentBranch.GB_Code;
			var deptCode = GlbDepartment.CurrentDepartment.GE_Code;

			var expectedMessage = FormattableString.Invariant($@"

[GC:{companyCode}][GB:{branchCode}][GE:{deptCode}][GS:~BP]
JCS Diagnosis Information: 1 : Debug: [{companyCode}][S00987][DSB]: This job is not eligible for automatic status update

[GC:{companyCode}][GB:{branchCode}][GE:{deptCode}][GS:~BP]
JCS Diagnosis Information: 2 : Debug: [{companyCode}][S00987][JFC]: This job is not eligible for automatic status update

[GC:{companyCode}][GB:{branchCode}][GE:{deptCode}][GS:~BP]
JCS Diagnosis Information: 3 : Debug: [{companyCode}][S00987][JCS]: Auto Job Closure eligibility verification details - 
 [S00987]:
Job Type: SHP | Direction: EXP | Mode: SEA | Department: {deptCode} | Open WIP: No | Open Accrual: No | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: No.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: Yes- Open Accrual: Yes- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 19 May 2024.
Calculated earliest job closure date: 29 May 2024.
Satisfied registry settings.

[GC:{companyCode}][GB:{branchCode}][GE:{deptCode}][GS:~BP]
JCS Diagnosis Information: 4 : Debug: [{companyCode}][S00987][JCS]: This job is eligible for automatic closing.

[GC:{companyCode}][GB:{branchCode}][GE:{deptCode}][GS:~BP]
JCS Diagnosis Information: 5 : Information: Job Status Update cycle completed.
Assessed 1 jobs for status update.");

			var actualMessage = string.Concat((tracer.MessageWriter as DummyMessageWriter).Messages);
			AssertMultilineASCIIEquals(expectedMessage, actualMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JCSDiagnosisMonitor(new ZGuid[] { Guid.NewGuid() });
		}
	}
}
