using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(JobCostingJournalForm))]
	public class JobCostingJournalFormTest : ZFormBasherTest
	{
		#region Plug-ins

		public void TestDataExportBatchPluginIsAdded()
		{
			using (var form = (JobCostingJournalForm)GetFormToBashCore())
			{
				var source = form.BusinessEntity as IDataExportBatchSource;
				AssertNotNull("Precondition: entity is IDataExportBatchSource", source);
				AssertNotNull("Precondition: IDataExportBatchSource entity has support", source.IsDataExportBatchSupported);
				AssertNotNull("IDataExportBatchSource entity should have plugin", form.PlugIns.GetPlugIn(ControllerIDs.DataExportBatchPlugin));
			}
		}

		#endregion

		protected override Form GetFormToBashCore()
		{
			JCJournalHeader header = Factory.New<JCJournalHeader>();
			header.SetReadOnlyIncludingChildren(true);//only displayed as readonly, never edited
			return new JobCostingJournalForm(header);
		}

		public void TestFormBecomesReadOnlyAfterACriticalValidationError()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year);
			Factory.Save();

			var job = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 250m, testObjectCreator.Agent, 250m);
			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, 250m, 250m);
			var testJournal = testObjectCreator.CreateJCJournalHeader(new ZDateTime(2014, 06, 01), 250m);
			testObjectCreator.CreateJCJournalLine(testJournal, testObjectCreator.CC1, job, new ZDateTime(2014, 06, 01), 250m);
			Factory.Save();

			var bizo = Factory.Load<DummyJCJournalCriticalValidationParent>(testJournal.PK);
			bizo.PostPeriod = 201406;

			using (var form = new JobCostingJournalFormTestForCriticalValidationException(bizo))
			{
				AssertEquals("Context (Before Critical Validation Error)", false, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertNotEquals("DisplayMode (Before Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);

				bizo.CriticalValidation.RegisterOnSavingCheck();
				form.DisplayMode = ODisplayMode.New;

				form.Show();
				form.OnPostButtonClick_ForTestOnly(null, null);

				AssertEquals("Context (After Critical Validation Error)", true, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertEquals("DisplayMode (After Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		class JobCostingJournalFormTestForCriticalValidationException : JobCostingJournalForm
		{
			public JobCostingJournalFormTestForCriticalValidationException(DummyJCJournalCriticalValidationParent bizo)
				: base(bizo)
			{
			}
		}

		class DummyJCJournalCriticalValidationParent : JCJournalHeader, ISupportCriticalValidation
		{
			public DummyJCJournalCriticalValidationParent(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			#endregion

		}

		class DummyCriticalValidation : CriticalValidation<DummyJCJournalCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyJCJournalCriticalValidationParent parent)
				: base(parent)
			{
			}

			protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
			}

			protected override IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
			}
		}
	}
}
