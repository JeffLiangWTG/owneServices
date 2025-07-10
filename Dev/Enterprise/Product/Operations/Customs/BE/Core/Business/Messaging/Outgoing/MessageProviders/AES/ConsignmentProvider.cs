using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;
using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.BE.Business;

public class ConsignmentProvider : IConsignment
{
	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;
	readonly CusEntryHeader entryHeader;

	public ConsignmentProvider(JobDeclaration declaration, CusEntryInstruction entryInstruction, CusEntryHeader entryHeader)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	public bool ContainerIndicator => declaration.ContainerMode == Core.Constants.ContainerModes.LCL
		|| declaration.ContainerMode == Core.Constants.ContainerModes.FCL
		|| declaration.ContainerMode == Core.Constants.ContainerModes.ULD
		|| declaration.ContainerMode == Core.Constants.ContainerModes.Containerised;

	public string InlandModeOfTransport => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = declaration.CusContainers.Select((c, index) => new TransportEquipmentProvider(c, index + 1)).ToArray());
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new LocationOfGoodsProvider((CusGoodsLocation)entryInstruction.GoodsLocation, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	ILocationOfGoods locationOfGoods;

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans().ToArray());
	IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

	IEnumerable<IDepartureTransportMeans> GetDepartureTransportMeans()
	{
		if (!declaration.JE_TransportModeInland.IsEmpty)
		{
			int sequence = 1;
			yield return new DepartureTransportMeansProvider(declaration, declaration.JE_TransportIDInland, declaration.JE_RN_NKTransportNationalityInland, sequence++);

			if (declaration.JE_TransportModeInland == TransportTypeList.Codes.Road)
			{
				var id = declaration.JE_Trailer1RegNo;
				if (!id.IsEmpty)
				{
					yield return new DepartureTransportMeansProvider(declaration, declaration.JE_Trailer1RegNo, declaration.JE_RN_NKTrailer1Nationality, sequence++);
				}

				id = declaration.JE_Trailer2RegNo;
				if (!id.IsEmpty)
				{
					yield return new DepartureTransportMeansProvider(declaration, declaration.JE_Trailer2RegNo, declaration.JE_RN_NKTrailer2Nationality, sequence++);
				}
			}
		}
	}

	public ITransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans,
		() => !declaration.ZG_BorderTransportMeans.IsEmpty ? new TransportMeansProvider(declaration) : null);
	CachedValue<ITransportMeans> activeBorderTransportMeans;

	public string CarrierIdentificationNumber => GetCarrierIdentificationNumber(declaration);

	public IParty Consignee => consignee ?? (consignee = new PartyProvider(declaration.ImporterDocumentaryAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty consignee;

	public IParty Consignor => consignor ?? (consignor = new PartyProvider(declaration.SupplierDocumentaryAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty consignor;

	public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRouting => countryOfRouting ?? (countryOfRouting = declaration.ItineraryCountries.Cast<ItineraryCountry>().Select((t, index) => new CountryOfRoutingOfConsignmentProvider(t, index + 1)).ToArray());
	IReadOnlyCollection<ICountryOfRoutingOfConsignment> countryOfRouting;

	public decimal? GrossMass => WeightUnits.ConvertWeightToKilogramsIfRequired(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit);

	public string ModeOfTransportAtTheBorder => GetModeOfTransportAtTheBorder(declaration);

	public string ReferenceNumberUCR => entryHeader.InvoiceHeaders.Length > 0 ? entryHeader.InvoiceHeaders[0]?.JZ_UCR : null;

	public string TransportChargesMOP => entryHeader.InvoiceHeaders.Length > 0 ? entryHeader.InvoiceHeaders[0].ZG_TransportChargesMethodOfPayment : null;

	public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
		entryInstruction.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>()
			.Where(x => x.CSI_SubType == BEAdditionalDocTypeList.Codes.TransportDocuments)
			.Select((x, index) => new TransportDocumentProvider(x, index + 1)).ToArray<ITransportDocument>());
	IReadOnlyCollection<ITransportDocument> transportDocuments;

	public string ArrivalTransportMeansType => default;

	public string ArrivalTransportMeansId => default;

	public int TransportDocumentSequenceNumber => default;

	public string TransportDocumentType => default;

	public string TransportDocumentReferenceNumber => default;

	static ZString? GetCarrierIdentificationNumber(JobDeclaration declaration)
	{
		ZString? result = null;
		var organisation = declaration.CarrierEUBorderDocAddress?.Organisation;
		if (organisation != null)
		{
			result = organisation.GetCustomsRegNoIgnoringCountryPrefixed(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			if (result.Value.IsEmpty)
			{
				result = organisation.GetCustomsRegNoIgnoringCountryPrefixed(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
			}
		}

		return result;
	}

	static string GetModeOfTransportAtTheBorder(JobDeclaration declaration)
	{
		var result = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);

		return MessageProviderHelper.ReturnNullIfEmpty(result);
	}
}
