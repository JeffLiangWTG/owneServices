using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CUSWATLineProvider : ImportDecLineProvider, ICUSWATLine
	{
		public CUSWATLineProvider(CusEntryLine entryLine)
			: base(entryLine)
		{
		}

		public string ArticleNumber => RandomInvoiceLine.JI_PartNo;

		public string DepartureCountry => RandomInvoiceLine.JI_RN_NKCountryOfExport;

		public DateTime? DecisiveDate => RandomInvoiceLine.JI_CustomDate1.ToNullableDateTime();

		public bool ContainerFlag => ContainerNumbersLinkedToRandomInvoiceLine.Length > 0;

		public IReadOnlyCollection<string> ContainerIdentificationNumbers => containerIdentificationNumbers ?? (containerIdentificationNumbers = ContainerNumbersLinkedToRandomInvoiceLine);
		IReadOnlyCollection<string> containerIdentificationNumbers;

		public IAmount InwardMovementAmount => CachedValueHelper.GetValue(ref inwardMovementAmount, () => new AmountProvider(RandomInvoiceLine.JI_BondedWhsQuantity, RandomInvoiceLine.JI_BondedWhsUnitQty));
		CachedValue<IAmount> inwardMovementAmount;

		public string RequestedPreferentialTreatment => RandomInvoiceLine.JI_PrimaryPreference;

		public string InwardMovementDepartureCustomsWarehouseReferenceNumber => EntryInstructionPrevDocLinkedToRandomInvoiceLine?.CSI_ReferenceNumber;

		public int InwardMovementDepartureCustomsWarehouseSequenceNumber => EntryInstructionPrevDocLinkedToRandomInvoiceLine?.CSI_LineNo ?? 0;

		public bool InwardMovementDepartureCustomsWarehouseAccessViaAtlasFlag => EntryInstructionPrevDocLinkedToRandomInvoiceLine?.Status ?? false;

		public bool InwardMovementDepartureCustomsWarehouseUsualProcessingFlag => EntryInstructionPrevDocLinkedToRandomInvoiceLine?.UsualProcessingFlag ?? false;

		public string InwardMovementDepartureCustomsWarehouseAdditionalInformation => EntryInstructionPrevDocLinkedToRandomInvoiceLine?.CSI_Description;

		public IAmount InwardMovementDepartureCustomsWarehouseDebitAmount => CachedValueHelper.GetValue(ref inwardMovementDepartureCustomsWarehouseDebitAmount,
			() => new AmountProvider(EntryInstructionPrevDocLinkedToRandomInvoiceLine?.CSI_Quantity2 ?? decimal.Zero, EntryInstructionPrevDocLinkedToRandomInvoiceLine?.CSI_UnitOfQuantity2 ?? string.Empty));
		CachedValue<IAmount> inwardMovementDepartureCustomsWarehouseDebitAmount;

		decimal IImportDecLine.ForeignTradeStatisticsQuantity => throw new NotSupportedException("CUSWAT message does not contain ForeignTradeStatisticsQuantity");

		decimal IImportDecLine.ForeignTradeStatisticsGrossMassMeasure => throw new NotSupportedException("CUSWAT message does not contain ForeignTradeStatisticsGrossMassMeasure");

		internal PreviousDocument EntryInstructionPrevDocLinkedToRandomInvoiceLine => CachedValueHelper.GetValue(ref entryInstructionPrevDocLinkedToRandomInvoiceLine,
			() => RandomInvoiceLine.EntryInstruction?.PreviousDocuments.Cast<PreviousDocument>().SingleOrDefault(x => x.CSI_ItemNumber == RandomInvoiceLine.JI_LineNo));
		CachedValue<PreviousDocument> entryInstructionPrevDocLinkedToRandomInvoiceLine;

		string[] ContainerNumbersLinkedToRandomInvoiceLine => containerNumbersLinkedToRandomInvoiceLine ?? (containerNumbersLinkedToRandomInvoiceLine = RandomInvoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Select(x => x.ContainerNumber.ToString()).Except(string.Empty).ToArray());
		string[] containerNumbersLinkedToRandomInvoiceLine;
	}
}
