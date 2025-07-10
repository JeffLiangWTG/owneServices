using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideInvoiceAddressContactValidationTest : OverrideInvoiceDetailValidationTest
	{
		public void TestCheckDisplayInvoiceAddressOverride()
		{
			InvoicingBase header = (InvoicingBase)Factory.New(InvoiceType);
			header.AH_OH = Creator.ABIGAS.PK;

			header.SetContext(BusinessContext.OverrideInvoiceAddressContact);
			OverrideInvoiceAddressContactValidation testValidation = (OverrideInvoiceAddressContactValidation)header.Validation;
			header.DisplayInvoiceAddressOverride = ZGuid.Invalid;
			AssertHasErrors(header.DisplayInvoiceAddressOverrideInfo);

			var overrideAddress = Creator.CreateAddress(Creator.ABIGAS);
			header.DisplayInvoiceAddressOverride = overrideAddress.PK;
			AssertNoErrors(header.DisplayInvoiceAddressOverrideInfo);
		}

		public void TestCheckDisplayInvoiceConactOverride()
		{
			InvoicingBase header = (InvoicingBase)Factory.New(InvoiceType);
			header.AH_OH = Creator.ABIGAS.PK;

			header.SetContext(BusinessContext.OverrideInvoiceAddressContact);
			OverrideInvoiceAddressContactValidation testValidation = (OverrideInvoiceAddressContactValidation)header.Validation;
			header.DisplayInvoiceContactOverride = ZGuid.Invalid;
			AssertHasErrors(header.DisplayInvoiceContactOverrideInfo);

			var overrideContact = Creator.CreateContact(Creator.ABIGAS);
			header.DisplayInvoiceContactOverride = overrideContact.PK;
			AssertNoErrors(header.DisplayInvoiceContactOverrideInfo);
		}

		protected override OverrideInvoiceDetailValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as OverrideInvoiceAddressContactValidation;
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(ARInvoice);
			}
		}

		protected override void SetBusinessContext(InvoicingBase invoice)
		{
			invoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);
		}

		TestObjectCreator fCreator;
		public TestObjectCreator Creator
		{
			get
			{
				return fCreator ?? (fCreator = new TestObjectCreator(Factory));
			}
		}
	}
}