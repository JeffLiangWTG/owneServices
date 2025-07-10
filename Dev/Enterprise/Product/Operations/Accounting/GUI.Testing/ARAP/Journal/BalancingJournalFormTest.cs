using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.GUI.ARAP.Journal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.ARAP.Journal
{
	public abstract class BalancingJournalFormTest : AccountingZFormBasherTest
	{
		protected override bool ShouldHaveAuditPlugIn => true;

		protected override Form GetFormToBashCore()
		{
			return new BalancingJournalForm(CreateJournalCore());
		}

		protected abstract Business.ARAP.Journal.Journal CreateJournalCore();

		protected abstract Business.ARAP.Journal.Journal CreateValidJournalCore();

		protected abstract NewMatchGroupForm CreateNewMatchGroupForm();

		protected abstract ZController Controller { get; }

		public void TestCloseJournalsFormWillNotPostJournalWhenEditInNewMatchGroupForm()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			using (var newMatchGroupForm = CreateNewMatchGroupForm())
			{
				var journal = CreateValidJournalCore();
				journal.Validation.ValidateAll();
				AssertEquals(0, journal.NotificationsIncludingChildren.Count());

				using (var form = Controller.ShowEditForm(journal) as ZForm)
				{
					(form as IPostingButtonsProvider).CommandButtonCancel.PerformClick();
					Assert("Check whether the form is closed", form.IsDisposed);
					AssertNull(new BusinessObjectFactory().Load<AccTransactionHeader>(journal.PK));
				}

				using (var form = Controller.ShowEditForm(journal) as ZForm)
				{
					var dialogBox = UnitTestUserNotification.Instance;
					dialogBox.AddAnswer(DialogResult.Yes);
					form.Close();
					AssertNull(new BusinessObjectFactory().Load<AccTransactionHeader>(journal.PK));
					AssertNotEquals("Should not notice user whether or not they want to save", "This record has been modified.\r\nWould you like to save the changes?", dialogBox.LastMessage.Text);
					Assert("Check whether the form is closed", form.IsDisposed);
				}
			}
		}
	}

	[TestedType(typeof(BalancingJournalForm))]
	public class TestAR : BalancingJournalFormTest
	{
		protected override ZController Controller => ZControllerFactory.Create(ControllerIDs.ARBalancingJournal);

		protected override NewMatchGroupForm CreateNewMatchGroupForm()
		{
			var matchingBase = new ARMatchingBase(Factory,Factory.NewWithValidTestData<ARPayment>());
			return new NewMatchGroupForm(matchingBase);
		}

		protected override Business.ARAP.Journal.Journal CreateValidJournalCore()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			Assert(testObjectCreator.Debtor1.OH_IsDebtor);
			Factory.Save();

			var journal = Factory.NewWithValidTestData<ARJournal>();
			journal.AH_OH = testObjectCreator.Debtor1.PK;
			journal.AH_OSExTaxAmount = 1m;
			return journal;
		}

		protected override Business.ARAP.Journal.Journal CreateJournalCore() => Factory.New<ARJournal>();
	}

	[TestedType(typeof(BalancingJournalForm))]
	public class TestAP : BalancingJournalFormTest
	{
		protected override ZController Controller => ZControllerFactory.Create(ControllerIDs.APBalancingJournal);

		protected override NewMatchGroupForm CreateNewMatchGroupForm()
		{
			var matchingBase = new APMatchingBase(Factory,Factory.NewWithValidTestData<APPayment>());
			return new NewMatchGroupForm(matchingBase);
		}

		protected override Business.ARAP.Journal.Journal CreateValidJournalCore()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			Assert(testObjectCreator.Creditor1.OH_IsCreditor);
			Factory.Save();

			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_OH = testObjectCreator.Creditor1.PK;
			journal.AH_OSExTaxAmount = 1m;
			return journal;
		}

		protected override Business.ARAP.Journal.Journal CreateJournalCore() => Factory.New<APJournal>();
	}
}
