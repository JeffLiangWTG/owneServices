using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentWrapper : IDeclarationGoodsShipment
	{
		DeclarationGoodsShipmentWrapper(CusEntryInstruction entryInstruction, JobComInvoiceHeader invoiceHeader)
		{
			this.entryInstruction = entryInstruction;
			this.invoiceHeader = invoiceHeader;
		}

		internal static DeclarationGoodsShipmentWrapper NewOrNull(CusEntryInstruction entryInstruction, JobComInvoiceHeader invoiceHeader)
			=> entryInstruction == null || invoiceHeader == null ? null : new DeclarationGoodsShipmentWrapper(entryInstruction, invoiceHeader);

		#region IDeclarationGoodsShipment

		ICollection<IDeclarationGoodsShipmentAdditionalDocument> IDeclarationGoodsShipment.AdditionalDocument => null;

		ICollection<IDeclarationGoodsShipmentConsignment> IDeclarationGoodsShipment.Consignment
		{
			get
			{
				if (((IDeclarationGoodsShipment)this).SequenceNumeric != 1)
				{
					return null;
				}
				var collection = new Collection<IDeclarationGoodsShipmentConsignment>();
				collection.Add(DeclarationGoodsShipmentConsignmentWrapper.NewOrNull(entryInstruction, invoiceHeader.JobDeclaration));
				return collection;
			}
		}

		ICollection<IDeclarationGoodsShipmentCustomsValuation> IDeclarationGoodsShipment.CustomsValuation
			=> invoiceHeader.Charges.Select(c => DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(c)).ToArray();

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItem> IDeclarationGoodsShipment.GovernmentAgencyGoodsItem =>
			invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Select(invoiceLine => DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper.NewOrNull(invoiceLine)).ToArray();

		IDeclarationGoodsShipmentInvoice IDeclarationGoodsShipment.Invoice => DeclarationGoodsShipmentInvoiceWrapper.NewOrNull(invoiceHeader);

		decimal? IDeclarationGoodsShipment.SequenceNumeric => invoiceHeader.JZ_InvoiceDisplaySequence;

		IDeclarationGoodsShipmentSupplier IDeclarationGoodsShipment.Supplier => DeclarationGoodsShipmentSupplierWrapper.NewOrNull(invoiceHeader.Supplier);

		IDeclarationGoodsShipmentTradeTerms IDeclarationGoodsShipment.TradeTerms => DeclarationGoodsShipmentTradeTermsWrapper.NewOrNull(invoiceHeader);

		ICollection<IDeclarationGoodsShipmentUniqueConsignmentReference> IDeclarationGoodsShipment.Ucr => GetUcr(invoiceHeader);

		#endregion

		ICollection<IDeclarationGoodsShipmentUniqueConsignmentReference> GetUcr(JobComInvoiceHeader invoiceHeader)
		{
			var declarationGoodsShipmentUcrs = new List<IDeclarationGoodsShipmentUniqueConsignmentReference>();
			if (DeclarationGoodsShipmentUcrWrapper.NewOrNull(invoiceHeader) is DeclarationGoodsShipmentUcrWrapper declarationGoodsShipmentUcrWrapper)
			{
				declarationGoodsShipmentUcrs.Add(declarationGoodsShipmentUcrWrapper);
			}
			return declarationGoodsShipmentUcrs;
		}

		readonly JobComInvoiceHeader invoiceHeader;
		readonly CusEntryInstruction entryInstruction;
	}
}
