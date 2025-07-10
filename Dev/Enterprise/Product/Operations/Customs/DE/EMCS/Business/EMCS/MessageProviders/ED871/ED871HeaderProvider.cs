using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED871HeaderProvider : HeaderProvider, IED871Header
	{
		public ED871HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, ZString reasonForShortage)
		: base(emcsJobDeclaration)
		{
			this.reasonForShortage = Argument.NotNull(reasonForShortage, nameof(reasonForShortage));
			helper = new Message871HeaderProviderHelper(emcsJobDeclaration);
		}
		readonly ZString reasonForShortage;
		readonly Message871HeaderProviderHelper helper;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumber, () => int.TryParse(emcsJobDeclaration.SequenceNumber, out var result) ? result : 0);
		CachedValue<int> sequenceNumber;

		public string SubmitterType => helper.SubmitterType;

		public IDateAndTime DateOfAnalysisCET => dateOfAnalysisCET ?? (dateOfAnalysisCET = new CentralEuropeanStandardDateAndTimeProvider());
		IDateAndTime dateOfAnalysisCET;

		public ITextAndLanguage GlobalExplanation => GetComplementaryInformation(reasonForShortage);

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (emcsJobDeclaration.IsConsignee ? consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress) : null);
		IEMCSPartyConsignee consigneeTrader;

		public IEMCSPartyConsignor ConsignorTrader => consignorTrader ?? (emcsJobDeclaration.IsConsignor ? consignorTrader = PartyConsignorProvider.NewOrNull(emcsJobDeclaration.SupplierDocumentaryAddress) : null);
		IEMCSPartyConsignor consignorTrader;

		public IReadOnlyCollection<IED871Line> Lines => lines ?? (lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Where(x => helper.CanSendLine(x)).Select(x => new ED871LineProvider(x)).ToArray());
		IReadOnlyCollection<IED871Line> lines;
	}
}
