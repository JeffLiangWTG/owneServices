using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageContracts;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class ImportSiscomexProvider : IDeclarationImport
	{
		public ImportSiscomexProvider(ImportSiscomexMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			EntryHeader = Argument.NotNull(sendingObject.Header, "sendingObject.Header");
			Declaration = Argument.NotNull(sendingObject.Header.Declaration, "sendingObject.Header.Declaration");
			EntryInstruction = sendingObject.Header.EntryInstruction;
		}

		readonly ImportSiscomexMessageSendingObject sendingObject;

		CusEntryHeader EntryHeader { get; set; }
		JobDeclaration Declaration { get; set; }
		CusEntryInstruction EntryInstruction { get; set; }

		public string TransmissionReason => sendingObject.MessageType == ImportSiscomexActionCodeList.Codes.ANA ? "1" : "2";

		#region InstructionsDocuments

		public IEnumerable<IDeclarationInstructionsDocument> DeclarationInstructionsDocuments => instructionDocumentList ??
			(instructionDocumentList = Declaration.DispatchInstructionNumbers.Cast<CusEntryNumber>().
				Where(x => !x.CE_EntryType.IsEmpty).Select(x => DeclarationInstructionDocumentProvider.New(x)).ToArray());
		IDeclarationInstructionsDocument[] instructionDocumentList;

		#endregion

		#region AdditionItemList

		public IEnumerable<IDeclarationAdditionItem> DeclarationAdditionItemList => additionItemList ?? (additionItemList = EntryHeader.MergedLines.Select(x => DeclarationAdditionItemProvider.New(x)).ToArray());
		IDeclarationAdditionItem[] additionItemList;

		public string TotalAddition => DeclarationAdditionItemList.Count().ToString();

		#endregion

		#region ProcessRelated

		public IEnumerable<IDeclarationProcessRelated> DeclarationProcessRelatedList => processRelatedList ??
				(processRelatedList = Declaration.ProcessRelatedNumbers.Cast<CusEntryNumber>().
				Where(x => !x.CE_EntryType.IsEmpty).Select(x => DeclarationProcessRelatedProvider.New(x)).ToArray());
		IDeclarationProcessRelated[] processRelatedList;

		#endregion

		#region PackingList

		public IEnumerable<IDeclarationPacking> DeclarationPackingList => packingList ?? (packingList = Declaration.Packages.Cast<BasePackage>().GroupBy(packages => packages.CW_PackType)
										.Select(packages => DeclarationPackingProvider.New(packages)).ToArray());
		IDeclarationPacking[] packingList;

		#endregion

		public string ImporterId => DeclarantTypeCode != DeclarantTypeList.Codes.DiplomaticMission ? Declaration.Importer?.GetCNPJOrCPF() : null;

		public IDeclarationOrganization Importer => fImporter ?? (fImporter = DeclarantTypeCode == DeclarantTypeList.Codes.DiplomaticMission ? DeclarationImportOrganizationProvider.New(Declaration.Importer) : null);
		IDeclarationOrganization fImporter;

		public IDeclarationOrganization Consignee => fConsignee ?? (fConsignee = DeclarationImportOrganizationProvider.New(Declaration.IntermConsignee));
		IDeclarationOrganization fConsignee;

		public IEnumerable<IDeclarationPayment> DeclarationPayment => fPaymentProvider ?? (fPaymentProvider = DeclarationPaymentProvider.New(EntryHeader).ToArray());
		IEnumerable<IDeclarationPayment> fPaymentProvider;

		public IDeclarationBankAccountPayment DeclarationBankAccountPayment => fDeclarationBankAccountPayment ?? (fDeclarationBankAccountPayment = DeclarationBankAccountPaymentProvider.New(Declaration));
		IDeclarationBankAccountPayment fDeclarationBankAccountPayment;

		#region Warehouse

		public IEnumerable<IDeclarationWarehouse> DeclarationWarehouseList => warehouses ?? (warehouses = Declaration.WarehouseAreas.Cast<WarehouseArea>().Select(x => DeclarationWarehouseProvider.New(x)).ToArray());
		IDeclarationWarehouse[] warehouses;

		#endregion

		public IEnumerable<IDeclarationMercosulForeign> DeclarationMercosulForeignList => mercosulForeignList ?? (mercosulForeignList = EntryInstruction?.MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>()
										.Select(mercosulForeign => DeclarationMercosulForeignProvider.New(mercosulForeign)).ToArray());
		IDeclarationMercosulForeign[] mercosulForeignList;

		public string VesselCountry
		{
			get
			{
				string countryCode = null;

				if (Declaration.RequiresTransportDetails)
				{
					if (Declaration.IsSea)
					{
						countryCode = Declaration.VesselCountry;
					}
					else if (Declaration.IsAir)
					{
						countryCode = Declaration.ShippingLine?.CountryCode ?? string.Empty;
					}
				}
				return countryCode == null ? null : BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Declaration.Factory, countryCode).ToString();
			}
		}

		public string MessageSubType => Declaration.JE_MessageSubType;

		public string CargoDocType => Declaration.BillType;

		public string ManifestType => Declaration.RequiresTransportDetails && Declaration.IsCargoArrivalDocumentApplicable ? Declaration.JE_CargoArrivalDocumentType.ToString() : null;

		public string MasterCargoDocNumber
		{
			get
			{
				if (Declaration.RequiresTransportDetails)
				{
					if (Declaration.IsTransportByWater)
					{
						return Declaration.JE_UCR;
					}
					else if (Declaration.IsRail && Declaration.BillType == BillTypeList.Codes.HRWB)
					{
						return Declaration.JE_MasterBill;
					}
					else if (Declaration.IsAir && Declaration.BillType == BillTypeList.Codes.HAWB)
					{
						return Declaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(BrazilAdditionalReferenceNumberTypes.Codes.MBL)?.CE_EntryNum ?? Declaration.JE_MasterBill;
					}
				}

				return null;
			}
		}

		public string CargoDocNumber
		{
			get
			{
				if (Declaration.RequiresTransportDetails)
				{
					if (Declaration.IsTransportByWater)
					{
						return Cemercante;
					}
					else if (Declaration.IsPost || Declaration.IsAir || Declaration.IsRail || Declaration.IsRoad)
					{
						if (Declaration.BillType == BillTypeList.Codes.HAWB || Declaration.BillType == BillTypeList.Codes.HRWB)
						{
							return Declaration.JE_HouseBill;
						}
						else if (Declaration.BillType == BillTypeList.Codes.DSIC || Declaration.BillType == BillTypeList.Codes.TIFDTA || Declaration.IsMail || Declaration.IsRoad)
						{
							return Declaration.JE_UCR;
						}
						else if (Declaration.BillType == BillTypeList.Codes.AWB || Declaration.BillType == BillTypeList.Codes.RWB)
						{
							return Declaration.JE_MasterBill;
						}
					}
				}

				return null;
			}
		}

		public string ManifestNumber => Declaration.RequiresTransportDetails && Declaration.IsCargoArrivalDocumentApplicable ? Declaration.JE_CargoArrivalDocumentNumber.ToString() : null;

		public string CargoArrivalDocUtilization => Declaration.RequiresTransportDetails && Declaration.IsCargoArrivalDocumentApplicable ? Declaration.JE_CargoArrivalDocumentUtilization.ToString() : null;

		public string ModalTransport => Declaration.RequiresTransportDetails ? BRRefCusMapper.MapCW1ModalTransportCodeToCustomsCode(Declaration.Factory, Declaration.BRTransportMode).ToString() : null;

		public DateTime? ArriveDate => Declaration.JE_DateOfArrival.ToNullableDateTime();

		public DateTime? DepartureDate => Declaration.RequiresDepartureDetails ? Declaration.JE_ExportDate.ToNullableDateTime() : null;

		public string PortOfLoading => Declaration.RequiresDepartureDetails ? Declaration.PortOfLoading?.RL_PortName : null;

		public string MultiModal => MessageBuilderHelper.MapBooleanValue(Declaration.JE_IsMultimodal);

		public string CargoProvenance => BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Declaration.Factory, Declaration.JE_GoodsOrigin).ToString();

		public string OperationType
		{
			get
			{
				switch (Declaration.DeclarantType)
				{
					case DeclarantTypeList.Codes.LegalPerson:
					case DeclarantTypeList.Codes.NaturalPerson:
						return Declaration.OperationType;
					case DeclarantTypeList.Codes.DiplomaticMission:
						return "1";
					case DeclarantTypeList.Codes.DoorToDoor:
						return "3";
					default:
						return null;
				}
			}
		}

		public string DispatchModality => Declaration.JE_DispatchModality.ReturnNullIfEmpty();

		IEnumerable<JobComInvoiceLine> invoiceLines => invoiceLinesCached ?? (invoiceLinesCached = EntryHeader.InvoiceLines.Cast<JobComInvoiceLine>().ToArray());

		JobComInvoiceLine[] invoiceLinesCached;

		public decimal FreightCollect => (fFreightCollect ?? (fFreightCollect = invoiceLines.GetTotalChargesAmountOnInvoiceLines(
			c => (c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.OverseasFreightCollect
				|| (c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory && c.J7_PrepaidCollect == Core.Constants.PaymentType.Collect)),
			OverseasFreightCurrency))).Value;
		decimal? fFreightCollect;

		public decimal FreightPrepaid => (fFreightPrepaid ?? (fFreightPrepaid = invoiceLines.GetTotalChargesAmountOnInvoiceLines(
			c => (c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid
				|| (c.J7_ChargeType == ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory && c.J7_PrepaidCollect == Core.Constants.PaymentType.Prepaid)),
			OverseasFreightCurrency))).Value;
		decimal? fFreightPrepaid;

		public decimal FreightNationalTerritory => (fFreightNationalTerritory ?? (fFreightNationalTerritory = invoiceLines.GetTotalChargesAmountOnInvoiceLines(
			ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, invoiceLines.FirstOrDefault()?.LinePriceRefCurrency))).Value;
		decimal? fFreightNationalTerritory;

		public decimal InsuranceInLocalCurrency => (fInsuranceInLocalCurrencyValue ?? (fInsuranceInLocalCurrencyValue = invoiceLines.GetTotalChargesAmountOnInvoiceLines(
			CustomsChargeTypeList.Codes.OverseasInsurance, LocalCurrency))).Value;
		decimal? fInsuranceInLocalCurrencyValue;

		public decimal Insurance => (fOverseas ?? (fOverseas = invoiceLines.GetTotalChargesAmountOnInvoiceLines(
			CustomsChargeTypeList.Codes.OverseasInsurance, OverseasInsuranceCurrency))).Value;
		decimal? fOverseas;

		public string SectorCode => Declaration.JE_SubLocationOfGoods;

		public string DeclarantTypeCode => Declaration.DeclarantType;

		public string TaxPaymentTypeCode => "1";

		public string EntranceLocationCustomsOfficeCode => Declaration.EntranceOfficeCode;

		public string ClearanceLocationCustomsOfficeCode => Declaration.JE_CustomsOffice;

		public string ClearanceLocationCustomsEnclosureCode => Declaration.JE_LocationOfGoods;

		public string EntryReferenceNumber => EntryHeader.CH_BGMReference.KeepAlphanumericCharacters();

		public string AdditionalInformation => EntryInstruction?.AdditionalInformationConcatenated ?? string.Empty;

		internal static readonly string Cemercante = "CEMERCANTE31032008";

		public decimal FOBInLocalCurrency => EntryHeader.FOBInLocalCurrency.Amount;

		public string ShipperName => Declaration.RequiresShippingLine ? Declaration.ShippingLine?.OH_FullName.ToString() : null;

		public string VesselName => Declaration.RequiresDepartureDetails && Declaration.IsTransportByWater ? Declaration.JE_VesselName.ToString() : null;

		public string ForwarderNumber => Declaration.Forwarder?.PrimaryRegistrationNumber.Number;

		public string PaymentBankNumber => Declaration.JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Importer ? Declaration.ImporterAddInfo?.ZO_AccountNumber : Declaration.PaymentBankAccount?.AB_AccountNum;

		public decimal NetWeightInKG => invoiceLines.Sum(line => line.NetWeightInKG);

		public decimal GrossWeightInKG => WeightUnits.ConvertWeightToKilogramsIfRequired(Declaration.JE_TotalWeight, Declaration.JE_TotalWeightUnit);

		public string TruckRef => Declaration.IsRoad ? Declaration.JE_VesselName.ToString() : null;

		public string FundapOperation => "N";

		public string FreightCurrency => BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(Declaration.Factory, OverseasFreightCurrency?.Code);

		public string InsuranceCurrency => BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(Declaration.Factory, OverseasInsuranceCurrency?.Code);

		RefCurrency OverseasFreightCurrency => fOverseasFreightCurrency ?? (fOverseasFreightCurrency =
			Declaration.AllOverseasFreightChargesHaveTheSameCurrency ? invoiceLines.GetFirstChargeCurrency(ImportCustomsChargeTypeList.OverseasFreightChargeTypes) : LocalCurrency);
		RefCurrency fOverseasFreightCurrency;

		RefCurrency OverseasInsuranceCurrency => fOverseasInsuranceCurrency ?? (fOverseasInsuranceCurrency =
			Declaration.AllOverseasInsuranceChargesHaveTheSameCurrency ? invoiceLines.GetFirstChargeCurrency(CustomsChargeTypeList.Codes.OverseasInsurance) : LocalCurrency);
		RefCurrency fOverseasInsuranceCurrency;

		RefCurrency LocalCurrency => fLocalCurrency ?? (fLocalCurrency = Declaration.LocalCurrency);
		RefCurrency fLocalCurrency;
	}
}
