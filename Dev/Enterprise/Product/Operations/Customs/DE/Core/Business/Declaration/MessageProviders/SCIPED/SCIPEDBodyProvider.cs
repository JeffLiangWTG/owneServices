using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIPEDBodyProvider : MonthlyClosingDecBodyProvider, ISCIPEDBody
	{
		public SCIPEDBodyProvider(CusReconEntry entry, bool isModificationMessage)
			: base(entry, isModificationMessage)
		{
		}

		public IImportParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () => ImportPartyProvider.NewOrNull(SupplierDocumentaryAddress));
		CachedValue<IImportParty> consignorCached;

		public Guid ConsignorPK => SupplierDocumentaryAddress?.PK.ToGuid() ?? Guid.Empty;

		public IReadOnlyCollection<ISCIPEDLine> Lines => lines ?? (lines = GetLinesQuery().Select(l => new SCIPEDLineProvider(l, IsModificationMessage)).ToArray());
		IReadOnlyCollection<ISCIPEDLine> lines;
	}
}
