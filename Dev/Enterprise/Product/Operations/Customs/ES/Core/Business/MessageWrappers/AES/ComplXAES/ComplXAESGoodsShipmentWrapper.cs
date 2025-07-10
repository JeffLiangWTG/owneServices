using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ComplXAESGoodsShipmentWrapper : AESCommonGoodsShipmentWrapper, IComplXAESGoodsShipment
{
	public ComplXAESGoodsShipmentWrapper(CusEntryHeader entryHeader) : base(entryHeader, false, isComlpX: true)
	{
		invoiceHeader = entryHeader.RandomHeader;
	}
	readonly JobComInvoiceHeader invoiceHeader;

	public ZString NatureOfTransaction => invoiceHeader.JZ_ValuationCode;

	public ICommonDeliveryTerms DeliveryTerms => deliveryTerms ?? (deliveryTerms = new CommonDeliveryTermsWrapper(entryHeader));
	CommonDeliveryTermsWrapper deliveryTerms;

	public IComplXAESConsignment Consignment => consignment ?? (consignment = new ComplXAESConsignmentWrapper(entryHeader));
	ComplXAESConsignmentWrapper consignment;

	public IReadOnlyCollection<IComplXAESLine> Lines => lines ?? (lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new ComplXAESLineWrapper(x)).ToList().AsReadOnly());
	IReadOnlyCollection<ComplXAESLineWrapper> lines;
}
