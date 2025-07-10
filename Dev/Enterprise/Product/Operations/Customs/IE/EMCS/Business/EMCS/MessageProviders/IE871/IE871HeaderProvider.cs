using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE871HeaderProvider : HeaderProvider, IIE871Header
	{
		public IE871HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, ZString reasonForShortage) : base(emcsJobDeclaration)
		{
			this.reasonForShortage = Argument.NotNull(reasonForShortage, nameof(reasonForShortage));
			helper = new Message871HeaderProviderHelper(emcsJobDeclaration);
		}
		readonly ZString reasonForShortage;
		readonly Message871HeaderProviderHelper helper;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumber, () => int.TryParse(emcsJobDeclaration.SequenceNumber, out var result) ? result : 0);
		CachedValue<int> sequenceNumber;

		public string SubmitterType => helper.SubmitterType;

		public DateTime? DateOfAnalysis => ZDateTime.UtcNow.ToDateTime();

		public ITextAndLanguage GlobalExplanation => GetComplementaryInformation(reasonForShortage);

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (emcsJobDeclaration.IsConsignee ? consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress) : null);
		IEMCSPartyConsignee consigneeTrader;

		public IEMCSPartyConsignor ConsignorTrader => consignorTrader ?? (emcsJobDeclaration.IsConsignor ? consignorTrader = PartyConsignorProvider.NewOrNull(emcsJobDeclaration.SupplierDocumentaryAddress) : null);
		IEMCSPartyConsignor consignorTrader;

		public IReadOnlyCollection<IIE871Line> Lines => lines ?? (lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Where(x => helper.CanSendLine(x)).Select(x => new IE871LineProvider(x)).ToArray());
		IReadOnlyCollection<IIE871Line> lines;

		public DateTime? DateAndTimeOfValidationOfExplanationOnShortage => null;
	}
}
