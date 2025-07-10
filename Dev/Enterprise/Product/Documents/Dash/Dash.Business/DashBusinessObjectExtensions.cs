using System;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.Dash.Business.Entities;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Dash.Business
{
	public static class DashBusinessObjectExtensions
	{
		public static IDashCommercialInvoice ToEntityCommercialInvoice(this DashCommercialInvoice commercialInvoiceBusinessObject)
		{
			var entityCommercialInvoice = new EntityDashCommercialInvoice();

			entityCommercialInvoice.DCI_PK = commercialInvoiceBusinessObject.PK.IsEmpty
				? Guid.Empty
				: commercialInvoiceBusinessObject.PK.ToGuid();
			entityCommercialInvoice.DCI_DDD_DashDocID = commercialInvoiceBusinessObject.DCI_DDD_DashDocID.IsEmpty
				? Guid.Empty
				: commercialInvoiceBusinessObject.DCI_DDD_DashDocID.ToGuid();
			entityCommercialInvoice.DCI_GrossTotal = commercialInvoiceBusinessObject.DCI_GrossTotal;
			entityCommercialInvoice.DCI_ImporterParsedRawText = commercialInvoiceBusinessObject.DCI_ImporterParsedRawText;
			entityCommercialInvoice.DCI_ImporterParsedAddressRawText = commercialInvoiceBusinessObject.DCI_ImporterParsedAddressRawText;
			entityCommercialInvoice.DCI_ImporterParsedNameRawText = commercialInvoiceBusinessObject.DCI_ImporterParsedNameRawText;
			entityCommercialInvoice.DCI_SupplierParsedRawText = commercialInvoiceBusinessObject.DCI_SupplierParsedRawText;
			entityCommercialInvoice.DCI_SupplierParsedAddressRawText = commercialInvoiceBusinessObject.DCI_SupplierParsedAddressRawText;
			entityCommercialInvoice.DCI_SupplierParsedNameRawText = commercialInvoiceBusinessObject.DCI_SupplierParsedNameRawText;
			entityCommercialInvoice.DCI_Incoterm = commercialInvoiceBusinessObject.DCI_Incoterm;
			entityCommercialInvoice.DCI_InvoiceDate = commercialInvoiceBusinessObject.DCI_InvoiceDate.IsEmpty
				? null
				: commercialInvoiceBusinessObject.DCI_InvoiceDate.ToDateTime();
			entityCommercialInvoice.DCI_InvoiceNumber = commercialInvoiceBusinessObject.DCI_InvoiceNumber;
			entityCommercialInvoice.DCI_OA_MatchedImporterAddressID = commercialInvoiceBusinessObject.DCI_OA_MatchedImporterAddressID.IsEmpty
				? null
				: commercialInvoiceBusinessObject.DCI_OA_MatchedImporterAddressID.ToGuid();
			entityCommercialInvoice.DCI_OA_MatchedSupplierAddressID = commercialInvoiceBusinessObject.DCI_OA_MatchedSupplierAddressID.IsEmpty
				? null
				: commercialInvoiceBusinessObject.DCI_OA_MatchedSupplierAddressID.ToGuid();
			entityCommercialInvoice.DCI_OH_MatchedImporterID = commercialInvoiceBusinessObject.DCI_OH_MatchedImporterID.IsEmpty
				? null
				: commercialInvoiceBusinessObject.DCI_OH_MatchedImporterID.ToGuid();
			entityCommercialInvoice.DCI_OH_MatchedSupplierID = commercialInvoiceBusinessObject.DCI_OH_MatchedSupplierID.IsEmpty
				? null
				: commercialInvoiceBusinessObject.DCI_OH_MatchedSupplierID.ToGuid();
			entityCommercialInvoice.DCI_RX_NKInvoiceCurrency = commercialInvoiceBusinessObject.DCI_RX_NKInvoiceCurrency;
			entityCommercialInvoice.DCI_SystemCreateTimeUtc = commercialInvoiceBusinessObject.DCI_SystemCreateTimeUtc.IsEmpty
				? DateTime.MinValue
				: commercialInvoiceBusinessObject.DCI_SystemCreateTimeUtc.ToDateTime();
			entityCommercialInvoice.DCI_SystemCreateUser = commercialInvoiceBusinessObject.DCI_SystemCreateUser;
			entityCommercialInvoice.DCI_SystemLastEditTimeUtc = commercialInvoiceBusinessObject.DCI_SystemLastEditTimeUtc.IsEmpty
				? DateTime.MinValue
				: commercialInvoiceBusinessObject.DCI_SystemLastEditTimeUtc.ToDateTime();
			entityCommercialInvoice.DCI_SystemLastEditUser = commercialInvoiceBusinessObject.DCI_SystemLastEditUser;

			entityCommercialInvoice.MatchedImporterID = commercialInvoiceBusinessObject.MatchedImporterID?.ToEntityOrgHeaderInfo();
			entityCommercialInvoice.MatchedSupplierID = commercialInvoiceBusinessObject.MatchedSupplierID?.ToEntityOrgHeaderInfo();
			entityCommercialInvoice.MatchedImporterAddressID = commercialInvoiceBusinessObject.MatchedImporterAddressID?.ToEntityOrgAddressInfo();
			entityCommercialInvoice.MatchedSupplierAddressID = commercialInvoiceBusinessObject.MatchedSupplierAddressID?.ToEntityOrgAddressInfo();

			if (commercialInvoiceBusinessObject.CommercialInvoiceLineItems != null)
			{
				foreach (DashCommercialInvoiceLineItem lineItem in commercialInvoiceBusinessObject.CommercialInvoiceLineItems)
				{
					var entityCommercialInvoiceLineItem = lineItem.ToEntityCommercialInvoiceLineItem();
					entityCommercialInvoice.DashCommercialInvoiceLineItems.Add(entityCommercialInvoiceLineItem);
				}
			}

			return entityCommercialInvoice;
		}

		static IDashCommercialInvoiceLineItem ToEntityCommercialInvoiceLineItem(this DashCommercialInvoiceLineItem dashCommercialInvoiceLineItem)
		{
			var entityCommercialInvoiceLineItem = new EntityDashCommercialInvoiceLineItem();

			entityCommercialInvoiceLineItem.DLI_DCI_HeaderID = dashCommercialInvoiceLineItem.DLI_DCI_HeaderID.IsEmpty
				? Guid.Empty
				: dashCommercialInvoiceLineItem.DLI_DCI_HeaderID.ToGuid();
			entityCommercialInvoiceLineItem.DLI_CI_MatchedHSCodeID = dashCommercialInvoiceLineItem.DLI_CI_MatchedHSCodeID.IsEmpty
				? null
				: dashCommercialInvoiceLineItem.DLI_CI_MatchedHSCodeID.ToGuid();

			entityCommercialInvoiceLineItem.DLI_OP_MatchedProductCodeID = dashCommercialInvoiceLineItem.DLI_OP_MatchedProductCodeID.IsEmpty
				? null
				: dashCommercialInvoiceLineItem.DLI_OP_MatchedProductCodeID.ToGuid();

			entityCommercialInvoiceLineItem.DLI_EditedHSCode = dashCommercialInvoiceLineItem.DLI_EditedHSCode;
			entityCommercialInvoiceLineItem.DLI_EditedProductCode = dashCommercialInvoiceLineItem.DLI_EditedProductCode;
			entityCommercialInvoiceLineItem.DLI_EditedProductDescription = dashCommercialInvoiceLineItem.DLI_EditedProductDescription;
			entityCommercialInvoiceLineItem.DLI_F3_NKUnitType = dashCommercialInvoiceLineItem.DLI_F3_NKUnitType;
			entityCommercialInvoiceLineItem.DLI_HSCode = dashCommercialInvoiceLineItem.DLI_HSCode;
			entityCommercialInvoiceLineItem.DLI_Index = dashCommercialInvoiceLineItem.DLI_Index;
			entityCommercialInvoiceLineItem.DLI_IsActive = dashCommercialInvoiceLineItem.DLI_IsActive;
			entityCommercialInvoiceLineItem.DLI_LineTotal = dashCommercialInvoiceLineItem.DLI_LineTotal;
			entityCommercialInvoiceLineItem.DLI_MatchedType = dashCommercialInvoiceLineItem.DLI_MatchedType;
			entityCommercialInvoiceLineItem.DLI_ParsedHSCode = dashCommercialInvoiceLineItem.DLI_ParsedHSCode;
			entityCommercialInvoiceLineItem.DLI_ParsedProductCode = dashCommercialInvoiceLineItem.DLI_ParsedProductCode;
			entityCommercialInvoiceLineItem.DLI_ParsedProductDescription = dashCommercialInvoiceLineItem.DLI_ParsedProductDescription;
			entityCommercialInvoiceLineItem.DLI_PricePerUnit = dashCommercialInvoiceLineItem.DLI_PricePerUnit;
			entityCommercialInvoiceLineItem.DLI_ProductCode = dashCommercialInvoiceLineItem.DLI_ProductCode;
			entityCommercialInvoiceLineItem.DLI_ProductDescription = dashCommercialInvoiceLineItem.DLI_ProductDescription;
			entityCommercialInvoiceLineItem.DLI_Quantity = dashCommercialInvoiceLineItem.DLI_Quantity;
			entityCommercialInvoiceLineItem.DLI_RN_NKOriginCountry = dashCommercialInvoiceLineItem.DLI_RN_NKOriginCountry;
			entityCommercialInvoiceLineItem.DLI_ParsedRawText = dashCommercialInvoiceLineItem.DLI_ParsedRawText;
			entityCommercialInvoiceLineItem.DLI_SystemCreateTimeUtc = dashCommercialInvoiceLineItem.DLI_SystemCreateTimeUtc.IsEmpty
				? DateTime.MinValue
				: dashCommercialInvoiceLineItem.DLI_SystemCreateTimeUtc.ToDateTime();
			entityCommercialInvoiceLineItem.DLI_SystemCreateUser = dashCommercialInvoiceLineItem.DLI_SystemCreateUser;
			entityCommercialInvoiceLineItem.DLI_SystemLastEditTimeUtc = dashCommercialInvoiceLineItem.DLI_SystemLastEditTimeUtc.IsEmpty
				? DateTime.MinValue
				: dashCommercialInvoiceLineItem.DLI_SystemLastEditTimeUtc.ToDateTime();
			entityCommercialInvoiceLineItem.DLI_SystemLastEditUser = dashCommercialInvoiceLineItem.DLI_SystemLastEditUser;

			return entityCommercialInvoiceLineItem;
	}

		static IOrgHeaderInfo ToEntityOrgHeaderInfo(this OrgHeader orgHeader)
		{
			var orgHeaderInfo = new EntityOrgHeaderInfo();

			orgHeaderInfo.OH_PK = orgHeader.PK.IsEmpty
				? Guid.Empty
				: orgHeader.PK.ToGuid();
			orgHeaderInfo.OH_Code = orgHeader.OH_Code;
			orgHeaderInfo.OH_FullName = orgHeader.OH_FullName;
			orgHeaderInfo.OH_IsActive = orgHeader.OH_IsActive;
			orgHeaderInfo.OH_IsAirCTO = orgHeader.OH_IsAirCTO;
			orgHeaderInfo.OH_IsAirLine = orgHeader.OH_IsAirLine;
			orgHeaderInfo.OH_IsAirWholesaler = orgHeader.OH_IsAirWholesaler;
			orgHeaderInfo.OH_IsConsignee = orgHeader.OH_IsConsignee;
			orgHeaderInfo.OH_IsConsignor = orgHeader.OH_IsConsignor;
			orgHeaderInfo.OH_IsContainerLeasingCompany = orgHeader.OH_IsContainerLeasingCompany;
			orgHeaderInfo.OH_IsControllingCustomer = orgHeader.OH_IsControllingCustomer;
			orgHeaderInfo.OH_IsFerryWaterTerminal = orgHeader.OH_IsFerryWaterTerminal;
			orgHeaderInfo.OH_IsForwarder = orgHeader.OH_IsForwarder;
			orgHeaderInfo.OH_IsInlandWaterwayProvider = orgHeader.OH_IsInlandWaterwayProvider;
			orgHeaderInfo.OH_IsLineHaulProvider = orgHeader.OH_IsLineHaulProvider;
			orgHeaderInfo.OH_IsLocalTransport = orgHeader.OH_IsLocalTransport;
			orgHeaderInfo.OH_IsMiscFreightServices = orgHeader.OH_IsMiscFreightServices;
			orgHeaderInfo.OH_IsRailHead = orgHeader.OH_IsRailHead;
			orgHeaderInfo.OH_IsRailProvider = orgHeader.OH_IsRailProvider;
			orgHeaderInfo.OH_IsSeaCTO = orgHeader.OH_IsSeaCTO;
			orgHeaderInfo.OH_IsSeaWholesaler = orgHeader.OH_IsSeaWholesaler;
			orgHeaderInfo.OH_IsShippingConsortium = orgHeader.OH_IsShippingConsortium;
			orgHeaderInfo.OH_IsShippingLine = orgHeader.OH_IsShippingLine;
			orgHeaderInfo.OH_IsShippingProvider = orgHeader.OH_IsShippingProvider;
			orgHeaderInfo.OH_IsVGMContractor = orgHeader.OH_IsVGMContractor;
			orgHeaderInfo.OH_RL_NKClosestPort = orgHeader.OH_RL_NKClosestPort;
			orgHeaderInfo.OH_RSL_ShippingLine = orgHeader.OH_RSL_ShippingLine.IsEmpty
				? null
				: orgHeader.OH_RSL_ShippingLine.ToGuid();

			return orgHeaderInfo;
		}

		static IOrgAddressInfo ToEntityOrgAddressInfo(this OrgAddress orgAddress)
		{
			var orgAddressInfo = new EntityOrgAddressInfo();

			orgAddressInfo.OA_PK = orgAddress.PK.IsEmpty
				? Guid.Empty
				: orgAddress.PK.ToGuid();

			orgAddress.OA_Code = orgAddress.OA_Code;
			orgAddress.OA_IsActive = orgAddress.OA_IsActive;
			orgAddress.OA_AdditionalAddressInformation = orgAddress.OA_AdditionalAddressInformation;
			orgAddress.OA_Address1 = orgAddress.OA_Address1;
			orgAddress.OA_Address2 = orgAddress.OA_Address2;
			orgAddress.OA_AuthorityToLeave = orgAddress.OA_AuthorityToLeave;
			orgAddress.OA_City = orgAddress.OA_City;
			orgAddress.OA_CompanyNameOverride = orgAddress.OA_CompanyNameOverride;
			orgAddress.OA_Email = orgAddress.OA_Email;
			orgAddress.OA_Fax = orgAddress.OA_Fax;
			orgAddress.OA_GeofencePolygon = orgAddress.OA_GeofencePolygon;
			orgAddress.OA_GeoLocation = orgAddress.OA_GeoLocation;
			orgAddress.OA_Mobile = orgAddress.OA_Mobile;
			orgAddress.OA_OH = orgAddress.OA_OH;
			orgAddress.OA_Phone = orgAddress.OA_Phone;
			orgAddress.OA_PostCode = orgAddress.OA_PostCode;
			orgAddress.OA_RL_NKRelatedPortCode = orgAddress.OA_RL_NKRelatedPortCode;
			orgAddress.OA_RN_NKCountryCode = orgAddress.OA_RN_NKCountryCode;
			orgAddress.OA_State = orgAddress.OA_State;

			return orgAddressInfo;
		}
	}
}
