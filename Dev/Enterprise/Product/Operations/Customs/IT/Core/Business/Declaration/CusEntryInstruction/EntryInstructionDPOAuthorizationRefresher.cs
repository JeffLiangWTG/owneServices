using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryInstructionDPOAuthorizationRefresher : IEntryInstructionDPOAuthorizationRefresher
{
	public static EntryInstructionDPOAuthorizationRefresher New(CusEntryInstructionCollection entryInstructionCollection)
	{
		Argument.NotNull(entryInstructionCollection, nameof(entryInstructionCollection));
		var declaration = Argument.NotNull(entryInstructionCollection.Master, nameof(entryInstructionCollection.Master));
		return new EntryInstructionDPOAuthorizationRefresher(entryInstructionCollection, declaration);
	}

	public EntryInstructionDPOAuthorizationRefresher(CusEntryInstructionCollection entryInstructionCollection)
	{
		Argument.NotNull(entryInstructionCollection, nameof(entryInstructionCollection));
		this.entryInstructionCollection = entryInstructionCollection;
		this.declaration = Argument.NotNull(entryInstructionCollection.Master, nameof(entryInstructionCollection.Master));
	}

	EntryInstructionDPOAuthorizationRefresher(CusEntryInstructionCollection entryInstructionCollection, JobDeclaration declaration)
	{
		this.entryInstructionCollection = entryInstructionCollection;
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;
	readonly CusEntryInstructionCollection entryInstructionCollection;

	void IEntryInstructionDPOAuthorizationRefresher.HookEvents()
	{
		declaration.JE_DefermentAccountNumberInfo.ValueChanged += JE_DefermentAccountNumberInfo_ValueChanged;
		entryInstructionCollection.CountChanged += EntryInstructionCollection_CountChanged;
	}

	void IEntryInstructionDPOAuthorizationRefresher.UnhookEvents()
	{
		declaration.JE_DefermentAccountNumberInfo.ValueChanged -= JE_DefermentAccountNumberInfo_ValueChanged;
		entryInstructionCollection.CountChanged -= EntryInstructionCollection_CountChanged;
	}

	#region Implementation

	void EntryInstructionCollection_CountChanged(object sender, CargoWise.EntityFramework.CollectionCountChangedEventArgs e)
	{
		if (!entryInstructionCollection.IsLoading && e.ItemAdded && e.BizObject is CusEntryInstruction entryInstruction)
		{
			RemoveAndAddNewAuthorizationRecord(entryInstruction);
		}
	}

	void JE_DefermentAccountNumberInfo_ValueChanged(object sender, System.EventArgs e)
	{
		if (!IsEntryInstructionDPOAuthorizationRefreshSupported)
		{
			return;
		}

		if (DefermentAccountNumber.IsEmpty)
		{
			ClearAllApplicableAuthorizationRecords();
			return;
		}

		RemoveOldAndAddNewDPOAuthorizationUsagesForAllEntryInstructions();
	}

	void ClearAllApplicableAuthorizationRecords() => EntryInstructionsToIEnumerable.Where(IsEntryNotMergedOrWithEmptyStatus).ForEach(RemoveAuthorizationUsageRecordsIfExists);

	void RemoveOldAndAddNewDPOAuthorizationUsagesForAllEntryInstructions() => EntryInstructionsToIEnumerable.ForEach(RemoveAndAddNewAuthorizationRecord);

	void RemoveAndAddNewAuthorizationRecord(CusEntryInstruction entryInstruction)
	{
		if (CanManageAuthorizationCodeUsageRecords(entryInstruction))
		{
			RemoveAuthorizationUsageRecordsIfExists(entryInstruction);
			AddAuthorizationUsageRecord(entryInstruction);
		}
	}

	void AddAuthorizationUsageRecord(CusEntryInstruction entryInstruction)
	{
		var owner = GetAuthorizationUsageOwner();
		entryInstruction.AddAuthorizationUsage(AuthorizationCode, owner, DefermentAccountNumber);
	}

	void RemoveAuthorizationUsageRecordsIfExists(CusEntryInstruction entryInstruction)
	{
		entryInstruction.RemoveAuthorizationUsageRecordsWithTypeIfExists(AuthorizationCode);
	}

	bool CanManageAuthorizationCodeUsageRecords(CusEntryInstruction entryInstruction)
	{
		var owner = GetAuthorizationUsageOwner();
		return IsEntryInstructionDPOAuthorizationRefreshSupported && owner != null && IsEntryNotMergedOrWithEmptyStatus(entryInstruction);
	}

	OrgHeader GetAuthorizationUsageOwner()
	{
		if (declaration.IsImport)
		{
			return GetAuthorizationUsageOwnerForImportDeclaration();
		}

		if (declaration.IsUCC6AndIsExport)
		{
			return GetAuthorizationUsageOwnerForUcc6ExportDeclaration();
		}

		return null;
	}

	OrgHeader GetAuthorizationUsageOwnerForUcc6ExportDeclaration()
	{
		switch (declaration.JE_PaymentMethod)
		{
			case Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions:
				return declaration.Declarant?.Header;
			case Ucc6ExportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions:
				return declaration.Importer;
			case Ucc6ExportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions:
				return declaration.Forwarder;
			case Ucc6ExportDefermentMethodList.Codes.SuppliersAccountFromCustomsDecisions:
				return declaration.Supplier;
			case Ucc6ExportDefermentMethodList.Codes.ExportersAccountFromCustomsDecisions:
				return declaration.ExporterDocAddress?.Organisation;
			default:
				return null;
		}
	}

	OrgHeader GetAuthorizationUsageOwnerForImportDeclaration()
	{
		switch (declaration.JE_PaymentMethod)
		{
			case ImportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions:
				return declaration.Declarant?.Header;
			case ImportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions:
				return declaration.Consignee;
			case ImportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions:
				return declaration.Forwarder;
			case ImportDefermentMethodList.Codes.RepresentativesAccountFromCustomsDecisions:
				return declaration.Representative?.Header;
			default:
				return null;
		}
	}

	IEnumerable<CusEntryInstruction> EntryInstructionsToIEnumerable => entryInstructionCollection.Cast<CusEntryInstruction>();

	ZString DefermentAccountNumber => declaration.JE_DefermentAccountNumber;

	bool IsEntryInstructionDPOAuthorizationRefreshSupported => declaration.IsImport || declaration.IsUCC6AndIsExport;

	bool IsEntryNotMergedOrWithEmptyStatus(CusEntryInstruction entryInstruction) => entryInstruction.EntryHeader == null || entryInstruction.EntryHeader.CH_EntryStatus.IsEmpty;

	string AuthorizationCode => CusAuthorizationHeaderTypeList.Codes.DeferredPayment;

	#endregion
}
