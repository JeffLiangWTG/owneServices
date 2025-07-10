using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLJournalControllerChina))]
	public class GLJournalControllerChinaTest : GLJournalControllerTest
	{
		public new void TestGetForm()
		{
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.Lines.AddNew();
			ChinaController.FReversing_ForTestOnly = null;
			using (ZForm form = (ZForm)ChinaController.GetForm_ForTestOnly(journal))
			{
				AssertEquals("Form's business entity should be the transaction journal", journal.PK, form.BusinessEntity.Identifier);
			}

			ChinaController.FReversing_ForTestOnly = new GLJournalReversing(journal);
			using (ZForm form = (ZForm)ChinaController.GetForm_ForTestOnly(journal))
			{
				Assert("Form's business entity should be a reversing journal", journal.PK != form.BusinessEntity.Identifier);
			}
		}

		public override void TestDeleteForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase() as GLJournal;
			var form = Controller.ShowDeleteForm(sourceEntity);
			AssertNotNull("DeleteForm should not be null for China", form);
			AssertEquals(ODisplayMode.Delete, form.DisplayMode);
		}

		public void TestShowDeleteFormForAutoCurrencyAdjustmentReversing()
		{
			var gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			var gLAccount2 = Factory.NewWithValidTestData<AccGLHeader>();

			var journal = Factory.NewWithValidTestData<GLJournal>();
			var period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_IsGeneralLedgerClosed = false;
			journal.PeriodPK = period.PK;

			var line1 = (GLJournalLine)journal.Lines.AddNew();
			line1.AL_AG = gLAccount1.PK;
			line1.AL_OSExTaxAmount = 10m;
			line1.AL_LocalExTaxAmount = 10m;
			var line2 = (GLJournalLine)journal.Lines.AddNew();
			line2.AL_AG = gLAccount2.PK;
			line2.AL_OSExTaxAmount = -10m;
			line2.AL_LocalExTaxAmount = -10m;
			Factory.Save();

			using (var journalForm = (GLJournalForm)ChinaController.ShowDeleteForm(journal))
			{
				var reversingGLJournal = journalForm.BusinessEntity as GLJournal;
				Assert("The form should show the reversing GLJournal", reversingGLJournal.PK != journal.PK);
				AssertEquals("There should be 2 lines in the Reversing GLJournal", 2, reversingGLJournal.Lines.Count);
				var revLine1 = (GLJournalLine)reversingGLJournal.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AG, gLAccount1.PK))[0];
				AssertEquals("Line1 should have OS amount -10", -10m, revLine1.AL_OSExTaxAmount);
				AssertEquals("Line1 should have Local amount -10", -10m, revLine1.AL_LocalExTaxAmount);
				var revLine2 = (GLJournalLine)reversingGLJournal.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AG, gLAccount2.PK))[0];
				AssertEquals("Line2 should have OS amount 10", 10m, revLine2.AL_OSExTaxAmount);
				AssertEquals("Line2 should have Local amount 10", 10m, revLine2.AL_LocalExTaxAmount);

				AssertNotNull("FReversing_ForTestOnly should be set on the controller", ChinaController.FReversing_ForTestOnly);
			}

			journal.AH_IsCancelled = true;
			using (var journalForm2 = ChinaController.ShowDeleteForm(journal))
			{
				AssertNull("cannot reverse an already reversed journal", journalForm2);
				AssertEquals("This transaction cannot be reversed because it has already been reversed or is a reversal of another transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			OriginalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(OriginalCountry);
		}

		protected GLJournalControllerChina ChinaController
		{
			get { return Controller as GLJournalControllerChina; }
		}

		string OriginalCountry;
	}
}
