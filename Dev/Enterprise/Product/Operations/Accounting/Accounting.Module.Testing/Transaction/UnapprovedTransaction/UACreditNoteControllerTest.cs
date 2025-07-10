using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(UACreditNoteController))]
	class UACreditNoteControllerTest : CreditNoteInvoiceControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.UACreditNote;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(UACreditNote);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(UACreditNoteForm);
		}

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
