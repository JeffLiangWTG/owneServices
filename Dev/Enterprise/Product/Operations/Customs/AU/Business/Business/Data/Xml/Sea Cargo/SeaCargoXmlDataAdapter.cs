using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoXmlDataAdapter<TBusinessObject, TValueObject> : BaseCargoXmlDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : CusSCAOceanBill
		where TValueObject : Xsd.Consol
	{
		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
		{
			CusSCAOceanBill oceanBill = bizObj;
			Xsd.Consol consolValue = value;
			ImportOceanBill(oceanBill, consolValue, context);

			if (consolValue.ConsolDetail.Containers.IsSpecified)
			{
				foreach (Xsd.Container containerXsd in consolValue.ConsolDetail.Containers)
				{
					CusSCAContainer container = oceanBill.Containers.AddNew();
					ImportContainer(container, containerXsd, context);
				}
			}

			if (consolValue.Shipments.IsSpecified)
			{
				ImportHouseBills(oceanBill, consolValue.Shipments, context);
			}
		}

		#endregion

		#region OceanBill

		void ImportOceanBill(CusSCAOceanBill oceanBill, Xsd.Consol consolXsd, IValueObjectImportContext context)
		{
			if (consolXsd.ConsolDetail.IsSpecified)
			{
				Xsd.ConsolIdentifier consolIdentifier = consolXsd.ConsolIdentifier.FindFirst(Xsd.ConsolIdentifierType.MasterWaybill);
				if (consolIdentifier != null)
				{
					context.SetPropertyInfoValue(oceanBill.CB_OceanBillInfo, consolIdentifier.Value, consolIdentifier.ValueSpecified);
				}

				Xsd.ConsolIdentifier masterHouseIdentifier = consolXsd.ConsolIdentifier.FindFirst(Xsd.ConsolIdentifierType.Other);
				if (masterHouseIdentifier != null)
				{
					context.SetPropertyInfoValue(oceanBill.CB_MasterHouseBillInfo, masterHouseIdentifier.Value, masterHouseIdentifier.ValueSpecified);
				}

				context.SetPropertyInfoValue(oceanBill.CB_RL_NKPortOfLoadingInfo, consolXsd.ConsolDetail.PortOfLoading.Port.Value, consolXsd.ConsolDetail.PortOfLoading.Port.ValueSpecified);
				context.SetPropertyInfoValue(oceanBill.CB_RL_NKPortOfDischargeInfo, consolXsd.ConsolDetail.PortOfDischarge.Port.Value, consolXsd.ConsolDetail.PortOfDischarge.Port.ValueSpecified);
				context.SetPropertyInfoValue(oceanBill.CB_RL_NKPortOfFirstArrivalInfo, consolXsd.ConsolDetail.PortFirstArrival.Port.Value, consolXsd.ConsolDetail.PortFirstArrival.Port.ValueSpecified);

				Xsd.SailingWithVesselVoyage voyage = consolXsd.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;
				if (voyage != null)
				{
					context.SetPropertyInfoValue(oceanBill.CB_VoyageInfo, voyage.VoyageNo, voyage.VoyageNoSpecified);
					context.SetPropertyInfoValue(oceanBill.CB_VesselNameInfo, voyage.VesselName, voyage.VesselNameSpecified);
					context.SetPropertyInfoValue(oceanBill.CB_LloydsIMOInfo, voyage.LloydsNo, voyage.LloydsNoSpecified);
					oceanBill.CB_DateOfArrival = voyage.ETA;
				}

				if (consolXsd.ConsolDetail.ReceivingAgent != null &&
					consolXsd.ConsolDetail.ReceivingAgent.OrganisationDetails != null &&
					consolXsd.ConsolDetail.ReceivingAgent.OrganisationDetails.RegistrationNumbers.Count > 0)
				{
					Xsd.RegistrationNumber aBN = consolXsd.ConsolDetail.ReceivingAgent.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.GST, "AU");
					if (aBN != null && aBN.IsSpecified)
					{
						oceanBill.CB_PrincipalID = aBN.Number.Replace(" ", "").Substring(0, oceanBill.CB_PrincipalIDInfo.MaxLength);
					}
				}

				PopulateNotes(oceanBill.Notes, consolXsd.Notes, context);
			}
		}

		#endregion

		#region HouseBill

		#region Import HouseBills

		protected virtual void ImportHouseBills(CusSCAOceanBill oceanBill, Xsd.ShipmentCollection shipments, IValueObjectImportContext context)
		{
			foreach (Xsd.Shipment houseBillXsd in shipments)
			{
				CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
				ImportHouseBill(houseBill, houseBillXsd, context);
				foreach (Xsd.Package packageXsd in houseBillXsd.ShipmentDetails.Packages)
				{
					CusSCAPivot package = houseBill.Pivot.AddNew();
					ImportPackage(package, houseBillXsd, packageXsd, context);

					CusSCAContainer container = FindContainerByContainerNumber(oceanBill, packageXsd.ContainerNumber);
					if (container != null)
					{
						container.Pivots.Add(package);
					}
				}
			}
		}

		#endregion

		#region ImportHouseBill

		protected void ImportHouseBill(CusSCAHouse houseBill, Xsd.Shipment houseBillXsd, IValueObjectImportContext context)
		{
			if (houseBillXsd.ShipmentDetails.IsSpecified)
			{
				context.SetPropertyInfoValue(houseBill.CA_HouseBillInfo, GetHouseBillOrEmptyFromHouseBillXsd(houseBillXsd, Xsd.ShipmentIdentifierType.Housebill), houseBillXsd.ShipmentDetails.IsSpecified);
				context.SetPropertyInfoValue(houseBill.CA_MasterHouseBillInfo, GetHouseBillOrEmptyFromHouseBillXsd(houseBillXsd, Xsd.ShipmentIdentifierType.CoLoadMaster), houseBillXsd.ShipmentDetails.IsSpecified);

				context.SetPropertyInfoValue(houseBill.CA_RL_NK_PortOfOriginInfo, houseBillXsd.ShipmentDetails.PortOfOrigin.Port.Value, houseBillXsd.ShipmentDetails.PortOfOrigin.Port.ValueSpecified);
				context.SetPropertyInfoValue(houseBill.CA_RL_NK_PortOfDestinationInfo, houseBillXsd.ShipmentDetails.PortofDestination.Port.Value, houseBillXsd.ShipmentDetails.PortofDestination.Port.ValueSpecified);

				PopulateConsignor(houseBill, houseBillXsd.ShipmentDetails.Consignor.OrganisationDetails, context);
				PopulateConsignee(houseBill, houseBillXsd.ShipmentDetails.Consignee.OrganisationDetails, context);
				PopulateNotifyParty(houseBill, houseBillXsd.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails, context);
				PopulateFromDeclaration(houseBill, houseBillXsd.Declaration, context);
				PopulateNotes(houseBill.Notes, houseBillXsd.Notes, context);
			}
		}
		#endregion

		#region PopulateConsignor
		void PopulateConsignor(CusSCAHouse houseBill, Xsd.OrganisationDetail consignor, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(houseBill.CA_ConsignorNameInfo, consignor.Name, consignor.NameSpecified);
			if (consignor.Addresses.Count > 0)
			{
				Xsd.OrgAddress consignorMainAddress = consignor.Addresses[0];
				context.SetPropertyInfoValue(houseBill.CA_ConsignorAddress1Info, consignorMainAddress.AddressLine1, consignorMainAddress.AddressLine1Specified);

				PopulateAddress2SuburbDetails(houseBill.CA_ConsignorAddress2Info, houseBill.CA_ConsignorSuburbInfo,
					consignorMainAddress.AddressLine2, consignorMainAddress.CityOrSuburb, context);

				context.SetPropertyInfoValue(houseBill.CA_ConsignorPostcodeInfo, consignorMainAddress.PostCode, consignorMainAddress.PostCodeSpecified);
			}
			if (!consignor.Location.Country.IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryName(houseBill.Factory, consignor.Location.Country);
				if (country != null)
				{
					houseBill.CA_RN_NKConsignorCountryCode = country.RN_Code;
				}
			}
		}
		#endregion

		#region PopulateConsignee
		void PopulateConsignee(CusSCAHouse houseBill, Xsd.OrganisationDetail consignee, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(houseBill.CA_ConsigneeNameInfo, consignee.Name, consignee.NameSpecified);
			if (consignee.Addresses.Count > 0)
			{
				Xsd.OrgAddress consigneeMainAddress = consignee.Addresses[0];
				context.SetPropertyInfoValue(houseBill.CA_ConsigneeAddress1Info, consigneeMainAddress.AddressLine1, consigneeMainAddress.AddressLine1Specified);

				PopulateAddress2SuburbDetails(houseBill.CA_ConsigneeAddress2Info, houseBill.CA_ConsigneeSuburbInfo,
					consigneeMainAddress.AddressLine2, consigneeMainAddress.CityOrSuburb, context);
				context.SetPropertyInfoValue(houseBill.CA_ConsigneePostcodeInfo, consigneeMainAddress.PostCode, consigneeMainAddress.PostCodeSpecified);

				ZString phone = consigneeMainAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business);
				if (phone.IsEmpty)
				{
					phone = consigneeMainAddress.TelephoneNumbers.AddNew().Value;
				}
				context.SetPropertyInfoValueIfValueNotEmpty(houseBill.CA_ConsigneePhoneInfo, phone);

				ZString fax = consigneeMainAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax);
				if (fax.IsEmpty)
				{
					fax = consigneeMainAddress.TelephoneNumbers.AddNew().Value;
				}
				context.SetPropertyInfoValueIfValueNotEmpty(houseBill.CA_ConsigneeFaxInfo, fax);
			}
			if (!consignee.Location.Country.IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryName(houseBill.Factory, consignee.Location.Country);
				if (country != null)
				{
					houseBill.CA_RN_NKConsigneeCountryCode = country.RN_Code;
				}
			}
		}
		#endregion

		#region PopulateFromDeclaration
		void PopulateFromDeclaration(CusSCAHouse houseBill, Xsd.Declaration declaration, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(houseBill.CA_RN_NKGoodsOriginInfo, declaration.GoodsOrigin, declaration.GoodsOriginSpecified);

			string paymentTerms = declaration.PaymentTerms;
			if (houseBill.OceanBill.CB_ApplicationCode == Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages)
			{
				if (declaration.PaymentTerms == "PPD")
				{
					paymentTerms = CMRMethodsOfPayment.Codes.PrepaidOnly;
				}
				else if (declaration.PaymentTerms == "COL")
				{
					paymentTerms = CMRMethodsOfPayment.Codes.Collect;
				}
			}
			context.SetPropertyInfoValueIfValueNotEmpty(houseBill.CA_PrepaidCollectOtherInfo, paymentTerms);
		}
		#endregion

		#region PopulateNotifyParty
		void PopulateNotifyParty(CusSCAHouse houseBill, Xsd.OrganisationDetail notifyParty, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(houseBill.CA_NotifyNameInfo, notifyParty.Name, notifyParty.NameSpecified);
			if (notifyParty.Addresses.Count > 0)
			{
				Xsd.OrgAddress notifyPartyMainAddress = notifyParty.Addresses[0];
				context.SetPropertyInfoValue(houseBill.CA_NotifyAddress1Info, notifyPartyMainAddress.AddressLine1, notifyPartyMainAddress.AddressLine1Specified);
				context.SetPropertyInfoValue(houseBill.CA_NotifyAddress2Info, notifyPartyMainAddress.AddressLine2, notifyPartyMainAddress.AddressLine2Specified);
				context.SetPropertyInfoValue(houseBill.CA_NotifySuburbInfo, notifyPartyMainAddress.CityOrSuburb, notifyPartyMainAddress.CityOrSuburbSpecified);
				context.SetPropertyInfoValue(houseBill.CA_NotifyPostcodeInfo, notifyPartyMainAddress.PostCode, notifyPartyMainAddress.PostCodeSpecified);

				ZString phone = notifyPartyMainAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business);
				if (phone.IsEmpty)
				{
					phone = notifyPartyMainAddress.TelephoneNumbers.AddNew().Value;
				}
				context.SetPropertyInfoValueIfValueNotEmpty(houseBill.CA_NotifyPhoneInfo, phone);

				ZString fax = notifyPartyMainAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax);
				if (fax.IsEmpty)
				{
					fax = notifyPartyMainAddress.TelephoneNumbers.AddNew().Value;
				}
				context.SetPropertyInfoValueIfValueNotEmpty(houseBill.CA_NotifyFaxInfo, fax);
			}

			if (!notifyParty.Location.Country.IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryName(houseBill.Factory, notifyParty.Location.Country);
				if (country != null)
				{
					houseBill.CA_RN_NKNotifyCountryCode = country.RN_Code;
				}
			}
		}
		#endregion

		#region GetHouseBillOrEmptyFromHouseBillXsd

		protected ZString GetHouseBillOrEmptyFromHouseBillXsd(Xsd.Shipment houseBillXsd, Xsd.ShipmentIdentifierType idType)
		{
			ZString houseBill = "";
			if (houseBillXsd.ShipmentDetails.IsSpecified)
			{
				Xsd.ShipmentIdentifier shipmentIdentifier = houseBillXsd.ShipmentIdentifier.FindFirst(idType);
				if (shipmentIdentifier != null)
				{
					houseBill = shipmentIdentifier.Value;
				}
			}
			return houseBill;
		}
		#endregion

		#endregion

		#region Containers

		protected void ImportContainer(CusSCAContainer container, Xsd.Container containerXsd, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(container.CN_ContainerNumberInfo, containerXsd.ContainerNumber, containerXsd.ContainerNumberSpecified);
			context.SetPropertyInfoValue(container.CN_SealNumberInfo, containerXsd.Seal, containerXsd.SealSpecified);
			context.SetPropertyInfoValueIfValueNotEmpty(container.CN_ContainerModeInfo, containerXsd.PackingMode.ToString());
			container.CN_ShipperOwnedContainer = containerXsd.IsShipperOwnedContainer;
			context.SetPropertyInfoValue(container.CN_ContainerSizeOrISOCodeInfo, containerXsd.ContainerType.ISOCode, containerXsd.ContainerType.ISOCodeSpecified);
			if (!containerXsd.ContainerType.USContainerCode.IsEmpty)
			{
				container.CN_TypeOfContainer = containerXsd.ContainerType.USContainerCode;
			}
		}

		protected CusSCAContainer FindContainerByContainerNumber(CusSCAOceanBill oceanBill, string containerNumber)
		{
			foreach (CusSCAContainer container in oceanBill.Containers)
			{
				if (container.CN_ContainerNumber.EqualsIgnoringCase(containerNumber))
				{
					return container;
				}
			}
			return null;
		}

		#endregion

		#region Package

		protected void ImportPackage(CusSCAPivot package, Xsd.Shipment houseBillXsd, Xsd.Package packageXsd, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(package.CV_AssociatedContainerInfo, packageXsd.ContainerNumber, packageXsd.ContainerNumberSpecified);
			context.SetPropertyInfoValueIfValueNotEmpty(package.CV_AssociatedHouseInfo, package.HouseBill.CA_HouseBill);
			context.SetPropertyInfoValueIfValueNotEmpty(package.CV_PackageCountInfo, packageXsd.NumberOfPacks.ToString());
			ImportPackageType(packageXsd.PackType, package, context);
			context.SetPropertyInfoValueIfValueNotEmpty(package.CV_VolumeInfo, packageXsd.Volume.Value.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(package.CV_WeightInfo, packageXsd.Weight.Value.ToString());
			context.SetPropertyInfoValue(package.CV_WeightUQInfo, packageXsd.Weight.DimensionType, packageXsd.Weight.DimensionTypeSpecified);
			context.SetPropertyInfoValue(package.CV_GoodsDescriptionInfo, packageXsd.GoodsDescription, packageXsd.GoodsDescriptionSpecified);
			context.SetPropertyInfoValue(package.CV_MarksAndNumbersInfo, houseBillXsd.ShipmentDetails.MarksAndNumbers, houseBillXsd.ShipmentDetails.MarksAndNumbersSpecified);
			package.CV_IsSAC = houseBillXsd.Declaration.IsSAC;
			package.CV_FumigationCert = houseBillXsd.Declaration.IsFumigationCert;
			package.CV_HazardousGoods = houseBillXsd.Declaration.IsHazardousGoods;
			package.CV_PerishableGoods = houseBillXsd.Declaration.IsPerishableGoods;
			package.CV_PersonalEffects = houseBillXsd.Declaration.IsPersonalEffects;
			package.CV_IsDocuments = houseBillXsd.Declaration.IsDocuments == Xsd.TrueFalse.@true;
			package.CV_Timber = houseBillXsd.Declaration.IsTimber;
		}

		protected virtual void ImportPackageType(ZString packType, CusSCAPivot package, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(package.CV_PackageTypeInfo, packType);
		}

		#endregion
	}
}
