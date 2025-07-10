using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	internal static class DepartureMessageTextBuilder
	{
		internal static void PopulateCUSDECMessage(CUSDECMessage message, IDepartureMessageDataProvider source)
		{
			CommonMessageTextBuilder.PopulateUNHSegment(message.UNH[0], MessageReleaseNumberList.TrialRelease1992, "TEX011");
			CommonMessageTextBuilder.PopulateBGMSegment(message.BGM[0], DocumentMessageNameCodedList.GetFromString("969"), source.LocalReferenceNumber, MessageFunctionCodedList.Original);
			PopulateCSTSegment(message.CST[0], source);
			PopulateLOCSegments(message.LOC, source);
			PopulateGISSegments(message.GIS, source);
			PopulateEQDSegments(message.EQD, source);
			PopulateSELSegments(message.SEL, source);
			PopulateFTXSegment(message.FTX, source);
			PopulateSG1Group(message.Group1, source);
			PopulateSG4Groups(message.Group4, source);
			PopulateSG6Groups(message.Group6, source);
			AddNewMOAInSG8Group(message.Group8);
			CommonMessageTextBuilder.PopulateUNS1Segment(message.UNS1[0]);
			PopulateSG30Group(message.Group30, source);
			CommonMessageTextBuilder.PopulateUNS2Segment(message.UNS2[0]);
			PopulateCNTSegments(message.CNT, source);
			CommonMessageTextBuilder.PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}
		public static void AddToMessageIfSecurityDeclaration(IDepartureMessageDataProvider source, Action addSegmentAction)
		{
			if (source.SecurityDeclaration)
			{
				addSegmentAction();
			}
		}

		#region Fields Population

		static void PopulateCSTSegment(CSTSegment cstSegment, IDepartureMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes3.CustomsCodeIdentification = source.CustomsProcedureCategory3;
			cstSegment.CustomsIdentityCodes3.CodeListQualifier = CodeListQualifierList.CustomsTransitType;
			cstSegment.CustomsIdentityCodes3.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.CustomsProcedureCategory5;
			cstSegment.CustomsIdentityCodes5.CodeListQualifier = CodeListQualifierList.CustomsOffice;
			cstSegment.CustomsIdentityCodes5.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
		}

		static void PopulateLOCSegments(LOCSegmentMessageSection locSection, IDepartureMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfDeparture, PlaceLocationQualifierList.CountryOfExportationDespatch, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);//LOC+35
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfDestination, PlaceLocationQualifierList.CountryOfUltimateDestination, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); // LOC+36
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.LocationOfGoodsExamCustomsOffice, PlaceLocationQualifierList.PlaceOfCustomsExamination, source.LocationOfGoodsExam, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); // LOC+43
			foreach (var office in source.CustomsOfficesOfTransit)
			{
				CommonMessageTextBuilder.AddNewLOCSegment(locSection, office.CustomsTransitOfficeState, PlaceLocationQualifierList.CustomsOfficeOfTransit, office.CustomsTransitOfficeCode, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			}//LOC+50
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsOfficeOfDestination.CustomsTransitOfficeState, PlaceLocationQualifierList.CustomsOfficeOfDestinationTransit, source.CustomsOfficeOfDestination.CustomsTransitOfficeCode, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+45
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CodeOfLoadingLocation, PlaceLocationQualifierList.PlacePortOfLoading);//LOC+9
			AddToMessageIfSecurityDeclaration(source, () => CommonMessageTextBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.FullTrackLoadingOrUnloading, source.CodeOfUnloadingLocation)); //LOC+58
		}

		static void PopulateGISSegments(GISSegmentMessageSection gisSection, IDepartureMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, source.GoodsInContainerIndicator, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, source.SecurityDeclaration, CodeListQualifierList.GetFromString("187"), CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
		}

		static void PopulateEQDSegments(EQDSegmentMessageSection eqdSection, IDepartureMessageDataProvider source)
		{
			foreach (var country in source.CountryCodes)
			{
				AddToMessageIfSecurityDeclaration(source, () => CommonMessageTextBuilder.AddNewEQDSegment(eqdSection, EquipmentQualifierList.Chassis, country));
			}
		}

		static void PopulateSELSegments(SELSegmentMessageSection selSection, IDepartureMessageDataProvider source)
		{
			foreach (var seal in source.SealCodes)
			{
				CommonMessageTextBuilder.AddNewSELSegment(selSection, source.SealCodes.Count, SealingPartyCodedList.Carrier, seal);
			}
		}

		static void PopulateFTXSegment(FTXSegmentMessageSection ftxSection, IDepartureMessageDataProvider source)
		{
			AddToMessageIfSecurityDeclaration(source, () =>
			{
				CommonMessageTextBuilder.AddNewFTXSegment(ftxSection, source.TransportMethodOfPayment);
				CommonMessageTextBuilder.AddNewFTXSegmentTextLiteral(ftxSection, source.ConveyanceReferenceNumber.Left(35));
			});
		}

		#region SG1 Population

		static void PopulateSG1Group(SegmentGroup1MessageSection sg1Section, IDepartureMessageDataProvider source)
		{
			foreach (var guarantee in source.GuaranteeNumbers)
			{
				var rff = AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.TransitOnwardCarriageGuaranteeBondNumber, guarantee.Type); //RFF+ABK
				if (rff != null)
				{
					rff.Reference.LineNumber = guarantee.AccessCode;
				}
			}
			AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.QuotaNumber, source.ReferenceNumber); //RFF+ABJ
			AddToMessageIfSecurityDeclaration(source, () => AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.GetFromString("AJK"), source.SpecificCircumstancesIndicator)); //RFF+AJK
		}
		static RFFSegment AddNewRFFInSG1Group(SegmentGroup1MessageSection sg1Section, ReferenceQualifierList referenceQualifier, ZString referenceNumber)
		{
			RFFSegment result = null;
			if (!referenceNumber.IsEmpty)
			{
				var sg1Segment = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
				result = CommonMessageTextBuilder.AddNewRFFSegment(sg1Segment.RFF, referenceQualifier, referenceNumber);
			}
			return result;
		}

		#endregion

		#region SG4 Population

		static void PopulateSG4Groups(SegmentGroup4MessageSection sg4Section, IDepartureMessageDataProvider source)
		{
			var borderTransportMode = source.BorderTransportMode;
			var transitTransportMedium = source.TransitTransportMedium;

			var borderTransportId = borderTransportMode?.TransportId ?? ZString.Empty;
			var transitTransportId = transitTransportMedium?.TransportId ?? ZString.Empty;

			if (borderTransportMode != null && !borderTransportId.IsEmpty)
			{
				CommonMessageTextBuilder.AddNewSG4Group(sg4Section, TransportStageQualifierList.AtBorder, borderTransportMode.TransportMode, ZString.Empty, borderTransportMode.TransportNationality, borderTransportId);
			}
			if (transitTransportMedium != null && !transitTransportId.IsEmpty)
			{
				CommonMessageTextBuilder.AddNewSG4Group(sg4Section, TransportStageQualifierList.OnCarriageTransport, transitTransportMedium.TransportMode, ZString.Empty, transitTransportMedium.TransportNationality, transitTransportId);
			}
		}

		#endregion

		#region SG6 Population

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, IDepartureMessageDataProvider source)
		{
			var responsibleAgency = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.Exporter, responsibleAgency, source.Consignor); //NAD+EX
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.Consignee, responsibleAgency, source.Consignee); //NAD+CN
			CommonMessageTextBuilder.AddNewNADWithEmail(sg6.NAD, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.Declarant); //NAD+DT
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.TransitPrincipal, responsibleAgency, source.Principal); //NAD+AF
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.TransitPrincipalsAgentRepresentative, responsibleAgency, source.Representative); //NAD+AH
			AddToMessageIfSecurityDeclaration(source, () =>
			{
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.GetFromString("GA"), responsibleAgency, source.SecurityCarrier); //NAD+GA
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.GetFromString("GL"), responsibleAgency, source.SecurityConsignor); //NAD+GL
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.UltimateConsignee, responsibleAgency, source.SecurityConsignee); //NAD+UC
			});
		}

		#endregion

		#region SG8 Population

		static void AddNewMOAInSG8Group(SegmentGroup8MessageSection sg8Section)
		{
			CommonMessageTextBuilder.AddNewMOAInSG8Group(sg8Section, MonetaryAmountTypeQualifierList.MutuallyDefined, 0, Core.Constants.CurrencyCodes.EuropeanUnion);
		}

		#endregion

		#region SG30 Population

		static void PopulateSG30Group(SegmentGroup30MessageSection sg30Section, IDepartureMessageDataProvider source)
		{
			foreach (IDepartureLine line in source.Lines)
			{
				AddNewSG30Group(sg30Section, line);
			}
		}

		static void AddNewSG30Group(SegmentGroup30MessageSection sg30Section, IDepartureLine source)
		{
			var sg30 = sg30Section.InstantiateAChildAndAddItToChildrenCollection();
			AddCSTSegmentOnSG30(sg30.CST[0], source);
			CommonMessageTextBuilder.AddFTXSegmentForGoodsDescription(sg30, TextSubjectQualifierList.GoodsDescription, source.GoodsDescription.Left(MessageSchema.NctsMessageSchema.GoodsDescriptionMaxLengthDepartureAndTIR), 70);
			CommonMessageTextBuilder.AddNewLOCSegment(sg30.LOC, source.CountryOfDeparture, PlaceLocationQualifierList.CountryOfExportationDespatch, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);//LOC+35
			CommonMessageTextBuilder.AddNewLOCSegment(sg30.LOC, source.CountryOfDestination, PlaceLocationQualifierList.CountryOfUltimateDestination, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+36
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.GrossWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 0, MeasurementDimensionCodedList.GrossWeight); //Decimals should be rounded before
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.NetWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.NetNetWeight);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.FiscalUnitsNumber, source.FiscalUnitsQualifier, MeasurementApplicationQualifierList._3rdSpecifiedTariffQuantity);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.OtherUnitsNumber, source.OtherUnitsQualifier, MeasurementApplicationQualifierList.Measurement);
			CommonMessageTextBuilder.AddNewNADWithAddress(sg30.NAD, PartyQualifierList.Exporter, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.GoodsConsignor); //NAD+EX
			CommonMessageTextBuilder.AddNewNADWithAddress(sg30.NAD, PartyQualifierList.Consignee, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.GoodsConsignee); //NAD+CN
			CommonMessageTextBuilder.AddNewNADWithAddress(sg30.NAD, PartyQualifierList.GetFromString("GM"), CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.SecurityGoodsConsignor); //NAD+GM
			CommonMessageTextBuilder.AddNewNADWithAddress(sg30.NAD, PartyQualifierList.DeliveryParty, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.SecurityGoodsConsignee); //NAD+DP
			CommonMessageTextBuilder.AddNewTDTSegmentOnSG30(sg30, source.DangerousGoodsCode, TransportStageQualifierList.PreCarriageTransport);
			CommonMessageTextBuilder.AddNewSG31Group(sg30.Group31, source.ExternalPackages, PackagingLevelCodedList.Outer);
			AddNewSG31Group(sg30.Group31, source.InternalPackages, PackagingLevelCodedList.Inner);
			CommonMessageTextBuilder.AddNewSG31Group(sg30.Group31, source.VehiclePackages, PackagingLevelCodedList.GetFromString("4"));
			CommonMessageTextBuilder.AddNewMOASegmentSG33Group(sg30.Group33, MonetaryAmountTypeQualifierList.StatisticalValue, source.TotalGoodValueInEuros, true);
			if (!source.DocumentReferenceNumber.IsEmpty)
			{
				var sg35 = sg30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				CommonMessageTextBuilder.AddNewRFFSegment(sg35.RFF, ReferenceQualifierList.GetFromString(source.DocumentTypeCode), source.DocumentReferenceNumber, source.DocumentLineNo, source.DocumentClass);
			}
			AddNewSG37Groups(sg30.Group37, source);
			AddNewSG38Groups(sg30.Group38, source);
		}

		static void AddCSTSegmentOnSG30(CSTSegment cstSegment, IDepartureLine source)
		{
			cstSegment.GoodsItemNumber = Utilities.FormatNumberFromZIntNational(source.GoodsItemNumber, 0);
			cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory1;
			cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.Commodity;
			cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;

			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory5;
			cstSegment.CustomsIdentityCodes5.CodeListQualifier = CodeListQualifierList.CustomsPreference;
			cstSegment.CustomsIdentityCodes5.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;
		}

		public static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, IDepartureInternalPackagesInfo internalPackages, PackagingLevelCodedList packagingLevel)
		{
			if (internalPackages != null && internalPackages.Packages.Any())
			{
				var (pacSegment, sg32) = CommonMessageTextBuilder.SetPACSegmentAndAddNewSG32GroupForInternal(sg31Section, packagingLevel);
				if (internalPackages.Packages.Skip(1).Any() || internalPackages.IsVehiclePackage)
				{
					CommonMessageTextBuilder.SetMultipleInternalPackages(sg32, internalPackages);
				}
				else
				{
					CommonMessageTextBuilder.SetSingleInternalPackage(sg32, pacSegment, internalPackages);
				}
			}
		}

		static void AddNewSG37Groups(SegmentGroup37MessageSection sg37Section, IDepartureLine source)
		{
			foreach (var doc in source.Documents)
			{
				var sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
				CommonMessageTextBuilder.AddNewDOCSegment(sg37.DOC, doc.Name, doc.Number, doc.Source, ZString.Empty);
			}
		}

		static void AddNewSG38Groups(SegmentGroup38MessageSection sg7Section, IDepartureLine source)
		{
			var sg37 = sg7Section.InstantiateAChildAndAddItToChildrenCollection();
			AddNewSG7Group(sg37.TOD, source.GoodsTransportMethodOfPayment, source.GoodsCountryCode);
		}

		static void AddNewSG7Group(TODSegmentMessageSection todSection, ZString methodOfPayment, ZString country)
		{
			if (!methodOfPayment.IsEmpty)
			{
				var todSegment = todSection.InstantiateAChildAndAddItToChildrenCollection();
				todSegment.TermsOfDeliveryFunctionCoded = TermsOfDeliveryFunctionCodedList.DespatchCondition;
				todSegment.TransportChargesMethodOfPaymentCoded = TransportChargesMethodOfPaymentCodedList.GetFromString(methodOfPayment);
				if (country.IsEmpty)
				{
					todSegment.TermsOfDelivery.TermsOfDeliveryCoded = "1";
				}
				else
				{
					todSegment.TermsOfDelivery.CodeListQualifier = CodeListQualifierList.GetFromString(country);
				}
			}
		}

		#endregion

		static void PopulateCNTSegments(CNTSegmentMessageSection cntSection, IDepartureMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZIntNational(source.TotalNumberOfGoods, 0), ControlQualifierList.NumberOfCustomsItemDetailLines);
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZLongNational(source.TotalNumberOfPackageElements, 0), ControlQualifierList.TotalNumberOfPackages);
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, source.NationalSimplificationIndicator, ControlQualifierList.NumberOfLoadingLists);
		}

		#endregion
	}
}
