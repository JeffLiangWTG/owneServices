using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Transaction.Base.Testing
{
	public abstract class TransactionControllerWithReadOnlyBehaviourControlledBySourceTest : TransactionControllerWithLoginCompanyCheckTest
	{
		public void TestCheckPointForEdit()
		{
			var transaction = TransactionImportedFromUniversalXML;
			if (transaction != null)
			{
				AssertCheckPointForEdit(transaction, ExpectedEditCheckPointForUniveralXMLImportedTransaction);
			}
			else
			{
				AssertNull("CheckPoint is null", ExpectedEditCheckPointForDirectEnteredTransaction);
				AssertNull("CheckPoint is null", ExpectedEditCheckPointForUniveralXMLImportedTransaction);
				AssertNull("CheckPoint is null", ExpectedEditHeaderCheckPointForDirectEnteredTransaction);
				AssertNull("CheckPoint is null", ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction);
			}

			transaction = GetBusinessObjectThatIsInTheDatabase() as InvoicingBase;
			AssertCheckPointForEdit(transaction, ExpectedEditCheckPointForDirectEnteredTransaction);

			Assert("Edit CheckPoints should be overriden in pair", (ExpectedEditCheckPointForDirectEnteredTransaction == null && ExpectedEditCheckPointForUniveralXMLImportedTransaction == null)
				|| ((ExpectedEditCheckPointForDirectEnteredTransaction != null && ExpectedEditCheckPointForUniveralXMLImportedTransaction != null) && (ExpectedEditCheckPointForDirectEnteredTransaction != ExpectedEditCheckPointForUniveralXMLImportedTransaction)));
		}

		protected void AssertCheckPointForEdit(InvoicingBase transaction, SecurityCheckpoint expectedCheckPoint)
		{
			var checkpoint = Controller.GetCheckPointForEdit(transaction);
			AssertNotNull("CheckPointForEdit should not be null", checkpoint);
			if (expectedCheckPoint != null)
			{
				AssertEquals("CheckPoints match", expectedCheckPoint, checkpoint);
			}
		}

		public void TestShowLoadedForm_EditAction()
		{
			var controller = Controller as TransactionControllerWithReadOnlyBehaviourControlledBySource;
			var transaction = TransactionImportedFromUniversalXML;
			AssertForShowLoadedForm_EditAction(transaction, ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction);

			transaction = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
			AssertForShowLoadedForm_EditAction(transaction, ExpectedEditHeaderCheckPointForDirectEnteredTransaction);

			Assert("Header Read Only CheckPoints should be overriden in pair", (ExpectedEditHeaderCheckPointForDirectEnteredTransaction == null && ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction == null)
				|| ((ExpectedEditHeaderCheckPointForDirectEnteredTransaction != null && ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction != null) && (ExpectedEditHeaderCheckPointForDirectEnteredTransaction != ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction)));
		}

		protected void AssertForShowLoadedForm_EditAction(InvoicingBase transaction, SecurityCheckpoint checkpoint)
		{
			IZForm form = null;
			if (checkpoint != null)
			{
				checkpoint.IsAllowed = true;
				using (form = Controller.ShowEditForm(transaction))
				{
					form.Show();
					AssertPropertiesVisibility(false);
				}

				checkpoint.IsAllowed = false;
				using (form = Controller.ShowEditForm(transaction))
				{
					form.Show();
					AssertPropertiesVisibility(true);
				}
			}
			else if (transaction != null)
			{
				using (form = Controller.ShowEditForm(transaction))
				{
					AssertNotNull("Form not null", form);
				}
			}

			void AssertPropertiesVisibility(bool isReadOnly)
			{
				var bizObj = (InvoicingBase)form.BusinessEntityForPersistingForm;

				if (form is BaseInvoicingForm)
				{
					Assert(!bizObj.AH_RequisitionDateInfo.ReadOnly);
					Assert(!bizObj.AH_RequisitionStatusInfo.ReadOnly);
					Assert(!bizObj.IsSelfBillingInvoiceInfo.ReadOnly);
					AssertEquals(isReadOnly, ((BaseInvoicingForm)form).InvoiceDetails.AddressWithContactControl.ReadOnly);
				}
				else if (form is TransactionPendingAllocationForm)
				{
					AssertEquals(isReadOnly, bizObj.AH_TransactionNumInfo.ReadOnly);
					AssertEquals(isReadOnly, bizObj.AH_InvoiceDateInfo.ReadOnly);
					AssertEquals(isReadOnly, bizObj.AH_OSExTaxAmountInfo.ReadOnly);
				}
				else
				{
					Assert("All form types should be covered by this unit test. Edit this unit test to cover your changes.", false);
				}
			}
		}

		public void TestNullFormWithEditAction_NoException()
		{
			var transaction = Factory.NewWithValidTestData(GetBusinessObjectType());
			var form = Controller.ShowEditForm(transaction);
			AssertNull("Form is null", form);
		}

		protected abstract InvoicingBase TransactionImportedFromUniversalXML { get; }

		protected virtual SecurityCheckpoint ExpectedEditCheckPointForDirectEnteredTransaction => null;

		protected virtual SecurityCheckpoint ExpectedEditCheckPointForUniveralXMLImportedTransaction => null;

		protected virtual SecurityCheckpoint ExpectedEditHeaderCheckPointForDirectEnteredTransaction => null;

		protected virtual SecurityCheckpoint ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction => null;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
