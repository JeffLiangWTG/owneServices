using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIRECHeaderProvider : ImportDecHeaderProvider, ISCIRECHeader
	{
		public SCIRECHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public DateTime? LocalClearanceDate => CachedValueHelper.GetValue(ref localClearanceDate, () => EntryHeader.Style == ImportDeclarationTypeList.Codes.AAV ? EntryInstruction.CEI_LocalClearanceDate.ToNullableDateTime() : null);
		CachedValue<DateTime?> localClearanceDate;

		public string LocalClearanceProcedure => CachedValueHelper.GetValue(ref localClearanceProcedure, () => GetLocalClearanceProcedureAuthorisationNumber((x) => x == ImportDeclarationTypeList.Codes.VAV, (y) => y == ImportDeclarationTypeList.Codes.AAV));
		CachedValue<string> localClearanceProcedure;

		public string ArrivalTransportMeansIdentity => Declaration.JE_TransportMode != TransportTypeList.Codes.FixedTransportInstallations ? Declaration.ZG_Box18TransportID.ToString() : null;

		public IReadOnlyCollection<ISCIRECLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new SCIRECLineProvider(l)).ToArray());
		IReadOnlyCollection<ISCIRECLine> lines;

		string IImportDecHeader.ProcedureAuthorisation => CachedValueHelper.GetValue(ref procedureAuthorisation, () => GetCusAuthorizationUsageNumber(CusAuthorizationHeaderTypeList.Codes.InwardProcessing));
		CachedValue<string> procedureAuthorisation;

		IImportParty IImportDecHeader.Declarant => CachedValueHelper.GetValue(ref declarantCached, () => ImportPartyProvider.NewOrNull(DeclarantAddress));
		CachedValue<IImportParty> declarantCached;
	}
}
