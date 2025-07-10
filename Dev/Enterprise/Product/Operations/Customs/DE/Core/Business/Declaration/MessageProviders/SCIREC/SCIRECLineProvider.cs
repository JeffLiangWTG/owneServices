using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIRECLineProvider : ImportDecLineProvider, ISCIRECLine
	{
		public SCIRECLineProvider(CusEntryLine entryLine)
			: base(entryLine)
		{
		}

		public IAmount InwardMovementAmount => CachedValueHelper.GetValue(ref inwardMovementAmountCached, () => new AmountProvider(InvoiceLines.Sum(l => l.JI_BondedWhsQuantity), RandomInvoiceLine.JI_BondedWhsUnitQty));
		CachedValue<IAmount> inwardMovementAmountCached;

		public string RequestedPreferentialTreatment => (!IsProcedureInF01OrF02OrF03 || Declaration.JE_EntryStyle != EntryStyleListImport.Codes.ImportFromSpecialTerritory) ? RandomInvoiceLine.JI_PrimaryPreference.ToString() : null;

		string IImportDecLine.OriginCountry => (RandomInvoiceLine.JI_PrimaryPreference.IsEmpty || RandomInvoiceLine.JI_PrimaryPreference != RandomInvoiceLine.JI_CountryOfOrigin) ? RandomInvoiceLine.JI_CountryOfOrigin.ToString() : null;

		protected override bool IsHighValueOvrdValidForAssessmentCustomsValue => true;

		string Concession => RandomInvoiceLine.JI_Procedure.SubstringSafe(4);

		bool IsProcedureInF01OrF02OrF03 => Concession == CustomsProcedureCodeList.Import.Concession._F01 || Concession == CustomsProcedureCodeList.Import.Concession._F02 || Concession == CustomsProcedureCodeList.Import.Concession._F02;
	}
}
