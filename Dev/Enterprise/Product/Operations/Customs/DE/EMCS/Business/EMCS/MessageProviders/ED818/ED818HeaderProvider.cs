using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED818HeaderProvider : HeaderProvider, IED818Header
	{
		public ED818HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IReportOfReceipt reportOfReceipt)
		: base(emcsJobDeclaration)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
			helper = new Message818HeaderProviderHelper(emcsJobDeclaration, reportOfReceipt);
		}
		readonly IReportOfReceipt reportOfReceipt;
		readonly Message818HeaderProviderHelper helper;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => ZInt.ParseSafe(emcsJobDeclaration.SequenceNumber, ZInt.Zero));
		CachedValue<int> sequenceNumberCache;

		public string DestinationOfficeReferenceNumber => helper.DestinationOfficeReferenceNumber;

		public DateTime? DateOfArrivalOfExciseProducts => helper.DateOfArrivalOfExciseProducts;

		public string GlobalConclusionOfReceipt => helper.GlobalConclusionOfReceipt;

		public bool IsReceiptPartiallyRefused => helper.IsReceiptPartiallyRefused;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(reportOfReceipt.ComplementaryInformation);

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress));
		IEMCSPartyConsignee consigneeTrader;

		public IEMCSPartyDeliveryPlace DeliveryPlaceTrader => deliveryPlaceTrader ?? (deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(emcsJobDeclaration.DestinationWarehouseDocumentaryAddress));
		IEMCSPartyDeliveryPlace deliveryPlaceTrader;

		public IReadOnlyCollection<IED818Line> Lines
		{
			get
			{
				if (lines == null)
				{
					if (reportOfReceipt.ReceiptResult == EMCSReceiptResultList.Codes.ReceiptAcceptedAndSatisfactory ||
						reportOfReceipt.ReceiptResult == EMCSReceiptResultList.Codes.ExitAcceptedAndSatisfactory)
					{
						lines = Array.Empty<IED818Line>();
					}
					else
					{
						lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Where(x => x.Outturn.ReportOfReceiptReasons.Any()).Select(x => new ED818LineProvider(x)).ToArray();
					}
				}
				return lines;
			}
		}
		IReadOnlyCollection<IED818Line> lines;
	}
}
