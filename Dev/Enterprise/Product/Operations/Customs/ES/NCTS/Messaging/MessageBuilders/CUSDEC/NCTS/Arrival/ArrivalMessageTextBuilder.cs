using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Edifact.Utilities;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	internal static class ArrivalMessageTextBuilder
	{
		internal static void PopulateCUSDECMessage(CUSDECMessage message, IArrivalMessageDataProvider source)
		{
			CommonMessageTextBuilder.PopulateUNHSegment(message.UNH[0], MessageReleaseNumberList.Release1996B, source.DocumentMessageName + "006");
			CommonMessageTextBuilder.PopulateBGMSegment(message.BGM[0], DocumentMessageNameCodedList.GetFromString(source.DocumentMessageName), source.LocalReferenceNumber, MessageFunctionCodedList.Original);

			if (source.IsArrivalWithTNN)
			{
				PopulateCSTSegment(message.CST[0], source);
			}

			PopulateLOCSegments(message.LOC, source);
			PopulateDTMSegments(message.DTM, source);
			PopulateGISSegments(message.GIS, source);
			PopulateSELSegments(message.SEL, source);
			PopulateSG1Group(message.Group1, source);
			PopulateSG4Groups(message.Group4, source);
			PopulateSG6Groups(message.Group6, source);
			PopulateSG7Group(message.Group7, source);
			PopulateSG8Group(message.Group8, source);
			CommonMessageTextBuilder.PopulateUNS1Segment(message.UNS1[0]);
			PopulateSG30Group(message.Group30, source);
			CommonMessageTextBuilder.PopulateUNS2Segment(message.UNS2[0]);
			PopulateCNTSegments(message.CNT, source);
			CommonMessageTextBuilder.PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}

		const int TextLiteralFreeTextMaxLength = 70;

		static void PopulateCSTSegment(CSTSegment cstSegment, IArrivalMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.CustomsProcedureCategory1;
			cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.CustomsTransitType;
			cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = source.CustomsProcedureCategory2;
			cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsOffice;
			cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;
		}

		static void PopulateLOCSegments(LOCSegmentMessageSection locSection, IArrivalMessageDataProvider source)
		{
			if (source.IsArrivalWithTNN)
			{
				CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsTransitDestinationOffice, PlaceLocationQualifierList.CustomsOfficeOfDestinationTransit, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+45
			}
			if (source.IsOnlyUnloadingRemarks)
			{
				CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsOfficesOfDestination.CustomsDestinationOfficeCode, PlaceLocationQualifierList.PlaceOfDestination, ZString.Empty, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); //LOC+8
			}
			else
			{
				CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsOfficesOfDestination.CustomsDestinationOfficeCode, PlaceLocationQualifierList.PlaceOfDestination, source.CustomsOfficesOfDestination.CustomsDestinationLocationCode, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); //LOC+8
			}
			if (source.IsArrivalWithTNN)
			{
				CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfDestination, PlaceLocationQualifierList.CountryOfUltimateDestination, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); //LOC+36
			}
		}

		static void PopulateDTMSegments(DTMSegmentMessageSection dtmSection, IArrivalMessageDataProvider source)
		{
			if (source.IsArrivalWithAVI)
			{
				CommonMessageTextBuilder.AddNewDTMSegment(dtmSection, source.DateOfArrival.ToCustomsFormatDateString(), DateTimePeriodQualifierList.GoodsReceiptDateTime, DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
			if (source.IsArrivalWithOBS)
			{
				CommonMessageTextBuilder.AddNewDTMSegment(dtmSection, source.DateOfUnloading.ToCustomsFormatDateString(), DateTimePeriodQualifierList.DeliveryDateTimeActual, DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		static void PopulateGISSegments(GISSegmentMessageSection gisSection, IArrivalMessageDataProvider source)
		{
			if (source.IsArrivalWithTNN)
			{
				AddNewGISSegment(gisSection, source.GoodsInContainerIndicator, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			}
			if (source.IsArrivalWithOBS)
			{
				AddNewGISSegment(gisSection, source.UnloadingComplianceIndicator, CodeListQualifierList.CustomsSpecialCodes, CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
				AddNewGISSegment(gisSection, source.SealsInGoodState, CodeListQualifierList.CustomsStatusOfGoods, CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
			}
			if (source.IsArrivalWithAVI)
			{
				AddNewGISSegment(gisSection, source.AttachedDocumentTypeA, CodeListQualifierList.BusinessFunction, CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
				AddNewGISSegment(gisSection, source.ReceiverComplianceForAutoDischarge, CodeListQualifierList.GetFromString("63"), CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
				AddNewGISSegment(gisSection, source.DirectlyLoadedOnCompletion, CodeListQualifierList.GetFromString("181"), CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
			}
			if (source.IsArrivalWithOBS)
			{
				foreach (var discrepancy in source.SealsStateDiscrepancies)
				{
					AddNewGISSegment(gisSection, discrepancy, CodeListQualifierList.GetFromString("158"), CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
				}
			}
			if (source.IsArrivalWithAVI)
			{
				AddNewGISSegment(gisSection, source.TIRCompletionNumber, CodeListQualifierList.GetFromString("215"), CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
				AddNewGISSegment(gisSection, source.TIRIsCompleteUnloading, CodeListQualifierList.GetFromString("218"), CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
			}
		}

		static void AddNewGISSegment(GISSegmentMessageSection gisSection, ZBool indicatorCode, CodeListQualifierList indicatorQualifier, CodeListResponsibleAgencyCodedList responsibleAgency)
		{
			if (indicatorCode)
			{
				var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
				gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString("U");
				gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
				gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
			}
		}

		static void AddNewGISSegment(GISSegmentMessageSection gisSection, ZString indicatorCode, CodeListQualifierList indicatorQualifier, CodeListResponsibleAgencyCodedList responsibleAgency)
		{
			if (!indicatorCode.IsEmpty)
			{
				var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
				gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString(indicatorCode);
				gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
				gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
			}
		}

		static void PopulateSELSegments(SELSegmentMessageSection selSection, IArrivalMessageDataProvider source)
		{
			if (source.IsArrivalWithTNN)
			{
				foreach (var seal in source.SealCodes)
				{
					CommonMessageTextBuilder.AddNewSELSegment(selSection, source.SealCodes.Count, SealingPartyCodedList.Carrier, seal);
				}
			}
			if (source.IsArrivalWithOBS)
			{
				foreach (var seal in source.SealCodesWithDiscrepancies) //Must be the same number as the sealsStateDiscrepancies declared in GIS
				{
					CommonMessageTextBuilder.AddNewSELSegment(selSection, source.SealCodesWithDiscrepancies.Count, SealingPartyCodedList.GetFromString("AB"), seal);
				}
			}
		}

		#region SG1 Population

		static void PopulateSG1Group(SegmentGroup1MessageSection sg1Section, IArrivalMessageDataProvider source)
		{
			AddNewSG1Group(sg1Section, source.ReferenceGroup, source.IsArrivalWithAVI);
		}

		static void AddNewSG1Group(SegmentGroup1MessageSection sg1Section, IArrivalReferencesGroup refGroup, ZBool isArrivalWithAVI)
		{
			var sg1Segment = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
			CommonMessageTextBuilder.AddNewRFFSegment(sg1Segment.RFF, ReferenceQualifierList.GoodsDeclarationNumber, refGroup.TransitNumber);
			if (isArrivalWithAVI)
			{
				if (!refGroup.PreviousSummaryDeclarationNumber.IsEmpty)
				{
					CommonMessageTextBuilder.AddNewRFFSegment(sg1Segment.RFF, ReferenceQualifierList.CustomsDeclarationNumber, refGroup.PreviousSummaryDeclarationNumber);
				}
				foreach (var rEvent in refGroup.RouteEvents)
				{
					var sg1SegmentEvent = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
					CommonMessageTextBuilder.AddNewRFFSegment(sg1SegmentEvent.RFF, ReferenceQualifierList.GetFromString("AIV"), rEvent.EventPlace, rEvent.EventPlaceLanguage, ZString.Empty);
					var sg2Segment = sg1SegmentEvent.Group2.InstantiateAChildAndAddItToChildrenCollection();
					AddNewPACSegment(sg2Segment.PAC, rEvent.EventCountry, "2");
					var sg2SegmentEvent = sg1SegmentEvent.Group2.InstantiateAChildAndAddItToChildrenCollection();

					#region Incident

					AddNewPACSegment(sg2SegmentEvent.PAC, rEvent.IncidentInEvent);
					var incidentFormData = rEvent.IncidentFormData;
					if (rEvent.IncidentInEvent && incidentFormData != null)
					{
						var sg3SegmentIncident = sg2SegmentEvent.Group3.InstantiateAChildAndAddItToChildrenCollection();
						AddNewPCISegment(sg3SegmentIncident.PCI, incidentFormData, MarkingInstructionsCodedList.CarriersInstructions);//18
						AddNewFTXSegment(sg3SegmentIncident.FTX, incidentFormData.FormText, incidentFormData.FormTextLanguage, TextSubjectQualifierList.OnwardRoutingInformation);
					}

					#endregion

					#region Seals

					var sg2SegmentSeals = sg1SegmentEvent.Group2.InstantiateAChildAndAddItToChildrenCollection();
					AddNewPACSegment(sg2SegmentSeals.PAC, rEvent.NewSealsInEventNum, "4");
					if (!rEvent.NewSealsInEventNum.IsEmpty && rEvent.NewSealsInformation.Any())
					{
						var sg3SegmentSeals = sg2SegmentSeals.Group3.InstantiateAChildAndAddItToChildrenCollection();
						foreach (var sealData in rEvent.NewSealsInformation)
						{
							AddNewPCISegment(sg3SegmentSeals.PCI, sealData);//21
						}
					}

					#endregion

					#region Transport

					var sg2SegmentTransp = sg1SegmentEvent.Group2.InstantiateAChildAndAddItToChildrenCollection();
					AddNewPACSegment(sg2SegmentTransp.PAC, rEvent.NewTransportNationality, "5");
					if (!rEvent.NewTransportNationality.IsEmpty)
					{
						var sg3SegmentTransfer = sg2SegmentTransp.Group3.InstantiateAChildAndAddItToChildrenCollection();
						var transferFormData = rEvent.TransferFormData;
						if (transferFormData != null)
						{
							AddNewPCISegment(sg3SegmentTransfer.PCI, transferFormData, MarkingInstructionsCodedList.EntireShipment);//23
							AddNewFTXSegment(sg3SegmentTransfer.FTX, transferFormData.FormText, transferFormData.FormTextLanguage, TextSubjectQualifierList.TransportationInformation);
						}
						foreach (var contId in rEvent.NewContainerIDs)
						{
							var sg3SegmentTransferContainer = sg2SegmentTransp.Group3.InstantiateAChildAndAddItToChildrenCollection();
							AddNewPCISegment(sg3SegmentTransferContainer.PCI, contId);
						}
					}

					#endregion
				}
			}
		}

		static void AddNewPACSegment(PACSegmentMessageSection pacSection, ZString value, ZString numberPackages)
		{
			if (numberPackages == "2" || numberPackages == "4")
			{
				if (!value.IsEmpty)
				{
					var pacSegment = pacSection.InstantiateAChildAndAddItToChildrenCollection();
					pacSegment.NumberOfPackages = numberPackages;
					pacSegment.PackageTypeIdentification.ItemDescriptionTypeCoded = ItemDescriptionTypeCodedList.GetFromString(value);
				}
			}
			else if (numberPackages == "5")
			{
				if (!value.IsEmpty)
				{
					var pacSegment = pacSection.InstantiateAChildAndAddItToChildrenCollection();
					pacSegment.NumberOfPackages = numberPackages;
					pacSegment.PackageType.TypeOfPackages = value;
				}
			}
		}

		static void AddNewPACSegment(PACSegmentMessageSection pacSection, ZBool incidentInEvent)
		{
			if (incidentInEvent)
			{
				var pacSegment = pacSection.InstantiateAChildAndAddItToChildrenCollection();
				pacSegment.NumberOfPackages = "3";
				pacSegment.PackagingDetails.PackagingLevelCoded = PackagingLevelCodedList.Inner;
			}
		}

		static void AddNewPCISegment(PCISegmentMessageSection pciSection, IArrivalFormData formData, MarkingInstructionsCodedList markingInstruction)
		{
			var pciSegment = pciSection.InstantiateAChildAndAddItToChildrenCollection();
			pciSegment.MarkingInstructionsCoded = markingInstruction;
			pciSegment.MarksLabels.ShippingMarks1 = formData.FormDate.ToCustomsFormatDateString();
			pciSegment.MarksLabels.ShippingMarks2 = formData.FormAuthority;
			pciSegment.MarksLabels.ShippingMarks3 = formData.FormAuthorityLanguage;
			pciSegment.MarksLabels.ShippingMarks4 = formData.FormLocation;
			pciSegment.MarksLabels.ShippingMarks5 = formData.FormLocationLanguage;
			pciSegment.MarksLabels.ShippingMarks6 = formData.FormCountry;
		}

		static void AddNewPCISegment(PCISegmentMessageSection pciSection, IArrivalNewSealsInformation sealData)
		{
			if (sealData != null)
			{
				var pciSegment = pciSection.InstantiateAChildAndAddItToChildrenCollection();
				pciSegment.MarkingInstructionsCoded = MarkingInstructionsCodedList.LineItemOnly;
				pciSegment.MarksLabels.ShippingMarks1 = sealData.SealId;
				pciSegment.MarksLabels.ShippingMarks2 = sealData.SealIdLanguage;
			}
		}

		static void AddNewPCISegment(PCISegmentMessageSection pciSection, ZString containerId)
		{
			if (!containerId.IsEmpty)
			{
				var pciSegment = pciSection.InstantiateAChildAndAddItToChildrenCollection();
				pciSegment.MarkingInstructionsCoded = MarkingInstructionsCodedList.GetFromString("30");
				pciSegment.MarksLabels.ShippingMarks1 = containerId;
			}
		}

		static void AddNewFTXSegment(FTXSegmentMessageSection ftxSection, ZString formText, ZString formTextLanguage, TextSubjectQualifierList textSubjectQualifier)
		{
			if (!formText.IsEmpty)
			{
				var ftxSegment = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
				ftxSegment.TextSubjectQualifier = textSubjectQualifier;
				ftxSegment.LanguageCoded = formTextLanguage;
				var splitter = new TextSplitter(TextLiteralFreeTextMaxLength)
				{
					Text = formText
				};
				ftxSegment.TextLiteral.FreeText1 = splitter[0];
				ftxSegment.TextLiteral.FreeText2 = splitter[1];
				ftxSegment.TextLiteral.FreeText3 = splitter[2];
				ftxSegment.TextLiteral.FreeText4 = splitter[3];
				ftxSegment.TextLiteral.FreeText5 = splitter[4];
			}
		}

		#endregion

		#region SG4 Population

		static void PopulateSG4Groups(SegmentGroup4MessageSection sg4Section, IArrivalMessageDataProvider source)
		{
			var transitTransportMedium = source.TransitTransportMedium;
			if (source.IsArrivalWithAVI && !transitTransportMedium.IsEmpty)
			{
				CommonMessageTextBuilder.AddNewSG4Group(sg4Section, TransportStageQualifierList.OnCarriageTransport, ZString.Empty, transitTransportMedium, source.TransportNationality, source.TransportId);
			}
		}

		#endregion

		#region SG6 Population

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, IArrivalMessageDataProvider source)
		{
			//Must be the coded country (numeric)
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			if (source.IsArrivalWithTNN)
			{
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.Exporter, null, source.Consignor); //NAD+EX
				CommonMessageTextBuilder.AddNewNADWithAddress(sg6.NAD, PartyQualifierList.Consignee, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.Consignee); //NAD+CN
			}
			AddNewNADWithoutAddressInSG6Group(sg6.NAD, source.Declarant, PartyQualifierList.Declarant); //NAD+Declarant
		}

		static void AddNewNADWithoutAddressInSG6Group(NADSegmentMessageSection nadSegSection, IPartyNameProvider addressDetails, PartyQualifierList addressType)
		{
			if (addressDetails != null)
			{
				var nadSegment = nadSegSection.InstantiateAChildAndAddItToChildrenCollection();
				nadSegment.PartyQualifier = addressType;
				nadSegment.PartyIdentificationDetails.PartyIdIdentification = addressDetails.Id;
				nadSegment.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
				nadSegment.PartyName.PartyName1 = addressDetails.Name.Left(35);
			}
		}

		#endregion

		#region SG7 Population

		static void PopulateSG7Group(SegmentGroup7MessageSection sg7Section, IArrivalMessageDataProvider source)
		{
			AddNewSG7Group(sg7Section, source.UnloadingObservations, source.IsArrivalWithOBS);
		}

		static void AddNewSG7Group(SegmentGroup7MessageSection sg7Section, ZString unloadingObservations, ZBool isArrivalWithOBS)
		{
			if (isArrivalWithOBS && !unloadingObservations.IsEmpty)
			{
				var sg7 = sg7Section.InstantiateAChildAndAddItToChildrenCollection();
				var todSegment = sg7.TOD.InstantiateAChildAndAddItToChildrenCollection();
				todSegment.TermsOfDeliveryFunctionCoded = TermsOfDeliveryFunctionCodedList.TransportCondition;

				var ftxSegment = sg7.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.LoadingRemarks;
				var splitter = new TextSplitter(TextLiteralFreeTextMaxLength)
				{
					Text = unloadingObservations
				};
				ftxSegment.TextLiteral.FreeText1 = splitter[0];
				ftxSegment.TextLiteral.FreeText2 = splitter[1];
			}
		}

		#endregion

		#region SG8 Population

		static void PopulateSG8Group(SegmentGroup8MessageSection sg8Section, IArrivalMessageDataProvider source)
		{
			if (!source.IsOnlyArrivalNotification)
			{
				CommonMessageTextBuilder.AddNewMOAInSG8Group(sg8Section, MonetaryAmountTypeQualifierList.MutuallyDefined, 0, source.IsDeclarationInEuros ? Core.Constants.CurrencyCodes.EuropeanUnion : null);
			}
		}

		#endregion

		#region SG30 Population

		static void PopulateSG30Group(SegmentGroup30MessageSection sg30Section, IArrivalMessageDataProvider source)
		{
			if (!source.IsOnlyArrivalNotification)
			{
				foreach (IArrivalLine line in source.Lines)
				{
					AddNewSG30Group(sg30Section, line, source.IsArrivalWithTNN, source.IsArrivalWithOBS);
				}
			}
		}

		static void AddNewSG30Group(SegmentGroup30MessageSection sg30Section, IArrivalLine source, ZBool isArrivalWithTNN, ZBool isArrivalWithOBS)
		{
			var sg30 = sg30Section.InstantiateAChildAndAddItToChildrenCollection();
			AddCSTSegmentOnSG30(sg30.CST, source, isArrivalWithTNN, isArrivalWithOBS);
			CommonMessageTextBuilder.AddFTXSegmentForGoodsDescription(sg30, TextSubjectQualifierList.GoodsDescription, source.GoodsDescription, 70);
			AddFTXSegmentForNotSubmittedC44DocumentsOnSG30(sg30, source.NotSubmittedC44Documents, isArrivalWithOBS);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.GrossWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 0, MeasurementDimensionCodedList.GrossWeight); //Decimals should be rounded before
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.NetWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.NetNetWeight);
			AddNewSG31Group(sg30.Group31, source.ExternalPackages, PackagingLevelCodedList.Outer);
			CommonMessageTextBuilder.AddNewSG31Group(sg30.Group31, source.InternalPackages, PackagingLevelCodedList.Inner);
			CommonMessageTextBuilder.AddNewMOASegmentSG33Group(sg30.Group33, MonetaryAmountTypeQualifierList.StatisticalValue, source.TotalGoodValueInEuros, false);
			AddNewSG37GroupsOnSG30(sg30.Group37, source, isArrivalWithOBS);
		}

		static void AddCSTSegmentOnSG30(CSTSegmentMessageSection cstSection, IArrivalLine source, ZBool isArrivalWithTNN, ZBool isArrivalWithOBS)
		{
			var cstSegment = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cstSegment.GoodsItemNumber = Utilities.FormatNumberFromZIntNational(source.GoodsItemNumber, 0);
			if (!source.GoodsCustomsProcedureCategory1.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory1;
				cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.Commodity;
				cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			}

			if (isArrivalWithTNN && !source.GoodsCustomsProcedureCategory2.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory2;
				cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsPreference;
				cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;
			}

			if (isArrivalWithOBS)
			{
				if (!source.GoodsCustomsProcedureCategory3.IsEmpty)
				{
					cstSegment.CustomsIdentityCodes3.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory3;
					cstSegment.CustomsIdentityCodes3.CodeListQualifier = CodeListQualifierList.MutuallyDefined;
					cstSegment.CustomsIdentityCodes3.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
				}

				if (!source.GoodsCustomsProcedureCategory4.IsEmpty)
				{
					cstSegment.CustomsIdentityCodes4.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory4;
				}

				if (!source.GoodsCustomsProcedureCategory5.IsEmpty)
				{
					cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory5;
				}
			}
		}

		static void AddFTXSegmentForNotSubmittedC44DocumentsOnSG30(SegmentGroup30 sg30, IEnumerable<ZString> c44Documents, ZBool isArrivalWithOBS)
		{
			if (isArrivalWithOBS)
			{
				foreach (var doc in c44Documents)
				{
					var ftxSegment = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
					ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.AcceptanceTermsAdditional;
					ftxSegment.TextReference.FreeTextCoded = "NP";
					ftxSegment.TextLiteral.FreeText1 = doc;
				}
			}
		}

		static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, IExternalPackagesInfoCommon externalPackages, PackagingLevelCodedList packagingLevel)
		{
			if (externalPackages != null && !externalPackages.NumberOfPackages.IsEmpty)
			{
				var sg31 = CommonMessageTextBuilder.SetPACSegmentForExternal(sg31Section, externalPackages, packagingLevel);
				if (externalPackages.Tags.Any())
				{
					var sg32 = sg31.Group32.InstantiateAChildAndAddItToChildrenCollection();
					foreach (var tag in externalPackages.Tags)
					{
						var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
						var splitter = new TextSplitter(CommonMessageTextBuilder.MarksLabelsShippingMarksLength)
						{
							Text = tag
						};
						pciSegment.MarksLabels.ShippingMarks1 = splitter[0];
						pciSegment.MarksLabels.ShippingMarks2 = splitter[1];
					}
				}
			}
		}

		static void AddNewSG37GroupsOnSG30(SegmentGroup37MessageSection sg37Section, IArrivalLine source, ZBool isArrivalWithOBS)
		{
			if (isArrivalWithOBS)
			{
				foreach (var doc in source.Documents)
				{
					var sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
					CommonMessageTextBuilder.AddNewDOCSegment(sg37.DOC, ZString.Empty, doc.Number, ZString.Empty, doc.Name);
				}
			}
		}

		#endregion

		static void PopulateCNTSegments(CNTSegmentMessageSection cntSection, IArrivalMessageDataProvider source)
		{
			if (!source.IsOnlyArrivalNotification)
			{
				CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZIntNational(source.TotalNumberOfGoods, 0), ControlQualifierList.NumberOfCustomsItemDetailLines);
				CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZLongNational(source.TotalNumberOfPackageElements, 0), ControlQualifierList.TotalNumberOfPackages);
			}
		}
	}
}
