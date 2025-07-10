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
	public static class TIRMessageTextBuilder
	{
		public static void PopulateCUSDECMessage(CUSDECMessage message, ITIRMessageDataProvider source)
		{
			CommonMessageTextBuilder.PopulateUNHSegment(message.UNH[0], MessageReleaseNumberList.TrialRelease1992, "TIR002");
			CommonMessageTextBuilder.PopulateBGMSegment(message.BGM[0], DocumentMessageNameCodedList.GetFromString("TIR"), source.LocalReferenceNumber, MessageFunctionCodedList.Original);
			PopulateCSTSegment(message.CST[0], source);
			PopulateLOCSegments(message.LOC, source);
			PopulateGISSegments(message.GIS, source);
			PopulateEQDSegments(message.EQD, source);
			PopulateSELSegments(message.SEL, source);
			PopulateFTXSegments(message.FTX, source);
			PopulateSG1Group(message.Group1, source);
			PopulateSG4Groups(message.Group4, source);
			PopulateSG6Groups(message.Group6, source);
			CommonMessageTextBuilder.PopulateUNS1Segment(message.UNS1[0]);
			PopulateSG30Group(message.Group30, source);
			CommonMessageTextBuilder.PopulateUNS2Segment(message.UNS2[0]);
			PopulateCNTSegments(message.CNT, source);
			CommonMessageTextBuilder.PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}

		public static void AddToMessageIfSecurityDeclaration(ITIRMessageDataProvider source, Action addSegmentAction)
		{
			if (source.SecurityDeclaration)
			{
				addSegmentAction();
			}
		}

		static void PopulateCSTSegment(CSTSegment cstSegment, ITIRMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes3.CustomsCodeIdentification = "TIR";
			cstSegment.CustomsIdentityCodes3.CodeListQualifier = CodeListQualifierList.CustomsTransitType;
			cstSegment.CustomsIdentityCodes3.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.CustomsProcedureCategory5;
			cstSegment.CustomsIdentityCodes5.CodeListQualifier = CodeListQualifierList.CustomsOffice;
			cstSegment.CustomsIdentityCodes5.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
		}

		static void PopulateLOCSegments(LOCSegmentMessageSection locSection, ITIRMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfOrigin, PlaceLocationQualifierList.CountryOfExportationDespatch, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+35
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfDestination, PlaceLocationQualifierList.CountryOfUltimateDestination, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+36
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.LocationOfGoodsExamCustomsOffice, PlaceLocationQualifierList.PlaceOfCustomsExamination, source.LocationOfGoodsExam, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); //LOC+43
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsOfficeOfTransit.CustomsTransitOfficeState, PlaceLocationQualifierList.CustomsOfficeOfDestinationTransit, source.CustomsOfficeOfTransit.CustomsTransitOfficeCode, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+45
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CodeOfLoadingLocation, PlaceLocationQualifierList.PlacePortOfLoading); //LOC+9
			AddToMessageIfSecurityDeclaration(source, () => CommonMessageTextBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.FullTrackLoadingOrUnloading, source.CodeOfUnloadingLocation)); //LOC+58
		}

		static void PopulateGISSegments(GISSegmentMessageSection gisSection, ITIRMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, source.GoodsInContainerIndicator, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, source.SecurityDeclaration, CodeListQualifierList.GetFromString("187"), CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
		}

		static void PopulateEQDSegments(EQDSegmentMessageSection eqdSection, ITIRMessageDataProvider source)
		{
			foreach (var country in source.CountryCodes)
			{
				AddToMessageIfSecurityDeclaration(source, () => CommonMessageTextBuilder.AddNewEQDSegment(eqdSection, EquipmentQualifierList.Chassis, country));
			}
		}

		static void PopulateSELSegments(SELSegmentMessageSection selSection, ITIRMessageDataProvider source)
		{
			foreach (var seal in source.SealCodes)
			{
				CommonMessageTextBuilder.AddNewSELSegment(selSection, source.SealCodes.Count, SealingPartyCodedList.Carrier, seal);
			}
		}

		static void PopulateFTXSegments(FTXSegmentMessageSection ftxSection, ITIRMessageDataProvider source)
		{
			AddToMessageIfSecurityDeclaration(source, () =>
			{
				CommonMessageTextBuilder.AddNewFTXSegment(ftxSection, source.TransportMethodOfPayment);
				CommonMessageTextBuilder.AddNewFTXSegmentTextLiteral(ftxSection, source.ConveyanceReferenceNumber.Left(35));
			});
		}

		#region SG1 Population

		static void PopulateSG1Group(SegmentGroup1MessageSection sg1Section, ITIRMessageDataProvider source)
		{
			var carnetNumber = source.TIRCarnetNumber;
			AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.TransitOnwardCarriageGuaranteeBondNumber, !carnetNumber.IsEmpty ? (NoResString)"b" + carnetNumber : string.Empty); // RFF+ABK Need to add a b to the beginning of the referenceNumber
			AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.QuotaNumber, source.ReferenceNumber); //RFF+ABJ
			AddToMessageIfSecurityDeclaration(source, () => AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.GetFromString("AJK"), source.SpecificCircumstancesIndicator)); //RFF+AJK
		}

		static void AddNewRFFInSG1Group(SegmentGroup1MessageSection sg1Section, ReferenceQualifierList referenceQualifier, ZString referenceNumber)
		{
			if (!referenceNumber.IsEmpty)
			{
				var sg1Segment = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
				CommonMessageTextBuilder.AddNewRFFSegment(sg1Segment.RFF, referenceQualifier, referenceNumber);
			}
		}

		#endregion

		#region SG4 Population

		static void PopulateSG4Groups(SegmentGroup4MessageSection sg4Section, ITIRMessageDataProvider source)
		{
			var loadingTransport = source.LoadingTransport;
			var borderTransportMode = source.BorderTransportMode;

			var borderTransportId = borderTransportMode?.TransportId ?? ZString.Empty;

			CommonMessageTextBuilder.AddNewSG4Group(sg4Section, TransportStageQualifierList.OnCarriageTransport, ZString.Empty, ZString.Empty, loadingTransport.TransportNationality, loadingTransport.TransportId);
			if (borderTransportMode != null && !borderTransportId.IsEmpty)
			{
				CommonMessageTextBuilder.AddNewSG4Group(sg4Section, TransportStageQualifierList.AtBorder, borderTransportMode.TransportMode, ZString.Empty, borderTransportMode.TransportNationality, borderTransportId);
			}
		}

		#endregion

		#region SG6 Population

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, ITIRMessageDataProvider source)
		{
			var responsibleAgency = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.Exporter, responsibleAgency, source.Consignor); //NAD+EX
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.Consignee, responsibleAgency, source.Consignee); //NAD+CN
			CommonMessageTextBuilder.AddNewNADWithEmail(sg6.NAD, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.Declarant); //NAD+Declarant
			CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.TransitPrincipal, responsibleAgency, source.Holder); //NAD+AF
			AddToMessageIfSecurityDeclaration(source, () =>
			{
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.GetFromString("GA"), responsibleAgency, source.SecurityCarrier); //NAD+GA
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.GetFromString("GL"), responsibleAgency, source.SecurityConsignor); //NAD+GL
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.UltimateConsignee, responsibleAgency, source.SecurityConsignee); //NAD+UC
			});
		}

		#endregion

		#region SG30 Population

		static void PopulateSG30Group(SegmentGroup30MessageSection sg30Section, ITIRMessageDataProvider source)
		{
			foreach (ITIRLine line in source.Lines)
			{
				AddNewSG30Group(sg30Section, line, source);
			}
		}

		static void AddNewSG30Group(SegmentGroup30MessageSection sg30Section, ITIRLine source, ITIRMessageDataProvider headerSource)
		{
			var sg30 = sg30Section.InstantiateAChildAndAddItToChildrenCollection();
			AddCSTSegmentOnSG30(sg30.CST[0], source);
			CommonMessageTextBuilder.AddFTXSegmentForGoodsDescription(sg30, TextSubjectQualifierList.GoodsDescription, source.GoodsDescription.Left(MessageSchema.NctsMessageSchema.GoodsDescriptionMaxLengthDepartureAndTIR), 70);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.GrossWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 0, MeasurementDimensionCodedList.GrossWeight); //Decimals should be rounded before
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.OtherUnitsNumber, source.OtherUnitsQualifier, MeasurementApplicationQualifierList.Measurement);
			CommonMessageTextBuilder.AddNewTDTSegmentOnSG30(sg30, source.DangerousGoodsCode, TransportStageQualifierList.PreCarriageTransport);
			CommonMessageTextBuilder.AddNewSG31Group(sg30.Group31, source.ExternalPackages, PackagingLevelCodedList.Outer);
			AddNewSG31Group(sg30.Group31, source.InternalPackages, PackagingLevelCodedList.Inner);
			if (!source.DocumentReferenceNumber.IsEmpty)
			{
				var sg35 = sg30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				CommonMessageTextBuilder.AddNewRFFSegment(sg35.RFF, ReferenceQualifierList.GetFromString(source.DocumentTypeCode), source.DocumentReferenceNumber, source.DocumentLineNo, source.DocumentClass);
			}
			AddNewSG37Groups(sg30.Group37, source, headerSource);
		}

		static void AddCSTSegmentOnSG30(CSTSegment cstSegment, ITIRLine source)
		{
			cstSegment.GoodsItemNumber = Utilities.FormatNumberFromZIntNational(source.GoodsItemNumber, 0);
			if (!source.GoodsCustomsProcedureCategory1.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory1;
				cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.Commodity;
				cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			}
		}

		#region SG31 Population

		static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, ITIRInternalPackagesInfo internalPackages, PackagingLevelCodedList packagingLevel)
		{
			if (internalPackages != null && internalPackages.Packages.Any())
			{
				var (pacSegment, sg32) = CommonMessageTextBuilder.SetPACSegmentAndAddNewSG32GroupForInternal(sg31Section, packagingLevel);
				if (internalPackages.IsVehiclePackage)
				{
					SetInternalPackagesWhenVehicles(sg32, pacSegment, internalPackages);
				}
				else if (internalPackages.Packages.Skip(1).Any())
				{
					CommonMessageTextBuilder.SetMultipleInternalPackages(sg32, internalPackages);
				}
				else
				{
					CommonMessageTextBuilder.SetSingleInternalPackage(sg32, pacSegment, internalPackages);
				}
			}
		}

		static void SetInternalPackagesWhenVehicles(SegmentGroup32 sg32, PACSegment pacSegment, IInternalPackagesInfoCommon internalPackages)
		{
			var package = internalPackages.Packages.First();
			pacSegment.NumberOfPackages = Utilities.FormatNumberFromZLongNational(package.NumberOfElements, 0);
			pacSegment.PackageType.TypeOfPackagesIdentification = ContainerPackageStatusCodedList.GetFromString(package.ElementsType);

			var packageTags = internalPackages.Packages.Select(x => x.Tag).ToArray();
			var packagesAmount = packageTags.Length;

			for (int i = 0; i < packagesAmount; i += 2)
			{
				var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
				pciSegment.MarksLabels.ShippingMarks1 = packageTags[i];
				if (packagesAmount > (i + 1))
				{
					pciSegment.MarksLabels.ShippingMarks2 = packageTags[i + 1];
				}
			}
		}

		#endregion

		static void AddNewSG37Groups(SegmentGroup37MessageSection sg37Section, ITIRLine source, ITIRMessageDataProvider headerSource)
		{
			var sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
			CommonMessageTextBuilder.AddNewDOCSegment(sg37.DOC, "N952", headerSource.TIRCarnetNumber, headerSource.TIRCarnetExpiryDate.ToCustomsFormatDateStringddMMyyyy(), ZString.Empty);

			foreach (var doc in source.Documents)
			{
				CommonMessageTextBuilder.AddNewDOCSegment(sg37.DOC, doc.Name, doc.Number, ZString.Empty, ZString.Empty);
			}
		}

		#endregion

		static void PopulateCNTSegments(CNTSegmentMessageSection cntSection, ITIRMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZIntNational(source.TotalNumberOfGoods, 0), ControlQualifierList.NumberOfCustomsItemDetailLines);
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZLongNational(source.TotalNumberOfPackageElements, 0), ControlQualifierList.TotalNumberOfPackages);
		}
	}
}
