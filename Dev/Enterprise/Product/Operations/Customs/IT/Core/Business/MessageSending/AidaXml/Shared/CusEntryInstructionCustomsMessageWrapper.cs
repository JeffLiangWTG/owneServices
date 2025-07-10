using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Argument = CargoWise.Common.Argument;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

class CusEntryInstructionCustomsMessageWrapper : ICusEntryInstructionCustomsMessageWrapper
{
	public CusEntryInstructionCustomsMessageWrapper(CusEntryInstruction entryInstruction)
	{
		EntryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		Declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));

		InitializeLazy();
	}

	protected CusEntryInstruction EntryInstruction { get; }

	protected JobDeclaration Declaration { get; }

	#region ICusEntryInstructionCustomsMessageWrapper

	ZString ICusEntryInstructionCustomsMessageWrapper.AdditionalDeclarationType => EntryInstruction.CEI_SubStyle;

	IReadOnlyCollection<IAuthorization> ICusEntryInstructionCustomsMessageWrapper.Authorizations => lazyAuthorizations.Value;
	Lazy<IReadOnlyCollection<IAuthorization>> lazyAuthorizations;

	IReadOnlyCollection<IAdditionalSupplyChainActor> ICusEntryInstructionCustomsMessageWrapper.AdditionalSupplyChainActors => lazyAdditionalSupplyChainActors.Value;
	Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>> lazyAdditionalSupplyChainActors;

	IWarehouse ICusEntryInstructionCustomsMessageWrapper.Warehouse => lazyWarehouse.Value;
	Lazy<IWarehouse> lazyWarehouse;

	DateTime? ICusEntryInstructionCustomsMessageWrapper.AcceptanceDate => lazyAcceptanceDate.Value;
	Lazy<DateTime?> lazyAcceptanceDate;

	IReadOnlyCollection<IGuarantee> ICusEntryInstructionCustomsMessageWrapper.Guarantees => lazyGuarantees.Value;
	Lazy<IReadOnlyCollection<IGuarantee>> lazyGuarantees;

	IReadOnlyCollection<string> ICusEntryInstructionCustomsMessageWrapper.GuaranteeTypes => lazyGuaranteeTypes.Value;
	Lazy<IReadOnlyCollection<string>> lazyGuaranteeTypes;

	IReadOnlyCollection<IFiscalReference> ICusEntryInstructionCustomsMessageWrapper.FiscalReferences => lazyFiscalReferences.Value;
	Lazy<IReadOnlyCollection<IFiscalReference>> lazyFiscalReferences;

	string ICusEntryInstructionCustomsMessageWrapper.GetGuaranteeHolderIdentificationNumber(IEoriTrader declarant)
	{
		var allGuarantees = lazyGuarantees.Value;
		if (allGuarantees.Count == 0)
		{
			return null;
		}

		var eorNumber = declarant?.EoriNumber;
		var guarantees = allGuarantees.Where(g => !string.IsNullOrWhiteSpace(g.HolderIdentification));

		if (!string.IsNullOrWhiteSpace(eorNumber))
		{
			guarantees = guarantees
				.Where(g => !string.Equals(eorNumber, g.HolderIdentification, StringComparison.OrdinalIgnoreCase));
		}

		return guarantees
			.Select(g => g.HolderIdentification)
			.FirstOrDefault();
	}

	DateTime? ICusEntryInstructionCustomsMessageWrapper.GoodsPresentationDateTime => lazyGoodsPresentationDateTime.Value;
	Lazy<DateTime?> lazyGoodsPresentationDateTime;

	int? ICusEntryInstructionCustomsMessageWrapper.InlandTransportMode => InlandTransportMode;
	Lazy<int?> lazyInlandTransportMode;

	IReadOnlyCollection<IMeansOfTransport> ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports => lazyDepartureMeansOfTransports.Value;
	Lazy<IReadOnlyCollection<IMeansOfTransport>> lazyDepartureMeansOfTransports;

	IReadOnlyCollection<IPreviousDocument> ICusEntryInstructionCustomsMessageWrapper.PreviousDocuments => lazyPreviousDocuments.Value;
	Lazy<IReadOnlyCollection<IPreviousDocument>> lazyPreviousDocuments;

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> ICusEntryInstructionCustomsMessageWrapper.SupportingDocuments => lazySupportingDocuments.Value;
	Lazy<IReadOnlyCollection<MessageBuilder.ISupportingDocument>> lazySupportingDocuments;
	#endregion

	#region Implementation

	void InitializeLazy()
	{
		lazyAuthorizations = new Lazy<IReadOnlyCollection<IAuthorization>>(GetAuthorizations);
		lazyAdditionalSupplyChainActors = new Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>>(GetAdditionalSupplyChainActors);
		lazyWarehouse = new Lazy<IWarehouse>(GetWarehouse);
		lazyAcceptanceDate = new Lazy<DateTime?>(GetAcceptanceDate);
		lazyGuarantees = new Lazy<IReadOnlyCollection<IGuarantee>>(GetGuarantees);
		lazyGuaranteeTypes = new Lazy<IReadOnlyCollection<string>>(GetGuaranteeTypes);
		lazyFiscalReferences = new Lazy<IReadOnlyCollection<IFiscalReference>>(GetFiscalReferences);
		lazyGoodsPresentationDateTime = new Lazy<DateTime?>(GetGoodsPresentationDateTime);
		lazyInlandTransportMode = new Lazy<int?>(GetInlandTransportMode);
		lazyDepartureMeansOfTransports = new Lazy<IReadOnlyCollection<IMeansOfTransport>>(GetDepartureMeansOfTransports);
		lazyPreviousDocuments = new Lazy<IReadOnlyCollection<IPreviousDocument>>(GetPreviousDocuments);
		lazySupportingDocuments = new Lazy<IReadOnlyCollection<MessageBuilder.ISupportingDocument>>(GetSupportingDocuments);
	}

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> GetSupportingDocuments()
	{
		var entryInstruction = EntryInstruction;

		var supportingDocuments = entryInstruction.SupportingDocuments
			.Cast<SupportingDocument>()
			.Select(x => new SupportingDocumentWrapper(x))
			.Cast<MessageBuilder.ISupportingDocument>()
			.ToList();

		if (entryInstruction.JobDeclaration.IsImport && !entryInstruction.ClearanceByEntryLine)
		{
			supportingDocuments.Add(new SupportingDocument33YYWrapper());
		}

		return supportingDocuments;
	}

	IReadOnlyCollection<IFiscalReference> GetFiscalReferences()
	{
		return EntryInstruction.FiscalReferences
			.Cast<EU.Business.Declaration.CusFiscalReference>()
			.Select(x => new FiscalReferenceWrapper(x))
			.ToCollection();
	}

	DateTime? GetAcceptanceDate()
	{
		if (EntryInstruction.CanSetSimplifiedDecAcceptanceDate)
		{
			var acceptanceDateTime = EntryInstruction.ZG_SimplifiedDecAcceptanceDate;

			return acceptanceDateTime.IsEmpty || !acceptanceDateTime.IsValid
				? null
				: acceptanceDateTime.ToDateTime();
		}
		return null;
	}

	IReadOnlyCollection<IGuarantee> GetGuarantees()
	{
		return AllGuarantees
			.Select(x => new GuaranteeWrapper(x))
			.ToCollection();
	}

	IReadOnlyCollection<string> GetGuaranteeTypes()
	{
		return AllGuarantees
			.Select(x => x.PW_BondType.ToString())
			.Distinct()
			.ToCollection();
	}

	IEnumerable<GuaranteeForEntryInstruction> AllGuarantees => EntryInstruction.Guarantees.Cast<GuaranteeForEntryInstruction>();

	DateTime? GetGoodsPresentationDateTime()
	{
		var goodsPresentationDateTime = EntryInstruction.ZG_PresentationStartDate;

		return goodsPresentationDateTime.IsEmpty || !goodsPresentationDateTime.IsValid
			? null
			: goodsPresentationDateTime.ToDateTime();
	}

	int? GetInlandTransportMode()
	{
		var inlandTransportModeAssessor = new CusEntryInstructionInlandTransportModeAssessor(EntryInstruction);
		if (!inlandTransportModeAssessor.IsRequiredInCustomsMessage)
		{
			return null;
		}

		var declaration = EntryInstruction.JobDeclaration;
		var wcoCode = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);
		if (!int.TryParse(wcoCode, out var inlandTransportMode))
		{
			return InvalidInlandTransportMode;
		}
		return inlandTransportMode;
	}

	int? InlandTransportMode => lazyInlandTransportMode.Value;

	IReadOnlyCollection<IMeansOfTransport> GetDepartureMeansOfTransports()
	{
		return new DepartureMeansOfTransportWrappersProvider(EntryInstruction, InlandTransportMode)
			.GetDepartureMeansOfTransports();
	}

	IReadOnlyCollection<IPreviousDocument> GetPreviousDocuments()
	{
		return EntryInstruction.PreviousDocuments
			.Cast<PreviousDocument>()
			.Select(x => new EntryInstructionPreviousDocumentWrapper(x))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
	{
		return EntryInstruction.CusSupplyChainActorReferences
			.Cast<CusSupplyChainActorReference>()
			.Select(x => new AdditionalSupplyChainActorWrapper(x))
			.ToCollection();
	}

	IReadOnlyCollection<IAuthorization> GetAuthorizations()
	{
		return EntryInstruction.CusAuthorizationUsages
			.Select(x => new AuthorizationWrapper(x))
			.ToCollection();
	}

	IWarehouse GetWarehouse()
	{
		var entryInstruction = EntryInstruction;
		var warehouseType = ZString.Empty;
		var warehouseId = ZString.Empty;

		if (entryInstruction.IsIntoWarehouseWarehousing)
		{
			warehouseType = entryInstruction.ZG_ToWarehouseType;
			warehouseId = entryInstruction.ZG_ToWarehouseID;
		}

		if (entryInstruction.IsOutOfWarehouseWarehousing)
		{
			warehouseType = entryInstruction.ZG_FromWarehouseType;
			warehouseId = entryInstruction.ZG_FromWarehouseID;
		}

		return !warehouseType.IsEmpty && !warehouseId.IsEmpty
			? new WarehouseWrapper(warehouseType, warehouseId)
			: null;
	}

	#endregion

	const int InvalidInlandTransportMode = -1;
}
