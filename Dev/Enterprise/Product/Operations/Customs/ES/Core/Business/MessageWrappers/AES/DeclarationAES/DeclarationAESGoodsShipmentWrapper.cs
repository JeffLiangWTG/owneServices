using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESGoodsShipmentWrapper : AESCommonGoodsShipmentWrapper, IDeclarationAESGoodsShipment
{
	public DeclarationAESGoodsShipmentWrapper(CusEntryHeader entryHeader, bool isComplementaryCWithMRN) : base(entryHeader, isComplementaryCWithMRN: isComplementaryCWithMRN)
	{
		invoiceHeader = entryHeader.RandomHeader;
		shouldDeclareConsignorInConsignment = ShouldDeclareConsignorInConsignment();
		shouldDeclareConsigneeInConsignment = ShouldDeclareConsigneeInConsignment();

		isProvisionalPeriod = declaration?.IsTransitionPeriodAES30 ?? false;
	}
	readonly JobComInvoiceHeader invoiceHeader;
	readonly ZBool shouldDeclareConsignorInConsignment;
	readonly ZBool shouldDeclareConsigneeInConsignment;
	readonly ZBool isProvisionalPeriod;

	public ZString NatureOfTransaction => (entryInstruction.IsSubStyleBOrC && !isComplementaryCWithMRN) ? ZString.Empty : invoiceHeader.JZ_ValuationCode;

	public ZString CountryOfExport => declaration.JE_GoodsOrigin;

	public ZString CountryOfDestination => declaration.JE_GoodsDestination;

	public IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyActors
	{
		get
		{
			if (additionalSupplyActors == null)
			{
				var additionalSupplyActorsList = new List<CommonAdditionalSupplyChainActorSeqNumWrapper>();

				ZShort seqNum = 1;
				foreach (var chainActor in entryInstruction.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>())
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

	public ICommonDeliveryTerms DeliveryTerms => deliveryTerms ?? (deliveryTerms = (entryInstruction.IsSubStyleBOrC && !isComplementaryCWithMRN) ? null : new CommonDeliveryTermsWrapper(entryHeader));
	CommonDeliveryTermsWrapper deliveryTerms;

	public IReadOnlyCollection<IDeclarationAESSupportingDocumentHeader> SupportingDocuments
	{
		get
		{
			if (supportingDocuments == null)
			{
				var supportingDocumentsList = new List<DeclarationAESSupportingDocumentHeaderWrapper>();

				if (!isProvisionalPeriod)
				{
					var supDocs = declaration.SupportingDocuments.Cast<SupportingDocument>().Where(doc => !doc.CSI_Code.StartsWith(csiCodeStartWithY)).ToList();
					supDocs.AddRange(entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(doc => !doc.CSI_Code.StartsWith(csiCodeStartWithY)));

					supDocs = supDocs.OrderBy(doc => !doc.CSI_Code.IsEmpty && !char.IsDigit(doc.CSI_Code[0]) ? 0 : 1).ToList();

					ZShort seqNum = 1;
					foreach (var doc in supDocs)
					{
						supportingDocumentsList.Add(new DeclarationAESSupportingDocumentHeaderWrapper(doc, seqNum));
						seqNum++;
					}
				}
				supportingDocuments = supportingDocumentsList.AsReadOnly();
			}
			return supportingDocuments;
		}
	}
	IReadOnlyCollection<DeclarationAESSupportingDocumentHeaderWrapper> supportingDocuments;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReferences
	{
		get
		{
			if (additionalReferences == null)
			{
				if (!isProvisionalPeriod)
				{
					var supDocs = declaration.SupportingDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Code.StartsWith(csiCodeStartWithY)).ToList();
					supDocs.AddRange(entryInstruction.SupportingDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Code.StartsWith(csiCodeStartWithY)));

					supDocs.AddRange(entryInstruction.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalReference)));
					supDocs.AddRange(declaration.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalReference)));

					additionalReferences = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(supDocs);
				}
				else
				{
					additionalReferences = new List<CommonDocumentSequenceNumberWrapper>().AsReadOnly();
				}
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
				if (!isProvisionalPeriod)
				{  
					var infDocs = declaration.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)).ToList();
					infDocs.AddRange(entryInstruction.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)));

					additionalInfos = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(infDocs);
				}
				else
				{
					additionalInfos = new List<CommonDocumentSequenceNumberWrapper>().AsReadOnly();
				}
			}
			return additionalInfos;
		}
	}
	IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalInfos;

	public IDeclarationAESConsignment Consignment => consignment ?? (consignment = new DeclarationAESConsignmentWrapper(entryHeader, shouldDeclareConsignorInConsignment, shouldDeclareConsigneeInConsignment, isComplementaryCWithMRN));
	DeclarationAESConsignmentWrapper consignment;

	public IReadOnlyCollection<IDeclarationAESLine> Lines => lines ?? (lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new DeclarationAESLineWrapper(x, !shouldDeclareConsignorInConsignment, !shouldDeclareConsigneeInConsignment, isComplementaryCWithMRN)).ToList().AsReadOnly());
	IReadOnlyCollection<DeclarationAESLineWrapper> lines;

	ZBool ShouldDeclareConsignorInConsignment()
	{
		var consignorInLines = entryHeader.MergedLines
			.SelectMany(x => x.InvoiceLines
			.Select(y => y.JI_OA_ExporterAddress))
			.Distinct();

		return consignorInLines.Count() == 1;
	}

	ZBool ShouldDeclareConsigneeInConsignment()
	{
		var consigneeInLines = entryHeader.MergedLines
			.SelectMany(x => x.InvoiceLines
			.Select(y => y.InvoiceHeader.JZ_OH_Buyer))
			.Distinct();

		return consigneeInLines.Count() == 1;
	}

	const string csiCodeStartWithY = "Y";
}
