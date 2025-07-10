using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCPEDBodyProvider : MonthlyClosingDecBodyProvider, ICFCPEDBody
	{
		public CFCPEDBodyProvider(CusReconEntry entry, bool isModificationMessage)
			: base(entry, isModificationMessage)
		{
		}

		public IImportParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () => ImportPartyProvider.NewOrNull(SupplierDocumentaryAddress));
		CachedValue<IImportParty> consignorCached;

		public Guid? ConsignorPK => SupplierDocumentaryAddress?.PK.ToGuid();

		public IReadOnlyCollection<IImportAdditionalDutyReference> AdditionalDutyReferences => additionalDutyReferences ??
			(additionalDutyReferences = EntryInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>()
				.Select(x => new ImportAdditionalDutyReferenceProvider(x)).ToArray());
		IReadOnlyCollection<IImportAdditionalDutyReference> additionalDutyReferences;

		public IReadOnlyCollection<ICFCPEDLine> Lines => lines ?? (lines = GetLinesQuery().Select(l => new CFCPEDLineProvider(l, IsModificationMessage)).ToArray());
		IReadOnlyCollection<ICFCPEDLine> lines;
	}
}
