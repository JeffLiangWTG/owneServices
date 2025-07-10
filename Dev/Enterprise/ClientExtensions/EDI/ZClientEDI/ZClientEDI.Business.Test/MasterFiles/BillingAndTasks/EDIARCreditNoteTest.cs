using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.Base.Interfaces;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDIARCreditNote))]
	public class EDIARCreditNoteTest : ARCreditNoteTest
	{
		public void TestTypeDecidingTheCorrectType()
		{
			var invoice = Factory.NewWithValidTestData<EDIARCreditNote>();
			Factory.Save();

			BusinessObject newInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			AssertEquals("If this fails, check the base class. It has to be subclassed from AR CreditNote", typeof(EDIARCreditNote), newInvoice.GetType());
		}

		[TestedType(typeof(EDIARCreditNote))]
		public class EDIARCreditNoteMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<EDIARCreditNote>();
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EDIARCreditNote>();
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(EDICreditNoteValidation); }
		}

		#endregion
	}

	[TestedType(typeof(EDIARCreditNote))]
	public class EDIARCreditNoteIUnmatchDateSupporterTest : ARCreditNoteIUnmatchDateSupporterTest
	{
		protected override ITransaction GetNewObject()
		{
			return Factory.New<EDIARCreditNote>();
		}
	}
}
