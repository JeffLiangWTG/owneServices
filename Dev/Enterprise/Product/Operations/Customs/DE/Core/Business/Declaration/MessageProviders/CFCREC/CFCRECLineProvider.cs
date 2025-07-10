using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCRECLineProvider : ImportDecLineProvider, ICFCRECLine
	{
		public CFCRECLineProvider(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public string TobaccoRevenueStampNumber => RandomInvoiceLine.JI_TobaccoStamp;

		public ILinePreferentialTreatment PreferentialTreatment => CachedValueHelper.GetValue(ref preferentialTreatment, () => Declaration.JE_EntryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory || IsProcedureInF01OrF02OrF03 ? null : new LinePreferentialTreatmentProvider(RandomInvoiceLine));
		CachedValue<ILinePreferentialTreatment> preferentialTreatment;

		public string PreferentialOriginCountry => (int.TryParse(RandomInvoiceLine.JI_PrimaryPreference, out var primaryPreferenceAsInteger) && primaryPreferenceAsInteger >= 200) ? (string)RandomInvoiceLine.ZG_CountryOfSupply : null;

		public string CessionManagementFlag
		{
			get
			{
				string result = null;
				var procedure = RandomInvoiceLine.JI_Procedure;
				if (!procedure.IsEmpty && !procedure.SubstringSafe(4, 3).IsEmpty)
				{
					result = RandomInvoiceLine.JI_CessionFlag;
				}
				return result;
			}
		}

		protected override bool IsHighValueOvrdValidForAssessmentCustomsValue => true;

		bool IsProcedureInF01OrF02OrF03 => RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._F01 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._F02 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._F03;
	}
}
