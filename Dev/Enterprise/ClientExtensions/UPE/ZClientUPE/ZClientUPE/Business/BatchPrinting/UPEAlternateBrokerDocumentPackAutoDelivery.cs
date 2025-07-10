using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Client.UPE.Business
{
	public class UPEAlternateBrokerDocumentPackAutoDelivery : UPEDocumentAutoDelivery
	{
		public UPEAlternateBrokerDocumentPackAutoDelivery(UPEJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerDocumentPack(); }
		}

		protected override ZString PrintBatchType
		{
			get { return DisableQueueForBatchPrint; }
		}

		protected override string DeliveryFailureEmailSubject
		{
			get { return "Delivery instructions incomplete for Alternate Broker Documents - " + Declaration.AlternateBroker.OH_Code; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override string DeliveryFailureDocumentDetails
		{
			get { return "CargoWise One Code : " + Declaration.AlternateBroker.OH_Code; }
		}

		UPEJobDeclaration Declaration
		{
			get { return (UPEJobDeclaration)base.DocumentSupportable; }
		}

		public override void Deliver()
		{
			base.Deliver();
			UPEJobDeclarationDocumentSupporter documentSupporter = (UPEJobDeclarationDocumentSupporter)Declaration.DocumentSupporter;
			documentSupporter.AddCommercialInvoiceNotAvailableNoteIfRequired();
		}
	}
}
