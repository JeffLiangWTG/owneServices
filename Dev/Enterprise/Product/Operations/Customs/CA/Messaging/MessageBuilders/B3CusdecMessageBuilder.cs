using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Messaging
{
	public class B3CusdecMessageBuilder<TResult> : D99BMessageBuilder<IB3Header, CUSDECMessage, TResult>
		where TResult : EDIMessage
	{
		public B3CusdecMessageBuilder(IB3Header header, MessageSubTypes messageSubType)
			: base(header, messageSubType)
		{
			dataDecider = new B3WhsEntryRequiredDataDecider(header);
		}
		ZInt globalLineCount = 1;
		ZInt globalSubHeaderCount = 1;

		#region Overrides of EDIFACTMessageBuilder

		protected override void PopulateEdifactMessage()
		{
			#region PopulateUNH

			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateUNH(
				unh,
				Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder,
				Enterprise.Edifact.D99B.Elements.MessageTypeList.CustomsDeclarationMessage,
				"S",
				"99B",
				ControllingAgencyList.UnCefact);

			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region PopulateBGM

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateBGM(bgm, data.B3TypeCode, data.BatchNumber, MessageFunctionCode);

			var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
			bgmInterpretation.AddElementInterpretation(() => data.B3TypeCode);
			bgmInterpretation.AddElementInterpretation(() => data.BatchNumber);
			bgmInterpretation.AddElementInterpretation(() => MessageFunctionCode);

			#endregion

			#region Payment Code

			if (!data.PaymentCode.IsEmpty)
			{
				var cst = edifactMessage.CST.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateCST(cst, data.PaymentCode);
				interpretation.AddNewSegmentInterpretation(cst, () => data.PaymentCode);
			}

			#endregion

			#region CBSA Office

			var loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.CustomsOfficeOfEntry, data.CBSAOffice);
			interpretation.AddNewSegmentInterpretation(loc, () => data.CBSAOffice);

			#endregion

			#region Port of Unlading

			if (dataDecider.IsRequired(B3WhsData.PortOfDischarge) && !data.PortOfUnlading.IsEmpty)
			{
				loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlacePortOfDischarge, data.PortOfUnlading);
				interpretation.AddNewSegmentInterpretation(loc, () => data.PortOfUnlading);
			}

			#endregion

			#region Warehouse Number

			if (!data.WarehouseNumber.IsEmpty)
			{
				loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.Warehouse, data.WarehouseNumber);
				interpretation.AddNewSegmentInterpretation(loc, () => data.WarehouseNumber);
			}

			#endregion

			#region Transaction Number

			var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();

			var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, data.TransactionNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => data.TransactionNumber);

			#endregion

			#region Business Number

			rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.NationalGovernmentBusinessIdentificationNumber, data.BusinessNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => data.BusinessNumber);

			#endregion

			#region GST Number

			if (!data.GSTNumber.IsEmpty)
			{
				rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.GovernmentAgencyReferenceNumber, data.GSTNumber);
				interpretation.AddNewSegmentInterpretation(rff, () => data.GSTNumber);
			}

			#endregion

			#region Transport Mode & Carrier Code At Importation

			if ((dataDecider.IsRequired(B3WhsData.TransportMode) && !data.TransportMode.IsEmpty)
				|| (dataDecider.IsRequired(B3WhsData.CarrierCodeAtImportation) && !data.CarrierCodeAtImportation.IsEmpty))
			{
				var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();

				var tdt = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder, data.TransportMode, data.CarrierCodeAtImportation);

				var tdtInterpretation = interpretation.AddNewSegmentInterpretation(tdt);
				tdtInterpretation.AddElementInterpretationIfNotEmpty(() => data.TransportMode);
				tdtInterpretation.AddElementInterpretationIfNotEmpty(() => data.CarrierCodeAtImportation);
			}

			#endregion

			#region Populate B3 B Release

			foreach (var release in data.B3BInputReleases)
			{
				PopulateB3BRelease(release);
			}

			#endregion

			#region Total Value For Duty

			if (dataDecider.IsRequired(B3WhsData.TotalValueForDuty) && !data.TotalValueForDuty.IsEmpty)
			{
				var group8 = edifactMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
				var moa = group8.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateMOARounded(moa, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, data.TotalValueForDuty);
				interpretation.AddNewSegmentInterpretation(moa, () => data.TotalValueForDuty.ToString(RoundedFormat));
			}

			#endregion

			#region UNS (Header Section Separator)

			var uns = edifactMessage.UNS1.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateUNS(uns, SectionIdentificationList.HeaderDetailSectionSeparation);
			interpretation.AddUNS1Interpretation(uns);

			#endregion

			#region Negative B3 Sub Headers

			foreach (var subHeader in data.NegativeB3SubHeaders)
			{
				PopulateB3SubHeader(subHeader);
			}

			#endregion

			#region Negative Classification Lines

			foreach (var line in data.NegativeClassificationLines)
			{
				PopulateClassificationLine(line);
			}

			#endregion

			#region Positive B3 Sub Headers

			foreach (var subHeader in data.PositiveB3SubHeaders)
			{
				PopulateB3SubHeader(subHeader);
			}

			#endregion

			#region Positive Classification Lines

			foreach (var line in data.PositiveClassificationLines)
			{
				PopulateClassificationLine(line);
			}
			#endregion

			#region UNS (Detail/Summary Section Separator)

			uns = edifactMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateUNS(uns, SectionIdentificationList.DetailSummarySectionSeparation);
			interpretation.AddUNS2Interpretation(uns);

			#endregion

			#region Total Positive Amounts

			PopulateTotalAmounts(data.PositiveTotalAmounts, true, data.SumPosAndNeg);

			#endregion

			#region Total Negative Amounts

			if (!data.SumPosAndNeg)
			{
				PopulateTotalAmounts(data.NegativeTotalAmounts, false, data.SumPosAndNeg);
			}

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unh.MessageReferenceNumber);

			#endregion
		}

		#region B3 B Release

		void PopulateB3BRelease(IB3BRelease release)
		{
			var group5 = edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection();

			#region Cargo Control Number (CCN)

			if (dataDecider.IsRequired(B3WhsData.CargoControlNumber) || (dataDecider.IsRequired(B3WhsData.DateOfRelease) && !release.DateOfRelease.IsEmpty))
			{
				var doc = group5.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.CargoManifest, release.CargoControlNumber.ExcludeChars(" "));
				interpretation.AddNewSegmentInterpretation(doc, () => release.CargoControlNumber.ExcludeChars(" "));
			}

			#endregion

			#region Date Of Release

			if (dataDecider.IsRequired(B3WhsData.DateOfRelease) && !release.DateOfRelease.IsEmpty)
			{
				var dtm = group5.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateDTM(dtm, DateTimePeriodFunctionCodeQualifierList.ReleaseDateCustoms, release.DateOfRelease);
				interpretation.AddNewSegmentInterpretation(dtm, () => release.DateOfRelease.ToShortDateString());
			}

			#endregion
		}

		#endregion

		#region B3 Sub Header

		void PopulateB3SubHeader(IB3SubHeader subHeader)
		{
			var group10 = edifactMessage.Group10.InstantiateAChildAndAddItToChildrenCollection();

			#region B3 Sub Header Number

			var subHeaderNum = subHeader.B3SubHeaderNumber;
			var isNegativeNum = subHeaderNum < 0;

			if (isNegativeNum)
			{
				subHeaderNum = globalSubHeaderCount;
				globalSubHeaderCount++;
			}

			var dms = group10.DMS.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateDMS(dms, subHeaderNum.ToString());
			interpretation.AddNewSegmentInterpretation(dms, () => isNegativeNum ? subHeaderNum : subHeader.B3SubHeaderNumber);

			#endregion

			#region Freight Charges

			MOASegment moa;
			if (dataDecider.IsRequired(B3WhsData.FreightCharges) && !subHeader.FreightCharges.IsEmpty)
			{
				var group11 = group10.Group11.InstantiateAChildAndAddItToChildrenCollection();
				moa = group11.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateMOARounded(moa, MonetaryAmountTypeCodeQualifierList.FreightCharge, subHeader.FreightCharges);
				interpretation.AddNewSegmentInterpretation(moa, () => subHeader.FreightCharges.ToString(RoundedFormat));
			}

			#endregion

			#region Vendor

			var group14 = group10.Group14.InstantiateAChildAndAddItToChildrenCollection();
			var nad = group14.NAD.InstantiateAChildAndAddItToChildrenCollection();
			var vendor = subHeader.Vendor;
			var vendorStateAndZip = subHeader.VendorStateAndZip;

			D99BMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Seller, vendor, vendorStateAndZip.State, vendorStateAndZip.Zip);

			if (vendor != null)
			{
				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);
				nadInterpretation.AddElementInterpretation(Res.GetString("a347be1a-1d37-4d79-b7e5-16c8996c5efa", "Vendor Name"), vendor.E2_CompanyName.Left(30));
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => vendorStateAndZip.State);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => vendorStateAndZip.Zip);
			}

			#endregion

			#region DOC (Customs Invoice)

			var group15 = group14.Group15.InstantiateAChildAndAddItToChildrenCollection();
			var doc = group15.DOC.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.CustomsInvoice, string.Empty);
			interpretation.AddMandatoryTriggerSegmentInterpretation(doc);

			#endregion

			#region Date Of Direct Shipment

			if (!subHeader.DateOfDirectShipment.IsEmpty)
			{
				var dtm = group15.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateDTM(dtm, DateTimePeriodFunctionCodeQualifierList.ExportationDate, subHeader.DateOfDirectShipment);
				interpretation.AddNewSegmentInterpretation(dtm, () => subHeader.DateOfDirectShipment.ToShortDateString());
			}

			#endregion

			#region Country/Region Of Origin, Place Of Export, US Port Of Exit

			var loc = group15.LOC.InstantiateAChildAndAddItToChildrenCollection();
			var usPortOfExit = dataDecider.IsRequired(B3WhsData.USPortOfExit) ? subHeader.USPortOfExit : ZString.Empty;
			D99BMessageUtilities.PopulateLOC(loc, subHeader.CountryOfOrigin, subHeader.PlaceOfExport, usPortOfExit);

			var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
			locInterpretation.AddElementInterpretation(() => subHeader.CountryOfOrigin);
			locInterpretation.AddElementInterpretation(() => subHeader.PlaceOfExport);
			if (!usPortOfExit.IsEmpty)
			{
				locInterpretation.AddElementInterpretation(() => subHeader.USPortOfExit);
			}

			#endregion

			#region Tariff Treatment Code, Time Limit

			var group18 = group10.Group18.InstantiateAChildAndAddItToChildrenCollection();

			var pat = group18.PAT.InstantiateAChildAndAddItToChildrenCollection();
			ZString timeLimits = dataDecider.IsRequired(B3WhsData.B3TimeLimits) ? subHeader.B3TimeLimits.ToString() : string.Empty;

			D99BMessageUtilities.PopulatePAT(
				pat,
				PaymentTermsTypeCodeQualifierList.Basic,
				"CONSIGN",
				subHeader.TariffTreatmentCode,
				TimeReferenceCodeList.SpecifiedDate,
				subHeader.TimeLimitUnit,
				timeLimits);

			var patInterpretation = interpretation.AddNewSegmentInterpretation(pat, () => subHeader.TariffTreatmentCode);

			if (!subHeader.TimeLimitUnit.IsEmpty && !timeLimits.IsEmpty)
			{
				patInterpretation.AddElementInterpretation(() => subHeader.TimeLimitUnit);
				patInterpretation.AddElementInterpretation(() => subHeader.B3TimeLimits);
			}

			#endregion

			#region Currency Code

			moa = group18.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.AmountReferenceCurrency, subHeader.CurrencyCode);
			interpretation.AddNewSegmentInterpretation(moa, () => subHeader.CurrencyCode);

			#endregion
		}

		#endregion

		#region Classification Line

		void PopulateClassificationLine(IClassificationLine1 line)
		{
			#region Classification Line 1

			var group30 = edifactMessage.Group30.InstantiateAChildAndAddItToChildrenCollection();

			#region  B3 Line Number

			var isLineNumberNegative = line.B3LineNumber < 0;
			var cst = group30.CST.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateCST(
				cst,
				isLineNumberNegative ? globalLineCount.ToString() : line.B3LineNumber.ToString(),
				line.RecordIdentifier,
				line.B3SubHeaderNumber.ToString(),
				line.ClassificationNumber.ToString(),
				line.ValueForDutyCode,
				line.TariffCode);

			var cstInterpretation = interpretation.AddNewSegmentInterpretation(cst);
			cstInterpretation.AddElementInterpretation(() => isLineNumberNegative ? globalLineCount : line.B3LineNumber);
			cstInterpretation.AddElementInterpretation(Res.GetString("6a739425-531e-4e4a-b684-ce6809b25d8c", "Positive or Negative Values"), line.RecordIdentifier);
			cstInterpretation.AddElementInterpretation(() => line.B3SubHeaderNumber);
			cstInterpretation.AddElementInterpretation(() => line.ClassificationNumber);
			cstInterpretation.AddElementInterpretation(() => line.ValueForDutyCode);
			cstInterpretation.AddElementInterpretationIfNotEmpty(() => line.TariffCode);

			#endregion

			#region Value For Currency Conversion

			var group33 = group30.Group33.InstantiateAChildAndAddItToChildrenCollection();
			var moa = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.CustomsValue, line.ValueForCurrency);
			interpretation.AddNewSegmentInterpretation(moa, () => line.ValueForCurrency.ToString(AmountFormat));

			#endregion

			#region Value For Duty

			moa = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, line.ValueForDuty);
			interpretation.AddNewSegmentInterpretation(moa, () => line.ValueForDuty.ToString(AmountFormat));

			#endregion

			#region Value For Tax

			if (dataDecider.IsRequired(B3WhsData.ValueForTax) && (dataDecider.IsWarehouseEntry || !line.ValueForTax.IsEmpty))
			{
				moa = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.TaxableAmount, line.ValueForTax);
				interpretation.AddNewSegmentInterpretation(moa, () => line.ValueForTax.ToString(AmountFormat));
			}

			#endregion

			#region Authority Number

			if (!line.AuthorityNumber.IsEmpty)
			{
				var group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				var rff = group35.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.CustomsDecisionRequestNumber, line.AuthorityNumber);
				interpretation.AddNewSegmentInterpretation(rff, () => line.AuthorityNumber);
			}

			#endregion

			#region TRS Number

			if (dataDecider.IsRequired(B3WhsData.TRSNumber) && !line.TRSNumber.IsEmpty)
			{
				var group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				var rff = group35.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.CustomsValuationDecisionNumber, line.TRSNumber, line.B3LineNumber.ToString());
				interpretation.AddNewSegmentInterpretation(rff, () => line.TRSNumber);
			}

			#endregion

			#region Automotive Part Numbers

			if (line.PartNumberDescriptions.Length > 0 && (data.B3TypeCode == B3EntryTypeList.Codes.AutomotiveS || data.B3TypeCode == B3EntryTypeList.Codes.AutomotiveP))
			{
				var group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();

				var rff = group35.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.ManufacturersPartNumber, string.Empty, line.B3LineNumber.ToString());
				interpretation.AddNewSegmentInterpretation(rff, Res.GetString("c8881101-f3da-4c37-b0c6-95719c20e517", "Automotive Part Numbers Of B3 Line"), line.B3LineNumber);

				D99BMessageUtilities.PopulateGIN(group35, ObjectIdentificationCodeQualifierList.PartNumber, line.PartNumberDescriptions, interpretation);
			}

			#endregion

			#region Invoice Cross References

			var invoiceCrossReferences = line.InvoiceCrossReferences.ToArray();
			if (invoiceCrossReferences.Length > 999)
			{
				var grouped = invoiceCrossReferences.GroupBy(x => x.InvoicePageNumber);
				foreach (var group in grouped)
				{
					PopulateInvoiceCrossReferences(group30, InvoiceCrossReference.New(group.Key, group));
				}
			}
			else
			{
				foreach (var invoiceCrossReference in invoiceCrossReferences)
				{
					PopulateInvoiceCrossReferences(group30, invoiceCrossReference);
				}
			}
			#endregion

			#endregion

			#region Classification Line 3

			#region SIMA

			//TODO INC Check result here
			var group41 = PopulateAmount(
				group30,
				DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem,
				DutyTaxFeeTypeNameCodeList.AntiDumpingDuty,
				line.SIMACode,
				MonetaryAmountTypeCodeQualifierList.DeductionsCustoms,
				line.SIMAAssessment, line.SIMACode);

			if (group41 != null)
			{
				interpretation.AddNewSegmentInterpretation(group41.TAX[0], () => line.SIMACode);
				interpretation.AddNewSegmentInterpretation(group41.MOA[0], () => line.SIMAAssessment.ToString(AmountFormat));
			}

			#endregion

			#region Excise Tax

			if (dataDecider.IsRequired(B3WhsData.ExciseTaxAmount))
			{
				var rateFormat = GetRateFormat(line.ExciseTaxRateType);
				group41 = PopulateAmount(
					group30,
					DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem,
					DutyTaxFeeTypeNameCodeList.ExciseDuty,
					GetRate(line.ExciseExemptionCode, line.ExciseTaxRate, rateFormat),
					MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount,
					line.ExciseTaxAmount, line.ExciseExemptionCode, line.IsDummyExciseTaxRate);

				if (group41 != null)
				{
					var expression = line.ExciseExemptionCode.IsEmpty ? () => line.ExciseTaxRate.ToString(rateFormat)
										: (Expression<Func<string>>)(() => line.ExciseExemptionCode.ToString());
					interpretation.AddNewSegmentInterpretation(group41.TAX[0], expression);
					interpretation.AddNewSegmentInterpretation(group41.MOA[0], () => line.ExciseTaxAmount.ToString(AmountFormat));
				}
			}

			#endregion

			#region GST

			if (dataDecider.IsRequired(B3WhsData.GSTAmount))
			{
				var rateFormat = GetRateFormat(line.GSTRateType);
				var code = GetRate(line.GSTExemptionCode, line.RateOfGST, rateFormat);
				group41 = PopulateAmount(
					group30,
					DutyTaxFeeFunctionQualifierList.Tax,
					DutyTaxFeeTypeNameCodeList.ValueAddedTax,
					code,
					MonetaryAmountTypeCodeQualifierList.Vat1stValue,
					line.GSTAmount, code);

				if (group41 != null)
				{
					var expression = line.GSTExemptionCode.IsEmpty ? () => line.RateOfGST.ToString(rateFormat)
										: (Expression<Func<string>>)(() => line.GSTExemptionCode.ToString());
					interpretation.AddNewSegmentInterpretation(group41.TAX[0], expression);
					interpretation.AddNewSegmentInterpretation(group41.MOA[0], () => line.GSTAmount.ToString(AmountFormat));
				}
			}

			#endregion

			#endregion

			#region Classification Line 2

			var classLines = line.ClassificationLines;
			if (classLines.Any())
			{
				foreach (var line2 in classLines)
				{
					PopulateClassificationLine(group30, line2);
				}
			}
			else
			{
				string girValue = "0";
				if (line.B3LineNumber < 0)
				{
					girValue = globalLineCount.ToString();
					globalLineCount++;
				}
				var group44 = group30.Group44.InstantiateAChildAndAddItToChildrenCollection();
				var gir = group44.GIR.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateGIR(gir, SetIdentificationQualifierList.Product, girValue);
				interpretation.AddNewSegmentInterpretation(gir, Res.GetString("f598771c-e60d-4181-b12d-7cc3a112df3c", "Classification Line Of B3 Line"), girValue);
			}

			#endregion
		}

		ZString GetRateFormat(ZString rateType)
		{
			return rateType == RateTypes.Codes.AdValorem ? RateFormatAdValorem : RateFormatSpecific;
		}

		ZString GetRate(ZString code, ZDecimal rate, ZString rateFormat)
		{
			return !code.IsEmpty ? code : (ZString)rate.ToString(rateFormat);
		}

		SegmentGroup41 PopulateAmount(SegmentGroup30 group30, DutyTaxFeeFunctionQualifierList dutyTaxFeeQualifier, DutyTaxFeeTypeNameCodeList dutyTaxFeeTypeNameCode,
								string code, MonetaryAmountTypeCodeQualifierList monetaryAmountQualifier, ZDecimal amount, ZString exemptionCode, bool populateWhenAmountIsZero = false)
		{
			if (!amount.IsEmpty || populateWhenAmountIsZero || !exemptionCode.IsEmpty)
			{
				var group41 = group30.Group41.InstantiateAChildAndAddItToChildrenCollection();

				D99BMessageUtilities.PopulateTAX(
					group41.TAX.InstantiateAChildAndAddItToChildrenCollection(),
					dutyTaxFeeQualifier,
					dutyTaxFeeTypeNameCode,
					code);

				D99BMessageUtilities.PopulateMOA(
					group41.MOA.InstantiateAChildAndAddItToChildrenCollection(),
					monetaryAmountQualifier,
					amount);

				return group41;
			}
			return null;
		}

		void PopulateClassificationLine(SegmentGroup30 group30, IClassificationLine2 line2)
		{
			var group44 = group30.Group44.InstantiateAChildAndAddItToChildrenCollection();

			#region B3 Line Number

			ZInt lineNumber = line2.B3LineNumber;
			if (lineNumber < 0)
			{
				lineNumber = globalLineCount;
				globalLineCount++;
			}

			var gir = group44.GIR.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateGIR(gir, SetIdentificationQualifierList.Product, lineNumber.ToString());
			interpretation.AddNewSegmentInterpretation(gir, Res.GetString("f598771c-e60d-4181-b12d-7cc3a112df3c", "Classification Line Of B3 Line"), lineNumber);

			#endregion

			#region Classification Line Quantity

			if (!line2.ClassificationLineQuantity.IsEmpty)
			{
				var mea = group44.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateMEA(mea, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, line2.UnitOfMeasureCode, line2.ClassificationLineQuantity);

				var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
				meaInterpretation.AddElementInterpretationIfNotEmpty(() => line2.UnitOfMeasureCode);
				meaInterpretation.AddElementInterpretation(() => line2.ClassificationLineQuantity.ToString(QuantityFormat));
			}

			#endregion

			#region Weight in KGM

			if (dataDecider.IsRequired(B3WhsData.WeightInKGM) && !line2.WeightInKGM.IsEmpty)
			{
				var mea = group44.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateMEARounded(mea, MeasurementAttributeCodeList.LineItemMeasurement, CanadianUnitOfWeightList.Codes.Kilogram, line2.WeightInKGM);
				interpretation.AddNewSegmentInterpretation(mea, () => line2.WeightInKGM.ToString(RoundedFormat));
			}

			#endregion

			#region Customs Duty

			var group47 = group44.Group47.InstantiateAChildAndAddItToChildrenCollection();

			if (dataDecider.IsRequired(B3WhsData.CustomsDutyRate))
			{
				var rateFormat = GetRateFormat(line2.CustomsDutyRateType);
				var tax = group47.TAX.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateTAX(tax, DutyTaxFeeFunctionQualifierList.CustomsDuty, GetRate(ZString.Empty, line2.CustomsDutyRate, rateFormat));
				interpretation.AddNewSegmentInterpretation(tax, () => line2.CustomsDutyRate.ToString(rateFormat));
			}

			if (!dataDecider.IsWarehouseEntry || (dataDecider.IsRequired(B3WhsData.CustomsDutyAmount) && !line2.CustomsDutyAmount.IsEmpty))
			{
				var moa = group47.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.StandardDuty, line2.CustomsDutyAmount);
				interpretation.AddNewSegmentInterpretation(moa, () => line2.CustomsDutyAmount.ToString(AmountFormat));
			}

			#endregion

			#region Previous Transaction Number

			if (data.PaymentCode == "D"
				|| (data.B3TypeCode != "X" && dataDecider.IsRequired(B3WhsData.PreviousLineNumber) && !line2.PreviousLineNumber.IsEmpty
				&& dataDecider.IsRequired(B3WhsData.PreviousTransactionNumber) && !line2.PreviousTransactionNumber.IsEmpty))
			{
				var group48 = group44.Group48.InstantiateAChildAndAddItToChildrenCollection();

				var doc = group48.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateDOC(
					doc,
					DocumentNameCodeList.PreviousCustomsDocumentMessage,
					line2.PreviousTransactionNumber,
					line2.PreviousLineNumber.ToString());

				var docInterpretation = interpretation.AddNewSegmentInterpretation(doc);
				docInterpretation.AddElementInterpretation(() => line2.PreviousTransactionNumber);
				docInterpretation.AddElementInterpretation(() => line2.PreviousLineNumber);
			}

			#endregion
		}

		void PopulateInvoiceCrossReferences(SegmentGroup30 group30, IInvoiceCrossReference invoice)
		{
			var group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();

			#region Reference Number

			var rff = group35.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateRFF(
				rff,
				ReferenceFunctionCodeQualifierList.LineItemReferenceNumber,
				invoice.InvoicePageNumber.ToString(),
				invoice.InvoiceLineNumber.ToString());

			var rffInterpretation = interpretation.AddNewSegmentInterpretation(rff);
			rffInterpretation.AddElementInterpretation(() => invoice.InvoicePageNumber);
			rffInterpretation.AddElementInterpretation(() => invoice.InvoiceLineNumber);

			#endregion

			#region Invoice Item Amount

			var moa = group35.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.InvoiceItemAmount, invoice.InvoiceValue);
			interpretation.AddNewSegmentInterpretation(moa, () => invoice.InvoiceValue.ToString(AmountFormat));

			#endregion
		}

		#endregion

		#region Total Amounts

		void PopulateTotalAmounts(ITotalAmounts totalAmounts, bool isPositive, bool sumPosAndNeg)
		{
			if (isPositive || !(totalAmounts.TotalAllDutyAndTaxes.IsEmpty && totalAmounts.TotalCustomsDuty.IsEmpty && totalAmounts.TotalExciseTax.IsEmpty
				  && totalAmounts.TotalGST.IsEmpty && totalAmounts.TotalSIMAAssessment.IsEmpty))
			{
				#region Total Customs Duty

				var isTotalNegative = totalAmounts.TotalCustomsDuty < 0;

				PopulateTotalAmount(
					DutyTaxFeeFunctionQualifierList.CustomsDuty,
					GetTaxFeeTypeName(isTotalNegative, isPositive, sumPosAndNeg),
					MonetaryAmountTypeCodeQualifierList.StandardDuty,
					() => sumPosAndNeg ? GetAbsTotal(isTotalNegative, totalAmounts.TotalCustomsDuty) : totalAmounts.TotalCustomsDuty);

				#endregion

				#region Total SIMA Assessment

				if (isPositive)
				{
					isTotalNegative = totalAmounts.TotalSIMAAssessment < 0;
					PopulateTotalAmount(
						DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem,
						GetTaxFeeTypeName(isTotalNegative, isPositive, sumPosAndNeg),
						MonetaryAmountTypeCodeQualifierList.OtherValuationChargesCustoms,
						() => sumPosAndNeg ? GetAbsTotal(isTotalNegative, totalAmounts.TotalSIMAAssessment) : totalAmounts.TotalSIMAAssessment);
				}

				#endregion

				#region Total Excise Tax

				isTotalNegative = totalAmounts.TotalExciseTax < 0;
				PopulateTotalAmount(
					DutyTaxFeeFunctionQualifierList.TotalOfEachDutyTaxOrFeeTypeCustomsDeclaration,
					GetTaxFeeTypeName(isTotalNegative, isPositive, sumPosAndNeg),
					MonetaryAmountTypeCodeQualifierList.AdditionalRoyaltiesCustoms,
					() => sumPosAndNeg ? GetAbsTotal(isTotalNegative, totalAmounts.TotalExciseTax) : totalAmounts.TotalExciseTax);

				#endregion

				#region Total GST

				isTotalNegative = totalAmounts.TotalGST < 0;
				PopulateTotalAmount(
					DutyTaxFeeFunctionQualifierList.Tax,
					GetTaxFeeTypeName(isTotalNegative, isPositive, sumPosAndNeg),
					MonetaryAmountTypeCodeQualifierList.Vat1stValue,
					() => sumPosAndNeg ? GetAbsTotal(isTotalNegative, totalAmounts.TotalGST) : totalAmounts.TotalGST);

				#endregion

				#region Total All Duty And Taxes

				isTotalNegative = totalAmounts.TotalAllDutyAndTaxes < 0;
				PopulateTotalAmount(
					DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration,
					GetTaxFeeTypeName(isTotalNegative, isPositive, sumPosAndNeg),
					MonetaryAmountTypeCodeQualifierList.MessageTotalDutyTaxFeeAmount,
					() => sumPosAndNeg ? GetAbsTotal(isTotalNegative, totalAmounts.TotalAllDutyAndTaxes) : totalAmounts.TotalAllDutyAndTaxes,
					true);

				#endregion
			}
		}

		ZDecimal GetAbsTotal(bool isTotalNegative, ZDecimal value)
		{
			if (isTotalNegative)
			{
				return value * -1;
			}
			else
			{
				return value;
			}
		}

		string GetTaxFeeTypeName(bool isTotalNegative, bool isPositiveLine, bool sumPosAndNeg)
		{
			string result;
			if (sumPosAndNeg)
			{
				result = isTotalNegative ? "K90" : "K92";
			}
			else
			{
				result = isPositiveLine ? "K90" : "K92";
			}
			return result;
		}

		void PopulateTotalAmount(DutyTaxFeeFunctionQualifierList dutyTaxFeeQualifier, string dutyTaxFeeTypeName,
									MonetaryAmountTypeCodeQualifierList monetaryAmountQualifier, Expression<Func<ZDecimal>> amountExpression, bool sendEvenIfZero = false)
		{
			var amount = amountExpression.Compile()();
			if (sendEvenIfZero || !amount.IsEmpty)
			{
				var dutyTaxDescription = interpretation.GetFriendlyPropertyName(amountExpression);

				var group49 = edifactMessage.Group49.InstantiateAChildAndAddItToChildrenCollection();
				var tax = group49.TAX.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateTAXTypeName(tax, dutyTaxFeeQualifier, dutyTaxFeeTypeName);
				interpretation.AddNewSegmentInterpretation(tax, Res.GetString("1deba541-d6c4-4f76-9bc8-e22c09e24074", "{0} Identifier", dutyTaxDescription), (dutyTaxFeeTypeName == "K90" ? Res.GetString("4aca4941-1f54-473a-a138-4a9a7e73be10", "Positive") + " " : Res.GetString("721e8270-095b-45ec-9d61-0f3aba59b52c", "Negative") + " "));

				var moa = group49.MOA.InstantiateAChildAndAddItToChildrenCollection();

				D99BMessageUtilities.PopulateMOA(moa, monetaryAmountQualifier, amount);
				interpretation.AddNewSegmentInterpretation(moa, Res.GetString("885f3296-ae62-40bc-b2c8-1f32dca0eb9a", "{0} Amount", dutyTaxDescription), amount.ToString(AmountFormat));
			}
		}

		#endregion

		#endregion

		readonly B3WhsEntryRequiredDataDecider dataDecider;

		const string AmountFormat = "0.00";
		const string RateFormatSpecific = "0.00###";
		const string RateFormatAdValorem = "0.0####";
		const string RoundedFormat = "0";
		const string QuantityFormat = "0.000";
	}
}
