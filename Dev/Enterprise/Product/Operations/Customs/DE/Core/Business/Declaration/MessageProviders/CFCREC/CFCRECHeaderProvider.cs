using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCRECHeaderProvider : ImportDecHeaderProvider, ICFCRECHeader
	{
		public CFCRECHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public string TaxOffice => CachedValueHelper.GetValue(ref taxOffice, () =>
		{
			var declarationSender = Declaration.JE_DeclarantType == RepresentationTypeList.Codes._2Direct ? Declaration.Representative : Declaration.Declarant;
			return declarationSender?.Header.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice).ToString();
		});
		CachedValue<string> taxOffice;

		public string ArrivalTransportMeansIdentity => Declaration.JE_TransportMode != TransportTypeList.Codes.FixedTransportInstallations ? Declaration.ZG_Box18TransportID.ToString() : null;

		public IReadOnlyCollection<IImportAdditionalDutyReference> AdditionalDutyReferences => additionalDutyReferences ?? (additionalDutyReferences = EntryInstruction.FiscalReferences.Cast<CusFiscalReference>().Select(x => new ImportAdditionalDutyReferenceProvider(x)).ToArray());
		IReadOnlyCollection<IImportAdditionalDutyReference> additionalDutyReferences;

		public IReadOnlyCollection<ICFCRECLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new CFCRECLineProvider(l)).ToArray());
		IReadOnlyCollection<ICFCRECLine> lines;

		public DateTime? LocalClearanceDate => CachedValueHelper.GetValue(ref localClearanceDate, () => EntryHeader.Style == ImportDeclarationTypeList.Codes.AZ ? EntryInstruction.CEI_LocalClearanceDate.ToNullableDateTime() : null);
		CachedValue<DateTime?> localClearanceDate;

		public string LocalClearanceProcedure => CachedValueHelper.GetValue(ref localClearanceProcedure, () => GetLocalClearanceProcedureAuthorisationNumber((x) => x == ImportDeclarationTypeList.Codes.VZA, (y) => y == ImportDeclarationTypeList.Codes.AZ));
		CachedValue<string> localClearanceProcedure;

		string IImportDecHeader.ProcedureAuthorisation => CachedValueHelper.GetValue(ref procedureAuthorisation, () => GetCusAuthorizationUsageNumber(CusAuthorizationHeaderTypeList.Codes.EndUse));
		CachedValue<string> procedureAuthorisation;

		IImportParty IImportDecHeader.Declarant => CachedValueHelper.GetValue(ref declarantCached, () => ImportPartyProvider.NewOrNull(DeclarantAddress));
		CachedValue<IImportParty> declarantCached;
	}
}
