using System;
using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public static class EMCSJobDeclarationUpdateHelper
	{
		public static void CreateOrUpdateGuarantor(this EMCSJobDeclaration emcsDeclaration, IEMCSPartyGuarantor guarantorInMessage)
		{
			emcsDeclaration.OwnerDocumentaryAddress.Delete();
			if (guarantorInMessage != null)
			{
				var factory = emcsDeclaration.Factory;
				var owner = emcsDeclaration.OwnerDocumentaryAddress;
				var traderExciseNumber = guarantorInMessage.TraderExciseNumber;
				var vatNumber = guarantorInMessage.VatNumber;
				var countryCode = guarantorInMessage.Country;
				var vatCodeType = ((ZString)countryCode).GetVATCodeType();
				var orgHeader = factory.GetOrgHeaderByCustomsRegNo(countryCode, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, traderExciseNumber) ?? factory.GetOrgHeaderByCustomsRegNo(countryCode, vatCodeType, vatNumber);
				if (orgHeader != null)
				{
					owner.OrganisationPK = orgHeader.PK;
				}
				else
				{
					owner.E2_AddressOverride = true;
					if (!traderExciseNumber.IsEmpty())
					{
						owner.E2_GovRegNum = traderExciseNumber;
						owner.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
					}
					else if (!vatNumber.IsEmpty())
					{
						owner.E2_GovRegNum = vatNumber;
						owner.E2_GovRegNumType = vatCodeType;
					}
					UpdatePartyAddress(owner, guarantorInMessage);
				}
			}
		}

		public static void CreateOrUpdateDeliveryPlace(this EMCSJobDeclaration emcsDeclaration, IEMCSPartyDeliveryPlace deliveryPlaceInMessage)
		{
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.Delete();
			if (deliveryPlaceInMessage != null)
			{
				var destinationWarehouse = emcsDeclaration.DestinationWarehouseDocumentaryAddress;
				var traderId = deliveryPlaceInMessage.TraderId;
				var orgAddress = emcsDeclaration.Factory.GetOrgAddressByCustomsRegNo(deliveryPlaceInMessage.Country, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, traderId);
				if (orgAddress != null)
				{
					destinationWarehouse.OrganisationPK = orgAddress.OA_OH;
					destinationWarehouse.E2_OA_Address = orgAddress.PK;
				}
				else
				{
					destinationWarehouse.E2_AddressOverride = true;
					if (!traderId.IsEmpty())
					{
						destinationWarehouse.E2_GovRegNum = traderId;
						destinationWarehouse.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
					}
					UpdatePartyAddress(destinationWarehouse, deliveryPlaceInMessage);
				}
			}
		}

		public static void CreateOrUpdateCarrierAgent(this EMCSJobDeclaration emcsDeclaration, IEMCSPartyTransporter transportInMessage) =>
			emcsDeclaration.CreateOrUpdateTransportParty(() => emcsDeclaration.CarrierAgentDocumentaryAddress, transportInMessage, () => emcsDeclaration.CarrierAgentDocumentaryAddress.Delete());

		public static void CreateOrUpdateTransporter(this EMCSJobDeclaration emcsDeclaration, IEMCSPartyTransporter transportInMessage) =>
			emcsDeclaration.CreateOrUpdateTransportParty(() => emcsDeclaration.TransporterDocumentaryAddress, transportInMessage, () => emcsDeclaration.TransporterDocumentaryAddress.Delete());

		public static void CreateOrUpdateTransportDetails(this EMCSJobDeclaration emcsDeclaration, IReadOnlyCollection<IEMCSTransportDetails> transportDetails)
		{
			emcsDeclaration.CusContainers.RemoveAndDeleteAll();
			foreach (var transportDetail in transportDetails)
			{
				var transport = emcsDeclaration.CusContainers.AddNew();
				transport.ZG_UnitCode = transportDetail.UnitCode;
				transport.CO_ContainerNumber = transportDetail.IdentityOfUnit.Left(CusContainer.Schema.CO_ContainerNumberMaxLength);
				transport.CO_Seal = transportDetail.CommercialSealIdentification.Left(CusContainer.Schema.CO_SealMaxLength);
				transport.SealDetails = transportDetail.SealInformation;
				transport.Comment = transportDetail.ComplementaryInformation;
			}
		}

		static void CreateOrUpdateTransportParty(this EMCSJobDeclaration emcsDeclaration, Func<JobDocAddress> getTransportParty, IEMCSPartyTransporter transportInMessage, Action deleteTransportParty)
		{
			deleteTransportParty.Invoke();
			if (transportInMessage != null)
			{
				var transportParty = getTransportParty.Invoke();
				var vatNumber = transportInMessage.VatNumber;
				var countryCode = transportInMessage.Country;
				var vatCodeType = ((ZString)countryCode).GetVATCodeType();
				var orgHeader = emcsDeclaration.Factory.GetOrgHeaderByCustomsRegNo(countryCode, vatCodeType, vatNumber);
				if (orgHeader != null)
				{
					transportParty.OrganisationPK = orgHeader.PK;
				}
				else
				{
					transportParty.E2_AddressOverride = true;
					if (!vatNumber.IsEmpty())
					{
						transportParty.E2_GovRegNum = vatNumber;
						transportParty.E2_GovRegNumType = vatCodeType;
					}
					UpdatePartyAddress(transportParty, transportInMessage);
				}
			}
		}

		public static void UpdatePartyAddress(JobDocAddress jobDocAddress, IEMCSPartyAddress addressInMessage)
		{
			jobDocAddress.E2_RN_NKCountryCode = addressInMessage.Country;
			jobDocAddress.E2_City = addressInMessage.City;
			jobDocAddress.E2_Address1 = addressInMessage.Address;
			jobDocAddress.E2_Postcode = addressInMessage.Postcode;
			jobDocAddress.E2_CompanyName = addressInMessage.Name;
		}
	}
}
