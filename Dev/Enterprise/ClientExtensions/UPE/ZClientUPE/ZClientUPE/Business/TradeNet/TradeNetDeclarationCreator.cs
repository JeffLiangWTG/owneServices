using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Client.UPE.Business
{
	public class TradeNetDeclarationCreator : IUnitConverterDataProvider
	{
		public TradeNetDeclarationCreator(BusinessObjectFactory factory, Level1DataImport level1DataImport, Level1Record billRecord, ZBool isExport, OrgHeader consignor, OrgHeader consignee, ZDecimal freightValue, ZDecimal discount, ZString currency, Dictionary<_500000Line, TariffView> lineTariffs)
		{
			Level1DataImport = level1DataImport;
			BillRecord = billRecord;
			IsExport = isExport;
			Consignee = consignee;
			Consignor = consignor;
			FreightValue = freightValue;
			Discount = discount;
			Currency = currency;
			Factory = factory;
			LineTariffs = lineTariffs;
		}
		readonly Level1DataImport Level1DataImport;
		readonly Level1Record BillRecord;
		readonly ZBool IsExport;
		readonly OrgHeader Consignee;
		readonly OrgHeader Consignor;
		readonly ZDecimal FreightValue;
		readonly ZDecimal Discount;
		readonly ZString Currency;
		readonly BusinessObjectFactory Factory;
		readonly Dictionary<_500000Line, TariffView> LineTariffs;

		public JobDeclaration CreateTradeNet(ReadOnlyBusinessObjectFactory readOnlyFactory)
		{
			readOnlyFactory.SuspendValidation();
			var declaration = readOnlyFactory.New<JobDeclaration>();
			var shipmentWeight = BillRecord._202000.Weight;
			var shipmentWeightUnit = BillRecord._200000.ShipmentWeightUnit;
			shipmentWeight = shipmentWeightUnit == Core.Constants.Weight.Kilograms ? shipmentWeight / 10.0m : Core.Constants.Weight.ConvertSafe(shipmentWeight, shipmentWeightUnit, Core.Constants.Weight.Kilograms);
			PopulateMainDeclaration(declaration);
			PopulatePortCodes(declaration);
			PopulateWeightAndVolumn(declaration, shipmentWeight);
			PopulateInvoiceAndInvoices(declaration, shipmentWeight, shipmentWeightUnit);
			return declaration;
		}

		void PopulateMainDeclaration(JobDeclaration declaration)
		{
			PopulateOrganizations(declaration);

			var transportMode = Level1DataImport.IsRoad ? Level1DataFileImporterForSGAccess.Constants.TransportMode.Road : Level1DataFileImporterForSGAccess.Constants.TransportMode.Air;
			var voyageFlightNo = Level1DataImport.FlightNumber;
			if (IsExport)
			{
				declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
				declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
				declaration.SG_OutwardTransportMode = transportMode;
				declaration.SG_OutwardVoyageFlightNo = voyageFlightNo;
			}
			else
			{
				declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
				declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
				declaration.JE_TransportMode = transportMode;
				declaration.JE_VoyageFlightNo = voyageFlightNo;
			}

			declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			declaration.JE_RS_NKServiceLevel = StandardServiceLevel;
			declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			declaration.JE_MasterBill = Level1DataImport.MasterBill;
			declaration.JE_HouseBill = BillRecord._200000.TrackingNumber;
			declaration.JE_DateAtOrigin = Level1DataImport.DepartureDate;
			declaration.JE_DateAtFinalDestination = Level1DataImport.ArrivalDate;
		}
		const string StandardServiceLevel = "STD";

		void PopulateOrganizations(JobDeclaration declaration)
		{
			if (IsExport)
			{
				declaration.JE_OH_Supplier = Consignee?.PK ?? ZGuid.Empty;
				declaration.JE_OH_Importer = Consignor?.PK ?? ZGuid.Empty;
			}
			else
			{
				declaration.JE_OH_Importer = Consignee?.PK ?? ZGuid.Empty;
				declaration.JE_OH_Supplier = Consignor?.PK ?? ZGuid.Empty;
			}
		}

		void PopulateWeightAndVolumn(JobDeclaration declaration, ZDecimal shipmentWeight)
		{
			declaration.JE_TotalWeight = shipmentWeight;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;

			var volume = ZDecimal.Zero;
			var dimensionalWeight = BillRecord._200000.DimensionalWeight;
			if (dimensionalWeight != 0)
			{
				volume = Core.Constants.Volume.ConvertSafe(dimensionalWeight, Core.Constants.Weight.Kilograms, Core.Constants.Volume.CubicMetres);
				declaration.JE_TotalVolume = volume;
				declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicMetres;
			}
		}

		void PopulatePortCodes(JobDeclaration declaration)
		{
			declaration.JE_RL_NKPortOfLoading = Level1DataImport.PortOfLoading;
			declaration.JE_RL_NKPortOfArrival = Level1DataImport.PortOfDischarge;
			declaration.JE_RL_NKOrigin = GetPortOfOrigin();
			declaration.JE_RL_NKFinalDestination = GetPortOfDestination();
		}

		ZString GetPortOfOrigin()
		{
			var originPort = BillRecord._200000.OriginPort;
			var originUNLoco = GetUNLOCOFromUPSMapping(originPort);
			return originUNLoco?.RL_Code ?? ZString.Empty;
		}

		ZString GetPortOfDestination()
		{
			var result = ZString.Empty;
			if (BillRecord._200000.DestinationCountry != Core.Constants.CountryCodes.Singapore)
			{
				var destinationPort = BillRecord._200000.DestinationPort;
				var destinationUNLoco = GetUNLOCOFromUPSMapping(destinationPort);
				if (destinationUNLoco != null)
				{
					result = destinationUNLoco.RL_Code;
				}
			}
			else
			{
				result = Level1DataImport.PortOfDischarge;
			}

			return result;
		}

		void PopulateInvoiceAndInvoices(JobDeclaration declaration, ZDecimal shipmentWeight, ZString shipmentWeightUnit)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = BillRecord._200000.TrackingNumber;
			invoice.JZ_InvoiceAmount = FreightValue;
			invoice.JZ_RX_NKInvoice_Currency = Currency;
			AddInvoiceCharge(invoice, CustomsChargeTypeList.Codes.OverseasFreight, BillRecord._200000.Freight);
			AddInvoiceCharge(invoice, CustomsChargeTypeList.Codes.OverseasInsurance, BillRecord._200000.Insurance);
			AddInvoiceCharge(invoice, CustomsChargeTypeList.Codes.Discount, Discount);
			AddInvoiceCharge(invoice, CustomsChargeTypeList.Codes.OtherCharges, BillRecord._200000.OtherCharges);

			foreach (_500000Line packRecod in BillRecord._500000Lines)
			{
				PopulateInvoiceLines(invoice, packRecod, shipmentWeight, shipmentWeightUnit);
			}
		}

		void AddInvoiceCharge(JobComInvoiceHeader invoice, string chargeCode, ZDecimal amount)
		{
			if (!amount.IsEmpty)
			{
				invoice.Charges.AddNew(chargeCode, amount, Currency);
			}
		}

		void PopulateInvoiceLines(JobComInvoiceHeader invoice, _500000Line packLine, ZDecimal shipmentWeight, ZString shipmentWeightUnit)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = packLine.PartNumber;
			var description = packLine.Description.Trim();
			LineTariffs.TryGetValue(packLine, out var tariff);
			var packQty = packLine.Quantity;
			var packType = packLine.UnitOfQuantity;
			invoiceLine.JI_Tariff = tariff?.ZZ1_TariffCode ?? ZString.Empty;
			invoiceLine.JI_Description = description;
			invoiceLine.JI_InvoiceQuantity = packQty;
			invoiceLine.JI_InvoiceUQ = packType;
			invoiceLine.JI_LinePrice = GetValueInLocalCurrency(packLine.Price, Currency, packLine.CurrencyCode);
			invoiceLine.JI_CountryOfOrigin = !packLine.CountryOfOrigin.IsEmpty ? packLine.CountryOfOrigin : packLine.OriginCountry;
			var customsQty = packQty;
			var customsUnitOfQty = packType;
			if (tariff != null)
			{
				if (!customsQty.IsEmpty)
				{
					var customsUQ = tariff.ZZ1_ZZ8_UQ1;
					if (customsUQ.IsEmpty || !Factory.GetCachedValue<UnitOfQuantityCodeList>().ContainsCode(customsUQ))
					{
						customsUQ = UnitOfQuantityCodeList.Codes.NMB;
					}

					if (UnitConverter.Convertible(packType, customsUQ))
					{
						customsQty = UnitConverter.Convert(new ZDecimal(packQty), packType, customsUQ);
						customsUnitOfQty = customsUQ;
					}
					else
					{
						var packLineCount = BillRecord._500000Lines.Count;

						// use shipment weight divided over number of pack lines
						if (UnitConverter.Convertible(shipmentWeightUnit, customsUQ) && packLineCount > 0)
						{
							ZDecimal shipmentWgtPerPackLine = shipmentWeight / packLineCount;
							customsQty = UnitConverter.Convert(shipmentWgtPerPackLine, shipmentWeightUnit, customsUQ);
							customsUnitOfQty = customsUQ;
						}
					}
				}
			}

			invoiceLine.JI_CustomsQuantity = customsQty;
			invoiceLine.JI_CustomsUnitQty = customsUnitOfQty;
		}

		ZDecimal GetValueInLocalCurrency(ZDecimal value, ZString localCurrencyCode, ZString fromCurrencyCode)
		{
			ZDecimal result = 0;

			if (localCurrencyCode == fromCurrencyCode)
			{
				result = value;
			}
			else
			{
				var fromCurrency = RefCurrency.LoadFromCurrencyCode(Factory, fromCurrencyCode);
				var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, localCurrencyCode);
				if (fromCurrency != null && localCurrency != null)
				{
					var moneyInLocalCurrency = CurrencyConverter.ConvertRounded(new Money(value, fromCurrency), localCurrency);
					if (moneyInLocalCurrency != null)
					{
						result = moneyInLocalCurrency.Amount.Round(2);
					}
				}
			}

			return result;
		}

		RefUNLOCO GetUNLOCOFromUPSMapping(ZString code)
		{
			return code.IsEmpty ? null : RefUNLOCO.LoadFromLocalMap(Factory, code, Core.Constants.CountryCodes.Singapore, UPEDataLine.Constants.RefLocoSystemUsage);
		}

		CurrencyConverter CurrencyConverter
		{
			get { return fCurrencyConverter ?? (fCurrencyConverter = CurrencyConverter.New(Factory, ZDateTime.Now, ZArchitecture.Core.ExchangeRateType.Customs, 0)); }
		}
		CurrencyConverter fCurrencyConverter;

		UnitConverter UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new UnitConverter(this)); }
		}
		UnitConverter fUnitConverter;

		#region IUnitConverterDataProvider Members

		ZString IUnitConverterDataProvider.CountryCode
		{
			get { return Core.Constants.CountryCodes.Singapore; }
		}

		BusinessObjectFactory IUnitConverterDataProvider.Factory
		{
			get { return Factory; }
		}

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
		{
			return System.Array.Empty<IUnitConverter>();
		}

		MasterFiles.Business.OrgSupplierPart IUnitConverterDataProvider.Product
		{
			get { return null; }
		}

		ZGuid IUnitConverterDataProvider.SupplierFK
		{
			get { return ZGuid.Empty; }
		}

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions
		{
			get { return false; }
		}

		ZString IUnitConverterDataProvider.Type
		{
			get { return RPTypeList.Codes.CommercialInvoice; }
		}

		#endregion
	}
}
