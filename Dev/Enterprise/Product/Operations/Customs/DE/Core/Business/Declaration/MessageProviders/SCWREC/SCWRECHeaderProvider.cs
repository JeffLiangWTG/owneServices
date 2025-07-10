using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWRECHeaderProvider : SingleDecHeaderProvider, ISCWRECHeader
	{
		public SCWRECHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public DateTime? LocalClearanceDate => CachedValueHelper.GetValue(ref localClearanceDate, () => EntryHeader.Style == ImportDeclarationTypeList.Codes.AZL ? EntryInstruction.CEI_LocalClearanceDate.ToNullableDateTime() : null);
		CachedValue<DateTime?> localClearanceDate;

		public string ForeignTradeImportEarlyClearanceFlag => EntryInstruction.CEI_EarlyClearanceFlag.ValueOrNullIfEmpty();

		public string LocalClearanceProcedure
		{
			get
			{
				string result = null;
				var declarantType = Declaration.JE_DeclarantType;
				if (!declarantType.IsEmpty)
				{
					if (EntryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.VZL)
					{
						result = GetCusAuthorizationUsageNumber(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
					}
					else if (EntryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.AZL)
					{
						result = GetCusAuthorizationUsageNumber(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
					}
				}
				return result;
			}
		}

		public string CurrentProcedure
		{
			get
			{
				var result = GetCusAuthorizationUsageNumber(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
				if (string.IsNullOrEmpty(result))
				{
					result = GetCusAuthorizationUsageNumber(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
				}
				return result;
			}
		}

		public IReadOnlyCollection<ISCWRECLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new SCWRECLineProvider(l)).ToArray());
		IReadOnlyCollection<ISCWRECLine> lines;

		IImportParty IImportDecHeader.Declarant => CachedValueHelper.GetValue(ref declarantCached, () => ImportPartyProvider.NewOrNull(DeclarantAddress));
		CachedValue<IImportParty> declarantCached;
	}
}
