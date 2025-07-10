using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	internal static class DUAExportMessageTextBuilder
	{
		internal static void PopulateCUSDECMessage(CUSDECMessage message, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.PopulateUNHSegment(message.UNH[0], MessageReleaseNumberList.TrialRelease1992, "ECS003");
			CommonMessageTextBuilder.PopulateBGMSegment(message.BGM[0], DocumentMessageNameCodedList.GoodsDeclarationForExportation, source.LocalReferenceNumber, MessageFunctionCodedList.GetFromString(source.MessageType));
			PopulateCSTSegment(message.CST[0], source);
			PopulateLOCSegments(message.LOC, source);
			PopulateDTMSegments(message.DTM, source);
			PopulateGISSegments(message.GIS, source);
			PopulateEQDSegments(message.EQD, source);
			PopulateSELSegments(message.SEL, source);
			PopulateFTXSegment(message.FTX, source);
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

		#region Fields Population

		static void PopulateCSTSegment(CSTSegment cstSegment, IDUAExportMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.CustomsProcedureCategory1;
			cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.CustomsAreaOfTransaction;
			cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = source.CustomsProcedureCategory2;
			cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsDeclarationType;
			cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			if (!source.CustomsProcedureCategory3.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes3.CustomsCodeIdentification = source.CustomsProcedureCategory3;
				cstSegment.CustomsIdentityCodes3.CodeListQualifier = CodeListQualifierList.CustomsTransitType;
				cstSegment.CustomsIdentityCodes3.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;
			}

			cstSegment.CustomsIdentityCodes4.CustomsCodeIdentification = source.CustomsProcedureCategory4;
			cstSegment.CustomsIdentityCodes4.CodeListQualifier = CodeListQualifierList.CustomsNatureOfTheTransaction;
			cstSegment.CustomsIdentityCodes4.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.CustomsProcedureCategory5;
			cstSegment.CustomsIdentityCodes5.CodeListQualifier = CodeListQualifierList.CustomsOffice;
			cstSegment.CustomsIdentityCodes5.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
		}

		static void PopulateLOCSegments(LOCSegmentMessageSection locSection, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfExport, PlaceLocationQualifierList.CountryOfExportationDespatch, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); // LOC+35
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CountryOfDestination, PlaceLocationQualifierList.CountryOfUltimateDestination, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); // LOC+36
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsOfficeofExitCountryCode, PlaceLocationQualifierList.CustomsOfficeOfExit, source.CustomsOfficeofExit, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); // LOC+42
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.LocationOfGoodsExamCustomsOffice, PlaceLocationQualifierList.PlaceOfCustomsExamination, source.LocationOfGoodsExam, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); // LOC+43
			AddNewLOCSegment(locSection, PlaceLocationQualifierList.Warehouse, source.Warehouse); // LOC+18
		}

		static void AddNewLOCSegment(LOCSegmentMessageSection locSection, PlaceLocationQualifierList locType, ZString placeLoc)
		{
			if (!placeLoc.IsEmpty)
			{
				var locSegment = locSection.InstantiateAChildAndAddItToChildrenCollection();
				locSegment.PlaceLocationQualifier = locType;
				locSegment.LocationIdentification.PlaceLocation = placeLoc;
			}
		}

		static void PopulateDTMSegments(DTMSegmentMessageSection dtmSection, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewDTMSegment(dtmSection, source.DateOfRecap.ToCustomsFormatDateString(), DateTimePeriodQualifierList.DeliveryDateTimeLast, DateTimePeriodFormatQualifierList.Ccyymmdd);
		}

		static void PopulateGISSegments(GISSegmentMessageSection gisSection, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, source.GoodsInContainerIndicator, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, source.RMTIndicator, CodeListQualifierList.CustomsSpecialCodes, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1, ProcessTypeIdentificationList.GetFromString("RMT"));
		}

		static void PopulateEQDSegments(EQDSegmentMessageSection eqdSection, IDUAExportMessageDataProvider source)
		{
			foreach (var country in source.CountryCodes)
			{
				CommonMessageTextBuilder.AddNewEQDSegment(eqdSection, EquipmentQualifierList.Chassis, country);
			}
		}

		static void PopulateSELSegments(SELSegmentMessageSection selSection, IDUAExportMessageDataProvider source)
		{
			foreach (var seal in source.SealCodes)
			{
				CommonMessageTextBuilder.AddNewSELSegment(selSection, source.SealCodes.Count, SealingPartyCodedList.Carrier, seal);
			}
		}

		static void PopulateFTXSegment(FTXSegmentMessageSection ftxSection, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewFTXSegment(ftxSection, source.TextFunctionCode);
		}

		#region SG1 Population

		static void PopulateSG1Group(SegmentGroup1MessageSection sg1Section, IDUAExportMessageDataProvider source)
		{
			AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.QuotaNumber, source.ReferenceNumber); //RFF+ABJ
			AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.GetFromString("AJK"), source.SpecificCircumstancesIndicator); //RFF+AJK
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

		static void PopulateSG4Groups(SegmentGroup4MessageSection sg4Section, IDUAExportMessageDataProvider source)
		{
			if (source.BorderTransportMode != null)
			{
				AddNewSG4Group(sg4Section, TransportStageQualifierList.AtBorder, source.BorderTransportMode.TransportMode, source.BorderTransportMode.TransportId, source.BorderTransportMode.TransportNationality);
			}
			AddNewSG4Group(sg4Section, TransportStageQualifierList.AtDeparture, source.InternalTransportMode, null, null);
			AddNewSG4Group(sg4Section, TransportStageQualifierList.OnCarriageTransport, null, source.TransportModeName, null);
		}

		static void AddNewSG4Group(SegmentGroup4MessageSection group4, TransportStageQualifierList qualifier, ZString transportMode, ZString transportName, ZString transportNationality)
		{
			if (!transportMode.IsEmpty || !transportName.IsEmpty)
			{
				var sg4 = group4.InstantiateAChildAndAddItToChildrenCollection();
				var tdtSegment = sg4.TDT[0];
				tdtSegment.TransportStageQualifier = qualifier;
				if (!transportMode.IsEmpty)
				{
					tdtSegment.ModeOfTransport.ModeOfTransportCoded = transportMode;
					if (qualifier == TransportStageQualifierList.AtBorder)
					{
						CommonMessageTextBuilder.AddNewTPLSegment(sg4.TPL, transportName, transportNationality);
					}
				}
				else
				{
					tdtSegment.ModeOfTransport.ModeOfTransport = transportName;
				}
			}
		}

		#endregion

		#region SG6 Population

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, IDUAExportMessageDataProvider source)
		{
			AddNewNADWithAddressInSG6Group(sg6Section, PartyQualifierList.Exporter, source.Exporter); //NAD+EX
			AddNewNADWithAddressInSG6Group(sg6Section, PartyQualifierList.GetFromString("CN"), source.Receiver); //NAD+CN
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(sg6Section, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.Declarant); //NAD+Declarant
		}

		static SegmentGroup6 AddNewNADWithAddressInSG6Group(SegmentGroup6MessageSection sg6Section, PartyQualifierList addressType, IDUAExportPartyProvider addressDetails)
		{
			SegmentGroup6 result = null;
			if (addressDetails != null)
			{
				var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
				var nadSegment = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nadSegment.PartyQualifier = addressType;
				var partyId = addressDetails.Id;
				nadSegment.PartyIdentificationDetails.PartyIdIdentification = partyId;
				var partyIdQualifier = addressDetails.OrganizationCodeQualifier;
				if (addressType == PartyQualifierList.Exporter && !partyIdQualifier.IsEmpty)//Can only be "P"
				{
					nadSegment.PartyIdentificationDetails.CodeListQualifier = CodeListQualifierList.GetFromString(partyIdQualifier);
				}
				if (!partyId.IsEmpty)
				{
					nadSegment.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
				}
				CommonMessageTextBuilder.SetNADAddressDetails(nadSegment, addressDetails);
			}
			return result;
		}

		#endregion

		#region SG7 Population

		static void PopulateSG7Group(SegmentGroup7MessageSection sg7Section, IDUAExportMessageDataProvider source)
		{
			CommonExportMessageTextBuilder.AddNewSG7Group(sg7Section, source.TermsOfDeliveryCode, source.DeliveryLocation, source.LocationId);
		}

		#endregion

		#region SG8 Population

		static void PopulateSG8Group(SegmentGroup8MessageSection sg8Section, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewMOAInSG8Group(sg8Section, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, source.TotalAmount, source.TotalAmountCurrencyCode);
			CommonMessageTextBuilder.AddNewMOAInSG8Group(sg8Section, MonetaryAmountTypeQualifierList.MutuallyDefined, 0, source.IsDeclarationInEuros ? "EUR" : null);
		}

		#endregion

		#region SG30 Population

		static void PopulateSG30Group(SegmentGroup30MessageSection sg30Section, IDUAExportMessageDataProvider source)
		{
			foreach (IDUAExportLine line in source.Lines)
			{
				AddNewSG30Group(sg30Section, line);
			}
		}

		static void AddNewSG30Group(SegmentGroup30MessageSection sg30Section, IDUAExportLine source)
		{
			var sg30 = sg30Section.InstantiateAChildAndAddItToChildrenCollection();
			AddCSTSegmentOnSG30(sg30.CST[0], source);
			CommonMessageTextBuilder.AddFTXSegmentForGoodsDescription(sg30, TextSubjectQualifierList.GoodsDescription, source.GoodsDescription, 70);
			AddFTXSegmentForAdditionalInfomation(sg30, TextSubjectQualifierList.RegulatoryInformation_Reg, source.SpecialConditions);
			CommonMessageTextBuilder.AddNewLOCSegment(sg30.LOC, source.CountryOfOrigin, PlaceLocationQualifierList.CountryOfOrigin, source.StateOfOrigin, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); // LOC+43
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.GrossWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.GrossWeight);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.NetWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.NetNetWeight);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.SupplementaryUnitsNumber, source.SupplementaryUnitsQualifier, MeasurementApplicationQualifierList._2ndSpecifiedTariffQuantity);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.OtherUnitsNumber, source.OtherUnitsQualifier, MeasurementApplicationQualifierList.Measurement);
			CommonMessageTextBuilder.AddNewTDTSegmentOnSG30(sg30, source.DangerousGoodsCode, TransportStageQualifierList.PreCarriageTransport);
			CommonMessageTextBuilder.AddNewSG31Group(sg30.Group31, source.ExternalPackages, PackagingLevelCodedList.Outer);
			AddNewSG31Group(sg30.Group31, source.InternalPackages, PackagingLevelCodedList.Inner);
			CommonMessageTextBuilder.AddNewSG31Group(sg30.Group31, source.VehiclePackages, PackagingLevelCodedList.GetFromString("4"));
			CommonMessageTextBuilder.AddNewMOASegmentSG33Group(sg30.Group33, MonetaryAmountTypeQualifierList.StatisticalValue, source.TotalGoodValueInEuros);
			if (!source.DocumentReferenceNumber.IsEmpty)
			{
				var sg35 = sg30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				CommonMessageTextBuilder.AddNewRFFSegment(sg35.RFF, ReferenceQualifierList.GoodsDeclarationNumber, source.DocumentReferenceNumber, source.DocumentTypeCode, ZString.Empty);
			}
			AddNewSG37Groups(sg30.Group37, source);
		}

		static void AddCSTSegmentOnSG30(CSTSegment cstSegment, IDUAExportLine source)
		{
			cstSegment.GoodsItemNumber = Utilities.FormatNumberFromZIntNational(source.GoodsItemNumber, 0);
			if (!source.GoodsCustomsProcedureCategory1.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory1;
				cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.Commodity;
				cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			}

			if (!source.GoodsCustomsProcedureCategory2.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory2;
				cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsProcedure;
				cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;
			}

			if (!source.GoodsCustomsProcedureCategory3.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes3.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory3;
				cstSegment.CustomsIdentityCodes3.CodeListQualifier = CodeListQualifierList.CustomsProcedure;
				cstSegment.CustomsIdentityCodes3.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			}

			if (!source.GoodsCustomsProcedureCategory4.IsEmpty)
			{
				cstSegment.CustomsIdentityCodes4.CustomsCodeIdentification = source.GoodsCustomsProcedureCategory4;
				cstSegment.CustomsIdentityCodes4.CodeListQualifier = CodeListQualifierList.CustomsProcedure;
				cstSegment.CustomsIdentityCodes4.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
			}
		}

		static void AddFTXSegmentForAdditionalInfomation(SegmentGroup30 sg30, TextSubjectQualifierList textType, IDUAExportSpecialConditions conditions)
		{
			if (conditions != null && (!conditions.Code1.IsEmpty || !conditions.Code2.IsEmpty || !conditions.Code3.IsEmpty || !conditions.Code4.IsEmpty || !conditions.Text.IsEmpty))
			{
				var ftxSegment = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftxSegment.TextSubjectQualifier = textType;
				if (!conditions.Code1.IsEmpty)
				{
					ftxSegment.TextLiteral.FreeText1 = conditions.Code1;
				}
				if (!conditions.Code2.IsEmpty)
				{
					ftxSegment.TextLiteral.FreeText2 = conditions.Code2;
				}
				if (!conditions.Code3.IsEmpty)
				{
					ftxSegment.TextLiteral.FreeText3 = conditions.Code3;
				}
				if (!conditions.Code4.IsEmpty)
				{
					ftxSegment.TextLiteral.FreeText4 = conditions.Code4;
				}
				if (!conditions.Text.IsEmpty)
				{
					ftxSegment.TextLiteral.FreeText5 = conditions.Text;
				}
			}
		}

		static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, IInternalPackagesInfoCommon internalPackages, PackagingLevelCodedList packagingLevel)
		{
			if (internalPackages != null && internalPackages.Packages.Any())
			{
				var sg32 = CommonMessageTextBuilder.SetPACSegmentAndAddNewSG32GroupForInternal(sg31Section, packagingLevel).sg32;
				foreach (var package in internalPackages.Packages)
				{
					var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
					pciSegment.MarksLabels.ShippingMarks1 = package.Tag;
					pciSegment.MarksLabels.ShippingMarks3 = Utilities.FormatNumberFromZLongNational(package.NumberOfElements, 0);
					pciSegment.ContainerPackageStatusCoded = ContainerPackageStatusCodedList.GetFromString(package.ElementsType);
				}
			}
		}

		static void AddNewSG37Groups(SegmentGroup37MessageSection sg37Section, IDUAExportLine source)
		{
			foreach (var doc in source.Documents)
			{
				CommonExportMessageTextBuilder.AddNewSG37(sg37Section, doc.Name, doc.Number, GetSource(doc.Quantity, doc.QtyUnit), doc.DateOfIssue, doc.DateOfExpiry);
			}
		}

		static ZString GetSource(ZDecimal quantity, ZString unit)
		{
			var source = ZString.Empty;
			if (!unit.IsEmpty && !quantity.IsEmpty)
			{
				ZDecimal quantityAux = quantity * 1000;
				var qty = quantityAux.Truncate(0).ToString();

				source = unit + qty.PadLeft(15, '0');
			}
			return source;
		}

		#endregion

		static void PopulateCNTSegments(CNTSegmentMessageSection cntSection, IDUAExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZIntNational(source.TotalNumberOfGoods, 0), ControlQualifierList.NumberOfCustomsItemDetailLines);
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZIntNational(source.TotalNumberOfPackageElements, 0), ControlQualifierList.TotalNumberOfPackages);
		}

		#endregion
	}
}
