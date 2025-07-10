using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDHeaderWrapper : IDeclarationDVDHeader
	{
		public DeclarationDVDHeaderWrapper(CusEntryHeader entryHeader, bool shouldDeclareUCRInHeader = false, bool shouldDeclareAddSupplyActorsInHeader = false, bool shouldDeclareCountryOfDestinationInHeader = false, bool shouldDeclareCountryOfExportInHeader = false, bool shouldDeclarePreviousDocumentsInHeader = false)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			entryInstruction = entryHeader.EntryInstruction;

			this.shouldDeclareUCRInHeader = shouldDeclareUCRInHeader;
			this.shouldDeclareAddSupplyActorsInHeader = shouldDeclareAddSupplyActorsInHeader;
			this.shouldDeclareCountryOfDestinationInHeader = shouldDeclareCountryOfDestinationInHeader;
			this.shouldDeclareCountryOfExportInHeader = shouldDeclareCountryOfExportInHeader;
			this.shouldDeclarePreviousDocumentsInHeader = shouldDeclarePreviousDocumentsInHeader;

			exporterId = GetExporterId();
			consigneeId = GetConsigneeId();
			declarantId = GetDeclarantId();
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;

		readonly ZBool shouldDeclareUCRInHeader;
		readonly ZBool shouldDeclareAddSupplyActorsInHeader;
		readonly ZBool shouldDeclareCountryOfDestinationInHeader;
		readonly ZBool shouldDeclareCountryOfExportInHeader;
		readonly ZBool shouldDeclarePreviousDocumentsInHeader;

		readonly ZString exporterId;
		readonly ZString consigneeId;
		readonly ZString declarantId;

		const string UECode5 = "00500";
		const string UECode4 = "00400";

		readonly ZString[] requestedProceduresForExporter = new ZString[] { "76", "77" };
		readonly ZString[] requestedProceduresForConsignee = new ZString[] { "71", "78", "95" };

		public ZString CustomsOffice => declaration.JE_CustomsOffice;

		public ZString LRN => entryHeader.CH_BGMReference;

		public ZString DeclarationType => declaration.JE_EntryStyle;

		public ZString DeclarationSubType => entryInstruction.CEI_SubStyle;

		public ZDecimal TotalGrossMass => (ZDecimal)entryHeader.MergedLines.Sum(x => x.EffectiveGrossWeight.InUnroundedKilogramsSafe);

		public ZString ExporterId => exporterId;

		public ZString ConsigneeId => consigneeId;

		public ZString DeclarantUECode => declarantId.IsEmpty ? string.Empty
															: declarantId == consigneeId
																	? UECode5
																	: declarantId == exporterId
																				? UECode4
																				: string.Empty;

		public ZString DeclarantId => declarantId;

		public ZString DeclarationEmail => declaration.DeclEmailAddr;

		public ZString DeclarationOtherEmail => declaration.ZG_OtherEmailAddr;

		public ZString RepresentativeId => declaration.Representative?.Header.GetIDCode() ?? ZString.Empty;

		public ZString RepresentativeType
		{
			get
			{
				var representativeType = declaration.JE_DeclarantType;

				switch (representativeType)
				{
					case EU.Business.RepresentationTypeList.Codes._2Direct:
						representativeType = ESRepresentationTypeList.Codes._2Direct;
						break;
					case EU.Business.RepresentationTypeList.Codes._3Indirect:
						representativeType = ESRepresentationTypeList.Codes._3Indirect;
						break;
				}

				return representativeType;
			}
		}

		public IReadOnlyCollection<IDeclarationDVDAuthorisation> Authorisations
		{
			get
			{
				if (authorisations == null)
				{
					var authorisationsList = new List<DeclarationDVDAuthorisationWrapper>();

					entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().ForEach(x => authorisationsList.Add(new DeclarationDVDAuthorisationWrapper(x)));

					authorisations = authorisationsList.AsReadOnly();
				}
				return authorisations;
			}
		}
		IReadOnlyCollection<DeclarationDVDAuthorisationWrapper> authorisations;

		public ZString TransportCode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, false);

		public IReadOnlyCollection<IAdditionalSupplyChainActorCommon> AdditionalSupplyActors
		{
			get
			{
				if (additionalSupplyActors == null)
				{
					var additionalSupplyActorsList = new List<AdditionalSupplyChainActorCommonWrapper>();

					if (shouldDeclareAddSupplyActorsInHeader)
					{
						entryInstruction.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().ForEach(x => additionalSupplyActorsList.Add(new AdditionalSupplyChainActorCommonWrapper(x)));
					}

					additionalSupplyActors = additionalSupplyActorsList.AsReadOnly();
				}
				return additionalSupplyActors;
			}
		}
		IReadOnlyCollection<AdditionalSupplyChainActorCommonWrapper> additionalSupplyActors;

		public IReadOnlyCollection<IDeclarationDVDSupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					var supportingDocumentsList = new List<DeclarationDVDSupportingDocumentWrapper>();

					declaration.SupportingDocuments.Cast<SupportingDocument>().ForEach(doc => supportingDocumentsList.Add(new DeclarationDVDSupportingDocumentWrapper(doc)));
					entryHeader.EntryInstruction.SupportingDocuments.Cast<SupportingDocument>().ForEach(doc => supportingDocumentsList.Add(new DeclarationDVDSupportingDocumentWrapper(doc)));
					supportingDocuments = supportingDocumentsList.AsReadOnly();
				}
				return supportingDocuments;
			}
		}
		IReadOnlyCollection<DeclarationDVDSupportingDocumentWrapper> supportingDocuments;

		public ZBool IsContainerised => entryHeader.IsContainerised();

		public IDeclarationDVDLocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new DeclarationDVDLocationOfGoodsWrapper(entryInstruction));
		DeclarationDVDLocationOfGoodsWrapper locationOfGoods;

		public ZString CountryOfDestination => shouldDeclareCountryOfDestinationInHeader ? declaration.JE_GoodsDestination : ZString.Empty;

		public ZString CountryOfExport => shouldDeclareCountryOfExportInHeader ? declaration.JE_GoodsOrigin : ZString.Empty;

		public ZString UCRReferenceNumber => shouldDeclareUCRInHeader ? ((CusEntryLine)entryHeader.RandomEntryLine).RandomLine.ZG_CommercialReference : ZString.Empty;

		public IWarehouseCommon Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					if (entryInstruction.CusAuthorizationUsages.Any(x => CommonWrappersHelper.WarehouseTypeListContainsCode(x.AGC_Code)))
					{
						warehouse = new WarehouseCommonWrapper(entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(x => CommonWrappersHelper.WarehouseTypeListContainsCode(x.AGC_Code)));
					}
				}
				return warehouse;
			}
		}
		WarehouseCommonWrapper warehouse;

		public IReadOnlyCollection<IDeclarationDVDPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = new List<DeclarationDVDPreviousDocumentWrapper>();

					if (shouldDeclarePreviousDocumentsInHeader)
					{
						entryHeader.PreviousDocuments.ForEach(x => previousDocuments.Add(new DeclarationDVDPreviousDocumentWrapper((PreviousDocument)x)));
					}
				}
				return previousDocuments;
			}
		}
		List<DeclarationDVDPreviousDocumentWrapper> previousDocuments;

		public IReadOnlyCollection<IDeclarationDVDGuarantee> Guarantees
		{
			get
			{
				if (guarantees == null)
				{
					var guaranteesList = new List<DeclarationDVDGuaranteeWrapper>();

					declaration.Guarantees.Cast<ESGuarantee>().Where(x => x.EntryInstructionID == entryHeader.CH_CEI_Instruction).ForEach(x => guaranteesList.Add(new DeclarationDVDGuaranteeWrapper(x)));
					guarantees = guaranteesList.AsReadOnly();
				}
				return guarantees;
			}
		}
		IReadOnlyCollection<DeclarationDVDGuaranteeWrapper> guarantees;

		public ZString CustomOfficeOfPresentation => declaration.GetCustomsOfficeFromList(EuOfficeCodesTypes.Codes.OfficeOfPresentation);

		ZString GetExporterId() => GetIdWhenRequestedProcedureExists(declaration.Supplier, requestedProceduresForExporter);
		ZString GetConsigneeId() => GetIdWhenRequestedProcedureExists(declaration.Importer, requestedProceduresForConsignee);
		ZString GetDeclarantId() => declaration.Declarant?.Header.GetIDCode() ?? ZString.Empty;

		ZString GetIdWhenRequestedProcedureExists(OrgHeader orgHeader, ZString[] requestedProcedures)
		{
			var result = ZString.Empty;

			var hasRequestedProcedure = entryHeader.MergedLines.Any(x => x.InvoiceLines.Any(y => requestedProcedures.Contains(((JobComInvoiceLine)y).JI_FormattedProcedure.SubstringSafe(0, 2))));
			if (hasRequestedProcedure)
			{
				result = orgHeader.GetIDCode();
			}

			return result;
		}
	}
}
