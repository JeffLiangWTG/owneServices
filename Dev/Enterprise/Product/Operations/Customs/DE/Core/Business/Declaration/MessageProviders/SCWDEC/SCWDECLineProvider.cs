using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWDECLineProvider : SingleDecLineProvider, ISCWDECLine
	{
		public SCWDECLineProvider(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public string ArticleNumber => RandomInvoiceLine.JI_PartNo;

		public IAmount InwardMovementAmount => CachedValueHelper.GetValue(ref inwardMovementAmountCached, () => new AmountProvider(InvoiceLines.Sum(l => l.JI_BondedWhsQuantity), RandomInvoiceLine.JI_BondedWhsUnitQty));
		CachedValue<IAmount> inwardMovementAmountCached;

		public IImportLineCustomsValue CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => Declaration.ZG_IsHighValueOvrd && !IsProcedureInE01OrE02 ? new ImportLineCustomsValueProvider(Declaration, InvoiceLines) : null);
		CachedValue<IImportLineCustomsValue> customsValue;

		public string RequestedPreferentialTreatment => RandomInvoiceLine.JI_PrimaryPreference;

		bool IsProcedureInE01OrE02 => RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E01 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E02;
	}
}
