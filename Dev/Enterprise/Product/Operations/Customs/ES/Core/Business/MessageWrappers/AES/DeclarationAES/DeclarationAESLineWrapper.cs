using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESLineWrapper : AESCommonLineWrapper, IDeclarationAESLine
{
	public DeclarationAESLineWrapper(CusEntryLine entryLine, ZBool shouldDeclareConsignorInLine, ZBool shouldDeclareConsigneeInLine, bool isComplementaryCWithMRN) : base(entryLine)
	{
		entryInstruction = randomLine.EntryInstruction;
		this.shouldDeclareConsignorInLine = shouldDeclareConsignorInLine;
		this.shouldDeclareConsigneeInLine = shouldDeclareConsigneeInLine;
		this.isComplementaryCWithMRN = isComplementaryCWithMRN;

		isProvisionalPeriod = entryLine.Declaration?.IsTransitionPeriodAES30 ?? false;
	}
	readonly CusEntryInstruction entryInstruction;
	readonly ZBool shouldDeclareConsignorInLine;
	readonly ZBool shouldDeclareConsigneeInLine;
	readonly bool isComplementaryCWithMRN;
	readonly ZBool isProvisionalPeriod;

	public ZString UCRReferenceNumber => randomLine.ZG_CommercialReference;

	public IReadOnlyCollection<ICommonAuthorisation> Authorisations
	{
		get
		{
			if (authorisations == null)
			{
				var authorisationsList = new List<CommonAuthorisationWrapper>();

				var cusAuthorizations = new List<CusAuthorizationUsage>();
				entryLine.InvoiceLines.ForEach(x => cusAuthorizations.AddRange(((JobComInvoiceLine)x).CusAuthorizationUsages.Cast<CusAuthorizationUsage>()));
				ZShort seqNum = 1;
				foreach (var authorization in cusAuthorizations)
				{
					authorisationsList.Add(new CommonAuthorisationWrapper(authorization, seqNum));
					seqNum++;
				}

				authorisations = authorisationsList.AsReadOnly();
			}
			return authorisations;
		}
	}
	IReadOnlyCollection<CommonAuthorisationWrapper> authorisations;

	public IDeclarationAESProcedure Procedure => procedure ?? (procedure = new DeclarationAESProcedureWrapper(randomLine));
	DeclarationAESProcedureWrapper procedure;

	public IPartyIdProvider Consignor => consignor ?? (consignor = shouldDeclareConsignorInLine ? PartyIdWrapper.New(randomLine.ExporterAddress) : null);
	PartyIdWrapper consignor;

	public IDeclarationAESConsignee Consignee
	{
		get
		{
			if (consignee == null)
			{
				if (shouldDeclareConsigneeInLine)
				{
					var declaration = (JobDeclaration)entryLine.Declaration;
					var header = randomLine.InvoiceHeader;
					var declarationImporter = declaration.ImporterDocumentaryAddress;
					var orgHeader = header.Buyer ?? declarationImporter.Organisation;
					var orgAddress = header.Buyer != null ? header.BuyerAddress : declarationImporter.Address;

					consignee = DeclarationAESConsigneeWrapper.New(orgHeader, orgAddress, isProvisionalPeriod, declaration.ZG_DontSendImporterId, declaration);
				}
			}
			return consignee;
		}
	}
	DeclarationAESConsigneeWrapper consignee;

	public IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyActors
	{
		get
		{
			if (additionalSupplyActors == null)
			{
				var additionalSupplyActorsList = new List<CommonAdditionalSupplyChainActorSeqNumWrapper>();

				var supplyChainActors = new List<CusSupplyChainActorReference>();
				entryLine.InvoiceLines.ForEach(x => supplyChainActors.AddRange(((JobComInvoiceLine)x).CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()));
				ZShort seqNum = 1;
				foreach (var chainActor in supplyChainActors)
				{
					additionalSupplyActorsList.Add(new CommonAdditionalSupplyChainActorSeqNumWrapper(chainActor, seqNum));
					seqNum++;
				}
				additionalSupplyActors = additionalSupplyActorsList.AsReadOnly();
			}
			return additionalSupplyActors;
		}
	}
	IReadOnlyCollection<CommonAdditionalSupplyChainActorSeqNumWrapper> additionalSupplyActors;

	public IAESCommonOrigin Origin => origin ?? (origin = (entryInstruction.IsSubStyleBOrC && !isComplementaryCWithMRN) ? null : new AESCommonOriginWrapper(randomLine));
	AESCommonOriginWrapper origin;

	public IDeclarationAESCommodity Commodity => commodity ?? (commodity = new DeclarationAESCommodityWrapper(entryLine));
	DeclarationAESCommodityWrapper commodity;

	public IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum> InternalPackages => internalPackages ?? (internalPackages = CommonPackageWithSequenceAndPackNumWrapper.GetPackagesList(entryLine));
	IReadOnlyCollection<CommonPackageWithSequenceAndPackNumWrapper> internalPackages;

	public IReadOnlyCollection<IAESCommonDocument> PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				var previousDocumentsList = new List<AESCommonDocumentWrapper>();

				var prevDocs = entryLine.PreviousDocuments;

				if (!prevDocs.IsNullOrEmpty())
				{
					var hasC651Doc = prevDocs.Any(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651);

					ZShort seqNum = 1;
					if (entryLine.HasPRECustomsOffice)
					{
						foreach (PreviousDocument doc in prevDocs)
						{
							PreviousDocumentHelper.AddPreviousDocumentWithLengthForGoodsItemNumber(entryLine, previousDocumentsList, doc, seqNum);
							seqNum++;
						}
					}
					else if (hasC651Doc)
					{
						var prevDocsWithC651 = prevDocs.Cast<PreviousDocument>().Where(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651);
						foreach (var doc in prevDocsWithC651)
						{
							PreviousDocumentHelper.AddPreviousDocumentWithLengthForGoodsItemNumber(entryLine, previousDocumentsList, doc, seqNum);
							seqNum++;
						}
					}
					else
					{
						var prevDocsFromOneParent = randomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
						var doc = prevDocsFromOneParent[0];
						PreviousDocumentHelper.AddPreviousDocumentWithLengthForGoodsItemNumber(entryLine, previousDocumentsList, doc, seqNum);
					}
				}
				previousDocuments = previousDocumentsList.AsReadOnly();
			}
			return previousDocuments;
		}
	}
	IReadOnlyCollection<AESCommonDocumentWrapper> previousDocuments;

	public IReadOnlyCollection<IDeclarationAESSupportingDocumentLine> SupportingDocuments
	{
		get
		{
			if (supportingDocuments == null)
			{
				var supportingDocumentsList = new List<DeclarationAESSupportingDocumentLineWrapper>();

				var supDocs = entryLine.SupportingDocuments.Cast<SupportingDocument>().Where(doc => !doc.CSI_Code.StartsWith(csiCodeStartWithY)).ToList();
				supDocs.AddRange(randomLine.InvoiceHeader.EffectiveSupportingDocuments().Cast<SupportingDocument>().Where(doc => !doc.CSI_Code.StartsWith(csiCodeStartWithY)));

				if (isProvisionalPeriod)
				{
					supDocs.AddRange(entryLine.Declaration.SupportingDocuments.Cast<SupportingDocument>().Where(doc => !doc.CSI_Code.StartsWith(csiCodeStartWithY)));
					supDocs.AddRange(entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(doc => !doc.CSI_Code.StartsWith(csiCodeStartWithY)));
					supDocs.AddRange(entryLine.GetPreviouslySentSupportingDocuments().Where(doc => doc.CSI_SubType.Equals(SupportingDocumentSubType.LIQ) && doc.CSI_Status != DocumentStatus.Accepted && !doc.CSI_Code.StartsWith(csiCodeStartWithY)));
				}

				supDocs = supDocs.OrderBy(doc => !doc.CSI_Code.IsEmpty && !char.IsDigit(doc.CSI_Code[0]) ? 0 : 1).ToList();

				ZShort seqNum = 1;
				foreach (var doc in supDocs)
				{
					supportingDocumentsList.Add(new DeclarationAESSupportingDocumentLineWrapper(doc, seqNum));
					seqNum++;
				}

				var notWantedProcedures = new ZString[] { procedureCodeF61, procedureCode144 };
				var hasLineWithNotWantedProcedure = entryLine.InvoiceLines.Any(x => ((JobComInvoiceLine)x).GetAdditionalProcedureCodesListForExportUccMessage().Any(p => notWantedProcedures.Contains(p)));
				if (isComplementaryCWithMRN && !hasLineWithNotWantedProcedure && !supportingDocumentsList.Any(doc => doc.Name == SupportingDocumentType.ComplYDoc))
				{
					supportingDocumentsList.Add(new DeclarationAESSupportingDocumentLineWrapper(SupportingDocumentType.ComplYDoc, entryLine.Header.MovementReferenceNumber, seqNum));
					seqNum++;
				}

				supportingDocuments = supportingDocumentsList.AsReadOnly();
			}
			return supportingDocuments;
		}
	}
	IReadOnlyCollection<DeclarationAESSupportingDocumentLineWrapper> supportingDocuments;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments
	{
		get
		{
			if (transportDocuments == null)
			{
				if (isProvisionalPeriod)
				{
					var addDocs = entryLine.Declaration.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments)).ToList();
					addDocs.AddRange(randomLine.InvoiceHeader.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments)));
					addDocs.AddRange(entryLine.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments)));
					addDocs.AddRange(entryInstruction.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments)));

					transportDocuments = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(addDocs);
				}
				else
				{
					transportDocuments = new List<CommonDocumentSequenceNumberWrapper>().AsReadOnly();
				}
			}
			return transportDocuments;
		}
	}
	IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> transportDocuments;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReferences
	{
		get
		{
			if (additionalReferences == null)
			{
				var supDocs = entryLine.SupportingDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Code.StartsWith(csiCodeStartWithY)).ToList();
				supDocs.AddRange(randomLine.InvoiceHeader.EffectiveSupportingDocuments().Cast<CusSupportingInfo>().Where(doc => doc.CSI_Code.StartsWith(csiCodeStartWithY)));

				if (isProvisionalPeriod)
				{
					supDocs.AddRange(entryLine.Declaration.SupportingDocuments.Cast<SupportingDocument>().Where(doc => doc.CSI_Code.StartsWith(csiCodeStartWithY)));
					supDocs.AddRange(entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(doc => doc.CSI_Code.StartsWith(csiCodeStartWithY)));
					supDocs.AddRange(entryLine.GetPreviouslySentSupportingDocuments().Where(doc => doc.CSI_SubType.Equals(SupportingDocumentSubType.LIQ) && doc.CSI_Status != DocumentStatus.Accepted && doc.CSI_Code.StartsWith(csiCodeStartWithY)));
				}
				else
				{
					supDocs.AddRange(entryLine.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalReference)));
					supDocs.AddRange(randomLine.InvoiceHeader.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalReference)));
				}

				additionalReferences = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(supDocs);
			}
			return additionalReferences;
		}
	}
	IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalReferences;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInfos
	{
		get
		{
			if (additionalInfos == null)
			{
				var infDocs = entryLine.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)).ToList();
				infDocs.AddRange(randomLine.InvoiceHeader.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)));
				
				if (isProvisionalPeriod)
				{
				infDocs.AddRange(entryInstruction.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)));
				infDocs.AddRange(entryLine.Declaration.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)));
				}

				additionalInfos = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(infDocs, shouldSendReferenceNumber: isProvisionalPeriod);
			}
			return additionalInfos;
		}
	}
	IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalInfos;

	const string csiCodeStartWithY = "Y";
	const string procedureCodeF61 = "F61";
	const string procedureCode144 = "144";
}
