using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoValueObjectDataAdapter : BaseCargoXmlDataAdapter<CusMAWB, Xsd.Consol>
	{
		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(CusMAWB bizObj, Xsd.Consol value, IValueObjectImportContext context)
		{
			ImportMAWB(bizObj, value, context);

			foreach (Xsd.Shipment shipmentXml in value.Shipments)
			{
				ImportHAWB(bizObj.ChildBills.AddNew(), shipmentXml, context);
			}
		}

		#endregion

		#region ImportMAWB

		public void ImportMAWB(CusMAWB cusMAWB, Xsd.Consol consol, IValueObjectImportContext context)
		{
			if (consol.ConsolDetail.IsSpecified)
			{
				Xsd.ConsolIdentifier consolIdentifier = consol.ConsolIdentifier.FindFirst(Xsd.ConsolIdentifierType.MasterWaybill);
				if (consolIdentifier != null)
				{
					context.SetPropertyInfoValue(cusMAWB.CM_MAWBInfo, consolIdentifier.Value, consolIdentifier.ValueSpecified);
				}

				Xsd.ConsolIdentifier masterHouseBill = consol.ConsolIdentifier.FindFirst(Xsd.ConsolIdentifierType.Other);
				if (masterHouseBill != null)
				{
					context.SetPropertyInfoValue(cusMAWB.CM_MasterHouseBillInfo, masterHouseBill.Value, masterHouseBill.ValueSpecified);
				}

				context.SetPropertyInfoValue(cusMAWB.CM_RL_NKLoadPortInfo, consol.ConsolDetail.PortOfLoading.Port.Value, consol.ConsolDetail.PortOfLoading.Port.ValueSpecified);
				context.SetPropertyInfoValue(cusMAWB.CM_RL_NKDischargePortInfo, consol.ConsolDetail.PortOfDischarge.Port.Value, consol.ConsolDetail.PortOfDischarge.Port.ValueSpecified);
				context.SetPropertyInfoValue(cusMAWB.CM_RL_NKFirstArrivalPortInfo, consol.ConsolDetail.PortFirstArrival.Port.Value, consol.ConsolDetail.PortFirstArrival.Port.ValueSpecified);

				Xsd.FlightWithFlightNumber roadRailFlight = consol.ConsolDetail.Item as Xsd.FlightWithFlightNumber;
				if (roadRailFlight != null)
				{
					context.SetPropertyInfoValue(cusMAWB.CM_FlightNoInfo, roadRailFlight.FlightNoJourneyNoTruckRegNo, roadRailFlight.FlightNoJourneyNoTruckRegNoSpecified);
					cusMAWB.CM_ArrivalDate = roadRailFlight.ETA;
				}

				PopulateNotes(cusMAWB.Notes, consol.Notes, context);
			}
		}

		public void ImportHAWB(CusHAWB cusHAWB, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			Xsd.ShipmentIdentifier housebillIdentifier = shipment.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.Housebill);
			if (housebillIdentifier != null)
			{
				context.SetPropertyInfoValue(cusHAWB.CS_HAWBInfo, housebillIdentifier.Value, housebillIdentifier.ValueSpecified);
			}
			context.SetPropertyInfoValue(cusHAWB.CS_RL_NKOriginInfo, shipment.ShipmentDetails.PortOfOrigin.Port.Value, shipment.ShipmentDetails.PortOfOrigin.Port.ValueSpecified);
			context.SetPropertyInfoValue(cusHAWB.CS_RL_NKDestinationInfo, shipment.ShipmentDetails.PortofDestination.Port.Value, shipment.ShipmentDetails.PortofDestination.Port.ValueSpecified);

			Xsd.ShipmentIdentifier coLoadMasterIdentifier = shipment.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.CoLoadMaster);
			if (coLoadMasterIdentifier != null)
			{
				context.SetPropertyInfoValue(cusHAWB.CS_MasterHouseBillInfo, coLoadMasterIdentifier.Value, coLoadMasterIdentifier.ValueSpecified);
			}

			ImportHAWBDetails(cusHAWB, shipment, context);

			PopulateConsignor(cusHAWB, shipment.ShipmentDetails.Consignor, context);
			PopulateConsignee(cusHAWB, shipment.ShipmentDetails.Consignee, context);
			PopulateFromDeclaration(cusHAWB, shipment.Declaration, context);
			PopulateNotes(cusHAWB.Notes, shipment.Notes, context);
		}

		void ImportHAWBDetails(CusHAWB cusHAWB, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(cusHAWB.CS_WeightInfo, shipment.ShipmentDetails.Weight.Value.ToString());
			context.SetPropertyInfoValue(cusHAWB.CS_WeightUQInfo, shipment.ShipmentDetails.Weight.DimensionType, shipment.ShipmentDetails.Weight.DimensionTypeSpecified);
			cusHAWB.CS_ChargableWeight = shipment.ShipmentDetails.ChargeableWeight.Value;

			cusHAWB.CS_GoodsValue = shipment.ShipmentDetails.GoodsValue.Value;
			context.SetPropertyInfoValue(cusHAWB.CS_RX_NKGoodsCurrencyInfo, shipment.ShipmentDetails.GoodsValue.CurrencyCode, shipment.ShipmentDetails.GoodsValue.CurrencyCodeSpecified);
			context.SetPropertyInfoValue(cusHAWB.CS_GoodsDescriptionInfo, shipment.ShipmentDetails.GoodsDescription, shipment.ShipmentDetails.GoodsDescriptionSpecified);
			cusHAWB.CS_PiecesManifested = (ZShort)shipment.ShipmentDetails.TotalOuterPacksQty.Value.ToZInt();
			context.SetPropertyInfoValue(cusHAWB.CS_RS_NK_ServiceLevelInfo, shipment.ShipmentDetails.ServiceLevel, shipment.ShipmentDetails.ServiceLevelSpecified);
		}

		#endregion

		#region PopulateHAWBConsignor

		void PopulateConsignor(CusHAWB cusHAWB, Xsd.Organisation consignor, IValueObjectImportContext context)
		{
			//if you want to do organisational matching here, then make a method that you override in another class that you can 
			//use to implement the Org matching
			//adding organisational matching is expensive and will affect data import performance			
			if (consignor.OrganisationDetails.IsSpecified)
			{
				context.SetPropertyInfoValue(cusHAWB.CS_ConsignorNameInfo, consignor.OrganisationDetails.Name, consignor.OrganisationDetails.NameSpecified);
				if (!consignor.OrganisationDetails.Location.Country.IsEmpty)
				{
					RefCountry country = RefCountry.LoadFromCountryName(cusHAWB.Factory, consignor.OrganisationDetails.Location.Country);
					if (country != null)
					{
						cusHAWB.CS_RN_NKConsignorCountry = country.RN_Code;
					}
				}

				Xsd.OrgAddress mainAddress = consignor.OrganisationDetails.Addresses.GetMainAddress();
				if (mainAddress != null)
				{
					context.SetPropertyInfoValue(cusHAWB.CS_ConsignorStreetInfo, mainAddress.AddressLine1, mainAddress.AddressLine1Specified);
					PopulateAddress2SuburbDetails(cusHAWB.CS_ConsignorStreet2Info, cusHAWB.CS_ConsignorCityInfo, mainAddress.AddressLine2, mainAddress.CityOrSuburb, context);
					context.SetPropertyInfoValue(cusHAWB.CS_ConsignorStateInfo, mainAddress.StateOrProvince, mainAddress.StateOrProvinceSpecified);
					context.SetPropertyInfoValue(cusHAWB.CS_ConsignorPostcodeInfo, mainAddress.PostCode, mainAddress.PostCodeSpecified);
					context.SetPropertyInfoValueIfValueNotEmpty(cusHAWB.CS_ConsignorPhoneInfo, mainAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
				}

				if (consignor.OrganisationDetails.Contacts.IsSpecified && consignor.OrganisationDetails.Contacts.Count > 0)
				{
					context.SetPropertyInfoValue(cusHAWB.CS_ConsignorContactNameInfo, consignor.OrganisationDetails.Contacts[0].Name, consignor.OrganisationDetails.Contacts[0].NameSpecified);
				}
			}
		}
		#endregion

		#region PopulateHAWBConsignee

		void PopulateConsignee(CusHAWB cusHAWB, Xsd.Organisation consignee, IValueObjectImportContext context)
		{
			//if you want to do organisational matching here, then make a method that you override in another class that you can 
			//use to implement the Org matching
			//adding organisational matching is expensive and will affect data import performance
			if (consignee.OrganisationDetails.IsSpecified)
			{
				context.SetPropertyInfoValue(cusHAWB.CS_ConsigneeNameInfo, consignee.OrganisationDetails.Name, consignee.OrganisationDetails.NameSpecified);
				if (!consignee.OrganisationDetails.Location.Country.IsEmpty)
				{
					RefCountry country = RefCountry.LoadFromCountryName(cusHAWB.Factory, consignee.OrganisationDetails.Location.Country);
					if (country != null)
					{
						cusHAWB.CS_RN_NKConsigneeCountry = country.RN_Code;
					}
				}

				Xsd.OrgAddress mainAddress = consignee.OrganisationDetails.Addresses.GetMainAddress();
				if (mainAddress != null)
				{
					context.SetPropertyInfoValue(cusHAWB.CS_ConsigneeStreetInfo, mainAddress.AddressLine1, mainAddress.AddressLine1Specified);
					PopulateAddress2SuburbDetails(cusHAWB.CS_ConsigneeStreet2Info, cusHAWB.CS_ConsigneeCityInfo, mainAddress.AddressLine2, mainAddress.CityOrSuburb, context);
					context.SetPropertyInfoValue(cusHAWB.CS_ConsigneeStateInfo, mainAddress.StateOrProvince, mainAddress.StateOrProvinceSpecified);
					context.SetPropertyInfoValue(cusHAWB.CS_ConsigneePostcodeInfo, mainAddress.PostCode, mainAddress.PostCodeSpecified);
					context.SetPropertyInfoValueIfValueNotEmpty(cusHAWB.CS_ConsigneePhoneInfo, mainAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
				}

				if (consignee.OrganisationDetails.Contacts.IsSpecified && consignee.OrganisationDetails.Contacts.Count > 0)
				{
					context.SetPropertyInfoValue(cusHAWB.CS_ConsigneeContactNameInfo, consignee.OrganisationDetails.Contacts[0].Name, consignee.OrganisationDetails.Contacts[0].NameSpecified);
				}
			}
		}
		#endregion

		#region PopulateFromDeclaration

		void PopulateFromDeclaration(CusHAWB cusHAWB, Xsd.Declaration declaration, IValueObjectImportContext context)
		{
			cusHAWB.CS_IsSurplus = declaration.IsSurplus == Xsd.TrueFalse.@true;
			cusHAWB.CS_ShipmentType = (declaration.IsDocuments == Xsd.TrueFalse.@true) ? "DOC" : "STD";

			string paymentTerms = declaration.PaymentTerms;
			if (paymentTerms == "PPD")
			{
				paymentTerms = CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
			else if (paymentTerms == "COL")
			{
				paymentTerms = CMRMethodsOfPayment.Codes.Collect;
			}

			context.SetPropertyInfoValueIfValueNotEmpty(cusHAWB.CS_FreightPrepaidCollectInfo, paymentTerms);
			cusHAWB.CS_IsSelfAssessedClearance = declaration.IsSAC;
			cusHAWB.CS_IsPersonalEffects = declaration.IsPersonalEffects;
			CreateImporter(cusHAWB, declaration.Importer, context);
		}
		#endregion

		#region CreateImporter

		void CreateImporter(CusHAWB cusHAWB, Xsd.Organisation importer, IValueObjectImportContext context)
		{
			if (importer.IsSpecified)
			{
				OrgPatternMatchAddressDataAdapter orgPatternMatchAddressDataAdapter = new OrgPatternMatchAddressDataAdapter(cusHAWB.PK, typeof(CusHAWB), OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter);
				OrgPatternMatchAddress newImporterAddress = orgPatternMatchAddressDataAdapter.CreateOrUpdateFromValueObject(importer, context);

				// CV - 1/6/06 to troubleshoot a problem at UPS, can remove this later
				if (newImporterAddress == null)
				{
					ErrorReporter.ReportOnce("AirCargoValueObjectDataAdapter.ImporterOrgPatternMatchAddressNotCreated", "Importer OrgPatternMatchAddress not created!");
				}
				else if (newImporterAddress.P3_ParentID != cusHAWB.PK || newImporterAddress.P3_AddressType != OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter)
				{
					ErrorReporter.ReportOnce("AirCargoValueObjectDataAdapter.ImporterOrgPatternMatchAddressHasWrongForeignKeyOrType", "Importer OrgPatternMatchAddress has the wrong FK or the wrong AddressType (P3_ParentID=" + newImporterAddress.P3_ParentID + ", P3_AddressType=" + newImporterAddress.P3_AddressType + ", CusHAWB.PK=" + cusHAWB.PK + ")"); // Column name used in error message, not key
				}
			}
		}
		#endregion
	}
}
