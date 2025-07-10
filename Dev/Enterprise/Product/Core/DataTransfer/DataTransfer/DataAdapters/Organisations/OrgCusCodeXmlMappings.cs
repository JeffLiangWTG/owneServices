using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class OrgCusCodeXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		OrgCusCodeXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrgCusCode.CodeTypes.RegulatedAgentID, nameof(Xsd.RegistrationNumberTypes.RAI));
			yield return new Mapping(OrgCusCode.CodeTypes.GSTCode, nameof(Xsd.RegistrationNumberTypes.GST));
			yield return new Mapping(OrgCusCode.CodeTypes.GS1, nameof(Xsd.RegistrationNumberTypes.GS1));
			yield return new Mapping(OrgCusCode.CodeTypes.GovBusinessCode, nameof(Xsd.RegistrationNumberTypes.GBR));
			yield return new Mapping(OrgCusCode.CodeTypes.CorporationCode, nameof(Xsd.RegistrationNumberTypes.GCR));
			yield return new Mapping(OrgCusCode.CodeTypes.TaxFileCode, nameof(Xsd.RegistrationNumberTypes.GTX));
			yield return new Mapping(OrgCusCode.CodeTypes.CustomsClientCode, nameof(Xsd.RegistrationNumberTypes.CCD));
			yield return new Mapping(OrgCusCode.CodeTypes.SupplierCode, nameof(Xsd.RegistrationNumberTypes.CSC));
			yield return new Mapping(OrgCusCode.CodeTypes.ControlledPremisesID, nameof(Xsd.RegistrationNumberTypes.CCP));
			yield return new Mapping(OrgCusCode.CodeTypes.BrokerageRegistration, nameof(Xsd.RegistrationNumberTypes.CBR));
			yield return new Mapping(OrgCusCode.CodeTypes.BrokerageSiteID, nameof(Xsd.RegistrationNumberTypes.CBS));
			yield return new Mapping(OrgCusCode.CodeTypes.BrokeragePrinter, nameof(Xsd.RegistrationNumberTypes.CBP));
			yield return new Mapping(OrgCusCode.CodeTypes.CarrierCode, nameof(Xsd.RegistrationNumberTypes.CCC));
			yield return new Mapping(OrgCusCode.CodeTypes.TruckCarrierCode, nameof(Xsd.RegistrationNumberTypes.CCT));
			yield return new Mapping(OrgCusCode.CodeTypes.ManifestProviderID, nameof(Xsd.RegistrationNumberTypes.CMP));
			yield return new Mapping(OrgCusCode.CodeTypes.LegacySystemCode, nameof(Xsd.RegistrationNumberTypes.LSC));
			yield return new Mapping(OrgCusCode.CodeTypes.OneStopCode, nameof(Xsd.RegistrationNumberTypes.Item1ST));
			yield return new Mapping(OrgCusCode.CodeTypes.RebateUserCode, nameof(Xsd.RegistrationNumberTypes.REB));
			yield return new Mapping(OrgCusCode.CodeTypes.VATCode, nameof(Xsd.RegistrationNumberTypes.VAT));
			yield return new Mapping(OrgCusCode.CodeTypes.BuyerCode, nameof(Xsd.RegistrationNumberTypes.BYR));
			yield return new Mapping(OrgCusCode.CodeTypes.DeliveranceCode, nameof(Xsd.RegistrationNumberTypes.DLV));
			yield return new Mapping(OrgCusCode.CodeTypes.UniversalNettingCode, nameof(Xsd.RegistrationNumberTypes.UNC));
			yield return new Mapping(OrgCusCode.CodeTypes.UniversalOfficeCode, nameof(Xsd.RegistrationNumberTypes.UOC));
			yield return new Mapping(OrgCusCode.CodeTypes.ReleaseAgentCode, nameof(Xsd.RegistrationNumberTypes.RAC));
			yield return new Mapping(OrgCusCode.CodeTypes.EDISiteID, nameof(Xsd.RegistrationNumberTypes.EID));
			yield return new Mapping(OrgCusCode.CodeTypes.EUTracesID, nameof(Xsd.RegistrationNumberTypes.ETI));
			yield return new Mapping(OrgCusCode.CodeTypes.GlobalTrackingName, nameof(Xsd.RegistrationNumberTypes.GTN));
			yield return new Mapping(OrgCusCode.CodeTypes.PassportID, nameof(Xsd.RegistrationNumberTypes.PAS));
			yield return new Mapping(OrgCusCode.CodeTypes.DriverLicenceID, nameof(Xsd.RegistrationNumberTypes.DRV));
			yield return new Mapping(OrgCusCode.CodeTypes.OrganizationNumber, nameof(Xsd.RegistrationNumberTypes.ORG));
			yield return new Mapping(OrgCusCode.CodeTypes.BusinessRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.BRN));
			yield return new Mapping(OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, nameof(Xsd.RegistrationNumberTypes.APC));
			yield return new Mapping(OrgCusCode.CodeTypes.eNettRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.ENE));
			yield return new Mapping(OrgCusCode.CodeTypes.CarrierPrincipalCode, nameof(Xsd.RegistrationNumberTypes.CAR));
			yield return new Mapping(OrgCusCode.CodeTypes.InntraCode, nameof(Xsd.RegistrationNumberTypes.INT));
			yield return new Mapping(OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode, nameof(Xsd.RegistrationNumberTypes.CMM));
			yield return new Mapping(OrgCusCode.CodeTypes.PIMAAddress, nameof(Xsd.RegistrationNumberTypes.PIM));
			yield return new Mapping(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, nameof(Xsd.RegistrationNumberTypes.EDR));
			yield return new Mapping(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, nameof(Xsd.RegistrationNumberTypes.ECR));
			yield return new Mapping(OrgCusCode.CodeTypes.CreditAgencyCode, nameof(Xsd.RegistrationNumberTypes.CAC));
			yield return new Mapping(OrgCusCode.CodeTypes.IVA, nameof(Xsd.RegistrationNumberTypes.IVA));
			yield return new Mapping(OrgCusCode.CodeTypes.EHubOrganisationID, nameof(Xsd.RegistrationNumberTypes.HID));
			yield return new Mapping(OrgCusCode.CodeTypes.RoadCarrierRegistration, nameof(Xsd.RegistrationNumberTypes.RCR));
			yield return new Mapping(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, nameof(Xsd.RegistrationNumberTypes.TIR));
			yield return new Mapping(OrgCusCode.CodeTypes.PalletTradingAccountChep, nameof(Xsd.RegistrationNumberTypes.CHP));
			yield return new Mapping(OrgCusCode.CodeTypes.PalletTradingAccountLoscam, nameof(Xsd.RegistrationNumberTypes.LOS));
			yield return new Mapping(OrgCusCode.CodeTypes.SEPACreditorIdentifier, nameof(Xsd.RegistrationNumberTypes.SID));
			yield return new Mapping(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, nameof(Xsd.RegistrationNumberTypes.C1C));
			yield return new Mapping(OrgCusCode.CodeTypes.DomesticCarrierCode, nameof(Xsd.RegistrationNumberTypes.DCC));
			yield return new Mapping(OrgCusCode.CodeTypes.VGMRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.VGM));
			yield return new Mapping(OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem, nameof(Xsd.RegistrationNumberTypes.NAI));
			yield return new Mapping(OrgCusCode.CodeTypes.StandardIndustrialClassification, nameof(Xsd.RegistrationNumberTypes.SIC));
			yield return new Mapping(OrgCusCode.CodeTypes.JNP, nameof(Xsd.RegistrationNumberTypes.JNP));
			yield return new Mapping(OrgCusCode.CodeTypes.WorldCargoAssociationNumber, nameof(Xsd.RegistrationNumberTypes.WCA));
			yield return new Mapping(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity, nameof(Xsd.RegistrationNumberTypes.CAG));
			yield return new Mapping(OrgCusCode.CodeTypes.DEA, nameof(Xsd.RegistrationNumberTypes.DEA));
			yield return new Mapping(OrgCusCode.CodeTypes.PortSystemNumber, nameof(Xsd.RegistrationNumberTypes.PSN));
			yield return new Mapping(OrgCusCode.CodeTypes.PortServiceReference, nameof(Xsd.RegistrationNumberTypes.PSR));
			yield return new Mapping(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode, nameof(Xsd.RegistrationNumberTypes.C1R));
			yield return new Mapping(OrgCusCode.CodeTypes.NVOCCReference, nameof(Xsd.RegistrationNumberTypes.NVO));
			yield return new Mapping(OrgCusCode.CodeTypes.BoleroTitleRegisterID, nameof(Xsd.RegistrationNumberTypes.TRI));
			yield return new Mapping(OrgCusCode.CodeTypes.ContainerChainCommunityCode, nameof(Xsd.RegistrationNumberTypes.CC1));
			yield return new Mapping(OrgCusCode.CodeTypes.SpecialEconomicZone, nameof(Xsd.RegistrationNumberTypes.SEZ));

			// AU Specific
			yield return new Mapping(OrgCusCode.CodeTypes.CustomsClientID, nameof(Xsd.RegistrationNumberTypes.CID));
			yield return new Mapping(OrgCusCode.CodeTypes.CustomsCPPermitCode, nameof(Xsd.RegistrationNumberTypes.CPC));
			yield return new Mapping(OrgCusCode.CodeTypes.MedicareID, nameof(Xsd.RegistrationNumberTypes.MED));
			yield return new Mapping(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, nameof(Xsd.RegistrationNumberTypes.ABN));
			yield return new Mapping(OrgCusCode.AustraliaCodeTypes.eParcelMerchantLocationID, nameof(Xsd.RegistrationNumberTypes.EPL));
			yield return new Mapping(OrgCusCode.AustraliaCodeTypes.Diplomat, nameof(Xsd.RegistrationNumberTypes.DIP));
			yield return new Mapping(OrgCusCode.AustraliaCodeTypes.ARN, nameof(Xsd.RegistrationNumberTypes.ARN));
			yield return new Mapping(OrgCusCode.AustraliaCodeTypes.QuotaExporterNumber, nameof(Xsd.RegistrationNumberTypes.QEN));
			yield return new Mapping(OrgCusCode.AustraliaCodeTypes.ApprovedArrangementNumber, nameof(Xsd.RegistrationNumberTypes.AAN));

			// Canada
			yield return new Mapping(OrgCusCode.CACodeTypes.AuthorizationID, nameof(Xsd.RegistrationNumberTypes.AID));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax, nameof(Xsd.RegistrationNumberTypes.BRC));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, nameof(Xsd.RegistrationNumberTypes.BRT));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, nameof(Xsd.RegistrationNumberTypes.BRM));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberForExport, nameof(Xsd.RegistrationNumberTypes.BRE));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, nameof(Xsd.RegistrationNumberTypes.BRL));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions, nameof(Xsd.RegistrationNumberTypes.BRP));
			yield return new Mapping(OrgCusCode.CACodeTypes.QuebecSalesTaxID, nameof(Xsd.RegistrationNumberTypes.QST));
			yield return new Mapping(OrgCusCode.CACodeTypes.SocialInsuranceNumber, nameof(Xsd.RegistrationNumberTypes.SIN));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, nameof(Xsd.RegistrationNumberTypes.CAI));
			yield return new Mapping(OrgCusCode.CACodeTypes.CSAReferenceID, nameof(Xsd.RegistrationNumberTypes.CSA));
			yield return new Mapping(OrgCusCode.CACodeTypes.ExportLicenceNumber, nameof(Xsd.RegistrationNumberTypes.CAX));
			yield return new Mapping(OrgCusCode.CACodeTypes.AccountSecurityCode, nameof(Xsd.RegistrationNumberTypes.ASC));
			yield return new Mapping(OrgCusCode.CACodeTypes.CFIAAccountNumber, nameof(Xsd.RegistrationNumberTypes.CFI));
			yield return new Mapping(OrgCusCode.CACodeTypes.WorldManufacturerIdentifier, nameof(Xsd.RegistrationNumberTypes.WMI));
			yield return new Mapping(OrgCusCode.CACodeTypes.NuclearSafetyCommissionLicenseNumber, nameof(Xsd.RegistrationNumberTypes.NSL));
			yield return new Mapping(OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense, nameof(Xsd.RegistrationNumberTypes.SFC));
			yield return new Mapping(OrgCusCode.CACodeTypes.ECCCAuthorizationNumber, nameof(Xsd.RegistrationNumberTypes.ECC));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, nameof(Xsd.RegistrationNumberTypes.BRB));
			yield return new Mapping(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, nameof(Xsd.RegistrationNumberTypes.BNC));

			// EXDOC Specific (AQIS)
			yield return new Mapping(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, nameof(Xsd.RegistrationNumberTypes.EEN));
			yield return new Mapping(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, nameof(Xsd.RegistrationNumberTypes.NEN));
			yield return new Mapping(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, nameof(Xsd.RegistrationNumberTypes.NEI));
			yield return new Mapping(OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber, nameof(Xsd.RegistrationNumberTypes.EAP));
			yield return new Mapping(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, nameof(Xsd.RegistrationNumberTypes.ESN));
			yield return new Mapping(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, nameof(Xsd.RegistrationNumberTypes.EEU));

			// MY Specific
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfBusiness, nameof(Xsd.RegistrationNumberTypes.ROB));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, nameof(Xsd.RegistrationNumberTypes.ROC));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.ImportCustomsAgentBusinessCode, nameof(Xsd.RegistrationNumberTypes.IAG));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.ExportCustomsAgentBusinessCode, nameof(Xsd.RegistrationNumberTypes.EAG));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.OtherBusinessCode, nameof(Xsd.RegistrationNumberTypes.OTH));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.CFSBondedPackUnpack, nameof(Xsd.RegistrationNumberTypes.CFS));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.CustomsStation, nameof(Xsd.RegistrationNumberTypes.CST));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.SAL, nameof(Xsd.RegistrationNumberTypes.SAL));
			yield return new Mapping(MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, nameof(Xsd.RegistrationNumberTypes.PIC));

			// ZA Specific
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.BillIssuer, nameof(Xsd.RegistrationNumberTypes.BIL));
			yield return new Mapping(OrgCusCode.CodeTypes.BondHolderCode, nameof(Xsd.RegistrationNumberTypes.BHR));
			yield return new Mapping(OrgCusCode.CodeTypes.AgentCode, nameof(Xsd.RegistrationNumberTypes.AGT));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, nameof(Xsd.RegistrationNumberTypes.CDP));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, nameof(Xsd.RegistrationNumberTypes.REM));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter, nameof(Xsd.RegistrationNumberTypes.APE));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.IDNumber, nameof(Xsd.RegistrationNumberTypes.IDO));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.TPT, nameof(Xsd.RegistrationNumberTypes.TPT));
			yield return new Mapping(OrgCusCode.CodeTypes.TerminalControlledPremisesID, nameof(Xsd.RegistrationNumberTypes.CPT));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.TNP, nameof(Xsd.RegistrationNumberTypes.TNP));
			yield return new Mapping(OrgCusCode.SouthAfricaCodeTypes.BGV, nameof(Xsd.RegistrationNumberTypes.BGV));

			//UZ
			yield return new Mapping(UzbekistanOrgCusCodeInfo.OrgCusCodes.QQS, nameof(Xsd.RegistrationNumberTypes.QQS));
			yield return new Mapping(UzbekistanOrgCusCodeInfo.OrgCusCodes.STR, nameof(Xsd.RegistrationNumberTypes.STR));

			// NZ
			yield return new Mapping(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, nameof(Xsd.RegistrationNumberTypes.ATF));
			yield return new Mapping(OrgCusCode.NZCodeTypes.SecureExportPartner, nameof(Xsd.RegistrationNumberTypes.SEP));
			yield return new Mapping(OrgCusCode.NZCodeTypes.RegistrationNumber, nameof(Xsd.RegistrationNumberTypes.RGN));
			yield return new Mapping(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, nameof(Xsd.RegistrationNumberTypes.MQE));

			// SG
			yield return new Mapping(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, nameof(Xsd.RegistrationNumberTypes.CPF));
			yield return new Mapping(OrgCusCode.CodeTypes.IdentityCardNumber, nameof(Xsd.RegistrationNumberTypes.IDN));
			yield return new Mapping(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, nameof(Xsd.RegistrationNumberTypes.UEN));
			yield return new Mapping(OrgCusCode.SingaporeCodeTypes.QualifiedCompanyIdentificationCode, nameof(Xsd.RegistrationNumberTypes.QCI));
			yield return new Mapping(OrgCusCode.SingaporeCodeTypes.PartyStatusType, nameof(Xsd.RegistrationNumberTypes.PST));
			yield return new Mapping(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, nameof(Xsd.RegistrationNumberTypes.IBG));
			yield return new Mapping(OrgCusCode.SingaporeCodeTypes.DirectDelivery, nameof(Xsd.RegistrationNumberTypes.DIR));

			// USA
			yield return new Mapping(OrgCusCode.USACodeTypes.ABIRoutingCode, nameof(Xsd.RegistrationNumberTypes.ABR));
			yield return new Mapping(OrgCusCode.USACodeTypes.ACEAssignedNumber, nameof(Xsd.RegistrationNumberTypes.ACE));
			yield return new Mapping(OrgCusCode.USACodeTypes.FreeAndSecureTradeCode, nameof(Xsd.RegistrationNumberTypes.FST));
			yield return new Mapping(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, nameof(Xsd.RegistrationNumberTypes.EIN));
			yield return new Mapping(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, nameof(Xsd.RegistrationNumberTypes.FEI));
			yield return new Mapping(OrgCusCode.USACodeTypes.SocialSecurityNumber, nameof(Xsd.RegistrationNumberTypes.SSN));
			yield return new Mapping(OrgCusCode.USACodeTypes.CBPAssignedNumber, nameof(Xsd.RegistrationNumberTypes.CBN));
			yield return new Mapping(OrgCusCode.USACodeTypes.ManufacturerID, nameof(Xsd.RegistrationNumberTypes.MID));
			yield return new Mapping(OrgCusCode.USACodeTypes.DeprecatedSpecialAddressNotification, nameof(Xsd.RegistrationNumberTypes.SAN));
			yield return new Mapping(OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber, nameof(Xsd.RegistrationNumberTypes.FAA));
			yield return new Mapping(OrgCusCode.USACodeTypes.CTPAT, nameof(Xsd.RegistrationNumberTypes.CTP));
			yield return new Mapping(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, nameof(Xsd.RegistrationNumberTypes.DUN));
			yield return new Mapping(OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4, nameof(Xsd.RegistrationNumberTypes.DN4));
			yield return new Mapping(OrgCusCode.USACodeTypes.DOTDepartmentOfTransportation, nameof(Xsd.RegistrationNumberTypes.DOT));
			yield return new Mapping(OrgCusCode.USACodeTypes.NMFCParticipant, nameof(Xsd.RegistrationNumberTypes.NMF));
			yield return new Mapping(OrgCusCode.USACodeTypes.AlcoholImportLicence, nameof(Xsd.RegistrationNumberTypes.ALC));
			yield return new Mapping(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, nameof(Xsd.RegistrationNumberTypes.ECN));
			yield return new Mapping(OrgCusCode.USACodeTypes.ForeignRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.FRN));
			yield return new Mapping(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.PFR));
			yield return new Mapping(OrgCusCode.USACodeTypes.FIRMSCode, nameof(Xsd.RegistrationNumberTypes.FRM));
			yield return new Mapping(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, nameof(Xsd.RegistrationNumberTypes.FMC));
			yield return new Mapping(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, nameof(Xsd.RegistrationNumberTypes.CHB));
			yield return new Mapping(OrgCusCode.USACodeTypes.ShipperRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.SFR));
			yield return new Mapping(OrgCusCode.USACodeTypes.EntryFilerCode, nameof(Xsd.RegistrationNumberTypes.ENF));
			yield return new Mapping(OrgCusCode.USACodeTypes.CertifiedCargoScreening, nameof(Xsd.RegistrationNumberTypes.CCS));
			yield return new Mapping(OrgCusCode.USACodeTypes.TireManufacturerCode, nameof(Xsd.RegistrationNumberTypes.TMC));
			yield return new Mapping(OrgCusCode.USACodeTypes.GlazingManufacturerCode, nameof(Xsd.RegistrationNumberTypes.GMC));
			yield return new Mapping(OrgCusCode.USACodeTypes.APHISAssignedNumber, nameof(Xsd.RegistrationNumberTypes.APH));
			yield return new Mapping(OrgCusCode.USACodeTypes.DDTCRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.DDT));
			yield return new Mapping(OrgCusCode.USACodeTypes.TTBPermitNumber, nameof(Xsd.RegistrationNumberTypes.TTB));
			yield return new Mapping(OrgCusCode.USACodeTypes.TTIRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.TTI));
			yield return new Mapping(OrgCusCode.USACodeTypes.TTEPermitNumber, nameof(Xsd.RegistrationNumberTypes.TTE));
			yield return new Mapping(OrgCusCode.USACodeTypes.CPSCAccreditedLabId, nameof(Xsd.RegistrationNumberTypes.LAB));
			yield return new Mapping(OrgCusCode.USACodeTypes.AMSRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.AMS));
			yield return new Mapping(OrgCusCode.USACodeTypes.IFTPPermitNumber, nameof(Xsd.RegistrationNumberTypes.IFT));
			yield return new Mapping(OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode, nameof(Xsd.RegistrationNumberTypes.DOD));
			yield return new Mapping(OrgCusCode.USACodeTypes.ACASOriginatorCode, nameof(Xsd.RegistrationNumberTypes.ACA));
			yield return new Mapping(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, nameof(Xsd.RegistrationNumberTypes.AMO));
			yield return new Mapping(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, nameof(Xsd.RegistrationNumberTypes.FWE));
			yield return new Mapping(OrgCusCode.USACodeTypes.FDAForeignSellerRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.FSR));
			yield return new Mapping(OrgCusCode.USACodeTypes.ForeignProducerIdentifier, nameof(Xsd.RegistrationNumberTypes.FPI));
			yield return new Mapping(OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer, nameof(Xsd.RegistrationNumberTypes.FPB));
			yield return new Mapping(OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits, nameof(Xsd.RegistrationNumberTypes.FPS));
			yield return new Mapping(OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine, nameof(Xsd.RegistrationNumberTypes.FPW));
			yield return new Mapping(OrgCusCode.USACodeTypes.LegalEntityIdentifier, nameof(Xsd.RegistrationNumberTypes.LEI));
			yield return new Mapping(OrgCusCode.USACodeTypes.GlobalLocationNumber, nameof(Xsd.RegistrationNumberTypes.GLN));
			yield return new Mapping(OrgCusCode.USACodeTypes.StandardCarrierAlphaCodeAir, nameof(Xsd.RegistrationNumberTypes.CCA));

			// Brasil
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, nameof(Xsd.RegistrationNumberTypes.CJN));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.OccupationCodesOfIndividuals, nameof(Xsd.RegistrationNumberTypes.CBO));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration, nameof(Xsd.RegistrationNumberTypes.IMF));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.StateTaxPayerRegistration, nameof(Xsd.RegistrationNumberTypes.IEF));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.RSN, nameof(Xsd.RegistrationNumberTypes.RSN));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.RLR, nameof(Xsd.RegistrationNumberTypes.RLR));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.RLP, nameof(Xsd.RegistrationNumberTypes.RLP));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, nameof(Xsd.RegistrationNumberTypes.IMM));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, nameof(Xsd.RegistrationNumberTypes.RTC));
			yield return new Mapping(BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode, nameof(Xsd.RegistrationNumberTypes.FOI));

			//Cyprus
			yield return new Mapping(CyprusOrgCusCodeInfo.OrgCusCodes.TIC, nameof(Xsd.RegistrationNumberTypes.TIC));

			//Moldova
			yield return new Mapping(MoldovaOrgCusCodeInfo.OrgCusCodes.NCF, nameof(Xsd.RegistrationNumberTypes.NCF));

			// HK
			yield return new Mapping(OrgCusCode.HKCodeTypes.KnownConsignorNumber, nameof(Xsd.RegistrationNumberTypes.KCN));

			// RU
			yield return new Mapping(OrgCusCode.RussiaCodeTypes.OGRN, nameof(Xsd.RegistrationNumberTypes.OGR));
			yield return new Mapping(OrgCusCode.RussiaCodeTypes.KPP, nameof(Xsd.RegistrationNumberTypes.KPP));

			// DE
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.CWC, nameof(Xsd.RegistrationNumberTypes.CWC));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.BHT, nameof(Xsd.RegistrationNumberTypes.BHT));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, nameof(Xsd.RegistrationNumberTypes.ZAP));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.UST, nameof(Xsd.RegistrationNumberTypes.UST));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, nameof(Xsd.RegistrationNumberTypes.TAO));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.GermanCivilAviationAuthority, nameof(Xsd.RegistrationNumberTypes.GCA));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister, nameof(Xsd.RegistrationNumberTypes.HRB));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, nameof(Xsd.RegistrationNumberTypes.DPC));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, nameof(Xsd.RegistrationNumberTypes.DBI));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.AuthorizationTemporaryStorage, nameof(Xsd.RegistrationNumberTypes.ATS));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, nameof(Xsd.RegistrationNumberTypes.API));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber, nameof(Xsd.RegistrationNumberTypes.EPI));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, nameof(Xsd.RegistrationNumberTypes.WPI));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.LocalClearanceOutwardProcessing, nameof(Xsd.RegistrationNumberTypes.LCO));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.AccreditedExporter, nameof(Xsd.RegistrationNumberTypes.AEX));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.LID, nameof(Xsd.RegistrationNumberTypes.LID));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.MST, nameof(Xsd.RegistrationNumberTypes.MST));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.ZMI, nameof(Xsd.RegistrationNumberTypes.ZMI));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.SteuerlicheIdentifikationsnummer, nameof(Xsd.RegistrationNumberTypes.STE));
			yield return new Mapping(GermanyOrgCusCodeInfo.OrgCusCodes.IMA, nameof(Xsd.RegistrationNumberTypes.IMA));

			// IE
			yield return new Mapping(OrgCusCode.IrelandCodeTypes.PYE, nameof(Xsd.RegistrationNumberTypes.PYE));
			yield return new Mapping(OrgCusCode.IrelandCodeTypes.ITX, nameof(Xsd.RegistrationNumberTypes.ITX));
			yield return new Mapping(OrgCusCode.IrelandCodeTypes.CGT, nameof(Xsd.RegistrationNumberTypes.CGT));
			yield return new Mapping(OrgCusCode.IrelandCodeTypes.VatFreeAuthorisation, nameof(Xsd.RegistrationNumberTypes.VFA));
			yield return new Mapping(OrgCusCode.IrelandCodeTypes.TraderAccountNumber, nameof(Xsd.RegistrationNumberTypes.TRA));
			yield return new Mapping(OrgCusCode.IrelandCodeTypes.VatZeroRatedAct2010, nameof(Xsd.RegistrationNumberTypes.VZR));

			// AT
			yield return new Mapping(OrgCusCode.AustriaCodeTypes.UID, nameof(Xsd.RegistrationNumberTypes.UID));

			// PL
			yield return new Mapping(OrgCusCode.PolandCodeTypes.NIP, nameof(Xsd.RegistrationNumberTypes.NIP));
			yield return new Mapping(OrgCusCode.PolandCodeTypes.PTU, nameof(Xsd.RegistrationNumberTypes.PTU));
			yield return new Mapping(OrgCusCode.PolandCodeTypes.PES, nameof(Xsd.RegistrationNumberTypes.PES));

			// IS
			yield return new Mapping(OrgCusCode.IcelandCodeTypes.VSK, nameof(Xsd.RegistrationNumberTypes.VSK));
			yield return new Mapping(OrgCusCode.IcelandCodeTypes.Kennitala, nameof(Xsd.RegistrationNumberTypes.KEN));
			yield return new Mapping(OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, nameof(Xsd.RegistrationNumberTypes.COC));

			// MM
			yield return new Mapping(OrgCusCode.CodeTypes.CompanyRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.CRN));

			// AR
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, nameof(Xsd.RegistrationNumberTypes.CUI));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, nameof(Xsd.RegistrationNumberTypes.CUL));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF, nameof(Xsd.RegistrationNumberTypes.CUF));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE, nameof(Xsd.RegistrationNumberTypes.IVE));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVF, nameof(Xsd.RegistrationNumberTypes.IVF));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI, nameof(Xsd.RegistrationNumberTypes.IVI));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVM, nameof(Xsd.RegistrationNumberTypes.IVM));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVN, nameof(Xsd.RegistrationNumberTypes.IVN));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVP, nameof(Xsd.RegistrationNumberTypes.IVP));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVR, nameof(Xsd.RegistrationNumberTypes.IVR));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVS, nameof(Xsd.RegistrationNumberTypes.IVS));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IVX, nameof(Xsd.RegistrationNumberTypes.IVX));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IBL, nameof(Xsd.RegistrationNumberTypes.IBL));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IBM, nameof(Xsd.RegistrationNumberTypes.IBM));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IBS, nameof(Xsd.RegistrationNumberTypes.IBS));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.IBN, nameof(Xsd.RegistrationNumberTypes.IBN));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.MIP, nameof(Xsd.RegistrationNumberTypes.MIP));
			yield return new Mapping(ArgentinaOrgCusCodeInfo.OrgCusCodes.OLS, nameof(Xsd.RegistrationNumberTypes.OLS));

			// CL
			yield return new Mapping(ChileOrgCusCodeInfo.OrgCusCodes.RUT, nameof(Xsd.RegistrationNumberTypes.RUT));
			yield return new Mapping(ChileOrgCusCodeInfo.OrgCusCodes.GEM, nameof(Xsd.RegistrationNumberTypes.GEM));
			yield return new Mapping(ChileOrgCusCodeInfo.OrgCusCodes.SOL, nameof(Xsd.RegistrationNumberTypes.SOL));
			yield return new Mapping(ChileOrgCusCodeInfo.OrgCusCodes.ACT, nameof(Xsd.RegistrationNumberTypes.ACT));

			// CO
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.AEC, nameof(Xsd.RegistrationNumberTypes.AEC));
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, nameof(Xsd.RegistrationNumberTypes.NIT));
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.NRS, nameof(Xsd.RegistrationNumberTypes.NRS));
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.NGC, nameof(Xsd.RegistrationNumberTypes.NGC));
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.NGA, nameof(Xsd.RegistrationNumberTypes.NGA));
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.NAR, nameof(Xsd.RegistrationNumberTypes.NAR));
			yield return new Mapping(ColombiaOrgCusCodeInfo.OrgCusCodes.FID, nameof(Xsd.RegistrationNumberTypes.FID));

			// EG
			yield return new Mapping(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.COM));

			// TH
			yield return new Mapping(OrgCusCode.CodeTypes.TaxIDNumber, nameof(Xsd.RegistrationNumberTypes.TAX));

			// DK
			yield return new Mapping(OrgCusCode.DenmarkCodeTypes.CentralBusinessRegister, nameof(Xsd.RegistrationNumberTypes.CVR));
			yield return new Mapping(OrgCusCode.DenmarkCodeTypes.ProductionNumber, nameof(Xsd.RegistrationNumberTypes.PNR));
			yield return new Mapping(OrgCusCode.DenmarkCodeTypes.EANLocationNumber, nameof(Xsd.RegistrationNumberTypes.EAN));

			// ID
			yield return new Mapping(OrgCusCode.IndonesiaCodeTypes.PPN, nameof(Xsd.RegistrationNumberTypes.PPN));
			yield return new Mapping(OrgCusCode.IndonesiaCodeTypes.PP2, nameof(Xsd.RegistrationNumberTypes.PP2));
			yield return new Mapping(OrgCusCode.IndonesiaCodeTypes.PP3, nameof(Xsd.RegistrationNumberTypes.PP3));

			// MX
			yield return new Mapping(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, nameof(Xsd.RegistrationNumberTypes.RFC));
			yield return new Mapping(MexicoOrgCusCodeInfo.OrgCusCodes.CUR, nameof(Xsd.RegistrationNumberTypes.CUR));
			yield return new Mapping(MexicoOrgCusCodeInfo.OrgCusCodes.RFG, nameof(Xsd.RegistrationNumberTypes.RFG));
			yield return new Mapping(MexicoOrgCusCodeInfo.OrgCusCodes.CFD, nameof(Xsd.RegistrationNumberTypes.CFD));
			yield return new Mapping(MexicoOrgCusCodeInfo.OrgCusCodes.ELN, nameof(Xsd.RegistrationNumberTypes.ELN));

			// UY
			yield return new Mapping(UruguayOrgCusCodeInfo.OrgCusCodes.FZU, nameof(Xsd.RegistrationNumberTypes.FZU));

			// CZ
			yield return new Mapping(CzechRepublicOrgCusCodeInfo.OrgCusCodes.DPH, nameof(Xsd.RegistrationNumberTypes.DPH));

			// GB (UK)
			yield return new Mapping(OrgCusCode.CodeTypes.CompanyNumber, nameof(Xsd.RegistrationNumberTypes.CNO));
			yield return new Mapping(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, nameof(Xsd.RegistrationNumberTypes.GCC));
			yield return new Mapping(OrgCusCode.UnitedKingdomCodeTypes.AirCargoAgentsListedNumber, nameof(Xsd.RegistrationNumberTypes.ACL));
			yield return new Mapping(OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, nameof(Xsd.RegistrationNumberTypes.EBS));
			yield return new Mapping(OrgCusCode.UnitedKingdomCodeTypes.CTOShed, nameof(Xsd.RegistrationNumberTypes.SHD));
			yield return new Mapping(OrgCusCode.UnitedKingdomCodeTypes.CustomsComprehensiveGuarantee, nameof(Xsd.RegistrationNumberTypes.CCG));

			// EU - European Union - GB IE FR DE ES PT PL CZ SL SK SE FI DK HU IT MT GR CY LT LV EE BE LX NL RO BG AT
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, nameof(Xsd.RegistrationNumberTypes.EOR));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, nameof(Xsd.RegistrationNumberTypes.TRN));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, nameof(Xsd.RegistrationNumberTypes.BTW));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, nameof(Xsd.RegistrationNumberTypes.AEO));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, nameof(Xsd.RegistrationNumberTypes.TEN));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber, nameof(Xsd.RegistrationNumberTypes.IPR));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, nameof(Xsd.RegistrationNumberTypes.DAN));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, nameof(Xsd.RegistrationNumberTypes.TID));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo, nameof(Xsd.RegistrationNumberTypes.CEN));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.OutwardProcessingReliefNumber, nameof(Xsd.RegistrationNumberTypes.OPR));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber, nameof(Xsd.RegistrationNumberTypes.REX));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration, nameof(Xsd.RegistrationNumberTypes.IOS));
			yield return new Mapping(OrgCusCode.CodeTypes.CustomsOfficeForTransit, nameof(Xsd.RegistrationNumberTypes.CTR));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit, nameof(Xsd.RegistrationNumberTypes.CEX));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.TrustedTrader, nameof(Xsd.RegistrationNumberTypes.TTD));
			yield return new Mapping(OrgCusCode.EuropeanUnionSharedCodeTypes.UKInternalMarketSchemeCode, nameof(Xsd.RegistrationNumberTypes.UKM));

			// EU's friends
			yield return new Mapping(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, nameof(Xsd.RegistrationNumberTypes.TCU));

			// FR
			yield return new Mapping(OrgCusCode.FranceCodeTypes.TVA, nameof(Xsd.RegistrationNumberTypes.TVA));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.NAF, nameof(Xsd.RegistrationNumberTypes.NAF));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.Siren, nameof(Xsd.RegistrationNumberTypes.SRN));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.Siret, nameof(Xsd.RegistrationNumberTypes.SRT));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.ALT, nameof(Xsd.RegistrationNumberTypes.ALT));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.IST, nameof(Xsd.RegistrationNumberTypes.IST));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.CIN, nameof(Xsd.RegistrationNumberTypes.CIN));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.ROU, nameof(Xsd.RegistrationNumberTypes.ROU));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.SUF, nameof(Xsd.RegistrationNumberTypes.SUF));

			// FR and territories (GP MQ GF YT PF WF PM MF BL TF NC except RE)
			yield return new Mapping(OrgCusCode.FranceCodeTypes.CI5, nameof(Xsd.RegistrationNumberTypes.CI5));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.SON, nameof(Xsd.RegistrationNumberTypes.SON));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.SOA, nameof(Xsd.RegistrationNumberTypes.SOA));
			yield return new Mapping(OrgCusCode.FranceCodeTypes.SOW, nameof(Xsd.RegistrationNumberTypes.SOW));

			// JP
			yield return new Mapping(OrgCusCode.JapanCodeTypes.CON, nameof(Xsd.RegistrationNumberTypes.CON));
			yield return new Mapping(OrgCusCode.JapanCodeTypes.CIE, nameof(Xsd.RegistrationNumberTypes.CIE));
			yield return new Mapping(OrgCusCode.JapanCodeTypes.FSB, nameof(Xsd.RegistrationNumberTypes.FSB));
			yield return new Mapping(OrgCusCode.JapanCodeTypes.JAS, nameof(Xsd.RegistrationNumberTypes.JAS));
			yield return new Mapping(OrgCusCode.JapanCodeTypes.LPC, nameof(Xsd.RegistrationNumberTypes.LPC));
			yield return new Mapping(OrgCusCode.JapanCodeTypes.NUC, nameof(Xsd.RegistrationNumberTypes.NUC));
			yield return new Mapping(OrgCusCode.JapanCodeTypes.AAL, nameof(Xsd.RegistrationNumberTypes.AAL));

			// IN
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, nameof(Xsd.RegistrationNumberTypes.IEC));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.SER, nameof(Xsd.RegistrationNumberTypes.SER));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, nameof(Xsd.RegistrationNumberTypes.PAN));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.TAN, nameof(Xsd.RegistrationNumberTypes.TAN));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.UIN, nameof(Xsd.RegistrationNumberTypes.UIN));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.GID, nameof(Xsd.RegistrationNumberTypes.GID));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.UDY, nameof(Xsd.RegistrationNumberTypes.UDY));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.ADH, nameof(Xsd.RegistrationNumberTypes.ADH));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, nameof(Xsd.RegistrationNumberTypes.BSN));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.ADC, nameof(Xsd.RegistrationNumberTypes.ADC));
			yield return new Mapping(IndiaOrgCusCodeInfo.OrgCusCodes.CAN, nameof(Xsd.RegistrationNumberTypes.CAN));

			// ES
			yield return new Mapping(OrgCusCode.SpainCodeTypes.NIF, nameof(Xsd.RegistrationNumberTypes.NIF));
			yield return new Mapping(OrgCusCode.SpainCodeTypes.DNI, nameof(Xsd.RegistrationNumberTypes.DNI));
			yield return new Mapping(OrgCusCode.SpainCodeTypes.IGC, nameof(Xsd.RegistrationNumberTypes.IGC));
			yield return new Mapping(SpainOrgCusCodeInfo.OrgCusCodes.SII, nameof(Xsd.RegistrationNumberTypes.SII));

			// MM
			yield return new Mapping(OrgCusCode.MyanmarCodeTypes.CMT, nameof(Xsd.RegistrationNumberTypes.CMT));

			// PA
			yield return new Mapping(PanamaOrgCusCodeInfo.OrgCusCodes.RUC, nameof(Xsd.RegistrationNumberTypes.RUC));
			yield return new Mapping(PanamaOrgCusCodeInfo.OrgCusCodes.NAO, nameof(Xsd.RegistrationNumberTypes.NAO));

			//Peru - if you change Panama's RUC code, add another RUC for Peru.
			//yield return new Mapping(OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, Xsd.RegistrationNumberTypes.RUC.ToString());

			//Peru - if you change Spain's DNI code, add another DNI for Peru.
			//yield return new Mapping(OrgCusCode.PeruCodeTypes.DNI, Xsd.RegistrationNumberTypes.DNI.ToString());

			// AO
			//Angola - if you change Spain's NIF code, add another NIF for Angola.
			//yield return new Mapping(OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, Xsd.RegistrationNumberTypes.NIF.ToString());

			// TD
			//Chad - if you change Spain's NIF code, add another NIF for Chad.
			//yield return new Mapping(OrgCusCode.ChadCodeTypes.NIF, Xsd.RegistrationNumberTypes.NIF.ToString());

			// BA
			yield return new Mapping(OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB, nameof(Xsd.RegistrationNumberTypes.IDB));
			yield return new Mapping(OrgCusCode.BosniaAndHerzegovinaCodeTypes.JMB, nameof(Xsd.RegistrationNumberTypes.JMB));
			yield return new Mapping(OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV, nameof(Xsd.RegistrationNumberTypes.PDV));

			// NL
			yield return new Mapping(OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber, nameof(Xsd.RegistrationNumberTypes.CCN));
			yield return new Mapping(OrgCusCode.NetherlandsCodeTypes.FenexLocationCode, nameof(Xsd.RegistrationNumberTypes.FNL));
			yield return new Mapping(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, nameof(Xsd.RegistrationNumberTypes.LFR));
			yield return new Mapping(OrgCusCode.NetherlandsCodeTypes.GFRVATNumberCode, nameof(Xsd.RegistrationNumberTypes.GFR));
			yield return new Mapping(OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode, nameof(Xsd.RegistrationNumberTypes.CGN));

			// LT
			yield return new Mapping(OrgCusCode.LithuaniaCodeTypes.PVM, nameof(Xsd.RegistrationNumberTypes.PVM));
			yield return new Mapping(OrgCusCode.LithuaniaCodeTypes.IMK, nameof(Xsd.RegistrationNumberTypes.IMK));

			// LV
			yield return new Mapping(OrgCusCode.LatviaCodeTypes.PVN, nameof(Xsd.RegistrationNumberTypes.PVN));

			// NO
			yield return new Mapping(OrgCusCode.NorwayCodeTypes.MVA, nameof(Xsd.RegistrationNumberTypes.MVA));
			yield return new Mapping(OrgCusCode.NorwayCodeTypes.EMD, nameof(Xsd.RegistrationNumberTypes.EMD));

			// SI
			yield return new Mapping(OrgCusCode.SloveniaCodeTypes.DDV, nameof(Xsd.RegistrationNumberTypes.DDV));

			// IT
			yield return new Mapping(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, nameof(Xsd.RegistrationNumberTypes.COD));
			yield return new Mapping(ItalyOrgCusCodeInfo.OrgCusCodes.NBO, nameof(Xsd.RegistrationNumberTypes.NBO));
			yield return new Mapping(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceAttive, nameof(Xsd.RegistrationNumberTypes.CAT));
			yield return new Mapping(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, nameof(Xsd.RegistrationNumberTypes.CUU));
			yield return new Mapping(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, nameof(Xsd.RegistrationNumberTypes.PEC));
			yield return new Mapping(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, nameof(Xsd.RegistrationNumberTypes.DAT));

			// CH
			yield return new Mapping(OrgCusCode.SwissCodeTypes.CAD, nameof(Xsd.RegistrationNumberTypes.CAD));
			yield return new Mapping(OrgCusCode.SwissCodeTypes.CAV, nameof(Xsd.RegistrationNumberTypes.CAV));
			yield return new Mapping(OrgCusCode.SwissCodeTypes.ASN, nameof(Xsd.RegistrationNumberTypes.ASN));

			// GR
			yield return new Mapping(OrgCusCode.GreeceCodeTypes.AFM, nameof(Xsd.RegistrationNumberTypes.AFM));
			yield return new Mapping(OrgCusCode.GreeceCodeTypes.DOY, nameof(Xsd.RegistrationNumberTypes.DOY));

			// MG
			yield return new Mapping(OrgCusCode.MadagascarCodeTypes.TIN, nameof(Xsd.RegistrationNumberTypes.TIN));
			yield return new Mapping(OrgCusCode.MadagascarCodeTypes.NIS, nameof(Xsd.RegistrationNumberTypes.NIS));

			// LK
			yield return new Mapping(OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.SVT));

			// CN
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.BST, nameof(Xsd.RegistrationNumberTypes.BST));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.CIQ, nameof(Xsd.RegistrationNumberTypes.CIQ));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.VAG, nameof(Xsd.RegistrationNumberTypes.VAG));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.VAS, nameof(Xsd.RegistrationNumberTypes.VAS));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.ENP, nameof(Xsd.RegistrationNumberTypes.ENP));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.USC, nameof(Xsd.RegistrationNumberTypes.USC));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.NGB, nameof(Xsd.RegistrationNumberTypes.NGB));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.MMR, nameof(Xsd.RegistrationNumberTypes.MMR));
			yield return new Mapping(OrgCusCode.ChinaCodeTypes.SMR, nameof(Xsd.RegistrationNumberTypes.SMR));

			// VE
			yield return new Mapping(OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal, nameof(Xsd.RegistrationNumberTypes.RIF));

			// PR
			yield return new Mapping(OrgCusCode.PuertoRicoCodeTypes.ImpuestoSobreVentasyUso, nameof(Xsd.RegistrationNumberTypes.IVU));
			yield return new Mapping(OrgCusCode.PuertoRicoCodeTypes.NumeroDeFianza, nameof(Xsd.RegistrationNumberTypes.TBN));

			// CR
			yield return new Mapping(CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber, nameof(Xsd.RegistrationNumberTypes.CIJ));
			yield return new Mapping(CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber, nameof(Xsd.RegistrationNumberTypes.DIM));
			yield return new Mapping(CostaRicaOrgCusCodeInfo.OrgCusCodes.UBILocacionCode, nameof(Xsd.RegistrationNumberTypes.UBI));
			yield return new Mapping(CostaRicaOrgCusCodeInfo.OrgCusCodes.PYMPYME, nameof(Xsd.RegistrationNumberTypes.PYM));
			yield return new Mapping(CostaRicaOrgCusCodeInfo.OrgCusCodes.EACEconomicActivityCode, nameof(Xsd.RegistrationNumberTypes.EAC));

			// SV
			yield return new Mapping(ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, nameof(Xsd.RegistrationNumberTypes.NRC));

			// PF
			yield return new Mapping(OrgCusCode.FrenchPolynesiaCodeTypes.TAH, nameof(Xsd.RegistrationNumberTypes.TAH));

			// HN
			yield return new Mapping(OrgCusCode.HondurasCodeTypes.RTN, nameof(Xsd.RegistrationNumberTypes.RTN));

			// KE
			yield return new Mapping(OrgCusCode.KenyaCodeTypes.PIN, nameof(Xsd.RegistrationNumberTypes.PIN));

			// NC
			yield return new Mapping(OrgCusCode.NewCaledoniaCodeTypes.RDT, nameof(Xsd.RegistrationNumberTypes.RDT));
			yield return new Mapping(OrgCusCode.NewCaledoniaCodeTypes.RID, nameof(Xsd.RegistrationNumberTypes.RID));
			yield return new Mapping(OrgCusCode.NewCaledoniaCodeTypes.TGC, nameof(Xsd.RegistrationNumberTypes.TGC));

			//TZ
			yield return new Mapping(OrgCusCode.TanzaniaCodeTypes.VRN, nameof(Xsd.RegistrationNumberTypes.VRN));

			//ZW
			yield return new Mapping(ZimbabweOrgCusCodeInfo.OrgCusCodes.BPN, nameof(Xsd.RegistrationNumberTypes.BPN));
			//KR
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.KBC, nameof(Xsd.RegistrationNumberTypes.KBC));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.KBT, nameof(Xsd.RegistrationNumberTypes.KBT));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode, nameof(Xsd.RegistrationNumberTypes.IPC));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, nameof(Xsd.RegistrationNumberTypes.BNO));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, nameof(Xsd.RegistrationNumberTypes.RNA));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident, nameof(Xsd.RegistrationNumberTypes.Item01));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner, nameof(Xsd.RegistrationNumberTypes.Item03));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.UnipassIDForIndividual, nameof(Xsd.RegistrationNumberTypes.Item05));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.UnipassIDForOrganization, nameof(Xsd.RegistrationNumberTypes.Item06));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.ForeignCompanyID, nameof(Xsd.RegistrationNumberTypes.Item07));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.OfficeID, nameof(Xsd.RegistrationNumberTypes.Item08));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.ECommerceCompanyID, nameof(Xsd.RegistrationNumberTypes.CEC));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.CourierCompanyID, nameof(Xsd.RegistrationNumberTypes.SDC));
			yield return new Mapping(KoreaSouthComplianceInfo.CodeTypes.CertificateOfOriginExporterNumber, nameof(Xsd.RegistrationNumberTypes.Item10));

			//TH
			yield return new Mapping(OrgCusCode.ThailandCodeTypes.BID, nameof(Xsd.RegistrationNumberTypes.BID));

			//RO
			yield return new Mapping(OrgCusCode.RomaniaCodeTypes.CIF, nameof(Xsd.RegistrationNumberTypes.CIF));
			yield return new Mapping(OrgCusCode.RomaniaCodeTypes.CNP, nameof(Xsd.RegistrationNumberTypes.CNP));

			//RS
			yield return new Mapping(SerbiaOrgCusCodeInfo.OrgCusCodes.JBK, nameof(Xsd.RegistrationNumberTypes.JBK));
			yield return new Mapping(SerbiaOrgCusCodeInfo.OrgCusCodes.PIB, nameof(Xsd.RegistrationNumberTypes.PIB));

			//SN
			yield return new Mapping(OrgCusCode.SenegalCodeTypes.NIN, nameof(Xsd.RegistrationNumberTypes.NIN));

			//CI
			yield return new Mapping(OrgCusCode.CoteDivoireCodeTypes.NCC, nameof(Xsd.RegistrationNumberTypes.NCC));

			//CM
			yield return new Mapping(OrgCusCode.CameroonCodeTypes.NIU, nameof(Xsd.RegistrationNumberTypes.NIU));

			//MZ
			yield return new Mapping(OrgCusCode.MozambiqueCodeTypes.NUI, nameof(Xsd.RegistrationNumberTypes.NUI));

			//DO
			yield return new Mapping(DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC, nameof(Xsd.RegistrationNumberTypes.RNC));
			yield return new Mapping(DominicanRepublicOrgCusCodeInfo.OrgCusCodes.CED, nameof(Xsd.RegistrationNumberTypes.CED));
			yield return new Mapping(DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RCS, nameof(Xsd.RegistrationNumberTypes.RCS));
			yield return new Mapping(DominicanRepublicOrgCusCodeInfo.OrgCusCodes.REG, nameof(Xsd.RegistrationNumberTypes.REG));

			//BD
			yield return new Mapping(OrgCusCode.BangladeshCodeTypes.AIN, nameof(Xsd.RegistrationNumberTypes.AIN));
			yield return new Mapping(OrgCusCode.BangladeshCodeTypes.BIN, nameof(Xsd.RegistrationNumberTypes.BIN));

			//NE
			yield return new Mapping(OrgCusCode.NigerCodeTypes.RCC, nameof(Xsd.RegistrationNumberTypes.RCC));

			//TR
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, nameof(Xsd.RegistrationNumberTypes.VDM));
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, nameof(Xsd.RegistrationNumberTypes.VTE));
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.VTC, nameof(Xsd.RegistrationNumberTypes.VTC));
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.TCK, nameof(Xsd.RegistrationNumberTypes.TCK));
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.MER, nameof(Xsd.RegistrationNumberTypes.MER));
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.VTP, nameof(Xsd.RegistrationNumberTypes.VTP));
			yield return new Mapping(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, nameof(Xsd.RegistrationNumberTypes.YFK));

			//BF
			yield return new Mapping(BurkinaFasoOrgCusCodeInfo.OrgCusCodes.IFU, nameof(Xsd.RegistrationNumberTypes.IFU));

			//JM
			yield return new Mapping(OrgCusCode.JamaicaCodeTypes.GCT, nameof(Xsd.RegistrationNumberTypes.GCT));

			//CW
			yield return new Mapping(OrgCusCode.CuracaoCodeTypes.CCR, nameof(Xsd.RegistrationNumberTypes.CCR));
			yield return new Mapping(OrgCusCode.CuracaoCodeTypes.CRB, nameof(Xsd.RegistrationNumberTypes.CRB));

			//TT
			yield return new Mapping(OrgCusCode.TrinidadAndTobagoCodeTypes.BIR, nameof(Xsd.RegistrationNumberTypes.BIR));

			//TG
			yield return new Mapping(OrgCusCode.TogoCodeTypes.NIC, nameof(Xsd.RegistrationNumberTypes.NIC));

			//HR
			yield return new Mapping(OrgCusCode.CroatiaCodeTypes.OIB, nameof(Xsd.RegistrationNumberTypes.OIB));

			//XK
			yield return new Mapping(OrgCusCode.KosovoCodeTypes.TVS, nameof(Xsd.RegistrationNumberTypes.TVS));
			yield return new Mapping(OrgCusCode.KosovoCodeTypes.NFK, nameof(Xsd.RegistrationNumberTypes.NFK));

			//EC
			yield return new Mapping(OrgCusCode.EcuadorCodeTypes.SRF, nameof(Xsd.RegistrationNumberTypes.SRF));
			yield return new Mapping(OrgCusCode.EcuadorCodeTypes.SRI, nameof(Xsd.RegistrationNumberTypes.SRI));

			//TW
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.TPC, nameof(Xsd.RegistrationNumberTypes.TPC));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.PID, nameof(Xsd.RegistrationNumberTypes.PID));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.PBR, nameof(Xsd.RegistrationNumberTypes.PBR));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.PIG, nameof(Xsd.RegistrationNumberTypes.PIG));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.MCI, nameof(Xsd.RegistrationNumberTypes.MCI));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.CBF, nameof(Xsd.RegistrationNumberTypes.CBF));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.EPZ, nameof(Xsd.RegistrationNumberTypes.EPZ));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.FTZ, nameof(Xsd.RegistrationNumberTypes.FTZ));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, nameof(Xsd.RegistrationNumberTypes.FRI));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, nameof(Xsd.RegistrationNumberTypes.ATP));
			yield return new Mapping(OrgCusCode.TaiwanCodeTypes.SciencePark, nameof(Xsd.RegistrationNumberTypes.SPK));

			//MA
			yield return new Mapping(OrgCusCode.MoroccoCodeTypes.ICE, nameof(Xsd.RegistrationNumberTypes.ICE));

			//GA
			yield return new Mapping(GabonOrgCusCodeInfo.OrgCusCodes.RCM, nameof(Xsd.RegistrationNumberTypes.RCM));

			//PK
			yield return new Mapping(PakistanOrgCusCodeInfo.OrgCusCodes.NTN, nameof(Xsd.RegistrationNumberTypes.NTN));

			//SA
			yield return new Mapping(SaudiArabiaOrgCusCodeInfo.OrgCusCodes.NAT, nameof(Xsd.RegistrationNumberTypes.NAT));

			//AD
			yield return new Mapping(AndorraOrgCusCodeInfo.OrgCusCodes.IGI, nameof(Xsd.RegistrationNumberTypes.IGI));

			//AE
			yield return new Mapping(OrgCusCode.UnitedArabEmiratesCodeTypes.CBLSNumber, nameof(Xsd.RegistrationNumberTypes.CBL));
			yield return new Mapping(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, nameof(Xsd.RegistrationNumberTypes.MPC));

			//CK
			yield return new Mapping(CookIslandsOrgCusCodeInfo.OrgCusCodes.RMD, nameof(Xsd.RegistrationNumberTypes.RMD));

			//TV
			yield return new Mapping(TuvaluOrgCusCodeInfo.OrgCusCodes.TCT, nameof(Xsd.RegistrationNumberTypes.TCT));

			//FO
			yield return new Mapping(FaeroeIslandsOrgCusCodeInfo.OrgCusCodes.MVG, nameof(Xsd.RegistrationNumberTypes.MVG));

			//HU
			yield return new Mapping(HungaryOrgCusCodeInfo.OrgCusCodes.IDM, nameof(Xsd.RegistrationNumberTypes.IDM));

			//JO
			yield return new Mapping(JordanOrgCusCodeInfo.OrgCusCodes.BusinessActivityNumber, nameof(Xsd.RegistrationNumberTypes.BAN));
		}

		public static readonly OrgCusCodeXmlMappings Instance = new OrgCusCodeXmlMappings();

		public new Xsd.RegistrationNumberTypes GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.RegistrationNumberTypes.GST, errorContext, notifications);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML Mapping Name")]
		protected override string Name
		{
			get { return "Registration Number"; }
		}
	}
}
