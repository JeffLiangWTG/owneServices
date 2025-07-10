using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public abstract class MonthlyClosingDecHeaderProvider : IMonthlyClosingDecHeader
	{
		public MonthlyClosingDecHeaderProvider(CusReconDeclaration declaration, string messageRole)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
			Factory = Declaration.Factory;
			this.messageRole = messageRole;
		}
		protected readonly CusReconDeclaration Declaration;
		protected readonly BusinessObjectFactory Factory;
		readonly string messageRole;

		public string MessageRole => messageRole;

		public string DeclarationKind => Factory.GetValue(ref declarationKind, () =>
		{
			switch (Declaration.CRD_DeclarationType)
			{
				case MonthlyClosingDeclarationTypeList.Codes.AAV:
				case MonthlyClosingDeclarationTypeList.Codes.AZ:
				case MonthlyClosingDeclarationTypeList.Codes.AZL:
					return "Z";
				case MonthlyClosingDeclarationTypeList.Codes.VAV:
				case MonthlyClosingDeclarationTypeList.Codes.VZA:
				case MonthlyClosingDeclarationTypeList.Codes.VZL:
					return "Y";
				default:
					return string.Empty;
			}
		});
		CachedProperty<string> declarationKind;

		public string ReferenceNumber => !IsInitialMessage ? (string)Declaration.RegistrationNumber : string.Empty;

		public string LocalReferenceNumber => Declaration.CRD_JobReferenceNumber;

		public DateTime? StartAccountingPeriodDate => Declaration.CRD_PeriodFrom.ToNullableDateTime();

		public DateTime? EndAccountingPeriodDate => Declaration.CRD_PeriodTo.ToNullableDateTime();

		public bool DeclarantIsConsigneeFlag => Declaration.IsDeclarantImporter;

		public string LocalClearanceProcedure => ClearanceAuthorization?.CPH_Number;

		public string ProcedureAuthorization => Factory.GetValue(ref procedureAuthorization, () => GetProcedureAuthorizationCore());
		CachedProperty<string> procedureAuthorization;

		public string CurrencyCode => CurrencyCodes.Germany;

		public string RepresentativeRelationshipFlag => Factory.GetValue(ref representativeRelationshipFlag, () =>
		{
			switch (Declaration.CRD_DeclarantType)
			{
				case RepresentationTypeList.Codes._1Self:
					return "0";
				case RepresentationTypeList.Codes._2Direct:
					return "1";
				case RepresentationTypeList.Codes._3Indirect:
					return "2";
				default:
					return string.Empty;
			}
		});
		CachedProperty<string> representativeRelationshipFlag;

		public string DeclarationPlace => GlbBranch.CurrentBranch.GB_City;

		public IImportParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => ImportPartyProvider.NewOrNull(Declaration.DeclarantAddress));
		CachedValue<IImportParty> declarantCached;

		public IPartyID Representative => CachedValueHelper.GetValue(ref represetativeCached, () => Declaration.CRD_DeclarantType == RepresentationTypeList.Codes._2Direct ? ImportPartyIDProvider.NewOrNull(Declaration.RepresentativeAddress) : null);
		CachedValue<IPartyID> represetativeCached;

		public IImportParty Principal => CachedValueHelper.GetValue(ref principalCached, () => Declaration.CRD_DeclarantType == RepresentationTypeList.Codes._3Indirect ? ImportPartyProvider.NewOrNull(Declaration.BuyingAgentAddress) : null);
		CachedValue<IImportParty> principalCached;

		public IImportPartyContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPersonCached, () => ImportPartyContactPersonProvider.NewOrNull(GlbStaff.CurrentUser));
		CachedValue<IImportPartyContactPerson> contactPersonCached;

		protected CusAuthorisationHeader ClearanceAuthorization => Factory.GetValue(ref clearanceAuthorization, () =>
		{
			if (!Declaration.CRD_CPH_ReconClearanceAuthorisation.IsEmpty)
			{
				var permit = Factory.Load<CusAuthorisationHeader>(Declaration.CRD_CPH_ReconClearanceAuthorisation);
				return permit;
			}
			return null;
		});
		CachedProperty<CusAuthorisationHeader> clearanceAuthorization;

		protected bool IsModificationMessage => MessageRole == MonthlyClosingMessageRoleList.Codes.ModificationMessage;

		protected CusEntryInstruction EntryInstruction
			=> Factory.GetValue(ref entryInstruction, () => (CusEntryInstruction)Declaration.CusReconEntries.FirstOrDefault()?.EntryHeader?.EntryInstruction);
		CachedProperty<CusEntryInstruction> entryInstruction;

		protected virtual string GetProcedureAuthorizationCore() => EntryInstruction.GetCusAuthorizationUsageNumber(CusAuthorizationHeaderTypeList.Codes.EndUse);

		bool IsInitialMessage => messageRole == MonthlyClosingMessageRoleList.Codes.FirstPartialMessage || messageRole == MonthlyClosingMessageRoleList.Codes.FinalMessage;
	}
}
