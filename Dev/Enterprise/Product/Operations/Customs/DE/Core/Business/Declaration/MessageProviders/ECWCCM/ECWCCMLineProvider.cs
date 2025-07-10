using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ECWCCMLineProvider : ImportDecLineProvider, IECWCCMLine
	{
		public ECWCCMLineProvider(CusEntryLine entryLine)
			: base(entryLine)
		{
		}

		int IECWCCMLine.SequenceNumber => SequenceNumber;

		public string InwardMovementRegistrationNumber => RandomInvoiceLine.JI_PreviousEntryNumber;

		public int InwardMovementSequenceNumber => RandomInvoiceLine.JI_PreviousEntryLineNumber;

		public string OutwardMovementCompletionType => RandomInvoiceLine.JI_Procedure.Left(2);

		public string OutwardMovementCompletionRegistrationNumber => MRNCusEntryNumber?.CE_EntryNum;

		public DateTime? OutwardMovementDecisiveDate => MRNCusEntryNumber?.CE_ExpiryDate.ToNullableDateTime();

		public IAmount OutwardMovementAmount => CachedValueHelper.GetValue(ref outwardMovementAmount, () => new AmountProvider(RandomInvoiceLine.JI_BondedWhsQuantity, RandomInvoiceLine.JI_BondedWhsUnitQty));
		CachedValue<IAmount> outwardMovementAmount;

		CusEntryNumber MRNCusEntryNumber => CachedValueHelper.GetValue(ref mrnCusEntryNumber, () => CusEntryNumber.Load(RandomInvoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany));
		CachedValue<CusEntryNumber> mrnCusEntryNumber;
	}
}
