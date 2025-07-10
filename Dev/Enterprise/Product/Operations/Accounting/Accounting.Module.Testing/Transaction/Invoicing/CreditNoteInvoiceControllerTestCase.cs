using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class CreditNoteInvoiceControllerTestCase : InvoicingBaseControllerTest
	{
		public void TestPreventCreationOfCreditNotes()
		{
			if (GetExpectedFormType() == typeof(CreditNoteForm))
			{
				AssertNotNull("PreventCreationOfCreditNotesRegistryConfiguration should be overridden", PreventCreationOfCreditNotesRegistryConfiguration);
				ZController controller = ZControllerFactory.Create(GetControllerID());
				using (PreventCreationOfCreditNotesRegistryConfiguration)
				using (IZForm testForm = controller.ShowNewForm())
				{
					var pattern = "Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> .* Defaults -> Default Settings -> Prevent Creation of Credit Notes.";
					AssertEquals("User notification", 1, new Regex(pattern).Matches(UnitTestNotification.LastMessage.Text).Count);
					AssertNull("Form should be null because creation is not allowed", testForm);
				}
			}
			else
			{
				AssertNull("PreventCreationOfCreditNotesRegistryConfiguration should not be overridden", PreventCreationOfCreditNotesRegistryConfiguration);
			}
		}

		protected virtual IDisposable PreventCreationOfCreditNotesRegistryConfiguration => null;

		public void TestSecurityOverrideProviderWhenReversingSingleBusinessObject()
		{
			Factory.Save();

			ZController controller = ZControllerFactory.Create(GetControllerID());
			using (var form = (BaseInvoicingForm)controller.ShowDeleteForm(ParentTransactionHeaderRow))
			{
				AssertSecurityOverrideProviders(form.BusinessEntity as InvoicingBase);

				AssertSecurityOverrideProviderTypes(form.BusinessEntity as InvoicingBase);
			}
		}

		public void TestSecurityOverrideProviderWhenReversingMultipleBusinessObjects()
		{
			var invoice = ParentTransactionHeaderRow as InvoicingBase;
			Factory.Save();

			invoice.GenerateReverseTransaction(false);

			var multipleReversingProvider = new MultipleReversingProviderForHeader();

			ZController controller = ZControllerFactory.Create(GetControllerID());

			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			controller.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

			AssertEquals("One transaction added to already reversed list", 1, multipleReversingProvider.TransactionsAlreadyReversed.Count);
			using ((MultipleReversingBaseForm)controller.LastShownForm)
			{
				var reversedTransaction = multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity as InvoicingBase;

				AssertSecurityOverrideProviders(reversedTransaction);

				AssertSecurityOverrideProviderTypes(reversedTransaction);
			}
		}

		protected virtual void AssertSecurityOverrideProviders(InvoicingBase businessObject)
		{
			AssertNotNull("Security override provider for Original Transaction should be set", SecurityOverrideProviderSource.Get(businessObject.OriginalTransaction).Provider);
			AssertNotNull("Security override provider for Reversed Transaction should be set", SecurityOverrideProviderSource.Get(businessObject).Provider);

			AssertEquals("Security override provider of Original and Reversed transactions are the same"
				, SecurityOverrideProviderSource.Get(businessObject.OriginalTransaction).Provider
				, SecurityOverrideProviderSource.Get(businessObject).Provider);
		}

		protected virtual void AssertSecurityOverrideProviderTypes(InvoicingBase businessObject)
		{
			AssertEquals(typeof(InvoicingSecurityOverrideProvider), SecurityOverrideProviderSource.Get(businessObject.OriginalTransaction).Provider.GetType());
			AssertEquals(typeof(InvoicingSecurityOverrideProvider), SecurityOverrideProviderSource.Get(businessObject).Provider.GetType());
		}

		protected virtual CreditNoteInvoiceController GetController()
		{
			return null;
		}
		protected virtual bool IsNewAllowed
		{
			get { return false; }
			set { }
		}

		public void TestSecurityWhenAmendingPeriodicInvoice()
		{
			CreditNoteInvoiceController contr = GetController();
			if (contr == null)
			{
				Assert(true);
				return; //as APControllers do not override it and we do not test them
			}
			AssertNotNull(contr.AmendSecurityCheckPointCode_ForTestOnly);

			#region Create Shipment, Job, Charge, Invoice
			var date = new ZDateTime(2015, 07, 28);

			TestObjectCreator toCreator = new TestObjectCreator(Factory);

			var shipment1 = toCreator.CreateShipment("S00001");
			var job1 = toCreator.CreateJob(shipment1, false);

			var charge1 = toCreator.CreateCharge(job1, toCreator.CC3, 0M, 600M);
			charge1.JR_InvoiceType = "FID";

			var shipment2 = toCreator.CreateShipment("S00002");
			var job2 = toCreator.CreateJob(shipment2, false);

			var charge2 = toCreator.CreateCharge(job2, toCreator.CC3, 0M, 600M);
			charge2.JR_InvoiceType = "FID";

			Factory.Save();

			var invoice = toCreator.CreateARInvoice<ARInvoice>("00005000", toCreator.AUD, 1.0m, toCreator.ABIGAS);
			invoice.AH_TransactionCategory = "FID";

			var line1 = toCreator.CreateARInvoiceLine(invoice, job1, toCreator.CC3, toCreator.AUD, 1.0m, "ARInv1", 600m);
			var line2 = toCreator.CreateARInvoiceLine(invoice, job2, toCreator.CC3, toCreator.AUD, 1.0m, "ARInv2", 600m);

			charge1.ReverseWIP(date);
			charge2.ReverseWIP(date);
			charge1.JR_AL_ARLine = line1.PK;
			charge2.JR_AL_ARLine = line2.PK;

			Factory.Save();
			#endregion

			var amending = (invoice as IAmending).GenerateAmendingTransaction(TransactionTypes.Invoice);
			var amendingAsInvoice = (InvoicingBase)amending;

			var securityTestHelperJob1 = new JobInvoicingSecurityHelper(shipment1.InvoicingSupporter.JobInvoicingSecurity);
			var securityTestHelperJob2 = new JobInvoicingSecurityHelper(shipment2.InvoicingSupporter.JobInvoicingSecurity);
			var amendWCreditOrInvoiceJob1 = securityTestHelperJob1.GetInvSecurity(contr.AmendSecurityCheckPointCode_ForTestOnly);
			var amendWCreditOrInvoiceJob2 = securityTestHelperJob2.GetInvSecurity(contr.AmendSecurityCheckPointCode_ForTestOnly);

			var temp = this.IsNewAllowed;
			var tempAmendJob1 = amendWCreditOrInvoiceJob1.IsAllowed;
			var tempAmendJob2 = amendWCreditOrInvoiceJob2.IsAllowed;

			try
			{
				this.IsNewAllowed = false;
				amendWCreditOrInvoiceJob1.IsAllowed = true;
				amendWCreditOrInvoiceJob2.IsAllowed = true;
				var checkPoint = contr.GetCheckPointForNew(amendingAsInvoice);
				AssertEquals("Amendment is allowed", true, checkPoint.IsAllowed);

				this.IsNewAllowed = false;
				amendWCreditOrInvoiceJob1.IsAllowed = true;
				amendWCreditOrInvoiceJob2.IsAllowed = false;
				checkPoint = contr.GetCheckPointForNew(amendingAsInvoice);
				AssertEquals("Amendment is allowed", false, checkPoint.IsAllowed);

				this.IsNewAllowed = true;
				amendWCreditOrInvoiceJob1.IsAllowed = true;
				amendWCreditOrInvoiceJob2.IsAllowed = true;
				checkPoint = contr.GetCheckPointForNew(amendingAsInvoice);
				AssertEquals("Amendment is not allowed", true, checkPoint.IsAllowed);

				this.IsNewAllowed = true;
				amendWCreditOrInvoiceJob1.IsAllowed = true;
				amendWCreditOrInvoiceJob2.IsAllowed = false;
				checkPoint = contr.GetCheckPointForNew(amendingAsInvoice);
				AssertEquals("Amendment is not allowed", false, checkPoint.IsAllowed);
			}
			finally
			{
				this.IsNewAllowed = temp;
				amendWCreditOrInvoiceJob1.IsAllowed = tempAmendJob1;
				amendWCreditOrInvoiceJob2.IsAllowed = tempAmendJob2;
			}
		}

		public void TestSecurityOverrideWhenAmending()
		{
			CreditNoteInvoiceController contr = GetController();
			if (contr == null)
			{
				Assert(true);
				return; //as APControllers do not override it and we do not test them
			}
			AssertNotNull(contr.AmendSecurityCheckPointCode_ForTestOnly);
			TestObjectCreator toCreator = new TestObjectCreator(Factory);
			var shipment = toCreator.CreateShipment("S0001");
			var job = toCreator.CreateJob(shipment, false);
			var invoice = toCreator.CreateARInvoice<ARInvoice>("00004000", toCreator.AUD, 1.0m, toCreator.ABIGAS);
			invoice.AH_JH = job.PK;
			Factory.Save();

			var amending = (invoice as IAmending).GenerateAmendingTransaction(TransactionTypes.Invoice);

			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			var amendWCreditOrInvoice = securityTestHelper.GetInvSecurity(contr.AmendSecurityCheckPointCode_ForTestOnly);
			var temp = this.IsNewAllowed;
			var tempAmend = amendWCreditOrInvoice.IsAllowed;
			try
			{
				this.IsNewAllowed = false;
				amendWCreditOrInvoice.IsAllowed = true;
				var checkPoint = contr.GetCheckPointForNew((InvoicingBase)amending);
				AssertEquals(checkPoint.Code, amendWCreditOrInvoice.Code);
				Assert(checkPoint.IsAllowed);

				this.IsNewAllowed = true;
				amendWCreditOrInvoice.IsAllowed = false;
				checkPoint = contr.GetCheckPointForNew((InvoicingBase)amending);
				AssertEquals("Amendment is not allowed even if New does", false, checkPoint.IsAllowed);

				this.IsNewAllowed = false;
				amendWCreditOrInvoice.IsAllowed = false;
				Assert(!contr.GetCheckPointForNew((InvoicingBase)amending).IsAllowed);
			}
			finally
			{
				this.IsNewAllowed = temp;
				amendWCreditOrInvoice.IsAllowed = tempAmend;
			}
		}

		public void TestSecurityOverrideWhenAmendingConsol()
		{
			CreditNoteInvoiceController contr = GetController();
			if (contr == null)
			{
				Assert(true);
				return; //as APControllers do not override it and we do not test them
			}
			AssertNotNull(contr.AmendSecurityCheckPointCode_ForTestOnly);

			TestObjectCreator toCreator = new TestObjectCreator(Factory);
			var consol = toCreator.CreateConsol("SYD", "BKK", "S0001");
			var invoice = toCreator.CreateARInvoice<ARInvoice>("00004000", toCreator.AUD, 1.0m, toCreator.ABIGAS);
			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			Factory.Save();

			var amending = (invoice as IAmending).GenerateAmendingTransaction(TransactionTypes.Invoice);

			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainConsolJobInvoicing);
			var amendWCreditOrInvoice = securityTestHelper.GetInvSecurity(contr.AmendSecurityCheckPointCode_ForTestOnly);
			var temp = this.IsNewAllowed;
			var tempAmend = amendWCreditOrInvoice.IsAllowed;
			try
			{
				this.IsNewAllowed = false;
				amendWCreditOrInvoice.IsAllowed = true;
				var checkPoint = contr.GetCheckPointForNew((InvoicingBase)amending);
				AssertEquals(checkPoint.Code, amendWCreditOrInvoice.Code);
				Assert(checkPoint.IsAllowed);
				this.IsNewAllowed = false;
				amendWCreditOrInvoice.IsAllowed = false;
				AssertEquals(false, contr.GetCheckPointForNew((InvoicingBase)amending).IsAllowed);
			}
			finally
			{
				this.IsNewAllowed = temp;
				amendWCreditOrInvoice.IsAllowed = tempAmend;
			}
		}
	}
}
