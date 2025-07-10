using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationDrawbackProvider : IDeclarationDrawback
	{
		public DeclarationDrawbackProvider(SuspensionDrawback drawback)
		{
			this.drawback = Argument.NotNull(drawback, nameof(drawback));
		}
		readonly SuspensionDrawback drawback;

		public string CategoryCode => drawback.CSI_SubType;
		public string ID => drawback.CSI_ReferenceNumber2;
		public string DrawbackHsClassification => drawback.CSI_Tariff;
		public string DrawbackRecipientId => drawback.CSI_ReferenceNumber;
		public decimal ValueWithoutExchangeCoverAmount => decimal.Zero;
		public decimal ValueWithExchangeCoverAmount => drawback.CSI_Value;
		public int ItemID => drawback.CSI_LineNo;
		public decimal Quantity => drawback.CSI_Quantity;
		public IEnumerable<IDeclarationDrawbackInvoice> Invoices
		{
			get
			{
				if (invoices == null)
				{
					invoices = new List<IDeclarationDrawbackInvoice>();
					foreach (var drawbackInvoice in drawback.SuspensionDrawbackInvoiceCollection.Cast<SuspensionDrawbackInvoice>())
					{
						invoices.Add(new DeclarationDrawbackInvoiceProvider(drawbackInvoice));
					}
				}
				return invoices;
			}
		}
		List<IDeclarationDrawbackInvoice> invoices;

		public IEnumerable<IDeclarationDrawbackPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = new List<IDeclarationDrawbackPreviousDocument>();
					foreach (var drawbackPreviousDoc in drawback.SuspensionDrawbackImportEntryDocumentCollection.Cast<SuspensionDrawbackImportEntryDocument>())
					{
						previousDocuments.Add(new DeclarationDrawbackPreviousDocumentProvider(drawbackPreviousDoc));
					}
				}
				return previousDocuments;
			}
		}
		List<IDeclarationDrawbackPreviousDocument> previousDocuments;
	}
}

