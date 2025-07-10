using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	class UPETaxInvoiceAutoDeliveryTest : UPEDocumentAutoDeliveryTest
	{
		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new UPETaxInvoiceAutoDelivery(Callout);
		}

		protected override IUPEDocumentSupportable NewDocumentSupportable()
		{
			Callout result = Factory.NewWithValidTestData<Callout>();
			result.CS_HAWB = "HAWB";
			result.BillToAccountNumber = "BillToAccountNum";
			OrgCusCode accountNumber = DeliveryOrganisation.CustomsCodes.AddNew();
			accountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			accountNumber.OK_CustomsRegNo = "BillToAccountNum";
			DeliveryOrganisation.OH_FullName = "Bill-to Party";
			return result;
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return DocumentLoader.LoadUPSTaxInvoice();
			}
		}

		protected override ZString ExpectedPrintBatchType
		{
			get
			{
				return UPEPrintBatchTypes.Codes.TaxInvoice;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailSubject
		{
			get
			{
				return "Delivery instructions incomplete for bill to party 'BillToAccountNum'";
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return @"
Delivery instructions incomplete for Tax Invoice; Generated 11-Nov-05 00:00:00

HAWB              : HAWB
Bill To Name      : Bill-to Party
Bill To Account # : BillToAccountNum

Error: DeliveryAddress: Please enter a Fax Number.
";
			}
		}

		protected Callout Callout
		{
			get
			{
				return (Callout)DocumentSupportable;
			}
		}
	}
}
