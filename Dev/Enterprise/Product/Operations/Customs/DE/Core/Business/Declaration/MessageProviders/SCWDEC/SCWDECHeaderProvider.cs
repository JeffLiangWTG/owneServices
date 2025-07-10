using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWDECHeaderProvider : SingleDecHeaderProvider, ISCWDECHeader
	{
		public SCWDECHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public bool ForeignTradeImportEarlyClearanceFlag => EntryInstruction.CEI_EarlyClearanceFlag == EarlyClearanceFlagsList.Codes.J;

		public ICustomsValue CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => Declaration.ZG_IsHighValueOvrd ? new CustomsValueProvider(EntryHeader) : null);
		CachedValue<ICustomsValue> customsValue;

		public IReadOnlyCollection<ISCWDECLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new SCWDECLineProvider(l)).ToArray());
		IReadOnlyCollection<ISCWDECLine> lines;

		string IImportDecHeader.ProcedureAuthorisation => CachedValueHelper.GetValue(ref procedureAuthorisation, () => EntryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>()
			.FirstOrDefault(x => ((string)x.AGC_Code).In(new[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1 }))?.AGC_Number ?? string.Empty);
		CachedValue<string> procedureAuthorisation;
	}
}
