using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(APIncompleteInvoicesController))]
	public class APIncompleteInvoicesControllerTest : APIncompleteTransactionsWithApprovalRequestsControllerTest<APInvoice, InvoiceForm>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APIncompleteInvoice;
		}

		protected override ControllerID GetRelativeControllerID()
		{
			return ControllerIDs.APInvoice;
		}

		protected override string ExpectedTransactionType
		{
			get { return "AP Invoice"; }
		}

		public void TestControllerCatchesMutexErrorWhenOpeningIncompleteInvoice()
		{
			var anotherFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(anotherFactory);
			var consol = creator.CreateConsol("KRSEL", "AUSYD", "C1");
			var shipment1 = creator.CreateShipment("S1", consol);
			var job1 = creator.CreateJob(shipment1, false);
			anotherFactory.Save();

			var invoice = GetBusinessObject();
			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 200m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.SetIsUsedForApportionment();
			invoice.ImportAllApportionmentsFromCosting();
			SaveAsIncomplete(invoice);

			var shipment2 = creator.CreateShipment("S2", consol);
			anotherFactory.Save();

			var lockingFactory = new BusinessObjectFactory();
			var loader = new Job.Loader(lockingFactory, lockingFactory.Load<ForwardingShipment>(shipment2.PK));
			var job2 = loader.TryCreateWithMutex();
			try
			{
				using (ZArchitecture.GUI.IZForm form = Controller.ShowEditForm(invoice))
				{
					AssertMutexError();
				}
			}
			finally
			{
				job2.Dispose();
			}
		}

		protected override ZController GetController() => Controller as APIncompleteInvoicesController;

		protected override IEnumerable<ControllerID> EditFormControllerIDs => new[] { ControllerIDs.APIncompleteInvoice, ControllerIDs.APInvoiceLinkedToApproval };

		protected override ControllerID NewFormControllerID => ControllerIDs.APInvoiceNewForApproval;

		protected virtual void AssertMutexError()
		{
			AssertEquals(@"You have created the job S2 on another form, but haven't saved it yet.
Please close or save other forms that use job S2 to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void AssertOtherGetValidControllerIDCase(INavigationControllerIDProvider provider)
		{
			base.AssertOtherGetValidControllerIDCase(provider);

			var creditNote = Factory.NewWithValidTestData<APCreditNote>();

			AssertEquals("ControllerID for AP Credit Note bizO should be APCreditNote.", ControllerIDs.APCreditNote, provider.GetValidControllerID(creditNote));
		}
	}
}
