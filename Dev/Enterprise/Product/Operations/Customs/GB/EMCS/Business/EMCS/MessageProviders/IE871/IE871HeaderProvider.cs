using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE871HeaderProvider : HeaderProvider, IIE871Header
	{
		public IE871HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, ZString reasonForShortage) : base(emcsJobDeclaration)
		{
			this.reasonForShortage = Argument.NotNull(reasonForShortage, nameof(reasonForShortage));
		}
		readonly ZString reasonForShortage;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => emcsJobDeclaration.SequenceNumber == "0" ? DefaultSequenceNumber : ZInt.ParseSafe(emcsJobDeclaration.SequenceNumber, DefaultSequenceNumber));
		CachedValue<int> sequenceNumberCache;

		public string SubmitterType => emcsJobDeclaration.JE_DeclarantType;

		public DateTime DateOfAnalysis => ZDateTime.UtcNow.ToDateTime();

		public ITextAndLanguage GlobalExplanation => GetComplementaryInformation(reasonForShortage);

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (emcsJobDeclaration.IsConsignee ? consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress) : null);
		IEMCSPartyConsignee consigneeTrader;

		public IEMCSPartyConsignor ConsignorTrader => consignorTrader ?? (emcsJobDeclaration.IsConsignor ? consignorTrader = PartyConsignorProvider.NewOrNull(emcsJobDeclaration.SupplierDocumentaryAddress) : null);
		IEMCSPartyConsignor consignorTrader;

		public IReadOnlyCollection<IIE871Line> Lines => lines ?? (lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Where(x => CanSendLine(x)).Select(x => new IE871LineProvider(x)).ToArray());
		IReadOnlyCollection<IIE871Line> lines;

		public DateTime? DateAndTimeOfValidationOfExplanationOnShortage => this.IsValidationAttributeAllowed ? ZDateTime.Now.ToDateTime().ToUnspecifiedKindWithSecondsPrecision() : null;

		bool CanSendLine(EMCSJobComInvoiceLine invoiceLine) => !invoiceLine.Outturn.ObservedDifference.IsEmpty && !invoiceLine.Outturn.C5_OutturnResultReason.IsEmpty;
	}
}
