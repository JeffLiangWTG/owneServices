using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE818HeaderProvider : HeaderProvider, IIE818Header
	{
		public IE818HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IReportOfReceipt reportOfReceipt) : base(emcsJobDeclaration)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly IReportOfReceipt reportOfReceipt;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => emcsJobDeclaration.SequenceNumber == "0" ? DefaultSequenceNumber : ZInt.ParseSafe(emcsJobDeclaration.SequenceNumber, DefaultSequenceNumber));
		CachedValue<int> sequenceNumberCache;

		public string DestinationOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);

		public DateTime? DateOfArrivalOfExciseProducts => reportOfReceipt.ArrivalDate.ToNullableDateTime();

		public string GlobalConclusionOfReceipt => reportOfReceipt.ReceiptResult;

		public bool IsReceiptPartiallyRefused => reportOfReceipt.ReceiptResult == EMCSReceiptResultList.Codes.ReceiptPartiallyRefused;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(reportOfReceipt.ComplementaryInformation);

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress));
		IEMCSPartyConsignee consigneeTrader;

		public IEMCSPartyDeliveryPlace DeliveryPlaceTrader => deliveryPlaceTrader ?? (deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(emcsJobDeclaration.DestinationWarehouseDocumentaryAddress));
		IEMCSPartyDeliveryPlace deliveryPlaceTrader;

		public IReadOnlyCollection<IIE818Line> Lines
		{
			get
			{
				if (lines == null)
				{
					if (reportOfReceipt.ReceiptResult == EMCSReceiptResultList.Codes.ReceiptAcceptedAndSatisfactory ||
						reportOfReceipt.ReceiptResult == EMCSReceiptResultList.Codes.ExitAcceptedAndSatisfactory)
					{
						lines = Array.Empty<IIE818Line>();
					}
					else
					{
						lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Where(x => x.Outturn.ReportOfReceiptReasons.Any()).Select(x => new IE818LineProvider(x)).ToArray();
					}
				}
				return lines;
			}
		}
		IReadOnlyCollection<IIE818Line> lines;

		public DateTime? DateAndTimeOfValidationOfReportOfReceiptExport => this.IsValidationAttributeAllowed ? ZDateTime.Now.ToDateTime().ToUnspecifiedKindWithSecondsPrecision() : null;
	}
}
