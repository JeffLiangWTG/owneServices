using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	class InvoiceNote
	{
		internal NoteType[] BuildNote(EInvoiceHelper helper)
		{
			var uInvoice = helper.UInvoice;

			var notesDict = new Dictionary<ZString, ZStringBuilder>()
			{
				{ InvoiceDescription, uInvoice.Description },
				{ BranchAddress, $"Şube Adresi: {helper.CheckAddressForNull(uInvoice.BranchAddress.Address1, uInvoice.BranchAddress.Address2)}" } // Turkish Language
			};

			if (uInvoice.OrganizationAddress.Contact.HasValue && !uInvoice.OrganizationAddress.Contact.Value.IsEmpty)
			{
				notesDict.Add(OrganizationContact, (NoResString)"İlgili: " + uInvoice.OrganizationAddress.Contact.Value); // Turkish Language
			}

			if (uInvoice.OSCurrency.Code.HasValue)
			{
				if (uInvoice.OSCurrency.Code.Value != Constants.CurrencyCodes.Turkey)
				{
					notesDict.Add(CurrencyRate, uInvoice.OSCurrency.Code + $" {Kuru}: {uInvoice.ExchangeRate.Value.ToString("F5", CultureInfo.CurrentCulture)}"); // Turkish Language
				}

				uInvoice.PostingJournalCollection
					.Where(x => x.ChargeExchangeRate != null && x.ChargeExchangeRate != 1m && x.OSCurrency.Code.HasValue && x.OSCurrency.Code.Value != uInvoice.OSCurrency.Code.Value)
					.GroupBy(x => x.OSCurrency.Code)
					.Select(g => g.First())
					.ForEach(y => notesDict.Add(CurrencyRate, string.Format("{0} {1}: {2}", y.OSCurrency.Code, Kuru, y.ChargeExchangeRate.Value.ToString("F5", CultureInfo.CurrentCulture))));
			}

			notesDict.Add(CurrencyToString, helper.CurrencyToString());
			notesDict.Add(InvoiceMessage, AccountingConfigurationRegistry.Instance.InvoiceMessage.Value);

			SetShipmentInformationToNotes(helper, notesDict);
			GetLocalTotalsByGroupAndSumFromPostingJournal(notesDict, helper);
			notesDict.Add(string.Format("{0}-{1}", LocalLegalMoney, TaxExclusiveAmount), Decimal.Round(helper.ConvertToPositiveValue(uInvoice.LocalExVATAmount.Value), 2).ToString());
			var withholdingTaxTotal = helper.FixDecimalPlacesAndSign(helper.NonCommentInvoiceLines.Where(x => helper.HasWithholdingTax(x)).ToList().Sum(x => x.LocalExtraVATAmount).Value);
			notesDict.Add(string.Format("{0}-{1}", LocalLegalMoney, TaxInclusiveAmount), Decimal.Round(helper.ConvertToPositiveValue(uInvoice.LocalTotal.Value) + withholdingTaxTotal, 2).ToString());
			notesDict.Add(string.Format("{0}-{1}", LocalLegalMoney, PayableAmount), Decimal.Round(helper.ConvertToPositiveValue(uInvoice.LocalTotal.Value), 2).ToString());
			notesDict.Add(ExternalDebtorCode, uInvoice.ExternalDebtorCode);

			var notes = new List<NoteType>();
			notesDict.ForEach(x => AddNote(notes, x.Key, x.Value.ToStringWithDelimiterBetweenAppends(Extensions.Comma)));

			helper.CommentInvoiceLines.ForEach(x => notes.Add(new NoteType() { Value = $"{GetType().Name}|{x.Description}" }));

			return notes.ToArray();
		}

		void SetShipmentInformationToNotes(EInvoiceHelper helper, Dictionary<ZString, ZStringBuilder> notesDict)
		{
			var shipments = helper.UInvoice.ShipmentCollection;
			if (shipments == null || !shipments.Any())
			{
				return;
			}

			var tempStringBuilder = new ZStringBuilder();
			var volumeCountByTypeCode = new Dictionary<ZString, ZDecimal>();
			var volumeCountByTypeDescription = new Dictionary<ZString, ZDecimal>();
			var weightCountByTypeCode = new Dictionary<ZString, ZDecimal>();
			var weightCountByTypeDescription = new Dictionary<ZString, ZDecimal>();
			var chargeableWeightCountByTypeCode = new Dictionary<ZString, ZDecimal>();
			var chargeableWeightCountByTypeDescription = new Dictionary<ZString, ZDecimal>();
			var containers = new List<Container>();
			var packingLines = new List<PackingLine>();
			var organisationAddresses = new List<OrganizationAddress>();

			var shipmentsToProcess = new List<Shipment>();
			foreach (var shipment in shipments)
			{
				foreach (var dataSource in shipment.DataContext.DataSourceCollection)
				{
					var shipmentType = dataSource.Type.Value;
					var shipmentNumber = dataSource.Key.Value;
					if (!shipmentNumber.IsEmpty)
					{
						if (shipment.SubShipmentCollection != null && shipment.SubShipmentCollection.Any())
						{
							helper.CollectShipments(shipment.SubShipmentCollection, shipmentsToProcess, shipmentType, shipmentNumber);
						}
						else
						{
							shipmentsToProcess.Add(shipment);
						}
					}
					SetWaybillNumbers(shipment, notesDict);
					notesDict.Add(ShipmentPlaceOfDelivery, shipment.PlaceOfDelivery?.Code.FallbackToEmptyStringIfNull(), shipment.PlaceOfDelivery?.Name.FallbackToEmptyStringIfNull());
					notesDict.Add(ShipmentPlaceOfReceipt, shipment.PlaceOfReceipt?.Code.FallbackToEmptyStringIfNull(), shipment.PlaceOfReceipt?.Name.FallbackToEmptyStringIfNull());
				}
			}

			var invoicePaymentCodeList = ObjectFactory.Get<ICustomsTRInvoicePaymentCodeList>() as ICodeDescriptionPairList;
			foreach (var shipment in shipmentsToProcess)
			{
				shipment.DataContext.DataSourceCollection.ForEach(x => notesDict.Add(ShipmentNumber, x.Key.Value));
				notesDict.Add(ShipmentGoodsDescription, shipment.GoodsDescription.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentIncoTermCode, shipment.ShipmentIncoTerm?.Code.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentIncoTermDescription, shipment.ShipmentIncoTerm?.Description.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPortOfDischarge, shipment.PortOfDischarge?.Code.FallbackToEmptyStringIfNull(), shipment.PortOfDischarge?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPortOfLoading, shipment.PortOfLoading?.Code.FallbackToEmptyStringIfNull(), shipment.PortOfLoading?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPortOfDestination, shipment.PortOfDestination?.Code.FallbackToEmptyStringIfNull(), shipment.PortOfDestination?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPortOfOrigin, shipment.PortOfOrigin?.Code.FallbackToEmptyStringIfNull(), shipment.PortOfOrigin?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPlaceOfDelivery, shipment.PlaceOfDelivery?.Code.FallbackToEmptyStringIfNull(), shipment.PlaceOfDelivery?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPlaceOfReceipt, shipment.PlaceOfReceipt?.Code.FallbackToEmptyStringIfNull(), shipment.PlaceOfReceipt?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentPortOfFirstArrival, shipment.PortOfFirstArrival?.Code.FallbackToEmptyStringIfNull(), shipment.PortOfFirstArrival?.Name.FallbackToEmptyStringIfNull());
				volumeCountByTypeCode.AddByType(shipment.TotalVolumeUnit?.Code.FallbackToEmptyStringIfNull(), shipment.TotalVolume);
				volumeCountByTypeDescription.AddByType(shipment.TotalVolumeUnit?.Description.FallbackToEmptyStringIfNull(), shipment.TotalVolume);
				weightCountByTypeCode.AddByType(shipment.TotalWeightUnit?.Code.FallbackToEmptyStringIfNull(), shipment.TotalWeight);
				weightCountByTypeDescription.AddByType(shipment.TotalWeightUnit?.Description.FallbackToEmptyStringIfNull(), shipment.TotalWeight);
				notesDict.Add(ShipmentTotalChargeableWeight, shipment.ActualChargeable.FallbackToEmptyStringIfZeroOrNull());
				notesDict.Add(ShipmentVesselName, shipment.VesselName.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentVoyageFlightNo, shipment.VoyageFlightNo.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentTransportModeCode, shipment.TransportMode?.Code.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentTransportModeDescription, shipment.TransportMode?.Description.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentConsignee, shipment.CarrierDocumentsOverride?.AWBHeader?.Consignee?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentShipper, shipment.CarrierDocumentsOverride?.AWBHeader?.Shipper?.Name.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentAWBIssueDate, shipment.CarrierDocumentsOverride?.AWBHeader?.AWBIssueDate.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentAWBIssuePlace, shipment.CarrierDocumentsOverride?.AWBHeader?.AWBIssuePlace.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentAWBNumber, shipment.CarrierDocumentsOverride?.AWBHeader?.AWBNumber.FallbackToEmptyStringIfNull());

				shipment.AdditionalReferenceCollection?.Where(x => x.CountryOfIssue != null && x.CountryOfIssue.Code.Value == Constants.CountryCodes.Turkey)?
					.ForEach(y => notesDict.Add(ShipmentRef + y.Type.Code, y.ReferenceNumber.FallbackToEmptyStringIfNull()));

				var commercialInvoice = shipment.CommercialInfo?.CommercialInvoiceCollection?.FirstOrDefault();
				notesDict.Add(ShipmentCommercialInvSupplier, commercialInvoice?.Supplier?.CompanyName.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentCommercialInvoiceAmount, commercialInvoice?.InvoiceAmount.FallbackToEmptyStringIfZeroOrNull(), commercialInvoice?.InvoiceCurrency?.Code.FallbackToEmptyStringIfNull());
				var invoiceLocalAmount = (ZDecimal?)(commercialInvoice?.InvoiceAmount.Value * commercialInvoice?.AgreedExchangeRate.Value);
				notesDict.Add(ShipmentCommercialInvLocalAmount, invoiceLocalAmount.FallbackToEmptyStringIfZeroOrNull());
				notesDict.Add(ShipmentAgreedExchangeRate, commercialInvoice?.AgreedExchangeRate.FallbackToEmptyStringIfZeroOrNull());
				notesDict.Add(ShipmentCommercialInvoiceNo, commercialInvoice?.InvoiceNumber.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentCommercialInvoiceDate, commercialInvoice?.InvoiceDate.FallbackToEmptyStringIfNull());

				var commercialInvoiceLineCollection = commercialInvoice?.CommercialInvoiceLineCollection;
				notesDict.Add(ShipmentCommercialLineCount, commercialInvoiceLineCollection?.Count.FallbackToEmptyStringIfZeroOrNull());

				var invoiceLine = commercialInvoiceLineCollection?.FirstOrDefault();
				notesDict.Add(ShipmentCommercialInvoiceProcedure, invoiceLine?.Procedure.FallbackToEmptyStringIfNull());

				var valuation = invoiceLine?.AddInfoCollection?.FirstOrDefault(x => x.Key.HasValue && x.Key.Value == CommercialPaymentCode)?.Value ?? string.Empty;
				var valuationDescription = invoicePaymentCodeList.GetDescriptionFromCode(valuation);
				if (!valuationDescription.IsNullOrEmpty())
				{
					notesDict.Add(ShipmentCommercialLineValuation, $"{valuation} - {valuationDescription}");
				}

				notesDict.Add(ShipmentImporterOrganization, shipment.OrganizationAddressCollection?
					.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ImporterDocumentaryAddress))?.CompanyName.FallbackToEmptyStringIfNull());

				if (shipment.CustomsOffice != null && shipment.CustomsOffice.Code.HasValue)
				{
					notesDict.Add(ShipmentCustomsOfficeCode, shipment.CustomsOffice.Code.Value.SubstringSafe(2));
					notesDict.Add(ShipmentCustomsOfficeDescription, helper.GetCustomsOfficeDescription(shipment.CustomsOffice.Code).FallbackToEmptyStringIfNull());
				}

				notesDict.Add(ShipmentEntryReleaseDate, shipment.EntryHeaderCollection?.FirstOrDefault()?.EntryReleaseDate.FallbackToEmptyStringIfNull());
				notesDict.Add(ShipmentMRNNumber, GetEntryNumberCollectionHaveCode(shipment.EntryHeaderCollection, MRN).FallbackToEmptyStringIfNull());

				SetWaybillNumbers(shipment, notesDict);

				containers.AddRangeIfNotNull(shipment.ContainerCollection?.ToList());
				packingLines.AddRangeIfNotNull(shipment.PackingLineCollection?.ToList());
				organisationAddresses.AddRangeIfNotNull(shipment.OrganizationAddressCollection);
			}

			volumeCountByTypeCode.ForEach(x => tempStringBuilder.Append(string.Format("{0} {1}", x.Value, x.Key)));
			notesDict.Add(ShipmentTotalVolumeAndUnitCode, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
			tempStringBuilder.Clear();

			volumeCountByTypeDescription.ForEach(x => tempStringBuilder.Append(string.Format("{0} {1}", x.Value, x.Key)));
			notesDict.Add(ShipmentTotalVolumeAndUnitDescription, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
			tempStringBuilder.Clear();

			weightCountByTypeCode.ForEach(x => tempStringBuilder.Append(string.Format("{0} {1}", x.Value, x.Key)));
			notesDict.Add(ShipmentTotalWeightAndUnitCode, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
			tempStringBuilder.Clear();

			weightCountByTypeDescription.ForEach(x => tempStringBuilder.Append(string.Format("{0} {1}", x.Value, x.Key)));
			notesDict.Add(ShipmentTotalWeightAndUnitDescription, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
			tempStringBuilder.Clear();

			containers.RemoveAll(t => packingLines.FirstOrDefault(x => x.ContainerNumber == t.ContainerNumber) == null);

			CalculateAndAddPackingQuantity(packingLines, notesDict);

			SetContainerValues(containers, notesDict);

			notesDict.Add(ShipmentShipper, organisationAddresses.FirstOrDefault(x => x.AddressType.FallbackToEmptyStringIfNull() == ConsignorDocumentaryAddress)?.CompanyName.FallbackToEmptyStringIfNull());
			notesDict.Add(ShipmentConsignee, organisationAddresses.FirstOrDefault(x => x.AddressType.FallbackToEmptyStringIfNull() == ConsigneeDocumentaryAddress)?.CompanyName.FallbackToEmptyStringIfNull());
		}

		ZString? GetEntryNumberCollectionHaveCode(List<UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader> entryHeaderCollection, string code)
		{
			if (entryHeaderCollection != null)
			{
				foreach (var entryHeader in entryHeaderCollection)
				{
					return entryHeader.EntryNumberCollection?.FirstOrDefault(x => x.Type.Code.Value == code)?.Number;
				}
			}
			return string.Empty;
		}

		void CalculateAndAddPackingQuantity(List<PackingLine> packingLines, Dictionary<ZString, ZStringBuilder> notesDict)
		{
			if (!packingLines.Any())
			{
				return;
			}

			var packsCountByTypeCode = new Dictionary<ZString, ZLong>();
			var packsCountByTypeDescription = new Dictionary<ZString, ZLong>();

			foreach (var packingLine in packingLines)
			{
				packsCountByTypeCode.AddByType(packingLine.PackType?.Code.FallbackToEmptyStringIfNull(), packingLine.PackQty);
				packsCountByTypeDescription.AddByType(packingLine.PackType?.Description.FallbackToEmptyStringIfNull(), packingLine.PackQty);
			}

			var tempStringBuilder = new ZStringBuilder();

			packsCountByTypeCode.ForEach(x => tempStringBuilder.Append(string.Format("{0} {1}", x.Value, x.Key)));
			notesDict.Add(ShipmentTotalNoOfPacksAndUnitCode, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
			tempStringBuilder.Clear();

			packsCountByTypeDescription.ForEach(x => tempStringBuilder.Append(string.Format("{0} {1}", x.Value, x.Key)));
			notesDict.Add(ShipmentTotalNoOfPacksAndUnitDescription, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
			tempStringBuilder.Clear();
		}

		void SetWaybillNumbers(Shipment shipment, Dictionary<ZString, ZStringBuilder> notesDict)
		{
			if (shipment.WayBillType != null)
			{
				switch (shipment.WayBillType.Code.Value)
				{
					case MWB:
						notesDict.Add(ShipmentMasterWaybillNumber, shipment.WayBillNumber.FallbackToEmptyStringIfNull());
						return;
					case HWB:
						notesDict.Add(ShipmentHouseWaybillNumber, shipment.WayBillNumber.FallbackToEmptyStringIfNull());
						return;
				}
			}
			notesDict.Add(ShipmentWaybillNumbers, shipment.WayBillNumber.FallbackToEmptyStringIfNull());
		}

		void SetContainerValues(List<Container> containers, Dictionary<ZString, ZStringBuilder> notesDict)
		{
			var containerCountByType = new Dictionary<ZString, ZInt>();
			var tempStringBuilder = new ZStringBuilder();

			if (!containers.Any())
			{
				return;
			}

			notesDict.Add(ShipmentContainerCount, containers.Count.FallbackToEmptyStringIfZeroOrNull());

			foreach (var container in containers)
			{
				notesDict.Add(ShipmentContainerNumber, GetFormattedContainerNumber(container.ContainerNumber.FallbackToEmptyStringIfNull()));
				if (container.ContainerType != null)
				{
					var countValue = ZInt.Zero;
					containerCountByType.TryGetValue(container.ContainerType.Code.Value, out countValue);
					if (countValue.IsEmpty)
					{
						containerCountByType.Add(container.ContainerType.Code.Value, ZInt.Zero);
					}
					containerCountByType[container.ContainerType.Code.Value]++;

					notesDict.Add(ShipmentContainerTypeCategory, container.ContainerType?.Category?.Code.FallbackToEmptyStringIfNull(), container.ContainerType?.Category?.Description.FallbackToEmptyStringIfNull());
					notesDict.Add(ShipmentContainerFclLclAir, container.FCL_LCL_AIR?.Code.FallbackToEmptyStringIfNull(), container.FCL_LCL_AIR?.Description.FallbackToEmptyStringIfNull());
				}
			}
			containerCountByType.ForEach(c => tempStringBuilder.Append(string.Format("{0}{1}{2}", c.Value, x, c.Key)));
			notesDict.Add(ShipmentContainerByType, tempStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma));
		}

		string GetFormattedContainerNumber(string containerNumber)
		{
			if (!string.IsNullOrEmpty(containerNumber) && containerNumber.Length >= 11)
			{
				return $"{containerNumber.Substring(0, 4)} {containerNumber.Substring(4, 6)}-{containerNumber.Substring(10, 1)}";
			}
			return "";
		}

		void GetLocalTotalsByGroupAndSumFromPostingJournal(Dictionary<ZString, ZStringBuilder> notesDict, EInvoiceHelper helper)
		{
			var key = "";
			var groupedLocalAmountList = helper.NonCommentInvoiceLines
				.Where(x => x.VATTaxID != null)
				.GroupBy(x => new
				{
					whtRate = helper.HasWithholdingTax(x) ? x.VATTaxID.ExtraTaxRate : null,
					rate = x.VATTaxID.TaxRate,
					messageCode = x.GetTaxMessageCodeWithFallback()
				})
				.Select(n => new GroupedLocalAmount()
				{
					TaxRate = n.Key.rate,
					TaxMessage = n.Key.messageCode,
					LocalTaxableAmountSum = n.Sum(a => a.LocalAmount ?? ZDecimal.Zero),
					LocalTaxAmountSum = n.Sum(a => a.LocalGSTVATAmount ?? ZDecimal.Zero),
					LocalExtraRate = n.Key.whtRate.HasValue && n.Key.rate.HasValue && !n.Key.rate.Value.IsEmpty ? new ZDecimal((n.Key.whtRate.Value * 100m) / n.Key.rate.Value) : null,
					LocalVATAmountSum = n.Sum(a => (a.LocalGSTVATAmount ?? ZDecimal.Zero) - (a.LocalExtraVATAmount ?? ZDecimal.Zero)),
					TaxAmountWithheldSum = -n.Sum(a => a.LocalExtraVATAmount ?? ZDecimal.Zero)
				})
				.OrderBy(x => x.TaxRate ?? 100)
				.ToArray();

			#region WithHolding

			foreach (var wht in groupedLocalAmountList.Where(x => x.TaxRate.HasValue && x.LocalExtraRate.HasValue))
			{
				key = string.Format("{0}-{1}-{2}-{3}", LocalWHTTotalTax, formatRate(wht.TaxRate.Value), wht.TaxMessage, formatRate(wht.LocalExtraRate.Value));
				var value = (helper.ConvertToPositiveValue(wht.LocalVATAmountSum).ToString("0.00") + "," + helper.ConvertToPositiveValue(wht.TaxAmountWithheldSum).ToString("0.00"));
				notesDict.Add(key, value);
			}

			#endregion

			#region VAT Normal Tax

			foreach (var wht in groupedLocalAmountList.Where(x => x.TaxRate.HasValue && !x.TaxRate.Value.IsEmpty)
				.GroupBy(x => x.TaxRate))
			{
				key = string.Format("{0}-{1}", LocalTotalTax, wht.Key.ToString());
				var value = helper.ConvertToPositiveValue(wht.Sum(x => x.LocalTaxableAmountSum)).ToString("0.00") + "," + helper.ConvertToPositiveValue(wht.Sum(x => x.LocalVATAmountSum)).ToString("0.00");
				notesDict.Add(key, value);
			}

			#endregion

			#region EXEMPT

			var exemptLocalAmountGroups = groupedLocalAmountList.Where(x => x.TaxRate.HasValue
				&& (!x.TaxMessage.IsEmpty || x.LocalTaxAmountSum == 0)
				&& (!x.LocalExtraRate.HasValue || x.LocalExtraRate.Value.IsEmpty));

			foreach (var taxGroup in exemptLocalAmountGroups)
			{
				var taxMessage = !taxGroup.TaxMessage.IsEmpty ? taxGroup.TaxMessage : (ZString)EInvoiceTaxCategoryConstants.DefaultExemptionReasonCode;
				key = LocalTotalTax + "-" + formatRate(taxGroup.TaxRate.Value) + "-" + taxMessage;
				var value = taxGroup.LocalTaxableAmountSum.ToString("0.00") + "," + taxGroup.LocalTaxAmountSum.ToString("0.00");
				notesDict.Add(key, value);
			}

			#endregion

			// In case in future we get withholding rates with decimal places. Now we show them with no decimal places.
			ZString formatRate(ZDecimal rate) => new ZDecimal(rate.ToZInt()).Equals(rate) ? rate.ToString("0") : rate.ToString("0.0");
		}

		#region Constants

		#region Invoice Details Labels

		const string InvoiceDescription = nameof(InvoiceDescription);
		const string BranchAddress = nameof(BranchAddress);
		const string CurrencyRate = nameof(CurrencyRate);
		const string OrganizationContact = nameof(OrganizationContact);
		const string CurrencyToString = nameof(CurrencyToString);
		const string InvoiceMessage = nameof(InvoiceMessage);
		const string LocalLegalMoney = nameof(LocalLegalMoney);
		const string TaxExclusiveAmount = nameof(TaxExclusiveAmount);
		const string TaxInclusiveAmount = nameof(TaxInclusiveAmount);
		const string PayableAmount = nameof(PayableAmount);
		const string LocalTotalTax = nameof(LocalTotalTax);
		const string LocalWHTTotalTax = nameof(LocalWHTTotalTax);
		const string ExternalDebtorCode = nameof(ExternalDebtorCode);

		#endregion

		#region ShipmentDetailsLabels

		const string ShipmentNumber = nameof(ShipmentNumber);
		const string ShipmentGoodsDescription = nameof(ShipmentGoodsDescription);
		const string ShipmentPlaceOfDelivery = nameof(ShipmentPlaceOfDelivery);
		const string ShipmentPlaceOfReceipt = nameof(ShipmentPlaceOfReceipt);
		const string ShipmentPortOfDischarge = nameof(ShipmentPortOfDischarge);
		const string ShipmentPortOfFirstArrival = nameof(ShipmentPortOfFirstArrival);
		const string ShipmentPortOfLoading = nameof(ShipmentPortOfLoading);
		const string ShipmentPortOfDestination = nameof(ShipmentPortOfDestination);
		const string ShipmentPortOfOrigin = nameof(ShipmentPortOfOrigin);
		const string ShipmentTotalNoOfPacksAndUnitCode = nameof(ShipmentTotalNoOfPacksAndUnitCode);
		const string ShipmentTotalNoOfPacksAndUnitDescription = nameof(ShipmentTotalNoOfPacksAndUnitDescription);
		const string ShipmentTotalVolumeAndUnitCode = nameof(ShipmentTotalVolumeAndUnitCode);
		const string ShipmentTotalVolumeAndUnitDescription = nameof(ShipmentTotalVolumeAndUnitDescription);
		const string ShipmentTotalWeightAndUnitCode = nameof(ShipmentTotalWeightAndUnitCode);
		const string ShipmentTotalWeightAndUnitDescription = nameof(ShipmentTotalWeightAndUnitDescription);
		const string ShipmentTotalChargeableWeight = nameof(ShipmentTotalChargeableWeight);
		const string ShipmentVesselName = nameof(ShipmentVesselName);
		const string ShipmentVoyageFlightNo = nameof(ShipmentVoyageFlightNo);
		const string ShipmentMasterWaybillNumber = nameof(ShipmentMasterWaybillNumber);
		const string ShipmentHouseWaybillNumber = nameof(ShipmentHouseWaybillNumber);
		const string ShipmentWaybillNumbers = nameof(ShipmentWaybillNumbers);
		const string ShipmentConsignee = nameof(ShipmentConsignee);
		const string ShipmentShipper = nameof(ShipmentShipper);
		const string ShipmentAWBIssueDate = nameof(ShipmentAWBIssueDate);
		const string ShipmentAWBIssuePlace = nameof(ShipmentAWBIssuePlace);
		const string ShipmentAWBNumber = nameof(ShipmentAWBNumber);
		const string ShipmentContainerCount = nameof(ShipmentContainerCount);
		const string ShipmentContainerNumber = nameof(ShipmentContainerNumber);
		const string ShipmentContainerByType = nameof(ShipmentContainerByType);
		const string ShipmentContainerTypeCategory = nameof(ShipmentContainerTypeCategory);
		const string ShipmentContainerFclLclAir = nameof(ShipmentContainerFclLclAir);
		const string ShipmentTransportModeCode = nameof(ShipmentTransportModeCode);
		const string ShipmentTransportModeDescription = nameof(ShipmentTransportModeDescription);
		const string ShipmentIncoTermCode = nameof(ShipmentIncoTermCode);
		const string ShipmentIncoTermDescription = nameof(ShipmentIncoTermDescription);
		const string ShipmentRef = nameof(ShipmentRef);
		const string ShipmentCommercialInvSupplier = nameof(ShipmentCommercialInvSupplier);
		const string ShipmentCommercialInvoiceAmount = nameof(ShipmentCommercialInvoiceAmount);
		const string ShipmentCommercialInvLocalAmount = nameof(ShipmentCommercialInvLocalAmount);
		const string ShipmentCommercialInvoiceNo = nameof(ShipmentCommercialInvoiceNo);
		const string ShipmentCommercialInvoiceDate = nameof(ShipmentCommercialInvoiceDate);
		const string ShipmentCommercialInvoiceProcedure = nameof(ShipmentCommercialInvoiceProcedure);
		const string ShipmentCommercialLineCount = nameof(ShipmentCommercialLineCount);
		const string ShipmentCommercialLineValuation = nameof(ShipmentCommercialLineValuation);
		const string ShipmentAgreedExchangeRate = nameof(ShipmentAgreedExchangeRate);
		const string ShipmentImporterOrganization = nameof(ShipmentImporterOrganization);
		const string ShipmentCustomsOfficeCode = nameof(ShipmentCustomsOfficeCode);
		const string ShipmentCustomsOfficeDescription = nameof(ShipmentCustomsOfficeDescription);
		const string ShipmentEntryReleaseDate = nameof(ShipmentEntryReleaseDate);
		const string ShipmentMRNNumber = nameof(ShipmentMRNNumber);

		#endregion

		#region Processing Constants

		const string Kuru = nameof(Kuru);
		const string MWB = nameof(MWB);
		const string HWB = nameof(HWB);
		const string x = nameof(x);
		const string ConsignorDocumentaryAddress = nameof(ConsignorDocumentaryAddress);
		const string ConsigneeDocumentaryAddress = nameof(ConsigneeDocumentaryAddress);
		const string CommercialPaymentCode = nameof(CommercialPaymentCode);  //Customs Declaration Property
		const string MRN = nameof(MRN);  //Customs Declaration Property

		#endregion

		#endregion

		void AddNote(List<NoteType> notes, string label, string value, string delimiter = "|")
		{
			if (!string.IsNullOrEmpty(value))
			{
				notes.Add(new NoteType() { Value = string.Format("{0}{1}{2}", label, delimiter, value) });
			}
		}

		internal class GroupedLocalAmount
		{
			internal ZDecimal? TaxRate { get; set; }
			internal ZString TaxMessage { get; set; }
			internal ZDecimal LocalTaxableAmountSum { get; set; }
			internal ZDecimal LocalTaxAmountSum { get; set; }
			internal ZDecimal? LocalExtraRate { get; set; }
			internal ZDecimal LocalVATAmountSum { get; set; }
			internal ZDecimal TaxAmountWithheldSum { get; set; }
		}
	}
}
