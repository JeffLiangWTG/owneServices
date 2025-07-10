using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESConsignmentWrapper : AESCommonConsignmentWrapper, IDeclarationAESConsignment
{
	public DeclarationAESConsignmentWrapper(CusEntryHeader entryHeader, ZBool shouldDeclareConsignorInConsignment, ZBool shouldDeclareConsigneeInConsignment, bool isComplementaryCWithMRN) : base(entryHeader, isComplementaryCWithMRN)
	{
		this.shouldDeclareConsignorInConsignment = shouldDeclareConsignorInConsignment;
		this.shouldDeclareConsigneeInConsignment = shouldDeclareConsigneeInConsignment;
		randomInvoiceLine = ((CusEntryLine)entryHeader.RandomEntryLine).RandomLine;

		isProvisionalPeriod = declaration?.IsTransitionPeriodAES30 ?? false;
		modeOfTransportAtBorder = (entryInstruction.IsSubStyleBOrC && !isComplementaryCWithMRN)
						? ZString.Empty
						: declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, returnOriginalInput: false);
	}
	readonly ZBool shouldDeclareConsignorInConsignment;
	readonly ZBool shouldDeclareConsigneeInConsignment;
	readonly JobComInvoiceLine randomInvoiceLine;
	readonly ZBool isProvisionalPeriod;
	readonly ZString modeOfTransportAtBorder;

	public ZString ModeOfTransportAtBorder => modeOfTransportAtBorder;

	public ZDecimal GrossMass => (ZDecimal)entryHeader.MergedLines.Sum(x => x.EffectiveGrossWeight.InKilogramsSafe);

	public ZString ReferenceNumberUCR => declaration.JE_OwnerRef;

	public IPartyIdProvider Carrier => carrier ?? (carrier = declaration.JE_OH_ShippingLine != declaration.DeclarantAddress?.OA_OH ? PartyIdWrapper.New(entryHeader.Declaration.ShippingLine) : null);
	PartyIdWrapper carrier;

	public IPartyIdProvider Consignor => consignor ?? (consignor = shouldDeclareConsignorInConsignment ? PartyIdWrapper.New(randomInvoiceLine.ExporterAddress) : null);
	PartyIdWrapper consignor;

	public IDeclarationAESConsignee Consignee
	{
		get
		{
			if (consignee == null)
			{
				if (shouldDeclareConsigneeInConsignment)
				{
					var header = entryHeader.RandomHeader;
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

	public IReadOnlyCollection<IAESCommonTransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader));
	IReadOnlyCollection<AESCommonTransportEquipmentWrapper> transportEquipment;

	public IAESCommonLocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new AESCommonLocationOfGoodsWrapper(entryInstruction));
	AESCommonLocationOfGoodsWrapper locationOfGoods;

	public IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
	IReadOnlyCollection<CommonDepartureTransportMeansWrapper> departureTransportMeans;

	public IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments
	{
		get
		{
			if (countryOfRoutingOfConsignments == null)
			{
				var countryOfRoutingOfConsignmentList = new List<CommonCountryOfRoutingOfConsignmentWrapper>();

				if (entryInstruction.IncludeRoutingSecurityData)
				{
					ZShort seqNum = 1;
					foreach (var country in entryHeader.CountriesOfRouting)
					{
						countryOfRoutingOfConsignmentList.Add(new CommonCountryOfRoutingOfConsignmentWrapper(seqNum, country));
						seqNum++;
					}
				}

				countryOfRoutingOfConsignments = countryOfRoutingOfConsignmentList.AsReadOnly();
			}
			return countryOfRoutingOfConsignments;
		}
	}
	IReadOnlyCollection<CommonCountryOfRoutingOfConsignmentWrapper> countryOfRoutingOfConsignments;

	public ITransportMediumInfoCommon ActiveBorderTransportMeans => activeBorderTransportMeans ??= !modeOfTransportAtBorder.IsEmpty ? AESWrappersHelper.GetActiveBorderTransportMeans(declaration) : null;
	TransportMediumInfoCommonWrapper activeBorderTransportMeans;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments
	{
		get
		{
			if (transportDocuments == null)
			{
				if (!isProvisionalPeriod)
				{
					var addDocs = declaration.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.TransportDocuments).ToList();
					entryHeader.InvoiceHeaders.ForEach(x => addDocs.AddRange(x.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.TransportDocuments)));
					entryHeader.MergedLines.ForEach(x => addDocs.AddRange(x.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.TransportDocuments)));
					addDocs.AddRange(entryInstruction.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.TransportDocuments));

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

	public ZString TransportChargesMoP => (entryInstruction.IsSubStyleBOrC && !isComplementaryCWithMRN) ? ZString.Empty : entryHeader.RandomHeader.ZG_TransportChargesMethodOfPayment;
}
