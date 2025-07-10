using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Edifact.D96A.Messages.CUSDEC;
using Enterprise.Edifact.D96A.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	class EDIReleaseMessageBuilder : D96AMessageBuilder<IEDIReleaseOGD, CUSDECMessage, EDIReleaseMessage>
	{
		#region Constructor

		public EDIReleaseMessageBuilder(MessageSubTypes messageSubType, IEDIReleaseOGD ediRelease)
			: base(ediRelease, messageSubType)
		{
			isOGDDataRequired = GetIsOGDDataRequired();
			isAQDataRequired = isOGDDataRequired || GetIsAQDataRequired();
		}

		bool GetIsAQDataRequired()
		{
			return new[]
					{
						new[] { ServiceOptions.Codes.ReplaceRMDwithAQ, AssessmentOptions.Codes.AppraisalQualityData },
						new[] { ServiceOptions.Codes.PARS, AssessmentOptions.Codes.AppraisalQualityData },
					}.Any(pair => pair[0] == data.ServiceOptionID && pair[1] == data.AssessmentOption);
		}

		bool GetIsOGDDataRequired()
		{
			return new[]
					{
						new[] { ServiceOptions.Codes.RMDOGD, AssessmentOptions.Codes.AppraisalQualityData },
						new[] { ServiceOptions.Codes.PARSOGD, AssessmentOptions.Codes.AppraisalQualityData }
					}.Any(pair => pair[0] == data.ServiceOptionID && pair[1] == data.AssessmentOption);
		}

		#endregion

		#region Overrides of EDIFACTMessageBuilder

		protected override void PopulateEdifactMessage()
		{
			#region PopulateUNH

			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateUNH(
				unh,
				EDIMessage.MessageNumberPlaceHolder,
				Enterprise.Edifact.D96A.Elements.MessageTypeList.CustomsDeclarationMessage,
				"D",
				"96A",
				ControllingAgencyList.UnEceTradeWp4UnitedNationsStandardMessagesUnsm);

			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region Transaction Number

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateBGM(bgm, string.Empty, data.TransactionNumber, MessageFunctionCode, null);
			var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
			bgmInterpretation.AddElementInterpretation(() => data.TransactionNumber);
			bgmInterpretation.AddElementInterpretation(() => MessageFunctionCode);

			#endregion

			#region Service Option ID / Assessment Option / Importer Number / Priority Indicator

			var cst = edifactMessage.CST.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateCST(
				cst,
				data.ServiceOptionID, CodeListQualifierList.CustomsDeclarationType,
				data.AssessmentOption, CodeListQualifierList.CustomsProcedure,
				data.ImporterNumber, CodeListQualifierList.BusinessAccountNumber,
				string.Empty, CodeListQualifierList.PartyIdentification,
				data.PriorityIndicator, CodeListQualifierList.CustomsSpecialCodes);

			var cstInterpretation = interpretation.AddNewSegmentInterpretation(cst);
			cstInterpretation.AddElementInterpretationIfNotEmpty(() => data.ServiceOptionID);
			cstInterpretation.AddElementInterpretationIfNotEmpty(() => data.AssessmentOption);
			cstInterpretation.AddElementInterpretationIfNotEmpty(() => data.ImporterNumber);
			cstInterpretation.AddElementInterpretationIfNotEmpty(() => data.PriorityIndicator);

			#endregion

			#region Port Of Clearance / Goods Location Code/Name

			var loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateLOC(
				loc,
				PlaceLocationQualifierList.CustomsOfficeOfClearance,
				data.PortOfClearance,
				null,
				string.Empty,
				data.GoodsLocationCode,
				data.GoodsLocationName);

			var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => data.PortOfClearance.PadLeft(4, '0'));
			locInterpretation.AddElementInterpretationIfNotEmpty(() => data.GoodsLocationCode);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => data.GoodsLocationName);

			#endregion

			#region Date Of Arrival/Departure

			if (!data.DateOfArrival.IsEmpty)
			{
				var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateDTM203(dtm, DateTimePeriodQualifierList.ArrivalDateTimeScheduled, data.DateOfArrival);
				interpretation.AddNewSegmentInterpretation(dtm, () => data.DateOfArrival.ToString("g"));
			}
			if (!data.DateOfDeparture.IsEmpty)
			{
				var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateDTM203(dtm, DateTimePeriodQualifierList.DepartureDateTimeEstimated, data.DateOfDeparture);
				interpretation.AddNewSegmentInterpretation(dtm, () => data.DateOfDeparture.ToString("g"));
			}

			#endregion

			#region (OGD) CFIA Commodities Indicator

			if (isOGDDataRequired && data.OGDCFIA)
			{
				var gis = edifactMessage.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gis.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.RequestForClearance;
				gis.ProcessingIndicator.ProcessTypeIdentification = ProcessTypeIdentificationList.WoodPreparation; // WoodPreparation = 1
				interpretation.AddNewSegmentInterpretation(gis, Res.GetString("94385d7a-d154-4911-98f0-69075b32715c", "CFIA Indicator"), string.Empty);
			}

			#endregion

			#region Gross/Net Weight

			if (data.GrossWeight > 0)
			{
				var mea = edifactMessage.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateMEAWeight(mea, MeasurementDimensionCodedList.TotalGrossWeight, data.GrossWeight, data.GrossWeightUnits);
				var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
				meaInterpretation.AddElementInterpretation(() => data.GrossWeightUnits, mea.ValueRange.MeasureUnitQualifier);
				meaInterpretation.AddElementInterpretation(() => data.GrossWeight, mea.ValueRange.MeasurementValue);
			}

			if (data.NetWeight > 0)
			{
				var mea = edifactMessage.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateMEAWeight(mea, MeasurementDimensionCodedList.TotalNetWeight, data.NetWeight, data.NetWeightUnits);

				var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
				meaInterpretation.AddElementInterpretation(() => data.NetWeightUnits, mea.ValueRange.MeasureUnitQualifier);
				meaInterpretation.AddElementInterpretation(() => data.NetWeight, mea.ValueRange.MeasurementValue);
			}

			#endregion

			#region Container Numbers

			foreach (var containerNumber in data.ContainerNumbers)
			{
				var eqd = edifactMessage.EQD.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateEQDContainer(eqd, containerNumber);
				interpretation.AddNewSegmentInterpretation(eqd, Res.GetString("211c7d13-af6e-49b5-bfce-77c246f86546", "Container Number"), containerNumber);
			}

			#endregion

			#region Cargo Control Numbers

			var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			foreach (var ccn in data.CargoControlNumbers)
			{
				var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateRFF(rff, ReferenceQualifierList.CarriersReferenceNumber, ccn);
				interpretation.AddNewSegmentInterpretation(rff, Res.GetString("069207cb-d91e-494b-bdb8-a87dbe00f5a9", "Cargo Control Number"), ccn);
			}

			#endregion

			#region Number/Type Of Packages

			var numberOfPackagesDescription = interpretation.GetFriendlyPropertyName(() => data.NumberOfPackages);
			var typeOfPackagesDescription = interpretation.GetFriendlyPropertyName(() => data.TypeOfPackages);
			var group2 = group1.Group2.InstantiateAChildAndAddItToChildrenCollection();

			for (var i = 0; i < data.NumberOfPackages.Length && i < 10; i++)
			{
				var pac = group2.PAC.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulatePAC(pac, data.NumberOfPackages[i], data.TypeOfPackages[i]);

				var pacInterpretation = interpretation.AddNewSegmentInterpretation(pac);
				pacInterpretation.AddElementInterpretation(numberOfPackagesDescription, data.NumberOfPackages[i]);
				pacInterpretation.AddElementInterpretation(typeOfPackagesDescription, data.TypeOfPackages[i]);
			}

			#endregion

			#region Importer / Carrier / Broker / (OGD) Delivery Party

			Func<NADSegment> getNadFunc =
				() =>
				{
					var group6 = edifactMessage.Group6.InstantiateAChildAndAddItToChildrenCollection();
					return group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};

			var isCFIA = isOGDDataRequired && data.OGDCFIA;
			PopulateDocAddress(getNadFunc, () => data.Importer, PartyQualifierList.Importer, false, true, restrictStateCodeForCFIA: isCFIA);
			PopulateDocAddress(getNadFunc, () => data.Carrier, PartyQualifierList.Carrier, true, true);
			PopulateDocAddress(getNadFunc, () => data.Broker, PartyQualifierList.DeclarantsAgentRepresentative, true, true);

			#region (OGD) Delivery Party

			if (CACustomsDataRegistry.Instance.AlwaysSendDeliveryAddressOnReleaseMessages.Value || (isOGDDataRequired && data.OGDCFIA))
			{
				PopulateDocAddress(getNadFunc, () => data.DeliveryAddress, PartyQualifierList.DeliveryParty, false, false);

				if (data.DeliveryAddress != null && !data.DeliveryAddress.E2_CompanyName.IsEmpty)
				{
					var deliveryPartyGroup = edifactMessage.Group6.Cast<SegmentGroup6>().Last();
					const int phoneNumberLength = 10;
					const string phoneNumberChars = "1234567890";
					if (!data.DeliveryPhone.IsEmpty)
					{
						var com = deliveryPartyGroup.COM.InstantiateAChildAndAddItToChildrenCollection();
						D96AMessageUtilities.PopulateCOM(com, CommunicationChannelQualifierList.Telephone, data.DeliveryPhone);
						interpretation.AddNewSegmentInterpretation(com, () => data.DeliveryPhone.KeepChars(phoneNumberChars).Right(phoneNumberLength));
					}

					if (!data.DeliveryFax.IsEmpty)
					{
						var com = deliveryPartyGroup.COM.InstantiateAChildAndAddItToChildrenCollection();
						D96AMessageUtilities.PopulateCOM(com, CommunicationChannelQualifierList.Telefax, data.DeliveryFax);
						interpretation.AddNewSegmentInterpretation(com, () => data.DeliveryFax.KeepChars(phoneNumberChars).Right(phoneNumberLength));
					}
				}
			}

			#endregion

			#endregion

			#region Delivery Instructions

			if (!data.DeliveryInstructions.IsEmpty)
			{
				var group7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();
				var tod = group7.TOD.InstantiateAChildAndAddItToChildrenCollection();
				PopulateText(tod, () => data.DeliveryInstructions);
			}

			#endregion

			#region Total Value For Duty

			var group8 = edifactMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
			var moa = group8.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, data.TotalValueForDuty, string.Empty, 0);
			interpretation.AddNewSegmentInterpretation(moa, () => data.TotalValueForDuty.ToString(0));

			#endregion

			#region Header/Detail Section Separation

			var uns = edifactMessage.UNS1.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateUNS(uns, SectionIdentificationList.HeaderDetailSectionSeparation);
			interpretation.AddUNS1Interpretation(uns);

			#endregion

			#region Invoices

			foreach (var invoice in data.Invoices)
			{
				PopulateInvoice(invoice);
			}

			#endregion

			#region Detail/Summary Section Separation

			uns = edifactMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateUNS(uns, SectionIdentificationList.DetailSummarySectionSeparation);
			interpretation.AddUNS2Interpretation(uns);

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unh.MessageReferenceNumber);

			#endregion
		}

		#region Populate Invoice

		void PopulateInvoice(IEDIInvoiceOGD invoice)
		{
			#region Invoice Number

			var group10 = edifactMessage.Group10.InstantiateAChildAndAddItToChildrenCollection();
			var dms = group10.DMS.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateDMS(dms, invoice.InvoiceNumber);
			interpretation.AddNewSegmentInterpretation(dms, () => invoice.InvoiceNumber);

			#endregion

			#region (AQ) Invoice Date

			if (isAQDataRequired)
			{
				var dtm = group10.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateDTM102(dtm, DateTimePeriodQualifierList.InvoiceDateTime, invoice.InvoiceDate);
				interpretation.AddNewSegmentInterpretation(dtm, () => invoice.InvoiceDate);
			}

			#endregion

			#region Invoice Amount / Invoice Currency

			var invoiceAmount = invoice.InvoiceAmount;
			if (isAQDataRequired || invoiceAmount > 0)
			{
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, () => invoiceAmount, () => invoice.InvoiceCurrency, 2);
			}

			#endregion

			#region (AQ) Invoice Amounts / Other Reference

			if (isAQDataRequired)
			{
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.TransportChargesCustoms, () => invoice.IncludedOFTAndONS);
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.OtherValuationChargesCustoms, () => invoice.IncludedConstruction);
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.PackingCostCustoms, () => invoice.IncludedPacking);
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.TransportChargesIncurredOutsideCustomsTerritory, () => invoice.ExcludedOFTAndONS);
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.AgentCommissionAmount, () => invoice.ExcludedCommission);
				PopulateInvoiceAmount(group10, MonetaryAmountTypeQualifierList.PackingCost, () => invoice.ExcludedPacking);
			}

			if (isAQDataRequired)
			{
				if (!invoice.OtherReference.IsEmpty)
				{
					var group13 = group10.Group13.InstantiateAChildAndAddItToChildrenCollection();
					var tod = group13.TOD.InstantiateAChildAndAddItToChildrenCollection();
					PopulateText(tod, () => invoice.OtherReference);
				}
			}

			#endregion

			#region Vendor / Purchaser / Consignee / (AQ) Shipper / Exporter / (OGD) Manufacturer

			Func<NADSegment> getNadFunc =
				() =>
				{
					var sg14 = group10.Group14.InstantiateAChildAndAddItToChildrenCollection();
					return sg14.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};

			var isCFIA = isOGDDataRequired && data.OGDCFIA;
			PopulateDocAddress(getNadFunc, () => invoice.Vendor, PartyQualifierList.Vendor, false, true, restrictStateCodeForCFIA: isCFIA);
			PopulateDocAddress(getNadFunc, () => invoice.Purchaser, PartyQualifierList.Buyer, false, true, restrictStateCodeForCFIA: isCFIA);
			PopulateDocAddress(getNadFunc, () => invoice.Consignee, PartyQualifierList.UltimateConsignee, false, true, restrictStateCodeForCFIA: isCFIA);

			if (isAQDataRequired)
			{
				PopulateDocAddress(getNadFunc, () => invoice.Shipper, PartyQualifierList.OriginalShipper, false, true, restrictStateCodeForCFIA: isCFIA);
			}

			PopulateDocAddress(getNadFunc, () => invoice.Exporter, PartyQualifierList.Exporter, false, true, restrictStateCodeForCFIA: isCFIA);

			if (isOGDDataRequired && data.OGDTC)
			{
				PopulateDocAddress(getNadFunc, () => invoice.Manufacturer, PartyQualifierList.ManufacturerOfGoods, false, true, restrictStateCodeForCFIA: isCFIA);
			}

			#endregion

			#region (AQ) - Department Ruling / Last Port Name / Last Port Date

			var group14 = group10.Group14.InstantiateAChildAndAddItToChildrenCollection();
			var group15 = group14.Group15.InstantiateAChildAndAddItToChildrenCollection();
			var doc = group15.DOC.InstantiateAChildAndAddItToChildrenCollection();

			if (isAQDataRequired)
			{
				D96AMessageUtilities.PopulateDOC(doc, invoice.DepartmentRuling, invoice.LastPortName);
				var docInterpretation = interpretation.AddNewSegmentInterpretation(doc);
				docInterpretation.AddElementInterpretationIfNotEmpty(() => invoice.DepartmentRuling);
				docInterpretation.AddElementInterpretationIfNotEmpty(() => invoice.LastPortName);

				if (invoice.LastPortDate.IsValid)
				{
					var dtm = group15.DTM.InstantiateAChildAndAddItToChildrenCollection();
					D96AMessageUtilities.PopulateDTM102(dtm, DateTimePeriodQualifierList.DepartureDateTimeFromLastPortOfCall, invoice.LastPortDate);
					interpretation.AddNewSegmentInterpretation(dtm, () => invoice.LastPortDate);
				}
			}
			else
			{
				D96AMessageUtilities.PopulateDOC(doc, string.Empty, string.Empty);
				interpretation.AddMandatoryTriggerSegmentInterpretation(doc);
			}

			#endregion

			#region Common Country/Region Of Origin / Common Country/Region Of Export / (AQ) Transhipment Country/Region

			var loc = group15.LOC.InstantiateAChildAndAddItToChildrenCollection();
			var transhipmentCountry = isAQDataRequired ? invoice.TranshipmentCountry : ZString.Empty;
			D96AMessageUtilities.PopulateLOC(loc, invoice.CommonCountryOfOrigin, invoice.CommonCountryOfExport, transhipmentCountry);

			var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
			locInterpretation.AddElementInterpretation(() => invoice.CommonCountryOfOrigin);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => invoice.CommonCountryOfExport);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => invoice.TranshipmentCountry, transhipmentCountry);

			#endregion

			#region (AQ) - Conditions Of Sale / Terms Of Payment / Royalty Payments & Goods Services Indicators

			if (isAQDataRequired)
			{
				#region Conditions Of Sale / Terms Of Payment

				if (!invoice.ConditionsOfSale.IsEmpty || !invoice.TermsOfPayment.IsEmpty)
				{
					var group18 = group10.Group18.InstantiateAChildAndAddItToChildrenCollection();
					var pat = group18.PAT.InstantiateAChildAndAddItToChildrenCollection();
					pat.PaymentTermsTypeQualifier = PaymentTermsTypeQualifierList.Basic;
					pat.PaymentTerms.TermsOfPaymentIdentification = TermsOfPaymentIdentificationList.NoDrafts;
					pat.PaymentTerms.TermsOfPayment1 = invoice.ConditionsOfSale;
					pat.PaymentTerms.TermsOfPayment2 = invoice.TermsOfPayment;

					var patInterpretation = interpretation.AddNewSegmentInterpretation(pat);
					patInterpretation.AddElementInterpretationIfNotEmpty(() => invoice.ConditionsOfSale);
					patInterpretation.AddElementInterpretationIfNotEmpty(() => invoice.TermsOfPayment);
				}

				#endregion

				#region Royalty Payments & Goods Services Indicators

				if (invoice.RoyaltyInd || invoice.ServicesInd)
				{
					var group19 = group10.Group19.InstantiateAChildAndAddItToChildrenCollection();
					var alc = group19.ALC.InstantiateAChildAndAddItToChildrenCollection();
					var indicator = !invoice.RoyaltyInd ? RoyaltyServicesIndicators.Codes.GoodsServices
										: !invoice.ServicesInd ? RoyaltyServicesIndicators.Codes.RoyaltyPayments
											: RoyaltyServicesIndicators.Codes.Both;
					alc.AllowanceOrChargeQualifier = AllowanceOrChargeQualifierList.AllowanceLineItems;
					alc.AllowanceChargeInformation.AllowanceOrChargeNumber = indicator;

					interpretation.AddNewSegmentInterpretation(alc, new RoyaltyServicesIndicators().GetDescriptionFromCode(indicator), indicator);
				}

				#endregion
			}

			#endregion

			#region Invoice Lines

			foreach (var invoiceLine in invoice.InvoiceLines)
			{
				PopulateInvoiceLine(invoiceLine, invoice, group10);
			}

			#endregion
		}

		void PopulateInvoiceAmount(SegmentGroup10 group10, MonetaryAmountTypeQualifierList qualifier, Expression<Func<ZDecimal>> amount)
		{
			if (!amount.Compile()().IsEmpty)
			{
				PopulateInvoiceAmount(group10, qualifier, amount, () => ZString.Empty, ZInt.Zero);
			}
		}

		void PopulateInvoiceAmount(SegmentGroup10 group10, MonetaryAmountTypeQualifierList qualifier, Expression<Func<ZDecimal>> amount, Expression<Func<ZString>> currencyCode, ZInt decimalsToShow)
		{
			var group11 = group10.Group11.InstantiateAChildAndAddItToChildrenCollection();
			var moa = group11.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateMOA(moa, qualifier, amount.Compile()(), currencyCode.Compile()(), decimalsToShow);

			var moaInterpretation = interpretation.AddNewSegmentInterpretation(moa);
			moaInterpretation.AddElementInterpretation(amount, moa.MonetaryAmount.MonetaryAmount);
			moaInterpretation.AddElementInterpretationIfNotEmpty(currencyCode);
		}

		void PopulateText(TODSegment tod, Expression<Func<ZString>> text)
		{
			var value = text.Compile()();
			var splitter = new TextSplitter(TextLength) { Text = value };
			D96AMessageUtilities.PopulateTOD(tod, null, splitter[0].Trim(), splitter[1].Trim());
			interpretation.AddNewSegmentInterpretation(tod, text, value.Left(TextLength * 2));
		}

		const int TextLength = 70;

		#endregion

		#region Populate Invoice Line

		void PopulateInvoiceLine(IEDIInvoiceLineOGD invoiceLine, IEDIInvoiceOGD invoice, SegmentGroup10 group10)
		{
			#region (AQ) Page Number / Tariff Number / (AQ) Line Number

			var group21 = group10.Group21.InstantiateAChildAndAddItToChildrenCollection();
			var lin = group21.LIN.InstantiateAChildAndAddItToChildrenCollection();

			if (isAQDataRequired)
			{
				D96AMessageUtilities.PopulateLIN(lin, invoiceLine.PageNumber, invoiceLine.TariffNumber, invoiceLine.LineNumber);

				var linInterpretation = interpretation.AddNewSegmentInterpretation(lin);
				linInterpretation.AddElementInterpretation(() => invoiceLine.PageNumber);
				linInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.TariffNumber);
				linInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.LineNumber);
			}
			else
			{
				D96AMessageUtilities.PopulateLIN(lin, 1, invoiceLine.TariffNumber, ZInt.Zero);

				if (invoiceLine.TariffNumber.IsEmpty)
				{
					interpretation.AddMandatoryTriggerSegmentInterpretation(lin);
				}
				else
				{
					interpretation.AddNewSegmentInterpretation(lin, () => invoiceLine.TariffNumber);
				}
			}

			#endregion

			#region (OGD) - Make / Model / ModelNumber / BrandName / VehicleClass

			if (isOGDDataRequired && (data.OGDIC || data.OGDNR || data.OGDTC))
			{
				var make = invoiceLine.Make;
				var model = data.OGDIC || data.OGDNR ? invoiceLine.Model : ZString.Empty;
				var modelNumber = data.OGDIC || data.OGDNR ? invoiceLine.ModelNumber : ZString.Empty;
				var brandName = data.OGDIC || data.OGDNR || data.OGDTC ? invoiceLine.BrandName : ZString.Empty;
				var vehicleClass = invoiceLine.VehicleClass;

				if (!make.IsEmpty || !model.IsEmpty || !modelNumber.IsEmpty || !brandName.IsEmpty || !vehicleClass.IsEmpty)
				{
					var pia = group21.PIA.InstantiateAChildAndAddItToChildrenCollection();
					pia.ProductIdFunctionQualifier = ProductIdFunctionQualifierList.AdditionalIdentification;
					var piaInterpretation = interpretation.AddNewSegmentInterpretation(pia);

					if (!make.IsEmpty)
					{
						pia.ItemNumberIdentification1.ItemNumber = make;
						pia.ItemNumberIdentification1.ItemNumberTypeCoded = ItemNumberTypeCodedList.VendorItemNumber;
						piaInterpretation.AddElementInterpretation(() => invoiceLine.Make);
					}

					if (!model.IsEmpty)
					{
						pia.ItemNumberIdentification2.ItemNumber = model;
						pia.ItemNumberIdentification2.ItemNumberTypeCoded = ItemNumberTypeCodedList.ManufacturersProducersArticleNumber;
						piaInterpretation.AddElementInterpretation(() => invoiceLine.Model);
					}

					if (!modelNumber.IsEmpty)
					{
						pia.ItemNumberIdentification3.ItemNumber = modelNumber;
						pia.ItemNumberIdentification3.ItemNumberTypeCoded = ItemNumberTypeCodedList.ModelNumber;
						piaInterpretation.AddElementInterpretation(() => invoiceLine.ModelNumber);
					}

					if (!brandName.IsEmpty)
					{
						pia.ItemNumberIdentification4.ItemNumber = brandName;
						pia.ItemNumberIdentification4.ItemNumberTypeCoded = ItemNumberTypeCodedList.ProductServiceIdentificationNumber;
						piaInterpretation.AddElementInterpretation(() => invoiceLine.BrandName);
					}

					if (!vehicleClass.IsEmpty)
					{
						pia.ItemNumberIdentification5.ItemNumber = vehicleClass;
						pia.ItemNumberIdentification5.ItemNumberTypeCoded = ItemNumberTypeCodedList.VendorsSupplementalItemNumber;
						piaInterpretation.AddElementInterpretation(() => invoiceLine.VehicleClass);
					}
				}
			}

			#endregion

			#region Quantity

			if (invoiceLine.Quantity > 0)
			{
				var qty = group21.QTY.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateQTY(qty, invoiceLine.Quantity, invoiceLine.QuantityUnits);

				var qtyInterpretation = interpretation.AddNewSegmentInterpretation(qty);
				qtyInterpretation.AddElementInterpretation(() => invoiceLine.QuantityUnits);
				qtyInterpretation.AddElementInterpretation(() => invoiceLine.Quantity.ToString("0.####"));
			}

			#endregion

			#region (OGD) Type/Size

			if (isOGDDataRequired && (data.OGDNR || data.OGDTC) && !invoiceLine.TypeSize.IsEmpty)
			{
				var mea = group21.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementApplicationQualifier = MeasurementApplicationQualifierList.X_Size;
				mea.MeasurementDetails.MeasurementAttribute = invoiceLine.TypeSize;

				interpretation.AddNewSegmentInterpretation(mea, Res.GetString("eb55f7d0-a1c0-4128-8b91-4741218b3eb5", "Type / Size"), invoiceLine.TypeSize);
			}

			#endregion

			#region UnitPrice

			var linePrice = invoiceLine.LinePrice;
			var unitPrice = invoiceLine.UnitPrice.IsEmpty && invoiceLine.Quantity > 0
									? (ZDecimal)(linePrice / invoiceLine.Quantity)
									: invoiceLine.UnitPrice;

			if (isAQDataRequired || unitPrice > 0)
			{
				var moa = group21.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeQualifierList.UnitPrice, unitPrice, string.Empty, 4);

				interpretation.AddNewSegmentInterpretation(moa, () => unitPrice, moa.MonetaryAmount.MonetaryAmount);
			}

			#endregion

			#region Line Price / Currency

			if (isAQDataRequired || linePrice > 0)
			{
				var moa = group21.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeQualifierList.InvoiceItemAmount, linePrice, invoiceLine.LinePriceCurrency, 2);
				var moaInterpretation = interpretation.AddNewSegmentInterpretation(moa);
				moaInterpretation.AddElementInterpretation(() => linePrice.ToString(2));
				moaInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.LinePriceCurrency);
			}

			#endregion

			#region (OGD) - RequirementID / RequirementVersion / AirsCode / DestinationProvince / EndUse / MiscID

			if (isOGDDataRequired && data.OGDCFIA)
			{
				if (!invoiceLine.RequirementID.IsEmpty || !invoiceLine.RequirementVersion.IsEmpty || !invoiceLine.AirsCode.IsEmpty
					|| !invoiceLine.DestinationProvince.IsEmpty || !invoiceLine.EndUse.IsEmpty)
				{
					var gir1 = group21.GIR.InstantiateAChildAndAddItToChildrenCollection();
					gir1.SetIdentificationQualifier = SetIdentificationQualifierList.Product;
					gir1.IdentificationNumber1.IdentityNumber = invoiceLine.RequirementID;
					gir1.IdentificationNumber2.IdentityNumber = invoiceLine.RequirementVersion;
					gir1.IdentificationNumber3.IdentityNumber = invoiceLine.AirsCode;
					gir1.IdentificationNumber4.IdentityNumber = invoiceLine.DestinationProvince;
					gir1.IdentificationNumber5.IdentityNumber = invoiceLine.EndUse;

					var girInterpretation = interpretation.AddNewSegmentInterpretation(gir1);
					girInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.RequirementID);
					girInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.RequirementVersion);
					girInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.AirsCode);
					girInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.DestinationProvince);
					girInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.EndUse);
				}

				if (!invoiceLine.MiscID.IsEmpty)
				{
					var gir2 = group21.GIR.InstantiateAChildAndAddItToChildrenCollection();
					gir2.SetIdentificationQualifier = SetIdentificationQualifierList.ValueList;
					gir2.IdentificationNumber1.IdentityNumber = invoiceLine.MiscID;

					interpretation.AddNewSegmentInterpretation(gir2, () => invoiceLine.MiscID);
				}
			}

			#endregion

			#region (OGD) Registration Numbers/Types

			if (isOGDDataRequired && (data.OGDCFIA || data.OGDIC) && invoiceLine.RegistrationNumbers.Length > 0)
			{
				if (invoiceLine.RegistrationNumbers.Length != invoiceLine.RegistrationTypes.Length)
				{
					throw new ArgumentException(string.Format("Lengths of Registration Numbers and Codes inconsistent: {0}/{1}", invoiceLine.RegistrationNumbers.Length, invoiceLine.RegistrationTypes.Length));
				}

				var registrationNumberDescription = Res.GetString("da3238ed-86c0-46e4-af9e-550a20b13559", "Registration Number");
				var registrationTypeDescription = Res.GetString("5607adc4-a9a0-44b5-a25e-4cb7ff6f1b8d", "Registration Type");

				for (var i = 0; i < invoiceLine.RegistrationNumbers.Length; i++)
				{
					var gir3 = group21.GIR.InstantiateAChildAndAddItToChildrenCollection();
					gir3.SetIdentificationQualifier = SetIdentificationQualifierList.Licence;
					gir3.IdentificationNumber1.IdentityNumber = invoiceLine.RegistrationNumbers[i];
					gir3.IdentificationNumber2.IdentityNumber = invoiceLine.RegistrationTypes[i];

					var girInterpretation = interpretation.AddNewSegmentInterpretation(gir3);
					girInterpretation.AddElementInterpretation(registrationNumberDescription, gir3.IdentificationNumber1.IdentityNumber);
					girInterpretation.AddElementInterpretation(registrationTypeDescription, gir3.IdentificationNumber2.IdentityNumber);
				}
			}

			#endregion

			#region (OGD) - Compliant Import Date Indicator / Compliant Completion Indicator / TIIN

			if (isOGDDataRequired && data.OGDTC && !invoiceLine.ImportReasonCode.IsEmpty)
			{
				var gir4 = group21.GIR.InstantiateAChildAndAddItToChildrenCollection();
				gir4.SetIdentificationQualifier = SetIdentificationQualifierList.Package;

				ZString complianceDateInd = invoiceLine.CompliantImportDateIndicator ? "3" : "2";
				ZString complianceCompInd = invoiceLine.CompliantCompletionIndicator ? "1" : "0";
				gir4.IdentificationNumber1.IdentityNumber = complianceDateInd;
				gir4.IdentificationNumber1.IdentityNumberQualifier = IdentityNumberQualifierList._1stStructureElementName;
				gir4.IdentificationNumber2.IdentityNumber = complianceCompInd;
				gir4.IdentificationNumber2.IdentityNumberQualifier = IdentityNumberQualifierList._2ndStructureElementName;

				if (!invoiceLine.TIIN.IsEmpty)
				{
					gir4.IdentificationNumber3.IdentityNumber = invoiceLine.TIIN;
					gir4.IdentificationNumber3.IdentityNumberQualifier = IdentityNumberQualifierList._3rdStructureElementName;
				}

				var girInterpretation = interpretation.AddNewSegmentInterpretation(gir4);
				girInterpretation.AddElementInterpretation(() => invoiceLine.CompliantImportDateIndicator, complianceDateInd);
				girInterpretation.AddElementInterpretation(() => invoiceLine.CompliantCompletionIndicator, complianceCompInd);
				girInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.TIIN);
			}

			#endregion

			#region (OGD) - VIN / AssemblyMonth

			if (isOGDDataRequired && data.OGDTC && invoiceLine.VIN.Length > 0)
			{
				if (invoiceLine.VIN.Length != invoiceLine.AssemblyMonth.Length)
				{
					throw new ArgumentException(string.Format("Lengths of VIN Numbers and Assembly Year/Month inconsistent: {0}/{1}", invoiceLine.VIN.Length, invoiceLine.AssemblyMonth.Length));
				}

				var vinDescription = interpretation.GetFriendlyPropertyName(() => invoiceLine.VIN);
				var assemblyMonthDescription = interpretation.GetFriendlyPropertyName(() => invoiceLine.AssemblyMonth);

				for (var i = 0; i < invoiceLine.VIN.Length; i++)
				{
					var gir5 = group21.GIR.InstantiateAChildAndAddItToChildrenCollection();
					gir5.SetIdentificationQualifier = SetIdentificationQualifierList.VehicleReferenceSet;
					gir5.IdentificationNumber1.IdentityNumber = invoiceLine.VIN[i];
					gir5.IdentificationNumber1.IdentityNumberQualifier = IdentityNumberQualifierList.VehicleIdentityNumber;
					gir5.IdentificationNumber2.IdentityNumber = invoiceLine.AssemblyMonth[i];
					gir5.IdentificationNumber2.IdentityNumberQualifier = IdentityNumberQualifierList.ManufacturingReferenceNumber;

					var girInterpretation = interpretation.AddNewSegmentInterpretation(gir5);
					girInterpretation.AddElementInterpretation(vinDescription, gir5.IdentificationNumber1.IdentityNumber);
					girInterpretation.AddElementInterpretation(assemblyMonthDescription, gir5.IdentificationNumber2.IdentityNumber);
				}
			}

			#endregion

			#region Country/Region Of Origin / (OGD) CFIA Origin

			if (invoice.CommonCountryOfOrigin == "VAR"
				|| (isOGDDataRequired && data.OGDCFIA && !invoiceLine.CFIAOrigin.IsEmpty))
			{
				#region Mandatory Trigger Segment

				var group25 = group21.Group25.InstantiateAChildAndAddItToChildrenCollection();
				var tod = group25.TOD.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateTOD(tod, TermsOfDeliveryOrTransportFunctionCodedList.TransportCondition, string.Empty, string.Empty);
				interpretation.AddMandatoryTriggerSegmentInterpretation(tod);

				#endregion

				#region Country/Region Of Origin / (OGD) CFIA Origin

				var lineCOC = invoiceLine.CountryOfOrigin.IsEmpty ? invoice.HeaderOrigin : invoiceLine.CountryOfOrigin;
				if (isOGDDataRequired)
				{
					var loc = group25.LOC.InstantiateAChildAndAddItToChildrenCollection();
					var lineCFIACOC = data.OGDCFIA ? invoiceLine.CFIAOrigin : ZString.Empty;
					D96AMessageUtilities.PopulateLOC(loc, lineCOC, lineCFIACOC, string.Empty);

					var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
					locInterpretation.AddElementInterpretation(() => invoiceLine.CountryOfOrigin, lineCOC);
					locInterpretation.AddElementInterpretationIfNotEmpty(() => invoiceLine.CFIAOrigin, lineCFIACOC);
				}
				else if (!lineCOC.IsEmpty)
				{
					var loc = group25.LOC.InstantiateAChildAndAddItToChildrenCollection();
					D96AMessageUtilities.PopulateLOC(loc, lineCOC, string.Empty, string.Empty);
					interpretation.AddNewSegmentInterpretation(loc, () => invoiceLine.CountryOfOrigin);
				}

				#endregion
			}

			#endregion

			#region Item Description

			var group27 = group21.Group27.InstantiateAChildAndAddItToChildrenCollection();
			var imd = group27.IMD.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateIMD(imd, "TRDESC", invoiceLine.ItemDescription);
			interpretation.AddNewSegmentInterpretation(imd, () => invoiceLine.ItemDescription.Left(TextLength));

			if (invoiceLine.ItemDescription.Length > TextLength)
			{
				var ftx = group27.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateFTX(ftx, TextSubjectQualifierList.GoodsDescription, invoiceLine.ItemDescription.SubstringSafe(TextLength, TextLength));

				var propertyName = interpretation.GetFriendlyPropertyName(() => invoiceLine.ItemDescription);
				interpretation.AddNewSegmentInterpretation(ftx, Res.GetString("d59a45ec-fdf9-4122-a04f-dced0b6f24a6", "{0} (Continued)", propertyName), ftx.TextLiteral.FreeText1);
			}

			#endregion

			#region (OGD) Import Reason Code

			if (isOGDDataRequired && (data.OGDIC || data.OGDNR || data.OGDTC) && !invoiceLine.ImportReasonCode.IsEmpty)
			{
				group27 = group21.Group27.InstantiateAChildAndAddItToChildrenCollection();
				imd = group27.IMD.InstantiateAChildAndAddItToChildrenCollection();
				imd.ItemCharacteristicCoded = ItemCharacteristicCodedList.EndUseApplication;
				imd.ItemDescription.ItemDescriptionIdentification = invoiceLine.ImportReasonCode;

				interpretation.AddNewSegmentInterpretation(imd, () => invoiceLine.ImportReasonCode);
			}

			#endregion
		}

		#endregion

		#region Populate Doc Address

		void PopulateDocAddress(Func<NADSegment> getNadFunc, Expression<Func<IDocAddress>> addressExpression, PartyQualifierList addressType,
					bool populateCompanyNameOnly, bool populateCountryCode, bool restrictStateCodeForCFIA = false)
		{
			var address = addressExpression.Compile()();
			if (address != null && !address.E2_CompanyName.IsEmpty)
			{
				var nad = getNadFunc();
				nad.PartyQualifier = addressType;
				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);

				const int companyNameAndAddressMaxLength = 35;
				var companyNameSplitter = new TextSplitter(companyNameAndAddressMaxLength) { Text = address.E2_CompanyName };
				nad.PartyName.PartyName1 = companyNameSplitter[0].Trim();
				nad.PartyName.PartyName2 = companyNameSplitter[1].Trim();
				nadInterpretation.AddElementInterpretation(addressExpression, address.E2_CompanyName.Left(companyNameAndAddressMaxLength * 2));

				if (!populateCompanyNameOnly)
				{
					if (address.E2_Address1.Length > companyNameAndAddressMaxLength && address.E2_Address2.IsEmpty)
					{
						companyNameSplitter = new TextSplitter(companyNameAndAddressMaxLength) { Text = address.E2_Address1 };
						nad.Street.StreetAndNumberPOBox1 = companyNameSplitter[0].Trim();
						nad.Street.StreetAndNumberPOBox2 = companyNameSplitter[1].Trim();

						nadInterpretation.AddElementInterpretation(() => address.E2_Address1.Left(companyNameAndAddressMaxLength * 2));
					}
					else
					{
						nad.Street.StreetAndNumberPOBox1 = address.E2_Address1.Left(companyNameAndAddressMaxLength);
						nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Address1.Left(companyNameAndAddressMaxLength));

						nad.Street.StreetAndNumberPOBox2 = address.E2_Address2.Left(companyNameAndAddressMaxLength);
						nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Address2.Left(companyNameAndAddressMaxLength));
					}

					const int cityMaxLength = 20;
					nad.CityName = address.E2_City.Left(cityMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_City.Left(cityMaxLength));

					int stateMaxLength = restrictStateCodeForCFIA ? 2 : 4;
					nad.CountrySubEntityIdentification = address.E2_State.Left(stateMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_State.Left(stateMaxLength));

					const int postcodeMaxLength = 9;
					nad.PostcodeIdentification = address.E2_Postcode.TrimStart().Left(postcodeMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Postcode.TrimStart().Left(postcodeMaxLength));

					if (populateCountryCode)
					{
						const int countryMaxLength = 2;
						nad.CountryCoded = address.CountryCode.Left(countryMaxLength);
						nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.CountryCode.Left(countryMaxLength));
					}
				}
			}
		}

		#endregion

		readonly bool isAQDataRequired;
		readonly bool isOGDDataRequired;

		#endregion
	}
}
