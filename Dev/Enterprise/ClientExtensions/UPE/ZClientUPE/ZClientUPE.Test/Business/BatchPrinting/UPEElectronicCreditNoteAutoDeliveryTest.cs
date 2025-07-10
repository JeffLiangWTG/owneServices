using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Client.UPE.Business.Testing
{
	sealed class UPEElectronicCreditNoteAutoDeliveryTest : UPEDocumentAutoDeliveryTest
	{
		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new UPEElectronicCreditNoteAutoDeliveryDummyFail(Declaration);
		}

		protected override IUPEDocumentSupportable NewDocumentSupportable()
		{
			UPEJobDeclaration result = Factory.NewWithValidTestData<UPEJobDeclaration>();
			result.JE_DeclarationReference = "DEC#0001";
			return result;
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadElectronicCreditNote();
			}
		}

		protected override ZString ExpectedPrintBatchType
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailSubject
		{
			get
			{
				return "Delivery Electronic Credit Note failed for Declaration " + ((UPEJobDeclaration)DocumentSupportable).JE_DeclarationReference;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return string.Format(@"Delivery instructions incomplete for Electronic Credit Note; Generated 11-Nov-05 00:00:00

Delivery Electronic Credit Note failed for Declaration {0}

Error: DeliveryMethod: Please enter a value.
Error: DeliveryMethodDescription: Please enter a value.", ((UPEJobDeclaration)DocumentSupportable).JE_DeclarationReference);
			}
		}

		UPEJobDeclaration Declaration
		{
			get
			{
				return (UPEJobDeclaration)DocumentSupportable;
			}
		}
	}
}
