using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobManagement.Testing
{
	[TestedType(typeof(AutoJobClosureDiagnosisForm))]
	public class AutoJobClosureDiagnosisFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_JobNum = shipment.JS_UniqueConsignRef;
			job.JH_GB = Env.CurrentBranch.PK;
			job.JH_GE = Env.CurrentDepartment.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = new TestObjectCreator(Factory).CC1.PK;
			charge.JR_LocalSellAmt = 1818m;
			Factory.Save();

			var monitor = new JCSDiagnosisMonitor(new ZGuid[] { job.PK });
			return new AutoJobClosureDiagnosisForm(monitor);
		}

		[TestDate(2024, 6, 18)]
		public void TestDiagnosisFunctionality()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var config = objectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true);
			var regValue = objectCreator.CreateJobClosureConfiguration(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var shipment = objectCreator.CreateShipment("S00987", "AUSYD", "USLAX");
			var job = objectCreator.CreateJob(shipment);
			job.JH_A_JOP = new ZDateTime(TestDateAttribute.Date.AddDays(-30));
			Factory.Save();

			var monitor = new JCSDiagnosisMonitor(new ZGuid[] { job.PK });

			using (var form = new AutoJobClosureDiagnosisForm(monitor))
			{
				form.Show();
				form.DiagnoseButton_ForTestOnly.PerformClick();

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

				AssertMultilineASCIIEquals(expectedMessage, form.TextBoxStackTrace_ForTestOnly.Text);

				SafeClipboard.Clear();

				form.ClipboardButton_ForTestOnly.PerformClick();
				var copiedText = SafeClipboard.GetText();
				if (copiedText.IsNullOrEmpty())
				{
					AssertMultilineASCIIEquals(SafeClipboard.ClipboardNotAccessibleWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertMultilineASCIIEquals(expectedMessage, SafeClipboard.GetText());
				}
			}
		}
	}
}
