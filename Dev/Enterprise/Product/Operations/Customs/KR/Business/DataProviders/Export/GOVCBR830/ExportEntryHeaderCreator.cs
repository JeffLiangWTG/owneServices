using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ExportEntryHeaderCreator
	{
		static class MaxByteSizes
		{
			public const int GoodsLocationAddress = 150;
			public const int DeclarantAdditionalDescription = JobComInvoiceHeader.Schema.JZRemarksMaxLength;
			public const int DeclarantCompanyName = 50;
			public const int DeclarantRepresentativeName = 12;
			public const int ExporterCompanyName = 28;
			public const int SupplierCompanyName = 28;
			public const int SupplierRepresentativeName = 12;
			public const int ManufacturerCompanyName = 28;
			public const int FreightForwarderContactName = 30;
			public const int DocumentName = 300;
			public const int ReasonForMissingApprovalNumber = 200;
		}

		const string PreApprovalType = "A";

		public ExportEntryHeader Create(CusEntryHeader entry)
		{
			var entryHeaderData = PopulateEntryHeaderLevelData(entry);

			entryHeaderData.Containers = PopulateContainers(entry);

			var entryLineDataList = new List<ExportEntryLine>();
			var orderedEntryLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			foreach (CusEntryLine entryLine in orderedEntryLines)
			{
				var entryLineData = PopulateEntryLine(entry, entryLine);
				entryLineDataList.Add(entryLineData);
			}
			entryHeaderData.EntryLines = entryLineDataList.ToArray();

			return entryHeaderData;
		}
		ExportEntryHeader PopulateEntryHeaderLevelData(CusEntryHeader entry)
		{
			var entryHeaderData = new ExportEntryHeader();
			#region Declaration
			var declaration = entry.Declaration;
			entryHeaderData.ExportDeclarationNumber = entry.EntryNumber;
			entryHeaderData.TransactionType = declaration.JE_ExportGoodsType;
			entryHeaderData.ExportTypeCode = declaration.JE_MessageSubType;
			entryHeaderData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			entryHeaderData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			entryHeaderData.CountryOfDestination = declaration.JE_GoodsDestination;
			entryHeaderData.PortOfLoading = declaration.KRPortOfLoading;
			entryHeaderData.SouthNorthTradeIdentification = declaration.JE_TradeIDWithKP;
			if (!declaration.JE_ExportDate.IsEmpty)
			{
				entryHeaderData.DepartureDate = declaration.JE_ExportDate.ToDateTime();
			}
			entryHeaderData.DrawbackApplicantType = entry.RandomHeader.JZ_DRWApplicantType;
			entryHeaderData.ExporterType = declaration.JE_ExporterType;
			entryHeaderData.OutOfHoursDeclarationIndicator = OutOfHoursDeclarationIndicatorCodeList.Codes.N;
			entryHeaderData.ReturnReason = declaration.JE_ReturnReason;
			entryHeaderData.ReturnType = declaration.JE_ReturnType;
			entryHeaderData.GoodsStatus = declaration.JE_GoodsCondition;
			entryHeaderData.ApplicationForSimpleDrawback = declaration.JE_SimpleDRWApp;
			entryHeaderData.ContainerizedIndicator = (entry.Containers != null && entry.Containers.Length > 0);
			entryHeaderData.SouthNorthTradeYN = declaration.JE_TradeIndicatorWithKP;
			entryHeaderData.ContainerPackMode = declaration.JE_ContainerPackMode;

			var cargoManagement = new ExportCargoMeanagement();
			cargoManagement.ImportCargoManagementNumber = entry.RandomHeader.JZ_ImportCargoManagementNumber;
			entryHeaderData.CargoManagement = cargoManagement;
			entryHeaderData.TransportMode = Constants.TransportModes.ConvertTransportMode(declaration.JE_TransportMode);
			entryHeaderData.VesselNameOrFlightNo = declaration.IsSea ? declaration.JE_VesselName : declaration.JE_VoyageFlightNo;
			entryHeaderData.UCR = declaration.JE_UCR;
			entryHeaderData.CarrierID = declaration.JE_CarrierCode;

			var query = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_Code, declaration.JE_CarrierCode);
			query.AddToFilter(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, Enterprise.Core.Constants.CountryCodes.KoreaSouth);
			ZZRefCarrierCombined carrierCode = entry.Factory.LoadTop1<ZZRefCarrierCombined>(query);

			if (carrierCode != null)
			{
				entryHeaderData.ShippingLineOrAirlineName = carrierCode.UnTranslatedZZ4_Description;
			}
			entryHeaderData.TotalCustomsValue = entry.CustomsValue;
			entryHeaderData.PackType = declaration.JE_TotalNoOfPacksPackType;

			entryHeaderData.GoodsLocationPostcode = declaration.JE_LocationQualifier;
			entryHeaderData.GoodsLocationAddress = StringUtils.GetSubstringBytes(declaration.JE_LocationOfGoods, MaxByteSizes.GoodsLocationAddress);
			entryHeaderData.GoodsLocationAdditionalDetails = declaration.JE_SubLocationOfGoods;
			entryHeaderData.GoodsLocationBondedAreaCode = declaration.JE_LocationOtherInformation;
			entryHeaderData.LocationIDInBondedArea = declaration.JE_LocationIDInBondedArea;
			if (!declaration.UnderbondMovementArrivalDate.IsEmpty && !declaration.UnderbondMovementDepartureDate.IsEmpty)
			{
				entryHeaderData.BondedTransportationFromDate = declaration.UnderbondMovementArrivalDate.ToDateTime();
				entryHeaderData.BondedTransportationToDate = declaration.UnderbondMovementDepartureDate.ToDateTime();
			}

			var preferredInspectionDate = entry.IsFixedInspectionDateRelevant ? entry.FixedInspectionDate : declaration.InspectionDate;
			if (preferredInspectionDate.IsValid)
			{
				entryHeaderData.PreferredInspectionDate = preferredInspectionDate.ToDateTime();
			}
			entryHeaderData.DeclarationProcedureType = declaration.JE_ProcedureType;
			#endregion

			PopulateInvoiceHeaderData(entryHeaderData, entry);
			PopulateOrganisationData(entryHeaderData, entry);
			return entryHeaderData;
		}
		void PopulateInvoiceHeaderData(ExportEntryHeader entryHeaderData, CusEntryHeader entry)
		{
			var invoiceRandomHeader = entry.RandomHeader;
			entryHeaderData.InvoicePaymentTerm = invoiceRandomHeader.JZ_PaymentTerms;
			entryHeaderData.LCNo = invoiceRandomHeader.JZ_LetterOfCreditNumber;
			entryHeaderData.Incoterm = entry.Incoterm;
			entryHeaderData.Currency = entry.InvoiceAmountCurrency;
			var usd = RefCurrency.LoadFromCurrencyCode(entry.Factory, Core.Constants.CurrencyCodes.UnitedStates);
			entryHeaderData.ExchangeRate = entry.CurrencyConverter.GetExchangeRate(usd);
			entryHeaderData.TotalInvoiceAmount = entry.TotalInvoiceAmount;
			entryHeaderData.TotalPackQty = entry.TotalPackages;
			entryHeaderData.TotalGrossWeightInKG = entry.TotalGrossWeightInKG;
			entryHeaderData.Freight = entry.Freight;
			entryHeaderData.Insurance = entry.Insurance;

			if (!IsExchangeRatePublished(entry.InvoiceAmountCurrency))
			{
				var currency = RefCurrency.LoadFromCurrencyCode(entry.Factory, entry.InvoiceAmountCurrency);
				var currencyKRW = RefCurrency.LoadFromCurrencyCode(entry.Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
				entryHeaderData.TotalInvoiceAmount = entry.CurrencyConverter.ConvertExact(new Money(entryHeaderData.TotalInvoiceAmount, currency), currencyKRW).Amount;
				entryHeaderData.Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			}

			var strBuilder = new ZStringBuilder();
			foreach (var invoiceHeader in entry.InvoiceHeaders())
			{
				if (!string.IsNullOrEmpty(invoiceHeader.JZ_Remarks))
				{
					strBuilder.Append(invoiceHeader.JZ_Remarks);
				}
			}
			ZString shortDeclarantAdditionalDescription = strBuilder.ToStringWithDelimiterBetweenAppends(" ");
			entryHeaderData.DeclarantAdditionalDescription = StringUtils.GetSubstringBytes(shortDeclarantAdditionalDescription, MaxByteSizes.DeclarantAdditionalDescription);
		}

		void PopulateOrganisationData(ExportEntryHeader entryHeaderData, CusEntryHeader entry)
		{
			var declaration = entry.Declaration;
			entryHeaderData.UnipassDeclarantID = declaration.UNIPASSDeclarantID;

			if (declaration.BrokerAddress != null)
			{
				entryHeaderData.Declarant = new Organisation(RoleType.Declarant)
				{
					CompanyName = StringUtils.GetSubstringBytes(declaration.BrokerAddress.CompanyName, MaxByteSizes.DeclarantCompanyName),
					RepresentativeName = StringUtils.GetSubstringBytes(declaration.BrokerAddress.Header.GetRepresentativeName(), MaxByteSizes.DeclarantRepresentativeName)
				};
			}
			var exporterPostCode = ZString.Empty;
			if (declaration.SellerAddress != null)
			{
				var exporterCodes = new string[] { IdentificationType.UnipassIDForOrganization, IdentificationType.OfficeID };
				entryHeaderData.Exporter = new Organisation(RoleType.Exporter)
				{
					CompanyName = StringUtils.GetSubstringBytes(declaration.SellerAddress.CompanyName, MaxByteSizes.ExporterCompanyName),
				};
				entryHeaderData.Exporter.SetRegistrationIDNumbers(declaration.SellerAddress.GetRegistrationIDNumbers(exporterCodes));
				exporterPostCode = declaration.SellerAddress.Postcode;
			}

			var supplier = declaration.SupplierAddress;
			if (supplier != null)
			{
				var supplierCodes = new string[] { IdentificationType.UnipassIDForOrganization, IdentificationType.OfficeID, IdentificationType.BuildingNumber, IdentificationType.RoadNameCode, IdentificationType.CorporationCode };
				entryHeaderData.Supplier = new Organisation(RoleType.Supplier)
				{
					CompanyName = StringUtils.GetSubstringBytes(supplier.CompanyName, MaxByteSizes.SupplierCompanyName),
					RepresentativeName = StringUtils.GetSubstringBytes(supplier.Header.GetRepresentativeName(), MaxByteSizes.SupplierRepresentativeName),
					AddressLine1 = supplier.Address1,
					AddressLine2 = supplier.Address2,
					Postcode = supplier.Postcode,
					RoadNameCode = supplier.GetRoadNameCode(),
					BuildingNumber = supplier.GetBuildingNumber(),
					IsIndividual = supplier.Header.GetIsIndividual(),
				};
				entryHeaderData.Supplier.SetRegistrationIDNumbers(supplier.GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(supplierCodes));
			}

			var randomHeader = entry.RandomHeader;
			if (randomHeader.ManufacturerAddress != null)
			{
				var manufacturerCodes = new string[] { IdentificationType.UnipassIDForOrganization, IdentificationType.OfficeID };
				entryHeaderData.Manufacturer = new Organisation(RoleType.Manufacturer)
				{
					CompanyName = StringUtils.GetSubstringBytes(randomHeader.ManufacturerAddress.CompanyName, MaxByteSizes.ManufacturerCompanyName),
					Postcode = randomHeader.ManufacturerAddress.Postcode,
					IsIndividual = randomHeader.ManufacturerAddress.Header.GetIsIndividual(),
				};
				List<IDNumberAndType> idNumberAndTypes = new List<IDNumberAndType>(randomHeader.ManufacturerAddress.GetRegistrationIDNumbers(manufacturerCodes));
				if (!idNumberAndTypes.Any(item => item.Type == IdentificationType.UnipassIDForOrganization))
				{
					idNumberAndTypes.Insert(0, new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = randomHeader.ManufacturerUnipassID });
				}
				entryHeaderData.Manufacturer.SetRegistrationIDNumbers(idNumberAndTypes.ToArray());
				entryHeaderData.IndustrialParkCode = randomHeader.ManufacturerIPCCode;
			}
			else
			{
				entryHeaderData.Manufacturer = new Organisation(RoleType.Manufacturer)
				{
					CompanyName = ManufacturerDefaultCode.CompanyName,
					Postcode = exporterPostCode
				};
				if (declaration.JE_ProcedureType != DeclarationProcedureTypeList.Codes.E)
				{
					var manufacturerCodes = new IDNumberAndType[]
					{
						new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = ManufacturerDefaultCode.UnipassID },
					};
					entryHeaderData.Manufacturer.SetRegistrationIDNumbers(manufacturerCodes);
					entryHeaderData.IndustrialParkCode = ManufacturerDefaultCode.IndustrialParkCode;
				}
			}

			if (randomHeader.Buyer != null)
			{
				entryHeaderData.Importer = new Organisation(RoleType.Importer)
				{
					CompanyName = randomHeader.Buyer.OH_FullName,
				};
				entryHeaderData.Importer.SetRegistrationIDNumbers(new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = randomHeader.BuyerID } });
			}
			if (declaration.Forwarder != null)
			{
				entryHeaderData.FreightForwarderContactName = StringUtils.GetSubstringBytes(declaration.Forwarder.GetRepresentativeName(), MaxByteSizes.FreightForwarderContactName);
			}
			entryHeaderData.FinalLoadingPlace = declaration.FinalBondedWarehouse;
		}

		ExportContainer[] PopulateContainers(CusEntryHeader entry)
		{
			var containers = new List<ExportContainer>();
			var orderedContainerPivots = entry.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().OrderBy(x => x.CCE_SequenceNumber);
			foreach (var pivot in orderedContainerPivots)
			{
				var container = pivot.Container;
				var exportContainer = new ExportContainer();
				exportContainer.SequenceNo = pivot.CCE_SequenceNumber.ToString(IdNumbersFormatConstants.ExportContainerNo);
				exportContainer.ContainerNo = container.CO_ContainerNumber;
				containers.Add(exportContainer);
			}
			return containers.ToArray();
		}

		ExportEntryLine PopulateEntryLine(CusEntryHeader entry, CusEntryLine entryLine)
		{
			var entryLineData = new ExportEntryLine();
			var invoiceLine = entryLine.RandomLine;
			entryLineData.EntryLineNo = entryLine.CL_LineNumber.ToString(IdNumbersFormatConstants.EntryLineNo);
			entryLineData.CustomsValue = entryLine.CL_CustomsValue;
			entryLineData.HSCode = entryLine.CL_AdValoremTariff;
			entryLineData.HSDescription = invoiceLine.UniversalTariff?.ZZ1_Description;
			entryLineData.TradeName = invoiceLine.JI_Model;
			entryLineData.BrandName = invoiceLine.JI_BrandName;
			entryLineData.InvoiceNo = invoiceLine.InvoiceHeader.JZ_InvoiceNumber;
			entryLineData.CountryOfOrigin = invoiceLine.JI_CountryOfOrigin;
			entryLineData.DocumentAttached = YesNoList.Codes.No;

			bool unitQtyIsWeightUQ = Core.Constants.Weight.ContainsCode(invoiceLine.JI_CustomsUnitQty);
			var certificateOfOrigin = invoiceLine.CertificateOfOriginData;
			if (certificateOfOrigin != null)
			{
				entryLineData.CountryOfOriginDeterminationRule = certificateOfOrigin.CSI_SubType;
				entryLineData.CertificateOfOriginIssued = certificateOfOrigin.CSI_Code;
			}
			if (!invoiceLine.PRA_ReferenceNumber.IsEmpty)
			{
				entryLineData.PreApprovalType = PreApprovalType;
			}
			entryLineData.PreApprovalNo = invoiceLine.PRA_ReferenceNumber;
			if (!invoiceLine.PRA_DateOfIssue.IsEmpty)
			{
				entryLineData.PreApprovalEffectiveFromDate = invoiceLine.PRA_DateOfIssue.ToDateTime();
			}
			if (!invoiceLine.PRA_DateOfExpiry.IsEmpty)
			{
				entryLineData.PreApprovalEffectiveToDate = invoiceLine.PRA_DateOfExpiry.ToDateTime();
			}
			entryLineData.CountryOfOriginLabelLocation = invoiceLine.JI_COOLabelLocation;
			entryLineData.FTAType = invoiceLine.JI_PrimaryPreference;

			var invoiceLines = new List<ExportInvoiceLine>();
			var orderedInvoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_SequenceNumber);
			foreach (var orderedInvoiceLine in orderedInvoiceLines)
			{
				entryLineData.NetWeightInKG += Core.Constants.Weight.Convert(orderedInvoiceLine.JI_NetWeight, orderedInvoiceLine.JI_NetWeightUQ, Core.Constants.Weight.Kilograms);
				if (!unitQtyIsWeightUQ)
				{
					entryLineData.Qty += orderedInvoiceLine.JI_CustomsQuantity;
				}
				entryLineData.PackQty += orderedInvoiceLine.JI_NoOfPacks;
				if (string.IsNullOrEmpty(entryLineData.PackType))
				{
					entryLineData.PackType = orderedInvoiceLine.JI_PackType;
				}
				var invoiceLineData = PopulateInvoiceLine(orderedInvoiceLine);
				invoiceLines.Add(invoiceLineData);
			}

			if (!unitQtyIsWeightUQ)
			{
				entryLineData.QtyUnit = invoiceLine.JI_CustomsUnitQty;
			}
			entryLineData.NetWeightUQ = Core.Constants.Weight.Kilograms;
			entryLineData.ImportDeclarationNumber = invoiceLine.JI_PreviousEntryNumber;
			entryLineData.ImportEntryLineNo = invoiceLine.JI_PreviousEntryLineNumber == ZShort.Zero ? string.Empty : invoiceLine.JI_PreviousEntryLineNumber.ToString(IdNumbersFormatConstants.ImportEntryLineNo);
			entryLineData.SkipManifestReporting = invoiceLine.JI_SkipManifestReport;

			entryLineData.InvoiceLines = invoiceLines.ToArray();

			return entryLineData;
		}

		ExportInvoiceLine PopulateInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var invoiceLineData = new ExportInvoiceLine();
			invoiceLineData.InvoiceLineNo = invoiceLine.JI_SequenceNumber.ToString(IdNumbersFormatConstants.InvoiceLineNo);
			if (invoiceLine.HasHSRequiringInvQuantityInCustomsUQ)
			{
				invoiceLineData.QtyOrWeight = invoiceLine.JI_CustomsQuantity;
				invoiceLineData.QtyOrWeightUnit = invoiceLine.JI_CustomsUnitQty;
				invoiceLineData.UnitPrice = invoiceLine.CustomsUnitPrice;
			}
			else
			{
				invoiceLineData.QtyOrWeight = invoiceLine.JI_InvoiceQuantity;
				invoiceLineData.QtyOrWeightUnit = invoiceLine.JI_InvoiceUQ;
				invoiceLineData.UnitPrice = invoiceLine.UnitPrice;
			}
			invoiceLineData.Amount = invoiceLine.JI_LinePrice;
			invoiceLineData.Ingredient = invoiceLine.JI_Ingredient;
			invoiceLineData.DetailDescription = invoiceLine.JI_Description;
			invoiceLineData.LotNumber = invoiceLine.JI_LotNumber;

			invoiceLineData.GAApprovalDocuments = PopulateExportGAApprovalDocuments(invoiceLine);
			invoiceLineData.VehicleNumbers = PopulateExportVehicleNumbers(invoiceLine);

			var invoiceCurrency = invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency;
			if (!invoiceCurrency.IsEmpty && !IsExchangeRatePublished(invoiceCurrency))
			{
				var currencyKRW = RefCurrency.LoadFromCurrencyCode(invoiceLine.InvoiceHeader.Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);

				invoiceLineData.Amount = invoiceLine.CurrencyConverter.ConvertExact(new Money(invoiceLineData.Amount, invoiceLine.InvoiceHeader.Invoice_Currency), currencyKRW).Amount;
				if (invoiceLineData.QtyOrWeight != 0)
				{
					invoiceLineData.UnitPrice = invoiceLineData.Amount / invoiceLineData.QtyOrWeight;
				}
			}

			return invoiceLineData;
		}
		ExportGAApprovalDocument[] PopulateExportGAApprovalDocuments(JobComInvoiceLine invoiceLine)
		{
			var exportApprovalDocuments = new List<ExportGAApprovalDocument>();
			var orderedGAApprovalDataCollection = invoiceLine.GAApprovalDataCollection.Cast<GAApproval>().OrderBy(x => x.CSI_LineNo);
			foreach (GAApproval approvalDocument in orderedGAApprovalDataCollection)
			{
				var exportApprovalDocument = new ExportGAApprovalDocument();
				exportApprovalDocument.SequenceNo = approvalDocument.CSI_LineNo.ToString(IdNumbersFormatConstants.ExportGAApprovalSequenceNo);
				exportApprovalDocument.RegulationCategoryCode = approvalDocument.CSI_Procedure;
				if (!approvalDocument.CSI_DateOfIssue.IsEmpty)
				{
					exportApprovalDocument.ApprovalDate = approvalDocument.CSI_DateOfIssue.ToDateTime();
				}
				exportApprovalDocument.RequirementType = approvalDocument.CSI_SubType;
				exportApprovalDocument.RequirementDocumentType = approvalDocument.CSI_Code;
				exportApprovalDocument.RequirementApprovalNumber = approvalDocument.CSI_ReferenceNumber;
				exportApprovalDocument.DocumentName = StringUtils.GetSubstringBytes(approvalDocument.CSI_Description, MaxByteSizes.DocumentName);
				exportApprovalDocument.ReasonForMissingApprovalNumber = StringUtils.GetSubstringBytes(approvalDocument.CSI_AdditionalDescription, MaxByteSizes.ReasonForMissingApprovalNumber);
				exportApprovalDocument.UniqueItemID = approvalDocument.CSI_ReferenceNumber2;
				exportApprovalDocument.NonGAReasonType = approvalDocument.CSI_Status == ZString.Empty ? (string)ZString.Empty : approvalDocument.CSI_Procedure + approvalDocument.CSI_SubType + approvalDocument.CSI_Status;

				exportApprovalDocuments.Add(exportApprovalDocument);
			}
			return exportApprovalDocuments.ToArray();
		}
		ExportVehicleNo[] PopulateExportVehicleNumbers(JobComInvoiceLine invoiceLine)
		{
			var exportVehicleNumbers = new List<ExportVehicleNo>();
			var orderedVehicleNumbers = invoiceLine.VehicleNumbers.Cast<VehicleNumber>().OrderBy(x => x.CY_Order);
			foreach (VehicleNumber vehicleNumber in orderedVehicleNumbers)
			{
				var exportVehicleNumber = new ExportVehicleNo();
				exportVehicleNumber.SequenceNo = vehicleNumber.CY_Order.ToString(IdNumbersFormatConstants.ExportVehicleNo);
				exportVehicleNumber.VIN = vehicleNumber.CY_Data;

				exportVehicleNumbers.Add(exportVehicleNumber);
			}
			return exportVehicleNumbers.ToArray();
		}
	}
}
