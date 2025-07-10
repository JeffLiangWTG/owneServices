using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.NEXDOC.RC4.ReadRex;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ReadRexResponseReaderRC4 : DataObjectReader
	{
		public ReadRexResponseReaderRC4(BusinessObjectFactory factory, LoggingInformation logger)
			: base(new XmlSessionTracker(logger))
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public JobComInvoiceHeader GenerateInvoice(ReadRexResponse readRexResponse, ZString rexNumber)
		{
			var invoice = factory.New<JobComInvoiceHeader>();
			using (invoice.GetValidationSuspender())
			{
				using (new CopySuspender(invoice))
				{
					invoice.JZ_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

					var invoiceNumber = ZString.Empty;
					var exportDetails = readRexResponse.exportDetails;
					if (exportDetails != null)
					{
						invoiceNumber = exportDetails.imaDetailsList?.Items?.Cast<ImaDetailsType>().FirstOrDefault(ima => !string.IsNullOrEmpty(ima.invoiceNumber))?.invoiceNumber;

						invoice.JZ_OH_Supplier = MatchSupplier(exportDetails.ownerExporterId);
						invoice.JZ_OH_Buyer = MatchConsignee(exportDetails.consigneeDetails);

						var currency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ExDocCurrencyCodeConverter.ConvertToIso4217(exportDetails.sew?.currency));
						if (currency != null)
						{
							invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
						}

						invoice.JZ_ExporterReference = ((ZString)exportDetails.exporterReference).Left(JobComInvoiceHeader.Schema.JZ_ExporterReferenceMaxLength);
					}

					invoice.JZ_InvoiceNumber = !invoiceNumber.IsEmpty ? invoiceNumber : rexNumber;
				}

				var quarantineHeader = PopulateQuarantineHeader(invoice, readRexResponse);
				var entryNum = PopulateEntryNumber(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, rexNumber);

				var rexResponseDetails = readRexResponse.rexResponseDetails;
				if (rexResponseDetails?.complianceStatusSpecified ?? false)
				{
					entryNum.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.GetCodeFromEXDOCComplianceStatusCode(rexResponseDetails.complianceStatus.ToString());
				}

				var productLines = readRexResponse.productLines?.productLine;
				if (productLines != null)
				{
					// assign each permit to a line until all permits are assigned.
					var permitEnumerator = readRexResponse.exportDetails?.importPermits?.Items?.Cast<ImportPermitType>().GetEnumerator();
					// lines grouped by certificate 
					var certificateLines = readRexResponse.certificateDetails?.certificates?.Items?.Cast<CertificateLineType>().ToArray();
					// lines grouped by manufacturer
					var manufacturerLines = readRexResponse.manufacturers?.Cast<ManufacturerLineType>().ToArray();

					var invoiceAmount = ZDecimal.Zero;
					foreach (var productLine in productLines)
					{
						var permit = permitEnumerator != null && permitEnumerator.MoveNext() ? permitEnumerator.Current : null;
						var line = PopulateLine(invoice, productLine, permit, certificateLines, manufacturerLines);
						invoiceAmount += line.JI_LinePrice;
					}
					invoice.JZ_InvoiceAmount = invoiceAmount;
				}
			}

			return invoice;
		}

		string ConvertTemperatureUnitToCoreUnit(string unit)
		{
			string result;
			switch (unit)
			{
				case EXDOCTemperatureUnitCodes.Codes.Celsius:
					result = Core.Constants.Temperature.Centigrade;
					break;
				case EXDOCTemperatureUnitCodes.Codes.Fahrenheit:
					result = Core.Constants.Temperature.Fahrenheit;
					break;
				default:
					result = unit;
					break;
			}
			return result;
		}

		internal void PopulateStorageTemperature(QuarantineExDocHeader quarantineHeader, FishExportDetails fishExportDetails, TransportDetailsType transportDetails)
		{
			if (quarantineHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Fish)
			{
				var storeTransportTemperature = transportDetails.storeTransportTemperature;
				var transportStorageMinimumTemperature = fishExportDetails?.transportStorageMinimumTemperature;
				if (storeTransportTemperature != null && transportStorageMinimumTemperature != null && storeTransportTemperature.unit != transportStorageMinimumTemperature.unit)
				{
					var maximum = Core.Constants.Temperature.Convert(storeTransportTemperature.Value, ConvertTemperatureUnitToCoreUnit(storeTransportTemperature.unit), Core.Constants.Temperature.Centigrade);
					var minimum = Core.Constants.Temperature.Convert(transportStorageMinimumTemperature.Value, ConvertTemperatureUnitToCoreUnit(transportStorageMinimumTemperature.unit), Core.Constants.Temperature.Centigrade);
					quarantineHeader.QH_MaximumTemperature = maximum;
					quarantineHeader.QH_MinimumTemperature = minimum;
					quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
				}
				else
				{
					if (storeTransportTemperature != null)
					{
						quarantineHeader.QH_MaximumTemperature = storeTransportTemperature.Value;
						quarantineHeader.QH_TemperatureUM = storeTransportTemperature.unit;
					}
					if (transportStorageMinimumTemperature != null)
					{
						quarantineHeader.QH_MinimumTemperature = transportStorageMinimumTemperature.Value;
						quarantineHeader.QH_TemperatureUM = transportStorageMinimumTemperature.unit;
					}
				}
			}
			else
			{
				quarantineHeader.QH_AbsoluteTemperature = transportDetails.storeTransportTemperature?.Value ?? 0m;
				quarantineHeader.QH_TemperatureUM = transportDetails.storeTransportTemperature?.unit;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		QuarantineExDocHeader PopulateQuarantineHeader(JobComInvoiceHeader invoice, ReadRexResponse readRexResponse)
		{
			var quarantineHeader = invoice.QuarantineExDocHeader;

			using (quarantineHeader.GetValidationSuspender())
			using (new CopySuspender(quarantineHeader))
			{
				quarantineHeader.QH_LastAmendDateTime = readRexResponse.rexResponseDetails is RexResponseDetailsType rexResponseDetails && rexResponseDetails.lastAmendDateTimeSpecified ? rexResponseDetails.lastAmendDateTime : ZDateTimeOffset.Empty;

				var exportDetails = readRexResponse.exportDetails;
				if (exportDetails != null)
				{
					quarantineHeader.QH_ProduceType = EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(exportDetails.commodityType);
					quarantineHeader.QH_ObtainExportCustomsPermit = exportDetails.sew?.customsAgentIndicator?.Value == YesNoType.Y;
					quarantineHeader.QH_RL_NKBorderInspectionPort = exportDetails.borderInspectionPort;
					quarantineHeader.QH_ImportedProductFlag = ConvertYesNoTypeToEXDOCYesNoEmpty(exportDetails.importedProductFlag);
					quarantineHeader.QH_LegallyImportedFlag = ConvertYesNoTypeToEXDOCYesNoEmpty(exportDetails.legallyImported);
					quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia = ConvertYesNoTypeToEXDOCYesNoEmpty(exportDetails.manufacturedTreatedPackagedLabelledInAustralia);
					quarantineHeader.QH_ShipsStores = exportDetails.shipStoresFlag == YesNoType.Y;

					if (exportDetails.productUseIndicatorSpecified)
					{
						quarantineHeader.QH_ProductUseIndicator = exportDetails.productUseIndicator.ToString();
					}

					var exporterDeclaration = exportDetails.exporterDeclaration;
					if (exporterDeclaration != null)
					{
						quarantineHeader.QH_ExporterDeclaration = exporterDeclaration.exporterDeclaration;

						foreach (var exporterDeclarationCode in exporterDeclaration.Items.Cast<string>().Distinct())
						{
							quarantineHeader.SupportingInfos.AddNew().CSI_Description = exporterDeclarationCode;
						}
					}

					var authorisationDetails = exportDetails.authorisationDetails;
					if (authorisationDetails != null)
					{
						quarantineHeader.QH_AuthorisationEstablishment = authorisationDetails.authorisingEstablishmentNumber;
						quarantineHeader.QH_AuthorisationComments = authorisationDetails.comments;

						if (authorisationDetails.authorisationDateSpecified)
						{
							quarantineHeader.QH_AuthorisationDate = new ZDate(authorisationDetails.authorisationDate);
						}
					}

					var transportDetails = exportDetails.transportDetails;
					var fishExportDetails = exportDetails.Item;
					if (transportDetails != null)
					{
						PopulateStorageTemperature(quarantineHeader, fishExportDetails, transportDetails);

						quarantineHeader.QH_StartHoldSeal = transportDetails.vesselHoldSeals?.sealStartNumber;
						quarantineHeader.QH_EndHoldSeal = transportDetails.vesselHoldSeals?.sealEndNumber;
					}

					var catchingZones = fishExportDetails?.catchingZones?.catchingZone;
					if (catchingZones != null && catchingZones.Length > 0)
					{
						foreach (var catchingZone in catchingZones)
						{
							var zone = quarantineHeader.NexDocCatchZones.AddNew();
							zone.CY_Data = catchingZone;
						}
					}
				}

				var certificatePrintControls = readRexResponse.certificateDetails?.certificatePrintControls;
				if (certificatePrintControls != null)
				{
					if (certificatePrintControls.certificatePrintIndicatorSpecified)
					{
						quarantineHeader.QH_CertificatePrintIndicator = certificatePrintControls.certificatePrintIndicator.ToString(); // A, C, M ,N
					}

					if (certificatePrintControls.separateBySpecified)
					{
						quarantineHeader.QH_SplitHealthCertByContainer = certificatePrintControls.separateBy == CertificateSeparatorType.CONTAINER;
						quarantineHeader.QH_SplitHealthCertByPacker = certificatePrintControls.separateBy == CertificateSeparatorType.PACKING_ESTABLISHMENT;
						quarantineHeader.QH_SplitHealthCertByMarks = certificatePrintControls.separateBy == CertificateSeparatorType.SHIPPING_MARK;
					}

					string printRegion;
					if (certificatePrintControls.ItemElementName == ItemChoiceType1.certificatePrintRegion)
					{
						printRegion = certificatePrintControls.Item;
					}
					else
					{
						var certificate = readRexResponse.certificateDetails.certificates?.Items?.Cast<CertificateLineType>().FirstOrDefault(x => x.certificatePrintDetails?.ItemElementName == ItemChoiceType2.certificatePrintRegion);
						printRegion = certificate?.certificatePrintDetails?.Item;
					}
					if (printRegion != null)
					{
						quarantineHeader.QH_CertificateRequiredLocation = printRegion;
					}
				}
			}

			return quarantineHeader;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		JobComInvoiceLine PopulateLine(JobComInvoiceHeader invoice, ProductLineType productLine, ImportPermitType permit, IEnumerable<CertificateLineType> certificateLines, IEnumerable<ManufacturerLineType> manufacturerLines)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineAddInfo = invoiceLine.AddInfo;
			var quarantineLine = invoiceLine.QuarantineExDocLine;

			using (invoiceLine.GetValidationSuspender())
			using (invoiceLineAddInfo.GetValidationSuspender())
			using (quarantineLine.GetValidationSuspender())
			using (new CopySuspender(invoiceLine))
			using (new CopySuspender(quarantineLine))
			{
				var productLineItems = ConvertItemsToDictionary(productLine.ItemsElementName, productLine.Items);

				if (GetItemSafe(productLineItems, ItemsChoiceType2.lineNumber) is string lineNumber)
				{
					invoiceLine.JI_LineNo = ZShort.ParseSafe(lineNumber, 0);

					var certificateLine = certificateLines?.FirstOrDefault(x => (x.Item as CertificateLineTypeLineNumbers)?.lineNumber?.Contains(lineNumber) ?? false);
					if (certificateLine != null)
					{
						quarantineLine.QL_HCFormatAllocated = certificateLine.certificateDetails?.certificateTemplate;
						quarantineLine.QL_HCFormatRequested = certificateLine.certificateDetails?.certificateEndorsement;
					}

					var manufacturerDetails = manufacturerLines?.FirstOrDefault(x => x.lineNumbers?.lineNumber?.Contains(lineNumber) ?? false)?.manufacturerDetails;
					if (manufacturerDetails != null)
					{
						var addr = MatchManufacturer(manufacturerDetails.name, manufacturerDetails.address);
						if (addr != null)
						{
							invoiceLine.JI_OA_ManufacturerAddress = addr.PK;
						}
					}
				}

				if (permit != null)
				{
					//line.JI_TempImportNum
					invoiceLineAddInfo.ZA_TemporaryImportNumbers_Hidden = permit.importPermitNumber;
					//line.JI_TempImportDate
					if (permit.importPermitDateSpecified)
					{
						invoiceLineAddInfo.ZA_TemporaryImportDate_Hidden = permit.importPermitDate;
					}
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.aheccCode) is string aheccCode)
				{
					invoiceLine.JI_Tariff = new AUExportTariffUniversalFormatter().FormatDotted(aheccCode);
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.sew) is ProductLineCustomsDetailsType sew)
				{
					invoiceLine.JI_LinePrice = sew.fobAmountSpecified ? (ZDecimal)sew.fobAmount : ZDecimal.Zero;

					//invoiceLine.JI_AUState
					invoiceLineAddInfo.ZA_AUState_Hidden = sew.productSourceState?.Value ?? ZString.Empty;

					// quarantineLine.QL_AqisCustomsWeight
					invoiceLineAddInfo.ZA_AQISCustomsWt_Hidden = sew.netCustomsWeight?.Value ?? 0m;
					// quarantineLine.QL_AqisCustomsWeightUQ
					invoiceLineAddInfo.ZA_AQISCustomsWtUQ_Hidden = sew.netCustomsWeight?.unit;

					invoiceLine.JI_Weight = quarantineLine.QL_GrossMetricWeight = sew.grossMetricWeight?.Value ?? 0m;
					quarantineLine.QL_GrossMetricWeightUnit = sew.grossMetricWeight?.unit ?? string.Empty;
					invoiceLine.JI_WeightUQ = new CustomsWeightUnitToRFPWeightUnit().GetCodeFromDescription(quarantineLine.QL_GrossMetricWeightUnit);
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.productSourceCountries) is ProductLineTypeProductSourceCountries productSourceCountries)
				{
					invoiceLineAddInfo.ZA_ORG = productSourceCountries.Items?.Cast<string>().FirstOrDefault();
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.durabilityStartDate) is DateTime durabilityStartDate)
				{
					quarantineLine.QL_UseByStart = durabilityStartDate;
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.durabilityEndDate) is DateTime durabilityEndDate)
				{
					quarantineLine.QL_UseByEnd = durabilityEndDate;
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.batchCode) is string batchCode)
				{
					quarantineLine.QL_BatchCode = batchCode;
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.productDetails) is ProductDetails productDetails)
				{
					ProcessProductDetails(quarantineLine, productDetails);
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.treatments) is ProductLineTypeTreatments treatments && treatments.treatmentType != null)
				{
					ProcessTreatments(quarantineLine, treatments);
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.productionProcesses) is ProductLineTypeProductionProcesses processes && processes.Items != null)
				{
					ProcessProductionProcesses(invoice, quarantineLine, processes);
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.containers) is ProductLineTypeContainers containers)
				{
					ProcessContainers(invoice, invoiceLineAddInfo, containers);
				}

				if (GetItemSafe(productLineItems, ItemsChoiceType2.fishProductLineDetails) is FishProductLineDetails fishProductLineDetails)
				{
					ProcessFishProductLineDetails(invoice, quarantineLine, fishProductLineDetails);
				}
			}

			return invoiceLine;
		}

		void ProcessProductDetails(QuarantineExDocLine quarantineLine, ProductDetails productDetails)
		{
			quarantineLine.QL_ProductType = productDetails.productType;
			quarantineLine.QL_PackType = productDetails.packType;
			quarantineLine.QL_PreservationType = productDetails.preservationType;
			quarantineLine.QL_Category = productDetails.category;
			quarantineLine.QL_CutCode = productDetails.cutType;
			quarantineLine.QL_SupplimentaryCode = productDetails.suppCode;
			quarantineLine.QL_NetQuantity = productDetails.netMetricWeight?.Value ?? 0m;
			quarantineLine.QL_NetQuantityUnit = productDetails.netMetricWeight?.unit;
			quarantineLine.QL_ImperialNetWeight = productDetails.netImperialWeight?.Value ?? 0m;
			quarantineLine.QL_ImperialNetWeightUnit = productDetails.netImperialWeight?.unit;

			var outerProductPackaging = productDetails.outerProductPackaging;
			if (outerProductPackaging != null)
			{
				quarantineLine.QL_OuterPackCount = ZInt.ParseSafe(outerProductPackaging.quantity?.Value, 0);
				quarantineLine.QL_OuterPackType = outerProductPackaging.quantity?.packageType;
				quarantineLine.QL_OuterPackWeight = outerProductPackaging.unitAmount?.Value ?? 0m;
				quarantineLine.QL_OuterPackWeightUnit = outerProductPackaging.unitAmount?.unit;
				quarantineLine.QL_OuterPackAccuracy = ConvertPackageMeasureAccuracyTypeToEXDOCPackAccuracyCode(outerProductPackaging.packageMeasureAccuracy);
				quarantineLine.QL_ShippingMarks = outerProductPackaging.shippingMarks;
			}

			var intermediateProductPackaging = productDetails.intermediateProductPackaging;
			if (intermediateProductPackaging != null)
			{
				quarantineLine.QL_IntermediatePackCount = ZInt.ParseSafe(intermediateProductPackaging.quantity?.Value, 0);
				quarantineLine.QL_IntermediatePackType = intermediateProductPackaging.quantity?.packageType;
				quarantineLine.QL_IntermediatePackWeight = intermediateProductPackaging.unitAmount?.Value ?? 0m;
				quarantineLine.QL_IntermediatePackWeightUnit = intermediateProductPackaging.unitAmount?.unit;
				quarantineLine.QL_IntermediatePackAccuracy = ConvertPackageMeasureAccuracyTypeToEXDOCPackAccuracyCode(intermediateProductPackaging.packageMeasureAccuracy);
			}

			var innerProductPackaging = productDetails.innerProductPackaging;
			if (innerProductPackaging != null)
			{
				quarantineLine.QL_InnerPackCount = ZInt.ParseSafe(innerProductPackaging.quantity?.Value, 0);
				quarantineLine.QL_InnerPackType = innerProductPackaging.quantity?.packageType;
				quarantineLine.QL_InnerPackWeight = innerProductPackaging.unitAmount?.Value ?? 0m;
				quarantineLine.QL_InnerPackWeightUnit = innerProductPackaging.unitAmount?.unit;
				quarantineLine.QL_InnerPackAccuracy = ConvertPackageMeasureAccuracyTypeToEXDOCPackAccuracyCode(innerProductPackaging.packageMeasureAccuracy);
			}
		}

		void ProcessTreatments(QuarantineExDocLine quarantineLine, ProductLineTypeTreatments treatments)
		{
			foreach (var treatmentType in treatments.treatmentType)
			{
				var process = quarantineLine.Processes.AddNew();
				using (process.GetValidationSuspender())
				{
					process.EE_StartDate = treatmentType.treatmentStartDate;
					process.EE_EndDate = treatmentType.treatmentEndDateSpecified ? (ZDateTime)treatmentType.treatmentEndDate : ZDateTime.Empty;
					process.EE_TreatmentCode = treatmentType.treatmentCode;
					process.EE_TreatmentInfo = treatmentType.treatmentInformation;
					process.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				}
			}
		}

		void ProcessProductionProcesses(JobComInvoiceHeader invoice, QuarantineExDocLine quarantineLine, ProductLineTypeProductionProcesses processes)
		{
			foreach (var process in processes.Items.Cast<ProductionProcessType>())
			{
				var processBO = quarantineLine.Processes.AddNew();
				using (processBO.GetValidationSuspender())
				{
					processBO.EE_StartDate = process.processingStartDateSpecified ? (ZDateTime)process.processingStartDate : ZDateTime.Empty;
					processBO.EE_EndDate = process.processingEndDateSpecified ? (ZDateTime)process.processingEndDate : ZDateTime.Empty;
					processBO.EE_ProcessingType = process.establishmentIndicator;
					processBO.EE_EstablishmentIndicator = process.establishmentIndicator;
					processBO.EE_AuthorisationEstablishmentID = process.processingEstablishmentNumber;
					processBO.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				}
			}
		}

		void ProcessContainers(JobComInvoiceHeader invoice, AUAddInfo invoiceLineAddInfo, ProductLineTypeContainers containers)
		{
			var containerType = containers.Items?.Cast<ContainerType>().FirstOrDefault();
			var containerNumber = containerType?.containerNumber?.Trim().ToUpper(CultureInfo.InvariantCulture);

			if (!string.IsNullOrEmpty(containerNumber))
			{
				invoiceLineAddInfo.ZA_AQISTempContainerNumber_Hidden = containerNumber;

				var seal = containerType?.containerSeals?.containerSeal?.FirstOrDefault();
				if (seal != null)
				{
					var sealNumberIndex = Array.IndexOf(seal.ItemsElementName, ItemsChoiceType.sealNumber);
					var sealNumber = sealNumberIndex > -1 && seal.Items.Length > sealNumberIndex ? seal.Items[sealNumberIndex]?.Trim().ToUpper(CultureInfo.InvariantCulture) : null;

					if (!string.IsNullOrEmpty(sealNumber))
					{
						invoiceLineAddInfo.ZA_AQISTempContainerSeal_Hidden = sealNumber;
					}
				}

				if (!invoice.InvoiceHeaderRefs.Find(r => r.J2_ReferenceType == Customs.Business.InvoiceHeaderRefsTypeList.Codes.CN && r.J2_ReferenceNumber == containerNumber).Any())
				{
					invoice.InvoiceHeaderRefs.AddNew(Customs.Business.InvoiceHeaderRefsTypeList.Codes.CN, containerNumber);
				}
			}
		}

		internal void ProcessFishProductLineDetails(JobComInvoiceHeader invoice, QuarantineExDocLine quarantineLine, FishProductLineDetails fishProductLineDetails)
		{
			if (fishProductLineDetails.catchStartDateSpecified)
			{
				quarantineLine.QL_CatchStartDate = fishProductLineDetails.catchStartDate;
			}
			if (fishProductLineDetails.catchEndDateSpecified)
			{
				quarantineLine.QL_CatchEndDate = fishProductLineDetails.catchEndDate;
			}
			if (fishProductLineDetails.fishWaterIndicatorSpecified)
			{
				quarantineLine.QL_FishWaterIndicator = fishProductLineDetails.fishWaterIndicator.ToString();
			}

			var harvestAreas = fishProductLineDetails.fishEstablishments?.harvestArea;
			if (harvestAreas != null)
			{
				foreach (var harvestArea in harvestAreas)
				{
					if (harvestArea.depuration == null && harvestArea.Item == null)
					{
						continue;
					}

					var process = quarantineLine.Processes.AddNew();
					using (process.GetValidationSuspender())
					{
						process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;

						var depuration = harvestArea.depuration;
						if (depuration != null)
						{
							process.EE_Depuration = depuration.startDate;
							process.EE_AuthorisationEstablishmentID = depuration.establishmentNumber;
						}

						if (harvestArea.Item is Offshore offshore)
						{
							process.EE_StartDate = offshore.startDate;
							process.EE_EndDate = offshore.endDate;
						}
						else if (harvestArea.Item is HarvestAreaType harvestAreaType)
						{
							process.EE_StartDate = harvestAreaType.harvestAreaDate;
							process.EE_HarvestArea = harvestAreaType.harvestAreaName;
							process.EE_LeaseNumber = harvestAreaType.leaseNumber;
						}

						process.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
					}
				}
			}
		}

		CusEntryNumber PopulateEntryNumber(QuarantineExDocHeader quarantineHeader, ZString entryType, ZString entryNumber)
		{
			var entryNum = factory.New<CusEntryNumber>();
			using (entryNum.GetValidationSuspender())
			{
				entryNum.CE_ParentID = quarantineHeader.PK;
				entryNum.CE_ParentTable = "QuarantineExDocHeader";
				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNum.CE_EntryType = entryType;
				entryNum.CE_EntryNum = entryNumber;
			}
			return entryNum;
		}

		ZGuid MatchSupplier(string exporterId)
		{
			OrgHeader supplier = null;
			if (exporterId != null)
			{
				supplier = OrgHeader.FindByOrgCusCode(factory, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, exporterId, Core.Constants.CountryCodes.Australia);
			}
			return supplier?.PK ?? ZGuid.Empty;
		}

		ZGuid MatchConsignee(ConsigneeType consignee)
		{
			if (consignee != null)
			{
				return MatchOrg(consignee.consigneeName, consignee.consigneeAddress, OrganisationTypes.Consignee);
			}
			return ZGuid.Empty;
		}

		OrgAddress MatchManufacturer(ZString orgName, AddressType address)
		{
			ZString city = address?.city;
			ZString state = address?.state;
			ZString postCode = address?.postalCode;

			bool checkCityFunc(OrgAddress a) => city.IsEmpty || a.City.IsEmpty || a.City == city;
			bool checkStateFunc(OrgAddress a) => state.IsEmpty || a.State.IsEmpty || a.State == state;
			bool checkPostcodeFunc(OrgAddress a) => postCode.IsEmpty || a.Postcode.IsEmpty || a.Postcode == postCode;

			return factory.GetCachedValue(ZString.Format("NEXDOCManufacturer+{0}+{1}+{2}+{3}", orgName, city, state, postCode), () =>
			{
				OrgAddress orgAddress = null;

				var orgPk = MatchOrg(orgName, address, OrganisationTypes.Consignor);
				if (orgPk.IsValid)
				{
					var org = factory.Load<OrgHeader>(orgPk);
					orgAddress = org.Addresses.Cast<OrgAddress>().FirstOrDefault(a => checkCityFunc(a) && checkStateFunc(a) && checkPostcodeFunc(a));
					if (orgAddress == null)
					{
						orgAddress = org.MainAddress;
					}
				}
				return orgAddress;
			});
		}

		ZGuid MatchOrg(ZString orgName, AddressType address, OrganisationTypes orgTypes)
		{
			// this is as in EXDOC but adapted.
			if (!orgName.IsEmpty && address != null)
			{
				var criteria = new Xsd.Organisation();
				criteria.OrganisationDetails.Name = orgName;

				criteria.OrganisationDetails.Location.Country = address.country;
				criteria.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();

				var criteriaAddress = criteria.OrganisationDetails.Addresses.AddNew();
				var streetLines = address.streetAddress?.streetLine;
				if (streetLines != null)
				{
					foreach (var streetLine in streetLines)
					{
						if (criteriaAddress.AddressLine1.IsEmpty)
						{
							criteriaAddress.AddressLine1 = streetLine;
						}
						else if (criteriaAddress.AddressLine2.IsEmpty)
						{
							criteriaAddress.AddressLine2 = streetLine;
						}
					}
				}

				criteriaAddress.CityOrSuburb = address.city;
				criteriaAddress.PostCode = address.postalCode;
				criteriaAddress.StateOrProvince = address.state;

				var matching = new AUOrganisationMatching(new BusinessObjectFactoryProvider(factory), Xsd.XmlInterchange.Empty, new NotificationBuffer());
				var result = matching.Match(criteria, orgTypes);
				if (result.MatchFound || !result.TempOrgCouldNotBeCreated)
				{
					return result.Match.PK;
				}
			}

			return ZGuid.Empty;
		}

		Dictionary<T1, T2> ConvertItemsToDictionary<T1, T2>(T1[] names, T2[] items)
		{
			var result = new Dictionary<T1, T2>(names.Length);

			var nameEnumerator = names.GetEnumerator();
			var itemEnumerator = items.GetEnumerator();
			while (nameEnumerator.MoveNext() && itemEnumerator.MoveNext())
			{
				result.Add((T1)nameEnumerator.Current, (T2)itemEnumerator.Current);
			}

			return result;
		}

		T2 GetItemSafe<T1, T2>(Dictionary<T1, T2> elements, T1 name) where T2 : class
		{
			return elements.ContainsKey(name) ? elements[name] : null;
		}

		ZString ConvertPackageMeasureAccuracyTypeToEXDOCPackAccuracyCode(PackageMeasureAccuracyType accuracyType)
		{
			if (accuracyType == PackageMeasureAccuracyType.A)
			{
				return EXDOCPackAccuracyCodes.Codes.Approximate;
			}
			else if (accuracyType == PackageMeasureAccuracyType.E)
			{
				return EXDOCPackAccuracyCodes.Codes.EqualTo;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString ConvertYesNoTypeToEXDOCYesNoEmpty(YesNoType yesNo)
		{
			return yesNo == YesNoType.Y ? EXDOCYesNoEmpty.Codes.Yes : EXDOCYesNoEmpty.Codes.No;
		}
	}
}
