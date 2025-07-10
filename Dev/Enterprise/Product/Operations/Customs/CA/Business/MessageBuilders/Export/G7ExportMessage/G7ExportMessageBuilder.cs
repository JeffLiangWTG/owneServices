//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System;
	using System.Linq.Expressions;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Customs.CA.Edifact.GSIMEX;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Edifact.D00A.Segments;
	using Enterprise.Edifact.Utilities;
	using Enterprise.MasterFiles.Integration;

	public class G7ExportMessageBuilder : D00AMessageBuilder<IG7Export, GSIMEXMessage, EX1STPMessage>
	{
		public G7ExportMessageBuilder(IG7Export g7ExportData, MessageSubTypes messageSubType)
			: base(g7ExportData, messageSubType, DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail)
		{
		}

		#region Overrides of EDIFACTMessageBuilder

		protected override void PopulateEdifactMessage()
		{
			#region PopulateUNH

			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, "GSIMEX", "D", "00A", "CC", "EX1STP");
			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region PopulateBGM

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateBGM(bgm, DocumentNameCode, "", data.DocumentMessageNumber, MessageFunctionCode);

			var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
			bgmInterpretation.AddElementInterpretation(() => DocumentNameCode);
			bgmInterpretation.AddElementInterpretation(() => data.DocumentMessageNumber);
			bgmInterpretation.AddElementInterpretation(() => MessageFunctionCode);

			#endregion

			#region Port Of Exit

			var loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateLOC(
				loc,
				LocationFunctionCodeQualifierList.CustomsOfficeOfExit,
				data.PortOfExit,
				CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);

			interpretation.AddNewSegmentInterpretation(loc, () => data.PortOfExit);

			#endregion

			#region Place Of Report

			if (!data.PlaceOfReport.IsEmpty)
			{
				loc = edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.ReportingLocation,
					data.PlaceOfReport,
					CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);

				interpretation.AddNewSegmentInterpretation(loc, () => data.PlaceOfReport);
			}

			#endregion

			#region Date Of Export

			var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateDTM203(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.ExportationDate, data.DateOfExport);
			interpretation.AddNewSegmentInterpretation(dtm, () => data.DateOfExport);

			#endregion

			#region Commodity Gross Weight

			var mea = edifactMessage.MEA.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateMEAWeightInKG(mea, data.CommodityGrossWeight, data.CommodityGrossWeightUnitOfMeasure);

			var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
			meaInterpretation.AddElementInterpretation(() => data.CommodityGrossWeight, mea.ValueRange.MeasurementValue);
			meaInterpretation.AddElementInterpretation(() => data.CommodityGrossWeightUnitOfMeasure, mea.ValueRange.MeasurementUnitCode);

			#endregion

			#region Containers

			if (data.Containers != null)
			{
				foreach (var container in data.Containers)
				{
					PopulateContainer(container);
				}
			}

			#endregion

			#region Transport Details

			var tdt = edifactMessage.TDT.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateTDT(
				tdt,
				TransportStageCodeQualifierList.AtBorder,
				data.ModeOfTransport,
				data.CarrierCode,
				data.CarrierName,
				data.VesselName);

			var tdtInterpretation = interpretation.AddNewSegmentInterpretation(tdt);
			tdtInterpretation.AddElementInterpretation(() => data.ModeOfTransport);
			tdtInterpretation.AddElementInterpretationIfNotEmpty(() => data.CarrierCode);
			if (data.CarrierCode.IsEmpty)
			{
				tdtInterpretation.AddElementInterpretation(() => data.CarrierName);
			}
			tdtInterpretation.AddElementInterpretationIfNotEmpty(() => data.VesselName);

			#endregion

			#region Transaction Number

			var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber, data.TransactionNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => data.TransactionNumber);

			#endregion

			#region Transportation Document Number

			group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.TransportDocumentNumber, data.TransportationDocumentNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => data.TransportationDocumentNumber);

			#endregion

			#region Packages

			var pac = group1.PAC.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulatePAC(pac, data.NumberOfPackages, data.TypeOfPackages);

			var pacInterpretation = interpretation.AddNewSegmentInterpretation(pac);
			pacInterpretation.AddElementInterpretation(() => data.NumberOfPackages);
			pacInterpretation.AddElementInterpretation(() => data.TypeOfPackages);

			#endregion

			#region CAED Authorization ID

			if (!data.CAEDAuthorizationID.IsEmpty)
			{
				group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
				rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.CustomersIndividualTransactionReferenceNumber, data.CAEDAuthorizationID);
				interpretation.AddNewSegmentInterpretation(rff, () => data.CAEDAuthorizationID);
			}

			#endregion

			#region Service Option

			var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			var cst = group2.CST.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateCST(cst, data.ServiceOption, CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);
			cst.CustomsIdentityCodes1.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsProcedure;
			interpretation.AddNewSegmentInterpretation(cst, () => data.ServiceOption);

			#endregion

			#region Exporter

			Func<NADSegment> nadFunc = () =>
										{
											var group3 = edifactMessage.Group3.InstantiateAChildAndAddItToChildrenCollection();
											return group3.NAD.InstantiateAChildAndAddItToChildrenCollection();
										};

			PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.Exporter, () => data.Exporter, data.ExporterBusinessNumber);

			#endregion

			#region Broker Security Number

			if (!data.BrokerSecurityNumber.IsEmpty)
			{
				var group3 = edifactMessage.Group3.InstantiateAChildAndAddItToChildrenCollection();
				var nad = group3.NAD.InstantiateAChildAndAddItToChildrenCollection();

				D00AMessageUtilities.PopulateNAD(
					nad,
					PartyFunctionCodeQualifierList.AgentRepresentative,
					data.BrokerSecurityNumber,
					CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);

				interpretation.AddNewSegmentInterpretation(nad, () => data.BrokerSecurityNumber);
			}

			#endregion

			#region Delivery Party

			PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.DeliveryParty, () => data.DeliveryParty, data.DeliveryPartyBusinessNumber);

			#endregion

			#region Invoice Total

			var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			var moa = group4.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount, data.InvoiceTotal, data.InvoiceCurrencyCode, 2);

			var moaInterpretation = interpretation.AddNewSegmentInterpretation(moa);
			moaInterpretation.AddElementInterpretation(() => data.InvoiceTotal, moa.MonetaryAmount.MonetaryAmount);
			moaInterpretation.AddElementInterpretationIfNotEmpty(() => data.InvoiceCurrencyCode);

			#endregion

			#region Freight Charges In CAD

			group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			moa = group4.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.FreightCharge, data.FreightChargesInCAD, Constants.CurrencyCodes.Canada, 0);
			interpretation.AddNewSegmentInterpretation(moa, () => data.FreightChargesInCAD, moa.MonetaryAmount.MonetaryAmount);

			#endregion

			#region UNS (Headers/Details Section Separator)

			var uns1 = edifactMessage.UNS1.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateUNS(uns1, "D");
			interpretation.AddUNS1Interpretation(uns1);

			#endregion

			#region Sub-Header Sequence Number

			var group5 = edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection();
			var seq = group5.SEQ.InstantiateAChildAndAddItToChildrenCollection();
			seq.SequenceInformation.SequencePositionIdentifier = "1";
			interpretation.AddMandatoryTriggerSegmentInterpretation(seq);

			#endregion

			#region References

			var builder = new ZStringBuilder(data.References);
			var dms = group5.DMS.InstantiateAChildAndAddItToChildrenCollection();
			var references = new ZString(builder.ToStringWithDelimiterBetweenAppends(",")).Left(35).TrimEnd(',');
			dms.DocumentMessageIdentification.DocumentIdentifier = references;
			interpretation.AddNewSegmentInterpretation(dms, () => data.References, references);

			#endregion

			#region Reason For Export

			if (!data.ReasonForExport.IsEmpty)
			{
				var gei = group5.GEI.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateGEI(gei, "3", data.ReasonForExport);
				interpretation.AddNewSegmentInterpretation(gei, () => data.ReasonForExport);
			}

			#endregion

			#region Vendor / Consignee

			nadFunc = () =>
						{
							var group7 = group5.Group7.InstantiateAChildAndAddItToChildrenCollection();
							return group7.NAD.InstantiateAChildAndAddItToChildrenCollection();
						};

			PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.Seller, () => data.Vendor);
			PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.Consignee, () => data.Consignee);

			#endregion

			#region Populate Details

			var lineNumber = 0;
			foreach (var detailLine in data.Details)
			{
				PopulateDetailLine(group5, detailLine, ++lineNumber);
			}

			#endregion

			#region UNS (Details/Summary Section Separator)

			var uns2 = edifactMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateUNS(uns2, "S");
			interpretation.AddUNS2Interpretation(uns2);

			#endregion

			#region Authentication

			if (!data.Authentication.IsEmpty)
			{
				var aut = edifactMessage.AUT.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateAUT(aut, data.Authentication);
				interpretation.AddNewSegmentInterpretation(aut, () => data.Authentication);
			}

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);

			#endregion
		}

		#region Populate Container

		void PopulateContainer(IG7Container container)
		{
			var eqd = edifactMessage.EQD.InstantiateAChildAndAddItToChildrenCollection();

			D00AMessageUtilities.PopulateEQDContainer(
				eqd,
				container.
					ContainerNumber,
				container.CountryOfRegistration,
				container.ContainerSizeCode);

			var eqdInterpretation = interpretation.AddNewSegmentInterpretation(eqd);
			eqdInterpretation.AddElementInterpretation(() => container.ContainerNumber);
			if (!container.CountryOfRegistration.IsEmpty && !container.ContainerSizeCode.IsEmpty)
			{
				eqdInterpretation.AddElementInterpretationIfNotEmpty(() => container.CountryOfRegistration);
				eqdInterpretation.AddElementInterpretationIfNotEmpty(() => container.ContainerSizeCode);
			}
		}

		#endregion

		#region Populate Doc Address

		void PopulateDocAddress(Func<NADSegment> getNadFunc, PartyFunctionCodeQualifierList addressType, Expression<Func<IDocAddress>> addressExpression, string businessNumber = "")
		{
			var address = addressExpression.Compile()();
			if (address != null && !address.E2_CompanyName.IsEmpty)
			{
				var nad = getNadFunc();
				nad.PartyFunctionCodeQualifier = addressType;
				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);

				const int companyNameAndAddressMaxLength = 35;
				var companyNameSplitter = new TextSplitter(companyNameAndAddressMaxLength) { Text = address.E2_CompanyName };
				nad.PartyName.PartyName1 = companyNameSplitter[0].Trim();
				nad.PartyName.PartyName2 = companyNameSplitter[1].Trim();
				nadInterpretation.AddElementInterpretation(addressExpression, address.E2_CompanyName.Left(companyNameAndAddressMaxLength * 2));

				if (address.E2_Address1.Length > companyNameAndAddressMaxLength && address.E2_Address2.IsEmpty)
				{
					companyNameSplitter = new TextSplitter(companyNameAndAddressMaxLength) { Text = address.E2_Address1 };
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = companyNameSplitter[0].Trim();
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = companyNameSplitter[1].Trim();

					nadInterpretation.AddElementInterpretation(() => address.E2_Address1.Left(companyNameAndAddressMaxLength * 2));
				}
				else
				{
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = address.E2_Address1.Left(companyNameAndAddressMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Address1.Left(companyNameAndAddressMaxLength));

					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = address.E2_Address2.Left(companyNameAndAddressMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Address2.Left(companyNameAndAddressMaxLength));
				}

				const int cityMaxLength = 35;
				nad.CityName = address.E2_City.Left(cityMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_City.Left(cityMaxLength));

				const int stateMaxLength = 9;
				nad.CountrySubEntityDetails.CountrySubEntityNameCode = address.E2_State.Left(stateMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_State.Left(stateMaxLength));

				const int postcodeMaxLength = 9;
				nad.PostalIdentificationCode = address.E2_Postcode.Left(postcodeMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Postcode.Left(postcodeMaxLength));

				const int countryMaxLength = 2;
				nad.CountryNameCode = address.CountryCode.Left(countryMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.CountryCode.Left(countryMaxLength));

				if (!string.IsNullOrEmpty(businessNumber))
				{
					nad.PartyIdentificationDetails.PartyIdentifier = businessNumber;
					nad.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise;
					nadInterpretation.AddElementInterpretation(Res.GetString("24d9620f-fc4d-49d5-8002-25e1cd1a1755", "Business Number"), businessNumber);
				}
			}
		}

		#endregion

		#region Populate Detail Line

		public const int G7DescriptionMaxLength = 140;

		void PopulateDetailLine(SegmentGroup5 group5, IG7ItemLine detailLine, int lineNumber)
		{
			#region B13 Line Number

			var group8 = group5.Group8.InstantiateAChildAndAddItToChildrenCollection();
			var lin = group8.LIN.InstantiateAChildAndAddItToChildrenCollection();
			lin.LineItemIdentifier = lineNumber.ToString();
			interpretation.AddNewSegmentInterpretation(lin, Res.GetString("bfe0001b-2d5b-433a-8b8b-b184dff53918", "B13 Line Number"), lineNumber);

			#endregion

			#region Country/Region / Province Of Origin

			var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateLOC(
				loc,
				LocationFunctionCodeQualifierList.CountryOfOrigin,
				detailLine.CountryOfOrigin,
				CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization,
				detailLine.ProvinceOfOrigin,
				CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);

			var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
			locInterpretation.AddElementInterpretation(() => detailLine.CountryOfOrigin);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => detailLine.ProvinceOfOrigin);

			#endregion

			#region Vehicle Identification Numbers

			foreach (var vin in detailLine.VINs)
			{
				var description = Res.GetString("6f8e9308-cfff-46c0-a220-585187228494", "VIN");
				if (!vin.Trim().IsEmpty)
				{
					var rff = group8.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.VehicleIdentificationNumberVin, vin.Trim());
					interpretation.AddNewSegmentInterpretation(rff, description, vin);
				}
			}

			#endregion

			#region Export Permit Numbers

			var exportPermitNumber = Res.GetString("91733ac2-bfdf-4397-b38c-1da73d603252", "Export Permit Number");
			foreach (var permit in detailLine.Permits)
			{
				var group10 = group8.Group10.InstantiateAChildAndAddItToChildrenCollection();
				var doc = group10.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.ExportLicence, permit);
				doc.DocumentMessageName.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise;
				interpretation.AddNewSegmentInterpretation(doc, exportPermitNumber, permit);
			}

			#endregion

			#region Product Description

			var group11 = group8.Group11.InstantiateAChildAndAddItToChildrenCollection();
			var imd = group11.IMD.InstantiateAChildAndAddItToChildrenCollection();
			imd.ItemDescription.ItemDescription1 = detailLine.ProductDescription.Left(G7DescriptionMaxLength);
			interpretation.AddNewSegmentInterpretation(imd, () => detailLine.ProductDescription.Left(G7DescriptionMaxLength));

			#endregion

			#region Invoice Line Number

			if (detailLine.InvoiceLineNumber > 0)
			{
				var rff = group11.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.LineItemReferenceNumber, detailLine.InvoiceLineNumber.ToString());
				interpretation.AddNewSegmentInterpretation(rff, () => detailLine.InvoiceLineNumber);
			}

			#endregion

			#region Customs Item Number

			var group12 = group8.Group12.InstantiateAChildAndAddItToChildrenCollection();
			var gid = group12.GID.InstantiateAChildAndAddItToChildrenCollection();
			gid.GoodsItemNumber = "1";
			interpretation.AddNewSegmentInterpretation(gid, Res.GetString("3d616930-9167-4357-9816-15b88e928673", "Customs Item Number"), gid.GoodsItemNumber);

			#endregion

			#region Classification Number

			var cst = group12.CST.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateCST(cst, detailLine.ClassificationNumber, CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);
			cst.CustomsIdentityCodes1.CodeListIdentificationCode = CodeListIdentificationCodeList.HarmonizedSystem;
			interpretation.AddNewSegmentInterpretation(cst, () => detailLine.ClassificationNumber);

			#endregion

			#region Quantity

			if (!detailLine.Quantity.IsEmpty)
			{
				var mea = group12.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateMEA(mea, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, detailLine.Quantity, detailLine.UnitOfMeasure);

				var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
				meaInterpretation.AddElementInterpretation(() => detailLine.Quantity, mea.ValueRange.MeasurementValue);
				meaInterpretation.AddElementInterpretation(() => detailLine.UnitOfMeasure, mea.ValueRange.MeasurementUnitCode);
			}

			#endregion

			#region Customs Value

			var moa = group12.MOA.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.CustomsValue, detailLine.CustomsValue, detailLine.CurrencyCode, 2);

			var moaInterpretation = interpretation.AddNewSegmentInterpretation(moa);
			moaInterpretation.AddElementInterpretation(() => detailLine.CustomsValue, moa.MonetaryAmount.MonetaryAmount);
			moaInterpretation.AddElementInterpretationIfNotEmpty(() => detailLine.CurrencyCode);

			#endregion
		}

		#endregion

		#endregion
	}
}
