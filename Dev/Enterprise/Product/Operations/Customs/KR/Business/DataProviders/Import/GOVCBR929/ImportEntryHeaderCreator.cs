using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ImportEntryHeaderCreator
	{
		RefCurrency krwCurrency;
		RefCurrency usdCurrency;
		public ImportEntryHeader Create(CusEntryHeader entry)
		{
			var entryHeaderData = PopulateEntryHeaderLevelData(entry);
			var entryLineDataList = new List<ImportEntryLine>();
			var orderedEntryLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			usdCurrency = entry.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			krwCurrency = entry.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			foreach (CusEntryLine entryLine in orderedEntryLines)
			{
				var entryLineData = PopulateEntryLine(entry, entryLine);
				entryLineDataList.Add(entryLineData);
				entryHeaderData.TotalGrossWeightInKG += entryLine.EffectiveGrossWeight.InKilograms;
				entryHeaderData.TotalValueForVAT += entryLine.CL_ValueForVAT;
				entryHeaderData.TotalVATExemptionValue += entryLine.CL_ValueExemptForVAT;
			}

			entryHeaderData.TotalVAT = entry.TotalVAT;
			entryHeaderData.TotalDutyAmount = entry.FormattedTotalDutyAmount;
			entryHeaderData.TotalLiquorTax = entry.TotalLiquorTax;
			entryHeaderData.TotalTransportationTax = entry.TotalTransportationTax;
			entryHeaderData.TotalSpecialConsumptionTax = entry.TotalSpecialConsumptionTax;
			entryHeaderData.TotalEducationTax = entry.TotalEducationTax;
			entryHeaderData.TotalAgricultureTax = entry.TotalAgricultureTax;
			entryHeaderData.PenaltyForLateDeclaration = entry.PenaltyForLateDeclaration;
			entryHeaderData.PenaltyForMissedDeclaration = entry.PenaltyForMissedDeclaration;

			entryHeaderData.EntryLines = entryLineDataList.ToArray();
			return entryHeaderData;
		}

		ImportEntryHeader PopulateEntryHeaderLevelData(CusEntryHeader entry)
		{
			var entryHeaderData = new ImportEntryHeader();
			#region Declaration
			entryHeaderData.TotalCustomsValueKRW = entry.CustomsValue;
			entryHeaderData.TotalCustomsValueUSD = entry.CustomsValueUSD;
			entryHeaderData.TotalInvoiceAmount = entry.TotalInvoiceAmount;
			entryHeaderData.Freight = entry.Freight;
			entryHeaderData.Insurance = entry.Insurance;
			entryHeaderData.TotalPayableAmount = entry.TotalAmountPayable;
			var declaration = entry.Declaration;
			entryHeaderData.ImportDeclarationNumber = entry.EntryNumber;
			entryHeaderData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			entryHeaderData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			entryHeaderData.PaymentType = declaration.JE_PaymentMethod;
			entryHeaderData.ImporterType = declaration.JE_PaidBy == PaidByCodeList.Codes.CLI ? ImportPersonTypeCodeList.Codes.A : ImportPersonTypeCodeList.Codes.B;
			entryHeaderData.DeclarationPlanCode = declaration.JE_DeclarationPlan;
			entryHeaderData.ImportTypeCode = declaration.JE_MessageSubType;
			entryHeaderData.TradeType = declaration.JE_TradeType;
			entryHeaderData.PackType = declaration.JE_TotalNoOfPacksPackType;
			entryHeaderData.ArrivalPort = declaration.JE_RL_NKPortOfArrival;
			entryHeaderData.TransportMode = Constants.TransportModes.ConvertTransportMode(declaration.JE_TransportMode);
			entryHeaderData.ContainerPackMode = declaration.JE_ContainerPackMode;
			entryHeaderData.DepartureCountryCode = declaration.JE_CustomsLoadPort;
			entryHeaderData.VesselOrFlightNo = declaration.IsSea ? declaration.JE_VesselName : declaration.JE_VoyageFlightNo;

			entryHeaderData.CarrierID = declaration.JE_CarrierCode;
			entryHeaderData.OwnerReferenceNumber = declaration.JE_OwnerRef;
			entryHeaderData.SouthNorthTradeYN = declaration.JE_TradeIndicatorWithKP;
			entryHeaderData.GoldTradeTransactionYN = declaration.JE_GoldTrade;
			entryHeaderData.CustomsBrokerCommentCode1 = declaration.DeclarationRefs.Cast<JobDecRefs>().FirstOrDefault(x => x.J3_ReferenceType == AdditionalInformationStatementCodes_929._257)?.J3_ReferenceNumber ?? ZString.Empty;
			entryHeaderData.CustomsBrokerCommentCode2 = declaration.DeclarationRefs.Cast<JobDecRefs>().FirstOrDefault(x => x.J3_ReferenceType == AdditionalInformationStatementCodes_929._258)?.J3_ReferenceNumber ?? ZString.Empty;
			entryHeaderData.CustomsBrokerCommentCode3 = declaration.DeclarationRefs.Cast<JobDecRefs>().FirstOrDefault(x => x.J3_ReferenceType == AdditionalInformationStatementCodes_929._259)?.J3_ReferenceNumber ?? ZString.Empty;
			entryHeaderData.BondedAreaCode = declaration.JE_LocationOtherInformation;
			entryHeaderData.LocationIDInBondedArea = declaration.JE_LocationIDInBondedArea;
			entryHeaderData.DeclarationProcedureType = declaration.JE_ProcedureType;

			var relatedBill = entry.RelatedBill;
			if (relatedBill != null && relatedBill.IsHouseBill)
			{
				if (relatedBill.ParentBill != null && relatedBill.ParentBill.IsMasterBill)
				{
					entryHeaderData.MasterBillNumber = relatedBill.ParentBill.CU_BillNum;
				}
				entryHeaderData.HouseBillNumber = relatedBill.CU_BillNum;
				entryHeaderData.HouseBillSplitDeclarationIndicator = relatedBill.CU_HBSplitDecInd.ToUpper().Equals(HouseBillSplitDeclarationIndicatorCodeList.Codes.Y);
				entryHeaderData.HouseBillSplitDeclarationReasonCode = relatedBill.CU_HBSplitDecReasonCode;
				entryHeaderData.HouseBillSplitDeclarationReasonDescription = relatedBill.HBSplitDecReasonRemark;
			}
			entryHeaderData.CargoManagementNo = entry.RandomHeader.JZ_ImportCargoManagementNumber;

			if (!declaration.JE_DateOfArrival.IsEmpty)
			{
				entryHeaderData.ArrivalDateAtDischargePort = declaration.JE_DateOfArrival.ToDateTime();
			}

			entryHeaderData.VesselCountryCode = declaration.JE_RN_NKTransportNationality;
			#endregion
			#region OrgHeader
			entryHeaderData.UnipassDeclarantID = declaration.UNIPASSDeclarantID;

			if (entry.Declaration.BrokerAddress != null)
			{
				entryHeaderData.Declarant = new Organisation(RoleType.Declarant)
				{
					CompanyName = entry.Declaration.BrokerAddress.CompanyName,
					RepresentativeName = entry.Declaration.BrokerAddress.Header.GetRepresentativeName(),
					PhoneNumber = entry.Declaration.BrokerAddress.OA_Phone,
					ExtensionNumber = entry.Declaration.BrokerAddress.Header.GetExtensionNumber(),
					Email = entry.Declaration.BrokerAddress.OA_Email
				};
			}
			#endregion

			#region Importer
			var importerAddress = entry.Declaration.ImporterAddress;
			if (importerAddress != null)
			{
				entryHeaderData.Importer = new Organisation(RoleType.Importer)
				{
					CompanyName = importerAddress.CompanyName == ZString.Empty ? importerAddress.Header.GetRepresentativeName() : importerAddress.CompanyName,
					RepresentativeName = importerAddress.Header.GetRepresentativeName(),
				};

				var idNumbers = importerAddress.GetRegistrationIDNumbers(new string[] { IdentificationType.UnipassIDForOrganization });
				entryHeaderData.Importer.SetRegistrationIDNumbers(idNumbers);
				entryHeaderData.AuthorizedImporterRegNo = importerAddress.GetRegistrationIDNumber(IdentificationType.AuthorizedImporterRegNo)?.Number ?? ZString.Empty;
			}
			#endregion

			#region Payer
			if (declaration.PayerAddress != null)
			{
				entryHeaderData.Payer = new Organisation(RoleType.Payer)
				{
					Postcode = declaration.PayerAddress.Postcode,
					RoadNameCode = declaration.PayerAddress.GetRoadNameCode(),
					BuildingNumber = declaration.PayerAddress.GetBuildingNumber(),
					AddressLine1 = declaration.PayerAddress.Address1,
					AddressLine2 = declaration.PayerAddress.Address2,
					CompanyName = declaration.PayerAddress.CompanyName,
					PhoneNumber = declaration.PayerAddress.OA_Phone,
					Email = declaration.PayerAddress.OA_Email,
					RepresentativeName = declaration.DutyPayer.GetRepresentativeName(),
					IsIndividual = declaration.DutyPayer.GetIsIndividual(),
				};
				var idNumbers = declaration.PayerAddress.GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(new string[] { IdentificationType.UnipassIDForOrganization, IdentificationType.OfficeID });
				entryHeaderData.Payer.SetRegistrationIDNumbers(idNumbers);
			}
			else
			{
				entryHeaderData.Payer = new Organisation(RoleType.Payer);
			}
			#endregion

			var randomHeader = entry.RandomHeader;
			if (randomHeader != null)
			{
				#region Supplier
				if (randomHeader.Supplier != null)
				{
					var supplierCodes = new string[] { IdentificationType.ForeignCompanyID };
					var supplierAddress = randomHeader.Supplier.GetCustomsAddressThenMainAddress();
					entryHeaderData.Supplier = new Organisation(RoleType.Supplier)
					{
						CompanyName = supplierAddress.CompanyName,
						CountryCode = supplierAddress.OA_RN_NKCountryCode,
					};

					var idNumbers = randomHeader.Supplier.GetRegistrationIDNumbers(new string[] { IdentificationType.ForeignCompanyID });
					entryHeaderData.Supplier.SetRegistrationIDNumbers(idNumbers);
				}
				#endregion
				#region Distributor
				if (randomHeader.DistributorAddress != null)
				{
					entryHeaderData.OnlineTradeDistributor = new Organisation(RoleType.OnlineTradeDistributor)
					{
						CompanyName = randomHeader.DistributorAddress.CompanyName,
						ECommerceCompanyID = randomHeader.DistributorAddress.GetRegistrationIDNumber(IdentificationType.ECommerceCompanyID)?.Number ?? ZString.Empty
					};
				}
				#endregion
				#region OnlineTradeSeller
				if (randomHeader.SellerAddress != null)
				{
					entryHeaderData.OnlineTradeSeller = new Organisation(RoleType.OnlineTradeSeller)
					{
						CompanyName = randomHeader.SellerAddress.CompanyName,
						ECommerceCompanyID = randomHeader.SellerAddress.GetRegistrationIDNumber(IdentificationType.ECommerceCompanyID)?.Number ?? ZString.Empty
					};
				}
				#endregion
				#region ShipperAddress
				if (randomHeader.ShipperAddress != null)
				{
					entryHeaderData.Shipper = new Organisation(RoleType.Shipper)
					{
						CountryCode = randomHeader.ShipperAddress.OA_RN_NKCountryCode,
						CompanyName = randomHeader.ShipperAddress.CompanyName,
						ForeignCompanyID = randomHeader.ShipperAddress.GetRegistrationIDNumber(IdentificationType.ForeignCompanyID)?.Number ?? ZString.Empty,
					};
				}
				#endregion

				if (randomHeader.SellingAgent != null)
				{
					entryHeaderData.OnlineTradeSellingAgent = new Organisation(RoleType.OnlineTradeSellingAgent)
					{
						CompanyName = randomHeader.SellingAgent.OH_FullName,
						ECommerceCompanyID = randomHeader.SellingAgent.GetRegistrationIDNumber(IdentificationType.ECommerceCompanyID)?.Number ?? ZString.Empty
					};
				}

				entryHeaderData.OnlineTradeType = randomHeader.JZ_OnlineTradeType;
				entryHeaderData.CertificateOfOriginIssued = randomHeader.JZ_COOStatus;
			}

			#region Forwarder
			if (declaration.Forwarder != null)
			{
				entryHeaderData.FreightForwarderCompanyName = declaration.Forwarder.OH_FullName;
				entryHeaderData.FreightForwarderID = declaration.Forwarder.GetRegistrationIDNumber(MasterFiles.Business.OrgCusCode.CodeTypes.CarrierCode)?.Number ?? ZString.Empty;
				entryHeaderData.CourierCompanyID = declaration.Forwarder.GetRegistrationIDNumber(IdentificationType.CourierCompanyID)?.Number ?? ZString.Empty;
			}
			#endregion

			#region entryInstruction
			var entryInstruction = entry.EntryInstruction;
			if (entryInstruction != null)
			{
				entryHeaderData.BondedFactoryUseCode = entryInstruction.CEI_BondedFactoryUseCode;

				if (entryInstruction.CEI_BondedFactoryArrivalDate.IsValid)
				{
					entryHeaderData.BondedFactoryUseDate = entryInstruction.CEI_BondedFactoryArrivalDate.ToDateTime();
				}
				entryHeaderData.TotalPackQty = entryInstruction.CEI_PackQty;
				entryHeaderData.ApplicationForAgreedRate = entryInstruction.CEI_AgreedDutyRatePreferenceCode != ZString.Empty ? ZBool.True : ZBool.False;
			}
			#endregion
			#region invoiceHeader
			var invoiceSeq = 0;
			foreach (var invoiceHeader in entry.InvoiceHeaders())
			{
				if (invoiceSeq == 0)
				{
					entryHeaderData.Incoterm = invoiceHeader.JZ_IncoTerm;
					entryHeaderData.InvoiceAmountCurrency = invoiceHeader.JZ_RX_NKInvoice_Currency;
					entryHeaderData.InvoicePaymentTerm = invoiceHeader.JZ_PaymentTerms;
					entryHeaderData.ExchangeRate = invoiceHeader.JZ_InvoiceCurrExRate;
					entryHeaderData.ValueDeclarationAttached = invoiceHeader.JZ_ValuationDecAttachCode;
					entryHeaderData.BlanketValuationDeclarationNumber = invoiceHeader.JZ_BlanketValuationDeclarationNumber;
					if (invoiceHeader.JZ_Remarks.Length > Constants.CustomsBrokerCommentLength)
					{
						entryHeaderData.CustomsBrokerComment1 = invoiceHeader.JZ_Remarks.SubstringSafe(0, Constants.CustomsBrokerCommentLength);
						entryHeaderData.CustomsBrokerComment2 = invoiceHeader.JZ_Remarks.SubstringSafe(Constants.CustomsBrokerCommentLength);
					}
					else
					{
						entryHeaderData.CustomsBrokerComment1 = invoiceHeader.JZ_Remarks;
					}
				}
				invoiceSeq++;
			}
			#endregion
			#region entryNumber
			if (declaration.UnderbondMovementArrivalDate.IsValid)
			{
				entryHeaderData.UnderbondMovementArrivalDate = declaration.UnderbondMovementArrivalDate.ToDateTime();
			}
			#endregion
			entryHeaderData.AdditionalAmount = entry.AdditionalAmount;
			entryHeaderData.DeductedAmount = entry.DeductedAmount;

			entryHeaderData.Containers = PopulateContainers(entry);
			entryHeaderData.OnlineOrders = PopulateOnlineOrders(entry);

			return entryHeaderData;
		}

		ImportContainer[] PopulateContainers(CusEntryHeader entry)
		{
			var containers = new List<ImportContainer>();
			var orderedContainerPivots = entry.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().OrderBy(x => x.CCE_SequenceNumber);
			foreach (var pivot in orderedContainerPivots)
			{
				var container = pivot.Container;
				var importContainer = new ImportContainer();
				importContainer.SequenceNo = pivot.CCE_SequenceNumber;
				importContainer.ContainerNo = container.CO_ContainerNumber;
				containers.Add(importContainer);
			}
			return containers.ToArray();
		}

		ImportOnlineOrder[] PopulateOnlineOrders(CusEntryHeader entry)
		{
			var onlineOrderList = new List<ImportOnlineOrder>();
			var orderedOnlineOrders = entry.EntryInstruction?.OnlineOrders?.Cast<OnlineOrder>().OrderBy(x => x.CY_Order);
			if (orderedOnlineOrders != null)
			{
				foreach (var order in orderedOnlineOrders)
				{
					var importOnlineOrder = new ImportOnlineOrder();
					importOnlineOrder.SequenceNo = order.CY_Order;
					importOnlineOrder.OnlineOrderNo = order.CY_Data;
					onlineOrderList.Add(importOnlineOrder);
				}
			}
			return onlineOrderList.ToArray();
		}

		ImportEntryLine PopulateEntryLine(CusEntryHeader entry, CusEntryLine entryLine)
		{
			var entryLineData = new ImportEntryLine();
			var invoiceLine = entryLine.RandomLine;
			entryLineData.EntryLineNo = entryLine.CL_LineNumber;
			entryLineData.HSCode = entryLine.CL_AdValoremTariff;
			entryLineData.CustomsValueKRW = entryLine.CL_CustomsValue;
			entryLineData.CustomsValueUSD = entry.CurrencyConverter.ConvertExact(new Money(entryLineData.CustomsValueKRW, krwCurrency), usdCurrency).Amount.Truncate();
			entryLineData.HSDescription = invoiceLine.UniversalTariff?.ZZ1_Description;
			entryLineData.ModelName = invoiceLine.JI_Model;
			entryLineData.BrandName = invoiceLine.JI_BrandName;
			entryLineData.CountryOfOrigin = invoiceLine.JI_CountryOfOrigin;
			entryLineData.MaterialLineNo = invoiceLine.JI_ParentLine;
			entryLineData.DutyRateCode = invoiceLine.JI_PrimaryPreference;
			entryLineData.DutyAmount = entryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);
			entryLineData.ValueExemptForVAT = entryLine.CL_ValueExemptForVAT;

			var dutyRateTypeCode = invoiceLine.UniversalDutyRate?.RateCode;
			if (dutyRateTypeCode.Equals(Constants.ZZ.RateCodes.DutyAdValorem))
			{
				entryLineData.DutyRateTypeCode = Constants.DutyRateTypeCode.DutyAdValorem;
				entryLineData.AdValoremDutyRate = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)?.CF_Rate ?? decimal.Zero;
			}
			else
			{
				entryLineData.DutyRateTypeCode = Constants.DutyRateTypeCode.DutySpecific;
				entryLineData.SpecificDutyRate = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)?.CF_Rate ?? decimal.Zero;
			}

			PopulateKRInvoiceLine(entryLine, entryLineData);
			PopulateCertificateOfCountryOfOrigin(entryLine, entryLineData);
			PopulateVariousTaxItems(entry, entryLine, entryLineData, invoiceLine);

			entryLineData.NetWeightInKG = entryLine.EffectiveNetWeight.InKilograms;
			if (!Core.Constants.Weight.ContainsCode(invoiceLine.JI_CustomsUnitQty))
			{
				entryLineData.Quantity = entryLine.CustomsQuantity;
				entryLineData.QuantityUnit = invoiceLine.JI_CustomsUnitQty;
			}

			var invoiceLines = new List<ImportInvoiceLine>();
			var nonGADetails = new List<ImportNonGADetail>();
			var previousExpDecLines = new List<ImportPreviousExpDecLine>();
			var orderedInvoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_SequenceNumber);
			var immediateDeliveries = new List<ImportImmediateDelivery>();
			foreach (var item in entryLine.ImmediateDeliveries.Cast<ImmediateDelivery>().OrderBy(x => x.CY_Order))
			{
				immediateDeliveries.Add(new ImportImmediateDelivery() { SequenceNo = item.CY_Order, ImmediateDeliveryNo = item.CY_Data });
			}
			foreach (var item in entryLine.NonGADetailCollection.Cast<NonGADetail>().OrderBy(x => x.CSI_LineNo))
			{
				nonGADetails.Add(PopulateImportNonGADetail(item));
			}
			foreach (var item in entryLine.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>().OrderBy(x => x.CSI_LineNo))
			{
				previousExpDecLines.Add(PopulatePreviousExpDecLine(item));
			}
			foreach (var orderedInvoiceLine in orderedInvoiceLines)
			{
				var invoiceLineData = PopulateInvoiceLine(orderedInvoiceLine);
				entryLineData.QuantityToClaimRefund += orderedInvoiceLine.JI_DrawbackQuantity;
				entryLineData.DomesticTaxBaseQtyOrPrice += orderedInvoiceLine.DomesticTaxBaseQtyOrPrice;

				invoiceLines.Add(invoiceLineData);
			}
			entryLineData.InvoiceLines = invoiceLines.ToArray();
			entryLineData.NonGADetails = nonGADetails.ToArray();
			entryLineData.PreviousExpDecLines = previousExpDecLines.ToArray();
			entryLineData.ImmediateDeliveries = immediateDeliveries.ToArray();

			return entryLineData;
		}
		void PopulateVariousTaxItems(CusEntryHeader entry, CusEntryLine entryLine, ImportEntryLine entryLineData, JobComInvoiceLine invoiceLine)
		{
			#region DutyReductionClassification
			entryLineData.DutyReductionClassification = invoiceLine.DutyReductionClassificationCode;
			switch (entryLineData.DutyReductionClassification)
			{
				case ImportDutyReductionClassificationList.Codes.DutyExemption:
				case ImportDutyReductionClassificationList.Codes.DutyReduction:
					entryLineData.DutyReductionOrInstallmentCode = invoiceLine.JI_SecondaryPreference;
					break;
				case ImportDutyReductionClassificationList.Codes.InstallmentPayment:
					entryLineData.DutyReductionOrInstallmentCode = invoiceLine.JI_InstallmentCode;
					break;
				case ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment:
					entryLineData.DutyReductionOrInstallmentCode = invoiceLine.JI_InstallmentCode.IsEmpty ? invoiceLine.JI_SecondaryPreference : invoiceLine.JI_InstallmentCode;
					break;
			}

			if (entryLineData.DutyReductionClassification != ImportDutyReductionClassificationList.Codes.InstallmentPayment && entryLineData.DutyReductionClassification != ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly)
			{
				entryLineData.DutyReductionRate = invoiceLine.DutyReductionRate;
			}
			entryLineData.DutyReductionAmount = entryLine.CL_DutyReductionAmount;
			#endregion

			#region DomesticTaxClassification
			var domesticTariff = invoiceLine.DomesticTax;
			if (domesticTariff != null)
			{
				entryLineData.DomesticTaxClassification = invoiceLine.DomesticTaxType;
				ZDecimal domesticTaxAmount = invoiceLine.DomesticTaxAmount;
				if (invoiceLine.TaxClassification1 == Constants.DomestictaxClassificationCode.LQT)
				{
					entryLineData.ExemptionCodeOfLiquorTax = invoiceLine.JI_DomesticTaxExemptionCode;
					entryLineData.LiquorTax = domesticTaxAmount;
				}
				else if (invoiceLine.TaxClassification1 == Constants.DomestictaxClassificationCode.TRT)
				{
					entryLineData.ExemptionCodeOfTransportationTax = invoiceLine.JI_DomesticTaxExemptionCode;
					entryLineData.TransportationTax = domesticTaxAmount;
				}
				else
				{
					entryLineData.ExemptionCodeOfSpecialConsumptionTax = invoiceLine.JI_DomesticTaxExemptionCode;
					entryLineData.SpecialConsumptionTax = domesticTaxAmount;
				}
				entryLineData.DomesticTaxRate = invoiceLine.DomesticTaxRate;
			}
			#endregion

			#region AgricultureTaxClassification
			entryLineData.AgricultureTaxClassification = invoiceLine.AgricultureTaxClassification;
			#endregion
		}
		void PopulateKRInvoiceLine(CusEntryLine entryLine, ImportEntryLine entryLineData)
		{
			var invoiceLine = entryLine.RandomLine;

			entryLineData.BrandCode = invoiceLine.JI_BrandCode;
			if (!invoiceLine.AdditionalTariffCode.IsEmpty)
			{
				entryLineData.AdditionalTariffCode = entryLine.CL_AdValoremTariff + "-" + invoiceLine.AdditionalTariffCode;
			}
			entryLineData.SpecificUseCodeDutyRatePermitNo = invoiceLine.JI_SpecificUseCodeDutyRatePermitNo;
			entryLineData.CountryOfOriginLabelLocation = invoiceLine.JI_COOLabelLocation;
			entryLineData.CountryOfOriginLabelType = invoiceLine.JI_COOLabelType;
			entryLineData.CertificateOfOriginExemptionReason = invoiceLine.JI_COOExemptionReason;
			entryLineData.ProductOrMaterialCode = invoiceLine.JI_ProductTypeCode;
			entryLineData.MightRequireInspectionIndicator = invoiceLine.JI_MightRequireInspection;
			entryLineData.PostClearanceProcedureAgency1 = invoiceLine.JI_PostClearanceProcedureGA1;
			entryLineData.PostClearanceProcedureAgency2 = invoiceLine.JI_PostClearanceProcedureGA2;
			entryLineData.PostClearanceProcedureAgency3 = invoiceLine.JI_PostClearanceProcedureGA3;
			entryLineData.CourierCargoSelectivityIndicator = invoiceLine.JI_CourierCargoSelectivityIndicator;
			entryLineData.AdditionalDutyRate = invoiceLine.JI_AdditionalDutyRate;
			entryLineData.AdditionalDutyCode = invoiceLine.JI_AdditionalDutyType;
			entryLineData.DomesticTaxCode = invoiceLine.DomesticTaxCode;
			entryLineData.QuantityUQToClaimRefund = invoiceLine.JI_DrawbackUQ;

			if (invoiceLine.JI_ZZF_NKTaxType == VATRateTypeCodeList.Codes.VATLevy)
			{
				entryLineData.ValueForVAT = entryLine.CL_ValueForVAT;
				entryLineData.VATAmount = entryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.VAT);
				entryLineData.VATRateCode = VATRateCode.VATTaxation;
			}
			else if (invoiceLine.JI_ZZF_NKTaxType == VATRateTypeCodeList.Codes.VATExemption)
			{
				entryLineData.VATReductionCode = invoiceLine.JI_VATReductionCode;
				entryLineData.VATRateCode = VATRateCode.VATExemption;
			}
			else if(invoiceLine.JI_ZZF_NKTaxType == VATRateTypeCodeList.Codes.VATReduction)
			{
				entryLineData.VATRateCode = VATRateCode.VATCut;
				entryLineData.ValueForVAT = entryLine.CL_ValueForVAT;
				entryLineData.VATAmount = entryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.VAT);
				entryLineData.VATReductionCode = invoiceLine.JI_VATReductionCode;
			}
			entryLineData.EducationTaxExemptIndicator = invoiceLine.EducationTaxExemptIndicator;
			entryLineData.EducationTaxAmount = entryLine.Fees.GetAmount(ChargeTypeList.Codes.EducationTax);
			entryLineData.AgricultureTax = entryLine.Fees.GetAmount(ChargeTypeList.Codes.AgricultureTax);
		}
		void PopulateCertificateOfCountryOfOrigin(CusEntryLine entryLine, ImportEntryLine entryLineData)
		{
			var certificateOfOrigin = entryLine.RandomLine.CertificateOfOriginData;
			if (certificateOfOrigin != null)
			{
				entryLineData.CountryOfOriginDeterminationRule = certificateOfOrigin.CSI_SubType;

				if (!entryLine.RandomLine.IsFTAPreference)
				{
					entryLineData.CertificateOfOriginCriteriaCode = certificateOfOrigin.CSI_Procedure;
					entryLineData.CertificateOfOriginNo = certificateOfOrigin.CSI_ReferenceNumber;
					entryLineData.CertificateOfOriginSplitIndicator = certificateOfOrigin.CSI_Status;
					entryLineData.CertificateOfOriginIssuingCountry = certificateOfOrigin.CSI_RN_NKCountryCode;
					entryLineData.CertificateOfOriginAgencyName = certificateOfOrigin.CSI_Description;
					if (!certificateOfOrigin.CSI_DateOfIssue.IsEmpty)
					{
						entryLineData.CertificateOfOriginIssueDate = certificateOfOrigin.CSI_DateOfIssue.ToDateTime();
					}
					entryLineData.CertificateOfOriginPersonName = certificateOfOrigin.CSI_ReferenceNumber2;
					entryLineData.CertificateOfOriginAreaName = certificateOfOrigin.CSI_AdditionalDescription;
				}
			}
		}
		ImportInvoiceLine PopulateInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var invoiceLineData = new ImportInvoiceLine();
			invoiceLineData.InvoiceLineNo = invoiceLine.JI_SequenceNumber;
			invoiceLineData.ItemDescription = invoiceLine.JI_Description;
			invoiceLineData.Ingredient = invoiceLine.JI_Ingredient;
			if (invoiceLine.HasHSRequiringInvQuantityInCustomsUQ)
			{
				invoiceLineData.InvoiceUnitOfQuantiy = invoiceLine.JI_CustomsUnitQty;
				invoiceLineData.InvoiceQuantity = invoiceLine.JI_CustomsQuantity;
				invoiceLineData.UnitPrice = invoiceLine.CustomsUnitPrice;
			}
			else
			{
				invoiceLineData.InvoiceUnitOfQuantiy = invoiceLine.JI_InvoiceUQ;
				invoiceLineData.InvoiceQuantity = invoiceLine.JI_InvoiceQuantity;
				invoiceLineData.UnitPrice = invoiceLine.UnitPrice;
			}

			invoiceLineData.Amount = invoiceLine.JI_LinePrice;
			invoiceLineData.PartNumber = invoiceLine.JI_LotNumber;
			invoiceLineData.GAApprovalDocuments = PopulateImportGAApprovalDocuments(invoiceLine);

			return invoiceLineData;
		}
		ImportNonGADetail PopulateImportNonGADetail(NonGADetail nonGADetail)
		{
			var importNonGADetail = new ImportNonGADetail();
			importNonGADetail.SequenceNo = nonGADetail.CSI_LineNo;
			importNonGADetail.ReasonType = nonGADetail.CSI_Code;
			importNonGADetail.RegulationCategoryCode = nonGADetail.CSI_Procedure;
			importNonGADetail.Reason = nonGADetail.CSI_Description;
			importNonGADetail.NonGAReasonType = nonGADetail.NonGAReasonType;

			return importNonGADetail;
		}
		ImportPreviousExpDecLine PopulatePreviousExpDecLine(PreviousExpDecLine previousExpDecLine)
		{
			var importPreviousExpDecLine = new ImportPreviousExpDecLine();
			importPreviousExpDecLine.SequenceNumber = (short)previousExpDecLine.CSI_LineNo;
			importPreviousExpDecLine.DeclarationNumber = previousExpDecLine.CSI_ReferenceNumber;
			importPreviousExpDecLine.EntryLineNo = previousExpDecLine.EntryLineNumber;
			importPreviousExpDecLine.InvoiceLineNo = previousExpDecLine.CSI_ItemNumber;
			importPreviousExpDecLine.UQ = previousExpDecLine.CSI_UnitOfQuantity;
			importPreviousExpDecLine.UsedQty = previousExpDecLine.CSI_Quantity;

			return importPreviousExpDecLine;
		}
		ImportGAApprovalDocument[] PopulateImportGAApprovalDocuments(JobComInvoiceLine invoiceLine)
		{
			var importApprovalDocuments = new List<ImportGAApprovalDocument>();
			var orderedGAApprovalDataCollection = invoiceLine.GAApprovalDataCollection.Cast<GAApproval>().OrderBy(x => x.CSI_LineNo);
			foreach (GAApproval approvalDocument in orderedGAApprovalDataCollection)
			{
				var importApprovalDocument = new ImportGAApprovalDocument();
				importApprovalDocument.SequenceNo = approvalDocument.CSI_LineNo;
				importApprovalDocument.RequirementDocumentType = approvalDocument.CSI_Code;
				importApprovalDocument.RequirementApprovalNumber = approvalDocument.CSI_ReferenceNumber;
				importApprovalDocument.RegulationCategoryCode = approvalDocument.CSI_Procedure;
				importApprovalDocument.DocumentName = approvalDocument.CSI_Description;

				if (!approvalDocument.CSI_DateOfIssue.IsEmpty)
				{
					importApprovalDocument.ApprovalDate = approvalDocument.CSI_DateOfIssue.ToDateTime();
				}
				importApprovalDocument.UseCode = approvalDocument.CSI_SubType;
				importApprovalDocument.UniqueItemID = approvalDocument.CSI_ReferenceNumber2;

				importApprovalDocuments.Add(importApprovalDocument);
			}
			return importApprovalDocuments.ToArray();
		}
	}
}
