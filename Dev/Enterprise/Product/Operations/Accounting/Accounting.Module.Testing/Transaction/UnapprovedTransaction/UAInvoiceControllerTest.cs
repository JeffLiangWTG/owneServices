using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(UAInvoiceController))]
	class UAInvoiceControllerTest : CreditNoteInvoiceControllerTestCase
	{
		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.APUnapprovedInvoicesCancel; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.APUnapprovedInvoicesNew; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.APUnapprovedInvoices; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		public void TestDefaultChargeCodeBehaviour()
		{
			OrgHeader orgWithDefaultCharge = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Margin;
			charge.AC_AG_CostAccount = ObjectCreator.GLHeader1.PK;
			orgWithDefaultCharge.CompanyData.OB_AC_APDefaultChargeCode = charge.PK;
			OrgHeader orgWithoutDefaultCharge = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			UAInvController = Controller as UAInvoiceController;
			using (var form = (InvoiceForm)UAInvController.ShowNewForm())
			{
				var invoice = (APInvoice)form.BusinessEntity;
				invoice.AH_OH = orgWithDefaultCharge.PK;
				AssertEquals("Creates default charge line", 1, invoice.Lines.Count);
				AssertEquals("Creates default charge code", charge.PK, invoice.Lines[0].AL_AC);
			}
		}

		TestObjectCreator fObjectCreator;
		protected TestObjectCreator ObjectCreator
		{
			get { return fObjectCreator ?? (fObjectCreator = new TestObjectCreator(Factory)); }
		}

		UAInvoiceController UAInvController;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.UAInvoice;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(UAInvoice);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(UAInvoiceForm);
		}

		protected override bool ShouldHaveReversedBizoForTest
		{
			get { return false; }
		}

		protected override void AssertSecurityOverrideProviders(InvoicingBase businessObject)
		{
			AssertNotNull("Security override provider for Reversed Transaction should be set", SecurityOverrideProviderSource.Get(businessObject).Provider);
		}

		protected override void AssertSecurityOverrideProviderTypes(InvoicingBase businessObject)
		{
			AssertEquals(typeof(InvoicingSecurityOverrideProvider), SecurityOverrideProviderSource.Get(businessObject).Provider.GetType());
		}
	}
}
