using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWRECLineProvider : SingleDecLineProvider, ISCWRECLine
	{
		public SCWRECLineProvider(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public IAmount InwardMovementAmount => CachedValueHelper.GetValue(ref inwardMovementAmount, () => new AmountProvider(InvoiceLines.Sum(l => l.JI_BondedWhsQuantity), RandomInvoiceLine.JI_BondedWhsUnitQty));
		CachedValue<IAmount> inwardMovementAmount;

		public string RequestedPreferentialTreatment => RandomInvoiceLine.JI_PrimaryPreference.ValueOrNullIfEmpty();

		protected override bool IsHighValueOvrdValidForAssessmentCustomsValue => true;
	}
}
